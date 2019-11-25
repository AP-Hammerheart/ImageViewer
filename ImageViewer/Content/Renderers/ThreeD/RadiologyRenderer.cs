using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Image {
    internal class RadiologyRenderer : BasePlaneRenderer {

        private readonly TextureLoader loader;

        internal RadiologyRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight,
            int width = 0,
            int height = 0 )
            : base( deviceResources: deviceResources ) {
            this.loader = loader;

            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            BottomRight = bottomRight;
            TopRight = topRight;

            Width = width;
            Height = height;
        }

        internal string TextureID { get; set; } = "YOUR DEFAULT URL";

        internal override bool TextureReady => loader.TextureReady( TextureID );

        public Vector3 BottomLeft {
            get;
        }

        public Vector3 TopLeft {
            get;
        }

        public Vector3 BottomRight {
            get;
        }

        public Vector3 TopRight {
            get;
        }

        public int Width {
            get;
        }

        public int Height {
            get;
        }

        internal override void LoadGeometry() {
            VertexPlane[] planeVertices =
            {
                new VertexPlane(BottomLeft, new Vector2(0.0f, 1.0f)),
                new VertexPlane(TopLeft, new Vector2(0.0f, 0.0f)),
                new VertexPlane(BottomRight, new Vector2(1.0f, 1.0f)),
                new VertexPlane(TopRight, new Vector2(1.0f, 0.0f))
            };

            vertexBuffer = ToDispose( SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.VertexBuffer,
                planeVertices ) );

            ushort[] planeIndices =
            {
                1,2,0,
                3,2,1,
            };

            indexCount = planeIndices.Length;
            indexBuffer = ToDispose( SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.IndexBuffer,
                planeIndices ) );

            modelConstantBuffer = ToDispose( SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData ) );
        }

        internal override void SetTextureResource( PixelShaderStage pixelShader ) {
            loader.SetTextureResource( pixelShader, TextureID );
        }

        internal override async Task LoadTextureAsync() {
            await base.LoadTextureAsync();

            var list = new List<string>();
            list.Add( TextureID );
            await loader.LoadTexturesAsync( list );
        }
    }
}

