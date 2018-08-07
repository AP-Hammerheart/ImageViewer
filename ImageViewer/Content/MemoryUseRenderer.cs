using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal class MemoryUseRenderer : StatusBarRenderer
    {
        internal MemoryUseRenderer(DeviceResources deviceResources, TextureLoader loader)
            : base(deviceResources, loader)
        {
        }

        internal MemoryUseRenderer(
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
            var mem = loader.MemoryUseInMB().ToString() + " MB";
            if (!mem.Equals(Text))
            {
                updating = true;
                Text = mem;

                Task task = new Task(async () =>
                {
                    await UpdateTextureAsync();
                });
                task.Start();
            }
        }
    }
}
