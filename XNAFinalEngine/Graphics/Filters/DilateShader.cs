
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
	/// <summary>
    /// Dilate a single channel texture.
	/// </summary>
    public class DilateShader : Shader
	{

        #region Variables

        // Singleton reference.
        private static DilateShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a dilate shader.
        /// </summary>
        public static DilateShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new DilateShader();
                return instance;
            }
        } // Instance

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

        private static float? lastUsedDilateWidth;
        private static void SetDilateWidth(float _dilateWidth)
        {
            if (lastUsedDilateWidth != _dilateWidth || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedDilateWidth = _dilateWidth;
                epDilateWidth.SetValue(_dilateWidth);
            }
        } // SetDilateWidth

		#endregion

        #region Half Pixel

        private static Vector2? lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 halfPixel)
        {
            if (lastUsedHalfPixel != halfPixel || EngineManager.DeviceDisposedThisFrame)
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
            if (lastUsedTexture != texture.Resource || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedTexture = texture.Resource;
                epTexture.SetValue(texture.Resource);
            }
        } // SetTexture

        #endregion

        #region Texture Resolution

        private static Vector2? lastUsedTextureResolution;
        private static void SetTextureResolution(Vector2 textureResolution)
        {
            if (lastUsedTextureResolution != textureResolution || EngineManager.DeviceDisposedThisFrame)
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
        private DilateShader() : base("Filters\\Dilate") { }

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
                epDilateWidth       = Resource.Parameters["dilateWidth"];
                epTexture           = Resource.Parameters["sceneMap"];
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
		/// Dilates a texture.
		/// </summary>
        /// <param name="texture">The texture to dilate. The result will be placed here.</param>
        /// <param name="width">Dilate Width.</param>
        public void Filter(RenderTarget texture, float width = 1.0f)
		{   
            // Only apply if the texture is valid.
			if (texture == null || texture.Resource == null)
                throw new ArgumentNullException("texture");
            if (texture.SurfaceFormat != SurfaceFormat.HalfSingle && texture.SurfaceFormat != SurfaceFormat.Single)
                throw new ArgumentException("Dilate Shader: Invalid texture surface format. This shader only works with single channel textures", "texture");
            try
            {
                // Auxiliary render target.
                RenderTarget dilatedTempTexture = RenderTarget.Fetch(texture.Size, SurfaceFormat.HalfSingle, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set render states.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.SamplerStates[7] = SamplerState.PointClamp;

                // Set parameters.
                SetDilateWidth(width);
                SetTextureResolution(new Vector2(texture.Width, texture.Height));
                SetTexture(texture);
                SetHalfPixel(new Vector2(-1f / texture.Width, 1f / texture.Height));

                foreach (EffectPass pass in Resource.CurrentTechnique.Passes)
                {
                    if (pass.Name == "DilateHorizontal")
                        dilatedTempTexture.EnableRenderTarget();
                    else
                        texture.EnableRenderTarget();

                    pass.Apply();
                    RenderScreenPlane();

                    if (pass.Name == "DilateHorizontal")
                    {
                        dilatedTempTexture.DisableRenderTarget();
                        SetTexture(dilatedTempTexture);
                    }
                    else
                        texture.DisableRenderTarget();
                }
                // It's not used anymore.
                RenderTarget.Release(dilatedTempTexture);
            }
            catch (Exception e)
            {
                throw new Exception("Dilate Shader: Unable to render.", e);
            }
        } // Filter

		#endregion

    } // DilateShader
} // XNAFinalEngine.Graphics
