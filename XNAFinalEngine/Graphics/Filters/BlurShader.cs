
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
	/// <summary>
	/// Blur shader.
	/// </summary>
    public class BlurShader : Shader
	{

		#region Variables

		/// <summary>
		/// Auxiliary render target.
		/// </summary>
        private readonly RenderTarget blurTempTexture;
        
        #endregion

        #region Shader Parameters

	    /// <summary>
	    /// Effect handles
	    /// </summary>
	    private static EffectParameter epTextureResolution,
	                                   epBlurWidth,
	                                   epTexture,
                                       epHalfPixel;

        #region Blur Width

        /// <summary>
        /// Last used blur width
        /// </summary>
        private static float? lastUsedBlurWidth;
        /// <summary>
        /// Set Blur Width (between 0 and 10)
        /// </summary>
        private static void SetBlurWidth(float _blurWidth)
        {
            if (lastUsedBlurWidth != _blurWidth && _blurWidth >= 0.0f && _blurWidth <= 10.0f)
            {
                lastUsedBlurWidth = _blurWidth;
                epBlurWidth.SetValue(_blurWidth);
            }
        } // SetBlurWidth

		#endregion

        #region Half Pixel

        /// <summary>
        /// Last used half pixel.
        /// </summary>
        private static Vector2? lastUsedHalfPixel;
        /// <summary>
        /// Set Half Pixel.
        /// </summary>
        private static void SetHalfPixel(Vector2 halfPixel)
        {
            if (lastUsedHalfPixel != halfPixel)
            {
                lastUsedHalfPixel = halfPixel;
                epHalfPixel.SetValue(halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region Texture

        /// <summary>
        /// Last used texture.
        /// </summary>
        private static Texture lastUsedTexture;
        /// <summary>
        /// Set scene texture.
        /// </summary>
        private static void SetTexture(Texture texture)
        {
            // XNA 4.0 reconstructs automatically the render targets when a device is lost.
            // However the shaders have to re set to the GPU the new render targets to work properly.
            // This problem seems to manifest only with floating point formats.
            // So it's a floating point texture set it every time that is need it.
            if (lastUsedTexture != texture ||
                (texture is RenderTarget && ((RenderTarget)texture).SurfaceFormat != SurfaceFormat.Color))
            {
                lastUsedTexture = texture;
                epTexture.SetValue(texture.Resource);
            }
        } // SetTexture

        #endregion

        #region Texture Resolution

        /// <summary>
        /// Last used texture resolution.
        /// </summary>
        private static Vector2? lastUsedTextureResolution;
        /// <summary>
        /// Set texture resolution.
        /// </summary>
        private static void SetTextureResolution(Vector2 textureResolution)
        {
            if (lastUsedTextureResolution != textureResolution)
            {
                lastUsedTextureResolution = textureResolution;
                epTextureResolution.SetValue(textureResolution);
            }
        } // SetTextureResolution

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Blur shader.
        /// </summary>
        private BlurShader(Size size) : base("Filters\\Blur")
		{
            blurTempTexture = new RenderTarget(size, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
        } // BlurShader

		#endregion

		#region Get parameters handles

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
                epTextureResolution = Resource.Parameters["textureResolution"];
                epBlurWidth         = Resource.Parameters["blurWidth"];
                epTexture           = Resource.Parameters["sceneTexture"];
                epHalfPixel         = Resource.Parameters["halfPixel"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

		#endregion

        #region Filter

        /// <summary>
        /// Blurs a texture.
        /// </summary>
        /// <param name="texture">The texture to blur. The result will be placed here.</param>
        /// <param name="pointFilter">Use point filter or linear filter.</param>
        /// <param name="width">Blur Width. A value of 1 gives normally the better results and the better performance.</param>
        internal void Filter(RenderTarget texture, bool pointFilter = true, float width = 1.0f)
		{
            if (texture == null || texture.Resource == null)
                throw new ArgumentNullException("texture");
            if (texture.Width != blurTempTexture.Width || texture.Height != blurTempTexture.Height)
                throw new ArgumentException("Blur Shader: Texture size does not match the blur internal texture size", "texture");
            try
            {
                // Set Render States
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                // Works with point or linear filter?
                if (pointFilter)
                {
                    Resource.CurrentTechnique = Resource.Techniques["BlurPoint"];
                    EngineManager.Device.SamplerStates[5] = SamplerState.PointClamp;
                }
                else
                {
                    Resource.CurrentTechnique = Resource.Techniques["BlurLinear"];
                    EngineManager.Device.SamplerStates[6] = SamplerState.LinearClamp;
                }
                
                // Set shader parameters
                SetBlurWidth(width);
                SetTextureResolution(new Vector2(texture.Width, texture.Height));
                SetTexture(texture);
                SetHalfPixel(new Vector2(-1f / texture.Width, 1f / texture.Height));

                foreach (EffectPass pass in Resource.CurrentTechnique.Passes)
                {
                    if (pass.Name == "BlurHorizontal")
                        blurTempTexture.EnableRenderTarget();
                    else
                        texture.EnableRenderTarget();

                    pass.Apply();
                    RenderScreenPlane();

                    if (pass.Name == "BlurHorizontal")
                    {
                        blurTempTexture.DisableRenderTarget();
                        SetTexture(blurTempTexture);
                    }
                    else
                        texture.DisableRenderTarget();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Blur Shader: Unable to render.", e);
            }
        } // Render

		#endregion
        
        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            blurTempTexture.Dispose();
        } // DisposeManagedResources

        #endregion

        #region Stored Shaders

        // A pool of all bloom shaders.
        private static readonly Dictionary<Size, BlurShader> shaders = new Dictionary<Size, BlurShader>(1);

        /// <summary>
        /// Returns a bloom shader for this size.
        /// The shaders are stored in a pool.
        /// </summary>
        public static BlurShader GetShader(Size size)
        {
            if (shaders.ContainsKey(size))
                return shaders[size];
            // If not return a new one.
            shaders[size] = new BlurShader(size);
            return shaders[size];
        } // GetShader

        /// <summary>
        /// Dispose stored shaders.
        /// </summary>
        public static void DisposeShaders()
        {
            foreach (var shader in shaders)
            {
                shader.Value.Dispose();
            }
            shaders.Clear();
        } // DisposeShaders

        #endregion

    } // BlurShader
} // XNAFinalEngine.Graphics
