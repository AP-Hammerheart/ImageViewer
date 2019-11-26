// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Renderers.Dev;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Renderers.Status;
using ImageViewer.Content.Renderers.ThreeD;
using ImageViewer.Content.Utils;
using System.Numerics;
using Windows.UI;

namespace ImageViewer.Content.Views
{
    abstract class BaseView : SpeechView
    {
        protected BaseView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(main, loader)
        {
            navigationFrame = new NavigationRenderer(
                deviceResources: deviceResources,
                loader: loader,
                view: this,
                depth: 0.005f,
                thickness: 0.002f,
                topLeft: new Vector3(Constants.X00, Constants.Y2, Constants.Z1 + Constants.DistanceFromUser),
                bottomLeft: new Vector3(Constants.X00, Constants.Y1, Constants.Z1 + Constants.DistanceFromUser),
                topRight: new Vector3(Constants.X01, Constants.Y2, Constants.Z0 + Constants.DistanceFromUser))
            {
                RotationY = 45,
            };

            settingViewer = new SettingViewer(main, deviceResources, loader);

            statusItems = new BasePlaneRenderer[24];

            statusItems[0] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X01, Constants.Y3, Constants.Z0),
                topLeft: new Vector3(Constants.X01, Constants.Y4, Constants.Z0),
                bottomRight: new Vector3(Constants.X02, Constants.Y3, Constants.Z0),
                topRight: new Vector3(Constants.X02, Constants.Y4, Constants.Z0))
            {
                TextPosition = new Vector2(20, 10),
                Text = "WSI",
                FontSize = 40.0f,
                ImageWidth = 640,
            };

