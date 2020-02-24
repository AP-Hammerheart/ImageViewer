// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Content.Renderers.ThreeD;
using ImageViewer.Content.Utils;
using static ImageViewer.ImageViewerMain;
using Windows.System;

namespace ImageViewer.Content.Views
{
    abstract class KeyEventView : NavigationView
    {
        internal Windows.System.VirtualKey VirtualKey { get; set; } = Windows.System.VirtualKey.None;
        internal Windows.System.VirtualKey LastKey { get; set; } = Windows.System.VirtualKey.None;
        internal int KeyCount { get; set; } = 0;

        protected KeyEventView() {}

        //private readonly int modes = 3;

        private enum inputModes {
            caseSelection,
            window,
            radiology,
            macro,
            histology,
            model,
        }
        private inputModes mode = inputModes.caseSelection;

        internal void OnKeyPressed(Windows.System.VirtualKey key)
        {
            VirtualKey = key;

            switch( mode ) {
                case inputModes.caseSelection:
                    CaseSelectionInputs( key );
                    break;
                case inputModes.window:
                    WindowInputs( key );
                    break;
                case inputModes.radiology:
                    RadiologyInputs( key );
                    break;
                case inputModes.macro:
                    MacroInputs( key );
                    break;
                case inputModes.histology:
                    HistologyInputs( key );
                    break;
                case inputModes.model:
                    ModelInputs( key );
                    break;
            }

            //switch( key ) {
            //    case Windows.System.VirtualKey.Number1:
            //    case Windows.System.VirtualKey.NumberPad1:
            //        Move( Direction.LEFT, Settings.Scaler );
            //        Move( Direction.DOWN, Settings.Scaler );
            //        break;

            //    case Windows.System.VirtualKey.Number3:
            //    case Windows.System.VirtualKey.NumberPad3:
            //        Move( Direction.RIGHT, Settings.Scaler );
            //        Move( Direction.DOWN, Settings.Scaler );
            //        break;

            //    case Windows.System.VirtualKey.Number7:
            //    case Windows.System.VirtualKey.NumberPad7:
            //        Move( Direction.LEFT, Settings.Scaler );
            //        Move( Direction.UP, Settings.Scaler );
            //        break;

            //    case Windows.System.VirtualKey.Number9:
            //    case Windows.System.VirtualKey.NumberPad9:
            //        Move( Direction.RIGHT, Settings.Scaler );
            //        Move( Direction.UP, Settings.Scaler );
            //        break;

            //    case Windows.System.VirtualKey.Number4:
            //    case Windows.System.VirtualKey.NumberPad4:
            //    case Windows.System.VirtualKey.Left:
            //    case Windows.System.VirtualKey.GamepadRightThumbstickLeft:
            //        if( Settings.Mode == 0 ) {
            //            if( ShowSettings ) {
            //                settingViewer.SetItem( 2 );
            //            } else {
            //                Move( Direction.LEFT, Settings.Scaler );
            //            }
            //        } else {
            //            model.Position += new System.Numerics.Vector3( -0.1f, 0.0f, 0.0f );
            //        }
            //        break;

            //    case Windows.System.VirtualKey.Number6:
            //    case Windows.System.VirtualKey.NumberPad6:
            //    case Windows.System.VirtualKey.Right:
            //    case Windows.System.VirtualKey.GamepadRightThumbstickRight:
            //        if( Settings.Mode == 0 ) {
            //            if( ShowSettings ) {
            //                settingViewer.SetItem( 3 );
            //            } else {
            //                Move( Direction.RIGHT, Settings.Scaler );
            //            }
            //        } else {
            //            model.Position += new System.Numerics.Vector3( 0.1f, 0.0f, 0.0f );
            //        }
            //        break;

            //    case Windows.System.VirtualKey.Number8:
            //    case Windows.System.VirtualKey.NumberPad8:
            //    case Windows.System.VirtualKey.Up:
            //    case Windows.System.VirtualKey.GamepadRightThumbstickUp:
            //        if( Settings.Mode == 0 ) {
            //            if( ShowSettings ) {
            //                settingViewer.SetItem( 0 );
            //            } else {
            //                Move( Direction.UP, Settings.Scaler );
            //            }
            //        } else {
            //            model.Position += new System.Numerics.Vector3( 0.0f, 0.1f, 0.0f );
            //        }
            //        break;
                    
            //    case Windows.System.VirtualKey.Number2:
            //    case Windows.System.VirtualKey.NumberPad2:
            //    case Windows.System.VirtualKey.Down:
            //    case Windows.System.VirtualKey.GamepadRightThumbstickDown:
            //        if( Settings.Mode == 0 ) {
            //            if( ShowSettings ) {
            //                settingViewer.SetItem( 1 );
            //            } else {
            //                Move( Direction.DOWN, Settings.Scaler );
            //            }
            //        } else {
            //            model.Position += new System.Numerics.Vector3( 0.0f, -0.1f, 0.0f );
            //        }
            //        break;

            //    case Windows.System.VirtualKey.O:
            //    case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
            //        if( Settings.Mode == 0 ) {
            //            if( Pointers[0].Locked ) {
            //                MovePointer( -0.01f, 0.0f );
            //            } else {
            //                Rotate( Direction.LEFT );
            //            }
            //        } else {
            //            model.RotationY -= 0.1f;
            //        }

            //        break;

            //    case Windows.System.VirtualKey.P:
            //    case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
            //        if( Settings.Mode == 0 ) {
            //            if( Pointers[0].Locked ) {
            //                MovePointer( 0.01f, 0.0f );
            //            } else {
            //                Rotate( Direction.RIGHT );
            //            }
            //        } else {
            //            model.RotationY += 0.1f;
            //        }

            //        break;

            //    case Windows.System.VirtualKey.I:
            //    case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
            //        if( Settings.Mode == 0 ) {
            //            if( Pointers[0].Locked ) {
            //                MovePointer( 0.0f, 0.01f );
            //            } else {
            //                if( Settings.Scaler < 4096 ) {
            //                    Settings.Scaler *= 2;
            //                }
            //            }
            //        } else {
            //            model.RotationX += 0.1f;
            //        }
            //        break;

            //    case Windows.System.VirtualKey.L:
            //    case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
            //        if( Settings.Mode == 0 ) {
            //            if( Pointers[0].Locked ) {
            //                MovePointer( 0.0f, -0.01f );
            //            } else {
            //                if( Settings.Scaler > 1 ) {
            //                    Settings.Scaler /= 2;
            //                }
            //            }
            //        } else {
            //            model.RotationX -= 0.1f;
            //        }
            //        break;

            //    case Windows.System.VirtualKey.Q:
            //    case Windows.System.VirtualKey.GamepadY:
            //        if( Settings.Mode == 0 ) {
            //            macro.ChangeType();
            //        } else {
            //            model.RotationZ += 0.1f;
            //        }
            //        break;

            //    case Windows.System.VirtualKey.M:
            //    case Windows.System.VirtualKey.GamepadMenu:
            //        Settings.Mode = (Settings.Mode + 1) % modes;
            //        break;

            //    case Windows.System.VirtualKey.A:
            //    case Windows.System.VirtualKey.GamepadA:
            //        if( Settings.Mode == 0 ) {
            //            Scale( Direction.DOWN, 1 );
            //        } else {
            //            model.RotationZ -= 0.1f;
            //        }
            //        break;

            //    case Windows.System.VirtualKey.T:
            //    case Windows.System.VirtualKey.GamepadRightTrigger:
            //        if( Settings.Mode == 0 ) {
            //            ((PointerRenderer)Pointers[0]).AddTag();
            //        } else if( Settings.Mode == 1 ) {
            //            model.Position += new System.Numerics.Vector3( 0.0f, 0.0f, 0.1f );
            //        } else if( Settings.Mode == 2 ) {
            //            PrevRadiologyImage();
            //        }

            //        break;
            //    case Windows.System.VirtualKey.R:
            //    case Windows.System.VirtualKey.GamepadRightShoulder:
            //        if( Settings.Mode == 0 ) {
            //            ((PointerRenderer)Pointers[0]).RemoveTag();
            //        } else if( Settings.Mode == 1 ) {
            //            model.Position += new System.Numerics.Vector3( 0.0f, 0.0f, -0.1f );
            //        } else if( Settings.Mode == 2 ) {
            //            NextRadiologyImage();
            //        }
            //        break;

            //    case Windows.System.VirtualKey.Space:
            //    case Windows.System.VirtualKey.GamepadView:
            //        if( Settings.Mode == 0 ) {
            //            if( Settings.Online ) {
            //                //settingViewer.NextSlide();
            //                //UpdateImages();
            //            }
            //        } else {
            //            model.Colored = !model.Colored;

            //            if( Settings.Online ) {
            //                ShowSettings = !ShowSettings;
            //                if( !ShowSettings ) {
            //                    Scale( Direction.DOWN, 0 );
            //                }
            //            }
            //        }
            //        break;

            //    case Windows.System.VirtualKey.Z:
            //    case Windows.System.VirtualKey.GamepadLeftTrigger:
            //        SetPosition( 0, 0, 0.1f );
            //        break;

            //    case Windows.System.VirtualKey.C:
            //    case Windows.System.VirtualKey.GamepadLeftShoulder:
            //        SetPosition( 0, 0, -0.1f );
            //        break;

            //    case Windows.System.VirtualKey.D:
            //    case Windows.System.VirtualKey.GamepadDPadLeft:
            //        SetPosition( -0.1f, 0, 0 );
            //        break;

            //    case Windows.System.VirtualKey.X:
            //    case Windows.System.VirtualKey.GamepadDPadRight:
            //        SetPosition( 0.1f, 0, 0 );
            //        break;

            //    case Windows.System.VirtualKey.Y:
            //    case Windows.System.VirtualKey.GamepadDPadUp:
            //        SetPosition( 0, 0.1f, 0 );
            //        break;

            //    case Windows.System.VirtualKey.U:
            //    case Windows.System.VirtualKey.GamepadDPadDown:
            //        SetPosition( 0, -0.1f, 0 );
            //        break;

            //    case Windows.System.VirtualKey.B:
            //    case Windows.System.VirtualKey.GamepadX:
            //        SetAngle( 5.0f );
            //        break;

            //    case Windows.System.VirtualKey.N:
            //    case Windows.System.VirtualKey.GamepadB:
            //        SetAngle( -5.0f );
            //        break;

            //    case Windows.System.VirtualKey.F:
            //    case Windows.System.VirtualKey.GamepadLeftThumbstickButton:
            //        if( Settings.Mode == 0 ) {
            //            Pointers[0].Locked = !Pointers[0].Locked;
            //        } else {
            //            model.Scale -= 0.1f;
            //            if( model.Scale < 0.5f ) {
            //                model.Scale = 0.5f;
            //            }
            //        }
            //        break;

            //    case Windows.System.VirtualKey.W:
            //    case Windows.System.VirtualKey.GamepadRightThumbstickButton:
            //        if( Settings.Mode == 0 ) {
            //            Zoom( Direction.UP, 1 );
            //        } else {
            //            model.Scale += 0.1f;
            //        }
            //        break;
            //}
            
            DebugString = Origo.ToString("0.00") + " "
                + RotationAngle.ToString() + "° "
                + Pointers[0].Position.ToString("0.00");
        }

