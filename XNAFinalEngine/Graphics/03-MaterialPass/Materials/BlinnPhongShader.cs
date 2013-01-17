
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
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Blinn Phong Shader.
    /// </summary>
    internal class BlinnPhongShader : Shader
    {

        #region Variables
        
        // Current view and projection matrix. Used to set the shader parameters.
        private Matrix viewProjectionMatrix;

        // Singleton reference.
        private static BlinnPhongShader instance;

        // Shader Parameters.
        private static ShaderParameterInt spLODThreshold, spMinimumNumberSamples, spMaximumNumberSamples;
        private static ShaderParameterFloat spSpecularIntensity, spMaxRange, spHeightMapScale;
        private static ShaderParameterVector2 spHalfPixel, spNormalTextureSize;
        private static ShaderParameterVector3 spCameraPosition;
        private static ShaderParameterMatrix spWorldMatrix, spWorldITMatrix, spWorldViewProjMatrix, spViewInverseMatrix;
        private static ShaderParameterMatrixArray spBones;
        private static ShaderParameterColor spDiffuseColor;
        private static ShaderParameterTexture spNormalTexture, spDiffuseTexture, spDiffuseAccumulationTexture, spSpecularAccumulationTexture, spSpecularTexture;
        private static ShaderParameterTextureCube spReflectionTexture;
        private static ShaderParameterBool spReflectionTextured, spIsRGBM;
        
        // Techniques references.
        private static EffectTechnique blinnPhongSkinnedTechnique,
                                       blinnPhongSimpleTechnique,
                                       blinnPhongWithParrallaxTechnique;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a Blinn Phong shader.
        /// </summary>
        public static BlinnPhongShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new BlinnPhongShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Blinn Phong Shader.
		/// </summary>
        private BlinnPhongShader() : base("Materials\\BlinnPhong") { }

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
                spLODThreshold         = new ShaderParameterInt("LODThreshold", this);
                spMinimumNumberSamples = new ShaderParameterInt("minimumNumberSamples", this);
                spMaximumNumberSamples = new ShaderParameterInt("maximumNumberSamples", this);
                spSpecularIntensity    = new ShaderParameterFloat("specularIntensity", this);
                spMaxRange             = new ShaderParameterFloat("maxRange", this);
                spHeightMapScale       = new ShaderParameterFloat("heightMapScale", this);
                spHalfPixel            = new ShaderParameterVector2("halfPixel", this);
                spNormalTextureSize    = new ShaderParameterVector2("objectNormalTextureSize", this);
                spCameraPosition       = new ShaderParameterVector3("cameraPosition", this);
                spWorldMatrix          = new ShaderParameterMatrix("world", this);
                spWorldITMatrix        = new ShaderParameterMatrix("worldIT", this);
                spWorldViewProjMatrix  = new ShaderParameterMatrix("worldViewProj", this);
                spViewInverseMatrix    = new ShaderParameterMatrix("viewI", this);
                spBones                = new ShaderParameterMatrixArray("Bones", this, ModelAnimationClip.MaxBones);
                spDiffuseColor         = new ShaderParameterColor("diffuseColor", this);
                spDiffuseTexture       = new ShaderParameterTexture("diffuseTexture", this, SamplerState.AnisotropicWrap, 0);
                spNormalTexture        = new ShaderParameterTexture("normalTexture", this, SamplerState.PointClamp, 1); // SamplerState.AnisotropicWrap;
                spSpecularTexture      = new ShaderParameterTexture("specularTexture", this, SamplerState.AnisotropicWrap, 2);
                spDiffuseAccumulationTexture = new ShaderParameterTexture("diffuseAccumulationTexture", this, SamplerState.PointClamp, 4);
                spSpecularAccumulationTexture = new ShaderParameterTexture("specularAccumulationTexture", this, SamplerState.PointClamp, 5);
                spReflectionTexture    = new ShaderParameterTextureCube("reflectionTexture", this, SamplerState.LinearClamp, 3);
                spReflectionTextured   = new ShaderParameterBool("reflectionTextured", this);
                spIsRGBM               = new ShaderParameterBool("isRGBM", this);
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
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix, Texture diffuseAccumulationTexture, Texture specularAccumulationTexture, Texture normalTexture)
        {
            try
            {
                // Set initial parameters
                Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjectionMatrix);
                spHalfPixel.Value = new Vector2(0.5f / diffuseAccumulationTexture.Width, 0.5f / diffuseAccumulationTexture.Height);
                spCameraPosition.Value = Matrix.Invert(viewMatrix).Translation;
                spDiffuseAccumulationTexture.Value = diffuseAccumulationTexture;
                spSpecularAccumulationTexture.Value = specularAccumulationTexture;
                spNormalTexture.Value = normalTexture;
                spViewInverseMatrix.Value = Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix)));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Blinn Phong Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Get Techniques Handles

        /// <summary>
        /// Get the handles of the techniques from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override void GetTechniquesHandles()
        {
            try
            {
                blinnPhongSkinnedTechnique = Resource.Techniques["BlinnPhongSkinned"];
                blinnPhongSimpleTechnique = Resource.Techniques["BlinnPhongSimple"];
                blinnPhongWithParrallaxTechnique = Resource.Techniques["BlinnPhongWithParrallax"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model using the simple technique.
        /// </summary>
        internal void RenderModelSimple(ref Matrix worldMatrix, Model model, BlinnPhong material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = blinnPhongSimpleTechnique;
                // Matrix
                Matrix worldViewProjection;
                Matrix.Multiply(ref worldMatrix, ref viewProjectionMatrix, out worldViewProjection);
                spWorldViewProjMatrix.QuickSetValue(ref worldViewProjection);
                spWorldMatrix.QuickSetValue(ref worldMatrix);
                // Diffuse
                if (material.DiffuseTexture == null)
                {
                    spDiffuseColor.Value = material.DiffuseColor;
                    spDiffuseTexture.Value = Texture.BlackTexture;
                }
                else
                {
                    spDiffuseColor.Value = Color.Black;
                    spDiffuseTexture.Value = material.DiffuseTexture;
                }
                // Specular
                spSpecularIntensity.Value = material.SpecularIntensity;
                spSpecularTexture.Value = material.SpecularTexture ?? Texture.WhiteTexture;
                // Reflection
                if (material.ReflectionTexture != null)
                {
                    spReflectionTexture.Value = material.ReflectionTexture;
                    if (material.ReflectionTexture.IsRgbm)
                    {
                        spIsRGBM.Value = true;
                        spMaxRange.Value = material.ReflectionTexture.RgbmMaxRange;
                    }
                    else
                        spIsRGBM.Value = false;
                    spReflectionTextured.Value = true;
                }
                else
                    spReflectionTextured.Value = false;

                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Bling Phong Material: Unable to render model.", e);
            }
        } // RenderModelSimple

        /// <summary>
        /// Render a skinned model.
		/// </summary>
        internal void RenderModelSkinned(ref Matrix worldMatrix, Model model, Matrix[] boneTransform, BlinnPhong material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = blinnPhongSkinnedTechnique;
                // Skinned
                spBones.Value = boneTransform;
                // Matrix
                Matrix worldViewProjection;
                Matrix.Multiply(ref worldMatrix, ref viewProjectionMatrix, out worldViewProjection);
                spWorldViewProjMatrix.QuickSetValue(ref worldViewProjection);
                spWorldMatrix.QuickSetValue(ref worldMatrix);
                // Diffuse
                if (material.DiffuseTexture == null)
                {
                    spDiffuseColor.Value = material.DiffuseColor;
                    spDiffuseTexture.Value = Texture.BlackTexture;
                }
                else
                {
                    spDiffuseColor.Value = Color.Black;
                    spDiffuseTexture.Value = material.DiffuseTexture;
                }
                // Specular
                spSpecularIntensity.Value = material.SpecularIntensity;
                spSpecularTexture.Value = material.SpecularTexture ?? Texture.WhiteTexture;
                // Reflection
                if (material.ReflectionTexture != null)
                {
                    spReflectionTexture.Value = material.ReflectionTexture;
                    if (material.ReflectionTexture.IsRgbm)
                    {
                        spIsRGBM.Value = true;
                        spMaxRange.Value = material.ReflectionTexture.RgbmMaxRange;
                    }
                    else
                        spIsRGBM.Value = false;
                    spReflectionTextured.Value = true;
                }
                else
                    spReflectionTextured.Value = false;

                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Bling Phong Material: Unable to render model.", e);
            }
        } // RenderModelSkinned

        /// <summary>
        /// Render a model using the parallax technique.
        /// </summary>
        internal void RenderModelParallax(ref Matrix worldMatrix, Model model, BlinnPhong material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = blinnPhongWithParrallaxTechnique;
                // Matrix
                Matrix worldViewProjection;
                Matrix.Multiply(ref worldMatrix, ref viewProjectionMatrix, out worldViewProjection);
                spWorldViewProjMatrix.QuickSetValue(ref worldViewProjection);
                spWorldMatrix.QuickSetValue(ref worldMatrix);
                // Diffuse
                if (material.DiffuseTexture == null)
                {
                    spDiffuseColor.Value = material.DiffuseColor;
                    spDiffuseTexture.Value = Texture.BlackTexture;
                }
                else
                {
                    spDiffuseColor.Value = Color.Black;
                    spDiffuseTexture.Value = material.DiffuseTexture;
                }
                // Specular
                spSpecularIntensity.Value = material.SpecularIntensity;
                spSpecularTexture.Value = material.SpecularTexture ?? Texture.WhiteTexture;
                // Parallax
                spNormalTexture.Value = material.NormalTexture;
                spNormalTextureSize.Value = new Vector2(material.NormalTexture.Width, material.NormalTexture.Height);
                EngineManager.Device.SamplerStates[1] = SamplerState.AnisotropicClamp;
                spLODThreshold.Value = material.ParallaxLodThreshold;
                spMinimumNumberSamples.Value = material.ParallaxMinimumNumberSamples;
                spMaximumNumberSamples.Value = material.ParallaxMaximumNumberSamples;
                spHeightMapScale.Value = material.ParallaxHeightMapScale;
                spWorldITMatrix.Value = Matrix.Transpose(Matrix.Invert(worldMatrix));
                // Reflection
                if (material.ReflectionTexture != null)
                {
                    spReflectionTexture.Value = material.ReflectionTexture;
                    if (material.ReflectionTexture.IsRgbm)
                    {
                        spIsRGBM.Value = true;
                        spMaxRange.Value = material.ReflectionTexture.RgbmMaxRange;
                    }
                    else
                        spIsRGBM.Value = false;
                    spReflectionTextured.Value = true;
                }
                else
                    spReflectionTextured.Value = false;

                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Bling Phong Material: Unable to render model.", e);
            }
        } // RenderModelParallax

		#endregion        
        
    } // BlinnPhongShader
} // XNAFinalEngine.Graphics

