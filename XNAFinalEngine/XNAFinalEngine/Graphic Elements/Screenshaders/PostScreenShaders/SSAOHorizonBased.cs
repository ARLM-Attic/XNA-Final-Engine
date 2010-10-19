
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
using XNAFinalEngine.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Horizon Based Screen Space Ambient Occlusion.
    /// Este algoritmo fue sacado de un proyecto demostrativo de NVIDIA.
    /// El algoritmo esta pensando para DirectX 10 y fue pasado para DirectX 9. El problema fue el uso de las sentencias for
    /// en conjunto con un contador variable, dependiente de una variable global. Esto trae problemas al momento de compilar.
    /// Opte por fijar los paremtros con valores fijos, estos son facilmente cambiables en el codigo fuente del shader.
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
        private EffectParameter // Textures
                                epDepthNormalTexture,
                                epHighPresicionDepthTexture,
                                // Parameters
                                epNumberSteps,
                                epNumberDirections,
                                epContrast,
                                epLineAttenuation,
                                epAspectRatio,
                                epFocalLenght,
                                epAngleBias,
                                epRadius;

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
        private static float? lastUsedNumberSteps = null;
        /// <summary>
        /// Set Number of Steps (valor mayor a cero)
        /// </summary>
        private void SetNumberSteps(float _numberSteps)
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
        private static float? lastUsedNumberDirections = null;
        /// <summary>
        /// Set Number of Directions (valor mayor a cero)
        /// </summary>
        private void SetNumberDirections(float _numberDirections)
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
        private static float? lastUsedContrast = null;
        /// <summary>
        /// Contrast (valor mayor igual a cero y menos igual a 2)
        /// </summary>
        private void SetContrast(float _contrast)
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
        private static float? lastUsedLineAttenuation = null;
        /// <summary>
        /// Set Line Attenuation (valor mayor igual a cero y menos igual a 2)
        /// </summary>
        private void SetLineAttenuation(float _lineAttenuation)
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
        private static float? lastUsedAspectRatio = null;
        /// <summary>
        /// Set Aspect Ratio (valor mayor igual a 1 y menos igual a 3)
        /// </summary>
        private void SetAspectRatio(float _aspectRatio)
        {
            if (lastUsedAspectRatio != _aspectRatio && _aspectRatio >= 1.0f && _aspectRatio <= 3.0f)
            {
                lastUsedAspectRatio = _aspectRatio;
                Vector2 focalLenght = new Vector2();
                focalLenght.X = (float)(1.0 / Math.Tan(3.1416 / 6) * _aspectRatio);
                focalLenght.Y = (float)(1.0 / Math.Tan(3.1416 / 6));
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
        private static float? lastUsedRadius = null;
        /// <summary>
        /// Set Radius (valor mayor igual a cero y menos igual a 2)
        /// </summary>
        private void SetRadius(float _radius)
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
        private static float? lastUsedAngleBias = null;
        /// <summary>
        /// Set Angle Bias (valor mayor a cero y menos igual a 60)
        /// </summary>
        private void SetAngleBias(float _angleBias)
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

            GetParameters();

            // Set some parameters automatically
            Texture randomNormalTexture = new Texture("RANDOMNORMAL");
            Effect.Parameters["randomTexture"].SetValue(randomNormalTexture.XnaTexture);
            AspectRatio = EngineManager.AspectRatio;
            Effect.Parameters["g_Resolution"].SetValue(new Vector2(EngineManager.Width, EngineManager.Height));
            Effect.Parameters["g_InvResolution"].SetValue(new Vector2(1/(float)EngineManager.Width, 1/(float)EngineManager.Height));

            LoadUITestElements();
        } // SSAOHorizonBased

		#endregion

		#region Get parameters

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParameters()
		{
            try
            {
                epDepthNormalTexture = Effect.Parameters["depthNormalTexture"];
                epHighPresicionDepthTexture = Effect.Parameters["highPresicionDepthTexture"];

                epNumberSteps = Effect.Parameters["g_NumSteps"];
                epNumberDirections = Effect.Parameters["g_NumDir"];
                epContrast = Effect.Parameters["g_Contrast"];
                epLineAttenuation = Effect.Parameters["g_Attenuation"];
                epAspectRatio = Effect.Parameters["AspectRatio"];
                epFocalLenght = Effect.Parameters["g_FocalLen"];
                epRadius = Effect.Parameters["g_R"];
                epAngleBias = Effect.Parameters["g_AngleBias"];
            }
            catch (Exception ex)
            {
                throw new Exception("Get the handles from the SSAO Horizon Based shader failed. " + ex.ToString());
            }
		} // GetParameters

		#endregion

        #region Set Attributes
        
        /// <summary>
        /// Set to the shader the specific atributes of this effect.
        /// </summary>
        public void SetAttributes()
        {
            SetNumberSteps(NumberSteps);
            SetNumberDirections(NumberDirections);
            SetContrast(Contrast);
            SetLineAttenuation(LineAttenuation);
            SetAspectRatio(AspectRatio);
            SetRadius(Radius);
            SetAngleBias(AngleBias);
        } // SetAttributes

        #endregion

        #region GenerateSSAO
        
        /// <summary>
        /// Generate SSAO texture
		/// </summary>
        public void GenerateSSAO(Texture2D _depthTexture, Texture2D _normalTexture)
        {
            // Set shader atributes
            epDepthNormalTexture.SetValue(_normalTexture);
            epHighPresicionDepthTexture.SetValue(_depthTexture);
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp;
            SetAttributes();

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

        #region Test

        #region Variables

        private UISliderNumeric uiNumSteps,
                                uiNumDirs,
                                uiRadius,
                                uiContrast,
                                uiLinearAttenuation,
                                uiAngleBias;
        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        public void LoadUITestElements()
        {

            uiNumSteps = new UISliderNumeric("Number of Steps",           new Vector2(EngineManager.Width - 390, 110), 1, 32, 1,      NumberSteps);
            uiNumDirs = new UISliderNumeric("Number of Directions",       new Vector2(EngineManager.Width - 390, 150), 0, 15, 1,      NumberDirections);
            uiRadius = new UISliderNumeric("Radius",                      new Vector2(EngineManager.Width - 390, 190), 0, 2, 0.0025f, Radius);
            uiContrast = new UISliderNumeric("Contrast",                  new Vector2(EngineManager.Width - 390, 230), 0, 3, 0.05f,   Contrast);
            uiLinearAttenuation = new UISliderNumeric("Line Attenuation", new Vector2(EngineManager.Width - 390, 270), 0, 2, 0.05f,   LineAttenuation);
            uiAngleBias = new UISliderNumeric("Angle Bias",               new Vector2(EngineManager.Width - 390, 310), 1, 60, 1f,     AngleBias);
        } // LoadUITestElements

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public void Test()
        {

            #region Reset Parameters

            // Si los parametros se han modificado es mejor tener los nuevos valores
            uiNumSteps.CurrentValue = NumberSteps;
            uiNumDirs.CurrentValue = NumberDirections;
            uiRadius.CurrentValue = Radius;
            uiContrast.CurrentValue = Contrast;
            uiLinearAttenuation.CurrentValue = LineAttenuation;
            uiAngleBias.CurrentValue = AngleBias;

            #endregion

            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("SSAO Horizon Based Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiNumSteps.UpdateAndRender();
            uiNumDirs.UpdateAndRender();
            uiRadius.UpdateAndRender();
            uiContrast.UpdateAndRender();
            uiContrast.UpdateAndRender();
            uiLinearAttenuation.UpdateAndRender();
            uiAngleBias.UpdateAndRender();

            NumberSteps = uiNumSteps.CurrentValue;
            NumberDirections = uiNumDirs.CurrentValue;
            Radius = uiRadius.CurrentValue;
            Contrast = uiContrast.CurrentValue;
            LineAttenuation = uiLinearAttenuation.CurrentValue;
            AngleBias = uiAngleBias.CurrentValue;

        } // Test

        #endregion

    } // SSAOHorizonBased
} // XNAFinalEngine.GraphicElements
