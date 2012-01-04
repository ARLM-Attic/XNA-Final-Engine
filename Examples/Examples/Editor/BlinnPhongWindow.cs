
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
    public static class BlinnPhongWindow
    {

        /// <summary>
        /// Creates and shows the configuration window of this material.
        /// </summary>
        public static void Show(BlinnPhong material)
        {

            #region Window

            var window = new Window
            {
                Text = material.Name + " : Constant"
            };
            UserInterfaceManager.Add(window);
            window.Closed += delegate { };

            #endregion

            #region Name

            var materialNameLabel = new Label {Text = "Name", Left = 10, Top = 10,};
            window.Add(materialNameLabel);
            var materialNameTextBox = new TextBox { Text = material.Name, Left = 60, Top = 10 };
            window.Add(materialNameTextBox);
            materialNameTextBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    material.Name = materialNameTextBox.Text;
                    window.Text = material.Name + " : Constant";
                }
            };
            materialNameTextBox.FocusLost += delegate
            {
                material.Name = materialNameTextBox.Text;
                window.Text = material.Name + " : Constant";
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
                Top = materialNameLabel.Top + materialNameLabel.Height + 15,
                Text = "Diffuse",
                TextColor = Color.Gray,
            };

            #region Diffuse Color

            var sliderDiffuseColor = new SliderColor
            {
                Left = 10,
                Top = 20,
                Color = material.DiffuseColor,
                Text = "Diffuse Color",
            };
            groupDiffuse.Add(sliderDiffuseColor);
            sliderDiffuseColor.ColorChanged += delegate { material.DiffuseColor = sliderDiffuseColor.Color; };
            sliderDiffuseColor.Draw += delegate { sliderDiffuseColor.Color = material.DiffuseColor; };

            #endregion

            #region Diffuse Texture

            var labelDiffuseTexture = new Label
            {
                Left = 10,
                Top = 10 + sliderDiffuseColor.Top + sliderDiffuseColor.Height,
                Width = 150,
                Text = "Diffuse Texture"
            };
            var comboBoxDiffuseTexture = new ComboBox
            {
                Left = labelDiffuseTexture.Left + labelDiffuseTexture.Width,
                Top = 10 + sliderDiffuseColor.Top + sliderDiffuseColor.Height,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
            };
            comboBoxDiffuseTexture.Width = groupDiffuse.Width - 10 - comboBoxDiffuseTexture.Left;
            // Add textures name
            comboBoxDiffuseTexture.Items.Add("No texture");
            comboBoxDiffuseTexture.Items.AddRange(Texture.TexturesFilename);
            // Events
            comboBoxDiffuseTexture.ItemIndexChanged += delegate
            {
                if (comboBoxDiffuseTexture.ItemIndex <= 0)
                    material.DiffuseTexture = null;
                else
                {
                    if (material.DiffuseTexture == null || material.DiffuseTexture.Name != (string)comboBoxDiffuseTexture.Items[comboBoxDiffuseTexture.ItemIndex])
                        material.DiffuseTexture = new Texture((string)comboBoxDiffuseTexture.Items[comboBoxDiffuseTexture.ItemIndex]);
                }
            };
            comboBoxDiffuseTexture.Draw += delegate
            {
                if (comboBoxDiffuseTexture.ListBoxVisible)
                    return;
                // Identify current index
                if (material.DiffuseTexture == null)
                    comboBoxDiffuseTexture.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < comboBoxDiffuseTexture.Items.Count; i++)
                        if ((string)comboBoxDiffuseTexture.Items[i] == material.DiffuseTexture.Name)
                        {
                            comboBoxDiffuseTexture.ItemIndex = i;
                            break;
                        }
                }
            };
            groupDiffuse.Add(labelDiffuseTexture);
            groupDiffuse.Add(comboBoxDiffuseTexture);
            
            #endregion

            groupDiffuse.Height = comboBoxDiffuseTexture.Top + comboBoxDiffuseTexture.Height + 20;

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
                Left = 10,
                Top = 25,
                Value = material.SpecularIntensity,
                Text = "Specular Intensity",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 2,
            };
            groupSpecular.Add(sliderSpecularIntensity);
            sliderSpecularIntensity.ValueChanged += delegate
            {
                material.SpecularIntensity = sliderSpecularIntensity.Value;
            };
            sliderSpecularIntensity.Draw += delegate { sliderSpecularIntensity.Value = material.SpecularIntensity; };

            #endregion

            #region Specular Power

            var sliderSpecularPower = new SliderNumeric
            {
                Left = 10,
                Top = 10 + sliderSpecularIntensity.Top + sliderSpecularIntensity.Height,
                Value = material.SpecularPower,
                Text = "Specular Power",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 100,
            };
            groupSpecular.Add(sliderSpecularPower);
            sliderSpecularPower.ValueChanged += delegate
            {
                material.SpecularPower = sliderSpecularPower.Value;
            };
            sliderSpecularPower.Draw += delegate { sliderSpecularPower.Value = material.SpecularPower; };

            #endregion

            #region Specular Texture

            var labelSpecularTexture = new Label
            {
                Left = 10,
                Top = 10 + sliderSpecularPower.Top + sliderSpecularPower.Height,
                Width = 150,
                Text = "Specular Texture"
            };
            var comboBoxSpecularTexture = new ComboBox
            {
                Left = labelSpecularTexture.Left + labelSpecularTexture.Width,
                Top = 10 + sliderSpecularPower.Top + sliderSpecularPower.Height,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
            };
            comboBoxSpecularTexture.Width = groupSpecular.Width - 10 - comboBoxSpecularTexture.Left;
            // Add textures name
            comboBoxSpecularTexture.Items.Add("No texture");
            comboBoxSpecularTexture.Items.AddRange(Texture.TexturesFilename);
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
            groupSpecular.Add(labelSpecularTexture);
            groupSpecular.Add(comboBoxSpecularTexture);

            #endregion

            #region Specular Texture Power Enabled

            var checkBoxSpecularTexturePowerEnabled = new CheckBox
            {
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
            groupSpecular.Add(checkBoxSpecularTexturePowerEnabled);

            #endregion

            groupSpecular.Height = checkBoxSpecularTexturePowerEnabled.Top + checkBoxSpecularTexturePowerEnabled.Height + 20;
            
            #endregion

            
            window.Height = groupSpecular.Top + groupSpecular.Height + 40;

        } // Show

    } // BlinnPhongWindow
} // XNAFinalEngine.Editor