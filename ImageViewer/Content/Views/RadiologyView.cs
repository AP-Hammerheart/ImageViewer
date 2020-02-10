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

namespace ImageViewer.Content.Views {
    internal class RadiologyView : IDisposable {

        //private ImageRenderer image;
        private int Type = 0;
        private int level = 1;
        private int maxLevel = 515;
        private int minLevel = 1;
        readonly TextureLoader loader;

        RadiologyRenderer dicom;

        string ZoomString = "&w=100&h=100&x=200&y=150";//read from connections on a connection, by connection basis.
        bool isZoom = false;

        public int Level {
            get => level;
        }

        internal RadiologyView( DeviceResources deviceResources, TextureLoader loader ) {
            this.loader = loader;
            //image = new ImageRenderer( deviceResources: deviceResources,
            //                           loader: loader,
            //                           bottomLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z3 ),
            //                           topLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z3 ),
            //                           bottomRight: new Vector3( Constants.X00, Constants.Y1, Constants.Z2 ),
            //                           topRight: new Vector3( Constants.X00, Constants.Y2, Constants.Z2 ),
            //                           width: 3456,
            //                           height: 2304 ) {
            //    Position = new Vector3( 0.0f, 0.0f, Constants.DistanceFromUser ),
            //    TextureFile = "Content\\Textures\\test.png"
            //};

            dicom = new RadiologyRenderer( deviceResources: deviceResources,
                                       loader: loader,
                                       bottomLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z3 ),
                                       topLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z3 ),
                                       bottomRight: new Vector3( Constants.X00, Constants.Y1, Constants.Z2 ),
                                       topRight: new Vector3( Constants.X00, Constants.Y2, Constants.Z2 ),
                                       width: 3456,
                                       height: 2304 ) {
                Position = new Vector3( 0.0f, 0.0f, Constants.DistanceFromUser ),
            };
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

        internal void Zoom()
        {
            isZoom = !isZoom;
            UpdateImage(level);
        }

        protected void UpdateImage( int level) {
            var ims = Image(level) + (isZoom ? ZoomString : "");
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

        internal void Update( StepTimer timer ) {


            dicom?.Update( timer );
        }

        internal void Render() {
            dicom?.Render();


        }

        internal void ChangeType() {
            Type = (Type + 1) % 3;

        }

        internal void SetPosition( Vector3 dp ) {
            dicom.Position += dp;

        }

        internal void SetRotator( Matrix4x4 rotator ) {
            dicom.GlobalRotator = rotator;

        }

        internal async Task CreateDeviceDependentResourcesAsync() {


            await dicom?.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources() {


            dicom?.ReleaseDeviceDependentResources();
        }

        internal void Dispose() {

            dicom?.Dispose();
        }

        void IDisposable.Dispose() {

            dicom?.Dispose();
        }

    }
}
