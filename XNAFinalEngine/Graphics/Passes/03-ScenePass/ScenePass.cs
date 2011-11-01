
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
    /// Scene Pass.
    /// </summary>
    /// <remarks>
    /// The scene will be render in HDR linear space here. Then, the post process will apply tone mapping to transform it to LDR gamma space.
    /// </remarks>
    internal class ScenePass : Disposable
    {

        #region Properties

        /// <summary>
        /// Light Texture.
        /// </summary>
        /// <remarks> 
        /// It's in linear space. In this same render target the transparent object will be rendered. Maybe an RGBM encoding could work, but how?
        /// Multisampling could generate indeseable artifacts. Be careful!
        /// </remarks>
        internal RenderTarget SceneTexture { get; private set; }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Scene Pass.
        /// </summary>
        internal ScenePass(Size size)
        {
            SceneTexture = new RenderTarget(size, SurfaceFormat.HdrBlendable, DepthFormat.Depth24, RenderTarget.AntialiasingType.NoAntialiasing);
        } // ScenePass

        #endregion

        #region Begin
        
        /// <summary>
        /// Begins the G-Buffer render.
        /// </summary>
        internal void Begin(Color clearColor)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState        = BlendState.Opaque; // The resulting color will be added to current render target color.
                EngineManager.Device.RasterizerState   = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.
                
                SceneTexture.EnableRenderTarget();
                
                RenderTarget.ClearCurrentRenderTargets(new Color(clearColor.R, clearColor.G, clearColor.B, 255));
            } // try
            catch (Exception e)
            {
                throw new InvalidOperationException("Linear Space Pass: Unable to begin the rendering.", e);
            }
        } // Begin
        
        #endregion

        #region End

        /// <summary>
        /// Resolve render targets.
        /// </summary>
        internal void End()
        {
            try
            {
                RenderTarget.DisableCurrentRenderTargets();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("LightPrePass: Unable to end the rendering.", e);
            }
        } // Begin

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            SceneTexture.Dispose();
        } // DisposeManagedResources

        #endregion

    } // ScenePass
} // XNAFinalEngine.Graphics
