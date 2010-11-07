
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
    /// Car paint material.
    /// This material accepts up to three point lights.
    /// However it doesn't accept other types of lights, it can be extended though.
    /// 
    /// This material includes some features to imitate the look of tricked-out car paint.
    /// Some car paint appears to change color based on your viewing angle.
    /// The paint looks like one color when you're looking straight at the surface and another color when
    /// the surface is parallel to your view. This shader imitates that effect.
    ///
    /// To achieve the effect, the first thing we need is a color gradient.
    /// The shader includes two colors. Surfaces on the model that are facing you will receive
    /// the "Middle Color" while surfaces at glancing angles will receive the "Edge Color".
    ///
    /// The amount of each color in the gradient is determined by the "Fresnel Power"
    /// and "Fresnel Bias" values. These will allow you to tweek how much of each color gets applied to the effect.
    /// 
    /// Finally, the two colors are subtracted from the per-pixel lighting result.
    /// Because the colors are subtracted, you'll end up with the opposite colors from
    /// the ones you selected and they'll darken the final result.
    ///
    /// The fresnel term in this shader is a very useful shader component.
    /// Most objects that are reflective are more reflective at glancing angles than straight on and
    /// the fresnel term can be used to achieve that effect. 
    /// It can also be used to blend between reflection and refraction in water.
    /// </summary>
    public class CarPaint : Material
    {   

        #region Shader Parameters

        #region Variables

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               // Surface //
                               epSpecularColor,
                               epShininess,
                               epEdgeColor,
                               epMiddleColor,
                               epFresnelBias,
                               epFresnelPower,
                               epReflection,
                               // Textures //
                               EPcubeMapTexture;

        #endregion

        #region Floats

        /// <summary>
        /// Shininess.
        /// </summary>
        private float shininess = 25.0f;
        /// <summary>
        /// Shininess.
        /// </summary>
        public float Shininess
        {
            get { return shininess; }
            set { shininess = value; }
        } // Shininess

        /// <summary>
        /// Last used shininess.
        /// </summary>
        private static float? lastUsedShininess = null;
        /// <summary>
        /// Set surface's shininess (greater or equal to 0)
        /// </summary>
        private void SetShininess(float _shininess)
        {
            if (lastUsedShininess != _shininess && _shininess >= 0.0f)
            {
                lastUsedShininess = _shininess;
                epShininess.SetValue(_shininess);
            } // if
        } // SetShininess
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Fresnel bias
        /// </summary>
        private float fresnelBias = 0.5f;
        /// <summary>
        /// Fresnel bias
        /// </summary>
        public float FresnelBias
        {
            get { return fresnelBias; }
            set { fresnelBias = value; }
        } // FresnelBias

        /// <summary>
        /// Last used fresnel bias
        /// </summary>
        private static float? lastUsedFresnelBias = null;
        /// <summary>
        /// Set surface's fresnel bias (greater or equal to 0)
        /// </summary>
        private void SetFresnelBias(float _fresnelBias)
        {
            if (lastUsedFresnelBias != _fresnelBias && _fresnelBias >= 0.0f)
            {
                lastUsedFresnelBias = _fresnelBias;
                epFresnelBias.SetValue(_fresnelBias);
            } // if
        } // SetFresnelBias
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Fresnel power
        /// </summary>
        private float fresnelPower = 5;

        /// <summary>
        /// Fresnel power
        /// </summary>
        public float FresnelPower
        {
            get { return fresnelPower; }
            set { fresnelPower = value; }
        } // FresnelPower

        /// <summary>
        /// Last used fresnel power
        /// </summary>
        private static float? lastUsedFresnelPower = null;
        /// <summary>
        /// Set surface's fresnel power (greater or equal to 0)
        /// </summary>
        private void SetFresnelPower(float _fresnelPower)
        {
            if (lastUsedFresnelPower != _fresnelPower && _fresnelPower >= 0.0f)
            {
                lastUsedFresnelPower = _fresnelPower;
                epFresnelPower.SetValue(_fresnelPower);
            } // if
        } // SetFresnelPower
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Reflection
        /// </summary>
        private float reflection = 0.8f;

        /// <summary>
        /// Reflection
        /// </summary>
        public float Reflection
        {
            get { return reflection; }
            set { reflection = value; }
        } // Reflection

        /// <summary>
        /// Last used reflection
        /// </summary>
        private static float ?lastUsedReflection = null;
        /// <summary>
        /// Set surface's reflection (value between 0 and 1)
        /// </summary>
        private void SetReflection(float _reflection)
        {
            if (lastUsedReflection != _reflection && _reflection >= 0.0f && _reflection <= 1.0f)
            {
                lastUsedReflection = _reflection;
                epReflection.SetValue(_reflection);
            } // if
        } // SetReflection

        #endregion

        #region Colors

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
                epSpecularColor.SetValue(new Vector3(_specularColor.R / 255f, _specularColor.G / 255f, _specularColor.B / 255f));
            } // if
        } // SetSpecularColor
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Edge Color
        /// </summary>
        private Color edgeColor = Color.Gray;
        /// <summary>
        /// Edge Color
        /// </summary>
        public Color EdgeColor
        {
            get { return edgeColor; }
            set { edgeColor = value; }
        }

        /// <summary>
        /// Last used edge color
        /// </summary>
        private static Color? lastUsedEdgeColor = null;
        /// <summary>
        /// Set edge color
        /// </summary>
        private void SetEdgeColor(Color _edgeColor)
        {
            if (lastUsedEdgeColor != _edgeColor)
            {
                lastUsedEdgeColor = _edgeColor;
                epEdgeColor.SetValue(new Vector3(_edgeColor.R / 255f, _edgeColor.G / 255f, _edgeColor.B / 255f));
            } // if
        } // SetEdgeColor
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Middle Color
        /// </summary>
        private Color middleColor = Color.Gray;
        /// <summary>
        /// Middle Color
        /// </summary>
        public Color MiddleColor
        {
            get { return middleColor; }
            set { middleColor = value; }
        }

        /// <summary>
        /// Last used middle color
        /// </summary>
        private static Color? lastUsedMiddleColor = null;
        /// <summary>
        /// Set middle color
        /// </summary>
        private void SetMiddleColor(Color _middleColor)
        {
            if (lastUsedMiddleColor != _middleColor)
            {
                lastUsedMiddleColor = _middleColor;
                epMiddleColor.SetValue(new Vector3(_middleColor.R / 255f, _middleColor.G / 255f, _middleColor.B / 255f));
            } // if
        } // SetMiddleColor

        #endregion

        #region Textures

        /// <summary>
        /// Reflection texture (cube map)
        /// </summary>
        private TextureCube reflectionTexture = null;

        /// <summary>
        /// Reflection texture (cube map)
        /// </summary>
        public void ReflectionTexture(string _reflectionTexture)
        {
            string fullFilename = Directories.TexturesDirectory + "\\" + _reflectionTexture;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load texture: File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                if (EngineManager.UsesSystemContent)
                    reflectionTexture = EngineManager.SystemContent.Load<TextureCube>(fullFilename);
                else
                    reflectionTexture = EngineManager.CurrentContent.Load<TextureCube>(fullFilename);
            } // try
            catch (Exception)
            {
                throw new Exception("Failed to load the cube texture");
            }
        } // ReflectionTexture

        /// <summary>
        /// Last used reflection texture
        /// </summary>
        private static TextureCube lastUsedReflectionTexture = null;
        /// <summary>
        /// Set reflection texture
        /// </summary>
        private void SetReflectionTexture(TextureCube _reflectionTexture)
        {
            if (_reflectionTexture != null && lastUsedReflectionTexture != _reflectionTexture)
            {
                lastUsedReflectionTexture = _reflectionTexture;
                EPcubeMapTexture.SetValue(_reflectionTexture);
            }
        } // SetReflectionTexture
                
        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Car paint material.
        /// </summary>
        /// <param name="reflectionTexture">A cube texture for reflections</param>
        public CarPaint(string _reflectionTexture = "Skybox-GrimNight")
		{
            Effect = LoadShader("CarPaint");

            GetParametersHandles();

            ReflectionTexture(_reflectionTexture);
            
            LoadUITestElements();
        } // CarPaint

		#endregion
        
		#region Get Parameters Handles
		
        /// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected override void GetParametersHandles()
		{
			try
			{
                // Matrices //
                GetCommonParametersHandles();
                // Lights //
                GetAmbientLightParametersHandles();
                GetPointLight1ParametersHandles();
                GetPointLight2ParametersHandles();
                GetPointLight3ParametersHandles();
                // Textures //
                EPcubeMapTexture = Effect.Parameters["CubeEnvMap"];
                // Surface //
                epSpecularColor = Effect.Parameters["specularColor"];
                epShininess = Effect.Parameters["shininess"];
                epEdgeColor = Effect.Parameters["paintedge"];
                epMiddleColor = Effect.Parameters["paintmiddle"];
                epFresnelBias = Effect.Parameters["fresnelbias"];
                epFresnelPower = Effect.Parameters["fresnelpower"];
                epReflection = Effect.Parameters["reflection"];
			} // try
			catch
			{
                throw new Exception("Get the handles from the car paint material failed.");
			} // catch
		} // GetParametersHandles

		#endregion

        #region Set Car Paint Attributes

        /// <summary>
        /// Set to the shader the specific atributes of this material.
        /// </summary>
        private void SetCarPaintParameters()
        {
            SetSpecularColor(SpecularColor);
            SetShininess(Shininess);
            SetEdgeColor(EdgeColor);
            SetMiddleColor(MiddleColor);
            SetFresnelBias(FresnelBias);
            SetFresnelPower(FresnelPower);
            SetReflection(Reflection);
            SetReflectionTexture(reflectionTexture);
        } // SetCarPaintParameters

        #endregion

        #region Render

        /// <summary>
        /// Render this shader/material; to do this job it takes an object model, its associated lights, and its matrices.
		/// </summary>		
        internal override void Render(Matrix worldMatrix, PointLight[] pointLight, DirectionalLight[] directionalLight, SpotLight[] spotLight, Model model)
        {

            #region Set Parameters

            try
            {
                if (pointLight != null)
                {
                    if (pointLight.Length == 1)
                    {
                        SetPointLight1Parameters(pointLight[0]);
                        Effect.CurrentTechnique = Effect.Techniques["CarPaint1PointLight"];
                    }
                    else
                    {
                        SetPointLight1Parameters(pointLight[0]);
                        SetPointLight2Parameters(pointLight[1]);
                        if (pointLight.Length == 3)
                            SetPointLight3Parameters(pointLight[2]);
                        Effect.CurrentTechnique = Effect.Techniques["CarPaint3PointLight"];
                    }
                }
                else
                {
                    SetPointLight1Parameters(null);
                    Effect.CurrentTechnique = Effect.Techniques["CarPaint1PointLight"];
                }
                SetCarPaintParameters();
                SetCommomParameters(worldMatrix);
                SetAmbientLightParameters();
            }
            catch
            {
                throw new Exception("Unable to set the car paint parameters.");
            }
            #endregion

            base.Render(model);
        } // Render

		#endregion        

        #region Test

        #region Variables

        private UISliderNumeric uiAlphaBlending,
                                uiShininess,
                                uiFresnelBias,
                                uiFresnelPower,
                                uiReflection;
        private UISliderColor uiSurfaceColor,
                                uiSpecularColor,
                                uiEdgeColor,
                                uiMiddleColor;
        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        protected override void LoadUITestElements()
        {
            uiShininess = new UISliderNumeric("Shininess", new Vector2(EngineManager.Width - 390, 110), 0.0f, 500f, 0.5f, Shininess);
            uiFresnelBias = new UISliderNumeric("Fresnel Bias", new Vector2(EngineManager.Width - 390, 150), 0.0f, 5.0f, 0.05f, FresnelBias);
            uiFresnelPower = new UISliderNumeric("Fresnel Power", new Vector2(EngineManager.Width - 390, 190), 0f, 5f, 0.01f, FresnelPower);
            uiReflection = new UISliderNumeric("Reflection", new Vector2(EngineManager.Width - 390, 230), 0f, 1f, 0.01f, Reflection);
            uiSurfaceColor = new UISliderColor("Surface Color", new Vector2(EngineManager.Width - 390, 270), SurfaceColor);
            uiSpecularColor = new UISliderColor("Specular Color", new Vector2(EngineManager.Width - 390, 310), SpecularColor);
            uiEdgeColor = new UISliderColor("Edge Color", new Vector2(EngineManager.Width - 390, 350), EdgeColor);
            uiMiddleColor = new UISliderColor("Middle Color", new Vector2(EngineManager.Width - 390, 390), MiddleColor);
            uiAlphaBlending = new UISliderNumeric("Alpha Blending", new Vector2(EngineManager.Width - 390, 430), 0.0f, 1f, 0.01f, AlphaBlending);
        } // LoadUITestElements

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public override void Test()
        {

            #region Reset Parameters

            // If the parameters were modified is better to have the new values. 
            uiShininess.CurrentValue = Shininess;
            uiFresnelBias.CurrentValue = FresnelBias;
            uiFresnelPower.CurrentValue = FresnelPower;
            uiReflection.CurrentValue = Reflection;
            uiSurfaceColor.CurrentValue = SurfaceColor;
            uiSpecularColor.CurrentValue = SpecularColor;
            uiEdgeColor.CurrentValue = EdgeColor;
            uiMiddleColor.CurrentValue = MiddleColor;
            uiAlphaBlending.CurrentValue = AlphaBlending;

            #endregion

            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Car-Paint Material Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiAlphaBlending.UpdateAndRender();
            uiShininess.UpdateAndRender();
            uiFresnelBias.UpdateAndRender();
            uiFresnelPower.UpdateAndRender();
            uiReflection.UpdateAndRender();
            uiSurfaceColor.UpdateAndRender();
            uiSpecularColor.UpdateAndRender();
            uiEdgeColor.UpdateAndRender();
            uiMiddleColor.UpdateAndRender();

            AlphaBlending = uiAlphaBlending.CurrentValue;
            Shininess = uiShininess.CurrentValue;
            FresnelBias = uiFresnelBias.CurrentValue;
            FresnelPower = uiFresnelPower.CurrentValue;
            Reflection = uiReflection.CurrentValue;
            SurfaceColor = uiSurfaceColor.CurrentValue;
            SpecularColor = uiSpecularColor.CurrentValue;
            EdgeColor = uiEdgeColor.CurrentValue;
            MiddleColor = uiMiddleColor.CurrentValue;
        } // Test

        #endregion

        // Common code to other shaders //

        #region Common Atributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles for all material shaders.
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
        /// Last used transpose inverse world matrix.
        /// </summary>
        private static Matrix? lastUsedTransposeInverseWorldMatrix = null;
        /// <summary>
        /// Set transpose inverse world matrix.
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
        /// Last used world matrix.
        /// </summary>
        private static Matrix? lastUsedWorldMatrix = null;
        /// <summary>
        /// Set world matrix.
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
        /// Last used inverse view matrix.
        /// </summary>
        private static Matrix? lastUsedInverseViewMatrix = null;
        /// <summary>
        /// Set view inverse matrix.
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
        /// Last used world view projection matrix.
        /// </summary>
        private static Matrix? lastUsedWorldViewProjMatrix = null;
        /// <summary>
        /// Set world view projection matrix.
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
        /// Last used view projection matrix.
        /// </summary>
        private static Matrix? lastUsedViewProjMatrix = null;
        /// <summary>
        /// Set view projection matrix.
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
        /// Surface color.
        /// </summary>
        private Color surfaceColor = Color.White;

        /// <summary>
        /// Surface color.
        /// </summary>
        public Color SurfaceColor
        {
            get { return surfaceColor; }
            set { surfaceColor = value; }
        } // SurfaceColor

        /// <summary>
        /// Last used surface color.
        /// </summary>
        private static Color? lastUsedSurfaceColor = null;
        /// <summary>
        /// Set surface color.
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
        /// Alpha blending.
        /// </summary>
        private float alphaBlending = 1.0f;

        /// <summary>
        /// Alpha blending.
        /// </summary>
        public float AlphaBlending
        {
            get { return alphaBlending; }
            set { alphaBlending = value; }
        } // AlphaBlending

        /// <summary>
        /// Last used alpha blending.
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
        /// Get the common handlers from the shader.
        /// </summary>
        protected void GetCommonParametersHandles()
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
        } // GetCommonParametersHandles

        /// <summary>
        /// Set to the shader the common atributes of this material.
        /// </summary>
        protected virtual void SetCommomParameters(Matrix worldMatrix)
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
        } // SetCommomParameters

        #endregion

        #region Ambient Light Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epAmbientLightColor;

        /// <summary>
        /// Last used ambient light color.
        /// </summary>
        private static Color? lastUsedAmbientLightColor = null;
        /// <summary>
        /// Set ambient light color.
        /// </summary>
        protected void SetAmbientLightColor(Color _ambientLightColor)
        {
            if (lastUsedAmbientLightColor != _ambientLightColor)
            {
                lastUsedAmbientLightColor = _ambientLightColor;
                epAmbientLightColor.SetValue(new Vector3(_ambientLightColor.R / 255f, _ambientLightColor.G / 255f, _ambientLightColor.B / 255f));
            }
        } // SetAmbientLightColor

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetAmbientLightParametersHandles()
        {
            epAmbientLightColor = Effect.Parameters["AmbientLightColor"];
        } // GetAmbientLightParametersHandles

        /// <summary>
        /// Set the ambient light parameters to the shader.
        /// </summary>
        public virtual void SetAmbientLightParameters()
        {
            SetAmbientLightColor(AmbientLight.LightColor);
        } // SetAmbientLightParameters

        #endregion

        #region Point Light 1 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles.
        /// </summary>
        protected static EffectParameter epPointLight1Pos,
                                         epPointLight1Color;

        #region Color

        /// <summary>
        /// Last used point light 1 color.
        /// </summary>
        private static Color? lastUsedPointLight1Color = null;
        /// <summary>
        /// Set point light 1 color.
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
        /// Last used point light 1 position.
        /// </summary>
        private static Vector3? lastUsedPointLight1Pos = null;
        /// <summary>
        /// Set point light 1 position.
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
        protected void GetPointLight1ParametersHandles()
        {
            epPointLight1Color = Effect.Parameters["PointLightColor"];
            epPointLight1Pos = Effect.Parameters["PointLightPos"];
        } // GetPointLight1ParametersHandles

        /// <summary>
        /// Set the point light parameters to the shader.
        /// </summary>
        public virtual void SetPointLight1Parameters(PointLight pointLight)
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
        } // SetPointLight1Parameters

        #endregion

        #region Point Light 2 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epPointLight2Pos,
                                         epPointLight2Color;

        #region Color

        /// <summary>
        /// Last used point light 2 color.
        /// </summary>
        private static Color? lastUsedPointLight2Color = null;
        /// <summary>
        /// Set point light 2 color.
        /// </summary>
        protected void SetPointLight2Color(Color _pointLight2Color)
        {
            if (lastUsedPointLight2Color != _pointLight2Color)
            {
                lastUsedPointLight2Color = _pointLight2Color;
                epPointLight2Color.SetValue(new Vector3(_pointLight2Color.R / 255f, _pointLight2Color.G / 255f, _pointLight2Color.B / 255f));
            }
        } // SetPointLight2Color

        #endregion

        #region Position

        /// <summary>
        /// Last used point light 2 position.
        /// </summary>
        private static Vector3? lastUsedPointLight2Pos = null;
        /// <summary>
        /// Set point light 2 position.
        /// </summary>
        protected void SetPointLight2Pos(Vector3 _pointLight2Pos)
        {
            if (lastUsedPointLight2Pos != _pointLight2Pos)
            {
                lastUsedPointLight2Pos = _pointLight2Pos;
                epPointLight2Pos.SetValue(new Vector3(_pointLight2Pos.X, _pointLight2Pos.Y, _pointLight2Pos.Z));
            } // if
        } // SetPointLight2Pos

        #endregion

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetPointLight2ParametersHandles()
        {
            epPointLight2Color = Effect.Parameters["PointLightColor2"];
            epPointLight2Pos = Effect.Parameters["PointLightPos2"];
        } // GetPointLight2ParametersHandles

        /// <summary>
        /// Set the point light parameters to the shader.
        /// </summary>
        public virtual void SetPointLight2Parameters(PointLight pointLight)
        {
            if (pointLight != null)
            {
                SetPointLight2Color(pointLight.Color);
                SetPointLight2Pos(pointLight.Position);
            }
            else
            {
                SetPointLight2Color(Color.Black);
            }
        } // SetPointLight2Parameters

        #endregion

        #region Point Light 3 Attributes

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected static EffectParameter epPointLight3Pos,
                                         epPointLight3Color;

        #region Color

        /// <summary>
        /// Last used point light 3 color.
        /// </summary>
        private static Color? lastUsedPointLight3Color = null;
        /// <summary>
        /// Set point light 3 color.
        /// </summary>
        protected void SetPointLight3Color(Color _pointLight3Color)
        {
            if (lastUsedPointLight3Color != _pointLight3Color)
            {
                lastUsedPointLight3Color = _pointLight3Color;
                epPointLight3Color.SetValue(new Vector3(_pointLight3Color.R / 255f, _pointLight3Color.G / 255f, _pointLight3Color.B / 255f));
            }
        } // SetPointLight3Color

        #endregion

        #region Position

        /// <summary>
        /// Last used point light 3 position.
        /// </summary>
        private static Vector3? lastUsedPointLight3Pos = null;
        /// <summary>
        /// Set point light 3 position.
        /// </summary>
        protected void SetPointLight3Pos(Vector3 _pointLight3Pos)
        {
            if (lastUsedPointLight3Pos != _pointLight3Pos)
            {
                lastUsedPointLight3Pos = _pointLight3Pos;
                epPointLight3Pos.SetValue(new Vector3(_pointLight3Pos.X, _pointLight3Pos.Y, _pointLight3Pos.Z));
            } // if
        } // SetPointLight3Pos

        #endregion

        #endregion

        /// <summary>
        /// Get the handle of the parameters from the shader.
        /// </summary>
        protected void GetPointLight3ParametersHandles()
        {
            epPointLight3Color = Effect.Parameters["PointLightColor3"];
            epPointLight3Pos = Effect.Parameters["PointLightPos3"];
        } // GetPointLight3ParametersHandles

        /// <summary>
        /// Set the point light parameters to the shader.
        /// </summary>
        public virtual void SetPointLight3Parameters(PointLight pointLight)
        {
            if (pointLight != null)
            {
                SetPointLight3Color(pointLight.Color);
                SetPointLight3Pos(pointLight.Position);
            }
            else
            {
                SetPointLight3Color(Color.Black);
            }
        } // SetPointLight3Parameters

        #endregion
        
    } // CarPaint
} // XNAFinalEngine.GraphicElements

