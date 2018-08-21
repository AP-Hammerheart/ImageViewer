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
            Matrix4x4 transformer,
            Vector3 positionLeft,
            Vector3 positionRight
            )
        {
            this.X = X;
            this.Y = Y;

            left = new PyramidRenderer(deviceResources, loader)
            {
                Position = positionLeft,
                Transformer = transformer,
                TextureFile = "Content\\Textures\\red.png"
            };

            right = new PyramidRenderer(deviceResources, loader)
            {
                Position = positionRight,
                Transformer = transformer,
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

        internal void SetTransformer(Matrix4x4 transformer)
        {
            left.Transformer = transformer;
            right.Transformer = transformer;
        }

        internal void Update(BaseView view, Vector3 topLeft)
        {
            var step = view.Step;
            if (X < view.ImageX || Y < view.ImageY || X > view.ImageX + step || Y > view.ImageY + step)
            {
                visible = false;
            }
            else
            {
                var xx = (float)((double)BaseView.ViewSize * ((double)(X - view.ImageX) / (double)step));
                var yy = (float)((double)BaseView.ViewSize * ((double)(Y - view.ImageY) / (double)step));

                left.Position = new Vector3(topLeft.X + xx, topLeft.Y - yy, topLeft.Z);
                right.Position = new Vector3(topLeft.X + xx + BaseView.ViewSize, topLeft.Y - yy, topLeft.Z);

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
    }
}
