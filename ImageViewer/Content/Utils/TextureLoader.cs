// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using Newtonsoft.Json.Linq;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;



namespace ImageViewer.Content.Utils
{
    internal class TextureLoader : IDisposable
    {
        private readonly ulong MAX_MEMORY_USE = 900000000; // 900 MB
        private static Mutex mutex = new Mutex();

        private Dictionary<string, Tuple<ShaderResourceView, Texture2D>> textures; 
        private Queue<string> loadQueue = new Queue<string>();
        private List<string> lastUse = new List<string>();

        private readonly ImagingFactory2 factory;
        private readonly StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;

        private static bool loading = false;
        
        private static readonly SharpDX.Size2 TextureTileSize = new SharpDX.Size2(256, 256);

        private readonly DeviceResources deviceResources;
        private string currentlyLoading = null;

        public int TilesInMemory() => textures.Count;

        internal TextureLoader(DeviceResources deviceResources)
        {
            this.deviceResources = deviceResources;
            factory = new ImagingFactory2();

            textures = new Dictionary<string, Tuple<ShaderResourceView, Texture2D>>();
        }

        internal void ReleaseDeviceDependentResources()
        {
            foreach (var texture in textures)
            {
                var key = texture.Key;
                var val = texture.Value;
                textures.Remove(key);
                val.Item1.Dispose();
                val.Item2.Dispose();
                val = null;
            }         
        }

        internal bool TextureReady(string url)
        {
            return textures.ContainsKey(url);
        }

        internal void SetTextureResource(PixelShaderStage pixelShader, string id)
        {
            if (textures.ContainsKey(id))
            {
                if (textures.TryGetValue(id, out Tuple<ShaderResourceView, Texture2D> texture))
                {
                    pixelShader.SetShaderResource(0, texture.Item1);
                }
            }     
        }

        private bool InitLoadQueue(List<string> IDs)
        {
            mutex.WaitOne();
            loadQueue.Clear();
            foreach (var ID in IDs)
            {
                if (!(ID.Equals(currentlyLoading) || textures.ContainsKey(ID) || loadQueue.Contains(ID)))
                {
                    loadQueue.Enqueue(ID);
                }
            }
            mutex.ReleaseMutex();
            return loading;
        }

        private bool KeepLoading()
        {
            mutex.WaitOne();
            loading = true;
            currentlyLoading = null;
            var keepLoading = loadQueue.Count > 0;
            mutex.ReleaseMutex();
            return keepLoading;
        }

        private string NextID()
        {
            mutex.WaitOne();
            var id = loadQueue.Peek();
            currentlyLoading = id;            
            if (lastUse.Contains(id))
            {
                lastUse.Remove(id);
            }
            lastUse.Add(id);
            mutex.ReleaseMutex();
            return id;
        }

        private void CheckMemoryUse()
        {
            var report = Windows.System.MemoryManager.GetAppMemoryReport();
            var memoryUse = report.PrivateCommitUsage;

            if (memoryUse > MAX_MEMORY_USE && lastUse.Count > 200) // Release old textures if memory runs low
            {
                var clean = 100;
                for (var i = 0; i < clean; i++)
                {
                    var oldest = lastUse[0];
                    lastUse.RemoveAt(0);
                    if (textures.TryGetValue(oldest, out Tuple<ShaderResourceView, Texture2D> texture))
                    {
                        textures.Remove(oldest);
                        texture.Item1.Dispose();
                        texture.Item2.Dispose();
                        texture = null;
                    }
                }
            }
        }

        internal async Task<Texture2D> GetTextureAsync(string id, SharpDX.Size2 size)
        {
            Texture2D texture2D = null;
            using (var dataStream = await GetImageAsync(id))
            {
                if (dataStream != null)
                {
                    CheckMemoryUse();

                    if (Settings.UseJpeg)
                    {
                        texture2D = Texture2DFromJpeg(deviceResources, dataStream);
                    }
                    else if (Settings.DownloadRaw)
                    {
                        texture2D = Texture2DRaw(deviceResources, dataStream, size);
                    }
                    else
                    {
                        texture2D = Texture2D(deviceResources, dataStream);
                    }
                }
            }

            return texture2D;
        }

