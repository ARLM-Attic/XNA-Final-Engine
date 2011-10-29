
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
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.SceneAdministration;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Glass Material.
    /// Alpha blending doesn't work very well with a deferred rendering.
    /// There are a couple of ways to fix this but neither is completely elegant or easy to implement.
    /// Working with a forward rendering for transparent objects is the common solution, K Buffer is another solution but not so common, and there are others.
    /// I choose the forward rendering alternative, but of course the light management curse is back to wreak more horror ;) 
    /// 
    /// Instead of trying to achieve a flexible transparent system,
    /// you can try to identify the game requirement and make a transparent shader that fits better your necessities.
    /// I make one for my requirements and I suppose that it will be helpful for the majority of you. 
    /// 
    /// Thank Søren for the help and tips.
    /// </summary>
    public class Glass : TransparentMaterial
    {

        #region Variables

        /// <summary>
        /// The XNA shader effect.
        /// </summary>
        private static Effect effect;

        /// <summary>
        /// Diffuse color.
        /// </summary>
        private Color diffuseColor = Color.Gray;
                
        /// <summary>
        /// Specular Power.
        /// </summary>
        private float specularPower = 50;

        /// <summary>
        /// Specular Intensity.
        /// </summary>
        private float specularIntensity = 1.0f;
        
        #endregion

        #region Properties

        /// <summary>
        /// Diffuse color. If a diffuse texture exists this color will be ignored.
        /// </summary>
        public Color DiffuseColor
        {
            get { return diffuseColor; }
            set
            {
                diffuseColor = value;
            }
        } // DiffuseColor

        /// <summary>
        /// Specular Power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPower; }
            set { specularPower = value; }
        } // SpecularPower
        
        /// <summary>
        /// Specular Intensity.
        /// </summary>
        public float SpecularIntensity
        {
            get { return specularIntensity; }
            set { specularIntensity = value; }
        } // SpecularIntensity

        /// <summary>
        /// Reflection Texture
        /// </summary>
        public TextureCube ReflectionTexture { get; set; }

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               epHalfPixel,
                               epCameraPosition,
                               // Matrices //
                               epWorldViewProj,
                               epWorld,
                               epWorldIT,
                               // Surface //
                               epDiffuseColor,                               
                               epSpecularIntensity,
                               epSpecularPower,
                               epAlphaBlending,
                               epReflectionTextured,
                               epReflectionTexture,
                               epIsRGBM,
                               epMaxRange,
                               // Lights //
                               epAmbientIntensity,
                               epSphericalHarmonicBase,
                               epAmbientColor,
                               epHasAmbientSphericalHarmonics,
                               epDirectionalLightDirection,
                               epDirectionalLightColor,
                               epDirectionalLightIntensity;
                               

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

        #region Camera Position

        private static Vector3? lastUsedCameraPosition;
        private static void SetCameraPosition(Vector3 cameraPosition)
        {
            if (lastUsedCameraPosition != cameraPosition)
            {
                lastUsedCameraPosition = cameraPosition;
                epCameraPosition.SetValue(cameraPosition);
            }
        } // SetCameraPosition

        #endregion

        // Matrices //
        
        #region World View Projection Matrix

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjMatrix;
                epWorldViewProj.SetValue(worldViewProjMatrix);
            }
        } // WorldViewProjMatrix

        #endregion

        #region Transpose Inverse World Matrix

        private static Matrix? lastUsedTransposeInverseWorldMatrix;
        private static void SetTransposeInverseWorldMatrix(Matrix transposeInverseWorldMatrix)
        {
            if (lastUsedTransposeInverseWorldMatrix != transposeInverseWorldMatrix)
            {
                lastUsedTransposeInverseWorldMatrix = transposeInverseWorldMatrix;
                epWorldIT.SetValue(transposeInverseWorldMatrix);
            }
        } // SetTransposeInverseWorldMatrix

        #endregion

        #region World Matrix

        private static Matrix? lastUsedWorldMatrix;
        private static void SetWorldMatrix(Matrix worldMatrix)
        {
            if (lastUsedWorldMatrix != worldMatrix)
            {
                lastUsedWorldMatrix = worldMatrix;
                epWorld.SetValue(worldMatrix);
            }
        } // SetWorldMatrix

        #endregion

        // Surface //

        #region Diffuse Color

        private static Color? lastUsedDiffuseColor;
        private static void SetDiffuseColor(Color diffuseColor)
        {
            if (lastUsedDiffuseColor != diffuseColor)
            {
                lastUsedDiffuseColor = diffuseColor;
                epDiffuseColor.SetValue(new Vector3(diffuseColor.R / 255f, diffuseColor.G / 255f, diffuseColor.B / 255f));
            }
        } // SetDiffuseColor

        #endregion

        #region Specular Intensity

        private static float? lastUsedSpecularIntensity;
        private static void SetSpecularIntensity(float specularIntensity)
        {
            if (lastUsedSpecularIntensity != specularIntensity)
            {
                lastUsedSpecularIntensity = specularIntensity;
                epSpecularIntensity.SetValue(specularIntensity);
            }
        } // SetSpecularIntensity

        #endregion

        #region Specular Power

        private static float? lastUsedSpecularPower;
        private static void SetSpecularPower(float specularPower)
        {
            if (lastUsedSpecularPower != specularPower)
            {
                lastUsedSpecularPower = specularPower;
                epSpecularPower.SetValue(specularPower);
            }
        } // SetSpecularPower

        #endregion

        #region Reflection Textured

        private static bool lastUsedReflectionTextured;
        private static void SetReflectionTextured(bool reflectionTextured)
        {
            if (lastUsedReflectionTextured != reflectionTextured)
            {
                lastUsedReflectionTextured = reflectionTextured;
                epReflectionTextured.SetValue(reflectionTextured);
            }
        } // SetReflectionTextured

        #endregion

        #region Reflection Texture

        private static TextureCube lastUsedReflectionTexture;
        private static void SetReflectionTexture(TextureCube reflectionTexture)
        {
            if (lastUsedReflectionTexture != reflectionTexture)
            {
                lastUsedReflectionTexture = reflectionTexture;
                if (reflectionTexture.IsRgbm)
                {
                    epIsRGBM.SetValue(true);
                    epMaxRange.SetValue(reflectionTexture.RgbmMaxRange);
                }
                else
                    epIsRGBM.SetValue(false);
                epReflectionTexture.SetValue(reflectionTexture.XnaTexture);
            }
        } // SetReflectionTexture

        #endregion

        #region Alpha Blending

        private static float? lastUsedAlphaBlending;
        private static void SetAlphaBlending(float alphaBlending)
        {
            if (lastUsedAlphaBlending != alphaBlending)
            {
                lastUsedAlphaBlending = alphaBlending;
                epAlphaBlending.SetValue(alphaBlending);
            }
        } // SetAlphaBlending

        #endregion

        // Lights //

        #region Ambient Intensity

        private static float? lastUsedAmbientIntensity;
        private static void SetAmbientIntensity(float ambientIntensity)
        {
            if (lastUsedAmbientIntensity != ambientIntensity)
            {
                lastUsedAmbientIntensity = ambientIntensity;
                epAmbientIntensity.SetValue(ambientIntensity);
            }
        } // SetAmbientIntensity

        #endregion

        #region Spherical Harmonic Base

        private static Vector3[] lastUsedSphericalHarmonicBase;
        private static void SetSphericalHarmonicBase(Vector3[] sphericalHarmonicBase)
        {
            if (!ArrayHelper.Equals(lastUsedSphericalHarmonicBase, sphericalHarmonicBase))
            {
                lastUsedSphericalHarmonicBase = (Vector3[])(sphericalHarmonicBase.Clone());
                epSphericalHarmonicBase.SetValue(sphericalHarmonicBase);
            }
        } // SetSphericalHarmonicBase

        #endregion

        #region Ambient Color

        private static Color? lastUsedAmbientColor;
        private static void SetAmbientColor(Color ambientColor)
        {
            if (lastUsedAmbientColor != ambientColor)
            {
                lastUsedAmbientColor = ambientColor;
                epAmbientColor.SetValue(new Vector3(ambientColor.R / 255f, ambientColor.G / 255f, ambientColor.B / 255f));
            }
        } // SetAmbientColor

        #endregion

        #region Has Ambient Spherical Harmonics

        private static bool lastUsedHasAmbientSphericalHarmonics;
        private static void SetHasAmbientSphericalHarmonics(bool hasAmbientSphericalHarmonics)
        {
            if (lastUsedHasAmbientSphericalHarmonics != hasAmbientSphericalHarmonics)
            {
                lastUsedHasAmbientSphericalHarmonics = hasAmbientSphericalHarmonics;
                epHasAmbientSphericalHarmonics.SetValue(hasAmbientSphericalHarmonics);
            }
        } // SetHasAmbientSphericalHarmonics

        #endregion

        #region Directional Light Color

        private static Color? lastUsedDirectionalLightColor;
        private static void SetDirectionalLightColor(Color directionalLightColor)
        {
            if (lastUsedDirectionalLightColor != directionalLightColor)
            {
                lastUsedDirectionalLightColor = directionalLightColor;
                epDirectionalLightColor.SetValue(new Vector3(directionalLightColor.R / 255f, directionalLightColor.G / 255f, directionalLightColor.B / 255f));
            }
        } // SetDirectionalLightColor

        #endregion

        #region Directional Light Direction

        private static Vector3? lastUsedDirectionalLightDirection;
        private static void SetDirectionalLightDirection(Vector3 directionalLightDirection)
        {
            if (lastUsedDirectionalLightDirection != directionalLightDirection)
            {
                lastUsedDirectionalLightDirection = directionalLightDirection;
                epDirectionalLightDirection.SetValue(directionalLightDirection);
            }
        } // SetDirectionalLightDirection

        #endregion

        #region Directional Light Intensity

        private static float? lastUsedDirectionalLightIntensity;
        private static void SetDirectionalLightIntensity(float directionalLightIntensity)
        {
            if (lastUsedDirectionalLightIntensity != directionalLightIntensity)
            {
                lastUsedDirectionalLightIntensity = directionalLightIntensity;
                epDirectionalLightIntensity.SetValue(directionalLightIntensity);
            }
        } // SetDirectionalLightIntensity

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Glass Material.
		/// </summary>
        public Glass()
		{
            LoadShader("Materials\\ForwardBlinnPhong", ref effect);
        } // Glass

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
                epWorldViewProj = effect.Parameters["worldViewProj"];
                epWorld         = effect.Parameters["world"];
                epWorldIT       = effect.Parameters["worldIT"];
                // Parameters //
                epHalfPixel          = effect.Parameters["halfPixel"];
                epCameraPosition     = effect.Parameters["cameraPosition"];
                epSpecularIntensity  = effect.Parameters["specularIntensity"];
                epSpecularPower      = effect.Parameters["specularPower"];
                epDiffuseColor       = effect.Parameters["diffuseColor"];
                epAlphaBlending      = effect.Parameters["alphaBlending"];
                epReflectionTexture  = effect.Parameters["reflectionTexture"];
                epReflectionTextured = effect.Parameters["reflectionTextured"];
                epIsRGBM             = effect.Parameters["isRGBM"];
                epMaxRange           = effect.Parameters["maxRange"];
                // Lights //
                epSphericalHarmonicBase        = effect.Parameters["sphericalHarmonicBase"];
                epAmbientIntensity             = effect.Parameters["ambientIntensity"];
                epAmbientColor                 = effect.Parameters["ambientColor"];
                epHasAmbientSphericalHarmonics = effect.Parameters["hasAmbientSphericalHarmonics"];
                epDirectionalLightDirection    = effect.Parameters["directionalLightDirection"];
                epDirectionalLightColor        = effect.Parameters["directionalLightColor"];
                epDirectionalLightIntensity    = effect.Parameters["directionalLightIntensity"];
			}
			catch
			{
                throw new Exception("Get the handles from the forward blinn phong material failed.");
			}
		} // GetParametersHandles

		#endregion

        #region Render

        /// <summary>
        /// Render this shader/material.
		/// </summary>		
        internal override void Render(Matrix worldMatrix, Model model)
        {

            EngineManager.Device.BlendState = BlendState.AlphaBlend;
            if (BothSides)
                EngineManager.Device.RasterizerState = RasterizerState.CullNone;
            else
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            #region Set Parameters

            try
            {
                SetHalfPixel(new Vector2(0.5f / DeferredLightingManager.LightMap.Width, 0.5f / DeferredLightingManager.LightMap.Height));
                SetCameraPosition(ApplicationLogic.Camera.Position);
                // Matrices //
                SetWorldViewProjMatrix(worldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix);
                SetWorldMatrix(worldMatrix);
                SetTransposeInverseWorldMatrix(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                // Surface //
                SetDiffuseColor(DiffuseColor);   
                SetSpecularIntensity(SpecularIntensity);
                SetSpecularPower(SpecularPower);
                SetAlphaBlending(AlphaBlending);
                // Reflection //
                if (ReflectionTexture != null)
                {
                    SetReflectionTexture(ReflectionTexture);
                    SetReflectionTextured(true);
                }
                else
                    SetReflectionTextured(false);
                // Lights //
                SetAmbientColor(AmbientLight.Color);
                SetAmbientIntensity(AmbientLight.Intensity);
                if (AmbientLight.SphericalHarmonicAmbientLight == null)
                {
                    SetHasAmbientSphericalHarmonics(false);
                }
                else
                {
                    SetHasAmbientSphericalHarmonics(true);
                    SetSphericalHarmonicBase(AmbientLight.SphericalHarmonicAmbientLight.Coeficients);
                }
                if (SceneManager.DirectionalLightForTransparentObjects != null)
                {
                    SetDirectionalLightColor(SceneManager.DirectionalLightForTransparentObjects.DiffuseColor);
                    SetDirectionalLightDirection(SceneManager.DirectionalLightForTransparentObjects.Direction);
                    SetDirectionalLightIntensity(SceneManager.DirectionalLightForTransparentObjects.Intensity);
                }
                else
                {
                    SetDirectionalLightColor(Color.Black);
                }
            }
            catch
            {
                throw new Exception("Unable to set the forward blinn phong shader parameters.");
            }
            
            #endregion

            Render(model, effect);

            EngineManager.SetDefaultRenderStates();

        } // Render

		#endregion        
        
    } // Glass
} // XNAFinalEngine.Graphics