        internal void CaseSelectionInputs( Windows.System.VirtualKey key ) {
            switch( key ) {

                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.W:
                    ChangeSelectedIDUp();
                    break;
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.S:
                    ChangeSelectedIDDown();
                    break;
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.A:
                    break;
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.D:
                    break;
                case VirtualKey.GamepadLeftThumbstickButton:
                    break;

                case VirtualKey.GamepadRightThumbstickUp:

                    break;
                case VirtualKey.GamepadRightThumbstickDown:

                    break;
                case VirtualKey.GamepadRightThumbstickLeft:
                case VirtualKey.Left:
                    break;
                case VirtualKey.GamepadRightThumbstickRight:
                case VirtualKey.Right:
                    break;
                case VirtualKey.GamepadRightThumbstickButton:
                    break;

                case VirtualKey.GamepadLeftShoulder:
                    break;
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.Number1:
                    break;
                case VirtualKey.GamepadRightShoulder:
                    break;
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.Number3:
                    break;

                case VirtualKey.GamepadDPadUp:
                    ChangeSelectedIDUp();
                    break;
                case VirtualKey.GamepadDPadDown:
                    ChangeSelectedIDDown();
                    break;
                case VirtualKey.GamepadDPadLeft:
                    break;
                case VirtualKey.GamepadDPadRight:
                    break;

                case VirtualKey.GamepadA:
                case VirtualKey.K:
                    ConfirmSelectedID();
                    ToggleCaseSelectionMenu( false );
                    mode = inputModes.window;
                    break;
                case VirtualKey.GamepadB:
                    break;
                case VirtualKey.GamepadX:
                    break;
                case VirtualKey.GamepadY:
                    break;

                case VirtualKey.GamepadMenu:
                case VirtualKey.Escape:
                    ToggleCaseSelectionMenu(false);
                    mode = inputModes.window;                
                    break;
                case VirtualKey.GamepadView:
                case VirtualKey.M:
                    break;
            }
        }

