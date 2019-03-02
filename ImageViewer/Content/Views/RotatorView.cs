// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Utils;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Views
{
    class RotatorView : BaseView
    {
        private readonly int maxTiles = 24;

        private int tileX = 1;
        private int tileY = 1;

        private double dx = 0.0;
        private double dy = 0.0;

        protected readonly DeviceResources deviceResources;

        internal RotatorView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(main, deviceResources, loader)
        {
            this.deviceResources = deviceResources;

            Tiles = new RotateRenderer[2 * maxTiles];

            for (var i = 0; i < 2 * maxTiles; i++)
            {
                Tiles[i] = new RotateRenderer(deviceResources, loader, "", null, null);
            }

            Refresh();
        }

        private void Update()
        {
            var step = PixelSize(Level) * Constants.TileResolution;

            tileX = CenterX / step;
            tileY = CenterY / step;

            dx = (double)((CenterX % step) - (step / 2)) / (double)(step) * Constants.TileSize;
            dy = (double)((step / 2) - (CenterY % step)) / (double)(step) * Constants.TileSize;
        }

        private string Image(string file, int idx)
        {
            var y = idx / 7;
            var x = idx % 7;

            var step = PixelSize(Level) * Constants.TileResolution;

            return file
                        + "&x=" + ((tileX + (x - 3)) * step).ToString()
                        + "&y=" + ((tileY + (y - 3)) * step).ToString()
                        + "&w=" + Constants.TileResolution.ToString()
                        + "&h=" + Constants.TileResolution.ToString()
                        + "&level=" + Level.ToString();
        }

        protected override void UpdateImages()
        {
            Update();
            Pointer.Update();

            var textures = new List<string>();

            var tiles = Rotator.Tiles(Angle, dx, dy, Constants.TileSize);
            var z = Constants.DistanceFromUser;

            if (tiles.Count > maxTiles)
            {
                ErrorString = "Too many tiles: " + tiles.Count.ToString();
                return;
            }

            var rotator1 = Matrix4x4.CreateRotationZ(
                -1.0f * (float)(Angle),
                new Vector3(Origo.X - Constants.HalfViewSize, Origo.Y, Origo.Z));

            var rotator2 = Matrix4x4.CreateRotationZ(
                -1.0f * (float)(Angle),
                new Vector3(Origo.X + Constants.HalfViewSize, Origo.Y, Origo.Z));

            for (var i = 0; i < tiles.Count; i++)
            {
                var image1 = Image(Settings.Image1, tiles[i].Item1);
                textures.Add(image1);

                Tiles[i].TextureID = image1;
                (Tiles[i] as RotateRenderer).PlaneVertices = tiles[i].Item2;
                (Tiles[i] as RotateRenderer).PlaneIndices = tiles[i].Item3;

                Tiles[i].UpdateGeometry();
                Tiles[i].ViewRotator = rotator1;
                Tiles[i].Position = new Vector3(
                    Origo.X - Constants.HalfViewSize - (float)dx,
                    Origo.Y - 1.0f * (float)dy,
                    Origo.Z + z);

                var image2 = Image(Settings.Image2, tiles[i].Item1);
                textures.Add(image2);

                Tiles[maxTiles + i].TextureID = image2;
                (Tiles[maxTiles + i] as RotateRenderer).PlaneVertices = tiles[i].Item2;
                (Tiles[maxTiles + i] as RotateRenderer).PlaneIndices = tiles[i].Item3;

                Tiles[maxTiles + i].UpdateGeometry();
                Tiles[maxTiles + i].ViewRotator = rotator2;
                Tiles[maxTiles + i].Position = new Vector3(
                    Origo.X + Constants.HalfViewSize - (float)dx,
                    Origo.Y - 1.0f * (float)dy,
                    Origo.Z + z);
            }

            for (var i = tiles.Count; i < maxTiles; i++)
            {
                Tiles[i].TextureID = "";
                Tiles[maxTiles + i].TextureID = "";
            }

            var task = new Task(async () =>
            {
                await loader.LoadTexturesAsync(textures);
            });
            task.Start();
            task.Wait();
        }      
    }
}
