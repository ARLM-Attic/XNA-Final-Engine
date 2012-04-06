
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
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Combine depth and normals (foliage and opaque) for shadows (locals and global)
    /// </summary>
    /// <remarks>
    /// This version was not tested.
    /// </remarks>
    internal class CombineDepthAndNormalsShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static CombineDepthAndNormalsShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static CombineDepthAndNormalsShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new CombineDepthAndNormalsShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epDepthFoliageTexture,
                                       epDepthOpaqueTexture,
                                       epNormalsFoliageTexture,
                                       epNormalsOpaqueTexture;

        #region Half Pixel

        private static Vector2? lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 halfPixel)
        {
            if (lastUsedHalfPixel != halfPixel)
            {
                lastUsedHalfPixel = halfPixel;
                epHalfPixel.SetValue(halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region Depth Foliage Texture

        private static Texture2D lastUsedDepthFoliageTexture;
        private static void SetDepthFoliageTexture(Assets.Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[2] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDepthFoliageTexture != depthTexture.Resource)
            {
                lastUsedDepthFoliageTexture = depthTexture.Resource;
                epDepthFoliageTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthFoliageTexture

        #endregion

        #region Depth Opaque Texture

        private static Texture2D lastUsedDepthOpaqueTexture;
        private static void SetDepthOpaqueTexture(Assets.Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDepthOpaqueTexture != depthTexture.Resource)
            {
                lastUsedDepthOpaqueTexture = depthTexture.Resource;
                epDepthOpaqueTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthOpaqueTexture

        #endregion

        #region Normals Foliage Texture

        private static Texture2D lastUsedNormalsFoliageTexture;
        private static void SetNormalsFoliageTexture(Assets.Texture normalTexture)
        {
            EngineManager.Device.SamplerStates[3] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedNormalsFoliageTexture != normalTexture.Resource)
            {
                lastUsedNormalsFoliageTexture = normalTexture.Resource;
                epNormalsFoliageTexture.SetValue(normalTexture.Resource);
            }
        } // SetNormalsFoliageTexture

        #endregion

        #region Normals Opaque Texture

        private static Texture2D lastUsedNormalsOpaqueTexture;
        private static void SetNormalsOpaqueTexture(Assets.Texture normalTexture)
        {
            EngineManager.Device.SamplerStates[1] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedNormalsOpaqueTexture != normalTexture.Resource)
            {
                lastUsedNormalsOpaqueTexture = normalTexture.Resource;
                epNormalsOpaqueTexture.SetValue(normalTexture.Resource);
            }
        } // SetNormalsOpaqueTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Combine depth and normals (foliage and opaque) for shadows (locals and global)
        /// </summary>
        private CombineDepthAndNormalsShader() : base("GBuffer\\CombineDepthAndNormals") { }

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
                epHalfPixel             = Resource.Parameters["halfPixel"];
                epDepthFoliageTexture   = Resource.Parameters["depthFoliageTexture"];
                epDepthOpaqueTexture    = Resource.Parameters["depthOpaqueTexture"];
                epNormalsFoliageTexture = Resource.Parameters["normalsFoliageTexture"];
                epNormalsOpaqueTexture  = Resource.Parameters["normalsOpaqueTexture"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Combine

        /// <summary>
        /// Combine depth and normals (foliage and opaque) for shadows (locals and global)
        /// </summary>
        internal RenderTarget.RenderTargetBinding Render(RenderTarget depthOpaqueTexture, RenderTarget depthFoliageTexture,
                                                       RenderTarget normalOpaqueTexture, RenderTarget normalFoliageTexture)
        {
            try
            {
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                SetHalfPixel(new Vector2(-1f / depthOpaqueTexture.Width, 1f / depthOpaqueTexture.Height));
                SetDepthFoliageTexture(depthFoliageTexture);
                SetDepthOpaqueTexture(depthOpaqueTexture);
                SetNormalsFoliageTexture(normalFoliageTexture);
                SetNormalsOpaqueTexture(normalOpaqueTexture);

                RenderTarget.RenderTargetBinding renderTargetBinding = RenderTarget.Fetch(depthOpaqueTexture.Size,
                                                                                          depthOpaqueTexture.SurfaceFormat,
                                                                                          DepthFormat.None,
                                                                                          normalOpaqueTexture.SurfaceFormat);

                // With multiple render targets the performance can be improved.
                RenderTarget.EnableRenderTargets(renderTargetBinding);
                RenderTarget.ClearCurrentRenderTargets(Color.White);

                // Start effect (current technique should be set));
                Resource.CurrentTechnique = Resource.Techniques["CombineDepthNormals"];
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();

                RenderTarget.DisableCurrentRenderTargets();
                return renderTargetBinding;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Combine Depth and Normals Shader: Unable to render.", e);
            }
        } // Render

        #endregion

    } // CombineDepthAndNormalsShader
} // XNAFinalEngine.Graphics
