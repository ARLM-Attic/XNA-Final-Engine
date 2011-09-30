
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
using XNAFinalEngine.Helpers;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// Screen class can be used to get and set information about the screen and window.
    /// </summary>
    public static class Screen
    {

        #region Variables
        
        /// <summary>
        /// Changing the multi sample quality requires a device reset and the setting of this new value
        /// has to be done in a specific place, the Graphics_PreparingDeviceSettings method.
        /// </summary>
        private static int multiSampleQuality;

        /// <summary>
        /// Aspect Ratio.
        /// </summary>
        private static float aspectRatio;

        #endregion

        #region Properties

        /// <summary>
        /// Window Title.
        /// </summary>
        public static string WindowTitle
        {
            get { return SystemInformation.GameWindow.Title; }
            set { SystemInformation.GameWindow.Title = value; }
        } // WindowTitle

        /// <summary>
        /// Gets or sets a value indicating whether the mouse cursor should be visible.
        /// </summary>
        public static bool IsMouseVisible
        {
            get { return EngineManager.EngineManagerReference.IsMouseVisible; }
            set { EngineManager.EngineManagerReference.IsMouseVisible = value; }
        } // IsMouseVisible

        /// <summary>
        /// Fullscreen mode enabled?
        /// </summary>
        public static bool Fullscreen
        {
            get { return SystemInformation.GraphicsDeviceManager.IsFullScreen; }
            set
            {
                SystemInformation.GraphicsDeviceManager.IsFullScreen = value;
                SystemInformation.GraphicsDeviceManager.ApplyChanges();
            }
        } // Fullscreen

        /// <summary>
        /// VSync enabled?
        /// </summary>
        public static bool VSync
        {
            get { return SystemInformation.GraphicsDeviceManager.SynchronizeWithVerticalRetrace; }
            set
            {
                SystemInformation.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = value;
                SystemInformation.GraphicsDeviceManager.ApplyChanges();
            }
        } // Fullscreen

        /// <summary>
        /// Enables the option to resize the application window.
        /// </summary>
        public static bool ChangeWindowSize
        {
            get { return SystemInformation.GameWindow.AllowUserResizing; }
            set { SystemInformation.GameWindow.AllowUserResizing = value; }
        } // ChangeWindowSize

        /// <summary>
        /// System Multi Sample Quality.
        /// Because the back buffer will be used only for 2D operations this value won’t be affect the back buffer.
        /// It's the level of multisampling, in this case 4 means 4X, and 0 means no multisampling.
        /// </summary>
        public static int MultiSampleQuality
        {
            get { return multiSampleQuality; }
            set
            {
                multiSampleQuality = value;
                if (SystemInformation.Device != null)
                    SystemInformation.Device.PresentationParameters.MultiSampleCount = value;
                //SystemInformation.GraphicsDeviceManager.PreferMultiSampling = multiSampleQuality != 0; // Activate the antialiasing if multisamplequality is different than zero.
                //SystemInformation.GraphicsDeviceManager.ApplyChanges();
            }
        } // MultiSampleQuality

        #region Resolution

        /// <summary>
        /// Screen width.
        /// </summary>        
        public static int Width { get { return SystemInformation.Device.PresentationParameters.BackBufferWidth; } }

        /// <summary>
        /// Screen height.
        /// </summary>        
        public static int Height { get { return SystemInformation.Device.PresentationParameters.BackBufferHeight; } }

        /// <summary>
        /// Screen resolution.
        /// </summary>
        public static Size Resolution
        {
            get { return new Size(Width, Height); }
            set
            {
                // Use current desktop resolution if autodetect is selected.
                if (value.Width <= 0 || value.Height <= 0)
                {
                    SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                }
                else
                {
                    SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth = value.Width;
                    SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight = value.Height;
                }
                SystemInformation.GraphicsDeviceManager.ApplyChanges();
            }
        } // Resolution

        #endregion

        /// <summary>
        /// This is the default aspect ratio for cameras. 
        /// Some cameras could ask to work only in the default aspect ratio, even if the default aspect ratio changes.
        /// </summary>
        public static float AspectRatio
        {
            get
            {
                if (aspectRatio == 0)
                    return (float)Width / Height;
                return aspectRatio;
            }
            set { aspectRatio = value; }
        } // AspectRatio
                
        #endregion

        #region Toggle Fullscreen

        /// <summary>
        /// Toggles between full screen and windowed mode.
        /// </summary>
        public static void ToggleFullscreen()
        {
            SystemInformation.GraphicsDeviceManager.ToggleFullScreen();
        } // ToggleFullscreen

        #endregion

    } // Screen
} // XNAFinalEngine.EngineCore