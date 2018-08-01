﻿using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class TileCounterRenderer : StatusBarRenderer
    {
        internal TileCounterRenderer(DeviceResources deviceResources, TextureLoader loader)
            : base(deviceResources, loader)
        {
        }

        internal TileCounterRenderer(
            DeviceResources deviceResources, 
            TextureLoader loader, 
            Vector3 bottomLeft, 
            Vector3 topLeft, 
            Vector3 bottomRight, 
            Vector3 topRight)
            : base(deviceResources, loader, bottomLeft, topLeft, bottomRight, topRight)
        {
        }

        public override void Update(StepTimer timer)
        {
            var tiles = loader.TilesInMemory().ToString();
            if (!tiles.Equals(Text))
            {
                updating = true;
                Text = tiles;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
