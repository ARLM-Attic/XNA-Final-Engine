
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
using TextureCube = Microsoft.Xna.Framework.Graphics.TextureCube;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Skybox Shader.
    /// </summary>
    internal class SkyboxShader : Shader
    {

        #region Variables

        /// <summary>
        /// The geometry used by the shader.
        /// </summary>
        private readonly Box skyboxModel;

        // Singleton reference.
        private static SkyboxShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static SkyboxShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new SkyboxShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters
        /// <summary>
        /// Effect handles
        /// </summary>
        private static  EffectParameter epViewProjection,
                                        epCubeMapTexture,
                                        epIntensity,
                                        epMaxRange,
                                        epAlphaBlending;

        #region View Projection Matrix

        private static Matrix? lastUsedViewProjectionMatrix;
        private static void SetViewProjectionMatrix(Matrix viewProjectionMatrix)
        {
            if (lastUsedViewProjectionMatrix != viewProjectionMatrix)
            {
                lastUsedViewProjectionMatrix = viewProjectionMatrix;
                epViewProjection.SetValue(viewProjectionMatrix);
            }
        } // SetViewProjectionMatrix

        #endregion

        #region Cube Map

        private static TextureCube lastUsedCubeTexture;
        private static void SetCubeTexture(Assets.TextureCube cubeTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.LinearClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedCubeTexture != cubeTexture.Resource)
            {
                lastUsedCubeTexture = cubeTexture.Resource;
                epCubeMapTexture.SetValue(cubeTexture.Resource);
            }
        } // SetCubeTexture

        #endregion

        #region Intensity

        private static float? lastUsedIntensity;
        private static void SetIntensity(float _intensity)
        {
            if (lastUsedIntensity != _intensity)
            {
                lastUsedIntensity = _intensity;
                epIntensity.SetValue(_intensity);
            }
        } // SetIntensity

        #endregion

        #region Max Range

        private static float? lastUsedMaxRange;
        private static void SetMaxRange(float maxRange)
        {
            if (lastUsedMaxRange != maxRange)
            {
                lastUsedMaxRange = maxRange;
                epMaxRange.SetValue(maxRange);
            }
        } // SetMaxRange

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

        #endregion

        #region Constructor

        /// <summary>
        /// Skybox Shader.
		/// </summary>
        internal SkyboxShader() : base("Sky\\Skybox")
        {
            skyboxModel = new Box(1);
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
                epViewProjection = Resource.Parameters["ViewITProj"];
                epCubeMapTexture = Resource.Parameters["CubeMapTexture"];
                epAlphaBlending  = Resource.Parameters["alphaBlending"];
                epIntensity      = Resource.Parameters["intensity"];
                epMaxRange       = Resource.Parameters["maxRange"];
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
        internal void Render(Matrix viewMatrix, Matrix projectionMatrix, float farPlane, Skybox skybox)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = BlendState.NonPremultiplied;
                EngineManager.Device.RasterizerState = RasterizerState.CullClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                SetViewProjectionMatrix(Matrix.CreateScale(farPlane) * Matrix.Transpose(Matrix.Invert(viewMatrix)) * projectionMatrix); // I remove the translation and scale of the view matrix.
                SetAlphaBlending(skybox.AlphaBlending);
                SetIntensity(skybox.ColorIntensity);
                SetCubeTexture(skybox.TextureCube);

                if (skybox.TextureCube.IsRgbm)
                {
                    SetMaxRange(skybox.TextureCube.RgbmMaxRange);
                    Resource.CurrentTechnique = Resource.Techniques["SkyboxRGBM"];
                }
                else
                    Resource.CurrentTechnique = Resource.Techniques["Skybox"];

                Resource.CurrentTechnique.Passes[0].Apply();
                skyboxModel.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Skybox shader: Unable to render the sky.", e);
            }
        } // Render

		#endregion

    } // SkyboxShader
} // XNAFinalEngine.Graphics

