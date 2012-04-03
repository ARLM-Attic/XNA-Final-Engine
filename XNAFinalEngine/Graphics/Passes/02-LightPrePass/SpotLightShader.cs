
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Light Pre Pass Spot Light Shader.
    /// </summary>
    internal class SpotLightShader : Shader
    {

        #region Variables

        private float nearPlane;

        // Current view matrix. Used to set the shader parameters.
        private Matrix viewMatrix, projectionMatrix;

        // Bounding Light Object.
        private static Model boundingLightObject;

        // Singleton reference.
        private static SpotLightShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a spot light shader.
        /// </summary>
        public static SpotLightShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new SpotLightShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epLightRadius,
                                       epDepthTexture,
                                       epNormalTexture,
                                       epMotionVectorSpecularPowerTexture,
                                       epFarPlane,
                                       epWorldViewProj,
                                       epWorldView,
                                       // Light
                                       epLightColor,
                                       epLightPosition,
                                       epLlightIntensity,
                                       epLightDirection,
                                       epLightOuterConeAngle,
                                       epLightInnerConeAngle,
                                       epInsideBoundingLightObject,
                                       epShadowTexture;


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

        #region Depth Texture

        private static Texture2D lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp; // depthTexture
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

        #region Motion Vector Specular Power Texture

        private static Texture2D lastUsedMotionVectorSpecularPower;
        private static void SetMotionVectorSpecularPower(Texture motionVectorSpecularPower)
        {
            EngineManager.Device.SamplerStates[2] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedMotionVectorSpecularPower != motionVectorSpecularPower.Resource)
            {
                lastUsedMotionVectorSpecularPower = motionVectorSpecularPower.Resource;
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

        #region Light Position

        private static Vector3? lastUsedLightPosition;
        private static void SetLightPosition(Vector3 lightPosition)
        {
            if (lastUsedLightPosition != lightPosition)
            {
                lastUsedLightPosition = lightPosition;
                epLightPosition.SetValue(lightPosition);
            }
        } // SetLightPosition

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

        #region Radius

        private static float? lastUsedLightRadius;
        private static void SetLightRadius(float lightRadius)
        {
            if (lastUsedLightRadius != lightRadius)
            {
                lastUsedLightRadius = lightRadius;
                epLightRadius.SetValue(lightRadius);
            }
        } // SetLightRadius

        #endregion

        #region Light Direction

        private static Vector3? lastUsedLightDirection;
        private static void SetLightDirection(Vector3 lightDirection)
        {
            if (lastUsedLightDirection != lightDirection)
            {
                lastUsedLightDirection = lightDirection;
                epLightDirection.SetValue(lightDirection);
            }
        } // SetLightDirection

        #endregion

        #region Light Inner Cone Angle

        private static float? lastUsedLightInnerConeAngle;
        private static void SetLightInnerConeAngle(float lightInnerConeAngle)
        {
            if (lastUsedLightInnerConeAngle != lightInnerConeAngle)
            {
                lastUsedLightInnerConeAngle = lightInnerConeAngle;
                epLightInnerConeAngle.SetValue(lightInnerConeAngle / (3.141592f / 180.0f));
            }
        } // SetLightInnerConeAngle

        #endregion

        #region Light Outer Cone Angle

        private static float? lastUsedLightOuterConeAngle;
        private static void SetLightOuterConeAngle(float lightOuterConeAngle)
        {
            if (lastUsedLightOuterConeAngle != lightOuterConeAngle)
            {
                lastUsedLightOuterConeAngle = lightOuterConeAngle;
                epLightOuterConeAngle.SetValue(lightOuterConeAngle / (3.141592f / 180.0f));
            }
        } // SetLightOuterConeAngle

        #endregion

        #region Far Plane

        private static float lastUsedFarPlane;
        private static void SetFarPlane(float _farPlane)
        {
            if (lastUsedFarPlane != _farPlane)
            {
                lastUsedFarPlane = _farPlane;
                epFarPlane.SetValue(_farPlane);
            }
        } // SetFarPlane

        #endregion

        #region Inside Bounding Light Object

        private static bool? lastUsedInsideBoundingLightObject;
        private static void SetInsideBoundingLightObject(bool insideBoundingLightObject)
        {
            if (lastUsedInsideBoundingLightObject != insideBoundingLightObject)
            {
                lastUsedInsideBoundingLightObject = insideBoundingLightObject;
                epInsideBoundingLightObject.SetValue(insideBoundingLightObject);
            }
        } // SetInsideBoundingLightObject

        #endregion

        #region Matrices

        private static Matrix? lastUsedWorldViewMatrix;
        private static void SetWorldViewMatrix(Matrix worldViewMatrix)
        {
            if (lastUsedWorldViewMatrix != worldViewMatrix)
            {
                lastUsedWorldViewMatrix = worldViewMatrix;
                epWorldView.SetValue(worldViewMatrix);
            }
        } // SetWorldViewMatrix

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjectionMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjectionMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjectionMatrix;
                epWorldViewProj.SetValue(worldViewProjectionMatrix);
            }
        } // SetWorldViewProjMatrix

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

        #endregion

        #region Constructor

        /// <summary>
        /// Light Pre Pass Spot Light Shader.
        /// </summary>
        internal SpotLightShader() : base("LightPrePass\\SpotLight")
        {
            boundingLightObject = new Sphere(50, 50, 1);
        } // SpotLightShader

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
                epLightPosition                    = Resource.Parameters["lightPosition"];
                epLlightIntensity                  = Resource.Parameters["lightIntensity"];
                epLightRadius                      = Resource.Parameters["lightRadius"];
                epLightDirection                   = Resource.Parameters["lightDirection"];
                epLightOuterConeAngle              = Resource.Parameters["lightOuterAngle"];
                epLightInnerConeAngle              = Resource.Parameters["lightInnerAngle"];
                epDepthTexture                     = Resource.Parameters["depthTexture"];
                epNormalTexture                    = Resource.Parameters["normalTexture"];
                epMotionVectorSpecularPowerTexture = Resource.Parameters["motionVectorSpecularPowerTexture"];
                epFarPlane                         = Resource.Parameters["farPlane"];
                epWorldViewProj                    = Resource.Parameters["worldViewProj"];
                epWorldView                        = Resource.Parameters["worldView"];
                epInsideBoundingLightObject        = Resource.Parameters["insideBoundingLightObject"];
                epShadowTexture                    = Resource.Parameters["shadowTexture"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters
        
        #endregion

        #region Begin

        /// <summary>
        /// Begins the spot light rendering.
        /// </summary>
        /// <param name="depthTexture">Gbuffer's depth buffer.</param>
        /// <param name="normalTexture">Gbuffer's normal map.</param>
        /// <param name="motionVectorSpecularPowerTexture">Gbuffer's motion vector and specular power texture.</param>
        /// <param name="viewMatrix">Camera view matrix.</param>
        /// <param name="projectionMatrix">Camera projection matrix.</param>
        /// <param name="nearPlane">Camera near plane.</param>
        /// <param name="farPlane">Camera far plane.</param>
        internal void Begin(RenderTarget depthTexture, RenderTarget normalTexture, RenderTarget motionVectorSpecularPowerTexture,
                            Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane)
        {
            try
            {
                SetDepthTexture(depthTexture);
                SetNormalTexture(normalTexture);
                SetMotionVectorSpecularPower(motionVectorSpecularPowerTexture);
                SetHalfPixel(new Vector2(-1f / depthTexture.Width, 1f / depthTexture.Height)); // I use the depth texture, but I just need the destination render target dimension.
                SetFarPlane(farPlane);
                this.nearPlane = nearPlane;
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Spot Light:: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Light

        /// <summary>
        /// Render to the light pre pass texture.
        /// </summary>
        public void RenderLight(Color diffuseColor, Vector3 position, Vector3 direction, float intensity,
                                float radius, float innerConeAngle, float outerConeAngle, Texture shadowTexture)
        {
            try
            {
                #region Set Parameters
                
                SetLightColor(diffuseColor);
                SetLightPosition(Vector3.Transform(position, viewMatrix));
                SetLightIntensity(intensity);
                SetLightRadius(radius);
                Vector3 directionVS = Vector3.TransformNormal(direction, viewMatrix);
                directionVS.Normalize();
                SetLightDirection(directionVS);
                SetLightInnerConeAngle(innerConeAngle);
                SetLightOuterConeAngle(outerConeAngle);

                // Compute the light world matrix.
                // Scale according to light radius, and translate it to light position.
                Matrix boundingLightObjectWorldMatrix = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);

                SetWorldViewProjMatrix(boundingLightObjectWorldMatrix * viewMatrix * projectionMatrix);
                SetWorldViewMatrix(boundingLightObjectWorldMatrix * viewMatrix);

                if (shadowTexture != null)
                {
                    SetShadowTexture(shadowTexture);
                    Resource.CurrentTechnique = Resource.Techniques["SpotLightWithShadows"];
                }
                else
                    Resource.CurrentTechnique = Resource.Techniques["SpotLight"];

                #endregion

                // Calculate the distance between the camera and light center.
                float cameraToCenter = Vector3.Distance(Matrix.Invert(viewMatrix).Translation, position) - nearPlane;
                // If we are inside the light volume, draw the sphere's inside face.
                if (cameraToCenter <= radius)
                {
                    SetInsideBoundingLightObject(true);
                    EngineManager.Device.RasterizerState = RasterizerState.CullClockwise;
                }
                else
                {
                    SetInsideBoundingLightObject(false);
                    EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                }
                
                Resource.CurrentTechnique.Passes[0].Apply();
                boundingLightObject.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Spot Light: Unable to render.", e);
            }
        } // RenderLight

        #endregion

    } // SpotLightShader
} // XNAFinalEngine.Graphics
