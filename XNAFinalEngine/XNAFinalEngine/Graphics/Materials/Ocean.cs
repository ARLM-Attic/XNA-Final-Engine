
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.Graphics
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
        private static Matrix? lastUsedTransposeInverseWorldMatrix;
        /// <summary>
        /// Set transpose inverse world matrix
        /// </summary>
        private static Matrix TransposeInverseWorldMatrix
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
        private static Matrix? lastUsedWorldMatrix;
        /// <summary>
        /// Set world matrix
        /// </summary>
        private static Matrix WorldMatrix
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
        private static Matrix? lastUsedInverseViewMatrix;
        /// <summary>
        /// Set view inverse matrix
        /// </summary>
        private static Matrix InverseViewMatrix
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
        private static Matrix? lastUsedWorldViewProjMatrix;
        /// <summary>
        /// Set world view projection matrix
        /// </summary>
        private static Matrix WorldViewProjMatrix
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
        private static float? lastUsedBumpHeight;
        /// <summary>
        /// Set Bump Height (greater or equal to 0)
        /// </summary>
        private static void SetBumpHeight(float _bumpHeight)
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
        private static float? lastUsedTextureRepeatX;
        /// <summary>
        /// Set Texture Repeat X (greater or equal to 0)
        /// </summary>
        private static void SetTextureRepeatX(float _textureRepeatX)
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
        private static float? lastUsedTextureRepeatY;
        /// <summary>
        /// Set Texture Repeat Y (greater or equal to 0)
        /// </summary>
        private static void SetTextureRepeatY(float _textureRepeatY)
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
        private static float? lastUsedBumpSpeedX;
        /// <summary>
        /// Set Bump Speed X
        /// </summary>
        private static void SetBumpSpeedX(float _bumpSpeedX)
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
        private float bumpSpeedY;

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
        private static float? lastUsedBumpSpeedY;
        /// <summary>
        /// Set Bump Speed Y
        /// </summary>
        private static void SetBumpSpeedY(float _bumpSpeedY)
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
        private static float? lastUsedFresnelBias;
        /// <summary>
        /// Set FresnelBias (greater or equal to 0)
        /// </summary>
        private static void SetFresnelBias(float _fresnelBias)
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
        private static float? lastUsedFresnelExponent;
        /// <summary>
        /// Set Fresnel Exponent (greater or equal to 0)
        /// </summary>
        private static void SetFresnelExponent(float _fresnelExponent)
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
        private static float? lastUsedHDRMultiplier;
        /// <summary>
        /// Set HDR Multiplier (greater or equal to 0)
        /// </summary>
        private static void SetHDRMultiplier(float _hdrMultiplier)
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
        private static Color? lastUsedDeepWater;
        /// <summary>
        /// Set Deep Water Color
        /// </summary>
        private static void SetDeepWater(Color _deepWater)
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
        private static Color? lastUsedShallowWater;
        /// <summary>
        /// Set Shallow Water
        /// </summary>
        private static void SetShallowWater(Color _shallowWater)
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
        private static Color? lastUsedReflectionTint;
        /// <summary>
        /// Set Reflection Tint
        /// </summary>
        private static void SetReflectionTint(Color _reflectionTint)
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
        private static float? lastUsedReflectionStrength;
        /// <summary>
        /// Set Reflection Strength (greater or equal to 0)
        /// </summary>
        private static void SetReflectionStrength(float _reflectionStrength)
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
        private static float? lastUsedWaterColorStrength;
        /// <summary>
        /// Set Water Color Strength (greater or equal to 0)
        /// </summary>
        private static void SetWaterColorStrength(float _waterColorStrength)
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
        private static float? lastUsedWaveAmplitude;
        /// <summary>
        /// Set Wave Amplitude (greater or equal to 0)
        /// </summary>
        private static void SetWaveAmplitude(float _waveAmplitude)
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
        private static float? lastUsedWaveFrequency;
        /// <summary>
        /// Set Wave Frequency (greater or equal to 0)
        /// </summary>
        private static void SetWaveFrequency(float _waveFrequency)
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
        private TextureCube reflectionTexture;

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
        private TextureCube lastUsedReflectionTexture;
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
        private Texture normalTexture;

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
        private static Texture lastUsedNormalTexture;
        /// <summary>
        /// Set normal texture
        /// </summary>
        private static void SetNormalTexture(Texture _normalTexture)
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

        } // Ocean
        
        #endregion
        
		#region Get Parameters Handles
		
        /// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected override sealed void GetParametersHandles()
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

            Render(model);
        } // Render

		#endregion         

        #region Configuration Window

        /// <summary>
        /// Open the configuration window of this material.
        /// </summary>
        public override void OpenConfigurationWindow()
        {
            if (!configurationWindowOpen)
            {
                configurationWindowOpen = true;

                #region Window

                var window = new Window
                {
                    Text = Name + " : Ocean"
                };
                UIManager.Add(window);
                window.Closed += delegate { configurationWindowOpen = false; };

                #endregion

                #region Name

                var materialNameLabel = new Label { Text = "Name", Left = 10, Top = 10, };
                window.Add(materialNameLabel);
                var materialNameTextBox = new TextBox { Text = Name, Left = 60, Top = 10 };
                window.Add(materialNameTextBox);
                materialNameTextBox.KeyDown += delegate(object sender, KeyEventArgs e)
                {
                    if (e.Key == Keys.Enter)
                    {
                        Name = materialNameTextBox.Text;
                        window.Text = Name + " : Constant";
                    }
                };
                materialNameTextBox.FocusLost += delegate
                {
                    Name = materialNameTextBox.Text;
                    window.Text = Name + " : Constant";
                };

                #endregion

                #region Group Surface Parameters

                GroupBox group = new GroupBox
                {
                    Parent = window,
                    Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                    Width = window.ClientWidth - 16,
                    Height = 160,
                    Left = 8,
                    Top = materialNameLabel.Top + materialNameLabel.Height + 15,
                    Text = "Surface Parameters",
                    TextColor = Color.Gray,
                };

                #endregion

                #region Bump Height
                
                var sliderBumpHeight = new SliderNumeric
                {
                    Left = 10,
                    Top = 20,
                    Value = BumpHeight,
                    Text = "Bump Height",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0,
                    MaximumValue = 2,
                };
                group.Add(sliderBumpHeight);
                sliderBumpHeight.ValueChanged += delegate
                {
                    BumpHeight = sliderBumpHeight.Value;
                };
                sliderBumpHeight.Draw += delegate { sliderBumpHeight.Value = BumpHeight; };
                
                #endregion

                #region Texture Repeat X

                var sliderTextureRepeatX = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderBumpHeight.Top + sliderBumpHeight.Height,
                    Value = TextureRepeatX,
                    Text = "Texture Repeat X",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 1,
                    MaximumValue = 16,
                };
                group.Add(sliderTextureRepeatX);
                sliderTextureRepeatX.ValueChanged += delegate
                {
                    TextureRepeatX = sliderTextureRepeatX.Value;
                };
                sliderTextureRepeatX.Draw += delegate { sliderTextureRepeatX.Value = TextureRepeatX; };

                #endregion

                #region Texture Repeat Y

                var sliderTextureRepeatY = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderTextureRepeatX.Top + sliderTextureRepeatX.Height,
                    Value = TextureRepeatY,
                    Text = "Texture Repeat Y",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 1,
                    MaximumValue = 16,
                };
                group.Add(sliderTextureRepeatY);
                sliderTextureRepeatY.ValueChanged += delegate
                {
                    TextureRepeatY = sliderTextureRepeatY.Value;
                };
                sliderTextureRepeatY.Draw += delegate { sliderTextureRepeatY.Value = TextureRepeatY; };

                #endregion

                #region Bump Speed X

                var sliderBumpSpeedX = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderTextureRepeatY.Top + sliderTextureRepeatY.Height,
                    Value = BumpSpeedX,
                    Text = "Bump Speed X",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = -0.2f,
                    MaximumValue = 0.2f,
                };
                group.Add(sliderBumpSpeedX);
                sliderBumpSpeedX.ValueChanged += delegate
                {
                    BumpSpeedX = sliderBumpSpeedX.Value;
                };
                sliderBumpSpeedX.Draw += delegate { sliderBumpSpeedX.Value = BumpSpeedX; };

                #endregion

                #region Bump Speed Y

                var sliderBumpSpeedY = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderBumpSpeedX.Top + sliderBumpSpeedX.Height,
                    Value = BumpSpeedY,
                    Text = "Bump Speed Y",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = -0.2f,
                    MaximumValue = 0.2f,
                };
                group.Add(sliderBumpSpeedY);
                sliderBumpSpeedY.ValueChanged += delegate
                {
                    BumpSpeedY = sliderBumpSpeedY.Value;
                };
                sliderBumpSpeedY.Draw += delegate { sliderBumpSpeedY.Value = BumpSpeedY; };

                #endregion

                #region Fresnel Bias

                var sliderFresnelBias = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderBumpSpeedY.Top + sliderBumpSpeedY.Height,
                    Value = FresnelBias,
                    Text = "Fresnel Bias",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0.0f,
                    MaximumValue = 1.0f,
                };
                group.Add(sliderFresnelBias);
                sliderFresnelBias.ValueChanged += delegate
                {
                    FresnelBias = sliderFresnelBias.Value;
                };
                sliderFresnelBias.Draw += delegate { sliderFresnelBias.Value = FresnelBias; };

                #endregion

                #region Fresnel Exponent

                var sliderFresnelExponent = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderFresnelBias.Top + sliderFresnelBias.Height,
                    Value = FresnelExponent,
                    Text = "Fresnel Exponent",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0.0f,
                    MaximumValue = 5.0f,
                };
                group.Add(sliderFresnelExponent);
                sliderFresnelExponent.ValueChanged += delegate
                {
                    FresnelExponent = sliderFresnelExponent.Value;
                };
                sliderFresnelExponent.Draw += delegate { sliderFresnelExponent.Value = FresnelExponent; };

                #endregion

                #region HDR Multiplier

                var sliderHDRMultiplier = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderFresnelExponent.Top + sliderFresnelExponent.Height,
                    Value = HDRMultiplier,
                    Text = "Fresnel Exponent",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0.0f,
                    MaximumValue = 100.0f,
                };
                group.Add(sliderHDRMultiplier);
                sliderHDRMultiplier.ValueChanged += delegate
                {
                    HDRMultiplier = sliderHDRMultiplier.Value;
                };
                sliderHDRMultiplier.Draw += delegate { sliderHDRMultiplier.Value = HDRMultiplier; };

                #endregion

                #region Reflection Strength

                var sliderReflectionStrength = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderHDRMultiplier.Top + sliderHDRMultiplier.Height,
                    Value = ReflectionStrength,
                    Text = "Reflection Strength",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0.0f,
                    MaximumValue = 2.0f,
                };
                group.Add(sliderReflectionStrength);
                sliderReflectionStrength.ValueChanged += delegate
                {
                    ReflectionStrength = sliderReflectionStrength.Value;
                };
                sliderReflectionStrength.Draw += delegate { sliderReflectionStrength.Value = ReflectionStrength; };

                #endregion

                #region Water Color Strength

                var sliderWaterColorStrength = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderReflectionStrength.Top + sliderReflectionStrength.Height,
                    Value = WaterColorStrength,
                    Text = "Water Color Strength",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0.0f,
                    MaximumValue = 2.0f,
                };
                group.Add(sliderWaterColorStrength);
                sliderWaterColorStrength.ValueChanged += delegate
                {
                    WaterColorStrength = sliderWaterColorStrength.Value;
                };
                sliderWaterColorStrength.Draw += delegate { sliderWaterColorStrength.Value = WaterColorStrength; };

                #endregion

                #region Wave Amplitude

                var sliderWaveAmplitude = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderWaterColorStrength.Top + sliderWaterColorStrength.Height,
                    Value = WaveAmplitude,
                    Text = "Wave Amplitude",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0.0f,
                    MaximumValue = 0.25f,
                };
                group.Add(sliderWaveAmplitude);
                sliderWaveAmplitude.ValueChanged += delegate
                {
                    WaveAmplitude = sliderWaveAmplitude.Value;
                };
                sliderWaveAmplitude.Draw += delegate { sliderWaveAmplitude.Value = WaveAmplitude; };

                #endregion

                #region Wave Frequency

                var sliderWaveFrequency = new SliderNumeric
                {
                    Left = 10,
                    Top = 10 + sliderWaveAmplitude.Top + sliderWaveAmplitude.Height,
                    Value = WaveFrequency,
                    Text = "Wave Frequency",
                    IfOutOfRangeRescale = false,
                    ValueCanBeOutOfRange = true,
                    MinimumValue = 0.0f,
                    MaximumValue = 6f,
                };
                group.Add(sliderWaveFrequency);
                sliderWaveFrequency.ValueChanged += delegate
                {
                    WaveFrequency = sliderWaveFrequency.Value;
                };
                sliderWaveFrequency.Draw += delegate { sliderWaveFrequency.Value = WaveFrequency; };

                #endregion

                #region Deep Water Color

                var sliderDeepWaterColor = new SliderColor
                {
                    Left = 10,
                    Top = 10 + sliderWaveFrequency.Top + sliderWaveFrequency.Height,
                    Color = DeepWater,
                    Text = "Deep Water Color",
                };
                group.Add(sliderDeepWaterColor);
                sliderDeepWaterColor.ColorChanged += delegate { DeepWater = sliderDeepWaterColor.Color; };
                sliderDeepWaterColor.Draw += delegate { sliderDeepWaterColor.Color = DeepWater; };

                #endregion

                #region Shallow Water Color

                var sliderShallowWaterColor = new SliderColor
                {
                    Left = 10,
                    Top = 10 + sliderDeepWaterColor.Top + sliderDeepWaterColor.Height,
                    Color = ShallowWater,
                    Text = "Shallow Water Color",
                };
                group.Add(sliderShallowWaterColor);
                sliderShallowWaterColor.ColorChanged += delegate { ShallowWater = sliderShallowWaterColor.Color; };
                sliderShallowWaterColor.Draw += delegate { sliderShallowWaterColor.Color = ShallowWater; };

                #endregion

                #region Reflection Tint Color

                var sliderReflectionTintColor = new SliderColor
                {
                    Left = 10,
                    Top = 10 + sliderShallowWaterColor.Top + sliderShallowWaterColor.Height,
                    Color = ReflectionTint,
                    Text = "Reflection Tint Color",
                };
                group.Add(sliderReflectionTintColor);
                sliderReflectionTintColor.ColorChanged += delegate { ReflectionTint = sliderReflectionTintColor.Color; };
                sliderReflectionTintColor.Draw += delegate { sliderReflectionTintColor.Color = ReflectionTint; };

                #endregion

                group.Height = sliderReflectionTintColor.Top + sliderReflectionTintColor.Height + 20;
                window.Height = 500;
                
            }
        } // OpenConfigurationWindow

        #endregion

    } // Ocean
} // XNAFinalEngine.Graphics