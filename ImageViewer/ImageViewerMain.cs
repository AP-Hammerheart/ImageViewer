﻿using ImageViewer.Common;
using ImageViewer.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.Graphics.Holographic;
using Windows.Perception.Spatial;
using Windows.UI;
using Windows.UI.Input.Spatial;

namespace ImageViewer
{
    /// <summary>
    /// Updates, renders, and presents holographic content using Direct3D.
    /// </summary>
    internal class ImageViewerMain : IDisposable
    {
        #region ENUMs

        private enum Command
        {
            UNDEFINED = 0,
            MOVE,
            ROTATE,
            SCALE,
            SET,
            PRELOAD,
            CANCEL,
            CLEAR_CACHE
        }

        private enum Direction
        {
            UNDEFINED = -1,
            UP = 0,
            DOWN,
            LEFT,
            RIGHT,
            FRONT,
            BACK
        }

        #endregion

        #region Variables

        #region Static setting variables

        private static readonly int maxX = 5;
        private static readonly int maxY = 5;
        private static readonly int focusReferenceTile = 22;
        private static readonly int tileResolution = 256;
        private static readonly int minScale = 8;
        private static readonly float distanceFromUser = 1.4f; // meters

        private static readonly string baseUrl = "http://10.10.10.4:8081/?command=image&name=";
        private static readonly string image1 = "image1.ndpi";
        private static readonly string image2 = "image2.ndpi";

        private static readonly int image2offsetX = -5500;
        private static readonly int image2offsetY = -2000;

        #endregion
        #region State variables
        private bool cancel = false;
        private bool loading = false;

        #endregion

        #region Content variables

        private TextureLoader               loader;
        private TileRenderer[]              tiles;
        private StatusBarRenderer[]         statusItems;

        #endregion

        #region Environment variables

        private SpatialInputHandler         spatialInputHandler;

        // Cached reference to device resources.
        private DeviceResources             deviceResources;

        // Render loop timer.
        private StepTimer                   timer = new StepTimer();

        // Represents the holographic space around the user.
        HolographicSpace                    holographicSpace;

        // SpatialLocator that is attached to the primary camera.
        SpatialLocator                      locator;

        // A reference frame attached to the holographic camera.
        SpatialStationaryFrameOfReference   referenceFrame;

        // Keep track of gamepads.
        List<Gamepad>                       gamepads = new List<Gamepad>();

        // Keep track of mouse input.
        bool                                pointerPressed = false;

        #endregion

        #endregion

        #region Initialize

        /// <summary>
        /// Loads and initializes application assets when the application is loaded.
        /// </summary>
        /// <param name="deviceResources"></param>
        public ImageViewerMain(DeviceResources deviceResources)
        {
            this.deviceResources = deviceResources;

            // Register to be notified if the Direct3D device is lost.
            this.deviceResources.DeviceLost     += this.OnDeviceLost;
            this.deviceResources.DeviceRestored += this.OnDeviceRestored;

            // If connected, a game controller can also be used for input.
            Gamepad.GamepadAdded += this.OnGamepadAdded;
            Gamepad.GamepadRemoved += this.OnGamepadRemoved;

            foreach (var gamepad in Gamepad.Gamepads)
            {
                OnGamepadAdded(null, gamepad);
            }

            timer.IsFixedTimeStep = true;
            timer.TargetElapsedSeconds = 1;
        }

