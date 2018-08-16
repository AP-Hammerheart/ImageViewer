using ImageViewer.Common;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Input.Spatial;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content
{
    internal abstract class BaseView
    {
        protected readonly TextureLoader loader;
        private PyramidRenderer pointer = null;

        private bool cancel = false;
        private bool loading = false;

        protected static readonly float distanceFromUser = 1.4f; // meters
        protected static readonly int minScale = 8;

        protected static readonly int image2offsetX = -5500;
        protected static readonly int image2offsetY = -2000;
        protected static readonly int maxResolution = 110000;

        protected StatusBarRenderer[] statusItems;

        internal int Level { get; set; } = 7;
        internal int ImageY { get; set; } = 0;
        internal int ImageX { get; set; } = 0;
        internal Windows.System.VirtualKey VirtualKey { get; set; } = Windows.System.VirtualKey.None;

        protected int PixelSize(int level) => (int)System.Math.Pow(2, level);
        protected virtual int TileOffset(int level) => TileResolution * PixelSize(level);
        protected int Step => TileOffset(Level);
        internal PlaneRenderer[] Tiles { get; set; }
        protected int TileResolution { get; set; }
        internal string DebugString { get; set; }

        internal BaseView(
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
                bottomRight: new Vector3(0.1f, 0.25f, 0.0f),
                topRight: new Vector3(0.1f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                ImageWidth = 480
            };

            statusItems[3] = new DebugRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.1f, 0.25f, 0.0f),
                topLeft: new Vector3(0.1f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.3f, 0.25f, 0.0f),
                topRight: new Vector3(0.3f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * distanceFromUser),
                ImageWidth = 320
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

            pointer = new PyramidRenderer(this, deviceResources, loader, 
                new PyramidRenderer.Corners(
                    origo: new Vector3(0.0f, 0.0f, -1 * distanceFromUser), 
                    topLeft: new Vector3(-0.5f, 0.25f, -1 * distanceFromUser),
                    bottomLeft: new Vector3(-0.5f, -0.25f, -1 * distanceFromUser)))
            {
                Position = new Vector3(0, 0, -1 * distanceFromUser)
            };
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

            pointer.Transformer = transformer;
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

            pointer?.Update(timer);
        }

        internal void Update(SpatialPointerPose pose)
        {
            pointer?.Update(pose);
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

            pointer?.Render();
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

            pointer?.Dispose();
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

            pointer?.CreateDeviceDependentResourcesAsync();
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

            pointer?.ReleaseDeviceDependentResources();
        }

        internal void HandleVoiceCommand(Command command, Direction direction, int number)
        {
            switch (command)
            {
                case Command.MOVE: Move(direction, number); break;
                case Command.SCALE: Scale(direction, number); break;
                case Command.SET: SetPointer(direction, number); break;
                case Command.PRELOAD: if (!loading) PreLoadTiles(); break;
                case Command.CANCEL: if (loading) cancel = true; break;
                case Command.CLEAR_CACHE: ClearCache(); break;
            }
        }

        internal void OnKeyPressed(Windows.System.VirtualKey key)
        {
            VirtualKey = key;

            switch (key)
            {
                case Windows.System.VirtualKey.NumberPad1:
                    Move(Direction.LEFT, 1);
                    Move(Direction.DOWN, 1);
                    break;

                case Windows.System.VirtualKey.NumberPad3:
                    Move(Direction.RIGHT, 1);
                    Move(Direction.DOWN, 1);
                    break;

                case Windows.System.VirtualKey.NumberPad7:
                    Move(Direction.LEFT, 1);
                    Move(Direction.UP, 1);
                    break;

                case Windows.System.VirtualKey.NumberPad9:
                    Move(Direction.RIGHT, 1);
                    Move(Direction.UP, 1);
                    break;

                case Windows.System.VirtualKey.NumberPad4:
                case Windows.System.VirtualKey.Left:
                    Move(Direction.LEFT, 1);
                    break;

                case Windows.System.VirtualKey.NumberPad6:
                case Windows.System.VirtualKey.Right: Move(Direction.RIGHT, 1);
                    break;

                case Windows.System.VirtualKey.NumberPad8:
                case Windows.System.VirtualKey.Up:
                    Move(Direction.UP, 1);
                    break;

                case Windows.System.VirtualKey.NumberPad2:
                case Windows.System.VirtualKey.Down: Move(Direction.DOWN, 1); break;

                case Windows.System.VirtualKey.Q: Scale(Direction.UP, 1); break;
                case Windows.System.VirtualKey.A: Scale(Direction.DOWN, 1); break;
            }
        }

        protected abstract void Scale(Direction direction, int number);

        protected abstract void Move(Direction direction, int number);

        private void SetPointer(Direction direction, int number)
        {
            if (direction == Direction.FRONT)
            {
                switch (number)
                {
                    case 0: pointer.Visible = false; break;
                    case 1: pointer.Visible = true; break;
                    case 2: pointer.Locked = false; break;
                    case 3: pointer.Locked = true; break;
                }
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

        private void PreLoadTiles()
        {
            loading = true;

            for (var level = minScale; level >= 0; level--)
            {
                var step = TileOffset(level);

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
                            + "&x=" + x.ToString()
                            + "&y=" + y.ToString()
                            + "&w=" + TileResolution.ToString()
                            + "&h=" + TileResolution.ToString()
                            + "&level=" + level.ToString();

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
                            + "&x=" + (x + image2offsetX).ToString()
                            + "&y=" + (y + image2offsetY).ToString()
                            + "&w=" + TileResolution.ToString()
                            + "&h=" + TileResolution.ToString()
                            + "&level=" + level.ToString();

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
    }
}
