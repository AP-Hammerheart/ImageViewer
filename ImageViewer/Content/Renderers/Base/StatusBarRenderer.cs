// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
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

namespace ImageViewer.Content.Renderers.Base
{
    internal class StatusBarRenderer : BasePlaneRenderer
    {
        private readonly Vector3 bottomLeft;
        private readonly Vector3 topLeft;
        private readonly Vector3 bottomRight;
        private readonly Vector3 topRight;

        protected readonly TextureLoader loader;
        protected readonly Texture2D[] texture = new Texture2D[2];
        protected readonly ShaderResourceView[] resourceView = new ShaderResourceView[2];

        internal StatusBarRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            Tuple<Vector3, Vector3, Vector3, Vector3> corners,
            float offset)
            : base(deviceResources: deviceResources)
        {
            this.loader = loader;

            var plane = Plane.CreateFromVertices(corners.Item2, corners.Item1, corners.Item4);
            var normal = plane.Normal;
          
            bottomLeft = corners.Item1 + offset * normal;
            topLeft = corners.Item2 + offset * normal;
            bottomRight = corners.Item3 + offset * normal;
            topRight = corners.Item4 + offset * normal;

            Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser);
        }

        internal StatusBarRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight) 
            : base(deviceResources: deviceResources)
        {
            this.loader = loader;
            this.bottomLeft = bottomLeft;
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
            this.topRight = topRight;

            Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser);
        }

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
        protected bool Updating { get; set; } = false;

        internal float UBL { get; set; } = 0.0f;
        internal float VBL { get; set; } = 1.0f;
        internal float UBR { get; set; } = 1.0f;
        internal float VTL { get; set; } = 0.0f;

        internal float UTL { get; set; } = 0.0f;
        internal float VBR { get; set; } = 1.0f;
        internal float UTR { get; set; } = 1.0f;
        internal float VTR { get; set; } = 0.0f;

        internal override void LoadGeometry()
        {
            VertexPlane[] planeVertices =
            {
                new VertexPlane(bottomLeft, new Vector2(UBL, VBL)),
                new VertexPlane(topLeft, new Vector2(UTL, VTL)),
                new VertexPlane(bottomRight, new Vector2(UBR, VBR)),
                new VertexPlane(topRight, new Vector2(UTR, VTR))
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

        internal override async Task LoadTextureAsync()
        {
            using (var stream = await DrawText())
            {
                texture[0] = loader.Texture2D(deviceResources, stream);
            }

            var shaderResourceDesc = TextureLoader.ShaderDescription();
            resourceView[0] = new ShaderResourceView(
                deviceResources.D3DDevice, 
                texture[0], 
                shaderResourceDesc);

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
                texture[idx] = loader.Texture2D(deviceResources, stream);
            }

            var shaderResourceDesc = TextureLoader.ShaderDescription();
            resourceView[idx] = new ShaderResourceView(
                deviceResources.D3DDevice, 
                texture[idx], 
                shaderResourceDesc);
             
            Active = idx;

            resourceView[old]?.Dispose();
            resourceView[old] = null;
                         
            texture[old]?.Dispose();
            texture[old] = null;
                
            Updating = false;          
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
            FreeResources();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);
            FreeResources();
        }

        private void FreeResources()
        {
            Active = -1;

            for (var i = 0; i < 2; i++)
            {
                resourceView[i]?.Dispose();
                resourceView[i] = null;

                texture[i]?.Dispose();
                texture[i] = null;
            }        
        }

        protected virtual void TextLines(CanvasDrawingSession drawingSession, CanvasTextFormat format)
        {
            format.FontSize = FontSize;
            drawingSession.DrawText(Text, TextPosition, TextColor, format);
        }

        private async Task<MemoryStream> DrawText()
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
                        drawingSession.Clear(BackgroundColor);
                        using (var format = new CanvasTextFormat())
                        {
                            TextLines(drawingSession, format);         
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
