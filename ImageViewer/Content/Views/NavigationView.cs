// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Content.Renderers.ThreeD;
using ImageViewer.Content.Utils;
using System;
using System.Numerics;
using static ImageViewer.ImageViewerMain;
using ImageViewer.Content.JsonClasses;
using Newtonsoft.Json;

namespace ImageViewer.Content.Views
{
    abstract class NavigationView : DisposeView {
        private readonly double A45 = Math.PI / 4;

        protected int TileOffset( int level )
            => Constants.TileResolution * PixelSize( level );

        internal int Level { get; set; } = 7;

        internal int TopLeftY { get; set; } = 0;
        internal int TopLeftX { get; set; } = 0;

        internal int BottomLeftY { get; set; } = 0;
        internal int BottomLeftX { get; set; } = 0;

        internal int TopRightY { get; set; } = 0;
        internal int TopRightX { get; set; } = 0;

        internal int BottomRightY { get; set; } = 0;
        internal int BottomRightX { get; set; } = 0;

        //internal int CenterY { get; set; } = 110000;
        //internal int CenterX { get; set; } = 50000;

        internal int CenterY { get; set; } = 125440 /2;
        internal int CenterX { get; set; } = 107520 / 2;

        internal double Angle { get; set; } = 0;

        internal int PixelSize(int level)
            => (int)Math.Pow(2, Settings.Multiplier * level);

        internal int Step => TileOffset(Level);       

        internal Vector3 Origo { get; set; } = Vector3.Zero;
        internal float RotationAngle { get; set; } = 0;

        //read connections json
        ImageConnections imageConnections = new ImageConnections();

        protected NavigationView() {
            string url = Settings.jsonURL + Settings.CaseID + "/connections/";
            using( var client = new System.Net.Http.HttpClient() ) {
                var j = client.GetStringAsync( url );
                imageConnections = JsonConvert.DeserializeObject<ImageConnections>( j.Result );
            }
        }

        protected abstract void UpdateImages();

        protected void MovePointer(float x, float y)
        {
            ((PointerRenderer)Pointers[0]).SetDeltaXY(x, y);
        }

        protected void Refresh()
        {
            SetCorners();
            UpdateImages();
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


            ((PointerRenderer)Pointers[0]).SetPosition(dp);
            Pointers[1].SetPosition(dp);

            navigationFrame.SetPosition(dp);
            navMacroFrame.SetPosition(dp);
            macro.SetPosition(dp);
            radiology.SetPosition( dp );
            histo.SetPosition( dp );
            model.Position = model.Position + dp;
            caseView.SetPosition( dp );

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

            var rotator = Matrix4x4.CreateRotationY((float)(Math.PI * RotationAngle / 180.0f), Pointers[0].Origo());

            foreach (var renderer in Tiles)
            {
                renderer.GlobalRotator = rotator;
            }

            foreach (var renderer in statusItems)
            {
                renderer.GlobalRotator = rotator;
            }

            settingViewer.SetRotator(rotator);

            Pointers[0].RotationY = RotationAngle;
            ((PointerRenderer)Pointers[0]).SetRotator(rotator);

            Pointers[1].RotationY = 45.0f + RotationAngle;
            Pointers[1].SetRotator(rotator);

            navigationFrame.SetRotator(rotator);
            navMacroFrame.SetRotator(rotator);
            macro.SetRotator(rotator);
            radiology.SetRotator( rotator );
            histo.SetRotator( rotator );
            caseView.SetRotator( rotator );
            model.GlobalRotator = rotator;
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

            Refresh();
            navigationFrame.UpdateGeometry();
            navMacroFrame.Scale(direction);
            navMacroFrame.UpdateGeometry();
        }

        protected void Move(Direction direction, int number)
        {
            //var moveStep = PixelSize(Level) * number;

            //switch (direction)
            //{
            //    case Direction.RIGHT:
            //        CenterX += (int)(Math.Cos(-1 * Angle) * moveStep);
            //        CenterY += (int)(Math.Sin(-1 * Angle) * moveStep);
            //        break;
            //    case Direction.LEFT:
            //        CenterX -= (int)(Math.Cos(-1 * Angle) * moveStep);
            //        CenterY -= (int)(Math.Sin(-1 * Angle) * moveStep);
            //        break;
            //    case Direction.UP:
            //        CenterY -= (int)(Math.Cos(-1 * Angle) * moveStep);
            //        CenterX += (int)(Math.Sin(-1 * Angle) * moveStep);
            //        break;
            //    case Direction.DOWN:
            //        CenterY += (int)(Math.Cos(-1 * Angle) * moveStep);
            //        CenterX -= (int)(Math.Sin(-1 * Angle) * moveStep);
            //        break;
            //}

            navigationFrame.UpdatePosition(direction, number);
            navMacroFrame.UpdatePosition(direction, number);
            Refresh();
            //navigationFrame.UpdatePosition();
            //navMacroFrame.UpdatePosition();
        }

