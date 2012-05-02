
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
    /// Skydome Shader.
    /// </summary>
    internal class SkydomeShader : Shader
    {

        #region Variables

        /// <summary>
        /// The geometry used by the shader.
        /// </summary>
        private readonly Model skydomeModel;

        // Singleton reference.
        private static SkydomeShader instance;

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

        #region Shader Parameters
        /// <summary>
        /// Effect handles
        /// </summary>
        private static  EffectParameter epViewProjection,
                                        epTexture;

        #region View Projection Matrix

        private static Matrix? lastUsedViewProjectionMatrix;
        private static void SetViewProjectionMatrix(Matrix viewProjectionMatrix)
        {
            if (lastUsedViewProjectionMatrix != viewProjectionMatrix || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedViewProjectionMatrix = viewProjectionMatrix;
                epViewProjection.SetValue(viewProjectionMatrix);
            }
        } // SetViewProjectionMatrix

        #endregion
        
        #region Texture

        private static Texture2D lastUsedTexture;
        private static void SetTexture(Texture texture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.AnisotropicClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedTexture != texture.Resource || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedTexture = texture.Resource;
                epTexture.SetValue(texture.Resource);
            }
        } // SetTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Skybox Shader.
		/// </summary>
        private SkydomeShader() : base("Sky\\Skydome")
        {
            skydomeModel = new FileModel("Skydome");

            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            Texture skyTextureNight = new Texture("Shaders\\SkyNight");
            Resource.Parameters["SkyTextureNight"].SetValue(skyTextureNight.Resource);
            Texture skyTextureSunset = new Texture("Shaders\\SkySunset");
            Resource.Parameters["SkyTextureSunset"].SetValue(skyTextureSunset.Resource);
            Texture skyTextureDay = new Texture("Shaders\\SkyDay");
            Resource.Parameters["SkyTextureDay"].SetValue(skyTextureDay.Resource);
            ContentManager.CurrentContentManager = userContentManager;
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
                epTexture = Resource.Parameters["diffuseTexture"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
		} // GetParametersHandles

		#endregion

        #region Render

        private float angle = 1;//3.1416f / 2;

        /// <summary>
        /// Render the sky.
		/// </summary>		
        internal void Render(Matrix viewMatrix, Matrix projectionMatrix, float farPlane, Skydome skydome)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = BlendState.NonPremultiplied;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                Matrix worldMatrix = Matrix.CreateScale(1f);
                SetViewProjectionMatrix(worldMatrix * Matrix.Transpose(Matrix.Invert(viewMatrix)) * projectionMatrix); // I remove the translation and scale of the view matrix.
                SetTexture(skydome.Texture);

                Resource.Parameters["World"].SetValue(worldMatrix);
                Resource.Parameters["ViewInv"].SetValue(Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix))));
                angle += 0.001f;
                Resource.Parameters["LightDirection"].SetValue(new Vector3(0, (float)Math.Cos(angle), (float)Math.Sin(angle)));
                
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

