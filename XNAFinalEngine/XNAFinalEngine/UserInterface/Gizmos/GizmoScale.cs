
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
using XNAFinalEngine.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using DirectionalLight = XNAFinalEngine.Graphics.DirectionalLight;
#endregion

namespace XNAFinalEngine.UI
{
    /// <summary>
    /// Un manipulador de escalado basado en el funcionamiento del manipulador de escaldo de Softimage XSI.
    /// </summary>
    public class GizmoScale : Gizmo
    {

        #region Variables
                
        /// <summary>
        /// Los cubos del manipulador.
        /// </summary>
        private static GraphicObject redBox,
                                     greenBox,
                                     blueBox;

        
        /// <summary>
        /// Los planos que marcan la conjuncion de dos ejes. Son invisibles en el renderizado.
        /// Pero visibles para el picker (aunque con menor prioridad que el resto, por eso se borra el zbuffer cuando se los renderiza)
        /// </summary>
        private static GraphicObject planeAll,
                                     planeRedGreen,
                                     planeGreenBlue,
                                     planeBlueRed;
         
        /// <summary>
        /// Almacena la escala anterior a la manipuacion.
        /// </summary>
        private static Vector3 oldScale = Vector3.Zero;

        /// <summary>
        /// La orientacion de los ojes en un escalado puede ser negativa.
        /// </summary>
        private static int redAxisOrientation = 1,
                           greenAxisOrientation = 1,
                           blueAxisOrientation = 1;

        private static Picker picker;

        #endregion

        #region Constructor

        /// <summary>
        /// Aqui se crean los objetos graficos que se utilizaran para el manipulador.
        /// </summary>
        static GizmoScale()
        {
            Box box = new Box(0.15f);
            redBox = new GraphicObject(box, new Blinn());
            greenBox = new GraphicObject(box, new Blinn());
            blueBox = new GraphicObject(box, new Blinn());
            // Hagamos lucir algo mejor a las cajas, rendericemoslas con informacion de luz.
            DirectionalLight directionalLight1 = new DirectionalLight(new Vector3(1, -1, 1), Color.White);
            DirectionalLight directionalLight2 = new DirectionalLight(new Vector3(-1, 1, -1), Color.White);
            redBox.AssociateLight(directionalLight1);
            redBox.AssociateLight(directionalLight2);
            greenBox.AssociateLight(directionalLight1);
            greenBox.AssociateLight(directionalLight2);
            blueBox.AssociateLight(directionalLight1);
            blueBox.AssociateLight(directionalLight2);
            picker = new Picker();
        }

        #endregion
        
        #region InitializeManipulator

        /// <summary>
        /// Inicializo el manipulador para trabajar con el objeto indicado.
        /// </summary>
        public static void InitializeManipulator(XNAFinalEngine.Graphics.Object _obj)
        {
            active = false;
            obj = _obj;
            SetOldScaleAndActualizeAxisOrientation();
        } // InitializeManipulator

        #endregion

        #region ManipulateObject

