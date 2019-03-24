// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Input.Spatial;

namespace ImageViewer.Content.Renderers.ThreeD
{
    internal class PointerRenderer : BasePointerRenderer
    {
        private readonly NavigationView view;
        private List<Tag> tags = new List<Tag>();

        internal PointerRenderer(
            NavigationView view,
            NavigationRenderer frame,
            DeviceResources deviceResources, 
            TextureLoader loader, 
            Corners corners)
            : base(frame, deviceResources, loader, corners)
        {
            this.view = view;
        }

        internal void AddTag()
        {
            var c = Coordinates();

            var task = new Task(async () =>
            {
                var tag = new Tag(
                    deviceResources, 
                    loader, 
                    c.X, 
                    c.Y, 
                    corners.Rotator, 
                    c.pL, 
                    c.pR);

                await tag.CreateDeviceDependentResourcesAsync();
                tags.Add(tag);
            });

            task.Start();

            frame.AddTag(c.X, c.Y);
        }

        internal void RemoveTag()
        {
            if (tags.Count > 0)
            {
                var tag = tags[tags.Count - 1];
                tags.RemoveAt(tags.Count - 1);
                tag.ReleaseDeviceDependentResources();
            }

            frame.RemoveTag();
        }

        internal override void Render()
        {
            base.Render();

            for (var i=0; i < tags.Count; i++)
            {
                tags[i].Render();
            }
        }

        internal override void Update()
        {
            D2[] square =
            {
                new D2(view.TopRightX, view.TopRightY),
                new D2(view.TopLeftX, view.TopLeftY),
                new D2(view.BottomLeftX, view.BottomLeftY),
                new D2(view.BottomRightX, view.BottomRightY)
            };
             
            foreach (var tag in tags)
            {
                tag.Update(view, corners, square);
            }
        }

        internal override void Update(SpatialPointerPose pose)
        {
            base.Update(pose);

            if (!Locked && pose != null)
            {
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

            var left = pos1.X <= corners.orig_origo.X;

            var pL = left ? pos1 : pos2;
            var pR = left ? pos2 : pos1;

            var a = (pL.X - corners.orig_topLeft.X) / Constants.ViewSize;
            var b = (pL.Y - corners.orig_bottomLeft.Y) / Constants.ViewSize;

            var X = (int)(view.BottomLeftX 
                + a * (view.BottomRightX - view.BottomLeftX) 
                + b * (view.TopLeftX - view.BottomLeftX));

            var Y = (int)(view.BottomLeftY 
                + a * (view.BottomRightY - view.BottomLeftY) 
                + b * (view.TopLeftY - view.BottomLeftY));

            p = Vector4.Transform(pL, translation);
            pL = new Vector3(p.X, p.Y, p.Z);

            p = Vector4.Transform(pR, translation);
            pR = new Vector3(p.X, p.Y, p.Z);

            var c = new Coordinate(X, Y, pL, pR);
            return c;
        }

        internal void SetDeltaXY(float x, float y)
        {
            var left = corners.bottomLeft 
                + 0.5f * (corners.topLeft - corners.bottomLeft);

            var x_norm = Vector3.Normalize(corners.origo - left);
            var y_norm = Vector3.Normalize(corners.topLeft - corners.bottomLeft);

            Position += (x * x_norm) + (y * y_norm); 
        }

        private void SetXY(float x, float y)
        {
            var left = corners.bottomLeft
                + 0.5f * (corners.topLeft - corners.bottomLeft);

            var x_norm = Vector3.Normalize(corners.origo - left);
            var y_norm = Vector3.Normalize(corners.topLeft - corners.bottomLeft);

            Position = left + x * x_norm + y * y_norm;
        }

        private Tuple<float, float> GetXY()
        {
            var left = corners.bottomLeft
                + 0.5f * (corners.topLeft - corners.bottomLeft);

            var x_vec = corners.origo - left;
            var vec = (Position - left);
            var len = vec.Length();

            var x = 0.0f;
            var y = 0.0f;

            if (len > 0)
            {
                var ang = Math.Acos(Vector3.Dot(x_vec, vec) 
                    / (len * x_vec.Length()));

                var len1 = (Position - corners.topLeft).Length();
                var len2 = (Position - corners.bottomLeft).Length();

                x = len * (float)Math.Cos(ang);
                y = (len1 < len2 ? 1 : -1) * len * (float)Math.Sin(ang);                
            }

            return new Tuple<float, float>(x, y);
        }

        internal override void SetPosition(Vector3 dp)
        {
            if (Locked)
            {
                var pos = GetXY();
                base.SetPosition(dp);
                SetXY(pos.Item1, pos.Item2);
            }
            else
            {
                base.SetPosition(dp);
            }

            foreach (var tag in tags)
            {
                tag.SetPosition(dp);
            }
        }

        internal override void SetRotator(Matrix4x4 rotator)
        {
            if (Locked)
            {
                var pos = GetXY();
                base.SetRotator(rotator);
                SetXY(pos.Item1, pos.Item2);
            }
            else
            {
                base.SetRotator(rotator);
            }

            foreach (var tag in tags)
            {
                tag.SetRotator(rotator);
            }
        }
    }
}
