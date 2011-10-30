
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
using Texture = Microsoft.Xna.Framework.Graphics.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
	/// <summary>
	/// Dilate a texture.
	/// </summary>
    public class Dilate : Shader
	{

		#region Variables

		/// <summary>
		/// Auxiliary render target.
		/// </summary>
        private readonly RenderTarget dilatedTempTexture;

        /// <summary>
        /// Dilate Width
        /// </summary>
        private float width = 1.0f;
        
        #endregion

        #region Properties

        /// <summary>
        /// Dilate Width
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
                                       epDilateWidth,
                                       epTexture,
                                       epHalfPixel;

        #region Dilate Width

        /// <summary>
        /// Last used dilate width
        /// </summary>
        private static float? lastUsedDilateWidth;
        /// <summary>
        /// Set Blur Width (between 0 and 10)
        /// </summary>
        private static void SetDilateWidth(float _dilateWidth)
        {
            if (lastUsedDilateWidth != _dilateWidth && _dilateWidth >= 0.0f && _dilateWidth <= 10.0f)
            {
                lastUsedDilateWidth = _dilateWidth;
                epDilateWidth.SetValue(_dilateWidth);
            }
        } // SetDilateWidth

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
        /// Dilate a single channel texture.
		/// </summary>
        public Dilate(RenderTarget.SizeType _rendeTargetSize = RenderTarget.SizeType.FullScreen)
		{
            Effect = LoadShader("Filters\\Dilate");
            Effect.CurrentTechnique = Effect.Techniques["Dilate"];

            GetParametersHandles();
            
            dilatedTempTexture = new RenderTarget(_rendeTargetSize, SurfaceFormat.HalfSingle, false, 0);

        } // Dilate

		#endregion

		#region Get parameters handles

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParametersHandles()
		{
            try
            {
			    epTextureResolution = Effect.Parameters["textureResolution"];
                epDilateWidth = Effect.Parameters["dilateWidth"];
                epTexture = Effect.Parameters["sceneMap"];
                epHalfPixel = Effect.Parameters["halfPixel"];
            }
            catch
            {
                throw new Exception("Get the handles from the dilate shader failed.");
            }
		} // GetParametersHandles

		#endregion

        #region Generate Dilate
        
        /// <summary>
		/// Generate the dilate effect.
		/// </summary>
		public void GenerateDilate(RenderTarget texture)
		{   
            // Only apply if the texture is valid
			if (texture == null || texture.XnaTexture == null)
				return;

            try
            {
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;

                SetDilateWidth(width);
                SetTextureResolution(new Vector2(texture.Width, texture.Height));
                SetTexture(texture);
                SetHalfPixel(new Vector2(-1f / texture.Width, 1f / texture.Height));

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    if (pass.Name == "DilateHorizontal")
                    {
                        dilatedTempTexture.EnableRenderTarget();
                    }
                    else
                    {
                        texture.EnableRenderTarget();
                    }

                    pass.Apply();
                    ScreenPlane.Render();

                    if (pass.Name == "DilateHorizontal")
                    {
                        dilatedTempTexture.DisableRenderTarget();
                        SetTexture(dilatedTempTexture);
                    }
                    else
                    {
                        texture.DisableRenderTarget();
                    }
                }
                EngineManager.SetDefaultRenderStates();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to render the dilate effect. " + e.Message);
            }
        } // GenerateDilate

		#endregion

	} // Dilate
} // XNAFinalEngine.Graphics
