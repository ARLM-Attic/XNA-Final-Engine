
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
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
    /// Its function is automatic and transparent to the application�s developer.
	/// </summary>
	public class InputManager
    {   

        #region KeyBoardHook

        /// <summary>
        /// Variable for keyboard hook
        /// </summary>
        private static KeyboardHook keyboardHook;

        /// <summary>
        /// Disable certain keys like win, alt-tab, etc.
        /// </summary>
        public static void EnableKeyboardHook()
        {
            if (keyboardHook == null)
                keyboardHook = new KeyboardHook();
        } // EnableKeyboardHook

        /// <summary>
        /// Disable keyboard hook, allowing again the use of certain keys.
        /// </summary>
        public static void DisableKeyboardHook()
        {
            if (keyboardHook != null)
                keyboardHook.Dispose();
        } // DisableKeyboardHook

        #endregion

        #region Init Input Devices

        /// <summary>
        /// Init Input Devices. Only Wiimotes need special operations for initialization.
        /// </summary>
        public static void InitInputDevices()
        {
            Wiimote.InitWiimotes();
        } // InitInputDevices

        #endregion

        #region DisposeInputDevices()

        /// <summary>
        /// D�spose Input Devices
        /// </summary>
        public static void DisposeInputDevices()
        {
            Wiimote.WiimotePlayerOne.Disconnect();
            Wiimote.WiimotePlayerTwo.Disconnect();
            Wiimote.WiimotePlayerThree.Disconnect();
            Wiimote.WiimotePlayerFour.Disconnect();
        } // DisposeInputDevices

        #endregion

        #region Update

        /// <summary>
        /// Update, called from EngineManager.Update().
		/// Will catch all new states for keyboard, mouse, gamepads, and Wiimotes.
		/// </summary>
		internal static void Update()
		{
            if (EngineManager.IsApplicationActive)
            {
                Mouse.Update();
                Keyboard.Update();
                XInputGamePad.XInputGamePadPlayerOne.Update();
                XInputGamePad.XInputGamePadPlayerTwo.Update();
                XInputGamePad.XInputGamePadPlayerThree.Update();
                XInputGamePad.XInputGamePadPlayerFour.Update();
                Wiimote.WiimotePlayerOne.Update();
                Wiimote.WiimotePlayerTwo.Update();
                Wiimote.WiimotePlayerThree.Update();
                Wiimote.WiimotePlayerFour.Update();
            }
		} // Update

		#endregion

    } // InputManager
} // XNAFinalEngine.Input
