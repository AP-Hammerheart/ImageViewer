// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Dev {
    internal class RadiologyIndexRenderer : StatusBarRenderer {

        private readonly RadiologyView dicom;
        internal RadiologyIndexRenderer(
            RadiologyView rv,
            DeviceResources deviceResources,
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight )
            : base( deviceResources,
                  loader,
                  bottomLeft,
                  topLeft,
                  bottomRight,
                  topRight ) => this.dicom = rv;

        internal override void Update( StepTimer timer ) {
            var index = dicom.level;

            if( !index.Equals( Text ) ) {
                Updating = true;
                Text = "        Image: " + index;

                Task task = new Task( async () => {
                    await UpdateTextureAsync();
                } );
                task.Start();
            }
        }
    }
}
