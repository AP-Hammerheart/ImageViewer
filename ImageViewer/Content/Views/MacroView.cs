// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.JsonClasses;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Views
{
    internal class MacroView : IDisposable
    {
        private Label[] labels;
        private readonly ImageRenderer image;
        private readonly MacroRenderer macroR;
        readonly TextureLoader loader;
        private int Type = 0;
        List<MacroJson> mj = new List<MacroJson>();
        string currentImage;
        int currentImageIndex = 0;

        internal Label[] Labels { get => labels; set => labels = value; }

        internal MacroView(
            DeviceResources deviceResources,
            TextureLoader loader)
        {
            this.loader = loader;
            image = new ImageRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z2 ),
                topLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z2 ),
                bottomRight: new Vector3( Constants.X00, Constants.Y1, Constants.Z1 ),
                topRight: new Vector3( Constants.X00, Constants.Y2, Constants.Z1 ),
                width: 3456,
                height: 2304 ) {
                Position = new Vector3( 0.0f, 0.0f, Constants.DistanceFromUser ),
            };
            
            macroR = new MacroRenderer( deviceResources: deviceResources,
                           loader: loader,
                           bottomLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z2 ),
                           topLeft: new Vector3(Constants.X00, Constants.Y2, Constants.Z2),
                           bottomRight: new Vector3(Constants.X00, Constants.Y1, Constants.Z1),
                           topRight: new Vector3(Constants.X00, Constants.Y2, Constants.Z1),
                           width: 3456,
                           height: 2304 ) {
                Position = new Vector3( 0.0f, 0.0f, Constants.DistanceFromUser ),
            };

            var labelTexts = new string[] 
            {
                "Z18"
                //"M", "N", "O", "P", "Q", "R", "S", "T", "U", "V",
                //"W", "X", "Y", "Z", "Z1", "Z2", "Z3",
                //"Z4", "Z5", "Z6", "Z7", "Z8", "Z9", "Z10", "Z11", "Z12",
                //"Z13"
            };

            var coords = new int[][]
            {

                    //                "P1x": 690.9188842773438,
                    //"P1y": 897.7476806640625,
                    //"P2x": 690.9188842773438,
                    //"P2y": 1416.2972412109375,
                    //"P3x": 1399.397216796875,
                    //"P3y": 1416.2972412109375,
                    //"P4x": 1399.397216796875,
                    //"P4y": 897.7476806640625,

                new int[] { 690, 897, 1399, 897, 690, 1416, 1399, 1416 },
                //new int[] { 212, 221, 344, 48, 495, 438, 627, 265 },
                //new int[] { 614, 457, 616, 284, 922, 458, 924, 285 },
                //new int[] { 634, 279, 636, 103, 871, 281, 873, 105 },
                //new int[] { 1035, 510, 1036, 324, 1333, 512, 1334, 325 },
                //new int[] { 1009, 320, 1010, 133, 1308, 322, 1309, 135 },
                //new int[] { 1728, 405, 1688, 218, 1951, 357, 1911, 169 },
                //new int[] { 1491, 509, 1435, 325, 1735, 433, 1679, 252 },
                //new int[] { 2075, 547, 2004, 338, 2323, 462, 2252, 254 },
                //new int[] { 2807, 677, 2703, 492, 3009, 563, 2905, 378 },
                //new int[] { 2957, 452, 2960, 241, 3128, 456, 3131, 244 },
                //new int[] { 347, 1197, 349, 1005, 645, 1200, 647, 1008 },
                //new int[] { 347, 983, 349, 792, 645, 985, 647, 794 },
                //new int[] { 984, 992, 985, 801, 1282, 995, 1284, 803 },
                //new int[] { 1632, 1203, 1644, 939, 1823, 1211, 1835, 948 },
                //new int[] { 1471, 885, 1471, 694, 1770, 888, 1770, 696 },
                //new int[] { 1423, 1177, 1434, 914, 1615, 1185, 1625, 921 },
                //new int[] { 2121, 1221, 2131, 978, 2313, 1228, 2321, 985 },
                //new int[] { 120, 1799, 129, 1557, 328, 1808, 337, 1566 },
                //new int[] { 353, 1765, 362, 1568, 563, 1773, 571, 1577 },
                //new int[] { 149, 1555, 151, 1370, 448, 1558, 450, 1372 },
                //new int[] { 782, 1727, 784, 1535, 1081, 1730, 1083, 1538 },
                //new int[] { 1070, 1803, 1168, 1589, 1243, 1881, 1340, 1667 },
                //new int[] { 1370, 1720, 1372, 1564, 1595, 1722, 1596, 1566 },
                //new int[] { 2180, 1751, 2268, 1554, 2327, 1816, 2415, 1621 },
                //new int[] { 1897, 1630, 1898, 1481, 2094, 1631, 2095, 1483 },
                //new int[] { 3042, 1797, 3043, 1636, 3204, 1798, 3205, 1638 },
                //new int[] { 488, 2137, 398, 2014, 676, 1997, 586, 1874 },
            };

            labels = new Label[labelTexts.Length];

            for (var i = 0; i < labelTexts.Length; i++)
            {
                labels[i] = new Label(
                    deviceResources: deviceResources,
                    loader: loader,
                    image: image,
                    coordinates: coords[i],
                    labelText: labelTexts[i]);
            }
            GetMacroFromServer();
            currentImage = ";MACRO;" + mj[0].name;
            UpdateImage();
            ChangeType();
        }

        protected void UpdateImage() {
            var ims = currentImage;
            var textures = new List<string>();
            textures.Add( ims );
            macroR.TextureID = ims;
            var task = new Task( async () => {
                await loader.LoadTexturesAsync( textures );
            } );
            task.Start();
            task.Wait();
        }

        internal void GetMacroFromServer() {
            //get number of images in dicom directory.
            string url = Settings.jsonURL + Settings.CaseID + "/macro/";
            using( var client = new System.Net.Http.HttpClient() ) {
                var j = client.GetStringAsync( url );
                List<RadiologyJson> rj = new List<RadiologyJson>();
                mj = JsonConvert.DeserializeObject<List<MacroJson>>( j.Result );
            }
        }

        internal void ChangeImageUp() {
            currentImageIndex--;
            if( currentImageIndex < 0 ) {
                currentImageIndex = 0;
            }
            currentImage = ";MACRO;" + mj[currentImageIndex].name;
            UpdateImage();
        }

        internal void ChangeImageDown() {
            currentImageIndex++;
            if( currentImageIndex >= mj.Count ) {
                currentImageIndex = mj.Count-1;
            }
            currentImage = ";MACRO;" + mj[currentImageIndex].name;
            UpdateImage();
        }

        internal void ChangeCase() {
            GetMacroFromServer();
            currentImageIndex = 0;
            currentImage = ";MACRO;" + mj[currentImageIndex].name;
            UpdateImage();
        }

        internal void ChangeToImage( string path ) {
            int index = path.LastIndexOf( "/" );
            string s = path.Substring( index + 1 );
            currentImage = s;
            for( int i = 0; i < mj.Count; i++ ) {
                if( mj[i].name.ToLower() == s.ToLower() ) {
                    currentImageIndex = i;
                    break;
                }
            }
            //System.Diagnostics.Debug.WriteLine( s );
            //System.Diagnostics.Debug.WriteLine( currentImageIndex + ", " + currentImage );
            UpdateImage();
        }

        internal void SetLabel(bool isHighlight) {
            for( int i = 0; i < labels.Length; i++ ) {
                labels[i].SetColorToDefault();
            }
            if( isHighlight ) {
                labels[0].HighLightLabel();
            } else {
                labels[0].SetColorToDefault();
            }           
        }

        internal void Update(StepTimer timer)
        {
            foreach (var renderer in labels)
            {
                renderer?.Update(timer);
            }

            macroR?.Update(timer);
            image?.Update( timer );
        }

        internal void Render()
        {
            macroR?.Render();
            image?.Render();
            foreach (var renderer in labels)
            {
                renderer?.Render();
            }
        }

        internal void ChangeType()
        {
            Type = (Type + 1) % 3;
            foreach (var renderer in labels)
            {
                renderer.Type = Type;
            }
        }

        internal void SetPosition(Vector3 dp)
        {
            macroR.Position += dp;
            image.Position += dp;
            foreach (var label in labels)
            {
                label?.SetPosition(dp);
            }
        }

        internal  void SetRotator(Matrix4x4 rotator)
        {
            macroR.GlobalRotator = rotator;
            image.GlobalRotator = rotator;
            foreach (var label in labels)
            {
                label?.SetRotator(rotator);
            }
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            foreach (var renderer in labels)
            {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            await macroR?.CreateDeviceDependentResourcesAsync();
            await image?.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources()
        {
            foreach (var renderer in labels)
            {
                renderer?.ReleaseDeviceDependentResources();
            }

            macroR?.ReleaseDeviceDependentResources();
            image?.ReleaseDeviceDependentResources();
        }

        internal void Dispose()
        {
            if (labels != null)
            {
                foreach (var renderer in labels)
                {
                    renderer?.Dispose();
                }
                labels = null;
            }

            macroR?.Dispose();
            image?.Dispose();
        }

        void IDisposable.Dispose()
        {
            foreach (var renderer in labels)
            {
                renderer?.Dispose();
            }

            macroR?.Dispose();
            image?.Dispose();
        }
    }
}