        /// <summary>
        /// Realizo toda la labor del manipulador en el cuadro excepto el renderizado del mismo. 
        /// Esta operacion debe realizarse al inicio del cuadro.
        /// </summary>
        public static void ManipulateObject()
        {
            Vector3 scale = new Vector3(0, 0, 0);            
            Color[] colorArray = new Color[regionSize * regionSize];
            //////////////////////////////////// Activo //////////////////////////////////////
            if (active)
            {
                // Si se termino de manipular
                if (!Mouse.LeftButtonPressed)
                {
                    active = false;
                    if (oldScale != obj.LocalScale)
                        produceTransformation = true;
                    SetOldScaleAndActualizeAxisOrientation();
                }
                // Si lo manipulamos actualizamos el escalado segun el desplazamaiento del mouse
                else
                {
                    if (redAxisSelected)
                    {
                        scale.X = (Mouse.DraggingAmount.X * amout.X / (float)EngineManager.Width * 10);
                        scale.X += (Mouse.DraggingAmount.Y * amout.Y / (float)EngineManager.Height * 10);
                    }
                    if (greenAxisSelected)
                    {
                        scale.Y = (Mouse.DraggingAmount.X * amout.X / (float)EngineManager.Width * 10);
                        scale.Y += (Mouse.DraggingAmount.Y * amout.Y / (float)EngineManager.Height * 10);
                    }
                    if (blueAxisSelected)
                    {
                        scale.Z = (Mouse.DraggingAmount.X * amout.X / (float)EngineManager.Width * 10);
                        scale.Z += (Mouse.DraggingAmount.Y * amout.Y / (float)EngineManager.Height * 10);
                    }
                    obj.ScaleAbs(scale * oldScale + oldScale);
                }
                if (Keyboard.EscapeJustPressed)
                {
                    active = false;
                    obj.ScaleAbs(oldScale);
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
                picker.BeginManualRenderPickerTexture();
                    RenderManipulatorForPicker(obj.CenterPoint, obj.WorldRotation);
                colorArray = picker.ManualPickFromCurrentPickerTexture(regionSize);
                
                redAxisSelected = true;
                greenAxisSelected = true;
                blueAxisSelected = true;
                for (int i = 0; i < regionSize * regionSize; i++)
                {
                    // Si no se encontre ningun eje asilado entonces hay posibilidades de que dos ejes en conjunto sean ganadores.
                    if (redAxisSelected == true && greenAxisSelected == true && blueAxisSelected == true)
                    {
                        // Ejes X e Y
                        if (colorArray[i].R == 255 && colorArray[i].G == 255 && colorArray[i].B == 0)
                        {
                            redAxisSelected = true;
                            greenAxisSelected = true;
                            blueAxisSelected = false;
                        }
                        // Ejes X e Z
                        if (colorArray[i].R == 255 && colorArray[i].G == 0 && colorArray[i].B == 255)
                        {
                            redAxisSelected = true;
                            greenAxisSelected = false;
                            blueAxisSelected = true;
                        }
                        // Ejes Y e Z
                        if (colorArray[i].R == 0 && colorArray[i].G == 255 && colorArray[i].B == 255)
                        {
                            redAxisSelected = false;
                            greenAxisSelected = true;
                            blueAxisSelected = true;
                        }
                    }
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
                    // Si es el plano de todos los ejes
                    if (colorArray[i].R == 255 && colorArray[i].G == 255 && colorArray[i].B == 255)
                    {
                        redAxisSelected = true;
                        greenAxisSelected = true;
                        blueAxisSelected = true;
                        i = regionSize * regionSize;
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
                vertices[1] = Vector3.Transform(new Vector3(redAxisOrientation, 0, 0), transformationMatrix);
            }
            // Green Axis
            if (!redAxisSelected && greenAxisSelected && !blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(0, greenAxisOrientation, 0), transformationMatrix);
            }
            // Blue Axis
            if (!redAxisSelected && !greenAxisSelected && blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(0, 0, blueAxisOrientation), transformationMatrix);
            }
            // Red and Green Axis
            if (redAxisSelected && greenAxisSelected && !blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(redAxisOrientation, greenAxisOrientation, 0), transformationMatrix);
            }
            // Green and Blue Axis
            if (!redAxisSelected && greenAxisSelected && blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(0, greenAxisOrientation, blueAxisOrientation), transformationMatrix);
            }
            // Red and Blue Axis
            if (redAxisSelected && greenAxisSelected && !blueAxisSelected)
            {
                vertices[1] = Vector3.Transform(new Vector3(redAxisOrientation, 0, blueAxisOrientation), transformationMatrix);
            }

            screenPositions[0] = EngineManager.Device.Viewport.Project(vertices[0], ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, Matrix.Identity);
            screenPositions[1] = EngineManager.Device.Viewport.Project(vertices[1], ApplicationLogic.Camera.ProjectionMatrix, ApplicationLogic.Camera.ViewMatrix, Matrix.Identity);

            aux = screenPositions[1] - screenPositions[0];
            amout.X = aux.X / aux.Length();
            amout.Y = aux.Y / aux.Length();
        } // Calculate2DMouseDirection

        #endregion

        #region Set OldScale and Actualize Axis Orientation

        /// <summary>
        /// Seteo oldScale con la escala actual y reoriento los ejes por si el escalado es negativo.
        /// </summary>
        private static void SetOldScaleAndActualizeAxisOrientation()
        {
            oldScale = obj.LocalScale;
            // Si la escala es negativa invierto los ejes
            if (oldScale.X < 0)
                redAxisOrientation = -1;
            else
                redAxisOrientation = 1;
            if (oldScale.Y < 0)
                greenAxisOrientation = -1;
            else
                greenAxisOrientation = 1;
            if (oldScale.Z < 0)
                blueAxisOrientation = -1;
            else
                blueAxisOrientation = 1;
        } // SetOldScaleAndActualizeAxisOrientation

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

