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
        private Texture2D texture = null;
        private ShaderResourceView view = null;
        private bool textureReady = false;

        private float TileSize { get; }
        private int BackBufferResolution { get; }

        internal override bool TextureReady => textureReady;

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

        private void UpdateGeometry()
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

        internal override void LoadGeometry()
        {
            UpdateGeometry();
            
            ushort[] planeIndices =
            {
                1,2,0,
                3,2,1,
            };

            indexCount = planeIndices.Length;
            indexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.IndexBuffer, planeIndices));

            modelConstantBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            if (textureReady)
            {
                pixelShader.SetShaderResource(0, view);
            }
        }

        internal async Task UpdateTextureAsync()
        {
            textureReady = false;
            view?.Dispose();
            view = null;
            texture?.Dispose();
            texture = null;

            try
            {
                using (var stream = await loader.LoadPixelDataAsync(TextureID))
                {
                    if (stream == null)
                    {
                        try
                        {
                            using (var dataStream = await loader.GetImageAsync(TextureID))
                            {
                                if (dataStream != null)
                                {
                                    using (var bitmap = loader.CreateBitmap(dataStream, out SharpDX.Size2 size))
                                    {
                                        texture = loader.Texture2D(deviceResources, bitmap, size);
                                        await loader.SavePixelDataAsync(TextureID, bitmap);
                                    }
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            // Delete corrupted cache file
                            await loader.DeleteCacheFile(TextureID, ".PNG");
                        }
                    }
                    else
                    {
                        var size = new SharpDX.Size2(BackBufferResolution, BackBufferResolution);
                        texture = loader.Texture2D(deviceResources, stream, size);
                    }
                }

                var shaderResourceDesc = TextureLoader.ShaderDescription();
                view = new ShaderResourceView(deviceResources.D3DDevice, texture, shaderResourceDesc);
                textureReady = true;
            }
            catch (System.Exception)
            {
                // Delete corrupted cache file
                await loader.DeleteCacheFile(TextureID, ".RAW");
            }
        }

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();

            textureReady = false;

            view?.Dispose();
            view = null;

            texture?.Dispose();
            texture = null;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);

            textureReady = false;

            view?.Dispose();
            view = null;

            texture?.Dispose();
            texture = null;
        }
    }
}
