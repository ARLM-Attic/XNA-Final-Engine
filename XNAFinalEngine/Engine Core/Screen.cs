
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// Used to get and set information about the screen and window.
    /// </summary>
    public static class Screen
    {

        #region Variables

        // Aspect Ratio.
        private static float aspectRatio;

        #endregion

        #region Properties

        #region Window Title

        /// <summary>
        /// Window Title.
        /// </summary>
        public static string WindowTitle
        {
            get { return EngineManager.GameWindow.Title; }
            set { EngineManager.GameWindow.Title = value; }
        } // WindowTitle

        #endregion

        #region Is Mouse Visible

        /// <summary>
        /// Gets or sets a value indicating whether the mouse cursor should be visible.
        /// </summary>
        public static bool IsMouseVisible
        {
            get { return EngineManager.EngineManagerReference.IsMouseVisible; }
            set { EngineManager.EngineManagerReference.IsMouseVisible = value; }
        } // IsMouseVisible

        #endregion

        #region Fullscreen

        /// <summary>
        /// Fullscreen mode enabled?
        /// </summary>
        public static bool Fullscreen
        {
            get { return EngineManager.GraphicsDeviceManager.IsFullScreen; }
            set
            {
                if (EngineManager.GraphicsDeviceManager.IsFullScreen != value)
                {
                    EngineManager.GraphicsDeviceManager.IsFullScreen = value;
                    EngineManager.ResetDevice = true;
                }
            }
        } // Fullscreen

        #endregion

        #region VSync

        /// <summary>
        /// VSync enabled?
        /// </summary>
        public static bool VSync
        {
            get { return EngineManager.GraphicsDeviceManager.SynchronizeWithVerticalRetrace; }
            set
            {
                if (EngineManager.GraphicsDeviceManager.SynchronizeWithVerticalRetrace != value)
                {
                    EngineManager.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = value;
                    EngineManager.ResetDevice = true;
                }
            }
        } // VSync

        #endregion

        #region Allow Resizing

        /// <summary>
        /// Enables the option to resize the application window using the mouse.
        /// </summary>
        public static bool AllowResizing
        {
            get { return EngineManager.GameWindow.AllowUserResizing; }
            set { EngineManager.GameWindow.AllowUserResizing = value; }
        } // AllowResizing

        #endregion

        #region Multi Sample Quality

        /// <summary>
        /// System Multi Sample Quality.
        /// Because the back buffer will be used only for 2D operations this value won’t be affect the back buffer.
        /// It's the level of multisampling, in this case 4 means 4X, and 0 means no multisampling.
        /// </summary>
        public static int MultiSampleQuality { get; set; }

        #endregion

        #region Resolution

        /// <summary>
        /// Screen width.
        /// </summary>        
        public static int Width { get { return EngineManager.Device.PresentationParameters.BackBufferWidth; } }

        /// <summary>
        /// Screen height.
        /// </summary>        
        public static int Height { get { return EngineManager.Device.PresentationParameters.BackBufferHeight; } }

        /// <summary>
        /// Screen resolution.
        /// </summary>
        public static Size Resolution
        {
            get { return new Size(Width, Height); }
            set
            {
                if (value.Width == Width && value.Height == Height)
                    return;
                if (value.Width > 0 && value.Height > 0)
                {
                    EngineManager.GraphicsDeviceManager.PreferredBackBufferWidth  = value.Width;
                    EngineManager.GraphicsDeviceManager.PreferredBackBufferHeight = value.Height;
                    EngineManager.ResetDevice = true;
                }
            }
        } // Resolution

        #endregion
        
        #endregion

        #region Toggle Fullscreen

        /// <summary>
        /// Toggles between full screen and windowed mode.
        /// </summary>
        public static void ToggleFullscreen()
        {
            EngineManager.ToggleFullScreen();
        } // ToggleFullscreen

        #endregion

        #region Events

        /// <summary>
        /// Raised when the window size changes.
        /// </summary>
        public static event EventHandler ScreenSizeChanged;

        #endregion

        #region On Screen Size Changed

        /// <summary>
        /// Raised when the window size changes.
        /// </summary>
        internal static void OnScreenSizeChanged(object sender, EventArgs e)
        {
            if (ScreenSizeChanged != null)
                ScreenSizeChanged(sender, EventArgs.Empty);
        } // OnScreenSizeChanged

        #endregion

    } // Screen
} // XNAFinalEngine.EngineCore