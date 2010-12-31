
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Horizon Based Screen Space Ambient Occlusion.
    /// Este algoritmo fue sacado de un proyecto demostrativo de NVIDIA.
    /// El algoritmo esta pensando para DirectX 10 y fue pasado para DirectX 9. El problema fue el uso de las sentencias for
    /// en conjunto con un contador variable, dependiente de una variable global. Esto trae problemas al momento de compilar.
    /// Opte por fijar los parametros con valores fijos, estos son facilmente cambiables en el codigo fuente del shader.
    /// </summary>
    public class SSAOHorizonBased : ScreenShader
    {

        #region Properties

        /// <summary>
        /// The result of the SSAO pass.
        /// </summary>
        public RenderToTexture SSAOTexture { get; private set; }

        #endregion

        #region Shader Parameters

        #region Handles

		/// <summary>
		/// Effect handles
		/// </summary>
        private static EffectParameter  // Textures
                                        epDepthNormalTexture,
                                        epHighPresicionDepthTexture,
                                        // Parameters
                                        epResolution,
                                        epInverseResolution,
                                        epNumberSteps,
                                        epNumberDirections,
                                        epContrast,
                                        epLineAttenuation,
                                        epAspectRatio,
                                        epFocalLenght,
                                        epAngleBias,
                                        epRadius;

		#endregion

        #region Resolution

        /// <summary>
        /// Last used resolution
        /// </summary>
        private static Vector2? lastUsedResolution;
        /// <summary>
        /// Set resolution.
        /// </summary>
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

        /// <summary>
        /// Last used inverse resolution
        /// </summary>
        private static Vector2? lastUsedInverseResolution;
        /// <summary>
        /// Set inverse resolution.
        /// </summary>
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

        /// <summary>
        /// Number of Steps
        /// </summary>
        private float numberSteps = 10.0f;

        /// <summary>
        /// Number of Steps
        /// </summary>
        public float NumberSteps
        {
            get { return numberSteps; }
            set { numberSteps = value; }
        } // NumberSteps

        /// <summary>
        /// Last used number of steps
        /// </summary>
        private static float? lastUsedNumberSteps;
        /// <summary>
        /// Set Number of Steps (greater or equal to 0)
        /// </summary>
        private static void SetNumberSteps(float _numberSteps)
        {
            if (lastUsedNumberSteps != _numberSteps && _numberSteps > 0.0f)
            {
                lastUsedNumberSteps = _numberSteps;
                epNumberSteps.SetValue(_numberSteps);
            }
        } // SetNumberSteps

        #endregion
        
        #region Number Directions

        /// <summary>
        /// Number of Directions
        /// </summary>
        private float numberDirections = 5.0f;

        /// <summary>
        /// Number of Directions
        /// </summary>
        public float NumberDirections
        {
            get { return numberDirections; }
            set { numberDirections = value; }
        } // NumberDirections

        /// <summary>
        /// Last used number of directions
        /// </summary>
        private static float? lastUsedNumberDirections;
        /// <summary>
        /// Set Number of Directions (greater or equal to 0)
        /// </summary>
        private static void SetNumberDirections(float _numberDirections)
        {
            if (lastUsedNumberDirections != _numberDirections && _numberDirections > 0.0f)
            {
                lastUsedNumberDirections = _numberDirections;
                epNumberDirections.SetValue(_numberDirections);
            } // if        
        } // SetNumberDirections

        #endregion

        #region Contrast

        /// <summary>
        /// Contrast
        /// </summary>
        private float contrast = 1.75f;

        /// <summary>
        /// Contrast
        /// </summary>
        public float Contrast
        {
            get { return contrast; }
            set { contrast = value; }
        } // Contrast

        /// <summary>
        /// Last used contrast
        /// </summary>
        private static float? lastUsedContrast;
        /// <summary>
        /// Contrast (value between 0 and 3)
        /// </summary>
        private static void SetContrast(float _contrast)
        {
            if (lastUsedContrast != _contrast && _contrast >= 0.0f && _contrast <= 3.0f)
            {
                lastUsedContrast = _contrast;
                epContrast.SetValue(_contrast);
            }
        } // SetContrast

        #endregion

        #region Line Attenuation

        /// <summary>
        /// Line Attenuation
        /// </summary>
        private float lineAttenuation = 1.5f;

        /// <summary>
        /// Line Attenuation
        /// </summary>
        public float LineAttenuation
        {
            get { return lineAttenuation; }
            set { lineAttenuation = value; }
        } // Line Attenuation

        /// <summary>
        /// Last used Line Attenuation
        /// </summary>
        private static float? lastUsedLineAttenuation;
        /// <summary>
        /// Set Line Attenuation (value between 0 and 2)
        /// </summary>
        private static void SetLineAttenuation(float _lineAttenuation)
        {
            if (lastUsedLineAttenuation != _lineAttenuation && _lineAttenuation >= 0.0f && _lineAttenuation <= 2.0f)
            {
                lastUsedLineAttenuation = _lineAttenuation;
                epLineAttenuation.SetValue(_lineAttenuation);
            } // if
        } // SetLineAttenuation

        #endregion

        #region Aspect Ratio

        /// <summary>
        /// Aspect Ratio
        /// </summary>
        private float aspectRatio = 1.6f;

        /// <summary>
        /// Aspect Ratio
        /// </summary>
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        } // Aspect Ratio

        /// <summary>
        /// Last used Aspect Ratio
        /// </summary>
        private static float? lastUsedAspectRatio;
        /// <summary>
        /// Set Aspect Ratio (value between 1 and 3)
        /// </summary>
        private static void SetAspectRatio(float _aspectRatio)
        {
            if (lastUsedAspectRatio != _aspectRatio && _aspectRatio >= 1.0f && _aspectRatio <= 3.0f)
            {
                lastUsedAspectRatio = _aspectRatio;
                Vector2 focalLenght = new Vector2
                {
                    X = (float) (1.0/Math.Tan(3.1416/6)*_aspectRatio),
                    Y = (float) (1.0/Math.Tan(3.1416/6))
                };
                epFocalLenght.SetValue(focalLenght);
                epAspectRatio.SetValue(_aspectRatio);
            } // if
        } // SetAspectRatio

        #endregion

        #region Radius

        /// <summary>
        /// Radius
        /// </summary>
        private float radius = 0.03f;

        /// <summary>
        /// Radius
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        } // Radius

        /// <summary>
        /// Last used radius
        /// </summary>
        private static float? lastUsedRadius;
        /// <summary>
        /// Set Radius (value between 0 and 2)
        /// </summary>
        private static void SetRadius(float _radius)
        {
            if (lastUsedRadius != _radius && _radius >= 0.0f && _radius <= 2.0f)
            {
                lastUsedRadius = _radius;
                epRadius.SetValue(_radius);
            } // if
        } // SetRadius

        #endregion

        #region AngleBias

        /// <summary>
        /// Angle Bias (grades)
        /// </summary>
        private float angleBias = 30f;

        /// <summary>
        /// Angle Bias (grades)
        /// </summary>
        public float AngleBias
        {
            get { return angleBias; }
            set { angleBias = value; }
        } // AngleBias

        /// <summary>
        /// Last used angle bias
        /// </summary>
        private static float? lastUsedAngleBias;
        /// <summary>
        /// Set Angle Bias (value between 0.1 and 60)
        /// </summary>
        private static void SetAngleBias(float _angleBias)
        {
            if (lastUsedAngleBias != _angleBias && _angleBias > 0.0f && _angleBias <= 60)
            {
                lastUsedAngleBias = _angleBias;
                epAngleBias.SetValue(_angleBias * 3.1416f / 180f);
            } //
        } // SetAngleBias

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Horizon Based Screen Space Ambient Occlusion.
		/// </summary>
        public SSAOHorizonBased(RenderToTexture.SizeType _rendeTargetSize = RenderToTexture.SizeType.HalfScreen)			
		{
            Effect = LoadShader("PostSSAOHorizonBased");
            Effect.CurrentTechnique = Effect.Techniques["SSAO"];

            // The render target texture
            if (SSAOTexture == null)
                SSAOTexture = new RenderToTexture(_rendeTargetSize, false, false);

            GetParametersHandles();

            // Set some parameters automatically
            Texture randomNormalTexture = new Texture("RANDOMNORMAL");
            Effect.Parameters["randomTexture"].SetValue(randomNormalTexture.XnaTexture);
        } // SSAOHorizonBased

		#endregion

		#region Get parameters handles

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParametersHandles()
		{
            try
            {
                epDepthNormalTexture = Effect.Parameters["depthNormalTexture"];
                epHighPresicionDepthTexture = Effect.Parameters["highPresicionDepthTexture"];

                epResolution = Effect.Parameters["g_Resolution"];
                epInverseResolution = Effect.Parameters["g_InvResolution"];
                epNumberSteps = Effect.Parameters["g_NumSteps"];
                epNumberDirections = Effect.Parameters["g_NumDir"];
                epContrast = Effect.Parameters["g_Contrast"];
                epLineAttenuation = Effect.Parameters["g_Attenuation"];
                epAspectRatio = Effect.Parameters["AspectRatio"];
                epFocalLenght = Effect.Parameters["g_FocalLen"];
                epRadius = Effect.Parameters["g_R"];
                epAngleBias = Effect.Parameters["g_AngleBias"];
            }
            catch
            {
                throw new Exception("Get the handles from the SSAO Horizon Based shader failed.");
            }
		} // GetParametersHandles

		#endregion

        #region Set Parameters
        
        /// <summary>
        /// Set to the shader the specific atributes of this effect.
        /// </summary>
        public void SetParameters()
        {
            AspectRatio = EngineManager.AspectRatio;
            SetResolution(new Vector2(EngineManager.Width, EngineManager.Height));
            SetInverseResolution(new Vector2(1 / (float)EngineManager.Width, 1 / (float)EngineManager.Height));
            SetNumberSteps(NumberSteps);
            SetNumberDirections(NumberDirections);
            SetContrast(Contrast);
            SetLineAttenuation(LineAttenuation);
            SetAspectRatio(AspectRatio);
            SetRadius(Radius);
            SetAngleBias(AngleBias);
        } // SetParameters

        #endregion

        #region Generate SSAO
        
        /// <summary>
        /// Generate SSAO texture
		/// </summary>
        public void GenerateSSAO(Texture2D _depthTexture, Texture2D _normalTexture)
        {
            // Set shader atributes
            epDepthNormalTexture.SetValue(_normalTexture);
            epHighPresicionDepthTexture.SetValue(_depthTexture);
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp;
            SetParameters();

            // Start rendering onto the shadow map
            SSAOTexture.EnableRenderTarget();

            // Clear render target
            SSAOTexture.Clear(Color.Black);
                       
            // Start effect (current technique should be set)
            try
            {
                Effect.CurrentTechnique.Passes[0].Apply();

                // Render
                ScreenPlane.Render();

            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the SSAO Horizon Based effect. " + e.Message);
            }

            // Resolve the render target to get the texture (required for Xbox)
            SSAOTexture.DisableRenderTarget();

        } // GenerateSSAO

		#endregion

    } // SSAOHorizonBased
} // XNAFinalEngine.Graphics
