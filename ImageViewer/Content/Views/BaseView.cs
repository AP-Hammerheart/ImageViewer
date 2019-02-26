// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Spatial;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Views
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

        private SettingViewer settingViewer;
        private bool ShowSettings = false;

        protected StatusBarRenderer[] statusItems;

        internal int Level { get; set; } = 7;
        internal int TopLeftY { get; set; } = 0;
        internal int TopLeftX { get; set; } = 0;

        internal int BottomLeftY { get; set; } = 0;
        internal int BottomLeftX { get; set; } = 0;

        internal int TopRightY { get; set; } = 0;
        internal int TopRightX { get; set; } = 0;

        internal int BottomRightY { get; set; } = 0;
        internal int BottomRightX { get; set; } = 0;

        internal int CenterY { get; set; } = 0;
        internal int CenterX { get; set; } = 0;

        internal double Angle { get; set; } = 0.0;

        internal Windows.System.VirtualKey VirtualKey { get; set; } = Windows.System.VirtualKey.None;
        internal Windows.System.VirtualKey LastKey { get; set; } = Windows.System.VirtualKey.None;
        internal int KeyCount { get; set; } = 0;
        protected abstract int LargeStep { get; }
        protected int PixelSize(int level) => (int)Math.Pow(2, Settings.Multiplier * level);
        protected virtual int TileOffset(int level) => TileResolution * PixelSize(level);
        internal int Step => TileOffset(Level);
        internal PlaneRenderer[] Tiles { get; set; }
        protected int TileResolution { get; set; }

        internal int FPS { get; set; } = 0;

        internal string DebugString { get; set; } = "";
        internal string ErrorString { get; set; } = "";

        internal PointerRenderer Pointer { get; set; }
        internal Vector3 Origo { get; set; } = Vector3.Zero;
        internal float RotationAngle { get; set; } = 0;

        internal virtual int TileCountX { get; } = 1;
        internal virtual int TileCountY { get; } = 1;

        internal BaseView(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader)
        {
            this.main = main;
            this.loader = loader;

            settingViewer = new SettingViewer(main, deviceResources, loader);

            statusItems = new StatusBarRenderer[11];

            statusItems[0] = new StatusBarRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.6f, 0.33f, 0.0f),
                topLeft: new Vector3(-0.6f, 0.38f, 0.0f),
                bottomRight: new Vector3(-0.2f, 0.33f, 0.0f),
                topRight: new Vector3(-0.2f, 0.38f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                TextPosition = new Vector2(20, 10),
                Text = "ImageViewer",
                FontSize = 40,
                ImageWidth = 640,
            };

            statusItems[1] = new ZoomRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.2f, 0.33f, 0.0f),
                topLeft: new Vector3(-0.2f, 0.38f, 0.0f),
                bottomRight: new Vector3(0.25f, 0.33f, 0.0f),
                topRight: new Vector3(0.25f, 0.38f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 720
            };

            statusItems[2] = new MemoryUseRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.25f, 0.33f, 0.0f),
                topLeft: new Vector3(0.25f, 0.38f, 0.0f),
                bottomRight: new Vector3(0.35f, 0.33f, 0.0f),
                topRight: new Vector3(0.35f, 0.38f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 160
            };

            statusItems[3] = new FpsRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.35f, 0.33f, 0.0f),
                topLeft: new Vector3(0.35f, 0.38f, 0.0f),
                bottomRight: new Vector3(0.45f, 0.33f, 0.0f),
                topRight: new Vector3(0.45f, 0.38f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 160
            };

            statusItems[4] = new ClockRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.45f, 0.33f, 0.0f),
                topLeft: new Vector3(0.45f, 0.38f, 0.0f),
                bottomRight: new Vector3(0.6f, 0.33f, 0.0f),
                topRight: new Vector3(0.6f, 0.38f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 240
            };

            statusItems[5] = new NameRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.6f, 0.30f, 0.0f),
                topLeft: new Vector3(-0.6f, 0.33f, 0.0f),
                bottomRight: new Vector3(0.0f, 0.30f, 0.0f),
                topRight: new Vector3(0.0f, 0.33f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                TextPosition = new Vector2(10, 10),
                ImageWidth = 960,
                ImageHeight = 48,
                FontSize = 25.0f,
                BackgroundColor = Colors.LightGray,
                Index = 0
            };

            statusItems[6] = new NameRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.0f, 0.30f, 0.0f),
                topLeft: new Vector3(0.0f, 0.33f, 0.0f),
                bottomRight: new Vector3(0.6f, 0.30f, 0.0f),
                topRight: new Vector3(0.6f, 0.33f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                TextPosition = new Vector2(10, 10),
                ImageWidth = 960,
                ImageHeight = 48,
                FontSize = 25.0f,
                BackgroundColor = Colors.LightGray,
                Index = 1
            };

            statusItems[7] = new KeyRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.6f, -0.35f, 0.0f),
                topLeft: new Vector3(-0.6f, -0.30f, 0.0f),
                bottomRight: new Vector3(-0.1f, -0.35f, 0.0f),
                topRight: new Vector3(-0.1f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 800
            };

            statusItems[8] = new DebugRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(-0.1f, -0.35f, 0.0f),
                topLeft: new Vector3(-0.1f, -0.30f, 0.0f),
                bottomRight: new Vector3(0.35f, -0.35f, 0.0f),
                topRight: new Vector3(0.35f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 720
            };

            statusItems[9] = new TileCounterRenderer(
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.35f, -0.35f, 0.0f),
                topLeft: new Vector3(0.35f, -0.30f, 0.0f),
                bottomRight: new Vector3(0.5f, -0.35f, 0.0f),
                topRight: new Vector3(0.5f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 240
            };

            statusItems[10] = new ScalerRenderer(
                view: this,
                deviceResources: deviceResources,
                loader: loader,
                bottomLeft: new Vector3(0.5f, -0.35f, 0.0f),
                topLeft: new Vector3(0.5f, -0.30f, 0.0f),
                bottomRight: new Vector3(0.6f, -0.35f, 0.0f),
                topRight: new Vector3(0.6f, -0.30f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                ImageWidth = 160
            };

            Pointer = new PointerRenderer(this, deviceResources, loader, 
                new PointerRenderer.Corners(
                    origo: new Vector3(0.0f, 0.0f, Settings.DistanceFromUser), 
                    topLeft: new Vector3(-0.6f, 0.30f, Settings.DistanceFromUser),
                    bottomLeft: new Vector3(-0.6f, -0.30f, Settings.DistanceFromUser)))
            {
                Position = new Vector3(0, 0, Settings.DistanceFromUser)
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

            settingViewer?.Update(timer);

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

            if (ShowSettings)
            {
                settingViewer?.Render();
            }
            else
            {
                foreach (var renderer in Tiles)
                {
                    renderer?.Render();
                }
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

            settingViewer?.Dispose();
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

            await settingViewer?.CreateDeviceDependentResourcesAsync();

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

            settingViewer?.ReleaseDeviceDependentResources();

            Pointer?.ReleaseDeviceDependentResources();
        }

        internal void HandleVoiceCommand(Command command, Direction direction, int number)
        {
            switch (command)
            {
                case Command.MOVE: Move(direction, number); break;
                case Command.SCALE: Scale(direction, number); break;
                case Command.SET:
                    if (direction == Direction.BACK)
                    {
                        SetPointer(direction, number);
                    }
                    else
                    {
                        Settings.SetIP(direction, number);
                        settingViewer.Update();
                    }
                    break;
                case Command.PRELOAD: break;
                case Command.CANCEL: break;
                case Command.CLEAR_CACHE: ClearCache(); break;
                case Command.ADD_TAG: Pointer.AddTag(); break;
                case Command.REMOVE_TAG: Pointer.RemoveTag(); break;
                case Command.RESET_POSITION: Reset(); break;
                case Command.HELP: Help(); break;
                case Command.ZOOM: Zoom(direction, number); break;
                case Command.SWITCH: main.Switch(); break;
                case Command.FORMAT: Settings.DownloadRaw = !Settings.DownloadRaw; break;
            }
        }

        internal virtual void OnKeyPressed(Windows.System.VirtualKey key)
        {
            VirtualKey = key;

            switch (key)
            {
                case Windows.System.VirtualKey.NumberPad1:
                    Move(Direction.LEFT, Settings.Scaler * 1);
                    Move(Direction.DOWN, Settings.Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad3:
                    Move(Direction.RIGHT, Settings.Scaler * 1);
                    Move(Direction.DOWN, Settings.Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad7:
                    Move(Direction.LEFT, Settings.Scaler * 1);
                    Move(Direction.UP, Settings.Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad9:
                    Move(Direction.RIGHT, Settings.Scaler * 1);
                    Move(Direction.UP, Settings.Scaler * 1);
                    break;

                case Windows.System.VirtualKey.NumberPad4:
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadRightThumbstickLeft:
                    if (ShowSettings)
                    {
                        settingViewer.SetItem(2);
                    }
                    else
                    {
                        Move(Direction.LEFT, Settings.Scaler * 1);
                    }                
                    break;

                case Windows.System.VirtualKey.NumberPad6:
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadRightThumbstickRight:
                    if (ShowSettings)
                    {
                        settingViewer.SetItem(3);
                    }
                    else
                    {
                        Move(Direction.RIGHT, Settings.Scaler * 1);
                    }               
                    break;

                case Windows.System.VirtualKey.NumberPad8:
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadRightThumbstickUp:
                    if (ShowSettings)
                    {
                        settingViewer.SetItem(0);
                    }
                    else
                    {
                        Move(Direction.UP, Settings.Scaler * 1);
                    }               
                    break;

                case Windows.System.VirtualKey.NumberPad2:
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadRightThumbstickDown:
                    if (ShowSettings)
                    {
                        settingViewer.SetItem(1);
                    }
                    else
                    {
                        Move(Direction.DOWN, Settings.Scaler * 1);
                    }             
                    break;

                case Windows.System.VirtualKey.O:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                    if (Pointer.Locked)
                    {
                        MovePointer(-0.01f, 0.0f);
                    }
                    else
                    {
                        Move(Direction.LEFT, Settings.Scaler * LargeStep);
                    }         
                    break;

                case Windows.System.VirtualKey.P:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                    if (Pointer.Locked)
                    {
                        MovePointer(0.01f, 0.0f);
                    }
                    else
                    {
                        Move(Direction.RIGHT, Settings.Scaler * LargeStep);
                    }        
                    break;

                case Windows.System.VirtualKey.I:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    if (Pointer.Locked)
                    {
                        MovePointer(0.0f, 0.01f);
                    }
                    else
                    {
                        Move(Direction.UP, Settings.Scaler * LargeStep);
                    }           
                    break;

                case Windows.System.VirtualKey.L:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    if (Pointer.Locked)
                    {
                        MovePointer(0.0f, -0.01f);
                    }
                    else
                    {
                        Move(Direction.DOWN, Settings.Scaler * LargeStep);
                    } 
                    break;

                case Windows.System.VirtualKey.Q:
                case Windows.System.VirtualKey.GamepadY:
                    Scale(Direction.UP, 1);
                    break;

                case Windows.System.VirtualKey.M:
                case Windows.System.VirtualKey.GamepadMenu:
                    if (Settings.Online)
                    {
                        ShowSettings = !ShowSettings;
                        if (!ShowSettings)
                        {
                            Scale(Direction.DOWN, 0);
                        }
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
                    //Reset();
                    if (Settings.Online)
                    {
                        settingViewer.NextSlide();
                        UpdateImages();
                    }                 
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
                    SetPosition(0, -0.1f, 0);
                    break;

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
                    Pointer.Locked = !Pointer.Locked;
                    //Zoom(Direction.DOWN, 1);
                    break;

                case Windows.System.VirtualKey.W:
                case Windows.System.VirtualKey.GamepadRightThumbstickButton:
                    Zoom(Direction.UP, 1);
                    break;
            }

            DebugString = Origo.ToString("0.00") + " "
                + RotationAngle.ToString() + "° "
                + Pointer.Position.ToString("0.00");
        }

        private void MovePointer(float x, float y)
        {
            Pointer.SetDeltaXY(x, y);
        }

        protected virtual void SetPosition(float dX, float dY, float dZ)
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

            settingViewer.SetPosition(dp);
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

            var rotator = Matrix4x4.CreateRotationY((float)(Math.PI * RotationAngle / 180.0f), Pointer.Origo());

            foreach (var renderer in Tiles)
            {
                renderer.GlobalRotator = rotator;
            }

            foreach (var renderer in statusItems)
            {
                renderer.GlobalRotator = rotator;
            }

            settingViewer.SetRotator(rotator);

            Pointer.RotationY = RotationAngle;
            Pointer.SetRotator(rotator);
        }

        protected virtual void SetCorners()
        {
            var width = Step * TileCountX;
            var height = Step * TileCountY;

            TopRightX = TopLeftX + width;
            TopRightY = TopLeftY;

            BottomLeftX = TopLeftX;
            BottomLeftY = TopLeftY + height;

            BottomRightX = TopLeftX + width;
            BottomRightY = TopLeftY + height;
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
                    if (Level > Settings.MinScale)
                    {
                        Level = Settings.MinScale;
                    }
                    break;
            }

            SetCorners();
            UpdateImages();       
        }

        protected virtual void Move(Direction direction, int number)
        {
            var distance = number * PixelSize(Level);

            switch (direction)
            {
                case Direction.LEFT:
                    TopLeftX += distance;
                    if (TopLeftX > Settings.MaxResolutionX)
                    {
                        TopLeftX = Settings.MaxResolutionX;
                    }
                    break;
                case Direction.RIGHT:
                    TopLeftX -= distance;
                    if (TopLeftX < 0)
                    {
                        TopLeftX = 0;
                    }
                    break;
                case Direction.DOWN:
                    TopLeftY -= distance;
                    if (TopLeftY < 0)
                    {
                        TopLeftY = 0;
                    }
                    break;
                case Direction.UP:
                    TopLeftY += distance;
                    if (TopLeftY > Settings.MaxResolutionY)
                    {
                        TopLeftY = Settings.MaxResolutionY;
                    }
                    break;
            }

            SetCorners();
            UpdateImages();
        }

        protected virtual void Zoom(Direction direction, int number)
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
                    if (Level > Settings.MinScale)
                    {
                        Level = Settings.MinScale;    
                    }
                    break;
            }

            TopLeftX = Math.Max(c.X - ((TileCountX * Step) / 2), 0);
            TopLeftY = Math.Max(c.Y - ((TileCountY * Step) / 2), 0);

            SetCorners();
            UpdateImages();
        }

        protected abstract void UpdateImages();

        private void SetPointer(Direction direction, int number)
        {
            if (direction == Direction.BACK)
            {
                switch (number)
                {
                    case 0: Pointer.Visible = false; break;
                    case 1: Pointer.Visible = true; break;
                    case 2:
                        Pointer.Locked = false;
                        Pointer.GlobalRotator = Matrix4x4.Identity;
                        break;
                    case 3: Pointer.Locked = true; break;
                    case 4: Pointer.AddTag(); break;
                    case 5: Pointer.RemoveTag(); break;
                }
            }
        }

        private void ClearCache()
        {
            Task task = new Task(async () =>
            {
                await loader.ClearCacheAsync();
            });
            task.Start();
        }

        private void Reset()
        {
            Level = 7;
            TopLeftX = 0;
            TopLeftY = 0;
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
