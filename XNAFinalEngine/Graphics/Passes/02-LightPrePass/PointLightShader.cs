
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
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Light Pre Pass Point Light Shader.
    /// </summary>
    internal class PointLightShader : Shader
    {

        #region Variables

        // Camera values
        private float nearPlane, fieldOfView;

        // Current view matrix. Used to set the shader parameters.
        private Matrix viewMatrix, projectionMatrix;

        // Bounding Light Object.
        private static Model boundingLightObject;

        // Singleton reference.
        private static PointLightShader instance;

        private readonly BlendState stencilBlendState, lightBlendState;
        private readonly DepthStencilState stencilDepthStencilState, lightDepthStencilState, lightNotStencilDepthStencilState;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a point light shader.
        /// </summary>
        public static PointLightShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new PointLightShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epInvLightRadius,
                                       epDepthTexture,
                                       epNormalTexture,
                                       epLightColor,
                                       epLightPosition,
                                       epLlightIntensity,
                                       epFarPlane,
                                       epWorldViewProj,
                                       epWorldView;


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

        #region Light Position

        private static Vector3 lastUsedLightPosition;
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

        #region Inverse Light Radius

        private static float lastUsedInvLightRadius;
        private static void SetInvLightRadius(float lightRadius)
        {
            if (lastUsedInvLightRadius != lightRadius)
            {
                lastUsedInvLightRadius = lightRadius;
                epInvLightRadius.SetValue(lightRadius);
            }
        } // SetInvLightRadius

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
        
        #region Matrices

        private static Matrix lastUsedWorldViewMatrix;
        private static void SetWorldViewMatrix(Matrix worldViewMatrix)
        {
            if (lastUsedWorldViewMatrix != worldViewMatrix)
            {
                lastUsedWorldViewMatrix = worldViewMatrix;
                epWorldView.SetValue(worldViewMatrix);
            }
        } // SetWorldViewMatrix

        private static Matrix lastUsedWorldViewProjMatrix;
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

        #region Constructor

        /// <summary>
        /// Light Pre Pass Point Light Shader.
        /// </summary>
        private PointLightShader() : base("LightPrePass\\PointLight")
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            //boundingLightObject = new Sphere(6, 6, 1);   // Algorithmically generated mesh normally sucks when optimized vertex access is needed.
            boundingLightObject = new FileModel("Sphere"); // Exported models for the contrary are great.
            ContentManager.CurrentContentManager = userContentManager;
            
            stencilBlendState = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.None,
                ColorWriteChannels1 = ColorWriteChannels.None,
            };
            lightBlendState = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.One,
            };
            stencilDepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.Less,
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilDepthBufferFail = StencilOperation.Replace,
                ReferenceStencil = 1,
            };
            lightDepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.Greater,
                StencilEnable = true,
                StencilFunction = CompareFunction.NotEqual,
                ReferenceStencil = 1,
            };
        } // PointLightShader

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
                epLightPosition                    = Resource.Parameters["lightPosition"];
                    epLightPosition.SetValue(lastUsedLightPosition);
                epLlightIntensity                  = Resource.Parameters["lightIntensity"];
                    epLlightIntensity.SetValue(lastUsedLightIntensity);
                epInvLightRadius                      = Resource.Parameters["invLightRadius"];
                    epInvLightRadius.SetValue(lastUsedInvLightRadius);
                epDepthTexture                     = Resource.Parameters["depthTexture"];
                    if (lastUsedDepthTexture != null && !lastUsedDepthTexture.IsDisposed)
                        epDepthTexture.SetValue(lastUsedDepthTexture);
                epNormalTexture                    = Resource.Parameters["normalTexture"];
                    if (lastUsedNormalTexture != null && !lastUsedNormalTexture.IsDisposed)
                        epNormalTexture.SetValue(lastUsedNormalTexture);
                epFarPlane                         = Resource.Parameters["farPlane"];
                    epFarPlane.SetValue(lastUsedFarPlane);
                epWorldViewProj                    = Resource.Parameters["worldViewProj"];
                    epWorldViewProj.SetValue(lastUsedWorldViewProjMatrix);
                epWorldView                        = Resource.Parameters["worldView"];
                    epWorldView.SetValue(lastUsedWorldViewMatrix);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters
        
        #endregion

        #region Begin

        /// <summary>
        /// Begins the point light rendering.
        /// </summary>
        /// <param name="depthTexture">Gbuffer's depth buffer.</param>
        /// <param name="normalTexture">Gbuffer's normal map.</param>
        /// <param name="viewMatrix">Camera view matrix.</param>
        /// <param name="projectionMatrix">Camera projection matrix.</param>
        /// <param name="nearPlane">Camera near plane.</param>
        /// <param name="farPlane">Camera far plane.</param>
        internal void Begin(RenderTarget depthTexture, RenderTarget normalTexture,
                            Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane, float fov)
        {
            try
            {
                SetDepthTexture(depthTexture);
                SetNormalTexture(normalTexture);
                SetHalfPixel(new Vector2(0.5f / depthTexture.Width, 0.5f / depthTexture.Height)); // I use the depth texture, but I just need the destination render target dimension.
                SetFarPlane(farPlane);
                this.nearPlane = nearPlane;
                this.fieldOfView = fov;
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Point Light:: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Light

        /// <summary>
        /// Render to the light pre pass texture.
        /// </summary>
        public void RenderLight(Color diffuseColor, Vector3 position, float intensity, float radius, Shadow shadow)
        {
            try
            {
                // Long time ago, when I was started the deferred lighting pipeline I read that it is possible to use the depth information 
                // and the stencil buffer to mark in a two pass rendering exactly what pixels are affected by the light. 
                // This helps to reduce pixel shader load but at the same time allows implementing clip volumes. 
                // With clip volumes you can put, for example, a box and the light won’t bleed outside this box even if the radius is bigger.
                // I.e. you can place lights in a wall and the opposite side of that wall won’t be illuminated.
                //
                // The problem is I don’t have the Z-Buffer available because XNA 4 does not allow sharing depth buffers between render targets. 
                // However I can reconstruct the Z-Buffer with a shader and my G-Buffer.
                // 
                // If you don’t use custom clip volumes (i.e. we use the default sphere) and the light is too far then we could have more vertex processing 
                // than pixel processing. Some games use glow planes (a colored mask) to see the light’s bright when they are far away, 
                // this is good for open environment games but not for interior games. 
                // Instead games like Killzone 2 ignore the first pass on these lights and only compute the second (and this second pass still does one
                // part of the filter). Also the far plane "problem" is addressed in this optimization.
                //
                // Another optimization that I made is the use of a Softimage sphere instead of my procedural spheres. 
                // Models exported in this kind of tools are optimized for accessing. For example my stress test changes from 20/21 frames to 22 frames. 
                // Not a big change, but still a change nevertheless.
                //
                // I also research the possibility to use instancing with some lights.
                // But no article talk about this technique so I try to think why is not useful and it was easy to find that:
                // 1) It can be only used with spheres (not custom clip volumes).
                // 2) The dynamic buffers used for the instancing information could be too dynamic or difficult to maintain.
                // 3) The stencil optimization could be very important on interior games and could not be mixed with instancing and custom clip volumes.
                // Extra complexity added (including the use of vfetch for Xbox 360).
                
                // Fill the stencil buffer with 0s.
                EngineManager.Device.Clear(ClearOptions.Stencil, Color.White, 1.0f, 0);

                #region Set Parameters

                SetLightColor(diffuseColor);
                SetLightPosition(Vector3.Transform(position, viewMatrix));
                SetLightIntensity(intensity);
                SetInvLightRadius(1 / radius);

                if (shadow != null && shadow is CubeShadow)
                {
                    // TODO!!!
                    Resource.Parameters["cubeShadowTexture"].SetValue(((CubeShadow)shadow).LightDepthTexture.Resource);
                    Resource.Parameters["viewI"].SetValue(Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix))));
                }

                // Compute the light world matrix.
                // Scale according to light radius, and translate it to light position.
                Matrix boundingLightObjectWorldMatrix = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position); // This operation could be optimized.
                SetWorldViewProjMatrix(boundingLightObjectWorldMatrix * viewMatrix * projectionMatrix);
                SetWorldViewMatrix(boundingLightObjectWorldMatrix * viewMatrix);

                #endregion

                // http://en.wikipedia.org/wiki/Angular_diameter
                // The formula was inspired from Guerilla´s GDC 09 presentation.
                float distanceToCamera = Vector3.Distance(Matrix.Invert(viewMatrix).Translation, position) - nearPlane;
                float angularDiameter = (float)(2 * Math.Atan(radius / distanceToCamera));
                if (angularDiameter > 0.2f * (3.1416f * fieldOfView / 180.0f))
                {
                    // This only works when the clip volume does not intercept the camera´s far plane.

                    // First pass.
                    // The stencil buffer was already filled with 0 and if the back of the clip volume
                    // is in front of the geometry then it marks the pixel as useful.
                    // I prefer to do it in that way because when the clip volume intercept the camera’s near plane 
                    // we don’t need to perform a special case and we still have custom volume support.
                    Resource.CurrentTechnique = Resource.Techniques["PointLightStencil"];
                    EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                    EngineManager.Device.BlendState = stencilBlendState;
                    EngineManager.Device.DepthStencilState = stencilDepthStencilState;
                    Resource.CurrentTechnique.Passes[0].Apply();
                    boundingLightObject.Render();

                    // Second pass.
                    // Render the clip volume back faces with the light shader.
                    // The pixel with stencil value of 1 that are in front of the geometry will be discarded.
                    if (shadow != null && shadow is CubeShadow)
                        Resource.CurrentTechnique = Resource.Techniques["PointLightWithShadows"];
                    else
                        Resource.CurrentTechnique = Resource.Techniques["PointLight"];
                    EngineManager.Device.RasterizerState = RasterizerState.CullClockwise;
                    EngineManager.Device.BlendState = lightBlendState;
                    EngineManager.Device.DepthStencilState = lightDepthStencilState;
                    Resource.CurrentTechnique.Passes[0].Apply();
                    boundingLightObject.Render();
                }
                else // Far lights
                {
                    // Render the clip volume front faces with the light shader.
                    Resource.CurrentTechnique = Resource.Techniques["PointLight"];
                    EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                    EngineManager.Device.BlendState = lightBlendState;
                    EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                    Resource.CurrentTechnique.Passes[0].Apply();
                    boundingLightObject.Render();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Point Light: Unable to render.", e);
            }
        } // RenderLight

        #endregion

    } // PointLightShader
} // XNAFinalEngine.Graphics
