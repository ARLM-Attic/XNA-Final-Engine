
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
using XNAFinalEngine.Helpers;
using Texture = Microsoft.Xna.Framework.Graphics.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
	/// <summary>
	/// Blurs a texture.
	/// </summary>
    public class Blur : Shader
	{

		#region Variables

		/// <summary>
		/// Auxiliary render target.
		/// </summary>
        private readonly RenderTarget blurTempTexture;

        /// <summary>
        /// Blur Width.
        /// A value of 1 gives the better results and the better performance.
        /// </summary>
        private float width = 1.0f;
        
        #endregion

        #region Properties

        /// <summary>
        /// Blur Width.
        /// A value of 1 gives the better results and the better performance.
        /// </summary>
        public float Width
        {
            get { return width; }
            set { width = value; }
        } // Width

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
            if (EngineManager.DeviceLostInThisFrame || lastUsedTexture != texture)
            {
                lastUsedTexture = texture;
                epTexture.SetValue(texture.XnaTexture);
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
        /// Blurs a texture.
		/// </summary>
        internal Blur(Size size) : base("Filters\\Blur")
		{
            blurTempTexture = new RenderTarget(size, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
        } // Blur

        /// <summary>
        /// Blurs a texture.
        /// </summary>
        internal Blur(RenderTarget.SizeType size)
            : base("Filters\\Blur")
        {
            blurTempTexture = new RenderTarget(size, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
        } // Blur

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

        #region Generate Blur
        /*
        /// <summary>
		/// Generate the blur effect.
		/// </summary>
		/// 
		*/
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="pointFilter"></param>
        /// <param name="width">Blur Width. A value of 1 gives normally the better results and the better performance.</param>
        public void GenerateBlur(RenderTarget texture, bool pointFilter = true, float width = 1.0f)
		{   
            try
            {
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;

                SetBlurWidth(width);
                SetTextureResolution(new Vector2(texture.Width, texture.Height));
                SetTexture(texture);
                SetHalfPixel(new Vector2(-1f / texture.Width, 1f / texture.Height));

                if (pointFilter)
                    Resource.CurrentTechnique = Resource.Techniques["BlurPoint"];
                else
                    Resource.CurrentTechnique = Resource.Techniques["BlurLinear"];

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
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
                throw new Exception("Unable to render the blur effect. " + e.Message);
            }
        } // GenerateBlur

		#endregion

	} // Blur
} // XNAFinalEngine.Graphics
