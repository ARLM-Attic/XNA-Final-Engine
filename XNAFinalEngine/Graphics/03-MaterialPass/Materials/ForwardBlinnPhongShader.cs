
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
using XNAFinalEngine.Helpers;
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Forward Blinn Phong Shader.
    /// Alpha blending doesn't work directly with a deferred rendering.
    /// 
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
    internal class ForwardBlinnPhongShader : Shader
    {

        #region Variables
        
        // Current view and projection matrix.
        private Matrix viewprojectionMatrix;

        // It's an auxiliary structure that helps avoiding garbage.
        private readonly Vector3[] coeficients = new Vector3[9];

        // Singleton reference.
        private static ForwardBlinnPhongShader instance;

        // Shader Parameters.
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterFloat spSpecularIntensity, spSpecularPower, spMaxRange, spAlphaBlending, spAmbientIntensity, spDirectionalLightIntensity,
                                            spSpotLightIntensity, spSpotLightInnerAngle, spSpotLightOuterAngle, spInvSpotLightRadius;
        private static ShaderParameterVector3 spCameraPosition, spDirectionalLightDirection, spSpotLightPosition, spSpotLightDirection;
        private static ShaderParameterMatrix spWorldMatrix, spWorldITMatrix, spWorldViewProjMatrix;
        private static ShaderParameterColor spDiffuseColor, spAmbientColor, spDirectionalLightColor, spSpotLightColor;
        private static ShaderParameterTexture spDiffuseTexture, spShadowTexture;
        private static ShaderParameterTextureCube spReflectionTexture;
        private static ShaderParameterBool spReflectionTextured, spIsRGBM, spHasAmbientSphericalHarmonics, spHasShadows;
        private static ShaderParameterVector3Array spSphericalHarmonicBase;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a Blinn Phong shader.
        /// </summary>
        public static ForwardBlinnPhongShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new ForwardBlinnPhongShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Forward Blinn Phong Shader.
		/// </summary>
        private ForwardBlinnPhongShader() : base("Materials\\ForwardBlinnPhong") { }

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
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spSpecularIntensity = new ShaderParameterFloat("specularIntensity", this);
                spSpecularPower = new ShaderParameterFloat("specularPower", this);
                spAlphaBlending = new ShaderParameterFloat("alphaBlending", this);
                spMaxRange = new ShaderParameterFloat("maxRange", this);
                spCameraPosition = new ShaderParameterVector3("cameraPosition", this);
                spWorldMatrix = new ShaderParameterMatrix("world", this);
                spWorldITMatrix = new ShaderParameterMatrix("worldIT", this);
                spWorldViewProjMatrix = new ShaderParameterMatrix("worldViewProj", this);
                spDiffuseColor = new ShaderParameterColor("diffuseColor", this);
                spDiffuseTexture = new ShaderParameterTexture("diffuseTexture", this, SamplerState.AnisotropicWrap, 0);
                spReflectionTexture = new ShaderParameterTextureCube("reflectionTexture", this, SamplerState.LinearClamp, 4);
                spReflectionTextured = new ShaderParameterBool("reflectionTextured", this);
                spIsRGBM = new ShaderParameterBool("isRGBM", this);
                // Ambient Light //
                spSphericalHarmonicBase = new ShaderParameterVector3Array("sphericalHarmonicBase", this, 9);
                spAmbientIntensity = new ShaderParameterFloat("ambientIntensity", this);
                spAmbientColor = new ShaderParameterColor("ambientColor", this);
                spHasAmbientSphericalHarmonics = new ShaderParameterBool("hasAmbientSphericalHarmonics", this);
                // Directional Light //
                spDirectionalLightDirection = new ShaderParameterVector3("directionalLightDirection", this);
                spDirectionalLightColor = new ShaderParameterColor("directionalLightColor", this);
                spDirectionalLightIntensity = new ShaderParameterFloat("directionalLightIntensity", this);
                spShadowTexture = new ShaderParameterTexture("shadowTexture", this, SamplerState.PointClamp, 3);
                spHasShadows = new ShaderParameterBool("hasShadows", this);
                // Spot // 
                spSpotLightDirection = new ShaderParameterVector3("spotLightDirection", this);
                spSpotLightPosition = new ShaderParameterVector3("spotLightPos", this);
                spSpotLightColor = new ShaderParameterColor("spotLightColor", this);
                spDirectionalLightIntensity = new ShaderParameterFloat("directionalLightIntensity", this);
                spSpotLightIntensity = new ShaderParameterFloat("spotLightIntensity", this);
                spSpotLightInnerAngle = new ShaderParameterFloat("spotLightInnerAngle", this);
                spSpotLightOuterAngle = new ShaderParameterFloat("spotLightOuterAngle", this);
                spInvSpotLightRadius = new ShaderParameterFloat("invSpotLightRadius", this);
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
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix, AmbientLight ambientLight,
                            Color directionalLightColor, Vector3 directionalLightDirection,
                            float directionalLightIntensity, Texture directionalShadowTexture,
                            Size renderTargetSize)
        {
            try
            {
                // Set initial parameters.
                viewprojectionMatrix = viewMatrix * projectionMatrix;
                spCameraPosition.Value = Matrix.Invert(viewMatrix).Translation;
                if (ambientLight != null)
                {
                    spAmbientColor.Value = ambientLight.Color;
                    spAmbientIntensity.Value = ambientLight.Intensity;
                    if (ambientLight.SphericalHarmonicLighting == null || ambientLight.Intensity <= 0)
                    {
                        spHasAmbientSphericalHarmonics.Value = false;
                    }
                    else
                    {
                        ambientLight.SphericalHarmonicLighting.GetCoeficients(coeficients);
                        spHasAmbientSphericalHarmonics.Value = true;
                        spSphericalHarmonicBase.Value = coeficients;
                    }
                }
                else
                {
                    spHasAmbientSphericalHarmonics.Value = false;
                    spAmbientColor.Value = Color.Black;
                }
                spDirectionalLightColor.Value = directionalLightColor;
                spDirectionalLightDirection.Value = directionalLightDirection;
                spDirectionalLightIntensity.Value = directionalLightIntensity;
                if (directionalShadowTexture != null)
                {
                    spHalfPixel.Value = new Vector2(0.5f / renderTargetSize.Width, 0.5f / renderTargetSize.Height); // I need the destination render target dimension.
                    spHasShadows.Value = true;
                    spShadowTexture.Value = directionalShadowTexture;
                }
                else
                {
                    spHasShadows.Value = false;
                    spShadowTexture.Value = Texture.WhiteTexture;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Forward Blinn Phong Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model.
		/// </summary>
        internal void RenderModel(ref Matrix worldMatrix, Model model, BlinnPhong material, int meshIndex, int meshPart,
                                  Vector3 spotLightPosition, Vector3 spotLightDirection, Color spotLightColor, float spotLightIntensity,
                                  float  spotLightInnerAngle, float  spotLightOuterAngle, float  range)
        {
            // Set Render States.
            EngineManager.Device.RasterizerState = material.BothSides ? RasterizerState.CullNone : RasterizerState.CullCounterClockwise;

            try
            {
                spWorldViewProjMatrix.Value = worldMatrix * viewprojectionMatrix;
                spWorldMatrix.Value = worldMatrix;
                spWorldITMatrix.Value = Matrix.Transpose(Matrix.Invert(worldMatrix));

                spSpecularIntensity.Value = material.SpecularIntensity;
                spSpecularPower.Value = material.SpecularPower;
                spDiffuseColor.Value = material.DiffuseColor;
                spAlphaBlending.Value = material.AlphaBlending;

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
                // Spot Light //
                spSpotLightDirection.Value  = spotLightDirection;
                spSpotLightPosition.Value   = spotLightPosition;
                spSpotLightColor.Value      = spotLightColor;
                spSpotLightIntensity.Value  = spotLightIntensity;
                spSpotLightInnerAngle.Value = spotLightInnerAngle * (3.141592f / 180.0f);
                spSpotLightOuterAngle.Value = spotLightOuterAngle * (3.141592f / 180.0f);
                spInvSpotLightRadius.Value  = 1 / range;
                
                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Forward Bling Phong Material: Unable to render model.", e);
            }
        }

        // RenderModel

		#endregion        
        
    } // ForwardBlinnPhongShader
} // XNAFinalEngine.Graphics

