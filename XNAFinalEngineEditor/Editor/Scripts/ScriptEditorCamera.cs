
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UserInterface;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Mouse = XNAFinalEngine.Input.Mouse;
using GamePad = XNAFinalEngine.Input.GamePad;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// Editor Camera. Simulate the camera of the program Softimage XSI.
    /// If the S key is pressed the camera is in manipulation mode.
    /// If the s key is just pressed, the camera enter in manipulation mode until the space or escape key is pressed.
    /// In manipulation mode if we press the left mouse button the look at position of the camera changes.
    ///                      if we press the right mouse button and move the mouse the orientation around the look at position changes.
    /// If we move the mouse wheel the zoom of distance to the look at position changes.
    /// </summary>
    public sealed class ScriptEditorCamera : Script
    {

        #region Enumerates

        /// <summary>
        /// This specified the key binding.
        /// </summary>
        public enum ModeType
        {
            Softimage,
            Maya,
            NoManipulationKey,
        } // ModeType

        #endregion

        #region Constants

        /// <summary>
        /// 2 Pi or 180 degrees.
        /// </summary>
        private const float twoPi = (float)Math.PI * 2;

        #endregion

        #region Variables

        /// <summary>
        /// Flags to register the S key functionality.
        /// If the S key is pressed the camera is in manipulation mode.
        /// If the S key is just pressed, the camera enter in manipulation mode until the space or escape key is pressed or the S key is pressed again.
        /// </summary>
        private bool sJustPressed;
        private static ScriptEditorCamera manipulating;
        private float sJustPressedTime;

        // Default values.
        private ModeType mode = ModeType.NoManipulationKey;
                
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
        
        /// <summary>
        /// This specified the key binding.
        /// Softimage: S key to activate manipulation mode. 
        ///            Left mouse button: translate the camera in the view plane.
        ///            Right mouse button: rotate the camera using the look at position as a pivot.
        /// Maya:      Alt to activate manipuation mode.
        ///            Left mouse button: rotate the camera using the look at position as a pivot.
        ///            Middle mouse button: translate the camera in the view plane.
        /// No manipulation key: it uses the Softimage binding.
        /// </summary>
        public ModeType Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                sJustPressed = false;
                if (Manipulating)
                    manipulating = null;
            }
        } // Mode

        /// <summary>
        /// The area in which the camera works.
        /// </summary>
        public Control ClientArea { get; set; }

        /// <summary>
        /// Is it the camera being manipulated?
        /// </summary>
        public bool Manipulating { get { return manipulating == this; } }

        /// <summary>
        /// Does it work on orthographic mode?
        /// </summary>
        public bool OrthographicMode { get; set; }
        
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
            if (!((GameObject3D)Owner).Camera.Enabled)
                return;
            if (CouldBeManipulated() || Manipulating)
            {
                if (mode == ModeType.Softimage || mode == ModeType.NoManipulationKey)
                {

                    #region Softimage mode

                    #region Is it in manipulation mode?

                    if (Manipulating)
                    {
                        // To exit the manipulation mode.
                        if (Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed)
                        {
                            manipulating = null;
                        }
                    }
                    // To enter or exit the manipulation mode using the s key.
                    if (Keyboard.KeyJustPressed(Keys.S))
                    {
                        sJustPressed = true;
                        sJustPressedTime = Time.ApplicationTime;
                    }
                    else
                    {
                        if (sJustPressed && !Keyboard.KeyPressed(Keys.S))
                        {
                            sJustPressed = false;
                            if (Time.ApplicationTime - sJustPressedTime < 0.3f)
                            {
                                manipulating = Manipulating ? null : this;
                            }
                        }
                    }

                    #endregion

                    #region Manipulate

                    if (Manipulating || Keyboard.KeyPressed(Keys.S) || mode == ModeType.NoManipulationKey)
                    {
                        // Translation
                        if (Mouse.LeftButtonPressed)
                        {
                            LookAtPosition -= ((GameObject3D)Owner).Transform.Right * Mouse.XMovement * Distance / 1000;
                            LookAtPosition += ((GameObject3D)Owner).Transform.Up    * Mouse.YMovement * Distance / 1000;
                        }
                        // Orientation
                        if (Mouse.RightButtonPressed && !OrthographicMode)
                        {
                            Yaw += Mouse.XMovement * 0.005f;
                            Pitch += Mouse.YMovement * 0.005f;
                        }
                        // Zoom
                        if (Mouse.MiddleButtonPressed)
                        {
                            Distance -= (-Mouse.XMovement + Mouse.YMovement) * Distance / 1300; // The minus is because I'm a little perfectionist.
                        }
                    }
                    // Zoom
                    Distance -= Mouse.WheelDelta * Distance / 1300;

                    #endregion

                    #endregion

                }
                else
                {

                    #region Maya mode

                    if ((Keyboard.KeyPressed(Keys.LeftAlt) || Keyboard.KeyPressed(Keys.RightAlt)) &&
                        (Mouse.LeftButtonPressed || Mouse.RightButtonPressed || Mouse.MiddleButtonPressed))
                    {
                        manipulating = this;
                        // Translation
                        if (Mouse.MiddleButtonPressed)
                        {
                            LookAtPosition -= ((GameObject3D)Owner).Transform.Right * Mouse.XMovement * Distance / 1000;
                            LookAtPosition += ((GameObject3D)Owner).Transform.Up * Mouse.YMovement * Distance / 1000;
                        }
                        // Orientation
                        if (Mouse.LeftButtonPressed && !OrthographicMode)
                        {
                            Yaw += Mouse.XMovement * 0.005f;
                            Pitch += Mouse.YMovement * 0.005f;
                        }
                        if (Mouse.RightButtonPressed)
                        {
                            // Distance or zoom
                            Distance -= (Mouse.XMovement + Mouse.YMovement) * Distance / 1300;
                        }
                    }
                    else
                    {
                        manipulating = null;
                    }
                    // Distance or zoom
                    Distance -= Mouse.WheelDelta * Distance / 1300; // 1300 is an arbitrary number.
                    
                    #endregion

                }
            }

            #region Gamepad

            LookAtPosition -= ((GameObject3D)Owner).Transform.Right * -GamePad.PlayerOne.LeftStickXMovement * Distance / 1000;
            LookAtPosition += ((GameObject3D)Owner).Transform.Up * GamePad.PlayerOne.LeftStickYMovement * Distance / 1000;
            // Orientation
            Yaw -= GamePad.PlayerOne.RightStickXMovement * 0.008f;
            Pitch += GamePad.PlayerOne.RightStickYMovement * 0.008f;
            // Distance or zoom
            Distance -= (GamePad.PlayerOne.RightTriggerMovement - GamePad.PlayerOne.LeftTriggerMovement) * Distance / 300;

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
            if (OrthographicMode)
                ((GameObject3D) Owner).Camera.OrthographicVerticalSize = Distance;
        } // Update

        #endregion

        #region Could Be Manipulated

        /// <summary>
        /// Indicates if the camera could perform a camera movement.
        /// </summary>
        internal bool CouldBeManipulated()
        {
            return UserInterfaceManager.IsOverThisControl(ClientArea, new Point(Mouse.Position.X, Mouse.Position.Y)) && manipulating == null &&
                   !Gizmo.Active /*&& !selectionRectangleBackground.LineRenderer.Enabled*/;
        } // CouldBeManipulated

        #endregion

    } // ScriptEditorCamera
} // XNAFinalEngine.Editor

