using ImageViewer.Common;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace ImageViewer.Content.Renderers
{
    internal class TextRenderer : StatusBarRenderer
    {
        internal TextRenderer(
            DeviceResources deviceResources, 
            TextureLoader loader)
            : base(deviceResources, loader) {}

        internal TextRenderer(
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

        internal string[] Lines { get; set; } = null;
        internal float LineHeight { get; set; } = 0.0f;

        internal int Index { get; set; } = -1;

        internal override void Update(StepTimer timer)
        {
            Updating = true;

            Task task = new Task(async () =>
            {
                await UpdateTextureAsync();
            });
            task.Start();
        }

        protected override void TextLines(
            CanvasDrawingSession drawingSession, 
            CanvasTextFormat format)
        {
            format.FontSize = FontSize;
            for (var i = 0; i < Lines.Length; i++)
            {
                if (i == Index)
                {
                    format.FontWeight = FontWeights.Bold;
                }
                else
                {
                    format.FontWeight = FontWeights.Normal;
                }
                
                var position = TextPosition + new Vector2(0.0f, i * LineHeight);
                drawingSession.DrawText(Lines[i], position, TextColor, format);
            }      
        }
    }
}