        internal void WindowInputs( Windows.System.VirtualKey key ) {
            switch( key ) {

                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.W:
                    SetPosition( 0, 0.1f, 0 );
                    break;
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.S:
                    SetPosition( 0, -0.1f, 0 );
                    break;
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.A:
                    SetPosition( -0.1f, 0, 0 );
                    break;
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.D:
                    SetPosition( 0.1f, 0, 0 );
                    break;
                case VirtualKey.GamepadLeftThumbstickButton:
                    break;

                case VirtualKey.GamepadRightThumbstickUp:

                    break;
                case VirtualKey.GamepadRightThumbstickDown:

                    break;
                case VirtualKey.GamepadRightThumbstickLeft:
                case VirtualKey.Left:
                    SetAngle( 5.0f );
                    break;
                case VirtualKey.GamepadRightThumbstickRight:
                case VirtualKey.Right:
                    SetAngle( -5.0f );
                    break;
                case VirtualKey.GamepadRightThumbstickButton:
                    break;

                case VirtualKey.GamepadLeftShoulder:
                    break;
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.Number1:
                    SetPosition( 0, 0, 0.1f );
                    break;
                case VirtualKey.GamepadRightShoulder:
                    break;
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.Number3:
                    SetPosition( 0, 0, -0.1f );
                    break;

                case VirtualKey.GamepadDPadUp:
                    break;
                case VirtualKey.GamepadDPadDown:
                    break;
                case VirtualKey.GamepadDPadLeft:
                    break;
                case VirtualKey.GamepadDPadRight:
                    break;

                case VirtualKey.GamepadA:
                    break;
                case VirtualKey.GamepadB:
                    break;
                case VirtualKey.GamepadX:
                    break;
                case VirtualKey.GamepadY:
                    break;

                case VirtualKey.GamepadMenu:
                case VirtualKey.Escape:
                    ToggleCaseSelectionMenu(true);
                    mode = inputModes.caseSelection;
                    break;
                case VirtualKey.GamepadView:
                case VirtualKey.M:
                    mode = inputModes.radiology;
                    break;

            }
        }

