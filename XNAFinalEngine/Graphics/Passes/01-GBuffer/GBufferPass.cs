
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
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// This shader generates a depth map, a normal map, and specular power map.
    /// It stores the result in several render targets.
    /// The depth texture has a surface format of 32 bits single channel precision. Equation: -DepthVS / FarPlane
    /// The normals are store using best fit normals for maximum compression (24 bits), and are stored in view space, 
    /// but best fit normals works better in world space, this is specially noticed in the presence of big triangles. 
    /// The specular power is stored in 8 bits following Killzone 2 method.
    /// There is room in the depth surface to store a mask for ambient lighting (Crysis 2 and Toy Story 3 method).
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
                // Except MSAA is controled like was proposed in the ShaderX7 article. 
                // The problem is that the available hardware does not support this technique.

                // The depth texture has a surface format of 32 bits single channel precision. Equation: -DepthVS / FarPlane
                // The normals are store using best fit normals for maximum compression (24 bits), and are stored in view space, 
                // but best fit normals works better in world space, this is specially noticed in the presence of big triangles. 
                // The specular power is stored in 8 bits following Killzone 2 method.
                // There is room in the depth surface to store a mask for ambient lighting (Crysis 2 and Toy Story 3 method).
                renderTargetBinding = RenderTarget.Fetch(size, SurfaceFormat.Single, DepthFormat.Depth24, SurfaceFormat.Color);

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
        } // End

        #endregion

    } // GBufferPass
} // XNAFinalEngine.Graphics
