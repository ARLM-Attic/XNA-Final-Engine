
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
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Light Pre Pass.
    /// </summary>
    internal static class LightPrePass
    {

        #region Variables

        /// <summary>
        /// Additive blending state.
        /// The resulting color will be added to current render target color.
        /// </summary>
        private static BlendState additiveBlendingState;

        /// <summary>
        /// Light Texture.
        /// </summary>
        /// <remarks> 
        /// In PC the HDR Blendable format is implemented using HalfVector4. However in XBOX 360 is implemented using
        /// an Xbox specific 7e3 (32 bits per pixel) EDRAM format (or 10101002). That means less precision, more speed,
        /// less memory space used and the lack of the fourth channel (2 bits doesn’t help much).
        /// 
        /// The lack of precision is not a big deal and we have better performance.
        /// Unfortunately this deferred pipeline implementation needs the fourth channel for monochromatic specular highlights.
        /// A possible solution for the XBOX 360 if to use a second HDR Blendable render target for specular highlight.
        /// With this we have the same memory requisites but at least we could have color specular highlights,
        /// in practice this is not an incredible gain, though (see Crytek’s presentation).
        /// 
        /// And to make things worse, outside the EDRAM it expands to HalfVector4 when resolved into the system memory texture. 
        /// Twice the size and twice the bandwidth with an already precision lost. But that just life, we don’t have a choice.
        ///
        /// In conclusion, we need two render targets for the light pass on the Xbox 360,
        /// but to make everything simpler, PC will use also a second render target.
        /// 
        /// One more thing, I can't use a color surface format because the RGBM format doesn't work with additive blending.
        /// </remarks>
        // private static RenderTarget lightTexture;
        // This structure is used to set multiple render targets without generating garbage in the process.
        private static RenderTarget.RenderTargetBinding renderTargetBinding;

        #endregion

        #region Begin
        
        /// <summary>
        /// Begins the G-Buffer render.
        /// </summary>
        internal static void Begin(Size size)
        {
            try
            {
                if (additiveBlendingState == null)
                {
                    // The resulting color will be added to current render target color.
                    additiveBlendingState = new BlendState
                    {
                        AlphaBlendFunction = BlendFunction.Add,
                        AlphaDestinationBlend = Blend.One,
                        AlphaSourceBlend = Blend.One,
                        ColorBlendFunction = BlendFunction.Add,
                        ColorDestinationBlend = Blend.One,
                        ColorSourceBlend = Blend.One,
                    };
                }

                // Set Render States.
                EngineManager.Device.BlendState        = additiveBlendingState; // The resulting color will be added to current render target color.
                EngineManager.Device.RasterizerState   = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                // lightTexture = RenderTarget.Fetch(size, SurfaceFormat.HdrBlendable, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                // lightTexture.EnableRenderTarget();
                renderTargetBinding = RenderTarget.Fetch(size, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8, SurfaceFormat.HdrBlendable);
                RenderTarget.EnableRenderTargets(renderTargetBinding);
                RenderTarget.ClearCurrentRenderTargets(new Color(0, 0, 0, 0));
            } // try
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass: Unable to begin the rendering.", e);
            }
        } // Begin
        
        #endregion

        #region End

        /// <summary>
        /// Resolve render targets and return a texture with the light information.
        /// </summary>
        internal static RenderTarget.RenderTargetBinding End()
        {
            try
            {
                RenderTarget.DisableCurrentRenderTargets();
                return renderTargetBinding;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass: Unable to end the rendering.", e);
            }
        } // End

        #endregion

    } // LightPrePass
} // XNAFinalEngine.Graphics
