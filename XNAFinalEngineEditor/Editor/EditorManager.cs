
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Texture = XNAFinalEngine.Assets.Texture;
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
            public override void Update() { EditorManager.Update(); }
            public override void PreRenderUpdate()  { PreRenderTask(); }
            public override void PostRenderUpdate() { PostRenderTasks(); }
        } // ScripEditorManager

        #endregion

        #region Enumerates

        /// <summary>
        /// The different gizmos.
        /// </summary>
        private enum GizmoSelected
        {
            None,
            Scale,
            Rotation,
            Translation
        }; // GizmoSelected

        /// <summary>
        /// The different layout options.
        /// </summary>
        public enum LayoutOptions
        {
            FourSplit,
            ThreeSplit,
            Wide,
        }; // LayoutOptions

        #endregion

        #region Variables

        // To avoid more than one initialization.
        private static bool initialized;

        // Indicates if the editor is active.
        private static bool editorModeEnabled;

        // Used to call the update and render method in the correct order without explicit calls.
        private static GameObject editorManagerGameObject;

        private static List<EditorViewport> editorViewports = new List<EditorViewport>();

        // Layout information.
        private static LayoutOptions currentLayout, previousLayout;
        private static EditorViewport currentWideViewport;

        #endregion

        #region Properties

        /// <summary>
        /// Is the editor mode enabled?
        /// </summary>
        public static bool EditorModeEnabled { get { return editorModeEnabled; } }

        /// <summary>
        /// The current layout in use.
        /// </summary>
        public static LayoutOptions Layout
        {
            get { return currentLayout; }
            set
            {
                currentLayout = value;
                SetLayout();
            }
        } // Layout

        /// <summary>
        /// Used to restore the previous active mask when the editor is disable. 
        /// </summary>
        public static uint GameActiveMask { get; private set; }

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
            UserInterfaceManager.Initialize(false);
            UserInterfaceManager.Visible = false;

            // Call the manager's update and render methods in the correct order without explicit calls. 
            editorManagerGameObject = new GameObject2D { Layer = Layer.GetLayerByNumber(31) };
            editorManagerGameObject.AddComponent<ScripEditorManager>();

            MainWindow.AddMainControls();

            // Create the four viewports. They are reused.
            editorViewports.Add(new EditorViewport(new RectangleF(0, 0, 0.5f, 0.5f), EditorViewport.ViewportMode.Top));
            editorViewports.Add(new EditorViewport(new RectangleF(0.5f, 0, 0.5f, 0.5f), EditorViewport.ViewportMode.Perspective));
            editorViewports.Add(new EditorViewport(new RectangleF(0, 0.5f, 0.5f, 0.5f), EditorViewport.ViewportMode.Game));
            editorViewports.Add(new EditorViewport(new RectangleF(0.5f, 0.5f, 0.5f, 0.5f), EditorViewport.ViewportMode.Right));
            Layout = LayoutOptions.ThreeSplit;

            MainWindow.AddGameObjectControlsToInspector(GameObject.GameObjects[1]);
            
        } // Initialize

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
            UserInterfaceManager.Visible = true;
            UserInterfaceManager.InputEnabled = true;
            // Stop the rendering of every camera.
            // Just render the viewport cameras
            SetLayout();
            GameActiveMask = Layer.ActiveLayers;
            Layer.ActiveLayers = Layer.GetLayerByNumber(31).Mask; // Update just the editor

            GameLoop.RenderMainCameraToScreen = false;
        } // EnableEditorMode

        /// <summary>
        /// Disable editor mode
        /// </summary>
        public static void DisableEditorMode()
        {
            if (!editorModeEnabled)
                return;
            UserInterfaceManager.Visible = false;
            editorModeEnabled = false;
            // Render everything, except the viewports.
            GameLoop.CamerasToRender = null;
            foreach (EditorViewport editorViewport in editorViewports)
                editorViewport.Camera.Enabled = false;
            Layer.ActiveLayers = GameActiveMask;

            GameLoop.RenderMainCameraToScreen = true;
        } // DisableEditorMode

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
                //UserInterfaceManager.InputEnabled = true;
                /*canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;*/
                return;
            }

            #endregion

            #region Update Viewports

            foreach (EditorViewport editorViewport in editorViewports)
            {
                if (editorViewport.Enabled)
                    editorViewport.Update();
            }

            #endregion

            #region Keyboard Shortcuts

            // Toggle Layout
            if (Keyboard.KeyJustPressed(Keys.F12))
                ToggleLayout();
            // Undo System
            if (Keyboard.KeyPressed(Keys.LeftControl) && Keyboard.KeyJustPressed(Keys.Z))
                ActionManager.Undo();
            if (Keyboard.KeyPressed(Keys.LeftControl) && Keyboard.KeyJustPressed(Keys.Y))
                ActionManager.Redo();

            #endregion
            
            #region User Interface Update

            UserInterfaceManager.Update();

            #endregion
            
            #region Update Gizmo Rendering Information
            /*
            gizmoCamera.Transform.WorldMatrix = editorCamera.Transform.WorldMatrix;
            gizmoCamera.Camera.ProjectionMatrix = editorCamera.Camera.ProjectionMatrix;
            switch (activeGizmo)
            {
                case GizmoType.Scale       : scaleGizmo.UpdateRenderingInformation(); break;
                case GizmoType.Rotation    : rotationGizmo.UpdateRenderingInformation(); break;
                case GizmoType.Translation : translationGizmo.UpdateRenderingInformation(); break;
            }*/

            #endregion

            #region If no update is needed...
            /*
            // Keyboard shortcuts, camera movement and similar should be ignored when the text box is active.
            if (!UserInterfaceManager.IsOverThisControl(renderSpace, new Point(Mouse.Position.X, Mouse.Position.Y)) && !Gizmo.Active && !selectionRectangleBackground.LineRenderer.Enabled)
            {
                UserInterfaceManager.InputEnabled = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }
            */
            #endregion

            #region Frame Object
            /*
            // Adjust the look at position and distance to frame the selected objects.
            // The orientation is not afected.
            if (Keyboard.KeyJustPressed(Keys.F) && !(UserInterfaceManager.FocusedControl is TextBox))
            {
                FrameObjects(selectedObjects);
            }
            // Frame all objects.
            if (Keyboard.KeyJustPressed(Keys.A) && !(UserInterfaceManager.FocusedControl is TextBox))
            {
                List<GameObject3D> gameObject3Ds = new List<GameObject3D>();
                // We only need the 3D game objects.
                foreach (GameObject gameObject in GameObject.GameObjects)
                {
                    if (gameObject is GameObject3D && gameObject.Layer != Layer.GetLayerByNumber(31))
                        gameObject3Ds.Add((GameObject3D)gameObject);
                }
                FrameObjects(gameObject3Ds);
            }
            */
            #endregion

            #region Reset Camera
            /*
            // Reset camera to default position and orientation.
            if (Keyboard.KeyJustPressed(Keys.R) && Keyboard.KeyPressed(Keys.LeftControl) && !(UserInterfaceManager.FocusedControl is TextBox))
            {
                ResetEditorCamera();
            }
            */
            #endregion

            #region If no update is needed... (besides gizmo feedback, frame object and reset camera)
            /*
            if (editorCameraScript.Manipulating)
            {
                UserInterfaceManager.InputEnabled = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }
            */
            #endregion

            #region No Gizmo Active
            /*
            // If no gizmo is active…
            if (activeGizmo == GizmoType.None)
            {
                
                #region Selection Rectangle

                if (Mouse.LeftButtonJustPressed)
                    canSelect = true;
                if (Mouse.LeftButtonPressed && canSelect)
                {
                    Color lineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                    Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);
                    selectionRectangle.LineRenderer.Enabled = true;
                    selectionRectangle.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[6] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[7] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y, 0), lineColor);
                    selectionRectangleBackground.LineRenderer.Enabled = true;
                    selectionRectangleBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(Mouse.DraggingRectangle.X + Mouse.DraggingRectangle.Width, Mouse.DraggingRectangle.Y + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                }
                else
                {
                    selectionRectangleBackground.LineRenderer.Enabled = false;
                    selectionRectangle.LineRenderer.Enabled = false;
                }

                #endregion
                
                #region Selection of objects

                if (Mouse.LeftButtonJustReleased && canSelect)
                {
                    
                    RemoveControlsFromInspector();

                    #region Clear selection list when control keys and shift keys are not pressed

                    if (!Keyboard.KeyPressed(Keys.LeftControl) && !Keyboard.KeyPressed(Keys.LeftShift) &&
                        !Keyboard.KeyPressed(Keys.RightControl) && !Keyboard.KeyPressed(Keys.RightShift))
                    {
                        // Remove bounding box off the screen.
                        foreach (var gameObject in selectedObjects)
                        {
                            HideGameObjectSelected(gameObject);
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
                            ShowGameObjectSelected(gameObject);
                        }
                        else if ((Keyboard.KeyPressed(Keys.LeftControl) || Keyboard.KeyPressed(Keys.RightControl)) &&
                                 (!Keyboard.KeyPressed(Keys.LeftShift) && !Keyboard.KeyPressed(Keys.RightShift)))
                        {
                            if (selectedObjects.Contains(gameObject))
                            {
                                selectedObjects.Remove(gameObject);
                                HideGameObjectSelected(gameObject);
                            }
                            else
                            {
                                selectedObjects.Add(gameObject);
                                ShowGameObjectSelected(gameObject);
                            }
                        }
                        else if ((Keyboard.KeyPressed(Keys.LeftControl) || Keyboard.KeyPressed(Keys.RightControl)) &&
                                 (Keyboard.KeyPressed(Keys.LeftShift) || Keyboard.KeyPressed(Keys.RightShift)))
                        {
                            if (selectedObjects.Contains(gameObject))
                            {
                                selectedObjects.Remove(gameObject);
                                HideGameObjectSelected(gameObject);
                            }
                        }
                        else if ((!Keyboard.KeyPressed(Keys.LeftControl) && !Keyboard.KeyPressed(Keys.RightControl)) &&
                                 (Keyboard.KeyPressed(Keys.LeftShift) || Keyboard.KeyPressed(Keys.RightShift)))
                        {
                            if (!selectedObjects.Contains(gameObject))
                            {
                                selectedObjects.Add(gameObject);
                                ShowGameObjectSelected(gameObject);
                            }
                        }
                    }

                    #endregion

                    AddControlsToInspector();
                }

                #endregion

                #region Deselect selected objects

                if (Keyboard.EscapeJustPressed && !(previousFocusedControl is TextBox))
                {
                    RemoveControlsFromInspector();
                    // Remove bounding box off the screen.
                    foreach (var gameObject in selectedObjects)
                    {
                        HideGameObjectSelected(gameObject);
                    }
                    selectedObjects.Clear();
                }

                #endregion
                
            }
            */
            #endregion

            #region Gizmo Active
            /*
            if (activeGizmo != GizmoType.None && (Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed) && !(Gizmo.Active) && !(UserInterfaceManager.FocusedControl is TextBox) && !(previousFocusedControl is TextBox))
            {
                switch (activeGizmo)
                {
                    case GizmoType.Scale       : scaleGizmo.DisableGizmo(); break;
                    case GizmoType.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case GizmoType.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = GizmoType.None;
            }
            */
            #endregion

            #region Activate Gizmo
            /*
            bool isPosibleToSwich = selectedObjects.Count > 0 && ((activeGizmo == GizmoType.None) || !(Gizmo.Active));
            if (Keyboard.KeyJustPressed(Keys.W) && isPosibleToSwich)
            {
                switch (activeGizmo)
                {
                    case GizmoType.Scale       : scaleGizmo.DisableGizmo(); break;
                    case GizmoType.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case GizmoType.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = GizmoType.Translation;
                translationGizmo.EnableGizmo(selectedObjects, picker);
            }
            if (Keyboard.KeyJustPressed(Keys.E) && isPosibleToSwich)
            {
                switch (activeGizmo)
                {
                    case GizmoType.Scale       : scaleGizmo.DisableGizmo(); break;
                    case GizmoType.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case GizmoType.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = GizmoType.Rotation;
                rotationGizmo.EnableGizmo(selectedObjects, picker);
            }
            if (Keyboard.KeyJustPressed(Keys.R) && !Keyboard.KeyPressed(Keys.LeftControl) && isPosibleToSwich)
            {
                switch (activeGizmo)
                {
                    case GizmoType.Scale       : scaleGizmo.DisableGizmo(); break;
                    case GizmoType.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case GizmoType.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = GizmoType.Scale;
                scaleGizmo.EnableGizmo(selectedObjects, picker);
            }
            */
            #endregion

            #region Update Gizmo
            /*
            switch (activeGizmo)
            {
                case GizmoType.Scale:
                    scaleGizmo.Update();
                    vector3BoxScale.Invalidate();
                    break;
                case GizmoType.Rotation: 
                    rotationGizmo.Update();
                    vector3BoxRotation.Invalidate();
                    break;
                case GizmoType.Translation: 
                    translationGizmo.Update();
                    vector3BoxPosition.Invalidate();
                    break;
            }
            
            UserInterfaceManager.InputEnabled = !Gizmo.Active && !selectionRectangleBackground.LineRenderer.Enabled;
            */
            #endregion

            //previousFocusedControl = UserInterfaceManager.FocusedControl;
            
        } // Update

        #endregion

        #region Render Tasks

        /// <summary>
        /// Tasks before the engine render.
        /// I create a viewport to place the editor scene rendering under the render space control.
        /// </summary>
        public static void PreRenderTask()
        {
            UserInterfaceManager.PreRenderControls();
        } // PreRenderTask

        /// <summary>
        /// Tasks after the engine render.
        /// </summary>
        public static void PostRenderTasks()
        {
            if (!editorModeEnabled)
                return;

            EngineManager.Device.Clear(Color.Black);
            // Render viewport cameras.
            SpriteManager.Begin2D();
            foreach (EditorViewport editorViewport in editorViewports)
            {
                #region Render Viewport

                if (editorViewport.Enabled)
                {
                    // Aspect ratio
                    Rectangle screenRectangle;
                    float renderTargetAspectRatio = editorViewport.Camera.AspectRatio,
                          renderSpaceAspectRatio = editorViewport.ClientArea.Width / (float)editorViewport.ClientArea.Height;

                    if (renderTargetAspectRatio > renderSpaceAspectRatio)
                    {
                        float vsAspectRatio = renderTargetAspectRatio / renderSpaceAspectRatio;
                        int blackStripe = (int)((editorViewport.ClientArea.Height - (editorViewport.ClientArea.Height / vsAspectRatio)) / 2);
                        screenRectangle = new Rectangle(editorViewport.ClientArea.ControlLeftAbsoluteCoordinate, editorViewport.ClientArea.ControlTopAbsoluteCoordinate + blackStripe,
                                                        editorViewport.ClientArea.Width, editorViewport.ClientArea.Height - blackStripe * 2);
                    }
                    else
                    {
                        float vsAspectRatio = renderSpaceAspectRatio / renderTargetAspectRatio;
                        int blackStripe = (int)((editorViewport.ClientArea.Width - (editorViewport.ClientArea.Width / vsAspectRatio)) / 2);
                        screenRectangle = new Rectangle(editorViewport.ClientArea.ControlLeftAbsoluteCoordinate + blackStripe, editorViewport.ClientArea.ControlTopAbsoluteCoordinate,
                                                        editorViewport.ClientArea.Width - blackStripe * 2, editorViewport.ClientArea.Height);
                    }
                    SpriteManager.Draw2DTexture(editorViewport.Camera.RenderTarget, 0,
                                                screenRectangle, //new Rectangle(editorViewport.ClientLeft, editorViewport.ClientTop, editorViewport.ClientWidth, editorViewport.ClientHeight),
                                                null, Color.White, 0, Vector2.Zero);
                }

                #endregion
            }
            SpriteManager.End();

            UserInterfaceManager.RenderUserInterfaceToScreen();
        } // PostRenderTasks

        #endregion

        /// <summary>
        /// Render Icon over game object.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        private static void RenderIcon(Texture texture, Vector3 position)
        {
            /*// Component's screen position.
            Viewport editorViewport = new Viewport(editorCamera.Camera.Viewport.X, editorCamera.Camera.Viewport.Y,
                                                   editorCamera.Camera.Viewport.Width, editorCamera.Camera.Viewport.Height);
            Vector3 screenPositions = editorViewport.Project(position, editorCamera.Camera.ProjectionMatrix, editorCamera.Camera.ViewMatrix, Matrix.Identity);
            // Center the icon.
            screenPositions.X -= 16;
            screenPositions.Y -= 16;
            // Draw.
            SpriteManager.Draw2DTexture(texture, screenPositions, null, Color.White, 0, Vector2.Zero, 1);*/
        } // RenderIcon

        #region Layout

        private static void SetLayout()
        {
            if (GameLoop.CamerasToRender == null)
                GameLoop.CamerasToRender = new List<Camera>();
            // All viewports are disabled and removed from rendering.
            for (int i = 0; i < 4; i++)
            {
                editorViewports[i].Enabled = false;
                GameLoop.CamerasToRender.Remove(editorViewports[i].Camera);
            }
            // Now some viewports are activated.
            if (Layout == LayoutOptions.Wide)
            {
                if (currentWideViewport == null)
                    currentWideViewport = editorViewports[1];
                currentWideViewport.Enabled = true;
                currentWideViewport.NormalizedViewport = new RectangleF(0f, 0f, 1f, 1f);
            }
            else if (Layout == LayoutOptions.ThreeSplit)
            {
                editorViewports[0].Enabled = true;
                editorViewports[1].Enabled = true;
                editorViewports[2].Enabled = true;
                editorViewports[0].NormalizedViewport = new RectangleF(0, 0, 0.5f, 0.5f);
                editorViewports[1].NormalizedViewport = new RectangleF(0.5f, 0, 0.5f, 0.5f);
                editorViewports[2].NormalizedViewport = new RectangleF(0, 0.5f, 1f, 0.5f);
            }
            else if (Layout == LayoutOptions.FourSplit)
            {
                editorViewports[0].Enabled = true;
                editorViewports[1].Enabled = true;
                editorViewports[2].Enabled = true;
                editorViewports[3].Enabled = true;
                editorViewports[0].NormalizedViewport = new RectangleF(0, 0, 0.5f, 0.5f);
                editorViewports[1].NormalizedViewport = new RectangleF(0.5f, 0, 0.5f, 0.5f);
                editorViewports[2].NormalizedViewport = new RectangleF(0, 0.5f, 0.5f, 0.5f);
                editorViewports[3].NormalizedViewport = new RectangleF(0.5f, 0.5f, 0.5f, 0.5f);
            }
            // Set the active viewports for rendering.
            foreach (EditorViewport editorViewport in editorViewports)
            {
                if (editorViewport.Enabled)
                    GameLoop.CamerasToRender.Add(editorViewport.Camera);
            }
        } // SetLayout

        /// <summary>
        /// Toggle wide layout to the previous layout or from another layout to the wide layout configuration using the viewport in which the mouse is over.
        /// </summary>
        public static void ToggleLayout()
        {
            // Search if we are pressing F12 on an active viewport
            EditorViewport editorViewportSelected = null;
            foreach (EditorViewport editorViewport in editorViewports)
            {
                if (editorViewport.Enabled && UserInterfaceManager.IsOverThisControl(editorViewport.ClientArea, new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y)))
                    editorViewportSelected = editorViewport;
            }
            // If we are...
            if (editorViewportSelected != null)
            {
                if (Layout == LayoutOptions.Wide)
                {
                    Layout = previousLayout;
                }
                else
                {
                    previousLayout = Layout;
                    currentWideViewport = editorViewportSelected;
                    Layout = LayoutOptions.Wide;
                }
            }
        } // ToggleLayout

        #endregion

    } // EditorManager
} // XNAFinalEngine.Editor
