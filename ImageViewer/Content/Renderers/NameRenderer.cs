using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers
{
    internal class NameRenderer : StatusBarRenderer
    {
        internal NameRenderer(
            DeviceResources deviceResources,
            TextureLoader loader)
            : base(deviceResources, loader) {}

        internal NameRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight)
            : base(deviceResources, 
                  loader, 
                  bottomLeft, 
                  topLeft, 
                  bottomRight, 
                  topRight) {}

        internal int Index { get; set; } = 0;

        internal override void Update(StepTimer timer)
        {
            var txt = Index == 0 ? Settings.Image1 : Settings.Image2;
            if (!txt.Equals(Text))
            {
                Updating = true;
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
