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

namespace ImageViewer.Content.Views {
    internal class HistologyView : IDisposable {
        //private readonly ImageRenderer image;
        private readonly HistologyRenderer histo;
        readonly TextureLoader loader;
        private int Type = 0;

        string currentImage;
        int currentImageIndex = 0;
        List<HistologyJson> hj = new List<HistologyJson>();
        int level = 7;
        int w = 950;//3456;
        int h = 850;//2304;


        internal HistologyView(
            DeviceResources deviceResources,
            TextureLoader loader ) {
            this.loader = loader;
            //image = new ImageRenderer(
            //    deviceResources: deviceResources,
            //    loader: loader,
            //    bottomLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z1 ),
            //    topLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z1 ),
            //    bottomRight: new Vector3( Constants.X01, Constants.Y1, Constants.Z0 ),
            //    topRight: new Vector3( Constants.X01, Constants.Y2, Constants.Z0 ),
            //    width: 3456,
            //    height: 2304 ) {
            //    Position = new Vector3( 0.0f, 0.0f, Constants.DistanceFromUser ),
            //    //TextureFile = "Content\\Textures\\base.jpg",
            //};

            histo = new HistologyRenderer( deviceResources: deviceResources,
                           loader: loader,
                           bottomLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z1 ),
                           topLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z1 ),
                           bottomRight: new Vector3( Constants.X01, Constants.Y1, Constants.Z0 ),
                           topRight: new Vector3( Constants.X01, Constants.Y2, Constants.Z0 ),
                           width: 3456,
                           height: 2304 ) {
                Position = new Vector3( 0.0f, 0.0f, Constants.DistanceFromUser ),
            };
            GetHistoFromServer();
            currentImage = ";histology;" + hj[currentImageIndex].Name + "&x=0&y=0" + "&w=" + w.ToString() + "&h=" + h.ToString() + "&level=" + level.ToString();
            Settings.Image1 = ";histology;" + hj[currentImageIndex].Name;
            Settings.Image2 = ";histology;" + hj[currentImageIndex].Name;
            UpdateImage();
        }

        protected void UpdateImage() {
            var ims = currentImage;
            var textures = new List<string>();
            textures.Add( ims );
            histo.TextureID = ims;
            var task = new Task( async () => {
                await loader.LoadTexturesAsync( textures );
            } );
            task.Start();
            task.Wait();
        }

        internal void GetHistoFromServer() {
            //get number of images in dicom directory.
            string url = Settings.jsonURL + Settings.CaseID + "/histology/";
            using( var client = new System.Net.Http.HttpClient() ) {
                var j = client.GetStringAsync( url );
                List<HistologyJson> rj = new List<HistologyJson>();
                hj = JsonConvert.DeserializeObject<List<HistologyJson>>( j.Result );
            }
        }

        internal void ChangeImageUp() {
            currentImageIndex--;
            if( currentImageIndex < 0 ) {
                currentImageIndex = 0;
            }
            currentImage = ";histology;" + hj[currentImageIndex].Name + "&x=0&y=0" + "&w=" + w.ToString() + "&h=" + h.ToString() + "&level=" + level.ToString();
            Settings.Image1 = ";histology;" + hj[currentImageIndex].Name;
            Settings.Image2 = ";histology;" + hj[currentImageIndex].Name;
            UpdateImage();
        }

        internal void ChangeImageDown() {
            currentImageIndex++;
            if( currentImageIndex >= hj.Count ) {
                currentImageIndex = hj.Count - 1;
            }
            currentImage = ";histology;" + hj[currentImageIndex].Name + "&x=0&y=0" + "&w=" + w.ToString() + "&h=" + h.ToString() + "&level=" + level.ToString();
            Settings.Image1 = ";histology;" + hj[currentImageIndex].Name;
            Settings.Image2 = ";histology;" + hj[currentImageIndex].Name;
            UpdateImage();
        }

        internal void ChangeCase() {
            GetHistoFromServer();
            currentImageIndex = 0;
            currentImage = ";histology;" + hj[currentImageIndex].Name + "&x=0&y=0" + "&w=" + w.ToString() + "&h=" + h.ToString() + "&level=" + level.ToString();
            Settings.Image1 = ";histology;" + hj[currentImageIndex].Name;
            Settings.Image2 = ";histology;" + hj[currentImageIndex].Name;
            UpdateImage();
        }

        internal void ChangeToImage( string path ) {
            int index = path.LastIndexOf( "/" );
            string s = path.Substring( index + 1 );
            //currentImage = s;
            for( int i = 0; i < hj.Count; i++ ) {
                if( hj[i].Name.ToLower() == s.ToLower() ) {
                    currentImageIndex = i;
                    break;
                }
            }
            currentImage = ";histology;" + hj[currentImageIndex].Name + "&x=0&y=0" + "&w=" + w.ToString() + "&h=" + h.ToString() + "&level=" + level.ToString();
            Settings.Image1 = ";histology;" + hj[currentImageIndex].Name;
            Settings.Image2 = ";histology;" + hj[currentImageIndex].Name;
            //System.Diagnostics.Debug.WriteLine( s );
            //System.Diagnostics.Debug.WriteLine( currentImageIndex + ", " + currentImage );
            UpdateImage();
        }

        internal void ChangeLevelUp() {
            level++;
            if( level >= 9 ) {
                level = 9;
            }
            currentImage = ";histology;" + hj[currentImageIndex].Name + "&x=0&y=0" + "&w=" + w.ToString() + "&h=" + h.ToString() + "&level=" + level.ToString();
            UpdateImage();
        }

        internal void ChangeLevelDown() {
            level--;
            if( level <= 5 ) {
                level = 5;
            }
            currentImage = ";histology;" + hj[currentImageIndex].Name + "&x=0&y=0" + "&w=" + w.ToString() + "&h=" + h.ToString() + "&level=" + level.ToString();
            UpdateImage();
        }

        internal void Update( StepTimer timer ) {
            //foreach( var renderer in labels ) {
            //    renderer?.Update( timer );
            //}

            histo?.Update( timer );
            //image?.Update( timer );
        }

        internal void Render() {
            histo?.Render();
            //image?.Render();

            //foreach( var renderer in labels ) {
            //    renderer?.Render();
            //}
        }

        internal void ChangeType() {
            Type = (Type + 1) % 3;
            //foreach( var renderer in labels ) {
            //    renderer.Type = Type;
            //}
        }

        internal void SetPosition( Vector3 dp ) {
            histo.Position += dp;
            //image.Position += dp;

            //foreach( var label in labels ) {
            //    label?.SetPosition( dp );
            //}
        }

        internal void SetRotator( Matrix4x4 rotator ) {
            histo.GlobalRotator = rotator;
            //image.GlobalRotator = rotator;
            //foreach( var label in labels ) {
            //    label?.SetRotator( rotator );
            //}
        }

        internal async Task CreateDeviceDependentResourcesAsync() {
            //foreach( var renderer in labels ) {
            //    await renderer?.CreateDeviceDependentResourcesAsync();
            //}

            await histo?.CreateDeviceDependentResourcesAsync();
           // await image?.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources() {
            //foreach( var renderer in labels ) {
            //    renderer?.ReleaseDeviceDependentResources();
            //}

            histo?.ReleaseDeviceDependentResources();
           // image?.ReleaseDeviceDependentResources();
        }

        internal void Dispose() {
            //if( labels != null ) {
            //    foreach( var renderer in labels ) {
            //        renderer?.Dispose();
            //    }
            //    labels = null;
            //}

            histo?.Dispose();
           // image?.Dispose();
        }

        void IDisposable.Dispose() {
            //foreach( var renderer in labels ) {
            //    renderer?.Dispose();
            //}

            histo?.Dispose();
           // image?.Dispose();
        }
    }
}
