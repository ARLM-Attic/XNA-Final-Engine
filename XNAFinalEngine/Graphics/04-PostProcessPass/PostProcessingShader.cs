    
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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

        private static Texture filmLutTexture;

        // Shader Parameters.
        private static ShaderParameterBool spAutoExposureEnabled,
                                           spBloomEnabled,
                                           spAdjustLevelsEnabled,
                                           spAdjustLevelsIndividualChannelsEnabled,
                                           spColorCorrectOneLutEnabled,
                                           spColorCorrectTwoLutEnabled,
                                           spFilmGrainEnabled;
        private static ShaderParameterFloat 
                                            // Exposure
                                            spLensExposure,
                                            spTimeDelta,
                                            spExposureAdjustTimeMultiplier,
                                            spLuminanceLowThreshold,
                                            spLuminanceHighThreshold,
                                            // Tone Mapping
                                            spWhiteLevel,
                                            spLuminanceSaturation,
                                            spBias,
                                            spShoulderStrength,
                                            spLinearStrength,
                                            spLinearAngle,
                                            spToeStrength,
                                            spToeNumerator,
                                            spToeDenominator,
                                            spLinearWhite,
                                            // Bloom
                                            spBloomScale,
                                            // Levels
                                            spInputBlack,
                                            spInputWhite,
                                            spInputGamma,
                                            spOutputBlack,
                                            spOutputWhite,
                                            // Color Correction
                                            spLookupTableScale,
                                            spLookupTableOffset,
                                            spLerpOriginalColorAmount,
                                            spLerpLookupTablesAmount,
                                            // Film Grain
                                            spFilmGrainStrength,
                                            spAccentuateDarkNoisePower,
                                            spRandomNoiseStrength,
                                            spRandomValue;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterVector3 // Leves Individual Channels
                                              spInputBlackRgb,
                                              spInputWhiteRgb,
                                              spInputGammaRgb,
                                              spOutputBlackRgb,
                                              spOutputWhiteRgb;
        private static ShaderParameterTexture spSceneTexture,
                                              spLastLuminanceTexture,
                                              spBloomTexture,
                                              spLensFlareTexture,
                                              spFilmLutTexture;
        private static ShaderParameterLookupTable spFirstlookupTable,
                                                  spSecondlookupTable;

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

        #region Constructors

        /// <summary>
        /// Über post processing shader.
        /// </summary>
        private PostProcessingShader() : base("PostProcessing\\PostProcessing")
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            filmLutTexture = new Texture("Shaders\\FilmLut");
            ContentManager.CurrentContentManager = userContentManager;
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
            //try
            {
                // Bool
                spAutoExposureEnabled = new ShaderParameterBool("autoExposure", this);
                spBloomEnabled = new ShaderParameterBool("bloomEnabled", this);
                spAdjustLevelsEnabled = new ShaderParameterBool("adjustLevelsEnabled", this);
                spAdjustLevelsIndividualChannelsEnabled = new ShaderParameterBool("adjustLevelsIndividualChannelsEnabled", this);
                spColorCorrectOneLutEnabled = new ShaderParameterBool("colorCorrectOneLutEnabled", this);
                spColorCorrectTwoLutEnabled = new ShaderParameterBool("colorCorrectTwoLutEnabled", this);
                spFilmGrainEnabled = new ShaderParameterBool("filmGrainEnabled", this);
                // Float
                // Exposure
                spLensExposure = new ShaderParameterFloat("lensExposure", this);
                spTimeDelta = new ShaderParameterFloat("timeDelta", this);
                spExposureAdjustTimeMultiplier = new ShaderParameterFloat("tau", this);
                spLuminanceLowThreshold = new ShaderParameterFloat("luminanceLowThreshold", this);
                spLuminanceHighThreshold = new ShaderParameterFloat("luminanceHighThreshold", this);
                // Tone Mapping
                spWhiteLevel = new ShaderParameterFloat("whiteLevel", this);
                spLuminanceSaturation = new ShaderParameterFloat("luminanceSaturation", this);
                spBias = new ShaderParameterFloat("bias", this);
                spShoulderStrength = new ShaderParameterFloat("shoulderStrength", this);
                spLinearStrength = new ShaderParameterFloat("linearStrength", this);
                spLinearAngle = new ShaderParameterFloat("linearAngle", this);
                spToeStrength = new ShaderParameterFloat("toeStrength", this);
                spToeNumerator = new ShaderParameterFloat("toeNumerator", this);
                spToeDenominator = new ShaderParameterFloat("toeDenominator", this);
                spLinearWhite = new ShaderParameterFloat("linearWhite", this);
                // Bloom
                spBloomScale = new ShaderParameterFloat("bloomScale", this);
                // Levels
                spInputBlack = new ShaderParameterFloat("inputBlack", this);
                spInputWhite = new ShaderParameterFloat("inputWhite", this);
                spInputGamma = new ShaderParameterFloat("inputGamma", this);
                spOutputBlack = new ShaderParameterFloat("outputBlack", this);
                spOutputWhite = new ShaderParameterFloat("outputWhite", this);
                // Color Correction
                spLookupTableScale = new ShaderParameterFloat("scale", this);
                spLookupTableOffset = new ShaderParameterFloat("offset", this);
                spLerpOriginalColorAmount = new ShaderParameterFloat("lerpOriginalColorAmount", this);
                spLerpLookupTablesAmount = new ShaderParameterFloat("lerpLookupTablesAmount", this);
                // Film Grain
                spFilmGrainStrength = new ShaderParameterFloat("filmGrainStrength", this);
                spAccentuateDarkNoisePower = new ShaderParameterFloat("accentuateDarkNoisePower", this);
                spRandomNoiseStrength = new ShaderParameterFloat("randomNoiseStrength", this);
                spRandomValue = new ShaderParameterFloat("randomValue", this);

                spHalfPixel = new ShaderParameterVector2("halfPixel", this);

                // Leves Individual Channels
                spInputBlackRgb = new ShaderParameterVector3("inputBlackRGB", this);
                spInputWhiteRgb = new ShaderParameterVector3("inputWhiteRGB", this);
                spInputGammaRgb = new ShaderParameterVector3("inputGammaRGB", this);
                spOutputBlackRgb = new ShaderParameterVector3("outputBlackRGB", this);
                spOutputWhiteRgb = new ShaderParameterVector3("outputWhiteRGB", this);

                spSceneTexture = new ShaderParameterTexture("sceneTexture", this, SamplerState.PointClamp, 9);
                spLastLuminanceTexture = new ShaderParameterTexture("lastLuminanceTexture", this, SamplerState.PointClamp, 12);
                spBloomTexture = new ShaderParameterTexture("bloomTexture", this, SamplerState.AnisotropicClamp, 10);
                spLensFlareTexture = new ShaderParameterTexture("lensFlareTexture", this, SamplerState.AnisotropicClamp, 8);
                spFilmLutTexture = new ShaderParameterTexture("filmLutTexture", this, SamplerState.AnisotropicClamp, 8);

                spFirstlookupTable = new ShaderParameterLookupTable("firstlookupTableTexture", this, SamplerState.LinearClamp, 6);
                spSecondlookupTable = new ShaderParameterLookupTable("secondlookupTableTexture", this, SamplerState.LinearClamp, 7);
            }
            /*catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }*/
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
                
                // Set parameters
                spHalfPixel.Value = new Vector2(-0.5f / (sceneTexture.Width / 2), 0.5f / (sceneTexture.Height / 2));
                spSceneTexture.Value = sceneTexture;
                spLuminanceLowThreshold.Value = postProcess.ToneMapping.AutoExposureLuminanceLowThreshold;
                spLuminanceHighThreshold.Value = postProcess.ToneMapping.AutoExposureLuminanceHighThreshold;

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

                RenderTarget luminanceTexture = RenderTarget.Fetch(currentLuminanceTexture.Size, SurfaceFormat.Single, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing, true);

                // Set parameters
                spHalfPixel.Value = new Vector2(-0.5f / (luminanceTexture.Width / 2), 0.5f / (luminanceTexture.Height / 2));
                spSceneTexture.Value = currentLuminanceTexture;
                spTimeDelta.Value = Time.FrameTime;
                spExposureAdjustTimeMultiplier.Value = postProcess.ToneMapping.AutoExposureAdaptationTimeMultiplier;
                
                // To avoid a problem in the first frame.
                if (lastLuminanceTexture != null && !lastLuminanceTexture.IsDisposed)
                    spLastLuminanceTexture.Value = lastLuminanceTexture;
                else
                    spLastLuminanceTexture.Value = currentLuminanceTexture;

                luminanceTexture.EnableRenderTarget();
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                RenderTarget.DisableCurrentRenderTargets();

                // This luminance texture is not needed anymore.
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
        public void Render(Texture sceneTexture, PostProcess postProcess, RenderTarget bloomTexture, RenderTarget lensFlareTexture, RenderTarget luminanceTexture)
        {
            if (sceneTexture == null || sceneTexture.Resource == null)
                throw new ArgumentNullException("sceneTexture");
            //try
            {
                if (postProcess != null)
                {
                    // Select technique with the tone mapping function.
                    switch (postProcess.ToneMapping.ToneMappingFunction)
                    {
                        case ToneMapping.ToneMappingFunctionEnumerate.FilmicALU        : Resource.CurrentTechnique = Resource.Techniques["PostProcessingFilmicALU"];
                            spFilmLutTexture.Value = filmLutTexture;
                            break;
                        case ToneMapping.ToneMappingFunctionEnumerate.FilmicUncharted2 : Resource.CurrentTechnique = Resource.Techniques["PostProcessingFilmicUncharted2"];
                            spShoulderStrength.Value = postProcess.ToneMapping.Uncharted2ShoulderStrength;
                            spLinearStrength.Value = postProcess.ToneMapping.Uncharted2LinearStrength;
                            spLinearAngle.Value = postProcess.ToneMapping.Uncharted2LinearAngle;
                            spToeStrength.Value = postProcess.ToneMapping.Uncharted2ToeStrength;
                            spToeNumerator.Value = postProcess.ToneMapping.Uncharted2ToeNumerator;
                            spToeDenominator.Value = postProcess.ToneMapping.Uncharted2ToeDenominator;
                            spLinearWhite.Value = postProcess.ToneMapping.Uncharted2LinearWhite;
                            break;
                        case ToneMapping.ToneMappingFunctionEnumerate.Reinhard         : Resource.CurrentTechnique = Resource.Techniques["PostProcessingReinhard"];
                            spLuminanceSaturation.Value = postProcess.ToneMapping.ToneMappingLuminanceSaturation;
                            break;
                        case ToneMapping.ToneMappingFunctionEnumerate.ReinhardModified : Resource.CurrentTechnique = Resource.Techniques["PostProcessingReinhardModified"];
                            spLuminanceSaturation.Value = postProcess.ToneMapping.ToneMappingLuminanceSaturation;
                            spWhiteLevel.Value = postProcess.ToneMapping.ToneMappingWhiteLevel;
                            break;
                        case ToneMapping.ToneMappingFunctionEnumerate.Exponential      : Resource.CurrentTechnique = Resource.Techniques["PostProcessingExponential"];
                            spLuminanceSaturation.Value = postProcess.ToneMapping.ToneMappingLuminanceSaturation;
                            spWhiteLevel.Value = postProcess.ToneMapping.ToneMappingWhiteLevel;
                            break;
                        case ToneMapping.ToneMappingFunctionEnumerate.Logarithmic      : Resource.CurrentTechnique = Resource.Techniques["PostProcessingLogarithmic"];
                            spLuminanceSaturation.Value = postProcess.ToneMapping.ToneMappingLuminanceSaturation;
                            spWhiteLevel.Value = postProcess.ToneMapping.ToneMappingWhiteLevel;
                            break;
                        case ToneMapping.ToneMappingFunctionEnumerate.DragoLogarithmic : Resource.CurrentTechnique = Resource.Techniques["PostProcessingDragoLogarithmic"];
                            spLuminanceSaturation.Value = postProcess.ToneMapping.ToneMappingLuminanceSaturation;
                            spWhiteLevel.Value = postProcess.ToneMapping.ToneMappingWhiteLevel;
                            spBias.Value = postProcess.ToneMapping.DragoBias;
                            break;
                        case ToneMapping.ToneMappingFunctionEnumerate.Duiker           : Resource.CurrentTechnique = Resource.Techniques["PostProcessingDuiker"]; break;
                    }
                    // Set parameters
                    spHalfPixel.Value = new Vector2(-0.5f / (sceneTexture.Width / 2), 0.5f / (sceneTexture.Height / 2));
                    spSceneTexture.Value = sceneTexture;

                    #region Tone Mapping

                    spAutoExposureEnabled.Value = postProcess.ToneMapping.AutoExposureEnabled;
                    if (postProcess.ToneMapping.AutoExposureEnabled)
                        spLastLuminanceTexture.Value = luminanceTexture;
                    else
                        spLensExposure.Value = postProcess.ToneMapping.LensExposure;

                    #endregion

                    #region Bloom

                    if (postProcess.Bloom != null && postProcess.Bloom.Enabled)
                    {
                        spBloomEnabled.Value = true;
                        spBloomScale.Value = postProcess.Bloom.Scale;
                        spBloomTexture.Value = bloomTexture;
                    }
                    else
                        spBloomEnabled.Value = false;

                    #endregion

                    #region Lens Flare

                    spLensFlareTexture.Value = lensFlareTexture ?? Texture.BlackTexture;

                    #endregion

                    #region Levels

                    // Adjust Levels
                    if (postProcess.AdjustLevels != null && postProcess.AdjustLevels.Enabled)
                    {
                        spAdjustLevelsEnabled.Value = true;
                        spInputBlack.Value = postProcess.AdjustLevels.InputBlack;
                        spInputWhite.Value = postProcess.AdjustLevels.InputWhite;
                        spInputGamma.Value = postProcess.AdjustLevels.InputGamma;
                        spOutputBlack.Value = postProcess.AdjustLevels.OutputBlack;
                        spOutputWhite.Value = postProcess.AdjustLevels.OutputWhite;
                    }
                    else
                        spAdjustLevelsEnabled.Value = false;
                    // Adjust Levels Individual Channels
                    if (postProcess.AdjustLevelsIndividualChannels != null && postProcess.AdjustLevelsIndividualChannels.Enabled)
                    {
                        spAdjustLevelsIndividualChannelsEnabled.Value = true;
                        spInputBlackRgb.Value = postProcess.AdjustLevelsIndividualChannels.InputBlack;
                        spInputWhiteRgb.Value = postProcess.AdjustLevelsIndividualChannels.InputWhite;
                        spInputGammaRgb.Value = postProcess.AdjustLevelsIndividualChannels.InputGamma;
                        spOutputBlackRgb.Value = postProcess.AdjustLevelsIndividualChannels.OutputBlack;
                        spOutputWhiteRgb.Value = postProcess.AdjustLevelsIndividualChannels.OutputWhite;
                    }
                    else
                        spAdjustLevelsIndividualChannelsEnabled.Value = false;

                    #endregion

                    #region Color Correction

                    if (postProcess.ColorCorrection != null && postProcess.ColorCorrection.Enabled)
                    {
                        if (postProcess.ColorCorrection.FirstLookupTable == null || postProcess.ColorCorrection.LerpOriginalColorAmount == 0)
                        {
                            // No color correction
                            spColorCorrectOneLutEnabled.Value = false;
                            spColorCorrectTwoLutEnabled.Value = false;
                        }
                        else
                        {
                            spLookupTableScale.Value = ((float)(postProcess.ColorCorrection.FirstLookupTable.Size) - 1f) / (float)(postProcess.ColorCorrection.FirstLookupTable.Size);
                            spLookupTableOffset.Value = 1f / (2f * (float)(postProcess.ColorCorrection.FirstLookupTable.Size));
                            if (postProcess.ColorCorrection.SecondLookupTable == null || postProcess.ColorCorrection.LerpLookupTablesAmount == 0)
                            {
                                // Lerp between two lookup tables
                                spColorCorrectOneLutEnabled.Value = true;
                                spColorCorrectTwoLutEnabled.Value = false;
                                spFirstlookupTable.Value = postProcess.ColorCorrection.FirstLookupTable;
                                spLerpOriginalColorAmount.Value = postProcess.ColorCorrection.LerpOriginalColorAmount;
                            }
                            else
                            {
                                // One lookup table
                                spColorCorrectOneLutEnabled.Value = false;
                                spColorCorrectTwoLutEnabled.Value = true;
                                spFirstlookupTable.Value = postProcess.ColorCorrection.FirstLookupTable;
                                spSecondlookupTable.Value = postProcess.ColorCorrection.SecondLookupTable;
                                spLerpOriginalColorAmount.Value = postProcess.ColorCorrection.LerpOriginalColorAmount;
                                spLerpLookupTablesAmount.Value = postProcess.ColorCorrection.LerpLookupTablesAmount;
                            }
                        }
                    }
                    else
                    {
                        spColorCorrectOneLutEnabled.Value = false;
                        spColorCorrectTwoLutEnabled.Value = false;
                    }


                    #endregion

                    #region Film Grain

                    if (postProcess.FilmGrain != null && postProcess.FilmGrain.Enabled && postProcess.FilmGrain.Strength != 0)
                    {
                        spFilmGrainEnabled.Value = true;
                        spFilmGrainStrength.Value = postProcess.FilmGrain.Strength;
                        spAccentuateDarkNoisePower.Value = postProcess.FilmGrain.AccentuateDarkNoisePower;
                        spRandomNoiseStrength.Value = postProcess.FilmGrain.RandomNoiseStrength;
                        spRandomValue.Value = randomNumber.Next(1, 10000) / 100.0f;
                    }
                    else
                        spFilmGrainEnabled.Value = false;

                    #endregion

                    // Render post process effect.
                    Resource.CurrentTechnique.Passes[0].Apply();
                    RenderScreenPlane();
                }
                else
                {
                    SpriteManager.DrawTextureToFullScreen(sceneTexture, false);
                }
            }
            /*catch (Exception e)
            {
                throw new InvalidOperationException("Post Process Shader: Unable to render.", e);
            }*/
        } // Render

        #endregion

    } // PostProcessingShader
} // XNAFinalEngine.Graphics