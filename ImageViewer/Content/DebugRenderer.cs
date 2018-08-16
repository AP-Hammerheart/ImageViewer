using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class DebugRenderer : StatusBarRenderer
    {
        private readonly BaseView view;

        internal DebugRenderer(
            BaseView view,
            DeviceResources deviceResources,
            TextureLoader loader)
            : base(deviceResources, loader)
        {
            this.view = view;
        }

        internal DebugRenderer(
            BaseView view,
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
            var txt = view.DebugString;
            if (!txt.Equals(Text))
            {
                updating = true;
                Text = txt;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
