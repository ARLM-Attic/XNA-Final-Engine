
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
    /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
    /// It doesn't have local illumination; however it can be affected by global illumination.
    /// </summary>
    public class ConstantDecal : Material
    {

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               // Textures //
                               epDiffuseTexture,
                               epAlphaTexture;

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

        /// <summary>
        /// Alpha texture
        /// </summary>
        private Texture alphaTexture = null;

        /// <summary>
        /// Alpha texture
        /// </summary>
        public Texture AlphaTexture
        {
            get { return alphaTexture; }
            set { alphaTexture = value; }
        } // AlphaTexture

        /// <summary>
        /// Last used alpha texture
        /// </summary>
        private static Texture lastUsedAlphaTexture = null;
        /// <summary>
        /// Set alpha texture
        /// </summary>
        private void SetAlphaTexture(Texture _alphaTexture)
        {
            if (lastUsedAlphaTexture != _alphaTexture)
            {
                lastUsedAlphaTexture = _alphaTexture;
                epAlphaTexture.SetValue(_alphaTexture.XnaTexture);
            } // if
        } // SetAlphaTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
        /// It doesn't have local illumination; however it can be affected by global illumination.
		/// </summary>
        protected ConstantDecal()
		{
            Effect = LoadShader("ConstantDecal");
            GetParameters();
            LoadUITestElements();
        } // Constant

        /// <summary>
        /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
        /// It doesn't have local illumination; however it can be affected by global illumination.
        /// </summary>
        public ConstantDecal(Color surfaceColor, String _alphaTexture) : this()
        {
            SurfaceColor = surfaceColor;
            alphaTexture = new Texture(_alphaTexture);
        } // Constant

        /// <summary>
        /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
        /// It doesn't have local illumination; however it can be affected by global illumination.
        /// </summary>
        public ConstantDecal(Texture _diffuseTexture, Texture _alphaTexture) : this()
        {
            diffuseTexture = _diffuseTexture;
            alphaTexture = _alphaTexture;
        } // Constant

        /// <summary>
        /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
        /// It doesn't have local illumination; however it can be affected by global illumination.
        /// </summary>
        public ConstantDecal(String _diffuseTexture, String _alphaTexture) : this()
        {
            diffuseTexture = new Texture(_diffuseTexture);
            alphaTexture = new Texture(_alphaTexture);
        } // Constant

        /// <summary>
        /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
        /// It doesn't have local illumination; however it can be affected by global illumination.
        /// </summary>
        public ConstantDecal(Color surfaceColor, String _alphaTexture, float alphaBlending) : this()
        {
            SurfaceColor = surfaceColor;
            alphaTexture = new Texture(_alphaTexture);
            AlphaBlending = alphaBlending;
        } // Constant

        /// <summary>
        /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
        /// It doesn't have local illumination; however it can be affected by global illumination.
        /// </summary>
        public ConstantDecal(Texture _diffuseTexture, Texture _alphaTexture, float alphaBlending) : this()
        {
            diffuseTexture = _diffuseTexture;
            alphaTexture = _alphaTexture;
            AlphaBlending = alphaBlending;
        } // Constant

        /// <summary>
        /// Constant Decal Material. Apply a alpha mask (white and black texture) to the color or texture indicated.
        /// It doesn't have local illumination; however it can be affected by global illumination.
        /// </summary>
        public ConstantDecal(String _diffuseTexture, String _alphaTexture, float alphaBlending) : this()
        {
            diffuseTexture = new Texture(_diffuseTexture);
            alphaTexture = new Texture(_alphaTexture);
            AlphaBlending = alphaBlending;
        } // Constant

		#endregion
        
		#region GetParameters

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
		protected override void GetParameters()
		{
			try
			{
                // Matrixes and Lights //
                GetCommonParameters();
                // Textures //
                epDiffuseTexture = Effect.Parameters["DiffuseTexture"];
                epAlphaTexture = Effect.Parameters["AlphaTexture"];
			}
			catch (Exception ex)
			{
                throw new Exception("Get the handles from the constant decal material failed. " + ex.ToString());				
			}
		} // GetParameters

		#endregion

        #region Render

        /// <summary>
        /// Render this shader/material; to do this job it takes an object model, its associated lights, and its matrixes.
		/// </summary>		
        public override void Render(Matrix worldMatrix, PointLight[] pointLight, DirectionalLight[] directionalLight, SpotLight[] spotLight,
                                    Model model)
		{   
            try
            {
                SetCommomAtributes(worldMatrix);
                SetAlphaTexture(alphaTexture);
                if (diffuseTexture == null)
                    Effect.CurrentTechnique = Effect.Techniques["ConstantDecalWithoutTexture"];
                else
                {
                    Effect.CurrentTechnique = Effect.Techniques["ConstantDecalWithTexture"];
                    SetDiffuseTexture(diffuseTexture);
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

        private UISliderNumeric uiAlphaBlending;
        private UISliderColor   uiSurfaceColor;

        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        protected override void LoadUITestElements()
        {            
            uiSurfaceColor  = new UISliderColor("Surface Color", new Vector2(EngineManager.Width - 390, 110), SurfaceColor);
            uiAlphaBlending = new UISliderNumeric("Alpha Blending", new Vector2(EngineManager.Width - 390, 150), 0.0f, 1f, 0.01f, AlphaBlending);
        } // LoadUITestElements

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public override void Test()
        {

            #region Reset Parameters

            // If the parameters were modified is better to have the new values. 
            uiAlphaBlending.CurrentValue = AlphaBlending;
            uiSurfaceColor.CurrentValue = SurfaceColor;

            #endregion
            
            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Constant Material Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiAlphaBlending.UpdateAndRender();
            uiSurfaceColor.UpdateAndRender();

            AlphaBlending = uiAlphaBlending.CurrentValue;
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

    } // ConstantDecal
} // XNAFinalEngine.GraphicElements

