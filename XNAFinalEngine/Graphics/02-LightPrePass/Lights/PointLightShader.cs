
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
using Model = XNAFinalEngine.Assets.Model;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
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

        // Render states used in the multipass render.
        private readonly BlendState stencilBlendState, lightBlendState;
        private readonly DepthStencilState stencilDepthStencilState, lightDepthStencilState;

        // Shader Parameters.
        private static ShaderParameterFloat spLightIntensity, spInvLightRadius, spFarPlane;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterColor spLightColor;
        private static ShaderParameterTexture spDepthTexture, spNormalTexture;
        private static ShaderParameterTextureCube spShadowTexture;
        private static ShaderParameterVector3 spLightPosition, spTextureSize, spTextureSizeInv;
        private static ShaderParameterMatrix spWorldView, spWorldViewProj, spViewInverse;

        // Techniques references.
        private static EffectTechnique pointLightStencilTechnique,
                                       pointLightTechnique,
                                       pointLightWithShadowsTechnique;

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

        #region Constructor

        /// <summary>
        /// Light Pre Pass Point Light Shader.
        /// </summary>
        private PointLightShader() : base("LightPrePass\\PointLight")
        {
            AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
            AssetContentManager.CurrentContentManager = AssetContentManager.SystemContentManager;
            //boundingLightObject = new Sphere(6, 6, 1);   // Algorithmically generated mesh normally sucks when optimized vertex access is needed.
            boundingLightObject = new FileModel("Sphere"); // Exported models for the contrary are great.
            AssetContentManager.CurrentContentManager = userContentManager;
            
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
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spLightIntensity = new ShaderParameterFloat("lightIntensity", this);
                spInvLightRadius = new ShaderParameterFloat("invLightRadius", this);
                spFarPlane = new ShaderParameterFloat("farPlane", this);
                spLightColor = new ShaderParameterColor("lightColor", this);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
                spNormalTexture = new ShaderParameterTexture("normalTexture", this, SamplerState.PointClamp, 1);
                spShadowTexture = new ShaderParameterTextureCube("cubeShadowTexture", this, SamplerState.PointClamp, 3);
                spLightPosition = new ShaderParameterVector3("lightPosition", this);
                spTextureSize = new ShaderParameterVector3("textureSize", this);
                spTextureSizeInv = new ShaderParameterVector3("textureSizeInv", this);
                spWorldView = new ShaderParameterMatrix("worldView", this);
                spWorldViewProj = new ShaderParameterMatrix("worldViewProj", this);
                spViewInverse = new ShaderParameterMatrix("viewI", this);
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
                pointLightStencilTechnique = Resource.Techniques["PointLightStencil"];
                pointLightTechnique = Resource.Techniques["PointLight"];
                pointLightWithShadowsTechnique = Resource.Techniques["PointLightWithShadows"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

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
        /// <param name="fieldOfView">Camera field of view.</param>
        internal void Begin(RenderTarget depthTexture, RenderTarget normalTexture, Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane, float fieldOfView)
        {
            try
            {
                spDepthTexture.Value = depthTexture;
                spNormalTexture.Value = normalTexture;
                spHalfPixel.Value = new Vector2(0.5f / depthTexture.Width, 0.5f / depthTexture.Height); // I use the depth texture, but I just need the destination render target dimension.
                spFarPlane.Value = farPlane;
                this.nearPlane = nearPlane;
                this.fieldOfView = fieldOfView;
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Point Light:: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render

        /// <summary>
        /// Render the point light.
        /// </summary>
        public void Render(Color diffuseColor, Vector3 position, float intensity, float radius, TextureCube shadowTexture, Matrix worldMatrix, bool renderClipVolumeInLocalSpace, Model clipVolume = null)
        {
            try
            {
                // It is possible to use the depth information and the stencil buffer to mark in a two pass rendering exactly what pixels are affected by the light. 
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

                spLightColor.Value = diffuseColor;
                spLightPosition.Value = Vector3.Transform(position, viewMatrix);
                spLightIntensity.Value = intensity;
                spInvLightRadius.Value = 1 / radius;

                if (shadowTexture != null)
                {
                    spShadowTexture.Value = shadowTexture;
                    spViewInverse.Value = Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix)));
                    spTextureSize.Value = new Vector3(shadowTexture.Size, shadowTexture.Size, shadowTexture.Size);
                    spTextureSizeInv.Value = new Vector3(1.0f / shadowTexture.Size, 1.0f / shadowTexture.Size, 1.0f / shadowTexture.Size);
                }
                else
                    spShadowTexture.Value = TextureCube.BlackTexture;

                // Compute the light world matrix.
                Matrix boundingLightObjectWorldMatrix;
                if (clipVolume != null)
                    boundingLightObjectWorldMatrix = renderClipVolumeInLocalSpace ? Matrix.Identity : worldMatrix;
                else
                    // Scale according to light radius, and translate it to light position.
                    boundingLightObjectWorldMatrix = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);

                spWorldViewProj.Value = boundingLightObjectWorldMatrix * viewMatrix * projectionMatrix;
                spWorldView.Value = boundingLightObjectWorldMatrix * viewMatrix;

                #endregion

                // http://en.wikipedia.org/wiki/Angular_diameter
                // The formula was inspired from Guerilla´s GDC 09 presentation.
                float distanceToCamera = Vector3.Distance(Matrix.Invert(viewMatrix).Translation, position);
                float angularDiameter = (float)(2 * Math.Atan(radius / distanceToCamera));
                if (angularDiameter > 0.2f * (3.1416f * fieldOfView / 180.0f)) // 0.2f is the original value.
                {
                    // This only works when the clip volume does not intercept the camera´s far plane.

                    // First pass.
                    // The stencil buffer was already filled with 0 and if the back of the clip volume
                    // is in front of the geometry then it marks the pixel as useful.
                    // I prefer to do it in that way because when the clip volume intercept the camera’s near plane 
                    // we don’t need to perform a special case and we still have custom volume support.
                    Resource.CurrentTechnique = pointLightStencilTechnique;
                    EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                    EngineManager.Device.BlendState = stencilBlendState;
                    EngineManager.Device.DepthStencilState = stencilDepthStencilState;
                    Resource.CurrentTechnique.Passes[0].Apply();
                    if (clipVolume != null)
                        clipVolume.Render();
                    else
                        boundingLightObject.Render();

                    // Second pass.
                    // Render the clip volume back faces with the light shader.
                    // The pixel with stencil value of 1 that are in front of the geometry will be discarded.
                    Resource.CurrentTechnique = shadowTexture != null ? pointLightWithShadowsTechnique : pointLightTechnique;
                    EngineManager.Device.RasterizerState = RasterizerState.CullClockwise;
                    EngineManager.Device.BlendState = lightBlendState;
                    EngineManager.Device.DepthStencilState = lightDepthStencilState;
                    Resource.CurrentTechnique.Passes[0].Apply();
                    if (clipVolume != null)
                        clipVolume.Render();
                    else
                        boundingLightObject.Render();
                }
                else // Far lights
                {
                    // Render the clip volume front faces with the light shader.
                    Resource.CurrentTechnique = shadowTexture != null ? pointLightWithShadowsTechnique : pointLightTechnique;
                    EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                    //EngineManager.Device.BlendState = lightBlendState; // Not need to set it.
                    EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
                    Resource.CurrentTechnique.Passes[0].Apply();
                    if (clipVolume != null)
                        clipVolume.Render();
                    else
                        boundingLightObject.Render();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Point Light: Unable to render.", e);
            }
        } // Render

        #endregion

    } // PointLightShader
} // XNAFinalEngine.Graphics
