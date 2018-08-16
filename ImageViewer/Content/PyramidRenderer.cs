using System.Numerics;
using ImageViewer.Common;
using SharpDX.Direct3D11;
using Windows.UI.Input.Spatial;

namespace ImageViewer.Content
{
    internal class PyramidRenderer : BaseCubeRenderer
    {
        internal struct Corners
        {
            internal Corners(Vector3 origo, Vector3 topLeft, Vector3 bottomLeft)
            {
                this.origo = origo;
                this.topLeft = topLeft;
                this.bottomLeft = bottomLeft;
            }

            internal Vector3 origo;
            internal Vector3 topLeft;
            internal Vector3 bottomLeft;
        }

        private Corners corners;
        private Plane plane;
        private readonly BaseView view;

        internal bool Locked { get; set; } = false;
        internal bool Visible { get; set; } = true;

        internal PyramidRenderer(
            BaseView view,
            DeviceResources deviceResources, 
            TextureLoader loader, Corners corners)
            : base(deviceResources, loader)
        {
            this.view = view;
            this.corners = corners;
            plane = Plane.CreateFromVertices(corners.origo, corners.topLeft, corners.bottomLeft);
        }

        internal override void Render()
        {
            if (Visible)
            {
                base.Render();
            }
        }

        internal override void LoadGeometry()
        {
            VertexCube[] vertices =
            {
                new VertexCube(new Vector3(-0.005f, -0.005f, 0.03f)),
                new VertexCube(new Vector3(-0.005f,  0.005f, 0.03f)),
                new VertexCube(new Vector3( 0.005f, -0.005f, 0.03f)),
                new VertexCube(new Vector3( 0.005f,  0.005f, 0.03f)),
                new VertexCube(new Vector3( 0.0f,    0.0f,   0.0f))
            };

            vertexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.VertexBuffer, vertices));

            ushort[] vertexIndices =
            {
                1,2,0,
                3,2,1,
                1,0,4,
                3,1,4,
                2,3,4,
                0,2,4,
            };

            indexCount = vertexIndices.Length;
            indexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.IndexBuffer, vertexIndices));

            modelConstantBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }

        internal void Update(SpatialPointerPose pose)
        {
            if (!Locked && pose != null && Matrix4x4.Invert(Transformer, out Matrix4x4 inverted))
            {
                var a1 = Vector4.Transform(corners.origo, Transformer);
                var a2 = Vector4.Transform(corners.topLeft, Transformer);
                var a3 = Vector4.Transform(corners.bottomLeft, Transformer);

                var v0 = Vector4.Transform(pose.Head.Position, inverted);
                var v1 = Vector4.Transform(pose.Head.ForwardDirection, inverted);

                var p0 = new Vector3(v0.X, v0.Y, v0.Z);
                var p1 = new Vector3(v1.X, v1.Y, v1.Z);

                var n = plane.Normal;

                var s = Vector3.Dot(n, corners.origo - p0) / Vector3.Dot(n, p1);
                var ps = p0 + s * p1;          

                var sx = System.Math.Sign(ps.X);
                var ax = System.Math.Min(System.Math.Abs(ps.X), System.Math.Abs(corners.topLeft.X));

                var sy = System.Math.Sign(ps.Y);
                var ay = System.Math.Min(System.Math.Abs(ps.Y), System.Math.Abs(corners.topLeft.Y));

                Position = new Vector3(sx * ax, sy * ay, Position.Z);
                view.DebugString = Position.X.ToString("0.00") + ", " + Position.Y.ToString("0.00");
            }
        }
    }
}
