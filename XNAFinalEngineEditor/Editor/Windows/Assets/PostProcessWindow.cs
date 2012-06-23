
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.UserInterface;
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

            var window = new AssetWindow
            {
                AssetName = asset.Name,
                AssetType = "Post Process"
            };
            window.AssetNameChanged += delegate
            {
                asset.Name = window.AssetName;
                window.AssetName = asset.Name; // If the new name is not unique
            };
            window.Draw += delegate { window.AssetName = asset.Name; };

            #endregion

            #region Group Lens Exposure

            GroupBox groupToneMapping = CommonControls.Group("Tone Mapping", window);
            
            #region Lens Exposure

            var sliderLensExposure = CommonControls.SliderNumeric("Lens Exposure", groupToneMapping, asset.ToneMapping.LensExposure, true, true, -10, 10);
            sliderLensExposure.ValueChanged += delegate { asset.ToneMapping.LensExposure = sliderLensExposure.Value; };
            sliderLensExposure.Draw += delegate { sliderLensExposure.Value = asset.ToneMapping.LensExposure; };

            #endregion

            #region Auto Exposure Enabled

            CheckBox checkBoxAutoExposureEnabled = CommonControls.CheckBox("Auto Exposure Enabled", groupToneMapping, asset.ToneMapping.AutoExposureEnabled);
            checkBoxAutoExposureEnabled.Draw += delegate { checkBoxAutoExposureEnabled.Checked = asset.ToneMapping.AutoExposureEnabled; };

            #endregion

            #region Auto Exposure Adaptation Time Multiplier

            var sliderAutoExposureAdaptationTimeMultiplier = CommonControls.SliderNumeric("Adaptation Time Multiplier", groupToneMapping, asset.ToneMapping.AutoExposureAdaptationTimeMultiplier, false, true, 0, 10);
            sliderAutoExposureAdaptationTimeMultiplier.ValueChanged += delegate { asset.ToneMapping.AutoExposureAdaptationTimeMultiplier = sliderAutoExposureAdaptationTimeMultiplier.Value; };
            sliderAutoExposureAdaptationTimeMultiplier.Draw += delegate { sliderAutoExposureAdaptationTimeMultiplier.Value = asset.ToneMapping.AutoExposureAdaptationTimeMultiplier; };

            #endregion

            #region Auto Exposure Luminance Low Threshold

            var sliderAutoExposureLuminanceLowThreshold = CommonControls.SliderNumeric("Luminance Low Threshold", groupToneMapping, asset.ToneMapping.AutoExposureLuminanceLowThreshold, false, true, 0, 0.5f);
            sliderAutoExposureLuminanceLowThreshold.ValueChanged += delegate { asset.ToneMapping.AutoExposureLuminanceLowThreshold = sliderAutoExposureLuminanceLowThreshold.Value; };
            sliderAutoExposureLuminanceLowThreshold.Draw += delegate { sliderAutoExposureLuminanceLowThreshold.Value = asset.ToneMapping.AutoExposureLuminanceLowThreshold; };

            #endregion

            #region Auto Exposure Luminance High Threshold

            var sliderAutoExposureLuminanceHighThreshold = CommonControls.SliderNumeric("Luminance High Threshold", groupToneMapping, asset.ToneMapping.AutoExposureLuminanceHighThreshold, false, true, 0.5f, 20f);
            sliderAutoExposureLuminanceHighThreshold.ValueChanged += delegate { asset.ToneMapping.AutoExposureLuminanceHighThreshold = sliderAutoExposureLuminanceHighThreshold.Value; };
            sliderAutoExposureLuminanceHighThreshold.Draw += delegate { sliderAutoExposureLuminanceHighThreshold.Value = asset.ToneMapping.AutoExposureLuminanceHighThreshold; };

            #endregion

            checkBoxAutoExposureEnabled.CheckedChanged += delegate
            {
                asset.ToneMapping.AutoExposureEnabled = checkBoxAutoExposureEnabled.Checked;
                sliderLensExposure.Enabled = !asset.ToneMapping.AutoExposureEnabled;
                sliderAutoExposureAdaptationTimeMultiplier.Enabled = asset.ToneMapping.AutoExposureEnabled;
                sliderAutoExposureLuminanceLowThreshold.Enabled = asset.ToneMapping.AutoExposureEnabled;
                sliderAutoExposureLuminanceHighThreshold.Enabled = asset.ToneMapping.AutoExposureEnabled;
            };

            #region Tone Mapping Curve

            ComboBox comboBoxToneMappingCurve = CommonControls.ComboBox("Tone Mapping Curve", groupToneMapping);
            comboBoxToneMappingCurve.Items.AddRange(new[] { "Filmic ALU", "Filmic Uncharted 2", "Duiker", "Reinhard", "Reinhard Modified", "Exponential", "Logarithmic", "Drago Logarithmic" });
            comboBoxToneMappingCurve.ItemIndex = (int)asset.ToneMapping.ToneMappingFunction;
            comboBoxToneMappingCurve.ItemIndexChanged += delegate { asset.ToneMapping.ToneMappingFunction = (ToneMapping.ToneMappingFunctionEnumerate)comboBoxToneMappingCurve.ItemIndex; };
            comboBoxToneMappingCurve.Draw += delegate
            {
                if (comboBoxToneMappingCurve.ListBoxVisible)
                    return;
                comboBoxToneMappingCurve.ItemIndex = (int)asset.ToneMapping.ToneMappingFunction;
            };

            #endregion

            #region White Level

            var sliderWhiteLevel = CommonControls.SliderNumeric("White Level", groupToneMapping, asset.ToneMapping.ToneMappingWhiteLevel, false, true, 0f, 50f);
            sliderWhiteLevel.ValueChanged += delegate { asset.ToneMapping.ToneMappingWhiteLevel = sliderWhiteLevel.Value; };
            sliderWhiteLevel.Draw += delegate { sliderWhiteLevel.Value = asset.ToneMapping.ToneMappingWhiteLevel; };

            #endregion

            #region Luminance Saturation

            var sliderLuminanceSaturation = CommonControls.SliderNumeric("Luminance Saturation", groupToneMapping, asset.ToneMapping.ToneMappingLuminanceSaturation, false, true, 0f, 2f);
            sliderLuminanceSaturation.ValueChanged += delegate { asset.ToneMapping.ToneMappingLuminanceSaturation = sliderLuminanceSaturation.Value; };
            sliderLuminanceSaturation.Draw += delegate { sliderLuminanceSaturation.Value = asset.ToneMapping.ToneMappingLuminanceSaturation; };

            #endregion

            #region Drago Bias

            var sliderDragoBias = CommonControls.SliderNumeric("Drago Bias", groupToneMapping, asset.ToneMapping.DragoBias, false, true, 0f, 1f);
            sliderDragoBias.ValueChanged += delegate { asset.ToneMapping.DragoBias = sliderDragoBias.Value; };
            sliderDragoBias.Draw += delegate { sliderDragoBias.Value = asset.ToneMapping.DragoBias; };

            #endregion

            #region Shoulder Strength

            var sliderShoulderStrength = CommonControls.SliderNumeric("Shoulder Strength", groupToneMapping, asset.ToneMapping.Uncharted2ShoulderStrength, false, true, 0f, 1f);
            sliderShoulderStrength.ValueChanged += delegate { asset.ToneMapping.Uncharted2ShoulderStrength = sliderShoulderStrength.Value; };
            sliderShoulderStrength.Draw += delegate { sliderShoulderStrength.Value = asset.ToneMapping.Uncharted2ShoulderStrength; };

            #endregion

            #region Linear Strength

            var sliderLinearStrength = CommonControls.SliderNumeric("Linear Strength", groupToneMapping, asset.ToneMapping.Uncharted2LinearStrength, false, true, 0f, 1f);
            sliderLinearStrength.ValueChanged += delegate { asset.ToneMapping.Uncharted2LinearStrength = sliderLinearStrength.Value; };
            sliderLinearStrength.Draw += delegate { sliderLinearStrength.Value = asset.ToneMapping.Uncharted2LinearStrength; };

            #endregion

            #region Linear Angle

            var sliderLinearAngle = CommonControls.SliderNumeric("Linear Angle", groupToneMapping, asset.ToneMapping.Uncharted2LinearAngle, false, true, 0f, 3f);
            sliderLinearAngle.ValueChanged += delegate { asset.ToneMapping.Uncharted2LinearAngle = sliderLinearAngle.Value; };
            sliderLinearAngle.Draw += delegate { sliderLinearAngle.Value = asset.ToneMapping.Uncharted2LinearAngle; };

            #endregion

            #region Toe Strength

            var sliderToeStrength = CommonControls.SliderNumeric("Toe Strength", groupToneMapping, asset.ToneMapping.Uncharted2ToeStrength, false, true, 0f, 1f);
            sliderToeStrength.ValueChanged += delegate { asset.ToneMapping.Uncharted2ToeStrength = sliderToeStrength.Value; };
            sliderToeStrength.Draw += delegate { sliderToeStrength.Value = asset.ToneMapping.Uncharted2ToeStrength; };

            #endregion

            #region Toe Numerator

            var sliderToeNumerator = CommonControls.SliderNumeric("Toe Numerator", groupToneMapping, asset.ToneMapping.Uncharted2ToeNumerator, false, true, 0f, 0.1f);
            sliderToeNumerator.ValueChanged += delegate { asset.ToneMapping.Uncharted2ToeNumerator = sliderToeNumerator.Value; };
            sliderToeNumerator.Draw += delegate { sliderToeNumerator.Value = asset.ToneMapping.Uncharted2ToeNumerator; };

            #endregion

            #region Toe Denominator

            var sliderToeDenominator = CommonControls.SliderNumeric("Toe Denominator", groupToneMapping, asset.ToneMapping.Uncharted2ToeDenominator, false, true, 0f, 1f);
            sliderToeDenominator.ValueChanged += delegate { asset.ToneMapping.Uncharted2ToeDenominator = sliderToeDenominator.Value; };
            sliderToeDenominator.Draw += delegate { sliderToeDenominator.Value = asset.ToneMapping.Uncharted2ToeDenominator; };

            #endregion

            #region Linear White

            var sliderLinearWhite = CommonControls.SliderNumeric("Linear White", groupToneMapping, asset.ToneMapping.Uncharted2LinearWhite, false, true, 0f, 40f);
            sliderLinearWhite.ValueChanged += delegate { asset.ToneMapping.Uncharted2LinearWhite = sliderLinearWhite.Value; };
            sliderLinearWhite.Draw += delegate { sliderLinearWhite.Value = asset.ToneMapping.Uncharted2LinearWhite; };

            #endregion

            #region Sliders enabled?

            comboBoxToneMappingCurve.ItemIndexChanged += delegate
            {
                sliderWhiteLevel.Enabled = false;
                sliderLuminanceSaturation.Enabled = false;
                sliderDragoBias.Enabled = false;
                sliderShoulderStrength.Enabled = false;
                sliderLinearStrength.Enabled = false;
                sliderLinearAngle.Enabled = false;
                sliderToeStrength.Enabled = false;
                sliderToeNumerator.Enabled = false;
                sliderToeDenominator.Enabled = false;
                sliderLinearWhite.Enabled = false;
                if (asset.ToneMapping.ToneMappingFunction == ToneMapping.ToneMappingFunctionEnumerate.DragoLogarithmic)
                {
                    sliderWhiteLevel.Enabled = true;
                    sliderLuminanceSaturation.Enabled = true;
                    sliderDragoBias.Enabled = true;
                }
                else if (asset.ToneMapping.ToneMappingFunction == ToneMapping.ToneMappingFunctionEnumerate.Exponential)
                {
                    sliderWhiteLevel.Enabled = true;
                    sliderLuminanceSaturation.Enabled = true;
                }
                else if (asset.ToneMapping.ToneMappingFunction == ToneMapping.ToneMappingFunctionEnumerate.FilmicUncharted2)
                {
                    sliderShoulderStrength.Enabled = true;
                    sliderLinearStrength.Enabled = true;
                    sliderLinearAngle.Enabled = true;
                    sliderToeStrength.Enabled = true;
                    sliderToeNumerator.Enabled = true;
                    sliderToeDenominator.Enabled = true;
                    sliderLinearWhite.Enabled = true;
                }
                else if (asset.ToneMapping.ToneMappingFunction == ToneMapping.ToneMappingFunctionEnumerate.Logarithmic)
                {
                    sliderWhiteLevel.Enabled = true;
                    sliderLuminanceSaturation.Enabled = true;
                }
                else if (asset.ToneMapping.ToneMappingFunction == ToneMapping.ToneMappingFunctionEnumerate.Reinhard)
                {
                    sliderLuminanceSaturation.Enabled = true;
                }
                else if (asset.ToneMapping.ToneMappingFunction == ToneMapping.ToneMappingFunctionEnumerate.ReinhardModified)
                {
                    sliderWhiteLevel.Enabled = true;
                    sliderLuminanceSaturation.Enabled = true;
                }
            };

            #endregion

            groupToneMapping.AdjustHeightFromChildren();

            #endregion

            #region Group Bloom

            GroupBox groupBloom = CommonControls.Group("Bloom", window);

            #region Enabled

            CheckBox checkBoxBloomEnabled = CommonControls.CheckBox("Enabled", groupBloom, asset.Bloom.Enabled, "The effect produces fringes (or feathers) of light around very bright objects in an image.");
            checkBoxBloomEnabled.Draw += delegate { checkBoxBloomEnabled.Checked = asset.Bloom.Enabled; };
            
            #endregion

            #region Scale
            
            var sliderBloomScale = CommonControls.SliderNumeric("Scale", groupBloom, asset.Bloom.Scale, false, true, 0, 2);
            sliderBloomScale.ValueChanged += delegate { asset.Bloom.Scale = sliderBloomScale.Value; };
            sliderBloomScale.Draw += delegate { sliderBloomScale.Value = asset.Bloom.Scale; };
            
            #endregion

            #region Threshold

            var sliderBloomThreshold = CommonControls.SliderNumeric("Threshold", groupBloom, asset.Bloom.Threshold, false, true, 0, 10);
            sliderBloomThreshold.ValueChanged += delegate { asset.Bloom.Threshold = sliderBloomThreshold.Value; };
            sliderBloomThreshold.Draw += delegate { sliderBloomThreshold.Value = asset.Bloom.Threshold; };

            #endregion

            checkBoxBloomEnabled.CheckedChanged += delegate
            {
                asset.Bloom.Enabled = checkBoxBloomEnabled.Checked;
                sliderBloomScale.Enabled = asset.Bloom.Enabled;
                sliderBloomThreshold.Enabled = asset.Bloom.Enabled;
            };

            groupBloom.AdjustHeightFromChildren();

            #endregion

            #region Group MLAA

            GroupBox groupMlaa = CommonControls.Group("MLAA", window);

            #region Enabled

            CheckBox checkBoxMlaaEnabled = CommonControls.CheckBox("Enabled", groupMlaa, asset.MLAA.Enabled, "Enables Morphological Antialiasing.");
            checkBoxMlaaEnabled.Draw += delegate { checkBoxMlaaEnabled.Checked = asset.MLAA.Enabled; };

            #endregion

            #region Edge Detection

            var comboBoxEdgeDetection = CommonControls.ComboBox("Edge Detection Type", groupMlaa,
                                                                "Color: uses the color information. Good for texture and geometry aliasing. Depth: uses the depth buffer. Great for geometry aliasing. Both: the two at the same time. A little more costly with slightly better results.");
            // Add textures name
            comboBoxEdgeDetection.Items.Add("Both");
            comboBoxEdgeDetection.Items.Add("Color");
            comboBoxEdgeDetection.Items.Add("Depth");
            // Events
            comboBoxEdgeDetection.ItemIndexChanged += delegate
            {
                switch (comboBoxEdgeDetection.ItemIndex)
                {
                    case 0: asset.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Both;  break;
                    case 1: asset.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color; break;
                    case 2: asset.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Depth; break;
                }
            };
            comboBoxEdgeDetection.Draw += delegate
            {
                if (comboBoxEdgeDetection.ListBoxVisible)
                    return;
                switch (asset.MLAA.EdgeDetection)
                {
                    case MLAA.EdgeDetectionType.Both:  comboBoxEdgeDetection.ItemIndex = 0; break;
                    case MLAA.EdgeDetectionType.Color: comboBoxEdgeDetection.ItemIndex = 1; break;
                    case MLAA.EdgeDetectionType.Depth: comboBoxEdgeDetection.ItemIndex = 2; break;
                }
            };

            #endregion
            
            #region Threshold Color

            var sliderMlaaColorThreshold = CommonControls.SliderNumeric("Color Threshold", groupMlaa, asset.MLAA.ThresholdColor, false, false, 0, 0.5f);
            sliderMlaaColorThreshold.ValueChanged += delegate { asset.MLAA.ThresholdColor = sliderMlaaColorThreshold.Value; };
            sliderMlaaColorThreshold.Draw += delegate { sliderMlaaColorThreshold.Value = asset.MLAA.ThresholdColor; };

            #endregion

            #region Threshold Depth

            var sliderMlaaDepthThreshold = CommonControls.SliderNumeric("Depth Threshold", groupMlaa, asset.MLAA.ThresholdDepth, false, false, 0, 0.5f);
            sliderMlaaDepthThreshold.ValueChanged += delegate { asset.MLAA.ThresholdDepth = sliderMlaaDepthThreshold.Value; };
            sliderMlaaDepthThreshold.Draw += delegate { sliderMlaaDepthThreshold.Value = asset.MLAA.ThresholdDepth; };

            #endregion
            
            checkBoxMlaaEnabled.CheckedChanged += delegate
            {
                asset.MLAA.Enabled = checkBoxMlaaEnabled.Checked;
                comboBoxEdgeDetection.Enabled = asset.MLAA.Enabled;
                sliderMlaaColorThreshold.Enabled = asset.MLAA.Enabled;
                sliderMlaaDepthThreshold.Enabled = asset.MLAA.Enabled;
            };

            groupMlaa.AdjustHeightFromChildren();
            
            #endregion

            #region Group Film Grain

            GroupBox groupFilmGrain = CommonControls.Group("Film Grain", window);

            #region Enabled

            CheckBox checkBoxFilmGrainEnabled = CommonControls.CheckBox("Enabled", groupFilmGrain, asset.FilmGrain.Enabled, "Is the random optical texture of processed photographic film.");
            checkBoxFilmGrainEnabled.Draw += delegate { checkBoxFilmGrainEnabled.Checked = asset.FilmGrain.Enabled; };
            
            #endregion

            #region Strength

            var sliderFilmgrainStrength = CommonControls.SliderNumeric("Strength", groupFilmGrain, asset.FilmGrain.Strength, false, true, 0, 0.5f);
            sliderFilmgrainStrength.ValueChanged += delegate { asset.FilmGrain.Strength = sliderFilmgrainStrength.Value; };
            sliderFilmgrainStrength.Draw += delegate { sliderFilmgrainStrength.Value = asset.FilmGrain.Strength; };

            #endregion

            #region Random Noise Strength

            var sliderFilmGrainRandomNoiseStrength = CommonControls.SliderNumeric("Random Noise Strength", groupFilmGrain, asset.FilmGrain.RandomNoiseStrength, false, false, 0, 5);
            sliderFilmGrainRandomNoiseStrength.ValueChanged += delegate { asset.FilmGrain.RandomNoiseStrength = sliderFilmGrainRandomNoiseStrength.Value; };
            sliderFilmGrainRandomNoiseStrength.Draw += delegate { sliderFilmGrainRandomNoiseStrength.Value = asset.FilmGrain.RandomNoiseStrength; };

            #endregion

            #region Accentuate Dark Noise Power

            var sliderFilmGrainAccentuateDarkNoisePower = CommonControls.SliderNumeric("Accentuate Dark Noise Power", groupFilmGrain, asset.FilmGrain.AccentuateDarkNoisePower, false, false, 0, 10);
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

            groupFilmGrain.AdjustHeightFromChildren();

            #endregion

            #region Group Adjust Levels

            GroupBox groupAdjustLevels = CommonControls.Group("Adjust Levels", window);

            #region Enabled

            CheckBox checkBoxAdjustLevelsEnabled = CommonControls.CheckBox("Enabled", groupAdjustLevels, asset.AdjustLevels.Enabled, "Adjust color levels just like Photoshop.");
            checkBoxAdjustLevelsEnabled.Draw += delegate { checkBoxAdjustLevelsEnabled.Checked = asset.AdjustLevels.Enabled; };

            #endregion

            #region Input Black

            var sliderAdjustLevelsInputBlack = CommonControls.SliderNumeric("Input Black", groupAdjustLevels, asset.AdjustLevels.InputBlack, false, false, 0, 0.9f);
            sliderAdjustLevelsInputBlack.ValueChanged += delegate { asset.AdjustLevels.InputBlack = sliderAdjustLevelsInputBlack.Value; };
            sliderAdjustLevelsInputBlack.Draw += delegate { sliderAdjustLevelsInputBlack.Value = asset.AdjustLevels.InputBlack; };

            #endregion

            #region Input White

            var sliderAdjustLevelsInputWhite = CommonControls.SliderNumeric("Input White", groupAdjustLevels, asset.AdjustLevels.InputWhite, false, false, 0.1f, 1f);
            sliderAdjustLevelsInputWhite.ValueChanged += delegate { asset.AdjustLevels.InputWhite = sliderAdjustLevelsInputWhite.Value; };
            sliderAdjustLevelsInputWhite.Draw += delegate { sliderAdjustLevelsInputWhite.Value = asset.AdjustLevels.InputWhite; };

            #endregion

            #region Input Gamma

            var sliderAdjustLevelsInputGamma = CommonControls.SliderNumeric("Input Gamma", groupAdjustLevels, asset.AdjustLevels.InputGamma, false, false, 0.01f, 9.99f);
            sliderAdjustLevelsInputGamma.ValueChanged += delegate { asset.AdjustLevels.InputGamma = sliderAdjustLevelsInputGamma.Value; };
            sliderAdjustLevelsInputGamma.Draw += delegate { sliderAdjustLevelsInputGamma.Value = asset.AdjustLevels.InputGamma; };

            #endregion

            #region Output Black

            var sliderAdjustLevelsOutputBlack = CommonControls.SliderNumeric("Output Black", groupAdjustLevels, asset.AdjustLevels.OutputBlack, false, false, 0, 1);
            sliderAdjustLevelsOutputBlack.ValueChanged += delegate { asset.AdjustLevels.OutputBlack = sliderAdjustLevelsOutputBlack.Value; };
            sliderAdjustLevelsOutputBlack.Draw += delegate { sliderAdjustLevelsOutputBlack.Value = asset.AdjustLevels.OutputBlack; };

            #endregion

            #region Output White

            var sliderAdjustLevelsOutputWhite = CommonControls.SliderNumeric("Output White", groupAdjustLevels, asset.AdjustLevels.OutputWhite, false, false, 0, 1);
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

            groupAdjustLevels.AdjustHeightFromChildren();

            #endregion

            #region Group Color Correction

            GroupBox groupColorCorrection = CommonControls.Group("Color Correction", window);

            #region Enabled

            CheckBox checkBoxColorCorrectionEnabled = CommonControls.CheckBox("Enabled", groupColorCorrection, asset.ColorCorrection.Enabled);
            checkBoxColorCorrectionEnabled.Draw += delegate { checkBoxColorCorrectionEnabled.Checked = asset.ColorCorrection.Enabled; };

            #endregion

            #region First Lookup Table

            var assetCreatorFirstLookupTable = CommonControls.AssetSelector("First Lookup Table", groupColorCorrection);
            assetCreatorFirstLookupTable.AssetAdded += delegate
            {
                LookupTableWindow.CurrentCreatedAssetChanged += delegate
                {
                    asset.ColorCorrection.FirstLookupTable = LookupTableWindow.CurrentCreatedAsset;
                    window.Invalidate();
                };
                LookupTableWindow.Show(null);
            };
            assetCreatorFirstLookupTable.AssetEdited += delegate
            {
                LookupTableWindow.Show(asset.ColorCorrection.FirstLookupTable);
            };
            // Events
            assetCreatorFirstLookupTable.ItemIndexChanged += delegate
            {
                if (assetCreatorFirstLookupTable.ItemIndex <= 0)
                    asset.ColorCorrection.FirstLookupTable = null;
                else
                {
                    foreach (Asset texture in Asset.LoadedAssets)
                    {
                        if (texture is LookupTable && texture.Name == (string)assetCreatorFirstLookupTable.Items[assetCreatorFirstLookupTable.ItemIndex])
                            asset.ColorCorrection.FirstLookupTable = (LookupTable)texture;
                    }
                }
                assetCreatorFirstLookupTable.EditButtonEnabled = asset.ColorCorrection.FirstLookupTable != null;
            };
            assetCreatorFirstLookupTable.Draw += delegate
            {
                // Add textures name here because someone could dispose or add new lookup tables.
                assetCreatorFirstLookupTable.Items.Clear();
                assetCreatorFirstLookupTable.Items.Add("No texture");
                foreach (Asset texture in Asset.LoadedAssets)
                    assetCreatorFirstLookupTable.Items.Add(texture.Name);

                if (assetCreatorFirstLookupTable.ListBoxVisible)
                    return;
                // Identify current index
                if (asset.ColorCorrection.FirstLookupTable == null)
                    assetCreatorFirstLookupTable.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < assetCreatorFirstLookupTable.Items.Count; i++)
                        if ((string)assetCreatorFirstLookupTable.Items[i] == asset.ColorCorrection.FirstLookupTable.Name)
                        {
                            assetCreatorFirstLookupTable.ItemIndex = i;
                            break;
                        }
                }
            };

            #endregion

            #region Second Lookup Table

            var assetCreatorSecondLookupTable = CommonControls.AssetSelector("Second Lookup Table", groupColorCorrection);
            // When the add button is pressed.
            assetCreatorSecondLookupTable.AssetAdded += delegate
            {
                LookupTableWindow.CurrentCreatedAssetChanged += delegate
                {
                    asset.ColorCorrection.SecondLookupTable = LookupTableWindow.CurrentCreatedAsset;
                    window.Invalidate();
                };
                LookupTableWindow.Show(null);
            };
            // When the edit button is pressed.
            assetCreatorSecondLookupTable.AssetEdited += delegate
            {
                LookupTableWindow.Show(asset.ColorCorrection.SecondLookupTable);
            };
            // Events
            assetCreatorSecondLookupTable.ItemIndexChanged += delegate
            {
                if (assetCreatorSecondLookupTable.ItemIndex <= 0)
                    asset.ColorCorrection.SecondLookupTable = null;
                else
                {
                    foreach (Asset texture in Asset.LoadedAssets)
                    {
                        if (texture.Name == (string)assetCreatorSecondLookupTable.Items[assetCreatorSecondLookupTable.ItemIndex])
                            asset.ColorCorrection.SecondLookupTable = (LookupTable)texture;
                    }
                }
                assetCreatorSecondLookupTable.EditButtonEnabled = asset.ColorCorrection.SecondLookupTable != null;
            };
            assetCreatorSecondLookupTable.Draw += delegate
            {
                assetCreatorSecondLookupTable.Enabled = asset.ColorCorrection.FirstLookupTable != null && checkBoxColorCorrectionEnabled.Checked;
                // Add textures name here because someone could dispose or add new lookup tables.
                assetCreatorSecondLookupTable.Items.Clear();
                assetCreatorSecondLookupTable.Items.Add("No texture");
                foreach (Asset texture in Asset.LoadedAssets)
                    assetCreatorSecondLookupTable.Items.Add(texture.Name);

                if (assetCreatorSecondLookupTable.ListBoxVisible)
                    return;
                // Identify current index
                if (asset.ColorCorrection.SecondLookupTable == null)
                    assetCreatorSecondLookupTable.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < assetCreatorSecondLookupTable.Items.Count; i++)
                        if ((string)assetCreatorSecondLookupTable.Items[i] == asset.ColorCorrection.SecondLookupTable.Name)
                        {
                            assetCreatorSecondLookupTable.ItemIndex = i;
                            break;
                        }
                }
            };

            #endregion

            #region Lerp Original Color Amount

            var sliderLerpOriginalColorAmount = CommonControls.SliderNumeric("Lerp Original Color", groupColorCorrection, asset.ColorCorrection.LerpOriginalColorAmount, false, false, 0, 1);
            sliderLerpOriginalColorAmount.ValueChanged += delegate
                                                              {
                                                                  asset.ColorCorrection.LerpOriginalColorAmount = sliderLerpOriginalColorAmount.Value;
                                                              };
            sliderLerpOriginalColorAmount.Draw += delegate
            {
                sliderLerpOriginalColorAmount.Enabled = asset.ColorCorrection.FirstLookupTable != null && checkBoxColorCorrectionEnabled.Checked;
                sliderLerpOriginalColorAmount.Value = asset.ColorCorrection.LerpOriginalColorAmount;
            };

            #endregion

            #region Lerp Lookup Tables Amount

            var sliderLerpLookupTablesAmount = CommonControls.SliderNumeric("Lerp Lookup Tables", groupColorCorrection, asset.ColorCorrection.LerpLookupTablesAmount, false, false, 0, 1);
            sliderLerpLookupTablesAmount.ValueChanged += delegate { asset.ColorCorrection.LerpLookupTablesAmount = sliderLerpLookupTablesAmount.Value; };
            sliderLerpLookupTablesAmount.Draw += delegate
            {
                sliderLerpLookupTablesAmount.Enabled = asset.ColorCorrection.SecondLookupTable != null && asset.ColorCorrection.FirstLookupTable != null && checkBoxColorCorrectionEnabled.Checked;
                sliderLerpLookupTablesAmount.Value = asset.ColorCorrection.LerpLookupTablesAmount;
            };

            #endregion

            checkBoxColorCorrectionEnabled.CheckedChanged += delegate
            {
                asset.ColorCorrection.Enabled = checkBoxColorCorrectionEnabled.Checked;
                assetCreatorFirstLookupTable.Enabled = asset.ColorCorrection.Enabled;
            };

            groupColorCorrection.AdjustHeightFromChildren();

            #endregion

            window.Height = 500;

        } // Show

    } // PostProcessWindow
} // XNAFinalEngine.Editor