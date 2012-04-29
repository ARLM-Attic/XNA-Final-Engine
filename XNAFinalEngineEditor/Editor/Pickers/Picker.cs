
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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.Editor
{

    /// <summary>
    /// A picker allows selecting an object from the screen.
    /// </summary>
    /// <remarks>
    /// It utilizes a texture method. This method produces better results that its alternative, the bounding volume method.
    /// One disadvantage is the the time that consumes the texture memory access.
    /// Possible addition: render in a render target of only one pixel.  Maybe it isn’t a good option.
    /// Besides, the picker efficiency is not critical.
    /// </remarks>
    public class Picker
    {

        #region Variables
        
        // The objects that can be selected.
        private readonly List<GameObject> objectsToPick;

        // It’s the texture where the scene is render.
        public readonly RenderTarget pickerTexture;

        // I need a constant shader to render the picker.
        private readonly Shader constantShader;

        // For manual picking.
        private bool hasBegun;
        private Matrix viewMatrix, projectionMatrix;

        #endregion

        #region Constructor

        /// <summary>
        /// A picker allows selecting an object from the screen.
        /// </summary>
        /// <remarks>
        /// It utilizes a texture method. This method produces better results that its alternative, the bounding volume method.
        /// One disadvantage is the the time that consumes the texture memory access.
        /// Possible addition: render in a render target of only one pixel. 
        /// Maybe it isn’t a good option.
        /// Besides, the picker efficiency is not critical.
        /// </remarks>
        public Picker(Size size)
        {            
            objectsToPick = new List<GameObject>();
            // No antialiasing because the colors can change.
            pickerTexture = new RenderTarget(size);
            constantShader = new Shader("Materials\\PickerConstant");
        } // Picker

        #endregion

        #region Add and Remove Objects

        /// <summary>
        /// Adds the object from the list of objects that can be selected.
        /// </summary>
        public void AddObject(GameObject obj)
        {
            objectsToPick.Add(obj);
        } // AddObject

        /// <summary>
        /// Removes the object from the list of objects that can be selected.
        /// </summary>
        public void RemoveObject(GameObject obj)
        {
            objectsToPick.Remove(obj);
        } // RemoveObject

        #endregion

        #region Texture Picking

        /// <summary>
        /// Pick the object that is on the mouse pointer.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public GameObject Pick(Matrix viewMatrix, Matrix projectionMatrix, Viewport viewport)
        {
            return Pick(Mouse.Position.X, Mouse.Position.Y, viewMatrix, projectionMatrix, viewport);
        } // Pick

        /// <summary>
        /// Pick the object that is on the X Y coordinates.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public GameObject Pick(int x, int y, Matrix viewMatrix, Matrix projectionMatrix, Viewport viewport)
        {
            try
            {

                RenderObjectsToPickerTexture(viewMatrix, projectionMatrix, viewport);
                
                #region Get the pixel from the texture
                
                Color[] color = new Color[1];
                pickerTexture.Resource.GetData(0, new Rectangle(x, y, 1, 1), color, 0, 1);
                Color pixel = color[0];

                #endregion

                #region Search the object

                byte red = 0, green = 0, blue = 0;
                foreach (GameObject obj in objectsToPick)
                {
                    // Select the next color
                    if (red < 255)
                        red++;
                    else
                    {
                        red = 0;
                        if (green < 255)
                            green++;
                        else
                        {
                            green = 0;
                            blue++;
                        }
                    }
                    if (pixel == new Color(red, green, blue))
                        return obj;
                }

                #endregion

            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Picker: operation failed: ", e);
            }
            return null;
        } // Pick

        /// <summary>
        /// Pick the object that is on the X Y coordinates.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public List<GameObject> Pick(Rectangle region, Matrix viewMatrix, Matrix projectionMatrix, Viewport viewport)
        {
            List<GameObject> pickedObjects = new List<GameObject>();

            try
            {

                RenderObjectsToPickerTexture(viewMatrix, projectionMatrix, viewport);

                #region Get the pixel from the texture

                if (region.Width == 0)
                    region.Width = 1;
                if (region.Height == 0)
                    region.Height = 1;
                Color[] colors = new Color[region.Width * region.Height];
                pickerTexture.Resource.GetData(0, region, colors, 0, region.Width * region.Height);

                #endregion

                #region Search the object

                byte red = 0, green = 0, blue = 0;
                foreach (GameObject obj in objectsToPick)
                {
                    // Select the next color
                    if (red < 255)
                        red++;
                    else
                    {
                        red = 0;
                        if (green < 255)
                            green++;
                        else
                        {
                            green = 0;
                            blue++;
                        }
                    }
                    if (colors.Any(color => color == new Color(red, green, blue)))
                    {
                        pickedObjects.Add(obj);
                    }
                }

                #endregion

            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Picker: operation failed: ", e);
            }
            return pickedObjects;
        } // Pick

        #endregion

        #region Render All Object to Picker

        /// <summary>
        /// Render the object using a constant shasder to picker texture.
        /// Each object will be render using a unique color.
        /// </summary>
        private void RenderObjectsToPickerTexture(Matrix viewMatrix, Matrix projectionMatrix, Viewport viewport)
        {
            this.viewMatrix = viewMatrix;
            this.projectionMatrix = projectionMatrix;

            byte red = 0, green = 0, blue = 0;
            Color colorMaterial = new Color(0, 0, 0);

            // Set Render States.
            EngineManager.Device.BlendState = BlendState.NonPremultiplied;
            EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineManager.Device.DepthStencilState = DepthStencilState.Default;

            // Start rendering onto the picker map
            pickerTexture.EnableRenderTarget();

            EngineManager.Device.Viewport = viewport;

            // Clear render target
            pickerTexture.Clear(Color.Black);

            constantShader.Resource.CurrentTechnique = constantShader.Resource.Techniques["ConstantsRGB"];

            // Render every object, one at a time
            foreach (GameObject obj in objectsToPick)
            {
                // Select the next color
                if (colorMaterial.R < 255)
                    red++;
                else
                {
                    red = 0;
                    if (colorMaterial.G < 255)
                        green++;
                    else
                    {
                        green = 0;
                        if (blue == 255)
                            throw new Exception("Color out of range.");
                        blue++;
                    }
                }
                colorMaterial = new Color(red, green, blue);
                // Render the object with the picker material
                if (obj is GameObject3D)
                    RenderObjectToPicker(obj, colorMaterial);
            }

            // Activate the frame buffer again.
            pickerTexture.DisableRenderTarget();

        } // RenderObjectsToPickerTexture

        #endregion

        #region Render Object To Picker

        /// <summary>
        /// Render Object To Picker.
        /// </summary>
        public void RenderObjectToPicker(GameObject gameObject, Color color)
        {
            if (gameObject is GameObject3D)
            {
                GameObject3D gameObject3D = (GameObject3D)gameObject;
                if (gameObject3D.ModelFilter != null && gameObject3D.ModelFilter.Model != null)
                {
                    constantShader.Resource.Parameters["diffuseColor"].SetValue(new Vector3(color.R / 255f, color.G / 255f, color.B / 255f));
                    constantShader.Resource.Parameters["worldViewProj"].SetValue(gameObject3D.Transform.WorldMatrix * viewMatrix * projectionMatrix);
                    constantShader.Resource.CurrentTechnique.Passes[0].Apply();
                    gameObject3D.ModelFilter.Model.Render();
                }
                else if (gameObject3D.LineRenderer != null)
                {
                    LineManager.Begin3D(gameObject3D.LineRenderer.PrimitiveType, viewMatrix, projectionMatrix);
                    for (int j = 0; j < gameObject3D.LineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(gameObject3D.LineRenderer.Vertices[j].Position, gameObject3D.LineRenderer.Vertices[j].Color);
                    LineManager.End();
                }
            }
        } // RenderObjectToPicker

        #endregion

        #region Bounding Mesh Methods
        /*
        /// <summary>
        /// Select the objects using the bounding volume method. 
        /// It wasn't finished.
        /// </summary>
        private void PickWithBoundingSpheres(int x, int y)
        {   
            Matrix world = Matrix.Identity;
            Vector3 nearsource = new Vector3(x, y, 0f),
                    farsource = new Vector3(x, y, 1f),
                    nearPoint = EngineManager.Device.Viewport.Unproject(nearsource, ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, world),
                    farPoint = EngineManager.Device.Viewport.Unproject(farsource, ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, world),
                    direction = farPoint - nearPoint;

            direction.Normalize();
            
            Ray pickRay = new Ray(nearPoint, direction);

            // TODO!!! It wasn't finished.

        } // PickWithBoundingSpheres
        */
        #endregion

        #region Manual Pick
        
        /// <summary>
        /// Manualy render the picker texture.
        /// This allow us to control deeply the pick operation.
        /// The black color is reserved.
        /// </summary>
        public void BeginManualPicking(Matrix viewMatrix, Matrix projectionMatrix, Viewport viewport)
        {
            if (hasBegun)
            {
                throw new InvalidOperationException("Picker: Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called.");
            }
            hasBegun = true;
            try
            {
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                // Set Render States.
                EngineManager.Device.BlendState = BlendState.NonPremultiplied;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;

                // Start rendering onto the picker map
                pickerTexture.EnableRenderTarget();

                EngineManager.Device.Viewport = viewport;

                // Clear render target
                pickerTexture.Clear(Color.Black);

                constantShader.Resource.CurrentTechnique = constantShader.Resource.Techniques["ConstantsRGB"];
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Picker: operation failed: ", e);
            }
        } // BeginManualPicking

        /// <summary>
        /// End manual render of the picker texture.
        /// </summary>
        public Color[] EndManualPicking(Rectangle region)
        {
            if (!hasBegun)
                throw new InvalidOperationException("Line Manager: End was called, but Begin has not yet been called. You must call Begin successfully before you can call End.");
            hasBegun = false;
            try
            {
                // Activate the frame buffer again.
                pickerTexture.DisableRenderTarget();

                Color[] colors = new Color[region.Width * region.Height];

                #region Fix out of region

                // Left side
                if (region.X < 0)
                {
                    region.Width += region.X;
                    region.X = 0;
                }
                // Top side
                if (region.Y < 0)
                {
                    region.Height += region.Y;
                    region.Y = 0;
                }
                // Right side
                if (region.X + region.Width > pickerTexture.Width)
                    region.Width = pickerTexture.Width - Mouse.Position.X;
                // Bottom side
                if (region.Y + region.Height > pickerTexture.Height)
                    region.Height = pickerTexture.Height - Mouse.Position.Y;

                #endregion

                pickerTexture.Resource.GetData(0, region, colors, 0, region.Width * region.Height);
            
                return colors;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Picker: operation failed: ", e);
            }
        } // EndManualPicking

        #endregion

    } // Picker
} // XNAFinalEngine.Editor
