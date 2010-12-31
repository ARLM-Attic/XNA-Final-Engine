
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
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.Helpers
{

    /// <summary>
    /// A picker allows selecting an object from the screen.
    /// It utilizes a texture method, this method produces better results that its alternative, the bounding volume method.
    /// One disadvantage is the limit in the amount of objects to track, the other its the time that consumes the texture memory access.
    /// Possible addition: render in a viewport of only one pixel. The difficult part is to do this viewport. Maybe it isn’t a good option.
    /// </summary>
    public class Picker
    {

        #region Variables

        /// <summary>
        /// The objects that can be selected.
        /// </summary>
        private readonly List<Graphics.Object> objects;

        /// <summary>
        /// The material used to render the objects in the picker texture.
        /// </summary>
        private readonly Constant constantMaterial;

        /// <summary>
        /// It’s the texture where the scene is render.
        /// </summary>
        private readonly RenderToTexture pickerMapTexture;

        #region For Manual Picking

        /// <summary>
        /// Store the texture in an array (if it search a region, not a pixel).
        /// </summary>
        private static Color[] colorArray;

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// A picker allows selecting an object from the screen.
        /// It utilizes a texture method, this method produces better results that its alternative, the bounding volume method.
        /// One disadvantage is the limit in the amount of objects to track, the other its the time that consumes.
        /// </summary>
        public Picker()
        {            
            objects = new List<Graphics.Object>();
            constantMaterial = new Constant();
            // No antialiasing because the colors can change.
            pickerMapTexture = new RenderToTexture(RenderToTexture.SizeType.FullScreen);
        } // Picker

        #endregion

        #region Add and Remove Objects

        /// <summary>
        /// Adds the object from the list of objects that can be selected.
        /// </summary>
        public void AddObject(Graphics.Object obj)
        {
            objects.Add(obj);
        } // AddObject

        /// <summary>
        /// Removes the object from the list of objects that can be selected.
        /// </summary>
        public void RemoveObject(Graphics.Object obj)
        {
            objects.Remove(obj);
        } // RemoveObject

        #endregion

        #region Texture Picking

        /// <summary>
        /// Pick the object that is on the mouse pointer.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public Graphics.Object Pick()
        {
            return Pick(Mouse.Position.X, Mouse.Position.Y);
        } // Pick

        /// <summary>
        /// Pick the object that is on the X Y coordinates.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public Graphics.Object Pick(int x, int y)
        {
            byte r = 0, g = 0, b = 0;
            Color colorMaterial = new Color(0,0,0);
            
            try
            {

                #region Render the objects to the picker texture

                // Start rendering onto the picker map
                pickerMapTexture.EnableRenderTarget();
                                
                // Clear render target
                pickerMapTexture.Clear(Color.Black);
                
                // Render every object, one at a time
                foreach (Graphics.Object obj in objects)
                {
                    // Select the next color
                    if (colorMaterial.R < 255)
                    {
                        r++;
                    }
                    else
                    {
                        r = 0;
                        if (colorMaterial.G < 255)
                        {
                            g++;
                        }
                        else
                        {
                            g = 0;
                            if (b == 255)
                                throw new Exception("Color out of range.");
                            b++;
                        }
                    }
                    colorMaterial = new Color(r, g, b);
                    // Set the material with the corresponded color.
                    constantMaterial.SurfaceColor = colorMaterial;
                    // Render the object with the picker material
                    obj.Render(constantMaterial);
                }
                    
                // Activate the frame buffer again.
                pickerMapTexture.DisableRenderTarget();

                #endregion
                
                #region Get the pixel from the texture
                
                Color[] color = new Color[1];
                pickerMapTexture.XnaTexture.GetData(0, new Rectangle(x, y, 1, 1), color, 0, 1);
                Color pixel = color[0];

                #endregion

                #region Search the object

                r = 0; g = 0; b = 0;
                foreach (Graphics.Object obj in objects)
                {
                    // Select the next color
                    if (colorMaterial.R < 255)
                        r++;
                    else
                    {
                        r = 0;
                        if (colorMaterial.G < 255)
                            g++;
                        else
                        {
                            g = 0;
                            b++;
                        }
                    }
                    if (pixel == new Color(r,g,b))
                        return obj;
                }

                #endregion

            } // try
            catch (Exception ex)
            {
                throw new Exception("The picker failed: " + ex);
            } // catch
            return null;
        } // Pick

        #endregion

        #region Bounding Mesh Methods

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

        #endregion

        #region Manual Pick

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

        #endregion

    } // Picker
} // XNAFinalEngine.Helpers
