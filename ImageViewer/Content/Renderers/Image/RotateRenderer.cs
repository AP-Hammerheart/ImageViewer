// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using SharpDX.Direct3D11;

namespace ImageViewer.Content.Renderers.Image
{
    internal class RotateRenderer : PlaneRenderer
    {
        public RotateRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            string url,
            VertexPlane[] planeVertices,
            ushort[] planeIndices)
            : base(deviceResources, loader, url)
        {
            this.PlaneVertices = planeVertices;
            this.PlaneIndices = planeIndices;
        }

        internal override void UpdateGeometry()
        {
            if (PlaneVertices == null || PlaneIndices == null)
            {
                return;
            }

            if (vertexBuffer != null)
            {
                RemoveAndDispose(ref vertexBuffer);
            }

            if (indexBuffer != null)
            {
                RemoveAndDispose(ref indexBuffer);
            }

            vertexBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice, 
                BindFlags.VertexBuffer, 
                PlaneVertices));

            indexCount = PlaneIndices.Length;
            indexBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice, 
                BindFlags.IndexBuffer, 
                PlaneIndices));       
        }

        internal override void LoadGeometry()
        {
            UpdateGeometry();

            modelConstantBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }

        internal override bool TextureReady => loader.TextureReady(TextureID);

        internal VertexPlane[] PlaneVertices { get; set; }
        public ushort[] PlaneIndices { get; set; }

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            loader.SetTextureResource(pixelShader, TextureID);
        }
    }
}

