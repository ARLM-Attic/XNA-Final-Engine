
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
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Über post processing shader.
    /// </summary>
    internal class PostProcessingShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static PostProcessingShader instance;
        
        // Random number for the film grain effect.
        private static readonly Random randomNumber = new Random();

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static PostProcessingShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new PostProcessingShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        #region Effect Handles

        /// <summary>
        /// Effect handles.
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epLensExposure,
                                       epSceneTexture,
                                       epLastLuminanceTexture,
                                       epAutoExposure,
                                       epTimeDelta,
                                       epExposureAdjustTimeMultiplier,
                                       epLuminanceLowThreshold,
                                       epLuminanceHighThreshold,
                                       // Tone Mapping
                                       epWhiteLevel,
                                       epLuminanceSaturation,
                                       epBias,
                                       epShoulderStrength,
                                       epLinearStrength,
                                       epLinearAngle,
                                       epToeStrength,
                                       epToeNumerator,
                                       epToeDenominator,
                                       epLinearWhite,
                                       // Bloom
                                       epBloomScale,
                                       epBloomTexture,
                                       epBloomEnabled,
                                       // Levels
                                       epInputBlack,
                                       epInputWhite,
                                       epInputGamma,
                                       epOutputBlack,
                                       epOutputWhite,
                                       epAdjustLevelsEnabled,
                                       // Leves Individual Channels
                                       epInputBlackRgb,
                                       epInputWhiteRgb,
                                       epInputGammaRgb,
                                       epOutputBlackRgb,
                                       epOutputWhiteRgb,
                                       epAdjustLevelsIndividualChannelsEnabled,
                                       // Color Correction
                                       epColorCorrectOneLutEnabled,
                                       epColorCorrectTwoLutEnabled,
                                       epLookupTableScale,
                                       epLookupTableOffset,
                                       epFirstlookupTable,
                                       epSecondlookupTable,
                                       epLerpOriginalColorAmount,
                                       epLerpLookupTablesAmount,
                                       // Film Grain
                                       epFilmGrainEnabled,
                                       epFilmGrainStrength,
                                       epAccentuateDarkNoisePower,
                                       epRandomNoiseStrength,
                                       epRandomValue;

        #endregion

        #region Half Pixel

        private static Vector2? lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region Scene Texture

        private static Texture2D lastUsedSceneTexture;
        private static void SetSceneTexture(Texture sceneTexture)
        {
            EngineManager.Device.SamplerStates[9] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedSceneTexture != sceneTexture.Resource)
            {
                lastUsedSceneTexture = sceneTexture.Resource;
                epSceneTexture.SetValue(sceneTexture.Resource);
            }
        } // SetSceneTexture

        #endregion

        #region Last Luminance Texture

        private static Texture2D lastUsedLastLuminanceTexture;
        private static void SetLuminanceTexture(Texture lastLuminanceTexture)
        {
            EngineManager.Device.SamplerStates[12] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedLastLuminanceTexture != lastLuminanceTexture.Resource)
            {
                lastUsedLastLuminanceTexture = lastLuminanceTexture.Resource;
                epLastLuminanceTexture.SetValue(lastLuminanceTexture.Resource);
            }
        } // SetLastLuminanceTexture

        #endregion

        #region Exposure

        #region Lens Exposure

        private static float? lastUsedLensExposure;
        private static void SetLensExposure(float lensExposure)
        {
            if (lastUsedLensExposure != lensExposure)
            {
                lastUsedLensExposure = lensExposure;
                epLensExposure.SetValue(lensExposure);
            }
        } // SetLensExposure

        #endregion

        #region Auto Exposure

        private static bool? lastUsedAutoExposure;
        private static void SetAutoExposure(bool autoExposure)
        {
            if (lastUsedAutoExposure != autoExposure)
            {
                lastUsedAutoExposure = autoExposure;
                epAutoExposure.SetValue(autoExposure);
            }
        } // SetAutoExposure

        #endregion

        #region Auto Exposure Adjust Time Multiplier

        private static float? lastUsedAutoExposureAdjustTimeMultiplier;
        private static void SetAutoExposureAdjustTimeMultiplier(float exposureAdjustTimeMultiplier)
        {
            if (lastUsedAutoExposureAdjustTimeMultiplier != exposureAdjustTimeMultiplier)
            {
                lastUsedAutoExposureAdjustTimeMultiplier = exposureAdjustTimeMultiplier;
                epExposureAdjustTimeMultiplier.SetValue(exposureAdjustTimeMultiplier);
            }
        } // SetExposureAdjustTimeMultiplier

        #endregion

        #region Auto Exposure Luminance Low Threshold

        private static float? lastUsedAutoExposureLuminanceLowThreshold;
        private static void SetAutoExposureLuminanceLowThreshold(float autoExposureLuminanceLowThreshold)
        {
            if (lastUsedAutoExposureLuminanceLowThreshold != autoExposureLuminanceLowThreshold)
            {
                lastUsedAutoExposureLuminanceLowThreshold = autoExposureLuminanceLowThreshold;
                epLuminanceLowThreshold.SetValue(autoExposureLuminanceLowThreshold);
            }
        } // SetAutoExposureLuminanceLowThreshold

        #endregion

        #region Auto Exposure Luminance High Threshold

        private static float? lastUsedAutoExposureLuminanceHighThreshold;
        private static void SetAutoExposureLuminanceHighThreshold(float autoExposureLuminanceHighThreshold)
        {
            if (lastUsedAutoExposureLuminanceHighThreshold != autoExposureLuminanceHighThreshold)
            {
                lastUsedAutoExposureLuminanceHighThreshold = autoExposureLuminanceHighThreshold;
                epLuminanceHighThreshold.SetValue(autoExposureLuminanceHighThreshold);
            }
        } // SetAutoExposureLuminanceHighThreshold

        #endregion

        #endregion

        #region Tone Mapping

        #region White Level

        private static float? lastUsedWhiteLevel;
        private static void SetWhiteLevel(float whiteLevel)
        {
            if (lastUsedWhiteLevel != whiteLevel)
            {
                lastUsedWhiteLevel = whiteLevel;
                epWhiteLevel.SetValue(whiteLevel);
            }
        } // SetWhiteLevel

        #endregion

        #region Luminance Saturation

        private static float? lastUsedLuminanceSaturation;
        private static void SetLuminanceSaturation(float luminanceSaturation)
        {
            if (lastUsedLuminanceSaturation != luminanceSaturation)
            {
                lastUsedLuminanceSaturation = luminanceSaturation;
                epLuminanceSaturation.SetValue(luminanceSaturation);
            }
        } // SetLuminanceSaturation

        #endregion

        #region Bias

        private static float? lastUsedBias;
        private static void SetBias(float bias)
        {
            if (lastUsedBias != bias)
            {
                lastUsedBias = bias;
                epBias.SetValue(bias);
            }
        } // SetBias

        #endregion

        #region Shoulder Strength

        private static float? lastUsedShoulderStrength;
        private static void SetShoulderStrength(float shoulderStrength)
        {
            if (lastUsedShoulderStrength != shoulderStrength)
            {
                lastUsedShoulderStrength = shoulderStrength;
                epShoulderStrength.SetValue(shoulderStrength);
            }
        } // SetShoulderStrength

        #endregion

        #region Linear Strength

        private static float? lastUsedLinearStrength;
        private static void SetLinearStrength(float linearStrength)
        {
            if (lastUsedLinearStrength != linearStrength)
            {
                lastUsedLinearStrength = linearStrength;
                epLinearStrength.SetValue(linearStrength);
            }
        } // SetLinearStrength

        #endregion

        #region Linear Angle

        private static float? lastUsedLinearAngle;
        private static void SetLinearAngle(float linearAngle)
        {
            if (lastUsedLinearAngle != linearAngle)
            {
                lastUsedLinearAngle = linearAngle;
                epLinearAngle.SetValue(linearAngle);
            }
        } // SetLinearAngle

        #endregion

        #region Toe Strength

        private static float? lastUsedToeStrength;
        private static void SetToeStrength(float toeStrength)
        {
            if (lastUsedToeStrength != toeStrength)
            {
                lastUsedToeStrength = toeStrength;
                epToeStrength.SetValue(toeStrength);
            }
        } // SetToeStrength

        #endregion

        #region Toe Numerator

        private static float? lastUsedToeNumerator;
        private static void SetToeNumerator(float toeNumerator)
        {
            if (lastUsedToeNumerator != toeNumerator)
            {
                lastUsedToeNumerator = toeNumerator;
                epToeNumerator.SetValue(toeNumerator);
            }
        } // SetToeNumerator

        #endregion

        #region Toe Denominator

        private static float? lastUsedToeDenominator;
        private static void SetToeDenominator(float toeDenominator)
        {
            if (lastUsedToeDenominator != toeDenominator)
            {
                lastUsedToeDenominator = toeDenominator;
                epToeDenominator.SetValue(toeDenominator);
            }
        } // SetToeDenominator

        #endregion

        #region Linear White

        private static float? lastUsedLinearWhite;
        private static void SetLinearWhite(float linearWhite)
        {
            if (lastUsedLinearWhite != linearWhite)
            {
                lastUsedLinearWhite = linearWhite;
                epLinearWhite.SetValue(linearWhite);
            }
        } // SetLinearWhite

        #endregion

        #endregion

        #region Bloom

        #region Bloom Scale

        private static float? lastUsedBloomScale;
        private static void SetBloomScale(float bloomScale)
        {
            if (lastUsedBloomScale != bloomScale)
            {
                lastUsedBloomScale = bloomScale;
                epBloomScale.SetValue(bloomScale);
            }
        } // SetBloomScale

        #endregion

        #region Bloom Texture

        private static Texture2D lastUsedBloomTexture;
        private static void SetBloomTexture(Texture bloomTexture)
        {
            EngineManager.Device.SamplerStates[10] = SamplerState.AnisotropicClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedBloomTexture != bloomTexture.Resource)
            {
                lastUsedBloomTexture = bloomTexture.Resource;
                epBloomTexture.SetValue(bloomTexture.Resource);
            }
        } // SetBloomTexture

        #endregion

        #region Bloom Enabled

        private static bool? lastUsedBloomEnabled;
        private static void SetBloomEnabled(bool bloomEnabled)
        {
            if (lastUsedBloomEnabled != bloomEnabled)
            {
                lastUsedBloomEnabled = bloomEnabled;
                epBloomEnabled.SetValue(bloomEnabled);
            }
        } // SetBloomEnabled

        #endregion

        #endregion

        #region Levels

        #region Input Black
        
        private static float? lastUsedInputBlack;
        private static void SetInputBlack(float inputBlack)
        {
            if (lastUsedInputBlack != inputBlack)
            {
                lastUsedInputBlack = inputBlack;
                epInputBlack.SetValue(inputBlack);
            }
        } // SetInputBlack

        #endregion

        #region Input White

        private static float? lastUsedInputWhite;
        private static void SetInputWhite(float inputWhite)
        {
            if (lastUsedInputWhite != inputWhite)
            {
                lastUsedInputWhite = inputWhite;
                epInputWhite.SetValue(inputWhite);
            }
        } // SetInputWhite

        #endregion

        #region Input Gamma

        private static float? lastUsedInputGamma;
        private static void SetInputGamma(float inputGamma)
        {
            if (lastUsedInputGamma != inputGamma)
            {
                lastUsedInputGamma = inputGamma;
                epInputGamma.SetValue(inputGamma);
            }
        } // SetInputGamma

        #endregion

        #region Output Black

        private static float? lastUsedOutputBlack;
        private static void SetOutputBlack(float outputBlack)
        {
            if (lastUsedOutputBlack != outputBlack)
            {
                lastUsedOutputBlack = outputBlack;
                epOutputBlack.SetValue(outputBlack);
            }
        } // SetOutputBlack

        #endregion

        #region Output White

        private static float? lastUsedOutputWhite;
        private static void SetOutputWhite(float outputWhite)
        {
            if (lastUsedOutputWhite != outputWhite)
            {
                lastUsedOutputWhite = outputWhite;
                epOutputWhite.SetValue(outputWhite);
            }
        } // SetOutputWhite

        #endregion

        #region Adjust Levels Enabled

        private static bool? lastUsedAdjustLevelsEnabled;
        private static void SetAdjustLevelsEnabled(bool adjustLevelsEnabled)
        {
            if (lastUsedAdjustLevelsEnabled != adjustLevelsEnabled)
            {
                lastUsedAdjustLevelsEnabled = adjustLevelsEnabled;
                epAdjustLevelsEnabled.SetValue(adjustLevelsEnabled);
            }
        } // SetAdjustLevelsEnabled

        #endregion

        #endregion

        #region Leves Individual Channels

        #region Input Black RGB

        private static Vector3? lastUsedInputBlackRgb;
        private static void SetInputBlackRgb(Vector3 inputBlackRgb)
        {
            if (lastUsedInputBlackRgb != inputBlackRgb)
            {
                lastUsedInputBlackRgb = inputBlackRgb;
                epInputBlackRgb.SetValue(inputBlackRgb);
            }
        } // SetInputBlackRgb

        #endregion

        #region Input White RGB

        private static Vector3? lastUsedInputWhiteRgb;
        private static void SetInputWhiteRgb(Vector3 inputWhiteRgb)
        {
            if (lastUsedInputWhiteRgb != inputWhiteRgb)
            {
                lastUsedInputWhiteRgb = inputWhiteRgb;
                epInputWhiteRgb.SetValue(inputWhiteRgb);
            }
        } // SetInputWhiteRgb

        #endregion

        #region Input Gamma RGB

        private static Vector3? lastUsedInputGammaRgb;
        private static void SetInputGammaRgb(Vector3 inputGammaRgb)
        {
            if (lastUsedInputGammaRgb != inputGammaRgb)
            {
                lastUsedInputGammaRgb = inputGammaRgb;
                epInputGammaRgb.SetValue(inputGammaRgb);
            }
        } // SetInputGammaRgb

        #endregion

        #region Output Black RGB

        private static Vector3? lastUsedOutputBlackRgb;
        private static void SetOutputBlackRgb(Vector3 outputBlackRgb)
        {
            if (lastUsedOutputBlackRgb != outputBlackRgb)
            {
                lastUsedOutputBlackRgb = outputBlackRgb;
                epOutputBlackRgb.SetValue(outputBlackRgb);
            }
        } // SetOutputBlackRgb

        #endregion

        #region Output White RGB

        private static Vector3? lastUsedOutputWhiteRgb;
        private static void SetOutputWhiteRgb(Vector3 outputWhiteRgb)
        {
            if (lastUsedOutputWhiteRgb != outputWhiteRgb)
            {
                lastUsedOutputWhiteRgb = outputWhiteRgb;
                epOutputWhiteRgb.SetValue(outputWhiteRgb);
            }
        } // SetOutputWhiteRgb

        #endregion

        #region Adjust Levels Individual Channels Enabled

        private static bool? lastUsedAdjustLevelsIndividualChannelsEnabled;
        private static void SetAdjustLevelsIndividualChannelsEnabled(bool adjustLevelsIndividualChannelsEnabled)
        {
            if (lastUsedAdjustLevelsIndividualChannelsEnabled != adjustLevelsIndividualChannelsEnabled)
            {
                lastUsedAdjustLevelsIndividualChannelsEnabled = adjustLevelsIndividualChannelsEnabled;
                epAdjustLevelsIndividualChannelsEnabled.SetValue(adjustLevelsIndividualChannelsEnabled);
            }
        } // SetAdjustLevelsIndividualChannelsEnabled

        #endregion

        #endregion

        #region Color Correction

        #region Color Correct One Lut Enabled

        private static bool? lastUsedColorCorrectOneLutEnabled;
        private static void SetColorCorrectOneLutEnabled(bool colorCorrectOneLutEnabled)
        {
            if (lastUsedColorCorrectOneLutEnabled != colorCorrectOneLutEnabled)
            {
                lastUsedColorCorrectOneLutEnabled = colorCorrectOneLutEnabled;
                epColorCorrectOneLutEnabled.SetValue(colorCorrectOneLutEnabled);
            }
        } // SetColorCorrectOneLutEnabled

        #endregion

        #region Color Correct Two Lut Enabled

        private static bool? lastUsedColorCorrectTwoLutEnabled;
        private static void SetColorCorrectTwoLutEnabled(bool colorCorrectTwoLutEnabled)
        {
            if (lastUsedColorCorrectTwoLutEnabled != colorCorrectTwoLutEnabled)
            {
                lastUsedColorCorrectTwoLutEnabled = colorCorrectTwoLutEnabled;
                epColorCorrectTwoLutEnabled.SetValue(colorCorrectTwoLutEnabled);
            }
        } // SetColorCorrectTwoLutEnabled

        #endregion

        #region Lookup Table Scale

        private static float? lastUsedLookupTableScale;
        private static void SetLookupTableScale(float lookupTableScale)
        {
            if (lastUsedLookupTableScale != lookupTableScale)
            {
                lastUsedLookupTableScale = lookupTableScale;
                epLookupTableScale.SetValue(lookupTableScale);
            }
        } // SetLookupTableScale

        #endregion

        #region Lookup Table Offset

        private static float? lastUsedLookupTableOffset;
        private static void SetLookupTableOffset(float lookupTableOffset)
        {
            if (lastUsedLookupTableOffset != lookupTableOffset)
            {
                lastUsedLookupTableOffset = lookupTableOffset;
                epLookupTableOffset.SetValue(lookupTableOffset);
            }
        } // SetLookupTableOffset

        #endregion

        #region Lerp Original Color Amount

        private static float? lastUsedLerpOriginalColorAmount;
        private static void SetLerpOriginalColorAmount(float lerpOriginalColorAmount)
        {
            if (lastUsedLerpOriginalColorAmount != lerpOriginalColorAmount)
            {
                lastUsedLerpOriginalColorAmount = lerpOriginalColorAmount;
                epLerpOriginalColorAmount.SetValue(lerpOriginalColorAmount);
            }
        } // SetLerpOriginalColorAmount

        #endregion

        #region Lerp Lookup Tables Amount

        private static float? lastUsedLerpLookupTablesAmount;
        private static void SetLerpLookupTablesAmount(float lerpLookupTablesAmount)
        {
            if (lastUsedLerpLookupTablesAmount != lerpLookupTablesAmount)
            {
                lastUsedLerpLookupTablesAmount = lerpLookupTablesAmount;
                epLerpLookupTablesAmount.SetValue(lerpLookupTablesAmount);
            }
        } // SetLerpLookupTablesAmount

        #endregion

        #region First Lookup Table

        private static Texture3D lastUsedFirstLookupTable;
        private static void SetFirstLookupTable(LookupTable firstLookupTable)
        {
            EngineManager.Device.SamplerStates[5] = SamplerState.LinearClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedFirstLookupTable != firstLookupTable.Resource)
            {
                lastUsedFirstLookupTable = firstLookupTable.Resource;
                epFirstlookupTable.SetValue(firstLookupTable.Resource);
            }
        } // SetFirstLookupTable

        #endregion

        #region Second Lookup Table

        private static Texture3D lastUsedSecondLookupTable;
        private static void SetSecondLookupTable(LookupTable secondLookupTable)
        {
            EngineManager.Device.SamplerStates[6] = SamplerState.LinearClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedSecondLookupTable != secondLookupTable.Resource)
            {
                lastUsedSecondLookupTable = secondLookupTable.Resource;
                epSecondlookupTable.SetValue(secondLookupTable.Resource);
            }
        } // SetSecondLookupTable

        #endregion

        #endregion

        #region Film Grain

        #region Film Grain Enabled

        private static bool? lastUsedFilmGrainEnabled;
        private static void SetFilmGrainEnabled(bool filmGrainEnabled)
        {
            if (lastUsedFilmGrainEnabled != filmGrainEnabled)
            {
                lastUsedFilmGrainEnabled = filmGrainEnabled;
                epFilmGrainEnabled.SetValue(filmGrainEnabled);
            }
        } // SetFilmGrainEnabled

        #endregion

        #region Film Grain Strength

        private static float? lastUsedFilmGrainStrength;
        private static void SetFilmGrainStrength(float filmGrainStrength)
        {
            if (lastUsedFilmGrainStrength != filmGrainStrength)
            {
                lastUsedFilmGrainStrength = filmGrainStrength;
                epFilmGrainStrength.SetValue(filmGrainStrength);
            }
        } // SetFilmGrainStrength

        #endregion

        #region Accentuate Dark Noise Power

        private static float? lastUsedAccentuateDarkNoisePower;
        private static void SetAccentuateDarkNoisePower(float accentuateDarkNoisePower)
        {
            if (lastUsedAccentuateDarkNoisePower != accentuateDarkNoisePower)
            {
                lastUsedAccentuateDarkNoisePower = accentuateDarkNoisePower;
                epAccentuateDarkNoisePower.SetValue(accentuateDarkNoisePower);
            }
        } // SetAccentuateDarkNoisePower

        #endregion

        #region Random Noise Strength

        private static float? lastUsedRandomNoiseStrength;
        private static void SetRandomNoiseStrength(float randomNoiseStrength)
        {
            if (lastUsedRandomNoiseStrength != randomNoiseStrength)
            {
                lastUsedRandomNoiseStrength = randomNoiseStrength;
                epRandomNoiseStrength.SetValue(randomNoiseStrength);
            }
        } // SetRandomNoiseStrength

        #endregion

        #region Random Value

        private static float? lastUsedRandomValue;
        private static void SetRandomValue(float randomValue)
        {
            if (lastUsedRandomValue != randomValue)
            {
                lastUsedRandomValue = randomValue;
                epRandomValue.SetValue(randomValue);
            }
        } // SetRandomValue

        #endregion

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Über post processing shader.
        /// </summary>
        private PostProcessingShader() : base("PostProcessing\\PostProcessing")
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            Texture filmLutTexture = new Texture("Shaders\\FilmLut");
            ContentManager.CurrentContentManager = userContentManager;

            Resource.Parameters["filmLutTexture"].SetValue(filmLutTexture.Resource);
        } // PostProcessingShader

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override void GetParametersHandles()
        {
            try
            {
                epHalfPixel    = Resource.Parameters["halfPixel"];
                epLensExposure = Resource.Parameters["lensExposure"];
                epSceneTexture = Resource.Parameters["sceneTexture"];
                epLastLuminanceTexture = Resource.Parameters["lastLuminanceTexture"];
                epAutoExposure = Resource.Parameters["autoExposure"];
                epTimeDelta = Resource.Parameters["timeDelta"];
                epExposureAdjustTimeMultiplier = Resource.Parameters["tau"];
                epLuminanceLowThreshold = Resource.Parameters["luminanceLowThreshold"];
                epLuminanceHighThreshold = Resource.Parameters["luminanceHighThreshold"];
                // Tone Mapping
                epWhiteLevel = Resource.Parameters["whiteLevel"];
                epLuminanceSaturation = Resource.Parameters["luminanceSaturation"];
                epBias = Resource.Parameters["bias"];
                epShoulderStrength = Resource.Parameters["shoulderStrength"];
                epLinearStrength = Resource.Parameters["linearStrength"];
                epLinearAngle = Resource.Parameters["linearAngle"];
                epToeStrength = Resource.Parameters["toeStrength"];
                epToeNumerator = Resource.Parameters["toeNumerator"];
                epToeDenominator = Resource.Parameters["toeDenominator"];
                epLinearWhite = Resource.Parameters["linearWhite"];
                // Bloom
                epBloomScale   = Resource.Parameters["bloomScale"];
                epBloomTexture = Resource.Parameters["bloomTexture"];
                epBloomEnabled = Resource.Parameters["bloomEnabled"];
                // Levels
                epInputBlack   = Resource.Parameters["inputBlack"];
                epInputWhite   = Resource.Parameters["inputWhite"];
                epInputGamma   = Resource.Parameters["inputGamma"];
                epOutputBlack  = Resource.Parameters["outputBlack"];
                epOutputWhite  = Resource.Parameters["outputWhite"];
                epAdjustLevelsEnabled = Resource.Parameters["adjustLevelsEnabled"];
                // Leves Individual Channels
                epInputBlackRgb = Resource.Parameters["inputBlackRGB"];
                epInputWhiteRgb = Resource.Parameters["inputWhiteRGB"];
                epInputGammaRgb = Resource.Parameters["inputGammaRGB"];
                epOutputBlackRgb = Resource.Parameters["outputBlackRGB"];
                epOutputWhiteRgb = Resource.Parameters["outputWhiteRGB"];
                epAdjustLevelsIndividualChannelsEnabled = Resource.Parameters["adjustLevelsIndividualChannelsEnabled"];
                // Color Correction
                epColorCorrectOneLutEnabled = Resource.Parameters["colorCorrectOneLutEnabled"];
                epColorCorrectTwoLutEnabled = Resource.Parameters["colorCorrectTwoLutEnabled"];
                epLookupTableScale = Resource.Parameters["scale"];
                epLookupTableOffset = Resource.Parameters["offset"];
                epFirstlookupTable = Resource.Parameters["firstlookupTableTexture"];
                epSecondlookupTable = Resource.Parameters["secondlookupTableTexture"];
                epLerpOriginalColorAmount = Resource.Parameters["lerpOriginalColorAmount"];
                epLerpLookupTablesAmount = Resource.Parameters["lerpLookupTablesAmount"];
                // Film Grain
                epFilmGrainEnabled = Resource.Parameters["filmGrainEnabled"];
                epFilmGrainStrength = Resource.Parameters["filmGrainStrength"];
                epAccentuateDarkNoisePower = Resource.Parameters["accentuateDarkNoisePower"];
                epRandomNoiseStrength = Resource.Parameters["randomNoiseStrength"];
                epRandomValue = Resource.Parameters["randomValue"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParametersHandles

        #endregion

        #region Luminance Texture Generation

        /// <summary>
        /// Luminance Texture Generation.
        /// This pass transforms the color information into luminance information.
        /// This also clamps the lower and higher values.
        /// </summary>
        public RenderTarget LuminanceTextureGeneration(Texture sceneTexture, PostProcess postProcess)
        {
            if (sceneTexture == null || sceneTexture.Resource == null)
                throw new ArgumentNullException("sceneTexture");
            
            try
            {
                // Quarter size is enough
                RenderTarget initialLuminanceTexture = RenderTarget.Fetch(sceneTexture.Size.HalfSize().HalfSize(), SurfaceFormat.Single, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing, false);

                Resource.CurrentTechnique = Resource.Techniques["LuminanceMap"];
                // Set render states
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                // Set parameters
                SetHalfPixel(new Vector2(-1f / sceneTexture.Width, 1f / sceneTexture.Height));
                SetSceneTexture(sceneTexture);
                SetAutoExposureLuminanceLowThreshold(postProcess.ToneMapping.AutoExposureLuminanceLowThreshold);
                SetAutoExposureLuminanceHighThreshold(postProcess.ToneMapping.AutoExposureLuminanceHighThreshold);

                initialLuminanceTexture.EnableRenderTarget();

                // Render post process effect.
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();

                initialLuminanceTexture.DisableRenderTarget();
                return initialLuminanceTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Post Process Shader: Unable to generate the luminace texture.", e);
            }
        } // LuminanceTextureGeneration

        #endregion

        #region Luminance Adaptation
        
        /// <summary>
        /// Luminance Adaptation.
        /// </summary>
        public RenderTarget LuminanceAdaptation(Texture currentLuminanceTexture, RenderTarget lastLuminanceTexture, PostProcess postProcess)
        {
            if (currentLuminanceTexture == null || currentLuminanceTexture.Resource == null)
                throw new ArgumentNullException("currentLuminanceTexture");

            try
            {
                Resource.CurrentTechnique = Resource.Techniques["LuminanceAdaptation"];
                // Set render states
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                RenderTarget luminanceTexture = RenderTarget.Fetch(currentLuminanceTexture.Size, SurfaceFormat.Single, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing, true);

                // Set parameters
                SetHalfPixel(new Vector2(-1f / luminanceTexture.Width, 1f / luminanceTexture.Height));
                SetSceneTexture(currentLuminanceTexture);
                epTimeDelta.SetValue(Time.FrameTime); // It always changes.
                SetAutoExposureAdjustTimeMultiplier(postProcess.ToneMapping.AutoExposureAdaptationTimeMultiplier);
                
                // The two first frames will use the current luminance texture.
                // One because there is no last luminance texture, and two because I render a frame in the load content (to reduce garbage).
                if (lastLuminanceTexture != null)
                    SetLuminanceTexture(lastLuminanceTexture);
                else
                    SetLuminanceTexture(currentLuminanceTexture);

                luminanceTexture.EnableRenderTarget();

                // Render post process effect.
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();

                RenderTarget.DisableCurrentRenderTargets();
                // This luminance texture is not need anymore.
                RenderTarget.Release(lastLuminanceTexture);
                return luminanceTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Post Process Shader: Unable to generate the luminace texture.", e);
            }
        } // LuminanceAdaptation

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public void Render(Texture sceneTexture, PostProcess postProcess, RenderTarget bloomTexture, RenderTarget luminanceTexture)
        {
            if (sceneTexture == null || sceneTexture.Resource == null)
                throw new ArgumentNullException("sceneTexture");
            if (postProcess == null)
                throw new ArgumentNullException("postProcess");

            try
            {
                switch (postProcess.ToneMapping.ToneMappingFunction)
                {
                    case ToneMapping.ToneMappingFunctionEnumerate.FilmicALU        : Resource.CurrentTechnique = Resource.Techniques["PostProcessingFilmicALU"]; break;
                    case ToneMapping.ToneMappingFunctionEnumerate.FilmicUncharted2 : Resource.CurrentTechnique = Resource.Techniques["PostProcessingFilmicUncharted2"];
                        SetShoulderStrength(postProcess.ToneMapping.Uncharted2ShoulderStrength);
                        SetLinearStrength(postProcess.ToneMapping.Uncharted2LinearStrength);
                        SetLinearAngle(postProcess.ToneMapping.Uncharted2LinearAngle);
                        SetToeStrength(postProcess.ToneMapping.Uncharted2ToeStrength);
                        SetToeNumerator(postProcess.ToneMapping.Uncharted2ToeNumerator);
                        SetToeDenominator(postProcess.ToneMapping.Uncharted2ToeDenominator);
                        SetLinearWhite(postProcess.ToneMapping.Uncharted2LinearWhite);
                        break;
                    case ToneMapping.ToneMappingFunctionEnumerate.Reinhard         : Resource.CurrentTechnique = Resource.Techniques["PostProcessingReinhard"]; 
                        SetLuminanceSaturation(postProcess.ToneMapping.ToneMappingLuminanceSaturation);
                        break;
                    case ToneMapping.ToneMappingFunctionEnumerate.ReinhardModified : Resource.CurrentTechnique = Resource.Techniques["PostProcessingReinhardModified"];
                        SetLuminanceSaturation(postProcess.ToneMapping.ToneMappingLuminanceSaturation);
                        SetWhiteLevel(postProcess.ToneMapping.ToneMappingWhiteLevel);
                        break;
                    case ToneMapping.ToneMappingFunctionEnumerate.Exponential      : Resource.CurrentTechnique = Resource.Techniques["PostProcessingExponential"];
                        SetLuminanceSaturation(postProcess.ToneMapping.ToneMappingLuminanceSaturation);
                        SetWhiteLevel(postProcess.ToneMapping.ToneMappingWhiteLevel);
                        break;
                    case ToneMapping.ToneMappingFunctionEnumerate.Logarithmic      : Resource.CurrentTechnique = Resource.Techniques["PostProcessingLogarithmic"]; 
                        SetLuminanceSaturation(postProcess.ToneMapping.ToneMappingLuminanceSaturation);
                        SetWhiteLevel(postProcess.ToneMapping.ToneMappingWhiteLevel);
                        break;
                    case ToneMapping.ToneMappingFunctionEnumerate.DragoLogarithmic : Resource.CurrentTechnique = Resource.Techniques["PostProcessingDragoLogarithmic"]; 
                        SetLuminanceSaturation(postProcess.ToneMapping.ToneMappingLuminanceSaturation);
                        SetWhiteLevel(postProcess.ToneMapping.ToneMappingWhiteLevel);
                        SetBias(postProcess.ToneMapping.DragoBias);
                        break;
                    case ToneMapping.ToneMappingFunctionEnumerate.Duiker           : Resource.CurrentTechnique = Resource.Techniques["PostProcessingDuiker"]; break;
                }

                // Set render states
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                // Set parameters
                SetHalfPixel(new Vector2(-1f / sceneTexture.Width, 1f / sceneTexture.Height));
                SetSceneTexture(sceneTexture);

                #region Tone Mapping

                SetAutoExposure(postProcess.ToneMapping.AutoExposureEnabled);
                EngineManager.Device.SamplerStates[12] = SamplerState.PointClamp; // To avoid an exception. Long story.
                if (postProcess.ToneMapping.AutoExposureEnabled)
                    SetLuminanceTexture(luminanceTexture);
                else
                    SetLensExposure(postProcess.ToneMapping.LensExposure);

                #endregion

                #region Bloom

                if (postProcess.Bloom != null && postProcess.Bloom.Enabled)
                {
                    SetBloomEnabled(true);
                    SetBloomScale(postProcess.Bloom.Scale);
                    SetBloomTexture(bloomTexture);
                }
                else
                    SetBloomEnabled(false);

                #endregion

                #region Levels

                // Adjust Levels
                if (postProcess.AdjustLevels != null && postProcess.AdjustLevels.Enabled)
                {
                    SetAdjustLevelsEnabled(true);
                    SetInputBlack(postProcess.AdjustLevels.InputBlack);
                    SetInputWhite(postProcess.AdjustLevels.InputWhite);
                    SetInputGamma(postProcess.AdjustLevels.InputGamma);
                    SetOutputBlack(postProcess.AdjustLevels.OutputBlack);
                    SetOutputWhite(postProcess.AdjustLevels.OutputWhite);
                }
                else
                    SetAdjustLevelsEnabled(false);
                // Adjust Levels Individual Channels
                if (postProcess.AdjustLevelsIndividualChannels != null && postProcess.AdjustLevelsIndividualChannels.Enabled)
                {
                    SetAdjustLevelsIndividualChannelsEnabled(true);
                    SetInputBlackRgb(postProcess.AdjustLevelsIndividualChannels.InputBlack);
                    SetInputWhiteRgb(postProcess.AdjustLevelsIndividualChannels.InputWhite);
                    SetInputGammaRgb(postProcess.AdjustLevelsIndividualChannels.InputGamma);
                    SetOutputBlackRgb(postProcess.AdjustLevelsIndividualChannels.OutputBlack);
                    SetOutputWhiteRgb(postProcess.AdjustLevelsIndividualChannels.OutputWhite);
                }
                else
                    SetAdjustLevelsIndividualChannelsEnabled(false);

                #endregion
                
                #region Color Correction

                if (postProcess.ColorCorrection != null && postProcess.ColorCorrection.Enabled)
                {
                    if (postProcess.ColorCorrection.FirstLookupTable == null || postProcess.ColorCorrection.LerpOriginalColorAmount == 0)
                    {
                        // No color correction
                        SetColorCorrectOneLutEnabled(false);
                        SetColorCorrectTwoLutEnabled(false);
                    }
                    else
                    {
                        SetLookupTableScale(((float)(postProcess.ColorCorrection.FirstLookupTable.Size) - 1f) / (float)(postProcess.ColorCorrection.FirstLookupTable.Size));
                        SetLookupTableOffset(1f / (2f * (float)(postProcess.ColorCorrection.FirstLookupTable.Size)));
                        if (postProcess.ColorCorrection.SecondLookupTable == null || postProcess.ColorCorrection.LerpLookupTablesAmount == 0) 
                        {
                            // Lerp between two lookup tables
                            SetColorCorrectOneLutEnabled(true);
                            SetColorCorrectTwoLutEnabled(false);
                            SetFirstLookupTable(postProcess.ColorCorrection.FirstLookupTable);
                            SetLerpOriginalColorAmount(postProcess.ColorCorrection.LerpOriginalColorAmount);
                        }
                        else 
                        {
                            // One lookup table
                            SetColorCorrectOneLutEnabled(false);
                            SetColorCorrectTwoLutEnabled(true);
                            SetFirstLookupTable(postProcess.ColorCorrection.FirstLookupTable);
                            SetSecondLookupTable(postProcess.ColorCorrection.SecondLookupTable);
                            SetLerpOriginalColorAmount(postProcess.ColorCorrection.LerpOriginalColorAmount);
                            SetLerpLookupTablesAmount(postProcess.ColorCorrection.LerpLookupTablesAmount);
                        }
                    }
                }
                else
                {
                    SetColorCorrectOneLutEnabled(false);
                    SetColorCorrectTwoLutEnabled(false);
                }
                

                #endregion

                #region Film Grain

                if (postProcess.FilmGrain != null && postProcess.FilmGrain.Enabled && postProcess.FilmGrain.Strength != 0)
                {
                    SetFilmGrainEnabled(true);
                    SetFilmGrainStrength(postProcess.FilmGrain.Strength);
                    SetAccentuateDarkNoisePower(postProcess.FilmGrain.AccentuateDarkNoisePower);
                    SetRandomNoiseStrength(postProcess.FilmGrain.RandomNoiseStrength);
                    SetRandomValue(randomNumber.Next(1, 10000) / 100.0f);
                }
                else
                    SetFilmGrainEnabled(false);

                #endregion

                // Render post process effect.
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Post Process Shader: Unable to render.", e);
            }
        } // Render

        #endregion

    } // PostProcessingShader
} // XNAFinalEngine.Graphics
