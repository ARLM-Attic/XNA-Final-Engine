
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using XNAFinalEngine.GraphicElements;
using XNAFinalEngine.Setting;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Sounds;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.EngineCore
{
    /// <summary>
    /// This class administrates the XNA device and the window in which the XNA program will run.
    /// Here will we create, update, render, reset and dispose the XNA window.
    /// We can also obtain information about time and frames per second.
    /// All the device creation parameters can only be assigned at the beginning of the execution through the settings file.
    /// There are other functions: clear the frame buffer, toggle fullscreen, and the exit command.
    /// </summary>
    public class EngineManager : Microsoft.Xna.Framework.Game
    {

        #region Variables
        
        /// <summary>
        /// Multi Sample Quality. 0X 2X 4X 8X or 16X. This don't match the XNA's term called multiSampleQuality.
        /// </summary>
        private static int multiSampleQuality;

        /// <summary>
        /// The window that supports the engine.
        /// </summary>
        private static GameWindow window;

        /// <summary>
        /// Uses system content?
        /// </summary>
        private static bool usesSystemContent = true;

        #region Time

        /// <summary>
        /// Elapsed time of this frame (in ms). Always have something valid here in case we devide through this values!
        /// </summary>
        private static double frameTimeInMs = 0.0001f;

        /// <summary>
        /// Total time of the application (in ms).
        /// </summary>
        private static double totalTimeMs = 0;

        /// <summary>
        /// Total time of the application in the previous frame (in ms).
        /// </summary>
        private static double lastFrameTotalTimeMs = 0;

        /// <summary>
        /// Helper for calculating frames per second.
        /// </summary>
        private static double startTimeThisSecond = 0;

        /// <summary>
        /// For more accurate frames per second calculations, just count for one second, then fpsLastSecond is updated.
        /// Start with 1 to help some tests avoid the devide through zero problem.
        /// </summary>
        private static int frameCountThisSecond = 0,
                           totalFrameCount = 0,
                           fpsLastSecond = 1;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Screenshot Capturer (Print Screen).
        /// </summary>
        public static ScreenshotCapturer ScreenshotCapturer { get; private set; }

        /// <summary>
        /// Exit the application? Exit takes action in the beggining of the update.
        /// </summary>
        public static new bool Exit { get; set; } // Override base.Exit(); // I don't want that the application exit when the engine wants, first it needs to finish the frame.

        /// <summary>
        /// Window resolution width.
        /// </summary>        
        public static int Width { get; private set; }

        /// <summary>
        /// Window resolution height.
        /// </summary>        
        public static int Height { get; private set; }

        /// <summary>
        /// Master Aspect Ratio (a camera can have a different aspect ratio).
        /// </summary>
        public static float AspectRatio { get; private set; }

        /// <summary>
        /// XNA graphics device manager.
        /// </summary>
        public static GraphicsDeviceManager GraphicsManager { get; private set; }

        /// <summary>
        /// XNA Graphic Device.
        /// </summary>
        public static GraphicsDevice Device { get; private set; }

        /// <summary>
        /// Uses system content manager? Or uses a custom content manager?
        /// </summary>
        public static bool UsesSystemContent { get { return usesSystemContent; } set { usesSystemContent = value;  } }

        /// <summary>
        /// Current content manager. It will be used if UsesSystemContent is false.
        /// </summary>
        public static ContentManager CurrentContent { get; set; }

        /// <summary>
        /// System content manager.
        /// </summary>
        public static ContentManager SystemContent { get; private set; }

        /// <summary>
        /// XNA Services.
        /// </summary>
        public static new GameServiceContainer Services { get; private set; }

        /// <summary>
        /// XNA Multi Sample Quality.
        /// </summary>
        public static int MultiSampleQuality { get { return Device.PresentationParameters.MultiSampleCount; } }

        /// <summary>
        /// Is application currently active?
        /// </summary>
        public static bool IsApplicationActive { get; private set; }

        /// <summary>
        /// XNA Game Time.
        /// </summary>
        public static GameTime GameTime { get; private set; }

        /// <summary>
        /// Show frames per second in the top corner of the screen.
        /// </summary>
        public static bool ShowFPS { get; set; }

        #region Frames per second

        /// <summary>
        /// Frames per second.
        /// </summary>
        public static int Fps { get { return fpsLastSecond; } }

        /// <summary>
        /// Total frames count.
        /// </summary>
        public static int TotalFramesCount { get { return totalFrameCount; } }

        #endregion

        #region Time

        /// <summary>
        /// Elapsed time of this frame (in seconds)
        /// </summary>
        public static double FrameTime { get { return frameTimeInMs / 1000.0f; } }

        /// <summary>
        /// Elapsed time of this frame (in ms).
        /// </summary>
        public static double FrameTimeInMilliseconds { get { return frameTimeInMs; } }

        /// <summary>
        /// Total time of the application (in seconds).
        /// </summary>
        public static double TotalTime { get { return totalTimeMs / 1000.0f; } }

        /// <summary>
        /// Total time of the application (in ms).
        /// </summary>
        public static double TotalTimeMilliseconds { get { return totalTimeMs; } }

        #endregion

        #endregion

        #region Constructor (Initialize XNA window)

        /// <summary>
        /// Creates the XNA window and set its main parameters.
        /// Parameters: Resolution (0 its desktop resolution), Aspect Ratio (0 its width/height),
        /// if it's fullscreen or windowed, if we can change windows size,
        /// and if it has v-sync and multisampling (O if not, 2, 4, 8 and 16 are the quality).
        /// </summary>
        public EngineManager(string _title, Size _resolution, float _aspectRatio, bool _fullscreen, bool _changeWindowSize, bool _vsync, int framePerSeconds, int _multiSampleQuality)
        {            
            // Set window title
            Window.Title = _title;
            Window.AllowUserResizing = _changeWindowSize;
            window = Window;

            // Set graphics
            GraphicsManager = new GraphicsDeviceManager(this);
                        
            // Set resolution
            Width = _resolution.Width;
            Height = _resolution.Height;
            // Use current desktop resolution if autodetect is selected.
            if (Width <= 0 || Height <= 0)
            {
                Width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            GraphicsManager.PreferredBackBufferWidth = Width;
            GraphicsManager.PreferredBackBufferHeight = Height;

            // Aspect Ratio
            if (_aspectRatio == 0)
                AspectRatio = (float)Width / (float)Height;
            else
                AspectRatio = _aspectRatio;
            
            // If fullscreen
            GraphicsManager.IsFullScreen = _fullscreen;

            // Multisampling
            multiSampleQuality = _multiSampleQuality;
            GraphicsManager.PreferMultiSampling = _multiSampleQuality != 0; // Activate the antialiasing if multisamplequality is different than zero.

            // VSync
            GraphicsManager.SynchronizeWithVerticalRetrace = _vsync;

            // Frame per seconds
            if (framePerSeconds == 0)
            {
                // Se actualiza ni bien puede. No a intervalos definidos de tiempo.
                this.IsFixedTimeStep = false;
            }
            else
            {
                this.IsFixedTimeStep = true;
                this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / framePerSeconds);
            }

            // System content manager
            SystemContent = base.Content;

            // Services
            Services = base.Services;

        } // EngineManager

        /// <summary>
        /// Creates the XNA window and set its main parameters using the settings file 
        /// </summary>
        public EngineManager() : this(Settings.Settings.Default.WindowName, new Size(Settings.Settings.Default.ResolutionWidth, Settings.Settings.Default.ResolutionHeight),
                                      Settings.Settings.Default.AspectRatio, Settings.Settings.Default.Fullscreen, Settings.Settings.Default.ChangeWindowSize,
                                      Settings.Settings.Default.VSync, Settings.Settings.Default.FramePerSeconds, Settings.Settings.Default.MultiSampleQuality)
        {
        } // EngineManager

        #endregion

        #region Initialize XNA device
        
        /// <summary>
        /// Creates the XNA device for the XNA windows and set its main parameters.
        /// It also load any common non-graphics resources. 
        /// </summary>
        protected override void Initialize()
        {
            // Set device
            Device = GraphicsManager.GraphicsDevice;

            Device.PresentationParameters.MultiSampleCount = multiSampleQuality;
            
            GraphicsManager.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            GraphicsManager.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            Device.Reset(GraphicsDevice.PresentationParameters);

            GraphicsManager.ApplyChanges();

            base.Initialize();
        } // Initialize

        #endregion

        #region XNA Device reset/change
        
        /// <summary>
        /// If the XNA device was detroyed (for example the user press alt-tab) chances are that the new device don’t have the correct multisample quality parameter assingned.
        /// If this happen the render targets wouldn’t work correctly.
        /// Important: we can’t do this in graphics_DeviceReset.
        /// </summary>
        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            Device.PresentationParameters.MultiSampleCount = Settings.Settings.Default.MultiSampleQuality;
        } // graphics_PreparingDeviceSettings
        
        /// <summary>
        /// Graphics device reset. This method is also call in the beginning of the execution. 
        /// </summary>
        private void graphics_DeviceReset(object sender, EventArgs e)
        {
            Device = GraphicsManager.GraphicsDevice;
            // Restore render to the frame buffer.
            RenderToTexture.BackToSceneRenderTarget();
            // Dispose the current sprite mananger
            SpriteManager.ClearListSprites();
        } // graphics_DeviceReset

        /// <summary>
        /// Window client size changed.
        /// If this happens the master aspect ratio is always width / height.
        /// </summary>
        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            // Update width and height
            Width = Device.Viewport.Width;
            Height = Device.Viewport.Height;
            AspectRatio = (float)Width / (float)Height;
            // Camera.BuildProjectionMatrix(); // TODO // Necesitamso listas de camaras y una variable relacion de aspecto con 0 representando la relacion de aspecto del sistema.
        } // Window_ClientSizeChanged
     
        #endregion

        #region Load Content

        /// <summary>
        ///  Load any necessary game assets.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Disable certain keys like win, alt-tab, etc.
            InputManager.EnableKeyboardHook();

            InputManager.InitInputDevices();

            MusicManager.InitMusicManager();

            ScreenshotCapturer = new ScreenshotCapturer();

            // Init the aplication logic. In other words, the main scene.
            ApplicationLogic.CreateScene();
        } // LoadContent

        #endregion

        #region Toggle Fullscreen

        /// <summary>
        /// Toggle Fullscreen to windomed.
        /// </summary>
        public static void ToggleFullscreen()
        {   
            GraphicsManager.IsFullScreen = !GraphicsManager.IsFullScreen;
            window.BeginScreenDeviceChange(GraphicsManager.IsFullScreen);
            Device.Reset();
            window.EndScreenDeviceChange(window.ScreenDeviceName);
        } // ToggleFullscreen

        #endregion

        #region On activated and on deactivated

        /// <summary>
        /// On activated
        /// </summary>
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
            IsApplicationActive = true;
        } // OnActivated

        /// <summary>
        /// On deactivated
        /// </summary>
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            IsApplicationActive = false;
        } // OnDeactivated

        #endregion

        #region Update

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime _gameTime)
        {
            if (Exit)
            {
                base.Exit();
            }
            else
            {

                #region Time

                GameTime = _gameTime;

                lastFrameTotalTimeMs = totalTimeMs;
                frameTimeInMs = GameTime.ElapsedGameTime.TotalMilliseconds;
                totalTimeMs = GameTime.TotalGameTime.TotalMilliseconds;

                // Make sure elapsedTimeThisFrameInMs is never 0
                if (frameTimeInMs <= 0)
                    frameTimeInMs = 1;

                // Increase frame counter for FramesPerSecond
                frameCountThisSecond++;
                totalFrameCount++;

                // One second elapsed?
                if (totalTimeMs - startTimeThisSecond > 1000)
                {
                    // Calc fps
                    fpsLastSecond = (int)(frameCountThisSecond * 1000.0f / (totalTimeMs - startTimeThisSecond));
                    // Reset startSecondTick and repaintCountSecond
                    startTimeThisSecond = totalTimeMs;
                    frameCountThisSecond = 0;
                }

                #endregion

                InputManager.Update();

                Animation.UpdateAnimations();

                Chronometer.UpdateAllChronometers();

                SoundManager.UpdateSound();

                MusicManager.Update();

                // Application update //
                ApplicationLogic.Update();
            }
            base.Update(_gameTime);
        } // Update

        #endregion

        #region Draw

        /// <summary>
        /// Draw
        /// </summary>        
        protected override void Draw(GameTime gameTime)
        {
            if (ScreenshotCapturer.NeedToMakeScreenshot)
                ScreenshotCapturer.BeginScreenshot();
            // Clear frame buffer and depth buffer
            Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Microsoft.Xna.Framework.Color(10, 10, 10), 1.0f, 0);
            // Handle custom user render code
            ApplicationLogic.Render();
            // Show frame per seconds if is wanted.
            if (ShowFPS)
            {
                FontArial14.Render("FPS  " + EngineManager.Fps.ToString(),
                                                new Vector2(EngineManager.Width - 130, 10),
                                                Microsoft.Xna.Framework.Color.Yellow, true, Microsoft.Xna.Framework.Color.Black);
            }
            // Render the sprites
            SpriteManager.DrawSprites();

            if (ScreenshotCapturer.NeedToMakeScreenshot)
                ScreenshotCapturer.EndScreenshot();

            base.Draw(gameTime);
        } // Draw

        #endregion

        #region UnloadContent

        /// <summary>
        /// Try to dispose everything before the program terminates.
        /// </summary>
        protected override void UnloadContent()
        {
            ApplicationLogic.UnloadContent();
            InputManager.DisableKeyboardHook();
            InputManager.DisposeInputDevices();
            base.UnloadContent();
        } // UnloadContent

        #endregion

        #region Clear Buffers

        /// <summary>
        /// Clear the depth buffer
        /// </summary>
        public static void ClearDepthBuffer()
        {
            Device.Clear(ClearOptions.DepthBuffer, Microsoft.Xna.Framework.Color.Black, 1.0f, 0);
        }

        /// <summary>
        /// Clear the frame buffer (or render target) and depth buffer.
        /// </summary>
        public static void ClearTargetAndDepthBuffer(Microsoft.Xna.Framework.Color color)
        {
            EngineManager.Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, color, 1.0f, 0);
        }

        #endregion

    } // EngineManager
} // XNAFinalEngine.EngineCore