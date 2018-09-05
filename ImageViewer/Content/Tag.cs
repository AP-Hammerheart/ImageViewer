// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class Tag : IDisposable
    {
        private PyramidRenderer left;
        private PyramidRenderer right;

        private readonly int X;
        private readonly int Y;
        private bool visible = true;

        internal Tag(
            DeviceResources deviceResources,
            TextureLoader loader,
            int X,
            int Y,
            Matrix4x4 rotator,
            Vector3 positionLeft,
            Vector3 positionRight
            )
        {
            this.X = X;
            this.Y = Y;

            left = new PyramidRenderer(deviceResources, loader)
            {
                Position = positionLeft,
                Rotator = rotator,
                TextureFile = "Content\\Textures\\red.png"
            };

            right = new PyramidRenderer(deviceResources, loader)
            {
                Position = positionRight,
                Rotator = rotator,
                TextureFile = "Content\\Textures\\green.png"
            };
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            await left.CreateDeviceDependentResourcesAsync();
            await right.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources()
        {
            visible = false;
            left.ReleaseDeviceDependentResources();
            right.ReleaseDeviceDependentResources();
        }

        internal void Update(BaseView view, PointerRenderer.Corners corners)
        {
            var width = view.Step * view.TileCount;
            if (X < view.ImageX || Y < view.ImageY || X > view.ImageX + width || Y > view.ImageY + width)
            {
                visible = false;
            }
            else
            {
                var xx = (float)((double)BaseView.ViewSize * ((double)(X - view.ImageX) / (double)width));
                var yy = (float)((double)BaseView.ViewSize * ((double)(Y - view.ImageY) / (double)width));

                var pL = new Vector3(corners.orig_topLeft.X + xx, corners.orig_topLeft.Y - yy, corners.orig_topLeft.Z);
                var pR = new Vector3(corners.orig_topLeft.X + xx + BaseView.ViewSize, corners.orig_topLeft.Y - yy, corners.orig_topLeft.Z);

                var translation = Matrix4x4.CreateTranslation(corners.Position);

                var p = Vector4.Transform(pL, translation);
                left.Position = new Vector3(p.X, p.Y, p.Z);

                p = Vector4.Transform(pR, translation);
                right.Position = new Vector3(p.X, p.Y, p.Z);

                visible = true;
            }
        }

        internal void Render()
        {
            if (visible)
            {
                left.Render();
                right.Render();
            }
        }

        public void Dispose()
        {
            left?.Dispose();
            left = null;

            right?.Dispose();
            right = null;
        }

        internal void SetPosition(Vector3 dp)
        {
            left.Position += dp;
            right.Position += dp;
        }

        internal void SetRotator(Matrix4x4 rotator)
        {
            left.Rotator = rotator;
            right.Rotator = rotator;
        }
    }
}
