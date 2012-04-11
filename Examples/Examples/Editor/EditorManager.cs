
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Components;
using XNAFinalEngine.UserInterface;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Mouse = XNAFinalEngine.Input.Mouse;
using Size = XNAFinalEngine.Helpers.Size;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// This put all the editor pieces together.
    /// The editor is inspired by Unity3D and Softimage XSI.
    /// </summary>
    /// <remarks>
    /// The editor is not garbage free because the editor uses the user interface (based in Neo Force Control).
    /// The user interface was heavily modified and improved but the garbage was not removed.
    /// Moreover the editor uses the texture picking method that stall the CPU but brings the best accuracy.
    /// The editor is not a good place to do any optimizations. I even use LinQ.
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
        /// The different gizmos.
        /// </summary>
        private enum Gizmo
        {
            None,
            Scale,
            Rotation,
            Translation
        }; // Gizmo

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
        private static ScriptEditorCamera editorCameraScript;

        // The game main camera.
        private static GameObject3D gameMainCamera;

        // The picker to select an object from the screen.
        private static Picker picker;

        // The active gizmo.
        private static Gizmo activeGizmo = Gizmo.None;

        // The selected object.
        private static List<GameObject> selectedObjects = new List<GameObject>();

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

        // Indicates if the editor is active.
        private static bool editorModeEnabled;

        // An easy way to avoid a mouse click in the scene world when a control is closed.
        private static Control previousFocusedControl;

        private static GameObject2D selectionRectangle, selectionRectangleBackground;

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
            editorCamera.Camera.Visible = false;
            editorCamera.Camera.RenderingOrder = int.MaxValue;
            editorCameraScript = (ScriptEditorCamera)editorCamera.AddComponent<ScriptEditorCamera>();
            editorCameraScript.Mode = ScriptEditorCamera.ModeType.Maya;
            // Reset camera to default position and orientation.
            ResetEditorCamera();

            // Selection game objects.
            selectionRectangle = new GameObject2D();
            selectionRectangle.AddComponent<LineRenderer>();
            selectionRectangle.LineRenderer.Vertices = new VertexPositionColor[4 * 2];
            selectionRectangle.LineRenderer.Visible = false;
            selectionRectangle.Layer = Layer.GetLayerByNumber(31);
            selectionRectangleBackground = new GameObject2D();
            selectionRectangleBackground.AddComponent<LineRenderer>();
            selectionRectangleBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            selectionRectangleBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            selectionRectangleBackground.LineRenderer.Visible = false;
            selectionRectangleBackground.Layer = Layer.GetLayerByNumber(31);

            // Call the manager's update and render methods in the correct order without explicit calls. 
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
            // Copy camera parameters to editor camera TODO!!
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

        #region Reset Editor Camera

        /// <summary>
        /// Reset camera to default position and orientation.
        /// </summary>
        private static void ResetEditorCamera()
        {
            editorCameraScript.LookAtPosition = new Vector3(0, 0.5f, 0);
            editorCameraScript.Distance = 30;
            editorCameraScript.Pitch = 0;
            editorCameraScript.Yaw = 0;
            editorCameraScript.Roll = 0;
        } // ResetEditorCamera

        #endregion
        
        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public static void Update()
        {

            #region If no update is need it...

            if (!editorModeEnabled || 
                // Keyboard shortcuts, camera movement and similar should be ignored when the text box is active.
                (UserInterfaceManager.FocusedControl != null && UserInterfaceManager.FocusedControl is TextBox) ||
                // If the camera is being manipulated…
                (editorCameraScript.Manipulating))
            {
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region Frame Object

            // Adjust the look at position and distance to frame the selected objects.
            // The orientation is not afected.
            if (Keyboard.KeyJustPressed(Keys.F))
            {
                BoundingSphere? frameBoundingSphere = null; // Gabage is not an issue in the editor.
                foreach (var gameObject in selectedObjects)
                {
                    if (gameObject is GameObject3D && ((GameObject3D)gameObject).ModelRenderer != null)
                    {
                        if (frameBoundingSphere == null)
                            frameBoundingSphere = ((GameObject3D)gameObject).ModelRenderer.BoundingSphere;
                        else
                            frameBoundingSphere = BoundingSphere.CreateMerged(frameBoundingSphere.Value, ((GameObject3D)gameObject).ModelRenderer.BoundingSphere);
                    }
                    // The rest of objects TODO!!!
                }
                if (frameBoundingSphere != null)
                {
                    editorCameraScript.LookAtPosition = frameBoundingSphere.Value.Center;
                    editorCameraScript.Distance = frameBoundingSphere.Value.Radius * 3;
                }
            }

            #endregion

            #region Reset Camera

            // Reset camera to default position and orientation.
            if (Keyboard.KeyJustPressed(Keys.R))
            {
                ResetEditorCamera();
            }

            #endregion

            #region If no update is need it...

            if (UserInterfaceManager.FocusedControl != null || previousFocusedControl != null)
            {
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            // If no gizmo is active…
            if (activeGizmo == Gizmo.None)
            {

                #region Selection Rectangle

                if (Mouse.LeftButtonPressed)
                {
                    Color lineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                    Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);
                    selectionRectangle.LineRenderer.Visible = true;
                    selectionRectangle.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[6] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[7] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangleBackground.LineRenderer.Visible = true;
                    selectionRectangleBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                }
                else
                {
                    selectionRectangleBackground.LineRenderer.Visible = false;
                    selectionRectangle.LineRenderer.Visible = false;
                }

                #endregion

                #region Selection of objects

                if (Mouse.LeftButtonJustReleased)
                {

                    #region Clear selection list when control keys and shift keys are not pressed

                    if (!Keyboard.KeyPressed(Keys.LeftControl) && !Keyboard.KeyPressed(Keys.LeftShift) &&
                        !Keyboard.KeyPressed(Keys.RightControl) && !Keyboard.KeyPressed(Keys.RightShift))
                    {
                        // Remove bounding box off the screen.
                        foreach (var gameObject in selectedObjects)
                        {
                            // If it is a model.
                            if (gameObject is GameObject3D && ((GameObject3D)gameObject).ModelRenderer != null)
                                ((GameObject3D)gameObject).ModelRenderer.RenderBoundingBox = false;
                            // ... TODO!!!!
                        }
                        selectedObjects.Clear();
                    }

                    #endregion

                    #region Pick objects

                    List<GameObject> newSelectedObjects = new List<GameObject>();
                    if (Mouse.NoDragging)
                        newSelectedObjects.Add(picker.Pick(editorCamera.Camera.ViewMatrix, editorCamera.Camera.ProjectionMatrix));
                    else
                        newSelectedObjects = picker.Pick(Mouse.DraggingRectangle, editorCamera.Camera.ViewMatrix, editorCamera.Camera.ProjectionMatrix);

                    #endregion

                    #region Add or Remove objects

                    // Add the bounding box on the screen.
                    foreach (var gameObject in newSelectedObjects)
                    {
                        if (!Keyboard.KeyPressed(Keys.LeftControl) && !Keyboard.KeyPressed(Keys.LeftShift) &&
                            !Keyboard.KeyPressed(Keys.RightControl) && !Keyboard.KeyPressed(Keys.RightShift))
                        {
                            selectedObjects.Add(gameObject);
                            ChangeGameObjectBoundingBoxVisibility(gameObject, true);
                        }
                        else if ((Keyboard.KeyPressed(Keys.LeftControl) || Keyboard.KeyPressed(Keys.RightControl)) &&
                                 (!Keyboard.KeyPressed(Keys.LeftShift) && !Keyboard.KeyPressed(Keys.RightShift)))
                        {
                            if (selectedObjects.Contains(gameObject))
                            {
                                selectedObjects.Remove(gameObject);
                                ChangeGameObjectBoundingBoxVisibility(gameObject, false);
                            }
                            else
                            {
                                selectedObjects.Add(gameObject);
                                ChangeGameObjectBoundingBoxVisibility(gameObject, true);
                            }
                        }
                        else if ((Keyboard.KeyPressed(Keys.LeftControl) || Keyboard.KeyPressed(Keys.RightControl)) &&
                                 (Keyboard.KeyPressed(Keys.LeftShift) || Keyboard.KeyPressed(Keys.RightShift)))
                        {
                            if (selectedObjects.Contains(gameObject))
                            {
                                selectedObjects.Remove(gameObject);
                                ChangeGameObjectBoundingBoxVisibility(gameObject, false);
                            }
                        }
                        else if ((!Keyboard.KeyPressed(Keys.LeftControl) && !Keyboard.KeyPressed(Keys.RightControl)) &&
                                 (Keyboard.KeyPressed(Keys.LeftShift) || Keyboard.KeyPressed(Keys.RightShift)))
                        {
                            if (!selectedObjects.Contains(gameObject))
                            {
                                selectedObjects.Add(gameObject);
                                ChangeGameObjectBoundingBoxVisibility(gameObject, true);
                            }
                        }
                    }

                    #endregion

                }

                #endregion

                #region Deselect selected objects

                if (Keyboard.EscapeJustPressed /*|| Keyboard.SpaceJustPressed*/)
                {
                    // Remove bounding box off the screen.
                    foreach (var gameObject in selectedObjects)
                    {
                        ChangeGameObjectBoundingBoxVisibility(gameObject, false);
                    }
                    selectedObjects.Clear();
                }

                #endregion

            }


            /*// Si se apreta escape o espacio y un manipulador esta activo
            else
            {
                if ((Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed) && !(Gizmo.Active))
                {
                    activeGizmo = Gizmo.None;
                    MousePointer.ManipulatorMode = false;
                }
            }
            // Habilitamos manipuladores, si es posible.
            isPosibleToSwich = selectedObject != null && ((activeGizmo == Gizmo.None) || !(Gizmo.Active));
            if (Keyboard.KeyJustPressed(Keys.X) && isPosibleToSwich)
            {
                activeGizmo = Gizmo.Scale;
                MousePointer.ManipulatorMode = true;
                GizmoScale.InitializeManipulator(selectedObject);
            }
            if (Keyboard.KeyJustPressed(Keys.C) && isPosibleToSwich)
            {
                activeGizmo = Gizmo.Rotation;
                MousePointer.ManipulatorMode = true;
                GizmoRotation.InitializeManipulator(selectedObject);
            }
            if (Keyboard.KeyJustPressed(Keys.V) && isPosibleToSwich)
            {
                activeGizmo = Gizmo.Translation;
                MousePointer.ManipulatorMode = true;
                GizmoTranslation.InitializeManipulator(selectedObject);
            }
            // Trabajamos con el manipulador activo
            switch (activeGizmo)
            {
                case Gizmo.Scale: GizmoScale.ManipulateObject(); break;
                case Gizmo.Rotation: GizmoRotation.ManipulateObject(); break;
                case Gizmo.Translation: GizmoTranslation.ManipulateObject(); break;
            }
            // Si el manipulador produjo un resultado
            if (Gizmo.ProduceTransformation)
            {
                undoStack.Push(new UndoStruct(selectedObject, Gizmo.OldLocalMatrix));
            }
            // Undo y Redo (// TODO!!!)
            if (Keyboard.KeyPressed(Keys.LeftControl) &&
                Keyboard.KeyJustPressed(Keys.Z) &&
                (activeGizmo == Gizmo.None || !(Gizmo.Active)))
            {
                if (undoStack.Count > 0)
                {
                    undoStack.Peek().obj.LocalMatrix = undoStack.Peek().localMatrix;
                    undoStack.Pop();
                    // Reiniciamos el manipulador
                    switch (activeGizmo)
                    {
                        case Gizmo.Scale: GizmoScale.InitializeManipulator(selectedObject); break;
                        case Gizmo.Rotation: GizmoRotation.InitializeManipulator(selectedObject); break;
                        case Gizmo.Translation: GizmoTranslation.InitializeManipulator(selectedObject); break;
                    }
                }
            }*/
        } // Update

        private static void ChangeGameObjectBoundingBoxVisibility(GameObject gameObject, bool boundingBoxVisibility)
        {
            // If it is a model.
            if (gameObject is GameObject3D && ((GameObject3D)gameObject).ModelRenderer != null)
                ((GameObject3D)gameObject).ModelRenderer.RenderBoundingBox = boundingBoxVisibility;
            // ... TODO!!!
        } // ChangeGameObjectBoundingBoxVisibility

        #endregion

    } // EditorManager
} // XNAFinalEngine.Editor
