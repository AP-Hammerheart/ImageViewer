// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Views
{
    internal class RotatorView : BaseView
    {
        private static readonly double tileSize = 0.2;
        private readonly int maxTiles = 24;

        protected override int LargeStep => 10;

        private int tileX = 1;
        private int tileY = 1;

        private double dx = 0.0;
        private double dy = 0.0;

        private readonly double diagonal;

        internal override int TileCountX { get; } = 3;
        internal override int TileCountY { get; } = 3;

        protected readonly DeviceResources deviceResources;

        internal RotatorView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(main, deviceResources, loader)
        {
            this.deviceResources = deviceResources;

            TileResolution = 256;
            diagonal = Math.Sqrt(2.0) * 1.5 * (double)(TileResolution);

            var z = Settings.DistanceFromUser;
            var textures = new List<string>();

            var tiles = Rotator.Tiles(Angle, dx, dy, tileSize);

            var rotator1 = Matrix4x4.CreateRotationZ(
                -1.0f * (float)(Angle),
                new Vector3(Origo.X - 0.3f, Origo.Y, Origo.Z));

            var rotator2 = Matrix4x4.CreateRotationZ(
                -1.0f * (float)(Angle),
                new Vector3(Origo.X + 0.3f, Origo.Y, Origo.Z));

            Tiles = new RotateRenderer[2 * maxTiles];

            for (var i = 0; i < tiles.Count; i++)
            {
                var image1 = Image(Settings.Image1, tiles[i].Item1);
                textures.Add(image1);

                Tiles[i] = new RotateRenderer(
                    deviceResources,
                    loader,
                    image1,
                    tiles[i].Item2,
                    tiles[i].Item3)
                {
                    Position = new Vector3(Origo.X - 0.3f, Origo.Y, Origo.Z + z),
                    ViewRotator = rotator1
                };

                var image2 = Image(Settings.Image2, tiles[i].Item1);
                textures.Add(image2);

                Tiles[maxTiles + i] = new RotateRenderer(
                    deviceResources,
                    loader,
                    image1,
                    tiles[i].Item2,
                    tiles[i].Item3)
                {
                    Position = new Vector3(Origo.X + 0.3f, Origo.Y, Origo.Z + z),
                    ViewRotator = rotator2
                };
            }

            for (var i = tiles.Count; i < maxTiles; i++)
            {
                Tiles[i] = new RotateRenderer(
                    deviceResources,
                    loader,
                    "",
                    tiles[0].Item2,
                    tiles[0].Item3)
                {
                    Position = new Vector3(Origo.X - 0.3f, Origo.Y, Origo.Z + z),
                };

                Tiles[maxTiles + i] = new RotateRenderer(
                    deviceResources,
                    loader,
                    "",
                    tiles[0].Item2,
                    tiles[0].Item3)
                {
                    Position = new Vector3(Origo.X + 0.3f, Origo.Y, Origo.Z + z),
                };
            }

            var task = new Task(async () =>
            {
                await loader.LoadTexturesAsync(textures);
            });
            task.Start();
            task.Wait();
        }

        private void Update()
        {
            var step = PixelSize(Level) * TileResolution;

            tileX = CenterX / step;
            tileY = CenterY / step;

            dx = (double)((CenterX % step) - (step / 2)) / (double)(step) * tileSize;
            dy = (double)((step / 2) - (CenterY % step)) / (double)(step) * tileSize;
        }

        private string Image(string file, int idx)
        {
            var y = idx / 7;
            var x = idx % 7;

            var step = PixelSize(Level) * TileResolution;

            return file
                        + "&x=" + ((tileX + (x - 3)) * step).ToString()
                        + "&y=" + ((tileY + (y - 3)) * step).ToString()
                        + "&w=" + TileResolution.ToString()
                        + "&h=" + TileResolution.ToString()
                        + "&level=" + Level.ToString();
        }

        private void Rotate(Direction direction)
        {
            switch (direction)
            {
                case Direction.LEFT:
                    Angle -= Math.PI / 36;
                    if (Angle < 0)
                    {
                        Angle += 2.0 * Math.PI;
                    }
                    break;
                case Direction.RIGHT:
                    Angle += Math.PI / 36;
                    if (Angle >= 2.0 * Math.PI)
                    {
                        Angle -= 2.0 * Math.PI;
                    }
                    break;
                case Direction.DOWN:
                    if (Settings.Scaler > 1)
                    {
                        Settings.Scaler /= 2;
                    }
                    break;
                case Direction.UP:
                    if (Settings.Scaler < 1024)
                    {
                        Settings.Scaler *= 2;
                    }
                    break;
            }

            SetCorners();
            UpdateImages();
        }

        protected override void Move(Direction direction, int number)
        {
            var moveStep = PixelSize(Level) * Settings.Scaler;

            switch (direction)
            {
                case Direction.LEFT:
                    CenterX += (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterY += (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
                case Direction.RIGHT:
                    CenterX -= (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterY -= (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
                case Direction.DOWN:
                    CenterY -= (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterX += (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
                case Direction.UP:
                    CenterY += (int)(Math.Cos(-1 * Angle) * moveStep);
                    CenterX -= (int)(Math.Sin(-1 * Angle) * moveStep);
                    break;
            }

            SetCorners();
            UpdateImages();
        }

        protected override void SetCorners()
        {
            var d = diagonal * (double)(PixelSize(Level));

            var beta1 = Angle - (Math.PI / 4);
            var beta2 = (Math.PI / 4) - Angle;

            var xx1 = (int)(Math.Round(d * Math.Cos(beta1)));
            var yy1 = (int)(Math.Round(d * Math.Sin(beta1)));

            var xx2 = (int)(Math.Round(d * Math.Sin(beta2)));
            var yy2 = (int)(Math.Round(d * Math.Cos(beta2)));

            TopLeftX = CenterX - xx1;
            TopLeftY = CenterY + yy1;

            BottomRightX = CenterX + xx1;
            BottomRightY = CenterY - yy1;

            BottomLeftX = CenterX - xx2;
            BottomLeftY = CenterY + yy2;

            TopRightX = CenterX + xx2;
            TopRightY = CenterY - yy2;
        }

        protected override void Zoom(Direction direction, int number)
        {
            var c = Pointer.Coordinates();

            switch (direction)
            {
                case Direction.UP:
                    Level -= number;
                    if (Level < 0)
                    {
                        Level = 0;
                    }
                    break;
                case Direction.DOWN:
                    Level += number;
                    if (Level > Settings.MinScale)
                    {
                        Level = Settings.MinScale;
                    }
                    break;
            }

            CenterX = c.X;
            CenterY = c.Y;

            SetCorners();
            UpdateImages();
        }

        protected override void UpdateImages()
        {
            Update();
            Pointer.Update();

            var textures = new List<string>();

            var tiles = Rotator.Tiles(Angle, dx, dy, tileSize);
            var z = Settings.DistanceFromUser;

            if (tiles.Count > maxTiles)
            {
                ErrorString = "Too many tiles: " + tiles.Count.ToString();
                return;
            }

            var rotator1 = Matrix4x4.CreateRotationZ(
                -1.0f * (float)(Angle),
                new Vector3(Origo.X - 0.3f, Origo.Y, Origo.Z));

            var rotator2 = Matrix4x4.CreateRotationZ(
                -1.0f * (float)(Angle),
                new Vector3(Origo.X + 0.3f, Origo.Y, Origo.Z));

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
                    Origo.X - 0.3f - (float)dx,
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
                    Origo.X + 0.3f - (float)dx,
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

        internal override void OnKeyPressed(Windows.System.VirtualKey key)
        {
            VirtualKey = key;

            if (Pointer.Locked)
            {
                base.OnKeyPressed(key);
                return;
            }

            switch (key)
            {
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                    Rotate(Direction.LEFT);
                    break;

                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                    Rotate(Direction.RIGHT);
                    break;

                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    Rotate(Direction.UP);
                    break;

                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    Rotate(Direction.DOWN);
                    break;

                default:
                    base.OnKeyPressed(key);
                    break;
            }
        }

        protected override void SetPosition(float dX, float dY, float dZ)
        {
            base.SetPosition(dX, dY, dZ);
            UpdateImages();
        }
    }
}
