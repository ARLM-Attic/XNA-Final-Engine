
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
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.GraphicElements;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using DirectionalLight = XNAFinalEngine.GraphicElements.DirectionalLight;
#endregion

namespace XNAFinalEngine.UI
{
    /// <summary>
    /// Un manipulador de escalado basado en el funcionamiento del manipulador de escaldo de Softimage XSI.
    /// </summary>
    public class UIManipulatorsRotation : UIManipulators
    {

        #region Variables
                
        /// <summary>
        /// Los cubos del manipulador.
        /// </summary>
        private static GraphicObject redRotator,
                                     greenRotator,
                                     blueRotator;
         
        /// <summary>
        /// Almacena la escala anterior a la manipuacion.
        /// </summary>
        private static Matrix oldRotation = Matrix.Identity;

        #endregion

        #region Constructor

        /// <summary>
        /// Aqui se crean los objetos graficos que se utilizaran para el manipulador.
        /// </summary>
        static UIManipulatorsRotation()
        {
            redRotator = new GraphicObject("UIRotator", new Blinn());
            greenRotator = new GraphicObject("UIRotator", new Blinn());
            blueRotator = new GraphicObject("UIRotator", new Blinn());
            // Hagamos lucir algo mejor a los conos, rendericemoslos con informacion de luz.
            DirectionalLight directionalLight1 = new DirectionalLight(new Vector3(-1, -1, -1), Color.White);
            DirectionalLight directionalLight2 = new DirectionalLight(new Vector3(1, 1, 1), Color.White);
            redRotator.AssociateLight(directionalLight1);
            redRotator.AssociateLight(directionalLight2);
            greenRotator.AssociateLight(directionalLight1);
            greenRotator.AssociateLight(directionalLight2);
            blueRotator.AssociateLight(directionalLight1);
            blueRotator.AssociateLight(directionalLight2);
        }

        #endregion
        
        #region InitializeManipulator

        /// <summary>
        /// Inicializo el manipulador para trabajar con el objeto indicado.
        /// </summary>
        public static void InitializeManipulator(XNAFinalEngine.GraphicElements.Object _obj)
        {
            active = false;
            obj = _obj;
            oldRotation = obj.LocalRotation;
        } // InitializeManipulator

        #endregion

        #region ManipulateObject

        /// <summary>
        /// Realizo toda la labor del manipulador en el cuadro excepto el renderizado del mismo. 
        /// Esta operacion debe realizarse al inicio del cuadro.
        /// </summary>
        public static void ManipulateObject()
        {
            float rotationFactor = 0;
            Color[] colorArray = new Color[regionSize * regionSize];
            //////////////////////////////////// Activo //////////////////////////////////////
            if (active)
            {
                // Si se termino de manipular
                if (!Mouse.LeftButtonPressed)
                {
                    active = false;
                    if (oldRotation != obj.LocalRotation)
                        produceTransformation = true;
                    oldRotation = obj.LocalRotation;
                    Picker.OptimizeForStaticScene();
                }
                // Si lo manipulamos actualizamos el escalado segun el desplazamaiento del mouse
                else
                {
                    rotationFactor = (Mouse.DraggingAmount.X * amout.Y / (float)EngineManager.Width * 360);
                    rotationFactor += (Mouse.DraggingAmount.Y * amout.X / (float)EngineManager.Height * 360);
                    if (redAxisSelected)
                    {
                        obj.RotateAbs(oldRotation);
                        obj.RotateRelLocal(rotationFactor, 0, 0);
                    }
                    if (greenAxisSelected)
                    {
                        obj.RotateAbs(oldRotation);
                        obj.RotateRelLocal(0, rotationFactor, 0);
                    }
                    if (blueAxisSelected)
                    {
                        obj.RotateAbs(oldRotation);
                        obj.RotateRelLocal(0, 0, rotationFactor);
                    }
                }
                if (Keyboard.EscapeJustPressed)
                {
                    active = false;
                    obj.RotateAbs(oldRotation);
                }
            }
            //////////////////////////////////// Inactivo //////////////////////////////////////
            else            
            {
                // Si apretamos el boton del mouse el manipulador se activa
                if (Mouse.LeftButtonJustPressed)
                {
                    active = true;
                    oldLocalMatrix = obj.LocalMatrix;
                    Calculate2DMouseDirection(obj.CenterPoint, obj.WorldRotation);
                }
                Picker.BeginManualPick();
                    RenderManipulatorForPicker(obj.CenterPoint, obj.WorldRotation);
                colorArray = Picker.EndManualPicking(regionSize);
                
                redAxisSelected = false;
                greenAxisSelected = false;
                blueAxisSelected = false;
                for (int i = 0; i < regionSize * regionSize; i++)
                {
                    // Eje X
                    if (colorArray[i].R == 255 && colorArray[i].G == 0 && colorArray[i].B == 0)
                    {
                        redAxisSelected = true;
                        greenAxisSelected = false;
                        blueAxisSelected = false;
                    }
                    // Eje Y
                    if (colorArray[i].R == 0 && colorArray[i].G == 255 && colorArray[i].B == 0)
                    {
                        redAxisSelected = false;
                        greenAxisSelected = true;
                        blueAxisSelected = false;
                    }
                    // Eje Z
                    if (colorArray[i].R == 0 && colorArray[i].G == 0 && colorArray[i].B == 255)
                    {
                        redAxisSelected = false;
                        greenAxisSelected = false;
                        blueAxisSelected = true;
                    }
                }
            }
        } // ManipulateObject

