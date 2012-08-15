
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Ray Marching Screen Space Ambient Occlusion.
    /// This shader is baded in a Shader X 7 article.
    /// 
    /// Horizon Based is similar in performance but better in results.
    /// However this is good example of how to make raymarching shaders and the results are not bad either.
    /// </summary>
    internal class RayMarchingAmbientOcclusionShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static RayMarchingAmbientOcclusionShader instance;

        private static Texture randomNormalTexture;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static RayMarchingAmbientOcclusionShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new RayMarchingAmbientOcclusionShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        #region Handles

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter  // Textures
                                        epNormalTexture,
                                        epDepthTexture,
                                        // Parameters
                                        epNumberSteps,
                                        epNumberRays,
                                        epNumberDirections,
                                        epContrast,
                                        epLineAttenuation,
                                        epRadius,
                                        // Others
                                        epHalfPixel,
                                        epFocalLength;

        #endregion

        #region Number Steps

        private static float lastUsedNumberSteps;
        private static void SetNumberSteps(float _numberSteps)
        {
            if (lastUsedNumberSteps != _numberSteps)
            {
                lastUsedNumberSteps = _numberSteps;
                epNumberSteps.SetValue(_numberSteps);
            }
        } // SetNumberSteps

        #endregion
        
        #region Number Rays

        private static float lastUsedNumberRays;
        private static void SetNumberRays(float _numberRays)
        {
            if (lastUsedNumberRays != _numberRays)
            {
                lastUsedNumberRays = _numberRays;
                epNumberRays.SetValue(_numberRays);
            }
        } // SetNumberRays
        
        #endregion

        #region Number Directions
        
        private static float lastUsedNumberDirections;
        private static void SetNumberDirections(float _numberDirections)
        {
            if (lastUsedNumberDirections != _numberDirections)
            {
                lastUsedNumberDirections = _numberDirections;
                epNumberDirections.SetValue(_numberDirections);
            }    
        } // SetNumberDirections

        #endregion
        
        #region Contrast

        private static float lastUsedContrast;
        private static void SetContrast(float _contrast)
        {
            if (lastUsedContrast != _contrast)
            {
                lastUsedContrast = _contrast;
                epContrast.SetValue(_contrast);
            }
        } // SetContrast

        #endregion

        #region Line Attenuation
        
        private static float lastUsedLineAttenuation;
        private static void SetLineAttenuation(float _lineAttenuation)
        {
            if (lastUsedLineAttenuation != _lineAttenuation)
            {
                lastUsedLineAttenuation = _lineAttenuation;
                epLineAttenuation.SetValue(_lineAttenuation);
            }
        } // SetLineAttenuation

        #endregion
        
        #region Radius

        private static float lastUsedRadius;
        private static void SetRadius(float _radius)
        {
            if (lastUsedRadius != _radius)
            {
                lastUsedRadius = _radius;
                epRadius.SetValue(_radius);
            }
        } // SetRadius

        #endregion

        #region Half Pixel

        private static Vector2 lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region Focal Length

        private static Vector2 lastUsedFocalLength;
        private static void SetFocalLength(Vector2 focalLength)
        {
            if (lastUsedFocalLength != focalLength)
            {
                lastUsedFocalLength = focalLength;
                epFocalLength.SetValue(focalLength);
            }
        } // SetFocalLength

        #endregion

        #region Depth Texture

        private static Texture2D lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDepthTexture != depthTexture.Resource)
            {
                lastUsedDepthTexture = depthTexture.Resource;
                epDepthTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthTexture

        #endregion

        #region Normal Texture

        private static Texture2D lastUsedNormalTexture;
        private static void SetNormalTexture(Texture normalTexture)
        {
            EngineManager.Device.SamplerStates[1] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedNormalTexture != normalTexture.Resource)
            {
                lastUsedNormalTexture = normalTexture.Resource;
                epNormalTexture.SetValue(normalTexture.Resource);
            }
        } // SetNormalTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Ray Marching Screen Space Ambient Occlusion.
		/// </summary>
        private RayMarchingAmbientOcclusionShader() : base("GlobalIllumination\\RayMarchingAmbientOcclusionShader")
		{
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            // Set the random normal map. Helps to make the samplers more random.
            randomNormalTexture = new Texture("Shaders\\RandomNormal");
            Resource.Parameters["randomTexture"].SetValue(randomNormalTexture.Resource);
            ContentManager.CurrentContentManager = userContentManager;
        } // RayMarchingAmbientOcclusionShader

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
                epDepthTexture = Resource.Parameters["depthTexture"];
                if (lastUsedDepthTexture != null && !lastUsedDepthTexture.IsDisposed)
                    epDepthTexture.SetValue(lastUsedDepthTexture);
                epNormalTexture = Resource.Parameters["normalTexture"];
                if (lastUsedNormalTexture != null && !lastUsedNormalTexture.IsDisposed)
                    epNormalTexture.SetValue(lastUsedNormalTexture);
                epNumberSteps      = Resource.Parameters["numberSteps"];
                    epNumberSteps.SetValue(lastUsedNumberSteps);
                epNumberRays       = Resource.Parameters["numberRays"];
                    epNumberRays.SetValue(lastUsedNumberRays);
                epNumberDirections = Resource.Parameters["numberDirections"];
                    epNumberDirections.SetValue(lastUsedNumberDirections);
                epContrast         = Resource.Parameters["contrast"];
                    epContrast.SetValue(lastUsedContrast);
                epLineAttenuation  = Resource.Parameters["linearAttenuation"];
                    epLineAttenuation.SetValue(lastUsedLineAttenuation);
                epRadius           = Resource.Parameters["radius"];
                    epRadius.SetValue(lastUsedRadius);
                epHalfPixel        = Resource.Parameters["halfPixel"];
                    epHalfPixel.SetValue(lastUsedHalfPixel);
                epFocalLength      = Resource.Parameters["focalLength"];
                    epFocalLength.SetValue(lastUsedFocalLength);

                if (randomNormalTexture != null)
                    Resource.Parameters["randomTexture"].SetValue(randomNormalTexture.Resource);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

		#endregion

        #region Render

        /// <summary>
        /// Generate ambient occlusion texture.
		/// </summary>
        public RenderTarget Render(RenderTarget depthTexture, RenderTarget normalTexture, RayMarchingAmbientOcclusion rmao, float fieldOfView)
        {
            try
            {
                // I decided to work with Color format for a number of reasons.
                // First, this format is very used so chances are that I can reuse it latter in another shader.
                // Second, GPUs tend to work faster in non-floating point render targets. 
                // Third, I can blur it with linear sampling.
                // The main disadvantage is that I am wasting three channels. 
                // A single 8 bit channel render target is not available in XNA 4.0 and I have two options for 16 bits render targets.
                // First, the compressed 4 channels formats, the compression is visible and the results are not satisfactory.
                // Last we have the half single format, it is a good option but I prefer to have linear sampling.
                RenderTarget ambientOcclusionTexture = RenderTarget.Fetch(depthTexture.Size, SurfaceFormat.HalfSingle,
                                                                          DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set shader atributes
                SetNormalTexture(normalTexture);
                SetDepthTexture(depthTexture);
                SetNumberSteps(rmao.NumberSteps);
                SetNumberRays(rmao.NumberRays);
                SetNumberDirections(rmao.NumberDirections);
                SetContrast(rmao.Contrast);
                SetLineAttenuation(rmao.LineAttenuation);
                SetRadius(rmao.Radius);
                SetHalfPixel(new Vector2(-0.5f / (ambientOcclusionTexture.Width / 2), 0.5f / (ambientOcclusionTexture.Height / 2)));
                Vector2 focalLen = new Vector2
                {
                    X =  1.0f / (float)Math.Tan(fieldOfView * (3.1416f / 180) * 0.5f) * (float)ambientOcclusionTexture.Height / (float)ambientOcclusionTexture.Width,
                    Y =  1.0f / (float)Math.Tan(fieldOfView * (3.1416f / 180) * 0.5f)
                };
                SetFocalLength(focalLen);
                
                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.SamplerStates[3] = SamplerState.PointWrap;

                Resource.CurrentTechnique = Resource.Techniques["SSAO"];
                Resource.CurrentTechnique.Passes[0].Apply();

                // Render
                ambientOcclusionTexture.EnableRenderTarget();
                ambientOcclusionTexture.Clear(Color.White);
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                ambientOcclusionTexture.DisableRenderTarget();

                RenderTarget bluredAmbientOcclusionTexture = RenderTarget.Fetch(depthTexture.Size, SurfaceFormat.HalfSingle,
                                                                                DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                BilateralBlurShader.Instance.Filter(ambientOcclusionTexture, bluredAmbientOcclusionTexture, depthTexture);
                RenderTarget.Release(ambientOcclusionTexture);
                return bluredAmbientOcclusionTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Ray Marching Ambient Occlusion Shader: Unable to render.", e);
            }
        } // Render

		#endregion

    } // RayMarchingAmbientOcclusionShader
} // XNAFinalEngine.Graphics
