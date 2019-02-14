// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Views;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers
{
    internal class DebugRenderer : StatusBarRenderer
    {
        private readonly BaseView view;

        internal DebugRenderer(
            BaseView view,
            DeviceResources deviceResources,
            TextureLoader loader)
            : base(deviceResources, loader) => this.view = view;

        internal DebugRenderer(
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
            var txt = view.DebugString;

            if (view.ErrorString.Length > 0)
            {
                txt = view.ErrorString;
            }

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
