
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
Authors: Gonzalo Webber and Lucrecia Ibaldi
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// A simple FPS Camera.
    /// WASD for movement, mouse movement for orientation.
    /// Developed by Gonzalo Webber and Lucrecia Ibaldi for Red Fire.
    /// This class is a part of a project developed in this engine for the subject Computación Grafica (Universidad Nacional del Sur).
    /// </summary>
    public class FPSCamera : Camera
    {

        #region Variables

        /// <summary>
        /// Rotation factor.
        /// </summary> 
        private float rotationFactor = 0.5f;

        /// <summary>
        /// Move factor.
        /// </summary> 
        private float moveFactor = 15f;

        /// <summary>
        /// Character height.
        /// </summary> 
        private float height = 2.0f;

        /// <summary>
        /// Absolute yaw. Controlled with the mouse Y movement.
        /// </summary> 
        private float pitch;
                
        /// <summary>
        /// Absolute pitch.  Controlled with the mouse X movement.
        /// </summary>
        private float yaw;
                                      
        #endregion

        #region Properties

        /// <summary>
        /// Rotation factor.
        /// </summary>
        public float RotationFactor { get { return rotationFactor; } set { rotationFactor = value; } }

        /// <summary>
        /// Move factor.
        /// </summary>
        public float MveFactor { get { return moveFactor; } set { moveFactor = value; } }

        /// <summary>
        /// Character height.
        /// </summary>
        public float Height
        {
            get { return height; }
            set
            {
                height = value;
                Position = new Vector3(Position.X, height, Position.Z);
            }
        } // Height

        #endregion
        
        #region Constructor

        /// <summary>
        /// Create a FPS camera.
		/// </summary>
        public FPSCamera(Vector3 _position) : base(new Vector3())
        {
            Mouse.OutOfBounds = true; // Mouse FPS mode.
            Position = _position;
            BuildProjectionMatrix();
        } // FPSCamera

        #endregion

        #region Handle FPS camera

        /// <summary>
        /// Handle FPS camera.
        /// </summary>
        private void HandleCamera()
        {

            #region Orientation

            // Orientation
            yaw += (Mouse.XMovement) * rotationFactor * (float)EngineManager.FrameTime;
            pitch += (Mouse.YMovement) * rotationFactor * (float)EngineManager.FrameTime;
            // Upper bound
            if (pitch > HalfPi - 1f)
            {
                pitch = HalfPi - 1f;
            }
            // Lower bound
            if (pitch < -HalfPi + 1f)
            {
                pitch = -HalfPi + 1f;
            }
            Orientation = Quaternion.Identity;
            RotateGlobal(RotationAxis.Pitch, pitch);
            RotateGlobal(RotationAxis.Yaw, yaw);

            #endregion

            #region Movement

            float deltaX = 0, deltaZ = 0;
            float deltaTime = (float)EngineManager.FrameTime;
            if (Keyboard.KeyPressed(Keys.W))
            {
                deltaZ = moveFactor * deltaTime;
            }
            if (Keyboard.KeyPressed(Keys.S))
            {
                deltaZ = -moveFactor * deltaTime;
            }
            if (Keyboard.KeyPressed(Keys.A))
            {
                deltaX = moveFactor * deltaTime;
            }
            if (Keyboard.KeyPressed(Keys.D))
            {
                deltaX = -moveFactor * deltaTime;
            }
            TranslateLocal(-deltaZ, MoveDirections.Z);
            TranslateLocal(-deltaX, MoveDirections.X);

            #endregion

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

    } // FPSCamera
} // XNAFinalEngine.Graphics

