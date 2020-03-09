using ImageViewer.Common;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
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

        public Label[] labels;
        int zoomLevel = 7;

        float multiplierX;
        float multiplierY;

        internal NavigationFrameRenderer(DeviceResources deviceResources,
            TextureLoader loader,
            float depth,
            float thickness,
            Label[] labels,
            NavigationView view,
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

            Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser);
            //this.x = x;
            //this.y = y;
            //this.w = w;
            //this.h = h;

            //centerY = h/2;
            //centerX = w/2;
            //this.angle = angle;
            //System.Diagnostics.Debug.WriteLine(Position.ToString());
        }

        //int index = 0;
        internal void SetNavigationArea(Label label)
        {
            //this.topLeft = labels[index].Frame.BottomLeft;
            //this.bottomLeft = labels[index].Frame.BottomRight;
            //this.topRight = labels[index].Frame.TopLeft;
            //this.bottomRight = labels[index].Frame.TopRight;


            this.topLeft = label.Frame.BottomLeft + dpPos;
            this.bottomLeft = label.Frame.BottomRight + dpPos;
            this.topRight = label.Frame.TopLeft + dpPos;
            this.bottomRight = label.Frame.TopRight + dpPos;
            zoomLevel = 7;
            //this.x = x;
            //this.y = y;
            //this.w = w;
            //this.h = h;
            //System.Diagnostics.Debug.WriteLine(Position.ToString());

            centerX = (h / 2) + y;
            centerY = (w / 2) + x;
            System.Diagnostics.Debug.WriteLine("center: " + centerX + ", " + centerY);
            //this.angle = Angle;
            Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser);

            //System.Diagnostics.Debug.WriteLine("tl: " + topLeft.ToString() + " bl: " + bottomLeft.ToString() + "tr: " + topRight.ToString());
            System.Diagnostics.Debug.WriteLine("tl: " + labels[0].Frame.TopLeft.ToString() + 
                " bl: " + labels[0].Frame.BottomLeft.ToString() + 
                " tr: " + labels[0].Frame.TopRight.ToString() +
                " br: " + labels[0].Frame.BottomRight.ToString() );
            multiplierX = (float)(Constants.TileCountX * Constants.TileResolution) / (float)w * Constants.HalfViewSize;
            multiplierY = (float)(Constants.TileCountY * Constants.TileResolution) / (float)h * Constants.HalfViewSize;
            System.Diagnostics.Debug.WriteLine("multiplier: " + multiplierX + ", " + multiplierY);
        }

        Vector3 dpPos = Vector3.Zero;
        internal override void SetPosition(Vector3 dp)
        {
            base.SetPosition(dp);

            topLeft += dp;
            bottomLeft += dp;
            topRight += dp;
            bottomRight += dp;

            dpPos += dp;
        }

        internal override void SetRotator(Matrix4x4 rotator)
        {
            base.SetRotator(rotator);
        }

        internal void UpdatePosition(Direction direction, int number)
        {
            Vector3 move = Vector3.Zero;
            switch (direction)
            {
                case Direction.RIGHT:
                    move = new Vector3(0f, 0f, -0.001f);
                    break;
                case Direction.LEFT:
                    move = new Vector3(0f, 0f, 0.001f);
                    break;
                case Direction.UP:
                    move = new Vector3(0f, 0.001f, 0);
                    break;
                case Direction.DOWN:
                    move = new Vector3(0f, -0.001f, 0);
                    break;
            }
            Position += move;
        }

        internal void Scale(Direction direction)
        {
            string s = " 0.00" + zoomLevel.ToString();
            float scaler = float.Parse(s); //0.01f;
            switch (direction)
            {
                case Direction.UP:
                    zoomLevel--;
                    if(zoomLevel < 0) {
                        zoomLevel=0;
                        break;
                    }
                    this.topLeft = new Vector3(topLeft.X, topLeft.Y - scaler, topLeft.Z - scaler);
                    this.topRight = new Vector3(topRight.X, topRight.Y - scaler, topRight.Z + scaler);
                    this.bottomLeft = new Vector3(bottomLeft.X, bottomLeft.Y + scaler, bottomLeft.Z - scaler);
                    this.bottomRight = new Vector3(bottomRight.X, bottomRight.Y + scaler, bottomRight.Z + scaler);
                    break;
                case Direction.DOWN:
                    zoomLevel++;
                    if(zoomLevel > 10) {
                        zoomLevel=10;
                        break;
                    }
                    this.topLeft = new Vector3(topLeft.X, topLeft.Y + scaler, topLeft.Z + scaler);
                    this.topRight = new Vector3(topRight.X, topRight.Y + scaler, topRight.Z - scaler);
                    this.bottomLeft = new Vector3(bottomLeft.X, bottomLeft.Y - scaler, bottomLeft.Z + scaler);
                    this.bottomRight = new Vector3(bottomRight.X, bottomRight.Y - scaler, bottomRight.Z - scaler);
                    break;
            }
            System.Diagnostics.Debug.WriteLine("zoomLevel: " + zoomLevel + " " + direction.ToString());
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
        }

        internal override void Render()
        {
            if(KeyEventView.mode != KeyEventView.inputModes.caseSelection) {
                base.Render();
            }
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
