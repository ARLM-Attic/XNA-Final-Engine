
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.Helpers;
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

	/// <summary>
	/// Basic shadow map.
	/// Only works with directional lights and spot lights.
	/// If performance is not an issue, use cascaded shadow maps for directional lights.
	/// If you need point light shadows use the cube shadow map.
	/// </summary>
    internal class BasicShadowMapShader : Shader
	{

		#region Variables
        
        // Light Matrices.
        private Matrix lightProjectionMatrix, lightViewMatrix;

        private RenderTarget shadowTexture;

        // Singleton reference.
        private static BasicShadowMapShader instance;

        // Shader Parameters.
        private static ShaderParameterFloat spDepthBias;
        private static ShaderParameterVector2 spHalfPixel, spShadowMapSize, spInvShadowMapSize;
        private static ShaderParameterVector3Array spFrustumCorners;
        private static ShaderParameterTexture spDepthTexture, spShadowMapTexture;
        private static ShaderParameterMatrix spViewToLightViewProjMatrix;

        // Techniques references.
        private static EffectTechnique renderShadowMap2x2PCFTechnique,
                                       renderShadowMap3x3PCFTechnique,
                                       renderShadowMap5x5PCFTechnique,
                                       renderShadowMap7x7PCFTechnique,
                                       renderShadowMapPoisonPCFTechnique;

        #endregion

        #region Properties

        /// <summary>
        /// Light Projection Matrix.
        /// </summary>
        public Matrix LightProjectionMatrix { get { return lightProjectionMatrix; } }

        /// <summary>
        /// Light Projection Matrix.
        /// </summary>
        public Matrix LightViewMatrix { get { return lightViewMatrix; } }

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static BasicShadowMapShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new BasicShadowMapShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Basic shadow map.
        /// Only works with directional lights and spot lights.
        /// If performance is not an issue, use cascaded shadow maps for directional lights.
        /// If you need point light shadows use the tetrahedron shadow map.
        /// </summary>
        private BasicShadowMapShader() : base("Shadows\\ShadowMap") { }

		#endregion

		#region Get parameters handles

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
                spDepthBias = new ShaderParameterFloat("depthBias", this);
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spShadowMapSize = new ShaderParameterVector2("shadowMapSize", this);
                spInvShadowMapSize = new ShaderParameterVector2("invShadowMapSize", this);
                spFrustumCorners = new ShaderParameterVector3Array("frustumCorners", this, 4);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
                spShadowMapTexture = new ShaderParameterTexture("shadowMap", this, SamplerState.PointClamp, 3);
                spViewToLightViewProjMatrix = new ShaderParameterMatrix("viewToLightViewProj", this);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

		#endregion

        #region Get Techniques Handles

        /// <summary>
        /// Get the handles of the techniques from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override void GetTechniquesHandles()
        {
            try
            {
                renderShadowMap2x2PCFTechnique = Resource.Techniques["RenderShadowMap2x2PCF"];
                renderShadowMap3x3PCFTechnique = Resource.Techniques["RenderShadowMap3x3PCF"];
                renderShadowMap5x5PCFTechnique = Resource.Techniques["RenderShadowMap5x5PCF"];
                renderShadowMap7x7PCFTechnique = Resource.Techniques["RenderShadowMap7x7PCF"];
                renderShadowMapPoisonPCFTechnique = Resource.Techniques["RenderShadowMapPoisonPCF"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

        #endregion

        #region Render

        /// <summary>
        /// The last pass of the shadow calculations.
        /// </summary>
        internal RenderTarget Render(Texture lightDepthTexture, RenderTarget depthTexture, float depthBias, Shadow.FilterType filterType)
        {
            try
            {
                // XBOX 360 Xbox does not support 16 bit render targets (http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/rendertarget-formats-in-xna-game-studio-4-0.aspx)
                // Color would be the better choice for the XBOX 360.
                // With color we have another good option, the possibility to gather four shadow results (local or global) in one texture.
                shadowTexture = RenderTarget.Fetch(depthTexture.Size, SurfaceFormat.HalfSingle, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set parameters.
                spShadowMapTexture.Value = lightDepthTexture;
                spHalfPixel.Value = new Vector2(-0.5f / (depthTexture.Width / 2), 0.5f / (depthTexture.Height / 2));
                spShadowMapSize.Value = new Vector2(lightDepthTexture.Width, lightDepthTexture.Height);
                spInvShadowMapSize.Value = new Vector2(1.0f / lightDepthTexture.Width, 1.0f / lightDepthTexture.Height);
                spDepthBias.Value = depthBias;
                spDepthTexture.Value = depthTexture;

                shadowTexture.EnableRenderTarget();
                shadowTexture.Clear(Color.White);

                switch (filterType)
                {
                    case Shadow.FilterType.Pcf2X2:
                        Resource.CurrentTechnique = renderShadowMap2x2PCFTechnique;
                        break;
                    case Shadow.FilterType.Pcf3X3:
                        Resource.CurrentTechnique = renderShadowMap3x3PCFTechnique;
                        break;
                    case Shadow.FilterType.Pcf5X5:
                        Resource.CurrentTechnique = renderShadowMap5x5PCFTechnique;
                        break;
                    case Shadow.FilterType.Pcf7X7:
                        Resource.CurrentTechnique = renderShadowMap7x7PCFTechnique;
                        break;
                    default:
                        Resource.CurrentTechnique = renderShadowMapPoisonPCFTechnique;
                        break;
                }

                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                shadowTexture.DisableRenderTarget();

                BilateralBlurShader.Instance.Filter(shadowTexture, shadowTexture, depthTexture, 10, 20);

                return shadowTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Basic Shadow Map Shader: Unable to render.", e);
            }
        } // Render

        #endregion
        
        #region Set Light

        // To avoid garbage use always the same values.
        private static readonly Vector3[] cornersWorldSpace = new Vector3[8];
        private static readonly Vector3[] frustumCornersLightSpace = new Vector3[8];
        private static readonly BoundingFrustum boundingFrustumTemp = new BoundingFrustum(Matrix.Identity);

        /// <summary>
		/// Calculate light matrices.
		/// </summary>
        internal void SetLight(Vector3 position, Vector3 direction, Matrix viewMatrix, float apertureCone, float range, Vector3[] boundingFrustum)
		{

            lightViewMatrix = Matrix.CreateLookAt(position, position + direction, Vector3.Up);
            lightProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(apertureCone * (float)Math.PI / 180.0f, // field of view
                                                                        1.0f,   // Aspect ratio
                                                                        1f,   // Near plane
                                                                        range); // Far plane
            spViewToLightViewProjMatrix.Value = Matrix.Invert(viewMatrix) * lightViewMatrix * lightProjectionMatrix;
            spFrustumCorners.Value = boundingFrustum;
		} // SetLight

        /// <summary>
        /// Determines the size of the frustum needed to cover the viewable area, then creates the light view matrix and an appropriate orthographic projection.
        /// </summary>
        internal void SetLight(Vector3 direction, Matrix viewMatrix, float range, Vector3[] boundingFrustum)
        {
            const float nearPlane = 1f;

            #region Far Frustum Corner in View Space

            boundingFrustumTemp.Matrix = viewMatrix * Matrix.CreatePerspectiveFieldOfView(3.1416f * 45 / 180.0f, 1, nearPlane, range);
            boundingFrustumTemp.GetCorners(cornersWorldSpace);
            Vector3 frustumCornersViewSpace4 = Vector3.Transform(cornersWorldSpace[4], viewMatrix);
            Vector3 frustumCornersViewSpace5 = Vector3.Transform(cornersWorldSpace[5], viewMatrix);

            #endregion

            // Find the centroid
            Vector3 frustumCentroid = new Vector3(0, 0, 0);
            for (int i = 0; i < 8; i++)
                frustumCentroid += cornersWorldSpace[i];
            frustumCentroid /= 8;

            // Position the shadow-caster camera so that it's looking at the centroid, and backed up in the direction of the sunlight
            float distFromCentroid = MathHelper.Max((range - nearPlane), Vector3.Distance(frustumCornersViewSpace4, frustumCornersViewSpace5)) + 50.0f;
            lightViewMatrix = Matrix.CreateLookAt(frustumCentroid - (direction * distFromCentroid), frustumCentroid, Vector3.Up);

            // Determine the position of the frustum corners in light space
            Vector3.Transform(cornersWorldSpace, ref lightViewMatrix, frustumCornersLightSpace);

            // Calculate an orthographic projection by sizing a bounding box to the frustum coordinates in light space
            Vector3 mins = frustumCornersLightSpace[0];
            Vector3 maxes = frustumCornersLightSpace[0];
            for (int i = 0; i < 8; i++)
            {
                if (frustumCornersLightSpace[i].X > maxes.X)
                    maxes.X = frustumCornersLightSpace[i].X;
                else if (frustumCornersLightSpace[i].X < mins.X)
                    mins.X = frustumCornersLightSpace[i].X;
                if (frustumCornersLightSpace[i].Y > maxes.Y)
                    maxes.Y = frustumCornersLightSpace[i].Y;
                else if (frustumCornersLightSpace[i].Y < mins.Y)
                    mins.Y = frustumCornersLightSpace[i].Y;
                if (frustumCornersLightSpace[i].Z > maxes.Z)
                    maxes.Z = frustumCornersLightSpace[i].Z;
                else if (frustumCornersLightSpace[i].Z < mins.Z)
                    mins.Z = frustumCornersLightSpace[i].Z;
            }

            // Create an orthographic camera for use as a shadow caster
            lightProjectionMatrix = Matrix.CreateOrthographicOffCenter(mins.X, maxes.X, mins.Y, maxes.Y, -maxes.Z - range, -mins.Z);

            spViewToLightViewProjMatrix.Value = Matrix.Invert(viewMatrix) * lightViewMatrix * lightProjectionMatrix;
            spFrustumCorners.Value = boundingFrustum;
        } // SetLight

		#endregion

    } // BasicShadowMapShader
} // XNAFinalEngine.Graphics
