
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

	/// <summary>
    /// Cascaded Shadows.
    /// Only works with directional lights.
    /// If you need point light shadows use the tetrahedron shadow map or if you need spot light use the basic shadow map.
	/// </summary>
    internal class CascadedShadowMapShader : Shader
    {

        #region Constants

        /// <summary>
        /// Number of splits. 
        /// This value have to be the same in both this class and the shader code.
        /// </summary>
        public const int NumberSplits = 4;

        #endregion

        #region Variables

        /// <summary>
        /// Light Projection Matrices (one for each split)
        /// </summary>
        private readonly Matrix[] lightProjectionMatrix = new Matrix[NumberSplits];

        /// <summary>
        /// Light View Matrices (one for each split)
        /// </summary>
        private readonly Matrix[] lightViewMatrix = new Matrix[NumberSplits];

        /// <summary>
        /// Clip Plane. First value is the split's near plane and the last is the far plane.
        /// </summary>
        private readonly Vector2[] lightClipPlanes = new Vector2[NumberSplits];

        /// <summary>
        /// Splits depths. 
        /// First is the camera near plane, the last is the far plane, and the middle values are the different near and far planes of each split.
        /// </summary>
        private readonly float[] splitDepths = new float[NumberSplits + 1];

        /// <summary>
        /// Matrix to go from view to light view projection space.
        /// </summary>
        private readonly Matrix[] viewToLightViewProj = new Matrix[NumberSplits];

        private RenderTarget shadowTexture;

        // Singleton reference.
        private static CascadedShadowMapShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// Light Projection Matrices (one for each split)
        /// </summary>
        public Matrix[] LightProjectionMatrix { get { return lightProjectionMatrix; } }

        /// <summary>
        /// Light Projection Matrices (one for each split)
        /// </summary>
        public Matrix[] LightViewMatrix { get { return lightViewMatrix; } }

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

        private static readonly Matrix[] lastUsedViewToLightViewProjMatrix = new Matrix[NumberSplits];
        private static void SetViewToLightViewProjMatrix(Matrix[] viewToLightViewProjMatrix)
        {
            if (!ArrayHelper.Equals(lastUsedViewToLightViewProjMatrix, viewToLightViewProjMatrix))
            {
                //lastUsedViewToLightViewProjMatrix = (Matrix[])(viewToLightViewProjMatrix.Clone()); // Produces garbage
                for (int i = 0; i < NumberSplits; i++)
                {
                    lastUsedViewToLightViewProjMatrix[i] = viewToLightViewProjMatrix[i];
                }
                epViewToLightViewProj.SetValue(viewToLightViewProjMatrix);
            }
        }

        #endregion

        #region Clip Planes

        private static readonly Vector2[] lastUsedClipPlanes = new Vector2[NumberSplits];
        private static void SetClipPlanes(Vector2[] clipPlanes)
        {
            if (!ArrayHelper.Equals(lastUsedClipPlanes, clipPlanes))
            {
                // lastUsedClipPlanes = (Vector2[])(clipPlanes.Clone()); // Produces garbage
                for (int i = 0; i < NumberSplits; i++)
                {
                    lastUsedClipPlanes[i] = clipPlanes[i];
                }
                epClipPlanes.SetValue(clipPlanes);
            }
        }

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
        }

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
        }

        #endregion

        #region Half Pixel

        private static Vector2 lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        }

        #endregion

        #region Frustum Corners

        private static readonly Vector3[] lastUsedFrustumCorners = new Vector3[NumberSplits];
        private static void SetFrustumCorners(Vector3[] frustumCorners)
        {
            if (!ArrayHelper.Equals(lastUsedFrustumCorners, frustumCorners))
            {
                // lastUsedFrustumCorners = (Vector3[])(frustumCorners.Clone()); // Produces garbage
                for (int i = 0; i < NumberSplits; i++)
                {
                    lastUsedFrustumCorners[i] = frustumCorners[i];
                }
                epFrustumCorners.SetValue(frustumCorners);
            }
        }

        #endregion

        #region Depth Bias

        private static float lastUsedDepthBias;
        private static void SetDepthBias(float _depthBias)
        {
            if (lastUsedDepthBias != _depthBias)
            {
                lastUsedDepthBias = _depthBias;
                epDepthBias.SetValue(_depthBias);
            }
        }

        #endregion
        
        #region Shadow Map Size

        private static Vector2 lastUsedShadowMapSize;
        private static void SetShadowMapSize(Vector2 shadowMapSize)
        {
            if (lastUsedShadowMapSize != shadowMapSize)
            {
                lastUsedShadowMapSize = shadowMapSize;
                epInvShadowMapSize.SetValue(new Vector2(1f / shadowMapSize.X, 1f / shadowMapSize.Y));
                epShadowMapSize.SetValue(shadowMapSize);
            }
        }

        #endregion
        
        #endregion

        #region Constructor

	    /// <summary>
	    /// Cascaded shadow map.
	    /// Only works with directional lights.
        /// If you need point light shadows use the tetrahedron shadow map or if you need spot light use the basic shadow map.
	    /// </summary>
	    private CascadedShadowMapShader() : base("Shadows\\CascadedShadowMap") { }

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
                epViewToLightViewProj = Resource.Parameters["viewToLightViewProj"];
                    epViewToLightViewProj.SetValue(lastUsedViewToLightViewProjMatrix);
                // Textures
                 epDepthTexture       = Resource.Parameters["depthTexture"];
                    if (lastUsedDepthTexture != null && !lastUsedDepthTexture.IsDisposed)
                        epDepthTexture.SetValue(lastUsedDepthTexture);
                epShadowMap           = Resource.Parameters["shadowMap"];
                    if (lastUsedShadowMapTexture != null && !lastUsedShadowMapTexture.IsDisposed)
                        epShadowMap.SetValue(lastUsedShadowMapTexture);
			    // Get additional parameters
                epClipPlanes          = Resource.Parameters["clipPlanes"];
                    epClipPlanes.SetValue(lastUsedClipPlanes);
                epHalfPixel           = Resource.Parameters["halfPixel"];
                    epHalfPixel.SetValue(lastUsedHalfPixel);
                epFrustumCorners      = Resource.Parameters["frustumCorners"];
                    epFrustumCorners.SetValue(lastUsedFrustumCorners);
                epDepthBias           = Resource.Parameters["depthBias"];
                    epDepthBias.SetValue(lastUsedDepthBias);
                epShadowMapSize       = Resource.Parameters["shadowMapSize"];
                    epShadowMapSize.SetValue(lastUsedShadowMapSize);
                epInvShadowMapSize    = Resource.Parameters["invShadowMapSize"];
                    epInvShadowMapSize.SetValue(new Vector2(1f / lastUsedShadowMapSize.X, 1f / lastUsedShadowMapSize.Y));
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

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
        internal void SetLight(Vector3 direction, Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane, Vector3[] boundingFrustum,
                               float farPlaneSplit1, float farPlaneSplit2, float farPlaneSplit3, float farPlaneSplit4)
        {
            // Calculate the cascade splits. 
            // We calculate these so that each successive split is larger than the previous, giving the closest split the most amount of shadow detail.  
            const float fNumberSplits = NumberSplits;
            float fNearPlane = nearPlane,
                  fFarPlane = farPlane;
            const float splitConstant = 0.7f;

            // First and last split.
            splitDepths[0] = fNearPlane;
            splitDepths[NumberSplits] = fFarPlane;
            // The middle splits.
            for (int i = 1; i < splitDepths.Length - 1; i++)
            {
                splitDepths[i] = splitConstant * fNearPlane * (float)Math.Pow(fFarPlane / fNearPlane, i / fNumberSplits) +
                                 (1.0f - splitConstant) * ((fNearPlane + (i / fNumberSplits)) * (fFarPlane - fNearPlane));
            }
            if (farPlaneSplit1 > 0)
                splitDepths[1] = farPlaneSplit1;
            if (farPlaneSplit2 > 0)
                splitDepths[2] = farPlaneSplit2;
            if (farPlaneSplit3 > 0)
                splitDepths[3] = farPlaneSplit3;
            if (farPlaneSplit4 > 0)
                splitDepths[4] = farPlaneSplit4;

            // Create the different view and projection matrices for each split.
            for (int splitNumber = 0; splitNumber < NumberSplits; splitNumber++)
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
            for (int i = 0; i < NumberSplits; i++)
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

        #region Render

        /// <summary>
        /// Calculate the final shadows using the scene depth and light depth information.
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
                SetShadowMapTexture(lightDepthTexture);
                SetHalfPixel(new Vector2(-0.5f/(depthTexture.Width/2), 0.5f/(depthTexture.Height/2)));
                SetShadowMapSize(new Vector2(lightDepthTexture.Width, lightDepthTexture.Height));
                SetDepthBias(depthBias);
                SetDepthTexture(depthTexture);
                
                shadowTexture.EnableRenderTarget();
                shadowTexture.Clear(Color.White);

                switch (filterType)
                {
                    case Shadow.FilterType.Pcf2X2:
                        Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap2x2PCF"];
                        break;
                    case Shadow.FilterType.Pcf3X3:
                        Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap3x3PCF"];
                        break;
                    case Shadow.FilterType.Pcf5X5:
                        Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap5x5PCF"];
                        break;
                    case Shadow.FilterType.Pcf7X7:
                        Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap7x7PCF"];
                        break;
                    default:
                        Resource.CurrentTechnique = Resource.Techniques["RenderShadowMapPoisonPCF"];
                        break;
                }

                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
                shadowTexture.DisableRenderTarget();

                //BilateralBlurShader.Instance.Filter(shadowTexture, true, 1); // TODO!!! Volver a activarla

                return shadowTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to render.", e);
            }
        } // Render

	    #endregion
        
    } // CascadedShadowMapShader
} // XNAFinalEngine.Graphics
