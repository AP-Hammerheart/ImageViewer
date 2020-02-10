// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using Microsoft.Graphics.Canvas;
using SharpDX.Direct3D11;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;


namespace ImageViewer.Content.Renderers.ThreeD
{
    internal abstract class FrameRenderer : BasePlaneRenderer
    {
        private Texture2D texture = null;
        private ShaderResourceView resourceView = null;

        protected readonly TextureLoader loader;
        private bool textureReady = false;

        internal FrameRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            float depth,
            float thickness)
            : base(deviceResources)
        {
            this.loader = loader;
            Depth = depth;
            Thickness = thickness;
        }

        internal override bool TextureReady => textureReady;

        internal string TextureFile { get; set; } = null;

        internal Color Color { get; set; } = Colors.LightCoral;

        internal int ImageHeight { get; set; } = 256;
        internal int ImageWidth { get; set; } = 256;
        internal int ImageDPI { get; set; } = 96;

        internal float Depth { get; set; }
        internal float Thickness { get; set; }

        internal virtual void SetPosition(Vector3 dp)
        {
            Position += dp;
        }

        internal virtual void SetRotator(Matrix4x4 rotator)
        {
            GlobalRotator = rotator;
        }

        internal abstract void UpdateGeometry();
        
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
            indexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.IndexBuffer,
                vertexIndices));

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

            if (TextureFile != null)
            {
                texture = ToDispose(loader.Texture2D(deviceResources, TextureFile));
            }
            else
            {
                using (var stream = await DrawImage())
                {
                    texture = ToDispose(loader.Texture2D(deviceResources, stream));
                }
            }

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

        private async Task<MemoryStream> DrawImage()
        {
            using (var device = new CanvasDevice())
            {
                using (var renderTarget = new CanvasRenderTarget(
                    device,
                    ImageWidth,
                    ImageHeight,
                    ImageDPI))
                {
                    using (var drawingSession = renderTarget.CreateDrawingSession())
                    {
                        drawingSession.Clear(Color);
                    }

                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png, 1f);

                        var memoryStream = new MemoryStream();
                        await stream.AsStream().CopyToAsync(memoryStream);

                        return memoryStream;
                    }
                }
            }
        }
    }
}
