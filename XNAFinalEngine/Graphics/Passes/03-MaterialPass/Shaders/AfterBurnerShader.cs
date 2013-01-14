
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Model = XNAFinalEngine.Assets.Model;
using Microsoft.Xna.Framework.Graphics;
using Texture = XNAFinalEngine.Assets.Texture;

#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// After Burner Shader.
    /// </summary>
    internal class AfterBurnerShader : Shader
    {

        #region Variables

        /// <summary>
        /// Current view and projection matrix. Used to set the shader parameters.
        /// </summary>
        private Matrix viewMatrix, projectionMatrix;

        // Singleton reference.
        private static AfterBurnerShader instance;

        // Shader Parameters.
        private static ShaderParameterVector2 spScaleParams;
        private static ShaderParameterVector3 spNoiseParams;
        private static ShaderParameterMatrix spWorldViewProj;
        private static ShaderParameterTexture spColorizeTexture, spDiffTexture;

        private readonly Texture colorizeTexture, diffTexture;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static AfterBurnerShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new AfterBurnerShader();
                return instance;
            }
        } // Instance

        #endregion
        
        #region Constructor

        /// <summary>
        /// After Burner Shader.
		/// </summary>
        private AfterBurnerShader() : base("Materials\\AfterBurner")
        {
            ContentManager userContentManager    = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            colorizeTexture = new Texture("Shaders\\JetFlameColor");
            diffTexture = new Texture("Shaders\\JetFlameDiff");
            ContentManager.CurrentContentManager = userContentManager;
        } // AfterBurnerShader

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
                spWorldViewProj = new ShaderParameterMatrix("worldViewProj", this);
                spNoiseParams = new ShaderParameterVector3("noiseParams", this);
                spScaleParams = new ShaderParameterVector2("scaleParams", this);
                spColorizeTexture = new ShaderParameterTexture("colorizeTexture", this, SamplerState.LinearWrap, 0);
                spDiffTexture = new ShaderParameterTexture("diffTexture", this, SamplerState.LinearWrap, 4);
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
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.
                EngineManager.Device.BlendState = BlendState.Additive;
                EngineManager.Device.RasterizerState = RasterizerState.CullNone;
                EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
                
                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                i = 0;
                randomNumber = random.Next(-15, 15);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("After Burner Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        private float i = 0;
        Random random = new Random();
        private int randomNumber;

        /// <summary>
        /// Render a model.
		/// </summary>		
        internal void RenderModel(Matrix worldMatrix, Model model, Matrix[] boneTransform, AfterBurner material, int meshIndex, int meshPart)
        {
            try
            {
                if (boneTransform != null)
                    worldMatrix = boneTransform[meshIndex + 1] * worldMatrix;
                spWorldViewProj.Value = worldMatrix * viewMatrix * projectionMatrix;
                spColorizeTexture.Value = colorizeTexture;
                spDiffTexture.Value = diffTexture;

                // This should be read it from the material but it is for testing and I just ignore it.
                spNoiseParams.Value = new Vector3(material.Intensity, Time.GameTotalTime * 2000 + i, 0.018f);
                i += Time.SmoothFrameTime * 3.4f;
                int randomOffset = random.Next(-5, 5);
                spScaleParams.Value = new Vector2(0.98f, material.Length + (float)randomNumber / 500f + (float)randomOffset / 500f);

                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to render model.", e);
            }
        } // RenderModel

		#endregion

    } // ConstantShader
} // XNAFinalEngine.Graphics

