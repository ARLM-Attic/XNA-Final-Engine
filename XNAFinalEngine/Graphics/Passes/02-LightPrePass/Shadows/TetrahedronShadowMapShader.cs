
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
    /// Render point light shadows from all directions using the tetrahedron shadow technique. 
    /// </summary>
    /// <remarks>
    /// This technique is faster than the cube map technique and it has a few interesting optimization options.
    /// For a full description read the GPU Pro 1 chapter called Shadow Mapping for Omnidirectional Light Using Tetrahedron Mapping.
    /// </remarks>
    internal class TetrahedronShadowMapShader : Shader
	{

		#region Variables

        // Singleton reference.
        private static TetrahedronShadowMapShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static TetrahedronShadowMapShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new TetrahedronShadowMapShader();
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
                                        epHalfPixel,
                                        epFrustumCorners,
                                        epDepthBias,
                                        epShadowMapSize,
                                        epInvShadowMapSize;

        #region Matrices

        private static Matrix lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjectionMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjectionMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjectionMatrix;
                epWorldViewProj.SetValue(worldViewProjectionMatrix);
            }
        } // SetWorldViewProjMatrix

        private static Matrix lastUsedViewToLightViewProjMatrix;
        private static void SetViewToLightViewProjMatrix(Matrix viewToLightViewProjMatrix)
        {
            if (lastUsedViewToLightViewProjMatrix != viewToLightViewProjMatrix)
            {
                lastUsedViewToLightViewProjMatrix = viewToLightViewProjMatrix;
                epViewToLightViewProj.SetValue(viewToLightViewProjMatrix);
            }
        } // SetViewToLightViewProjMatrix

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

        private static Vector2 lastUsedHalfPixel;
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

        private static readonly Vector3[] lastUsedFrustumCorners = new Vector3[4];
        private static void SetFrustumCorners(Vector3[] frustumCorners)
        {
            if (!ArrayHelper.Equals(lastUsedFrustumCorners, frustumCorners))
            {
                // lastUsedFrustumCorners = (Vector3[])(frustumCorners.Clone()); // Produces garbage
                for (int i = 0; i < 4; i++)
                {
                    lastUsedFrustumCorners[i] = frustumCorners[i];
                }
                epFrustumCorners.SetValue(frustumCorners);
            }
        } // SetFrustumCorners

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
        } // SetDepthBias

        #endregion
        
        #region Shadow Map Size

        private static Vector2 lastUsedShadowMapSize;
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
        /// Render point light shadows from all directions using the tetrahedron shadow technique. 
        /// </summary>
        /// <remarks>
        /// This technique is slightly faster than the cube map technique if the lookup and stencil optimizations are used.
        /// For a full description read the GPU Pro 1 chapter called Shadow Mapping for Omnidirectional Light Using Tetrahedron Mapping.
        /// </remarks>
        private TetrahedronShadowMapShader() : base("Shadows\\TetrahedronShadowMap") { }

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
                    epWorldViewProj.SetValue(lastUsedWorldViewProjMatrix);
                epViewToLightViewProj = Resource.Parameters["viewToLightViewProj"];
                    epViewToLightViewProj.SetValue(lastUsedViewToLightViewProjMatrix);
                // Textures
                epDepthTexture        = Resource.Parameters["depthTexture"];
                    if (lastUsedDepthTexture != null && !lastUsedDepthTexture.IsDisposed)
                        epDepthTexture.SetValue(lastUsedDepthTexture);
                epShadowMap           = Resource.Parameters["shadowMap"];
                    if (lastUsedShadowMapTexture != null && !lastUsedShadowMapTexture.IsDisposed)
                        epShadowMap.SetValue(lastUsedShadowMapTexture);
			    // Get additional parameters
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

        /// <summary>
        /// Determines the size of the frustum needed to cover the viewable area, then creates the light view matrix and an appropriate orthographic projection.
        /// </summary>
        internal void SetLight(Vector3 direction, Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane, Vector3[] boundingFrustum,
                               float farPlaneSplit1, float farPlaneSplit2, float farPlaneSplit3, float farPlaneSplit4)
        {

        } // SetLight

        #endregion

        #region Render
        /*
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
                SetHalfPixel(new Vector2(-0.5f / (depthTexture.Width / 2), 0.5f / (depthTexture.Height / 2)));
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
        */
        #endregion
        

    } // TetrahedronShadowMapShader
} // XNAFinalEngine.Graphics
