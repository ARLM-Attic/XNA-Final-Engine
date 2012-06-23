
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
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Input
{

	/// <summary>
    /// Manager for input devices.
	/// </summary>
	internal class InputManager
    {   

        #region KeyBoard Hook

        #if (!XBOX)
            /// <summary>
            /// Keyboard hook singleton reference.
            /// </summary>
            private static KeyboardHook keyboardHook;
        #endif

        /// <summary>
        /// Disable certain keys like win, alt-tab, etc.
        /// </summary>
        public static void EnableKeyboardHook()
        {
            #if (!XBOX)
                if (keyboardHook == null)
                    keyboardHook = new KeyboardHook();
            #endif
        } // EnableKeyboardHook

        /// <summary>
        /// Disable keyboard hook, allowing again the use of certain keys.
        /// </summary>
        public static void DisableKeyboardHook()
        {
            #if (!XBOX)
                if (keyboardHook != null)
                    keyboardHook.Dispose();
            #endif
        } // DisableKeyboardHook

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize Input Devices.
        /// </summary>
        internal static void Initialize()
        {
            #if (!XBOX)
                if (keyboardHook == null)
                    keyboardHook = new KeyboardHook();
            #endif
            //Wiimote.InitWiimotes();
        } // Initialize

        #endregion

        #region Unload

        /// <summary>
        /// Unload Input Devices.
        /// </summary>
        public static void UnloadInputDevices()
        {
            #if (!XBOX)
                if (keyboardHook != null)
                    keyboardHook.Dispose();
            #endif
            /*Wiimote.PlayerOne.Disconnect();
            Wiimote.PlayerTwo.Disconnect();
            Wiimote.PlayerThree.Disconnect();
            Wiimote.PlayerFour.Disconnect();*/
        } // UnloadInputDevices

        #endregion

        #region Update

        /// <summary>
        ///  Will catch all new states for keyboard, mouse, gamepads, and Wiimotes.
		/// </summary>
		internal static void Update()
		{
            if (EngineManager.IsApplicationActive)
            {
                GamePad.PlayerOne.Update();
                GamePad.PlayerTwo.Update();
                GamePad.PlayerThree.Update();
                GamePad.PlayerFour.Update();
                Keyboard.Update();
                #if (!XBOX)
                    Mouse.Update();
                    /*Wiimote.PlayerOne.Update();
                    Wiimote.PlayerTwo.Update();
                    Wiimote.PlayerThree.Update();
                    Wiimote.PlayerFour.Update();*/
                #endif
            }
		} // Update

		#endregion

    } // InputManager
} // XNAFinalEngine.Input
