using ImageViewer.Common;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageViewer.Content
{
    internal class TextureLoader
    {
        private readonly ulong MAX_MEMORY_USE = 900000000; // 900 MB
        private ulong memoryUse = 0;

        private Dictionary<string, Tuple<ShaderResourceView, Texture2D>> textures; 
        private Queue<string> loadQueue = new Queue<string>();
        private Queue<string> preloadQueue = new Queue<string>();
        private List<string> lastUse = new List<string>();

        private readonly ImagingFactory2 factory;
        private readonly StorageFolder localCacheFolder;

        private static bool loading = false;
        private static bool preloading = false;

        private readonly DeviceResources deviceResources;
        private readonly string baseUrl;

        public int TilesInMemory() => textures.Count;

        internal TextureLoader(DeviceResources deviceResources, string baseUrl)
        {
            this.deviceResources = deviceResources;
            this.baseUrl = baseUrl;
            factory = new ImagingFactory2();
            localCacheFolder = ApplicationData.Current.LocalCacheFolder;
            textures = new Dictionary<string, Tuple<ShaderResourceView, Texture2D>>();

            var report = Windows.System.MemoryManager.GetAppMemoryReport();
            memoryUse = report.PrivateCommitUsage;
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

        internal async Task LoadTextureAsync(string ID)
        {
            if (lastUse.Contains(ID))
            {
                lastUse.Remove(ID);
            }
            lastUse.Add(ID);

            if (textures.ContainsKey(ID) || loadQueue.Contains(ID)) return;

            loadQueue.Enqueue(ID);

            if (loading) return;

            loading = true;

            while (loadQueue.Count > 0)
            {
                var id = loadQueue.Peek();

                using (var dataStream = await GetImageAsync(id))
                {
                    if (dataStream != null)
                    {
                        var report = Windows.System.MemoryManager.GetAppMemoryReport();
                        memoryUse = report.PrivateCommitUsage;

                        if (memoryUse > MAX_MEMORY_USE && lastUse.Count > 200) // Release old texture if memory runs low
                        {
                            var clean = 100;
                            for (var i=0; i < clean; i++)
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

                        var texture2D = Texture2D(deviceResources, dataStream);
                        var shaderResourceDesc = ShaderDescription();
                        var resourceView = new ShaderResourceView(deviceResources.D3DDevice, texture2D, shaderResourceDesc);

                        textures.Add(id, new Tuple<ShaderResourceView, Texture2D>(resourceView, texture2D));     
                    }
                }
                loadQueue.Dequeue();
            }

            loading = false;
        }

        internal async Task<bool> PreloadImage(string ID)
        {
            if (preloadQueue.Contains(ID)) return true;

            preloadQueue.Enqueue(ID);

            if (preloading) return true;

            preloading = true;

            while (preloadQueue.Count > 0)
            {
                var id = preloadQueue.Dequeue();

                var fileName = id + ".PNG";
                var file = await localCacheFolder.TryGetItemAsync(fileName);

                if (file == null)
                {
                    var request = (HttpWebRequest)WebRequest.Create(baseUrl + id);
                    try
                    {
                        using (var response = (HttpWebResponse)await request.GetResponseAsync())
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                using (var stream = response.GetResponseStream())
                                {
                                    using (var memoryStream = new MemoryStream((int)response.ContentLength))
                                    {
                                        await stream.CopyToAsync(memoryStream);

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
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        preloading = false;
                        return false;
                    }
                }
            }
            preloading = false;
            return true;
        }

        internal async Task ClearCache()
        {     
            var files = await localCacheFolder.GetFilesAsync();
            foreach (var file in files)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        internal async Task DeleteCacheFile(string id, string fileExtension)
        {
            var fileName = id + fileExtension;
            var file = await localCacheFolder.TryGetItemAsync(fileName);

            if (file != null)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        internal async Task<SharpDX.DataStream> LoadPixelDataAsync(string id)
        {
            var fileName = id + ".RAW";
            var file = await localCacheFolder.TryGetItemAsync(fileName);

            if (file == null)
            {
                return null;
            }
            else
            {

                var bytes = await DirectXHelper.ReadDataAsync((StorageFile)file);
                var dataStream = new SharpDX.DataStream(bytes.Length, true, true);
                await dataStream.WriteAsync(bytes, 0, bytes.Length);
                return dataStream;
            }
        }

        internal async Task SavePixelDataAsync(string id, SharpDX.DataStream stream)
        {
            var fileName = id + ".RAW";
            var bytes = new byte[stream.Length];

            await stream.ReadAsync(bytes, 0, (int)stream.Length);

            var newFile = await localCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var fileStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var dataWriter = new DataWriter(fileStream))
                {
                    dataWriter.WriteBytes(bytes);

                    await dataWriter.StoreAsync();
                    await fileStream.FlushAsync();
                }
            }
        }

        internal async Task<MemoryStream> GetImageAsync(string id)
        {
            var fileName = id + ".PNG";
            var file = await localCacheFolder.TryGetItemAsync(fileName);
            
            if (file == null)
            {
                var request = (HttpWebRequest)WebRequest.Create(baseUrl + id);

                try
                {
                    using (var response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                var memoryStream = new MemoryStream((int)response.ContentLength);
                                await stream.CopyToAsync(memoryStream);

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
                                return memoryStream;
                            }
                        }
                        else
                        {
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

        internal static SamplerStateDescription SamplerStateDescription() 
            => new SamplerStateDescription()
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            BorderColor = new SharpDX.Mathematics.Interop.RawColor4(0f, 0f, 0f, 1f)
        };

        internal Texture2D Texture2D(DeviceResources deviceResources, MemoryStream imageData)
        {
            using (var bitmap = CreateBitmap(imageData, out SharpDX.Size2 size))
            {
                var texture2D = Texture2D(deviceResources, bitmap, size);
                return texture2D;         
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

        internal Texture2D Texture2D(DeviceResources deviceResources, SharpDX.DataStream stream, SharpDX.Size2 size)
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

        internal Texture2D TextureCube(DeviceResources deviceResources, SharpDX.DataStream stream, SharpDX.Size2 size)
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
    }
}
