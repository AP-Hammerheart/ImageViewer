using System.Numerics;
using System.Threading.Tasks;
using ImageViewer.Common;

namespace ImageViewer.Content
{
    internal class ZoomRenderer : StatusBarRenderer
    {
        private readonly TileView view;

        internal ZoomRenderer(
            TileView view, 
            DeviceResources deviceResources, 
            TextureLoader loader)
            : base(deviceResources, loader)
        {
            this.view = view;
        }

        internal ZoomRenderer(
            TileView view, 
            DeviceResources deviceResources, 
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight)
            : base(deviceResources, loader, bottomLeft, topLeft, bottomRight, topRight)
        {
            this.view = view;
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

        internal override void Update(StepTimer timer)
        {
            var zoom = "X: " + view.ImageX.ToString() + "  Y: " + view.ImageY.ToString() + "  Zoom: " + Zoom(view.Level);
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
