
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Mouse = XNAFinalEngine.Input.Mouse;
using Plane = XNAFinalEngine.Assets.Plane;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// Scale gizmo based in Softimage XSI.
    /// </summary>
    internal class ScaleGizmo : Gizmo
    {

        #region Variables
                
        /// <summary>
        /// The gizmo's cones.
        /// </summary>
        private readonly GameObject3D redBox,
                                      greenBox,
                                      blueBox,
                                      lines;

        
        /// <summary>
        /// This planes are not rendered to the screen.
        /// They are used by the picker to select two axis.
        /// </summary>
        private readonly GameObject3D planeRedGreen, planeRedGreenInv,
                                      planeGreenBlue, planeGreenBlueInv,
                                      planeBlueRed, planeBlueRedInv;
        private readonly GameObject2D planeAll;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Scale gizmo based in Softimage XSI.
        /// </summary>
        internal ScaleGizmo()
        {
            // Create the gizmo parts.
            Box box = new Box(0.15f, 0.15f, 0.15f);
            redBox   = new GameObject3D(box, new Constant()) { Layer = Layer.GetLayerByNumber(31) };
            greenBox = new GameObject3D(box, new Constant()) { Layer = Layer.GetLayerByNumber(31) };
            blueBox  = new GameObject3D(box, new Constant()) { Layer = Layer.GetLayerByNumber(31) };
            lines = new GameObject3D { Layer = Layer.GetLayerByNumber(31), };
            lines.AddComponent<LineRenderer>();
            lines.LineRenderer.Vertices = new VertexPositionColor[6];
            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(1, 0, 0);
            vertices[2] = new Vector3(0, 1, 0);
            vertices[3] = new Vector3(0, 0, 1);
            vertices[4] = new Vector3(1, 1, 0);
            vertices[5] = new Vector3(0, 1, 1);
            vertices[6] = new Vector3(1, 0, 1);
            planeRedGreen     = new GameObject3D(new Plane(vertices[2], vertices[0], vertices[4], vertices[1]), new Constant { DiffuseColor = new Color(255, 255, 0) }) { Layer = Layer.GetLayerByNumber(31), };
            planeGreenBlue    = new GameObject3D(new Plane(vertices[5], vertices[3], vertices[2], vertices[0]), new Constant { DiffuseColor = new Color(0, 255, 255) }) { Layer = Layer.GetLayerByNumber(31), };
            planeBlueRed      = new GameObject3D(new Plane(vertices[0], vertices[3], vertices[1], vertices[6]), new Constant { DiffuseColor = new Color(255, 0, 255) }) { Layer = Layer.GetLayerByNumber(31), };
            planeRedGreenInv  = new GameObject3D(new Plane(vertices[4], vertices[1], vertices[2], vertices[0]), new Constant { DiffuseColor = new Color(255, 255, 0) }) { Layer = Layer.GetLayerByNumber(31), };
            planeGreenBlueInv = new GameObject3D(new Plane(vertices[2], vertices[0], vertices[5], vertices[3]), new Constant { DiffuseColor = new Color(0, 255, 255) }) { Layer = Layer.GetLayerByNumber(31), };
            planeBlueRedInv   = new GameObject3D(new Plane(vertices[1], vertices[6], vertices[0], vertices[3]), new Constant { DiffuseColor = new Color(255, 0, 255) }) { Layer = Layer.GetLayerByNumber(31), };
            planeAll = new GameObject2D { Layer = Layer.GetLayerByNumber(31), };
            planeAll.AddComponent<LineRenderer>();
            planeAll.LineRenderer.Vertices = new VertexPositionColor[8];
            
            redBox.ModelRenderer.Enabled = false;
            greenBox.ModelRenderer.Enabled = false;
            blueBox.ModelRenderer.Enabled = false;
            lines.LineRenderer.Enabled = false;
            planeRedGreen.ModelRenderer.Enabled = false;
            planeGreenBlue.ModelRenderer.Enabled = false;
            planeBlueRed.ModelRenderer.Enabled = false;
            planeRedGreenInv.ModelRenderer.Enabled = false;
            planeGreenBlueInv.ModelRenderer.Enabled = false;
            planeBlueRedInv.ModelRenderer.Enabled = false;
            planeAll.LineRenderer.Enabled = false;
        } // ScaleGizmo

        #endregion

        #region Enable and Disable Gizmo

        /// <summary>
        /// Enable the gizmo for manipulation.
        /// </summary>
        public void EnableGizmo(List<GameObject3D> _selectedObjects)
        {
            Active = false;

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
                    Vector3 transformation = new Vector3(0, 0, 0);
                    // First we have to know how much to move the object in each axis.
                    if (redAxisSelected)
                    {
                        Calculate2DMouseDirection(selectedObject, gizmoCamera, new Vector3(1, 0, 0), out transformationAmount);
                        transformation.X = (Mouse.DraggingAmount.X * transformationAmount.X / (float)Screen.Width * 10);
                        transformation.X += (Mouse.DraggingAmount.Y * transformationAmount.Y / (float)Screen.Height * 10);
                    }
                    if (greenAxisSelected)
                    {
                        Calculate2DMouseDirection(selectedObject, gizmoCamera, new Vector3(0, 1, 0), out transformationAmount);
                        transformation.Y = (Mouse.DraggingAmount.X * transformationAmount.X / (float)Screen.Width * 10);
                        transformation.Y += (Mouse.DraggingAmount.Y * transformationAmount.Y / (float)Screen.Height * 10);
                    }
                    if (blueAxisSelected)
                    {
                        Calculate2DMouseDirection(selectedObject, gizmoCamera, new Vector3(0, 0, 1), out transformationAmount);
                        transformation.Z = (Mouse.DraggingAmount.X * transformationAmount.X / (float)Screen.Width * 10);
                        transformation.Z += (Mouse.DraggingAmount.Y * transformationAmount.Y / (float)Screen.Height * 10);
                    }
                    if (redAxisSelected && greenAxisSelected && blueAxisSelected)
                    {
                        transformation.X = Mouse.DraggingAmount.X / (float)Screen.Width * 10;
                        transformation.Y = transformation.X;
                        transformation.Z = transformation.X;
                    }
                    else if (redAxisSelected && greenAxisSelected)
                        transformation.Y = transformation.X = transformation.X + transformation.Y / 2;
                    else if (redAxisSelected && blueAxisSelected)
                        transformation.Z = transformation.X = transformation.X + transformation.Z / 2;
                    else if (blueAxisSelected && greenAxisSelected)
                        transformation.Y = transformation.Z = transformation.Z + transformation.Y / 2;
                    // Transform each object.
                    for (int i = 0; i < selectedObjects.Count; i++)
                    {
                        GameObject3D gameObject3D = selectedObjects[i];
                        Vector3 position;
                        Quaternion orientation;
                        Vector3 scale;
                        selectedObjectsLocalMatrix[i].Decompose(out scale, out orientation, out position);
                        gameObject3D.Transform.LocalScale = transformation * scale + scale;
                    }
                }
                if (Keyboard.KeyJustPressed(Keys.Escape))
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
                if (!Keyboard.KeyPressed(Keys.LeftAlt) && !Keyboard.KeyPressed(Keys.RightAlt))
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

                    redAxisSelected = true;
                    greenAxisSelected = true;
                    blueAxisSelected = true;
                    bool allAxis = false;
                    for (int i = 0; i < RegionSize * RegionSize; i++)
                    {
                        // This is the order of preference:
                        //  1) All axis.
                        //  2) 1 axis.
                        //  3) 2 axis.
                    
                        if (redAxisSelected && greenAxisSelected && blueAxisSelected)
                        {
                            // X and Y axis.
                            if (colorArray[i].R == 255 && colorArray[i].G == 255 && colorArray[i].B == 0)
                            {
                                redAxisSelected   = true;
                                greenAxisSelected = true;
                                blueAxisSelected  = false;
                            }
                            // X and Z  axis.
                            if (colorArray[i].R == 255 && colorArray[i].G == 0 && colorArray[i].B == 255)
                            {
                                redAxisSelected   = true;
                                greenAxisSelected = false;
                                blueAxisSelected  = true;
                            }
                            // Y and Z axis.
                            if (colorArray[i].R == 0 && colorArray[i].G == 255 && colorArray[i].B == 255)
                            {
                                redAxisSelected   = false;
                                greenAxisSelected = true;
                                blueAxisSelected  = true;
                            }
                        }
                        // X axis.
                        if (colorArray[i].R == 255 && colorArray[i].G == 0 && colorArray[i].B == 0)
                        {
                            redAxisSelected   = true;
                            greenAxisSelected = false;
                            blueAxisSelected  = false;
                        }
                        // Y axis.
                        if (colorArray[i].R == 0 && colorArray[i].G == 255 && colorArray[i].B == 0)
                        {
                            redAxisSelected   = false;
                            greenAxisSelected = true;
                            blueAxisSelected  = false;
                        }
                        // Z axis.
                        if (colorArray[i].R == 0 && colorArray[i].G == 0 && colorArray[i].B == 255)
                        {
                            redAxisSelected   = false;
                            greenAxisSelected = false;
                            blueAxisSelected  = true;
                        }
                        // All axis.
                        if (colorArray[i].R == 255 && colorArray[i].G == 255 && colorArray[i].B == 255)
                        {
                            redAxisSelected   = true;
                            greenAxisSelected = true;
                            blueAxisSelected  = true;
                            allAxis = true;
                            i = RegionSize * RegionSize; // Or break.
                        }
                    }

                    if (redAxisSelected && greenAxisSelected && blueAxisSelected && !allAxis)
                    {
                        redAxisSelected = false;
                        greenAxisSelected = false;
                        blueAxisSelected = false;
                    }

                #endregion

                }
                else
                {
                    redAxisSelected = false;
                    greenAxisSelected = false;
                    blueAxisSelected = false;
                }
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
            
            vertices[0] = Vector3.Transform(new Vector3(0, 0, 0), transformationMatrix);
            vertices[1] = Vector3.Transform(new Vector3(1, 0, 0), transformationMatrix);
            vertices[2] = Vector3.Transform(new Vector3(0, 1, 0), transformationMatrix);
            vertices[3] = Vector3.Transform(new Vector3(0, 0, 1), transformationMatrix);
            vertices[4] = Vector3.Transform(new Vector3(1, 1, 0), transformationMatrix);
            vertices[5] = Vector3.Transform(new Vector3(0, 1, 1), transformationMatrix);
            vertices[6] = Vector3.Transform(new Vector3(1, 0, 1), transformationMatrix);
            
            planeRedGreen.Transform .LocalMatrix = transformationMatrix;
            planeGreenBlue.Transform .LocalMatrix = transformationMatrix;
            planeBlueRed.Transform .LocalMatrix = transformationMatrix;
            Picker.RenderObjectToPicker(planeRedGreen, new Color(255, 255, 0));
            Picker.RenderObjectToPicker(planeGreenBlue, new Color(0, 255, 255));
            Picker.RenderObjectToPicker(planeBlueRed, new Color(255, 0, 255));
            // Render a second time but from the other side.
            planeRedGreenInv.Transform .LocalMatrix = transformationMatrix;
            planeGreenBlueInv.Transform.LocalMatrix = transformationMatrix;
            planeBlueRedInv.Transform.LocalMatrix = transformationMatrix;
            Picker.RenderObjectToPicker(planeRedGreenInv, new Color(255, 255, 0));
            Picker.RenderObjectToPicker(planeGreenBlueInv, new Color(0, 255, 255));
            Picker.RenderObjectToPicker(planeBlueRedInv, new Color(255, 0, 255));

            EngineManager.Device.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

            // Update Axis Lines
            lines.LineRenderer.Vertices[0] = new VertexPositionColor(vertices[0], Color.Red);
            lines.LineRenderer.Vertices[1] = new VertexPositionColor(vertices[1], Color.Red);
            lines.LineRenderer.Vertices[2] = new VertexPositionColor(vertices[0], new Color(0, 255, 0));
            lines.LineRenderer.Vertices[3] = new VertexPositionColor(vertices[2], new Color(0, 255, 0));
            lines.LineRenderer.Vertices[4] = new VertexPositionColor(vertices[0], Color.Blue);
            lines.LineRenderer.Vertices[5] = new VertexPositionColor(vertices[3], Color.Blue);
            Picker.RenderObjectToPicker(lines);
            
            // Update Cones
            redBox.Transform.LocalMatrix = Matrix.Identity;
            redBox.Transform.LocalPosition = new Vector3(1, 0, 0);
            redBox.Transform.Rotate(new Vector3(0, 0, -90));
            redBox.Transform.LocalMatrix = redBox.Transform.LocalMatrix * transformationMatrix;
            Picker.RenderObjectToPicker(redBox, Color.Red);
            
            greenBox.Transform.LocalMatrix = Matrix.Identity;
            greenBox.Transform.LocalPosition = new Vector3(0, 1, 0);
            greenBox.Transform.LocalMatrix = greenBox.Transform.LocalMatrix * transformationMatrix;
            Picker.RenderObjectToPicker(greenBox, new Color(0, 255, 0)); // Color.Green is not 0, 255, 0
            
            blueBox.Transform.LocalMatrix = Matrix.Identity;
            blueBox.Transform.LocalPosition = new Vector3(0, 0, 1);
            blueBox.Transform.Rotate(new Vector3(90, 0, 0));
            blueBox.Transform.LocalMatrix = blueBox.Transform.LocalMatrix * transformationMatrix;
            Picker.RenderObjectToPicker(blueBox, Color.Blue);
            
            Vector3 screenPositions = EngineManager.Device.Viewport.Project(vertices[0], gizmoCamera.Camera.ProjectionMatrix, gizmoCamera.Camera.ViewMatrix, Matrix.Identity);
            planeAll.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), Color.White);
            planeAll.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), Color.White);
            planeAll.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), Color.White);
            planeAll.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), Color.White);
            planeAll.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), Color.White);
            planeAll.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), Color.White);
            planeAll.LineRenderer.Vertices[6] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), Color.White);
            planeAll.LineRenderer.Vertices[7] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), Color.White);
            Picker.RenderObjectToPicker(planeAll, Color.White);
        } // RenderGizmoForPicker

        #endregion

        #region Render Gizmo

        /// <summary>
        /// Render Gizmo..
        /// </summary>
        public void RenderGizmo(GameObject3D gizmoCamera, Control clientArea, EditorViewport.ViewportMode mode)
        {

            #region Find Color

            Color redColor   = Color.Red;
            Color greenColor = new Color(0, 1f, 0);
            Color blueColor  = Color.Blue;

            // If the manipulation is uniform then the axis are not yellow.
            if (!redAxisSelected || !greenAxisSelected || !blueAxisSelected) 
            {
                if (redAxisSelected)   redColor   = Color.Yellow;
                if (greenAxisSelected) greenColor = Color.Yellow;
                if (blueAxisSelected)  blueColor  = Color.Yellow;
            }

            if (mode == EditorViewport.ViewportMode.Front)
                blueColor = Color.Transparent;
            if (mode == EditorViewport.ViewportMode.Top)
                greenColor = Color.Transparent;
            if (mode == EditorViewport.ViewportMode.Right)
                redColor = Color.Transparent;

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
            
            // Scale the scale gizmo.
            Vector3 objectPosition;
            Quaternion objectOrientation;
            Vector3 objectScale = new Vector3(1, 1, 1);
            if (Active)
            {
                selectedObjectsLocalMatrix[0].Decompose(out objectScale, out objectOrientation, out objectPosition);
                objectScale = selectedObjects[0].Transform.LocalScale / objectScale;
            }

            // This are the axis line's vertex
            vertices[0] = Vector3.Transform(new Vector3(0, 0, 0), transformationMatrix);
            vertices[1] = Vector3.Transform(new Vector3(objectScale.X, 0, 0), transformationMatrix);
            vertices[2] = Vector3.Transform(new Vector3(0, objectScale.Y, 0), transformationMatrix);
            vertices[3] = Vector3.Transform(new Vector3(0, 0, objectScale.Z), transformationMatrix);

            // The plane used to select all axis.
            Color planeColor;
            if (redAxisSelected && greenAxisSelected && blueAxisSelected)
            {
                planeColor = Color.Yellow;
            }
            else
                planeColor = Color.Gray;

            EngineManager.Device.Viewport = new Viewport(gizmoCamera.Camera.Viewport.X + clientArea.ControlLeftAbsoluteCoordinate,
                                                         gizmoCamera.Camera.Viewport.Y + clientArea.ControlTopAbsoluteCoordinate,
                                                         gizmoCamera.Camera.Viewport.Width, gizmoCamera.Camera.Viewport.Height);
            Vector3 screenPositions = EngineManager.Device.Viewport.Project(vertices[0], gizmoCamera.Camera.ProjectionMatrix, gizmoCamera.Camera.ViewMatrix, Matrix.Identity);
            planeAll.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[6] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[7] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
            LineManager.Begin2D(planeAll.LineRenderer.PrimitiveType);
                for (int j = 0; j < planeAll.LineRenderer.Vertices.Length; j++)
                    LineManager.AddVertex(planeAll.LineRenderer.Vertices[j].Position, planeAll.LineRenderer.Vertices[j].Color);
            LineManager.End();

            // Update Axis Lines
            lines.LineRenderer.Vertices[0] = new VertexPositionColor(vertices[0], redColor);
            lines.LineRenderer.Vertices[1] = new VertexPositionColor(vertices[1], redColor);
            lines.LineRenderer.Vertices[2] = new VertexPositionColor(vertices[0], greenColor);
            lines.LineRenderer.Vertices[3] = new VertexPositionColor(vertices[2], greenColor);
            lines.LineRenderer.Vertices[4] = new VertexPositionColor(vertices[0], blueColor);
            lines.LineRenderer.Vertices[5] = new VertexPositionColor(vertices[3], blueColor);
            LineManager.Begin3D(lines.LineRenderer.PrimitiveType, gizmoCamera.Camera.ViewMatrix, gizmoCamera.Camera.ProjectionMatrix);
                for (int j = 0; j < lines.LineRenderer.Vertices.Length; j++)
                    LineManager.AddVertex(lines.LineRenderer.Vertices[j].Position, lines.LineRenderer.Vertices[j].Color);
            LineManager.End();
            
            // Update Cones
            redBox.Transform.LocalMatrix = Matrix.Identity;
            redBox.Transform.LocalPosition = new Vector3(objectScale.X, 0, 0);
            redBox.Transform.Rotate(new Vector3(0, 0, -90));
            redBox.Transform.LocalMatrix = redBox.Transform.LocalMatrix * transformationMatrix;
            constantShader.Resource.Parameters["diffuseColor"].SetValue(new Vector3(redColor.R / 255f, redColor.G / 255f, redColor.B / 255f));
            constantShader.Resource.Parameters["worldViewProj"].SetValue(redBox.Transform.WorldMatrix * gizmoCamera.Camera.ViewMatrix * gizmoCamera.Camera.ProjectionMatrix);
            constantShader.Resource.CurrentTechnique.Passes[0].Apply();
            redBox.ModelFilter.Model.Render();
            
            greenBox.Transform.LocalMatrix = Matrix.Identity;
            greenBox.Transform.LocalPosition = new Vector3(0, objectScale.Y, 0);
            greenBox.Transform.LocalMatrix = greenBox.Transform.LocalMatrix * transformationMatrix;
            constantShader.Resource.Parameters["diffuseColor"].SetValue(new Vector3(greenColor.R / 255f, greenColor.G / 255f, greenColor.B / 255f));
            constantShader.Resource.Parameters["worldViewProj"].SetValue(greenBox.Transform.WorldMatrix * gizmoCamera.Camera.ViewMatrix * gizmoCamera.Camera.ProjectionMatrix);
            constantShader.Resource.CurrentTechnique.Passes[0].Apply();
            greenBox.ModelFilter.Model.Render();
            
            blueBox.Transform.LocalMatrix = Matrix.Identity;
            blueBox.Transform.LocalPosition = new Vector3(0, 0, objectScale.Z);
            blueBox.Transform.Rotate(new Vector3(90, 0, 0));
            blueBox.Transform.LocalMatrix = blueBox.Transform.LocalMatrix * transformationMatrix;
            constantShader.Resource.Parameters["diffuseColor"].SetValue(new Vector3(blueColor.R / 255f, blueColor.G / 255f, blueColor.B / 255f));
            constantShader.Resource.Parameters["worldViewProj"].SetValue(blueBox.Transform.WorldMatrix * gizmoCamera.Camera.ViewMatrix * gizmoCamera.Camera.ProjectionMatrix);
            constantShader.Resource.CurrentTechnique.Passes[0].Apply();
            blueBox.ModelFilter.Model.Render();

            EngineManager.Device.Viewport = new Viewport(0, 0, Screen.Width, Screen.Height);
        } // UpdateRenderingInformation

        #endregion
        
    } // ScaleGizmo
} // XNAFinalEngine.Editor