            statusItems[1] = new ZoomRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X02, Constants.Y3, Constants.Z0),
                topLeft: new Vector3(Constants.X02, Constants.Y4, Constants.Z0),
                bottomRight: new Vector3(Constants.X05, Constants.Y3, Constants.Z0),
                topRight: new Vector3(Constants.X05, Constants.Y4, Constants.Z0))
            {
                ImageWidth = 720
            };

            statusItems[2] = new MemoryUseRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X05, Constants.Y3, Constants.Z0),
                topLeft: new Vector3(Constants.X05, Constants.Y4, Constants.Z0),
                bottomRight: new Vector3(Constants.X06, Constants.Y3, Constants.Z0),
                topRight: new Vector3(Constants.X06, Constants.Y4, Constants.Z0))
            {
                ImageWidth = 160
            };

            statusItems[3] = new FpsRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X06, Constants.Y3, Constants.Z0),
                topLeft: new Vector3(Constants.X06, Constants.Y4, Constants.Z0),
                bottomRight: new Vector3(Constants.X07, Constants.Y3, Constants.Z0),
                topRight: new Vector3(Constants.X07, Constants.Y4, Constants.Z0))
            {
                ImageWidth = 160
            };

            statusItems[4] = new ClockRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X07, Constants.Y3, Constants.Z0),
                topLeft: new Vector3(Constants.X07, Constants.Y4, Constants.Z0),
                bottomRight: new Vector3(Constants.X09, Constants.Y3, Constants.Z0),
                topRight: new Vector3(Constants.X09, Constants.Y4, Constants.Z0))
            {
                ImageWidth = 240
            };

            statusItems[5] = new NameRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X01, Constants.Y2, Constants.Z0),
                topLeft: new Vector3(Constants.X01, Constants.Y3, Constants.Z0),
                bottomRight: new Vector3(Constants.X04, Constants.Y2, Constants.Z0),
                topRight: new Vector3(Constants.X04, Constants.Y3, Constants.Z0))
            {
                TextPosition = new Vector2(10, 10),
                ImageWidth = 960,
                ImageHeight = 48,
                FontSize = 25.0f,
                BackgroundColor = Colors.LightGray,
                Index = 0
            };

            statusItems[6] = new NameRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X04, Constants.Y2, Constants.Z0),
                topLeft: new Vector3(Constants.X04, Constants.Y3, Constants.Z0),
                bottomRight: new Vector3(Constants.X09, Constants.Y2, Constants.Z0),
                topRight: new Vector3(Constants.X09, Constants.Y3, Constants.Z0))
            {
                TextPosition = new Vector2(10, 10),
                ImageWidth = 960,
                ImageHeight = 48,
                FontSize = 25.0f,
                BackgroundColor = Colors.LightGray,
                Index = 1
            };

            statusItems[7] = new KeyRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X01, Constants.Y0, Constants.Z0),
                topLeft: new Vector3(Constants.X01, Constants.Y1, Constants.Z0),
                bottomRight: new Vector3(Constants.X03, Constants.Y0, Constants.Z0),
                topRight: new Vector3(Constants.X03, Constants.Y1, Constants.Z0))
            {
                ImageWidth = 800
            };

            statusItems[8] = new DebugRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X03, Constants.Y0, Constants.Z0),
                topLeft: new Vector3(Constants.X03, Constants.Y1, Constants.Z0),
                bottomRight: new Vector3(Constants.X06, Constants.Y0, Constants.Z0),
                topRight: new Vector3(Constants.X06, Constants.Y1, Constants.Z0))
            {
                ImageWidth = 720
            };

            statusItems[9] = new TileCounterRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X06, Constants.Y0, Constants.Z0),
                topLeft: new Vector3(Constants.X06, Constants.Y1, Constants.Z0),
                bottomRight: new Vector3(Constants.X08, Constants.Y0, Constants.Z0),
                topRight: new Vector3(Constants.X08, Constants.Y1, Constants.Z0))
            {
                ImageWidth = 240
            };

            statusItems[10] = new ScalerRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X08, Constants.Y0, Constants.Z0),
                topLeft: new Vector3(Constants.X08, Constants.Y1, Constants.Z0),
                bottomRight: new Vector3(Constants.X09, Constants.Y0, Constants.Z0),
                topRight: new Vector3(Constants.X09, Constants.Y1, Constants.Z0))
            {
                ImageWidth = 160
            };

            statusItems[11] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X00, Constants.Y3, Constants.Z1),
                topLeft: new Vector3(Constants.X00, Constants.Y4, Constants.Z1),
                bottomRight: new Vector3(Constants.X01, Constants.Y3, Constants.Z0),
                topRight: new Vector3(Constants.X01, Constants.Y4, Constants.Z0))
            {
                TextPosition = new Vector2(20, 10),
                Text = "Z4",
                FontSize = 40.0f,
                ImageWidth = 960,
                ImageHeight = 80,
            };

            statusItems[12] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X00, Constants.Y2, Constants.Z1),
                topLeft: new Vector3(Constants.X00, Constants.Y3, Constants.Z1),
                bottomRight: new Vector3(Constants.X01, Constants.Y2, Constants.Z0),
                topRight: new Vector3(Constants.X01, Constants.Y3, Constants.Z0))
            {
                TextPosition = new Vector2(10, 10),
                Text = "", //"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                FontSize = 25.0f,
                ImageWidth = 960,
                ImageHeight = 48,
                BackgroundColor = Colors.LightGray
            };

            //statusItems[13] = new ImageRenderer(
            //    deviceResources: deviceResources,
            //    loader: loader,
            //    bottomLeft: new Vector3(Constants.X00, Constants.Y1, Constants.Z1),
            //    topLeft: new Vector3(Constants.X00, Constants.Y2, Constants.Z1),
            //    bottomRight: new Vector3(Constants.X01, Constants.Y1, Constants.Z0),
            //    topRight: new Vector3(Constants.X01, Constants.Y2, Constants.Z0))
            //{
            //    Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser),
            //    TextureFile = "Content\\Textures\\base.png",
            //};

            histo = new HistologyView(deviceResources: deviceResources, loader: loader);

            statusItems[13] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X00, Constants.Y0, Constants.Z1),
                topLeft: new Vector3(Constants.X00, Constants.Y1, Constants.Z1),
                bottomRight: new Vector3(Constants.X01, Constants.Y0, Constants.Z0),
                topRight: new Vector3(Constants.X01, Constants.Y1, Constants.Z0))
            {
                TextPosition = new Vector2(20, 10),
                Text = "", //"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                ImageWidth = 960,
            };

            statusItems[14] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X09, Constants.Y3, Constants.Z0),
                topLeft: new Vector3(Constants.X09, Constants.Y4, Constants.Z0),
                bottomRight: new Vector3(Constants.X10, Constants.Y3, Constants.Z1),
                topRight: new Vector3(Constants.X10, Constants.Y4, Constants.Z1))
            {
                TextPosition = new Vector2(20, 10),
                Text = "Klinisk anamnes, frågeställning, preparat beskrivning",
                FontSize = 38.0f,
                ImageWidth = 960,
            };

            statusItems[15] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X09, Constants.Y1, Constants.Z0),
                topLeft: new Vector3(Constants.X09, Constants.Y3, Constants.Z0),
                bottomRight: new Vector3(Constants.X10, Constants.Y1, Constants.Z1),
                topRight: new Vector3(Constants.X10, Constants.Y3, Constants.Z1))
            {
                TextPosition = new Vector2(20, 10),
                Text = @"Anamnestext. 

Frågeställning / diagnos:                                               malignitet? staging

Anamnes: 
misstänkt distal kolangiocc, op pylorusbevarande whipple.tacksam us.

Preparatets natur:                                                          whipple resektat
Antal burkar / rör:                                                         1
Patienten ingår i standardiserade vårdförlopp:             nej",

                FontSize = 34.0f,
                ImageWidth = 1280,
                ImageHeight = 1344,
                BackgroundColor = Colors.LightGray,
            };

            statusItems[16] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X09, Constants.Y0, Constants.Z0),
                topLeft: new Vector3(Constants.X09, Constants.Y1, Constants.Z0),
                bottomRight: new Vector3(Constants.X10, Constants.Y0, Constants.Z1),
                topRight: new Vector3(Constants.X10, Constants.Y1, Constants.Z1))
            {
                TextPosition = new Vector2(20, 10),
                Text = "Provtagningsdatum:      2018-03-14 13:16 ",
                ImageWidth = 960,
            };

            statusItems[17] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X00, Constants.Y3, Constants.Z2),
                topLeft: new Vector3(Constants.X00, Constants.Y4, Constants.Z2),
                bottomRight: new Vector3(Constants.X00, Constants.Y3, Constants.Z1),
                topRight: new Vector3(Constants.X00, Constants.Y4, Constants.Z1))
            {
                Text = "", //"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                FontSize = 40.0f,
                ImageWidth = 1440,
            };

            statusItems[18] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X00, Constants.Y2, Constants.Z2),
                topLeft: new Vector3(Constants.X00, Constants.Y3, Constants.Z2),
                bottomRight: new Vector3(Constants.X00, Constants.Y2, Constants.Z1),
                topRight: new Vector3(Constants.X00, Constants.Y3, Constants.Z1))
            {
                Text = "", //"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                FontSize = 25.0f,
                ImageWidth = 1440,
                ImageHeight = 48,
                BackgroundColor = Colors.LightGray,
            };

            statusItems[19] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X00, Constants.Y0, Constants.Z2),
                topLeft: new Vector3(Constants.X00, Constants.Y1, Constants.Z2),
                bottomRight: new Vector3(Constants.X00, Constants.Y0, Constants.Z1),
                topRight: new Vector3(Constants.X00, Constants.Y1, Constants.Z1))
            {
                Text = "", //"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                ImageWidth = 1440,
            };

            statusItems[20] = new ImageRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(Constants.X10, Constants.Y0, Constants.Z3),
                topLeft: new Vector3(Constants.X10, Constants.Y4, Constants.Z3),
                bottomRight: new Vector3(Constants.X10, Constants.Y0, Constants.Z2),
                topRight: new Vector3(Constants.X10, Constants.Y4, Constants.Z2))
            {
                Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser),
                TextureFile = "Content\\Textures\\help.jpg",
            };

            macro = new MacroView(deviceResources: deviceResources, loader: loader);

            ///Radiology Top bar
            statusItems[21] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3( Constants.X00, Constants.Y3, Constants.Z3 ),
                topLeft: new Vector3( Constants.X00, Constants.Y4, Constants.Z3 ),
                bottomRight: new Vector3( Constants.X00, Constants.Y3, Constants.Z2 ),
                topRight: new Vector3( Constants.X00, Constants.Y4, Constants.Z2 ) )
                //bottomLeft: new Vector3( Constants.X00, Constants.Y0, Constants.Z3 ),
                //topLeft: new Vector3( Constants.X00, Constants.Y4, Constants.Z3 ),
                //bottomRight: new Vector3( Constants.X00, Constants.Y0, Constants.Z2 ),
                //topRight: new Vector3( Constants.X00, Constants.Y4, Constants.Z2 ),
            {
                TextPosition = new Vector2( 20, 10 ),
                Text = "RADIOLOGY HEADER",
                FontSize = 40.0f,
                ImageWidth = 960,
                ImageHeight = 80,
            };

            //Radiology Top LightGray bar
            statusItems[22] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z3 ),
                topLeft: new Vector3( Constants.X00, Constants.Y3, Constants.Z3 ),
                bottomRight: new Vector3( Constants.X00, Constants.Y2, Constants.Z2 ),
                topRight: new Vector3( Constants.X00, Constants.Y3, Constants.Z2 ) )
            {
                Text = "", //"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                FontSize = 25.0f,
                ImageWidth = 1440,
                ImageHeight = 48,
                BackgroundColor = Colors.LightGray,
            };

            //Radiology Bottom bar
            statusItems[23] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3( Constants.X00, Constants.Y0, Constants.Z3 ),
                topLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z3 ),
                bottomRight: new Vector3( Constants.X00, Constants.Y0, Constants.Z2 ),
                topRight: new Vector3( Constants.X00, Constants.Y1, Constants.Z2 ) )
            {
                Text = "", //"Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                ImageWidth = 1440,
            };

            radiology = new RadiologyView( deviceResources: deviceResources, loader: loader );

            Pointers = new BasePointerRenderer[2];

            Pointers[0] = new PointerRenderer(this, navigationFrame, deviceResources, loader, 
                new BasePointerRenderer.Corners(
                    topLeft: new Vector3(Constants.X01, Constants.Y2, Constants.DistanceFromUser),
                    bottomLeft: new Vector3(Constants.X01, Constants.Y1, Constants.DistanceFromUser),
                    topRight: new Vector3(Constants.X09, Constants.Y2, Constants.DistanceFromUser),
                    bottomRight: new Vector3(Constants.X09, Constants.Y1, Constants.DistanceFromUser)))
            {
                Position = new Vector3(0, 0, Constants.DistanceFromUser)
            };

            Pointers[1] = new BasePointerRenderer(navigationFrame, deviceResources, loader,
                new BasePointerRenderer.Corners(
                    topLeft: new Vector3(Constants.X00, Constants.Y2, Constants.Z1 + Constants.DistanceFromUser),
                    bottomLeft: new Vector3(Constants.X00, Constants.Y1, Constants.Z1 + Constants.DistanceFromUser),
                    topRight: new Vector3(Constants.X01, Constants.Y2, Constants.Z0 + Constants.DistanceFromUser),
                    bottomRight: new Vector3(Constants.X01, Constants.Y1, Constants.Z0 + Constants.DistanceFromUser)))              
            {
                RotationY = 45.0f,
                Position = new Vector3(0, 0, Constants.DistanceFromUser)
            };

            model = new ObjRenderer(deviceResources, loader)
            {
                Position = new Vector3(Constants.MX, Constants.MY, Constants.MZ)
            };
        }
    }
}
