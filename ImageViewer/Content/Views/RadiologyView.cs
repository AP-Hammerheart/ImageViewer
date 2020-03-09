using ImageViewer.Common;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ImageViewer.Content.JsonClasses;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Views {
    internal class RadiologyView : IDisposable {

        //private ImageRenderer image;
        private int Type = 0;
        private int level = 1;
        private int maxLevel = 515;
        private int minLevel = 1;
        readonly TextureLoader loader;
        private readonly ImageRenderer image;
        RadiologyRenderer dicom;

        Label[] labels;
        Dictionary<int, Label> labelLookUp = new Dictionary<int, Label>();

        string navString = string.Empty; //"&w=100&h=100&x=200&y=150";//read from connections on a connection, by connection basis.

        Label testlabel;

        int x = 0, y = 0;
        int w, h;
        int zoomFactor = 8; //need to be a multiple of 2
        int moveFactor = 4;

        public int Level {
            get => level;
        }

        internal RadiologyView( DeviceResources deviceResources, TextureLoader loader, ImageConnections  connections ) {
            this.loader = loader;
            image = new ImageRenderer(deviceResources: deviceResources,
                                       loader: loader,
                                       bottomLeft: new Vector3(Constants.X00, Constants.Y1, Constants.Z3),
                                       topLeft: new Vector3(Constants.X00, Constants.Y2, Constants.Z3),
                                       bottomRight: new Vector3(Constants.X00, Constants.Y1, Constants.Z2),
                                       topRight: new Vector3(Constants.X00, Constants.Y2, Constants.Z2),
                                       width: 512,
                                       height: 512) {
                Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser),
            };

            dicom = new RadiologyRenderer( deviceResources: deviceResources,
                                       loader: loader,
                                       bottomLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z3 ),
                                       topLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z3 ),
                                       bottomRight: new Vector3( Constants.X00, Constants.Y1, Constants.Z2 ),
                                       topRight: new Vector3( Constants.X00, Constants.Y2, Constants.Z2 ),
                                       width: 512,
                                       height: 512) {
                Position = new Vector3( 0.0f, 0.0f, Constants.DistanceFromUser ),
            };


            labels = new Label[connections.Items[0].Images.Count];
            System.Diagnostics.Debug.WriteLine("label count: " + connections.Items.Count);
            for(int i = 0; i < connections.Items.Count; i++) {
                for(int j = 0; j < connections.Items[i].Images.Count; j++) {
                    for(int k = 0; k < connections.Items[i].Images[j].dicom.Count; k++) {

                        //var labelTexts = new string[] {
                        //    connections.Items[0].Images[j].label
                        //};

                        var coords = new int[] {
                                (int)connections.Items[i].Images[j].dicom[k].P1x,
                                (int)connections.Items[i].Images[j].dicom[k].P1y,
                                (int)connections.Items[i].Images[j].dicom[k].P2x,
                                (int)connections.Items[i].Images[j].dicom[k].P2y,
                                (int)connections.Items[i].Images[j].dicom[k].P4x,
                                (int)connections.Items[i].Images[j].dicom[k].P4y,
                                (int)connections.Items[i].Images[j].dicom[k].P3x,
                                (int)connections.Items[i].Images[j].dicom[k].P3y,
                            };

                        //var coords = new int[][] {
     
                        //};

                        labels[j] = new Label(
                        deviceResources: deviceResources,
                        loader: loader,
                        image: image,
                        coordinates: coords,
                        labelText: connections.Items[i].Images[j].label);

                        for(int l = connections.Items[i].Images[j].dicom[k].imageIndexStart; l <= connections.Items[i].Images[j].dicom[k].imageIndexEnd; l++) {
                            labelLookUp.Add(l, labels[j]);
                        }
                    }
                }
                
            }


            var kord = new int[] {
                0,0,
                5120, 0,
                0,5120,
                5120,5120
                            };

            testlabel = new Label(
                       deviceResources: deviceResources,
                       loader: loader,
                       image: image,
                       coordinates: kord,
                       labelText: "tt");

            w = 512;
            h = 512;

            image.Width = w;
            image.Height = h;
            dicom.Width = w;
            dicom.Height = h;

            GetDICOMFromServer();
            UpdateImage( level );
        }


        internal void NextImage(int step) {
            level += step;
            if( level > maxLevel ) {
                level = maxLevel;
            }
            UpdateImage( level );
        }

        internal void PrevImage(int step) {
            level -= step;
            if( level < minLevel ) {
                level = minLevel;
            }
            UpdateImage( level );
        }

        internal void ZoomLabel()
        {
            //TODO: get label dimension for current label in series
            UpdateImage(level);
        }

        internal void UpdateZoomString() {
            navString = "&w=" + w + "&h=" + h + "&x=" + x + "&y=" + y;
            image.Width = w;
            image.Height = h;
            dicom.Width = w;
            dicom.Height = h;
        }

        internal void EmptyZoomString() {
            navString = string.Empty;
        }

        protected void UpdateImage( int level) {
            var ims = Image(level) +  navString;
            var textures = new List<string>();
            textures.Add( ims );
            dicom.TextureID = ims;
            var task = new Task( async () => {
                await loader.LoadTexturesAsync( textures );
            } );
            task.Start();
            task.Wait();
        }

        internal int PixelSize( int level )
            => (int)Math.Pow( 2, Settings.Multiplier * level );

        private string Image( int index ) {
            return @";dicom;1;" + Settings.CaseID + "-" + index;
        }

        internal void GetDICOMFromServer() {
            //get number of images in dicom directory.
            string url = Settings.jsonURL + Settings.CaseID + "/dicom/";
            using( var client = new System.Net.Http.HttpClient() ) {
                var j = client.GetStringAsync( url );
                List<RadiologyJson> rj = new List<RadiologyJson>();
                rj = JsonConvert.DeserializeObject<List<RadiologyJson>>( j.Result );
                maxLevel = rj[0].studyRecords[0].seriesRecords[0].imageRecords.Count;
            }
        }

        internal void ChangeCase() {
            GetDICOMFromServer();
            level = 1;
            UpdateImage( level );
        }

        internal void ChangeToImage( string path ) {
            int index = path.LastIndexOf( "/" );
            string s = path.Substring( index + 1 );
            level = int.Parse( "s" );
            //System.Diagnostics.Debug.WriteLine( level );
            UpdateImage(level);
        }

        internal void Move(Direction direction) {
            switch(direction) {
                case Direction.UP:
                    y += moveFactor;
                    break;
                case Direction.DOWN:
                    y -= moveFactor;
                    break;
                case Direction.LEFT:
                    x += moveFactor;
                    break;
                case Direction.RIGHT:
                    x -= moveFactor;
                    break;
            }
            UpdateZoomString();
            UpdateImage(level);
        }

        internal void Zoom(Direction direction) {
            switch(direction) {
                case Direction.UP:
                    w += zoomFactor;
                    h += zoomFactor;
                    break;
                case Direction.DOWN:
                    w -= zoomFactor;
                    h -= zoomFactor;
                    break;
            }
            UpdateZoomString();
            UpdateImage(level);
        }

        internal void Update( StepTimer timer ) {
            if(labelLookUp.ContainsKey(level)) {
                labelLookUp[level]?.Update(timer);
            }            
            image?.Update(timer);
            dicom?.Update( timer );
            testlabel?.Update(timer);
        }

        internal void Render() {
            dicom?.Render();
            image?.Render();
            if(labelLookUp.ContainsKey(level)) {
                labelLookUp[level]?.Render();
            }
            testlabel?.Render();
        }

        internal void ChangeType() {
            Type = (Type + 1) % 3;
            foreach(var renderer in labels) {
                renderer.Type = Type;
            }
        }

        internal void SetPosition( Vector3 dp ) {
            dicom.Position += dp;
            image.Position += dp;
            foreach(var label in labels) {
                label?.SetPosition(dp);
            }
            testlabel?.SetPosition(dp);
        }

        internal void SetRotator( Matrix4x4 rotator ) {
            dicom.GlobalRotator = rotator;
            image.GlobalRotator = rotator;
            foreach(var label in labels) {
                label?.SetRotator(rotator);
            }
            testlabel?.SetRotator(rotator);
        }

        internal async Task CreateDeviceDependentResourcesAsync() {
            if(labelLookUp.ContainsKey(level)) {
                System.Diagnostics.Debug.WriteLine("CreateDeviceDependentResourcesAsync");
                labelLookUp[level]?.CreateDeviceDependentResourcesAsync();
            }
            await image?.CreateDeviceDependentResourcesAsync();
            await dicom?.CreateDeviceDependentResourcesAsync();
            await testlabel?.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources() {
            if(labelLookUp.ContainsKey(level)) {
                System.Diagnostics.Debug.WriteLine("ReleaseDeviceDependentResources");
                labelLookUp[level]?.ReleaseDeviceDependentResources();
            }
            image?.ReleaseDeviceDependentResources();
            dicom?.ReleaseDeviceDependentResources();
            testlabel?.ReleaseDeviceDependentResources();
        }

        internal void Dispose() {
            if(labels != null) {
                foreach(var renderer in labels) {
                    renderer?.Dispose();
                }
                labels = null;
            }
            image?.Dispose();
            dicom?.Dispose();
            testlabel?.Dispose();
        }

        void IDisposable.Dispose() {
            foreach(var renderer in labels) {
                renderer?.Dispose();
            }
            image?.Dispose();
            dicom?.Dispose();
            testlabel?.Dispose();
        }

    }
}
