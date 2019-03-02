// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.ThreeD;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Utils
{
    internal class NavigationTag : IDisposable
    {
        private PyramidRenderer tag;

        internal NavigationTag(
            DeviceResources deviceResources,
            TextureLoader loader,
            Matrix4x4 rotator,
            Vector3 position
            )
        {
            tag = new PyramidRenderer(deviceResources, loader)
            {
                Position = position,
                RotationY = 45,
                Width = 0.5f,
                Length = 0.5f,
                GlobalRotator = rotator,
                TextureFile = "Content\\Textures\\solid.png"
            };
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            await tag.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources()
        {
            tag.ReleaseDeviceDependentResources();
        }

        internal void Render()
        {
            tag.Render();
        }

        internal void SetPosition(Vector3 dp)
        {
            tag.Position += dp;
        }

        internal void SetRotator(Matrix4x4 rotator)
        {
            tag.GlobalRotator = rotator;
        }

        internal void Dispose()
        {
            tag?.Dispose();
            tag = null;
        }

        void IDisposable.Dispose()
        {
            tag?.Dispose();
        }
    }
}
