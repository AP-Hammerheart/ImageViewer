using ImageViewer.Common;
using SharpDX.Direct3D11;
using System.Numerics;

namespace ImageViewer.Content
{
    internal class TileRenderer : PlaneRenderer
    {
        internal static float TileSize { get; } = 0.1f;

        public TileRenderer(DeviceResources deviceResources, TextureLoader loader, string url)
            : base(deviceResources, loader, url)
        {
        }

        internal override void LoadGeometry()
        {
            VertexPlane[] planeVertices =
            {
                new VertexPlane(new Vector3(-0.5f * TileSize, -0.5f * TileSize, 0.0f), new Vector2(0.0f, 0.0f)),
                new VertexPlane(new Vector3(-0.5f * TileSize,  0.5f * TileSize, 0.0f), new Vector2(0.0f, 1.0f)),
                new VertexPlane(new Vector3( 0.5f * TileSize, -0.5f * TileSize, 0.0f), new Vector2(1.0f, 0.0f)),
                new VertexPlane(new Vector3( 0.5f * TileSize,  0.5f * TileSize, 0.0f), new Vector2(1.0f, 1.0f))
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
    }
}
