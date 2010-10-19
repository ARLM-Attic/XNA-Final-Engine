
#region License
/*

 Based in the class Input.cs from RacingGame.
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Input
{

	/// <summary>
	/// Keyboard.
	/// </summary>
	public class Keyboard
	{

		#region Variables
        		
		/// <summary>
		/// The current keyboard state.
		/// We can NOT use the last state because everytime we call Keyboard.GetState() the old state is useless (see XNA help for more information, section Input). 
        /// We store our own array of keys from the last frame for comparing stuff.
		/// </summary>
		private static KeyboardState keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

		/// <summary>
		/// Keys pressed last frame, for comparison if a key was just pressed.
		/// </summary>
		private static List<Keys> keysPressedLastFrame = new List<Keys>();

		#endregion

        #region Properties

        /// <summary>
        /// The current keyboard state.
		/// </summary>
		public static KeyboardState KeyboardState
		{
			get { return keyboardState; }            
        } // KeyboardState

        /// <summary>
        /// Key just pressed
        /// </summary>
        public static bool KeyJustPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key) && keysPressedLastFrame.Contains(key) == false;
        } // KeyJustPressed

        /// <summary>
        /// Key pressed
        /// </summary>
        public static bool KeyPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        } // KeyPressed

        #region Pressed or Just Pressed

        /// <summary>
        /// Space just pressed
        /// </summary>
        public static bool SpaceJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Space) && keysPressedLastFrame.Contains(Keys.Space) == false;
            } // get
        } // SpaceJustPressed

        /// <summary>
        /// Enter just pressed
        /// </summary>
        public static bool EnterJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Enter) && keysPressedLastFrame.Contains(Keys.Enter) == false;
            } // get
        } // EnterJustPressed

        /// <summary>
        /// Escape just pressed
        /// </summary>
        public static bool EscapeJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Escape) && keysPressedLastFrame.Contains(Keys.Escape) == false;
            } // get
        } // EscapeJustPressed

        #region Cursors

        /// <summary>
        /// Left just pressed
        /// </summary>
        public static bool LeftJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Left) && keysPressedLastFrame.Contains(Keys.Left) == false;
            } // get
        } // LeftJustPressed

        /// <summary>
        /// Right just pressed
        /// </summary>
        public static bool RightJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Right) && keysPressedLastFrame.Contains(Keys.Right) == false;
            } // get
        } // RightJustPressed

        /// <summary>
        /// Up just pressed
        /// </summary>
        public static bool UpJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Up) && keysPressedLastFrame.Contains(Keys.Up) == false;
            } // get
        } // UpJustPressed

        /// <summary>
        /// Down just pressed
        /// </summary>
        public static bool DownJustPressed
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Down) && keysPressedLastFrame.Contains(Keys.Down) == false;
            } // get
        } // DownJustPressed

        /// <summary>
        /// Left pressed
        /// </summary>
        public static bool LeftPressed { get { return keyboardState.IsKeyDown(Keys.Left); } }

        /// <summary>
        /// Right pressed
        /// </summary>
        public static bool RightPressed { get { return keyboardState.IsKeyDown(Keys.Right); } }

        /// <summary>
        /// Up pressed
        /// </summary>
        public static bool UpPressed { get { return keyboardState.IsKeyDown(Keys.Up); } }

        /// <summary>
        /// Down pressed
        /// </summary>
        public static bool DownPressed { get { return keyboardState.IsKeyDown(Keys.Down); } }

        #endregion

        #endregion

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
			if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) ||
				(keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
				key == Keys.Space || // well, space ^^
				key == Keys.OemTilde || // `~
				key == Keys.OemMinus || // -_
				key == Keys.OemPipe || // \|
				key == Keys.OemOpenBrackets || // [{
				key == Keys.OemCloseBrackets || // ]}
				key == Keys.OemSemicolon || // ;:
				key == Keys.OemQuotes || // '"
				key == Keys.OemComma || // ,<
				key == Keys.OemPeriod || // .>
				key == Keys.OemQuestion || // /?
				key == Keys.OemPlus) // =+
				return false;

			// Else is is a special key
			return true;
        } // IsSpecialKey

        #endregion

        #region Key to Char

        /// <summary>
		/// Key to char helper conversion method.
		/// Note: If the keys are mapped other than on a default QWERTY
		/// keyboard, this method will not work properly. Most keyboards
		/// will return the same for A-Z and 0-9, but the special keys
		/// might be different. Sorry, no easy way to fix this with XNA ...
		/// For a game with chat (windows) you should implement the
		/// windows events for catching keyboard input, which are much better!
		/// </summary>
		public static char KeyToChar(Keys key, bool shiftPressed)
		{
			// If key will not be found, just return space
			char ret = ' ';
			int keyNum = (int)key;
			if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
			{
				if (shiftPressed)
					ret = key.ToString()[0];
				else
					ret = key.ToString().ToLower()[0];
			} // if (keyNum)
			else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 &&
				shiftPressed == false)
			{
				ret = (char)((int)'0' + (keyNum - Keys.D0));
			} // else if
			else if (key == Keys.D1 && shiftPressed)
				ret = '!';
			else if (key == Keys.D2 && shiftPressed)
				ret = '@';
			else if (key == Keys.D3 && shiftPressed)
				ret = '#';
			else if (key == Keys.D4 && shiftPressed)
				ret = '$';
			else if (key == Keys.D5 && shiftPressed)
				ret = '%';
			else if (key == Keys.D6 && shiftPressed)
				ret = '^';
			else if (key == Keys.D7 && shiftPressed)
				ret = '&';
			else if (key == Keys.D8 && shiftPressed)
				ret = '*';
			else if (key == Keys.D9 && shiftPressed)
				ret = '(';
			else if (key == Keys.D0 && shiftPressed)
				ret = ')';
			else if (key == Keys.OemTilde)
				ret = shiftPressed ? '~' : '`';
			else if (key == Keys.OemMinus)
				ret = shiftPressed ? '_' : '-';
			else if (key == Keys.OemPipe)
				ret = shiftPressed ? '|' : '\\';
			else if (key == Keys.OemOpenBrackets)
				ret = shiftPressed ? '{' : '[';
			else if (key == Keys.OemCloseBrackets)
				ret = shiftPressed ? '}' : ']';
			else if (key == Keys.OemSemicolon)
				ret = shiftPressed ? ':' : ';';
			else if (key == Keys.OemQuotes)
				ret = shiftPressed ? '"' : '\'';
			else if (key == Keys.OemComma)
				ret = shiftPressed ? '<' : '.';
			else if (key == Keys.OemPeriod)
				ret = shiftPressed ? '>' : ',';
			else if (key == Keys.OemQuestion)
				ret = shiftPressed ? '?' : '/';
			else if (key == Keys.OemPlus)
				ret = shiftPressed ? '+' : '=';

			// Return result
			return ret;
		} // KeyToChar(key)

        #endregion

        #region Handle Input Strings

        /// <summary>
        /// Auxiliary method used for input strings from the keyboard.
		/// </summary>
		public static void HandleInputStrings(ref string inputText)
		{
			// Is a shift key pressed (we have to check both, left and right)
			bool isShiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

			// Go through all pressed keys
			foreach (Keys pressedKey in keyboardState.GetPressedKeys())
				// Only process if it was not pressed last frame
				if (keysPressedLastFrame.Contains(pressedKey) == false)
				{
					// No special key?
					if (IsSpecialKey(pressedKey) == false &&
						// Max. allow 32 chars
						inputText.Length < 32)
					{
						// Then add the letter to our inputText.
						// Check also the shift state!
						inputText += KeyToChar(pressedKey, isShiftPressed);
					} // if (IsSpecialKey)
					else if (pressedKey == Keys.Back && inputText.Length > 0)
					{
						// Remove 1 character at end
						inputText = inputText.Substring(0, inputText.Length - 1);
					}
				}
		} // HandleKeyboardInput

        #endregion
        
        #region Update
        
        /// <summary>
		/// Update keyboard
		/// </summary>
		public static void Update()
		{
           	keysPressedLastFrame = new List<Keys>(keyboardState.GetPressedKeys());
            keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();            
		} // Update
		
        #endregion

    } // Keyboard
} // XNAFinalEngine.Input
