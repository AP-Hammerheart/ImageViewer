// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Renderers.Image;
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
        protected NavigationRenderer navigationFrame;
        protected SettingViewer settingViewer;
        protected MacroView macro;
        protected HistologyView histo;
        protected RadiologyView radiology;
        protected ObjRenderer model;
        protected CaseSelectionView caseView;

        internal RotateRenderer[] Tiles { get; set; }
        internal BasePointerRenderer[] Pointers { get; set; }

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

            foreach (var pointer in Pointers)
            {
                pointer?.Update(timer);
            }

            navigationFrame?.Update(timer);
            macro?.Update(timer);
            radiology?.Update( timer );
            histo?.Update( timer );
            model?.Update(timer);
            caseView?.Update( timer );
        }

        internal void Update(SpatialPointerPose pose)
        {
            foreach (var pointer in Pointers)
            {
                pointer?.Update(pose);
            }       
        }

        internal void Render()
        {
            navigationFrame?.Render();
            macro?.Render();
            histo?.Render();
            radiology?.Render();
            caseView?.Render();

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

                foreach (var pointer in Pointers)
                {
                    pointer?.Render();
                }
            }

            model?.Render();
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {          
            foreach (var renderer in statusItems)
            {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            await macro?.CreateDeviceDependentResourcesAsync();
            await radiology?.CreateDeviceDependentResourcesAsync();
            await histo?.CreateDeviceDependentResourcesAsync();
            await navigationFrame?.CreateDeviceDependentResourcesAsync();
            await caseView?.CreateDeviceDependentResourcesAsync();

            foreach (var renderer in Tiles)
            {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            foreach (var pointer in Pointers)
            {
                await pointer?.CreateDeviceDependentResourcesAsync();
            }
          
            await settingViewer?.CreateDeviceDependentResourcesAsync();

            await model?.CreateDeviceDependentResourcesAsync();
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

            foreach (var pointer in Pointers)
            {
                pointer?.ReleaseDeviceDependentResources();
            }

            navigationFrame?.ReleaseDeviceDependentResources();
            macro?.ReleaseDeviceDependentResources();
            radiology?.ReleaseDeviceDependentResources();
            histo?.ReleaseDeviceDependentResources();
            model?.ReleaseDeviceDependentResources();
            caseView?.ReleaseDeviceDependentResources();
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

            foreach (var pointer in Pointers)
            {
                pointer?.Dispose();
            }
          
            navigationFrame?.Dispose();
            macro?.Dispose();
            model?.Dispose();
            histo?.Dispose();
            radiology?.Dispose();
            caseView?.Dispose();
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

            foreach (var pointer in Pointers)
            {
                pointer?.Dispose();
            }
         
            navigationFrame?.Dispose();
            macro?.Dispose();
            histo?.Dispose();
            radiology?.Dispose();
            caseView?.Dispose();
        }
    }
}
