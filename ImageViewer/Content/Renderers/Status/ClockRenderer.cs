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
    internal class ClockRenderer : StatusBarRenderer
    {
        internal ClockRenderer(
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
            var time = System.DateTime.Now.ToString("h:mm:ss");

            if (!time.Equals(Text))
            {
                Updating = true;
                Text = time;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
