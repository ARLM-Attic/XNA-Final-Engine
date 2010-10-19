
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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

		/// <summary>
		/// Mouse state, set every frame in the Update method.
		/// </summary>
		private static MouseState state, stateLastFrame;
        
		/// <summary>
		/// Was a mouse detected? Returns true if the user moves the mouse.
		/// </summary>
		private static bool isConnected = false;

        /// <summary>
        /// X and Y movements of the mouse in this frame.
        /// </summary>
        private static int xMovement, yMovement;

        /// <summary>
        /// Current and last mouse position.
        /// </summary>
        private static int positionX, positionY;
        
		/// <summary>
		/// Mouse wheel delta. XNA does report only the total scroll value, but we usually need the current delta!
		/// </summary>
		private static int wheelDelta = 0, wheelValue = 0;

		/// <summary>
		/// Start dragging pos, will be set when we just pressed the left
		/// mouse button. Used for the MouseDraggingAmount property.
		/// </summary>
		private static Point startDraggingPosition;

        /// <summary>
        /// It allows to track the mouse movement when the mouse reach the system window border.
        /// In this case the mouse cursor can't be out of the XNA window.
        /// </summary>
        private static bool outOfBounds = false;

        #endregion

		#region Properties

        /// <summary>
        /// It allows to track the mouse movement when the mouse reach the system window border.
        /// In this case the mouse cursor can't be out of the XNA window.
        /// </summary>
        public static bool OutOfBounds
        {
            get { return outOfBounds; }
            set { outOfBounds = value; }
        }

		/// <summary>
		/// Was a mouse detected? Returns true if the user moves the mouse.
		/// </summary>
		public static bool IsConnected { get { return isConnected; }  }

		/// <summary>
		/// Mouse position in screen coordinates.
		/// </summary>
		public static Point Position
		{
			get
			{
                Point aux = new Point(positionX, positionY);
                if (state.X >= EngineCore.EngineManager.Width)
                    aux.X = EngineCore.EngineManager.Width - 1;
                if (state.X < 0)
                    aux.X = 0;
                if (state.Y >= EngineCore.EngineManager.Height)
                    aux.Y = EngineCore.EngineManager.Height - 1;
                if (state.Y < 0)
                    aux.Y = 0;
				return aux;
			} // get
			set
			{   
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(value.X, value.Y);
                positionX = value.X;
                positionY = value.Y;
			} // set
        } // MousePosition

		/// <summary>
        /// The amount of pixels that the mouse has moved in this frame.
		/// </summary>
		public static float XMovement { get { return xMovement; } }

		/// <summary>
        /// The amount of pixels that the mouse has moved in this frame.
		/// </summary>
		public static float YMovement { get { return yMovement; } }

        #region Buttons

        /// <summary>
		/// Mouse left button pressed
		/// </summary>
		public static bool LeftButtonPressed { get { return state.LeftButton == ButtonState.Pressed; } }

		/// <summary>
		/// Mouse right button pressed
		/// </summary>
		public static bool RightButtonPressed  { get { return state.RightButton == ButtonState.Pressed; } }

		/// <summary>
		/// Mouse middle button pressed
		/// </summary>
		public static bool MiddleButtonPressed { get { return state.MiddleButton == ButtonState.Pressed; } }

		/// <summary>
		/// Mouse left button just pressed
		/// </summary>
		public static bool LeftButtonJustPressed
		{
			get
			{
				return state.LeftButton == ButtonState.Pressed && stateLastFrame.LeftButton == ButtonState.Released;
			}
		} // MouseLeftButtonJustPressed

		/// <summary>
		/// Mouse right button just pressed
		/// </summary>
		public static bool RightButtonJustPressed
		{
			get
			{
				return state.RightButton == ButtonState.Pressed && stateLastFrame.RightButton == ButtonState.Released;
			}
		} // MouseRightButtonJustPressed

        /// <summary>
        /// Mouse middle button just pressed
        /// </summary>
        public static bool MiddleButtonJustPressed
        {
            get
            {
                return state.MiddleButton == ButtonState.Pressed && stateLastFrame.MiddleButton == ButtonState.Released;
            }
        } // MiddleButtonJustPressed

        #endregion

        /// <summary>
		/// Mouse dragging amount.
        /// It can be extended to allow dragging off the screen when OutOfBounds is true.
		/// </summary>
		public static Point DraggingAmount
		{
			get
			{
				return new Point(-startDraggingPosition.X + Position.X,
				                 -startDraggingPosition.Y + Position.Y);
			}
		} // MouseDraggingAmount

        /// <summary>
        /// Start Dragging Position
        /// </summary>
        public static Point StartDraggingPosition { get { return startDraggingPosition; } }

        /// <summary>
		/// Mouse wheel delta.
		/// </summary>
		public static int WheelDelta { get { return wheelDelta; } }

        /// <summary>
        /// Mouse wheel value.
        /// </summary>
        public static int WheelValue { get { return wheelValue; } }

        #endregion

        #region Reset Mouse Dragging Amount

        /// <summary>
		/// Reset mouse dragging amount
		/// </summary>
		public static void ResetDraggingAmount()
		{
			startDraggingPosition = Position;
		} // ResetMouseDraggingAmount

        #endregion

        #region Mouse In Box

        /// <summary>
		/// Mouse in box
		/// </summary>
		/// <param name="rect">Rectangle</param>
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
		/// Update
		/// </summary>
		public static void Update()
		{   
			// Handle mouse input variables
            stateLastFrame = state;
            state = Microsoft.Xna.Framework.Input.Mouse.GetState();

            if (!outOfBounds)
            {
                xMovement = state.X - stateLastFrame.X;
                yMovement = state.Y - stateLastFrame.Y;
                positionX = state.X;
                positionY = state.Y;
            }
            else
            {   
                xMovement = state.X - EngineManager.Width / 2;
                yMovement = state.Y - EngineManager.Height / 2;
                positionX += xMovement;
                positionY += yMovement;
                if (positionX >= EngineCore.EngineManager.Width)
                    positionX = EngineCore.EngineManager.Width - 1;
                if (positionX < 0)
                    positionX = 0;
                if (positionY >= EngineCore.EngineManager.Height)
                    positionY = EngineCore.EngineManager.Height - 1;
                if (positionY < 0)
                    positionY = 0;
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(EngineManager.Width / 2, EngineManager.Height / 2);
            }
            
            if (LeftButtonPressed == false)
            {
                startDraggingPosition = Position;
            }
			wheelDelta = state.ScrollWheelValue - wheelValue;
			wheelValue = state.ScrollWheelValue;

			// Check if mouse was moved this frame if it is not detected yet.
			// This allows us to ignore the mouse even when it is captured
			// on a windows machine if just the gamepad or keyboard is used.
            if (isConnected == false)// &&
            {
                isConnected = state.X != stateLastFrame.X ||
                                state.Y != stateLastFrame.Y ||
                                state.LeftButton != stateLastFrame.LeftButton;
            }
		} // Update

		#endregion

    } // Mouse
} // XNAFinalEngine.Input
