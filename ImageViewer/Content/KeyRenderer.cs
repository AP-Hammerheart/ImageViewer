using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Common;

namespace ImageViewer.Content
{
    internal class KeyRenderer : StatusBarRenderer
    {
        private readonly ImageViewerMain main;

        internal KeyRenderer(
            ImageViewerMain main,
            DeviceResources deviceResources, 
            TextureLoader loader)
            : base(deviceResources, loader)
        {
            this.main = main;
        }

        internal KeyRenderer(
            ImageViewerMain main,
            DeviceResources deviceResources, 
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight)
            : base(deviceResources, loader, bottomLeft, topLeft, bottomRight, topRight)
        {
            this.main = main;
        }

        public override void Update(StepTimer timer)
        {
            var chr = main.VirtualKey.ToString();
            if (!chr.Equals(Text))
            {
                updating = true;
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
