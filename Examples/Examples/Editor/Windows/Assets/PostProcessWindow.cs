
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
    public static class PostProcessWindow
    {

        /// <summary>
        /// Creates and shows the configuration window of this asset.
        /// </summary>
        public static void Show(PostProcess asset)
        {

            #region Window

            var window = new Window
            {
                Text = asset.Name + " : Post Process"
            };
            UserInterfaceManager.Add(window);
            window.Closed += delegate { };

            #endregion

            #region Name

            var nameLabel = new Label
            {
                Parent = window,
                Text = "Name", Left = 10, Top = 10,
            };
            var nameTextBox = new TextBox
            {
                Parent = window,
                Width = window.ClientWidth - nameLabel.Width - 25,
                Text = asset.Name, Left = 60, Top = 10
            };
            nameTextBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    asset.Name = nameTextBox.Text;
                    window.Text = asset.Name + " : Post Process";
                }
            };
            nameTextBox.FocusLost += delegate
            {
                asset.Name = nameTextBox.Text;
                window.Text = asset.Name + " : Post Process";
            };

            #endregion

            #region Group Lens Exposure

            GroupBox groupLensExposure = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Height = 160,
                Left = 8,
                Top = nameLabel.Top + nameLabel.Height + 15,
                Text = "Lens Exposure",
                TextColor = Color.Gray,
            };

            #region Lens Exposure

            var sliderLensExposure = new SliderNumeric
            {
                Parent = groupLensExposure,
                Left = 10,
                Top = 25,
                Value = asset.LensExposure,
                Text = "Lens Exposure",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 5,
            };
            sliderLensExposure.ValueChanged += delegate { asset.LensExposure = sliderLensExposure.Value; };
            sliderLensExposure.Draw += delegate { sliderLensExposure.Value = asset.LensExposure; };

            #endregion

            groupLensExposure.Height = sliderLensExposure.Top + sliderLensExposure.Height + 5;

            #endregion

            #region Group Bloom

            GroupBox groupBloom = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Left = 8,
                Top = groupLensExposure.Top + groupLensExposure.Height + 15,
                Text = "Bloom",
                TextColor = Color.Gray,
            };

            #region Enabled

            CheckBox checkBoxBloomEnabled = new CheckBox
            {
                Parent = groupBloom,
                Left = 8,
                Top = 25,
                Width = groupBloom.ClientWidth - 16,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Checked = asset.Bloom.Enabled,
                Text = " Enabled",
                ToolTip = { Text = "The effect produces fringes (or feathers) of light around very bright objects in an image." }
            };
            checkBoxBloomEnabled.Draw += delegate { checkBoxBloomEnabled.Checked = asset.Bloom.Enabled; };

            #endregion

            #region Scale
            
            var sliderBloomScale = new SliderNumeric
            {
                Parent = groupBloom,
                Left = 10,
                Top = checkBoxBloomEnabled.Top + checkBoxBloomEnabled.Height + 5,
                Value = asset.Bloom.Scale,
                Text = "Scale",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 2,
            };
            sliderBloomScale.ValueChanged += delegate { asset.Bloom.Scale = sliderBloomScale.Value; };
            sliderBloomScale.Draw += delegate { sliderBloomScale.Value = asset.Bloom.Scale; };
            
            #endregion

            #region Threshold

            var sliderBloomThreshold = new SliderNumeric
            {
                Parent = groupBloom,
                Left = 10,
                Top = sliderBloomScale.Top + sliderBloomScale.Height + 5,
                Value = asset.Bloom.Threshold,
                Text = "Threshold",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 10,
            };
            sliderBloomThreshold.ValueChanged += delegate { asset.Bloom.Threshold = sliderBloomThreshold.Value; };
            sliderBloomThreshold.Draw += delegate { sliderBloomThreshold.Value = asset.Bloom.Threshold; };

            #endregion

            checkBoxBloomEnabled.CheckedChanged += delegate
            {
                asset.Bloom.Enabled = checkBoxBloomEnabled.Checked;
                sliderBloomScale.Enabled = asset.Bloom.Enabled;
                sliderBloomThreshold.Enabled = asset.Bloom.Enabled;
            };

            groupBloom.Height = sliderBloomThreshold.Top + sliderBloomThreshold.Height + 5;

            #endregion

            #region Group MLAA

            GroupBox groupMLAA = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Left = 8,
                Top = groupBloom.Top + groupBloom.Height + 15,
                Text = "MLAA",
                TextColor = Color.Gray,
            };

            #region Enabled

            CheckBox checkBoxMLAAEnabled = new CheckBox
            {
                Parent = groupMLAA,
                Left = 8,
                Top = 25,
                Width = groupMLAA.ClientWidth - 16,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Checked = asset.Bloom.Enabled,
                Text = " Enabled",
                ToolTip = { Text = "Enables Morphological Antialiasing." }
            };
            checkBoxMLAAEnabled.Draw += delegate { checkBoxMLAAEnabled.Checked = asset.MLAA.Enabled; };

            #endregion

            #region Edge Detection

            var labelEdgeDetection = new Label
            {
                Parent = groupMLAA,
                Left = 10,
                Top = 5 + checkBoxMLAAEnabled.Top + checkBoxMLAAEnabled.Height,
                Width = 155,
                Text = "Edge Detection Type"
            };
            var comboBoxEdgeDetection = new ComboBox
            {
                Parent = groupMLAA,
                Left = labelEdgeDetection.Left + labelEdgeDetection.Width,
                Top = 5 + checkBoxMLAAEnabled.Top + checkBoxMLAAEnabled.Height,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
                ToolTip = { Text = "Color: uses the color information. Good for texture and geometry aliasing. Depth: uses the depth buffer. Great for geometry aliasing. Both: the two at the same time. A little more costly with slightly better results." }
            };
            comboBoxEdgeDetection.Width = groupMLAA.Width - 10 - comboBoxEdgeDetection.Left;
            // Add textures name
            comboBoxEdgeDetection.Items.Add("Both");
            comboBoxEdgeDetection.Items.Add("Color");
            comboBoxEdgeDetection.Items.Add("Depth");
            // Events
            comboBoxEdgeDetection.ItemIndexChanged += delegate
            {
                switch (comboBoxEdgeDetection.ItemIndex)
                {
                    case 0:
                        asset.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Both;
                        break;
                    case 1:
                        asset.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color;
                        break;
                    case 2:
                        asset.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Depth;
                        break;
                }
            };
            comboBoxEdgeDetection.Draw += delegate
            {
                if (comboBoxEdgeDetection.ListBoxVisible)
                    return;
                switch (asset.MLAA.EdgeDetection)
                {
                    case MLAA.EdgeDetectionType.Both:
                        comboBoxEdgeDetection.ItemIndex = 0;
                        break;
                    case MLAA.EdgeDetectionType.Color:
                        comboBoxEdgeDetection.ItemIndex = 1;
                        break;
                    case MLAA.EdgeDetectionType.Depth:
                        comboBoxEdgeDetection.ItemIndex = 2;
                        break;
                }
            };

            #endregion

            #region Threshold Color

            var sliderMLAAColorThreshold = new SliderNumeric
            {
                Parent = groupMLAA,
                Left = 10,
                Top = comboBoxEdgeDetection.Top + comboBoxEdgeDetection.Height + 10,
                Value = asset.MLAA.ThresholdColor,
                Text = "Color Threshold",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0,
                MaximumValue = 0.5f,
            };
            sliderMLAAColorThreshold.ValueChanged += delegate { asset.MLAA.ThresholdColor = sliderMLAAColorThreshold.Value; };
            sliderMLAAColorThreshold.Draw += delegate { sliderMLAAColorThreshold.Value = asset.MLAA.ThresholdColor; };

            #endregion

            #region Threshold Depth

            var sliderMLAADepthThreshold = new SliderNumeric
            {
                Parent = groupMLAA,
                Left = 10,
                Top = sliderMLAAColorThreshold.Top + sliderMLAAColorThreshold.Height + 5,
                Value = asset.MLAA.ThresholdDepth,
                Text = "Depth Threshold",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0,
                MaximumValue = 0.5f,
            };
            sliderMLAADepthThreshold.ValueChanged += delegate { asset.MLAA.ThresholdDepth = sliderMLAADepthThreshold.Value; };
            sliderMLAADepthThreshold.Draw += delegate { sliderMLAADepthThreshold.Value = asset.MLAA.ThresholdDepth; };

            #endregion
            
            checkBoxMLAAEnabled.CheckedChanged += delegate
            {
                asset.MLAA.Enabled = checkBoxMLAAEnabled.Checked;
                comboBoxEdgeDetection.Enabled = asset.MLAA.Enabled;
                sliderMLAAColorThreshold.Enabled = asset.MLAA.Enabled;
                sliderMLAADepthThreshold.Enabled = asset.MLAA.Enabled;
            };

            groupMLAA.Height = sliderMLAADepthThreshold.Top + sliderMLAADepthThreshold.Height + 5;

            #endregion

            #region Group Film Grain

            GroupBox groupFilmGrain = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Left = 8,
                Top = groupMLAA.Top + groupMLAA.Height + 15,
                Text = "Film Grain",
                TextColor = Color.Gray,
            };

            #region Enabled

            CheckBox checkBoxFilmGrainEnabled = new CheckBox
            {
                Parent = groupFilmGrain,
                Left = 8,
                Top = 25,
                Width = groupFilmGrain.ClientWidth - 16,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Checked = asset.FilmGrain.Enabled,
                Text = " Enabled",
                ToolTip = { Text = "Is the random optical texture of processed photographic film." }
            };
            checkBoxFilmGrainEnabled.Draw += delegate { checkBoxFilmGrainEnabled.Checked = asset.FilmGrain.Enabled; };

            #endregion

            #region Strength

            var sliderFilmgrainStrength = new SliderNumeric
            {
                Parent = groupFilmGrain,
                Left = 10,
                Top = checkBoxFilmGrainEnabled.Top + checkBoxFilmGrainEnabled.Height + 5,
                Value = asset.FilmGrain.Strength,
                Text = "Strength",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = true,
                MinimumValue = 0,
                MaximumValue = 0.5f,
            };
            sliderFilmgrainStrength.ValueChanged += delegate { asset.FilmGrain.Strength = sliderFilmgrainStrength.Value; };
            sliderFilmgrainStrength.Draw += delegate { sliderFilmgrainStrength.Value = asset.FilmGrain.Strength; };

            #endregion

            #region Random Noise Strength

            var sliderFilmGrainRandomNoiseStrength = new SliderNumeric
            {
                Parent = groupFilmGrain,
                Left = 10,
                Top = sliderFilmgrainStrength.Top + sliderFilmgrainStrength.Height + 5,
                Value = asset.FilmGrain.RandomNoiseStrength,
                Text = "Random Noise Strength",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0,
                MaximumValue = 5f,
            };
            sliderFilmGrainRandomNoiseStrength.ValueChanged += delegate { asset.FilmGrain.RandomNoiseStrength = sliderFilmGrainRandomNoiseStrength.Value; };
            sliderFilmGrainRandomNoiseStrength.Draw += delegate { sliderFilmGrainRandomNoiseStrength.Value = asset.FilmGrain.RandomNoiseStrength; };

            #endregion

            #region Accentuate Dark Noise Power

            var sliderFilmGrainAccentuateDarkNoisePower = new SliderNumeric
            {
                Parent = groupFilmGrain,
                Left = 10,
                Top = sliderFilmGrainRandomNoiseStrength.Top + sliderFilmGrainRandomNoiseStrength.Height + 5,
                Value = asset.FilmGrain.AccentuateDarkNoisePower,
                Text = "Accentuate Dark Noise Power",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0,
                MaximumValue = 10f,
            };
            sliderFilmGrainAccentuateDarkNoisePower.ValueChanged += delegate { asset.FilmGrain.AccentuateDarkNoisePower = sliderFilmGrainAccentuateDarkNoisePower.Value; };
            sliderFilmGrainAccentuateDarkNoisePower.Draw += delegate { sliderFilmGrainAccentuateDarkNoisePower.Value = asset.FilmGrain.AccentuateDarkNoisePower; };

            #endregion

            checkBoxFilmGrainEnabled.CheckedChanged += delegate
            {
                asset.FilmGrain.Enabled = checkBoxFilmGrainEnabled.Checked;
                sliderFilmgrainStrength.Enabled = asset.FilmGrain.Enabled;
                sliderFilmGrainRandomNoiseStrength.Enabled = asset.FilmGrain.Enabled;
                sliderFilmGrainAccentuateDarkNoisePower.Enabled = asset.FilmGrain.Enabled;
            };

            groupFilmGrain.Height = sliderFilmGrainAccentuateDarkNoisePower.Top + sliderFilmGrainAccentuateDarkNoisePower.Height + 5;

            #endregion

            #region Group Adjust Levels

            GroupBox groupAdjustLevels = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Left = 8,
                Top = groupFilmGrain.Top + groupFilmGrain.Height + 15,
                Text = "Adjust Levels",
                TextColor = Color.Gray,
            };

            #region Enabled

            CheckBox checkBoxAdjustLevelsEnabled = new CheckBox
            {
                Parent = groupAdjustLevels,
                Left = 8,
                Top = 25,
                Width = groupAdjustLevels.ClientWidth - 16,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Checked = asset.AdjustLevels.Enabled,
                Text = " Enabled",
                ToolTip = { Text = "Adjust color levels just like Photoshop." }
            };
            checkBoxAdjustLevelsEnabled.Draw += delegate { checkBoxAdjustLevelsEnabled.Checked = asset.AdjustLevels.Enabled; };

            #endregion

            #region Input Black

            var sliderAdjustLevelsInputBlack = new SliderNumeric
            {
                Parent = groupAdjustLevels,
                Left = 10,
                Top = checkBoxAdjustLevelsEnabled.Top + checkBoxAdjustLevelsEnabled.Height + 5,
                Value = asset.AdjustLevels.InputBlack,
                Text = "Input Black",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0,
                MaximumValue = 0.9f,
            };
            sliderAdjustLevelsInputBlack.ValueChanged += delegate { asset.AdjustLevels.InputBlack = sliderAdjustLevelsInputBlack.Value; };
            sliderAdjustLevelsInputBlack.Draw += delegate { sliderAdjustLevelsInputBlack.Value = asset.AdjustLevels.InputBlack; };

            #endregion

            #region Input White

            var sliderAdjustLevelsInputWhite = new SliderNumeric
            {
                Parent = groupAdjustLevels,
                Left = 10,
                Top = sliderAdjustLevelsInputBlack.Top + sliderAdjustLevelsInputBlack.Height + 5,
                Value = asset.AdjustLevels.InputWhite,
                Text = "Input White",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0.1f,
                MaximumValue = 1f,
            };
            sliderAdjustLevelsInputWhite.ValueChanged += delegate { asset.AdjustLevels.InputWhite = sliderAdjustLevelsInputWhite.Value; };
            sliderAdjustLevelsInputWhite.Draw += delegate { sliderAdjustLevelsInputWhite.Value = asset.AdjustLevels.InputWhite; };

            #endregion

            #region Input Gamma

            var sliderAdjustLevelsInputGamma = new SliderNumeric
            {
                Parent = groupAdjustLevels,
                Left = 10,
                Top = sliderAdjustLevelsInputWhite.Top + sliderAdjustLevelsInputWhite.Height + 5,
                Value = asset.AdjustLevels.InputGamma,
                Text = "Input Gamma",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0.01f,
                MaximumValue = 9.99f,
            };
            sliderAdjustLevelsInputGamma.ValueChanged += delegate { asset.AdjustLevels.InputGamma = sliderAdjustLevelsInputGamma.Value; };
            sliderAdjustLevelsInputGamma.Draw += delegate { sliderAdjustLevelsInputGamma.Value = asset.AdjustLevels.InputGamma; };

            #endregion

            #region Input Black

            var sliderAdjustLevelsOutputBlack = new SliderNumeric
            {
                Parent = groupAdjustLevels,
                Left = 10,
                Top = sliderAdjustLevelsInputGamma.Top + sliderAdjustLevelsInputGamma.Height + 5,
                Value = asset.AdjustLevels.OutputBlack,
                Text = "Output Black",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0,
                MaximumValue = 1f,
            };
            sliderAdjustLevelsOutputBlack.ValueChanged += delegate { asset.AdjustLevels.OutputBlack = sliderAdjustLevelsOutputBlack.Value; };
            sliderAdjustLevelsOutputBlack.Draw += delegate { sliderAdjustLevelsOutputBlack.Value = asset.AdjustLevels.OutputBlack; };

            #endregion

            #region Input White

            var sliderAdjustLevelsOutputWhite = new SliderNumeric
            {
                Parent = groupAdjustLevels,
                Left = 10,
                Top = sliderAdjustLevelsOutputBlack.Top + sliderAdjustLevelsOutputBlack.Height + 5,
                Value = asset.AdjustLevels.OutputWhite,
                Text = "Output White",
                IfOutOfRangeRescale = false,
                ValueCanBeOutOfRange = false,
                MinimumValue = 0f,
                MaximumValue = 1f,
            };
            sliderAdjustLevelsOutputWhite.ValueChanged += delegate { asset.AdjustLevels.OutputWhite = sliderAdjustLevelsOutputWhite.Value; };
            sliderAdjustLevelsOutputWhite.Draw += delegate { sliderAdjustLevelsOutputWhite.Value = asset.AdjustLevels.OutputWhite; };

            #endregion

            checkBoxAdjustLevelsEnabled.CheckedChanged += delegate
            {
                asset.AdjustLevels.Enabled = checkBoxAdjustLevelsEnabled.Checked;
                sliderAdjustLevelsInputBlack.Enabled = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsInputWhite.Enabled = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsInputGamma.Enabled = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsOutputBlack.Enabled = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsOutputWhite.Enabled = asset.AdjustLevels.Enabled;
            };

            groupAdjustLevels.Height = sliderAdjustLevelsOutputWhite.Top + sliderAdjustLevelsOutputWhite.Height + 5;

            #endregion

            #region Group Color Correction

            GroupBox groupColorCorrection = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Left = 8,
                Top = groupAdjustLevels.Top + groupAdjustLevels.Height + 15,
                Text = "Color Correction",
                TextColor = Color.Gray,
            };

            #region Enabled

            CheckBox checkBoxColorCorrectionEnabled = new CheckBox
            {
                Parent = groupColorCorrection,
                Left = 8,
                Top = 25,
                Width = groupColorCorrection.ClientWidth - 16,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Checked = asset.ColorCorrection.Enabled,
                Text = " Enabled",
            };
            checkBoxColorCorrectionEnabled.Draw += delegate { checkBoxColorCorrectionEnabled.Checked = asset.ColorCorrection.Enabled; };

            #endregion

            #region First Lookup Table

            var labelFirstLookupTable = new Label
            {
                Parent = groupColorCorrection,
                Left = 10,
                Top = 10 + checkBoxColorCorrectionEnabled.Top + checkBoxColorCorrectionEnabled.Height,
                Width = 150,
                Text = "First Lookup Table"
            };
            var comboBoxFirstLookupTable = new ComboBox
            {
                Parent = groupColorCorrection,
                Left = labelFirstLookupTable.Left + labelFirstLookupTable.Width,
                Top = labelFirstLookupTable.Top,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
            };
            comboBoxFirstLookupTable.Width = groupColorCorrection.Width - 10 - comboBoxFirstLookupTable.Left;
            // Events
            comboBoxFirstLookupTable.ItemIndexChanged += delegate
            {
                if (comboBoxFirstLookupTable.ItemIndex <= 0)
                    asset.ColorCorrection.FirstLookupTable = null;
                else
                {
                    // If we have to change the asset...
                    if (asset.ColorCorrection.FirstLookupTable == null ||
                        asset.ColorCorrection.FirstLookupTable.Name != (string)comboBoxFirstLookupTable.Items[comboBoxFirstLookupTable.ItemIndex])
                    {
                        asset.ColorCorrection.FirstLookupTable = LookupTable.LoadedLookupTables[comboBoxFirstLookupTable.ItemIndex - 1]; // The first item is the no texture item.
                    }
                }
            };
            comboBoxFirstLookupTable.Draw += delegate
            {
                // Add textures name here because someone could dispose or add new lookup tables.
                comboBoxFirstLookupTable.Items.Clear();
                comboBoxFirstLookupTable.Items.Add("No texture");
                foreach (LookupTable lookupTable in LookupTable.LoadedLookupTables)
                    comboBoxFirstLookupTable.Items.Add(lookupTable.Name);

                if (comboBoxFirstLookupTable.ListBoxVisible)
                    return;
                // Identify current index
                if (asset.ColorCorrection.FirstLookupTable == null)
                    comboBoxFirstLookupTable.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < comboBoxFirstLookupTable.Items.Count; i++)
                        if ((string)comboBoxFirstLookupTable.Items[i] == asset.ColorCorrection.FirstLookupTable.Name)
                        {
                            comboBoxFirstLookupTable.ItemIndex = i;
                            break;
                        }
                }
            };
            var buttonFirstLookupTable = new Button
            {
                Parent = groupColorCorrection,
                Anchor = Anchors.Left | Anchors.Top,
                Top = labelFirstLookupTable.Top,
                Left = 140,
                Width = 15,
                Height = 20,
                Text = "+"
            };

            #endregion

            checkBoxColorCorrectionEnabled.CheckedChanged += delegate
            {
                asset.ColorCorrection.Enabled = checkBoxColorCorrectionEnabled.Checked;
                labelFirstLookupTable.Enabled = asset.ColorCorrection.Enabled;
                comboBoxFirstLookupTable.Enabled = asset.ColorCorrection.Enabled;
                buttonFirstLookupTable.Enabled = asset.ColorCorrection.Enabled;
            };

            groupColorCorrection.Height = buttonFirstLookupTable.Top + buttonFirstLookupTable.Height + 15; // Use an offset of 15 instead of 5 for the last one.

            #endregion

            window.Height = 500;

        } // Show

    } // PostProcessWindow
} // XNAFinalEngine.Editor