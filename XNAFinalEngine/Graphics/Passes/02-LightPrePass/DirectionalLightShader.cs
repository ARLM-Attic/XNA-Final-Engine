
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
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Assets;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Light Pre Pass Directional Light Shader.
    /// </summary>
    internal class DirectionalLightShader : Shader
    {

        #region Variables
        
        // Current view matrix. Used to set the shader parameters.
        private Matrix viewMatrix;

        // Singleton reference.
        private static DirectionalLightShader instance;

        // State to avoid calculating lighting over the sky.
        private static DepthStencilState avoidSkyDepthStencilState;

        // Shader Parameters.
        private static ShaderParameterFloat spLightIntensity;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterColor spLightColor;
        private static ShaderParameterTexture spDepthTexture, spNormalTexture, spShadowTexture;
        private static ShaderParameterVector3 spLightDirection;
        private static ShaderParameterVector3Array spFrustumCorners;

        // Techniques references.
        private static EffectTechnique directionalLightWithShadowsTechnique,
                                       directionalLightTechnique;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a directional light shader.
        /// </summary>
        public static DirectionalLightShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new DirectionalLightShader();
                return instance;
            }
        } // Instance

        #endregion
        
        #region Constructor

        /// <summary>
        /// Light Pre Pass Directional Light Shader.
        /// </summary>
        private DirectionalLightShader() : base("LightPrePass\\DirectionalLight")
        {
            // If the depth is 1 (sky) then I do not calculate the ambient light in this texels.
            avoidSkyDepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.NotEqual,
            };
        } // DirectionalLightShader

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override void GetParametersHandles()
        {
            try
            {
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spLightIntensity = new ShaderParameterFloat("lightIntensity", this);
                spLightColor = new ShaderParameterColor("lightColor", this);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
                spNormalTexture = new ShaderParameterTexture("normalTexture", this, SamplerState.PointClamp, 1);
                spShadowTexture = new ShaderParameterTexture("shadowTexture", this, SamplerState.PointClamp, 3);
                spLightDirection = new ShaderParameterVector3("lightDirection", this);
                spFrustumCorners = new ShaderParameterVector3Array("frustumCorners", this, 4);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

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
                directionalLightWithShadowsTechnique = Resource.Techniques["DirectionalLightWithShadows"];
                directionalLightTechnique = Resource.Techniques["DirectionalLight"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

        #endregion

        #region Begin

        /// <summary>
        /// Begins the directional light rendering.
        /// </summary>
        /// <param name="depthTexture">Gbuffer's depth buffer.</param>
        /// <param name="normalTexture">Gbuffer's normal map.</param>
        /// <param name="viewMatrix">Camera view matrix.</param>
        /// <param name="boundingFrustum">Camera bounding frustum (use the camera's component method).</param>
        internal void Begin(RenderTarget depthTexture, RenderTarget normalTexture, Matrix viewMatrix, Vector3[] boundingFrustum)
        {
            try
            {
                // If the depth is 1 (sky) then I do not calculate the ambient light in this texels.
                EngineManager.Device.DepthStencilState = avoidSkyDepthStencilState;

                spDepthTexture.Value = depthTexture;
                spNormalTexture.Value = normalTexture;
                // The reason that it’s 1/width instead of 0.5/width is because, in clip space, the coordinates range from -1 to 1 (width 2),
                // and not from 0 to 1 (width 1) like they do in textures, so you need to double the movement to account for that.
                spHalfPixel.Value = new Vector2(-0.5f / (depthTexture.Width / 2), 0.5f / (depthTexture.Height / 2)); // I use the depth texture, but I just need the destination render target dimension.
                spFrustumCorners.Value = boundingFrustum;
                this.viewMatrix = viewMatrix;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Directional Light:: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render

        /// <summary>
        /// Render the directional light.
        /// </summary>
        internal void Render(Color diffuseColor, Vector3 direction, float intensity, Texture shadowTexture)
        {
            try
            {
                // Set Parameters
                spLightColor.Value = diffuseColor;
                // The next three lines produce the same result.
                //spLightDirection.Value = Vector3.Transform(light.Direction, Matrix.CreateFromQuaternion(ApplicationLogic.Camera.Orientation));
                //spLightDirection.Value = Vector3.Transform(light.Direction, Matrix.Transpose(Matrix.Invert(ApplicationLogic.Camera.ViewMatrix)));
                spLightDirection.Value = Vector3.TransformNormal(direction, viewMatrix);
                spLightIntensity.Value = intensity;

                if (shadowTexture != null)
                {
                    spShadowTexture.Value = shadowTexture;
                    Resource.CurrentTechnique = directionalLightWithShadowsTechnique;
                }
                else
                {
                    spShadowTexture.Value = Texture.BlackTexture; // To avoid a potential exception.
                    Resource.CurrentTechnique = directionalLightTechnique;
                }
                
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Directional Light: Unable to render.", e);
            }
        } // Render

        #endregion

    } // DirectionalLightShader
} // XNAFinalEngine.Graphics