
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Components;
using XNAFinalEngine.Input;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;

#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// Rotation gizmo based in Softimage XSI.
    /// </summary>
    internal class RotationGizmo : Gizmo
    {

        #region Variables
                
        /// <summary>
        /// The gizmo's cones.
        /// </summary>
        private readonly GameObject3D redLine,
                                      greenLine,
                                      blueLine;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Rotation gizmo based in Softimage XSI.
        /// </summary>
        internal RotationGizmo()
        {
            // Create the gizmo parts.
            redLine   = new GameObject3D { Layer = Layer.GetLayerByNumber(31) };
            greenLine = new GameObject3D { Layer = Layer.GetLayerByNumber(31) };
            blueLine  = new GameObject3D { Layer = Layer.GetLayerByNumber(31) };
            redLine.AddComponent<LineRenderer>();
            greenLine.AddComponent<LineRenderer>();
            blueLine.AddComponent<LineRenderer>();
            const int numberOfPoints = 50;
            const int radius = 1;
            redLine.LineRenderer.Vertices = new VertexPositionColor[numberOfPoints * 2];
            greenLine.LineRenderer.Vertices = new VertexPositionColor[numberOfPoints * 2];
            blueLine.LineRenderer.Vertices = new VertexPositionColor[numberOfPoints * 2];
            redLine.LineRenderer.Vertices[0]   = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), Color.Red);
            greenLine.LineRenderer.Vertices[0] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), new Color(0, 1f, 0));
            blueLine.LineRenderer.Vertices[0]  = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), Color.Blue);
            for (int i = 1; i < numberOfPoints; i++)
            {
                float angle = i * (3.1416f * 2 / numberOfPoints);
                float x = (float)Math.Sin(angle) * radius;
                float y = (float)Math.Cos(angle) * radius;
                redLine.LineRenderer.Vertices[i * 2 - 1]       = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), Color.Red);
                greenLine.LineRenderer.Vertices[i * 2 - 1]     = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), new Color(0, 1f, 0));
                blueLine.LineRenderer.Vertices[i * 2 - 1]      = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), Color.Blue);
                redLine.LineRenderer.Vertices[i * 2]   = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), Color.Red);
                greenLine.LineRenderer.Vertices[i * 2] = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), new Color(0, 1f, 0));
                blueLine.LineRenderer.Vertices[i * 2]  = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), Color.Blue);
            }
            redLine.LineRenderer.Vertices[numberOfPoints * 2 - 1]   = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), Color.Red);
            greenLine.LineRenderer.Vertices[numberOfPoints * 2 - 1] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), new Color(0, 1f, 0));
            blueLine.LineRenderer.Vertices[numberOfPoints * 2 - 1]  = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), Color.Blue);
            
            redLine.LineRenderer.Enabled = false;
            greenLine.LineRenderer.Enabled = false;
            blueLine.LineRenderer.Enabled = false;
        } // RotationGizmo

        #endregion

        #region Enable and Disable Gizmo

        /// <summary>
        /// Enable the gizmo for manipulation.
        /// </summary>
        public void EnableGizmo(List<GameObject3D> _selectedObjects)
        {
            Active = false;

            redLine.LineRenderer.Enabled = true;
            greenLine.LineRenderer.Enabled = true;
            blueLine.LineRenderer.Enabled = true;

            selectedObject = _selectedObjects[0];
            selectedObjects = _selectedObjects;
            selectedObjectsLocalMatrix = new List<Matrix>(_selectedObjects.Count);
            foreach (GameObject3D selectedObj in _selectedObjects)
            {
                selectedObjectsLocalMatrix.Add(selectedObj.Transform.LocalMatrix);
            }
        } // EnableGizmo

        /// <summary>
        /// Disable the gizmo.
        /// </summary>
        public void DisableGizmo()
        {
            Active = false;

            redLine.LineRenderer.Enabled = false;
            greenLine.LineRenderer.Enabled = false;
            blueLine.LineRenderer.Enabled = false;

            selectedObject = null;
            selectedObjects = null;
            selectedObjectsLocalMatrix.Clear();
        } // DisableGizmo

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        internal void Update(GameObject3D gizmoCamera, Control clientArea)
        {

            #region Active

            if (Active)
            {
                // If the manipulation is over...
                if (!Mouse.LeftButtonPressed)
                {
                    Active = false;
                    // Store new previous matrix.
                    if (selectedObjects[0].Transform.LocalMatrix != selectedObjectsLocalMatrix[0])
                    {
                        using (Transaction.Create())
                        {
                            for (int i = 0; i < selectedObjects.Count; i++)
                            {
                                // I store the action on the undo system. It seems complex. But it is pretty simple actually.
                                Matrix oldMatrix = selectedObjectsLocalMatrix[i];
                                Matrix newMatrix = selectedObjects[i].Transform.LocalMatrix;
                                GameObject3D gameObject3D = selectedObjects[i];
                                ActionManager.CallMethod(
                                    // Redo
                                    delegate
                                    {
                                        gameObject3D.Transform.LocalMatrix = newMatrix;
                                    },
                                    // Undo
                                    delegate
                                    {
                                        gameObject3D.Transform.LocalMatrix = oldMatrix;
                                    });
                            }
                        }
                    }
                }
                // Transformate object...
                else
                {
                    Vector2 transformationAmount;
                    Vector3 transformation = Vector3.Zero;
                    // First we have to know how much to move the object in each axis.
                    if (redAxisSelected)
                    {
                        Calculate2DMouseDirection(selectedObject, gizmoCamera, new Vector3(0, -1, 0), out transformationAmount);
                        transformation.X =  (Mouse.XMovement * transformationAmount.X / 4.0f);
                        transformation.X += (Mouse.YMovement * transformationAmount.Y / 4.0f);
                    }
                    if (greenAxisSelected)
                    {
                        Calculate2DMouseDirection(selectedObject, gizmoCamera, new Vector3(1, 0, 0), out transformationAmount);
                        transformation.Y =  (Mouse.XMovement * transformationAmount.X / 4.0f);
                        transformation.Y += (Mouse.YMovement * transformationAmount.Y / 4.0f);
                    }
                    if (blueAxisSelected)
                    {
                        Calculate2DMouseDirection(selectedObject, gizmoCamera, new Vector3(0, 0, 1), out transformationAmount);
                        transformation.Z =  (Mouse.XMovement * transformationAmount.X / 4.0f);
                        transformation.Z += (Mouse.YMovement * transformationAmount.Y / 4.0f);
                    }
                    // Transform each object.
                    foreach (GameObject3D gameObject3D in selectedObjects)
                    {
                        gameObject3D.Transform.Rotate(transformation, Space == SpaceMode.Local ? Components.Space.Local : Components.Space.World);
                    }
                }
                if (Keyboard.EscapeJustPressed)
                {
                    Active = false;
                    // Revert transformation to all selected objects.
                    for (int i = 0; i < selectedObjects.Count; i++)
                    {
                        selectedObjects[i].Transform.LocalMatrix = selectedObjectsLocalMatrix[i];
                    }
                    
                }
            }

            #endregion

            #region Inactive
            
            else            
            {
                // If we press the left mouse button the manipulator activates.
                if (Mouse.LeftButtonJustPressed)
                {
                    Active = true;
                    // Stores initial matrix because maybe the user press escape; i.e. maybe he cancel the transformation.
                    for (int i = 0; i < selectedObjects.Count; i++)
                    {
                        selectedObjectsLocalMatrix[i] = selectedObjects[i].Transform.LocalMatrix;
                    }
                }

                // Perform a pick around the mouse pointer.

                Viewport viewport = new Viewport(gizmoCamera.Camera.Viewport.X + clientArea.ControlLeftAbsoluteCoordinate,
                                                 gizmoCamera.Camera.Viewport.Y + clientArea.ControlTopAbsoluteCoordinate,
                                                 gizmoCamera.Camera.Viewport.Width, gizmoCamera.Camera.Viewport.Height);
                Picker.BeginManualPicking(gizmoCamera.Camera.ViewMatrix, gizmoCamera.Camera.ProjectionMatrix, viewport);
                RenderGizmoForPicker(gizmoCamera);
                Color[] colorArray = Picker.EndManualPicking(new Rectangle(Mouse.Position.X - 5, Mouse.Position.Y - 5, RegionSize, RegionSize));

                #region Find Selected Axis

                redAxisSelected   = false;
                greenAxisSelected = false;
                blueAxisSelected  = false;
                for (int i = 0; i < RegionSize * RegionSize; i++)
                {
                    // X axis.
                    if (colorArray[i].R == 255 && colorArray[i].G == 0 && colorArray[i].B == 0)
                    {
                        redAxisSelected   = true;
                        greenAxisSelected = false;
                        blueAxisSelected  = false;
                        break;
                    }
                    // Y axis.
                    if (colorArray[i].R == 0 && colorArray[i].G == 255 && colorArray[i].B == 0)
                    {
                        redAxisSelected   = false;
                        greenAxisSelected = true;
                        blueAxisSelected  = false;
                        break;
                    }
                    // Z axis.
                    if (colorArray[i].R == 0 && colorArray[i].G == 0 && colorArray[i].B == 255)
                    {
                        redAxisSelected   = false;
                        greenAxisSelected = false;
                        blueAxisSelected  = true;
                        break;
                    }
                }

                #endregion

            }
            
            #endregion

        } // Update

        #endregion
        
        #region Render Gizmo For Picker
        
        /// <summary>
        /// Render the gizmo to the picker.
        /// </summary>
        private void RenderGizmoForPicker(GameObject3D gizmoCamera)
        {
            // Calculate the center, scale and orientation of the gizmo.
            Vector3 center;
            Quaternion orientation;
            float scale;
            GizmoScaleCenterOrientation(selectedObject, gizmoCamera, out scale, out center, out orientation);

            // Calculate the gizmo matrix.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= Matrix.CreateFromQuaternion(orientation);
            transformationMatrix *= Matrix.CreateTranslation(center);
                       
            // Update Cones
            redLine.Transform.LocalMatrix = Matrix.Identity;
            redLine.Transform.Rotate(new Vector3(0, -90, 0));
            redLine.Transform.LocalMatrix = redLine.Transform.LocalMatrix * transformationMatrix;
            Picker.RenderObjectToPicker(redLine, Color.Red);
            
            greenLine.Transform.LocalMatrix = Matrix.Identity;
            greenLine.Transform.Rotate(new Vector3(90, 0, 0));
            greenLine.Transform.LocalMatrix = greenLine.Transform.LocalMatrix * transformationMatrix;
            Picker.RenderObjectToPicker(greenLine, new Color(0, 255, 0)); // Color.Green is not 0, 255, 0
            
            blueLine.Transform.LocalMatrix = Matrix.Identity;
            blueLine.Transform.LocalMatrix = blueLine.Transform.LocalMatrix * transformationMatrix;
            Picker.RenderObjectToPicker(blueLine, Color.Blue);
        } // RenderGizmoForPicker

        #endregion

        #region Render Gizmo

        /// <summary>
        /// Update Rendering Information.
        /// </summary>
        public void UpdateRenderingInformation(GameObject3D gizmoCamera)
        {

            #region Find Color

            Color redColor   = Color.Red;
            Color greenColor = new Color(0, 1f, 0);
            Color blueColor  = Color.Blue;

            for (int i = 1; i < 100; i++)
            {
                redLine.LineRenderer.Vertices[i].Color   = redAxisSelected ? Color.Yellow : redColor;
                greenLine.LineRenderer.Vertices[i].Color = greenAxisSelected ? Color.Yellow : greenColor;
                blueLine.LineRenderer.Vertices[i].Color  = blueAxisSelected ? Color.Yellow : blueColor;
            }

            #endregion

            // Calculate the center, scale and orientation of the gizmo.
            Vector3 center;
            Quaternion orientation;
            float scale;
            GizmoScaleCenterOrientation(selectedObject, gizmoCamera, out scale, out center, out orientation);
            
            // Calculate the gizmo matrix.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= Matrix.CreateFromQuaternion(orientation);
            transformationMatrix *= Matrix.CreateTranslation(center);
            
            // Update Cones
            redLine.Transform.LocalMatrix = Matrix.Identity;
            redLine.Transform.Rotate(new Vector3(0, -90, 0));
            redLine.Transform.LocalMatrix = redLine.Transform.LocalMatrix * transformationMatrix;
            
            greenLine.Transform.LocalMatrix = Matrix.Identity;
            greenLine.Transform.Rotate(new Vector3(90, 0, 0));
            greenLine.Transform.LocalMatrix = greenLine.Transform.LocalMatrix * transformationMatrix;
            
            blueLine.Transform.LocalMatrix = Matrix.Identity;
            blueLine.Transform.LocalMatrix = blueLine.Transform.LocalMatrix * transformationMatrix;
        } // UpdateRenderingInformation

        #endregion
        
    } // RotationGizmo
} // XNAFinalEngine.Editor