        internal void RadiologyInputs( Windows.System.VirtualKey key ) {
            switch( key ) {

                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.W:
                    PanRadiology(Direction.UP);
                    break;
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.S:
                    PanRadiology(Direction.DOWN);
                    break;
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.A:
                    PanRadiology(Direction.LEFT);
                    break;
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.D:
                    PanRadiology(Direction.RIGHT);
                    break;
                case VirtualKey.GamepadLeftThumbstickButton:
                    break;

                case VirtualKey.GamepadRightThumbstickUp:
                    break;
                case VirtualKey.GamepadRightThumbstickDown:
                    break;
                case VirtualKey.GamepadRightThumbstickLeft:
                    break;
                case VirtualKey.GamepadRightThumbstickRight:
                    break;
                case VirtualKey.GamepadRightThumbstickButton:
                    break;

                case VirtualKey.GamepadLeftShoulder:
                case VirtualKey.Q:
                    PrevRadiologyImage(1); //advance 1
                    break;
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.Number1:
                    PrevRadiologyImage(10); //advance 10
                    break;
                case VirtualKey.GamepadRightShoulder:
                case VirtualKey.E:
                    NextRadiologyImage(1); //advance 1
                    break;
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.Number3:
                    NextRadiologyImage(10); //advance 10
                    break;

                case VirtualKey.GamepadDPadUp:
                    break;
                case VirtualKey.GamepadDPadDown:
                    break;
                case VirtualKey.GamepadDPadLeft:
                    break;
                case VirtualKey.GamepadDPadRight:
                    break;

                case VirtualKey.GamepadA:
                case VirtualKey.K:
                    ZoomRadiology(Direction.DOWN);                    
                    break;
                case VirtualKey.GamepadB:
                case VirtualKey.L:
                    ZoomRadiology(Direction.UP);
                    break;
                case VirtualKey.GamepadX:
                case VirtualKey.J:
                    break;
                case VirtualKey.GamepadY:
                case VirtualKey.I:
                    ZoomRadiologyImage();
                    break;

                case VirtualKey.GamepadMenu:
                case VirtualKey.Escape:
                    ToggleCaseSelectionMenu(true);
                    mode = inputModes.caseSelection;
                    break;
                case VirtualKey.GamepadView:
                case VirtualKey.M:
                    mode = inputModes.macro;
                    break;

            }
        }

