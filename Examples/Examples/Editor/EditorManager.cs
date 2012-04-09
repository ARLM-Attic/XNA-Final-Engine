
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
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using XNAFinalEngine.UserInterface;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Size = XNAFinalEngine.Helpers.Size;

#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// This put all the editor pieces together.
    /// </summary>
    /// <remarks>
    /// The editor is not garbage free because the editor uses the user interface (based in Neo Force Control).
    /// The user interface was heavily modified and improved but the garbage was not removed.
    /// Moreover the editor uses the texture picking method that stall the CPU but brings the best accuracy.
    /// </remarks>
    public static class EditorManager
    {

        #region Script Class

        /// <summary>
        /// Used to call the manager's update and render methods in the correct order without explicit calls. 
        /// </summary>
        /// <remarks>
        /// Most XNA Final Engine managers don’t work this way because the GameLoop class controls their functionality.
        /// But this manager is in a higher level because the user interface dependency and
        /// because I consider that it is the best for the organization.
        /// </remarks>
        private sealed class ScripEditorManager : Script
        {

            /// <summary>
            /// Update camera.
            /// </summary>
            public override void Update()
            {
                EditorManager.Update();
            }

        } // ScripEditorManager

        #endregion

        #region Enumerates

        /// <summary>
        /// The active manipulator.
        /// </summary>
        private enum Manipulator
        {
            None,
            Scale,
            Rotation,
            Translation
        };

        #endregion

        #region Structs

        /// <summary>
        /// Stores the previous commands.
        /// </summary>
        private struct UndoStruct
        {
            public readonly GameObject obj;
            public readonly Matrix localMatrix;

            public UndoStruct(GameObject obj, Matrix localMatrix)
            {
                this.obj = obj;
                this.localMatrix = localMatrix;
            } // UndoStruct

        } // UndoStruct

        #endregion

        #region Variables

        // The editor camera.
        private static GameObject3D editorCamera;

        // The game main camera.
        private static GameObject3D gameMainCamera;

        // The picker to select an object from the screen.
        private static Picker picker;

        // The active manipulator.
        private static Manipulator activeManipulator = Manipulator.None;

        private static GameObject selectedObject;

        /// <summary>
        /// Calculos y guardamos en esta variable si es posible activar un manipulador. 
        /// Con esto evitamos el recalculo de esta situacion.
        /// </summary>
        private static bool isPosibleToSwich = false;
        
        /// <summary>
        /// Almacena las operaciones anteriormente realizadas
        /// </summary>
        private static Stack<UndoStruct> undoStack = new Stack<UndoStruct>();

        // To avoid more than one initialization.
        private static bool initialized;

        // Used to call the update and render method in the correct order without explicit calls.
        private static GameObject editorManagerGameObject;

        private static bool editorModeEnabled;

        #endregion

        #region Properties

        /// <summary>
        /// Is the editor mode enabled?
        /// </summary>
        public static bool EditorModeEnabled { get { return editorModeEnabled; } }

        #endregion

        #region Initialize

        /// <summary>
        /// This put all the editor pieces together.
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
                return;
            initialized = true;
            // If it already initialize don't worry.
            UserInterfaceManager.Initialize();
            picker = new Picker(Size.FullScreen);
            editorCamera = new GameObject3D();
            editorCamera.AddComponent<Camera>();
            ScriptEditorCamera script = (ScriptEditorCamera)editorCamera.AddComponent<ScriptEditorCamera>();
            script.SetPosition(new Vector3(0, 20, 15), Vector3.Zero);
            editorCamera.Camera.Visible = false;
            editorCamera.Camera.RenderingOrder = int.MaxValue;

            editorManagerGameObject = new GameObject2D();
            editorManagerGameObject.AddComponent<ScripEditorManager>();
        } // Initialize

        #endregion

        #region Add or remove objects for picking

        /// <summary>
        /// Adds the object from the list of objects that can be selected.
        /// </summary>
        public static void AddObject(GameObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj", "Editor Manager: object is null.");
            if (picker == null)
                throw new InvalidOperationException("Editor Manager: The editor was not initialized. If you use an Editable Scene call base.Load before adding or removing elements.");
            picker.AddObject(obj);
        } // AddObject

        /// <summary>
        /// Removes the object from the list of objects that can be selected.
        /// </summary>
        public static void RemoveObject(GameObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj", "Editor Manager: object is null.");
            if (picker == null)
                throw new InvalidOperationException("Editor Manager: The editor was not initialized. If you use an Editable Scene call base.Load before adding or removing elements.");
            picker.RemoveObject(obj);
        } // RemoveObject

        #endregion

        #region Enable Disable Editor Mode

        /// <summary>
        /// Enable editor mode
        /// </summary>
        /// <param name="mainCamera">The main camera it is needed.</param>
        public static void EnableEditorMode(GameObject3D mainCamera)
        {
            if (mainCamera == null)
                throw new ArgumentNullException("mainCamera");
            if (mainCamera.Camera == null)
                throw new ArgumentException("Editor Manager: Unable to activate editor mode. The game object passed does not have a camera component", "mainCamera");
            if (editorModeEnabled)
                return;
            editorModeEnabled = true;
            gameMainCamera = mainCamera;
            gameMainCamera.Camera.Visible = false;
            editorCamera.Camera.Visible = true;
        } // EnableEditorMode

        /// <summary>
        /// Disable editor mode
        /// </summary>
        public static void DisableEditorMode()
        {
            if (!editorModeEnabled)
                return;
            editorModeEnabled = false;
            gameMainCamera.Camera.Visible = true;
            editorCamera.Camera.Visible = false;
            gameMainCamera = null;
        } // DisableEditorMode

        #endregion
        
        #region Update

        /// <summary>
        /// Manipula la escena. Pero no renderiza nada en la pantalla.
        /// </summary>
        public static void Update()
        {
            if (!editorModeEnabled)
                return;
            /*
            if (Camera.MainCamera == null)
                return; // No camera, no editor. (for know at least)
            if (camera.Camera != Camera.MainCamera)
            {
                if (camera != null)
                    camera.RemoveComponent<ScriptEditorCamera>();
                ScriptEditorCamera script = (ScriptEditorCamera)camera.AddComponent<ScriptEditorCamera>();
            }
            */
            
            #region Frame Object and Reset Camera
            /*
            if (selectedObject != null && Keyboard.KeyJustPressed(Keys.F))
            {
                (camera.Camera).LookAtPosition = selectedObject.CenterPoint;
                ((XSICamera)ApplicationLogic.Camera).Distance = selectedObject.BoundingSphereOptimized.Radius * 3;
            }
            if (Keyboard.KeyJustPressed(Keys.R))
            {
                ((XSICamera)ApplicationLogic.Camera).LookAtPosition = new Vector3(0, 0.5f, 0);
                ((XSICamera)ApplicationLogic.Camera).Distance = 15;
            }
            */
            #endregion
            /*
            // Si el manipulador no esta activo //
            if (activeManipulator == Manipulator.None)
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
                if ((Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed) && !(Gizmo.Active))
                {
                    activeManipulator = Manipulator.None;
                    MousePointer.ManipulatorMode = false;
                }
            }
            // Habilitamos manipuladores, si es posible.
            isPosibleToSwich = selectedObject != null && ((activeManipulator == Manipulator.None) || !(Gizmo.Active));
            if (Keyboard.KeyJustPressed(Keys.X) && isPosibleToSwich)
            {
                activeManipulator = Manipulator.Scale;
                MousePointer.ManipulatorMode = true;
                GizmoScale.InitializeManipulator(selectedObject);
            }
            if (Keyboard.KeyJustPressed(Keys.C) && isPosibleToSwich)
            {
                activeManipulator = Manipulator.Rotation;
                MousePointer.ManipulatorMode = true;
                GizmoRotation.InitializeManipulator(selectedObject);
            }
            if (Keyboard.KeyJustPressed(Keys.V) && isPosibleToSwich)
            {
                activeManipulator = Manipulator.Translation;
                MousePointer.ManipulatorMode = true;
                GizmoTranslation.InitializeManipulator(selectedObject);
            }
            // Trabajamos con el manipulador activo
            switch (activeManipulator)
            {
                case Manipulator.Scale: GizmoScale.ManipulateObject(); break;
                case Manipulator.Rotation: GizmoRotation.ManipulateObject(); break;
                case Manipulator.Translation: GizmoTranslation.ManipulateObject(); break;
            }
            // Si el manipulador produjo un resultado
            if (Gizmo.ProduceTransformation)
            {
                undoStack.Push(new UndoStruct(selectedObject, Gizmo.OldLocalMatrix));
            }
            // Undo y Redo (// TODO!!!)
            if (Keyboard.KeyPressed(Keys.LeftControl) &&
                Keyboard.KeyJustPressed(Keys.Z) &&
                (activeManipulator == Manipulator.None || !(Gizmo.Active)))
            {
                if (undoStack.Count > 0)
                {
                    undoStack.Peek().obj.LocalMatrix = undoStack.Peek().localMatrix;
                    undoStack.Pop();
                    // Reiniciamos el manipulador
                    switch (activeManipulator)
                    {
                        case Manipulator.Scale: GizmoScale.InitializeManipulator(selectedObject); break;
                        case Manipulator.Rotation: GizmoRotation.InitializeManipulator(selectedObject); break;
                        case Manipulator.Translation: GizmoTranslation.InitializeManipulator(selectedObject); break;
                    }
                }
            }*/
        } // ManipulateScene

        #endregion
        /*
        #region Render Feedback

        /// <summary>
        /// Nos muestra en pantalla los elementos de la manipulacion.
        /// </summary>
        public void Render()
        {
            if (selectedObject != null)
            {
                Primitives.DrawBoundingBox(selectedObject.BoundingBox, new Color(100,150,250));
                switch (activeManipulator)
                {
                    case Manipulator.Scale: GizmoScale.RenderManipulator(); break;
                    case Manipulator.Rotation: GizmoRotation.RenderManipulator(); break;
                    case Manipulator.Translation: GizmoTranslation.RenderManipulator(); break;
                }
            }
        } // Render

        #endregion
        */
    } // EditorManager
} // XNAFinalEngine.Editor
