
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
#endregion

namespace XNAFinalEngine.GraphicElements
{
	/// <summary>
	/// Shadow map shader.
	/// </summary>
    public class ShadowMapShader : ScreenShader
	{

		#region Variables

		/// <summary>
		/// Light depth buffer.
		/// </summary>
		private RenderToTexture lightDepthBufferTexture = null;
             
        /// <summary>
        /// Auxiliary texture.
        /// </summary>
        private Texture ShadowDistanceFadeoutTexture = null;

        /// <summary>
        /// Texel width and height and offset for texScaleBiasMatrix, this way we can directly access the middle of each texel.
        /// </summary>
        private float texelWidth,
                      texelHeight,
                      texOffsetX,
                      texOffsetY;

        /// <summary>
        /// The matrix to convert projection screen coordinates in the -1..1 range to the shadow depth map texture coordinates.
        /// </summary>
        private Matrix texScaleBiasMatrix;

        #endregion

        #region Properties

        /// <summary>
        /// Shadow map texture.
        /// </summary>
        public RenderToTexture ShadowMapTexture { get; private set; }

        #endregion

        #region Shader Parameters

        #region Handles

        /// <summary>
        /// Effect handles
        /// </summary>
        private EffectParameter
                                // Matrices
                                epWorldViewProj,
                                // Light
                                epShadowTexTransform,
                                epWorldViewProjLight,
                                // Other Parameters
                                epFarPlane,
                                epDepthBias,
                                epShadowMapDepthBias,
                                epShadowMap,
                                epShadowColor,
                                epShadowMapTexelSize,
                                epShadowDistanceFadeoutTexture;

        #endregion

        #region Matrices

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
        private static float? lastUsedFarPlane = null;
        /// <summary>
        /// Set Far Plane (value between 1 and 100000)
        /// </summary>
        private void SetFarPlane(float _farPlane)
        {
            if (lastUsedFarPlane != _farPlane && _farPlane >= 1.0f && _farPlane <= 100000.0f)
            {
                lastUsedFarPlane = _farPlane;
                epFarPlane.SetValue(_farPlane);
            }
        } // SetFarPlane

        #endregion

        #region Near Plane

        /// <summary>
        /// Near Plane
        /// </summary>
        private float nearPlane = 1.0f;

        /// <summary>
        /// Near Plane
        /// </summary>
        public float NearPlane
        {
            get { return nearPlane; }
            set { nearPlane = value; }
        } // NearPlane

        #endregion

        #region Depth Bias

        /// <summary>
        /// Depth Bias
        /// </summary>
        private float depthBias = 0.0025f;

        /// <summary>
        /// Depth Bias
        /// </summary>
        public float DepthBias
        {
            get { return depthBias; }
            set { depthBias = value; }
        } // DepthBias

        /// <summary>
        /// Last used depth bias
        /// </summary>
        private static float? lastUsedDepthBias = null;
        /// <summary>
        /// Set depth bias (value between 0 and 0.1)
        /// </summary>
        private void SetDepthBias(float _depthBias)
        {
            if (lastUsedDepthBias != _depthBias && _depthBias >= 0.0f && _depthBias <= 0.1f)
            {
                lastUsedDepthBias = _depthBias;
                epDepthBias.SetValue(_depthBias);
            }
        } // SetDepthBias

        #endregion

        #region Shadow Map Color

        /// <summary>
        /// Shadow Color
        /// </summary>
        private Color shadowColor = Color.Black;
        /// <summary>
        /// Shadow color
        /// </summary>
        public Color ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        /// <summary>
        /// Last used shadow color
        /// </summary>
        private static Color? lastUsedShadowColor = null;
        /// <summary>
        /// Set shadow color
        /// </summary>
        protected void SetShadowColor(Color _shadowColor)
        {
            if (lastUsedShadowColor != _shadowColor)
            {
                lastUsedShadowColor = _shadowColor;
                epShadowColor.SetValue(new Vector4(_shadowColor.R / 255f, _shadowColor.G / 255f, _shadowColor.B / 255f, 1));
            }
        } // SetShadowColor

        #endregion

        #region Shadow Map Depth Bias

        /// <summary>
        /// Shadow Map Depth Bias
        /// </summary>
        private float shadowMapDepthBias = 0.0017f;

        /// <summary>
        /// Shadow Map Depth Bias
        /// </summary>
        public float ShadowMapDepthBias
        {
            get { return shadowMapDepthBias; }
            set { shadowMapDepthBias = value; }
        } // ShadowMapDepthBias

