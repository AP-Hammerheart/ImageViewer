// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using SharpDX.Direct3D11;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class PanRenderer : PlaneRenderer
    {
        private readonly Texture2D[] texture = new Texture2D[2];
        private readonly ShaderResourceView[] view = new ShaderResourceView[2];
        private readonly string[] IDs = new string[2];
        private bool textureReady = false;

        private float TileSize { get; }
        private int BackBufferResolution { get; }
        private int Active { get; set; } = -1;
        private bool Loading { get; set; } = false;
        internal override bool TextureReady => (Active != -1) && textureReady;
        internal int X { get; set; } = 0;
        internal int Y { get; set; } = 0;
        
        public PanRenderer(DeviceResources deviceResources, TextureLoader loader, string url, float tileSize, int backBufferResolution)
            : base(deviceResources, loader, url)
        {
            TileSize = tileSize; 
            BackBufferResolution = backBufferResolution;
        }

        internal void UpdateGeometry(int x, int y)
        {
            X = x;
            Y = y;
            UpdateGeometry();
        }

        internal override void UpdateGeometry()
        {
            var x0 = ((float)X) / ((float)BackBufferResolution);
            var y0 = ((float)Y) / ((float)BackBufferResolution);

            var x1 = x0 + ((float)PanView.ViewResolution) / ((float)BackBufferResolution);
            var y1 = y0 + ((float)PanView.ViewResolution) / ((float)BackBufferResolution);

            if (vertexBuffer != null)
            {
                RemoveAndDispose(ref vertexBuffer);
            }

            VertexPlane[] planeVertices =
            {
                new VertexPlane(new Vector3(-0.5f * TileSize, -0.5f * TileSize, 0.0f), new Vector2(x0, y1)),
                new VertexPlane(new Vector3(-0.5f * TileSize,  0.5f * TileSize, 0.0f), new Vector2(x0, y0)),
                new VertexPlane(new Vector3( 0.5f * TileSize, -0.5f * TileSize, 0.0f), new Vector2(x1, y1)),
                new VertexPlane(new Vector3( 0.5f * TileSize,  0.5f * TileSize, 0.0f), new Vector2(x1, y0))
            };

            vertexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.VertexBuffer, planeVertices));
        }

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            if (Active != -1 && textureReady)
            {
                pixelShader.SetShaderResource(0, view[Active]);
            }
        }

        internal async Task UpdateTextureAsync()
        {
            if (Loading) return;
            Loading = true;

            // Texture already in memory
            if (TextureID == IDs[Active == 0 ? 1 : 0])
            {
                Active = Active == 0 ? 1 : 0;
                Loading = false;
                return;
            }

            textureReady = false;

            string ID = null;

            // Load latest update
            while (ID != TextureID)
            {
                ID = TextureID;

                var idx = Active == 0 ? 1 : 0;
 
                view[idx]?.Dispose();
                view[idx] = null;
                texture[idx]?.Dispose();
                texture[idx] = null;

                try
                {
                    using (var stream = await loader.LoadPixelDataAsync(ID))
                    {
                        if (stream == null)
                        {
                            try
                            {
                                using (var dataStream = await loader.GetImageAsync(ID))
                                {
                                    if (dataStream != null)
                                    {
                                        using (var bitmap = loader.CreateBitmap(dataStream, out SharpDX.Size2 size))
                                        {
                                            texture[idx] = loader.Texture2D(deviceResources, bitmap, size);
                                            await loader.SavePixelDataAsync(ID, bitmap);
                                        }
                                    }
                                }
                            }
                            catch (System.Exception)
                            {
                                // Delete corrupted cache file
                                await loader.DeleteCacheFile(ID, ".PNG");
                            }
                        }
                        else
                        {
                            var size = new SharpDX.Size2(BackBufferResolution, BackBufferResolution);
                            texture[idx] = loader.Texture2D(deviceResources, stream, size);
                        }
                    }

                    var shaderResourceDesc = TextureLoader.ShaderDescription();
                    view[idx] = new ShaderResourceView(deviceResources.D3DDevice, texture[idx], shaderResourceDesc);
                    IDs[idx] = ID;
                    Active = idx;
                }
                catch (System.Exception)
                {
                    // Delete corrupted cache file
                    await loader.DeleteCacheFile(ID, ".RAW");
                }
            }

            textureReady = true;
            Loading = false;
        }

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();
            FreeResources();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);
            FreeResources();            
        }

        private void FreeResources()
        {
            textureReady = false;
            Active = -1;

            for (var i = 0; i < 2; i++)
            {
                view[i]?.Dispose();
                view[i] = null;

                texture[i]?.Dispose();
                texture[i] = null;
            }
        }
    }
}
