
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Mouse = XNAFinalEngine.Input.Mouse;
#endregion

namespace XNAFinalEngine.UserInterface
{

    #region Enumerators
    
    public enum MouseButton
    {
        None = 0,
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    } // MouseButton

    #endregion

    public class Input : Disposable
    {

        #region Classes

        private class InputKey
        {
            public Keys Key = Keys.None;
            public bool Pressed;
            public double Countdown = repeatDelay;
        } // InputKey

        private class InputMouseButton
        {
            public MouseButton Button = MouseButton.None;
            public bool Pressed;
        } // InputMouseButton

        #endregion

        #region Constants

        private const int repeatDelay = 500;
        private const int repeatRate = 50;

        #endregion

        #region Variables

        private readonly List<InputKey> keys = new List<InputKey>();
        private readonly List<InputMouseButton> mouseButtons = new List<InputMouseButton>();

        #endregion

        #region Events
	        
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyPress;
        public event KeyEventHandler KeyUp;

        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MousePress;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialice the user interface input system.
        /// </summary>
        public Input()
        {
            #if (WINDOWS)
                foreach (string keyName in Enum.GetNames(typeof(Keys)))
                {
                    InputKey key = new InputKey { Key = (Keys)Enum.Parse(typeof(Keys), keyName) };
                    keys.Add(key);
                }

                foreach (string mouseButtonName in Enum.GetNames(typeof(MouseButton)))
                {
                    InputMouseButton mouseButton = new InputMouseButton
                    {
                        Button = (MouseButton)Enum.Parse(typeof(MouseButton), mouseButtonName)
                    };
                    mouseButtons.Add(mouseButton);
                }
            #endif
        } // InputSystem

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public virtual void Update()
        {
            #if (WINDOWS)
                UpdateMouse();
                UpdateKeys();
            #endif
        } // Update

        #endregion

        #region Update Keyboard

        /// <summary>
        /// Update keyboard.
        /// </summary>
        private void UpdateKeys()
        {

            KeyEventArgs e = new KeyEventArgs { Caps = (((ushort) GetKeyState(0x14)) & 0xffff) != 0 };

            foreach (Keys key in Keyboard.KeyboardState.GetPressedKeys())
            {
                if      (key == Keys.LeftAlt     || key == Keys.RightAlt)     e.Alt = true;
                else if (key == Keys.LeftShift   || key == Keys.RightShift)   e.Shift = true;
                else if (key == Keys.LeftControl || key == Keys.RightControl) e.Control = true;
            }

            foreach (InputKey key in keys)
            {
                if (key.Key == Keys.LeftAlt     || key.Key == Keys.RightAlt   ||
                    key.Key == Keys.LeftShift   || key.Key == Keys.RightShift ||
                    key.Key == Keys.LeftControl || key.Key == Keys.RightControl)
                {
                    continue;
                }

                bool pressed = Keyboard.KeyboardState.IsKeyDown(key.Key);

                double frameTimeInMilliseconds = Time.GameDeltaTime * 1000; // From seconds to milliseconds.
                if (pressed) key.Countdown -= frameTimeInMilliseconds;

                if ((pressed) && (!key.Pressed))
                {
                    key.Pressed = true;
                    e.Key = key.Key;

                    if (KeyDown  != null) KeyDown.Invoke(this, e);
                    if (KeyPress != null) KeyPress.Invoke(this, e);
                }
                else if ((!pressed) && (key.Pressed))
                {
                    key.Pressed = false;
                    key.Countdown = repeatDelay;
                    e.Key = key.Key;

                    if (KeyUp != null) KeyUp.Invoke(this, e);
                }
                else if (key.Pressed && key.Countdown < 0)
                {
                    key.Countdown = repeatRate;
                    e.Key = key.Key;

                    if (KeyPress != null) KeyPress.Invoke(this, e);
                }
            }
        } // UpdateKeys
        
        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int key);

        #endregion

        #region Update Mouse

