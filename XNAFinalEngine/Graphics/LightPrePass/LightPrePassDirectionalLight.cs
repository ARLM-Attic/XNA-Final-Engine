
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
    /// Light Pre Pass Directional Light shader.
    /// </summary>
    internal class LightPrePassDirectionalLight : Shader
    {

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epFrustumCorners,
                                       epDepthTexture,
                                       epNormalTexture,
                                       epMotionVectorSpecularPowerTexture,
                                       epLightColor,
                                       epLightDirection,
                                       epLlightIntensity;


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

        #region Depth Texture

        private static Texture lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedDepthTexture != depthTexture)
            {
                lastUsedDepthTexture = depthTexture;
                epDepthTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthTexture

        #endregion

        #region Normal Texture

        private static Texture lastUsedNormalTexture;
        private static void SetNormalTexture(Texture normalTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedNormalTexture != normalTexture)
            {
                lastUsedNormalTexture = normalTexture;
                epNormalTexture.SetValue(normalTexture.Resource);
            }
        } // SetNormalTexture

        #endregion

        #region Motion Vector Specular Power Texture

        private static Texture lastUsedMotionVectorSpecularPower;
        private static void SetMotionVectorSpecularPower(Texture motionVectorSpecularPower)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedMotionVectorSpecularPower != motionVectorSpecularPower)
            {
                lastUsedMotionVectorSpecularPower = motionVectorSpecularPower;
                epMotionVectorSpecularPowerTexture.SetValue(motionVectorSpecularPower.Resource);
            }
        } // SetMotionVectorSpecularPower

        #endregion

        #region Light Color

        private static Color? lastUsedLightColor;
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

        private static Vector3? lastUsedLightDir;
        private static void SetLightDirection(Vector3 lightDirection)
        {
            if (lastUsedLightDir != lightDirection)
            {
                lastUsedLightDir = lightDirection;
                epLightDirection.SetValue(lightDirection);
            }
        } // SetLightDirection

        #endregion

        #region Light Intensity

        private static float? lastUsedLightIntensity;
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
        /// Light Pre Pass Directional Light shader.
        /// </summary>
        internal LightPrePassDirectionalLight() : base("LightPrePass\\DirectionalLight") { }

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
                epLightColor                       = Resource.Parameters["lightColor"];
                epLightDirection                   = Resource.Parameters["lightDirection"];
                epLlightIntensity                  = Resource.Parameters["lightIntensity"];
                epFrustumCorners                   = Resource.Parameters["frustumCorners"];
                epDepthTexture                     = Resource.Parameters["depthTexture"];
                epNormalTexture                    = Resource.Parameters["normalTexture"];
                epMotionVectorSpecularPowerTexture = Resource.Parameters["motionVectorSpecularPowerTexture"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Render

        /// <summary>
        /// Render to the light pre pass texture.
        /// </summary>
        public void Render(DirectionalLight light, RenderTarget depthTexture, RenderTarget normalTexture, RenderTarget motionVectorSpecularPowerTexture, RenderTarget lightPrePassMap)
        {
            try
            {/*
                #region Set Parameters

                SetHalfPixel(new Vector2(-1f / lightPrePassMap.Width, 1f / lightPrePassMap.Height));
                SetLightColor(light.DiffuseColor);
                // The next three lines produce the same result.
                SetLightDirection(Vector3.Transform(light.Direction, Matrix.CreateFromQuaternion(ApplicationLogic.Camera.Orientation)));
                //SetLightDirection(Vector3.Transform(light.Direction, Matrix.Transpose(Matrix.Invert(ApplicationLogic.Camera.ViewMatrix))));
                //SetLightDirection(Vector3.TransformNormal(light.Direction, ApplicationLogic.Camera.ViewMatrix));
                SetLightIntensity(light.Intensity);
                SetFrustumCorners(ApplicationLogic.Camera.BoundingFrustum());
                SetDepthTexture(depthTexture);
                SetNormalTexture(normalTexture);
                SetMotionVectorSpecularPower(motionVectorSpecularPowerTexture);

                #endregion

                if (light.ShadowMap != null)
                {
                    Resource.Parameters["shadowTexture"].SetValue(light.ShadowMap.ShadowTexture.XnaTexture);
                    Resource.CurrentTechnique = Effect.Techniques["DirectionalLightWithShadows"];
                }
                else
                    Resource.CurrentTechnique = Resource.Techniques["DirectionalLight"];
                */
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
            } // try
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Directional Light: Unable to render.", e);
            }
        } // Render

        #endregion

    } // LightPrePassDirectionalLight
} // XNAFinalEngine.Graphics