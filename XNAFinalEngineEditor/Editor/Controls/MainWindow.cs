
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// This has the controls of the editor main window.
    /// </summary>
    internal static class MainWindow
    {

        #region Variables

        // The user interface control for the right panel.
        private static TabPage inspector;

        #endregion

        #region Properties

        /// <summary>
        /// The control that contains all viewports.
        /// </summary>
        public static Container ViewportsSpace { get; set; }

        /// <summary>
        /// Viewport Area.
        /// </summary>
        public static Rectangle ViewportArea 
        {
            get
            {
                return new Rectangle(ViewportsSpace.Left, ViewportsSpace.Top, ViewportsSpace.Width, ViewportsSpace.Height);
            }
        } // ViewportArea

        #endregion

        #region Add Main Window Controls

        /// <summary>
        /// Add the editor's main window controls.
        /// </summary>
        public static void AddMainControls()
        {

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
            menuItemFile.Items.AddRange(new[]
            {
                new MenuItem("New Scene"),
                new MenuItem("Open Scene"), new MenuItem("Exit", true),
            });
            // Edit
            MenuItem menuItemEdit = new MenuItem("Edit", true);
            canvas.MainMenu.Items.Add(menuItemEdit);
            menuItemEdit.Items.AddRange(new[]
            {
                new MenuItem("Undo") { RightSideText = "Ctrl+Z"},
                new MenuItem("Redo") { RightSideText = "Ctrl+Y" },
                new MenuItem("Frame Selected") { RightSideText = "F", SeparationLine = true },
                new MenuItem("Frame All")      { RightSideText = "A" },
            });
            menuItemEdit.Items[0].Click += delegate { ActionManager.Undo(); };
            menuItemEdit.Items[1].Click += delegate { ActionManager.Redo(); };
            // Game Object
            MenuItem menuItemGameObject = new MenuItem("Game Object", true);
            canvas.MainMenu.Items.Add(menuItemGameObject);
            menuItemGameObject.Items.AddRange(new[]
            {
                new MenuItem("Create Empty") { RightSideText = "Ctrl+Shift+N"},
            });
            // Component
            MenuItem menuItemComponent = new MenuItem("Component", true);
            canvas.MainMenu.Items.Add(menuItemComponent);
            // Window
            MenuItem menuItemWindow = new MenuItem("Window", true);
            canvas.MainMenu.Items.Add(menuItemWindow);
            menuItemWindow.Items.AddRange(new[]
            {
                new MenuItem("Layouts"),
            });
            menuItemWindow.Items[0].Items.AddRange(new []
            {
                new MenuItem("4 Split"),
                new MenuItem("3 Split"),
                new MenuItem("Wide"),
                new MenuItem("Toggle") { RightSideText = "F12", SeparationLine = true, }, 
            });
            menuItemWindow.Items[0].Items[0].Click += delegate { EditorManager.Layout = EditorManager.LayoutOptions.FourSplit; };
            menuItemWindow.Items[0].Items[1].Click += delegate { EditorManager.Layout = EditorManager.LayoutOptions.ThreeSplit; };
            menuItemWindow.Items[0].Items[2].Click += delegate { EditorManager.Layout = EditorManager.LayoutOptions.Wide; };
            menuItemWindow.Items[0].Items[3].Click += delegate { EditorManager.ToggleLayout(); };
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

            #region Button Space

            Button buttonSpace = new Button
            {
                Text = "Global",
                Left = 10,
                Top = 10,
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

            #endregion

            #region Buttons Play, Pause and Stop

            Texture playTexture = new Texture("Editor\\PlayIconNotPressed");
            Texture playPressedTexture = new Texture("Editor\\PlayIcon");
            Button buttonPlay = new Button
            {
                Left = Screen.Width / 2 - 50,
                Top = 2,
                Height = 32,
                Width = 32,
                Glyph = new Glyph(playTexture) { SizeMode = SizeMode.Normal },
                Parent = toolBarTopPanel,
                CanFocus = false,
            };
            buttonPlay.Click += delegate { buttonPlay.Glyph.Texture = buttonPlay.Glyph.Texture == playTexture ? playPressedTexture : playTexture; };
            Texture pauseTexture = new Texture("Editor\\PauseIconNotPressed");
            Texture pausePressedTexture = new Texture("Editor\\PauseIcon");
            Button buttonPause = new Button
            {
                Left = buttonPlay.Left + 34,
                Top = 2,
                Height = 32,
                Width = 32,
                Glyph = new Glyph(pauseTexture) { SizeMode = SizeMode.Normal },
                Parent = toolBarTopPanel,
                CanFocus = false,
            };
            buttonPause.Click += delegate { buttonPause.Glyph.Texture = buttonPause.Glyph.Texture == pauseTexture ? pausePressedTexture : pauseTexture; };
            Texture stopTexture = new Texture("Editor\\StopIconNotPressed");
            Texture stopPressedTexture = new Texture("Editor\\StopIcon");
            Button buttonStop = new Button
            {
                Left = buttonPlay.Left + 68,
                Top = 2,
                Height = 32,
                Width = 32,
                Glyph = new Glyph(stopTexture) { SizeMode = SizeMode.Normal },
                Parent = toolBarTopPanel,
                CanFocus = false,
            };
            buttonStop.Click += delegate { buttonStop.Glyph.Texture = buttonStop.Glyph.Texture == stopTexture ? stopPressedTexture : stopTexture; };

            #endregion

            // Adjust top panel height.
            topPanel.Height = buttonStop.Top + buttonStop.Height + 2;
            toolBarTopPanel.Height = buttonStop.Top + buttonStop.Height + 2;

            #endregion

            #region Viewport Space
            
            // The canvas cover the whole window. I will place the static controls there.
            // I don’t place the controls directly to the manager to allow having some functionality
            // provided by the canvas like the automatic placing of menu items, status bar, etc.
            ViewportsSpace = new Container
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

            TabControl rightPanelTabControl = new TabControl
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
            inspector = rightPanelTabControl.TabPages[0];
            rightPanelTabControl.AddPage();
            rightPanelTabControl.TabPages[1].Text = "Layers";
            var label = new Label
            {
                Parent = rightPanelTabControl.TabPages[1],
                Width = 50,
                Top = 15,
                Left = rightPanelTabControl.TabPages[1].ClientWidth - 132,
                Text = "Active",
                Height = 10,
            };
            label.ToolTip.Text = "Active layers are updated.";
            label = new Label
            {
                Parent = rightPanelTabControl.TabPages[1],
                Width = 50,
                Top = 15,
                Left = rightPanelTabControl.TabPages[1].ClientWidth - 72,
                Text = "Visible",
                Height = 10,
            };
            label.ToolTip.Text = "Visible layers are rendered.";
            for (int i = 0; i < 30; i++)
            {
                CommonControls.LayerBox(i, rightPanelTabControl.TabPages[1]);
            }
            
            #endregion

        } // AddMainControls
        
        #endregion

        #region Add Game Object Controls

        /// <summary>
        /// Add game object component controls to the inspector.
        /// </summary>
        public static void AddGameObjectControlsToInspector(GameObject gameObject)
        {
            AddGameObjectControls(gameObject, inspector);
        } // AddGameObjectControlsToInspector

        /// <summary>
        /// Add game object component controls to the specified control.
        /// </summary>
        public static void AddGameObjectControls(GameObject gameObject, ClipControl control)
        {
            if (gameObject == null)
                throw new ArgumentNullException("gameObject");
            if (control == null)
                throw new ArgumentNullException("control");
            
            #region Name

            var nameLabel = new Label
            {
                Parent = control,
                Text = "Name",
                Left = 10,
                Top = 10,
                Height = 25,
                Alignment = Alignment.BottomCenter,
            };
            var nameTextBox = new TextBox
            {
                Parent = control,
                Width = control.ClientWidth - nameLabel.Width - 12,
                Text = gameObject.Name,
                Left = 60,
                Top = 10,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right
            };
            var lastSetName = gameObject.Name;
            nameTextBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    string oldName = gameObject.Name;
                    gameObject.Name = nameTextBox.Text; //asset.SetUniqueName(window.AssetName);
                    if (oldName != gameObject.Name)
                    {
                        nameTextBox.Text = gameObject.Name; // The name could be change if the name entered was not unique.
                        gameObject.Name = oldName; // This is done for the undo.
                        using (Transaction.Create())
                        {
                            // Apply the command and store for the undo feature.
                            ActionManager.SetProperty(gameObject, "Name", nameTextBox.Text);
                            ActionManager.CallMethod(// Redo
                                                     UserInterfaceManager.Invalidate,
                                                     // Undo
                                                     UserInterfaceManager.Invalidate);
                        }

                    }
                    lastSetName = gameObject.Name;
                }
            };
            nameTextBox.FocusLost += delegate
            {
                string oldName = gameObject.Name;
                gameObject.Name = nameTextBox.Text; //asset.SetUniqueName(window.AssetName);
                if (oldName != gameObject.Name)
                {
                    nameTextBox.Text = gameObject.Name; // The name could be change if the name entered was not unique.
                    gameObject.Name = oldName; // This is done for the undo.
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(gameObject, "Name", nameTextBox.Text);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                    lastSetName = gameObject.Name;
                }
            };
            nameTextBox.Draw += delegate
            {
                if (lastSetName != gameObject.Name)
                {
                    lastSetName = gameObject.Name;
                    nameTextBox.Text = gameObject.Name;
                }
            };

            #endregion

            #region Layer

            var comboBoxLayer = CommonControls.ComboBox("", control);
            comboBoxLayer.ItemIndexChanged += delegate
            {
                // Store new asset.
                if (gameObject.Layer.Number != comboBoxLayer.ItemIndex) // If it change
                {
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(gameObject, "Layer", Layer.GetLayerByNumber(comboBoxLayer.ItemIndex));
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                }
            };
            comboBoxLayer.Draw += delegate
            {
                // Add layer names here because someone could change them.
                comboBoxLayer.Items.Clear();
                for (int i = 0; i < 30; i++)
                {
                    comboBoxLayer.Items.Add(Layer.GetLayerByNumber(i).Name);
                }
                if (comboBoxLayer.ListBoxVisible)
                    return;
                // Identify current index
                comboBoxLayer.ItemIndex = gameObject.Layer.Number;
            };

            #endregion)

            #region Active

            CheckBox active = CommonControls.CheckBox("Active", control, gameObject.Active, gameObject, "Active");
            active.Top = comboBoxLayer.Top + 5;

            #endregion

            if (gameObject is GameObject3D)
            {
                GameObject3D gameObject3D = (GameObject3D)gameObject;

                #region Transform Component

                var panel = CommonControls.PanelCollapsible("Transform", control, 0);
                // Position
                var vector3BoxPosition = CommonControls.Vector3Box("Position", panel, gameObject3D.Transform.LocalPosition, gameObject3D.Transform, "LocalPosition");
                
                // Orientation.
                // This control has too many special cases, I need to do it manually.
                Vector3 initialValue = new Vector3(gameObject3D.Transform.LocalRotation.GetYawPitchRoll().Y * 180 / (float)Math.PI,
                                                   gameObject3D.Transform.LocalRotation.GetYawPitchRoll().X * 180 / (float)Math.PI,
                                                   gameObject3D.Transform.LocalRotation.GetYawPitchRoll().Z * 180 / (float)Math.PI);
                initialValue.X = (float)Math.Round(initialValue.X, 4);
                initialValue.Y = (float)Math.Round(initialValue.Y, 4);
                initialValue.Z = (float)Math.Round(initialValue.Z, 4);
                var vector3BoxRotation = CommonControls.Vector3Box("Rotation", panel, initialValue);
                vector3BoxRotation.ValueChanged += delegate
                {
                    Vector3 propertyValue = new Vector3(gameObject3D.Transform.LocalRotation.GetYawPitchRoll().Y * 180 / (float)Math.PI,
                                                        gameObject3D.Transform.LocalRotation.GetYawPitchRoll().X * 180 / (float)Math.PI,
                                                        gameObject3D.Transform.LocalRotation.GetYawPitchRoll().Z * 180 / (float)Math.PI);
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
                            ActionManager.SetProperty(gameObject3D.Transform, "LocalRotation", newValue);
                            ActionManager.CallMethod(// Redo
                                                        UserInterfaceManager.Invalidate,
                                                        // Undo
                                                        UserInterfaceManager.Invalidate);
                        }
                    }
                };
                vector3BoxRotation.Draw += delegate
                {
                    Vector3 localRotationDegrees = new Vector3(gameObject3D.Transform.LocalRotation.GetYawPitchRoll().Y * 180 / (float)Math.PI,
                                                               gameObject3D.Transform.LocalRotation.GetYawPitchRoll().X * 180 / (float)Math.PI,
                                                               gameObject3D.Transform.LocalRotation.GetYawPitchRoll().Z * 180 / (float)Math.PI);
                    // Round to avoid precision problems.
                    localRotationDegrees.X = (float)Math.Round(localRotationDegrees.X, 4);
                    localRotationDegrees.Y = (float)Math.Round(localRotationDegrees.Y, 4);
                    localRotationDegrees.Z = (float)Math.Round(localRotationDegrees.Z, 4);
                    if (vector3BoxRotation.Value != localRotationDegrees)
                        vector3BoxRotation.Value = localRotationDegrees;
                };
                
                // Scale
                var vector3BoxScale = CommonControls.Vector3Box("Scale", panel, gameObject3D.Transform.LocalScale, gameObject3D.Transform, "LocalScale");

                #endregion

                #region Camera

                if (gameObject3D.Camera != null)
                {
                    var panelCamera = CommonControls.PanelCollapsible("Camera", control, 0);
                    CameraControls.AddControls(gameObject3D.Camera, panelCamera);
                }

                #endregion

                #region Light

                if (gameObject3D.Light != null)
                {
                    if (gameObject3D.SpotLight != null)
                    {
                        var panelSpotLight = CommonControls.PanelCollapsible("Spot Light", control, 0);
                        SpotLightControls.AddControls(gameObject3D.SpotLight, panelSpotLight);
                    }
                    if (gameObject3D.PointLight != null)
                    {
                        var panelPointLight = CommonControls.PanelCollapsible("Point Light", control, 0);
                        PointLightControls.AddControls(gameObject3D.PointLight, panelPointLight);
                    }
                    if (gameObject3D.DirectionalLight != null)
                    {
                        var panelDirectionalLight = CommonControls.PanelCollapsible("Directional Light", control, 0);
                        DirectionalLightControls.AddControls(gameObject3D.DirectionalLight, panelDirectionalLight);
                    }
                }

                #endregion

            }
            else
            {
                GameObject2D gameObject2D = (GameObject2D)gameObject;
            }
        } // AddGameObjectControls

        #endregion

        #region Remove Game Object Controls

        /// <summary>
        /// Remove User Interface controls from inspector panel.
        /// </summary>
        public static void RemoveGameObjectControlsFromInspector()
        {
            inspector.RemoveControlsFromClientArea();
        } // RemoveGameObjectControlsFromInspector

        /// <summary>
        /// Remove User Interface controls from a clip control.
        /// </summary>
        public static void RemoveGameObjectControls(ClipControl control)
        {
            control.RemoveControlsFromClientArea();
        } // RemoveGameObjectControls

        #endregion

    } // MainWindow
} // XNAFinalEngine.Editor
