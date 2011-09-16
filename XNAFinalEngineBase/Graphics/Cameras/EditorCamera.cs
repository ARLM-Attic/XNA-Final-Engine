
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
using XNAFinalEngine.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Editor Camera. Simulate the camera of the program Softimage XSI.
    /// If the S key is pressed the camera is in manipulation mode.
    /// If the s key is just pressed, the camera enter in manipulation mode until the space or escape key is pressed.
    /// In manipulation mode if we press the left mouse button the look at position of the camera changes.
    ///                      if we press the right mouse button and move the mouse the orientation around the look at position changes.
    /// If we move the mouse wheel the zoom of distance to the look at position changes.
    /// </summary>
    public sealed class EditorCamera : Camera
    {

        #region Variables

        /// <summary>
        /// Yaw. Controlled with the mouse Y movement.
        /// </summary> 
        private float pitch;
                
        /// <summary>
        /// Pitch.  Controlled with the mouse X movement.
        /// </summary>
        private float yaw;

        /// <summary>
        /// Roll. Not used, but it can be extended to simulate the L key. Just use the FreeCamera roll code.
        /// </summary>
        //private float roll;

        /// <summary>
        /// The camera distance to the look at position (zoom), controled by the mouse wheel.
        /// </summary>
        private float distance;

        /// <summary>
        /// Flags to register the S key functionality.
        /// If the S key is pressed the camera is in manipulation mode.
        /// If the s key is just pressed, the camera enter in manipulation mode until the space or escape key is pressed.
        /// </summary>
        private bool sJustPressed,
                     sMode;
                
        #endregion

        #region Properties

        /// <summary>
        /// The camera distance to the look at position (zoom), controled by the mouse wheel.
        /// </summary>
        public float Distance
        {
            get { return distance; }
            set
            {
                distance = value;
            }
        } // Distance

        #endregion

        #region Constructor

        /// <summary>
        /// Editor Camera. Simulate the camera of the program Softimage XSI.
        /// If the S key is pressed the camera is in manipulation mode.
        /// If the s key is just pressed, the camera enter in manipulation mode until the space or escape key is pressed.
        /// In manipulation mode if we press the left mouse button the look at position of the camera changes.
        ///                      if we press the right mouse button and move the mouse the orientation around the look at position changes.
        /// If we move the mouse wheel the zoom of distance to the look at position changes.
        /// </summary>
        /// <param name="_lookAtPosition">Look at position</param>
        /// <param name="_distance">Distance to the look at position</param>
        /// <param name="_pitch">Pitch</param>
        /// <param name="_yaw">Yaw</param>
        public EditorCamera(Vector3 _lookAtPosition, float _distance = 20, float _pitch = 0, float _yaw = 0)
        {
            LookAtPosition = _lookAtPosition;
            distance = _distance;
            pitch = _pitch;
            yaw = _yaw;
        } // EditorCamera

        /// <summary>
        /// Editor Camera. Simulate the camera of the program Softimage XSI.
        /// If the S key is pressed the camera is in manipulation mode.
        /// If the s key is just pressed, the camera enter in manipulation mode until the space or escape key is pressed.
        /// In manipulation mode if we press the left mouse button the look at position of the camera changes.
        ///                      if we press the right mouse button and move the mouse the orientation around the look at position changes.
        /// If we move the mouse wheel the zoom of distance to the look at position changes.
        /// </summary>
        public EditorCamera(Vector3 position, Vector3 _lookAtPosition = new Vector3())
        {
            SetLookAt(position, _lookAtPosition, Vector3.Up);
            LookAtPosition = _lookAtPosition;
            distance = Vector3.Distance(Position, LookAtPosition);
            Vector3 yawPitchRoll = GetYawPitchRollFromQuaternion(Orientation);
            yaw = yawPitchRoll.X;
            pitch = yawPitchRoll.Y;
        } // EditorCamera

        #endregion

        #region Handle Camera

        /// <summary>
        /// Handle camera.
        /// </summary>
        private void HandleCamera()
        {
            //if (UserInterface.UserInterfaceManager.FocusedControl == null) // TODO Acordate de sacar el comentario
            {
                #region Is in S mode or not?

                if (sMode)
                {
                    if (Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed)
                    {
                        sMode = false;
                    }
                }
                else
                {
                    if (Keyboard.KeyJustPressed(Keys.S))
                    {
                        sJustPressed = true;
                    }
                    else
                    {
                        if (sJustPressed)
                        {
                            sJustPressed = false; // TODO!! Pasarla a tiempo, pero para eso debo implementar el tiempo.
                            if (!(Keyboard.KeyPressed(Keys.S)))
                            {
                                sMode = true;
                            }
                        }
                    }
                }

                #endregion

                if (sMode || Keyboard.KeyPressed(Keys.S))
                {
                    // Translation
                    if (Mouse.LeftButtonPressed)
                    {
                        LookAtPosition -= XAxis * Mouse.XMovement * Distance / 2000;
                        LookAtPosition += YAxis * Mouse.YMovement * Distance / 2000;
                    }
                    // Orientation
                    if (Mouse.RightButtonPressed)
                    {
                        yaw += Mouse.XMovement * 0.005f;
                        pitch += Mouse.YMovement * 0.005f;

                        // Orientation bounds
                        if (yaw >= TwoPi)
                        {
                            yaw -= TwoPi;
                        }
                        if (yaw < 0)
                        {
                            yaw += TwoPi;
                        }
                        if (pitch >= TwoPi)
                        {
                            pitch -= TwoPi;
                        }
                        if (pitch < 0)
                        {
                            pitch += TwoPi;
                        }
                    }
                }
                // Distance or zoom
                distance -= Mouse.WheelDelta * Distance / 1300;
                if (distance > FarPlane)
                    distance = FarPlane;
                if (distance < 2)
                    distance = 2;
            }

            // Update orientation
            Orientation = Quaternion.Identity;
            RotateGlobal(RotationAxis.Pitch, pitch);
            RotateGlobal(RotationAxis.Yaw, yaw);
            // Update position
            Matrix rotMatrix = Matrix.CreateFromQuaternion(Orientation);
            Position = LookAtPosition + new Vector3(rotMatrix.M13, rotMatrix.M23, rotMatrix.M33) * distance;

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
            UpdateProjectionMatrix();
        } // Update

        #endregion

    } // EditorCamera
} // XNAFinalEngine.Graphics

