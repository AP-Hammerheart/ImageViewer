using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class KeyRenderer : StatusBarRenderer
    {
        private readonly TileView view;

        internal KeyRenderer(
            TileView view,
            DeviceResources deviceResources, 
            TextureLoader loader)
            : base(deviceResources, loader)
        {
            this.view = view;
        }

        internal KeyRenderer(
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

        internal override void Update(StepTimer timer)
        {
            var chr = view.VirtualKey.ToString();
            if (!chr.Equals(Text))
            {
                updating = true;
                Text = chr;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