        public void SetHolographicSpace(HolographicSpace holographicSpace)
        {
            this.holographicSpace = holographicSpace;

            loader = new TextureLoader(deviceResources, baseUrl);

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
                main: this,
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
                main: this,
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

            tiles = new TileRenderer[2 * maxX * maxY];

            var step = Step;
            
            for (var x = 0; x < maxX; x++)
            {
                for (var y = 0; y < maxY; y++)
                {
                    tiles[maxY * x + y] = new TileRenderer(deviceResources, loader, image1 
                        + "&x=" + (x * step + ImageX).ToString() 
                        + "&y=" + (y * step + ImageY).ToString() 
                        + "&w=" + tileResolution.ToString() 
                        + "&h=" + tileResolution.ToString() 
                        + "&level=" + Level.ToString())
                    {
                        Position = new Vector3(
                            -1.0f * maxX * TileRenderer.TileSize + (0.5f * TileRenderer.TileSize) + x * TileRenderer.TileSize, 
                            0.5f * maxY * TileRenderer.TileSize - (0.5f * TileRenderer.TileSize) - y * TileRenderer.TileSize, 
                            -1 * distanceFromUser)
                    };

                    tiles[(maxX * maxY)+ (maxY * x + y)] = new TileRenderer(deviceResources, loader, image2 
                        + "&x=" + (x * step + ImageX + image2offsetX).ToString() 
                        + "&y=" + (y * step + ImageY + image2offsetY).ToString()
                        + "&w=" + tileResolution.ToString() 
                        + "&h=" + tileResolution.ToString() 
                        + "&level=" + Level.ToString())
                    {
                        Position = new Vector3(
                            (0.5f * TileRenderer.TileSize) + x * TileRenderer.TileSize,
                            0.5f * maxY * TileRenderer.TileSize - (0.5f * TileRenderer.TileSize) - y * TileRenderer.TileSize,
                            -1 * distanceFromUser)
                    };
                }         
            }

            spatialInputHandler = new SpatialInputHandler();

            // Use the default SpatialLocator to track the motion of the device.
            locator = SpatialLocator.GetDefault();

            // Be able to respond to changes in the positional tracking state.
            locator.LocatabilityChanged += this.OnLocatabilityChanged;

            // Respond to camera added events by creating any resources that are specific
            // to that camera, such as the back buffer render target view.
            // When we add an event handler for CameraAdded, the API layer will avoid putting
            // the new camera in new HolographicFrames until we complete the deferral we created
            // for that handler, or return from the handler without creating a deferral. This
            // allows the app to take more than one frame to finish creating resources and
            // loading assets for the new holographic camera.
            // This function should be registered before the app creates any HolographicFrames.
            holographicSpace.CameraAdded += this.OnCameraAdded;

            // Respond to camera removed events by releasing resources that were created for that
            // camera.
            // When the app receives a CameraRemoved event, it releases all references to the back
            // buffer right away. This includes render target views, Direct2D target bitmaps, and so on.
            // The app must also ensure that the back buffer is not attached as a render target, as
            // shown in DeviceResources.ReleaseResourcesForBackBuffer.
            holographicSpace.CameraRemoved += this.OnCameraRemoved;

            // The simplest way to render world-locked holograms is to create a stationary reference frame
            // when the app is launched. This is roughly analogous to creating a "world" coordinate system
            // with the origin placed at the device's position as the app is launched.
            referenceFrame = locator.CreateStationaryFrameOfReferenceAtCurrentLocation();

            // Notes on spatial tracking APIs:
            // * Stationary reference frames are designed to provide a best-fit position relative to the
            //   overall space. Individual positions within that reference frame are allowed to drift slightly
            //   as the device learns more about the environment.
            // * When precise placement of individual holograms is required, a SpatialAnchor should be used to
            //   anchor the individual hologram to a position in the real world - for example, a point the user
            //   indicates to be of special interest. Anchor positions do not drift, but can be corrected; the
            //   anchor will use the corrected position starting in the next frame after the correction has
            //   occurred.
        }

        #endregion

        #region Frame

        #region Update