        internal void MacroInputs( Windows.System.VirtualKey key ) {
            switch( key ) {

                case VirtualKey.GamepadLeftThumbstickUp:
                    break;
                case VirtualKey.GamepadLeftThumbstickDown:
                    break;
                case VirtualKey.GamepadLeftThumbstickLeft:
                    break;
                case VirtualKey.GamepadLeftThumbstickRight:
                    break;
                case VirtualKey.GamepadLeftThumbstickButton:
                    break;

                case VirtualKey.GamepadRightThumbstickUp:
                    break;
                case VirtualKey.GamepadRightThumbstickDown:
                    break;
                case VirtualKey.GamepadRightThumbstickLeft:
                    break;
                case VirtualKey.GamepadRightThumbstickRight:
                    break;
                case VirtualKey.GamepadRightThumbstickButton:
                    break;

                case VirtualKey.GamepadLeftShoulder:
                case VirtualKey.Q:
                    ChangeMacroImageUp();
                    break;
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.Number1:
                    break;
                case VirtualKey.GamepadRightShoulder:
                case VirtualKey.E:
                    ChangeMacroImageDown();
                    break;
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.Number3:
                    break;

                case VirtualKey.GamepadDPadUp:
                    break;
                case VirtualKey.GamepadDPadDown:
                    break;
                case VirtualKey.GamepadDPadLeft:
                    break;
                case VirtualKey.GamepadDPadRight:
                    break;

                case VirtualKey.GamepadA:
                case VirtualKey.K:
                    //Zoom In
                    break;
                case VirtualKey.GamepadB:
                case VirtualKey.L:
                    //Zoom Out
                    break;
                case VirtualKey.GamepadX:
                case VirtualKey.J:
                    //Toggle Labels
                    break;
                case VirtualKey.GamepadY:
                case VirtualKey.I:
                    macro.ChangeType();
                    break;

                case VirtualKey.GamepadMenu:
                case VirtualKey.Escape:
                    ToggleCaseSelectionMenu(true);
                    mode = inputModes.caseSelection;
                    break;
                case VirtualKey.GamepadView:
                case VirtualKey.M:
                    mode = inputModes.histology;
                    break;

            }
        }

        internal void HistologyInputs( Windows.System.VirtualKey key ) {
            switch( key ) {

                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.W:
                    Move( Direction.UP, Settings.Scaler );
                    break;
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.S:
                    Move( Direction.DOWN, Settings.Scaler );
                    break;
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.A:
                    Move( Direction.LEFT, Settings.Scaler );
                    break;
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.D:
                    Move( Direction.RIGHT, Settings.Scaler );
                    break;
                case VirtualKey.GamepadLeftThumbstickButton:
                    break;

                case VirtualKey.GamepadRightThumbstickUp:
                case VirtualKey.Up:
                    if( Pointers[0].Locked ) {
                        MovePointer( 0.0f, 0.01f );
                    }
                    break;
                case VirtualKey.GamepadRightThumbstickDown:
                case VirtualKey.Down:
                    if( Pointers[0].Locked ) {
                        MovePointer( 0.0f, -0.01f );
                    }
                    break;
                case VirtualKey.GamepadRightThumbstickLeft:
                case VirtualKey.Left:
                    if( Pointers[0].Locked ) {
                        MovePointer( -0.01f, 0.0f );
                    }
                    break;
                case VirtualKey.GamepadRightThumbstickRight:
                case VirtualKey.Right:
                    if( Pointers[0].Locked ) {
                        MovePointer( 0.01f, 0.0f );
                    }
                    break;
                case VirtualKey.GamepadRightThumbstickButton:
                    break;

                case VirtualKey.GamepadLeftShoulder:
                case VirtualKey.Q:
                    Rotate( Direction.RIGHT );
                    break;
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.Number1:
                    Scale( Direction.DOWN, 1 );
                    break;
                case VirtualKey.GamepadRightShoulder:
                case VirtualKey.E:
                    Rotate( Direction.LEFT );
                    break;
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.Number3:
                    Zoom( Direction.UP, 1 );
                    break;

                case VirtualKey.GamepadDPadUp:
                case VirtualKey.X:
                    ChangeHistologyMapUp();
                    break;
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.C:
                    ChangeHistologyMapDown();
                    break;
                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.Z:
                    ChangeOverviewLevelUp();
                    break;
                case VirtualKey.GamepadDPadRight:
                case VirtualKey.V:
                    ChangeOverviewLevelDown();
                    break;

                case VirtualKey.GamepadA:
                case VirtualKey.K:
                    ((PointerRenderer)Pointers[0]).AddTag();
                    break;
                case VirtualKey.GamepadB:
                case VirtualKey.L:
                    ((PointerRenderer)Pointers[0]).RemoveTag();
                    break;
                case VirtualKey.GamepadX:
                case VirtualKey.J:
                    Pointers[0].Locked = !Pointers[0].Locked;
                    break;
                case VirtualKey.GamepadY:
                case VirtualKey.I:
                    break;

                case VirtualKey.GamepadMenu:
                case VirtualKey.Escape:
                    ToggleCaseSelectionMenu(true);
                    mode = inputModes.caseSelection;
                    break;
                case VirtualKey.GamepadView:
                case VirtualKey.M:
                    mode = inputModes.model;
                    break;

            }
        }