        #endregion

        #region Calculate2DMouseDirection

        /// <summary>
        /// Calculamos como va a afectar el movimiento vertical y horizontal del mouse al manipulador.
        /// </summary>
        protected static void Calculate2DMouseDirection(Vector3 center, Matrix axisMatrix)
        {
            Vector3[] screenPositions = new Vector3[7];
            Vector3 aux;

            // Calculo la distancia del objecto o manipulador a la camara
            Vector3 cameraManipulatorVector = ApplicationLogic.Camera.Position - center;
            float cameraManipulatorDistance = cameraManipulatorVector.Length();
            float scale = cameraManipulatorDistance / 14;

            // Calculo los vertices de las distintas componentes, considerando su posicion rotacion y escala en la pantalla.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= axisMatrix;
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

            screenPositions[0] = EngineManager.Device.Viewport.Project(vertices[0], ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, Matrix.Identity);
            screenPositions[1] = EngineManager.Device.Viewport.Project(vertices[1], ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, Matrix.Identity);

            aux = screenPositions[1] - screenPositions[0];
            amout.X = aux.X / aux.Length();
            amout.Y = aux.Y / aux.Length();
        } // Calculate2DMouseDirection

        #endregion

        #region Render Manipulator For Picker

        /// <summary>
        /// Renderizado del manipulador para el picking. Va desde el de menor prioridad al de mayor.
        /// </summary>
        private static void RenderManipulatorForPicker(Vector3 center, Matrix axisMatrix)
        {
            Constant constantMaterial = new Constant();
            // Calculo la distancia del objecto o manipulador a la camara
            Vector3 cameraManipulatorVector = ApplicationLogic.Camera.Position - center;
            float cameraManipulatorDistance = cameraManipulatorVector.Length();
            float scale = cameraManipulatorDistance / 14;

            scale *= 0.25f;  // Compenso el tamaño del modelo

            // Calculo los vertices de las distintas componentes, considerando su posicion rotacion y escala en la pantalla.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= axisMatrix;
            transformationMatrix *= Matrix.CreateTranslation(center);

            // Renderizo los rotators            
            constantMaterial.SurfaceColor = Color.Red;
            redRotator.LocalMatrix = transformationMatrix;
            redRotator.Render(constantMaterial);

            constantMaterial.SurfaceColor = new Color(0, 255, 0); // Color.Green no es 0 255 0
            greenRotator.LocalMatrix = transformationMatrix;
            greenRotator.RotateRelLocal(0, 0, 90);
            greenRotator.Render(constantMaterial);

            constantMaterial.SurfaceColor = Color.Blue;
            blueRotator.LocalMatrix = transformationMatrix;
            blueRotator.RotateRelLocal(0, 90, 0);
            blueRotator.Render(constantMaterial);
        }

        #endregion

        #region Render Manipulator

        /// <summary>
        /// Renderizado del manipulador en pantalla. 
        /// Se necesita renderizar al final cuadro para no entrar en problemas con los render targets.
        /// </summary>
        public static void RenderManipulator()
        {
            Vector3 center = obj.CenterPoint;
            Matrix axisMatrix = obj.WorldRotation;

            Color redColor = Color.Red;
            Color greenColor = Color.Green;
            Color blueColor = Color.Blue;
                        
            if (redAxisSelected) redColor = Color.Yellow;
            if (greenAxisSelected) greenColor = Color.Yellow;
            if (blueAxisSelected) blueColor = Color.Yellow;

            // Calculo la distancia del objecto o manipulador a la camara
            Vector3 cameraManipulatorVector = ApplicationLogic.Camera.Position - center;
            float cameraManipulatorDistance = cameraManipulatorVector.Length();
            float scale = cameraManipulatorDistance / 14;

            scale *= 0.25f; // Compenso el tamaño del modelo

            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= axisMatrix;
            transformationMatrix *= Matrix.CreateTranslation(center);
            
            // Renderizo los cubos
            redRotator.LocalMatrix = transformationMatrix;
            redRotator.Render(new Blinn(redColor, 1f, 2, 1));

            greenRotator.LocalMatrix = transformationMatrix;
            greenRotator.RotateRelLocal(0, 0, 90);
            greenRotator.Render(new Blinn(greenColor, 1f, 2, 1));

            blueRotator.LocalMatrix = transformationMatrix;
            blueRotator.RotateRelLocal(0, 90, 0);
            blueRotator.Render(new Blinn(blueColor, 1f, 2, 1));
        } // RenderManipulator

        #endregion

    } // UIManipulatorsRotation
} // XNAFinalEngine.UI
