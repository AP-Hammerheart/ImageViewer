// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Views
{
    internal class TileView : BaseView
    {
        private static readonly float tileSize = 0.2f;
        private static readonly float negHalfTile = -0.5f * tileSize;
        private static readonly float posHalfTile = 0.5f * tileSize;

        protected override int LargeStep => 10;

        internal override int TileCountX { get; } = Settings.GridX - 1;
        internal override int TileCountY { get; } = Settings.GridY - 1;

        internal TileView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(main, deviceResources, loader)
        {
            TileResolution = 256;

            Tiles = new TileRenderer[2 * Settings.GridX * Settings.GridY];

            var step = Step;

            var lx0 = -1.0f * TileCountX * tileSize + posHalfTile;
            var rx0 = posHalfTile;
            var y0 = 0.5f * TileCountY * tileSize - posHalfTile;
            var z = Settings.DistanceFromUser;

            var ri0 = Settings.GridX * Settings.GridY;

            for (var x = 0; x < Settings.GridX; x++)
            {
                for (var y = 0; y < Settings.GridY; y++)
                {
                    Tiles[Settings.GridY * x + y] = new TileRenderer(deviceResources, loader, null, tileSize)
                    {
                        Position = new Vector3(lx0 + x * tileSize, y0 - y * tileSize, z)
                    };

                    Tiles[ri0 + (Settings.GridY * x + y)] = new TileRenderer(deviceResources, loader, null, tileSize)
                    {
                        Position = new Vector3(rx0 + x * tileSize, y0 - y * tileSize, z)
                    };
                }
            }

            SetCorners();
            UpdateImages();
        }

        protected override void UpdateImages()
        {
            Pointer.Update();

            var textures = new List<string>();

            var step = PixelSize(Level) * TileResolution;

            var gridImageX = TopLeftX - (TopLeftX % step);
            var gridImageY = TopLeftY - (TopLeftY % step);

            var rx = (TopLeftX % step) / PixelSize(Level);
            var ry = (TopLeftY % step) / PixelSize(Level);

            var dx = ((float)rx / (float)TileResolution) * tileSize;
            var dy = ((float)ry / (float)TileResolution) * tileSize;

            var x0 = (-1.0f * TileCountX * tileSize) + posHalfTile - dx;
            var x1 = posHalfTile - dx;
            var y0 = (0.5f * TileCountY * tileSize) - posHalfTile + dy;
            var z = Settings.DistanceFromUser;

            var tx = (float)rx / (float)TileResolution;
            var ty = (float)ry / (float)TileResolution;

            for (var x = 0; x < Settings.GridX; x++)
            {
                for (var y = 0; y < Settings.GridY; y++)
                {
                    var lurl = Settings.Image1
                        + "&x=" + (x * step + gridImageX).ToString()
                        + "&y=" + (y * step + gridImageY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString();

                    textures.Add(lurl);

                    var ltile = (TileRenderer)(Tiles[Settings.GridY * x + y]);
                    ltile.TextureID = lurl;
                    ltile.Position = Origo + new Vector3(x0 + (x * tileSize), y0 - (y * tileSize), z);

                    if (x == 0 || y == 0 || x == TileCountX || y == TileCountY)
                    {
                        ClipTile(ltile, x, y, tx, ty, dx, dy);
                    }

                    var rurl = Settings.Image2
                        + "&x=" + (x * step + gridImageX + Settings.Image2offsetX).ToString()
                        + "&y=" + (y * step + gridImageY + Settings.Image2offsetY).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString();

                    textures.Add(rurl);

                    var rtile = ((TileRenderer)Tiles[(Settings.GridX * Settings.GridY) + (Settings.GridY * x + y)]);
                    rtile.TextureID = rurl;
                    rtile.Position = Origo + new Vector3(x1 + (x * tileSize), y0 - (y * tileSize), z);

                    if (x == 0 || y == 0 || x == TileCountX || y == TileCountY)
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
            task.Wait();
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
                if (ix == TileCountX)
                {
                    tile.X1 = negHalfTile + dx;
                    tile.U1 = tx;
                }
            }
            else
            {
                if (ix == TileCountX)
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
                if (iy == TileCountY)
                {
                    tile.Y0 = posHalfTile - dy;
                    tile.V0 = ty;
                }
            }
            else
            {
                if (iy == TileCountY)
                {
                    tile.Y0 = tile.Y1;
                    tile.V0 = tile.V1;
                }
            }
            tile.UpdateGeometry();
        }
    }
}
