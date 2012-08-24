
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
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

	/// <summary>
	/// Basic shadow map.
	/// Only works with directional lights and spot lights.
	/// If performance is not an issue, use cascaded shadow maps for directional lights.
	/// If you need point light shadows use the tetrahedron shadow map.
	/// </summary>
    internal class BasicShadowMapShader : Shader
	{

		#region Variables
        
        // Light Matrices.
        private Matrix lightProjectionMatrix, lightViewMatrix;
        
	    private Shadow.FilterType filterType;

	    private RenderTarget lightDepthTexture, deferredShadowResult;

        // Singleton reference.
        private static BasicShadowMapShader instance;

        #endregion

        #region Properties

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
                                        epInvShadowMapSize,
                                        epBones;

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

        #region Bones

        private static readonly Matrix[] lastUsedBones = new Matrix[72];
        private static void SetBones(Matrix[] bones)
        {
            if (!ArrayHelper.Equals(lastUsedBones, bones))
            {
                // lastUsedFrustumCorners = (Vector3[])(frustumCorners.Clone()); // Produces garbage
                for (int i = 0; i < 4; i++)
                {
                    lastUsedBones[i] = bones[i];
                }
                epBones.SetValue(bones);
            }
        } // SetBones

        #endregion

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
                // Skinning //
                epBones = Resource.Parameters["Bones"];
                    epBones.SetValue(lastUsedBones);
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
                lightDepthTexture = RenderTarget.Fetch(lightDepthTextureSize, SurfaceFormat.HalfSingle, DepthFormat.Depth16, RenderTarget.AntialiasingType.NoAntialiasing);
                // Alpha8 doesn't work in my old G92 GPU processor and I opt to work with half single. Color is another good choice because support texture filtering.
                // XBOX 360 Xbox does not support 16 bit render targets (http://blogs.msdn.com/b/shawnhar/archive/2010/07/09/rendertarget-formats-in-xna-game-studio-4-0.aspx)
                // Color would be the better choice for the XBOX 360.
                // With color we have another good option, the possibility to gather four shadow results (local or global) in one texture.
                deferredShadowResult = RenderTarget.Fetch(depthTexture.Size, SurfaceFormat.HalfSingle, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                // Set parameters.
                SetHalfPixel(new Vector2(-0.5f / (depthTexture.Size.Width / 2), 0.5f / (depthTexture.Size.Height / 2)));
                SetShadowMapTexelSize(new Vector2(lightDepthTextureSize.Width, lightDepthTextureSize.Height));
                SetDepthBias(depthBias);
                SetDepthTexture(depthTexture);
                this.filterType = filterType;

                // Enable first render target.
                lightDepthTexture.EnableRenderTarget();
                lightDepthTexture.Clear(Color.White);
                
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Process

        /// <summary>
        /// The last pass of the shadow calculations.
        /// </summary>
        private void Process()
        {
            // Render deferred shadow result
            EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
            SetShadowMapTexture(lightDepthTexture);

            deferredShadowResult.EnableRenderTarget();
            deferredShadowResult.Clear(Color.White);

            switch (filterType)
            {
                case Shadow.FilterType.Pcf2X2: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap2x2PCF"]; break;
                case Shadow.FilterType.Pcf3X3: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap3x3PCF"]; break;
                case Shadow.FilterType.Pcf5X5: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap5x5PCF"]; break;
                case Shadow.FilterType.Pcf7X7: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMap7x7PCF"]; break;
                default: Resource.CurrentTechnique = Resource.Techniques["RenderShadowMapPoisonPCF"]; break;
            }

            Resource.CurrentTechnique.Passes[0].Apply();
            RenderScreenPlane();
            deferredShadowResult.DisableRenderTarget();
            //BlurShader.Instance.Filter(deferredShadowResult, true, 1); // TODO!!! Volver a activarla
        } // Process

        #endregion

        #region End

        /// <summary>
        /// Resolve render targets and return the deferred shadow map.
        /// </summary>
        internal RenderTarget End(ref RenderTarget _lightDepthTexture)
        {
            try
            {
                // Resolve shadow map.
                lightDepthTexture.DisableRenderTarget();
                _lightDepthTexture = lightDepthTexture;
                Process();
                return deferredShadowResult;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to end the rendering.", e);
            }
        } // End

        /// <summary>
        /// Resolve render targets and return the deferred shadow map.
        /// </summary>
        internal RenderTarget ProcessWithPrecalculedLightDepthTexture(RenderTarget lightDepthTexture)
        {
            try
            {
                // Use parameter.
                this.lightDepthTexture = lightDepthTexture;
                Process();
                return deferredShadowResult;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to end the rendering.", e);
            }
        } // ProcessWithPrecalculedLightDepthTexture

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
            SetViewToLightViewProjMatrix(Matrix.Invert(viewMatrix) * lightViewMatrix * lightProjectionMatrix);
            SetFrustumCorners(boundingFrustum);
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

            SetViewToLightViewProjMatrix(Matrix.Invert(viewMatrix) * lightViewMatrix * lightProjectionMatrix);
            SetFrustumCorners(boundingFrustum);
        } // SetLight

		#endregion

        #region Render Model

        /// <summary>
        /// Render objects in light space.
        /// </summary>
        internal void RenderModel(Matrix worldMatrix, Model model, Matrix[] boneTransform)
        {
            if (model is FileModel && ((FileModel)model).IsSkinned) // If it is a skinned model.
            {
                SetBones(boneTransform);
                Resource.CurrentTechnique = Resource.Techniques["GenerateShadowMapSkinned"];
            }
            else
                Resource.CurrentTechnique = Resource.Techniques["GenerateShadowMap"];
            SetWorldViewProjMatrix(worldMatrix * lightViewMatrix * lightProjectionMatrix);
            Resource.CurrentTechnique.Passes[0].Apply();
            model.Render();

        } // RenderModel

		#endregion

    } // BasicShadowMapShader
} // XNAFinalEngine.Graphics
