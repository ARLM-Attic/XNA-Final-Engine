
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
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Skydome Shader.
    /// TODO: Work in progress.
    /// </summary>
    internal class SkydomeShader : Shader
    {

        #region Variables
        
        // The geometry used by the shader.
        private readonly Model skydomeModel;

        // Singleton reference.
        private static SkydomeShader instance;

        private static Texture skyTextureNight, skyTextureSunset, skyTextureDay;

        private static ShaderParameterMatrix spViewProjectionMatrix, spWorldMatrix, spViewInverseMatrix;
        private static ShaderParameterVector3 spLightDirection;
        private static ShaderParameterTexture spTexture, spSkyNightTexture, spSkySunsetTexture, spSkyDayTexture;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static SkydomeShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new SkydomeShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Skybox Shader.
		/// </summary>
        private SkydomeShader() : base("Sky\\Skydome")
        {
            skydomeModel = new FileModel("Skydome");
            AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
            AssetContentManager.CurrentContentManager = AssetContentManager.SystemContentManager;
            skyTextureNight  = new Texture("Shaders\\SkyNight");
            skyTextureSunset = new Texture("Shaders\\SkySunset");
            skyTextureDay    = new Texture("Shaders\\SkyDay");
            AssetContentManager.CurrentContentManager = userContentManager;
        } // SkyboxShader

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
                spViewProjectionMatrix = new ShaderParameterMatrix("ViewITProj", this);
                spWorldMatrix = new ShaderParameterMatrix("World", this);
                spViewInverseMatrix = new ShaderParameterMatrix("ViewInv", this);
                spLightDirection = new ShaderParameterVector3("LightDirection", this);
                spTexture = new ShaderParameterTexture("diffuseTexture", this, SamplerState.AnisotropicClamp, 0);
                spSkyNightTexture = new ShaderParameterTexture("SkyTextureNight", this, SamplerState.AnisotropicClamp, 5);
                spSkySunsetTexture = new ShaderParameterTexture("SkyTextureSunset", this, SamplerState.AnisotropicClamp, 6);
                spSkyDayTexture = new ShaderParameterTexture("SkyTextureDay", this, SamplerState.AnisotropicClamp, 7);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
		} // GetParametersHandles

		#endregion

        #region Render

        /// <summary>
        /// Render the sky.
		/// </summary>		
        internal void Render(Matrix viewMatrix, Matrix projectionMatrix, float farPlane, Vector3 sunLightDirection, Skydome skydome)
        {
            try
            {
                Matrix worldMatrix = Matrix.CreateScale(1f);
                spViewProjectionMatrix.Value = worldMatrix * Matrix.Transpose(Matrix.Invert(viewMatrix)) * projectionMatrix; // I remove the translation and scale of the view matrix.
                spTexture.Value = skydome.Texture;
                spWorldMatrix.Value = worldMatrix;
                spViewInverseMatrix.Value = Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix)));
                spLightDirection.Value = sunLightDirection;
                spSkyDayTexture.Value = skyTextureDay;
                spSkyNightTexture.Value = skyTextureNight;
                spSkySunsetTexture.Value = skyTextureSunset;
                
                Resource.CurrentTechnique.Passes[0].Apply();
                skydomeModel.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Skybox shader: Unable to render the sky.", e);
            }
        } // Render

		#endregion

    } // SkydomeShader
} // XNAFinalEngine.Graphics

