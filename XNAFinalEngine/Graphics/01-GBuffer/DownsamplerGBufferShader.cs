
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
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Downsample a depth and normal texture to half size. 
    /// A special depth downsampler is need because the average of depth values doesn’t have physical sense. 
    /// Instead is better to take the maximum depth value of the four samples. This has the effect of shrinking the object silhouettes.
    /// Of course this isn’t perfect, but normally a depth downsampled buffer is used for low frequency effects.
    /// </summary>
    internal class DownsamplerGBufferShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static DownsamplerGBufferShader instance;

        // Shader Parameters.
        private static ShaderParameterVector2 spHalfPixel, spQuarterTexel;
        private static ShaderParameterTexture spDepthTexture, spNormalTexture;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static DownsamplerGBufferShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new DownsamplerGBufferShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Depth Downsampler Shader
        /// </summary>
        private DownsamplerGBufferShader() : base("GBuffer\\DownsampleGBuffer") { }

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
                spQuarterTexel = new ShaderParameterVector2("quarterTexel", this);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
                spNormalTexture = new ShaderParameterTexture("normalTexture", this, SamplerState.LinearClamp, 2);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Render

        /// <summary>
        /// Downsample G-Buffer.
        /// </summary>
        internal RenderTarget.RenderTargetBinding Render(RenderTarget depthTexture, RenderTarget normalTexture)
        {
            try
            {
                // Set Parameters
                spHalfPixel.Value = new Vector2(-0.5f / (depthTexture.Width / 2), 0.5f / (depthTexture.Height / 2)); // Use size of destinantion render target.
                spQuarterTexel.Value = new Vector2(0.25f / depthTexture.Width, 0.25f / depthTexture.Height);
                spDepthTexture.Value = depthTexture;
                spNormalTexture.Value = normalTexture;
                // Set Render States
                EngineManager.Device.DepthStencilState = DepthStencilState.None;

                RenderTarget.RenderTargetBinding renderTargetBinding = RenderTarget.Fetch(new Size(depthTexture.Width / 2, depthTexture.Height / 2), 
                                                                                          SurfaceFormat.Single, DepthFormat.None, SurfaceFormat.Color);

                RenderTarget.EnableRenderTargets(renderTargetBinding);
                Resource.CurrentTechnique = Resource.Techniques["DownsampleGBuffer"];
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                RenderTarget.DisableCurrentRenderTargets();

                return renderTargetBinding;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Downsampler Shader: Unable to render.", e);
            }
        } // Render

        #endregion

    } // DownsamplerGBufferShader
} // XNAFinalEngine.Graphics
