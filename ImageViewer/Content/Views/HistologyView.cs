// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Image;
using ImageViewer.Content.Utils;
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


            UpdateImage();
        }

        protected void UpdateImage() {
            var ims = Image();
            var textures = new List<string>();
            textures.Add( ims );
            histo.TextureID = ims;
            var task = new Task( async () => {
                await loader.LoadTexturesAsync( textures );
            } );
            task.Start();
            task.Wait();
        }

        private string Image() {
            return Settings.Image1 + "&x=0&y=0&w=950&h=850&level=7";
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
