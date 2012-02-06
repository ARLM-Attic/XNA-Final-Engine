
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.UserInterface;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Editor
{
    public static class CarPaintWindow
    {

        /// <summary>
        /// Creates and shows the configuration window of this material.
        /// </summary>
        public static void Show(CarPaint material)
        {

            #region Window

            var window = new Window { Text = material.Name + " : Car-Paint" };

            #endregion

            #region Name

            var nameLabel = new Label
            {
                Parent = window,
                Text = "Name", Left = 10, Top = 10,
            };
            var materialNameTextBox = new TextBox
            {
                Parent = window,
                Width = window.ClientWidth - nameLabel.Width - 25,
                Text = material.Name, Left = 60, Top = 10
            };
            materialNameTextBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    material.Name = materialNameTextBox.Text;
                    window.Text = material.Name + " : Car-Paint";
                }
            };
            materialNameTextBox.FocusLost += delegate
            {
                material.Name = materialNameTextBox.Text;
                window.Text = material.Name + " : Car-Paint";
            };

            #endregion

            #region Group Diffuse

            GroupBox groupDiffuse = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Height = 160,
                Left = 8,
                Top = nameLabel.Top + nameLabel.Height + 15,
                Text = "Diffuse",
                TextColor = Color.Gray,
            };

            #region Base Paint Color

            var sliderBasePaintColor = new SliderColor
            {
                Parent = groupDiffuse,
                Left = 10,
                Top = 20,
                Color = material.BasePaintColor,
                Text = "Base Paint Color",
            };
            sliderBasePaintColor.ColorChanged += delegate { material.BasePaintColor = sliderBasePaintColor.Color; };
            sliderBasePaintColor.Draw += delegate { sliderBasePaintColor.Color = material.BasePaintColor; };

            #endregion

            #region Second Base Paint Color

            var sliderSecondBasePaintColor = new SliderColor
            {
                Parent = groupDiffuse,
                Left = 10,
                Top = sliderBasePaintColor.Top + sliderBasePaintColor.Height + 20,
                Color = material.SecondBasePaintColor,
                Text = "Second Base Paint Color",
            };
            sliderSecondBasePaintColor.ColorChanged += delegate { material.SecondBasePaintColor = sliderSecondBasePaintColor.Color; };
            sliderSecondBasePaintColor.Draw += delegate { sliderSecondBasePaintColor.Color = material.SecondBasePaintColor; };

            #endregion

            #region Middle Paint Color

            var sliderMiddlePaintColor = new SliderColor
            {
                Parent = groupDiffuse,
                Left = 10,
                Top = sliderSecondBasePaintColor.Top + sliderSecondBasePaintColor.Height + 20,
                Color = material.MiddlePaintColor,
                Text = "Middle Paint Color",
            };
            sliderMiddlePaintColor.ColorChanged += delegate { material.MiddlePaintColor = sliderMiddlePaintColor.Color; };
            sliderMiddlePaintColor.Draw += delegate { sliderMiddlePaintColor.Color = material.MiddlePaintColor; };

            #endregion

            groupDiffuse.Height = sliderMiddlePaintColor.Top + sliderMiddlePaintColor.Height + 20;

            #endregion

            #region Group Specular

            GroupBox groupSpecular = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Height = 160,
                Left = 8,
                Top = groupDiffuse.Top + groupDiffuse.Height + 15,
                Text = "Specular",
                TextColor = Color.Gray,
            };

            #region Specular Intensity

            var sliderSpecularIntensity = new SliderNumeric
            {
                Parent = groupSpecular,
                Left = 10,
                Top = 25,
                Value = material.SpecularIntensity,
                Text = "Specular Intensity",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 2,
            };
            sliderSpecularIntensity.ValueChanged += delegate
            {
                material.SpecularIntensity = sliderSpecularIntensity.Value;
            };
            sliderSpecularIntensity.Draw += delegate { sliderSpecularIntensity.Value = material.SpecularIntensity; };

            #endregion

            #region Specular Power

            var sliderSpecularPower = new SliderNumeric
            {
                Parent = groupSpecular,
                Left = 10,
                Top = 10 + sliderSpecularIntensity.Top + sliderSpecularIntensity.Height,
                Value = material.SpecularPower,
                Text = "Specular Power",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 100,
            };
            sliderSpecularPower.ValueChanged += delegate
            {
                material.SpecularPower = sliderSpecularPower.Value;
            };
            sliderSpecularPower.Draw += delegate { sliderSpecularPower.Value = material.SpecularPower; };

            #endregion

            #region Specular Texture

            var labelSpecularTexture = new Label
            {
                Parent = groupSpecular,
                Left = 10,
                Top = 10 + sliderSpecularPower.Top + sliderSpecularPower.Height,
                Width = 150,
                Text = "Specular Texture"
            };
            var comboBoxSpecularTexture = new ComboBox
            {
                Parent = groupSpecular,
                Left = labelSpecularTexture.Left + labelSpecularTexture.Width,
                Top = 10 + sliderSpecularPower.Top + sliderSpecularPower.Height,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
            };
            comboBoxSpecularTexture.Width = groupSpecular.Width - 10 - comboBoxSpecularTexture.Left;
            // Add textures name
            comboBoxSpecularTexture.Items.Add("No texture");
            comboBoxSpecularTexture.Items.AddRange(Texture.TexturesFilenames);
            // Events
            comboBoxSpecularTexture.ItemIndexChanged += delegate
            {
                if (comboBoxSpecularTexture.ItemIndex <= 0)
                    material.SpecularTexture = null;
                else
                {
                    if (material.SpecularTexture == null || material.SpecularTexture.Name != (string)comboBoxSpecularTexture.Items[comboBoxSpecularTexture.ItemIndex])
                        material.SpecularTexture = new Texture((string)comboBoxSpecularTexture.Items[comboBoxSpecularTexture.ItemIndex]);
                }
            };
            comboBoxSpecularTexture.Draw += delegate
            {
                if (comboBoxSpecularTexture.ListBoxVisible)
                    return;
                // Identify current index
                if (material.SpecularTexture == null)
                    comboBoxSpecularTexture.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < comboBoxSpecularTexture.Items.Count; i++)
                        if ((string)comboBoxSpecularTexture.Items[i] == material.SpecularTexture.Name)
                        {
                            comboBoxSpecularTexture.ItemIndex = i;
                            break;
                        }
                }
            };

            #endregion

            #region Specular Texture Power Enabled

            var checkBoxSpecularTexturePowerEnabled = new CheckBox
            {
                Parent = groupSpecular,
                Left = 10,
                Top = 10 + comboBoxSpecularTexture.Top + comboBoxSpecularTexture.Height,
                Width = window.ClientWidth - 16,
                Checked = material.SpecularTexturePowerEnabled,
                Text = " Specular Texture Power Enabled",
                ToolTip =
                {
                    Text = "Indicates if the specular power will be read from the texture (the alpha channel of the specular texture) or from the specular power property."
                }
            };
            checkBoxSpecularTexturePowerEnabled.CheckedChanged += delegate
            {
                material.SpecularTexturePowerEnabled = checkBoxSpecularTexturePowerEnabled.Checked;
            };
            checkBoxSpecularTexturePowerEnabled.Draw += delegate { checkBoxSpecularTexturePowerEnabled.Checked = material.SpecularTexturePowerEnabled; };

            #endregion

            #region Reflection Texture

            var labelReflectionTexture = new Label
            {
                Parent = groupSpecular,
                Left = 10,
                Top = 10 + checkBoxSpecularTexturePowerEnabled.Top + checkBoxSpecularTexturePowerEnabled.Height,
                Width = 150,
                Text = "Reflection Texture"
            };
            var comboBoxReflectionTexture = new ComboBox
            {
                Parent = groupSpecular,
                Left = labelReflectionTexture.Left + labelReflectionTexture.Width,
                Top = labelReflectionTexture.Top,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
            };
            comboBoxReflectionTexture.Width = groupSpecular.Width - 10 - comboBoxReflectionTexture.Left;
            // Add textures name
            comboBoxReflectionTexture.Items.Add("No texture");
            comboBoxReflectionTexture.Items.AddRange(TextureCube.TexturesFilename);
            // Events
            comboBoxReflectionTexture.ItemIndexChanged += delegate
            {
                if (comboBoxReflectionTexture.ItemIndex <= 0)
                    material.ReflectionTexture = null;
                else
                {
                    if (material.ReflectionTexture == null || material.ReflectionTexture.Name != (string)comboBoxReflectionTexture.Items[comboBoxReflectionTexture.ItemIndex])
                        material.ReflectionTexture = new TextureCube((string)comboBoxReflectionTexture.Items[comboBoxReflectionTexture.ItemIndex]);
                }
            };
            comboBoxReflectionTexture.Draw += delegate
            {
                if (comboBoxReflectionTexture.ListBoxVisible)
                    return;
                // Identify current index
                if (material.ReflectionTexture == null)
                    comboBoxReflectionTexture.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < comboBoxReflectionTexture.Items.Count; i++)
                        if ((string)comboBoxReflectionTexture.Items[i] == material.ReflectionTexture.Name)
                        {
                            comboBoxReflectionTexture.ItemIndex = i;
                            break;
                        }
                }
            };

            #endregion

            groupSpecular.Height = comboBoxReflectionTexture.Top + comboBoxReflectionTexture.Height + 20;
            
            #endregion
            
            window.Height = groupSpecular.Top + groupSpecular.Height + 40;

        } // Show

    } // CarPaintWindow
} // XNAFinalEngine.Editor