// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Content.Renderers.ThreeD;
using ImageViewer.Content.Utils;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Views
{
    abstract class KeyEventView : NavigationView
    {
        internal Windows.System.VirtualKey VirtualKey { get; set; } = Windows.System.VirtualKey.None;
        internal Windows.System.VirtualKey LastKey { get; set; } = Windows.System.VirtualKey.None;
        internal int KeyCount { get; set; } = 0;

        protected KeyEventView() {}

        private readonly int modes = 2;

        internal void OnKeyPressed(Windows.System.VirtualKey key)
        {
            VirtualKey = key;

            switch (key)
            {
                case Windows.System.VirtualKey.NumberPad1:
                    Move(Direction.LEFT, Settings.Scaler);
                    Move(Direction.DOWN, Settings.Scaler);
                    break;

                case Windows.System.VirtualKey.NumberPad3:
                    Move(Direction.RIGHT, Settings.Scaler);
                    Move(Direction.DOWN, Settings.Scaler);
                    break;

                case Windows.System.VirtualKey.NumberPad7:
                    Move(Direction.LEFT, Settings.Scaler);
                    Move(Direction.UP, Settings.Scaler);
                    break;

                case Windows.System.VirtualKey.NumberPad9:
                    Move(Direction.RIGHT, Settings.Scaler);
                    Move(Direction.UP, Settings.Scaler);
                    break;

                case Windows.System.VirtualKey.NumberPad4:
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadRightThumbstickLeft:
                    if (Settings.Mode == 0)
                    {
                        if (ShowSettings)
                        {
                            settingViewer.SetItem(2);
                        }
                        else
                        {
                            Move(Direction.LEFT, Settings.Scaler);
                        }
                    }
                    else
                    {
                        model.Position = model.Position + new System.Numerics.Vector3(-0.1f, 0.0f, 0.0f);
                    }
                    break;

                case Windows.System.VirtualKey.NumberPad6:
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadRightThumbstickRight:
                    if (Settings.Mode == 0)
                    {
                        if (ShowSettings)
                        {
                            settingViewer.SetItem(3);
                        }
                        else
                        {
                            Move(Direction.RIGHT, Settings.Scaler);
                        }
                    }
                    else
                    {
                        model.Position = model.Position + new System.Numerics.Vector3(0.1f, 0.0f, 0.0f);
                    }
                    break;

                case Windows.System.VirtualKey.NumberPad8:
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadRightThumbstickUp:
                    if (Settings.Mode == 0)
                    {
                        if (ShowSettings)
                        {
                            settingViewer.SetItem(0);
                        }
                        else
                        {
                            Move(Direction.UP, Settings.Scaler);
                        }
                    }
                    else
                    {
                        model.Position = model.Position + new System.Numerics.Vector3(0.0f, 0.1f, 0.0f);
                    }
                    break;

                case Windows.System.VirtualKey.NumberPad2:
                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadRightThumbstickDown:
                    if (Settings.Mode == 0)
                    {
                        if (ShowSettings)
                        {
                            settingViewer.SetItem(1);
                        }
                        else
                        {
                            Move(Direction.DOWN, Settings.Scaler);
                        }
                    }
                    else
                    {
                        model.Position = model.Position + new System.Numerics.Vector3(0.0f, -0.1f, 0.0f);
                    }
                    break;

                case Windows.System.VirtualKey.O:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                    if (Settings.Mode == 0)
                    {
                        if (Pointers[0].Locked)
                        {
                            MovePointer(-0.01f, 0.0f);  
                        }
                        else
                        {
                            Rotate(Direction.LEFT);
                        }
                    }
                    else
                    {
                        model.RotationY -= 0.1f;
                    }
                   
                    break;

                case Windows.System.VirtualKey.P:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                    if (Settings.Mode == 0)
                    {
                        if (Pointers[0].Locked)
                        {
                            MovePointer(0.01f, 0.0f);  
                        }
                        else
                        {
                            Rotate(Direction.RIGHT);
                        }
                    }
                    else
                    {
                        model.RotationY += 0.1f;
                    }
                    
                    break;

                case Windows.System.VirtualKey.I:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    if (Settings.Mode == 0)
                    {
                        if (Pointers[0].Locked)
                        {
                            MovePointer(0.0f, 0.01f);    
                        }
                        else
                        {
                            if (Settings.Scaler < 4096)
                            {
                                Settings.Scaler *= 2;
                            }
                        }
                    }
                    else
                    {
                        model.RotationX += 0.1f;
                    }     
                    break;

                case Windows.System.VirtualKey.L:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    if (Settings.Mode == 0)
                    {
                        if (Pointers[0].Locked)
                        {
                            MovePointer(0.0f, -0.01f);     
                        }
                        else
                        {
                            if (Settings.Scaler > 1)
                            {
                                Settings.Scaler /= 2;
                            }
                        }
                    }
                    else
                    {
                        model.RotationX -= 0.1f;
                    }
                    break;

                case Windows.System.VirtualKey.Q:
                case Windows.System.VirtualKey.GamepadY:
                    if (Settings.Mode == 0)
                    {
                        macro.ChangeType();
                    }
                    else
                    {
                        model.RotationZ += 0.1f;
                    }
                    break;

                case Windows.System.VirtualKey.M:
                case Windows.System.VirtualKey.GamepadMenu:
                    Settings.Mode = (Settings.Mode + 1) % modes;
                    break;

                case Windows.System.VirtualKey.A:
                case Windows.System.VirtualKey.GamepadA:
                    if (Settings.Mode == 0)
                    {
                        Scale(Direction.DOWN, 1);
                    }
                    else
                    {
                        model.RotationZ -= 0.1f;
                    }
                    break;

                case Windows.System.VirtualKey.T:
                case Windows.System.VirtualKey.GamepadRightTrigger:
                    if (Settings.Mode == 0)
                    {
                        ((PointerRenderer)Pointers[0]).AddTag();
                    }
                    else
                    {
                        model.Position = model.Position + new System.Numerics.Vector3(0.0f, 0.0f, 0.1f);
                    }
                    
                    break;
                case Windows.System.VirtualKey.R:
                case Windows.System.VirtualKey.GamepadRightShoulder:
                    if (Settings.Mode == 0)
                    {
                        ((PointerRenderer)Pointers[0]).RemoveTag();
                    }
                    else
                    {
                        model.Position = model.Position + new System.Numerics.Vector3(0.0f, 0.0f, -0.1f);
                    }
                    break;

                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.GamepadView:
                    if (Settings.Mode == 0)
                    {
                        //if (Settings.Online)
                        //{
                            settingViewer.NextSlide();
                            UpdateImages();
                        //}
                    }
                    else
                    {
                        model.Colored = !model.Colored;

                        //if (Settings.Online)
                        //{
                        //ShowSettings = !ShowSettings;
                        //if (!ShowSettings)
                        //{
                        //    Scale(Direction.DOWN, 0);
                        //}
                        //}
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
                    if (Settings.Mode == 0)
                    {
                        Pointers[0].Locked = !Pointers[0].Locked;
                    }
                    else
                    {
                        model.Scale -= 0.1f;
                        if (model.Scale < 0.5f)
                        {
                            model.Scale = 0.5f;
                        }
                    }                  
                    break;

                case Windows.System.VirtualKey.W:
                case Windows.System.VirtualKey.GamepadRightThumbstickButton:
                    if (Settings.Mode == 0)
                    {
                        Zoom(Direction.UP, 1);
                    }
                    else
                    {
                        model.Scale += 0.1f;
                    }                    
                    break;
            }

            DebugString = Origo.ToString("0.00") + " "
                + RotationAngle.ToString() + "° "
                + Pointers[0].Position.ToString("0.00");
        }
    }
}
