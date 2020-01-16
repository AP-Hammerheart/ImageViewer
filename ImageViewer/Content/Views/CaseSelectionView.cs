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
using ImageViewer.Content.Renderers.Base;

namespace ImageViewer.Content.Views {
    class CaseSelectionView {
        
        //private ImageRenderer image;
        readonly TextureLoader loader;

        //RadiologyRenderer dicom;
        StatusBarRenderer[] view = new StatusBarRenderer[3];
        public bool showCaseSelection = true;

        internal CaseSelectionView( DeviceResources deviceResources, TextureLoader loader ) {
            this.loader = loader;
            gör så att view är centrerad och lite framför!
            //Top
            view[0] = new StatusBarRenderer(
                    deviceResources: deviceResources,
                    loader: loader,
                    bottomLeft: new Vector3( Constants.X00, Constants.Y3, Constants.Z3 ),
                    topLeft: new Vector3( Constants.X00, Constants.Y4, Constants.Z3 ),
                    bottomRight: new Vector3( Constants.X00, Constants.Y3, Constants.Z2 ),
                    topRight: new Vector3( Constants.X00, Constants.Y4, Constants.Z2 ) )
            {
                TextPosition = new Vector2( 20, 10 ),
                Text = "Select Case",
                FontSize = 40.0f,
                ImageWidth = 960,
                ImageHeight = 80,
            };

            //Content
            view[1] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3( Constants.X09, Constants.Y1, Constants.Z0 ),
                topLeft: new Vector3( Constants.X09, Constants.Y3, Constants.Z0 ),
                bottomRight: new Vector3( Constants.X10, Constants.Y1, Constants.Z1 ),
                topRight: new Vector3( Constants.X10, Constants.Y3, Constants.Z1 ) ) 
            {
                TextPosition = new Vector2( 20, 10 ),
                Text = "sadfasdfsadfasdf",
                FontSize = 34.0f,
                ImageWidth = 1280,
                ImageHeight = 1344,
                BackgroundColor = Windows.UI.Colors.LightGray,
            };

            //Bottom
            view[2] = new StatusBarRenderer(
                    deviceResources: deviceResources,
                    loader: loader,
                    bottomLeft: new Vector3( Constants.X00, Constants.Y3, Constants.Z3 ),
                    topLeft: new Vector3( Constants.X00, Constants.Y4, Constants.Z3 ),
                    bottomRight: new Vector3( Constants.X00, Constants.Y3, Constants.Z2 ),
                    topRight: new Vector3( Constants.X00, Constants.Y4, Constants.Z2 ) )
            {
                TextPosition = new Vector2( 20, 10 ),
                Text = "BOTTOM",
                FontSize = 40.0f,
                ImageWidth = 960,
                ImageHeight = 80,
            };

        }

        internal void Update( StepTimer timer ) {
            foreach( StatusBarRenderer sbr in view ) {
                sbr?.Update( timer );
            }
        }

        internal void Render() {
            if( showCaseSelection ) {
                foreach( StatusBarRenderer sbr in view ) {
                    sbr?.Render();
                }
            }
        }

        internal void SetPosition( Vector3 dp ) {
            foreach( StatusBarRenderer sbr in view ) {
                sbr.Position += dp;
            }
        }

        internal void SetRotator( Matrix4x4 rotator ) {
            foreach( StatusBarRenderer sbr in view ) {
                sbr.GlobalRotator = rotator;
            }
        }

        internal async Task CreateDeviceDependentResourcesAsync() {
            foreach( StatusBarRenderer sbr in view ) {
                await sbr?.CreateDeviceDependentResourcesAsync();
            }
        }

        internal void ReleaseDeviceDependentResources() {
            foreach( StatusBarRenderer sbr in view ) {
                sbr?.ReleaseDeviceDependentResources();
            }
        }

        internal void Dispose() {
            foreach( StatusBarRenderer sbr in view ) {
                sbr?.Dispose();
            }
        }

        //void IDisposable.Dispose() {
        //    foreach( StatusBarRenderer sbr in view ) {
        //        sbr?.Dispose();
        //    }
        //}

    }
}
