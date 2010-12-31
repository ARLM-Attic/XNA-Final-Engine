
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
    /// Ray Marching Screen Space Ambient Occlusion.
    /// Este algoritmo fue sacado de un proyecto demostrativo de NVIDIA.
    /// El algoritmo esta pensando para DirectX 10 y fue pasado para DirectX 9. El problema fue el uso de las sentencias for
    /// en conjunto con un contador variable, dependiente de una variable global. Esto trae problemas al momento de compilar.
    /// Opte por fijar los paremtros con valores fijos, estos son facilmente cambiables en el codigo fuente del shader.
    /// </summary>
    public class SSAORayMarching : ScreenShader
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
                                        epNumberSteps,
                                        epNumberRays,
                                        epNumberDirections,
                                        epContrast,
                                        epLineAttenuation,
                                        epAspectRatio,
                                        epFarPlane,
                                        epRadius,
                                        epFocalLenght;

        #endregion

        #region Number Steps

        /// <summary>
        /// Number of Steps
        /// </summary>
        private float numberSteps = 4.0f;
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
        
        #region Number Rays
        
        /// <summary>
        /// Number of Rays
        /// </summary>
        private float numberRays = 4.0f;
        /// <summary>
        /// Number of Rays
        /// </summary>
        public float NumberRays
        {
            get { return numberRays; }
            set { numberRays = value; }
        } // NumberRays

        /// <summary>
        /// Last used number of rays
        /// </summary>
        private static float? lastUsedNumberRays;
        /// <summary>
        /// Set Number of Rays (greater or equal to 0)
        /// </summary>
        private static void SetNumberRays(float _numberRays)
        {
            if (lastUsedNumberRays != _numberRays && _numberRays > 0.0f)
            {
                lastUsedNumberRays = _numberRays;
                epNumberRays.SetValue(_numberRays);
            }
        } // SetNumberRays
        
        #endregion

        #region Number Directions
        
        /// <summary>
        /// Number of Directions
        /// </summary>
        private float numberDirections = 6.0f;
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
            }    
        } // SetNumberDirections

        #endregion
        
        #region Contrast
        
        /// <summary>
        /// Contrast
        /// </summary>
        private float contrast = 1;
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
        /// Contrast (value between 0 and 2)
        /// </summary>
        private static void SetContrast(float _contrast)
        {
            if (lastUsedContrast != _contrast && _contrast >= 0.0f && _contrast <= 2.0f)
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
            }
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
                    X = 1.0f/(float) Math.Tan(3.1416/6)*_aspectRatio,
                    Y = 1.0f/(float) Math.Tan(3.1416/6)
                };
                epFocalLenght.SetValue(focalLenght);
                epAspectRatio.SetValue(_aspectRatio);
            }
        } // SetAspectRatio

        #endregion

        #region Far Plane

        /// <summary>
        /// Far Plane
        /// </summary>
        private float farPlane = 100.0f;
        /// <summary>
        /// Far Plane
        /// </summary>
        public float FarPlane
        {
            get { return farPlane; }
            set { farPlane = value; }
        } // FarPlane

        /// <summary>
        /// Last used far plane
        /// </summary>
        private static float? lastUsedFarPlane;
        /// <summary>
        /// Set Far Plane (greater or equal to 1)
        /// </summary>
        private static void SetFarPlane(float _farPlane)
        {
            if (lastUsedFarPlane != _farPlane && _farPlane >= 1.0f)
            {
                lastUsedFarPlane = _farPlane;
                epFarPlane.SetValue(_farPlane);
            }
        } // SetFarPlane

        #endregion

        #region Radius

        /// <summary>
        /// Radius
        /// </summary>
        private float radius = 0.05f;
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
            }
        } // SetRadius

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Ray Marching Screen Space Ambient Occlusion.
		/// </summary>
        public SSAORayMarching(RenderToTexture.SizeType _rendeTargetSize = RenderToTexture.SizeType.HalfScreen)			
		{
            Effect = LoadShader("PostSSAORayMarching");
            Effect.CurrentTechnique = Effect.Techniques["SSAO"];
                                    
            // The render target texture
            if (SSAOTexture == null)
                SSAOTexture = new RenderToTexture(_rendeTargetSize);

            GetParametersHandles();

            // Set some parameters automatically
            FarPlane = ApplicationLogic.Camera.FarPlane;
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
			    // Load screen border texture
                epDepthNormalTexture = Effect.Parameters["depthNormalTexture"];
                epHighPresicionDepthTexture = Effect.Parameters["highPresicionDepthTexture"];

                epNumberSteps = Effect.Parameters["g_NumSteps"];
                epNumberRays  = Effect.Parameters["g_NumRays"];
                epNumberDirections = Effect.Parameters["g_NumDirs"];
                epContrast = Effect.Parameters["g_Contrast"];
                epLineAttenuation = Effect.Parameters["g_LinAtt"];
                epAspectRatio = Effect.Parameters["AspectRatio"];
                epFarPlane = Effect.Parameters["FarPlane"];
                epFocalLenght = Effect.Parameters["g_FocalLen"];
                epRadius = Effect.Parameters["g_R"];
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
        public void SetParameters()
        {
            AspectRatio = EngineManager.AspectRatio;
            SetNumberSteps(NumberSteps);
            SetNumberRays(NumberRays);
            SetNumberDirections(NumberDirections);
            SetContrast(Contrast);
            SetLineAttenuation(LineAttenuation);
            SetAspectRatio(AspectRatio);
            SetFarPlane(FarPlane);
            SetRadius(Radius);
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
            EngineManager.Device.SamplerStates[1] = SamplerState.PointClamp;
            SetParameters();
                        
            // Start rendering onto the render target
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
                throw new Exception("Unable to render the SSAO Ray Marching effect " + e.Message);
            }

            // Resolve the render target to get the texture (required for Xbox)
            SSAOTexture.DisableRenderTarget();
           
        } // GenerateSSAO

		#endregion

    } // SSAORayMarching
} // XNAFinalEngine.Graphics
