
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// A free view camera. Allows to freely rotate and move around.
    /// WASD for movement, left mouse button and mouse movement for orientation,
    /// Q and E for roll rotations, and R to reset camera position and orientation.
    /// </summary>
    public class FreeCamera : Camera
    {

        #region Variables

        /// <summary>
        /// The initial position for resets.
        /// </summary>
        private Vector3 resetPosition;

        /// <summary>
        /// The initial orientation for resets.
        /// </summary>
        private Quaternion resetQuaternion;

        #endregion

        #region Constructor

        /// <summary>
		/// Create a free camera.
		/// </summary>
        public FreeCamera(Vector3 _position, Vector3 _lookPosition = new Vector3()) : base(_position, _lookPosition)
        {
            resetPosition = Position;
            resetQuaternion = Orientation;
        }

        #endregion
        
        #region Handle Camera

        /// <summary>
        /// Handle camera
        /// </summary>
        private void HandleCamera()
        {
            float moveFactor = (float)EngineManager.ElapsedTimeThisFrameInSeconds * 20;
            float rollFactor = (float)EngineManager.ElapsedTimeThisFrameInSeconds * 1;
            float rotationFactor = (float)EngineManager.ElapsedTimeThisFrameInSeconds / 10;
                        
            // Orientation
            if (Mouse.LeftButtonPressed)
            {
                RotateLocal(RotationAxis.Yaw, Mouse.XMovement * rotationFactor);
                RotateLocal(RotationAxis.Pitch, Mouse.YMovement * rotationFactor);
            }

            // Movement
            if (Keyboard.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                TranslateLocal(-moveFactor, MoveDirections.X);
            if (Keyboard.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                TranslateLocal(moveFactor,  MoveDirections.X);
            if (Keyboard.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                TranslateLocal(-moveFactor, MoveDirections.Z);
            if (Keyboard.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                TranslateLocal(moveFactor,  MoveDirections.Z);
          
            // Roll
            if (Keyboard.KeyPressed(Keys.Q))
                RotateLocal(RotationAxis.Roll, -rollFactor);
            if (Keyboard.KeyPressed(Keys.E))
                RotateLocal(RotationAxis.Roll, +rollFactor);

            // Reset
            if (Keyboard.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R))
            {
                Position = resetPosition;
                Orientation = resetQuaternion;
            }
        } // HandleCamera

        #endregion

        #region Update

        /// <summary>
        /// Update camera, the engine calls it every frame in the update stage.
        /// </summary>
        public override void Update()
        {
            HandleCamera();
            UpdateViewMatrix();
        } // Update

        #endregion

    } // FreeCamera
} // XNAFinalEngine.GraphicObjects

