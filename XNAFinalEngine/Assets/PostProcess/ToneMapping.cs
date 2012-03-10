
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

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Tone Mapping.
    /// Map one set of colors to another in order to approximate the appearance of high dynamic range images in a medium that has a more limited dynamic range.
    /// </summary>
    /// <remarks>
    /// References:
    ///   http://mynameismjp.wordpress.com/2010/04/30/a-closer-look-at-tone-mapping/
    ///   http://content.gpwiki.org/index.php/D3DBook:High-Dynamic_Range_Rendering
    ///   http://filmicgames.com/archives/75
    /// </remarks>
    public class ToneMapping
    {

        #region Enumerate

        /// <summary>
        /// The available tone mapping functions.
        /// </summary>
        public enum ToneMappingFunctionEnumerate
        {
            FilmicALU,
            FilmicUncharted2,
            Duiker,
            Reinhard,
            ReinhardModified,
            Exponential,
            Logarithmic,
            DragoLogarithmic
        } // ToneMappingFunctionEnumerate

        #endregion

        #region Variables

        // Is auto exposure enabled?
        private bool autoExposureEnabled = true;

        // Lens exposure.
        private float lensExposure = 1;

        // Controls how faster the camera’s auto exposure adaptation mechanism changes its response.
        private float exposureAdaptationTimeMultiplier = 0.5f;

        // When auto exposure is enabled, the luminance intensity is clamp using the low and high threshold.
        private float autoExposureLuminanceLowThreshold = 0.01f;
        private float autoExposureLuminanceHighThreshold = 20f;

        // Tone Mapping Parameters
        // Logarithmic
        float whiteLevel = 5;
        float luminanceSaturation = 1;
        // Drago
        float bias = 0.5f;
        // Uncharted 2
        float shoulderStrength = 0.22f;
        float linearStrength = 0.3f;
        float linearAngle = 0.1f;
        float toeStrength = 0.2f;
        float toeNumerator = 0.01f;
        float toeDenominator = 0.3f;
        float linearWhite = 11.2f;
      
        #endregion

        #region Properties

        /// <summary>
        /// Tone Mapping Function.
        /// </summary>
        public ToneMappingFunctionEnumerate ToneMappingFunction { get; set; }

        /// <summary>
        /// Lens exposure.
        /// </summary>
        public float LensExposure
        {
            get { return lensExposure; }
            set
            {
                lensExposure = value;
                if (lensExposure < 0)
                    lensExposure = 0;
            }
        } // LensExposure

        #region Auto Exposure

        /// <summary>
        /// Is auto exposure enabled?
        /// </summary>
        public bool AutoExposureEnabled
        {
            get { return autoExposureEnabled; }
            set { autoExposureEnabled = value; }
        } // AutoExposureEnabled

        /// <summary>
        /// Controls how faster the camera’s auto exposure adaptation mechanism changes its response.
        /// </summary>
        public float AutoExposureAdaptationTimeMultiplier
        {
            get { return exposureAdaptationTimeMultiplier; }
            set
            {
                exposureAdaptationTimeMultiplier = value;
                if (exposureAdaptationTimeMultiplier <= 0)
                    exposureAdaptationTimeMultiplier = 0.001f;
            }
        } // ExposureAdjustTimeMultiplier

        /// <summary>
        /// When auto exposure is enabled, the luminance intensity is clamp using a low and high threshold.
        /// </summary>
        public float AutoExposureLuminanceLowThreshold
        {
            get { return autoExposureLuminanceLowThreshold; }
            set
            {
                autoExposureLuminanceLowThreshold = value;
                if (autoExposureLuminanceLowThreshold < 0)
                    autoExposureLuminanceLowThreshold = 0f;
                if (autoExposureLuminanceLowThreshold > autoExposureLuminanceHighThreshold)
                    autoExposureLuminanceLowThreshold = autoExposureLuminanceHighThreshold;
            }
        } // AutoExposureLuminanceLowThreshold

        /// <summary>
        /// When auto exposure is enabled, the luminance intensity is clamp using a low and high threshold.
        /// </summary>
        public float AutoExposureLuminanceHighThreshold
        {
            get { return autoExposureLuminanceHighThreshold; }
            set
            {
                autoExposureLuminanceHighThreshold = value;
                if (autoExposureLuminanceHighThreshold < autoExposureLuminanceLowThreshold)
                    autoExposureLuminanceHighThreshold = autoExposureLuminanceLowThreshold;
            }
        } // LuminanceHighThreshold

        #endregion

        #region Common Parameters

        /// <summary>
        /// Logarithmic White Level.
        /// </summary>
        public float ToneMappingWhiteLevel
        {
            get { return whiteLevel; }
            set
            {
                whiteLevel = value;
                if (whiteLevel <= 0)
                    whiteLevel = 0.01f;
            }
        } // LogarithmicWhiteLevel

        /// <summary>
        /// Logarithmic luminance saturation.
        /// </summary>
        public float ToneMappingLuminanceSaturation
        {
            get { return luminanceSaturation; }
            set
            {
                luminanceSaturation = value;
                if (luminanceSaturation < 0)
                    luminanceSaturation = 0;
            }
        } // LogarithmicLuminanceSaturation

        #endregion

        #region Drago Parameters

        /// <summary>
        /// Drago Bias.
        /// </summary>
        public float DragoBias
        {
            get { return bias; }
            set
            {
                bias = value;
                if (bias < 0)
                    bias = 0;
            }
        } // DragoBias

        #endregion

        #region Uncharted 2 Parameters

        /// <summary>
        /// Shoulder Strength.
        /// </summary>
        public float Uncharted2ShoulderStrength
        {
            get { return shoulderStrength; }
            set
            {
                shoulderStrength = value;
                if (shoulderStrength < 0)
                    shoulderStrength = 0;
            }
        } // Uncharted2ShoulderStrength

        /// <summary>
        /// Linear Strength.
        /// </summary>
        public float Uncharted2LinearStrength
        {
            get { return linearStrength; }
            set
            {
                linearStrength = value;
                if (linearStrength < 0)
                    linearStrength = 0;
            }
        } // Uncharted2LinearStrength

        /// <summary>
        /// Linear Angle.
        /// </summary>
        public float Uncharted2LinearAngle
        {
            get { return linearAngle; }
            set
            {
                linearAngle = value;
                if (linearAngle < 0)
                    linearAngle = 0;
            }
        } // Uncharted2LinearAngle

        /// <summary>
        /// Toe Strength.
        /// </summary>
        public float Uncharted2ToeStrength
        {
            get { return toeStrength; }
            set
            {
                toeStrength = value;
                if (toeStrength < 0)
                    toeStrength = 0;
            }
        } // Uncharted2ToeStrength

        /// <summary>
        /// Toe Numerator.
        /// </summary>
        public float Uncharted2ToeNumerator
        {
            get { return toeNumerator; }
            set
            {
                toeNumerator = value;
                if (toeNumerator < 0)
                    toeNumerator = 0;
            }
        } // Uncharted2ToeNumerator

        /// <summary>
        /// Toe Denominator.
        /// </summary>
        public float Uncharted2ToeDenominator
        {
            get { return toeDenominator; }
            set
            {
                toeDenominator = value;
                if (toeDenominator < 0)
                    toeDenominator = 0;
            }
        } // Uncharted2ToeDenominator

        /// <summary>
        /// Linear White.
        /// </summary>
        public float Uncharted2LinearWhite
        {
            get { return linearWhite; }
            set
            {
                linearWhite = value;
                if (linearWhite < 0)
                    linearWhite = 0;
            }
        } // Uncharted2LinearWhite

        #endregion

        /// <summary>
        /// Luminance texture. Used in the adaptation pass.
        /// </summary>
        internal RenderTarget LuminanceTexture { get; set; }

        #endregion

    } // ToneMapping
} // XNAFinalEngine.Assets