        internal async Task LoadTexturesAsync(List<string> IDs)
        {
            if (InitLoadQueue(IDs)) return;

            while (KeepLoading())
            {
                var id = NextID();
                var texture2D = await GetTextureAsync(id, TextureTileSize);
                
                if (texture2D != null)
                {
                    var shaderResourceDesc = ShaderDescription();
                    var resourceView = new ShaderResourceView(deviceResources.D3DDevice, texture2D, shaderResourceDesc);
                    textures.Add(id, new Tuple<ShaderResourceView, Texture2D>(resourceView, texture2D));
                }           

                mutex.WaitOne();
                if (loadQueue.Contains(id)) loadQueue.Dequeue();
                mutex.ReleaseMutex();
            }

            mutex.WaitOne();
            loading = false;
            mutex.ReleaseMutex();
        }

        internal async Task ClearCacheAsync()
        {     
            var files = await localCacheFolder.GetFilesAsync();
            foreach (var file in files)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        internal async Task DeleteCacheFileAsync(string id, string fileExtension)
        {
            var fileName = id + fileExtension;

            IStorageItem file = null;

            file = await localCacheFolder.TryGetItemAsync(fileName);

            if (file != null)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        private string FileName(string id)
        {
            var raw = Settings.DownloadRaw;
            var jpg = Settings.UseJpeg;
            var fileName = id + (raw ? ".RAW" : jpg ? ".JPG" : ".PNG");
            return fileName;
        }

        private string Url(string id)
        {
            if( id.Contains( "&" ) ) {
                id = id.Insert( id.IndexOf( "&" ), "?" );
            } else {
                id = id + "?";
            }

            var url = Settings.BaseUrl() + id;

            if( Settings.DownloadRaw ) {
                url += "&format=RAW";
            } else if( Settings.UseJpeg ) {
                url += "&format=JPG";
            } else if( Settings.UsePNG ) {
                url += "&format=PNG";
            }

            return url;
        }

        internal async Task<JObject> GetJsonAsync(string URL, string url)
        {
            IStorageItem file = null;
            var baseUrl = URL + url;
            System.Diagnostics.Debug.WriteLine( baseUrl.ToString() );
            if (Settings.SaveTexture)
            {
                file = await localCacheFolder.TryGetItemAsync(url.Substring(1));
            }

            if (file == null)
            {
                var request = (HttpWebRequest)WebRequest.Create(baseUrl);
                try
                {
                    using (var response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Settings.Online = true;

                            var encoding = ASCIIEncoding.ASCII;
                            using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                            {
                                string responseText = reader.ReadToEnd();

                                if (Settings.SaveTexture)
                                {
                                    await SaveDataAsync(url.Substring(1), responseText);
                                }

                                return JObject.Parse(responseText);
                            }
                        }
                        else
                        {
                            Settings.Online = false;
                            return null;
                        }                      
                    }
                }
                catch (Exception)
                {
                    Settings.Online = false;
                    return null;
                }
            }
            else
            {
                var stream = await ((StorageFile)file).OpenAsync(FileAccessMode.Read);
                var buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
                await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

                return JObject.Parse(DataReader.FromBuffer(buffer).ReadString(buffer.Length));
            }
        }

        private async Task<MemoryStream> GetImageAsync(string id)
        {
            var fileName = FileName(Settings.CaseID + id);
            IStorageItem file = null;

            if (Settings.SaveTexture)
            {
                file = await localCacheFolder.TryGetItemAsync(fileName);
            }
                 
            if (file == null)
            {


                string urlID = Url( id.Replace( ";", "/" ));
                //System.Diagnostics.Debug.WriteLine(urlID);
                var request = (HttpWebRequest)WebRequest.Create( urlID  );

                try
                {
                    using (var response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Settings.Online = true;

                            using (var stream = response.GetResponseStream())
                            {
                                var memoryStream = new MemoryStream((int)response.ContentLength);
                                await stream.CopyToAsync(memoryStream);

                                if (Settings.SaveTexture)
                                {
                                    await SaveTextureAsync(fileName, memoryStream);
                                }

                                memoryStream.Position = 0;
                                return memoryStream;
                            }
                        }
                        else
                        {
                            Settings.Online = false;
                            return null;
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {            
                var bytes = await DirectXHelper.ReadDataAsync((StorageFile)file);
                var memoryStream = new MemoryStream(bytes);
                return memoryStream;
            }          
        }

        private async Task SaveDataAsync(string fileName, string data)
        {
            var newFile = await localCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var dataWriter = new DataWriter(fileStream))
                {
                    dataWriter.WriteString(data);

                    await dataWriter.StoreAsync();
                    await fileStream.FlushAsync();
                }
            }
        }

        private async Task SaveTextureAsync(string fileName, MemoryStream memoryStream)
        {
            var newFile = await localCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var dataWriter = new DataWriter(fileStream))
                {
                    var buffer = memoryStream.GetWindowsRuntimeBuffer();
                    dataWriter.WriteBuffer(buffer);

                    await dataWriter.StoreAsync();
                    await fileStream.FlushAsync();
                }
            }
        }

