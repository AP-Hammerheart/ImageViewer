// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Input.Spatial;

namespace ImageViewer.Content
{
    internal class PointerRenderer : PyramidRenderer
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

        private readonly BaseView view;
        private readonly Vector3 normal;
        private readonly Corners corners;

        private List<Tag> tags = new List<Tag>();

        internal bool Locked { get; set; } = false;
        internal bool Visible { get; set; } = true;

        internal PointerRenderer(
            BaseView view,
            DeviceResources deviceResources, 
            TextureLoader loader, 
            Corners corners)
            : base(deviceResources, loader)
        {
            this.view = view;
            this.corners = corners;
            var plane = Plane.CreateFromVertices(corners.origo, corners.topLeft, corners.bottomLeft);
            normal = plane.Normal;
        }

        internal void AddTag()
        {
            var pos = Position;
            pos.X = Position.X <= 0 ? Position.X + Math.Abs(corners.topLeft.X) : Position.X - Math.Abs(corners.topLeft.X);

            var pL = Position.X <= 0 ? Position : pos;
            var pR = Position.X <= 0 ? pos : Position;

            var v = (1.0f / BaseView.ViewSize) * view.Step;

            var X = view.ImageX + (int)(v * (pL.X - corners.topLeft.X));
            var Y = view.ImageY + (int)(v * (corners.topLeft.Y - pL.Y));

            var task = new Task(async () =>
            {
                var tag = new Tag(deviceResources, loader, X, Y, this.Transformer, pL, pR);
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

        internal override void Render()
        {
            if (Visible)
            {
                base.Render();
            }

            for (var i=0; i < tags.Count; i++)
            {
                tags[i].Render();
            }
        }

        internal void Update()
        {
            foreach (var tag in tags)
            {
                tag.Update(view, corners.topLeft);
            }
        }

        internal void Update(SpatialPointerPose pose)
        {
            if (!Locked && pose != null && Matrix4x4.Invert(Transformer, out Matrix4x4 inverted))
            {
                var v0 = Vector4.Transform(pose.Head.Position, inverted);
                var v1 = Vector4.Transform(pose.Head.ForwardDirection, inverted);

                var p0 = new Vector3(v0.X, v0.Y, v0.Z);
                var p1 = new Vector3(v1.X, v1.Y, v1.Z);

                var s = Vector3.Dot(normal, corners.origo - p0) / Vector3.Dot(normal, p1);
                var ps = p0 + s * p1;

                var sx = Math.Sign(ps.X);
                var ax = Math.Min(Math.Abs(ps.X), Math.Abs(corners.topLeft.X));

                var sy = Math.Sign(ps.Y);
                var ay = Math.Min(Math.Abs(ps.Y), Math.Abs(corners.topLeft.Y));

                Position = new Vector3(sx * ax, sy * ay, Position.Z);
                view.DebugString = Position.X.ToString("0.00") + ", " + Position.Y.ToString("0.00");
            }
        }

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();
            foreach (var tag in tags)
            {
                tag.ReleaseDeviceDependentResources();
            }
        }

        internal override Matrix4x4 Transformer
        {
            get
            {
                return base.Transformer;
            }

            set
            {
                base.Transformer = value;
                foreach (var tag in tags)
                {
                    tag.SetTransformer(value);
                }
            }
        }
    }
}