        /// <summary>
        /// Updates the application state once per frame.
        /// </summary>
        public HolographicFrame Update()
        {
            // Before doing the timer update, there is some work to do per-frame
            // to maintain holographic rendering. First, we will get information
            // about the current frame.

            // The HolographicFrame has information that the app needs in order
            // to update and render the current frame. The app begins each new
            // frame by calling CreateNextFrame.
            HolographicFrame holographicFrame = holographicSpace.CreateNextFrame();

            // Get a prediction of where holographic cameras will be when this frame
            // is presented.
            HolographicFramePrediction prediction = holographicFrame.CurrentPrediction;

            // Back buffers can change from frame to frame. Validate each buffer, and recreate
            // resource views and depth buffers as needed.
            deviceResources.EnsureCameraResources(holographicFrame, prediction);

            // Next, we get a coordinate system from the attached frame of reference that is
            // associated with the current frame. Later, this coordinate system is used for
            // for creating the stereo view matrices when rendering the sample content.
            SpatialCoordinateSystem currentCoordinateSystem = referenceFrame.CoordinateSystem;

            // Check for new input state since the last frame.
            foreach (var gamepad in gamepads)
            {
                pointerPressed |= ((gamepad.GetCurrentReading().Buttons & GamepadButtons.A) == GamepadButtons.A);
            }

            SpatialInteractionSourceState pointerState = spatialInputHandler.CheckForInput();
            SpatialPointerPose pose = null;
            if (null != pointerState)
            {
                pose = pointerState.TryGetPointerPose(currentCoordinateSystem);
            }
            else if (pointerPressed)
            {
                pose = SpatialPointerPose.TryGetAtTimestamp(currentCoordinateSystem, prediction.Timestamp);
            }
            pointerPressed = false;

            if (null != pose)
            {
                var angle = Angle(pose.Head.ForwardDirection, new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f));
                var rotator = Matrix4x4.CreateRotationY(-angle);
                var mover = Matrix4x4.CreateTranslation(pose.Head.Position);
                var transformer = rotator * mover; 

                foreach (var renderer in tiles)
                {
                    renderer.Transformer = transformer;
                }

                foreach (var renderer in statusItems)
                {
                    renderer.Transformer = transformer;
                }
            }

            timer.Tick(() => 
            {
                foreach (var renderer in tiles)
                {
                    renderer?.Update(timer);
                }
                foreach (var renderer in statusItems)
                {
                    renderer?.Update(timer);
                }
            });

            // We complete the frame update by using information about our content positioning
            // to set the focus point.
            foreach (var cameraPose in prediction.CameraPoses)
            {
                // The HolographicCameraRenderingParameters class provides access to set
                // the image stabilization parameters.
                HolographicCameraRenderingParameters renderingParameters = holographicFrame.GetRenderingParameters(cameraPose);

                // SetFocusPoint informs the system about a specific point in your scene to
                // prioritize for image stabilization. The focus point is set independently
                // for each holographic camera.
                // You should set the focus point near the content that the user is looking at.
                // In this example, we put the focus point at the center of the sample hologram,
                // since that is the only hologram available for the user to focus on.
                // You can also set the relative velocity and facing of that content; the sample
                // hologram is at a fixed point so we only need to indicate its position.

                if (tiles[focusReferenceTile] != null)
                {
                    renderingParameters.SetFocusPoint(currentCoordinateSystem,tiles[focusReferenceTile].Position);
                }              
            }

            // The holographic frame will be used to get up-to-date view and projection matrices and
            // to present the swap chain.
            return holographicFrame;
        }

        #endregion

        #region Render

