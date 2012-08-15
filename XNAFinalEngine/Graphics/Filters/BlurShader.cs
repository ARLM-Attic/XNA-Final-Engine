
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
    /// Simple fullscreen blurs.
    /// It does not use depth information and therefore they are not suited for shadows.
	/// </summary>
    public class BlurShader : Shader
	{

		#region Variables
        
        // Singleton reference.
	    private static BlurShader instance;
        
        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a blur shader.
        /// </summary>
	    public static BlurShader Instance
	    {
	        get
	        {
	            if (instance == null)
                    instance = new BlurShader();
	            return instance;
	        }
	    } // Instance

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

        private static float lastUsedBlurWidth;
        private static void SetBlurWidth(float _blurWidth)
        {
            if (lastUsedBlurWidth != _blurWidth)
            {
                lastUsedBlurWidth = _blurWidth;
                epBlurWidth.SetValue(_blurWidth);
            }
        } // SetBlurWidth

		#endregion

        #region Half Pixel

        private static Vector2 lastUsedHalfPixel;
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
        
        private static Texture2D lastUsedTexture;
        private static void SetTexture(Texture texture)
        {
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedTexture != texture.Resource)
            {
                lastUsedTexture = texture.Resource;
                epTexture.SetValue(texture.Resource);
            }
        } // SetTexture

        #endregion

        #region Texture Resolution

        private static Vector2 lastUsedTextureResolution;
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
        private BlurShader() : base("Filters\\Blur") { }

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
                    epTextureResolution.SetValue(lastUsedTextureResolution);
                epBlurWidth         = Resource.Parameters["blurWidth"];
                    epBlurWidth.SetValue(lastUsedBlurWidth);
                epTexture           = Resource.Parameters["sceneTexture"];
                    if (lastUsedTexture != null && !lastUsedTexture.IsDisposed)
                        epTexture.SetValue(lastUsedTexture);
                epHalfPixel         = Resource.Parameters["halfPixel"];
                    epHalfPixel.SetValue(lastUsedHalfPixel);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParametersHandles

		#endregion

        #region Filter

	    /// <summary>
	    /// Blurs a texture.
	    /// </summary>
	    /// <param name="texture">The texture to blur.</param>
	    /// <param name="destionationTexture">The blured texture.</param>
	    /// <param name="width">Blur Width. A value of 1 gives normally the better results and the better performance.</param>
        /// <param name="linearFilter">Use point filter or linear filter. Some render targets only works in point filter.</param>
	    public void Filter(RenderTarget texture, RenderTarget destionationTexture, float width = 1.0f, bool linearFilter = true)
		{
            if (texture == null || texture.Resource == null)
                throw new ArgumentNullException("texture");
            if (destionationTexture == null || destionationTexture.Resource == null)
                throw new ArgumentNullException("destionationTexture");
            try
            {
                RenderTarget blurTempTexture = RenderTarget.Fetch(destionationTexture.Size, destionationTexture.SurfaceFormat, 
                                                                  DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                // Set Render States
                EngineManager.Device.BlendState        = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState   = RasterizerState.CullCounterClockwise;

                // Works with point or linear filter?
                if (linearFilter)
                {
                    Resource.CurrentTechnique = Resource.Techniques["BlurLinear"];
                    EngineManager.Device.SamplerStates[6] = SamplerState.LinearClamp;
                }
                else
                {
                    Resource.CurrentTechnique = Resource.Techniques["BlurPoint"];
                    EngineManager.Device.SamplerStates[5] = SamplerState.PointClamp;
                }

                // Set shader parameters
                SetBlurWidth(width);
                SetTextureResolution(new Vector2(destionationTexture.Width, destionationTexture.Height));
                SetTexture(texture);
                SetHalfPixel(new Vector2(-0.5f / (destionationTexture.Width / 2), 0.5f / (destionationTexture.Height / 2)));

                foreach (EffectPass pass in Resource.CurrentTechnique.Passes)
                {
                    if (pass.Name == "BlurHorizontal")
                        blurTempTexture.EnableRenderTarget();
                    else
                        destionationTexture.EnableRenderTarget();

                    pass.Apply();
                    RenderScreenPlane();

                    if (pass.Name == "BlurHorizontal")
                    {
                        blurTempTexture.DisableRenderTarget();
                        SetTexture(blurTempTexture);
                    }
                    else
                        destionationTexture.DisableRenderTarget();
                }
                // It's not used anymore.
                RenderTarget.Release(blurTempTexture);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Blur Shader: Unable to render.", e);
            }
        } // Filter

		#endregion
        
    } // BlurShader
} // XNAFinalEngine.Graphics
