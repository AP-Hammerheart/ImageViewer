using System.Numerics;
using System.Threading.Tasks;
using ImageViewer.Common;

namespace ImageViewer.Content
{
    internal class ClockRenderer : StatusBarRenderer
    {
        internal ClockRenderer(DeviceResources deviceResources, TextureLoader loader)
            : base(deviceResources, loader)
        {
        }

        internal ClockRenderer(
            DeviceResources deviceResources, 
            TextureLoader loader, 
            Vector3 bottomLeft, 
            Vector3 topLeft, 
            Vector3 bottomRight, 
            Vector3 topRight)
            : base(deviceResources, loader, bottomLeft, topLeft, bottomRight, topRight)
        {
        }

        internal override void Update(StepTimer timer)
        {
            var time = System.DateTime.Now.ToString("h:mm:s");
            if (!time.Equals(Text))
            {
                updating = true;
                Text = time;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