        /// <summary>
        /// Renders the current frame to each holographic display, according to the 
        /// current application and spatial positioning state. Returns true if the 
        /// frame was rendered to at least one display.
        /// </summary>
        public bool Render(ref HolographicFrame holographicFrame)
        {
            // Don't try to render anything before the first Update.
            if (timer.FrameCount == 0)
            {
                return false;
            }

            //
            // TODO: Add code for pre-pass rendering here.
            //
            // Take care of any tasks that are not specific to an individual holographic
            // camera. This includes anything that doesn't need the final view or projection
            // matrix, such as lighting maps.
            //

            // Up-to-date frame predictions enhance the effectiveness of image stablization and
            // allow more accurate positioning of holograms.
            holographicFrame.UpdateCurrentPrediction();
            HolographicFramePrediction prediction = holographicFrame.CurrentPrediction;

            // Lock the set of holographic camera resources, then draw to each camera
            // in this frame.
            return deviceResources.UseHolographicCameraResources(
                (Dictionary<uint, CameraResources> cameraResourceDictionary) =>
            {
                bool atLeastOneCameraRendered = false;

                foreach (var cameraPose in prediction.CameraPoses)
                {
                    // This represents the device-based resources for a HolographicCamera.
                    CameraResources cameraResources = cameraResourceDictionary[cameraPose.HolographicCamera.Id];

                    // Get the device context.
                    var context = deviceResources.D3DDeviceContext;
                    var renderTargetView = cameraResources.BackBufferRenderTargetView;
                    var depthStencilView = cameraResources.DepthStencilView;

                    // Set render targets to the current holographic camera.
                    context.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);

                    // Clear the back buffer and depth stencil view.
                    SharpDX.Mathematics.Interop.RawColor4 transparent = new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.0f);
                    context.ClearRenderTargetView(renderTargetView, transparent);
                    context.ClearDepthStencilView(
                        depthStencilView,
                        SharpDX.Direct3D11.DepthStencilClearFlags.Depth | SharpDX.Direct3D11.DepthStencilClearFlags.Stencil,
                        1.0f,
                        0);

                    // Notes regarding holographic content:
                    //    * For drawing, remember that you have the potential to fill twice as many pixels
                    //      in a stereoscopic render target as compared to a non-stereoscopic render target
                    //      of the same resolution. Avoid unnecessary or repeated writes to the same pixel,
                    //      and only draw holograms that the user can see.
                    //    * To help occlude hologram geometry, you can create a depth map using geometry
                    //      data obtained via the surface mapping APIs. You can use this depth map to avoid
                    //      rendering holograms that are intended to be hidden behind tables, walls,
                    //      monitors, and so on.
                    //    * Black pixels will appear transparent to the user wearing the device, but you
                    //      should still use alpha blending to draw semitransparent holograms. You should
                    //      also clear the screen to Transparent as shown above.
                    //


                    // The view and projection matrices for each holographic camera will change
                    // every frame. This function refreshes the data in the constant buffer for
                    // the holographic camera indicated by cameraPose.
                    cameraResources.UpdateViewProjectionBuffer(deviceResources, cameraPose, referenceFrame.CoordinateSystem);

                    // Attach the view/projection constant buffer for this camera to the graphics pipeline.
                    bool cameraActive = cameraResources.AttachViewProjectionBuffer(deviceResources);

                    // Only render world-locked content when positional tracking is active.
                    if (cameraActive)
                    {
                        foreach (var renderer in statusItems)
                        {
                            renderer?.Render();
                        }

                        foreach (var renderer in tiles)
                        {
                            renderer?.Render();
                        }                
                    }

                    atLeastOneCameraRendered = true;
                }

                return atLeastOneCameraRendered;
            });
        }

        #endregion

        #endregion

        #region AppState

        public void Dispose()
        {

            loader?.ReleaseDeviceDependentResources();
            loader = null;

            if (statusItems != null)
            {
                foreach (var renderer in statusItems)
                {
                    renderer?.Dispose();
                }
                statusItems = null;
            }

            if (tiles != null)
            {
                foreach (var renderer in tiles)
                {
                    renderer?.Dispose();
                }
                tiles = null;
            }
        }

        public void SaveAppState()
        {
            //
            // TODO: Insert code here to save your app state.
            //       This method is called when the app is about to suspend.
            //
            //       For example, store information in the SpatialAnchorStore.
            //
        }

        public void LoadAppState()
        {
            //
            // TODO: Insert code here to load your app state.
            //       This method is called when the app resumes.
            //
            //       For example, load information from the SpatialAnchorStore.
            //
        }

        #endregion

        #region Handlers

        public void OnKeyPressed(Windows.System.VirtualKey key)
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

        public void OnPointerPressed()
        {
            this.pointerPressed = true;
        }

        /// <summary>
        /// Notifies renderers that device resources need to be released.
        /// </summary>
        public void OnDeviceLost(Object sender, EventArgs e)
        {
            foreach (var renderer in tiles)
            {
                renderer?.ReleaseDeviceDependentResources();
            }

            foreach (var renderer in statusItems)
            {
                renderer?.ReleaseDeviceDependentResources();
            }

            loader?.ReleaseDeviceDependentResources();
        }

        /// <summary>
        /// Notifies renderers that device resources may now be recreated.
        /// </summary>
        public void OnDeviceRestored(Object sender, EventArgs e)
        {
            foreach (var renderer in tiles)
            {
                renderer?.CreateDeviceDependentResourcesAsync();
            }

            foreach (var renderer in statusItems)
            {
                renderer?.CreateDeviceDependentResourcesAsync();
            }
        }

        void OnLocatabilityChanged(SpatialLocator sender, Object args)
        {
            switch (sender.Locatability)
            {
                case SpatialLocatability.Unavailable:
                    // Holograms cannot be rendered.
                    {
                        String message = "Warning! Positional tracking is " + sender.Locatability + ".";
                        Debug.WriteLine(message);
                    }
                    break;

                // In the following three cases, it is still possible to place holograms using a
                // SpatialLocatorAttachedFrameOfReference.
                case SpatialLocatability.PositionalTrackingActivating:
                // The system is preparing to use positional tracking.

                case SpatialLocatability.OrientationOnly:
                // Positional tracking has not been activated.

                case SpatialLocatability.PositionalTrackingInhibited:
                    // Positional tracking is temporarily inhibited. User action may be required
                    // in order to restore positional tracking.
                    break;

                case SpatialLocatability.PositionalTrackingActive:
                    // Positional tracking is active. World-locked content can be rendered.
                    break;
            }
        }

        public void OnCameraAdded(
            HolographicSpace sender,
            HolographicSpaceCameraAddedEventArgs args
            )
        {
            Deferral deferral = args.GetDeferral();
            HolographicCamera holographicCamera = args.Camera;

            Task task1 = new Task(() =>
            {
                //
                // TODO: Allocate resources for the new camera and load any content specific to
                //       that camera. Note that the render target size (in pixels) is a property
                //       of the HolographicCamera object, and can be used to create off-screen
                //       render targets that match the resolution of the HolographicCamera.
                //

                // Create device-based resources for the holographic camera and add it to the list of
                // cameras used for updates and rendering. Notes:
                //   * Since this function may be called at any time, the AddHolographicCamera function
                //     waits until it can get a lock on the set of holographic camera resources before
                //     adding the new camera. At 60 frames per second this wait should not take long.
                //   * A subsequent Update will take the back buffer from the RenderingParameters of this
                //     camera's CameraPose and use it to create the ID3D11RenderTargetView for this camera.
                //     Content can then be rendered for the HolographicCamera.
                deviceResources.AddHolographicCamera(holographicCamera);

                // Holographic frame predictions will not include any information about this camera until
                // the deferral is completed.
                deferral.Complete();
            });
            task1.Start();
        }

        public void OnCameraRemoved(
            HolographicSpace sender,
            HolographicSpaceCameraRemovedEventArgs args
            )
        {
            Task task2 = new Task(() =>
            {
                //
                // TODO: Asynchronously unload or deactivate content resources (not back buffer 
                //       resources) that are specific only to the camera that was removed.
                //
            });
            task2.Start();

            // Before letting this callback return, ensure that all references to the back buffer 
            // are released.
            // Since this function may be called at any time, the RemoveHolographicCamera function
            // waits until it can get a lock on the set of holographic camera resources before
            // deallocating resources for this camera. At 60 frames per second this wait should
            // not take long.
            deviceResources.RemoveHolographicCamera(args.Camera);
        }

        public void OnGamepadAdded(Object o, Gamepad args)
        {
            foreach (var knownGamepad in gamepads)
            {
                if (args == knownGamepad)
                {
                    // This gamepad is already in the list.
                    return;
                }
            }

            gamepads.Add(args);
        }

        public void OnGamepadRemoved(Object o, Gamepad args)
        {
            gamepads.Remove(args);
        }
        #endregion

        #region Voice commands

        internal void HandleVoiceCommand(IReadOnlyDictionary<string, IReadOnlyList<string>> dictionary)
        {
            var command = Command.UNDEFINED;
            var direction = Direction.UNDEFINED;
            var number = 0;

            if (dictionary.TryGetValue("COMMAND", out IReadOnlyList<string> list1))
            {
                command = (Command)Int32.Parse(list1[0]);
            }

            if (dictionary.TryGetValue("TYPE", out IReadOnlyList<string> list2))
            {
                direction = (Direction)Int32.Parse(list2[0]);
            }

            if (dictionary.TryGetValue("NUMBER", out IReadOnlyList<string> list3))
            {
                number = Int32.Parse(list3[0]);
            }

            switch (command)
            {
                case Command.MOVE: Move(direction, number); break;
                case Command.SCALE: Scale(direction, number); break;
                case Command.PRELOAD: if (!loading) PreLoadTiles(); break;
                case Command.CANCEL: if (loading) cancel = true; break;
                case Command.CLEAR_CACHE: ClearCache(); break;
            }
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
                    ImageX += number * step;
                    break;
                case Direction.RIGHT:
                    ImageX -= number * step;
                    if (ImageX < 0) ImageX = 0;
                    break;
                case Direction.DOWN:
                    ImageY -= number * step;
                    if (ImageY < 0) ImageY = 0;
                    break;
                case Direction.UP:
                    ImageY += number * step;
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

                for (var x = 0; x < 110000; x += step)
                {
                    for (var y = 0; y < 110000; y += step)
                    {
                        if (cancel)
                        {
                            loading = false;
                            cancel = false;
                            return;
                        }

                        var id1 = image1
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

                        var id2 = image2
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
            task.Wait();
        }

        #endregion

        #region Helpers

        private int Step => _Step(Level);

        public int Level { get; set; } = 7;
        public int ImageY { get; set; } = 0;
        public int ImageX { get; set; } = 0;
        public Windows.System.VirtualKey VirtualKey { get; set; } = Windows.System.VirtualKey.None;

        private int _Step(int _level)
        {
            return tileResolution * (int)Math.Pow(2, _level);
        }

        private static float Angle(Vector3 v1, Vector3 v2, Vector3 up)
        {
            var cross = Vector3.Cross(v1, v2);
            var dot = Vector3.Dot(v1, v2);
            var angle = (float)Math.Atan2(cross.Length(), dot);

            var sign = Vector3.Dot(up, cross);
            if (sign < 0.0f)
            {
                angle = -angle;
            }

            return angle;
        }

        private void UpdateImages(int step)
        {
            for (var x = 0; x < maxX; x++)
            {
                for (var y = 0; y < maxY; y++)
                {
                    var url1 = image1
                        + "&x=" + (x * step + ImageX).ToString()
                        + "&y=" + (y * step + ImageY).ToString()
                        + "&w=" + tileResolution.ToString()
                        + "&h=" + tileResolution.ToString()
                        + "&level=" + Level.ToString();

                    tiles[maxY * x + y].TextureID = url1;

                    Task task1 = new Task(async () =>
                    {
                        await loader.LoadTextureAsync(url1);
                    });
                    task1.Start();
                    task1.Wait();

                    var url2 = image2
                        + "&x=" + (x * step + ImageX + image2offsetX).ToString()
                        + "&y=" + (y * step + ImageY + image2offsetY).ToString()
                        + "&w=" + tileResolution.ToString()
                        + "&h=" + tileResolution.ToString()
                        + "&level=" + Level.ToString();

                    tiles[(maxX * maxY) + (maxY * x + y)].TextureID = url2;

                    Task task2 = new Task(async () =>
                    {
                        await loader.LoadTextureAsync(url2);
                    });
                    task2.Start();
                    task2.Wait();
                }
            }
        }

        #endregion
    }
}
