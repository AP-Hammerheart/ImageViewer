// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.ThreeD
{
    internal class NavigationRenderer : FrameRenderer
    {
        private List<NavigationTag> tags = new List<NavigationTag>();

        private readonly NavigationView view;

        private Vector3 topLeft;
        private Vector3 bottomLeft;
        private Vector3 topRight;

        private readonly int x;
        private readonly int y;
        private readonly int w;
        private readonly int h;

        private readonly float multiplierX;
        private readonly float multiplierY;

        internal NavigationRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            NavigationView view,
            float depth,
            float thickness,
            Vector3 topLeft,
            Vector3 bottomLeft,
            Vector3 topRight,
            int x = 0,
            int y = 63000,
            int w = 99840,
            int h = 99840)
            : base(deviceResources, loader, depth, thickness)
        {
            this.topLeft = topLeft;
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;

            this.view = view;

            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;

            multiplierX = (float)(Constants.TileCountX * Constants.TileResolution)
                / (float)w * Constants.HalfViewSize;
            multiplierY = (float)(Constants.TileCountY * Constants.TileResolution)
                / (float)h * Constants.HalfViewSize;
        }

        internal override void Render()
        {
            base.Render();

            for (var i = 0; i < tags.Count; i++)
            {
                tags[i].Render();
            }
        }

        internal override void SetPosition(Vector3 dp)
        {
            base.SetPosition(dp);

            topLeft += dp;
            bottomLeft += dp;
            topRight += dp;

            foreach (var tag in tags)
            {
                tag.SetPosition(dp);
            }
        }

        internal override void SetRotator(Matrix4x4 rotator)
        {
            base.SetRotator(rotator);

            foreach (var tag in tags)
            {
                tag.SetRotator(rotator);
            }
        }

        internal void UpdatePosition()
        {
            Position = GetPosition(view.CenterX, view.CenterY);
        }

        internal override void UpdateGeometry()
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

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();

            foreach (var tag in tags)
            {
                tag.ReleaseDeviceDependentResources();
            }
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);

            foreach (var tag in tags)
            {
                tag.Dispose();
            }
        }

        private Vector3 GetPosition(int xx, int yy)
        {
            var fx = (float)(xx - x) / (float)w;
            var fy = (float)(yy - y) / (float)h;

            return topLeft + fx * (topRight - topLeft) + fy * (bottomLeft - topLeft);
        }

        internal void AddTag(int xx, int yy)
        {
            var pos = GetPosition(xx, yy);

            var task = new Task(async () =>
            {
                var tag = new NavigationTag(
                    deviceResources,
                    loader,
                    GlobalRotator,
                    pos);

                await tag.CreateDeviceDependentResourcesAsync();
                tags.Add(tag);
            });

            task.Start();
        }

        internal void RemoveTag()
        {
            if (tags.Count > 0)
            {
                var tag = tags[tags.Count - 1];
                tags.RemoveAt(tags.Count - 1);
                tag.ReleaseDeviceDependentResources();
            }
        }
    }
}
