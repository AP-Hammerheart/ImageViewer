using System.Numerics;
using System.Threading.Tasks;
using ImageViewer.Common;

namespace ImageViewer.Content
{
    internal class ZoomRenderer : StatusBarRenderer
    {
        private readonly ImageViewerMain main;

        internal ZoomRenderer(
            ImageViewerMain main, 
            DeviceResources deviceResources, 
            TextureLoader loader)
            : base(deviceResources, loader)
        {
            this.main = main;
        }

        internal ZoomRenderer(
            ImageViewerMain main, 
            DeviceResources deviceResources, 
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight)
            : base(deviceResources, loader, bottomLeft, topLeft, bottomRight, topRight)
        {
            this.main = main;
        }

        private string Zoom(int level)
        {
            switch (level)
            {
                case 1: return "40x";
                case 2: return "20x";
                case 3: return "10x";
                case 4: return "5x";
                case 5: return "2.5x";
                case 6: return "1.25x";
                case 7: return "0.625x";
                case 8: return "0.3125x";
                default: return "80x";
            }
        }

        public override void Update(StepTimer timer)
        {
            var zoom = "X: " + main.ImageX.ToString() + "  Y: " + main.ImageY.ToString() + "  Zoom: " + Zoom(main.Level);
            if (!zoom.Equals(Text))
            {
                updating = true;
                Text = zoom;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
