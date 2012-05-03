
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Downsample a depth map to half size. 
    /// A special depth downsampler is need because the average of depth values doesn’t have physical sense. 
    /// Instead is better to take the maximum depth value of the four samples. This has the effect of shrinking the object silhouettes.
    /// Of course this isn’t perfect, but normally a depth downsampled buffer is used for low frequency effects.
    /// </summary>
    internal class DepthDownsamplerShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static DepthDownsamplerShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static DepthDownsamplerShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new DepthDownsamplerShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epQuarterTexel,
                                       epDepthTexture;


        private static Vector2 lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        } // SetHalfPixel

        private static Vector2 lastUsedQuarterTexel;
        private static void SetQuarterTexel(Vector2 _quarterTexel)
        {
            if (lastUsedQuarterTexel != _quarterTexel)
            {
                lastUsedQuarterTexel = _quarterTexel;
                epQuarterTexel.SetValue(_quarterTexel);
            }
        } // SetQuarterTexel

        #region Depth Texture

        private static Texture2D lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDepthTexture != depthTexture.Resource)
            {
                lastUsedDepthTexture = depthTexture.Resource;
                epDepthTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Depth Downsampler Shader
        /// </summary>
        private DepthDownsamplerShader() : base("GBuffer\\DownsampleDepth") { }

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
                epHalfPixel    = Resource.Parameters["halfPixel"];
                    epHalfPixel.SetValue(lastUsedHalfPixel);
                epQuarterTexel = Resource.Parameters["quarterTexel"];
                    epQuarterTexel.SetValue(lastUsedQuarterTexel);
                epDepthTexture = Resource.Parameters["depthTexture"];
                    if (lastUsedDepthTexture != null && !lastUsedDepthTexture.IsDisposed)
                        epDepthTexture.SetValue(lastUsedDepthTexture);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Downsample

        /// <summary>
        /// Downsample depth map.
        /// </summary>
        internal RenderTarget Render(RenderTarget depthTexture)
        {
            try
            {
                // Set Parameters
                SetHalfPixel(new Vector2(-1f / (depthTexture.Width / 2), 1f / (depthTexture.Height / 2))); // Use size of destinantion render target.
                SetQuarterTexel(new Vector2(0.5f / depthTexture.Width, 0.5f / depthTexture.Height));
                SetDepthTexture(depthTexture);
                // Set Render States
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                RenderTarget destinationRenderTarget = RenderTarget.Fetch(depthTexture.Size.HalfSize(),
                                                                          depthTexture.SurfaceFormat, depthTexture.DepthFormat,
                                                                          depthTexture.Antialiasing);

                destinationRenderTarget.EnableRenderTarget();
                Resource.CurrentTechnique = Resource.Techniques["DownsampleDepth"];
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                destinationRenderTarget.DisableRenderTarget();

                return destinationRenderTarget;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Depth Downsampler Shader: Unable to render.", e);
            }
        } // Render

        #endregion

    } // DepthDownsampler
} // XNAFinalEngine.Graphics
