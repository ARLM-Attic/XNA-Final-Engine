
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Input
{

	/// <summary>
    /// Mouse.
	/// </summary>
	public class Mouse
	{

		#region Variables
        
		// Mouse state, set every frame in the Update method.
		private static MouseState currentMouseState, previousMouseState;    
        
        // X and Y movements of the mouse in this frame.
        private static int xMovement, yMovement;

        // Current mouse position.
        private static int positionX, positionY;
        
		// Mouse wheel delta. XNA does report only the total scroll value, but we usually need the current delta!
		private static int wheelDelta, wheelValue;

		// Start dragging pos, will be set when we just pressed the left mouse button. Used for the MouseDraggingAmount property.
		private static Point startDraggingPosition;

        // This mode allows to track the mouse movement when the mouse reach and pass the system window border.
        private static bool trackRelativeMovementMode;

	    #endregion

		#region Properties

        /// <summary>
        /// The current mouse state.
        /// </summary>
        public static MouseState MouseState { get { return currentMouseState; } }

        /// <summary>
        /// The previous mouse state.
        /// </summary>
        public static MouseState PreviousMouseState { get { return previousMouseState; } }

        #region Cursor
        
	    /// <summary>
	    /// This mode allows to track the mouse movement when the mouse reach and pass the system window border.
	    /// Useful for FPS cameras or similar.
        /// </summary>
        /// <remarks>
        /// This mode produces garbage because the XNA's Microsoft.Xna.Framework.Input.Mouse.SetPosition method produces garbage.
        /// </remarks>
	    public static bool TrackRelativeMovementMode
	    {
	        get { return trackRelativeMovementMode; }
	        set
	        {
	            trackRelativeMovementMode = value;
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(Screen.Width / 2, Screen.Height / 2);
                positionX = Screen.Width / 2;
                positionY = Screen.Height / 2;
	        }
	    } // TrackRelativeMovementMode

	    /// <summary>
		/// Mouse position in screen coordinates.
		/// </summary>
		public static Point Position
		{
			get
			{
                Point aux = new Point(positionX, positionY);
                if (aux.X >= Screen.Width)
                    aux.X = Screen.Width - 1;
                if (aux.X < 0)
                    aux.X = 0;
                if (aux.Y >= Screen.Height)
                    aux.Y = Screen.Height - 1;
                if (aux.Y < 0)
                    aux.Y = 0;
                return aux;
			}
			set
			{
                if (!TrackRelativeMovementMode)
                {
                    Microsoft.Xna.Framework.Input.Mouse.SetPosition(value.X, value.Y);
                }
                positionX = value.X;
                positionY = value.Y;
			}
        } // MousePosition

		/// <summary>
        /// The amount of pixels that the mouse has moved in this frame.
		/// </summary>
		public static float XMovement { get { return xMovement; } }

		/// <summary>
        /// The amount of pixels that the mouse has moved in this frame.
		/// </summary>
		public static float YMovement { get { return yMovement; } }

        #endregion

        #region Buttons

        /// <summary>
	    /// Mouse left button pressed.
	    /// </summary>
	    public static bool LeftButtonPressed { get { return currentMouseState.LeftButton == ButtonState.Pressed; } }

	    /// <summary>
	    /// Mouse right button pressed.
	    /// </summary>
	    public static bool RightButtonPressed { get { return currentMouseState.RightButton == ButtonState.Pressed; } }

	    /// <summary>
	    /// Mouse middle button pressed.
	    /// </summary>
	    public static bool MiddleButtonPressed { get { return currentMouseState.MiddleButton == ButtonState.Pressed; } }

        /// <summary>
        /// X button 1 pressed.
        /// </summary>
        public static bool XButton1Pressed { get { return currentMouseState.XButton1 == ButtonState.Pressed; } }

        /// <summary>
        /// X button 1 pressed.
        /// </summary>
        public static bool XButton2Pressed { get { return currentMouseState.XButton2 == ButtonState.Pressed; } }

		/// <summary>
		/// Mouse left button just pressed.
		/// </summary>
		public static bool LeftButtonJustPressed { get { return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released; } }

		/// <summary>
		/// Mouse right button just pressed.
		/// </summary>
		public static bool RightButtonJustPressed { get { return currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released; } }

        /// <summary>
        /// Mouse middle button just pressed.
        /// </summary>
        public static bool MiddleButtonJustPressed { get { return currentMouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Released; } }

        /// <summary>
        /// X button 1 pressed.
        /// </summary>
        public static bool XButton1JustPressed { get { return currentMouseState.XButton1 == ButtonState.Pressed && previousMouseState.XButton1 == ButtonState.Released; } }

        /// <summary>
        /// X button 2 pressed.
        /// </summary>
        public static bool XButton2JustPressed { get { return currentMouseState.XButton2 == ButtonState.Pressed && previousMouseState.XButton2 == ButtonState.Released; } }

        #endregion

        #region Dragging

        /// <summary>
		/// Mouse dragging amount.
        /// It can be extended to allow dragging off the screen when Relative Mode is on.
		/// </summary>
		public static Point DraggingAmount { get { return new Point(-startDraggingPosition.X + Position.X, -startDraggingPosition.Y + Position.Y); } }

        /// <summary>
        /// Start Dragging Position.
        /// </summary>
        public static Point StartDraggingPosition { get { return startDraggingPosition; } }

        #endregion

        #region Wheel

        /// <summary>
		/// Mouse wheel delta.
        /// XNA does report only the total scroll value, but we usually need the current delta!
		/// </summary>
		public static int WheelDelta { get { return wheelDelta; } }

        /// <summary>
        /// Mouse wheel value.
        /// </summary>
        public static int WheelValue { get { return wheelValue; } }

        #endregion

        #endregion

        #region Reset Mouse Dragging Amount

        /// <summary>
		/// Reset mouse dragging amount.
		/// </summary>
		public static void ResetDraggingAmount()
		{
			startDraggingPosition = Position;
		} // ResetMouseDraggingAmount

        #endregion

        #region Mouse In Box

	    /// <summary>
	    /// Mouse in box.
	    /// </summary>
	    public static bool MouseInBox(Rectangle rectangle)
		{
            return positionX >= rectangle.X &&
                   positionY >= rectangle.Y &&
                   positionX < rectangle.Right &&
                   positionY < rectangle.Bottom;
		} // MouseInBox

        #endregion

        #region Update

        /// <summary>
		/// Update.
		/// </summary>
		internal static void Update()
		{   
			// Update mouse state.
            previousMouseState = currentMouseState;
            currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            
            if (!TrackRelativeMovementMode)
            {
                // Calculate mouse movement.
                xMovement = currentMouseState.X - positionX; // positionX is the old position.
                yMovement = currentMouseState.Y - positionY;
                // Update position.
                positionX = currentMouseState.X; // Now is the new one.
                positionY = currentMouseState.Y;
            }
            else
            {
                xMovement = currentMouseState.X - Screen.Width / 2;
                yMovement = currentMouseState.Y - Screen.Height / 2;
                positionX += xMovement;
                positionY += yMovement;
                if (positionX >= Screen.Width)
                    positionX = Screen.Width - 1;
                if (positionX < 0)
                    positionX = 0;
                if (positionY >= Screen.Height)
                    positionY = Screen.Height - 1;
                if (positionY < 0)
                    positionY = 0;
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(Screen.Width / 2, Screen.Height / 2);
            }
            
            // Dragging
            if (LeftButtonJustPressed)
            {
                startDraggingPosition = Position;
            }
            // Wheel
			wheelDelta = currentMouseState.ScrollWheelValue - wheelValue;
			wheelValue = currentMouseState.ScrollWheelValue;
		} // Update

		#endregion

    } // Mouse
} // XNAFinalEngine.Input
