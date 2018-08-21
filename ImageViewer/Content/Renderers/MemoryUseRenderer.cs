// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class MemoryUseRenderer : StatusBarRenderer
    {
        internal MemoryUseRenderer(DeviceResources deviceResources, TextureLoader loader)
            : base(deviceResources, loader)
        {
        }

        internal MemoryUseRenderer(
            DeviceResources deviceResources, 
            TextureLoader loader, 
            Vector3 bottomLeft, 
            Vector3 topLeft, 
            Vector3 bottomRight, 
            Vector3 topRight)
            : base(deviceResources, loader, bottomLeft, topLeft, bottomRight, topRight)
        {
        }

        internal override void Update(StepTimer timer)
        {
            var mem = MemoryUseInMB().ToString() + " MB";
            if (!mem.Equals(Text))
            {
                updating = true;
                Text = mem;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }

        private int MemoryUseInMB()
        {
            var report = Windows.System.MemoryManager.GetAppMemoryReport();
            var memoryUse = report.PrivateCommitUsage;

            return (int)(memoryUse / 1000000);
        }
    }
}
