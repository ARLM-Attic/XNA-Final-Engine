
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
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Blinn Phong Shader.
    /// </summary>
    internal class BlinnPhongShader : Shader
    {

        #region Variables

        /// <summary>
        /// Current view and projection matrix. Used to set the shader parameters.
        /// </summary>
        private Matrix viewMatrix, projectionMatrix;

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               epHalfPixel,
                               epCameraPosition,
                               epWorldViewProj,
                               epWorld,
                               epWorldIT,
                               epDiffuseColor,
                               epDiffuseTexture,
                               epSpecularIntensity,
                               epLightTexture,
                               epSpecularTexture,
                               epDiffuseTextured,
                               epSpecularTextured,
                               epReflectionTextured,
                               epReflectionTexture,
                               epIsRGBM,
                               epMaxRange,
                               // Parallax
                               epNormalTexture,
                               epNormalTextureSize,
                               epLODThreshold,
                               epMinimumNumberSamples,
                               epMaximumNumberSamples,
                               epHeightMapScale,
                               // Skinning
                               epBones;
                               

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

        #region Diffuse Texture

        private static Texture lastUsedDiffuseTexture;
        private static void SetDiffuseTexture(Texture _diffuseTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.AnisotropicWrap;
            if (lastUsedDiffuseTexture != _diffuseTexture)
            {
                lastUsedDiffuseTexture = _diffuseTexture;
                epDiffuseTexture.SetValue(_diffuseTexture.Resource);
            }
        } // SetDiffuseTexture

        #endregion

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

        #region Specular Texture

        private static Texture lastUsedSpecularTexture;
        private static void SetSpecularTexture(Texture specularTexture)
        {
            EngineManager.Device.SamplerStates[2] = SamplerState.AnisotropicWrap;
            if (lastUsedSpecularTexture != specularTexture)
            {
                lastUsedSpecularTexture = specularTexture;
                epSpecularTexture.SetValue(specularTexture.Resource);
            }
        } // SetSpecularTexture

        #endregion

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

        #region Light Map Texture

        private static Texture lastUsedLightTexture;
        private static void SetLightTexture(Texture lightTexture)
        {
            EngineManager.Device.SamplerStates[1] = SamplerState.PointClamp;
            // XNA 4.0 reconstructs automatically the render targets when a device is lost.
            // However the shaders have to re set to the GPU the new render targets to work properly.
            // This problem seems to manifest only with floating point formats.
            // So it's a floating point texture set it every time that is need it.
            if (lastUsedLightTexture != lightTexture ||
                (lightTexture is RenderTarget && ((RenderTarget)lightTexture).SurfaceFormat != SurfaceFormat.Color))
            {
                lastUsedLightTexture = lightTexture;
                epLightTexture.SetValue(lightTexture.Resource);
            }
        } // SetLightTexture

        #endregion

        #region Diffuse Textured

        private static bool lastUsedDiffuseTextured;
        private static void SetDiffuseTextured(bool diffuseTextured)
        {
            if (lastUsedDiffuseTextured != diffuseTextured)
            {
                lastUsedDiffuseTextured = diffuseTextured;
                epDiffuseTextured.SetValue(diffuseTextured);
            }
        } // SetDiffuseTextured

        #endregion

        #region Specular Textured

        private static bool lastUsedSpecularTextured;
        private static void SetSpecularTextured(bool specularTextured)
        {
            if (lastUsedSpecularTextured != specularTextured)
            {
                lastUsedSpecularTextured = specularTextured;
                epSpecularTextured.SetValue(specularTextured);
            }
        } // SetSpecularTextured

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

        #region Normal Texture (and size)

        private static Texture lastUsedNormalTextureTexture;
        private static void SetNormalTexture(Texture normalTexture)
        {
            EngineManager.Device.SamplerStates[3] = SamplerState.AnisotropicWrap;
            if (lastUsedNormalTextureTexture != normalTexture)
            {
                lastUsedNormalTextureTexture = normalTexture;
                epNormalTextureSize.SetValue(new Vector2(normalTexture.Width, normalTexture.Height));
                epNormalTexture.SetValue(normalTexture.Resource);
            }
        } // SetNormalTexture

        #endregion

        #region LOD Threshold

        private static int lastUsedLODThreshold;
        private static void SetLODThreshold(int lodThreshold)
        {
            if (lastUsedLODThreshold != lodThreshold)
            {
                lastUsedLODThreshold = lodThreshold;
                epLODThreshold.SetValue(lodThreshold);
            }
        } // SetLODThreshold

        #endregion

        #region Minimum Number Samples

        private static int lastUsedMinimumNumberSamples;
        private static void SetMinimumNumberSamples(int minimumNumberSamples)
        {
            if (lastUsedMinimumNumberSamples != minimumNumberSamples)
            {
                lastUsedMinimumNumberSamples = minimumNumberSamples;
                epMinimumNumberSamples.SetValue(minimumNumberSamples);
            }
        } // SetMinimumNumberSamples

        #endregion

        #region Maximum Number Samples

        private static int lastUsedMaximumNumberSamples;
        private static void SetMaximumNumberSamples(int maximumNumberSamples)
        {
            if (lastUsedMaximumNumberSamples != maximumNumberSamples)
            {
                lastUsedMaximumNumberSamples = maximumNumberSamples;
                epMaximumNumberSamples.SetValue(maximumNumberSamples);
            }
        } // SetMaximumNumberSamples

        #endregion

        #region Height Map Scale

        private static float lastUsedHeightMapScale;
        private static void SetHeightMapScale(float heightMapScale)
        {
            if (lastUsedHeightMapScale != heightMapScale)
            {
                lastUsedHeightMapScale = heightMapScale;
                epHeightMapScale.SetValue(heightMapScale);
            }
        } // SetHeightMapScale

        #endregion

        #region Reflection Texture

        private static TextureCube lastUsedReflectionTexture;
        private static void SetReflectionTexture(TextureCube reflectionTexture)
        {
            EngineManager.Device.SamplerStates[4] = SamplerState.LinearClamp;
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
                epReflectionTexture.SetValue(reflectionTexture.Resource);
            }
        } // SetReflectionTexture

        #endregion

        #region Bones

        //private static Matrix[] lastUsedBones;
        private static void SetBones(Matrix[] bones)
        {
            // The values are probably different and the operation is costly and garbage prone (but this can be avoided).
            /*if (!ArrayHelper.Equals(lastUsedBones, bones))
            {
                lastUsedBones = (Matrix[])(bones.Clone());
                epBones.SetValue(bones);
            }*/
            epBones.SetValue(bones);
        } // SetBones

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Blinn Phong Shader.
		/// </summary>
        internal BlinnPhongShader() : base("Materials\\BlinnPhong") { }

		#endregion
        
		#region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
		protected override sealed void GetParametersHandles()
		{
			try
			{
                // Matrices //
                epWorldViewProj        = Resource.Parameters["worldViewProj"];
                epWorld                = Resource.Parameters["world"];
                epWorldIT              = Resource.Parameters["worldIT"];
                // Parameters //
                epHalfPixel            = Resource.Parameters["halfPixel"];
                epCameraPosition       = Resource.Parameters["cameraPosition"];
                epSpecularIntensity    = Resource.Parameters["specularIntensity"];
                epDiffuseColor         = Resource.Parameters["diffuseColor"];
                epDiffuseTexture       = Resource.Parameters["diffuseTexture"];
                epLightTexture      = Resource.Parameters["lightMap"];
                epSpecularTexture      = Resource.Parameters["specularTexture"];
                epDiffuseTextured      = Resource.Parameters["diffuseTextured"];
                epSpecularTextured     = Resource.Parameters["specularTextured"];
                epReflectionTexture    = Resource.Parameters["reflectionTexture"];
                epReflectionTextured   = Resource.Parameters["reflectionTextured"];
                epIsRGBM               = Resource.Parameters["isRGBM"];
                epMaxRange             = Resource.Parameters["maxRange"];
                // Parallax //
                epNormalTexture        = Resource.Parameters["normalTexture"];
                epNormalTextureSize    = Resource.Parameters["objectNormalTextureSize"];
                epLODThreshold         = Resource.Parameters["LODThreshold"];
                epMinimumNumberSamples = Resource.Parameters["minimumNumberSamples"];
                epMaximumNumberSamples = Resource.Parameters["maximumNumberSamples"];
                epHeightMapScale       = Resource.Parameters["heightMapScale"];
                // Skinning //
                epBones = Resource.Parameters["Bones"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
		} // GetParametersHandles

		#endregion

        #region Begin

        /// <summary>
        /// Begins the render.
        /// </summary>
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix, RenderTarget lightTexture)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                SetHalfPixel(new Vector2(0.5f / lightTexture.Width, 0.5f / lightTexture.Height));
                SetCameraPosition(Matrix.Invert(viewMatrix).Translation); // Tener cuidado con esto.
                SetLightTexture(lightTexture);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Blinn Phong Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model.
		/// </summary>		
        internal void RenderModel(Matrix worldMatrix, Model model, Matrix[] boneTransform, BlinnPhong blinnPhongMaterial)
        {
            try
            {
                bool isSkinned = false;
                if (model is FileModel && ((FileModel)model).IsSkinned) // If it is a skinned model.
                {
                    SetBones(((FileModel)model).SkinTransforms);
                    isSkinned = true;
                }
                SetWorldViewProjMatrix(worldMatrix * viewMatrix * projectionMatrix);
                SetWorldMatrix(worldMatrix);
                SetTransposeInverseWorldMatrix(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                SetSpecularIntensity(blinnPhongMaterial.SpecularIntensity);
                SetDiffuseColor(blinnPhongMaterial.DiffuseColor);
                if (blinnPhongMaterial.DiffuseTexture == null && blinnPhongMaterial.SpecularTexture == null)
                {
                    if (!blinnPhongMaterial.ParallaxEnabled)
                    {
                        Resource.CurrentTechnique = isSkinned ? Resource.Techniques["SkinnedBlinnPhongWithoutTexture"] : Resource.Techniques["BlinnPhongWithoutTexture"];
                    }
                    SetSpecularTextured(false);
                    SetDiffuseTextured(false);
                }
                else
                {
                    if (!blinnPhongMaterial.ParallaxEnabled)
                        Resource.CurrentTechnique = isSkinned ? Resource.Techniques["SkinnedBlinnPhongWithTexture"] : Resource.Techniques["BlinnPhongWithTexture"];
                    if (blinnPhongMaterial.DiffuseTexture != null)
                    {
                        SetDiffuseTextured(true);
                        SetDiffuseTexture(blinnPhongMaterial.DiffuseTexture);
                    }
                    else
                        SetDiffuseTextured(false);
                    if (blinnPhongMaterial.SpecularTexture != null)
                    {
                        SetSpecularTextured(true);
                        SetSpecularTexture(blinnPhongMaterial.SpecularTexture);
                    }
                    else
                        SetSpecularTextured(false);
                }
                if (blinnPhongMaterial.ParallaxEnabled)
                {
                    Resource.CurrentTechnique = isSkinned ? Resource.Techniques["SkinnedBlinnPhongWithParrallax"] : Resource.Techniques["BlinnPhongWithParrallax"];
                    SetNormalTexture(blinnPhongMaterial.NormalTexture);
                    SetLODThreshold(blinnPhongMaterial.ParallaxLodThreshold);
                    SetMinimumNumberSamples(blinnPhongMaterial.ParallaxMinimumNumberSamples);
                    SetMaximumNumberSamples(blinnPhongMaterial.ParallaxMaximumNumberSamples);
                    SetHeightMapScale(blinnPhongMaterial.ParallaxHeightMapScale);
                }
                if (blinnPhongMaterial.ReflectionTexture != null)
                {
                    SetReflectionTexture(blinnPhongMaterial.ReflectionTexture);
                    SetReflectionTextured(true);
                }
                else
                    SetReflectionTextured(false);

                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Bling Phong Material: Unable to render model.", e);
            }
        } // Render

		#endregion        
        
    } // BlinnPhongShader
} // XNAFinalEngine.Graphics

