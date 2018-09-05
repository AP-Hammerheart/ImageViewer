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
        internal struct Coordinate
        {
            internal Coordinate(int X, int Y, Vector3 pL, Vector3 pR)
            {
                this.X = X;
                this.Y = Y;

                this.pL = pL;
                this.pR = pR;
            }

            internal int X;
            internal int Y;

            internal Vector3 pL;
            internal Vector3 pR;
        }

        internal struct Corners
        {
            internal Corners(Vector3 origo, Vector3 topLeft, Vector3 bottomLeft)
            {
                this.orig_origo = origo;
                this.orig_topLeft = topLeft;
                this.orig_bottomLeft = bottomLeft;

                rotator = Matrix4x4.Identity;
                position = Vector3.Zero;

                this.origo = Vector3.Zero;
                this.topLeft = Vector3.Zero;
                this.bottomLeft = Vector3.Zero;
                this.normal = Vector3.Zero;

                Update();
            }

            private void Update()
            {
                var translation = Matrix4x4.CreateTranslation(position);
                var transform = translation * rotator;

                var vo = Vector4.Transform(this.orig_origo, transform);
                origo = new Vector3(vo.X, vo.Y, vo.Z);

                var vtl = Vector4.Transform(this.orig_topLeft, transform);
                topLeft = new Vector3(vtl.X, vtl.Y, vtl.Z);

                var vbl = Vector4.Transform(this.orig_bottomLeft, transform);
                bottomLeft = new Vector3(vbl.X, vbl.Y, vbl.Z);

                var plane = Plane.CreateFromVertices(origo, topLeft, bottomLeft);
                normal = plane.Normal;
            }

            internal Vector3 origo;
            internal Vector3 topLeft;
            internal Vector3 bottomLeft;
            internal Vector3 normal;

            internal Vector3 orig_origo;
            internal Vector3 orig_topLeft;
            internal Vector3 orig_bottomLeft;

            private Matrix4x4 rotator;
            private Vector3 position;

            internal Matrix4x4 Rotator
            {
                get => rotator;
                set
                {
                    rotator = value;
                    Update();
                }
            }
            internal Vector3 Position
            {
                get => position;
                set
                {
                    position = value;
                    Update();
                }
            }
        }

        private readonly BaseView view;
        private Corners corners;

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
        }

        internal void AddTag()
        {
            var c = Coordinates();

            var task = new Task(async () =>
            {
                var tag = new Tag(deviceResources, loader, c.X, c.Y, corners.Rotator, c.pL, c.pR);
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
                tag.Update(view, corners);
            }
        }

        internal void Update(SpatialPointerPose pose)
        {
            if (!Locked && pose != null)
            {
                var p0 = pose.Head.Position;
                var p1 = pose.Head.ForwardDirection;

                var s = Vector3.Dot(corners.normal, corners.origo - p0) / Vector3.Dot(corners.normal, p1);
                var ps = p0 + s * p1;

                Position = ps;

                view.DebugString =
                    view.Origo.ToString("0.00") + " "
                    + view.RotationAngle.ToString() + "° "
                    + Position.ToString("0.00");
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

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);
            foreach (var tag in tags)
            {
                tag.Dispose();
            }
        }

        internal Coordinate Coordinates()
        {
            var translation = Matrix4x4.CreateTranslation(corners.Position);
            var transform = translation * corners.Rotator;

            Matrix4x4.Invert(transform, out Matrix4x4 inverted);

            var p = Vector4.Transform(Position, inverted);
            var pos1 = new Vector3(p.X, p.Y, p.Z);
            var pos2 = pos1;

            if (pos1.X <= corners.orig_origo.X)
            {
                pos2.X = corners.orig_origo.X + (pos1.X - corners.orig_topLeft.X);
            }
            else
            {
                pos2.X = corners.orig_topLeft.X + (pos1.X - corners.orig_origo.X);
            }

            var pL = pos1.X <= corners.orig_origo.X ? pos1 : pos2;
            var pR = pos1.X <= corners.orig_origo.X ? pos2 : pos1;

            var v = (1.0f / BaseView.ViewSize) * view.Step;
            var X = view.ImageX + (int)(v * (pL.X - corners.orig_topLeft.X));
            var Y = view.ImageY + (int)(v * (corners.orig_topLeft.Y - pL.Y));

            p = Vector4.Transform(pL, translation);
            pL = new Vector3(p.X, p.Y, p.Z);

            p = Vector4.Transform(pR, translation);
            pR = new Vector3(p.X, p.Y, p.Z);

            var c = new Coordinate(X, Y, pL, pR);
            return c;
        }

        internal void SetPosition(Vector3 dp)
        {
            corners.Position += dp;

            foreach (var tag in tags)
            {
                tag.SetPosition(dp);
            }
        }

        internal void SetRotator(Matrix4x4 rotator)
        {
            corners.Rotator = rotator;

            foreach (var tag in tags)
            {
                tag.SetRotator(rotator);
            }
        }
    }
}
