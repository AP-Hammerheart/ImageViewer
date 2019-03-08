// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using SharpDX.Direct3D11;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Image
{
    internal class ImageRenderer : BasePlaneRenderer
    {
        private Texture2D texture = null;
        private ShaderResourceView resourceView = null;

        private readonly TextureLoader loader;
        private bool textureReady = false;



        internal ImageRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight,
            int width = 0,
            int height = 0)
            : base(deviceResources: deviceResources)
        {
            this.loader = loader;

            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            BottomRight = bottomRight;
            TopRight = topRight;

            Width = width;
            Height = height;
        }

        internal override bool TextureReady => textureReady;
        internal string TextureFile { get; set; } = "Content\\Textures\\macro.jpg";

        public Vector3 BottomLeft { get; }

        public Vector3 TopLeft { get; }

        public Vector3 BottomRight { get; }

        public Vector3 TopRight { get; }

        public int Width { get; }

        public int Height { get; }

        internal override void LoadGeometry()
        {
            VertexPlane[] planeVertices =
            {
                new VertexPlane(BottomLeft, new Vector2(0.0f, 1.0f)),
                new VertexPlane(TopLeft, new Vector2(0.0f, 0.0f)),
                new VertexPlane(BottomRight, new Vector2(1.0f, 1.0f)),
                new VertexPlane(TopRight, new Vector2(1.0f, 0.0f))
            };

            vertexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.VertexBuffer,
                planeVertices));

            ushort[] planeIndices =
            {
                1,2,0,
                3,2,1,
            };

            indexCount = planeIndices.Length;
            indexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.IndexBuffer,
                planeIndices));

            modelConstantBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            if (textureReady)
            {
                pixelShader.SetShaderResource(0, resourceView);
            }
        }

        internal override async Task LoadTextureAsync()
        {
            await base.LoadTextureAsync();

            var shaderResourceDesc = TextureLoader.ShaderDescription();
            texture = ToDispose(loader.Texture2D(deviceResources, TextureFile));
            resourceView = ToDispose(new ShaderResourceView(
                deviceResources.D3DDevice,
                texture,
                shaderResourceDesc));

            textureReady = true;
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
            RemoveAndDispose(ref texture);
            RemoveAndDispose(ref resourceView);
        }
    }
}
