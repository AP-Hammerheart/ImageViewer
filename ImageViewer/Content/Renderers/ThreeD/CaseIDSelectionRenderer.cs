using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Utils;
using ImageViewer.Content.Views;
using System.Numerics;

namespace ImageViewer.Content.Renderers.ThreeD {
    class CaseIDSelectionRenderer : StatusBarRenderer {

        internal CaseIDSelectionRenderer(
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
                  topRight ) {
        }

        internal override void Update( StepTimer timer ) {
            Task task = new Task( async () => {
                await UpdateTextureAsync();
            } );
            task.Start();
        }
    }
}
