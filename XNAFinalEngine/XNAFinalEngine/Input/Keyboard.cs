
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
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
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

			// Else is is a special key
			return true;
        } // IsSpecialKey

        #endregion

        #region Key to String

        /// <summary>
		/// Key to string helper conversion method.
		/// If the keys are mapped other than on a default QWERTY keyboard or non English distribution, this method will not work properly.
		/// Most keyboards will return the same for A-Z and 0-9, but the special keys might be different.
		/// For a game with chat (windows) you should implement the windows events for catching keyboard input, which are much better!
		/// </summary>
        public static string KeyToString(Keys key, bool shift, bool caps)
        {
            bool uppercase = (caps && !shift) || (!caps && shift);

            int keyNum = (int) key;
            if (keyNum >= (int) Keys.A && keyNum <= (int) Keys.Z)
            {
                return uppercase ? key.ToString() : key.ToString().ToLower();
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
