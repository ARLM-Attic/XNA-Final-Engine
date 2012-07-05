
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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Mouse = XNAFinalEngine.Input.Mouse;
using Size = XNAFinalEngine.Helpers.Size;
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
        private enum Gizmos
        {
            None,
            Scale,
            Rotation,
            Translation
        }; // Gizmos

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

        // The selected object.
        private static readonly List<GameObject> selectedObjects = new List<GameObject>();

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

        // Indicates if we start draging the mouse on the viewport.
        private static bool canSelect;

        // This is used to know if a text box just lost its focus because escape was pressed.
        private static Control previousFocusedControl;

        private static GameObject2D selectionRectangle, selectionRectangleBackground;

        // The picker to select an object from the screen.
        private static Picker picker;

        private static EditorViewport viewportMouseOver;

        // The active gizmo.
        private static Gizmos activeGizmo = Gizmos.None;

        // The gizmos.
        private static TranslationGizmo translationGizmo;
        private static ScaleGizmo scaleGizmo;
        private static RotationGizmo rotationGizmo;

        // Icons.
        private static Texture lightIcon, lightIconDisable, cameraIcon, cameraIconDisable;

        // Used to render point light range, camera frustum.
        private static VertexPositionColor[] pointLightLines, frustumLines;
        private const int pointLightLinesNumberOfVerticesPerCurve = 50;

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
        internal static uint GameActiveMask { get; set; }

        /// <summary>
        /// The selected objects.
        /// </summary>
        public static List<GameObject> SelectedObjects { get { return selectedObjects; } }

        /// <summary>
        /// Indicates if the user is selecting game objects.
        /// </summary>
        internal static bool SelectingObjects { get { return selectionRectangleBackground.LineRenderer.Enabled; } }

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
            
            #region Selection Rectangle

            // Selection game objects.
            selectionRectangle = new GameObject2D();
            selectionRectangle.AddComponent<LineRenderer>();
            selectionRectangle.LineRenderer.Vertices = new VertexPositionColor[4 * 2];
            selectionRectangle.LineRenderer.Enabled = false;
            selectionRectangle.Layer = Layer.GetLayerByNumber(30);
            selectionRectangleBackground = new GameObject2D();
            selectionRectangleBackground.AddComponent<LineRenderer>();
            selectionRectangleBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            selectionRectangleBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            selectionRectangleBackground.LineRenderer.Enabled = false;
            selectionRectangleBackground.Layer = Layer.GetLayerByNumber(30);

            #endregion

            #region Icons

            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = new ContentManager("Editor Content Manager", true);
            // Load Icons
            lightIcon = new Texture("Editor\\LightIcon");
            lightIconDisable = new Texture("Editor\\LightIconDisable");
            cameraIcon = new Texture("Editor\\CameraIcon");
            cameraIconDisable = new Texture("Editor\\CameraIconDisable");
            ContentManager.CurrentContentManager = userContentManager;

            #endregion

            #region Feedback
            
            pointLightLines = new VertexPositionColor[pointLightLinesNumberOfVerticesPerCurve * 2 * 3];
            pointLightLines[0] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), Color.Red);
            pointLightLines[2 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI / 2)), new Color(0, 1f, 0));
            pointLightLines[4 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2)), Color.Blue);
            for (int i = 1; i < pointLightLinesNumberOfVerticesPerCurve; i++)
            {
                float angle = i * (3.1416f * 2 / pointLightLinesNumberOfVerticesPerCurve);
                float x = (float)Math.Sin(angle) * 1;
                float y = (float)Math.Cos(angle) * 1;
                pointLightLines[i * 2 - 1] = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), Color.Red);
                pointLightLines[i * 2 - 1 + 2 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI / 2)), new Color(0, 1f, 0));
                pointLightLines[i * 2 - 1 + 4 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2)), Color.Blue);
                pointLightLines[i * 2] = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.Identity), Color.Red);
                pointLightLines[i * 2 + 2 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI / 2)), new Color(0, 1f, 0));
                pointLightLines[i * 2 + 4 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3(x, y, 0), Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2)), Color.Blue);
            }
            pointLightLines[pointLightLinesNumberOfVerticesPerCurve * 2 - 1] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.Identity), Color.Red);
            pointLightLines[pointLightLinesNumberOfVerticesPerCurve * 2 - 1 + 2 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI / 2)), new Color(0, 1f, 0));
            pointLightLines[pointLightLinesNumberOfVerticesPerCurve * 2 - 1 + 4 * pointLightLinesNumberOfVerticesPerCurve] = new VertexPositionColor(Vector3.Transform(new Vector3((float)Math.Sin(0), (float)Math.Cos(0), 0), Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2)), Color.Blue);

            

            #endregion

            picker = new Picker(Size.FullScreen);
            Gizmo.Picker = picker;
            translationGizmo = new TranslationGizmo();
            scaleGizmo = new ScaleGizmo();
            rotationGizmo = new RotationGizmo();
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
            Layer.ActiveLayers = Layer.GetLayerByNumber(31).Mask; // Update just the editor layer

            GameLoop.RenderMainCameraToScreen = false;

            foreach (GameObject selectedObject in SelectedObjects)
            {
                ShowGameObjectSelected(selectedObject);    
            }
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

            foreach (GameObject selectedObject in SelectedObjects)
            {
                HideGameObjectSelected(selectedObject);
            }
        } // DisableEditorMode

        #endregion

        #region Show and Hide Game Object Selected

        /// <summary>
        /// This activates a visual feedback that shows that the game object was selected.
        /// </summary>
        private static void ShowGameObjectSelected(GameObject gameObject)
        {
            if (gameObject is GameObject3D)
            {
                GameObject3D gameObject3D = (GameObject3D)gameObject;
                // If it is a model.
                if (gameObject3D.ModelRenderer != null)
                {
                    gameObject3D.ModelRenderer.RenderNonAxisAlignedBoundingBox = true;
                }
            }
        } // ShowGameObjectSelected

        /// <summary>
        /// This activates a visual feedback that shows that the game object was selected.
        /// </summary>
        private static void HideGameObjectSelected(GameObject gameObject)
        {
            if (gameObject is GameObject3D)
            {
                GameObject3D gameObject3D = (GameObject3D)gameObject;
                // If it is a model.
                if (gameObject3D.ModelRenderer != null)
                    gameObject3D.ModelRenderer.RenderNonAxisAlignedBoundingBox = false;
            }
        } // HideGameObjectSelected

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public static void Update()
        {

            #region Search Viewport Mouse Over

            // Search the editor viewport in which the mouse is over. It does not have to be in game mode.
            if (!SelectingObjects && !Gizmo.Active)
            {
                viewportMouseOver = null;
                foreach (EditorViewport editorViewport in editorViewports)
                {
                    if (editorViewport.Enabled && editorViewport.Mode != EditorViewport.ViewportMode.Game &&
                        UserInterfaceManager.IsOverThisControl(editorViewport.ClientArea, new Point(Mouse.Position.X, Mouse.Position.Y)))
                        viewportMouseOver = editorViewport;
                }
            }

            #endregion

            #region If no update is needed...

            if (!editorModeEnabled)
            {
                UserInterfaceManager.InputEnabled = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region Update Viewports

            foreach (EditorViewport editorViewport in editorViewports)
            {
                if (editorViewport.Enabled)
                    editorViewport.Update();
            }
            // Only one camara will draw the selection rectangle.
            if (SelectingObjects)
                viewportMouseOver.HelperCamera.AddLayer(Layer.GetLayerByNumber(30));

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

            #region If no update is needed...
            
            // When we are outside of some viewport and we are not doing somethng viewport related...
            if (viewportMouseOver == null && !Gizmo.Active && !SelectingObjects)
            {
                UserInterfaceManager.InputEnabled = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }
            
            #endregion
            
            #region If no update is needed... (besides gizmo feedback, frame object and reset camera)

            if (ScriptEditorCamera.CameraBeingManipulated != null)
            {
                UserInterfaceManager.InputEnabled = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }
            
            #endregion
            
            #region No Gizmo Active
            
            // If no gizmo is active…
            if (activeGizmo == Gizmos.None)
            {
                
                #region Selection Rectangle

                if (Mouse.LeftButtonJustPressed && !Keyboard.KeyPressed(Keys.LeftAlt) && !Keyboard.KeyPressed(Keys.RightAlt))
                    canSelect = true;
                if (Mouse.LeftButtonPressed && canSelect)
                {
                    int mouseDraggingRectangleX = Mouse.DraggingRectangle.X - viewportMouseOver.ClientLeft;
                    int mouseDraggingRectangleY = Mouse.DraggingRectangle.Y - viewportMouseOver.ClientTop;
                    
                    Color lineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                    Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);
                    selectionRectangle.LineRenderer.Enabled = true;
                    selectionRectangle.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX,                                 mouseDraggingRectangleY, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX + Mouse.DraggingRectangle.Width, mouseDraggingRectangleY, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX + Mouse.DraggingRectangle.Width, mouseDraggingRectangleY, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX + Mouse.DraggingRectangle.Width, mouseDraggingRectangleY + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX + Mouse.DraggingRectangle.Width, mouseDraggingRectangleY + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX,                                 mouseDraggingRectangleY + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[6] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX,                                 mouseDraggingRectangleY + Mouse.DraggingRectangle.Height, 0), lineColor);
                    selectionRectangle.LineRenderer.Vertices[7] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX,                                 mouseDraggingRectangleY, 0), lineColor);
                    selectionRectangleBackground.LineRenderer.Enabled = true;
                    selectionRectangleBackground.LineRenderer.Vertices[0] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX,                                 mouseDraggingRectangleY, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[2] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX,                                 mouseDraggingRectangleY + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[1] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX + Mouse.DraggingRectangle.Width, mouseDraggingRectangleY, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[4] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX + Mouse.DraggingRectangle.Width, mouseDraggingRectangleY, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[3] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX,                                 mouseDraggingRectangleY + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                    selectionRectangleBackground.LineRenderer.Vertices[5] = new VertexPositionColor(new Vector3(mouseDraggingRectangleX + Mouse.DraggingRectangle.Width, mouseDraggingRectangleY + Mouse.DraggingRectangle.Height, -0.1f), backgroundColor);
                }
                else
                {
                    selectionRectangleBackground.LineRenderer.Enabled = false;
                    selectionRectangle.LineRenderer.Enabled = false;
                    viewportMouseOver.HelperCamera.RemoveLayer(Layer.GetLayerByNumber(30));
                }

                #endregion
                
                #region Selection of objects

                if (Mouse.LeftButtonJustReleased && canSelect && viewportMouseOver != null)
                {

                    MainWindow.RemoveGameObjectControlsFromInspector();

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

                    Viewport viewport = new Viewport(viewportMouseOver.ClientLeft, viewportMouseOver.ClientTop,
                                                     viewportMouseOver.ClientWidth, viewportMouseOver.ClientHeight);
                    List<GameObject3D> newSelectedObjects = new List<GameObject3D>();
                    if (Mouse.NoDragging)
                    {
                        GameObject gameObject = picker.Pick(viewportMouseOver.Camera.ViewMatrix, viewportMouseOver.Camera.ProjectionMatrix, viewport);
                        if (gameObject != null && gameObject is GameObject3D)
                            newSelectedObjects.Add((GameObject3D)gameObject);
                    }
                    else
                    {
                        List<GameObject> pickedObjects = picker.Pick(Mouse.DraggingRectangle, viewportMouseOver.Camera.ViewMatrix, viewportMouseOver.Camera.ProjectionMatrix, viewport);
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

                    if (SelectedObjects.Count > 0)
                        MainWindow.AddGameObjectControlsToInspector(SelectedObjects[0]);
                    canSelect = false;
                }

                #endregion

                #region Deselect selected objects

                if (Keyboard.EscapeJustPressed && !(previousFocusedControl is TextBox))
                {
                    MainWindow.RemoveGameObjectControlsFromInspector();
                    // Remove bounding box off the screen.
                    foreach (var gameObject in selectedObjects)
                    {
                        HideGameObjectSelected(gameObject);
                    }
                    selectedObjects.Clear();
                    canSelect = false;
                }

                #endregion
                
            }
            
            #endregion

            #region Gizmo Active

            if (activeGizmo != Gizmos.None && (Keyboard.EscapeJustPressed || Keyboard.SpaceJustPressed) && !(Gizmo.Active) &&
                !(UserInterfaceManager.FocusedControl is TextBox) && !(previousFocusedControl is TextBox))
            {
                switch (activeGizmo)
                {
                    case Gizmos.Scale       : scaleGizmo.DisableGizmo(); break;
                    case Gizmos.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case Gizmos.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = Gizmos.None;
            }
            
            #endregion

            #region Activate Gizmo

            bool isPosibleToSwich = selectedObjects.Count > 0 && ((activeGizmo == Gizmos.None) || !(Gizmo.Active));
            if (Keyboard.KeyJustPressed(Keys.W) && isPosibleToSwich)
            {
                switch (activeGizmo)
                {
                    case Gizmos.Scale       : scaleGizmo.DisableGizmo(); break;
                    case Gizmos.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case Gizmos.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = Gizmos.Translation;
                translationGizmo.EnableGizmo(selectedObjects.OfType<GameObject3D>().ToList());
            }
            if (Keyboard.KeyJustPressed(Keys.E) && isPosibleToSwich)
            {
                switch (activeGizmo)
                {
                    case Gizmos.Scale       : scaleGizmo.DisableGizmo(); break;
                    case Gizmos.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case Gizmos.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = Gizmos.Rotation;
                rotationGizmo.EnableGizmo(selectedObjects.OfType<GameObject3D>().ToList());
            }
            if (Keyboard.KeyJustPressed(Keys.R) && !Keyboard.KeyPressed(Keys.LeftControl) && isPosibleToSwich)
            {
                switch (activeGizmo)
                {
                    case Gizmos.Scale       : scaleGizmo.DisableGizmo(); break;
                    case Gizmos.Rotation    : rotationGizmo.DisableGizmo(); break;
                    case Gizmos.Translation : translationGizmo.DisableGizmo(); break;
                }
                activeGizmo = Gizmos.Scale;
                scaleGizmo.EnableGizmo(selectedObjects.OfType<GameObject3D>().ToList());
            }
            
            #endregion

            #region Update Gizmo
            
            if (viewportMouseOver != null)
            switch (activeGizmo)
            {
                case Gizmos.Scale:
                    scaleGizmo.Update((GameObject3D)viewportMouseOver.Camera.Owner, viewportMouseOver.ClientArea);
                    UserInterfaceManager.Invalidate();
                    break;
                case Gizmos.Rotation:
                    rotationGizmo.Update((GameObject3D)viewportMouseOver.Camera.Owner, viewportMouseOver.ClientArea);
                    UserInterfaceManager.Invalidate();
                    break;
                case Gizmos.Translation:
                    translationGizmo.Update((GameObject3D)viewportMouseOver.Camera.Owner, viewportMouseOver.ClientArea);
                    UserInterfaceManager.Invalidate();
                    break;
            }
            
            UserInterfaceManager.InputEnabled = !Gizmo.Active && !selectionRectangleBackground.LineRenderer.Enabled;
            
            #endregion

            previousFocusedControl = UserInterfaceManager.FocusedControl;
            
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

            #region Render Viewports
            
            foreach (EditorViewport editorViewport in editorViewports)
            {
                if (editorViewport.Enabled)
                {
                    SpriteManager.Begin2D();
                    // Aspect ratio
                    int horizontalOffset = (editorViewport.ClientArea.Width - editorViewport.Camera.RenderTargetSize.Width) / 2;
                    int verticalOffset = (editorViewport.ClientArea.Height - editorViewport.Camera.RenderTargetSize.Height) / 2;
                    SpriteManager.Draw2DTexture(editorViewport.Camera.RenderTarget, 1,
                                                new Rectangle(editorViewport.ClientLeft + horizontalOffset, 
                                                              editorViewport.ClientTop + verticalOffset,
                                                              editorViewport.Camera.RenderTarget.Width, 
                                                              editorViewport.Camera.RenderTarget.Height),
                                                null, Color.White, 0, Vector2.Zero);
                    SpriteManager.End();
                    
                    // Draw components icons.
                    if (editorViewport.Mode != EditorViewport.ViewportMode.Game)
                    {

                        #region Render Icons

                        SpriteManager.Begin2D();
                        EngineManager.Device.Viewport = new Viewport(editorViewport.Camera.Viewport.X + editorViewport.ClientArea.ControlLeftAbsoluteCoordinate,
                                                                     editorViewport.Camera.Viewport.Y + editorViewport.ClientArea.ControlTopAbsoluteCoordinate,
                                                                     editorViewport.Camera.Viewport.Width, editorViewport.Camera.Viewport.Height);
                        foreach (GameObject gameObject in GameObject.GameObjects)
                        {
                            if (gameObject.Layer != Layer.GetLayerByNumber(30) && gameObject.Layer != Layer.GetLayerByNumber(31) && // Exclude editor elements.
                                Layer.IsVisible(gameObject.Layer.Mask) && gameObject.Active) 
                            {
                                if (gameObject is GameObject3D)
                                {
                                    GameObject3D gameObject3D = (GameObject3D) gameObject;
                                    // Camera
                                    if (gameObject3D.Camera != null)
                                        RenderIcon(gameObject3D.Camera.Enabled ? cameraIcon : cameraIconDisable,
                                                   gameObject3D.Transform.Position, editorViewport.Camera,
                                                   editorViewport.ClientArea);
                                    // Light
                                    else if (gameObject3D.Light != null)
                                    {
                                        RenderIcon(gameObject3D.Light.Enabled ? lightIcon : lightIconDisable,
                                                   gameObject3D.Transform.Position, editorViewport.Camera,
                                                   editorViewport.ClientArea);
                                    }
                                }
                            }
                        }
                        SpriteManager.End();
                        EngineManager.Device.Viewport = new Viewport(0, 0, Screen.Width, Screen.Height);

                        #endregion

                        #region Render Feedback Lines
                        
                        LineManager.Begin3D(PrimitiveType.LineList, editorViewport.Camera.ViewMatrix, editorViewport.Camera.ProjectionMatrix);
                        EngineManager.Device.Viewport = new Viewport(editorViewport.Camera.Viewport.X + editorViewport.ClientArea.ControlLeftAbsoluteCoordinate,
                                                                     editorViewport.Camera.Viewport.Y + editorViewport.ClientArea.ControlTopAbsoluteCoordinate,
                                                                     editorViewport.Camera.Viewport.Width, editorViewport.Camera.Viewport.Height);
                        foreach (GameObject gameObject in SelectedObjects)
                        {
                            if (gameObject.Layer != Layer.GetLayerByNumber(30) && gameObject.Layer != Layer.GetLayerByNumber(31) && // Exclude editor elements.
                                Layer.IsVisible(gameObject.Layer.Mask) && gameObject.Active) 
                            {
                                if (gameObject is GameObject3D)
                                {
                                    GameObject3D gameObject3D = (GameObject3D)gameObject;
                                    if (gameObject3D.Light != null)
                                    {
                                        if (gameObject3D.PointLight != null && gameObject3D.PointLight.Enabled)
                                        {
                                            Matrix worldMatrix = Matrix.CreateScale(gameObject3D.PointLight.Range) * Matrix.CreateTranslation(gameObject3D.Transform.Position);
                                            for (int j = 0; j < pointLightLines.Length; j++)
                                                LineManager.AddVertex(Vector3.Transform(pointLightLines[j].Position, worldMatrix), gameObject3D.PointLight.Color);
                                        }
                                    }
                                    if (gameObject3D.Camera != null && gameObject3D.Camera.Enabled)
                                    {
                                        Vector3[] farPlaneFrustum = new Vector3[4];
                                        gameObject3D.Camera.BoundingFrustumWorldSpace(farPlaneFrustum);
                                        LineManager.AddVertex(farPlaneFrustum[0], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[1], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[1], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[3], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[3], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[2], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[2], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[0], Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[0], Color.Gray);
                                        LineManager.AddVertex(gameObject3D.Camera.Position, Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[1], Color.Gray);
                                        LineManager.AddVertex(gameObject3D.Camera.Position, Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[2], Color.Gray);
                                        LineManager.AddVertex(gameObject3D.Camera.Position, Color.Gray);
                                        LineManager.AddVertex(farPlaneFrustum[3], Color.Gray);
                                        LineManager.AddVertex(gameObject3D.Camera.Position, Color.Gray);
                                    }
                                }
                            }
                        }
                        LineManager.End();
                        EngineManager.Device.Viewport = new Viewport(0, 0, Screen.Width, Screen.Height);

                        #endregion

                    }
                }
            }

            #endregion

            #region Gizmos

            foreach (EditorViewport editorViewport in editorViewports)
            {
                if (editorViewport.Enabled && editorViewport.Mode != EditorViewport.ViewportMode.Game)
                {
                    // Render Gizmos
                    switch (activeGizmo)
                    {
                        case Gizmos.Scale       : scaleGizmo.RenderGizmo((GameObject3D)editorViewport.Camera.Owner, editorViewport.ClientArea, editorViewport.Mode); break;
                        case Gizmos.Rotation    : rotationGizmo.RenderGizmo((GameObject3D)editorViewport.Camera.Owner, editorViewport.ClientArea); break;
                        case Gizmos.Translation : translationGizmo.RenderGizmo((GameObject3D)editorViewport.Camera.Owner, editorViewport.ClientArea, editorViewport.Mode); break;
                    }
                }
            }

            #endregion

            UserInterfaceManager.RenderUserInterfaceToScreen();
        } // PostRenderTasks

        #endregion

        #region Render Icons

        /// <summary>
        /// Render an icon over game object to indicate visually if it is a light, a camera, etc.
        /// </summary>
        private static void RenderIcon(Texture texture, Vector3 position, Camera camera, Control clientArea)
        {
            // Component's screen position.
            Viewport editorViewport = new Viewport(camera.Viewport.X,
                                                   camera.Viewport.Y,
                                                   camera.Viewport.Width, camera.Viewport.Height);
            Vector3 screenPositions = editorViewport.Project(position, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            // Center the icon.
            screenPositions.X -= 16;
            screenPositions.Y -= 16;
            // Draw.
            SpriteManager.Draw2DTexture(texture, screenPositions, null, Color.White, 0, Vector2.Zero, 1);
        } // RenderIcon

        #endregion

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
            EditorViewport editorViewportMouseOver = null;
            foreach (EditorViewport editorViewport in editorViewports)
            {
                if (editorViewport.Enabled && UserInterfaceManager.IsOverThisControl(editorViewport.ClientArea, new Point(Mouse.Position.X, Mouse.Position.Y)))
                    editorViewportMouseOver = editorViewport;
            }
            // If we are...
            if (editorViewportMouseOver != null)
            {
                if (Layout == LayoutOptions.Wide)
                {
                    Layout = previousLayout;
                }
                else
                {
                    previousLayout = Layout;
                    currentWideViewport = editorViewportMouseOver;
                    Layout = LayoutOptions.Wide;
                }
            }
        } // ToggleLayout

        #endregion

    } // EditorManager
} // XNAFinalEngine.Editor
