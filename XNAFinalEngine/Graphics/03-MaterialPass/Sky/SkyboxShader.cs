
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
        private static FileModel skyboxModel;

        // Singleton reference.
        private static SkyboxShader instance;

        // Shader Parameters.
        private static ShaderParameterMatrix spViewProjectionMatrix;
        private static ShaderParameterTextureCube spCubeTexture;
        private static ShaderParameterFloat spIntensity, spMaxRange, spAlphaBlending;

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

        #region Constructor

        /// <summary>
        /// Skybox Shader.
		/// </summary>
        private SkyboxShader() : base("Sky\\Skybox")
        {
            if (skyboxModel == null)
                skyboxModel = new FileModel("Skybox"); // To avoid changing the razterization state.
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
                spCubeTexture = new ShaderParameterTextureCube("CubeMapTexture", this, SamplerState.LinearClamp, 0);
                spIntensity = new ShaderParameterFloat("intensity", this);
                spMaxRange = new ShaderParameterFloat("maxRange", this);
                spAlphaBlending = new ShaderParameterFloat("alphaBlending", this);
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
                spViewProjectionMatrix.Value = Matrix.CreateScale(farPlane) * Matrix.Transpose(Matrix.Invert(viewMatrix)) * projectionMatrix; // I remove the translation and scale of the view matrix.
                spAlphaBlending.Value = skybox.AlphaBlending;
                spIntensity.Value = skybox.ColorIntensity;
                spCubeTexture.Value = skybox.TextureCube;

                if (skybox.TextureCube.IsRgbm)
                {
                    spMaxRange.Value = skybox.TextureCube.RgbmMaxRange;
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

