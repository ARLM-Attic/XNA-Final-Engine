
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine
{

    /// <summary>
    /// Here there are important references to XNA configuration and management systems.
    /// This information is used by the engine’s manager systems.
    /// However, if you need some functionally that is not present in the engine you can use it.
    /// </summary>
    public static class SystemInformation
    {

        #region Variables

        /// <summary>
        /// XNA graphics device manager.
        /// </summary>
        private static GraphicsDeviceManager graphicsDeviceManager;

        /// <summary>
        /// Game Window.
        /// </summary>
        private static GameWindow gameWindow;

        #endregion

        #region Properties

        /// <summary>
        /// XNA 4.0 reconstructs automatically the render targets when a device is lost.
        /// However the shaders have to re set to the GPU the new render targets to work properly.
        /// This problem seems to manifest only with floating point formats.
        /// </summary>
        public static bool DeviceLostInThisFrame { get; set; }

        /// <summary>
        /// Is application currently active (focused)?
        /// Some operations like input reading could be disabled.
        /// </summary>
        public static bool IsApplicationActive { get; set; }

        /// <summary>
        /// XNA graphic device.
        /// </summary>
        public static GraphicsDevice Device { get; private set; }

        /// <summary>
        /// XNA graphics device manager.
        /// </summary>
        public static GraphicsDeviceManager GraphicsDeviceManager
        {
            get { return graphicsDeviceManager; }
            set
            {
                graphicsDeviceManager = value;
                graphicsDeviceManager.DeviceReset += OnDeviceReset;
            }
        } // GraphicsDeviceManager

        /// <summary>
        /// Game Window.
        /// </summary>
        public static GameWindow GameWindow
        {
            get { return gameWindow; }
            set
            {
                gameWindow = value;
                gameWindow.ClientSizeChanged += OnSizeChanged;
            }
        } // GameWindow

        /// <summary>
        /// Services.
        /// </summary>
        public static GameServiceContainer Services { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the window size changes.
        /// </summary>
        public static event EventHandler WindowSizeChanged;

        /// <summary>
        /// Raised when the device is reset.
        /// </summary>
        public static event EventHandler DeviceReset;

        #endregion

        #region  On Size Changed

        /// <summary>
        /// Raised when the window size changes.
        /// </summary>
        private static void OnSizeChanged(object sender, EventArgs e)
        {
            // I don't want that this method is called when a device reset occurs.
            if (Device.PresentationParameters.BackBufferWidth != GraphicsDeviceManager.PreferredBackBufferWidth ||
                Device.PresentationParameters.BackBufferHeight != GraphicsDeviceManager.PreferredBackBufferHeight)
            {
                if (WindowSizeChanged != null)
                    WindowSizeChanged(sender, e);
            }
        } // OnClientSizeChanged

        #endregion

        #region On Device Reset

        /// <summary>
        /// Raised when the device is reset.
        /// </summary>
        private static void OnDeviceReset(object sender, EventArgs e)
        {
            Device = graphicsDeviceManager.GraphicsDevice;
            if (DeviceReset != null)
                DeviceReset(sender, e);
        } // Graphics_DeviceReset

        #endregion

    } // SystemInformation
}  // XNAFinalEngine
