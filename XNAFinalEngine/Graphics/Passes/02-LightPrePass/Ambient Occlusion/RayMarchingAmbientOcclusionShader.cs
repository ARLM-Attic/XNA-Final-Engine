
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
    public class SSAORayMarching : AmbientOcclusion
    {

        #region Variables

        /// <summary>
        /// Number of Steps.
        /// It’s a sensitive performance parameter.
        /// </summary>
        private float numberSteps = 4.0f;

        /// <summary>
        /// Number of Rays.
        /// It’s a sensitive performance parameter.
        /// </summary>
        private float numberRays = 4.0f;

        /// <summary>
        /// Number of Directions.
        /// It’s a sensitive performance parameter.
        /// </summary>
        private float numberDirections = 6.0f;

        /// <summary>
        /// Contrast.
        /// </summary>
        private float contrast = 1;

        /// <summary>
        /// Line Attenuation.
        /// The far samplers have a lower effect in the result. This controls how faster their weight decay.
        /// </summary>
        private float lineAttenuation = 1f;

        /// <summary>
        /// Radius.
        /// Bigger the radius more cache misses will occur. Be careful!!
        /// </summary>
        private float radius = 0.01f;

        #endregion

        #region Properties

        /// <summary>
        /// Number of Steps.
        /// It’s a sensitive performance parameter.
        /// </summary>
        public float NumberSteps
        {
            get { return numberSteps; }
            set
            {
                numberSteps = value;
                if (numberSteps <= 0)
                    numberSteps = 0;
            }
        } // NumberSteps

        /// <summary>
        /// Number of Rays.
        /// It’s a sensitive performance parameter.
        /// </summary>
        public float NumberRays
        {
            get { return numberRays; }
            set
            {
                numberRays = value;
                if (numberRays <= 0)
                    numberRays = 0;
            }
        } // NumberRays

        /// <summary>
        /// Number of Directions.
        /// It’s a sensitive performance parameter.
        /// </summary>
        public float NumberDirections
        {
            get { return numberDirections; }
            set
            {
                numberDirections = value;
                if (numberDirections <= 0)
                    numberDirections = 0;
            }
        } // NumberDirections

        /// <summary>
        /// Contrast.
        /// </summary>
        public float Contrast
        {
            get { return contrast; }
            set
            {
                contrast = value;
                if (contrast <= 0)
                    contrast = 0;
            }
        } // Contrast

        /// <summary>
        /// Line Attenuation.
        /// The far samplers have a lower effect in the result. This controls how faster their weight decay.
        /// </summary>
        public float LineAttenuation
        {
            get { return lineAttenuation; }
            set
            {
                lineAttenuation = value;
                if (lineAttenuation <= 0)
                    lineAttenuation = 0;
            }
        } // Line Attenuation

        /// <summary>
        /// Radius.
        /// Bigger the radius more cache misses will occur. Be careful!!
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                if (radius <= 0)
                    radius = 0;
                if (radius > 1)
                    radius = 1;
            }
        } // Radius

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

        #region Textures

        private static Texture lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedDepthTexture != depthTexture)
            {
                lastUsedDepthTexture = depthTexture;
                epDepthTexture.SetValue(depthTexture.XnaTexture);
            }
        } // SetDepthTexture

        private static Texture lastUsedNormalTexture;
        private static void SetNormalTexture(Texture normalTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedNormalTexture != normalTexture)
            {
                lastUsedNormalTexture = normalTexture;
                epNormalTexture.SetValue(normalTexture.XnaTexture);
            }
        } // SetNormalTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Ray Marching Screen Space Ambient Occlusion.
		/// </summary>
        public SSAORayMarching(RenderTarget.SizeType _rendeTargetSize = RenderTarget.SizeType.HalfScreen)			
		{
            Effect = LoadShader("GlobalIllumination\\SSAORayMarching");

            // Alpha8 doesn't work in my G92 GPU processor and I opt to work with half single. Color is another good choice because support texture filtering.
            // XBOX 360 Xbox does not support 16 bit render targets (http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/rendertarget-formats-in-xna-game-studio-4-0.aspx)
            // Color would be the better choice for the XBOX 360.
            // With color we have another good option, the possibility to gather four shadow results (local or global) in one texture.
            AmbientOcclusionTexture = new RenderTarget(_rendeTargetSize, SurfaceFormat.HalfSingle, DepthFormat.None, 0);

            GetParametersHandles();

            // Set the random normal map. Helps to make the samplers more random.
            Texture randomNormalTexture = new Texture("Shaders\\RandomNormal");
            Effect.Parameters["randomTexture"].SetValue(randomNormalTexture.XnaTexture);
        } // SSAORayMarching

		#endregion

		#region Get parameters handles

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParametersHandles()
		{	
            try
			{
                epNormalTexture = Effect.Parameters["normalTexture"];
                epDepthTexture = Effect.Parameters["depthTexture"];
                epNumberSteps = Effect.Parameters["numberSteps"];
                epNumberRays  = Effect.Parameters["numberRays"];
                epNumberDirections = Effect.Parameters["numberDirections"];
                epContrast = Effect.Parameters["contrast"];
                epLineAttenuation = Effect.Parameters["linearAttenuation"];
                epRadius = Effect.Parameters["radius"];
                epHalfPixel = Effect.Parameters["halfPixel"];
                epFocalLength = Effect.Parameters["focalLength"];
            }
            catch
            {
                throw new Exception("Get the handles from the SSAO Ray Marching shader failed.");
            }
		} // GetParametersHandles

		#endregion

        #region Set parameters

        /// <summary>
        /// Set to the shader the specific atributes of this effect.
        /// </summary>
        private void SetParameters()
        {
            SetNumberSteps(NumberSteps);
            SetNumberRays(NumberRays);
            SetNumberDirections(NumberDirections);
            SetContrast(Contrast);
            SetLineAttenuation(LineAttenuation);
            SetRadius(Radius);
            SetHalfPixel(new Vector2(-1f / AmbientOcclusionTexture.Width, 1f / AmbientOcclusionTexture.Height));
            Vector2 focalLen = new Vector2
            {
                X = 1.0f / (float)Math.Tan(ApplicationLogic.Camera.FieldOfView * 0.5f) * (float)AmbientOcclusionTexture.Height / (float)AmbientOcclusionTexture.Width,
                Y = 1.0f / (float)Math.Tan(ApplicationLogic.Camera.FieldOfView * 0.5f)
            };
            SetFocalLength(focalLen);
        } // SetParameters

        #endregion

        #region Generate Ambient Occlusion

        /// <summary>
        /// Generate ambient occlusion texture.
		/// </summary>
        public override void GenerateAmbientOcclusion(RenderTarget depthTexture, RenderTarget normalTexture)
        {
            try
            {
                // Set shader atributes
                SetNormalTexture(normalTexture);
                SetDepthTexture(depthTexture);
                SetParameters();

                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;

                // Start rendering onto the render target
                AmbientOcclusionTexture.EnableRenderTarget();

                Effect.CurrentTechnique = Effect.Techniques["SSAO"];
                Effect.CurrentTechnique.Passes[0].Apply();

                // Render
                ScreenPlane.Render();

                // Resolve the render target to get the texture (required for Xbox)
                AmbientOcclusionTexture.DisableRenderTarget();

                EngineManager.SetDefaultRenderStates();
            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the SSAO Ray Marching effect " + e.Message);
            }
        } // GenerateAmbientOcclusion

		#endregion

    } // SSAORayMarching
} // XNAFinalEngine.Graphics
