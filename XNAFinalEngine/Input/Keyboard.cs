
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace XNAFinalEngine.Input
{

	/// <summary>
	/// Keyboard.
	/// </summary>
	public static class Keyboard
	{

		#region Variables
        
		// The current keyboard state.
		private static KeyboardState currentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        
        // The previous keyboard state.
        private static KeyboardState previousState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

		#endregion

        #region Properties

        /// <summary>
        /// The current keyboard state.
		/// </summary>
		public static KeyboardState State { get { return currentState; } }

        /// <summary>
        /// The previous keyboard state.
        /// </summary>
        public static KeyboardState PreviousState { get { return previousState; } }

        #endregion

        #region Key Pressed and Just Pressed

        /// <summary>
        /// Key just pressed.
        /// </summary>
        public static bool KeyJustPressed(Keys key) { return currentState.IsKeyDown(key) && !previousState.IsKeyDown(key); }

        /// <summary>
        /// Key pressed.
        /// </summary>
        public static bool KeyPressed(Keys key) { return currentState.IsKeyDown(key); }

        #endregion

        #region Is Special Key

        /// <summary>
        /// All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
        //  With shift pressed this also results in this keys:
        /// </summary>
        public static bool IsSpecialKey(Keys key)
		{
			// ~_|{}:"<>? !@#$%^&*().
			int keyNum = (int)key;
			if ((keyNum >= (int)Keys.A  && keyNum <= (int)Keys.Z)  ||
				(keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
				key == Keys.Space ||            // space
				key == Keys.OemTilde ||         // `~
				key == Keys.OemMinus ||         // -_
				key == Keys.OemPipe ||          // \|
				key == Keys.OemOpenBrackets ||  // [{
				key == Keys.OemCloseBrackets || // ]}
				key == Keys.OemSemicolon ||     // ;:
				key == Keys.OemQuotes ||        // '"
				key == Keys.OemComma ||         // ,<
				key == Keys.OemPeriod ||        // .>
				key == Keys.OemQuestion ||      // /?
				key == Keys.OemPlus)            // =+
				return false;

			// Else it is a special key.
			return true;
        } // IsSpecialKey

        #endregion

        #region Key to String

        /// <summary>
		/// Key to string helper conversion method.
		/// If the keys are mapped other than on a default QWERTY keyboard or non English distribution, this method will not work properly.
		/// Most keyboards will return the same for A-Z and 0-9, but the special keys might be different.
		/// </summary>
        public static string KeyToString(Keys key, bool shift, bool caps)
        {
            bool uppercase = (caps && !shift) || (!caps && shift);

            int keyNum = (int) key;
            if (keyNum >= (int) Keys.A && keyNum <= (int) Keys.Z)
            {
                if (uppercase) 
                    return key.ToString();
                else
                    return key.ToString().ToLower();
            }
            switch (key)
            {
                case Keys.Space: return " ";
                case Keys.D1: return shift ? "!" : "1";
                case Keys.D2: return shift ? "@" : "2";
                case Keys.D3: return shift ? "#" : "3";
                case Keys.D4: return shift ? "$" : "4";
                case Keys.D5: return shift ? "%" : "5";
                case Keys.D6: return shift ? "^" : "6";
                case Keys.D7: return shift ? "&" : "7";
                case Keys.D8: return shift ? "*" : "8";
                case Keys.D9: return shift ? "(" : "9";
                case Keys.D0: return shift ? ")" : "0";
                case Keys.OemTilde:         return shift ? "~" : "`";
                case Keys.OemMinus:         return shift ? "_" : "-";
                case Keys.OemPipe:          return shift ? "|" : "\\";
                case Keys.OemOpenBrackets:  return shift ? "{" : "[";
                case Keys.OemCloseBrackets: return shift ? "}" : "]";
                case Keys.OemSemicolon:     return shift ? ":" : ";";
                case Keys.OemQuotes:        return shift ? "\"" : "\\";
                case Keys.OemComma:         return shift ? "<" : ".";
                case Keys.OemPeriod:        return shift ? ">" : ",";
                case Keys.OemQuestion:      return shift ? "?" : "/";
                case Keys.OemPlus: return shift ? "+" : "=";
                case Keys.NumPad0: return shift ? "" : "0";
                case Keys.NumPad1: return shift ? "" : "1";
                case Keys.NumPad2: return shift ? "" : "2";
                case Keys.NumPad3: return shift ? "" : "3";
                case Keys.NumPad4: return shift ? "" : "4";
                case Keys.NumPad5: return shift ? "" : "5";
                case Keys.NumPad6: return shift ? "" : "6";
                case Keys.NumPad7: return shift ? "" : "7";
                case Keys.NumPad8: return shift ? "" : "8";
                case Keys.NumPad9: return shift ? "" : "9";
                case Keys.Divide:   return "/";
                case Keys.Multiply: return "*";
                case Keys.Subtract: return  "-";
                case Keys.Add:      return "+";
                default: return "";
            }
		} // KeyToString

        #endregion
        
        #region Update
        
        /// <summary>
		/// Update keyboard.
		/// </summary>
		internal static void Update()
		{
            previousState = currentState;
            currentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();            
		} // Update
		
        #endregion

    } // Keyboard
} // XNAFinalEngine.Input
