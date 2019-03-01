// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Views
{
    internal class PanView : BaseView
    {
        internal static int ViewResolution { get; } = 768;

        protected override int TileOffset(int level) 
            => PixelSize(level) * (TileResolution - ViewResolution);

        protected override int LargeStep => 10;

        internal PanView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(main, deviceResources, loader)
        {
            TileResolution = 1536;

            Tiles = new PanRenderer[2];

            Tiles[0] = new PanRenderer(deviceResources, loader, "", Constants.ViewSize, backBufferResolution: TileResolution)
            {
                Position = new Vector3(-0.5f * Constants.ViewSize, 0, Constants.DistanceFromUser)
            };

            Tiles[1] = new PanRenderer(deviceResources, loader, "", Constants.ViewSize, backBufferResolution: TileResolution)
            {
                Position = new Vector3(0.5f * Constants.ViewSize, 0, Constants.DistanceFromUser)
            };

            UpdateImages();
        }

        protected override void UpdateImages()
        {
            Pointer.Update();

            var step = PixelSize(Level) * (TileResolution - ViewResolution);
            var x = (TopLeftX / step) * step;
            var y = (TopLeftY / step) * step;

            var xrem = (TopLeftX % step) / PixelSize(Level);
            var yrem = (TopLeftY % step) / PixelSize(Level);

            ((PanRenderer)Tiles[0]).UpdateGeometry(xrem, yrem);
            ((PanRenderer)Tiles[1]).UpdateGeometry(xrem, yrem);

            var url1 = Settings.Image1
                + "&x=" + x.ToString()
                + "&y=" + y.ToString()
                + "&w=" + TileResolution.ToString()
                + "&h=" + TileResolution.ToString()
                + "&level=" + Level.ToString();

            if (Tiles[0].TextureID != url1)
            {
                Tiles[0].TextureID = url1;

                Task task1 = new Task(async () =>
                {
                    await ((PanRenderer)Tiles[0]).UpdateTextureAsync();
                });
                task1.Start();
                task1.Wait();
            }
      
            var url2 = Settings.Image2
                + "&x=" + (x + Settings.Image2offsetX).ToString()
                + "&y=" + (y + Settings.Image2offsetY).ToString()
                + "&w=" + TileResolution.ToString()
                + "&h=" + TileResolution.ToString()
                + "&level=" + Level.ToString();

            if (Tiles[1].TextureID != url2)
            {
                Tiles[1].TextureID = url2;

                Task task2 = new Task(async () =>
                {
                    await ((PanRenderer)Tiles[1]).UpdateTextureAsync();
                });
                task2.Start();
                task2.Wait();
            }               
        }
    }
}
