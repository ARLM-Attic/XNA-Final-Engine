
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
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.GraphicElements;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.Helpers
{

    /// <summary>
    /// A picker allows selecting an object from the screen.
    /// It utilizes a texture method, this method produces better results that its alternative, the bounding volume method.
    /// One disadvantage is the limit in the amount of objects to track, the other its the time that consumes the texture memory access.
    /// </summary>
    public class Picker
    {

        #region Variables

        /// <summary>
        /// The objects that can be selected.
        /// </summary>
        protected List<XNAFinalEngine.GraphicElements.Object> objects = null;

        /// <summary>
        /// The material used to render the objects in the picker texture.
        /// </summary>
        protected Constant constantMaterial = null;

        /// <summary>
        /// It’s the texture where the scene is render.
        /// </summary>
        public static RenderToTexture pickerMapTexture = null;

        #region For Manual Picking

        /// <summary>
        /// Store the camera view matrix because if the camera doesn't move the picker doesn't need to calculate all the process from the start.
        /// </summary>
        private static Matrix oldViewMatrix = Matrix.Identity;

        /// <summary>
        /// Store the texture in an array (if it search a region, not a pixel).
        /// </summary>
        private static Color[] colorArray = null;

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
            objects = new List<XNAFinalEngine.GraphicElements.Object>();
            constantMaterial = new Constant();
            if (pickerMapTexture == null)
                // No antialiasing because the colors can change.
                pickerMapTexture = new RenderToTexture(RenderToTexture.SizeType.FullScreen, false);
        } // Picker

        #endregion

        #region Add and Remove Objects

        /// <summary>
        /// Adds the object from the list of objects that can be selected.
        /// </summary>
        public void AddObject(XNAFinalEngine.GraphicElements.Object obj)
        {
            objects.Add(obj);
        } // AddObject

        /// <summary>
        /// Removes the object from the list of objects that can be selected.
        /// </summary>
        public void RemoveObject(XNAFinalEngine.GraphicElements.Object obj)
        {
            objects.Remove(obj);
        } // RemoveObject

        #endregion

        #region Texture Picking

        /// <summary>
        /// Pick the object that is on the mouse pointer.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public XNAFinalEngine.GraphicElements.Object Pick()
        {
            return Pick(Mouse.Position.X, Mouse.Position.Y);
        } // Pick

        /// <summary>
        /// Pick the object that is on the X Y coordinates.
        /// If no object was found the result is a null pointer.
        /// </summary>
        public XNAFinalEngine.GraphicElements.Object Pick(int x, int y)
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
                foreach (XNAFinalEngine.GraphicElements.Object obj in objects)
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
                            else
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
                pickerMapTexture.XnaTexture.GetData<Color>(0, new Rectangle(x, y, 1, 1), color, 0, 1);
                Color pixel = color[0];

                #endregion

                #region Search the object

                r = 0; g = 0; b = 0;
                foreach (XNAFinalEngine.GraphicElements.Object obj in objects)
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
                            b++;
                        }
                    }
                    if (pixel == new Color(r,g,b))
                    {
                        return obj;
                    }
                }

                #endregion

            } // try
            catch (Exception ex)
            {
                throw new Exception("The picker failed: " + ex.ToString());
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

            Vector3 nearsource = new Vector3(x, y, 0f);
            Vector3 farsource = new Vector3(x, y, 1f);

            Matrix world = Matrix.Identity;
            Vector3 nearPoint = EngineManager.Device.Viewport.Unproject(nearsource, ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, world);
            Vector3 farPoint = EngineManager.Device.Viewport.Unproject(farsource, ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, world);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            
            Ray pickRay = new Ray(nearPoint, direction);

            // TODO!!! It wasn't finished.

        } // PickWithBoundingSpheres

        #endregion

        #region Manual Pick TODO!!!

        /// <summary>
        /// Starts a session pick. 
        /// This allow us to search in a region of the things that we render in this session.
        /// With this we can pick mostly lines (that aren't graphic objects).
        /// The black color is reserved.
        /// </summary>
        public static void BeginManualPick()
        {   
            try
            {
                if (colorArray == null)
                    colorArray = new Color[EngineManager.Width * EngineManager.Height];

                // Start rendering onto the picker map
                pickerMapTexture.EnableRenderTarget();

                // Clear render target
                pickerMapTexture.Clear(Color.Black);

            } // try
            catch (Exception ex)
            {
                throw new Exception("The picker failed: " + ex.ToString());
            } // catch
        } // BeginManualPick

        /// <summary>
        /// Finaliza la secion de pick. Debemos especificar el tamaño de la region en la que se buscara.
        /// Este proceso esta optimizado para trabajar con manipuladores. Si se desea un trabajo mas general hay
        /// que modificar (simplificar) este metodo.
        /// </summary>
        public static Color[] EndManualPicking(int regionSize)
        {   
            Color[] colorArrayResult = new Color[regionSize * regionSize];
            try
            {
                // Activate the frame buffer again.
                pickerMapTexture.DisableRenderTarget();

                // The botton and right bounds don't matter.
                //if (EngineManager.Width - Mouse.Position.X > 5 && EngineManager.Height - Mouse.Position.Y > 5)
                {
                    // Chequamos si el rectangulo no se pasa de la pantalla
                    if (Mouse.Position.X + regionSize > EngineManager.Width)
                    {
                        regionSize = EngineManager.Width - Mouse.Position.X;
                    }
                    if (Mouse.Position.Y + regionSize > EngineManager.Height)
                    {
                        if (EngineManager.Height - Mouse.Position.Y < regionSize)
                            regionSize = EngineManager.Height - Mouse.Position.Y;
                    }
                    // Si la camara no se ha movido no retornamos la informacion de la textura (getdata es una operacion muy costosa)
                    if (oldViewMatrix != ApplicationLogic.Camera.ViewMatrix)
                    {
                        pickerMapTexture.XnaTexture.GetData<Color>(colorArray);
                        oldViewMatrix = ApplicationLogic.Camera.ViewMatrix;
                    }
                    for (int i = 0; i < regionSize; i++)
                        for (int j = 0; j < regionSize; j++)
                        {
                            colorArrayResult[i * regionSize + j] = colorArray[(Mouse.Position.X + i) + (Mouse.Position.Y + j) * EngineManager.Width];
                        }
                }
                return colorArrayResult;
            } // try
            catch (Exception ex)
            {
                throw new Exception("The picker failed: " + ex.ToString());
            } // catch
        } // EndManualPicking

        /// <summary>
        /// Optimiza la captura del picker manual. Solo vale si la escena esta estatica.
        /// Si hay movimiento de camara el picker se recalculara elevando el costo del proceso.
        /// </summary>
        public static void OptimizeForStaticScene()
        {
            oldViewMatrix = Matrix.Identity;
        } // OptimizeForStaticScene

        #endregion

    } // Picker
} // XNAFinalEngine.Helpers
