// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Globalization;
using Windows.Graphics.Holographic;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;

namespace ImageViewer
{
    /// <summary>
    /// The IFrameworkView connects the app with Windows and handles application lifecycle events.
    /// </summary>
    internal class AppView : IFrameworkView, IDisposable
    {
        /// <summary>
        /// the HResult 0x8004503a typically represents the case where a recognizer for a particular language cannot
        /// be found. This may occur if the language is installed, but the speech pack for that language is not.
        /// See Settings -> Time & Language -> Region & Language -> *Language* -> Options -> Speech Language Options.
        /// </summary>
        private static readonly uint HResultRecognizerNotFound = 0x8004503a;

        private SpeechRecognizer speechRecognizer;
        private ImageViewerMain main;

        private DeviceResources         deviceResources;
        private bool                    windowClosed        = false;
        private bool                    windowVisible       = true;

        // The holographic space the app will use for rendering.
        private HolographicSpace        holographicSpace    = null;

        private DateTime lastEvent = DateTime.Now;

        internal static MediaPlayer MediaPlayer { get; set; }
        internal static SpeechSynthesizer Synthesizer { get; set; }

        public AppView()
        {
            windowVisible = true;

            MediaPlayer = new MediaPlayer();
            Synthesizer = new SpeechSynthesizer();
        }

        public void Dispose()
        {
            if (deviceResources != null)
            {
                deviceResources.Dispose();
                deviceResources = null;
            }

            if (main != null)
            {
                main.Dispose();
                main = null;
            }

            if (speechRecognizer != null)
            {
                speechRecognizer.Dispose();
                speechRecognizer = null;
            }

            if (Synthesizer != null)
            {
                Synthesizer.Dispose();
                Synthesizer = null;
            }

            if (MediaPlayer != null)
            {
                MediaPlayer.Dispose();
                MediaPlayer = null;
            }
        }

        #region IFrameworkView Members

        /// <summary>
        /// The first method called when the IFrameworkView is being created.
        /// Use this method to subscribe for Windows shell events and to initialize your app.
        /// </summary>
        public void Initialize(CoreApplicationView applicationView)
        {
            applicationView.Activated += this.OnViewActivated;

            // Register event handlers for app lifecycle.
            CoreApplication.Suspending += this.OnSuspending;
            CoreApplication.Resuming += this.OnResuming;

            // At this point we have access to the device and we can create device-dependent
            // resources.
            deviceResources = new DeviceResources();

            main = new ImageViewerMain(deviceResources, this);
        }

        /// <summary>
        /// Called when the CoreWindow object is created (or re-created).
        /// </summary>
        public void SetWindow(CoreWindow window)
        {
            // Register for keypress notifications.
            window.KeyDown += this.OnKeyPressed;

            // Register for pointer pressed notifications.
            window.PointerPressed += this.OnPointerPressed;

            // Register for notification that the app window is being closed.
            window.Closed += this.OnWindowClosed;

            // Register for notifications that the app window is losing focus.
            window.VisibilityChanged += this.OnVisibilityChanged;

            // Create a holographic space for the core window for the current view.
            // Presenting holographic frames that are created by this holographic space will put
            // the app into exclusive mode.
            holographicSpace = HolographicSpace.CreateForCoreWindow(window);

            // The DeviceResources class uses the preferred DXGI adapter ID from the holographic
            // space (when available) to create a Direct3D device. The HolographicSpace
            // uses this ID3D11Device to create and manage device-based resources such as
            // swap chains.
            deviceResources.SetHolographicSpace(holographicSpace);

            // The main class uses the holographic space for updates and rendering.
            main.SetHolographicSpace(holographicSpace);
        }


        /// <summary>
        /// The Load method can be used to initialize scene resources or to load a
        /// previously saved app state.
        /// </summary>
        public void Load(string entryPoint)
        {
        }

