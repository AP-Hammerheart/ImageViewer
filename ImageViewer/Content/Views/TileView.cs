// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class TileView : BaseView
    {
        private static readonly int gridX = 6;
        private static readonly int gridY = 6;

        private static readonly float tileSize = 0.1f;
        private static readonly float negHalfTile = -0.5f * tileSize;
        private static readonly float posHalfTile = 0.5f * tileSize;

        protected override int LargeStep => 3;

        internal override int TileCountX { get; } = 5;
        internal override int TileCountY { get; } = 5;

        internal TileView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(main, deviceResources, loader)
        {
            TileResolution = 256;

            Tiles = new TileRenderer[2 * gridX * gridY];

            var step = Step;

            var lx0 = -1.0f * (gridX - 1) * tileSize + posHalfTile;
            var rx0 = posHalfTile;
            var y0 = 0.5f * (gridY - 1) * tileSize - posHalfTile;
            var z = -1.0f * DistanceFromUser;

            var ri0 = gridX * gridY;

            for (var x = 0; x < gridX; x++)
            {
                for (var y = 0; y < gridY; y++)
                {
                    Tiles[gridY * x + y] = new TileRenderer(deviceResources, loader, null, tileSize)
                    {
                        Position = new Vector3(lx0 + x * tileSize, y0 - y * tileSize, z)
                    };

                    Tiles[ri0 + (gridY * x + y)] = new TileRenderer(deviceResources, loader, null, tileSize)
                    {
                        Position = new Vector3(rx0 + x * tileSize, y0 - y * tileSize, z)
                    };
                }
            }
            UpdateImages();
        }

        protected override void UpdateImages()
        {
            Pointer.Update();

            var textures = new List<string>();

            var step = PixelSize(Level) * TileResolution;

            var gridImageX = ImageX - (ImageX % step);
            var gridImageY = ImageY - (ImageY % step);

            var rx = (ImageX % step) / PixelSize(Level);
            var ry = (ImageY % step) / PixelSize(Level);

            var dx = ((float)rx / (float)TileResolution) * tileSize;
            var dy = ((float)ry / (float)TileResolution) * tileSize;

            var x0 = (-1.0f * (gridX - 1) * tileSize) + posHalfTile - dx;
            var x1 = posHalfTile - dx;
            var y0 = (0.5f * (gridY - 1) * tileSize) - posHalfTile + dy;
            var z = -1.0f * DistanceFromUser;

            var tx = (float)rx / (float)TileResolution;
            var ty = (float)ry / (float)TileResolution;

            for (var x = 0; x < gridX; x++)
            {
                for (var y = 0; y < gridY; y++)
                {
                    var lurl = ImageViewerMain.Image1
                        + "&x=" + (x * step + gridImageX).ToString()
                        + "&y=" + (y * step + gridImageY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString();

                    textures.Add(lurl);

                    var ltile = (TileRenderer)(Tiles[gridY * x + y]);
                    ltile.TextureID = lurl;
                    ltile.Position = Origo + new Vector3(x0 + (x * tileSize), y0 - (y * tileSize), z);

                    if (x == 0 || y == 0 || x == (gridX - 1) || y == (gridY - 1))
                    {
                        ClipTile(ltile, x, y, tx, ty, dx, dy);
                    }

                    var rurl = ImageViewerMain.Image2
                        + "&x=" + (x * step + gridImageX + image2offsetX).ToString()
                        + "&y=" + (y * step + gridImageY + image2offsetY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString();

                    textures.Add(rurl);

                    var rtile = ((TileRenderer)Tiles[(gridX * gridY) + (gridY * x + y)]);
                    rtile.TextureID = rurl;
                    rtile.Position = Origo + new Vector3(x1 + (x * tileSize), y0 - (y * tileSize), z);

                    if (x == 0 || y == 0 || x == (gridX - 1) || y == (gridY - 1))
                    {
                        ClipTile(rtile, x, y, tx, ty, dx, dy);
                    }
                }
            }

            var task = new Task(async () =>
            {
                await loader.LoadTexturesAsync(textures);
            });
            task.Start();
        }

        private void ClipTile(TileRenderer tile, int ix, int iy, float tx, float ty, float dx, float dy)
        {
            if (ix == 0)
            {
                tile.X0 = negHalfTile + dx;
                tile.U0 = tx;
            }

            if (dx > 0.0f)
            {
                if (ix == gridX - 1)
                {
                    tile.X1 = negHalfTile + dx;
                    tile.U1 = tx;
                }
            }
            else
            {
                if (ix == gridX - 1)
                {
                    tile.X1 = tile.X0;
                    tile.U1 = tile.U0;
                }
            }

            if (iy == 0)
            {
                tile.Y1 = posHalfTile - dy;
                tile.V1 = ty;
            }

            if (dy > 0.0f)
            {
                if (iy == gridY - 1)
                {
                    tile.Y0 = posHalfTile - dy;
                    tile.V0 = ty;
                }
            }
            else
            {
                if (iy == gridY - 1)
                {
                    tile.Y0 = tile.Y1;
                    tile.V0 = tile.V1;
                }
            }
            tile.UpdateGeometry();
        }
    }
}
