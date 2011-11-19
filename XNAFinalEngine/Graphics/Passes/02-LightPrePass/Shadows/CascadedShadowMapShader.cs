
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System.Collections.Generic;
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
	/// Cascaded shadow map.
    /// Only works with directional lights.
    /// If you need point light shadows use the cubic shadow map or if you need spot light use the basic shadow map.
	/// </summary>
    public class CascadedShadowMapShader : Shader
    {

        #region Constants

        /// <summary>
        /// Number of splits. 
        /// This value have to be the same in both this class and the shader code.
        /// </summary>
        private const int numberSplits = 4;

        #endregion

        #region Variables

        /// <summary>
        /// Light Projection Matrices (one for each split)
        /// </summary>
        private readonly Matrix[] lightProjectionMatrix = new Matrix[numberSplits];

        /// <summary>
        /// Light View Matrices (one for each split)
        /// </summary>
        private readonly Matrix[] lightViewMatrix = new Matrix[numberSplits];

        /// <summary>
        /// Clip Plane. First value is the split's near plane and the last is the far plane.
        /// </summary>
        private readonly Vector2[] lightClipPlanes = new Vector2[numberSplits];

        /// <summary>
        /// Splits depths. 
        /// First is the camera near plane, the last is the far plane, and the middle values are the different near and far planes of each split.
        /// </summary>
        private readonly float[] splitDepths = new float[numberSplits + 1];

        /// <summary>
        /// Matrix to go from view to light view projection space.
        /// </summary>
        private readonly Matrix[] viewToLightViewProj = new Matrix[numberSplits];

        private Shadow.FilterType filterType;

        private RenderTarget lightDepthTexture, shadowTexture;

        // Singleton reference.
        private static CascadedShadowMapShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static CascadedShadowMapShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new CascadedShadowMapShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter
                                        // Matrices
                                        epWorldViewProj,
                                        epViewToLightViewProj,
                                        // Textures
                                        epDepthTexture,
                                        epShadowMap,
                                        // Other Parameters
                                        epClipPlanes,
                                        epHalfPixel,
                                        epFrustumCorners,
                                        epDepthBias,
                                        epShadowMapSize,
                                        epInvShadowMapSize;

        #region Matrices

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjectionMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjectionMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjectionMatrix;
                epWorldViewProj.SetValue(worldViewProjectionMatrix);
            }
        } // SetWorldViewProjMatrix

        private static Matrix[] lastUsedViewToLightViewProjMatrix;
        private static void SetViewToLightViewProjMatrix(Matrix[] viewToLightViewProjMatrix)
        {
            if (!ArrayHelper.Equals(lastUsedViewToLightViewProjMatrix, viewToLightViewProjMatrix))
            {
                lastUsedViewToLightViewProjMatrix = (Matrix[])(viewToLightViewProjMatrix.Clone());
                epViewToLightViewProj.SetValue(viewToLightViewProjMatrix);
            }
        } // SetViewToLightViewProjMatrix

        #endregion

        #region Clip Planes

        private static Vector2[] lastUsedClipPlanes;
        private static void SetClipPlanes(Vector2[] clipPlanes)
        {
            if (!ArrayHelper.Equals(lastUsedClipPlanes, clipPlanes))
            {
                lastUsedClipPlanes = (Vector2[])(clipPlanes.Clone());
                epClipPlanes.SetValue(clipPlanes);
            }
        } // SetClipPlanes

        #endregion

        #region Depth Texture

        private static Texture2D lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDepthTexture != depthTexture.Resource)
            {
                lastUsedDepthTexture = depthTexture.Resource;
                epDepthTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthTexture

        #endregion

        #region Shadow Map Texture

        private static Texture2D lastUsedShadowMapTexture;
        private static void SetShadowMapTexture(Texture shadowMapTexture)
        {
            EngineManager.Device.SamplerStates[3] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedShadowMapTexture != shadowMapTexture.Resource)
            {
                lastUsedShadowMapTexture = shadowMapTexture.Resource;
                epShadowMap.SetValue(shadowMapTexture.Resource);
            }
        } // SetShadowMapTexture

        #endregion

        #region Half Pixel

        private static Vector2? lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region Frustum Corners

        private static Vector3[] lastUsedFrustumCorners;
        private static void SetFrustumCorners(Vector3[] frustumCorners)
        {
            if (!ArrayHelper.Equals(lastUsedFrustumCorners, frustumCorners))
            {
                lastUsedFrustumCorners = (Vector3[])(frustumCorners.Clone());
                epFrustumCorners.SetValue(frustumCorners);
            }
        } // SetFrustumCorners

        #endregion

        #region Depth Bias

        private static float? lastUsedDepthBias;
        private static void SetDepthBias(float _depthBias)
        {
            if (lastUsedDepthBias != _depthBias)
            {
                lastUsedDepthBias = _depthBias;
                epDepthBias.SetValue(_depthBias);
            }
        } // SetDepthBias

        #endregion
        
        #region Shadow Map Size

        private static Vector2? lastUsedShadowMapSize;
        private static void SetShadowMapTexelSize(Vector2 shadowMapSize)
        {
            if (lastUsedShadowMapSize != shadowMapSize)
            {
                lastUsedShadowMapSize = shadowMapSize;
                epInvShadowMapSize.SetValue(new Vector2(1f / shadowMapSize.X, 1f / shadowMapSize.Y));
                epShadowMapSize.SetValue(shadowMapSize);
            }
        } // SetShadowMapTexelSize

        #endregion

        #endregion

        #region Constructor

	    /// <summary>
	    /// Cascaded shadow map.
	    /// Only works with directional lights.
	    /// If you need point light shadows use the cubic shadow map or if you need spot light use the basic shadow map.
	    /// </summary>
	    public CascadedShadowMapShader() : base("Shadows\\CascadedShadowMap") { }

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
                // Matrices
                epWorldViewProj       = Resource.Parameters["worldViewProj"];
                epViewToLightViewProj = Resource.Parameters["viewToLightViewProj"];
                // Textures
                epDepthTexture        = Resource.Parameters["depthTexture"];
                epShadowMap           = Resource.Parameters["shadowMap"];
			    // Get additional parameters
                epClipPlanes          = Resource.Parameters["clipPlanes"];
                epHalfPixel           = Resource.Parameters["halfPixel"];
                epFrustumCorners      = Resource.Parameters["frustumCorners"];
                epDepthBias           = Resource.Parameters["depthBias"];
                epShadowMapSize       = Resource.Parameters["shadowMapSize"];
                epInvShadowMapSize    = Resource.Parameters["invShadowMapSize"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

		#endregion

        #region Begin

        /// <summary>
        /// Begins the G-Buffer render.
        /// </summary>
        internal void Begin(Size lightDepthTextureSize, RenderTarget depthTexture, float depthBias, Shadow.FilterType filterType)
        {
            try
            {
                // Creates the render target textures
                lightDepthTexture = RenderTarget.Fetch(new Size(lightDepthTextureSize.Width * numberSplits, lightDepthTextureSize.Height), SurfaceFormat.HalfSingle, DepthFormat.Depth16, RenderTarget.AntialiasingType.NoAntialiasing);
                // Alpha8 doesn't work in my old G92 GPU processor and I opt to work with half single. Color is another good choice because support texture filtering.
                // XBOX 360 Xbox does not support 16 bit render targets (http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/rendertarget-formats-in-xna-game-studio-4-0.aspx)
                // Color would be the better choice for the XBOX 360.
                // With color we have another good option, the possibility to gather four shadow results (local or global) in one texture.
                shadowTexture = RenderTarget.Fetch(depthTexture.Size, SurfaceFormat.HalfSingle, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                // Set parameters.
                SetHalfPixel(new Vector2(-1f / depthTexture.Width, 1f / depthTexture.Height));
                SetShadowMapTexelSize(new Vector2(lightDepthTexture.Width, lightDepthTexture.Height));
                SetDepthBias(depthBias);
                SetDepthTexture(depthTexture);
                this.filterType = filterType;

                // Enable first render target.
                lightDepthTexture.EnableRenderTarget();
                lightDepthTexture.Clear(Color.White);
                Resource.CurrentTechnique = Resource.Techniques["GenerateShadowMap"];
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region End

        /// <summary>
        /// Resolve render targets and return the deferred shadow map.
        /// </summary>
        internal RenderTarget End()
        {
            try
            {
                // Resolve light depth texture.
                lightDepthTexture.DisableRenderTarget();

                // Render deferred shadow result.
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                SetShadowMapTexture(lightDepthTexture);

                shadowTexture.EnableRenderTarget();
                shadowTexture.Clear(Color.White);

                switch (filterType)
                {
                    case Shadow.FilterType.PCF2x2: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap2x2PCF"]; break;
                    case Shadow.FilterType.PCF3x3: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap3x3PCF"]; break;
                    case Shadow.FilterType.PCF5x5: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap5x5PCF"]; break;
                    case Shadow.FilterType.PCF7x7: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap7x7PCF"]; break;
                    default: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMapPoisonPCF"]; break;
                }

                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                shadowTexture.DisableRenderTarget();

                RenderTarget.Release(lightDepthTexture);
                BlurShader.Instance.Filter(shadowTexture, true, 1);
                return shadowTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to end the rendering.", e);
            }
        } // End

        #endregion

        #region Set Light

        // To avoid garbage use always the same values.
        private static readonly Vector3[] cornersWorldSpace = new Vector3[8];
        private static readonly Vector3[] frustumCornersLightSpace = new Vector3[8];
        private static readonly Vector3[] splitFrustumCornersViewSpace = new Vector3[8];
        private static readonly BoundingFrustum boundingFrustumTemp = new BoundingFrustum(Matrix.Identity);

        /// <summary>
        /// Determines the size of the frustum needed to cover the viewable area, then creates the light view matrix and an appropriate orthographic projection.
        /// </summary>
        internal void SetLight(Vector3 direction, Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane, Vector3[] boundingFrustum)
        {
            // Calculate the cascade splits. 
            // We calculate these so that each successive split is larger than the previous, giving the closest split the most amount of shadow detail.  
            const float fNumberSplits = numberSplits;
            float fNearPlane = nearPlane, 
                  fFarPlane = farPlane;
            const float splitConstant = 0.97f;

            // First and last split.
            splitDepths[0] = fNearPlane;
            splitDepths[numberSplits] = fFarPlane;
            // The middle splits.
            for (int i = 1; i < splitDepths.Length - 1; i++)
            {
                splitDepths[i] = splitConstant  *   fNearPlane * (float)Math.Pow(fFarPlane / fNearPlane, i / fNumberSplits) +
                                 (1.0f - splitConstant) * ((fNearPlane + (i / fNumberSplits)) * (fFarPlane - fNearPlane));
            }

            // Create the different view and projection matrices for each split.
            for (int splitNumber = 0; splitNumber < numberSplits; splitNumber++)
            {
                float minimumDepth = splitDepths[splitNumber];
                float maximumDepth = splitDepths[splitNumber + 1];

                #region Far Frustum Corner in View Space

                boundingFrustumTemp.Matrix = viewMatrix * projectionMatrix;
                boundingFrustumTemp.GetCorners(cornersWorldSpace);
                Vector3.Transform(cornersWorldSpace, ref viewMatrix, frustumCornersLightSpace);

                for (int i = 0; i < 4; i++)
                    splitFrustumCornersViewSpace[i] = frustumCornersLightSpace[i + 4] * (minimumDepth / fFarPlane);

                for (int i = 4; i < 8; i++)
                    splitFrustumCornersViewSpace[i] = frustumCornersLightSpace[i] * (maximumDepth / farPlane);

                Matrix viewInvert = Matrix.Invert(viewMatrix);
                Vector3.Transform(splitFrustumCornersViewSpace, ref viewInvert, cornersWorldSpace);
                
                #endregion

                // Find the centroid
                Vector3 frustumCentroid = new Vector3(0, 0, 0);
                for (int i = 0; i < 8; i++)
                    frustumCentroid += cornersWorldSpace[i];
                frustumCentroid /= 8;

                // Position the shadow-caster camera so that it's looking at the centroid, and backed up in the direction of the sunlight
                float distFromCentroid = MathHelper.Max((maximumDepth - minimumDepth), Vector3.Distance(splitFrustumCornersViewSpace[4], splitFrustumCornersViewSpace[5])) + 50.0f;
                lightViewMatrix[splitNumber] = Matrix.CreateLookAt(frustumCentroid - (direction * distFromCentroid), frustumCentroid, new Vector3(0, 1, 0));

                // Determine the position of the frustum corners in light space
                Vector3.Transform(cornersWorldSpace, ref lightViewMatrix[splitNumber], frustumCornersLightSpace);

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
                const float nearClipOffset = 100.0f;
                lightProjectionMatrix[splitNumber] = Matrix.CreateOrthographicOffCenter(mins.X, maxes.X, mins.Y, maxes.Y, -maxes.Z - nearClipOffset, -mins.Z);
            }
            // We'll use these clip planes to determine which split a pixel belongs to
            for (int i = 0; i < numberSplits; i++)
            {
                lightClipPlanes[i].X = -splitDepths[i];
                lightClipPlanes[i].Y = -splitDepths[i + 1];
                viewToLightViewProj[i] = Matrix.Invert(viewMatrix) * lightViewMatrix[i] * lightProjectionMatrix[i];
            }
            SetViewToLightViewProjMatrix(viewToLightViewProj);
            SetClipPlanes(lightClipPlanes);
            SetFrustumCorners(boundingFrustum);
        } // SetLight

        #endregion

        #region Render Model

        /// <summary>
        /// Render objects in light space.
        /// </summary>
        internal void RenderModel(Matrix worldMatrix, Model model, Matrix[] boneTransform)
        {
            for (int i = 0; i < numberSplits; i++)
            {
                // Set the viewport for the current split.
                Viewport splitViewport = new Viewport
                {
                    MinDepth = 0,
                    MaxDepth = 1,
                    X = i * lightDepthTexture.Height,
                    Y = 0,
                    Width = lightDepthTexture.Height,
                    Height = lightDepthTexture.Height
                };
                EngineManager.Device.Viewport = splitViewport;
                SetWorldViewProjMatrix(worldMatrix * lightViewMatrix[i] * lightProjectionMatrix[i]);
                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
        } // RenderModel

        #endregion    
        
    } // CascadedShadowMapShader
} // XNAFinalEngine.Graphics
