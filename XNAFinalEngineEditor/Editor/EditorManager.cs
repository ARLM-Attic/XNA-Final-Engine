
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
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Mouse = XNAFinalEngine.Input.Mouse;
using Size = XNAFinalEngine.Helpers.Size;
using Microsoft.Xna.Framework.Graphics;
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
            public override void PreRenderUpdate() { EditorManager.PreRenderTask(); }
            public override void PostRenderUpdate() { EditorManager.PostRenderTasks(); }

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

        private static GameObject2D selectionRectangle, selectionRectangleBackground;

        // The gizmos.
        private static TranslationGizmo translationGizmo;
        private static ScaleGizmo scaleGizmo;
        private static RotationGizmo rotationGizmo;

        // The user interface control for the viewport.
        private static Container renderSpace;
        // The user interface control for the right panel.
        private static TabControl rightPanelTabControl;

        // Indicates if we start draging the mouse on the viewport.
        private static bool canSelect;

        // When gizmos are being manipulated we want to update the control information.
        private static Vector3Box vector3BoxPosition, vector3BoxRotation, vector3BoxScale;

        // Icons.
        private static Texture lightIcon, cameraIcon;

        // This is used to know if a text box just lost its focus because escape was pressed.
        private static Control previousFocusedControl;
        
        // To avoid precisions problems with quaternions.
        private static bool updateRotation;

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
            // If it already initialize don't worry.
            UserInterfaceManager.Initialize(false);
            UserInterfaceManager.Visible = false;
            // Call the manager's update and render methods in the correct order without explicit calls. 
            editorManagerGameObject = new GameObject2D();
            editorManagerGameObject.AddComponent<ScripEditorManager>();
            picker = new Picker(Size.FullScreen);

            #region Cameras

            // Editor Camera
            editorCamera = new GameObject3D();
            editorCamera.AddComponent<Camera>();
            editorCamera.Camera.AmbientLight = new AmbientLight();
            editorCamera.Camera.PostProcess = new PostProcess();
            editorCamera.Camera.PostProcess.Bloom.Enabled = false;
            editorCamera.Camera.Enabled = false;
            editorCamera.Camera.RenderHeadUpDisplay = false;
            editorCamera.Layer = Layer.GetLayerByNumber(31);
            
            editorCameraScript = (ScriptEditorCamera)editorCamera.AddComponent<ScriptEditorCamera>();
            editorCameraScript.Mode = ScriptEditorCamera.ModeType.Maya;
            ResetEditorCamera(); // Reset camera to default position and orientation.
            // Camera to render the gizmos. 
            // This is done because the gizmos need to be in front of everything and I can't clear the ZBuffer wherever I want.
            gizmoCamera = new GameObject3D();
            gizmoCamera.AddComponent<Camera>();
            gizmoCamera.Camera.Enabled = false;
            gizmoCamera.Camera.CullingMask = Layer.GetLayerByNumber(31).Mask; // The editor layer.
            gizmoCamera.Camera.ClearColor = Color.Transparent;
            gizmoCamera.Layer = Layer.GetLayerByNumber(31);
            
            editorCamera.Camera.MasterCamera = gizmoCamera.Camera;
            gizmoCamera.Camera.RenderingOrder = int.MaxValue;

            #endregion

            translationGizmo = new TranslationGizmo(gizmoCamera);
            scaleGizmo = new ScaleGizmo(gizmoCamera);
            rotationGizmo = new RotationGizmo(gizmoCamera);

            #region Selection Rectangle

            // Selection game objects.
            selectionRectangle = new GameObject2D();
            selectionRectangle.AddComponent<LineRenderer>();
            selectionRectangle.LineRenderer.Vertices = new VertexPositionColor[4 * 2];
            selectionRectangle.LineRenderer.Enabled = false;
            selectionRectangle.Layer = Layer.GetLayerByNumber(31);
            selectionRectangleBackground = new GameObject2D();
            selectionRectangleBackground.AddComponent<LineRenderer>();
            selectionRectangleBackground.LineRenderer.PrimitiveType = PrimitiveType.TriangleList;
            selectionRectangleBackground.LineRenderer.Vertices = new VertexPositionColor[6];
            selectionRectangleBackground.LineRenderer.Enabled = false;
            selectionRectangleBackground.Layer = Layer.GetLayerByNumber(31);

            #endregion

            #region Icons

            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = new ContentManager("Editor Content Manager", true);
            // Load Icons
            lightIcon = new Texture("Editor\\LightIcon");
            cameraIcon = new Texture("Editor\\CameraIcon");
            ContentManager.CurrentContentManager = userContentManager;

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
            // File
            MenuItem menuItemFile = new MenuItem("File", true);
            canvas.MainMenu.Items.Add(menuItemFile);
            menuItemFile.ChildrenItems.AddRange(new[]
            {
                new MenuItem("New Scene"),
                new MenuItem("Open Scene"), new MenuItem("Exit", true),
            });
            // Edit
            MenuItem menuItemEdit = new MenuItem("Edit", true);
            canvas.MainMenu.Items.Add(menuItemEdit);
            menuItemEdit.ChildrenItems.AddRange(new[]
            {
                new MenuItem("Undo") { RightSideText = "Ctrl+Z"},
                new MenuItem("Redo") { RightSideText = "Ctrl+Y" },
                new MenuItem("Frame Selected") { RightSideText = "F", Separated = true },
                new MenuItem("Frame All")      { RightSideText = "A" },
            });
            menuItemEdit.ChildrenItems[0].Click += delegate { ActionManager.Undo(); };
            menuItemEdit.ChildrenItems[1].Click += delegate { ActionManager.Redo(); };
            // Game Object
            MenuItem menuItemGameObject = new MenuItem("Game Object", true);
            canvas.MainMenu.Items.Add(menuItemGameObject);
            menuItemGameObject.ChildrenItems.AddRange(new[]
            {
                new MenuItem("Create Empty") { RightSideText = "Ctrl+Shift+N"},
            });
            // Component
            MenuItem menuItemComponent = new MenuItem("Component", true);
            canvas.MainMenu.Items.Add(menuItemComponent);
            // Window
            MenuItem menuItemWindow = new MenuItem("Window", true);
            canvas.MainMenu.Items.Add(menuItemWindow);
            // Help
            MenuItem menuItemHelp = new MenuItem("Help", true);
            canvas.MainMenu.Items.Add(menuItemHelp);
            
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
                Passive = false,
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
            UserInterfaceManager.InputEnable = true;
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
            Camera.OnlyRendereableCamera = null;
            editorCamera.Camera.Enabled = false;
            gizmoCamera.Camera.Enabled = false;
            // Remove bounding box off the screen.
            foreach (var gameObject in selectedObjects)
            {
                HideGameObjectSelected(gameObject);
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
                    gameObject3D.ModelRenderer.RenderNonAxisAlignedBoundingBox = true;
                // If it is a light.
                if (gameObject3D.Light != null)
                {
                    //GameObject2D lightIcon
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
                // ... TODO!!!
            }
        } // HideGameObjectSelected

        #endregion

        #region Add and Remove Controls From Inspector

        /// <summary>
        /// Add User Interface controls to the inspector panel.
        /// </summary>
        private static void AddControlsToInspector()
        {
            if (selectedObjects.Count == 0)
                return;
            GameObject3D selectedObject = selectedObjects[0];

            #region Transform Component

            var panel = CommonControls.PanelCollapsible("Transform", rightPanelTabControl, 0);
            // Position
            vector3BoxPosition = CommonControls.Vector3Box("Position", panel, selectedObject.Transform.LocalPosition, selectedObject.Transform, "LocalPosition");
            // Orientation.
            #region Orientation

            // This has too many special cases, I need to do it manually.

            Vector3 initialValue = new Vector3(selectedObject.Transform.LocalRotation.GetYawPitchRoll().Y * 180 / (float)Math.PI,
                                               selectedObject.Transform.LocalRotation.GetYawPitchRoll().X * 180 / (float)Math.PI,
                                               selectedObject.Transform.LocalRotation.GetYawPitchRoll().Z * 180 / (float)Math.PI);
            initialValue.X = (float)Math.Round(initialValue.X, 4);
            initialValue.Y = (float)Math.Round(initialValue.Y, 4);
            initialValue.Z = (float)Math.Round(initialValue.Z, 4);
            vector3BoxRotation = CommonControls.Vector3Box("Rotation", panel, initialValue);
            vector3BoxRotation.ValueChanged += delegate
            {
                if (updateRotation)
                {
                    Vector3 propertyValue = new Vector3(selectedObject.Transform.LocalRotation.GetYawPitchRoll().Y * 180 / (float)Math.PI,
                                                        selectedObject.Transform.LocalRotation.GetYawPitchRoll().X * 180 / (float)Math.PI,
                                                        selectedObject.Transform.LocalRotation.GetYawPitchRoll().Z * 180 / (float)Math.PI);
                    // Round to avoid precision problems.
                    propertyValue.X = (float)Math.Round(propertyValue.X, 4);
                    propertyValue.Y = (float)Math.Round(propertyValue.Y, 4);
                    propertyValue.Z = (float)Math.Round(propertyValue.Z, 4);
                    // I compare the value of the transform property rounded to avoid a precision mismatch.
                    if (propertyValue != vector3BoxRotation.Value)
                    {
                        using (Transaction.Create())
                        {
                            Quaternion newValue = Quaternion.CreateFromYawPitchRoll(vector3BoxRotation.Value.Y * (float)Math.PI / 180,
                                                                                    vector3BoxRotation.Value.X * (float)Math.PI / 180,
                                                                                    vector3BoxRotation.Value.Z * (float)Math.PI / 180);
                            ActionManager.SetProperty(selectedObject.Transform, "LocalRotation", newValue);
                            ActionManager.CallMethod(// Redo
                                                     UserInterfaceManager.Invalidate,
                                                     // Undo
                                                     UserInterfaceManager.Invalidate);
                        }
                    }
                    updateRotation = false;
                }
            };
            vector3BoxRotation.Draw += delegate
            {
                Vector3 localRotationDegrees = new Vector3(selectedObject.Transform.LocalRotation.GetYawPitchRoll().Y * 180 / (float)Math.PI,
                                                           selectedObject.Transform.LocalRotation.GetYawPitchRoll().X * 180 / (float)Math.PI,
                                                           selectedObject.Transform.LocalRotation.GetYawPitchRoll().Z * 180 / (float)Math.PI);
                // Round to avoid precision problems.
                localRotationDegrees.X = (float)Math.Round(localRotationDegrees.X, 4);
                localRotationDegrees.Y = (float)Math.Round(localRotationDegrees.Y, 4);
                localRotationDegrees.Z = (float)Math.Round(localRotationDegrees.Z, 4);
                vector3BoxRotation.Value = localRotationDegrees;
                updateRotation = true;
            };

            #endregion
            // Scale
            vector3BoxScale = CommonControls.Vector3Box("Scale", panel, selectedObject.Transform.LocalScale, selectedObject.Transform, "LocalScale");

            #endregion

            #region Light Component

            if (selectedObject.Light != null)
            {

                #region Point Light

                if (selectedObject.PointLight != null)
                {
                    panel = CommonControls.PanelCollapsible("Point Light", rightPanelTabControl, 0);
                    CheckBox checkBoxLightEnabled = CommonControls.CheckBox("Enabled", panel, selectedObject.PointLight.Enabled);
                    checkBoxLightEnabled.Top = 10;
                    checkBoxLightEnabled.Draw += delegate { checkBoxLightEnabled.Checked = selectedObject.PointLight.Enabled; };

                    #region Intensity

                    var sliderLightIntensity = CommonControls.SliderNumeric("Intensity", panel, selectedObject.PointLight.Intensity, false, true, 0, 100);
                    sliderLightIntensity.ValueChanged += delegate { selectedObject.PointLight.Intensity = sliderLightIntensity.Value; };
                    sliderLightIntensity.Draw += delegate { sliderLightIntensity.Value = selectedObject.PointLight.Intensity; };

                    #endregion

                    #region Diffuse Color

                    var sliderDiffuseColor = CommonControls.SliderColor("Diffuse Color", panel, selectedObject.PointLight.DiffuseColor);
                    sliderDiffuseColor.ColorChanged += delegate { selectedObject.PointLight.DiffuseColor = sliderDiffuseColor.Color; };
                    sliderDiffuseColor.Draw += delegate { sliderDiffuseColor.Color = selectedObject.PointLight.DiffuseColor; };

                    #endregion

                    #region Range

                    var sliderLightRange = CommonControls.SliderNumeric("Range", panel, selectedObject.PointLight.Range, false, true, 0, 500);
                    sliderLightRange.ValueChanged += delegate { selectedObject.PointLight.Range = sliderLightRange.Value; };
                    sliderLightRange.Draw += delegate { sliderLightRange.Value = selectedObject.PointLight.Range; };

                    #endregion

                    checkBoxLightEnabled.CheckedChanged += delegate
                    {
                        selectedObject.PointLight.Enabled = checkBoxLightEnabled.Checked;
                        sliderLightIntensity.Enabled = selectedObject.PointLight.Enabled;
                        sliderDiffuseColor.Enabled = selectedObject.PointLight.Enabled;
                        sliderLightRange.Enabled = selectedObject.PointLight.Enabled;
                    };
                }

                #endregion
                
                #region Spot Light

                if (selectedObject.SpotLight != null)
                {
                    panel = CommonControls.PanelCollapsible("Spot Light", rightPanelTabControl, 0);
                    // Visible
                    CheckBox checkBoxLightEnabled = CommonControls.CheckBox("Enabled", panel, selectedObject.SpotLight.Enabled, selectedObject.SpotLight, "Enabled");
                    checkBoxLightEnabled.Top = 10;
                    // Intensity
                    var sliderLightIntensity = CommonControls.SliderNumeric("Intensity", panel, selectedObject.SpotLight.Intensity,
                                                                            false, true, 0, 100, selectedObject.SpotLight, "Intensity");
                    // Diffuse Color
                    var sliderDiffuseColor = CommonControls.SliderColor("Diffuse Color", panel, selectedObject.SpotLight.DiffuseColor, selectedObject.SpotLight, "DiffuseColor");
                    // Range
                    var sliderLightRange = CommonControls.SliderNumeric("Range", panel, selectedObject.SpotLight.Range, false, true, 0, 500, selectedObject.SpotLight, "Range");
                    // Inner Cone Angle
                    var sliderLightInnerConeAngle = CommonControls.SliderNumeric("Inner Cone Angle", panel, selectedObject.SpotLight.InnerConeAngle,
                                                                                 false, false, 0, 175, selectedObject.SpotLight, "InnerConeAngle");
                    // Outer Cone Angle
                    var sliderLightOuterConeAngle = CommonControls.SliderNumeric("Outer Cone Angle", panel, selectedObject.SpotLight.OuterConeAngle,
                                                                                 false, false, 0, 175, selectedObject.SpotLight, "OuterConeAngle");
                    // Mask Texture
                    var assetSelectorMaskTexture = CommonControls.AssetSelector<Texture>("Mask Texture", panel, selectedObject.SpotLight, "LightMaskTexture");
                    // Visible
                    checkBoxLightEnabled.CheckedChanged += delegate
                    {
                        sliderLightIntensity.Enabled = selectedObject.SpotLight.Enabled;
                        sliderDiffuseColor.Enabled = selectedObject.SpotLight.Enabled;
                        sliderLightRange.Enabled = selectedObject.SpotLight.Enabled;
                        sliderLightInnerConeAngle.Enabled = selectedObject.SpotLight.Enabled;
                        sliderLightOuterConeAngle.Enabled = selectedObject.SpotLight.Enabled;
                        assetSelectorMaskTexture.Enabled = selectedObject.SpotLight.Enabled;
                    };
                }

                #endregion

                #region Directional Light

                if (selectedObject.DirectionalLight != null)
                {
                    panel = CommonControls.PanelCollapsible("Directional Light", rightPanelTabControl, 0);
                    CheckBox checkBoxLightEnabled = CommonControls.CheckBox("Enabled", panel, selectedObject.DirectionalLight.Enabled);
                    checkBoxLightEnabled.Top = 10;
                    checkBoxLightEnabled.Draw += delegate { checkBoxLightEnabled.Checked = selectedObject.DirectionalLight.Enabled; };

                    #region Intensity

                    var sliderLightIntensity = CommonControls.SliderNumeric("Intensity", panel, selectedObject.DirectionalLight.Intensity, false, true, 0, 100);
                    sliderLightIntensity.ValueChanged += delegate { selectedObject.DirectionalLight.Intensity = sliderLightIntensity.Value; };
                    sliderLightIntensity.Draw += delegate { sliderLightIntensity.Value = selectedObject.DirectionalLight.Intensity; };

                    #endregion

                    #region Diffuse Color

                    var sliderDiffuseColor = CommonControls.SliderColor("Diffuse Color", panel, selectedObject.DirectionalLight.DiffuseColor);
                    sliderDiffuseColor.ColorChanged += delegate { selectedObject.DirectionalLight.DiffuseColor = sliderDiffuseColor.Color; };
                    sliderDiffuseColor.Draw += delegate { sliderDiffuseColor.Color = selectedObject.DirectionalLight.DiffuseColor; };

                    #endregion

                    checkBoxLightEnabled.CheckedChanged += delegate
                    {
                        selectedObject.DirectionalLight.Enabled = checkBoxLightEnabled.Checked;
                        sliderLightIntensity.Enabled = selectedObject.DirectionalLight.Enabled;
                        sliderDiffuseColor.Enabled = selectedObject.DirectionalLight.Enabled;
                    };
                }

                #endregion
            }

            #endregion

            #region Model Renderer

            if (selectedObject.ModelRenderer != null)
            {

                panel = CommonControls.PanelCollapsible("Model Renderer", rightPanelTabControl, 0);
                CheckBox checkBoxVisible = CommonControls.CheckBox("Enabled", panel, selectedObject.ModelRenderer.Enabled, selectedObject.ModelRenderer, "Enabled");
                checkBoxVisible.Top = 10;

                //var assetCreationMaterial = CommonControls.AssetSelectorMaterial("Material", panel, selectedObject.ModelRenderer, "Material");

                checkBoxVisible.CheckedChanged += delegate
                {
                };
            }

            #endregion

            #region Camera

            if (selectedObject.Camera != null)
            {
                panel = CommonControls.PanelCollapsible("Camera", rightPanelTabControl, 0);
                CameraControls.AddControls(selectedObject.Camera, panel);
            }

            #endregion

        } // AddControlsToInspector

        /// <summary>
        /// Remove User Interface controls from inspector panel.
        /// </summary>
        private static void RemoveControlsFromInspector()
        {
            rightPanelTabControl.TabPages[0].RemoveControlsFromClientArea();
        } // RemoveControlsFromInspector

        #endregion

        #region Frame Objects

        /// <summary>
        /// Adjust the look at position and distance to frame the selected objects.
        /// The orientation is not afected.
        /// </summary>
        public static void FrameObjects(List<GameObject3D> objects)
        {
            BoundingSphere? frameBoundingSphere = null; // Garbage is not an issue in the editor.
            foreach (var gameObject in objects)
            {
                if (gameObject.ModelRenderer != null)
                {
                    if (frameBoundingSphere == null)
                        frameBoundingSphere = gameObject.ModelRenderer.BoundingSphere;
                    else
                        frameBoundingSphere = BoundingSphere.CreateMerged(frameBoundingSphere.Value, gameObject.ModelRenderer.BoundingSphere);
                }
                // The rest of objects
                else
                {
                    if (frameBoundingSphere == null)
                        frameBoundingSphere = new BoundingSphere(gameObject.Transform.Position, 0);
                    else
                        frameBoundingSphere = BoundingSphere.CreateMerged(frameBoundingSphere.Value, new BoundingSphere(gameObject.Transform.Position, 0));
                }
            }
            if (frameBoundingSphere != null)
            {
                editorCameraScript.LookAtPosition = frameBoundingSphere.Value.Center;
                editorCameraScript.Distance = frameBoundingSphere.Value.Radius * 3 + editorCamera.Camera.NearPlane + 0.1f;
            }
        } // FrameObjects



        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public static void Update()
        {

            #region Undo System

            if (Keyboard.KeyPressed(Keys.LeftControl) && Keyboard.KeyJustPressed(Keys.Z))
            {
                ActionManager.Undo();
            }
            if (Keyboard.KeyPressed(Keys.LeftControl) && Keyboard.KeyJustPressed(Keys.Y))
            {
                ActionManager.Redo();
            }

            #endregion

            #region User Interface Update

            UserInterfaceManager.Update();

            #endregion

            #region If no update is needed...

            if (!editorModeEnabled)
            {
                UserInterfaceManager.InputEnable = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion
            
            #region Prepare viewport mode

            if (ViewportMode == ViewportModeType.Scene)
            {
                Camera.OnlyRendereableCamera = gizmoCamera.Camera;
                editorCamera.Camera.Enabled = true;
                gizmoCamera.Camera.Enabled = true;
                // Restore bounding box to the current selected objects.
                foreach (var gameObject in selectedObjects)
                {
                    ShowGameObjectSelected(gameObject);
                }
            }
            else
            {
                Camera.OnlyRendereableCamera = null;
                editorCamera.Camera.Enabled = false;
                gizmoCamera.Camera.Enabled = false;
                // Remove the bounding box in game mode.
                foreach (var gameObject in selectedObjects)
                {
                    HideGameObjectSelected(gameObject);
                }
                UserInterfaceManager.InputEnable = true;
                return;
            }

            #endregion

            #region Update Gizmo Rendering Information

            gizmoCamera.Transform.WorldMatrix = editorCamera.Transform.WorldMatrix;
            gizmoCamera.Camera.ProjectionMatrix = editorCamera.Camera.ProjectionMatrix;
            switch (activeGizmo)
            {
                case GizmoType.Scale       : scaleGizmo.UpdateRenderingInformation(); break;
                case GizmoType.Rotation    : rotationGizmo.UpdateRenderingInformation(); break;
                case GizmoType.Translation : translationGizmo.UpdateRenderingInformation(); break;
            }

            #endregion

            #region If no update is needed...

            // Keyboard shortcuts, camera movement and similar should be ignored when the text box is active.
            if (!UserInterfaceManager.IsOverThisControl(renderSpace, new Point(Mouse.Position.X, Mouse.Position.Y)) && !Gizmo.Active && !selectionRectangleBackground.LineRenderer.Enabled)
            {
                UserInterfaceManager.InputEnable = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region Frame Object

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

            #endregion

            #region Reset Camera

            // Reset camera to default position and orientation.
            if (Keyboard.KeyJustPressed(Keys.R) && Keyboard.KeyPressed(Keys.LeftControl) && !(UserInterfaceManager.FocusedControl is TextBox))
            {
                ResetEditorCamera();
            }

            #endregion

            #region If no update is needed... (besides gizmo feedback, frame object and reset camera)

            if (editorCameraScript.Manipulating)
            {
                UserInterfaceManager.InputEnable = true;
                canSelect = false;
                previousFocusedControl = UserInterfaceManager.FocusedControl;
                return;
            }

            #endregion

            #region No Gizmo Active

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

            #endregion

            #region Gizmo Active

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

            #endregion

            #region Activate Gizmo

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

            #endregion

            #region Update Gizmo

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

            UserInterfaceManager.InputEnable = !Gizmo.Active && !selectionRectangleBackground.LineRenderer.Enabled;

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
            if (ViewportMode == ViewportModeType.Scene)
            {
                Rectangle viewport = new Rectangle(renderSpace.ClientArea.ControlLeftAbsoluteCoordinate,
                                                   renderSpace.ClientArea.ControlTopAbsoluteCoordinate,
                                                   renderSpace.ClientArea.Width, renderSpace.ClientArea.Height);
                if (viewport.Width < 8)
                    viewport.Width = 8;
                if (viewport.Height < 8)
                    viewport.Height = 8;
                // The editor camera only use part of the render target.
                editorCamera.Camera.Viewport = viewport;
                gizmoCamera.Camera.Viewport = viewport;
            }
        } // PreRenderTask

        /// <summary>
        /// Tasks after the engine render.
        /// </summary>
        public static void PostRenderTasks()
        {
            if (!editorModeEnabled)
                return;
            if (ViewportMode == ViewportModeType.Game)
            {
                // Render the game scene under the render space control.
                EngineManager.Device.Clear(Color.Black);
                Camera mainCamera = Camera.MainCamera;
                if (mainCamera != null && mainCamera.RenderTarget != null)
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
            else
            {
                // Draw components icons.
                SpriteManager.Begin2D();
                foreach (GameObject gameObject in GameObject.GameObjects)
                {
                    if (gameObject is GameObject3D)
                    {
                        GameObject3D gameObject3D = (GameObject3D) gameObject;
                        // If it is a light.
                        if (gameObject3D.Camera != null && gameObject3D != editorCamera && gameObject3D != gizmoCamera)
                        {
                            RenderIcon(cameraIcon, gameObject3D.Transform.Position);
                        }
                        else if (gameObject3D.Light != null)
                        {
                            RenderIcon(lightIcon, gameObject3D.Transform.Position);
                        }
                    }
                }
                SpriteManager.End();
                // Render selected components feedback.
                foreach (GameObject3D selectedObject in selectedObjects)
                {
                    if (selectedObject.SpotLight != null)
                    {
                        // Draw a the spot cone with lines
                        // TODO!!!
                    }
                }
            }
            UserInterfaceManager.RenderUserInterfaceToScreen();
        } // PostRenderTasks

        /// <summary>
        /// Render Icon over game object.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        private static void RenderIcon(Texture texture, Vector3 position)
        {
            // Component's screen position.
            Viewport editorViewport = new Viewport(editorCamera.Camera.Viewport.X, editorCamera.Camera.Viewport.Y,
                                                   editorCamera.Camera.Viewport.Width, editorCamera.Camera.Viewport.Height);
            Vector3 screenPositions = editorViewport.Project(position, editorCamera.Camera.ProjectionMatrix, editorCamera.Camera.ViewMatrix, Matrix.Identity);
            // Center the icon.
            screenPositions.X -= 16;
            screenPositions.Y -= 16;
            // Draw.
            SpriteManager.Draw2DTexture(texture, screenPositions, null, Color.White, 0, Vector2.Zero, 1);
        } // RenderIcon

        #endregion

        #region Could Be Manipulated

        /// <summary>
        /// Indicates if the camera could perform a camera movement.
        /// </summary>
        internal static bool CouldBeManipulated(ScriptEditorCamera scriptEditorCamera)
        {
            return UserInterfaceManager.IsOverThisControl(renderSpace, new Point(Mouse.Position.X, Mouse.Position.Y)) && !Gizmo.Active && !selectionRectangleBackground.LineRenderer.Enabled;
        } // CouldBeManipulated

        #endregion

    } // EditorManager
} // XNAFinalEngine.Editor
