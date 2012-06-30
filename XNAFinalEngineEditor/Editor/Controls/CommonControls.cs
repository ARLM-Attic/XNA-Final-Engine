
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
using System.Reflection;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Input;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// I need to find a common denominator a CSS for my editor, so I made this helper class.
    /// I also add methods and properties that help placing new components.
    /// </summary>
    internal static class CommonControls
    {

        #region Variables

        // Stores the initial value when the slider is moving.
        private static int sliderNumericOldIntValue;

        // Stores the initial value when the slider is moving.
        private static float sliderNumericOldFloatValue;

        // Stores the initial value when the slider is moving.
        private static Color sliderColorOldValue;

        #endregion

        #region Properties

        /// <summary>
        /// Default separation between group controls.
        /// </summary>
        public static int GroupSeparation { get { return 10; } }

        /// <summary>
        /// Default separation between controls.
        /// </summary>
        public static int ControlSeparation { get { return 5; } }

        #endregion
        
        #region Group

        /// <summary>
        /// Returns a group control placed in the first free spot.
        /// </summary>
        public static GroupBox Group(string name, ClipControl parent)
        {
            GroupBox group = new GroupBox
            {
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = parent.ClientWidth - 16,
                Height = 160,
                Left = 8,
                Top = parent.AvailablePositionInsideControl + GroupSeparation,
                Text = name,
                TextColor = Color.Gray,
                Parent = parent,
            };

            return group;
        } // Group

        #endregion

        #region Vector3 Box

        /// <summary>
        /// Returns a vector3 box control placed in the first free spot.
        /// You need to update the value manually.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        public static Vector3Box Vector3Box(string name, ClipControl parent, Vector3 initialValue)
        {
            var label = new Label
            {
                Left = 10,
                Width = 155,
                Text = name,
                Height = 10,
            };
            if (parent.AvailablePositionInsideControl == 0)
                label.Top = 10;
            else
                label.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            label.Parent = parent;
            var vector3Box = new Vector3Box
            {
                Left = 10,
                Value = initialValue,
                Top = label.Top + label.Height + 5,
                Parent = parent,
                Width = parent.ClientWidth - 25,
            };

            return vector3Box;
        } // Vector3Box

        /// <summary>
        /// Returns a vector3 box control placed in the first free spot.
        /// It automatically manages the property setting, undo and redo.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="propertyOwner">The object to manipualte</param>
        /// <param name="propertyName">The property name.</param>
        public static Vector3Box Vector3Box(string name, ClipControl parent, Vector3 initialValue, object propertyOwner, string propertyName)
        {
            var vector3Box = Vector3Box(name, parent, initialValue);

            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);
            vector3Box.ValueChanged += delegate
            {
                if (vector3Box.Value != (Vector3)property.GetValue(propertyOwner, null))
                {
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(propertyOwner, propertyName, vector3Box.Value);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                }
            };
            vector3Box.Draw += delegate { vector3Box.Value = (Vector3)property.GetValue(propertyOwner, null); };

            return vector3Box;
        } // Vector3Box

        #endregion

        #region Slider Numeric

        /// <summary>
        /// Returns a numeric slider control placed in the first free spot.
        /// You need to update the value manually.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="ifOutOfRangeRescale">If the value is out of range, is rescaled or not?</param>
        /// <param name="valueCanBeOutOfRange">Can the value be out of range?</param>
        /// <param name="minimumValue">Minimum value.</param>
        /// <param name="maximumValue">Maximum value.</param>
        public static SliderNumeric SliderNumeric(string name, ClipControl parent, float initialValue, 
                                                  bool ifOutOfRangeRescale, bool valueCanBeOutOfRange,
                                                  float minimumValue, float maximumValue)
        {
            var sliderNumeric = new SliderNumeric
            {
                Left = 10,
                Text = name,
                IfOutOfRangeRescale = ifOutOfRangeRescale,
                ValueCanBeOutOfRange = valueCanBeOutOfRange,
                MinimumValue = minimumValue,
                MaximumValue = maximumValue,
                Value = initialValue,
                Width = parent.ClientWidth - 25,
            };
            if (parent.AvailablePositionInsideControl == 0)
                sliderNumeric.Top = 25;
            else
                sliderNumeric.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            sliderNumeric.Parent = parent;

            return sliderNumeric;
        } // SliderNumeric

        /// <summary>
        /// Returns a numeric slider control placed in the first free spot.
        /// It automatically manages the property setting, undo and redo.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="ifOutOfRangeRescale">If the value is out of range, is rescaled or not?</param>
        /// <param name="valueCanBeOutOfRange">Can the value be out of range?</param>
        /// <param name="minimumValue">Minimum value.</param>
        /// <param name="maximumValue">Maximum value.</param>
        /// <param name="propertyOwner">The object to manipualte</param>
        /// <param name="propertyName">The property name.</param>
        public static SliderNumeric SliderNumericFloat(string name, ClipControl parent, float initialValue,
                                                       bool ifOutOfRangeRescale, bool valueCanBeOutOfRange,
                                                       float minimumValue, float maximumValue, object propertyOwner, string propertyName)
        {
            var sliderNumeric = SliderNumeric(name, parent, initialValue, ifOutOfRangeRescale, valueCanBeOutOfRange, minimumValue, maximumValue);

            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);
            sliderNumeric.ValueChanged += delegate
            {
                if (sliderNumeric.Value != (float)property.GetValue(propertyOwner, null))
                {
                    // If it was set by the text box
                    if (!Mouse.LeftButtonJustReleased && !Mouse.LeftButtonPressed)
                    {
                        using (Transaction.Create())
                        {
                            // Apply the command and store for the undo feature.
                            ActionManager.SetProperty(propertyOwner, propertyName, sliderNumeric.Value);
                            ActionManager.CallMethod(// Redo
                                                     UserInterfaceManager.Invalidate,
                                                     // Undo
                                                     UserInterfaceManager.Invalidate);
                        }
                    }
                    else
                    {
                        property.SetValue(propertyOwner, sliderNumeric.Value, null);
                    }
                }
            };
            sliderNumeric.SliderDown += delegate { sliderNumericOldFloatValue = (float)property.GetValue(propertyOwner, null); };
            sliderNumeric.SliderUp += delegate
            {
                if (sliderNumeric.Value != sliderNumericOldFloatValue)
                {
                    property.SetValue(propertyOwner, sliderNumericOldFloatValue, null); // We put temporarely the initial value.
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(propertyOwner, propertyName, sliderNumeric.Value);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                }
            };
            sliderNumeric.Draw += delegate { sliderNumeric.Value = (float)property.GetValue(propertyOwner, null); };

            return sliderNumeric;
        } // SliderNumericFloat

        /// <summary>
        /// Returns a numeric slider control placed in the first free spot.
        /// It automatically manages the property setting, undo and redo.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="ifOutOfRangeRescale">If the value is out of range, is rescaled or not?</param>
        /// <param name="valueCanBeOutOfRange">Can the value be out of range?</param>
        /// <param name="minimumValue">Minimum value.</param>
        /// <param name="maximumValue">Maximum value.</param>
        /// <param name="propertyOwner">The object to manipualte</param>
        /// <param name="propertyName">The property name.</param>
        public static SliderNumeric SliderNumericInt(string name, ClipControl parent, int initialValue,
                                                     bool ifOutOfRangeRescale, bool valueCanBeOutOfRange,
                                                     float minimumValue, float maximumValue, object propertyOwner, string propertyName)
        {
            var sliderNumeric = SliderNumeric(name, parent, initialValue, ifOutOfRangeRescale, valueCanBeOutOfRange, minimumValue, maximumValue);

            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);
            sliderNumeric.ValueChanged += delegate
            {
                if (Convert.ToInt32(sliderNumeric.Value) != (int)property.GetValue(propertyOwner, null))
                {
                    // If it was set by the text box
                    if (!Mouse.LeftButtonJustReleased && !Mouse.LeftButtonPressed)
                    {
                        using (Transaction.Create())
                        {
                            // Apply the command and store for the undo feature.
                            ActionManager.SetProperty(propertyOwner, propertyName, Convert.ToInt32(sliderNumeric.Value));
                            ActionManager.CallMethod(// Redo
                                                     UserInterfaceManager.Invalidate,
                                                     // Undo
                                                     UserInterfaceManager.Invalidate);
                        }
                    }
                    else
                    {
                        property.SetValue(propertyOwner, Convert.ToInt32(sliderNumeric.Value), null);
                    }
                }
            };
            sliderNumeric.SliderDown += delegate { sliderNumericOldIntValue = (int)property.GetValue(propertyOwner, null); };
            sliderNumeric.SliderUp += delegate
            {
                if (Convert.ToInt32(sliderNumeric.Value) != sliderNumericOldIntValue)
                {
                    property.SetValue(propertyOwner, sliderNumericOldIntValue, null); // We put temporarely the initial value.
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(propertyOwner, propertyName, Convert.ToInt32(sliderNumeric.Value));
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                }
            };
            sliderNumeric.Draw += delegate { sliderNumeric.Value = (int)property.GetValue(propertyOwner, null); };

            return sliderNumeric;
        } // SliderNumericInt

        #endregion

        #region Slider Color

        /// <summary>
        /// Returns a color slider control placed in the first free spot.
        /// You need to update the value manually.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        public static SliderColor SliderColor(string name, ClipControl parent, Color initialValue)
        {
            var sliderColor = new SliderColor
            {
                Left = 10,
                Text = name,
                Color = initialValue,
                Width = parent.ClientWidth - 25,
            };
            if (parent.AvailablePositionInsideControl == 0)
                sliderColor.Top = 25;
            else
                sliderColor.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            sliderColor.Parent = parent;

            return sliderColor;
        } // SliderColor

        /// <summary>
        /// Returns a color slider control placed in the first free spot.
        /// You need to update the value manually.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="propertyOwner">The object to manipualte</param>
        /// <param name="propertyName">The property name.</param>
        public static SliderColor SliderColor(string name, ClipControl parent, Color initialValue, object propertyOwner, string propertyName)
        {
            var sliderColor = SliderColor(name, parent, initialValue);

            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);
            sliderColor.ColorChanged += delegate
            {
                if (sliderColor.Color != (Color)property.GetValue(propertyOwner, null))
                {
                    // If it was set by the text box
                    if (!Mouse.LeftButtonJustReleased && !Mouse.LeftButtonPressed)
                    {
                        using (Transaction.Create())
                        {
                            // Apply the command and store for the undo feature.
                            ActionManager.SetProperty(propertyOwner, propertyName, sliderColor.Color);
                            ActionManager.CallMethod(// Redo
                                                     UserInterfaceManager.Invalidate,
                                                     // Undo
                                                     UserInterfaceManager.Invalidate);
                        }
                    }
                    else
                    {
                        property.SetValue(propertyOwner, sliderColor.Color, null);
                    }
                }
            };
            sliderColor.SliderDown += delegate { sliderColorOldValue = (Color)property.GetValue(propertyOwner, null); };
            sliderColor.SliderUp += delegate
            {
                if (sliderColor.Color != sliderColorOldValue)
                {
                    property.SetValue(propertyOwner, sliderColorOldValue, null); // We put temporarely the initial value.
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(propertyOwner, propertyName, sliderColor.Color);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                }
            };
            sliderColor.Draw += delegate { sliderColor.Color = (Color)property.GetValue(propertyOwner, null); };

            return sliderColor;
        } // SliderColor

        #endregion

        #region Check Box

        /// <summary>
        /// Returns a check box control placed in the first free spot.
        /// You need to update the value manually.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="toolTip">Tool tip text.</param>
        public static CheckBox CheckBox(string name, ClipControl parent, bool initialValue, string toolTip = "")
        {
            var checkBox = new CheckBox
            {
                Parent = parent,
                Left = 10,
                Height = 25,
                Width = 250,
                Anchor = Anchors.Left | Anchors.Top,
                Checked = initialValue,
                Text = " " + name,
                
            };
            checkBox.ToolTip.Text = toolTip;
            if (parent.AvailablePositionInsideControl == 0)
                checkBox.Top = 25;
            else
                checkBox.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            checkBox.Parent = parent;

            return checkBox;
        } // CheckBox

        /// <summary>
        /// Returns a check box control placed in the first free spot.
        /// You need to update the value manually.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="propertyOwner">The object to manipualte</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="toolTip">Tool tip text.</param>
        public static CheckBox CheckBox(string name, ClipControl parent, bool initialValue, object propertyOwner, string propertyName, string toolTip = "")
        {
            var checkBox = CheckBox(name, parent, initialValue, toolTip);

            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);
            checkBox.CheckedChanged += delegate
            {
                if (checkBox.Checked != (bool)property.GetValue(propertyOwner, null))
                {
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(propertyOwner, propertyName, checkBox.Checked);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                }
            };
            checkBox.Draw += delegate { checkBox.Checked = (bool)property.GetValue(propertyOwner, null); };

            return checkBox;
        } // CheckBox

        #endregion

        #region ComboBox

        /// <summary>
        /// Returns a combo box control placed in the first free spot.
        /// You need to update the value and maintain the items manually.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="toolTip">Tool tip text.</param>
        public static ComboBox ComboBox(string name, ClipControl parent, string toolTip = "")
        {
            var label = new Label
            {
                Left = 10,
                Width = 155,
                Text = name,
                Height = 25,
            };
            var comboBox = new ComboBox
            {
                Parent = parent,
                Left = label.Left + label.Width,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
            };
            comboBox.Width = parent.ClientWidth - 15 - comboBox.Left;
            // ToolTip
            label.ToolTip.Text = toolTip;
            comboBox.ToolTip.Text = toolTip;
            // Top
            if (parent.AvailablePositionInsideControl == 0)
                label.Top = comboBox.Top = 25;
            else
                label.Top = comboBox.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            // Parent
            comboBox.Parent = parent;
            label.Parent = parent;
            // Disable label when the control is disable.
            comboBox.EnabledChanged += delegate { label.Enabled = comboBox.Enabled; };

            return comboBox;
        } // ComboBox

        #endregion

        #region Asset Selector

        /// <summary>
        /// Returns a asset selector control placed in the first free spot.
        /// You need to update do everything manually :p
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        public static AssetSelector AssetSelector(string name, ClipControl parent)
        {
            var assetSelector = new AssetSelector
            {
                Text = name,
                Left = 10,
                Width = parent.ClientWidth - 20,
            };
            // Top
            if (parent.AvailablePositionInsideControl == 0)
                assetSelector.Top = 25;
            else
                assetSelector.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            // Parent
            assetSelector.Parent = parent;

            return assetSelector;
        } // AssetSelector

        /// <summary>
        /// Returns a asset selector control placed in the first free spot.
        /// You need to update do everything manually :p
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="propertyOwner">The object to manipualte</param>
        /// <param name="propertyName">The property name.</param>
        public static AssetSelector AssetSelector<TAssetType>(string name, ClipControl parent, object propertyOwner, string propertyName) where TAssetType : Asset
        {
            var assetSelector = AssetSelector(name, parent);

            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);

            #region Add

            assetSelector.AssetAdded += delegate
            {
                if (typeof(TAssetType) == typeof(Sky))
                {
                    var assetTypeListBox = AssetTypeListBox(assetSelector, propertyOwner, propertyName, new[] { "Skybox", "Skydome" });
                    assetTypeListBox.Click += delegate
                    {
                        if (assetTypeListBox.ItemIndex == 0)
                            CreateAsset<Skybox>(assetSelector, propertyOwner, propertyName);
                        else
                            CreateAsset<Skydome>(assetSelector, propertyOwner, propertyName);
                        assetTypeListBox.Dispose();
                    };
                }
                else if (typeof(TAssetType) == typeof(AmbientOcclusion))
                {
                    var assetTypeListBox = AssetTypeListBox(assetSelector, propertyOwner, propertyName, new[] { "Horizon Based Ambient Occlusion", "Ray Marching Ambient Occlusion" });
                    assetTypeListBox.Click += delegate
                    {
                        if (assetTypeListBox.ItemIndex == 0)
                            CreateAsset<HorizonBasedAmbientOcclusion>(assetSelector, propertyOwner, propertyName);
                        else
                            CreateAsset<RayMarchingAmbientOcclusion>(assetSelector, propertyOwner, propertyName);
                        assetTypeListBox.Dispose();
                    };
                }
                else
                    CreateAsset<TAssetType>(assetSelector, propertyOwner, propertyName);
            };

            #endregion

            #region Edit

            assetSelector.AssetEdited += delegate
            {
                if (property.GetValue(propertyOwner, null) is Skybox)
                    AssetWindow.Show<Skybox>((Asset)property.GetValue(propertyOwner, null));
                else if (property.GetValue(propertyOwner, null) is Skydome)
                    AssetWindow.Show<Skydome>((Asset)property.GetValue(propertyOwner, null));
                else if (property.GetValue(propertyOwner, null) is HorizonBasedAmbientOcclusion)
                    AssetWindow.Show<HorizonBasedAmbientOcclusion>((Asset)property.GetValue(propertyOwner, null));
                else if (property.GetValue(propertyOwner, null) is RayMarchingAmbientOcclusion)
                    AssetWindow.Show<RayMarchingAmbientOcclusion>((Asset)property.GetValue(propertyOwner, null));
                else if (property.GetValue(propertyOwner, null) is BasicShadow)
                    AssetWindow.Show<BasicShadow>((Asset)property.GetValue(propertyOwner, null));
                else
                    AssetWindow.Show<TAssetType>((Asset)property.GetValue(propertyOwner, null));
            };

            #endregion

            #region Item Index Changed

            assetSelector.ItemIndexChanged += delegate
            {
                if (assetSelector.ItemIndex <= 0)
                {
                    if (property.GetValue(propertyOwner, null) != null) // If it change
                    {
                        using (Transaction.Create())
                        {
                            // Apply the command and store for the undo feature.
                            ActionManager.SetProperty(propertyOwner, propertyName, null);
                            ActionManager.CallMethod(// Redo
                                                     UserInterfaceManager.Invalidate,
                                                     // Undo
                                                     UserInterfaceManager.Invalidate);
                        }
                    }
                }
                else
                {
                    // Find Asset from index.
                    Asset assetToChange = null;
                    foreach (Asset asset in Asset.LoadedAssets)
                    {
                        // You can filter some assets here.
                        if (asset is TAssetType && asset.Name == (string)assetSelector.Items[assetSelector.ItemIndex])
                            assetToChange = asset;
                    }
                    // Store new asset.
                    if (property.GetValue(propertyOwner, null) != assetToChange) // If it change
                    {
                        using (Transaction.Create())
                        {
                            // Apply the command and store for the undo feature.
                            ActionManager.SetProperty(propertyOwner, propertyName, assetToChange);
                            ActionManager.CallMethod(// Redo
                                                     UserInterfaceManager.Invalidate,
                                                     // Undo
                                                     UserInterfaceManager.Invalidate);
                        }   
                    }
                }
                assetSelector.EditButtonEnabled = property.GetValue(propertyOwner, null) != null;
            };

            #endregion

            #region Draw

            assetSelector.Draw += delegate
            {
                // Add textures name here because someone could dispose or add new asset.
                assetSelector.Items.Clear();
                
                // Texture
                assetSelector.Items.Add("No " + typeof(TAssetType).Name);
                foreach (Asset asset in Asset.SortedLoadedAssets)
                {
                    // You can filter some assets here.
                    if (asset is TAssetType && (asset.ContentManager == null || !asset.ContentManager.Hidden))
                        assetSelector.Items.Add(asset.Name);
                }

                if (assetSelector.ListBoxVisible)
                    return;
                // Identify current index
                if (property.GetValue(propertyOwner, null) == null)
                    assetSelector.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < assetSelector.Items.Count; i++)
                    {
                        if ((string)assetSelector.Items[i] == ((Asset)property.GetValue(propertyOwner, null)).Name)
                        {
                            assetSelector.ItemIndex = i;
                            break;
                        }
                    }
                }
            };

            #endregion

            return assetSelector;
        } // AssetSelector

        #region Create Asset

        /// <summary>
        /// Opens a asset creation window and manages its behavior.
        /// </summary>
        private static void CreateAsset<TAssetType>(AssetSelector assetSelector, object propertyOwner, string propertyName) where TAssetType : Asset
        {
            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);
            // Store the previous asset because maybe the user cancel the creation.
            Asset previousAsset = (Asset)property.GetValue(propertyOwner, null);
            Asset currentAsset = null;

            AssetWindow.CurrentCreatedAssetChanged += delegate
            {
                currentAsset = AssetWindow.CurrentCreatedAsset;
                property.SetValue(propertyOwner, AssetWindow.CurrentCreatedAsset, null);
                assetSelector.Invalidate();
            };
            Window window = AssetWindow.Show<TAssetType>(null);

            // If the operation was cancel then restore the previous asset.
            window.Closed += delegate
            {
                // Set the previous asset onto the property.
                // This is done when the user cancel the operation and also in the creation because the undo feature needs the previous value.
                property.SetValue(propertyOwner, previousAsset, null);
                // If the operation was no canceled.
                if (currentAsset != null)
                {
                    // Store the undo/redo Operation
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(propertyOwner, propertyName, currentAsset);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                        // The operations to create and dispose the asset should be also included here.
                        // TODO!!
                    }
                }
            };
        } // CreateAsset

        #endregion

        #region Asset Type List Box

        /// <summary>
        /// Get a list box used to select asset types.
        /// </summary>
        private static ListBox AssetTypeListBox(AssetSelector assetSelector, object propertyOwner, string propertyName,
                                                     IEnumerable<string> assetTypes)
        {
            ListBox assetTypeListBox = new ListBox
            {
                HotTrack = true,
                Detached = true,
                Left = assetSelector.ControlLeftAbsoluteCoordinate - assetSelector.Root.Left + 140,
                Top = assetSelector.ControlTopAbsoluteCoordinate - assetSelector.Root.Top + assetSelector.Height - 8,
                Width = assetSelector.Width - 140,
            };
            // If there is no place to put the list box under the control then is moved up.
            if (assetTypeListBox.ControlTopAbsoluteCoordinate + assetTypeListBox.Height > Screen.Height)
                assetTypeListBox.Top = assetTypeListBox.Top - assetSelector.Height - assetTypeListBox.Height - 2;

            assetTypeListBox.SkinInformation = new SkinControlInformation(Skin.Controls["ComboBox.ListBox"]);
            assetTypeListBox.Items.AddRange(assetTypes);
            ((Container)assetSelector.Root).Add(assetTypeListBox, false);
            assetTypeListBox.AutoHeight(5);

            UserInterfaceManager.InputSystem.MouseDown += delegate
            {
                // If the user click outside the list box then it is hide.
                if (assetTypeListBox.Visible &&
                   (Mouse.Position.X < assetTypeListBox.ControlLeftAbsoluteCoordinate ||
                    Mouse.Position.X > assetTypeListBox.ControlLeftAbsoluteCoordinate + assetTypeListBox.Width ||
                    Mouse.Position.Y < assetTypeListBox.ControlTopAbsoluteCoordinate ||
                    Mouse.Position.Y > assetTypeListBox.ControlTopAbsoluteCoordinate + assetTypeListBox.Height))
                {
                    assetTypeListBox.Dispose();
                }
            };
            return assetTypeListBox;
        } // AssetTypeListBox

        #endregion

        #endregion

        #region Image Box

        /// <summary>
        /// Returns a image box control placed in the first free spot.
        /// </summary>
        /// <param name="texture">Texture to show.</param>
        /// <param name="parent">Parent.</param>
        public static ImageBox ImageBox(Texture texture, ClipControl parent)
        {
            var imageBox = new ImageBox
            {
                Left = 8,
                Width = parent.ClientWidth - 16,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                SizeMode = SizeMode.Fit,
                Texture = texture,
                Height = 200
            };
            // Top
            if (parent.AvailablePositionInsideControl == 0)
                imageBox.Top = 25;
            else
                imageBox.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            // Parent
            imageBox.Parent = parent;

            return imageBox;
        } // ImageBox

        #endregion

        #region Text Box

        /// <summary>
        /// Returns a text box control placed in the first free spot.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        public static TextBox TextBox(string name, ClipControl parent, string initialValue)
        {
            var label = new Label
            {
                Text = name,
                Left = 10,
                Width = 150,
                Height = 25,
            };
            var textBox = new TextBox
            {
                Width = parent.ClientWidth - label.Width - 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Text = initialValue,
                Left = 160,
            };
            // Top
            if (parent.AvailablePositionInsideControl == 0)
                textBox.Top = label.Top = 25;
            else
                textBox.Top = label.Top = parent.AvailablePositionInsideControl + ControlSeparation;
            // Parent
            textBox.Parent = parent;
            label.Parent = parent;
            // Disable label when the control is disable.
            textBox.EnabledChanged += delegate { label.Enabled = textBox.Enabled; };

            return textBox;
        } // TextBox

        /// <summary>
        /// Returns a text box control placed in the first free spot.
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="propertyOwner">The object to manipualte</param>
        /// <param name="propertyName">The property name.</param>
        public static TextBox TextBox(string name, ClipControl parent, string initialValue, object propertyOwner, string propertyName)
        {
            TextBox textBox = TextBox(name, parent, initialValue);
            
            PropertyInfo property = propertyOwner.GetType().GetProperty(propertyName);
            textBox.TextChanged += delegate
            {
                if (textBox.Text != (string)property.GetValue(propertyOwner, null))
                {
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(propertyOwner, propertyName, textBox.Text);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                }
            };
            textBox.Draw += delegate { textBox.Text = (string)property.GetValue(propertyOwner, null); };

            return textBox;
        } // TextBox

        #endregion

        #region Panel Collapsible

        /// <summary>
        /// Returns a panel collapsible control placed in the first free spot of a tab of a tab control.
        /// </summary>
        public static PanelCollapsible PanelCollapsible(string name, ClipControl parent, int page)
        {
            PanelCollapsible panelCollapsible = new PanelCollapsible
            {
                Anchor = Anchors.Left | Anchors.Right | Anchors.Top,
                Width = parent.ClientWidth,
                Top = parent.AvailablePositionInsideControl + ControlSeparation,
                Text = name,
                Parent = parent,
            };

            return panelCollapsible;
        } // PanelCollapsible

        #endregion
        
    } // CommonControls
} // XNAFinalEngine.Editor