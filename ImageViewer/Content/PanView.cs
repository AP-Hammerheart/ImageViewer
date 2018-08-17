using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content
{
    internal class PanView : BaseView
    {
        internal static int ViewResolution { get; } = 1280;

        protected override int TileOffset(int level) => PixelSize(level) * (TileResolution - ViewResolution);

        internal PanView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader) : base(deviceResources, loader)
        {
            TileResolution = 2560;

            Tiles = new PanRenderer[2];

            var tileSize = 0.5f;

            Tiles[0] = new PanRenderer(deviceResources, loader, "", tileSize, backBufferResolution: TileResolution)
            {
                Position = new Vector3(-0.5f * tileSize, 0, -1 * DistanceFromUser)
            };

            Tiles[1] = new PanRenderer(deviceResources, loader, "", tileSize, backBufferResolution: TileResolution)
            {
                Position = new Vector3(0.5f * tileSize, 0, -1 * DistanceFromUser)
            };

            UpdateImages();
        }

        protected override void Scale(Direction direction, int number)
        {
            switch (direction)
            {
                case Direction.UP:
                    if (Level > 0)
                    {
                        Level -= number;
                    }
                    else return;
                    break;
                case Direction.DOWN:
                    if (Level < MinScale)
                    {
                        Level += number;
                    }
                    else return;
                    break;
            }

            UpdateImages();
        }

        protected override void Move(Direction direction, int number)
        {
            var distance = number * PixelSize(Level);

            switch (direction)
            {
                case Direction.LEFT:
                    if (ImageX < maxResolution)
                    {
                        ImageX += distance;
                    }
                    else return;
                    break;
                case Direction.RIGHT:
                    ImageX -= distance;
                    if (ImageX < 0)
                    {
                        ImageX = 0;
                    }
                    break;
                case Direction.DOWN:
                    ImageY -= distance;
                    if (ImageY < 0)
                    {
                        ImageY = 0;
                    }
                    break;
                case Direction.UP:
                    if (ImageY < maxResolution)
                    {
                        ImageY += distance;
                    }
                    else return;
                    break;
            }

            UpdateImages();
        }

        private void UpdateImages()
        {
            Pointer.Update();

            var step = PixelSize(Level) * (TileResolution - ViewResolution);
            var x = (ImageX / step) * step;
            var y = (ImageY / step) * step;

            var xrem = (ImageX % step) / PixelSize(Level);
            var yrem = (ImageY % step) / PixelSize(Level);

            ((PanRenderer)Tiles[0]).UpdateGeometry(xrem, yrem);
            ((PanRenderer)Tiles[1]).UpdateGeometry(xrem, yrem);

            var url1 = ImageViewerMain.Image1
                + "&x=" + x.ToString()
                + "&y=" + y.ToString()
                + "&w=" + TileResolution.ToString()
                + "&h=" + TileResolution.ToString()
                + "&level=" + Level.ToString();

            if (Tiles[0].TextureID != url1)
            {
                Tiles[0].TextureID = url1;

                Task task1 = new Task(async () =>
                {
                    await ((PanRenderer)Tiles[0]).UpdateTextureAsync();
                });
                task1.Start();
                task1.Wait();
            }
      
            var url2 = ImageViewerMain.Image2
                + "&x=" + (x + image2offsetX).ToString()
                + "&y=" + (y + image2offsetY).ToString()
                + "&w=" + TileResolution.ToString()
                + "&h=" + TileResolution.ToString()
                + "&level=" + Level.ToString();

            if (Tiles[1].TextureID != url2)
            {
                Tiles[1].TextureID = url2;

                Task task2 = new Task(async () =>
                {
                    await ((PanRenderer)Tiles[1]).UpdateTextureAsync();
                });
                task2.Start();
                task2.Wait();
            }               
        }
    }
}