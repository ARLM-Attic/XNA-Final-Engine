
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Light Pre Pass Point Light.
    /// This class could be mixed with the directionalLight class or could be coded with the partial class command.
    /// </summary>
    internal static class LightPrePassPointLight
    {

        #region Variables

        /// <summary>
        /// Bounding Light Object.
        /// </summary>
        private static Model boundingLightObject;

        #endregion

        #region Shader Parameters

        /// <summary>
        /// The shader effect.
        /// </summary>
        private static Effect Effect { get; set; }

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epLightRadius,
                                       epDepthTexture,
                                       epNormalTexture,
                                       epMotionVectorSpecularPowerTexture,
                                       epLightColor,
                                       epLightPosition,
                                       epLlightIntensity,
                                       epFarPlane,
                                       epWorldViewProj,
                                       epWorldView,
                                       epInsideBoundingLightObject;


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

        private static Texture lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedDepthTexture != depthTexture)
            {
                lastUsedDepthTexture = depthTexture;
                epDepthTexture.SetValue(depthTexture.XnaTexture);
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
                epNormalTexture.SetValue(normalTexture.XnaTexture);
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
                epMotionVectorSpecularPowerTexture.SetValue(motionVectorSpecularPower.XnaTexture);
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

        #endregion

        #region Load Shader

        /// <summary>
        /// Load shader
        /// </summary>
        private static void LoadShader()
        {
            const string filename = "LightPrePass\\PointLight";
            // Load shader
            try
            {
                Effect = EngineManager.SystemContent.Load<Effect>(Path.Combine(Directories.ShadersDirectory, filename));
            } // try
            catch
            {
                throw new Exception("Unable to load the shader " + filename);
            } // catch
            GetParametersHandles();
        } // LoadShader

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        private static void GetParametersHandles()
        {
            try
            {
                epHalfPixel                        = Effect.Parameters["halfPixel"];
                epLightColor                       = Effect.Parameters["lightColor"];
                epLightPosition                    = Effect.Parameters["lightPosition"];
                epLlightIntensity                  = Effect.Parameters["lightIntensity"];
                epLightRadius                      = Effect.Parameters["lightRadius"];
                epDepthTexture                     = Effect.Parameters["depthTexture"];
                epNormalTexture                    = Effect.Parameters["normalTexture"];
                epMotionVectorSpecularPowerTexture = Effect.Parameters["motionVectorSpecularPowerTexture"];
                epFarPlane                         = Effect.Parameters["farPlane"];
                epWorldViewProj                    = Effect.Parameters["worldViewProj"];
                epWorldView                        = Effect.Parameters["worldView"];
                epInsideBoundingLightObject        = Effect.Parameters["insideBoundingLightObject"];
            }
            catch
            {
                throw new Exception("Get the handles from the light pre pass point light shader failed.");
            }
        } // GetParameters

        #endregion

        #region Render

        /// <summary>
        /// Render to the light pre pass texture.
        /// </summary>
        public static void Render(PointLight light, RenderTarget depthTexture, RenderTarget normalTexture, RenderTarget motionVectorSpecularPowerTexture, RenderTarget lightPrePassMap)
        {
            if (Effect == null)
            {
                LoadShader();
                boundingLightObject = new Sphere(50, 50, 1);
            }
            try
            {
                #region Set Parameters

                SetHalfPixel(new Vector2(0.5f / depthTexture.Width, 0.5f / depthTexture.Height));
                SetLightColor(light.DiffuseColor);
                SetLightPosition(Vector3.Transform(light.Position, ApplicationLogic.Camera.ViewMatrix));
                SetLightIntensity(light.Intensity);
                SetLightRadius(light.Radius);
                SetDepthTexture(depthTexture);
                SetNormalTexture(normalTexture);
                SetMotionVectorSpecularPower(motionVectorSpecularPowerTexture);
                SetFarPlane(ApplicationLogic.Camera.FarPlane);

                // Compute the light world matrix.
                // Scale according to light radius, and translate it to light position.
                Matrix boundingLightObjectWorldMatrix = Matrix.CreateScale(light.Radius) * Matrix.CreateTranslation(light.Position);

                SetWorldViewProjMatrix(boundingLightObjectWorldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix);
                SetWorldViewMatrix(boundingLightObjectWorldMatrix * ApplicationLogic.Camera.ViewMatrix);

                #endregion

                // Calculate the distance between the camera and light center.
                float cameraToCenter = Vector3.Distance(ApplicationLogic.Camera.Position, light.Position) - ApplicationLogic.Camera.NearPlane;
                // If we are inside the light volume, draw the sphere's inside face.
                if (cameraToCenter < light.Radius)
                {
                    SetInsideBoundingLightObject(true);
                    EngineManager.Device.RasterizerState = RasterizerState.CullClockwise;
                }
                else
                {
                    SetInsideBoundingLightObject(false);
                    EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                }
                
                Effect.CurrentTechnique = Effect.Techniques["PointLight"];
                Effect.CurrentTechnique.Passes[0].Apply();
                boundingLightObject.Render();
            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the light pre pass point light shader" + e.Message);
            }
        } // Render

        #endregion

    } // LightPrePassPointLights
} // XNAFinalEngine.Graphics
