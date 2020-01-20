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
        StatusBarRenderer[] view = new StatusBarRenderer[4];
        public bool showCaseSelection = true;
        List<CasesJson> cj = new List<CasesJson>();
        int selectedID = 0;

        internal CaseSelectionView( DeviceResources deviceResources, TextureLoader loader ) {
            this.loader = loader;
            //Top
            view[0] = new StatusBarRenderer(
                    deviceResources: deviceResources,
                    loader: loader,
                    bottomLeft: new Vector3( Constants.X00, Constants.Y3, Constants.Z1 ),
                    topLeft: new Vector3( Constants.X00, Constants.Y4, Constants.Z1 ),
                    bottomRight: new Vector3( Constants.X01, Constants.Y3, Constants.Z0 ),
                    topRight: new Vector3( Constants.X01, Constants.Y4, Constants.Z0 ) )
            {
                TextPosition = new Vector2( 20, 10 ),
                Text = "Select Case",
                FontSize = 40.0f,
                ImageWidth = 960,
                ImageHeight = 80,
            };

            //Header
            view[1] = new StatusBarRenderer(
                    deviceResources: deviceResources,
                    loader: loader,
                    bottomLeft: new Vector3( Constants.X00, Constants.Y2, Constants.Z2 ),
                    topLeft: new Vector3( Constants.X00, Constants.Y3, Constants.Z2 ),
                    bottomRight: new Vector3( Constants.X00, Constants.Y2, Constants.Z1 ),
                    topRight: new Vector3( Constants.X00, Constants.Y3, Constants.Z1 ) ) {
                TextPosition = new Vector2( 20, 10 ),
                FontSize = 40.0f,
                ImageWidth = 960,
                ImageHeight = 80,
                BackgroundColor = Windows.UI.Colors.LightGray,
            };

            //Content
            view[2] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3( Constants.X00 + 0.005f, Constants.Y1, Constants.Z2 ),
                topLeft: new Vector3( Constants.X00 + 0.005f, Constants.Y2, Constants.Z2 ),
                bottomRight: new Vector3( Constants.X00 + 0.005f, Constants.Y1, Constants.Z1 ),
                topRight: new Vector3( Constants.X00 + 0.005f, Constants.Y2, Constants.Z1 ) ) 
            {
                TextPosition = new Vector2( 20, 10 ),
                Text = "",
                FontSize = 34.0f,
                ImageWidth = 1280,
                ImageHeight = 1344,
                BackgroundColor = Windows.UI.Colors.White,
            };

            //Bottom
            view[3] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3( Constants.X00, Constants.Y0, Constants.Z2 ),
                topLeft: new Vector3( Constants.X00, Constants.Y1, Constants.Z2 ),
                bottomRight: new Vector3( Constants.X00, Constants.Y0, Constants.Z1 ),
                topRight: new Vector3( Constants.X00, Constants.Y1, Constants.Z1 ) ) 
            {
                TextPosition = new Vector2( 20, 10 ),
                Text = "",
                FontSize = 34.0f,
                ImageWidth = 1280,
                ImageHeight = 1344,
                BackgroundColor = Windows.UI.Colors.LightGray,
            };
            SetSelectionText();
        }

        internal void SetSelectionText () {
            string url = Settings.jsonURL;
            using( var client = new System.Net.Http.HttpClient() ) {
                var j = client.GetStringAsync( url );
                cj = JsonConvert.DeserializeObject<List<CasesJson>>( j.Result );
            }
            view[2].Text = string.Empty;
            for( int i = 0; i < cj.Count; i++ ) {
                view[2].Text += cj[i].caseId + "\n";
            }
        }

        internal void ConfirmSelectedID() {
            Settings.CaseID = cj[selectedID].caseId;
        }

        internal void ChangeSelectedIDUp() {
            selectedID--;
            if( selectedID < 0 ) {
                selectedID = 0;
            }
            view[3].Text = cj[selectedID].caseId;
        }

        internal void ChangeSelectedIDDown() {
            selectedID++;
            if( selectedID >= cj.Count ) {
                selectedID = cj.Count-1;
            }
            view[3].Text = cj[selectedID].caseId;
        }

        internal void ShowMenu() {
            view[3].Text = Settings.CaseID;
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
