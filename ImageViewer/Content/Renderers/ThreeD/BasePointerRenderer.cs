// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
using System;
using System.Numerics;
using Windows.UI.Input.Spatial;

namespace ImageViewer.Content.Renderers.ThreeD
{
    internal class BasePointerRenderer : PyramidRenderer
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
            internal Corners(
                Vector3 topLeft,
                Vector3 bottomLeft,
                Vector3 topRight,
                Vector3 bottomRight)
            {
                orig_topLeft = topLeft;
                orig_bottomLeft = bottomLeft;
                orig_topRight = topRight;
                orig_bottomRight = bottomRight;

                orig_origo = bottomLeft + 0.5f * (topRight - bottomLeft);

                rotator = Matrix4x4.Identity;
                position = Vector3.Zero;

                origo = Vector3.Zero;
                this.topLeft = Vector3.Zero;
                this.bottomLeft = Vector3.Zero;
                this.topRight = Vector3.Zero;
                this.bottomRight = Vector3.Zero;

                normal = Vector3.Zero;

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

                var vtr = Vector4.Transform(this.orig_topRight, transform);
                topRight = new Vector3(vtr.X, vtr.Y, vtr.Z);

                var vbr = Vector4.Transform(this.orig_bottomRight, transform);
                bottomRight = new Vector3(vbr.X, vbr.Y, vbr.Z);

                var plane = Plane.CreateFromVertices(origo, topLeft, bottomLeft);
                normal = plane.Normal;
            }

            internal bool Inside(Vector3 point)
            {
                var v1 = point - bottomLeft;
                var v2 = topLeft - bottomLeft;
                var v3 = bottomRight - bottomLeft;

                var angle = Math.Acos(Vector3.Dot(v1, v3) / (v1.Length() * v3.Length()));
                var d1 = v1.Length() * Math.Cos(angle);
                var d2 = v1.Length() * Math.Sin(angle);

                return angle >= 0 && angle <= Math.PI / 2 && d1 <= v3.Length() && d2 <= v2.Length() && point.Y >= bottomLeft.Y;
            }

            internal D2 XY(Vector3 point)
            {
                var v1 = point - bottomLeft;
                var v2 = topLeft - bottomLeft;
                var v3 = bottomRight - bottomLeft;

                var angle = Math.Acos(Vector3.Dot(v1, v3) / (v1.Length() * v3.Length()));

                var d1 = v1.Length() * Math.Cos(angle);
                var d2 = v1.Length() * Math.Sin(angle);

                if (angle >= 0 && angle <= Math.PI / 2 && d1 <= v3.Length() && d2 <= v2.Length() && point.Y >= bottomLeft.Y)
                {
                    return new D2(d1 / v3.Length(), 1 - (d2 / v2.Length()));
                }
                else
                {
                    return null;
                }
            }

            internal Vector3 origo;
            internal Vector3 topLeft;
            internal Vector3 bottomLeft;
            internal Vector3 topRight;
            internal Vector3 bottomRight;
            internal Vector3 normal;

            internal Vector3 orig_origo;
            internal Vector3 orig_topLeft;
            internal Vector3 orig_bottomLeft;
            internal Vector3 orig_topRight;
            internal Vector3 orig_bottomRight;

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

        protected NavigationRenderer frame;

        protected Corners corners;

        internal bool Locked { get; set; } = false;
        internal bool Visible { get; set; } = true;
        internal bool Inside { get; set; } = true;

        internal BasePointerRenderer(
            NavigationRenderer frame,
            DeviceResources deviceResources,
            TextureLoader loader,
            Corners corners)
            : base(deviceResources, loader)
        {
            this.frame = frame;
            this.corners = corners;
        }

        internal override void Render()
        {
            if (Visible && Inside)
            {
                base.Render();
            }
        }

        internal virtual void Update()
        {
        }

        internal virtual void Update(SpatialPointerPose pose)
        {
            if (!Locked && pose != null)
            {
                var p0 = pose.Head.Position;
                var p1 = pose.Head.ForwardDirection;

                var s = Vector3.Dot(corners.normal, corners.origo - p0) /
                    Vector3.Dot(corners.normal, p1);

                var ps = p0 + s * p1;

                Inside = corners.Inside(ps);

                if (!Inside && Locked)
                {
                    Locked = false;
                }

                Position = ps;
            }
        }

        internal Vector3 Origo() => corners.origo;

        internal virtual void SetPosition(Vector3 dp)
        {
            corners.Position += dp;
        }

        internal virtual void SetRotator(Matrix4x4 rotator)
        {
            corners.Rotator = rotator;
        }

        internal Coordinate XY() => frame.XY(corners.XY(Position));
    }
}

