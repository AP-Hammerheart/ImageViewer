using ImageViewer.Common;
using ImageViewer.Content.Utils;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Renderers.ThreeD
{
    class NavigationFrameRenderer : FrameRenderer
    {
        private Vector3 topLeft;
        private Vector3 bottomLeft;
        private Vector3 topRight;
        private Vector3 bottomRight;
        int x, y, w, h;
        int centerX, centerY;
        int angle;

        float multiplierX;
        float multiplierY;

        internal NavigationFrameRenderer(DeviceResources deviceResources,
            TextureLoader loader,
            float depth,
            float thickness,
            Vector3 topLeft,
            Vector3 bottomLeft,
            Vector3 topRight,
            //Vector3 bottomRight,
            int x = 0,
            int y = 0,
            int w = 1000,
            int h = 1000,
            int angle = 0)
            : base(deviceResources, loader, depth, thickness)
        {
            this.topLeft = topLeft;
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;

            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;

            centerY = w / 2;
            centerX = h / 2;
            this.angle = angle;


            multiplierX = (float)(Constants.TileCountX * Constants.TileResolution) / (float)w * Constants.HalfViewSize;
            multiplierY = (float)(Constants.TileCountY * Constants.TileResolution) / (float)h * Constants.HalfViewSize;
            System.Diagnostics.Debug.WriteLine(multiplierX + ", " + multiplierY);
        }
        internal void SetNavigationArea(int x, int y, int w, int h, int Angle)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;

            centerY = w / 2;
            centerX = h / 2;
            this.angle = Angle;

            multiplierX = (float)(Constants.TileCountX * Constants.TileResolution) / (float)w * Constants.HalfViewSize;
            multiplierY = (float)(Constants.TileCountY * Constants.TileResolution) / (float)h * Constants.HalfViewSize;
            System.Diagnostics.Debug.WriteLine(multiplierX + ", " + multiplierY);
        }

        internal override void SetPosition(Vector3 dp)
        {
            base.SetPosition(dp);

            topLeft += dp;
            bottomLeft += dp;
            topRight += dp;
        }

        internal override void SetRotator(Matrix4x4 rotator)
        {
            base.SetRotator(rotator);
        }

        private Vector3 GetPosition(int xx, int yy)
        {
            var fx = (float)(xx - x) / (float)w;
            var fy = (float)(yy - y) / (float)h;

            return topLeft + fx * (topRight - topLeft) + fy * (bottomLeft - topLeft);
        }

        internal int PixelSize(int level)
    => (int)Math.Pow(2, Settings.Multiplier * level);

        internal void UpdatePosition(Direction direction, int number)
        {
            var moveStep = PixelSize(7) * number;

            switch (direction)
            {
                case Direction.RIGHT:
                    centerX += (int)(Math.Cos(-1 * angle) * moveStep);
                    centerY += (int)(Math.Sin(-1 * angle) * moveStep);
                    break;
                case Direction.LEFT:
                    centerX -= (int)(Math.Cos(-1 * angle) * moveStep);
                    centerY -= (int)(Math.Sin(-1 * angle) * moveStep);
                    break;
                case Direction.UP:
                    centerY -= (int)(Math.Cos(-1 * angle) * moveStep);
                    centerX += (int)(Math.Sin(-1 * angle) * moveStep);
                    break;
                case Direction.DOWN:
                    centerY += (int)(Math.Cos(-1 * angle) * moveStep);
                    centerX -= (int)(Math.Sin(-1 * angle) * moveStep);
                    break;
            }
            Position = GetPosition(centerX, centerY);
        }

        internal override void UpdateGeometry()
        {
            var width = (float)(PixelSize(9)) * multiplierX + Thickness;
            var height = (float)(PixelSize(9)) * multiplierY + Thickness;

            System.Diagnostics.Debug.WriteLine(width + ", " + height);

            //UpdatePosition();
            Position = GetPosition(centerX, centerY);

            if (vertexBuffer != null)
            {
                RemoveAndDispose(ref vertexBuffer);
            }

            var rot = Quaternion.CreateFromAxisAngle(new Vector3(0f, 0f, 1f), (float)angle);

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

            vertexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.VertexBuffer,
                vertices));
        }

        internal override void Render()
        {
            base.Render();
        }

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);
        }
    }
}
