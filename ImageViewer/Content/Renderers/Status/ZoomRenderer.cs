// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Status
{
    internal class ZoomRenderer : StatusBarRenderer
    {
        private readonly NavigationView view;

        internal ZoomRenderer(
            NavigationView view,
            DeviceResources deviceResources,
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight)
            : base(deviceResources, 
                  loader, 
                  bottomLeft, 
                  topLeft, 
                  bottomRight, 
                  topRight) 
            => this.view = view;

        private string Zoom(int level)
        {
            switch (level)
            {
                case 1: return "40x";
                case 2: return "20x";
                case 3: return "10x";
                case 4: return "5x";
                case 5: return "2.5x";
                case 6: return "1.25x";
                case 7: return "0.625x";
                case 8: return "0.3125x";
                default: return "80x";
            }
        }

        internal override void Update(StepTimer timer)
        {
            var zoom = 
                "X: " + view.TopLeftX.ToString() 
                + "  Y: " + view.TopLeftY.ToString()
                + "  A: " + (Math.Round(view.Angle * 180.0 / Math.PI)).ToString()
                + "°  Level: " + view.Level;

            if (!zoom.Equals(Text))
            {
                Updating = true;
                Text = zoom;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
