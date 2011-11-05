
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
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// This shader generates a depth map, a normal map, a specular power map and a motion vectors map.
    /// It stores the result in several render targets (depth texture, normal texture, and motion vector and specular power texture).
    /// The depth texture has a surface format of 32 bits single channel precision, and the normal has a half vector 2 format (r16f g16f). 
    /// The normals are store with spherical coordinates and the depth is store using the equation: -DepthVS / FarPlane.
    /// </summary>
    internal static class GBufferPass
    {

        #region Variables

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
                // Multisampling on normal and depth maps makes no physical sense!!
                // Depth Texture: Single Z-Buffer texture with 32 bits single channel precision. Equation: -DepthVS / FarPlane
                //
                // Normal map: Normal Map in half vector 2 format (r16f g16f) and using spherical coordinates.
                // Half vector 2 format (r16f g16f). Be careful, in some GPUs this surface format is changed to the RGBA1010102 format.
                // The XBOX 360 supports it though (http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/rendertarget-formats-in-xna-game-studio-4-0.aspx)
                //
                // Motion Vector and Specular Power texture:
                // R: Motion vector X
                // G: Motion vector Y
                // B: Specular Power.
                // A: Unused... yet.
                renderTargetBinding = RenderTarget.Fetch(size, SurfaceFormat.Single, DepthFormat.Depth24, SurfaceFormat.HalfVector2, SurfaceFormat.Color);

                // Set Render States.
                EngineManager.Device.BlendState        = BlendState.Opaque;
                EngineManager.Device.RasterizerState   = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                // With multiple render targets the GBuffer performance can be vastly improved.
                RenderTarget.EnableRenderTargets(renderTargetBinding);
                RenderTarget.ClearCurrentRenderTargets(Color.White);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("GBuffer Pass: Unable to begin the rendering.", e);
            }
        } // Begin
        
        #endregion
        
        #region End

        /// <summary>
        /// Resolve render targets  and return three textures with the G-Buffer.
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
                throw new InvalidOperationException("GBuffer: Unable to end the rendering.", e);
            }
        } // Begin

        #endregion

    } // GBufferPass
} // XNAFinalEngine.Graphics
