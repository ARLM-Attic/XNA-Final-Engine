
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
#endregion

namespace XNAFinalEngine.UI
{
    /// <summary>
    /// Crea y administra un marco de trabajo para manipular objetos.
    /// El cual consiste de la seleccion de objetos, la seleccion del manipulador y la cancelacion del mismo, entre otras tareas.
    /// </summary>
    public static class UISceneManipulator
    {

        #region Variables

        /// <summary>
        /// El picker necesario para seleccionar los objetos de la escena.
        /// </summary>
        private static Picker picker;
                
        private enum Manipulator { None, Scale, Rotation, Translation };

        /// <summary>
        /// Indica que manipulador esta activo.
        /// </summary>
        private static Manipulator manipulatorActive = Manipulator.None;

        private static XNAFinalEngine.GraphicElements.Object selectedObject = null;

        /// <summary>
        /// Calculos y guardamos en esta variable si es posible activar un manipulador. 
        /// Con esto evitamos el recalculo de esta situacion.
        /// </summary>
        private static bool isPosibleToSwich = false;

        private struct undoStruct
        {            
            public XNAFinalEngine.GraphicElements.Object obj;
            public Matrix localMatrix;
            public undoStruct(XNAFinalEngine.GraphicElements.Object _obj, Matrix _localMatrix)
            {
                obj = _obj;
                localMatrix = _localMatrix;
            }
        }

        /// <summary>
        /// Almacena las operaciones anteriormente realizadas
        /// </summary>
        private static Stack<undoStruct> undoStack = new Stack<undoStruct>();

        #endregion

        #region Constructor

        /// <summary>
        /// Creo los elementos necesarios para la manipulacion de la escenas con transformadores.
        /// </summary>
        static UISceneManipulator()
        {
            picker = new Picker();
        } // UISceneManipulator

        #endregion

        #region Add or remove objects for picking

        /// <summary>
        /// Remueve al objeto en la lista de objetos a seleccionar.
        /// </summary>
        public static void RemoveObject(XNAFinalEngine.GraphicElements.Object obj)
        {
            picker.RemoveObject(obj);
        } // RemoveObject

        /// <summary>
        /// Agrega al objeto en la lista de objetos a seleccionar.
        /// </summary>
        public static void AddObject(XNAFinalEngine.GraphicElements.Object obj)
        {
            picker.AddObject(obj);
        } // AddObject

        #endregion

        #region Manipulate Scene

        /// <summary>
        /// Manipula la escena. Pero no renderiza nada en la pantalla.
        /// </summary>
        public static void ManipulateScene()
        {

            #region Frame Object and Reset Camera

            if (selectedObject != null && Keyboard.KeyJustPressed(Keys.F))
            {
                ((XSICamera)ApplicationLogic.Camera).LookAtPosition = selectedObject.CenterPoint;
                ((XSICamera)ApplicationLogic.Camera).Distance = selectedObject.BoundingSphereOptimized.Radius * 3;
            }
            if (Keyboard.KeyJustPressed(Keys.R))
            {
                ((XSICamera)ApplicationLogic.Camera).LookAtPosition = new Vector3(0, 0.5f, 0);
                ((XSICamera)ApplicationLogic.Camera).Distance = 15;
            }

            #endregion

            // Si el manipulador no esta activo //
            if (manipulatorActive == Manipulator.None)
            {
                if (Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed)
                {
                    selectedObject = null;
                }
                if (Mouse.LeftButtonJustPressed)
                {
                    selectedObject = picker.Pick();
                }
            }
            // Si se apreta escape o espacio y un manipulador esta activo
            else
            {
                if ((Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed) && !(UIManipulators.Active))
                {
                    manipulatorActive = Manipulator.None;
                    UIMousePointer.ManipulatorMode = false;
                }
            }
            // Habilitamos manipuladores, si es posible.
            isPosibleToSwich = selectedObject != null && ((manipulatorActive == Manipulator.None) || !(UIManipulators.Active));
            if (Keyboard.KeyJustPressed(Keys.X) && isPosibleToSwich)
            {
                manipulatorActive = Manipulator.Scale;
                Picker.OptimizeForStaticScene();
                UIMousePointer.ManipulatorMode = true;
                UIManipulatorsScale.InitializeManipulator(selectedObject);
            }
            if (Keyboard.KeyJustPressed(Keys.C) && isPosibleToSwich)
            {
                manipulatorActive = Manipulator.Rotation;
                Picker.OptimizeForStaticScene();
                UIMousePointer.ManipulatorMode = true;
                UIManipulatorsRotation.InitializeManipulator(selectedObject);
            }
            if (Keyboard.KeyJustPressed(Keys.V) && isPosibleToSwich)
            {
                manipulatorActive = Manipulator.Translation;
                Picker.OptimizeForStaticScene();
                UIMousePointer.ManipulatorMode = true;
                UIManipulatorsTranslation.InitializeManipulator(selectedObject);
            }
            // Trabajamos con el manipulador activo
            switch (manipulatorActive)
            {
                case Manipulator.Scale: UIManipulatorsScale.ManipulateObject(); break;
                case Manipulator.Rotation: UIManipulatorsRotation.ManipulateObject(); break;
                case Manipulator.Translation: UIManipulatorsTranslation.ManipulateObject(); break;
            }
            // Si el manipulador produjo un resultado
            if (UIManipulators.ProduceTransformation)
            {
                undoStack.Push(new undoStruct(selectedObject, UIManipulators.OldLocalMatrix));
            }
            // Undo y Redo (// TODO!!!)
            if (Keyboard.KeyPressed(Keys.LeftControl) &&
                Keyboard.KeyJustPressed(Keys.Z) &&
                (manipulatorActive == Manipulator.None || !(UIManipulators.Active)))
            {
                Picker.OptimizeForStaticScene();
                if (undoStack.Count > 0)
                {
                    undoStack.Peek().obj.LocalMatrix = undoStack.Peek().localMatrix;
                    undoStack.Pop();
                    // Reiniciamos el manipulador
                    switch (manipulatorActive)
                    {
                        case Manipulator.Scale: UIManipulatorsScale.InitializeManipulator(selectedObject); break;
                        case Manipulator.Rotation: UIManipulatorsRotation.InitializeManipulator(selectedObject); break;
                        case Manipulator.Translation: UIManipulatorsTranslation.InitializeManipulator(selectedObject); break;
                    }
                }
            }
        } // ManipulateScene

        #endregion

        #region Render Feedback

        /// <summary>
        /// Nos muestra en pantalla los elementos de la manipulacion.
        /// </summary>
        public static void Render()
        {
            if (selectedObject != null)
            {
                Primitives.DrawBoundingBox(selectedObject.BoundingBox, new Color(100,150,250));
                switch (manipulatorActive)
                {
                    case Manipulator.Scale: UIManipulatorsScale.RenderManipulator(); break;
                    case Manipulator.Rotation: UIManipulatorsRotation.RenderManipulator(); break;
                    case Manipulator.Translation: UIManipulatorsTranslation.RenderManipulator(); break;
                }
            }
        } // Render

        #endregion

    } // UIManipulatorsManager
} // XNAFinalEngine.UI