        /// <summary>
        /// This method is called after the window becomes active. It oversees the
        /// update, draw, and present loop, and also oversees window message processing.
        /// </summary>
        public void Run()
        {
            while (!windowClosed)
            {
                if (windowVisible && (null != holographicSpace))
                {
                    CoreWindow.GetForCurrentThread().Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);
                    
                    HolographicFrame frame = main.Update();

                    if (main.Render(ref frame))
                    {
                        deviceResources.Present(ref frame);
                    }
                }
                else
                {
                    CoreWindow.GetForCurrentThread().Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessOneAndAllPending);
                }
            }
        }

        /// <summary>
        /// Terminate events do not cause Uninitialize to be called. It will be called if your IFrameworkView
        /// class is torn down while the app is in the foreground.
        // This method is not often used, but IFrameworkView requires it and it will be called for
        // holographic apps.
        /// </summary>
        public void Uninitialize()
        {
        }

        #endregion

        #region Application lifecycle event handlers

        /// <summary>
        /// Called when the app is prelaunched.Use this method to load resources ahead of time
        /// and enable faster launch times.
        /// </summary>
        public void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PrelaunchActivated)
            {
                //
                // TODO: Insert code to preload resources here.
                //
            }
        }

        /// <summary>
        /// Called when the app view is activated. Activates the app's CoreWindow.
        /// </summary>
        private async void OnViewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            // Run() won't start until the CoreWindow is activated.
            sender.CoreWindow.Activate();

            await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            await speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private void OnSuspending(object sender, SuspendingEventArgs args)
        {
            // Save app state asynchronously after requesting a deferral. Holding a deferral
            // indicates that the application is busy performing suspending operations. Be
            // aware that a deferral may not be held indefinitely; after about five seconds,
            // the app will be forced to exit.
            var deferral = args.SuspendingOperation.GetDeferral();

            Task.Run(async () => 
                {
                    deviceResources.Trim();

                    if (null != main)
                    {
                        main.SaveAppState();
                    }

                    await UninitializeRecognizer();

                    deferral.Complete();             
                });
        }

        private void OnResuming(object sender, object args)
        {
            // Restore any data or state that was unloaded on suspend. By default, data
            // and state are persisted when resuming from suspend. Note that this event
            // does not occur if the app was previously terminated.

            if (null != main)
            {
                main.LoadAppState();
            }

            Task.Run(async () =>
            {
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            });
        }

        #endregion;

        #region Window event handlers

        private void OnVisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            windowVisible = args.Visible;
        }

        private void OnWindowClosed(CoreWindow sender, CoreWindowEventArgs arg)
        {
            windowClosed = true;
        }

        #endregion

        #region Input event handlers

        private void OnKeyPressed(CoreWindow sender, KeyEventArgs args)
        {
            //
            // TODO: Bluetooth keyboards are supported by HoloLens. You can use this method for
            //       keyboard input if you want to support it as an optional input method for
            //       your holographic app.
            //
            // Allow the user to interact with the holographic world using the mouse.

            var now = DateTime.Now;
            args.Handled = true;

            var delta = (now - lastEvent).TotalMilliseconds;
            if (delta < 100)
            {
                return;
            }

            lastEvent = now;

            if (null != main)
            {
                main.OnKeyPressed(args.VirtualKey);
            }
        }

        private void OnPointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            // Allow the user to interact with the holographic world using the mouse.
            if (null != main)
            {
                main.OnPointerPressed();
            }
        }

        #endregion

        #region Speech recognizer

        /// <summary>
        /// Uninitialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        private async Task UninitializeRecognizer()
        {
            if (speechRecognizer != null)
            {
                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    await speechRecognizer.ContinuousRecognitionSession.CancelAsync();
                }

                speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }
        }

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (speechRecognizer != null)
            {
                await UninitializeRecognizer();
            }

            try
            {
                // Initialize the SRGS-compliant XML file.
                // For more information about grammars for Windows apps and how to
                // define and use SRGS-compliant grammars in your app, see
                // https://msdn.microsoft.com/en-us/library/dn596121.aspx

                // determine the language code being used.
                var languageTag = recognizerLanguage.LanguageTag;
                var fileName = String.Format("Content\\SRGS\\{0}\\SRGS.xml", languageTag);
                var grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);

                // Initialize the SpeechRecognizer and add the grammar.
                speechRecognizer = new SpeechRecognizer(recognizerLanguage);

                // Provide feedback to the user about the state of the recognizer. This can be used to provide
                // visual feedback to help the user understand whether they're being heard.
                speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

                var grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);
                speechRecognizer.Constraints.Add(grammarConstraint);
                var compilationResult = await speechRecognizer.CompileConstraintsAsync();

                // Check to make sure that the constraints were in a proper format and the recognizer was able to compile them.
                if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                {
                    // TODO HANDLE ERROR
                }
                else
                {
                    // Set EndSilenceTimeout to give users more time to complete speaking a phrase.
                    speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(1.2);

                    // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                    // some recognized phrases occur, or the garbage rule is hit.
                    speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                    speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == HResultRecognizerNotFound)
                {
                    // TODO HANDLE ERROR
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }
        }

        #endregion

        #region Speech recognizer event handlers

        /// <summary>
        /// Handle events fired when the session ends, either from a call to
        /// CancelAsync() or StopAsync(), or an error condition, such as the 
        /// microphone becoming unavailable or some transient issues occuring.
        /// </summary>
        /// <param name="sender">The continuous recognition session</param>
        /// <param name="args">The state of the recognizer</param>
        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, 
            SpeechContinuousRecognitionCompletedEventArgs args)
        {
            // TODO HANDLE ERROR
        }

        /// <summary>
        /// Handle events fired when a result is generated. This may include a garbage rule that fires when general room noise
        /// or side-talk is captured (this will have a confidence of Rejected typically, but may occasionally match a rule with
        /// low confidence).
        /// </summary>
        /// <param name="sender">The Recognition session that generated this result</param>
        /// <param name="args">Details about the recognized speech</param>
        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, 
            SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High ||
                args.Result.Confidence == SpeechRecognitionConfidence.Low)
            {
                main.HandleVoiceCommand(args.Result.SemanticInterpretation.Properties);
            }
            else if (args.Result.Confidence == SpeechRecognitionConfidence.Rejected)
            {
                // TODO HANDLE ERROR
            }
        }

        /// <summary>
        /// Provide feedback to the user based on whether the recognizer is receiving their voice input.
        /// </summary>
        /// <param name="sender">The recognizer that is currently running.</param>
        /// <param name="args">The current state of the recognizer.</param>
        private void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            // TODO HANDLE ERROR
        }

        #endregion
    }
}
