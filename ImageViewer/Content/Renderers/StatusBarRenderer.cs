// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using SharpDX.Direct3D11;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;

namespace ImageViewer.Content
{
    internal class StatusBarRenderer : BaseRenderer
    {
        private readonly Vector3 bottomLeft = new Vector3(-0.5f, 0.25f, 0.0f);
        private readonly Vector3 topLeft = new Vector3(-0.5f, 0.30f, 0.0f);
        private readonly Vector3 bottomRight = new Vector3(0.5f, 0.25f, 0.0f);
        private readonly Vector3 topRight = new Vector3(0.5f, 0.30f, 0.0f);

        protected bool updating = false;

        protected readonly TextureLoader loader;
        protected Texture2D[] texture2D = new Texture2D[2];
        protected ShaderResourceView[] resourceView = new ShaderResourceView[2];

        internal StatusBarRenderer(DeviceResources deviceResources, TextureLoader loader) : base(
            deviceResources: deviceResources,
            vertexShader: "Content\\Shaders\\VertexShaderPlane.cso",
            VPRTvertexShader: "Content\\Shaders\\VPRTVertexShaderPlane.cso",
            geometryShader: "Content\\Shaders\\GeometryShaderPlane.cso",
            pixelShader: "Content\\Shaders\\PixelShaderPlane.cso") => this.loader = loader;

        internal StatusBarRenderer(
          DeviceResources deviceResources,
          TextureLoader loader,
          Vector3 bottomLeft,
          Vector3 topLeft,
          Vector3 bottomRight,
          Vector3 topRight) : 
            base(
                deviceResources: deviceResources,
                vertexShader: "Content\\Shaders\\VertexShaderPlane.cso",
                VPRTvertexShader: "Content\\Shaders\\VPRTVertexShaderPlane.cso",
                geometryShader: "Content\\Shaders\\GeometryShaderPlane.cso",
                pixelShader: "Content\\Shaders\\PixelShaderPlane.cso")
        {
            this.loader = loader;
            this.bottomLeft = bottomLeft;
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
            this.topRight = topRight;
        }

        internal override InputElement[] InputElement => new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };

        internal override int VertexSize => SharpDX.Utilities.SizeOf<VertexPlane>();
        internal override bool TextureReady => Active != -1;
        internal string Text { get; set; } = "";
        internal Vector2 TextPosition { get; set; } = new Vector2(10, 15);
        internal Color TextColor { get; set; } = Colors.Black;
        internal Color BackgroundColor { get; set; } = Colors.Gray;
        internal float FontSize { get; set; } = 36.0f;
        internal int ImageHeight { get; set; } = 80;
        internal int ImageWidth { get; set; } = 1600;
        internal int ImageDPI { get; set; } = 96;
        protected int Active { get; set; } = -1;

        internal override void LoadGeometry()
        {
            VertexPlane[] planeVertices =
            {
                new VertexPlane(bottomLeft, new Vector2(0.0f, 1.0f)),
                new VertexPlane(topLeft, new Vector2(0.0f, 0.0f)),
                new VertexPlane(bottomRight, new Vector2(1.0f, 1.0f)),
                new VertexPlane(topRight, new Vector2(1.0f, 0.0f))
            };

            vertexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(deviceResources.D3DDevice, BindFlags.VertexBuffer, planeVertices));

            ushort[] planeIndices =
            {
                1,2,0,
                3,2,1,
            };

            indexCount = planeIndices.Length;
            indexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(deviceResources.D3DDevice, BindFlags.IndexBuffer, planeIndices));

            modelConstantBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }

        internal override async Task LoadTextureAsync()
        {
            using (var stream = await DrawText())
            {
                texture2D[0] = loader.Texture2D(deviceResources, stream);
            }

            var shaderResourceDesc = TextureLoader.ShaderDescription();
            resourceView[0] = new ShaderResourceView(deviceResources.D3DDevice, texture2D[0], shaderResourceDesc);

            Active = 0;
        }

        protected async Task UpdateTextureAsync()
        {
            if (Active == -1) return;

            var old = Active;
            var idx = Active == 0 ? 1 : 0;

            using (var stream = await DrawText())
            {
                if (stream == null) return;
                texture2D[idx] = loader.Texture2D(deviceResources, stream);
            }

            var shaderResourceDesc = TextureLoader.ShaderDescription();
            resourceView[idx] = new ShaderResourceView(deviceResources.D3DDevice, texture2D[idx], shaderResourceDesc);
             
            Active = idx;

            resourceView[old]?.Dispose();
            resourceView[old] = null;
                         
            texture2D[old]?.Dispose();
            texture2D[old] = null;
                
            updating = false;          
        }

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            if (Active != -1)
            {
                pixelShader.SetShaderResource(0, resourceView[Active]);
            }      
        }

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();

            Active = -1;

            resourceView[0]?.Dispose();
            resourceView[1]?.Dispose();

            texture2D[0]?.Dispose();
            texture2D[1]?.Dispose();

            resourceView[0] = null;
            resourceView[1] = null;

            texture2D[0] = null;
            texture2D[1] = null;
        }

        private async Task<MemoryStream> DrawText()
        {
            using (var device = new CanvasDevice())
            {
                using (var renderTarget = new CanvasRenderTarget(device, ImageWidth, ImageHeight, ImageDPI))
                {
                    using (var drawingSession = renderTarget.CreateDrawingSession())
                    {
                        drawingSession.Clear(BackgroundColor);
                        using (var format = new CanvasTextFormat())
                        {
                            format.FontSize = FontSize;
                            drawingSession.DrawText(Text, TextPosition, TextColor, format);
                        }
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
