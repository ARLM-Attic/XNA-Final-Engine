
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
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Assets;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Light Pre Pass Directional Light Shader.
    /// </summary>
    internal class DirectionalLightShader : Shader
    {

        #region Variables
        
        // Current view matrix. Used to set the shader parameters.
        private Matrix viewMatrix;

        // Singleton reference.
        private static DirectionalLightShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a directional light shader.
        /// </summary>
        public static DirectionalLightShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new DirectionalLightShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epFrustumCorners,
                                       epDepthTexture,
                                       epNormalTexture,
                                       epLightColor,
                                       epLightDirection,
                                       epLlightIntensity,
                                       epShadowTexture;


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

        #region Normal Texture

        private static Texture2D lastUsedNormalTexture;
        private static void SetNormalTexture(Texture normalTexture)
        {
            EngineManager.Device.SamplerStates[1] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedNormalTexture != normalTexture.Resource)
            {
                lastUsedNormalTexture = normalTexture.Resource;
                epNormalTexture.SetValue(normalTexture.Resource);
            }
        } // SetNormalTexture

        #endregion
        
        #region Shadow Texture

        private static Texture2D lastUsedShadowTexture;
        private static void SetShadowTexture(Texture shadowTexture)
        {
            EngineManager.Device.SamplerStates[3] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedShadowTexture != shadowTexture.Resource)
            {
                lastUsedShadowTexture = shadowTexture.Resource;
                epShadowTexture.SetValue(shadowTexture.Resource);
            }
        } // SetNormalTexture

        #endregion

        #region Light Color

        private static Color lastUsedLightColor;
        private static void SetLightColor(Color lightColor)
        {
            if (lastUsedLightColor != lightColor)
            {
                lastUsedLightColor = lightColor;
                epLightColor.SetValue(new Vector3(lightColor.R / 255f, lightColor.G / 255f, lightColor.B / 255f));
            }
        } // SetLightColor

        #endregion

        #region Light Direction

        private static Vector3 lastUsedLightDirection;
        private static void SetLightDirection(Vector3 lightDirection)
        {
            if (lastUsedLightDirection != lightDirection)
            {
                lastUsedLightDirection = lightDirection;
                epLightDirection.SetValue(lightDirection);
            }
        } // SetLightDirection

        #endregion

        #region Light Intensity

        private static float lastUsedLightIntensity;
        private static void SetLightIntensity(float lightIntensity)
        {
            if (lastUsedLightIntensity != lightIntensity)
            {
                lastUsedLightIntensity = lightIntensity;
                epLlightIntensity.SetValue(lightIntensity);
            }
        } // SetLightIntensity

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Light Pre Pass Directional Light Shader.
        /// </summary>
        private DirectionalLightShader() : base("LightPrePass\\DirectionalLight") { }

        #endregion

        #region Get Parameters Handles

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
                epHalfPixel                        = Resource.Parameters["halfPixel"];
                    epHalfPixel.SetValue(lastUsedHalfPixel);
                epLightColor                       = Resource.Parameters["lightColor"];
                    epLightColor.SetValue(new Vector3(lastUsedLightColor.R / 255f, lastUsedLightColor.G / 255f, lastUsedLightColor.B / 255f));
                epLightDirection                   = Resource.Parameters["lightDirection"];
                    epLightDirection.SetValue(lastUsedLightDirection);
                epLlightIntensity                  = Resource.Parameters["lightIntensity"];
                    epLlightIntensity.SetValue(lastUsedLightIntensity);
                epFrustumCorners                   = Resource.Parameters["frustumCorners"];
                    epFrustumCorners.SetValue(lastUsedFrustumCorners);
                epDepthTexture                     = Resource.Parameters["depthTexture"];
                    if (lastUsedDepthTexture != null && !lastUsedDepthTexture.IsDisposed)
                        epDepthTexture.SetValue(lastUsedDepthTexture);
                epNormalTexture                    = Resource.Parameters["normalTexture"];
                    if (lastUsedNormalTexture != null && !lastUsedNormalTexture.IsDisposed)
                        epNormalTexture.SetValue(lastUsedNormalTexture);
                epShadowTexture = Resource.Parameters["shadowTexture"];
                    if (lastUsedShadowTexture != null && !lastUsedShadowTexture.IsDisposed)
                        epShadowTexture.SetValue(lastUsedShadowTexture);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Begin

        /// <summary>
        /// Begins the directional light rendering.
        /// </summary>
        /// <param name="depthTexture">Gbuffer's depth buffer.</param>
        /// <param name="normalTexture">Gbuffer's normal map.</param>
        /// <param name="viewMatrix">Camera view matrix.</param>
        /// <param name="boundingFrustum">Camera bounding frustum (use the camera's component method).</param>
        internal void Begin(RenderTarget depthTexture, RenderTarget normalTexture, Matrix viewMatrix, Vector3[] boundingFrustum)
        {
            try
            {
                SetDepthTexture(depthTexture);
                SetNormalTexture(normalTexture);
                // The reason that it’s 1/width instead of 0.5/width is because, in clip space, the coordinates range from -1 to 1 (width 2),
                // and not from 0 to 1 (width 1) like they do in textures, so you need to double the movement to account for that.
                SetHalfPixel(new Vector2(-0.5f / (depthTexture.Width / 2), 0.5f / (depthTexture.Height / 2))); // I use the depth texture, but I just need the destination render target dimension.
                SetFrustumCorners(boundingFrustum);
                this.viewMatrix = viewMatrix;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Directional Light:: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Light

        /// <summary>
        /// Render to the light pre pass texture.
        /// </summary>
        internal void RenderLight(Color diffuseColor, Vector3 direction, float intensity, Texture shadowTexture)
        {
            try
            {
                
                #region Set Parameters
              
                SetLightColor(diffuseColor);
                // The next three lines produce the same result.
                //SetLightDirection(Vector3.Transform(light.Direction, Matrix.CreateFromQuaternion(ApplicationLogic.Camera.Orientation)));
                //SetLightDirection(Vector3.Transform(light.Direction, Matrix.Transpose(Matrix.Invert(ApplicationLogic.Camera.ViewMatrix))));
                SetLightDirection(Vector3.TransformNormal(direction, viewMatrix));
                SetLightIntensity(intensity);

                #endregion

                if (shadowTexture != null)
                {
                    SetShadowTexture(shadowTexture);
                    Resource.CurrentTechnique = Resource.Techniques["DirectionalLightWithShadows"];
                }
                else
                    Resource.CurrentTechnique = Resource.Techniques["DirectionalLight"];
                
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Directional Light: Unable to render.", e);
            }
        } // RenderLight

        #endregion

    } // DirectionalLightShader
} // XNAFinalEngine.Graphics