        internal static SamplerStateDescription SamplerStateDescription() 
            => new SamplerStateDescription()
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0f, 0f, 0f, 1f)
        };

        internal Texture2D Texture2DRaw(DeviceResources deviceResources, MemoryStream rawData, SharpDX.Size2 size)
        {
            using (var dataStream = new SharpDX.DataStream((int)rawData.Length, true, true))
            {
                dataStream.Write(rawData.ToArray(), 0, (int)rawData.Length);
                var texture2D = Texture2D(deviceResources, dataStream, size);
                return texture2D;
            }               
        }

        internal Texture2D Texture2D(DeviceResources deviceResources, MemoryStream imageData)
        {
            using (var bitmap = CreateBitmap(imageData, out SharpDX.Size2 size))
            {
                var texture2D = Texture2D(deviceResources, bitmap, size);
                return texture2D;         
            }      
        }

        internal Texture2D Texture2DFromJpeg(DeviceResources deviceResources, MemoryStream imageData)
        {
            using (var bitmap = CreateBitmapFromJpeg(imageData, out SharpDX.Size2 size))
            {
                if (bitmap != null)
                {
                    var texture2D = Texture2D(deviceResources, bitmap, size);
                    return texture2D;
                }
                else return null;
            }
        }

        internal Texture2D Texture2D(DeviceResources deviceResources, string fileName)
        {
            using (var bitmap = CreateBitmap(fileName, out SharpDX.Size2 size))
            {
                var texture2D = Texture2D(deviceResources, bitmap, size);
                return texture2D;
            }
        }

        internal static Texture2D Texture2D(DeviceResources deviceResources, SharpDX.DataStream stream, SharpDX.Size2 size)
        {
            var textDesc = TextureDescription(1, size.Width, size.Height, ResourceOptionFlags.None);

            SharpDX.DataBox[] dataRects = { new SharpDX.DataBox(stream.DataPointer, size.Width * 4, 0) };

            var texture2D = new Texture2D(deviceResources.D3DDevice, textDesc, dataRects);
            
            return texture2D;
        }

        internal Texture2D TextureCube(DeviceResources deviceResources, string fileName)
        {
            using (var bitmap = CreateBitmap(fileName, out SharpDX.Size2 size))
            {
                var textureCube = TextureCube(deviceResources, bitmap, size);
                return textureCube;
            }
        }

        internal static Texture2D TextureCube(DeviceResources deviceResources, SharpDX.DataStream stream, SharpDX.Size2 size)
        {
            var arraySize = 6;
            var textDesc = TextureDescription(arraySize, size.Width, size.Height, ResourceOptionFlags.TextureCube);

            var stride = size.Width * 4;
            var dataRects = new SharpDX.DataBox[arraySize];

            for (var i = 0; i < arraySize; i++)
            {
                dataRects[i] = new SharpDX.DataBox(stream.DataPointer, stride, 0);
            }

            var texture2D = new Texture2D(deviceResources.D3DDevice, textDesc, dataRects);
            return texture2D;
        }

        private SharpDX.DataStream CreateBitmap(string fileName, out SharpDX.Size2 size)
        {
            using (var decoder = new BitmapDecoder(factory, fileName, DecodeOptions.CacheOnDemand))
            {
                using (var formatConverter = new FormatConverter(factory))
                {
                    formatConverter.Initialize(decoder.GetFrame(0), PixelFormat.Format32bppPRGBA);

                    var stride = formatConverter.Size.Width * 4;
                    var dataStream = new SharpDX.DataStream(formatConverter.Size.Height * stride, true, true);
                    formatConverter.CopyPixels(stride, dataStream);

                    size = formatConverter.Size;

                    return dataStream;
                }
            }             
        }

        internal SharpDX.DataStream CreateBitmap(MemoryStream stream, out SharpDX.Size2 size)
        {
            using (var decoder = new BitmapDecoder(factory, stream, DecodeOptions.CacheOnDemand))
            {
                using (var formatConverter = new FormatConverter(factory))
                {
                    formatConverter.Initialize(decoder.GetFrame(0), PixelFormat.Format32bppPRGBA);

                    var stride = formatConverter.Size.Width * 4;
                    var dataStream = new SharpDX.DataStream(formatConverter.Size.Height * stride, true, true);
                    formatConverter.CopyPixels(stride, dataStream);

                    size = formatConverter.Size;

                    return dataStream;
                }
            }           
        }

        internal SharpDX.DataStream CreateBitmapFromJpeg(MemoryStream stream, out SharpDX.Size2 size)
        {
            using (var istream = new WICStream(factory, stream))
            {
                using (var decoder = new PngBitmapDecoder(factory))
                {
                    decoder.Initialize(istream, DecodeOptions.CacheOnDemand);

                    using (var formatConverter = new FormatConverter(factory))
                    {
                        formatConverter.Initialize(decoder.GetFrame(0), PixelFormat.Format32bppPRGBA);

                        var stride = formatConverter.Size.Width * 4;
                        var dataStream = new SharpDX.DataStream(formatConverter.Size.Height * stride, true, true);
                        formatConverter.CopyPixels(stride, dataStream);

                        size = formatConverter.Size;

                        return dataStream;
                    }
                }
            }
        }

        internal static ShaderResourceViewDescription ShaderDescription() =>
            new ShaderResourceViewDescription()
            {
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource() { MipLevels = 1, MostDetailedMip = 0 }
            };

        internal static ShaderResourceViewDescription ShaderDescriptionCube() =>
            new ShaderResourceViewDescription()
            {
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube,
                TextureCube = new ShaderResourceViewDescription.TextureCubeResource() { MipLevels = 1, MostDetailedMip = 0 }
            };

        private static Texture2DDescription TextureDescription(int size, int w, int h, ResourceOptionFlags flags)
            => new Texture2DDescription()
            {
                Width = w,
                Height = h,
                ArraySize = size,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = flags,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            };

        public void Dispose()
        {
            foreach (var texture in textures)
            {
                var key = texture.Key;
                var val = texture.Value;
                textures.Remove(key);
                val.Item1.Dispose();
                val.Item2.Dispose();
                val = null;
            }
        }
    }
}
