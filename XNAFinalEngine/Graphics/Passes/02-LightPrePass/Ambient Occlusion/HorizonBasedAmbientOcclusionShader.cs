
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
    /// Horizon Based Screen Space Ambient Occlusion.
    /// Important: I have to put constants in the shader's for.
    /// If you want to change the number of steps and the number of directions will need to change the properties and the shader code.
    /// 
    /// This shader is baded in a Shader X 7 article with some minor modifications.
    /// </summary>
    internal class HorizonBasedAmbientOcclusionShader : Shader
    {
        
        #region Variables

        // Singleton reference.
        private static HorizonBasedAmbientOcclusionShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static HorizonBasedAmbientOcclusionShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new HorizonBasedAmbientOcclusionShader();
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
                                        epResolution,
                                        epInverseResolution,
                                        epNumberSteps,
                                        epNumberDirections,
                                        epContrast,
                                        epLineAttenuation,
                                        epAngleBias,
                                        epRadius,
                                        // Others
                                        epFocalLength,
                                        epInvFocalLength,
                                        epHalfPixel,
                                        epSqrRadius,
                                        epInvRadius,
                                        epTanAngleBias;

		#endregion

        #region Resolution

        private static Vector2? lastUsedResolution;
        private static void SetResolution(Vector2 _resolution)
        {
            if (lastUsedResolution != _resolution)
            {
                lastUsedResolution = _resolution;
                epResolution.SetValue(_resolution);
            }
        } // SetResolution

        #endregion

        #region Inverse Resolution

        private static Vector2? lastUsedInverseResolution;
        private static void SetInverseResolution(Vector2 _inverseResolution)
        {
            if (lastUsedInverseResolution != _inverseResolution)
            {
                lastUsedInverseResolution = _inverseResolution;
                epInverseResolution.SetValue(_inverseResolution);
            }
        } // SetInverseResolution

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
        
        #region Number Directions

        private static float? lastUsedNumberDirections;
        private static void SetNumberDirections(float _numberDirections)
        {
            if (lastUsedNumberDirections != _numberDirections)
            {
                lastUsedNumberDirections = _numberDirections;
                epNumberDirections.SetValue(_numberDirections);
            } // if        
        } // SetNumberDirections

        #endregion

        #region Contrast

        private static float? lastUsedContrast;
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
            } // if
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

        #region AngleBias
        
        private static float? lastUsedAngleBias;
        private static void SetAngleBias(float _angleBias)
        {
            if (lastUsedAngleBias != _angleBias)
            {
                lastUsedAngleBias = _angleBias;
                epAngleBias.SetValue(_angleBias);
            } //
        } // SetAngleBias

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

        #region Inverse Focal Length

        private static Vector2? lastUsedInverseFocalLength;
        private static void SetInverseFocalLength(Vector2 inverseFocalLength)
        {
            if (lastUsedInverseFocalLength != inverseFocalLength)
            {
                lastUsedInverseFocalLength = inverseFocalLength;
                epInvFocalLength.SetValue(inverseFocalLength);
            }
        } // SetInverseFocalLength

        #endregion

        #region Square Radius

        private static float? lastUsedSquareRadius;
        private static void SetSquareRadius(float squareRadius)
        {
            if (lastUsedSquareRadius != squareRadius)
            {
                lastUsedSquareRadius = squareRadius;
                epSqrRadius.SetValue(squareRadius);
            } // if
        } // SetSquareRadius

        #endregion

        #region Inverse Radius

        private static float? lastUsedInverseRadius;
        private static void SetInverseRadius(float inverseRadius)
        {
            if (lastUsedInverseRadius != inverseRadius)
            {
                lastUsedInverseRadius = inverseRadius;
                epInvRadius.SetValue(inverseRadius);
            }
        } // SetInverseRadius

        #endregion

        #region Tangent Angle Bias

        private static float? lastUsedTanAngleBias;
        private static void SetTanAngleBias(float tanAngleBias)
        {
            if (lastUsedTanAngleBias != tanAngleBias)
            {
                lastUsedTanAngleBias = tanAngleBias;
                epTanAngleBias.SetValue(tanAngleBias);
            }
        } // SetTanAngleBias

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
        /// Horizon Based Screen Space Ambient Occlusion.
        /// Important: I have to put constants in the shader's for.
        /// If you want to change the number of steps and the number of directions will need to change the properties and the shader code.
		/// </summary>
        public HorizonBasedAmbientOcclusionShader() : base("GlobalIllumination\\HorizonBasedAmbientOcclusion")
		{
            // Set the random normal map. Helps to make the samplers more random.
            Texture randomNormalTexture = new Texture("Shaders\\RandomNormal");
            Resource.Parameters["randomTexture"].SetValue(randomNormalTexture.Resource);
        } // HorizonBasedAmbientOcclusionShader

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
                epNormalTexture     = Resource.Parameters["normalTexture"];
                epDepthTexture      = Resource.Parameters["depthTexture"];
                epResolution        = Resource.Parameters["resolution"];
                epInverseResolution = Resource.Parameters["invResolution"];
                epNumberSteps       = Resource.Parameters["numberSteps"];
                epNumberDirections  = Resource.Parameters["numberDirections"];
                epContrast          = Resource.Parameters["contrast"];
                epLineAttenuation   = Resource.Parameters["attenuation"];
                epRadius            = Resource.Parameters["radius"];
                epAngleBias         = Resource.Parameters["angleBias"];
                // Others
                epFocalLength       = Resource.Parameters["focalLength"];
                epInvFocalLength    = Resource.Parameters["invFocalLength"];
                epHalfPixel         = Resource.Parameters["halfPixel"];
                epSqrRadius         = Resource.Parameters["sqrRadius"];
                epInvRadius         = Resource.Parameters["invRadius"];
                epTanAngleBias      = Resource.Parameters["tanAngleBias"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

		#endregion

        #region Generate Ambient Occlusion

        /// <summary>
        /// Generate ambient occlusion texture.
		/// </summary>
        public RenderTarget Render(RenderTarget depthTexture, RenderTarget normalTexture, HorizonBasedAmbientOcclusion hbao, float fieldOfView, Matrix viewMatrix)
        {
            // Alpha8 doesn't work in my G92 GPU processor and I opt to work with half single. Color is another good choice because support texture filtering.
            // XBOX 360 Xbox does not support 16 bit render targets (http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/rendertarget-formats-in-xna-game-studio-4-0.aspx)
            // Color would be the better choice for the XBOX 360.
            // With color we have another good option, the possibility to gather four shadow results (local or global) in one texture.
            RenderTarget ambientOcclusionTexture = RenderTarget.Fetch(depthTexture.Size, SurfaceFormat.HalfSingle, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
            try
            {
                // Set shader atributes
                SetNormalTexture(normalTexture);
                SetDepthTexture(depthTexture);
                // It works a the depth texture resolution. Use a downsampled version of the G-Buffer.
                SetResolution(new Vector2(depthTexture.Width, depthTexture.Height));
                SetInverseResolution(new Vector2(1 / (float)depthTexture.Width, 1 / (float)depthTexture.Height));
                SetNumberSteps(hbao.NumberSteps);
                SetNumberDirections(hbao.NumberDirections);
                SetContrast(hbao.Contrast / (1.0f - (float)Math.Sin(hbao.AngleBias)));
                SetLineAttenuation(hbao.LineAttenuation);
                SetRadius(hbao.Radius);
                SetAngleBias(hbao.AngleBias);
                SetHalfPixel(new Vector2(-1f / ambientOcclusionTexture.Width, 1f / ambientOcclusionTexture.Height));
                Vector2 focalLen = new Vector2
                {
                    X = 1.0f / ((float)Math.Tan(fieldOfView * (3.1416f / 180) * 0.5f) * ((float)ambientOcclusionTexture.Height / (float)ambientOcclusionTexture.Width)),
                    Y = 1.0f / ((float)Math.Tan(fieldOfView * (3.1416f / 180) * 0.5f))
                };
                SetFocalLength(focalLen);
                SetInverseFocalLength(new Vector2(1 / focalLen.X, 1 / focalLen.Y));
                SetSquareRadius(hbao.AngleBias * hbao.AngleBias);
                SetInverseRadius(1 / hbao.AngleBias);
                SetTanAngleBias((float)Math.Tan(hbao.AngleBias));

                switch (hbao.Quality)
                {
                    case HorizonBasedAmbientOcclusion.QualityType.LowQuality    : Resource.CurrentTechnique = Resource.Techniques["LowQuality"]; break;
                    case HorizonBasedAmbientOcclusion.QualityType.MiddleQuality : Resource.CurrentTechnique = Resource.Techniques["MiddleQuality"]; break;
                    case HorizonBasedAmbientOcclusion.QualityType.HighQuality   : Resource.CurrentTechnique = Resource.Techniques["HighQuality"]; break;
                }

                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.SamplerStates[3] = SamplerState.PointWrap;

                Resource.Parameters["viewI"].SetValue(Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix))));

                // Render
                ambientOcclusionTexture.EnableRenderTarget();
                ambientOcclusionTexture.Clear(Color.White);
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                ambientOcclusionTexture.DisableRenderTarget();

                //BlurShader.Instance.Filter(ambientOcclusionTexture, true, 2);
                return ambientOcclusionTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Horizon Based Ambient Occlusion Shader: Unable to render.", e);
            }
        } // GenerateAmbientOcclusion

		#endregion

    } // HorizonBasedAmbientOcclusionShader
} // XNAFinalEngine.Graphics
