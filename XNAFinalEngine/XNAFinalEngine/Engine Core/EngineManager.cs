
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Settings;
#if (!XBOX)
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    using MessageBoxIcon = System.Windows.Forms.MessageBoxIcon;
#endif
#endregion

namespace XNAFinalEngine.EngineCore
{
    /// <summary>
    /// This class administrates the XNA device and the window in which the XNA program will run. 
    /// Also it calls the Game Loop Manager and manages the exceptions.
    /// In here you can start the engine and terminate the application execution.
    /// </summary>
    public class EngineManager : Game
    {

        #region Variables

        /// <summary>
        /// Stores the exception raised when ShowExceptionsWithGuide is true.
        /// </summary>
        private static Exception exception;

        #endregion

        #region Properties

        /// <summary>
        /// Singleton reference for specific task like the exit method.
        /// </summary>
        internal static EngineManager EngineManagerReference { get; private set; }

        /// <summary>
        /// Show exceptions message with Games for Windows/XBOX Guide?
        /// </summary>
        private static bool ShowExceptionsWithGuide { get; set; }

        #endregion

        #region Constructor (Initialize XNA window)

        /// <summary>
        /// Creates the XNA window and set its main parameters.
        /// Parameters: Resolution (0 its desktop resolution), Aspect Ratio (0 its width/height),
        /// if it's fullscreen or windowed, if we can change windows size,
        /// and if it has v-sync and multisampling (O if not, 2, 4, 8 and 16 are the quality).
        /// </summary>
        public EngineManager()
        {
            if (EngineManagerReference != null)
                throw new Exception("Engine Manager: Engine already started.");
            EngineManagerReference = this;

            #region Load Settings

            // Some initial settings are loaded from an "ini" file.
            // This file could be extended or reduced if need it (look at MainSettings class).
            // Moreover, these options could be also changed in execution.
            MainSettings mainSettings;
            try
            {
                mainSettings = Content.Load<MainSettings>(ContentManager.GameDataDirectory + "MainSettings.xml");
            }
            catch
            {
                // If the file doesn't exist load default values.
                mainSettings = new MainSettings();
            }

            #endregion
            
            Window.Title = mainSettings.WindowName;
            Window.AllowUserResizing = mainSettings.ChangeWindowSize;
            
            IsMouseVisible = mainSettings.IsMouseVisible;

            // First we set GraphicsDeviceManager and latter GameWindow.
            // The order is important because we want that the System Information DeviceReset event was called before the System Information Size Changed event. Confused?
            SystemInformation.GraphicsDeviceManager = new GraphicsDeviceManager(this);
            SystemInformation.GameWindow = Window;

            #region Resolution

            // Set resolution
            int width  = mainSettings.WindowWidth;
            int height = mainSettings.WindowHeight;
            // Use current desktop resolution if autodetect is selected.
            if (width <= 0 || height <= 0)
            {
                width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth = width;
            SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight = height;

            #endregion

            // Aspect Ratio
            Screen.AspectRatio = mainSettings.AspectRatio;
            
            // Fullscreen
            SystemInformation.GraphicsDeviceManager.IsFullScreen = mainSettings.Fullscreen;

            // Multisampling
            Screen.MultiSampleQuality = mainSettings.MultiSampleQuality;
            //SystemInformation.GraphicsDeviceManager.PreferMultiSampling = mainSettings.MultiSampleQuality != 0; // Activate the antialiasing if multisamplequality is different than zero.
            // We will always use the back buffer for 2D operations, so no need to waste space and time in multisampling.
            SystemInformation.GraphicsDeviceManager.PreferMultiSampling = false;

            // VSync
            SystemInformation.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = mainSettings.VSync;

            #region Update Frequency

            // Update Frequency
            // http://blogs.msdn.com/b/shawnhar/archive/2007/07/25/understanding-gametime.aspx
            if (mainSettings.UpdateFrequency == 0)
            {
                // Update, Draw, repeat
                IsFixedTimeStep = false;
            }
            else
            {
                // We will call your Update method exactly x times per second and we will call your Draw method whenever we feel like it. Shawn says it, not me.
                IsFixedTimeStep = true;
                TargetElapsedTime = TimeSpan.FromSeconds(1.0f / mainSettings.UpdateFrequency);
            }

            #endregion

            SystemInformation.Services = Services;

        } // EngineManager

        #endregion

        #region Initialize XNA device
        
        /// <summary>
        /// Initialize.
        /// </summary>
        protected override void Initialize()
        {
            // Intercept events //
            SystemInformation.GraphicsDeviceManager.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            SystemInformation.GraphicsDeviceManager.DeviceReset += Graphics_DeviceReset;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            // Reset to take new parameters //
            GraphicsDevice.Reset(GraphicsDevice.PresentationParameters);

            // Initialize Gamer Services Dispatcher
            if (!GamerServicesDispatcher.IsInitialized)
            {
                GamerServicesDispatcher.Initialize(Services);                
            }
            GamerServicesDispatcher.WindowHandle = Window.Handle;
            // In classes that derive from Game, you need to call base.Initialize in Initialize,
            // which will automatically enumerate through any game components that have been added to Game.Components and call their Initialize methods.
            base.Initialize();
        } // Initialize

        #endregion

        #region XNA Device reset/change
        
        /// <summary>
        /// If the XNA device was detroyed (for example the user press alt-tab) chances are that the new device don’t have the correct multisample quality parameter assigned.
        /// If this happen the render targets wouldn’t work correctly.
        /// Important: we can’t do this in graphics_DeviceReset.
        /// </summary>
        private static void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // We will always use the back buffer for 2D operations, so no need to waste space in a depth buffer and multisampling.
            SystemInformation.Device.PresentationParameters.MultiSampleCount = Screen.MultiSampleQuality;
            SystemInformation.Device.PresentationParameters.DepthStencilFormat = DepthFormat.None;
        } // graphics_PreparingDeviceSettings
        
