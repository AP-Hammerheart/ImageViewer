// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using SharpDX.Direct3D11;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class TileRenderer : PlaneRenderer
    {
        internal float TileSize { get; }

        public TileRenderer(DeviceResources deviceResources, TextureLoader loader, string url, float tileSize = 0.1f)
            : base(deviceResources, loader, url) => TileSize = tileSize;

        internal override void LoadGeometry()
        {
            VertexPlane[] planeVertices =
            {
                new VertexPlane(new Vector3(-0.5f * TileSize, -0.5f * TileSize, 0.0f), new Vector2(0.0f, 1.0f)),
                new VertexPlane(new Vector3(-0.5f * TileSize,  0.5f * TileSize, 0.0f), new Vector2(0.0f, 0.0f)),
                new VertexPlane(new Vector3( 0.5f * TileSize, -0.5f * TileSize, 0.0f), new Vector2(1.0f, 1.0f)),
                new VertexPlane(new Vector3( 0.5f * TileSize,  0.5f * TileSize, 0.0f), new Vector2(1.0f, 0.0f))
            };

            vertexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.VertexBuffer, planeVertices));

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

        internal override bool TextureReady => loader.TextureReady(TextureID);

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            loader.SetTextureResource(pixelShader, TextureID);
        }

        internal override async Task LoadTextureAsync()
        {
            try
            {
                await loader.LoadTextureAsync(TextureID);
            }
            catch (System.Exception)
            {
                // Delete corrupted cache file
                await loader.DeleteCacheFile(TextureID, ".PNG");
            }
        }
    }
}
