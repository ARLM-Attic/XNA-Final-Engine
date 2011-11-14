
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
#endregion

namespace XNAFinalEngine.Input
{

	/// <summary>
    /// XInput Gamepad.
    /// Allows to work up to four different gamepad.
    /// XInput, an API controllers introduced with the launch of the Xbox 360. It has the advantage over DirectInput of significantly easier programmability.
    /// XInput is compatible with DirectX 9 and up.
    /// http://en.wikipedia.org/wiki/DirectInput
    /// </summary>
	public class GamePad
	{

		#region Variables

		// Gamepad state, set every frame in the update method.
		private GamePadState gamePadState, previousGamePadState;

        // The id number of the gamepad.
        private readonly PlayerIndex playerIndex;

		#endregion

		#region Properties

		/// <summary>
		/// The current gamepad state.
		/// </summary>
		public GamePadState GamePadState { get { return gamePadState; } }

        /// <summary>
        /// The previous mouse state.
        /// </summary>
        public GamePadState PreviousGamePadState { get { return previousGamePadState; } }

		/// <summary>
		/// Is the gamepad connected?
		/// </summary>
		public bool IsConnected { get { return gamePadState.IsConnected; } }

        /// <summary>
        /// Retrieves the capabilities of this game pad.
        /// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.gamepadcapabilities_members.aspx
        /// </summary>
        public GamePadCapabilities Capabilities { get { return Microsoft.Xna.Framework.Input.GamePad.GetCapabilities(playerIndex); } }

        /// <summary>
        /// Indicates if the input state has changed.
        /// </summary>
        public bool Iddle { get { return gamePadState.PacketNumber == previousGamePadState.PacketNumber; } }

        /// <summary>
        /// Specifies a type of dead zone processing to apply to Xbox 360 Controller analog sticks.
        /// Circular: The combined X and Y position of each stick is compared to the dead zone.
        ///           This provides better control than IndependentAxes when the stick is used as a two-dimensional control surface,
        ///           such as when controlling a character's view in a first-person game.
        /// IndependentAxes: The X and Y positions of each stick are compared against the dead zone independently. This setting is the default.
        /// None: The values of each stick are not processed and are returned as "raw" values. This is best if you intend to implement your own dead zone processing.
        /// </summary>
        public GamePadDeadZone DeadZone { get; set; }
        
        #region Back, Start, Big Button

        /// <summary>
        /// Gamepad start button pressed.
		/// </summary>
		public bool StartPressed { get { return gamePadState.Buttons.Start == ButtonState.Pressed; } }

        /// <summary>
        /// Gamepad back button pressed.
        /// </summary>
        public bool BackPressed { get { return gamePadState.Buttons.Back == ButtonState.Pressed; } }

        /// <summary>
        /// Gamepad big button pressed.
        /// </summary>
        public bool BigButtonPressed { get { return gamePadState.Buttons.BigButton == ButtonState.Pressed; } }
        
        /// <summary>
        /// Gamepad start button just pressed.
        /// </summary>
        public bool StartJustPressed { get { return gamePadState.Buttons.Start == ButtonState.Pressed && previousGamePadState.Buttons.Start == ButtonState.Released; } }

        /// <summary>
        /// Gamepad back button just pressed.
        /// </summary>
        public bool BackJustPressed { get { return gamePadState.Buttons.Back == ButtonState.Pressed && previousGamePadState.Buttons.Back == ButtonState.Released; } }

        /// <summary>
        /// Gamepad big button just pressed.
        /// </summary>
        public bool BigButtonJustPressed { get { return gamePadState.Buttons.BigButton == ButtonState.Pressed && previousGamePadState.Buttons.BigButton == ButtonState.Released; } }

        #endregion

        #region A, B, X, Y

        /// <summary>
		/// Gamepad A button pressed.
		/// </summary>
		public bool APressed { get { return gamePadState.Buttons.A == ButtonState.Pressed; } }

		/// <summary>
        /// Gamepad B button pressed.
		/// </summary>
		public bool BPressed { get { return gamePadState.Buttons.B == ButtonState.Pressed; } }

		/// <summary>
        /// Gamepad X button pressed.
		/// </summary>
		public bool XPressed { get { return gamePadState.Buttons.X == ButtonState.Pressed; } }

		/// <summary>
        /// Gamepad Y button pressed.
		/// </summary>
		public bool YPressed { get { return gamePadState.Buttons.Y == ButtonState.Pressed; } }
        
        /// <summary>
        /// Gamepad A button just pressed.
		/// </summary>
		public bool AJustPressed { get { return gamePadState.Buttons.A == ButtonState.Pressed && previousGamePadState.Buttons.A == ButtonState.Released; } }

		/// <summary>
        /// Gamepad B button just pressed.
		/// </summary>
		public bool BJustPressed { get { return gamePadState.Buttons.B == ButtonState.Pressed && previousGamePadState.Buttons.B == ButtonState.Released; } }
		
		/// <summary>
        /// Gamepad X button just pressed.
		/// </summary>
		public bool XJustPressed { get { return gamePadState.Buttons.X == ButtonState.Pressed && previousGamePadState.Buttons.X == ButtonState.Released; } }

