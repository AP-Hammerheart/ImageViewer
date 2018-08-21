// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content
{
    internal class TileView : BaseView
    {
        private static readonly int maxX = 5;
        private static readonly int maxY = 5;

        internal TileView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(deviceResources, loader)
        {
            TileResolution = 256;

            Tiles = new TileRenderer[2 * maxX * maxY];

            var step = Step;
            var tileSize = 0.1f;

            for (var x = 0; x < maxX; x++)
            {
                for (var y = 0; y < maxY; y++)
                {
                    Tiles[maxY * x + y] = new TileRenderer(deviceResources, loader, ImageViewerMain.Image1
                        + "&x=" + (x * step + ImageX).ToString()
                        + "&y=" + (y * step + ImageY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString(),
                        tileSize)
                    {
                        Position = new Vector3(
                            -1.0f * maxX * tileSize + (0.5f * tileSize) + x * tileSize,
                            0.5f * maxY * tileSize - (0.5f * tileSize) - y * tileSize,
                            -1 * DistanceFromUser)
                    };

                    Tiles[(maxX * maxY) + (maxY * x + y)] = new TileRenderer(deviceResources, loader, ImageViewerMain.Image2
                        + "&x=" + (x * step + ImageX + image2offsetX).ToString()
                        + "&y=" + (y * step + ImageY + image2offsetY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString(),
                        tileSize)
                    {
                        Position = new Vector3(
                            (0.5f * tileSize) + x * tileSize,
                            0.5f * maxY * tileSize - (0.5f * tileSize) - y * tileSize,
                            -1 * DistanceFromUser)
                    };
                }
            }
        }

        protected override void Scale(Direction direction, int number)
        {
            switch (direction)
            {
                case Direction.UP:
                    Level -= number;
                    if (Level < 0) Level = 0;
                    break;
                case Direction.DOWN:
                    Level += number;
                    if (Level > MinScale) Level = MinScale;
                    break;
            }

            var step = Step;

            ImageX = ImageX - (ImageX % step);
            ImageY = ImageY - (ImageY % step);

            UpdateImages(step);
        }

        protected override void Move(Direction direction, int number)
        {
            var step = Step;

            switch (direction)
            {
                case Direction.LEFT:
                    if (ImageX < maxResolution - ((maxX - 1) * step))
                    {
                        ImageX += number * step;
                    }
                    break;
                case Direction.RIGHT:
                    ImageX -= number * step;
                    if (ImageX < 0)
                    {
                        ImageX = 0;
                    }
                    break;
                case Direction.DOWN:
                    ImageY -= number * step;
                    if (ImageY < 0)
                    {
                        ImageY = 0;
                    }
                    break;
                case Direction.UP:
                    if (ImageY < maxResolution - ((maxY - 1) * step))
                    {
                        ImageY += number * step;
                    }
                    break;
            }

            UpdateImages(step);
        }

        private void UpdateImages(int step)
        {
            for (var x = 0; x < maxX; x++)
            {
                for (var y = 0; y < maxY; y++)
                {
                    var url1 = ImageViewerMain.Image1
                        + "&x=" + (x * step + ImageX).ToString()
                        + "&y=" + (y * step + ImageY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString();

                    Tiles[maxY * x + y].TextureID = url1;

                    Task task1 = new Task(async () =>
                    {
                        await loader.LoadTextureAsync(url1);
                    });
                    task1.Start();
                    task1.Wait();

                    var url2 = ImageViewerMain.Image2
                        + "&x=" + (x * step + ImageX + image2offsetX).ToString()
                        + "&y=" + (y * step + ImageY + image2offsetY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString();

                    Tiles[(maxX * maxY) + (maxY * x + y)].TextureID = url2;

                    Task task2 = new Task(async () =>
                    {
                        await loader.LoadTextureAsync(url2);
                    });
                    task2.Start();
                    task2.Wait();
                }
            }
        }
    }
}
