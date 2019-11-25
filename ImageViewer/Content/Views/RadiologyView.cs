using ImageViewer.Common;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Views {
    internal class RadiologyView : IDisposable {

        private Label[] labels;
        private ImageRenderer image;
        private int Type = 0;
        private int Level = 5;
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
            UpdateImage( 5 );
            var labelTexts = new string[]
            {

            };

            

            var coords = new int[][]
            {

            };

            labels = new Label[labelTexts.Length];

            for( var i = 0; i < labelTexts.Length; i++ ) {
            }
        }

        internal void NextImage() {
            Level++;
            UpdateImage( Level );
        }

        internal void PrevImage() {
            Level--;
            UpdateImage( Level );
        }

        protected void UpdateImage( int level) {
            var ims = Image( Level );
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

        private string Image( string file, int idx ) {
            var y = idx / 7;
            var x = idx % 7;

            var step = PixelSize( Level ) * Constants.TileResolution;

            return file
                        + "&x=" + 10000.ToString()
                        + "&y=" + 10000.ToString()
                        + "&w=" + 500.ToString()
                        + "&h=" + 500.ToString()
                        + "&level=" + Level.ToString();
        }

        private string Image( int index ) {
            return @"https://localhost:44399/imageapi/T2747-19/dicom/" + index + ".png";
        }

        internal void Update( StepTimer timer ) {
            foreach( var renderer in labels ) {
                renderer?.Update( timer );
            }

            dicom?.Update( timer );
        }

        internal void Render() {
            dicom?.Render();

            foreach( var renderer in labels ) {
                renderer?.Render();
            }
        }

        internal void ChangeType() {
            Type = (Type + 1) % 3;
            foreach( var renderer in labels ) {
                renderer.Type = Type;
            }
        }

        internal void SetPosition( Vector3 dp ) {
            dicom.Position += dp;

            foreach( var label in labels ) {
                label?.SetPosition( dp );
            }
        }

        internal void SetRotator( Matrix4x4 rotator ) {
            dicom.GlobalRotator = rotator;

            foreach( var label in labels ) {
                label?.SetRotator( rotator );
            }
        }

        internal async Task CreateDeviceDependentResourcesAsync() {
            foreach( var renderer in labels ) {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            await dicom?.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources() {
            foreach( var renderer in labels ) {
                renderer?.ReleaseDeviceDependentResources();
            }

            dicom?.ReleaseDeviceDependentResources();
        }

        internal void Dispose() {
            if( labels != null ) {
                foreach( var renderer in labels ) {
                    renderer?.Dispose();
                }
                labels = null;
            }

            dicom?.Dispose();
        }

        void IDisposable.Dispose() {
            foreach( var renderer in labels ) {
                renderer?.Dispose();
            }

            dicom?.Dispose();
        }

    }
}
