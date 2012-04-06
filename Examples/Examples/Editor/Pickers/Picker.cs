
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
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
        
        /*
        /// <summary>
        /// Store the texture in an array (if it search a region, not a pixel).
        /// </summary>
        private static Color[] colorArray;*/

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
            constantShader = new Shader("Materials\\Constant");

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
        public GameObject Pick(Matrix viewMatrix, Matrix projectionMatrix)
        {
            return Pick(Mouse.Position.X, Mouse.Position.Y, viewMatrix, projectionMatrix);
        } // Pick

        /// <summary>
        /// Pick the object that is on the X Y coordinates.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public GameObject Pick(int x, int y, Matrix viewMatrix, Matrix projectionMatrix)
        {
            byte red = 0, green = 0, blue = 0;
            Color colorMaterial = new Color(0,0,0);
            
            try
            {

                #region Render the objects to the picker texture

                // Start rendering onto the picker map
                pickerTexture.EnableRenderTarget();
                                
                // Clear render target
                pickerTexture.Clear(Color.Black);

                constantShader.Resource.CurrentTechnique = constantShader.Resource.Techniques["ConstantsRGB"];

                // Render every object, one at a time
                foreach (GameObject obj in objectsToPick)
                {
                    // Select the next color
                    if (colorMaterial.R < 255)
                    {
                        red++;
                    }
                    else
                    {
                        red = 0;
                        if (colorMaterial.G < 255)
                        {
                            green++;
                        }
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
                    {
                        GameObject3D gameObject3D = (GameObject3D) obj;
                        if (gameObject3D.ModelFilter != null && gameObject3D.ModelFilter.Model != null)
                        {
                            constantShader.Resource.Parameters["diffuseColor"].SetValue(new Vector3(colorMaterial.R / 255f, colorMaterial.G / 255f, colorMaterial.B / 255f));
                            constantShader.Resource.Parameters["worldViewProj"].SetValue(gameObject3D.Transform.WorldMatrix * viewMatrix * projectionMatrix);
                            constantShader.Resource.CurrentTechnique.Passes[0].Apply();
                            gameObject3D.ModelFilter.Model.Render();
                        }
                    }
                }
                    
                // Activate the frame buffer again.
                pickerTexture.DisableRenderTarget();

                #endregion
                
                #region Get the pixel from the texture
                
                Color[] color = new Color[1];
                pickerTexture.Resource.GetData(0, new Rectangle(x, y, 1, 1), color, 0, 1);
                Color pixel = color[0];

                #endregion

                #region Search the object

                red = 0; green = 0; blue = 0;
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
                            blue++;
                        }
                    }
                    if (pixel == new Color(red,green,blue))
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
        /*
        /// <summary>
        /// Manualy render the picker texture.
        /// This allow us to search a color in a specified region that we render in the next step.
        /// With this we can pick mostly lines (that aren't graphic objects).
        /// The black color is reserved.
        /// </summary>
        public void BeginManualRenderPickerTexture()
        {   
            try
            {
                if (colorArray == null)
                    colorArray = new Color[pickerMapTexture.Width * pickerMapTexture.Height];

                // Start rendering onto the picker map
                pickerMapTexture.EnableRenderTarget();

                // Clear render target
                pickerMapTexture.Clear(Color.Black);

            } // try
            catch (Exception ex)
            {
                throw new Exception("The picker failed: " + ex);
            } // catch
        } // BeginManualRenderPickerTexture

        /// <summary>
        /// End manual render of the picker texture.
        /// </summary>
        public void EndManualRenderPickerTexture()
        {
            // Comeback to the frame buffer.
            pickerMapTexture.DisableRenderTarget();
        } // EndManualRenderPickerTexture

        /// <summary>
        /// Returns a color set from the current picker texture that was created manually.
        /// This method is prepared to work with gizmos.
        /// </summary>
        public Color[] ManualPickFromCurrentPickerTexture(int regionSize)
        {   
            Color[] colorArrayResult = new Color[regionSize * regionSize];
            try
            {
                // If the region is off the screen
                if (Mouse.Position.X + regionSize > pickerMapTexture.Width)
                {
                    regionSize = pickerMapTexture.Width - Mouse.Position.X;
                }
                if (Mouse.Position.Y + regionSize > pickerMapTexture.Height)
                {
                    if (pickerMapTexture.Height - Mouse.Position.Y < regionSize)
                        regionSize = pickerMapTexture.Height - Mouse.Position.Y;
                }
                pickerMapTexture.XnaTexture.GetData<Color>(colorArray);
                for (int i = 0; i < regionSize; i++)
                    for (int j = 0; j < regionSize; j++)
                    {
                        colorArrayResult[i*regionSize + j] = colorArray[(Mouse.Position.X + i) + (Mouse.Position.Y + j)*EngineManager.Width];
                    }
                return colorArrayResult;
            } // try
            catch (Exception ex)
            {
                throw new Exception("The picker failed: " + ex);
            } // catch
        } // ManualPickFromCurrentPickerTexture
        */
        #endregion

    } // Picker
} // XNAFinalEngine.Helpers
