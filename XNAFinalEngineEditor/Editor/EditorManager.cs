
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
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

            /// <summary>
            /// Tasks executed during the last stage of the scene render.
            /// </summary>
            public override void PreRenderUpdate()
            {
                EditorManager.PreRenderTask();
            }

            /// <summary>
            /// Tasks executed during the last stage of the scene render.
            /// </summary>
            public override void PostRenderUpdate()
            {
                EditorManager.PostRenderTasks();
            }

        } // ScripEditorManager

        #endregion

        #region Enumerates

        /// <summary>
        /// The different gizmos.
        /// </summary>
        private enum GizmoType
        {
            None,
            Scale,
            Rotation,
            Translation
        }; // Gizmo

        /// <summary>
        /// This indicate the mode of the viewport (scene, game, etc.).
        /// </summary>
        private enum ViewportModeType
        {
            /// <summary>
            /// The editor camera.
            /// </summary>
            Scene,
            /// <summary>
            /// The game camera.
            /// </summary>
            Game,
        } // ViewportModeType

        #endregion

        #region Variables

        // The editor camera.
        private static GameObject3D editorCamera, gizmoCamera;
        private static ScriptEditorCamera editorCameraScript;
        
        // The picker to select an object from the screen.
        private static Picker picker;

        // The active gizmo.
        private static GizmoType activeGizmo = GizmoType.None;

        // The selected object.
        private static readonly List<GameObject3D> selectedObjects = new List<GameObject3D>();

        // To avoid more than one initialization.
        private static bool initialized;

        // Used to call the update and render method in the correct order without explicit calls.
        private static GameObject editorManagerGameObject;

        // Indicates if the editor is active.
        private static bool editorModeEnabled;

        // An easy way to avoid a mouse click in the scene world when a control is closed.
        private static Control previousFocusedControl;

        private static GameObject2D selectionRectangle, selectionRectangleBackground;

        // The gizmos.
        private static TranslationGizmo translationGizmo;

        // The user interface control for the viewport.
        private static Container renderSpace;
        // The user interface control for the right panel.
        private static TabControl rightPanelTabControl;

        
        #endregion

        #region Properties

        /// <summary>
        /// Is the editor mode enabled?
        /// </summary>
        public static bool EditorModeEnabled { get { return editorModeEnabled; } }

        /// <summary>
        /// This indicate the mode of the viewport (scene, game, etc.).
        /// </summary>
        private static ViewportModeType ViewportMode { get; set; }

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
            // Call the manager's update and render methods in the correct order without explicit calls. 
            editorManagerGameObject = new GameObject2D();
            editorManagerGameObject.AddComponent<ScripEditorManager>();
            // If it already initialize don't worry.
            UserInterfaceManager.Initialize();
            UserInterfaceManager.UserInterfaceVisible = false;
            picker = new Picker(Size.FullScreen);

            #region Cameras

            // Editor Camera
            editorCamera = new GameObject3D();
            editorCamera.AddComponent<Camera>();
            editorCamera.Camera.AmbientLight = new AmbientLight();
            editorCamera.Camera.PostProcess = new PostProcess();
            editorCamera.Camera.PostProcess.Bloom.Enabled = false;
            editorCamera.Camera.Visible = false;
            editorCamera.Camera.RenderHeadUpDisplay = false;
            
            editorCameraScript = (ScriptEditorCamera)editorCamera.AddComponent<ScriptEditorCamera>();
            editorCameraScript.Mode = ScriptEditorCamera.ModeType.Maya;
            ResetEditorCamera(); // Reset camera to default position and orientation.
            // Camera to render the gizmos. 
            // This is done because the gizmos need to be in front of everything and I can't clear the ZBuffer wherever I want.
            gizmoCamera = new GameObject3D();
            gizmoCamera.AddComponent<Camera>();
            gizmoCamera.Camera.Visible = false;
            gizmoCamera.Camera.CullingMask = Layer.GetLayerByNumber(31).Mask; // The editor layer.
            gizmoCamera.Camera.ClearColor = Color.Transparent;

            editorCamera.Camera.MasterCamera = gizmoCamera.Camera;
            gizmoCamera.Camera.RenderingOrder = int.MaxValue;

            #endregion

            translationGizmo = new TranslationGizmo(gizmoCamera);

            #region Selection Rectangle

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

            #endregion

            #region User Interface Controls

            #region Canvas

            // The canvas cover the whole window. I will place the static controls there.
            // I don’t place the controls directly to the manager to allow having some functionality
            // provided by the canvas like the automatic placing of menu items, status bar, etc.
            Container canvas = new Container
            {
                Top = 0,
                Left = 0,
                Width = Screen.Width,
                Height = Screen.Height,
                Anchor = Anchors.All,
                AutoScroll = false,
                BackgroundColor = Color.Transparent,
                StayOnBack = true,
                Passive = true,
                CanFocus = false,
            };
            
            #endregion

            #region Main Menu

            canvas.MainMenu = new MainMenu
            {
                Width = Screen.Width,
                Anchor = Anchors.Left | Anchors.Top,
                CanFocus = false
            };
            MenuItem menuItemFile = new MenuItem("File", true);
            canvas.MainMenu.Items.Add(menuItemFile);
            menuItemFile.ChildrenItems.AddRange(new[] { new MenuItem("New Scene"), new MenuItem("Open Scene"), new MenuItem("Exit", true) });
            MenuItem menuItemEdit = new MenuItem("Edit", true);
            canvas.MainMenu.Items.Add(menuItemEdit);
            menuItemEdit.ChildrenItems.AddRange(new[] { new MenuItem("Undo"), new MenuItem("Redo") });
            
            #endregion

            #region Top Panel

            ToolBarPanel topPanel = new ToolBarPanel();
            canvas.ToolBarPanel = topPanel;

            ToolBar toolBarTopPanel = new ToolBar
            {
                Parent = topPanel,
                Movable = true,
                FullRow = true
            };
            Button buttonSpace = new Button
            {
                Text = "Global",
                Left = 10,
                Top = 5,
                Height = 15,
                Parent = toolBarTopPanel,
                CanFocus = false,
            };
            buttonSpace.Click += delegate
            {
                if (Gizmo.Space == Gizmo.SpaceMode.Local)
                {
                    Gizmo.Space = Gizmo.SpaceMode.Global;
                    buttonSpace.Text = "Global";
                }
                else
                {
                    Gizmo.Space = Gizmo.SpaceMode.Local;
                    buttonSpace.Text = "Local";
                }
                buttonSpace.Focused = false;
            };
            TabControl tabControlRenderSpace = new TabControl
            {
                Left = 0,
                Top = buttonSpace.Top + buttonSpace.Height + 5,
                Width = 150,
                Height = 20,
                Anchor = Anchors.All,
                Parent = toolBarTopPanel,
            };
            tabControlRenderSpace.AddPage();
            tabControlRenderSpace.TabPages[0].Text = "Scene";
            tabControlRenderSpace.AddPage();
            tabControlRenderSpace.TabPages[1].Text = "Game";
            topPanel.Height = tabControlRenderSpace.Top + tabControlRenderSpace.Height;
            toolBarTopPanel.Height = tabControlRenderSpace.Top + tabControlRenderSpace.Height;
            tabControlRenderSpace.PageChanged += delegate
            {
                if (tabControlRenderSpace.SelectedPage == tabControlRenderSpace.TabPages[0])
                    ViewportMode = ViewportModeType.Scene;
                else
                    ViewportMode = ViewportModeType.Game;
            };

            #endregion

            #region Render Space

            // The canvas cover the whole window. I will place the static controls there.
            // I don’t place the controls directly to the manager to allow having some functionality
            // provided by the canvas like the automatic placing of menu items, status bar, etc.
            renderSpace = new Container
            {
                Top = 0,
                Left = 0,
                Width = canvas.ClientWidth - 420,
                Height = canvas.ClientHeight,
                Anchor = Anchors.All,
                AutoScroll = false,
                BackgroundColor = Color.Transparent,
                StayOnBack = true,
                Passive = true,
                CanFocus = false,
                Parent = canvas,
            };

            #endregion

            #region Right Panel

            Panel rightPanel = new Panel
            {
                StayOnBack = true,
                Passive = true,
                Parent = canvas,
                Width = 420,
                Left = canvas.ClientWidth - 420,
                Height = canvas.ClientHeight,
                Anchor = Anchors.Right | Anchors.Top | Anchors.Bottom,
                Color = new Color(64, 64, 64),
            };

            rightPanelTabControl = new TabControl
            {
                Parent = rightPanel,
                Left = 2,
                Top = 2,
                Width = rightPanel.ClientWidth - 2,
                Height = rightPanel.ClientHeight - 2,
                Anchor = Anchors.All
            };
            rightPanelTabControl.AddPage();
            rightPanelTabControl.TabPages[0].Text = "Inspector";

            #endregion

            #endregion

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
        public static void EnableEditorMode()
        {
            if (editorModeEnabled)
                return;
            editorModeEnabled = true;
            UserInterfaceManager.UserInterfaceVisible = true;
        } // EnableEditorMode

        /// <summary>
        /// Disable editor mode
        /// </summary>
        public static void DisableEditorMode()
        {
            if (!editorModeEnabled)
                return;
            UserInterfaceManager.UserInterfaceVisible = false;
            editorModeEnabled = false;
            Camera.OnlyRendereableCamera = null;
            editorCamera.Camera.Visible = false;
            gizmoCamera.Camera.Visible = false;
            // Remove bounding box off the screen.
            foreach (var gameObject in selectedObjects)
            {
                ChangeGameObjectBoundingBoxVisibility(gameObject, false);
            }
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

        #region Change GameObject Bounding Box Visibility

        private static void ChangeGameObjectBoundingBoxVisibility(GameObject gameObject, bool boundingBoxVisibility)
        {
            // If it is a model.
            if (gameObject is GameObject3D && ((GameObject3D)gameObject).ModelRenderer != null)
                ((GameObject3D)gameObject).ModelRenderer.RenderBoundingBox = boundingBoxVisibility;
            // ... TODO!!!
        } // ChangeGameObjectBoundingBoxVisibility

        #endregion

        #region Add and Remove Controls From Inspector

        /// <summary>
        /// Add User Interface controls to the inspector panel.
        /// </summary>
        private static void AddControlsToInspector()
        {
            var panel = new PanelCollapsible();
            panel.Anchor = Anchors.Left | Anchors.Right | Anchors.Top;
            panel.Parent = rightPanelTabControl.TabPages[0];
            panel.Width = rightPanelTabControl.TabPages[0].ClientWidth;
            panel.Text = "Transform";
            CommonControls.Vector3Box("Position", panel, new Vector3(2, 1, 4));
            CommonControls.Vector3Box("Rotation", panel, new Vector3(4, 1, 4));
            CommonControls.Vector3Box("Scale", panel, new Vector3(2, 1, 4));
        } // AddControlsToInspector

        /// <summary>
        /// Remove User Interface controls from inspector panel.
        /// </summary>
        private static void RemoveControlsFromInspector()
        {
            rightPanelTabControl.TabPages[0].RemoveControlsFromClientArea();
        } // RemoveControlsFromInspector

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public static void Update()
        {

            #region If no update is needed...

            if (!editorModeEnabled)
            {
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region Prepare viewport mode

            if (ViewportMode == ViewportModeType.Scene)
            {
                Camera.OnlyRendereableCamera = gizmoCamera.Camera;
                editorCamera.Camera.Visible = true;
                gizmoCamera.Camera.Visible = true;
                // Restore bounding box to the current selected objects.
                foreach (var gameObject in selectedObjects)
                {
                    ChangeGameObjectBoundingBoxVisibility(gameObject, true);
                }
            }
            else
            {
                Camera.OnlyRendereableCamera = null;
                editorCamera.Camera.Visible = false;
                gizmoCamera.Camera.Visible = false;
                // Remove the bounding box in game mode.
                foreach (var gameObject in selectedObjects)
                {
                    ChangeGameObjectBoundingBoxVisibility(gameObject, false);
                }
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region If no update is needed...

            // Keyboard shortcuts, camera movement and similar should be ignored when the text box is active.
            if ((UserInterfaceManager.FocusedControl != null && UserInterfaceManager.FocusedControl is TextBox))
            {
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region Update Gizmo Rendering Information

            gizmoCamera.Transform.WorldMatrix = editorCamera.Transform.WorldMatrix;
            gizmoCamera.Camera.ProjectionMatrix = editorCamera.Camera.ProjectionMatrix;
            switch (activeGizmo)
            {
                //case GizmoType.Scale: GizmoScale.ManipulateObject(); break;
                //case GizmoType.Rotation: GizmoRotation.ManipulateObject(); break;
                case GizmoType.Translation: translationGizmo.UpdateRenderingInformation(); break;
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
                    if (gameObject.ModelRenderer != null)
                    {
                        if (frameBoundingSphere == null)
                            frameBoundingSphere = gameObject.ModelRenderer.BoundingSphere;
                        else
                            frameBoundingSphere = BoundingSphere.CreateMerged(frameBoundingSphere.Value, gameObject.ModelRenderer.BoundingSphere);
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

            #region If no update is needed... (besides gizmo feedback, frame object and reset camera)

            if (editorCameraScript.Manipulating || (UserInterfaceManager.FocusedControl != null || previousFocusedControl != null))
            {
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region No Gizmo Active

            // If no gizmo is active…
            if (activeGizmo == GizmoType.None)
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
                    
                    RemoveControlsFromInspector();

                    #region Clear selection list when control keys and shift keys are not pressed

                    if (!Keyboard.KeyPressed(Keys.LeftControl) && !Keyboard.KeyPressed(Keys.LeftShift) &&
                        !Keyboard.KeyPressed(Keys.RightControl) && !Keyboard.KeyPressed(Keys.RightShift))
                    {
                        // Remove bounding box off the screen.
                        foreach (var gameObject in selectedObjects)
                        {
                            ChangeGameObjectBoundingBoxVisibility(gameObject, false);
                        }
                        selectedObjects.Clear();
                    }

                    #endregion

                    #region Pick objects

                    Viewport viewport = new Viewport(editorCamera.Camera.Viewport.X, editorCamera.Camera.Viewport.Y, 
                                                     editorCamera.Camera.Viewport.Width, editorCamera.Camera.Viewport.Height);
                    List<GameObject3D> newSelectedObjects = new List<GameObject3D>();
                    if (Mouse.NoDragging)
                    {
                        GameObject gameObject = picker.Pick(editorCamera.Camera.ViewMatrix, editorCamera.Camera.ProjectionMatrix, viewport);
                        if (gameObject != null && gameObject is GameObject3D)
                            newSelectedObjects.Add((GameObject3D)gameObject);
                    }
                    else
                    {
                        List<GameObject> pickedObjects = picker.Pick(Mouse.DraggingRectangle, editorCamera.Camera.ViewMatrix, editorCamera.Camera.ProjectionMatrix, viewport);
                        foreach (GameObject pickedObject in pickedObjects)
                        {
                            if (pickedObject is GameObject3D)
                            {
                                newSelectedObjects.Add((GameObject3D)pickedObject);
                            }
                        }
                    }

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

                    AddControlsToInspector();
                }

                #endregion

                #region Deselect selected objects

                if (Keyboard.EscapeJustPressed)
                {
                    RemoveControlsFromInspector();
                    // Remove bounding box off the screen.
                    foreach (var gameObject in selectedObjects)
                    {
                        ChangeGameObjectBoundingBoxVisibility(gameObject, false);
                    }
                    selectedObjects.Clear();
                }

                #endregion
                
            }

            #endregion

            #region Gizmo Active

            if (activeGizmo != GizmoType.None && (Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed) && !(Gizmo.Active))
            {
                switch (activeGizmo)
                {
                    case GizmoType.Translation: translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = GizmoType.None;
            }

            #endregion

            #region Activate Gizmo

            bool isPosibleToSwich = selectedObjects.Count > 0 && ((activeGizmo == GizmoType.None) || !(Gizmo.Active));
            if (Keyboard.KeyJustPressed(Keys.W) && isPosibleToSwich)
            {
                switch (activeGizmo)
                {
                    case GizmoType.Translation: translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = GizmoType.Translation;
                translationGizmo.EnableGizmo(selectedObjects, picker);
            }

            #endregion

            #region Update Gizmo

            switch (activeGizmo)
            {
                //case GizmoType.Scale: GizmoScale.ManipulateObject(); break;
                //case GizmoType.Rotation: GizmoRotation.ManipulateObject(); break;
                case GizmoType.Translation: translationGizmo.Update(); break;
            }

            #endregion

            previousFocusedControl = UserInterfaceManager.FocusedControl;
        } // Update

        #endregion

        #region Render Tasks

        public static void PreRenderTask()
        {
            if (ViewportMode == ViewportModeType.Scene)
            {
                // The editor camera only use part of the render target.
                editorCamera.Camera.Viewport = new Rectangle(renderSpace.ClientArea.ControlLeftAbsoluteCoordinate,
                                                             renderSpace.ClientArea.ControlTopAbsoluteCoordinate,
                                                             renderSpace.ClientArea.Width, renderSpace.ClientArea.Height);
                gizmoCamera.Camera.Viewport = new Rectangle(renderSpace.ClientArea.ControlLeftAbsoluteCoordinate,
                                                            renderSpace.ClientArea.ControlTopAbsoluteCoordinate,
                                                            renderSpace.ClientArea.Width, renderSpace.ClientArea.Height);
            }
        }

        /// <summary>
        /// Tasks after the engine render.
        /// Probably you won’t need to place any task here.
        /// </summary>
        public static void PostRenderTasks()
        {
            if (editorModeEnabled && ViewportMode == ViewportModeType.Game)
            {
                EngineManager.Device.Clear(Color.Black);
                Camera mainCamera = Camera.MainCamera;
                if (mainCamera != null)
                {
                    // Aspect ratio
                    Rectangle screenRectangle;
                    float renderTargetAspectRatio = mainCamera.RenderTarget.Width / (float)mainCamera.RenderTarget.Height,
                          renderSpaceAspectRatio = renderSpace.ClientArea.Width / (float)renderSpace.ClientArea.Height;

                    if (renderTargetAspectRatio > renderSpaceAspectRatio)
                    {
                        float vsAspectRatio = renderTargetAspectRatio / renderSpaceAspectRatio;
                        int blackStripe = (int)((renderSpace.ClientArea.Height - (renderSpace.ClientArea.Height / vsAspectRatio)) / 2);
                        screenRectangle = new Rectangle(renderSpace.ControlLeftAbsoluteCoordinate, renderSpace.ControlTopAbsoluteCoordinate + blackStripe,
                                                        renderSpace.ClientArea.Width, renderSpace.ClientArea.Height - blackStripe * 2);
                    }
                    else
                    {
                        float vsAspectRatio = renderSpaceAspectRatio / renderTargetAspectRatio;
                        int blackStripe = (int)((renderSpace.ClientArea.Width - (renderSpace.ClientArea.Width / vsAspectRatio)) / 2);
                        screenRectangle = new Rectangle(renderSpace.ControlLeftAbsoluteCoordinate + blackStripe, renderSpace.ControlTopAbsoluteCoordinate,
                                                        renderSpace.ClientArea.Width - blackStripe * 2, renderSpace.ClientArea.Height);
                    }
                    SpriteManager.Begin2D();
                    SpriteManager.Draw2DTexture(mainCamera.RenderTarget, 0,
                                                screenRectangle,
                                                null, Color.White, 0, Vector2.Zero);
                    SpriteManager.End();
                }
            }
        } // PostRenderTasks

        #endregion

    } // EditorManager
} // XNAFinalEngine.Editor