		/// <summary>
        /// Gamepad Y button just pressed.
		/// </summary>
		public bool YJustPressed { get { return gamePadState.Buttons.Y == ButtonState.Pressed && previousGamePadState.Buttons.Y == ButtonState.Released; } }

        #endregion

        #region D Pad

        /// <summary>
        /// Gamepad DPad left pressed.
        /// </summary>
        public bool DPadLeftPressed { get { return gamePadState.DPad.Left == ButtonState.Pressed; } }

        /// <summary>
        /// Gamepad DPad right pressed.
        /// </summary>
        public bool DPadRightPressed { get { return gamePadState.DPad.Right == ButtonState.Pressed; } }

        /// <summary>
        /// Gamepad DPad up pressed.
        /// </summary>
        public bool DPadUpPressed { get { return gamePadState.DPad.Up == ButtonState.Pressed; } }

        /// <summary>
        /// Gamepad DPad down pressed.
        /// </summary>
        public bool DPadDownPressed { get { return gamePadState.DPad.Down == ButtonState.Pressed; } }

        /// <summary>
        /// Gamepad DPad left just pressed.
        /// </summary>
        public bool DPadLeftJustPressed { get { return gamePadState.DPad.Left == ButtonState.Pressed && previousGamePadState.DPad.Left == ButtonState.Released; } }

        /// <summary>
        /// Gamepad DPad right just pressed.
        /// </summary>
        public bool DPadRightJustPressed { get { return gamePadState.DPad.Right == ButtonState.Pressed && previousGamePadState.DPad.Right == ButtonState.Released; } }

        /// <summary>
        /// Gamepad DPad up just pressed.
        /// </summary>
        public bool DPadUpJustPressed { get { return gamePadState.DPad.Up == ButtonState.Pressed && previousGamePadState.DPad.Up == ButtonState.Released; } }

        /// <summary>
        /// Gamepad DPad down just pressed.
        /// </summary>
        public bool DPadDownJustPressed { get { return gamePadState.DPad.Down == ButtonState.Pressed && previousGamePadState.DPad.Down == ButtonState.Released; } }

        #endregion
        
        #region Thumb Sticks

        /// <summary>
        /// Game pad left thumb stick X movement
        /// </summary>
        public float LeftStickXMovement { get { return gamePadState.ThumbSticks.Left.X; } }

        /// <summary>
        /// Game pad left thumb stick Y movement
        /// </summary>
        public float LeftStickYMovement { get { return gamePadState.ThumbSticks.Left.Y; } }

        /// <summary>
        /// Game pad right thumb stick X movement
        /// </summary>
        public float RightStickXMovement { get { return gamePadState.ThumbSticks.Right.X; } }

        /// <summary>
        /// Game pad right thumb stick Y movement
        /// </summary>
        public float RightStickYMovement { get { return gamePadState.ThumbSticks.Right.Y; } }

        /// <summary>
        /// Game pad left stick button pressed.
        /// </summary>
        public bool LeftStickPressed { get { return gamePadState.Buttons.LeftStick == ButtonState.Pressed; } }

        /// <summary>
        /// Game pad left stick button just pressed.
        /// </summary>
        public bool LeftStickJustPressed { get { return gamePadState.Buttons.LeftStick == ButtonState.Pressed && previousGamePadState.Buttons.LeftStick == ButtonState.Released; } }

        /// <summary>
        /// Game pad right stick button pressed.
        /// </summary>
        public bool RightStickPressed { get { return gamePadState.Buttons.RightStick == ButtonState.Pressed; } }

        /// <summary>
        /// Game pad right stick button just pressed.
        /// </summary>
        public bool RightStickJustPressed { get { return gamePadState.Buttons.RightStick == ButtonState.Pressed && previousGamePadState.Buttons.RightStick == ButtonState.Released; } }

        #endregion

        #region LT LB RT RB

        /// <summary>
        /// Game pad Left Button pressed (LB)
        /// </summary>
        public bool LeftButtonPressed { get { return gamePadState.Buttons.LeftShoulder == ButtonState.Pressed; } }

        /// <summary>
        /// Game pad Left Button just pressed (LB)
        /// </summary>
        public bool LeftButtonJustPressed { get { return gamePadState.Buttons.LeftShoulder == ButtonState.Pressed && previousGamePadState.Buttons.LeftShoulder == ButtonState.Released; } }

        /// <summary>
        /// Game pad Right Button pressed (RB)
        /// </summary>
        public bool RightButtonPressed { get { return gamePadState.Buttons.RightShoulder == ButtonState.Pressed; } }

        /// <summary>
        /// Game pad Right Button just pressed (RB)
        /// </summary>
        public bool RightButtonJustPressed { get { return gamePadState.Buttons.RightShoulder == ButtonState.Pressed && previousGamePadState.Buttons.RightShoulder == ButtonState.Released; } }

        /// <summary>
        /// Game pad left trigger axis value (LT)
        /// </summary>
        public float LeftTriggerMovement { get { return gamePadState.Triggers.Left; } }

