
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Ocean material.
    /// Simple ocean shader with animated bump map and geometric waves.
    /// Based partly on "Effective Water Simulation From Physical Models", GPU Gems.
    /// </summary>
    public class Ocean : Material
    {

        #region Shader Parameters

        #region Variables

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               // Matrices // 
                               epWorldIT,
                               epWorldViewProj,
                               epWorld,
                               epViewI,
                               // Surface //
                               epBumpHeight,
                               epTextureRepeatX,
                               epTextureRepeatY,
                               epBumpSpeedX,
                               epBumpSpeedY,
                               epFresnelBias,
                               epFresnelExponent,
                               epHDRMultiplier,
                               epDeepWater,
                               epShallowWater,
                               epReflectionTint,
                               epReflectionStrength,
                               epWaterColorStrength,
                               epWaveAmplitude,
                               epWaveFrequency,
                               // Textures //
                               epCubeMapTexture,
                               epNormalTexture,
                               // Time //
                               epTime;

        #endregion

        #region Matrices

        /// <summary>
        /// Last used transpose inverse world matrix
        /// </summary>
        private static Matrix? lastUsedTransposeInverseWorldMatrix = null;
        /// <summary>
        /// Set transpose inverse world matrix
        /// </summary>
        private Matrix TransposeInverseWorldMatrix
        {
            set
            {
                if (lastUsedTransposeInverseWorldMatrix != value)
                {
                    lastUsedTransposeInverseWorldMatrix = value;
                    epWorldIT.SetValue(value);
                }
            }
        } // TransposeInvertWorldMatrix

        /// <summary>
        /// Last used world matrix
        /// </summary>
        private static Matrix? lastUsedWorldMatrix = null;
        /// <summary>
        /// Set world matrix
        /// </summary>
        private Matrix WorldMatrix
        {
            set
            {
                if (lastUsedWorldMatrix != value)
                {
                    lastUsedWorldMatrix = value;
                    epWorld.SetValue(value);
                }
            }
        } // WorldMatrix

        /// <summary>
        /// Last used inverse view matrix
        /// </summary>
        private static Matrix? lastUsedInverseViewMatrix = null;
        /// <summary>
        /// Set view inverse matrix
        /// </summary>
        private Matrix InverseViewMatrix
        {
            set
            {
                if (lastUsedInverseViewMatrix != value)
                {
                    lastUsedInverseViewMatrix = value;
                    epViewI.SetValue(value);
                }
            }
        } // InverseViewMatrix

        /// <summary>
        /// Last used world view projection matrix
        /// </summary>
        private static Matrix? lastUsedWorldViewProjMatrix = null;
        /// <summary>
        /// Set world view projection matrix
        /// </summary>
        private Matrix WorldViewProjMatrix
        {
            set
            {
                if (lastUsedWorldViewProjMatrix != value)
                {
                    lastUsedWorldViewProjMatrix = value;
                    epWorldViewProj.SetValue(value);
                }
            }
        } // WorldViewProjMatrix
                
        #endregion

        #region Surface

        #region Bump Height

        /// <summary>
        /// Bump Height
        /// </summary>
        private float bumpHeight = 1.4f;

        /// <summary>
        /// Bump Height
        /// </summary>
        public float BumpHeight
        {
            get { return bumpHeight; }
            set { bumpHeight = value; }
        } // BumpHeight

        /// <summary>
        /// Last used Bump Height
        /// </summary>
        private static float? lastUsedBumpHeight = null;
        /// <summary>
        /// Set Bump Height (greater or equal to 0)
        /// </summary>
        private void SetBumpHeight(float _bumpHeight)
        {
            if (lastUsedBumpHeight != _bumpHeight && _bumpHeight >= 0.0f)
            {
                lastUsedBumpHeight = _bumpHeight;
                epBumpHeight.SetValue(_bumpHeight);
            } // if
        } // SetBumpHeight

        #endregion

        #region Texture Repeat X

        /// <summary>
        /// Texture Repeat X
        /// </summary>
        private float textureRepeatX = 10.0f;

        /// <summary>
        /// Texture Repeat X
        /// </summary>
        public float TextureRepeatX
        {
            get { return textureRepeatX; }
            set { textureRepeatX = value; }
        } // TextureRepeatX

        /// <summary>
        /// Last used Texture Repeat X
        /// </summary>
        private static float? lastUsedTextureRepeatX = null;
        /// <summary>
        /// Set Texture Repeat X (greater or equal to 0)
        /// </summary>
        private void SetTextureRepeatX(float _textureRepeatX)
        {
            if (lastUsedTextureRepeatX != _textureRepeatX && _textureRepeatX >= 0.0f)
            {
                lastUsedTextureRepeatX = _textureRepeatX;
                epTextureRepeatX.SetValue(_textureRepeatX);
            } // if
        } // SetTextureRepeatX

        #endregion

        #region Texture Repeat Y

        /// <summary>
        /// Texture Repeat Y
        /// </summary>
        private float textureRepeatY = 4.0f;

        /// <summary>
        /// Texture Repeat Y
        /// </summary>
        public float TextureRepeatY
        {
            get { return textureRepeatY; }
            set { textureRepeatY = value; }
        } // TextureRepeatY

        /// <summary>
        /// Last used Texture Repeat Y
        /// </summary>
        private static float? lastUsedTextureRepeatY = null;
        /// <summary>
        /// Set Texture Repeat Y (greater or equal to 0)
        /// </summary>
        private void SetTextureRepeatY(float _textureRepeatY)
        {
            if (lastUsedTextureRepeatY != _textureRepeatY && _textureRepeatY >= 0.0f)
            {
                lastUsedTextureRepeatY = _textureRepeatY;
                epTextureRepeatY.SetValue(_textureRepeatY);
            } // if
        } // SetTextureRepeatY

        #endregion

        #region Bump Speed X

        /// <summary>
        /// Bump Speed X
        /// </summary>
        private float bumpSpeedX = -0.05f;

        /// <summary>
        /// Bump Speed X
        /// </summary>
        public float BumpSpeedX
        {
            get { return bumpSpeedX; }
            set { bumpSpeedX = value; }
        } // BumpSpeedX

        /// <summary>
        /// Last used Bump Speed X
        /// </summary>
        private static float? lastUsedBumpSpeedX = null;
        /// <summary>
        /// Set Bump Speed X
        /// </summary>
        private void SetBumpSpeedX(float _bumpSpeedX)
        {
            if (lastUsedBumpSpeedX != _bumpSpeedX)
            {
                lastUsedBumpSpeedX = _bumpSpeedX;
                epBumpSpeedX.SetValue(_bumpSpeedX);
            } // if
        } // SetBumpSpeedX

        #endregion

        #region Bump Speed Y

        /// <summary>
        /// Bump Speed Y
        /// </summary>
        private float bumpSpeedY = 0.0f;

        /// <summary>
        /// Bump Speed Y
        /// </summary>
        public float BumpSpeedY
        {
            get { return bumpSpeedY; }
            set { bumpSpeedY = value; }
        } // BumpSpeedY

        /// <summary>
        /// Last used Bump Speed Y
        /// </summary>
        private static float? lastUsedBumpSpeedY = null;
        /// <summary>
        /// Set Bump Speed Y
        /// </summary>
        private void SetBumpSpeedY(float _bumpSpeedY)
        {
            if (lastUsedBumpSpeedY != _bumpSpeedY)
            {
                lastUsedBumpSpeedY = _bumpSpeedY;
                epBumpSpeedY.SetValue(_bumpSpeedY);
            } // if
        } // SetBumpSpeedY

        #endregion

        #region Fresnel Bias

        /// <summary>
        /// Fresnel Bias
        /// </summary>
        private float fresnelBias = 0.1f;

        /// <summary>
        /// Fresnel Bias
        /// </summary>
        public float FresnelBias
        {
            get { return fresnelBias; }
            set { fresnelBias = value; }
        } // FresnelBias

        /// <summary>
        /// Last used FresnelBias
        /// </summary>
        private static float? lastUsedFresnelBias = null;
        /// <summary>
        /// Set FresnelBias (greater or equal to 0)
        /// </summary>
        private void SetFresnelBias(float _fresnelBias)
        {
            if (lastUsedFresnelBias != _fresnelBias && _fresnelBias >= 0.0f)
            {
                lastUsedFresnelBias = _fresnelBias;
                epFresnelBias.SetValue(_fresnelBias);
            }
        } // SetFresnelBias

        #endregion

        #region Fresnel Exponent

        /// <summary>
        /// Fresnel Exponent
        /// </summary>
        private float fresnelExponent = 4.0f;

        /// <summary>
        /// Fresnel Exponent
        /// </summary>
        public float FresnelExponent
        {
            get { return fresnelExponent; }
            set { fresnelExponent = value; }
        } // FresnelExponent

        /// <summary>
        /// Last used Fresnel Exponent
        /// </summary>
        private static float? lastUsedFresnelExponent = null;
        /// <summary>
        /// Set Fresnel Exponent (greater or equal to 0)
        /// </summary>
        private void SetFresnelExponent(float _fresnelExponent)
        {
            if (lastUsedFresnelExponent != _fresnelExponent && _fresnelExponent >= 0.0f)
            {
                lastUsedFresnelExponent = _fresnelExponent;
                epFresnelExponent.SetValue(_fresnelExponent);
            }
        } // SetFresnelExponent

        #endregion

        #region HDR Multiplier
        /// <summary>
        /// HDR Multiplier
        /// </summary>
        private float hdrMultiplier = 3.0f;

        /// <summary>
        /// HDR Multiplier
        /// </summary>
        public float HDRMultiplier
        {
            get { return hdrMultiplier; }
            set { hdrMultiplier = value; }
        } // HDRMultiplier

        /// <summary>
        /// Last used HDR Multiplier
        /// </summary>
        private static float? lastUsedHDRMultiplier = null;
        /// <summary>
        /// Set HDR Multiplier (greater or equal to 0)
        /// </summary>
        private void SetHDRMultiplier(float _hdrMultiplier)
        {
            if (lastUsedHDRMultiplier != _hdrMultiplier && _hdrMultiplier >= 0.0f)
            {
                lastUsedHDRMultiplier = _hdrMultiplier;
                epHDRMultiplier.SetValue(_hdrMultiplier);
            }
        } // SetHDRMultiplier

        #endregion

        #region Deep Water

        /// <summary>
        /// Deep Water Color
        /// </summary>
        private Color deepWater = new Color(0, 0, 25);

        /// <summary>
        /// Deep Water Color
        /// </summary>
        public Color DeepWater 
        {
            get { return deepWater; }
            set { deepWater = value; }
        } // DeepWater

        /// <summary>
        /// Last used Deep Water Color
        /// </summary>
        private static Color? lastUsedDeepWater = null;
        /// <summary>
        /// Set Deep Water Color
        /// </summary>
        private void SetDeepWater(Color _deepWater)
        {
            if (lastUsedDeepWater != _deepWater)
            {
                lastUsedDeepWater = _deepWater;
                epDeepWater.SetValue(new Vector3(_deepWater.R / 255f, _deepWater.G / 255f, _deepWater.B / 255f));
            }
        } // SetDeepWater

        #endregion

        #region Shallow Water

        /// <summary>
        /// Shallow Water
        /// </summary>
        private Color shallowWater = new Color(0, 128, 128);

        /// <summary>
        /// Shallow Water
        /// </summary>
        public Color ShallowWater
        {
            get { return shallowWater; }
            set { shallowWater = value; }
        } // ShallowWater

        /// <summary>
        /// Last used Shallow Water
        /// </summary>
        private static Color? lastUsedShallowWater = null;
        /// <summary>
        /// Set Shallow Water
        /// </summary>
        private void SetShallowWater(Color _shallowWater)
        {
            if (lastUsedShallowWater != _shallowWater)
            {
                lastUsedShallowWater = _shallowWater;
                epShallowWater.SetValue(new Vector3(_shallowWater.R / 255f, _shallowWater.G / 255f, _shallowWater.B / 255f));
            } // if
        } // SetShallowWater

        #endregion

        #region Reflection Tint

        /// <summary>
        /// Reflection Tint
        /// </summary>
        private Color reflectionTint = new Color(76, 76, 76);

        /// <summary>
        /// Reflection Tint
        /// </summary>
        public Color ReflectionTint
        {
            get { return reflectionTint; }
            set { reflectionTint = value; }
        } // ReflectionTint

        /// <summary>
        /// Last used Reflection Tint
        /// </summary>
        private static Color? lastUsedReflectionTint = null;
        /// <summary>
        /// Set Reflection Tint
        /// </summary>
        private void SetReflectionTint(Color _reflectionTint)
        {
            if (lastUsedReflectionTint != _reflectionTint)
            {
                lastUsedReflectionTint = _reflectionTint;
                epReflectionTint.SetValue(new Vector3(_reflectionTint.R / 255f, _reflectionTint.G / 255f, _reflectionTint.B / 255f));
            } // if
        } // SetReflectionTint

        #endregion

        #region Reflection Strength

        /// <summary>
        /// Reflection Strength
        /// </summary>
        private float reflectionStrength = 1.0f;

        /// <summary>
        /// Reflection Strength
        /// </summary>
        public float ReflectionStrength
        {
            get { return reflectionStrength; }
            set { reflectionStrength = value; }
        } // ReflectionStrength

        /// <summary>
        /// Last used Reflection Strength
        /// </summary>
        private static float? lastUsedReflectionStrength = null;
        /// <summary>
        /// Set Reflection Strength (greater or equal to 0)
        /// </summary>
        private void SetReflectionStrength(float _reflectionStrength)
        {
            if (lastUsedReflectionStrength != _reflectionStrength && _reflectionStrength >= 0.0f)
            {
                lastUsedReflectionStrength = _reflectionStrength;
                epReflectionStrength.SetValue(_reflectionStrength);
            } // if
        } // SetReflectionStrength

        #endregion

        #region Water Color Strength

        /// <summary>
        /// Water Color Strength
        /// </summary>
        private float waterColorStrength = 0.3f;

        /// <summary>
        /// Water Color Strength
        /// </summary>
        public float WaterColorStrength
        {
            get { return waterColorStrength; }
            set { waterColorStrength = value; }
        } // WaterColorStrength

        /// <summary>
        /// Last used Water Color Strength
        /// </summary>
        private static float? lastUsedWaterColorStrength = null;
        /// <summary>
        /// Set Water Color Strength (greater or equal to 0)
        /// </summary>
        private void SetWaterColorStrength(float _waterColorStrength)
        {
            if (lastUsedWaterColorStrength != _waterColorStrength && _waterColorStrength >= 0.0f)
            {
                lastUsedWaterColorStrength = _waterColorStrength;
                epWaterColorStrength.SetValue(_waterColorStrength);
            } // if
        } // SetWaterColorStrength

        #endregion

        #region Wave Amplitude

        /// <summary>
        /// Wave Amplitude
        /// </summary>
        private float waveAmplitude = 0.05f;

        /// <summary>
        /// Wave Amplitude
        /// </summary>
        public float WaveAmplitude
        {
            get { return waveAmplitude; }
            set { waveAmplitude = value; }
        } // WaveAmplitude

        /// <summary>
        /// Last used Wave Amplitude
        /// </summary>
        private static float? lastUsedWaveAmplitude = null;
        /// <summary>
        /// Set Wave Amplitude (greater or equal to 0)
        /// </summary>
        private void SetWaveAmplitude(float _waveAmplitude)
        {
            if (lastUsedWaveAmplitude != _waveAmplitude && _waveAmplitude >= 0.0f)
            {
                lastUsedWaveAmplitude = _waveAmplitude;
                epWaveAmplitude.SetValue(_waveAmplitude);
            } // if
        } // SetWaveAmplitude

        #endregion

        #region Wave Frequency

        /// <summary>
        /// Wave Frequency
        /// </summary>
        private float waveFrequency = 3.0f;

        /// <summary>
        /// Wave Frequency
        /// </summary>
        public float WaveFrequency
        {
            get { return waveFrequency; }
            set { waveFrequency = value; }
        } // WaveFrequency

        /// <summary>
        /// Last used Wave Frequency
        /// </summary>
        private static float? lastUsedWaveFrequency = null;
        /// <summary>
        /// Set Wave Frequency (greater or equal to 0)
        /// </summary>
        private void SetWaveFrequency(float _waveFrequency)
        {
            if (lastUsedWaveFrequency != _waveFrequency && _waveFrequency >= 0.0f)
            {
                lastUsedWaveFrequency = _waveFrequency;
                epWaveFrequency.SetValue(_waveFrequency);
            } // if
        } // SetWaveFrequency

        #endregion

        #endregion

        #region Textures

        #region Environment Texture

        /// <summary>
        /// Reflection texture (cube map)
        /// </summary>
        private TextureCube reflectionTexture = null;

        /// <summary>
        /// Reflection texture (cube map)
        /// </summary>
        public void ReflectionTexture(string _reflectionTexture)
        {
            string fullFilename = Directories.TexturesDirectory + "\\" + _reflectionTexture;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load texture: File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                if (EngineManager.UsesSystemContent)
                    reflectionTexture = EngineManager.SystemContent.Load<TextureCube>(fullFilename);
                else
                    reflectionTexture = EngineManager.CurrentContent.Load<TextureCube>(fullFilename);

            } // try
            catch (Exception)
            {
                throw new Exception("Failed to load the cube texture");
            }
        } // ReflectionTexture

        /// <summary>
        /// Last used reflection texture
        /// </summary>
        private TextureCube lastUsedReflectionTexture = null;
        /// <summary>
        /// Set reflection texture
        /// </summary>
        private void SetReflectionTexture(TextureCube _reflectionTexture)
        {
            if (_reflectionTexture != null && lastUsedReflectionTexture != _reflectionTexture)
            {
                lastUsedReflectionTexture = _reflectionTexture;
                epCubeMapTexture.SetValue(_reflectionTexture);
            }
        } // SetReflectionTexture

        #endregion

        #region Normal

        /// <summary>
        /// Normal texture
        /// </summary>
        private Texture normalTexture = null;

        /// <summary>
        /// Normal texture
        /// </summary>
        public Texture NormalTexture
        {
            get { return normalTexture; }
            set { normalTexture = value; }
        } // NormalTexture

        /// <summary>
        /// Last used Normal texture
        /// </summary>
        private static Texture lastUsedNormalTexture = null;
        /// <summary>
        /// Set normal texture
        /// </summary>
        private void SetNormalTexture(Texture _normalTexture)
        {
            if (lastUsedNormalTexture != _normalTexture)
            {
                lastUsedNormalTexture = _normalTexture;
                epNormalTexture.SetValue(_normalTexture.XnaTexture);
            } // if
        } // SetNormalTexture

        #endregion

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Ocean material.
		/// </summary>
		public Ocean()
		{
            Effect = LoadShader("Ocean");
            
            GetParametersHandles();

            // Load the default reflection texture
            ReflectionTexture("OceanEnviromentMapCloudyHills");
            // Load the default normal map waves texture
            normalTexture = new Texture("OceanWavesNormal");
                        
            LoadUITestElements();
        } // Ocean
        
        #endregion
        
		#region Get Parameters Handles
		
        /// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected override void GetParametersHandles()
		{
			try
			{
                // Matrices //
                epWorldIT = Effect.Parameters["WorldIT"];
                epWorldViewProj = Effect.Parameters["WorldViewProj"];
                epWorld = Effect.Parameters["World"];
                epViewI = Effect.Parameters["ViewI"];
                // Textures //
                epCubeMapTexture = Effect.Parameters["EnvTexture"];
                epNormalTexture = Effect.Parameters["NormalTexture"];
                // Time //
                epTime = Effect.Parameters["Timer"];
                // Surface //
                epBumpHeight = Effect.Parameters["BumpScale"];
                epTextureRepeatX = Effect.Parameters["TexReptX"];
                epTextureRepeatY = Effect.Parameters["TexReptY"];
                epBumpSpeedX = Effect.Parameters["BumpSpeedX"];
                epBumpSpeedY = Effect.Parameters["BumpSpeedY"];
                epFresnelBias = Effect.Parameters["FresnelBias"];
                epFresnelExponent = Effect.Parameters["FresnelExp"];
                epHDRMultiplier = Effect.Parameters["HDRMultiplier"];
                epDeepWater = Effect.Parameters["DeepColor"];
                epShallowWater = Effect.Parameters["ShallowColor"];
                epReflectionTint = Effect.Parameters["ReflTint"];
                epReflectionStrength = Effect.Parameters["Kr"];
                epWaterColorStrength = Effect.Parameters["KWater"];
                epWaveAmplitude = Effect.Parameters["WaveAmp"];
                epWaveFrequency = Effect.Parameters["WaveFreq"];
			} // try
			catch
			{
                throw new Exception("Get the handles from the ocean material failed.");
			} // catch
		} // GetParametersHandles

		#endregion

        #region Render

        /// <summary>
        /// Render this shader/material; to do this job it takes an object model, its associated lights, and its matrices.
		/// </summary>		
        internal override void Render(Matrix worldMatrix, PointLight[] pointLight, DirectionalLight[] directionalLight, SpotLight[] spotLight, Model model)
        {

            #region Set Parameters

            try
            {
                // Matrices //
                TransposeInverseWorldMatrix = Matrix.Transpose(Matrix.Invert(worldMatrix));
                WorldMatrix = worldMatrix;
                WorldViewProjMatrix = worldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix;
                InverseViewMatrix = Matrix.Invert(ApplicationLogic.Camera.ViewMatrix);
                // Textures //
                SetReflectionTexture(reflectionTexture);
                SetNormalTexture(normalTexture);
                // Time //
                epTime.SetValue((float)EngineManager.TotalTime / 2);
                // Surface //
                SetBumpHeight(bumpHeight);
                SetTextureRepeatX(textureRepeatX);
                SetTextureRepeatY(textureRepeatY);
                SetBumpSpeedX(bumpSpeedX);
                SetBumpSpeedY(bumpSpeedY);
                SetFresnelBias(fresnelBias);
                SetFresnelExponent(fresnelExponent);
                SetHDRMultiplier(hdrMultiplier);
                SetDeepWater(deepWater);
                SetShallowWater(shallowWater);
                SetReflectionTint(reflectionTint);
                SetReflectionStrength(reflectionStrength);
                SetWaterColorStrength(waterColorStrength);
                SetWaveAmplitude(waveAmplitude);
                SetWaveFrequency(waveFrequency);
            }
            catch
            {
                throw new Exception("Unable to set the ocean material parameters.");
            }

            #endregion

            base.Render(model);
        } // Render

		#endregion         

        #region Test
        
        #region Variables
                       
        private UISliderNumeric uiBumpHeight,
                                uiTextureRepeatX,
                                uiTextureRepeatY,
                                uiBumpSpeedX,
                                uiBumpSpeedY,
                                uiFresnelBias,
                                uiFresnelExponent,
                                uiHDRMultiplier,
                                uiReflectionStrength,
                                uiWaterColorStrength,
                                uiWaveAmplitude,
                                uiWaveFrequency;
        private UISliderColor   uiDeepWater,
                                uiShallowWater,
                                uiReflectionTint;
                                
        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        protected override void LoadUITestElements()
        {
            uiBumpHeight         = new UISliderNumeric("Bump Height",          new Vector2(EngineManager.Width - 390, 110), 0.0f, 2.0f, 0.01f, BumpHeight);
            uiTextureRepeatX     = new UISliderNumeric("Texture Repeat X",     new Vector2(EngineManager.Width - 390, 150), 0.0f, 16.0f, 0.1f, TextureRepeatX);
            uiTextureRepeatY     = new UISliderNumeric("Texture Repeat Y",     new Vector2(EngineManager.Width - 390, 190), 0.0f, 16.0f, 0.1f, TextureRepeatY);
            uiBumpSpeedX         = new UISliderNumeric("Bump Speed X",         new Vector2(EngineManager.Width - 390, 230), -0.2f, 0.2f, 0.001f, BumpSpeedX);
            uiBumpSpeedY         = new UISliderNumeric("Bump Speed Y",         new Vector2(EngineManager.Width - 390, 270), -0.2f, 0.2f, 0.001f, BumpSpeedY);
            uiFresnelBias        = new UISliderNumeric("Fresnel Bias",         new Vector2(EngineManager.Width - 390, 310), 0.0f, 1.0f, 0.01f, FresnelBias);
            uiFresnelExponent    = new UISliderNumeric("Fresnel Exponent",     new Vector2(EngineManager.Width - 390, 350), 0.0f, 5.0f, 0.01f, FresnelExponent);
            uiHDRMultiplier      = new UISliderNumeric("HDR Multiplier",       new Vector2(EngineManager.Width - 390, 390), 0.0f, 100.0f, 0.01f, HDRMultiplier);
            uiReflectionStrength = new UISliderNumeric("Reflection Strength",  new Vector2(EngineManager.Width - 390, 430), 0.0f, 2.0f, 0.01f, ReflectionStrength);
            uiWaterColorStrength = new UISliderNumeric("Water Color Strength", new Vector2(EngineManager.Width - 390, 470), 0.0f, 2.0f, 0.01f, WaterColorStrength);
            uiWaveAmplitude      = new UISliderNumeric("Wave Amplitude",       new Vector2(EngineManager.Width - 390, 510), 0.0f, 0.25f, 0.001f, WaveAmplitude);
            uiWaveFrequency      = new UISliderNumeric("Wave Frequency",       new Vector2(EngineManager.Width - 390, 550), 0.0f, 6.0f, 0.01f, WaveFrequency);

            uiDeepWater          = new UISliderColor("Deep Water",             new Vector2(EngineManager.Width - 390, 590), DeepWater);
            uiShallowWater       = new UISliderColor("ShallowWater",           new Vector2(EngineManager.Width - 390, 630), ShallowWater);
            uiReflectionTint     = new UISliderColor("ReflectionTint",         new Vector2(EngineManager.Width - 390, 670), ReflectionTint);
        }

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public override void Test()
        {     
       
            #region Reset Parameters

            // If the parameters were modified is better to have the new values. 
            uiBumpHeight.CurrentValue = BumpHeight;
            uiTextureRepeatX.CurrentValue = TextureRepeatX;
            uiTextureRepeatY.CurrentValue = TextureRepeatY;
            uiBumpSpeedX.CurrentValue = BumpSpeedX;
            uiBumpSpeedY.CurrentValue = BumpSpeedY;
            uiFresnelBias.CurrentValue = FresnelBias;
            uiFresnelExponent.CurrentValue = FresnelExponent;
            uiHDRMultiplier.CurrentValue = HDRMultiplier;
            uiReflectionStrength.CurrentValue = ReflectionStrength;
            uiWaterColorStrength.CurrentValue = WaterColorStrength;
            uiWaveAmplitude.CurrentValue = WaveAmplitude;
            uiWaveFrequency.CurrentValue = WaveFrequency;
            
            uiDeepWater.CurrentValue = DeepWater;
            uiShallowWater.CurrentValue = ShallowWater;
            uiReflectionTint.CurrentValue = ReflectionTint;

            #endregion

            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Ocean Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiBumpHeight.UpdateAndRender();
            uiTextureRepeatX.UpdateAndRender();
            uiTextureRepeatY.UpdateAndRender();
            uiBumpSpeedX.UpdateAndRender();
            uiBumpSpeedY.UpdateAndRender();
            uiFresnelBias.UpdateAndRender();
            uiFresnelExponent.UpdateAndRender();
            uiHDRMultiplier.UpdateAndRender();
            uiReflectionStrength.UpdateAndRender();
            uiWaterColorStrength.UpdateAndRender();
            uiWaveAmplitude.UpdateAndRender();
            uiWaveFrequency.UpdateAndRender();
            uiDeepWater.UpdateAndRender();
            uiShallowWater.UpdateAndRender();
            uiReflectionTint.UpdateAndRender();

            BumpHeight = uiBumpHeight.CurrentValue;
            TextureRepeatX = uiTextureRepeatX.CurrentValue;
            TextureRepeatY = uiTextureRepeatY.CurrentValue;
            BumpSpeedX = uiBumpSpeedX.CurrentValue;
            BumpSpeedY = uiBumpSpeedY.CurrentValue;
            FresnelBias = uiFresnelBias.CurrentValue;
            FresnelExponent= uiFresnelExponent.CurrentValue;
            HDRMultiplier = uiHDRMultiplier.CurrentValue;
            ReflectionStrength = uiReflectionStrength.CurrentValue;
            WaterColorStrength = uiWaterColorStrength.CurrentValue;
            WaveAmplitude = uiWaveAmplitude.CurrentValue;
            WaveFrequency = uiWaveFrequency.CurrentValue;
            DeepWater = uiDeepWater.CurrentValue;
            ShallowWater = uiShallowWater.CurrentValue;
            ReflectionTint = uiReflectionTint.CurrentValue;
        } // Test
        
        #endregion

    } // Ocean
} // XNAFinalEngine.GraphicElements

