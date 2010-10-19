
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Parallax Mapping.
    /// Only works with one directional light. Can be extended if need it.
    /// Important: in order that the shader works we need to pass the vertex�s tangent information. 
    /// The XNA�s content pipeline has an option for x models that automatically generates the tangent information.
    /// Tangents and XSI: http://www.gamedev.net/reference/articles/article2547.asp
    /// </summary>
    public class ParallaxMapping : Material
    {

        #region Shader Parameters

        #region Variables

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               // Surface //   
                               epSpecularColor,
                               epParallaxAmount,
                               epShininess,
                               // Textures //
                               epDiffuseTexture,
                               epNormalTexture;

        #endregion

        #region Surface

        #region Specular Color

        /// <summary>
        /// Specular Color
        /// </summary>
        private Color specularColor = Color.White;
        /// <summary>
        /// Specular Color
        /// </summary>
        public Color SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        /// <summary>
        /// Last used specular color
        /// </summary>
        private static Color? lastUsedSpecularColor = null;
        /// <summary>
        /// Set specular color
        /// </summary>
        private void SetSpecularColor(Color _specularColor)
        {
            if (lastUsedSpecularColor != _specularColor)
            {
                lastUsedSpecularColor = _specularColor;
                epSpecularColor.SetValue(new Vector4(_specularColor.R / 255f, _specularColor.G / 255f, _specularColor.B / 255f, 1));
            } // if
        } // SetSpecularColor

        #endregion

        #region Shininess

        /// <summary>
        /// Shininess
        /// </summary>
        private float shininess = 25.0f;

        /// <summary>
        /// Shininess
        /// </summary>
        public float Shininess
        {
            get { return shininess; }
            set { shininess = value; }
        } // Shininess

        /// <summary>
        /// Last used shininess
        /// </summary>
        private static float? lastUsedShininess = null;
        /// <summary>
        /// Set surface's shininess (valor mayor igual a cero)
        /// </summary>
        private void SetShininess(float _shininess)
        {
            if (lastUsedShininess != _shininess && _shininess >= 0.0f)
            {
                lastUsedShininess = _shininess;
                epShininess.SetValue(_shininess);
            } // if
        } // SetShininess

        #endregion

        #region Parallax Amount

        /// <summary>
        /// Parallax Amount
        /// </summary>
        private float parallaxAmount = 0.1f;

        /// <summary>
        /// Parallax Amount
        /// </summary>
        public float ParallaxAmount
        {
            get { return parallaxAmount; }
            set { parallaxAmount = value; }
        } // ParallaxAmount

        /// <summary>
        /// Last used Parallax Amount
        /// </summary>
        private static float? lastUsedParallaxAmount = null;
        /// <summary>
        /// Set surface's Parallax Amount (valor mayor igual a cero y menos igual que 1)
        /// </summary>
        private void SetParallaxAmount(float _parallaxAmount)
        {
            if (lastUsedParallaxAmount != _parallaxAmount && _parallaxAmount >= 0.0f && _parallaxAmount <= 1.0f)
            {
                lastUsedParallaxAmount = _parallaxAmount;
                epParallaxAmount.SetValue(_parallaxAmount);
            } // if
        } // SetParallaxAmount

        #endregion

        #endregion

        #region Textures

        #region Diffuse

        /// <summary>
        /// Diffuse texture
        /// </summary>
        private Texture diffuseTexture = null;

        /// <summary>
        /// Diffuse texture
        /// </summary>
        public Texture DiffuseTexture
        {
            get { return diffuseTexture; }
            set { diffuseTexture = value; }
        } // DiffuseTexture

        /// <summary>
		/// Last used diffuse texture
		/// </summary>
		private static Texture lastUsedDiffuseTexture = null;
		/// <summary>
		/// Set diffuse texture
		/// </summary>
        private void SetDiffuseTexture(Texture _diffuseTexture)
		{
			if (lastUsedDiffuseTexture != _diffuseTexture)
			{   
			    lastUsedDiffuseTexture = _diffuseTexture;
                epDiffuseTexture.SetValue(_diffuseTexture.XnaTexture);
		    } // if
		} // SetDiffuseTexture

        #endregion
        
        #region Normal

        /// <summary>
        /// Normal texture
        /// </summary>
        private Texture normalTexture = null;

        /// <summary>
        /// Normal texture
        /// </summary>
        public Texture NormalTexture
        {
            get { return normalTexture; }
            set { normalTexture = value; }
        } // NormalTexture

        /// <summary>
        /// Last used Normal texture
        /// </summary>
        private static Texture lastUsedNormalTexture = null;
        /// <summary>
        /// Set normal texture
        /// </summary>
        private void SetNormalTexture(Texture _normalTexture)
        {
            if (lastUsedNormalTexture != _normalTexture)
            {
                lastUsedNormalTexture = _normalTexture;
                epNormalTexture.SetValue(_normalTexture.XnaTexture);
            } // if
        } // SetNormalTexture

        #endregion

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Parallax Mapping
		/// </summary>
		private ParallaxMapping()
		{
            Effect = LoadShader("ParallaxMapping");
            
            GetParameters();

            LoadUITestElements();
        } // Parallax Mapping

        /// <summary>
        /// Parallax Mapping
        /// </summary>
        public ParallaxMapping(Texture _diffuseTexture, Texture _normalTexture) : this()
        {
            diffuseTexture = _diffuseTexture;
            normalTexture = _normalTexture;
        } // ParallaxMapping

        /// <summary>
        /// Parallax Mapping
        /// </summary>
        public ParallaxMapping(String textures) : this()
        {
            diffuseTexture = new Texture(textures + "-Diffuse");
            normalTexture  = new Texture(textures + "-Normal");
        } // ParallaxMapping

        /// <summary>
        /// Parallax Mapping
        /// </summary>
        public ParallaxMapping(String _diffuseTexture, String _normalTexture)
            : this()
        {
            diffuseTexture = new Texture(_diffuseTexture);
            normalTexture = new Texture(_normalTexture);
        } // ParallaxMapping

        #endregion
        
		#region GetParameters
		
        /// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected override void GetParameters()
		{
			try
			{
                // Matrices //
                GetCommonParameters();
                // Lights //
                GetAmbientLightParameters();
                GetDirectionalLight1Parameters();
                // Surface //                
                epSpecularColor = Effect.Parameters["specularColor"];
                epParallaxAmount = Effect.Parameters["parallaxAmount"];
                epShininess = Effect.Parameters["shininess"];
                // Textures //
                epDiffuseTexture = Effect.Parameters["DiffuseTexture"];
                epNormalTexture = Effect.Parameters["normalTexture"];
			}
			catch (Exception ex)
			{
                throw new Exception("Get the handles from the parallax mapping material failed. " + ex.ToString());				
			}
		} // GetParameters

		#endregion

        #region Set Attributes

        /// <summary>
        /// Set to the shader the parrallax mapping specific atributes of this material.
        /// </summary>
        private void SetParallaxMappingAttributes()
        {
            SetShininess(shininess);
            SetParallaxAmount(parallaxAmount);
            SetSpecularColor(specularColor);
        } // SetParallaxMappingAttributes

        #endregion

        #region Render

        /// <summary>
        /// Render this shader/material; to do this job it takes an object model, its associated lights, and its matrices.
		/// </summary>		
        public override void Render(Matrix worldMatrix, PointLight[] pointLight, DirectionalLight[] directionalLight, SpotLight[] spotLight, Model model)
		{   
            try
            {
                SetCommomAtributes(worldMatrix);
                SetDiffuseTexture(diffuseTexture);
                SetNormalTexture(normalTexture);
                SetAmbientLightAttributes();
                SetParallaxMappingAttributes();
                if (directionalLight != null)
                {
                    SetDirectionalLight1Attributes(directionalLight[0]);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to set the shader parameters " + e.Message);
            }
            base.Render(model);
        } // Render

		#endregion        

        #region Test

        #region Variables

        private UISliderNumeric uiShininess,
                                uiParallaxAmount;
        private UISliderColor uiSurfaceColor,
                                uiSpecularColor;

        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        protected override void LoadUITestElements()
        {
            uiShininess = new UISliderNumeric("Shininess", new Vector2(EngineManager.Width - 390, 110), 0.0f, 100f, 0.05f, Shininess);
            uiParallaxAmount = new UISliderNumeric("Parallax Amount", new Vector2(EngineManager.Width - 390, 150), 0.0f, 1.0f, 0.005f, ParallaxAmount);
            uiSurfaceColor = new UISliderColor("Surface Color", new Vector2(EngineManager.Width - 390, 190), SurfaceColor);
            uiSpecularColor = new UISliderColor("Specular Color", new Vector2(EngineManager.Width - 390, 230), SpecularColor);
        } // LoadUITestElements

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public override void Test()
        {

            #region Reset Parameters

            // If the parameters were modified is better to have the new values. 
            uiShininess.CurrentValue = Shininess;
            uiParallaxAmount.CurrentValue = ParallaxAmount;
            uiSurfaceColor.CurrentValue = SurfaceColor;
            uiSpecularColor.CurrentValue = SpecularColor;

            #endregion

            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Parallax Mapping Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiShininess.UpdateAndRender();
            uiParallaxAmount.UpdateAndRender();
            uiSurfaceColor.UpdateAndRender();
            uiSpecularColor.UpdateAndRender();

            Shininess = uiShininess.CurrentValue;
            ParallaxAmount = uiParallaxAmount.CurrentValue;
            SurfaceColor = uiSurfaceColor.CurrentValue;
            SpecularColor = uiSpecularColor.CurrentValue;
        } // Test

        #endregion
        
        // Common code to other shaders //

        #region Common Atributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles for all material shaders
        /// </summary>
        protected static EffectParameter
            // Matrices // 
                               epWorldIT,
                               epWorldViewProj,
                               epWorld,
                               epViewI,
                               epViewProj,
            // Surface //
                               epSurfaceColor,
                               epAlphaBlending;

        #region Matrices

        /// <summary>
        /// Last used transpose inverse world matrix
        /// </summary>
        private static Matrix? lastUsedTransposeInverseWorldMatrix = null;
        /// <summary>
        /// Set transpose inverse world matrix
        /// </summary>
        private Matrix TransposeInverseWorldMatrix
        {
            set
            {
                if (lastUsedTransposeInverseWorldMatrix != value)
                {
                    lastUsedTransposeInverseWorldMatrix = value;
                    epWorldIT.SetValue(value);
                } // if
            } // set
        } // TransposeInvertWorldMatrix

        /// <summary>
        /// Last used world matrix
        /// </summary>
        private static Matrix? lastUsedWorldMatrix = null;
        /// <summary>
        /// Set world matrix
        /// </summary>
        private Matrix WorldMatrix
        {
            set
            {
                if (lastUsedWorldMatrix != value)
                {
                    lastUsedWorldMatrix = value;
                    epWorld.SetValue(value);
                } // if
            } // set
        } // WorldMatrix

        /// <summary>
        /// Last used inverse view matrix
        /// </summary>
        private static Matrix? lastUsedInverseViewMatrix = null;
        /// <summary>
        /// Set view inverse matrix
        /// </summary>
        private Matrix InverseViewMatrix
        {
            set
            {
                if (lastUsedInverseViewMatrix != value)
                {
                    lastUsedInverseViewMatrix = value;
                    epViewI.SetValue(value);
                } // if
            } // set
        } // InverseViewMatrix

        /// <summary>
        /// Last used world view projection matrix
        /// </summary>
        private static Matrix? lastUsedWorldViewProjMatrix = null;
        /// <summary>
        /// Set world view projection matrix
        /// </summary>
        private Matrix WorldViewProjMatrix
        {
            set
            {
                if (lastUsedWorldViewProjMatrix != value)
                {
                    lastUsedWorldViewProjMatrix = value;
                    epWorldViewProj.SetValue(value);
                } // if
            } // set
        } // WorldViewProjMatrix

        /// <summary>
        /// Last used view projection matrix
        /// </summary>
        private static Matrix? lastUsedViewProjMatrix = null;
        /// <summary>
        /// Set view projection matrix
        /// </summary>
        private Matrix ViewProjMatrix
        {
            set
            {
                if (lastUsedViewProjMatrix != value)
                {
                    lastUsedViewProjMatrix = value;
                    epViewProj.SetValue(value);
                } // if
            } // set
        } // ViewProjMatrix

        #endregion

        #region Surface

        /// <summary>
        /// Surface color
        /// </summary>
        private Color surfaceColor = Color.White;

        /// <summary>
        /// Surface color
        /// </summary>
        public Color SurfaceColor
        {
            get { return surfaceColor; }
            set { surfaceColor = value; }
        }

        /// <summary>
        /// Last used surface color
        /// </summary>
        private static Color? lastUsedSurfaceColor = null;
        /// <summary>
        /// Set surface color
        /// </summary>
        private void SetSurfaceColor(Color _surfaceColor)
        {
            if (lastUsedSurfaceColor != _surfaceColor)
            {
                lastUsedSurfaceColor = _surfaceColor;
                epSurfaceColor.SetValue(new Vector3(_surfaceColor.R / 255f, _surfaceColor.G / 255f, _surfaceColor.B / 255f));
            }
        } // SetSurfaceColor

        /// <summary>
        /// Alpha blending
        /// </summary>
        private float alphaBlending = 1.0f;

        /// <summary>
        /// Alpha blending
        /// </summary>
        public float AlphaBlending
        {
            get { return alphaBlending; }
            set { alphaBlending = value; }
        } // AlphaBlending

        /// <summary>
        /// Last used alpha blending
        /// </summary>
        private static float? lastUsedAlphaBlending = null;
        /// <summary>
        /// Set surface's alpha blending (value between 0 and 1)
        /// </summary>
        private void SetAlphaBlending(float _alphaBlending)
        {
            if (lastUsedAlphaBlending != _alphaBlending && _alphaBlending >= 0.0f && _alphaBlending <= 1.0f)
            {
                lastUsedAlphaBlending = _alphaBlending;
                epAlphaBlending.SetValue(_alphaBlending);
            }
        } // SetAlphaBlending

        #endregion

        #endregion

        /// <summary>
        /// Get the common handlers parameters from the shader.
        /// </summary>
        protected void GetCommonParameters()
        {
            // Matrices //
            epWorldIT = Effect.Parameters["WorldIT"];
            epWorldViewProj = Effect.Parameters["WorldViewProj"];
            epWorld = Effect.Parameters["World"];
            epViewI = Effect.Parameters["ViewI"];
            epViewProj = Effect.Parameters["ViewProj"];
            // Alpha Blending //
            epSurfaceColor = Effect.Parameters["SurfaceColor"];
            epAlphaBlending = Effect.Parameters["AlphaBlending"];
        } // GetCommonParameters

        /// <summary>
        /// Set to the shader the common atributes of this material.
        /// </summary>
        protected virtual void SetCommomAtributes(Matrix worldMatrix)
        {
            // Initialization of the Matrices
            TransposeInverseWorldMatrix = Matrix.Transpose(Matrix.Invert(worldMatrix));
            WorldMatrix = worldMatrix;
            WorldViewProjMatrix = worldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix;
            InverseViewMatrix = Matrix.Invert(ApplicationLogic.Camera.ViewMatrix);
            ViewProjMatrix = ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix;
            // Inicialization of the alpha blending
            SetSurfaceColor(surfaceColor);
            SetAlphaBlending(alphaBlending);
        } // SetCommomAtributes

        #endregion

        #region Ambient Light Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epAmbientLightColor;

        /// <summary>
        /// Last used ambient light color
        /// </summary>
        private static Color? lastUsedAmbientLightColor = null;
        /// <summary>
        /// Set ambient light color
        /// </summary>
        protected void SetAmbientLightColor(Color _ambientLightColor)
        {
            if (lastUsedAmbientLightColor != _ambientLightColor)
            {
                lastUsedAmbientLightColor = _ambientLightColor;
                epAmbientLightColor.SetValue(new Vector3(_ambientLightColor.R / 255f, _ambientLightColor.G / 255f, _ambientLightColor.B / 255f));
            }
        } // AmbientLightColor

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetAmbientLightParameters()
        {
            epAmbientLightColor = Effect.Parameters["AmbientLightColor"];
        } // GetAmbientLightParameters

        /// <summary>
        /// Set the ambient light parameters to the shader.
        /// </summary>
        public virtual void SetAmbientLightAttributes()
        {
            SetAmbientLightColor(AmbientLight.LightColor);
        } // SetAmbientLightAttributes

        #endregion

        #region Directional Light 1 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epDirectionalLight1Dir,
                                         epDirectionalLight1Color;

        /// <summary>
        /// Last used directional light 1 color
        /// </summary>
        private static Color? lastUsedDirectionalLight1Color = null;
        /// <summary>
        /// Set directional light 1 color
        /// </summary>
        protected void SetDirectionalLight1Color(Color _directionalLight1Color)
        {
            if (lastUsedDirectionalLight1Color != _directionalLight1Color)
            {
                lastUsedDirectionalLight1Color = _directionalLight1Color;
                epDirectionalLight1Color.SetValue(new Vector3(_directionalLight1Color.R / 255f, _directionalLight1Color.G / 255f, _directionalLight1Color.B / 255f));
            }
        } // SetDirectionalLight1Color

        /// <summary>
        /// Last used directional light 1 direction
        /// </summary>
        private static Vector3? lastUsedDirectionalLight1Dir = null;
        /// <summary>
        /// Set directional light 1 direction
        /// </summary>
        protected void SetDirectionalLight1Dir(Vector3 _directionalLight1Dir)
        {
            if (lastUsedDirectionalLight1Dir != _directionalLight1Dir)
            {
                lastUsedDirectionalLight1Dir = _directionalLight1Dir;
                epDirectionalLight1Dir.SetValue(new Vector3(_directionalLight1Dir.X, _directionalLight1Dir.Y, _directionalLight1Dir.Z));
            }
        } // SetDirectionalLight1Dir

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetDirectionalLight1Parameters()
        {
            epDirectionalLight1Color = Effect.Parameters["DirectionalLightColor"];
            epDirectionalLight1Dir = Effect.Parameters["DirectionalLightDir"];
        } // GetDirectionalLight1Parameters

        /// <summary>
        /// Set the directional light parameters to the shader.
        /// </summary>
        public virtual void SetDirectionalLight1Attributes(DirectionalLight directionalLight)
        {
            if (directionalLight != null)
            {
                SetDirectionalLight1Color(directionalLight.Color);
                SetDirectionalLight1Dir(directionalLight.Direction);
            }
            else
            {
                SetDirectionalLight1Color(Color.Black);
            }
        } // SetDirectionalLight1Attributes

        #endregion

    } // ParrallaxMapping
} // XNAFinalEngine.GraphicElements