            // Calculo los vertices de las distintas componentes, considerando su posicion rotacion y escala en la pantalla.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= axisMatrix;
            transformationMatrix *= Matrix.CreateTranslation(center);

            vertices[0] = Vector3.Transform(new Vector3(0, 0, 0), transformationMatrix);
            vertices[1] = Vector3.Transform(new Vector3(redAxisOrientation, 0, 0), transformationMatrix);
            vertices[2] = Vector3.Transform(new Vector3(0, greenAxisOrientation, 0), transformationMatrix);
            vertices[3] = Vector3.Transform(new Vector3(0, 0, blueAxisOrientation), transformationMatrix);
            vertices[4] = Vector3.Transform(new Vector3(redAxisOrientation, greenAxisOrientation, 0), transformationMatrix);
            vertices[5] = Vector3.Transform(new Vector3(0, greenAxisOrientation, blueAxisOrientation), transformationMatrix);
            vertices[6] = Vector3.Transform(new Vector3(redAxisOrientation, 0, blueAxisOrientation), transformationMatrix);

            // Creo los planos y los renderizo. Borrando el Zbuffer a continuacion para que no molesten.
            // Los renderizo de ambos lados

            planeRedGreen = new GraphicObject(new XNAFinalEngine.Graphics.Plane(vertices[2], vertices[0], vertices[4], vertices[1]), new Constant(new Color(255, 255, 0)));
            planeGreenBlue = new GraphicObject(new XNAFinalEngine.Graphics.Plane(vertices[5], vertices[3], vertices[2], vertices[0]), new Constant(new Color(0, 255, 255)));
            planeBlueRed = new GraphicObject(new XNAFinalEngine.Graphics.Plane(vertices[0], vertices[3], vertices[1], vertices[6]), new Constant(new Color(255, 0, 255)));

            planeRedGreen.Render();
            planeGreenBlue.Render();
            planeBlueRed.Render();

            planeRedGreen = new GraphicObject(new XNAFinalEngine.Graphics.Plane(vertices[4], vertices[1], vertices[2], vertices[0]), new Constant(new Color(255, 255, 0)));
            planeGreenBlue = new GraphicObject(new XNAFinalEngine.Graphics.Plane(vertices[2], vertices[0], vertices[5], vertices[3]), new Constant(new Color(0, 255, 255)));
            planeBlueRed = new GraphicObject(new XNAFinalEngine.Graphics.Plane(vertices[1], vertices[6], vertices[0], vertices[3]), new Constant(new Color(255, 0, 255)));

            planeRedGreen.Render();
            planeGreenBlue.Render();
            planeBlueRed.Render();

            // Renderizo las lineas

            EngineManager.Device.Clear(ClearOptions.DepthBuffer, new Color(0, 0, 0), 1.0f, 0);

            Primitives.Begin(PrimitiveType.LineList);
            Primitives.AddVertex(vertices[0], Color.Red);
            Primitives.AddVertex(vertices[1], Color.Red);
            Primitives.AddVertex(vertices[0], new Color(0, 255, 0)); // Color.Green no es 0 255 0
            Primitives.AddVertex(vertices[2], new Color(0, 255, 0)); // Color.Green no es 0 255 0
            Primitives.AddVertex(vertices[0], Color.Blue);
            Primitives.AddVertex(vertices[3], Color.Blue);
            Primitives.End();

            // Renderizo los cubos            

            constantMaterial.SurfaceColor = Color.Red;
            redBox.ScaleAbs(scale);
            redBox.RotateAbs(axisMatrix);
            redBox.TranslateAbs(vertices[1]);
            redBox.Render(constantMaterial);

            constantMaterial.SurfaceColor = new Color(0, 255, 0); // Color.Green no es 0 255 0
            greenBox.ScaleAbs(scale);
            greenBox.RotateAbs(axisMatrix);
            greenBox.TranslateAbs(vertices[2]);
            greenBox.Render(constantMaterial);

            constantMaterial.SurfaceColor = Color.Blue;
            blueBox.ScaleAbs(scale);
            blueBox.RotateAbs(axisMatrix);
            blueBox.TranslateAbs(vertices[3]);
            blueBox.Render(constantMaterial);

            // Renderizo el plano que se rota en conjunto con la camara, el de todos los ejes. Tiene la mayor prioridad de picking.