        /// <summary>
        /// Graphics device reset. This method is also call in the beginning of the execution. 
        /// </summary>
        private static void Graphics_DeviceReset(object sender, EventArgs e)
        {
            // Restore render to the frame buffer.
            RenderTarget.BackToBackBuffer();
            SystemInformation.DeviceLostInThisFrame = true;
            #if (!XBOX)
                Application.EnableVisualStyles();
                Application.VisualStyleState = VisualStyleState.ClientAndNonClientAreasEnabled;
            #endif
        } // graphics_DeviceReset

        /// <summary>
        /// Window client size changed.
        /// If this happens the master aspect ratio is always width / height.
        /// </summary>
        private static void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            // I don't want that this method is called when a device reset occurs.
            if (SystemInformation.Device.PresentationParameters.BackBufferWidth != SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth ||
                SystemInformation.Device.PresentationParameters.BackBufferHeight != SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight)
            {
                
            }
        } // Window_ClientSizeChanged
     
        #endregion

        #region On activated and on deactivated

        /// <summary>
        /// On activated.
        /// </summary>
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
            SystemInformation.IsApplicationActive = true;
        } // OnActivated

        /// <summary>
        /// On deactivated.
        /// </summary>
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            SystemInformation.IsApplicationActive = false;
        } // OnDeactivated

        #endregion

        #region Load Content

        /// <summary>
        ///  Load any necessary game assets.
        /// </summary>
        protected override void LoadContent()
        {
            if (ShowExceptionsWithGuide)
            {
                try { GameLoop.LoadContent(); }
                catch (Exception e)
                {
                    Time.PauseGame();
                    exception = e;
                }
            }
            else
            {
                GameLoop.LoadContent();
            }
        } // LoadContent

        #endregion

        #region Update
        
        /// <summary>
        /// Update
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            GamerServicesDispatcher.Update();
            if (ShowExceptionsWithGuide) // If we want to show exception in the Guide.
            {
                // If no exception was raised.
                if (exception == null)
                {
                    try { GameLoop.Update(gameTime); }
                    catch (Exception e)
                    {
                        Time.PauseGame();
                        exception = e;
                    }
                }
            }
            else // If not then the StarEngine method will managed them.
            {
                GameLoop.Update(gameTime);
            }
        } // Update

        #endregion

        #region Draw
        
        /// <summary>
        /// Draw
        /// </summary>        
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            SystemInformation.Device.Clear(ClearOptions.Target, new Color(30, 30, 40), 1.0f, 0);
            if (ShowExceptionsWithGuide) // If we want to show exception in the Guide.
            {
                if (exception == null) // If no exception was raised.
                {
                    try { GameLoop.Draw(gameTime); }
                    catch (Exception e)
                    {
                        Time.PauseGame();
                        exception = e;
                    }
                }
                else // Show exception screen
                {
                    if (!Guide.IsVisible)
                    {
                        Guide.BeginShowMessageBox("XNA Final Engine",
                                                    "There was a critical error.\n\n" + exception.Message,
                                                    new List<string> {"Try to continue", "Exit"}, 0,
                                                    Microsoft.Xna.Framework.GamerServices.MessageBoxIcon.Error,
                                                    MessageBoxExceptionEnd, null);
                    }
                }
            }
            else // If not then the StarEngine method will managed them.
            {
                GameLoop.Draw(gameTime);
            }
            SystemInformation.DeviceLostInThisFrame = false; // Always in the method’s bottom. TODO!!! Que pasa si el shader por alguna razon no se ejecuto en esa ejecucion del draw y si en la siguiente. Se necesita algo más robusto.
        } // Draw

        /// <summary>
        /// 
        /// </summary>
        private static void MessageBoxExceptionEnd(IAsyncResult result)
        {
            int? buttonPressed = Guide.EndShowMessageBox(result);
            if (buttonPressed != null && buttonPressed == 1)
                ExitApplication();
            else
            {
                exception = null;
                Time.ResumeGame();
            }
        } // MessageBoxExceptionEnd

        #endregion

        #region Exit

        /// <summary>
        /// Exit application.
        /// </summary>
        public static void ExitApplication()
        {
            EngineManagerReference.Exit();
        } // ExitApplication

        #endregion
        
        #region UnloadContent

        /// <summary>
        /// Try to dispose everything before the program terminates.
        /// </summary>
        protected override void UnloadContent()
        {
            GameLoop.UnloadContent();
            base.UnloadContent();
        } // UnloadContent

        #endregion

        #region Start Engine

        /// <summary>
        /// Gimme fuel, gimme fire, gimme that which I desire!
        /// Start the engine and the aplication's logic.
        /// But first, the initial tasks are performed and the exceptions are managed.
        /// </summary>
        public static void StarEngine()
        {
            if (Debugger.IsAttached)
            {
                // We want to know where the exceptions were raised. So it does not show any system message.
                StartupCommonCode();
            }
            else
            {
                // In release mode the user receives a system message if an exception was raised.
                try
                {
                    #if (XBOX)
                        ShowExceptionsWithGuide = true;
                    #endif
                    StartupCommonCode();
                }
                catch (Exception exception)
                {
                    #if (!XBOX)
                        MessageBox.Show("There was a critical error.\n\nDetails: " + exception.Message, "XNA Final Engine", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    #else
                        // XBOX exceptions are managed very different. They show a message via the XBOX Guide.
                        // However, when you are developing you can hide this exception messages so that Visual Studio shows you the exact line in wish the exception raised.
                        // If an exception occurs in the device creation then we cannot show any Guide message, instead the application just exits.
                        // You can throw an system Code 4 message in these cases (and in this exact place) but they are extremely rare
                        // and the user probably will never experience this kind of exceptions.
                        throw;
                    #endif
                }
            }

        } // StarEngine

        private static void StartupCommonCode()
        {
            #if (!XBOX)
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                // Handle otherwise unhandled exceptions that occur in Windows Forms threads.
                Application.ThreadException += UnhandledExceptions;
            #endif
            using (EngineManager engineManager = new EngineManager())
            {
                engineManager.Run();
            }
        } // StartupCommonCode
        
        #if (!XBOX)
            /// <summary>
            /// This method handle otherwise unhandled exceptions that occur in Windows Forms threads.
            /// In other words, the JIT debugger won’t be load.
            /// </summary>
            private static void UnhandledExceptions(object sender, ThreadExceptionEventArgs e)
            {
                MessageBox.Show("There was a critical error.\n\nDetails: " + e.Exception.Message, "XNA Final Engine", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } // UnhandledExceptions
        #endif

        #endregion

    } // EngineManager
} // XNAFinalEngine.EngineCore