﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Dev
{
    internal class DebugRenderer : StatusBarRenderer
    {
        private readonly DisposeView view;

        internal DebugRenderer(
            DisposeView view,
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
