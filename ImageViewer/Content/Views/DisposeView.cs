// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Renderers.ThreeD;
using System;
using System.Threading.Tasks;
using Windows.UI.Input.Spatial;

namespace ImageViewer.Content.Views
{
    abstract class DisposeView : IDisposable
    {
        internal string ErrorString { get; set; } = "";
        internal string DebugString { get; set; } = "";

        internal int FPS { get; set; } = 0;

        protected BasePlaneRenderer[] statusItems;
        protected FrameRenderer navigationFrame;        
        protected SettingViewer settingViewer;
        
        internal PlaneRenderer[] Tiles { get; set; }
        internal PointerRenderer Pointer { get; set; }

        protected bool ShowSettings { get; set; } = false;

        protected DisposeView() {}

        internal void Update(StepTimer timer)
        {
            foreach (var renderer in Tiles)
            {
                renderer?.Update(timer);
            }
            foreach (var renderer in statusItems)
            {
                renderer?.Update(timer);
            }

            settingViewer?.Update(timer);
            Pointer?.Update(timer);
            navigationFrame?.Update(timer);
        }

        internal void Update(SpatialPointerPose pose)
        {
            Pointer?.Update(pose);
        }

        internal void Render()
        {
            navigationFrame?.Render();

            foreach (var renderer in statusItems)
            {
                renderer?.Render();
            }

            if (ShowSettings)
            {
                settingViewer?.Render();
            }
            else
            {
                foreach (var renderer in Tiles)
                {
                    renderer?.Render();
                }

                Pointer?.Render();
            }           
        }

        internal void Dispose()
        {
            if (statusItems != null)
            {
                foreach (var renderer in statusItems)
                {
                    renderer?.Dispose();
                }
                statusItems = null;
            }

            if (Tiles != null)
            {
                foreach (var renderer in Tiles)
                {
                    renderer?.Dispose();
                }
                Tiles = null;
            }

            settingViewer?.Dispose();
            Pointer?.Dispose();
            navigationFrame?.Dispose();
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            foreach (var renderer in Tiles)
            {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            foreach (var renderer in statusItems)
            {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            await settingViewer?.CreateDeviceDependentResourcesAsync();
            await Pointer?.CreateDeviceDependentResourcesAsync();
            await navigationFrame?.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources()
        {
            foreach (var renderer in Tiles)
            {
                renderer?.ReleaseDeviceDependentResources();
            }

            foreach (var renderer in statusItems)
            {
                renderer?.ReleaseDeviceDependentResources();
            }

            settingViewer?.ReleaseDeviceDependentResources();
            Pointer?.ReleaseDeviceDependentResources();
            navigationFrame?.ReleaseDeviceDependentResources();
        }

        void IDisposable.Dispose()
        {
            foreach (var renderer in Tiles)
            {
                renderer?.Dispose();
            }

            foreach (var renderer in statusItems)
            {
                renderer?.Dispose();
            }

            settingViewer?.Dispose();
            Pointer?.Dispose();
            navigationFrame?.Dispose();
        }
    }
}