        /// <summary>
        /// Last used shadow map depth bias
        /// </summary>
        private static float? lastUsedShadowMapDepthBias = null;
        /// <summary>
        /// Set Shadow Map Depth Bias (value between -0.1 and 0.0)
        /// </summary>
        private void SetShadowMapDepthBias(float _shadowMapDepthBias)
        {
            if (lastUsedShadowMapDepthBias != _shadowMapDepthBias && _shadowMapDepthBias >= -0.1f && _shadowMapDepthBias <= 0.1f)
            {
                lastUsedShadowMapDepthBias = _shadowMapDepthBias;
                epDepthBias.SetValue(_shadowMapDepthBias);
            }
        } // SetShadowMapDepthBias

        #endregion

        #region Shadow Map Texel Size

        /// <summary>
        /// Last used shadow map texel size
        /// </summary>
        private static Vector2? lastUsedShadowMapTexelSize = null;
        /// <summary>
        /// Set Shadow Map texel size
        /// </summary>
        private void SetShadowMapTexelSize(Vector2 _shadowMapTexelSize)
        {
            if (lastUsedShadowMapTexelSize != _shadowMapTexelSize)
            {
                lastUsedShadowMapTexelSize = _shadowMapTexelSize;
                epShadowMapTexelSize.SetValue(_shadowMapTexelSize);
            }
        } // SetShadowMapDepthBias

        #endregion

        #region Light

        #region Virtual Light Distance

        /// <summary>
        /// Virtual Light Distance. Used to create a point light position for the directional light.
        /// </summary>
        private float virtualLightDistance = 50;

        /// <summary>
        /// Virtual Light Distance. Used to create a point light position for the directional light.
        /// </summary>
        public float VirtualLightDistance
        {
            get { return virtualLightDistance; }
            set { virtualLightDistance = value; }
        } // VirtualLightDistance

        #endregion
        
        #region Matrices

        /// <summary>
        /// Used matrices for the light casting the shadows.
        /// </summary>
        private Matrix lightProjectionMatrix, lightViewMatrix;

        /// <summary>
        /// Last used world view projection light matrix
        /// </summary>
        private static Matrix? lastUsedWorldViewProjLightMatrix = null;
        /// <summary>
        /// Set world view projection light matrix
        /// </summary>
        private Matrix WorldViewProjLightMatrix
        {
            set
            {
                if (lastUsedWorldViewProjLightMatrix != value)
                {
                    lastUsedWorldViewProjLightMatrix = value;
                    epWorldViewProjLight.SetValue(value);
                } // if
            } // set
        } // WorldViewProjLightMatrix

        /// <summary>
        /// Last used shadow tex transform matrix
        /// </summary>
        private static Matrix? lastUsedShadowTexTransformMatrix = null;
        /// <summary>
        /// Set shadow tex transform matrix
        /// </summary>
        private Matrix ShadowTexTransformMatrix
        {
            set
            {
                if (lastUsedShadowTexTransformMatrix != value)
                {
                    lastUsedShadowTexTransformMatrix = value;
                    epShadowTexTransform.SetValue(value);
                } // if
            } // set
        } // ShadowTexTransformMatrix

        #endregion

        #region Light Direction

        private Vector3 lightDirection = new Vector3(0, -1, 0);
        /// <summary>
        /// Light Direction. Only take action when there isn't a associated light. A set in this property erase the associated light.
        /// </summary>
        public Vector3 LightDirection
        {
            get { return lightDirection; }
            set { lightDirection = value; AssociatedLight = null; }
        } // LightDirection

        #endregion

        #region Light Look Position

        private Vector3 lightLookPosition = Vector3.Zero;
        /// <summary>
        /// Light Look Position. Only take action when there isn't a associated light. A set in this property erase the associated light.
        /// </summary>
        public Vector3 LightLookPosition
        {
            get {  return lightLookPosition; }
            set { lightLookPosition = value; AssociatedLight = null; }
        } // LightLookPosition

        #endregion

