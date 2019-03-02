// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Content.Utils;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Views
{
    abstract class SpeechView : KeyEventView
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

        protected SpeechView(ImageViewerMain main, TextureLoader loader)
        {
            this.main = main;
            this.loader = loader;
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

        private void ClearCache()
        {
            Task task = new Task(async () =>
            {
                await loader.ClearCacheAsync();
            });
            task.Start();
        }

        private void Help()
        {
            main.Speak(Text);
        }
    }
}

