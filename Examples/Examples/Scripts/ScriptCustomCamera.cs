
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using Mouse = XNAFinalEngine.Input.Mouse;
using GamePad = XNAFinalEngine.Input.GamePad;
#endregion

namespace XNAFinalEngineExamples
{
    /// <summary>
    /// Custom camera script.
    /// In manipulation mode if we press the left mouse button the look at position of the camera changes.
    ///                      if we press the right mouse button and move the mouse the orientation around the look at position changes.
    /// If we move the mouse wheel the zoom of distance to the look at position changes.
    /// </summary>
    public sealed class ScriptCustomCamera : Script
    {

        #region Constants

        /// <summary>
        /// 2 Pi or 180 degrees.
        /// </summary>
        private const float twoPi = (float)Math.PI * 2;

        #endregion

        #region Properties

        /// <summary>
        /// The camera distance to the look at position (zoom), controled by the mouse wheel.
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// Look At Position.
        /// </summary>
        public Vector3 LookAtPosition { get; set; }

        /// <summary>
        /// Yaw.
        /// Controlled with the mouse Y movement.
        /// </summary> 
        public float Pitch { get; set; }

        /// <summary>
        /// Pitch.
        /// Controlled with the mouse X movement.
        /// </summary>
        public float Yaw { get; set; }

        /// <summary>
        /// Roll.
        /// </summary>
        public float Roll { get; set; }

        #endregion

        #region Load

        /// <summary>
        /// Check that it works in 3D space and a camera component exists.
        /// </summary>
        public override void Load()
        {
            if (Owner is GameObject2D)
            {
                throw new InvalidOperationException("Editor Camera Script: Unable to apply to a 2D Game Object.");
            }
            if (((GameObject3D)Owner).Camera == null)
            {
                throw new InvalidOperationException("Editor Camera Script: the owner does not have a camera component.");
            }
        } // Load

        #endregion

        #region Set Position

        /// <summary>
        /// Set the camera position.
        /// </summary>
        public void SetPosition(Vector3 position, Vector3 lookAtPosition)
        {
            LookAtPosition = lookAtPosition;
            Distance = Vector3.Distance(position, lookAtPosition);
            Vector3 yawPitchRoll = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(position, lookAtPosition, Vector3.Up)).GetYawPitchRoll();
            Yaw = yawPitchRoll.X;
            Pitch = yawPitchRoll.Y;
            Roll = yawPitchRoll.Z;
        } // SetPosition

        #endregion

        #region Update

        /// <summary>
        /// Update camera.
        /// </summary>
        public override void Update()
        {
            if (!((GameObject3D)Owner).Camera.Enabled || ((GameObject3D)Owner).Camera != Camera.MainCamera)
                return;
            
            #region Manipulate
            
            // Translation
            if (Mouse.LeftButtonPressed)
            {
                LookAtPosition -= ((GameObject3D)Owner).Transform.Right * Mouse.DeltaX * Distance / 2000;
                LookAtPosition += ((GameObject3D)Owner).Transform.Up    * Mouse.DeltaY * Distance / 2000;
            }
            // Orientation
            if (Mouse.RightButtonPressed)
            {
                Yaw   += Mouse.DeltaX * 0.005f;
                Pitch += Mouse.DeltaY * 0.005f;
            }
            // Zoom
            if (Mouse.MiddleButtonPressed)
            {
                Distance -= (-Mouse.DeltaX + Mouse.DeltaY) * Distance / 1300; // The minus is because I'm a little perfectionist.
            }

            // Zoom
            Distance -= Mouse.WheelDelta * Distance / 1300;

            #endregion

            #region Gamepad

            LookAtPosition -= ((GameObject3D)Owner).Transform.Right * -GamePad.PlayerOne.LeftStickX * Distance / 5 * Time.GameDeltaTime;
            LookAtPosition += ((GameObject3D)Owner).Transform.Up * GamePad.PlayerOne.LeftStickY * Distance / 5 * Time.GameDeltaTime;
            // Orientation
            Yaw -= GamePad.PlayerOne.RightStickX * 1.5f * Time.GameDeltaTime;
            Pitch += GamePad.PlayerOne.RightStickY * 1.5f * Time.GameDeltaTime;
            // Distance or zoom
            Distance -= (GamePad.PlayerOne.RightTrigger - GamePad.PlayerOne.LeftTrigger) * Distance * Time.GameDeltaTime;

            #endregion

            #region Bounds

            // Orientation bounds
            if (Yaw >= twoPi)
            {
                Yaw -= twoPi;
            }
            if (Yaw < 0)
            {
                Yaw += twoPi;
            }
            if (Pitch >= twoPi)
            {
                Pitch -= twoPi;
            }
            if (Pitch < 0)
            {
                Pitch += twoPi;
            }
            // Distance bounds.
            if (Distance > ((GameObject3D)Owner).Camera.FarPlane)
                Distance = ((GameObject3D)Owner).Camera.FarPlane;
            if (Distance < ((GameObject3D)Owner).Camera.NearPlane)
                Distance = ((GameObject3D)Owner).Camera.NearPlane;

            #endregion

            // Calculate Rotation //
            Quaternion rotation = Quaternion.Identity;
            rotation *= Quaternion.CreateFromYawPitchRoll(0, Pitch, 0);
            rotation *= Quaternion.CreateFromYawPitchRoll(Yaw, 0, 0);
            rotation *= Quaternion.CreateFromYawPitchRoll(0, 0, Roll);
            // Its actually the invert...
            ((GameObject3D)Owner).Transform.Rotation = Quaternion.Inverse(rotation);
            // Now the position.
            Matrix rotationMatrix = Matrix.CreateFromQuaternion(rotation);   
            ((GameObject3D)Owner).Transform.Position = LookAtPosition + new Vector3(rotationMatrix.M13, rotationMatrix.M23, rotationMatrix.M33) * Distance;
        } // Update

        #endregion

    } // ScriptCustomCamera
} // XNAFinalEngineExamples