        /// <summary>
        /// Update mouse.
        /// </summary>
        private void UpdateMouse()
        {
            if ((Mouse.MouseState.X != Mouse.PreviousMouseState.X) || (Mouse.MouseState.Y != Mouse.PreviousMouseState.Y))
            {
                MouseEventArgs e = new MouseEventArgs();

                MouseButton btn = MouseButton.None;
                if      (Mouse.MouseState.LeftButton   == ButtonState.Pressed) btn = MouseButton.Left;
                else if (Mouse.MouseState.RightButton  == ButtonState.Pressed) btn = MouseButton.Right;
                else if (Mouse.MouseState.MiddleButton == ButtonState.Pressed) btn = MouseButton.Middle;
                else if (Mouse.MouseState.XButton1     == ButtonState.Pressed) btn = MouseButton.XButton1;
                else if (Mouse.MouseState.XButton2     == ButtonState.Pressed) btn = MouseButton.XButton2;

                BuildMouseEvent(btn, ref e);
                if (MouseMove != null)
                {
                    MouseMove.Invoke(this, e);
                }
            }
            UpdateButtons();
        } // UpdateMouse

        private void UpdateButtons()
        {

            MouseEventArgs e = new MouseEventArgs();

            foreach (InputMouseButton btn in mouseButtons)
            {
                ButtonState bs;

                if      (btn.Button == MouseButton.Left)     bs = Mouse.MouseState.LeftButton;
                else if (btn.Button == MouseButton.Right)    bs = Mouse.MouseState.RightButton;
                else if (btn.Button == MouseButton.Middle)   bs = Mouse.MouseState.MiddleButton;
                else if (btn.Button == MouseButton.XButton1) bs = Mouse.MouseState.XButton1;
                else if (btn.Button == MouseButton.XButton2) bs = Mouse.MouseState.XButton2;
                else continue;

                bool pressed = (bs == ButtonState.Pressed); // The current state

                if (pressed && !btn.Pressed) // If is pressed and the last frame wasn't pressed.
                {
                    btn.Pressed = true;
                    BuildMouseEvent(btn.Button, ref e);

                    if (MouseDown != null) MouseDown.Invoke(this, e);
                    if (MousePress != null) MousePress.Invoke(this, e);
                }
                else if (!pressed && btn.Pressed) // If isn't pressed and the last frame was pressed.
                {
                    btn.Pressed = false;
                    BuildMouseEvent(btn.Button, ref e);

                    if (MouseUp != null) MouseUp.Invoke(this, e);
                }
                else if (pressed && btn.Pressed) // If is pressed and was pressed.
                {
                    e.Button = btn.Button;
                    BuildMouseEvent(btn.Button, ref e);
                    if (MousePress != null) MousePress.Invoke(this, e);
                }
            }
        } // UpdateButtons

        private static void AdjustPosition(ref MouseEventArgs e)
        {
            Rectangle screen = SystemInformation.GameWindow.ClientBounds;

            if (e.Position.X < 0) e.Position.X = 0;
            if (e.Position.Y < 0) e.Position.Y = 0;
            if (e.Position.X >= screen.Width)  e.Position.X = screen.Width - 1;
            if (e.Position.Y >= screen.Height) e.Position.Y = screen.Height - 1;
        } // AdjustPosition

        private static void BuildMouseEvent(MouseButton button, ref MouseEventArgs e)
        {
            e.State = Mouse.MouseState;
            e.Button = button;

            e.Position = new Point(Mouse.MouseState.X, Mouse.MouseState.Y);
            AdjustPosition(ref e);

            e.State = new MouseState(e.Position.X, e.Position.Y, e.State.ScrollWheelValue, e.State.LeftButton, e.State.MiddleButton, e.State.RightButton, e.State.XButton1, e.State.XButton2);

            Point pos = new Point(Mouse.MouseState.X, Mouse.MouseState.Y);
            e.Difference = new Point(e.Position.X - pos.X, e.Position.Y - pos.Y);
        } // BuildMouseEvent

        #endregion

    } // Input
} // XNAFinalEngine.UserInterface