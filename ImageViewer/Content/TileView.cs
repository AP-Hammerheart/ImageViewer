using ImageViewer.Common;
using System;
using System.Numerics;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content
{
    internal class TileView
    {
        private readonly TextureLoader loader;

        private static readonly int maxX = 5;
        private static readonly int maxY = 5;
        
        private static readonly int tileResolution = 256;
        private static readonly int minScale = 8;
        private static readonly float distanceFromUser = 1.4f; // meters

        private static readonly int image2offsetX = -5500;
        private static readonly int image2offsetY = -2000;
        private static readonly int maxResolution = 110000;

        private bool cancel = false;
        private bool loading = false;

        private StatusBarRenderer[] statusItems;

        private int _Step(int _level)
        {
            return tileResolution * (int)Math.Pow(2, _level);
        }

        private int Step => _Step(Level);
        internal int Level { get; set; } = 7;
        internal int ImageY { get; set; } = 0;
        internal int ImageX { get; set; } = 0;
        internal TileRenderer[] Tiles { get; set; }
        internal Windows.System.VirtualKey VirtualKey { get; set; } = Windows.System.VirtualKey.None;

        internal TileView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader)
        {
            this.loader = loader;

            statusItems = new StatusBarRenderer[6];

            statusItems[0] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.5f, 0.25f, 0.0f),
                topLeft: new Vector3(-0.5f, 0.30f, 0.0f),
                bottomRight: new Vector3(-0.3f, 0.25f, 0.0f),
                topRight: new Vector3(-0.3f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                TextPosition = new Vector2(20, 10),
                Text = "ImageViewer",
                FontSize = 40,
                ImageWidth = 320,
            };

            statusItems[1] = new KeyRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.3f, 0.25f, 0.0f),
                topLeft: new Vector3(-0.3f, 0.30f, 0.0f),
                bottomRight: new Vector3(-0.2f, 0.25f, 0.0f),
                topRight: new Vector3(-0.2f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                ImageWidth = 160
            };

            statusItems[2] = new ZoomRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.2f, 0.25f, 0.0f),
                topLeft: new Vector3(-0.2f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.2f, 0.25f, 0.0f),
                topRight: new Vector3(0.2f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                ImageWidth = 640
            };

            statusItems[3] = new TileCounterRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.2f, 0.25f, 0.0f),
                topLeft: new Vector3(0.2f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.3f, 0.25f, 0.0f),
                topRight: new Vector3(0.3f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                ImageWidth = 160
            };

            statusItems[4] = new MemoryUseRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.3f, 0.25f, 0.0f),
                topLeft: new Vector3(0.3f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.4f, 0.25f, 0.0f),
                topRight: new Vector3(0.4f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                ImageWidth = 160
            };

            statusItems[5] = new ClockRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.4f, 0.25f, 0.0f),
                topLeft: new Vector3(0.4f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.5f, 0.25f, 0.0f),
                topRight: new Vector3(0.5f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                ImageWidth = 160
            };

            Tiles = new TileRenderer[2 * maxX * maxY];

            var step = Step;
            var tileSize = 0.1f;

            for (var x = 0; x < maxX; x++)
            {
                for (var y = 0; y < maxY; y++)
                {
                    Tiles[maxY * x + y] = new TileRenderer(deviceResources, loader, ImageViewerMain.Image1
                        + "&x=" + (x * step + ImageX).ToString()
                        + "&y=" + (y * step + ImageY).ToString()
                        + "&w=" + tileResolution.ToString()
                        + "&h=" + tileResolution.ToString()
                        + "&level=" + Level.ToString(),
                        tileSize)
                    {
                        Position = new Vector3(
                            -1.0f * maxX * tileSize + (0.5f * tileSize) + x * tileSize,
                            0.5f * maxY * tileSize - (0.5f * tileSize) - y * tileSize,
                            -1 * distanceFromUser)
                    };

                    Tiles[(maxX * maxY) + (maxY * x + y)] = new TileRenderer(deviceResources, loader, ImageViewerMain.Image2
                        + "&x=" + (x * step + ImageX + image2offsetX).ToString()
                        + "&y=" + (y * step + ImageY + image2offsetY).ToString()
                        + "&w=" + tileResolution.ToString()
                        + "&h=" + tileResolution.ToString()
                        + "&level=" + Level.ToString(),
                        tileSize)
                    {
                        Position = new Vector3(
                            (0.5f * tileSize) + x * tileSize,
                            0.5f * maxY * tileSize - (0.5f * tileSize) - y * tileSize,
                            -1 * distanceFromUser)
                    };
                }
            }
        }

        internal void SetTransformer(Matrix4x4 transformer)
        {
            foreach (var renderer in Tiles)
            {
                renderer.Transformer = transformer;
            }

            foreach (var renderer in statusItems)
            {
                renderer.Transformer = transformer;
            }
        }

        internal void Update(StepTimer timer)
        {
            foreach (var renderer in Tiles)
            {
                renderer?.Update(timer);
            }
            foreach (var renderer in statusItems)
            {
                renderer?.Update(timer);
            }
        }

        internal void Render()
        {
            foreach (var renderer in statusItems)
            {
                renderer?.Render();
            }

            foreach (var renderer in Tiles)
            {
                renderer?.Render();
            }
        }

        internal void Dispose()
        {
            if (statusItems != null)
            {
                foreach (var renderer in statusItems)
                {
                    renderer?.Dispose();
                }
                statusItems = null;
            }

            if (Tiles != null)
            {
                foreach (var renderer in Tiles)
                {
                    renderer?.Dispose();
                }
                Tiles = null;
            }
        }

        internal void HandleVoiceCommand(Command command, Direction direction, int number)
        {
            switch (command)
            {
                case Command.MOVE: Move(direction, number); break;
                case Command.SCALE: Scale(direction, number); break;
                case Command.PRELOAD: if (!loading) PreLoadTiles(); break;
                case Command.CANCEL: if (loading) cancel = true; break;
                case Command.CLEAR_CACHE: ClearCache(); break;
            }
        }

        internal void CreateDeviceDependentResourcesAsync()
        {
            foreach (var renderer in Tiles)
            {
                renderer?.CreateDeviceDependentResourcesAsync();
            }

            foreach (var renderer in statusItems)
            {
                renderer?.CreateDeviceDependentResourcesAsync();
            }
        }

        internal void ReleaseDeviceDependentResources()
        {
            foreach (var renderer in Tiles)
            {
                renderer?.ReleaseDeviceDependentResources();
            }

            foreach (var renderer in statusItems)
            {
                renderer?.ReleaseDeviceDependentResources();
            }
        }

        private void ClearCache()
        {
            if (loading)
            {
                cancel = false;
                while (loading) ;
            }

            Task task = new Task(async () =>
            {
                await loader.ClearCache();
            });
            task.Start();
        }

        private void Scale(Direction direction, int number)
        {
            switch (direction)
            {
                case Direction.UP:
                    Level -= number;
                    if (Level < 0) Level = 0;
                    break;
                case Direction.DOWN:
                    Level += number;
                    if (Level > minScale) Level = minScale;
                    break;
            }

            var step = Step;

            ImageX = ImageX - (ImageX % step);
            ImageY = ImageY - (ImageY % step);

            UpdateImages(step);
        }

        private void Move(Direction direction, int number)
        {
            var step = Step;

            switch (direction)
            {
                case Direction.LEFT:
                    if (ImageX < maxResolution - ((maxX - 1) * step))
                    {
                        ImageX += number * step;
                    }
                    break;
                case Direction.RIGHT:
                    ImageX -= number * step;
                    if (ImageX < 0)
                    {
                        ImageX = 0;
                    }
                    break;
                case Direction.DOWN:
                    ImageY -= number * step;
                    if (ImageY < 0)
                    {
                        ImageY = 0;
                    }
                    break;
                case Direction.UP:
                    if (ImageY < maxResolution - ((maxY - 1) * step))
                    {
                        ImageY += number * step;
                    }
                    break;
            }

            UpdateImages(step);
        }

        private void PreLoadTiles()
        {
            loading = true;

            for (var _level = 8; _level >= 0; _level--)
            {
                var step = _Step(_level);

                for (var x = 0; x < maxResolution; x += step)
                {
                    for (var y = 0; y < maxResolution; y += step)
                    {
                        if (cancel)
                        {
                            loading = false;
                            cancel = false;
                            return;
                        }

                        var id1 = Image1
                            + "&x=" + (x + image2offsetX).ToString()
                            + "&y=" + (y + image2offsetY).ToString()
                            + "&w=" + tileResolution.ToString()
                            + "&h=" + tileResolution.ToString()
                            + "&level=" + _level.ToString();

                        Task task1 = new Task(async () =>
                        {
                            var b1 = await loader.PreloadImage(id1);
                        });
                        task1.Start();
                        task1.Wait();

                        if (cancel)
                        {
                            loading = false;
                            cancel = false;
                            return;
                        }

                        var id2 = Image2
                            + "&x=" + x.ToString()
                            + "&y=" + y.ToString()
                            + "&w=" + tileResolution.ToString()
                            + "&h=" + tileResolution.ToString()
                            + "&level=" + _level.ToString();

                        Task task2 = new Task(async () =>
                        {
                            var b2 = await loader.PreloadImage(id2);
                        });
                        task2.Start();
                        task2.Wait();
                    }
                }
            }

            loading = false;
        }

        private void UpdateImages(int step)
        {
            for (var x = 0; x < maxX; x++)
            {
                for (var y = 0; y < maxY; y++)
                {
                    var url1 = ImageViewerMain.Image1
                        + "&x=" + (x * step + ImageX).ToString()
                        + "&y=" + (y * step + ImageY).ToString()
                        + "&w=" + tileResolution.ToString()
                        + "&h=" + tileResolution.ToString()
                        + "&level=" + Level.ToString();

                    Tiles[maxY * x + y].TextureID = url1;

                    Task task1 = new Task(async () =>
                    {
                        await loader.LoadTextureAsync(url1);
                    });
                    task1.Start();
                    task1.Wait();

                    var url2 = ImageViewerMain.Image2
                        + "&x=" + (x * step + ImageX + image2offsetX).ToString()
                        + "&y=" + (y * step + ImageY + image2offsetY).ToString()
                        + "&w=" + tileResolution.ToString()
                        + "&h=" + tileResolution.ToString()
                        + "&level=" + Level.ToString();

                    Tiles[(maxX * maxY) + (maxY * x + y)].TextureID = url2;

                    Task task2 = new Task(async () =>
                    {
                        await loader.LoadTextureAsync(url2);
                    });
                    task2.Start();
                    task2.Wait();
                }
            }
        }

        internal void OnKeyPressed(Windows.System.VirtualKey key)
        {
            VirtualKey = key;
            switch (key)
            {
                case Windows.System.VirtualKey.Left: Move(Direction.LEFT, 1); break;
                case Windows.System.VirtualKey.Right: Move(Direction.RIGHT, 1); break;
                case Windows.System.VirtualKey.Up: Move(Direction.UP, 1); break;
                case Windows.System.VirtualKey.Down: Move(Direction.DOWN, 1); break;
                case Windows.System.VirtualKey.Q: Scale(Direction.UP, 1); break;
                case Windows.System.VirtualKey.A: Scale(Direction.DOWN, 1); break;
            }
        }
    }
}
