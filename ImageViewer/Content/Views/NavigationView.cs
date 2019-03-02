// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Content.Utils;
using System;
using System.Numerics;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Views
{
    abstract class NavigationView : DisposeView
    {
        private readonly double A45 = Math.PI / 4;

        protected int TileOffset(int level)
            => Constants.TileResolution * PixelSize(level);

        internal int Level { get; set; } = 3;

        internal int TopLeftY { get; set; } = 0;
        internal int TopLeftX { get; set; } = 0;

        internal int BottomLeftY { get; set; } = 0;
        internal int BottomLeftX { get; set; } = 0;

        internal int TopRightY { get; set; } = 0;
        internal int TopRightX { get; set; } = 0;

        internal int BottomRightY { get; set; } = 0;
        internal int BottomRightX { get; set; } = 0;

        internal int CenterY { get; set; } = 110000;
        internal int CenterX { get; set; } = 50000;

        internal double Angle { get; set; } = 0;

        internal int PixelSize(int level)
            => (int)Math.Pow(2, Settings.Multiplier * level);

        internal int Step => TileOffset(Level);       

        internal Vector3 Origo { get; set; } = Vector3.Zero;
        internal float RotationAngle { get; set; } = 0;

        protected NavigationView() {}

        protected abstract void UpdateImages();

        protected void MovePointer(float x, float y)
        {
            Pointer.SetDeltaXY(x, y);
        }

        protected void SetPosition(float dX, float dY, float dZ)
        {
            var dp = new Vector3(dX, dY, dZ);
            Origo = Origo + dp;

            foreach (var renderer in Tiles)
            {
                renderer.Position = renderer.Position + dp;
            }

            foreach (var renderer in statusItems)
            {
                renderer.Position = renderer.Position + dp;
            }

            settingViewer.SetPosition(dp);
            Pointer.SetPosition(dp);
            navigationFrame.SetPosition(dp);

            UpdateImages();
        }

        protected void SetAngle(float angle)
        {
            RotationAngle = RotationAngle + angle;
            if (RotationAngle >= 360.0f)
            {
                RotationAngle -= 360.0f;
            }
            if (RotationAngle < 0.0f)
            {
                RotationAngle += 360.0f;
            }

            var rotator = Matrix4x4.CreateRotationY((float)(Math.PI * RotationAngle / 180.0f), Pointer.Origo());

            foreach (var renderer in Tiles)
            {
                renderer.GlobalRotator = rotator;
            }

            foreach (var renderer in statusItems)
            {
                renderer.GlobalRotator = rotator;
            }

            settingViewer.SetRotator(rotator);

            Pointer.RotationY = RotationAngle;
            Pointer.SetRotator(rotator);

            navigationFrame.GlobalRotator = rotator;
        }

        protected void Scale(Direction direction, int number)
        {
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

            SetCorners();
            UpdateImages();

            navigationFrame.UpdateGeometry();
        }

        protected void Move(Direction direction, int number)
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

            navigationFrame.UpdatePosition();
        }

        protected void Zoom(Direction direction, int number)
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

            navigationFrame.UpdateGeometry();
        }

        protected void Rotate(Direction direction)
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
            }

            SetCorners();
            UpdateImages();

            navigationFrame.UpdateGeometry();
        }

        protected void SetPointer(Direction direction, int number)
        {
            if (direction == Direction.BACK)
            {
                switch (number)
                {
                    case 0: Pointer.Visible = false; break;
                    case 1: Pointer.Visible = true; break;
                    case 2:
                        Pointer.Locked = false;
                        Pointer.GlobalRotator = Matrix4x4.Identity;
                        break;
                    case 3: Pointer.Locked = true; break;
                    case 4: Pointer.AddTag(); break;
                    case 5: Pointer.RemoveTag(); break;
                }
            }
        }

        protected void Reset()
        {
            Level = 3;

            CenterY = 110000;
            CenterX = 50000;
            
            SetPosition(-1 * Origo.X, -1 * Origo.Y, -1 * Origo.Z);
            SetAngle(-1 * RotationAngle);

            SetCorners();
            UpdateImages();

            navigationFrame.UpdateGeometry();
        }

        private void SetCorners()
        {
            var d = Constants.Diagonal * (double)(PixelSize(Level));

            var beta1 = Angle - A45;
            var beta2 = A45 - Angle;

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
    }
}