            EngineManager.Device.Clear(ClearOptions.DepthBuffer, new Color(0, 0, 0), 1.0f, 0);
            planeAll = new GraphicObject(new XNAFinalEngine.Graphics.Plane(0.2f * scale), new Constant(Color.White));
            planeAll.LocalMatrix = (Matrix.CreateBillboard(vertices[0], -ApplicationLogic.Camera.Position, Vector3.Up, null));
            planeAll.Render();
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

            float redAxisAmount = 1;
            float greenAxisAmount = 1;
            float blueAxisAmount = 1;
            // Si esta activo los handlers tienen un tamaño distinto al inicial.
            if (active)
            {
                if (redAxisSelected)
                {
                    redAxisAmount = (Mouse.DraggingAmount.X * amout.X / (float)EngineManager.Width * 10);
                    redAxisAmount += (Mouse.DraggingAmount.Y * amout.Y / (float)EngineManager.Height * 10) + 1;
                }
                if (greenAxisSelected)
                {
                    greenAxisAmount = (Mouse.DraggingAmount.X * amout.X / (float)EngineManager.Width * 10);
                    greenAxisAmount += (Mouse.DraggingAmount.Y * amout.Y / (float)EngineManager.Height * 10) + 1;
                }
                if (blueAxisSelected)
                {
                    blueAxisAmount = (Mouse.DraggingAmount.X * amout.X / (float)EngineManager.Width * 10);
                    blueAxisAmount += (Mouse.DraggingAmount.Y * amout.Y / (float)EngineManager.Height * 10) + 1;
                }
            }
            if (!redAxisSelected || !greenAxisSelected || !blueAxisSelected) // Si esta seleccionado el escalado uniforme los handlers no se pintan de amarillo
            {
                if (redAxisSelected) redColor = Color.Yellow;
                if (greenAxisSelected) greenColor = Color.Yellow;
                if (blueAxisSelected) blueColor = Color.Yellow;
            }

            // Calculo la distancia del objecto o manipulador a la camara
            Vector3 cameraManipulatorVector = ApplicationLogic.Camera.Position - center;
            float cameraManipulatorDistance = cameraManipulatorVector.Length();
            float scale = cameraManipulatorDistance / 14;

            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= axisMatrix;
            transformationMatrix *= Matrix.CreateTranslation(center);

            vertices[0] = Vector3.Transform(new Vector3(0, 0, 0), transformationMatrix);
            vertices[1] = Vector3.Transform(new Vector3(redAxisAmount * redAxisOrientation, 0, 0), transformationMatrix);
            vertices[2] = Vector3.Transform(new Vector3(0, greenAxisAmount * greenAxisOrientation, 0), transformationMatrix);
            vertices[3] = Vector3.Transform(new Vector3(0, 0, blueAxisAmount * blueAxisOrientation), transformationMatrix);

            // Renderizo el plano de todos los ejes
            if (redAxisSelected && greenAxisSelected && blueAxisSelected)
            {
                Primitives.Draw3DBillboardPlane(vertices[0], 0.2f * scale, Color.Yellow);
            }
            else
                Primitives.Draw3DBillboardPlane(vertices[0], 0.2f * scale, Color.Gray);

            // Renderizo las lineas

            Primitives.Begin(PrimitiveType.LineList);
                Primitives.AddVertex(vertices[0], redColor);
                Primitives.AddVertex(vertices[1], redColor);
                Primitives.AddVertex(vertices[0], greenColor);
                Primitives.AddVertex(vertices[2], greenColor);
                Primitives.AddVertex(vertices[0], blueColor);
                Primitives.AddVertex(vertices[3], blueColor);
            Primitives.End();
            
            // Renderizo los cubos

            redBox.ScaleAbs(scale);
            redBox.RotateAbs(axisMatrix);
            redBox.TranslateAbs(vertices[1]);
            redBox.Render(new Blinn(redColor));

            greenBox.ScaleAbs(scale);
            greenBox.RotateAbs(axisMatrix);
            greenBox.TranslateAbs(vertices[2]);
            greenBox.Render(new Blinn(greenColor));

            blueBox.ScaleAbs(scale);
            blueBox.RotateAbs(axisMatrix);
            blueBox.TranslateAbs(vertices[3]);
            blueBox.Render(new Blinn(blueColor));
        } // RenderManipulator

        #endregion

    } // UIManipulatorsScale
} // XNAFinalEngine.UI
