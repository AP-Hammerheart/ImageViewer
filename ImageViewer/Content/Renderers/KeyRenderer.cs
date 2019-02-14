// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Views;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers
{
    internal class KeyRenderer : StatusBarRenderer
    {
        private readonly BaseView view;

        internal KeyRenderer(
            BaseView view,
            DeviceResources deviceResources,
            TextureLoader loader)
            : base(deviceResources, loader) 
            => this.view = view;

        internal KeyRenderer(
            BaseView view,
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

        internal override void Update(StepTimer timer)
        {
            var chr = view.LastKey.ToString();
            if (!chr.Equals(Text))
            {
                Updating = true;
                Text = chr;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
