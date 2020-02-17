using ImageViewer.Common;
using ImageViewer.Content.Renderers.Image;
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
        Label[] labels;

        float multiplierX;
        float multiplierY;

        internal NavigationFrameRenderer(DeviceResources deviceResources,
            TextureLoader loader,
            float depth,
            float thickness,
            Label[] labels,
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

            this.labels = labels;
            //this.topLeft = labels[0].Frame.TopLeft;
            //this.bottomLeft = labels[0].Frame.BottomLeft;
            //this.topRight = labels[0].Frame.TopRight;
            //this.bottomRight = labels[0].Frame.BottomRight;

            //this.topLeft = topLeft;
            //this.bottomLeft = bottomLeft;
            //this.topRight = topRight;

            //System.Diagnostics.Debug.WriteLine("Input: "+"tl: " + topLeft.ToString() + " bl: " + bottomLeft.ToString() + "tr: " + topRight.ToString());
            //System.Diagnostics.Debug.WriteLine("Label: "+"tl: " + labels[0].Frame.TopLeft.ToString() + " bl: " + labels[0].Frame.BottomLeft.ToString() + "tr: " + labels[0].Frame.TopRight.ToString());

            //Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser);
            //this.x = x;
            //this.y = y;
            //this.w = w;
            //this.h = h;

            //centerY = h/2;
            //centerX = w/2;
            //this.angle = angle;
            //System.Diagnostics.Debug.WriteLine(Position.ToString());
        }
        internal void SetNavigationArea(int x, int y, int w, int h, int Angle)
        {

            this.topLeft = labels[0].Frame.TopLeft;
            this.bottomLeft = labels[0].Frame.BottomLeft;
            this.topRight = labels[0].Frame.TopRight;
            this.bottomRight = labels[0].Frame.BottomRight;

            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            ////System.Diagnostics.Debug.WriteLine(Position.ToString());
            centerY = h / 2;
            centerX = w / 2;
            this.angle = Angle;
            //Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser);

            //System.Diagnostics.Debug.WriteLine("tl: " + topLeft.ToString() + " bl: " + bottomLeft.ToString() + "tr: " + topRight.ToString());
            //System.Diagnostics.Debug.WriteLine("tl: " + labels[0].Frame.TopLeft.ToString() + " bl: " + labels[0].Frame.BottomLeft.ToString() + "tr: " + labels[0].Frame.ToString());
            //multiplierX = (float)(Constants.TileCountX * Constants.TileResolution) / (float)w * Constants.HalfViewSize;
            //multiplierY = (float)(Constants.TileCountY * Constants.TileResolution) / (float)h * Constants.HalfViewSize;
            ////System.Diagnostics.Debug.WriteLine(multiplierX + ", " + multiplierY);
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
    => (int)Math.Pow(2, Settings.Multiplier * /*level*/0);

        internal void UpdatePosition(Direction direction, int number)
        {
            var moveStep = /*PixelSize(9) * */ number;

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
            if (vertexBuffer != null)
            {
                RemoveAndDispose(ref vertexBuffer);
            }

            var plane = Plane.CreateFromVertices(topLeft, bottomLeft, topRight);
            var normal = plane.Normal;

            var x_axis = Vector3.Normalize((topRight - topLeft));
            var y_axis = Vector3.Normalize((topLeft - bottomLeft));

            VertexPlane[] vertices =
            {
                new VertexPlane(bottomLeft - Thickness * x_axis - Thickness * y_axis, new Vector2(0f,0f)),
                new VertexPlane(bottomLeft - Thickness * x_axis - Thickness * y_axis + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(bottomRight + Thickness * x_axis - Thickness * y_axis, new Vector2(0f,1f)),
                new VertexPlane(bottomRight + Thickness * x_axis - Thickness * y_axis + Depth * normal, new Vector2(1f,1f)),

                new VertexPlane(topLeft - Thickness * x_axis + Thickness * y_axis, new Vector2(0f,0f)),
                new VertexPlane(topLeft - Thickness * x_axis + Thickness * y_axis + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(topRight + Thickness * x_axis + Thickness * y_axis, new Vector2(0f,1f)),
                new VertexPlane(topRight + Thickness * x_axis + Thickness * y_axis + Depth * normal, new Vector2(1f,1f)),

                new VertexPlane(bottomLeft, new Vector2(0f,0f)),
                new VertexPlane(bottomLeft + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(bottomRight, new Vector2(0f,1f)),
                new VertexPlane(bottomRight + Depth * normal, new Vector2(1f,1f)),

                new VertexPlane(topLeft, new Vector2(0f,0f)),
                new VertexPlane(topLeft + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(topRight, new Vector2(0f,1f)),
                new VertexPlane(topRight + Depth * normal, new Vector2(1f,1f)),
            };

            vertexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.VertexBuffer,
                vertices));


            //var width = Vector3.Distance(topLeft, topRight) + /* (float)(PixelSize(9)) *  multiplierX +*/ Thickness;
            //var height = Vector3.Distance(topLeft, topRight) + /*(float)(PixelSize(9)) * multiplierY +*/ Thickness;

            //System.Diagnostics.Debug.WriteLine("UpdateGeomentry - w:" + width + ", h:" + height);

            //Position = GetPosition(centerX, centerY);
            //System.Diagnostics.Debug.WriteLine("UpdateGeomentry - cx:" + centerX + ", cy:" + centerY + ", position: " + Position.ToString());

            //if (vertexBuffer != null)
            //{
            //    RemoveAndDispose(ref vertexBuffer);
            //}

            //var rot = Quaternion.CreateFromAxisAngle(new Vector3(0f, 0f, 1f), (float)angle);

            //VertexPlane[] vertices =
            //{
            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, -1.0f * height, 0.0f), rot), new Vector2(0f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, -1.0f * height, Depth), rot), new Vector2(1f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width, -1.0f * height, 0.0f), rot), new Vector2(0f,1f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width, -1.0f * height, Depth), rot), new Vector2(1f,1f)),

            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, height, 0.0f), rot), new Vector2(0f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width, height, Depth), rot), new Vector2(1f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width, height, 0.0f), rot), new Vector2(0f,1f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width, height, Depth), rot), new Vector2(1f,1f)),

            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, -1.0f * height + Thickness, 0.0f), rot), new Vector2(0f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, -1.0f * height + Thickness, Depth), rot), new Vector2(1f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, -1.0f * height + Thickness, 0.0f), rot), new Vector2(0f,1f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, -1.0f * height + Thickness, Depth), rot), new Vector2(1f,1f)),

            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, height - Thickness, 0.0f), rot), new Vector2(0f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(-1.0f * width + Thickness, height - Thickness, Depth), rot), new Vector2(1f,0f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, height - Thickness, 0.0f), rot), new Vector2(0f,1f)),
            //    new VertexPlane(Vector3.Transform(new Vector3(width - Thickness, height - Thickness, Depth), rot), new Vector2(1f,1f)),
            //};

            //vertexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
            //    deviceResources.D3DDevice,
            //    BindFlags.VertexBuffer,
            //    vertices));
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
