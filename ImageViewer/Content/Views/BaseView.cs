// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Input.Spatial;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content
{
    internal abstract class BaseView : IDisposable
    {
        private const string Text = 
            @"The available speech commands are:
                1. 
                    move direction distance 
                    for example 
                        move left 100
                        move up 300
                    distance is given by pixels
                    the image width and height are 1280 pixels
                    if no distance is given 1 is used
                2. 
                    scale direction amount
                    for example 
                         scale up 2
                         scale down 3
                    if no amount is given 1 is used
                3.
                    set pointer 1
                    show visual pointer
                    set pointer 0
                    hide visual pointer
                4.
                    add tag
                    add marker to current pointer location
                5. 
                    remove tag
                    remove last marker
                6.
                    remove temporary files
                    delete cache files from hololens disk
                7.
                    reset position
                    place the hologram to the original position";

        private readonly ImageViewerMain main;
        protected readonly TextureLoader loader;

        private bool cancel = false;
        private bool loading = false;

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
        internal int Step => TileOffset(Level);
        internal PlaneRenderer[] Tiles { get; set; }
        protected int TileResolution { get; set; }
        internal string DebugString { get; set; } = "";
        internal PointerRenderer Pointer { get; set; }
        internal static float ViewSize { get; } = 0.5f;
        protected static float DistanceFromUser { get; } = 1.4f;
        protected static int MinScale { get; } = 8;

        internal BaseView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader)
        {
            this.main = main;
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
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
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
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 160
            };

            statusItems[2] = new ZoomRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.2f, 0.25f, 0.0f),
                topLeft: new Vector3(-0.2f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.15f, 0.25f, 0.0f),
                topRight: new Vector3(0.15f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 560
            };

            statusItems[3] = new DebugRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.15f, 0.25f, 0.0f),
                topLeft: new Vector3(0.15f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.3f, 0.25f, 0.0f),
                topRight: new Vector3(0.3f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 240
            };

            statusItems[4] = new MemoryUseRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.3f, 0.25f, 0.0f),
                topLeft: new Vector3(0.3f, 0.30f, 0.0f),
                bottomRight: new Vector3(0.4f, 0.25f, 0.0f),
                topRight: new Vector3(0.4f, 0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
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
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 160
            };

            Pointer = new PointerRenderer(this, deviceResources, loader, 
                new PointerRenderer.Corners(
                    origo: new Vector3(0.0f, 0.0f, -1 * DistanceFromUser), 
                    topLeft: new Vector3(-0.5f, 0.25f, -1 * DistanceFromUser),
                    bottomLeft: new Vector3(-0.5f, -0.25f, -1 * DistanceFromUser)))
            {
                Position = new Vector3(0, 0, -1 * DistanceFromUser)
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

            Pointer.Transformer = transformer;
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

            Pointer?.Update(timer);
        }

        internal void Update(SpatialPointerPose pose)
        {
            Pointer?.Update(pose);
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

            Pointer?.Render();
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

            Pointer?.Dispose();
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            foreach (var renderer in Tiles)
            {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            foreach (var renderer in statusItems)
            {
                await renderer?.CreateDeviceDependentResourcesAsync();
            }

            await Pointer?.CreateDeviceDependentResourcesAsync();
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

            Pointer?.ReleaseDeviceDependentResources();
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
                case Command.ADD_TAG: Pointer.AddTag(); break;
                case Command.REMOVE_TAG: Pointer.RemoveTag(); break;
                case Command.RESET_POSITION: Reset(); break;
                case Command.HELP: Help(); break;
                case Command.ZOOM: Zoom(direction, number); break;
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

                case Windows.System.VirtualKey.T: Pointer.AddTag(); break;
                case Windows.System.VirtualKey.R: Pointer.RemoveTag(); break;

                case Windows.System.VirtualKey.Space: Reset(); break;
            }
        }

        protected abstract void Scale(Direction direction, int number);

        protected abstract void Move(Direction direction, int number);

        protected abstract void Zoom(Direction direction, int number);

        private void SetPointer(Direction direction, int number)
        {
            if (direction == Direction.FRONT)
            {
                switch (number)
                {
                    case 0: Pointer.Visible = false; break;
                    case 1: Pointer.Visible = true; break;
                    case 2: Pointer.Locked = false; break;
                    case 3: Pointer.Locked = true; break;
                    case 4: Pointer.AddTag(); break;
                    case 5: Pointer.RemoveTag(); break;
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

            for (var level = MinScale; level >= 0; level--)
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

        private void Reset()
        {
            foreach (var renderer in Tiles)
            {
                renderer.Transformer = Matrix4x4.Identity;
            }

            foreach (var renderer in statusItems)
            {
                renderer.Transformer = Matrix4x4.Identity;
            }

            Pointer.Transformer = Matrix4x4.Identity;
        }

        private void Help()
        {
            main.Speak(Text);    
        }

        void IDisposable.Dispose()
        {
            foreach (var renderer in Tiles)
            {
                renderer?.Dispose();
            }

            foreach (var renderer in statusItems)
            {
                renderer?.Dispose();
            }

            Pointer?.Dispose();
        }
    }
}
