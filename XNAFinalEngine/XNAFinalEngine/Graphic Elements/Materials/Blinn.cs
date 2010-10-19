
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.GraphicElements
{

    /// <summary>
    /// Blinn Material.
    /// Accept a color or a texture, but not both.
    /// It has specular color that can be controlled with shininess and intensity.
    /// This material accepts one point light, one directional light and one spot light for the time being. 
    /// It also has an optimized version that works with only one directional light, but this is transparent to the developer,
    /// it loads only when there is one directional light applied to the object.
    /// This material can be extended to work with more flexibility, with more lights, and with a scheme more powerful.
    /// But for now it�s sufficient in the majority of cases. 
    /// </summary>
    public class Blinn : Material
    {

		#region Shader Parameters

        #region Variables

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               // Surface //   
                               epSpecIntensity,
                               epSpecExponent,
                               // Textures //
                               epDiffuseTexture;

        #endregion

        #region Surface

        /// <summary>
        /// Specular Intensity
        /// </summary>
        private float specIntensity;
        /// <summary>
        /// Specular Intensity
        /// </summary>
        public float SpecularIntensity
        {
            get { return specIntensity; }
            set { specIntensity = value; }
        } // SpecularIntensity
                
        /// <summary>
        /// Last used Specular Intensity
        /// </summary>
        private static float ?lastUsedSpecIntensity = null;
        /// <summary>
        /// Set Specular Intensity (valor mayor a 0)
        /// </summary>
        private void SetSpecularIntensity(float _specIntensity)
        {
            if (lastUsedSpecIntensity != _specIntensity && _specIntensity >= 0.0f)
            {
                lastUsedSpecIntensity = _specIntensity;
                epSpecIntensity.SetValue(_specIntensity);
            }
        } // SpecularIntensity
        
        /// <summary>
        /// Specular Exponent
        /// </summary>
        private float specExponent;
        /// <summary>
        /// Specular Exponent
        /// </summary>
        public float SpecularExponent
        {
            get { return specExponent; }
            set { specExponent = value; }
        } // SpecularExponent

        /// <summary>
        /// Last used Specular Exponent
        /// </summary>
        private static float ?lastUsedSpecExponent = null;
        /// <summary>
        /// Set Specular Exponent (valor mayor a 0)
        /// </summary>
        private void SetSpecularExponent(float _specExponent)
        {
            if (lastUsedSpecExponent != _specExponent && _specExponent >= 0.0f)
            {
                lastUsedSpecExponent = _specExponent;
                epSpecExponent.SetValue(_specExponent);
            }
        } // SpecularExponent
                
        #endregion

        #region Textures

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
        } // SpecularIntensity

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

        #endregion

        #region Constructor

        /// <summary>
        /// Blinn Material.
		/// </summary>
		public Blinn()
		{
            Effect = LoadShader("Blinn");            
            GetParameters();
            LoadUITestElements();
            SpecularIntensity = 1;
            SpecularExponent = 100;
        } // Blinn

        /// <summary>
        /// Blinn Material.
        /// </summary>
        public Blinn(Color surfaceColor, float specularIntensity = 1, float specularExponent = 100, float alphaBlending = 1) : this()
        {
            SurfaceColor = surfaceColor;
            SpecularIntensity = specularIntensity;
            SpecularExponent = specularExponent;
            AlphaBlending = alphaBlending;
        } // Blinn

        /// <summary>
        /// Blinn Material.
        /// </summary>
        public Blinn(Texture _diffuseTexture, float _specularIntensity = 1, float _specularExponent = 100, float _alphaBlending = 1) : this()
        {
            diffuseTexture = _diffuseTexture;
            SpecularIntensity = _specularIntensity;
            SpecularExponent = _specularExponent;
            AlphaBlending = _alphaBlending;
        } // Blinn

        /// <summary>
        /// Blinn Material.
        /// </summary>
        public Blinn(String _diffuseTexture, float _specularIntensity = 1, float _specularExponent = 100, float _alphaBlending = 1) : this()
        {
            diffuseTexture = new Texture(_diffuseTexture);
            SpecularIntensity = _specularIntensity;
            SpecularExponent = _specularExponent;
            AlphaBlending = _alphaBlending;
        } // Blinn

		#endregion
        
		#region Get Parameters
		
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
                GetPointLight1Parameters();
                GetDirectionalLight1Parameters();
                GetSpotLight1Parameters();
                // Surface //                
                epSpecIntensity = Effect.Parameters["SpecIntensity"];
                epSpecExponent  = Effect.Parameters["SpecExponent"];
                // Textures //
                epDiffuseTexture = Effect.Parameters["DiffuseTexture"];
			}
			catch (Exception ex)
			{
                throw new Exception("Get the handles from the blinn material failed. " + ex.ToString());
			}
		} // GetParameters

		#endregion

        #region Set Attributes

        /// <summary>
        /// Set to the shader the blinn specific atributes of this material.
        /// </summary>
        private void SetBlinnAttributes()
        {
            SetSpecularIntensity(SpecularIntensity);
            SetSpecularExponent(SpecularExponent);
        }

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
                SetAmbientLightAttributes();
                SetBlinnAttributes();
                if (pointLight != null || spotLight != null)
                {
                    if (diffuseTexture == null)
                        Effect.CurrentTechnique = Effect.Techniques["BlinnWithoutTexture"];
                    else
                    {
                        Effect.CurrentTechnique = Effect.Techniques["BlinnWithTexture"];
                        SetDiffuseTexture(diffuseTexture);
                    }
                    if (pointLight != null)
                    {
                        SetPointLight1Attributes(pointLight[0]);
                    }
                    else
                    {
                        SetPointLight1Attributes(null);
                    }
                    if (spotLight != null)
                    {
                        SetSpotLight1Attributes(spotLight[0]);
                    }
                    else
                    {
                        SetSpotLight1Attributes(null);
                    }
                }
                else
                {
                    if (diffuseTexture == null)
                        Effect.CurrentTechnique = Effect.Techniques["BlinnWithoutTextureOnlyDirectional"];
                    else
                    {
                        Effect.CurrentTechnique = Effect.Techniques["BlinnWithTextureOnlyDirectional"];
                        SetDiffuseTexture(diffuseTexture);
                    }  
                }
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

        private UISliderNumeric uiAlphaBlending,
                                uiSpecularIntensity,
                                uiSpecularExponent;
        private UISliderColor   uiSurfaceColor;

        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        protected override void LoadUITestElements()
        {
            uiSpecularIntensity = new UISliderNumeric("Specular Intensity", new Vector2(EngineManager.Width - 390, 110), 0.0f, 5f, 0.5f, SpecularIntensity);
            uiSpecularExponent = new UISliderNumeric("Specular Exponent", new Vector2(EngineManager.Width - 390, 150), 0.0f, 500.0f, 0.05f, SpecularExponent);
            uiSurfaceColor = new UISliderColor("Surface Color", new Vector2(EngineManager.Width - 390, 190), SurfaceColor);
            uiAlphaBlending = new UISliderNumeric("Alpha Blending", new Vector2(EngineManager.Width - 390, 230), 0.0f, 1f, 0.01f, AlphaBlending);

        }

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public override void Test()
        {

            #region Reset Parameters

            // If the parameters were modified is better to have the new values. 
            uiAlphaBlending.CurrentValue = AlphaBlending;
            uiSpecularIntensity.CurrentValue = SpecularIntensity;
            uiSpecularExponent.CurrentValue = SpecularExponent;
            uiSurfaceColor.CurrentValue = SurfaceColor;

            #endregion

            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Car-Paint Material Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiAlphaBlending.UpdateAndRender();
            uiSpecularIntensity.UpdateAndRender();
            uiSpecularExponent.UpdateAndRender();
            uiSurfaceColor.UpdateAndRender();

            AlphaBlending = uiAlphaBlending.CurrentValue;
            SpecularIntensity = uiSpecularIntensity.CurrentValue;
            SpecularExponent = uiSpecularExponent.CurrentValue;
            SurfaceColor = uiSurfaceColor.CurrentValue;
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

        #region Point Light 1 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epPointLight1Pos,
                                         epPointLight1Color;

        #region Color

        /// <summary>
        /// Last used point light 1 color
        /// </summary>
        private static Color? lastUsedPointLight1Color = null;
        /// <summary>
        /// Set point light 1 color
        /// </summary>
        protected void SetPointLight1Color(Color _pointLight1Color)
        {
            if (lastUsedPointLight1Color != _pointLight1Color)
            {
                lastUsedPointLight1Color = _pointLight1Color;
                epPointLight1Color.SetValue(new Vector3(_pointLight1Color.R / 255f, _pointLight1Color.G / 255f, _pointLight1Color.B / 255f));
            }
        } // SetPointLight1Color

        #endregion

        #region Position

        /// <summary>
        /// Last used point light 1 position
        /// </summary>
        private static Vector3? lastUsedPointLight1Pos = null;
        /// <summary>
        /// Set point light 1 position
        /// </summary>
        protected void SetPointLight1Pos(Vector3 _pointLight1Pos)
        {
            if (lastUsedPointLight1Pos != _pointLight1Pos)
            {
                lastUsedPointLight1Pos = _pointLight1Pos;
                epPointLight1Pos.SetValue(new Vector3(_pointLight1Pos.X, _pointLight1Pos.Y, _pointLight1Pos.Z));
            }
        } // SetPointLight1Pos

        #endregion

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetPointLight1Parameters()
        {
            epPointLight1Color = Effect.Parameters["PointLightColor"];
            epPointLight1Pos = Effect.Parameters["PointLightPos"];
        } // GetPointLight1Parameters

        /// <summary>
        /// Set the point light parameters to the shader.
        /// </summary>
        public virtual void SetPointLight1Attributes(PointLight pointLight)
        {
            if (pointLight != null)
            {
                SetPointLight1Color(pointLight.Color);
                SetPointLight1Pos(pointLight.Position);
            }
            else
            {
                SetPointLight1Color(Color.Black);
            }
        } // SetPointLight1Attributes

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

        #region Spot Light 1 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epSpotLight1Pos,
                                         epSpotLight1Dir,
                                         epSpotLight1Cone,
                                         epSpotLight1Color,
                                         epSpotLight1Intensity;

        #region Color

        /// <summary>
        /// Last used spot light 1 color
        /// </summary>
        private static Color? lastUsedSpotLight1Color = null;
        /// <summary>
        /// Set spot light 1 color
        /// </summary>
        protected void SetSpotLight1Color(Color _spotLight1Color)
        {
            if (lastUsedSpotLight1Color != _spotLight1Color)
            {
                lastUsedSpotLight1Color = _spotLight1Color;
                epSpotLight1Color.SetValue(new Vector3(_spotLight1Color.R / 255f, _spotLight1Color.G / 255f, _spotLight1Color.B / 255f));
            }
        } // SetSpotLight1Color

        #endregion

        #region Direction

        /// <summary>
        /// Last used spot light 1 direction
        /// </summary>
        private static Vector3? lastUsedSpotLight1Dir = null;
        /// <summary>
        /// Set spot light 1 direction
        /// </summary>
        protected void SetSpotLight1Dir(Vector3 _spotLight1Dir)
        {
            if (lastUsedSpotLight1Dir != _spotLight1Dir)
            {
                lastUsedSpotLight1Dir = _spotLight1Dir;
                epSpotLight1Dir.SetValue(new Vector3(_spotLight1Dir.X, _spotLight1Dir.Y, _spotLight1Dir.Z));
            }
        } // SetSpotLight1Dir

        #endregion

        #region Position

        /// <summary>
        /// Last used spot light 1 position
        /// </summary>
        private static Vector3? lastUsedSpotLight1Pos = null;
        /// <summary>
        /// Set spot light 1 position
        /// </summary>
        protected void SetSpotLight1Pos(Vector3 _spotLight1Pos)
        {
            if (lastUsedSpotLight1Pos != _spotLight1Pos)
            {
                lastUsedSpotLight1Pos = _spotLight1Pos;
                epSpotLight1Pos.SetValue(new Vector3(_spotLight1Pos.X, _spotLight1Pos.Y, _spotLight1Pos.Z));
            }
        } // SetPointLight1Pos

        #endregion

        #region Cone

        /// <summary>
        /// Last used Spot Light 1 Cone
        /// </summary>
        private static float? lastUsedSpotLight1Cone = null;
        /// <summary>
        /// Set Set Spot Light 1 Cone (value between 0 and 90)
        /// </summary>
        private void SetSpotLight1Cone(float _spotLight1Cone)
        {
            if (lastUsedSpotLight1Cone != _spotLight1Cone && _spotLight1Cone >= 0.0f && _spotLight1Cone <= 90.0f)
            {
                lastUsedSpotLight1Cone = _spotLight1Cone;
                epSpotLight1Cone.SetValue(_spotLight1Cone);
            }
        } // SetSpotLight1Cone

        #endregion

        #region Intensity

        /// <summary>
        /// Last used Spot Light 1 Intensity
        /// </summary>
        private static float? lastUsedSpotLight1Intensity = null;
        /// <summary>
        /// Set Set Spot Light 1 Intensity (value between 0 and 10)
        /// </summary>
        private void SetSpotLight1Intensity(float _spotLight1Intensity)
        {
            if (lastUsedSpotLight1Intensity != _spotLight1Intensity && _spotLight1Intensity >= 0.0f && _spotLight1Intensity <= 10f)
            {
                lastUsedSpotLight1Intensity = _spotLight1Intensity;
                epSpotLight1Intensity.SetValue(_spotLight1Intensity);
            }
        } // SetSpotLight1Intensity

        #endregion

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetSpotLight1Parameters()
        {
            epSpotLight1Color = Effect.Parameters["SpotLightColor"];
            epSpotLight1Dir = Effect.Parameters["SpotLightDir"];
            epSpotLight1Pos = Effect.Parameters["SpotLightPos"];
            epSpotLight1Cone = Effect.Parameters["SpotLightCone"];
            epSpotLight1Intensity = Effect.Parameters["SpotLightIntensity"];
        } // GetSpotLight1Parameters

        /// <summary>
        /// Set the spot light parameters to the shader.
        /// </summary>
        public virtual void SetSpotLight1Attributes(SpotLight spotLight)
        {
            if (spotLight != null)
            {
                SetSpotLight1Color(spotLight.Color);
                SetSpotLight1Dir(spotLight.Direction);
                SetSpotLight1Pos(spotLight.Position);
                SetSpotLight1Cone(spotLight.ApertureCone);
                SetSpotLight1Intensity(spotLight.Intensity);
            }
            else
            {
                SetSpotLight1Color(Color.Black);
            }
        } // SetSpotLight1Attributes

        #endregion

    } // Blinn
} // XNAFinalEngine.GraphicElements

