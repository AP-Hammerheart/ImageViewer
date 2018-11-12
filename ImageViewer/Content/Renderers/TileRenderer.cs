// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using SharpDX.Direct3D11;
using System.Numerics;

namespace ImageViewer.Content.Renderers
{
    internal class TileRenderer : PlaneRenderer
    {
        internal float TileSize { get; }

        internal float X0 { get; set; }
        internal float Y0 { get; set; }
        internal float X1 { get; set; }
        internal float Y1 { get; set; }
        internal float U0 { get; set; } = 0.0f;
        internal float V0 { get; set; } = 1.0f;
        internal float U1 { get; set; } = 1.0f;
        internal float V1 { get; set; } = 0.0f;

        public TileRenderer(DeviceResources deviceResources, TextureLoader loader, string url, float tileSize)
            : base(deviceResources, loader, url)
        {
            TileSize = tileSize;

            X0 = -0.5f * TileSize;
            Y0 = -0.5f * TileSize;
            X1 = 0.5f * TileSize;
            Y1 = 0.5f * TileSize;
        }

        internal override void UpdateGeometry()
        {
            if (vertexBuffer != null)
            {
                RemoveAndDispose(ref vertexBuffer);
            }

            VertexPlane[] planeVertices =
            {
                new VertexPlane(new Vector3(X0, Y0, 0.0f), new Vector2(U0, V0)),
                new VertexPlane(new Vector3(X0, Y1, 0.0f), new Vector2(U0, V1)),
                new VertexPlane(new Vector3(X1, Y0, 0.0f), new Vector2(U1, V0)),
                new VertexPlane(new Vector3(X1, Y1, 0.0f), new Vector2(U1, V1))
            };

            vertexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.VertexBuffer, planeVertices));
        }

        internal override bool TextureReady => loader.TextureReady(TextureID);

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            loader.SetTextureResource(pixelShader, TextureID);
        }
    }
}