        /// <summary>
        /// The light that generates the shadow.
        /// </summary>
        public Light AssociatedLight { get; set; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Shadow map shader
        /// </summary>
        /// <param name="_lightRendeTargetSize">The size of the render target used to calculted light depth buffer</param>
        /// <param name="_ShadowrendeTargetSize">The final render target size</param>
        public ShadowMapShader(RenderToTexture.SizeType _lightRendeTargetSize = RenderToTexture.SizeType.Custom512x512,
                               RenderToTexture.SizeType _ShadowrendeTargetSize = RenderToTexture.SizeType.HalfScreen)
		{
            Effect = LoadShader("PostShadowMap");
            GetParameters();
			
			// Creates the render target textures
            lightDepthBufferTexture = new RenderToTexture(RenderToTexture.SizeType.Custom512x512, true);
            ShadowMapTexture = new RenderToTexture(RenderToTexture.SizeType.FullScreen);

            // Set some parameters automatically
            CalculateShadowMapBiasMatrix();
            ShadowDistanceFadeoutTexture = new Texture("ScreenBorderFadeout");
            epShadowDistanceFadeoutTexture.SetValue(ShadowDistanceFadeoutTexture.XnaTexture);

            NearPlane = ApplicationLogic.Camera.NearPlane;
            FarPlane = ApplicationLogic.Camera.FarPlane;

            LoadUITestElements();
		} // ShadowMapShader

		#endregion

        #region Calculate shadow map bias matrix

        /// <summary>
        /// Calculate the texScaleBiasMatrix for converting projection screen coordinates in the -1..1 range to the shadow depth map texture coordinates.
        /// </summary>
        private void CalculateShadowMapBiasMatrix()
        {
            float texExtraScale = 1.02f;

            texelWidth  = 1.0f / (float)lightDepthBufferTexture.Width;
            texelHeight = 1.0f / (float)lightDepthBufferTexture.Height;
            texOffsetX  = 0.5f + (0.5f / (float)lightDepthBufferTexture.Width);
            texOffsetY  = 0.5f + (0.5f / (float)lightDepthBufferTexture.Height);

            texScaleBiasMatrix = new Matrix(0.5f * texExtraScale, 0.0f, 0.0f, 0.0f,
                                            0.0f, -0.5f * texExtraScale, 0.0f, 0.0f,
                                            0.0f, 0.0f, texExtraScale, 0.0f,
                                            texOffsetX, texOffsetY, 0.0f, 1.0f);
        } // CalcShadowMapBiasMatrix
        
        #endregion

		#region Get parameters

		/// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected void GetParameters()
		{
            try
            {
                // Matrices
                epWorldViewProj =                Effect.Parameters["worldViewProj"];

                // Lights
                epShadowTexTransform =           Effect.Parameters["shadowTexTransform"];
                epWorldViewProjLight =           Effect.Parameters["worldViewProjLight"];

			    // Get additional parameters
                epShadowColor =                  Effect.Parameters["shadowColor"];
			    epFarPlane =                     Effect.Parameters["farPlane"];
                epDepthBias =                    Effect.Parameters["depthBias"];
                epShadowMapDepthBias =           Effect.Parameters["shadowMapDepthBias"];
			    epShadowMap =                    Effect.Parameters["shadowMap"];
                epShadowMapTexelSize =           Effect.Parameters["shadowMapTexelSize"];
                epShadowDistanceFadeoutTexture = Effect.Parameters["shadowDistanceFadeoutTexture"];
            }
            catch (Exception ex)
            {
                throw new Exception("Get the handles from the shadow map shader failed. " + ex.ToString());
            }
		} // GetParameters

		#endregion

        #region SetAtributes

        /// <summary>
        /// Set to the shader the specific atributes of this effect.
        /// </summary>
        private void SetParameters()
        {
            // Additional parameters
            SetFarPlane(FarPlane);
            SetDepthBias(depthBias);
            SetShadowMapDepthBias(shadowMapDepthBias);
            SetShadowMapTexelSize(new Vector2(texelWidth, texelHeight));
            SetShadowColor(shadowColor);
        } // SetParameters

        /// <summary>
        /// Set the worldViewProjection matrix for the first pass of the shader (the light depth buffer calculation).
        /// </summary>
        private void SetLightAndObjectParametersGenerateLightDepthBuffer(Matrix worldMatrix)
        {   
            WorldViewProjMatrix = worldMatrix * lightViewMatrix * lightProjectionMatrix;
            Effect.CurrentTechnique.Passes[0].Apply(); // Commit Changes
        } // SetLightAndObjectParameters

        /// <summary>
        /// Set light and object paremeters for the last pass.
        /// </summary>
        private void SetLightAndObjectParametersGenerateShadowMap(Matrix worldMatrix)
        {
            WorldViewProjMatrix = worldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix;
            // Compute the matrix to transform from view space to light proj:
            // inverse of view matrix * light view matrix * light proj matrix
            ShadowTexTransformMatrix = worldMatrix * lightViewMatrix * lightProjectionMatrix * texScaleBiasMatrix;

            WorldViewProjLightMatrix = worldMatrix * lightViewMatrix * lightProjectionMatrix;
            Effect.CurrentTechnique.Passes[0].Apply(); // Commit Changes
        } // UpdateCalcShadowWorldMatrix

		#endregion

        #region Calculate Light Matrices

        /// <summary>
		/// Calculate light matrices
		/// </summary>
		private void CalculateLightMatrices()
		{
            float virtualFieldOfView = (float)Math.PI / 4.0f;

			// Set projection matrix for light
            lightProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(virtualFieldOfView, // Don't use graphics fov and aspect ratio in directional lighting mode
				                                                        1.0f,
				                                                        NearPlane,
				                                                        FarPlane);

			// Set view matrix for light
            if (AssociatedLight == null)
            {
                lightViewMatrix = Matrix.CreateLookAt(LightLookPosition - LightDirection * virtualLightDistance, LightLookPosition, new Vector3(0, 1, 0));
            }
            else
            {
                if (AssociatedLight is DirectionalLight)
                {
                    lightViewMatrix = Matrix.CreateLookAt(ApplicationLogic.Camera.LookAtPosition - ((DirectionalLight)AssociatedLight).Direction * virtualLightDistance,
                                                          ApplicationLogic.Camera.LookAtPosition, new Vector3(0, 1, 0));
                }
                else if (AssociatedLight is SpotLight)
                {
                    lightViewMatrix = Matrix.CreateLookAt(((SpotLight)AssociatedLight).Position,
                                                          ((SpotLight)AssociatedLight).Position + ((SpotLight)AssociatedLight).Direction, new Vector3(0, 1, 0));
                }
                else if (AssociatedLight is PointLight)
                {
                    lightViewMatrix = Matrix.CreateLookAt(((PointLight)AssociatedLight).Position,
                                                          ApplicationLogic.Camera.LookAtPosition, new Vector3(0, 1, 0));
                }
            }
		} // CalculateLightMatrices

		#endregion

        #region Generate Light Depth Buffer

        /// <summary>
        /// Render the object for the light depth buffer pass.
        /// </summary>
        private void RenderObjectsGenerateLightDepthBuffer(Object renderObjects)
        {
            if (renderObjects is GraphicObject)
            {
                // Set parameters
                SetLightAndObjectParametersGenerateLightDepthBuffer(((GraphicObject)renderObjects).WorldMatrix);
                // Render
                ((GraphicObject)renderObjects).Model.Render();
            }
            else // if is a container object
            {   
                foreach (GraphicObject graphicObj in ((ContainerObject)renderObjects).GraphicObjectsChildren)
                {
                    RenderObjectsGenerateLightDepthBuffer(graphicObj);
                }
                foreach (ContainerObject containerObject in ((ContainerObject)renderObjects).ContainerObjectsChildren)
                {
                    RenderObjectsGenerateLightDepthBuffer(containerObject);
                }
            }
        } // RenderObjectsGenerateLightDepthBuffer

		/// <summary>
        /// Generate Light Depth Buffer.
		/// </summary>
        public void GenerateLightDepthBuffer(Object renderObjects)
		{
            SetParameters();
            CalculateLightMatrices();
                        
			// Start rendering onto the shadow map            
			lightDepthBufferTexture.EnableRenderTarget();

			// Clear render target
			lightDepthBufferTexture.Clear(Color.White);
            
			Effect.CurrentTechnique = Effect.Techniques["GenerateShadowMap"];
                        
            // Start effect (current technique should be set)
            try
            {
                Effect.CurrentTechnique.Passes[0].Apply();

                // Render
                RenderObjectsGenerateLightDepthBuffer(renderObjects);

            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the Shadow Map effect. First pass. " + e.Message);
            }
            
			// Resolve the render target to get the texture (required for Xbox)
			lightDepthBufferTexture.DisableRenderTarget();
		} // GenerateLightDepthBuffer

		#endregion

        #region Generate Shadows

        /// <summary>
        /// Render the object for the last buffer pass.
        /// </summary>
        private void RenderObjectsGenerateShadowMap(Object renderObjects)
        {
            if (renderObjects is GraphicObject)
            {
                // Set parameters
                SetLightAndObjectParametersGenerateShadowMap(((GraphicObject)renderObjects).WorldMatrix);
                // Render
                ((GraphicObject)renderObjects).Model.Render();
            }
            else // If is a container object
            {
                foreach (GraphicObject graphicObj in ((ContainerObject)renderObjects).GraphicObjectsChildren)
                {
                    RenderObjectsGenerateShadowMap(graphicObj);
                }
                foreach (ContainerObject containerObject in ((ContainerObject)renderObjects).ContainerObjectsChildren)
                {
                    RenderObjectsGenerateShadowMap(containerObject);
                }
            }
        } // RenderObjectsGenerateShadowMap

		/// <summary>
        ///  Generate final shadows.
		/// </summary>
        public void GenerateShadows(Object renderObjects)
		{
            SetParameters();

            // Start rendering onto the shadow map
            ShadowMapTexture.EnableRenderTarget();

            // Clear render target
            ShadowMapTexture.Clear(Color.White);
            
            Effect.CurrentTechnique = Effect.Techniques["UseShadowMap"];
            
            // Use the shadow map texture here which was generated in  GenerateShadows().
            epShadowMap.SetValue(lightDepthBufferTexture.XnaTexture);
            
            // Start effect (current technique should be set)
            try
            {
                Effect.CurrentTechnique.Passes[0].Apply();

                // Render
                RenderObjectsGenerateShadowMap(renderObjects);

            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the shadow map effect. Final pass. " + e.Message);
            }
            
            // Resolve the render target to get the texture (required for Xbox)
            ShadowMapTexture.DisableRenderTarget();

		} // GenerateShadows

		#endregion

        #region Test

        #region Variables

        private UISliderNumeric uiDepthBias,
                                uiShadowMapDepthBias,
                                uiVirtualLightDistance,
                                uiNearPlane,
                                uiFarPlane;
        private UISliderColor   uiShadowMapColor;

        #endregion

        /// <summary>
        /// Load the different elements of the user interface needed to modify the shader.
        /// </summary>
        public void LoadUITestElements()
        {
            uiDepthBias = new UISliderNumeric("Depth Bias",                        new Vector2(EngineManager.Width - 390, 110), 0.0f, 0.01f, 0.0001f, DepthBias);
            uiShadowMapDepthBias = new UISliderNumeric("ShadowMap Depth Bias",     new Vector2(EngineManager.Width - 390, 150), -0.1f, 0.1f, 0.0001f, ShadowMapDepthBias);
            uiVirtualLightDistance = new UISliderNumeric("Virtual Light Distance", new Vector2(EngineManager.Width - 390, 190), 1f, 500f, 0.5f, VirtualLightDistance);
            uiShadowMapColor = new UISliderColor("Shadow Color",                   new Vector2(EngineManager.Width - 390, 230), ShadowColor);
            uiNearPlane = new UISliderNumeric("Near Plane",                        new Vector2(EngineManager.Width - 390, 270), 0.05f, 10, 0.05f, NearPlane);
            uiFarPlane = new UISliderNumeric("Far Plane",                          new Vector2(EngineManager.Width - 390, 310), 15, 1000, 5, FarPlane);
        } // LoadUITestElements

        /// <summary>
        /// It allows modifying the shader/material in real time with the help of the engine UI for testing purposes.
        /// </summary>
        public void Test()
        {

            #region Reset Parameters

            // Si los parametros se han modificado es mejor tener los nuevos valores
            uiDepthBias.CurrentValue = DepthBias;
            uiShadowMapDepthBias.CurrentValue = ShadowMapDepthBias;
            uiVirtualLightDistance.CurrentValue = VirtualLightDistance;
            uiShadowMapColor.CurrentValue = ShadowColor;
            uiNearPlane.CurrentValue = NearPlane;
            uiFarPlane.CurrentValue = FarPlane;
            
            #endregion

            Primitives.Draw2DSolidPlane(new Rectangle(EngineManager.Width - 400, 0, 400, EngineManager.Height), new Color(0.0f, 0.0f, 0.0f, 1f), new Color(0.0f, 0.0f, 0.0f, 0.0f));
            FontArial14.Render("Shadow Map Parameters", new Vector2(EngineManager.Width - 380, 40), Color.White);
            uiDepthBias.UpdateAndRender();
            uiShadowMapDepthBias.UpdateAndRender();
            uiVirtualLightDistance.UpdateAndRender();
            uiShadowMapColor.UpdateAndRender();
            uiNearPlane.UpdateAndRender();
            uiFarPlane.UpdateAndRender();

            VirtualLightDistance = uiVirtualLightDistance.CurrentValue;
            DepthBias = uiDepthBias.CurrentValue;
            ShadowMapDepthBias = uiShadowMapDepthBias.CurrentValue;
            ShadowColor = uiShadowMapColor.CurrentValue;
            NearPlane = uiNearPlane.CurrentValue;
            FarPlane = uiFarPlane.CurrentValue;
        } // Test

        #endregion

    } // ShadowMapShader
} // XNAFinalEngine.GraphicElements
