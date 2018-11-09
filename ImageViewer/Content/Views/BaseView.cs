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
        protected abstract int LargeStep { get; }
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
        public Vector3 Origo { get; set; } = Vector3.Zero;
        public float RotationAngle { get; set; } = 0;
        internal int Scaler { get; set; } = 1;

        internal virtual int TileCountX { get; } = 1;
        internal virtual int TileCountY { get; } = 1;

        internal BaseView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader)
        {
            this.main = main;
            this.loader = loader;

            statusItems = new StatusBarRenderer[8];

            statusItems[0] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.6f, 0.30f, 0.0f),
                topLeft: new Vector3(-0.6f, 0.35f, 0.0f),
                bottomRight: new Vector3(-0.2f, 0.30f, 0.0f),
                topRight: new Vector3(-0.2f, 0.35f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                TextPosition = new Vector2(20, 10),
                Text = "ImageViewer",
                FontSize = 40,
                ImageWidth = 640,
            };

            statusItems[1] = new ZoomRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.2f, 0.30f, 0.0f),
                topLeft: new Vector3(-0.2f, 0.35f, 0.0f),
                bottomRight: new Vector3(0.3f, 0.30f, 0.0f),
                topRight: new Vector3(0.3f, 0.35f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 800
            };

            statusItems[2] = new MemoryUseRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.3f, 0.30f, 0.0f),
                topLeft: new Vector3(0.3f, 0.35f, 0.0f),
                bottomRight: new Vector3(0.4f, 0.30f, 0.0f),
                topRight: new Vector3(0.4f, 0.35f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 160
            };

            statusItems[3] = new ClockRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.4f, 0.30f, 0.0f),
                topLeft: new Vector3(0.4f, 0.35f, 0.0f),
                bottomRight: new Vector3(0.6f, 0.30f, 0.0f),
                topRight: new Vector3(0.6f, 0.35f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 320
            };

            statusItems[4] = new KeyRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.6f, -0.35f, 0.0f),
                topLeft: new Vector3(-0.6f, -0.30f, 0.0f),
                bottomRight: new Vector3(-0.1f, -0.35f, 0.0f),
                topRight: new Vector3(-0.1f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 800
            };

            statusItems[5] = new DebugRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.1f, -0.35f, 0.0f),
                topLeft: new Vector3(-0.1f, -0.30f, 0.0f),
                bottomRight: new Vector3(0.35f, -0.35f, 0.0f),
                topRight: new Vector3(0.35f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 720
            };

            statusItems[6] = new TileCounterRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.35f, -0.35f, 0.0f),
                topLeft: new Vector3(0.35f, -0.30f, 0.0f),
                bottomRight: new Vector3(0.5f, -0.35f, 0.0f),
                topRight: new Vector3(0.5f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 240
            };

            statusItems[7] = new ScalerRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.5f, -0.35f, 0.0f),
                topLeft: new Vector3(0.5f, -0.30f, 0.0f),
                bottomRight: new Vector3(0.6f, -0.35f, 0.0f),
                topRight: new Vector3(0.6f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, -1 * DistanceFromUser),
                ImageWidth = 160
            };

            Pointer = new PointerRenderer(this, deviceResources, loader, 
                new PointerRenderer.Corners(
                    origo: new Vector3(0.0f, 0.0f, -1 * DistanceFromUser), 
                    topLeft: new Vector3(-0.6f, 0.30f, -1 * DistanceFromUser),
                    bottomLeft: new Vector3(-0.6f, -0.30f, -1 * DistanceFromUser)))
            {
                Position = new Vector3(0, 0, -1 * DistanceFromUser)
            };        
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
                case Command.SWITCH: main.Switch(); break;
                case Command.FORMAT: loader.DownloadRaw = !loader.DownloadRaw; break;
            }
        }

        internal void OnKeyPressed(Windows.System.VirtualKey key)
        {
            VirtualKey = key;

            switch (key)
            {
                case Windows.System.VirtualKey.NumberPad1:
                    Move(Direction.LEFT, Scaler * 1);
                    Move(Direction.DOWN, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad3:
                    Move(Direction.RIGHT, Scaler * 1);
                    Move(Direction.DOWN, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad7:
                    Move(Direction.LEFT, Scaler * 1);
                    Move(Direction.UP, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad9:
                    Move(Direction.RIGHT, Scaler * 1);
                    Move(Direction.UP, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad4:
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadRightThumbstickLeft:
                    Move(Direction.LEFT, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad6:
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadRightThumbstickRight:
                    Move(Direction.RIGHT, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad8:
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadRightThumbstickUp:
                    Move(Direction.UP, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad2:
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadRightThumbstickDown:
                    Move(Direction.DOWN, Scaler * 1);
                    break;

                case Windows.System.VirtualKey.O:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                    Move(Direction.LEFT, Scaler * LargeStep);
                    break;

                case Windows.System.VirtualKey.P:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                    Move(Direction.RIGHT, Scaler * LargeStep); break;

                case Windows.System.VirtualKey.I:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    Move(Direction.UP, Scaler * LargeStep);
                    break;

                case Windows.System.VirtualKey.L:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    Move(Direction.DOWN, Scaler * LargeStep);
                    break;

                case Windows.System.VirtualKey.Q:
                case Windows.System.VirtualKey.GamepadY:
                    Scale(Direction.UP, 1);
                    break;

                case Windows.System.VirtualKey.M:
                case Windows.System.VirtualKey.GamepadMenu:
                    switch (Scaler)
                    {
                        case 1: Scaler = 2; break;
                        case 2: Scaler = 5; break;
                        case 5: Scaler = 10; break;
                        default: Scaler = 1; break;
                    }
                    break;

                case Windows.System.VirtualKey.A:
                case Windows.System.VirtualKey.GamepadA:
                    Scale(Direction.DOWN, 1);
                    break;

                case Windows.System.VirtualKey.T:                
                case Windows.System.VirtualKey.GamepadRightTrigger:
                    Pointer.AddTag();
                    break;
                case Windows.System.VirtualKey.R:
                case Windows.System.VirtualKey.GamepadRightShoulder:
                    Pointer.RemoveTag();
                    break;

                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.GamepadView:
                    Reset();
                    break;

                case Windows.System.VirtualKey.Z:
                case Windows.System.VirtualKey.GamepadLeftTrigger:
                    SetPosition(0, 0, 0.1f);
                    break;

                case Windows.System.VirtualKey.C:
                case Windows.System.VirtualKey.GamepadLeftShoulder:
                    SetPosition(0, 0, -0.1f);
                    break;

                case Windows.System.VirtualKey.D:
                case Windows.System.VirtualKey.GamepadDPadLeft:
                    SetPosition(-0.1f, 0, 0);
                    break;

                case Windows.System.VirtualKey.X:
                case Windows.System.VirtualKey.GamepadDPadRight:
                    SetPosition(0.1f, 0, 0);
                    break;

                case Windows.System.VirtualKey.Y:
                case Windows.System.VirtualKey.GamepadDPadUp:
                    SetPosition(0, 0.1f, 0);
                    break;

                case Windows.System.VirtualKey.U:
                case Windows.System.VirtualKey.GamepadDPadDown:
                    SetPosition(0, -0.1f, 0); break;

                case Windows.System.VirtualKey.B:
                case Windows.System.VirtualKey.GamepadX:
                    SetAngle(5.0f);
                    break;

                case Windows.System.VirtualKey.N:
                case Windows.System.VirtualKey.GamepadB:
                    SetAngle(-5.0f);
                    break;

                case Windows.System.VirtualKey.F:
                case Windows.System.VirtualKey.GamepadLeftThumbstickButton:
                    Zoom(Direction.DOWN, 1);
                    break;

                case Windows.System.VirtualKey.W:
                case Windows.System.VirtualKey.GamepadRightThumbstickButton:
                    Zoom(Direction.UP, 1);
                    break;
            }
        }

        private void SetPosition(float dX, float dY, float dZ)
        {
            var dp = new Vector3(dX, dY, dZ);
            Origo = Origo + dp;

            foreach (var renderer in Tiles)
            {
                var pos = renderer.Position + dp;
                renderer.Position = pos;
            }

            foreach (var renderer in statusItems)
            {
                var pos = renderer.Position + dp;
                renderer.Position = pos;
            }

            Pointer.SetPosition(dp);
        }

        private void SetAngle(float angle)
        {
            RotationAngle = RotationAngle + angle;
            if (RotationAngle >= 360.0f)
            {
                RotationAngle -= 360.0f;
            }
            if (RotationAngle < 0.0f)
            {
                RotationAngle += 360.0f;
            }

            var rotator = Matrix4x4.CreateRotationY((float)(Math.PI * RotationAngle / 180.0f), Origo);

            foreach (var renderer in Tiles)
            {
                renderer.Rotator = rotator;
            }

            foreach (var renderer in statusItems)
            {
                renderer.Rotator = rotator;
            }

            Pointer.RotationY = RotationAngle;
            Pointer.SetRotator(rotator);
        }

        protected void Scale(Direction direction, int number)
        {
            switch (direction)
            {
                case Direction.UP:
                    Level -= number;
                    if (Level < 0)
                    {
                        Level = 0;
                    }
                    break;
                case Direction.DOWN:
                    Level += number;
                    if (Level > MinScale)
                    {
                        Level = MinScale;
                    }
                    break;
            }

            UpdateImages();
        }

        protected void Move(Direction direction, int number)
        {
            var distance = number * PixelSize(Level);

            switch (direction)
            {
                case Direction.LEFT:
                    ImageX += distance;
                    if (ImageX > maxResolution)
                    {
                        ImageX = maxResolution;
                    }
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
                    ImageY += distance;
                    if (ImageY > maxResolution)
                    {
                        ImageY = maxResolution;
                    }
                    break;
            }

            UpdateImages();
        }

        protected void Zoom(Direction direction, int number)
        {
            var c = Pointer.Coordinates();

            switch (direction)
            {
                case Direction.UP:
                    Level -= number;
                    if (Level < 0)
                    {
                        Level = 0;
                    }
                    break;
                case Direction.DOWN:
                    Level += number;
                    if (Level > MinScale)
                    {
                        Level = MinScale;    
                    }
                    break;
            }

            ImageX = c.X - ((TileCountX * Step) / 2);
            ImageY = c.Y - ((TileCountY * Step) / 2);

            UpdateImages();
        }

        protected abstract void UpdateImages();

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
            Level = 7;
            ImageX = 0;
            ImageY = 0;
            UpdateImages();

            SetPosition(-1 * Origo.X, -1 * Origo.Y, -1 * Origo.Z);
            SetAngle(-1 * RotationAngle);
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