        internal void ModelInputs( Windows.System.VirtualKey key ) {
            switch( key ) {

                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.W:
                    model.RotationY += 0.1f;
                    break;
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.S:
                    model.RotationY -= 0.1f;
                    break;
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.A:
                    model.RotationX -= 0.1f;
                    break;
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.D:
                    model.RotationX += 0.1f;
                    break;
                case VirtualKey.GamepadLeftThumbstickButton:
                    break;

                case VirtualKey.GamepadRightThumbstickUp:
                case VirtualKey.Up:
                    model.Position += new System.Numerics.Vector3( 0.0f, 0.1f, 0.0f );
                    break;
                case VirtualKey.GamepadRightThumbstickDown:
                case VirtualKey.Down:
                    model.Position += new System.Numerics.Vector3( 0.0f, -0.1f, 0.0f );
                    break;
                case VirtualKey.GamepadRightThumbstickLeft:
                case VirtualKey.Left:
                    model.Position += new System.Numerics.Vector3( -0.1f, 0.0f, 0.0f );
                    break;
                case VirtualKey.GamepadRightThumbstickRight:
                case VirtualKey.Right:
                    model.Position += new System.Numerics.Vector3( 0.1f, 0.0f, 0.0f );
                    break;
                case VirtualKey.GamepadRightThumbstickButton:
                    break;

                case VirtualKey.GamepadLeftShoulder:
                case VirtualKey.Q:
                    model.RotationZ -= 0.1f;
                    break;
                case VirtualKey.GamepadLeftTrigger:
                case VirtualKey.Number1:
                    model.RotationZ += 0.1f;
                    break;
                case VirtualKey.GamepadRightShoulder:
                case VirtualKey.E:
                    model.Position += new System.Numerics.Vector3( 0.0f, 0.0f, 0.1f );
                    break;
                case VirtualKey.GamepadRightTrigger:
                case VirtualKey.Number3:
                    model.Position += new System.Numerics.Vector3( 0.0f, 0.0f, -0.1f );
                    break;

                case VirtualKey.GamepadDPadUp:
                    break;
                case VirtualKey.GamepadDPadDown:
                    break;
                case VirtualKey.GamepadDPadLeft:
                    break;
                case VirtualKey.GamepadDPadRight:
                    break;

                case VirtualKey.GamepadA:
                case VirtualKey.K:
                    model.Scale += 0.1f;
                    break;
                case VirtualKey.GamepadB:
                case VirtualKey.L:
                    model.Scale -= 0.1f;
                    if( model.Scale < 0.5f ) {
                        model.Scale = 0.5f;
                    }
                    break;
                case VirtualKey.GamepadX:
                case VirtualKey.J:
                    model.Colored = !model.Colored;
                    break;
                case VirtualKey.GamepadY:
                case VirtualKey.I:
                    break;

                case VirtualKey.GamepadMenu:
                case VirtualKey.Escape:
                    ToggleCaseSelectionMenu(true);
                    mode = inputModes.caseSelection;
                    break;
                case VirtualKey.GamepadView:
                case VirtualKey.M:
                    mode = inputModes.window;
                    break;

            }
        }
    }
}
