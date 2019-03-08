// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Status
{
    internal class ScalerRenderer : StatusBarRenderer
    {
        internal ScalerRenderer(
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
                  topRight) {}

        internal override void Update(StepTimer timer)
        {
            var txt = (Settings.Mode == 0 ? "2D  " : "3D  ") + Settings.Scaler.ToString();
            if (!txt.Equals(Text))
            {
                Updating = true;
                Text = txt;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
