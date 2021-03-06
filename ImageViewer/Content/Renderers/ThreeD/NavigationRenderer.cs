﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Renderers.ThreeD
{
    internal class NavigationRenderer : FrameRenderer
    {
        private List<NavigationTag> tags = new List<NavigationTag>();

        private readonly NavigationView view;

        private Vector3 topLeft;
        private Vector3 bottomLeft;
        private Vector3 topRight;

        private  int x;
        private  int y;
        private  int w;
        private  int h;

        private  float multiplierX;
        private  float multiplierY;

        internal int CenterY { get; set; } = 125440 / 2;
        internal int CenterX { get; set; } = 107520 / 2;
        internal double Angle { get; set; } = 0;

        internal NavigationRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            NavigationView view,
            float depth,
            float thickness,
            Vector3 topLeft,
            Vector3 bottomLeft,
            Vector3 topRight,
            //int x = 0,
            //int y = 63000,
            //int w = 99840,
            //int h = 99840            
            int x = -512,
            int y = 1280,
            int w = 125440,
            int h = 107520,
            int angle = 0)
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


            CenterY = w / 2;
            CenterX  = h / 2;
            this.Angle = angle;

            multiplierX = (float)(Constants.TileCountX * Constants.TileResolution)
                / (float)w * Constants.HalfViewSize;
            multiplierY = (float)(Constants.TileCountY * Constants.TileResolution)
                / (float)h * Constants.HalfViewSize;

            //System.Diagnostics.Debug.WriteLine(multiplierX + ", " + multiplierY);
        }

        internal void SetNavigationArea(int x, int y, int w, int h, int Angle)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;

            CenterY = w / 2;
            CenterX = h / 2;
            this.Angle = Angle;

            multiplierX = (float)(Constants.TileCountX * Constants.TileResolution) / (float)w * Constants.HalfViewSize;
            multiplierY = (float)(Constants.TileCountY * Constants.TileResolution) / (float)h * Constants.HalfViewSize;

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

        internal void UpdatePosition( Direction direction, int number)
        {
            var moveStep = view.PixelSize(view.Level) * number;

            switch (direction)
            {
                case Direction.RIGHT:
                    CenterX += (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterY += (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
                case Direction.LEFT:
                    CenterX -= (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterY -= (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
                case Direction.UP:
                    CenterY -= (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterX += (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
                case Direction.DOWN:
                    CenterY += (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterX -= (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
            }
            Position = GetPosition(CenterX, CenterY);
        }

        internal override void UpdateGeometry()
        {
            var width = (float)(view.PixelSize(view.Level)) * multiplierX + Thickness;
            var height = (float)(view.PixelSize(view.Level)) * multiplierY + Thickness;

            //System.Diagnostics.Debug.WriteLine(width + ", " + height);

            //UpdatePosition();
            Position = GetPosition(CenterX, CenterY);

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

            vertexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
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

        internal BasePointerRenderer.Coordinate XY(D2 xy) => 
            new BasePointerRenderer.Coordinate(
                x + (int)(xy.X * w), 
                y + (int)(xy.Y * h), 
                Vector3.Zero, 
                Vector3.Zero);
    }
}
