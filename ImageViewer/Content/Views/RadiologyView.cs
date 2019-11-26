using ImageViewer.Common;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Views {
    internal class RadiologyView : IDisposable {

        //private ImageRenderer image;
        private int Type = 0;
        private int Level = 265;
        private int maxLevel = 515;
        private int minLevel = 1;
        readonly TextureLoader loader;

        RadiologyRenderer dicom;

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
            UpdateImage( Level );
        }

        internal void NextImage() {
            Level++;
            if( Level > maxLevel ) {
                Level = maxLevel;
            }
            UpdateImage( Level );
        }

        internal void PrevImage() {
            Level--;
            if( Level < minLevel ) {
                Level = minLevel;
            }
            UpdateImage( Level );
        }

        protected void UpdateImage( int level) {
            var ims = Image( level );
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
            return @";dicom;" + index;
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
