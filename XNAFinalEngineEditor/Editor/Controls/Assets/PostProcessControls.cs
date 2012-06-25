
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
    public static class PostProcessControls
    {
        
        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(PostProcess asset, Window owner, ComboBox comboBoxResource)
        {
            
            #region Group Lens Exposure

            GroupBox groupToneMapping = CommonControls.Group("Tone Mapping", owner);
            // Lens Exposure
            var sliderLensExposure = CommonControls.SliderNumericFloat("Lens Exposure", groupToneMapping, asset.ToneMapping.LensExposure, true, true, -10, 10,
                                                                  asset.ToneMapping, "LensExposure");
            // Auto Exposure Enabled
            CheckBox checkBoxAutoExposureEnabled = CommonControls.CheckBox("Auto Exposure Enabled", groupToneMapping, asset.ToneMapping.AutoExposureEnabled, 
                                                                           asset.ToneMapping, "AutoExposureEnabled");
            // Auto Exposure Adaptation Time Multiplier
            var sliderAutoExposureAdaptationTimeMultiplier = CommonControls.SliderNumericFloat("Adaptation Time Multiplier", groupToneMapping,
                                                                                          asset.ToneMapping.AutoExposureAdaptationTimeMultiplier,
                                                                                          false, true, 0, 10,
                                                                                          asset.ToneMapping, "AutoExposureAdaptationTimeMultiplier");
            // Auto Exposure Luminance Low Threshold
            var sliderAutoExposureLuminanceLowThreshold = CommonControls.SliderNumericFloat("Luminance Low Threshold", groupToneMapping,
                                                                                       asset.ToneMapping.AutoExposureLuminanceLowThreshold, false, true, 0, 0.5f,
                                                                                       asset.ToneMapping, "AutoExposureLuminanceLowThreshold");
            // Auto Exposure Luminance High Threshold

            var sliderAutoExposureLuminanceHighThreshold = CommonControls.SliderNumericFloat("Luminance High Threshold", groupToneMapping,
                asset.ToneMapping.AutoExposureLuminanceHighThreshold, false, true, 0.5f, 20f, asset.ToneMapping, "AutoExposureLuminanceHighThreshold");
            // Auto Exposure Enabled
            checkBoxAutoExposureEnabled.CheckedChanged += delegate
            {
                sliderLensExposure.Enabled                         = !asset.ToneMapping.AutoExposureEnabled;
                sliderAutoExposureAdaptationTimeMultiplier.Enabled = asset.ToneMapping.AutoExposureEnabled;
                sliderAutoExposureLuminanceLowThreshold.Enabled    = asset.ToneMapping.AutoExposureEnabled;
                sliderAutoExposureLuminanceHighThreshold.Enabled   = asset.ToneMapping.AutoExposureEnabled;
            };

            #region Tone Mapping Curve

            ComboBox comboBoxToneMappingCurve = CommonControls.ComboBox("Tone Mapping Curve", groupToneMapping);
            comboBoxToneMappingCurve.Items.AddRange(new[]
            {
                "Filmic ALU",
                "Filmic Uncharted 2",
                "Duiker",
                "Reinhard",
                "Reinhard Modified",
                "Exponential",
                "Logarithmic",
                "Drago Logarithmic"
            });
            comboBoxToneMappingCurve.ItemIndex = (int)asset.ToneMapping.ToneMappingFunction;
            comboBoxToneMappingCurve.ItemIndexChanged += delegate { asset.ToneMapping.ToneMappingFunction = (ToneMapping.ToneMappingFunctionEnumerate)comboBoxToneMappingCurve.ItemIndex; };
            comboBoxToneMappingCurve.Draw += delegate
            {
                if (comboBoxToneMappingCurve.ListBoxVisible)
                    return;
                comboBoxToneMappingCurve.ItemIndex = (int)asset.ToneMapping.ToneMappingFunction;
            };

            #endregion

            // White Level
            var sliderWhiteLevel = CommonControls.SliderNumericFloat("White Level", groupToneMapping, asset.ToneMapping.ToneMappingWhiteLevel,
                false, true, 0f, 50f, asset.ToneMapping, "ToneMappingWhiteLevel");
            // Luminance Saturation
            var sliderLuminanceSaturation = CommonControls.SliderNumericFloat("Luminance Saturation", groupToneMapping, asset.ToneMapping.ToneMappingLuminanceSaturation,
                false, true, 0f, 2f, asset.ToneMapping, "ToneMappingLuminanceSaturation");
            // Drago Bias
            var sliderDragoBias = CommonControls.SliderNumericFloat("Drago Bias", groupToneMapping, asset.ToneMapping.DragoBias,
                false, true, 0f, 1f, asset.ToneMapping, "DragoBias");
            // Shoulder Strength
            var sliderShoulderStrength = CommonControls.SliderNumericFloat("Shoulder Strength", groupToneMapping, asset.ToneMapping.Uncharted2ShoulderStrength,
                false, true, 0f, 1f, asset.ToneMapping, "Uncharted2ShoulderStrength");
            // Linear Strength
            var sliderLinearStrength = CommonControls.SliderNumericFloat("Linear Strength", groupToneMapping, asset.ToneMapping.Uncharted2LinearStrength,
                false, true, 0f, 1f, asset.ToneMapping, "Uncharted2LinearStrength");
            // Linear Angle
            var sliderLinearAngle = CommonControls.SliderNumericFloat("Linear Angle", groupToneMapping, asset.ToneMapping.Uncharted2LinearAngle,
                false, true, 0f, 3f, asset.ToneMapping, "Uncharted2LinearAngle");
            // Toe Strength
            var sliderToeStrength = CommonControls.SliderNumericFloat("Toe Strength", groupToneMapping, asset.ToneMapping.Uncharted2ToeStrength,
                false, true, 0f, 1f, asset.ToneMapping, "Uncharted2ToeStrength");
            // Toe Numerator
            var sliderToeNumerator = CommonControls.SliderNumericFloat("Toe Numerator", groupToneMapping, asset.ToneMapping.Uncharted2ToeNumerator,
                false, true, 0f, 0.1f, asset.ToneMapping, "Uncharted2ToeNumerator");
            // Toe Denominator
            var sliderToeDenominator = CommonControls.SliderNumericFloat("Toe Denominator", groupToneMapping, asset.ToneMapping.Uncharted2ToeDenominator,
                false, true, 0f, 1f, asset.ToneMapping, "Uncharted2ToeDenominator");
            // Linear White
            var sliderLinearWhite = CommonControls.SliderNumericFloat("Linear White", groupToneMapping, asset.ToneMapping.Uncharted2LinearWhite,
                false, true, 0f, 40f, asset.ToneMapping, "Uncharted2LinearWhite");

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

            GroupBox groupBloom = CommonControls.Group("Bloom", owner);

            // Enabled
            CheckBox checkBoxBloomEnabled = CommonControls.CheckBox("Enabled", groupBloom, asset.Bloom.Enabled,
                asset.Bloom, "Enabled", "The effect produces fringes (or feathers) of light around very bright objects in an image.");
            // Scale
            var sliderBloomScale = CommonControls.SliderNumericFloat("Scale", groupBloom, asset.Bloom.Scale, false, true, 0, 2,  asset.Bloom, "Scale");
            // Threshold
            var sliderBloomThreshold = CommonControls.SliderNumericFloat("Threshold", groupBloom, asset.Bloom.Threshold, false, true, 0, 10, asset.Bloom, "Threshold");
            // Enabled
            checkBoxBloomEnabled.CheckedChanged += delegate
            {
                sliderBloomScale.Enabled = asset.Bloom.Enabled;
                sliderBloomThreshold.Enabled = asset.Bloom.Enabled;
            };

            groupBloom.AdjustHeightFromChildren();

            #endregion

            #region Group MLAA

            GroupBox groupMlaa = CommonControls.Group("MLAA", owner);
            // Enabled
            CheckBox checkBoxMlaaEnabled = CommonControls.CheckBox("Enabled", groupMlaa, asset.MLAA.Enabled, asset.MLAA, "Enabled", "Enables Morphological Antialiasing.");
            
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
                    case 0: asset.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Both; break;
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
                    case MLAA.EdgeDetectionType.Both: comboBoxEdgeDetection.ItemIndex = 0; break;
                    case MLAA.EdgeDetectionType.Color: comboBoxEdgeDetection.ItemIndex = 1; break;
                    case MLAA.EdgeDetectionType.Depth: comboBoxEdgeDetection.ItemIndex = 2; break;
                }
            };

            #endregion

            // Threshold Color
            var sliderMlaaColorThreshold = CommonControls.SliderNumericFloat("Color Threshold", groupMlaa, asset.MLAA.ThresholdColor,
                false, false, 0, 0.5f, asset.MLAA, "ThresholdColor");
            // Threshold Depth
            var sliderMlaaDepthThreshold = CommonControls.SliderNumericFloat("Depth Threshold", groupMlaa, asset.MLAA.ThresholdDepth,
                false, false, 0, 0.5f, asset.MLAA, "ThresholdDepth");

            checkBoxMlaaEnabled.CheckedChanged += delegate
            {
                comboBoxEdgeDetection.Enabled = asset.MLAA.Enabled;
                sliderMlaaColorThreshold.Enabled = asset.MLAA.Enabled;
                sliderMlaaDepthThreshold.Enabled = asset.MLAA.Enabled;
            };

            groupMlaa.AdjustHeightFromChildren();

            #endregion

            #region Group Film Grain

            GroupBox groupFilmGrain = CommonControls.Group("Film Grain", owner);

            // Enabled
            CheckBox checkBoxFilmGrainEnabled = CommonControls.CheckBox("Enabled", groupFilmGrain, asset.FilmGrain.Enabled, asset.FilmGrain, "Enabled",
                "Is the random optical texture of processed photographic film.");
            // Strength
            var sliderFilmgrainStrength = CommonControls.SliderNumericFloat("Strength", groupFilmGrain, asset.FilmGrain.Strength, false, true, 0, 0.5f, asset.FilmGrain, "Strength");
            // Random Noise Strength
            var sliderFilmGrainRandomNoiseStrength = CommonControls.SliderNumericFloat("Random Noise Strength", groupFilmGrain, asset.FilmGrain.RandomNoiseStrength, 
                false, false, 0, 5, asset.FilmGrain, "RandomNoiseStrength");
            // Accentuate Dark Noise Power
            var sliderFilmGrainAccentuateDarkNoisePower = CommonControls.SliderNumericFloat("Accentuate Dark Noise Power", groupFilmGrain, asset.FilmGrain.AccentuateDarkNoisePower,
                false, false, 0, 10, asset.FilmGrain, "AccentuateDarkNoisePower");

            checkBoxFilmGrainEnabled.CheckedChanged += delegate
            {
                sliderFilmgrainStrength.Enabled = asset.FilmGrain.Enabled;
                sliderFilmGrainRandomNoiseStrength.Enabled = asset.FilmGrain.Enabled;
                sliderFilmGrainAccentuateDarkNoisePower.Enabled = asset.FilmGrain.Enabled;
            };

            groupFilmGrain.AdjustHeightFromChildren();

            #endregion

            #region Group Adjust Levels

            GroupBox groupAdjustLevels = CommonControls.Group("Adjust Levels", owner);

            // Enabled
            CheckBox checkBoxAdjustLevelsEnabled = CommonControls.CheckBox("Enabled", groupAdjustLevels, asset.AdjustLevels.Enabled,
                asset.AdjustLevels, "Enabled", "Adjust color levels just like Photoshop.");
            // Input Black
            var sliderAdjustLevelsInputBlack = CommonControls.SliderNumericFloat("Input Black", groupAdjustLevels, asset.AdjustLevels.InputBlack,
                false, false, 0, 0.9f, asset.AdjustLevels, "InputBlack");
            // Input White
            var sliderAdjustLevelsInputWhite = CommonControls.SliderNumericFloat("Input White", groupAdjustLevels, asset.AdjustLevels.InputWhite,
                false, false, 0.1f, 1f, asset.AdjustLevels, "InputWhite");
            // Input Gamma
            var sliderAdjustLevelsInputGamma = CommonControls.SliderNumericFloat("Input Gamma", groupAdjustLevels, asset.AdjustLevels.InputGamma,
                false, false, 0.01f, 9.99f, asset.AdjustLevels, "InputGamma");
            // Output Black
            var sliderAdjustLevelsOutputBlack = CommonControls.SliderNumericFloat("Output Black", groupAdjustLevels, asset.AdjustLevels.OutputBlack,
                false, false, 0, 1, asset.AdjustLevels, "OutputBlack");
            // Output White
            var sliderAdjustLevelsOutputWhite = CommonControls.SliderNumericFloat("Output White", groupAdjustLevels, asset.AdjustLevels.OutputWhite,
                false, false, 0, 1, asset.AdjustLevels, "OutputWhite");
            
            checkBoxAdjustLevelsEnabled.CheckedChanged += delegate
            {
                sliderAdjustLevelsInputBlack.Enabled  = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsInputWhite.Enabled  = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsInputGamma.Enabled  = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsOutputBlack.Enabled = asset.AdjustLevels.Enabled;
                sliderAdjustLevelsOutputWhite.Enabled = asset.AdjustLevels.Enabled;
            };

            groupAdjustLevels.AdjustHeightFromChildren();

            #endregion

            #region Group Color Correction

            GroupBox groupColorCorrection = CommonControls.Group("Color Correction", owner);

            // Enabled
            CheckBox checkBoxColorCorrectionEnabled = CommonControls.CheckBox("Enabled", groupColorCorrection, asset.ColorCorrection.Enabled, asset.ColorCorrection, "Enabled");
            // First Lookup Table
            var assetCreatorFirstLookupTable = CommonControls.AssetSelector<LookupTable>("First Lookup Table", groupColorCorrection, asset.ColorCorrection, "FirstLookupTable");
            // Second Lookup Table
            var assetCreatorSecondLookupTable = CommonControls.AssetSelector<LookupTable>("Second Lookup Table", groupColorCorrection, asset.ColorCorrection, "SecondLookupTable");
            assetCreatorSecondLookupTable.Draw += delegate
            {
                assetCreatorSecondLookupTable.Enabled = asset.ColorCorrection.FirstLookupTable != null && checkBoxColorCorrectionEnabled.Checked;                
            };
            // Lerp Original Color Amount
            var sliderLerpOriginalColorAmount = CommonControls.SliderNumericFloat("Lerp Original Color", groupColorCorrection, asset.ColorCorrection.LerpOriginalColorAmount, 
                false, false, 0, 1, asset.ColorCorrection, "LerpOriginalColorAmount");
            sliderLerpOriginalColorAmount.Draw += delegate
            {
                sliderLerpOriginalColorAmount.Enabled = asset.ColorCorrection.FirstLookupTable != null && checkBoxColorCorrectionEnabled.Checked;
            };
            // Lerp Lookup Tables Amount
            var sliderLerpLookupTablesAmount = CommonControls.SliderNumericFloat("Lerp Lookup Tables", groupColorCorrection, asset.ColorCorrection.LerpLookupTablesAmount,
                false, false, 0, 1, asset.ColorCorrection, "LerpLookupTablesAmount");
            sliderLerpLookupTablesAmount.Draw += delegate
            {
                sliderLerpLookupTablesAmount.Enabled = asset.ColorCorrection.SecondLookupTable != null && asset.ColorCorrection.FirstLookupTable != null && checkBoxColorCorrectionEnabled.Checked;
            };

            checkBoxColorCorrectionEnabled.CheckedChanged += delegate { assetCreatorFirstLookupTable.Enabled = asset.ColorCorrection.Enabled; };

            groupColorCorrection.AdjustHeightFromChildren();

            #endregion

        } // AddControls       

    } // PostProcessControls
} // XNAFinalEngine.Editor