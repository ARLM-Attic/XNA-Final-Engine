
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
	/// <summary>
    /// When a shadow is blured is important to avoid leaks in the edges.
    /// Moreover this shader allows to a more aggressive and configurable blurring with just a little more processing cost.
    /// This shader is baded on NVIDIA example.
    /// Chapter 7 of Game Engine Gems 2 talk about bilateral shaders. 
    /// This chapter introduce also a cool technique that could help to acchive good performance
    /// with complex low frecuency shaders (SSAO, shadows, etc.) running on the Xbox 360.
	/// </summary>
    public class BilateralBlurShader : Shader
	{

		#region Variables
        
        // Singleton reference.
	    private static BilateralBlurShader instance;

        // Shader Parameters.
        private static ShaderParameterFloat   spBlurRadius, spBlurFalloff, spSharpness;
        private static ShaderParameterVector2 spHalfPixel,
                                              spInvTextureResolution;
        private static ShaderParameterTexture spTexture, spDepthTexture;
        
        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a blur shader.
        /// </summary>
	    public static BilateralBlurShader Instance
	    {
	        get
	        {
	            if (instance == null)
                    instance = new BilateralBlurShader();
	            return instance;
	        }
	    } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Bilateral Blur Shader.
        /// </summary>
        private BilateralBlurShader() : base("Filters\\BilateralBlur") { }

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
                spInvTextureResolution = new ShaderParameterVector2("invTextureResolution", this);
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spBlurRadius = new ShaderParameterFloat("blurRadius", this);
                spBlurFalloff = new ShaderParameterFloat("blurFalloff", this);
                spSharpness = new ShaderParameterFloat("sharpness", this);
                spTexture = new ShaderParameterTexture("sceneTexture", this, SamplerState.PointClamp, 5);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
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
	    /// <param name="radius">Blur Radius.</param>
        public void Filter(Texture texture, RenderTarget destionationTexture, Texture depthTexture, float radius = 5.0f, float sharpness = 15)
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

                // Set shader parameters
                spBlurRadius.Value = radius;
                float sigma = (radius + 1) / 2;
                float invSigma2 = 1.0f / (2 * sigma * sigma);
                spBlurFalloff.Value = invSigma2;
                spSharpness.Value = sharpness * sharpness / 500;
                spInvTextureResolution.Value = new Vector2(1f / destionationTexture.Width, 1f / destionationTexture.Height);
                spTexture.Value = texture;
                spDepthTexture.Value = depthTexture;

                spHalfPixel.Value = new Vector2(-0.5f / (destionationTexture.Width / 2), 0.5f / (destionationTexture.Height / 2));

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
                        spTexture.Value = blurTempTexture;
                    }
                    else
                        destionationTexture.DisableRenderTarget();
                }
                // It's not used anymore.
                RenderTarget.Release(blurTempTexture);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Bilateral Blur Shader: Unable to render.", e);
            }
        } // Filter

		#endregion

    } // BilateralBlurShader
} // XNAFinalEngine.Graphics
