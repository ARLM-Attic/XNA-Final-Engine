
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Input;
using Plane = XNAFinalEngine.Assets.Plane;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// Translation gizmo based in Softimage XSI.
    /// </summary>
    internal class TranslationGizmo : Gizmo
    {

        #region Variables
                
        /// <summary>
        /// The gizmo's cones.
        /// </summary>
        private static GameObject3D redCone,
                                    greenCone,
                                    blueCone,
                                    lines;

        
        /// <summary>
        /// Los planos que marcan la conjuncion de dos ejes. Son invisibles en el renderizado.
        /// Pero visibles para el picker (aunque con menor prioridad que el resto, por eso se borra el zbuffer cuando se los renderiza)
        /// </summary>
        private static GameObject3D planeRedGreen,
                                    planeGreenBlue,
                                    planeBlueRed;
        private static GameObject2D planeAll;
         
        /// <summary>
        /// Store the position previous to the manipulation.
        /// </summary>
        private static Vector3 initialTranslation = Vector3.Zero;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Translation gizmo based in Softimage XSI.
        /// </summary>
        internal TranslationGizmo()
        {
            if (redCone != null)
                return;
            Cone cone = new Cone(0.1f, 0.2f, 10);
            redCone   = new GameObject3D(cone, new Constant()) { Layer = Layer.GetLayerByNumber(31) };
            greenCone = new GameObject3D(cone, new Constant()) { Layer = Layer.GetLayerByNumber(31) };
            blueCone  = new GameObject3D(cone, new Constant()) { Layer = Layer.GetLayerByNumber(31) };
            lines = new GameObject3D { Layer = Layer.GetLayerByNumber(31), };
            lines.AddComponent<LineRenderer>();
            lines.LineRenderer.Vertices = new VertexPositionColor[6];
            planeRedGreen = new GameObject3D(null, new Constant { DiffuseColor = new Color(255, 255, 0) }) { Layer = Layer.GetLayerByNumber(31), }; ;
            planeGreenBlue = new GameObject3D(null, new Constant { DiffuseColor = new Color(0, 255, 255) }) { Layer = Layer.GetLayerByNumber(31), }; ;
            planeBlueRed = new GameObject3D(null, new Constant { DiffuseColor = new Color(255, 0, 255) }) { Layer = Layer.GetLayerByNumber(31), }; ;
            planeAll = new GameObject2D { Layer = Layer.GetLayerByNumber(31), };
            planeAll.AddComponent<LineRenderer>();
            planeAll.LineRenderer.Vertices = new VertexPositionColor[8];

            redCone.ModelRenderer.Visible = false;
            greenCone.ModelRenderer.Visible = false;
            blueCone.ModelRenderer.Visible = false;
            lines.LineRenderer.Visible = false;
            planeRedGreen.ModelRenderer.Visible = false;
            planeGreenBlue.ModelRenderer.Visible = false;
            planeBlueRed.ModelRenderer.Visible = false;
        } // GizmoTranslation

        #endregion

        #region Enable and Disable Gizmo

        /// <summary>
        /// Enable the gizmo for manipulation.
        /// </summary>
        public void EnableGizmo(GameObject _selectedObject, Picker picker)
        {
            Active = false;

            redCone.ModelRenderer.Visible = true;
            greenCone.ModelRenderer.Visible = true;
            blueCone.ModelRenderer.Visible = true;
            lines.LineRenderer.Visible = true;

            Gizmo.picker = picker;

            selectedObject = _selectedObject;
        } // EnableGizmo

        /// <summary>
        /// Disable the gizmo.
        /// </summary>
        public void DisableGizmo()
        {
            Active = false;

            redCone.ModelRenderer.Visible = false;
            greenCone.ModelRenderer.Visible = false;
            blueCone.ModelRenderer.Visible = false;
            lines.LineRenderer.Visible = false;
        } // DisableGizmo

        #endregion

        #region Update

        /// <summary>
        /// 
        /// </summary>
        internal void Update()
        {
            Vector3 translation = new Vector3(0, 0, 0);            

            #region Active

            if (Active)
            {   
                
                // Si se termino de manipular
                if (!Mouse.LeftButtonPressed)
                {
                    Active = false;
                    if (initialTranslation != ((GameObject3D)selectedObject).Transform.LocalPosition)
                        produceTransformation = true;
                    initialTranslation = ((GameObject3D)selectedObject).Transform.LocalPosition;
                }
                // Si lo manipulamos actualizamos el escalado segun el desplazamaiento del mouse
                else
                {
                    if (redAxisSelected)
                    {
                        translation.X = (Mouse.DraggingAmount.X * transformationAmount.X / (float)Screen.Width * 10);
                        translation.X += (Mouse.DraggingAmount.Y * transformationAmount.Y / (float)Screen.Height * 10);
                    }
                    if (greenAxisSelected)
                    {
                        translation.Y = (Mouse.DraggingAmount.X * transformationAmount.X / (float)Screen.Width * 10);
                        translation.Y += (Mouse.DraggingAmount.Y * transformationAmount.Y / (float)Screen.Height * 10);
                    }
                    if (blueAxisSelected)
                    {
                        translation.Z = (Mouse.DraggingAmount.X * transformationAmount.X / (float)Screen.Width * 10);
                        translation.Z += (Mouse.DraggingAmount.Y * transformationAmount.Y / (float)Screen.Height * 10);
                    }
                    ((GameObject3D)selectedObject).Transform.LocalPosition = initialTranslation;
                    // Calculate the distance from the object to camera position.
                    Vector3 center;
                    Quaternion orientation;
                    GizmoCenterAndOrientation(out center, out orientation);
                    Vector3 cameraToCenter = GizmoCamera.Camera.Position - center;
                    float distanceToCamera = cameraToCenter.Length();
                    float scale = distanceToCamera / 14;
                    ((GameObject3D)selectedObject).Transform.Translate(translation * scale);
                }
                if (Keyboard.EscapeJustPressed)
                {
                    Active = false;
                    ((GameObject3D)selectedObject).Transform.LocalPosition = initialTranslation;
                }
            }

            #endregion

            #region Inactive
            
            else            
            {
                // Si apretamos el boton del mouse el manipulador se activa
                if (Mouse.LeftButtonJustPressed)
                {
                    Active = true;
                    //oldLocalMatrix = obj.LocalMatrix;
                    Calculate2DMouseDirection();
                }
                picker.BeginManualPicking(GizmoCamera.Camera.ViewMatrix, GizmoCamera.Camera.ProjectionMatrix);
                    RenderGizmoForPicker();
                Color[] colorArray = picker.EndManualPicking(new Rectangle(Mouse.Position.X, Mouse.Position.Y, RegionSize, RegionSize));

                #region Find Axis

                redAxisSelected   = true;
                greenAxisSelected = true;
                blueAxisSelected  = true;
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
                        i = RegionSize * RegionSize; // Or break.
                    }
                }

                #endregion
            }
            
            #endregion

            UpdateRenderingInformation();

        } // ManipulateObject

        #endregion

        #region Calculate2DMouseDirection

        /// <summary>
        /// Calculamos como va a afectar el movimiento vertical y horizontal del mouse al manipulador.
        /// </summary>
        protected static void Calculate2DMouseDirection()
        {
            // Calculate gizmo center and orientation.
            Vector3 center;
            Quaternion orientation;
            GizmoCenterAndOrientation(out center, out orientation);

            // Calculate the distance from the object to camera position.
            Vector3 cameraToCenter = GizmoCamera.Camera.Position - center;
            float distanceToCamera = cameraToCenter.Length();
            float scale = distanceToCamera / 14;

            // Calculate the gizmo matrix.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= Matrix.CreateFromQuaternion(orientation);
            transformationMatrix *= Matrix.CreateTranslation(center);

            vertices[0] = Vector3.Transform(new Vector3(0, 0, 0), transformationMatrix);
            // All and Red Axis
            if ((redAxisSelected && greenAxisSelected && blueAxisSelected) ||
                (redAxisSelected && !greenAxisSelected && !blueAxisSelected))
            {
                vertices[1] = Vector3.Transform(new Vector3(1, 0, 0), transformationMatrix);
            }
            // Green Axis
            if (!redAxisSelected && greenAxisSelected && !blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(0, 1, 0), transformationMatrix);
            }
            // Blue Axis
            if (!redAxisSelected && !greenAxisSelected && blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(0, 0, 1), transformationMatrix);
            }
            // Red and Green Axis
            if (redAxisSelected && greenAxisSelected && !blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(1, 1, 0), transformationMatrix);
            }
            // Green and Blue Axis
            if (!redAxisSelected && greenAxisSelected && blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(0, 1, 1), transformationMatrix);
            }
            // Red and Blue Axis
            if (redAxisSelected && greenAxisSelected && !blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(1, 0, 1), transformationMatrix);
            }

            Vector3[] screenPositions = new Vector3[2];
            screenPositions[0] = EngineManager.Device.Viewport.Project(vertices[0], GizmoCamera.Camera.ProjectionMatrix, GizmoCamera.Camera.ViewMatrix, Matrix.Identity);
            screenPositions[1] = EngineManager.Device.Viewport.Project(vertices[1], GizmoCamera.Camera.ProjectionMatrix, GizmoCamera.Camera.ViewMatrix, Matrix.Identity);

            Vector3 aux = screenPositions[1] - screenPositions[0];
            transformationAmount.X = aux.X / aux.Length();
            transformationAmount.Y = aux.Y / aux.Length();
        } // Calculate2DMouseDirection

        #endregion
        
        #region Render Manipulator For Picker
        
        /// <summary>
        /// Renderizado del manipulador para el picking. Va desde el de menor prioridad al de mayor.
        /// </summary>
        private static void RenderGizmoForPicker()
        {
            // Calculate gizmo center and orientation.
            Vector3 center;
            Quaternion orientation;
            GizmoCenterAndOrientation(out center, out orientation);

            // Calculate the distance from the object to camera position.
            Vector3 cameraToCenter = GizmoCamera.Camera.Position - center;
            float distanceToCamera = cameraToCenter.Length();
            float scale = distanceToCamera / 14;

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

            
            planeRedGreen.ModelFilter.Model = new Plane(vertices[2], vertices[0], vertices[4], vertices[1]);
            planeGreenBlue.ModelFilter.Model = new Plane(vertices[5], vertices[3], vertices[2], vertices[0]);
            planeBlueRed.ModelFilter.Model = new Plane(vertices[0], vertices[3], vertices[1], vertices[6]);
            picker.RenderObjectToManualPicker(planeRedGreen, new Color(255, 255, 0));
            picker.RenderObjectToManualPicker(planeGreenBlue, new Color(0, 255, 255));
            picker.RenderObjectToManualPicker(planeBlueRed, new Color(255, 0, 255));
            // Render a second time but from the other side.
            planeRedGreen.ModelFilter.Model = new Plane(vertices[4], vertices[1], vertices[2], vertices[0]);
            planeGreenBlue.ModelFilter.Model = new Plane(vertices[2], vertices[0], vertices[5], vertices[3]);
            planeBlueRed.ModelFilter.Model = new Plane(vertices[1], vertices[6], vertices[0], vertices[3]);
            picker.RenderObjectToManualPicker(planeRedGreen, new Color(255, 255, 0));
            picker.RenderObjectToManualPicker(planeGreenBlue, new Color(0, 255, 255));
            picker.RenderObjectToManualPicker(planeBlueRed, new Color(255, 0, 255));

            // Update Axis Lines
            lines.LineRenderer.Vertices[0] = new VertexPositionColor(vertices[0], Color.Red);
            lines.LineRenderer.Vertices[1] = new VertexPositionColor(vertices[1], Color.Red);
            lines.LineRenderer.Vertices[2] = new VertexPositionColor(vertices[0], new Color(0, 255, 0));
            lines.LineRenderer.Vertices[3] = new VertexPositionColor(vertices[2], new Color(0, 255, 0));
            lines.LineRenderer.Vertices[4] = new VertexPositionColor(vertices[0], Color.Blue);
            lines.LineRenderer.Vertices[5] = new VertexPositionColor(vertices[3], Color.Blue);
            picker.RenderObjectToManualPicker(lines, Color.White);

            // Update Cones
            redCone.Transform.LocalScale = new Vector3(scale);
            redCone.Transform.LocalRotation = orientation;
            redCone.Transform.LocalPosition = vertices[1];
            redCone.Transform.Rotate(new Vector3(0, 0, -90));
            picker.RenderObjectToManualPicker(redCone, Color.Red);

            greenCone.Transform.LocalScale = new Vector3(scale);
            greenCone.Transform.LocalRotation = orientation;
            greenCone.Transform.LocalPosition = vertices[2];
            picker.RenderObjectToManualPicker(greenCone, new Color(0, 255, 0)); // Color.Green is not 0,255,0

            blueCone.Transform.LocalScale = new Vector3(scale);
            blueCone.Transform.LocalRotation = orientation;
            blueCone.Transform.LocalPosition = vertices[3];
            blueCone.Transform.Rotate(new Vector3(90, 0, 0));
            picker.RenderObjectToManualPicker(blueCone, Color.Blue);
            
            // The plane used to select all axis.
            LineManager.Begin2D(PrimitiveType.LineList);
                Vector3 screenPositions = EngineManager.Device.Viewport.Project(vertices[0], GizmoCamera.Camera.ProjectionMatrix, GizmoCamera.Camera.ViewMatrix, Matrix.Identity);
                LineManager.DrawSolid2DPlane(new Rectangle((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, RegionSize, RegionSize), Color.White);
            LineManager.End();
        } // RenderGizmoForPicker

        #endregion

        #region Update Rendering Information

        /// <summary>
        /// Update Rendering Information.
        /// </summary>
        public void UpdateRenderingInformation()
        {

            #region Find Color

            Color redColor   = Color.Red;
            Color greenColor = Color.Green;
            Color blueColor  = Color.Blue;

            // If the manipulation is uniform then the axis are not yellow.
            if (!redAxisSelected || !greenAxisSelected || !blueAxisSelected) 
            {
                if (redAxisSelected)   redColor   = Color.Yellow;
                if (greenAxisSelected) greenColor = Color.Yellow;
                if (blueAxisSelected)  blueColor  = Color.Yellow;
            }

            #endregion

            // Calculate gizmo center and orientation.
            Vector3 center;
            Quaternion orientation;
            GizmoCenterAndOrientation(out center, out orientation);

            // Calculate the distance from the object to camera position.
            Vector3 cameraToCenter = GizmoCamera.Camera.Position - center;
            float distanceToCamera = cameraToCenter.Length();
            float scale = distanceToCamera / 14;
            
            // Calculate the gizmo matrix.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= Matrix.CreateFromQuaternion(orientation);
            transformationMatrix *= Matrix.CreateTranslation(center);

            // This are the axis line's vertex
            vertices[0] = Vector3.Transform(new Vector3(0, 0, 0), transformationMatrix);
            vertices[1] = Vector3.Transform(new Vector3(1, 0, 0), transformationMatrix);
            vertices[2] = Vector3.Transform(new Vector3(0, 1, 0), transformationMatrix);
            vertices[3] = Vector3.Transform(new Vector3(0, 0, 1), transformationMatrix);

            // The plane used to select all axis.
            Color planeColor;
            if (redAxisSelected && greenAxisSelected && blueAxisSelected)
            {
                planeColor = Color.Yellow;
            }
            else
                planeColor = Color.Gray;

            Vector3 screenPositions = EngineManager.Device.Viewport.Project(vertices[0], GizmoCamera.Camera.ProjectionMatrix, GizmoCamera.Camera.ViewMatrix, Matrix.Identity);
            planeAll.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3((int)screenPositions.X + RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[6] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y + RegionSize / 2, 0), planeColor);
            planeAll.LineRenderer.Vertices[7] = new VertexPositionColor(new Vector3((int)screenPositions.X - RegionSize / 2, (int)screenPositions.Y - RegionSize / 2, 0), planeColor);
                
            // Update Axis Lines
            lines.LineRenderer.Vertices[0] = new VertexPositionColor(vertices[0], redColor);
            lines.LineRenderer.Vertices[1] = new VertexPositionColor(vertices[1], redColor);
            lines.LineRenderer.Vertices[2] = new VertexPositionColor(vertices[0], greenColor);
            lines.LineRenderer.Vertices[3] = new VertexPositionColor(vertices[2], greenColor);
            lines.LineRenderer.Vertices[4] = new VertexPositionColor(vertices[0], blueColor);
            lines.LineRenderer.Vertices[5] = new VertexPositionColor(vertices[3], blueColor);
            
            // Update Cones
            redCone.Transform.LocalScale = new Vector3(scale);
            redCone.Transform.LocalRotation = orientation;
            redCone.Transform.LocalPosition = vertices[1];
            redCone.Transform.Rotate(new Vector3(0, 0, -90));
            ((Constant)redCone.ModelRenderer.Material).DiffuseColor = redColor;

            greenCone.Transform.LocalScale = new Vector3(scale);
            greenCone.Transform.LocalRotation = orientation;
            greenCone.Transform.LocalPosition = vertices[2];
            ((Constant)greenCone.ModelRenderer.Material).DiffuseColor = greenColor;

            blueCone.Transform.LocalScale = new Vector3(scale);
            blueCone.Transform.LocalRotation = orientation;
            blueCone.Transform.LocalPosition = vertices[3];
            blueCone.Transform.Rotate(new Vector3(90, 0, 0));
            ((Constant)blueCone.ModelRenderer.Material).DiffuseColor = blueColor;
        } // UpdateRenderingInformation

        #endregion
        
    } // TranslationGizmo
} // XNAFinalEngine.Editor
