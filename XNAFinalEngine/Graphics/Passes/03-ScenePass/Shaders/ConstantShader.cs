
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
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Constant Shader.
    /// </summary>
    /// <remarks>
    /// This works in both, forward and deferred lighting mode because a constant material does not have illumination.
    /// However, alpha values different than 1 are only “permitted” in forward mode.
    /// </remarks>
    internal class ConstantShader : Shader
    {

        #region Variables

        /// <summary>
        /// Current view and projection matrix. Used to set the shader parameters.
        /// </summary>
        private Matrix viewMatrix, projectionMatrix;

        // Singleton reference.
        private static ConstantShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a Constant shader.
        /// </summary>
        public static ConstantShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new ConstantShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        private static EffectParameter
                                       epWorldViewProj,
                                       epDiffuseTexture,
                                       epDiffuseColor,
                                       epAlphaBlending;

        #region World View Projection Matrix

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjMatrix || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedWorldViewProjMatrix = worldViewProjMatrix;
                epWorldViewProj.SetValue(worldViewProjMatrix);
            }
        } // WorldViewProjMatrix

        #endregion

        #region Diffuse Texture
        
        private static Texture2D lastUsedDiffuseTexture;
        private static void SetDiffuseTexture(Texture _diffuseTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.AnisotropicWrap;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDiffuseTexture != _diffuseTexture.Resource || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedDiffuseTexture = _diffuseTexture.Resource;
                epDiffuseTexture.SetValue(_diffuseTexture.Resource);
            }
        } // SetDiffuseTexture

        #endregion

        #region Diffuse Color

        private static Color? lastUsedDiffuseColor;
        private static void SetDiffuseColor(Color diffuseColor)
        {
            if (lastUsedDiffuseColor != diffuseColor || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedDiffuseColor = diffuseColor;
                epDiffuseColor.SetValue(new Vector3(diffuseColor.R / 255f, diffuseColor.G / 255f, diffuseColor.B / 255f));
            }
        } // SetDiffuseColor

        #endregion

        #region Alpha Blending

        private static float? lastUsedAlphaBlending;
        private static void SetAlphaBlending(float alphaBlending)
        {
            if (lastUsedAlphaBlending != alphaBlending || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedAlphaBlending = alphaBlending;
                epAlphaBlending.SetValue(alphaBlending);
            }
        } // SetAlphaBlending

        #endregion

        #endregion

        #region Constructor

        /// <summary>
		/// Constant shader.
		/// </summary>
        private ConstantShader() : base("Materials\\Constant") { }

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
                epDiffuseTexture = Resource.Parameters["diffuseTexture"];
                epDiffuseColor   = Resource.Parameters["diffuseColor"];
                epWorldViewProj  = Resource.Parameters["worldViewProj"];
                epAlphaBlending  = Resource.Parameters["alphaBlending"];
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
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = BlendState.NonPremultiplied;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.
                
                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model.
		/// </summary>		
        internal void RenderModel(Matrix worldMatrix, Model model, Constant constantMaterial)
        {
            try
            {
                SetWorldViewProjMatrix(worldMatrix * viewMatrix * projectionMatrix);
                SetAlphaBlending(constantMaterial.AlphaBlending);
                if (constantMaterial.DiffuseTexture == null)
                {
                    SetDiffuseColor(constantMaterial.DiffuseColor);
                    Resource.CurrentTechnique = Resource.Techniques["ConstantWithoutTexture"];
                }
                else
                {
                    SetDiffuseTexture(constantMaterial.DiffuseTexture);
                    Resource.CurrentTechnique = Resource.Techniques["ConstantWithTexture"];
                }

                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to render model.", e);
            }
        } // RenderModel

		#endregion

    } // ConstantShader
} // XNAFinalEngine.Graphics