        protected void Zoom(Direction direction, int number)
        {
            BasePointerRenderer.Coordinate c;

            if (Pointers[0].Inside)
            {
                c = ((PointerRenderer)Pointers[0]).Coordinates();
            }
            else if (Pointers[1].Inside)
            {
                c = Pointers[1].XY();
            }
            else
            {
                return;
            }
            
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

            Refresh();
            navigationFrame.UpdateGeometry();
            navMacroFrame.Scale(direction);
            navMacroFrame.UpdateGeometry();
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

            Refresh();
            navigationFrame.UpdateGeometry();
            navMacroFrame.UpdateGeometry();
        }

        protected void SetPointer(Direction direction, int number)
        {
            if (direction == Direction.BACK)
            {
                switch (number)
                {
                    case 0: Pointers[0].Visible = false; break;
                    case 1: Pointers[0].Visible = true; break;
                    case 2:
                        Pointers[0].Locked = false;
                        Pointers[0].GlobalRotator = Matrix4x4.Identity;
                        break;
                    case 3: Pointers[0].Locked = true; break;
                    case 4: ((PointerRenderer)Pointers[0]).AddTag(); break;
                    case 5: ((PointerRenderer)Pointers[0]).RemoveTag(); break;
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

            Refresh();
            navigationFrame.UpdateGeometry();
            navMacroFrame.UpdateGeometry();
        }

        private void SetCorners()
        {
            if (CenterX < -1 * Settings.MaxResolutionX) CenterX = -1 * Settings.MaxResolutionX;
            if (CenterY < -1 * Settings.MaxResolutionY) CenterY = -1 * Settings.MaxResolutionY;

            if (CenterX > Settings.MaxResolutionX) CenterX = Settings.MaxResolutionX;
            if (CenterY > Settings.MaxResolutionY) CenterY = Settings.MaxResolutionY;

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

        protected void NextRadiologyImage( int step ) {
            radiology.NextImage( step );
            CheckMatchFromRadio();
        }

        protected void PrevRadiologyImage( int step ) {
            radiology.PrevImage( step );
            CheckMatchFromRadio();
        }
        protected void ZoomRadiologyImage()
        {
            radiology.Zoom();
        }

        protected void ToggleCaseSelectionMenu(bool isShow) {
            caseView.showCaseSelection = isShow;
            caseView.ShowMenu();
        }

        internal void ConfirmSelectedID() {
            caseView.ConfirmSelectedID();
            radiology.ChangeCase();
            macro.ChangeCase();
            histo.ChangeCase();
        }

        internal void ChangeSelectedIDUp() {
            caseView.ChangeSelectedIDUp();
        }

        internal void ChangeSelectedIDDown() {
            caseView.ChangeSelectedIDDown();
        }

        internal void ChangeMacroImageUp() {
            macro.ChangeImageUp();
        }

        internal void ChangeMacroImageDown() {
            macro.ChangeImageDown();
        }

        internal void ChangeHistologyMapUp() {
            histo.ChangeImageUp();
        }
        internal void ChangeHistologyMapDown() {
            histo.ChangeImageDown();
        }

        internal void ChangeOverviewLevelUp() {
            //histo.ChangeLevelUp();
        }

        internal void ChangeOverviewLevelDown() {
            //histo.ChangeLevelDown();
        }

        internal void CheckMatchFromRadio() {
            //if( radiology.Level >= imageConnections.Items[0].Images[0].dicom[0].imageIndexStart &&
            //    radiology.Level <= imageConnections.Items[0].Images[0].dicom[0].imageIndexEnd ) {
            //    histo.ChangeToImage( imageConnections.Items[0].Images[0].histology[0].imageSource );
            //    macro.SetLabel( true );
            //}
        }

    }
}
