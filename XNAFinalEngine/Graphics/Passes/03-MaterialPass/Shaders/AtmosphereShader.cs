
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
    internal class AtmosphereShader : Shader
    {

        #region Variables

        /// <summary>
        /// Current view and projection matrix. Used to set the shader parameters.
        /// </summary>
        private Matrix viewMatrix, projectionMatrix;

        // Singleton reference.
        private static AtmosphereShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static AtmosphereShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new AtmosphereShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        private static EffectParameter
                                       epWorldViewProj;

        #region World View Projection Matrix

        private static Matrix lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjMatrix;
                epWorldViewProj.SetValue(worldViewProjMatrix);
            }
        } // WorldViewProjMatrix

        #endregion

        #endregion

        #region Constructor

        /// <summary>
		/// Constant shader.
		/// </summary>
        private AtmosphereShader() : base("Materials\\Atmosphere") { }

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
                /*epDiffuseTexture = Resource.Parameters["diffuseTexture"];
                    if (lastUsedDiffuseTexture != null && !lastUsedDiffuseTexture.IsDisposed)
                        epDiffuseTexture.SetValue(lastUsedDiffuseTexture);
                epDiffuseColor   = Resource.Parameters["diffuseColor"];
                    epDiffuseColor.SetValue(new Vector3(lastUsedDiffuseColor.R / 255f, lastUsedDiffuseColor.G / 255f, lastUsedDiffuseColor.B / 255f));*/
                epWorldViewProj  = Resource.Parameters["worldViewProj"];
			        epWorldViewProj.SetValue(lastUsedWorldViewProjMatrix);
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
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            try
            {
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.
                
                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                Resource.Parameters["cameraPosition"].SetValue(cameraPosition);
                Resource.Parameters["cameraHeightSquared"].SetValue(cameraPosition.LengthSquared());
                EngineManager.Device.BlendState = BlendState.AlphaBlend; // The resulting color will be added to current render target color.
                EngineManager.Device.RasterizerState = RasterizerState.CullClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Atmosphere Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model.
		/// </summary>		
        internal void RenderModel(Matrix worldMatrix, Model model, Matrix[] boneTransform, Atmosphere constantMaterial, int meshIndex, int meshPart)
        {
            try
            {
                if (boneTransform != null)
                    worldMatrix = boneTransform[meshIndex + 1] * worldMatrix;
                SetWorldViewProjMatrix(worldMatrix * viewMatrix * projectionMatrix);
                Resource.Parameters["world"].SetValue(worldMatrix);
                Vector3 sunDirection = new Vector3(-0.1f, -0.05f, -0.5f);
                sunDirection.Normalize();
                Resource.Parameters["sunDirection"].SetValue(sunDirection);
                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Atmosphere Material: Unable to render model.", e);
            }
        } // RenderModel

		#endregion

    } // AtmosphereShader
} // XNAFinalEngine.Graphics

