// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Views;
using SharpDX.Direct3D11;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers
{
    internal class FrameRenderer : BasePlaneRenderer
    {
        private Texture2D texture = null;
        private ShaderResourceView resourceView = null;

        private readonly BaseView view;
        protected readonly TextureLoader loader;
        private bool textureReady = false;

        private Vector3 topLeft;
        private Vector3 bottomLeft;
        private Vector3 topRight;

        private readonly int x;
        private readonly int y;
        private readonly int w;
        private readonly int h;

        private readonly float multiplierX;
        private readonly float multiplierY;

        internal FrameRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            BaseView view,
            float depth,
            float thickness, 
            Vector3 topLeft,
            Vector3 bottomLeft,
            Vector3 topRight,
            int x = 0,
            int y = 63000,
            int w = 99840,
            int h = 99840)
            : base(deviceResources)
        {
            this.loader = loader;
            Depth = depth;
            Thickness = thickness;

            this.topLeft = topLeft;
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;

            this.view = view;

            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;

            multiplierX = (float)(view.TileCountX * view.TileResolution) 
                / (float)w * Constants.HalfViewSize;
            multiplierY = (float)(view.TileCountY * view.TileResolution) 
                / (float)h * Constants.HalfViewSize;
        }

        internal override bool TextureReady => textureReady;

        internal string TextureFile { get; set; } = "Content\\Textures\\solid.png";

        internal float Depth { get; set; }
        internal float Thickness { get; set; }

        internal void SetPosition(Vector3 dp)
        {
            Position += dp;

            topLeft += dp;
            bottomLeft += dp;
            topRight += dp;
        }

        internal void UpdatePosition()
        {
            var fx = (float)(view.CenterX - x) / (float)w;
            var fy = (float)(view.CenterY - y) / (float)h;

            Position = topLeft + fx * (topRight - topLeft) + fy * (bottomLeft - topLeft);
        }

        internal void UpdateGeometry()
        {
            var width = (float)(view.PixelSize(view.Level)) * multiplierX + Thickness;
            var height = (float)(view.PixelSize(view.Level)) * multiplierY + Thickness;

            UpdatePosition();

            if (vertexBuffer != null)
            {
                RemoveAndDispose(ref vertexBuffer);
            }

            var rot = Quaternion.CreateFromAxisAngle(new Vector3(0f, 0f, 1f), (float)view.Angle);

            VertexPlane[] vertices =
            {
                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, -1.0f * height, 0.0f), rot), new Vector2(0f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, -1.0f * height, Depth), rot), new Vector2(1f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(width, -1.0f * height, 0.0f), rot), new Vector2(0f,1f)),
                new VertexPlane(Vector3.Transform(new Vector3(width, -1.0f * height, Depth), rot), new Vector2(1f,1f)),

                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, height, 0.0f), rot), new Vector2(0f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, height, Depth), rot), new Vector2(1f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(width, height, 0.0f), rot), new Vector2(0f,1f)),
                new VertexPlane(Vector3.Transform(new Vector3(width, height, Depth), rot), new Vector2(1f,1f)),

                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, -1.0f * height + Thickness, 0.0f), rot), new Vector2(0f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, -1.0f * height + Thickness, Depth), rot), new Vector2(1f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, -1.0f * height + Thickness, 0.0f), rot), new Vector2(0f,1f)),
                new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, -1.0f * height + Thickness, Depth), rot), new Vector2(1f,1f)),

                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, height - Thickness, 0.0f), rot), new Vector2(0f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, height - Thickness, Depth), rot), new Vector2(1f,0f)),
                new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, height - Thickness, 0.0f), rot), new Vector2(0f,1f)),
                new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, height - Thickness, Depth), rot), new Vector2(1f,1f)),
            };

            vertexBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.VertexBuffer,
                vertices));
        }

        internal override void LoadGeometry()
        {
            UpdateGeometry();

            ushort[] vertexIndices =
            {
                1,2,0,
                3,2,1,

                6,5,4,
                6,7,5,

                10,9,8,
                10,11,9,

                13,14,12,
                15,14,13,

                1,0,4,
                1,4,5,

                2,3,6,
                6,3,7,

                8,9,12,
                12,9,13,

                11,10,14,
                11,14,15,

                0,2,8,
                8,2,10,

                3,1,9,
                3,9,11,

                6,4,12,
                6,12,14,

                5,7,13,
                13,7,15,

                4,0,8,
                4,8,12,

                1,5,9,
                9,5,13,

                2,6,10,
                10,6,14,

                7,3,11,
                7,11,15,
            };

            indexCount = vertexIndices.Length;
            indexBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.IndexBuffer,
                vertexIndices));

            modelConstantBuffer = ToDispose(Buffer.Create(
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
