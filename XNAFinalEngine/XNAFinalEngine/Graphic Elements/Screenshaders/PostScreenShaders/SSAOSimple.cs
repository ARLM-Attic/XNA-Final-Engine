
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
using System.Collections;
using System.Text;
using System.IO;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.EngineCore;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Simple Screen Space Ambient Occlusion.
    /// </summary>
    public class SSAOSimple : ScreenShader
    {

        #region Variables

        /// <summary>
        /// Auxiliary texture.
        /// </summary>
        private Texture randomNormalTexture = null;

		#endregion

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
        private EffectParameter epDepthTexture,
                                epRandomTexture,
                                epSampleRadius,
                                epDistanceScale,
                                epProjection,
                                epCornerFustrum;

        #endregion

        #region Sample Radius

        /// <summary>
        /// Sample Radius
        /// </summary>
        private float sampleRadius = 1.0f;

        /// <summary>
        /// Sample Radius
        /// </summary>
        public float SampleRadius
        {
            get { return sampleRadius; }
            set { sampleRadius = value; }
        } // SampleRadius

        /// <summary>
        /// Last used sample radius
        /// </summary>
        private static float? lastUsedSampleRadius = null;
        /// <summary>
        /// Set sample radius (value between -20 and 20)
        /// </summary>
        private void SetSampleRadius(float _sampleRadius)
        {
            if (lastUsedSampleRadius != _sampleRadius && _sampleRadius >= -20.0f && _sampleRadius <= 20.0f)
            {
                lastUsedSampleRadius = _sampleRadius;
                epSampleRadius.SetValue(_sampleRadius);
            } // if
        } // SetSampleRadius

        #endregion

        #region Distance Scale

        /// <summary>
        /// Distance Scale
        /// </summary>
        private float distanceScale = 300.0f;

        /// <summary>
        /// Distance Scale
        /// </summary>
        public float DistanceScale
        {
            get { return distanceScale; }
            set { distanceScale = value; }
        } // DistanceScale

        /// <summary>
        /// Last used distance scale
        /// </summary>
        private static float? lastUsedDistanceScale = null;
        /// <summary>
        /// Set distance scale (value between -10000 and 10000)
        /// </summary>
        private void SetDistanceScale(float _distanceScale)
        {
            if (lastUsedDistanceScale != _distanceScale && _distanceScale >= -10000.0f && _distanceScale <= 10000.0f)
            {
                lastUsedDistanceScale = _distanceScale;
                epDistanceScale.SetValue(_distanceScale);
            } // if
        } // SetDistanceScale

        #endregion

        #region Corner Fustrum

        /// <summary>
        /// Last used corner fustrum
        /// </summary>
        private static Vector3? lastUsedCornerFustrum = null;
        /// <summary>
        /// Set point corner fustrum
        /// </summary>
        protected void SetCornerFustrum(Vector3 _cornerFustrum)
        {
            if (lastUsedCornerFustrum != _cornerFustrum)
            {
                lastUsedCornerFustrum = _cornerFustrum;
                epCornerFustrum.SetValue(_cornerFustrum);
            }
        } // SetCornerFustrum

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Simple Screen Space Ambient Occlusion.
		/// </summary>
        public SSAOSimple(RenderToTexture.SizeType _rendeTargetSize = RenderToTexture.SizeType.HalfScreen)			
		{
            Effect = LoadShader("PostSSAOSimple");
            Effect.CurrentTechnique = Effect.Techniques["SSAO"];
            
            // The render target texture
            if (SSAOTexture == null)
                SSAOTexture = new RenderToTexture(_rendeTargetSize);

            GetParameters();

            // Set some parameters automatically
            randomNormalTexture = new Texture("RANDOMNORMAL");
            epRandomTexture.SetValue(randomNormalTexture.XnaTexture);
            SetCornerFustrum(CalculateCorner());
            Effect.Parameters["g_InvResolution"].SetValue(new Vector2(1 / (float)EngineManager.Width, 1 / (float)EngineManager.Height));
        } // SSAOSimple

		#endregion

		#region Get parameters

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParameters()
		{
            try
            {
                epDepthTexture = Effect.Parameters["depthTexture"];
                epRandomTexture = Effect.Parameters["randomTexture"];
                epSampleRadius = Effect.Parameters["sampleRadius"];
                epDistanceScale = Effect.Parameters["distanceScale"];
                epProjection = Effect.Parameters["Projection"];
                epCornerFustrum = Effect.Parameters["cornerFustrum"];	
		    }
            catch (Exception ex)
            {
                throw new Exception("Get the handles from the SSAO Simple shader failed. " + ex.ToString());
            }
		} // GetParameters

		#endregion

        #region Calculate Corner

        /// <summary>
        /// Calculate Corner
        /// </summary>
        private Vector3 CalculateCorner()
        {
            float farPlane = ApplicationLogic.Camera.FarPlane;

            float farY = (float)Math.Tan(Math.PI / 3.0 / 2.0) * farPlane;
            float farX = farY * EngineManager.AspectRatio;

            return new Vector3(farX, farY, farPlane);
        } // CalculateCorner

        #endregion

        #region GenerateSSAO

        /// <summary>
        /// Generate SSAO texture
		/// </summary>
        public void GenerateSSAO(Texture2D _depthTexture)
        {
            // Set shader atributes
            epDepthTexture.SetValue(_depthTexture);
            epProjection.SetValue(ApplicationLogic.Camera.ProjectionMatrix);
            SetSampleRadius(sampleRadius);
            SetDistanceScale(distanceScale);
                        
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
                throw new Exception("Unable to render the SSAO Simple effect " + e.Message);
            }

            // Resolve the render target to get the texture (required for Xbox)
            SSAOTexture.DisableRenderTarget();

        } // GenerateSSAO

		#endregion

    } // SSAOSimple
} // XNAFinalEngine.GraphicElements