        /// <summary>
        /// Game pad right trigger axis value (RT)
        /// </summary>
        public float RightTriggerMovement { get { return gamePadState.Triggers.Right; } }

        #endregion

        #region Digital Movement (DPad or Left Stick)

        /// <summary>
        /// Game pad left pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementLeftPressed { get { return gamePadState.DPad.Left == ButtonState.Pressed || gamePadState.ThumbSticks.Left.X < -0.75f; } }

        /// <summary>
        /// Game pad right pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementRightPressed { get { return gamePadState.DPad.Right == ButtonState.Pressed || gamePadState.ThumbSticks.Left.X > 0.75f; } }

        /// <summary>
        /// Game pad up pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementUpPressed { get { return gamePadState.DPad.Down == ButtonState.Pressed || gamePadState.ThumbSticks.Left.Y > 0.75f; } }

        /// <summary>
        /// Game pad down pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementDownPressed { get { return gamePadState.DPad.Up == ButtonState.Pressed || gamePadState.ThumbSticks.Left.Y < -0.75f; } }

        /// <summary>
        /// Game pad left just pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementLeftJustPressed 
        {
            get
            {
                return (gamePadState.DPad.Left == ButtonState.Pressed &&
                        previousGamePadState.DPad.Left == ButtonState.Released) ||
                       (gamePadState.ThumbSticks.Left.X < -0.75f &&
                        previousGamePadState.ThumbSticks.Left.X > -0.75f);
            }
        } // DigitalMovementLeftJustPressed

        /// <summary>
        /// Game pad right just pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementRightJustPressed
        {
            get
            {
                return (gamePadState.DPad.Right == ButtonState.Pressed &&
                        previousGamePadState.DPad.Right == ButtonState.Released) ||
                       (gamePadState.ThumbSticks.Left.X > 0.75f &&
                        previousGamePadState.ThumbSticks.Left.X < 0.75f);
            }
        } // DigitalMovementRightJustPressed

        /// <summary>
        /// Game pad up just pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementUpJustPressed
        {
            get
            {
                return (gamePadState.DPad.Up == ButtonState.Pressed &&
                        previousGamePadState.DPad.Up == ButtonState.Released) ||
                       (gamePadState.ThumbSticks.Left.Y > 0.75f &&
                        previousGamePadState.ThumbSticks.Left.Y < 0.75f);
            }
        } // DigitalMovementUpJustPressed

        /// <summary>
        /// Game pad down just pressed. A digital movement considers the DPad and the left stick.
        /// </summary>
        public bool DigitalMovementDownJustPressed
        {
            get
            {
                return (gamePadState.DPad.Down == ButtonState.Pressed &&
                        previousGamePadState.DPad.Down == ButtonState.Released) ||
                       (gamePadState.ThumbSticks.Left.Y < -0.75f &&
                        previousGamePadState.ThumbSticks.Left.Y > -0.75f);
            }
        } // DigitalMovementDownJustPressed

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Init the xinput gamepad of this player index.
        /// </summary>
        private GamePad(PlayerIndex _playerIndex)
        {
            playerIndex = _playerIndex;
            gamePadState = Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex);
        } // GamePad

        #endregion

        #region Set Vibration

        /// <summary>
        /// Sets the vibration motor speeds on an Xbox 360 Controller.
        /// The speed of the left and right motor, between 0.0 and 1.0.
        /// </summary>
        public void SetVibration(float leftMotor, float rightMotor)
        {
            Microsoft.Xna.Framework.Input.GamePad.SetVibration(playerIndex, leftMotor, rightMotor);
        } // SetVibration

        #endregion
        
        #region Update

        /// <summary>
		/// Update.
        /// If the gamepad is not connected the operation won't trow exception, the state will be empty. 
		/// </summary>
		internal void Update()
		{
			previousGamePadState = gamePadState;
            gamePadState = Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex, DeadZone);
		} // Update

		#endregion

        #region Static

        #region Variables

        // The four possible gamepads.
        private readonly static GamePad playerOneGamePad   = new GamePad(PlayerIndex.One),
                                        playerTwoGamePad   = new GamePad(PlayerIndex.Two),
                                        playerThreeGamePad = new GamePad(PlayerIndex.Three),
                                        playerFourGamePad  = new GamePad(PlayerIndex.Four);


        #endregion

        #region Properties

        /// <summary>
        /// XInput gamePad assigned to player one. 
        /// </summary>
        public static GamePad PlayerOne { get { return playerOneGamePad; } }

        /// <summary>
        /// XInput gamePad assigned to player two. 
        /// </summary>
        public static GamePad PlayerTwo { get { return playerTwoGamePad; } }

        /// <summary>
        /// XInput gamePad assigned to player three. 
        /// </summary>
        public static GamePad PlayerThree { get { return playerThreeGamePad; } }

        /// <summary>
        /// XInput gamePad assigned to player four. 
        /// </summary>
        public static GamePad PlayerFour { get { return playerFourGamePad; } }

        #endregion

        #endregion

    } // GamePad
} // XNAFinalEngine.Input
