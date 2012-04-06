
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

        private static float? lastUsedNumberSteps;
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

        private static float? lastUsedNumberRays;
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
        
        private static float? lastUsedNumberDirections;
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

        /// <summary>
        /// Last used contrast
        /// </summary>
        private static float? lastUsedContrast;
        /// <summary>
        /// Contrast
        /// </summary>
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
        
        private static float? lastUsedLineAttenuation;
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

        private static float? lastUsedRadius;
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

        private static Vector2? lastUsedHalfPixel;
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

        private static Vector2? lastUsedFocalLength;
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
            Texture randomNormalTexture = new Texture("Shaders\\RandomNormal");
            ContentManager.CurrentContentManager = userContentManager;

            Resource.Parameters["randomTexture"].SetValue(randomNormalTexture.Resource);
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
                epNormalTexture    = Resource.Parameters["normalTexture"];
                epDepthTexture     = Resource.Parameters["depthTexture"];
                epNumberSteps      = Resource.Parameters["numberSteps"];
                epNumberRays       = Resource.Parameters["numberRays"];
                epNumberDirections = Resource.Parameters["numberDirections"];
                epContrast         = Resource.Parameters["contrast"];
                epLineAttenuation  = Resource.Parameters["linearAttenuation"];
                epRadius           = Resource.Parameters["radius"];
                epHalfPixel        = Resource.Parameters["halfPixel"];
                epFocalLength      = Resource.Parameters["focalLength"];
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
                // Alpha8 doesn't work in my G92 GPU processor and I opt to work with half single. Color is another good choice because support texture filtering.
                // XBOX 360 Xbox does not support 16 bit render targets (http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/rendertarget-formats-in-xna-game-studio-4-0.aspx)
                // Color would be the better choice for the XBOX 360.
                // With color we have another good option, the possibility to gather four shadow results (local or global) in one texture.
                RenderTarget ambientOcclusionTexture = RenderTarget.Fetch(depthTexture.Size, SurfaceFormat.HalfSingle, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set shader atributes
                SetNormalTexture(normalTexture);
                SetDepthTexture(depthTexture);
                SetNumberSteps(rmao.NumberSteps);
                SetNumberRays(rmao.NumberRays);
                SetNumberDirections(rmao.NumberDirections);
                SetContrast(rmao.Contrast);
                SetLineAttenuation(rmao.LineAttenuation);
                SetRadius(rmao.Radius);
                SetHalfPixel(new Vector2(-1f / ambientOcclusionTexture.Width, 1f / ambientOcclusionTexture.Height));
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

                BlurShader.Instance.Filter(ambientOcclusionTexture, true, 2);
                return ambientOcclusionTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Ray Marching Ambient Occlusion Shader: Unable to render.", e);
            }
        } // Render

		#endregion

    } // RayMarchingAmbientOcclusionShader
} // XNAFinalEngine.Graphics
