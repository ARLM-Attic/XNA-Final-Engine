
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

        private static DepthStencilState interiorOfBoundingVolumeDepthStencilState;

        // Shader Parameters.
        private static ShaderParameterFloat spLightIntensity, spInvLightRadius, spFarPlane, spLightOuterConeAngle, spLightInnerConeAngle;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterColor spLightColor;
        private static ShaderParameterTexture spDepthTexture, spNormalTexture, spShadowTexture, spLightMaskTexture;
        private static ShaderParameterVector3 spLightPosition, spLightDirection;
        private static ShaderParameterMatrix spWorldViewMatrix, spWorldViewProjMatrix, spViewToLightViewProjMatrix;

        // Techniques references.
        private static EffectTechnique spotLightWithShadowsWithMaskTechnique,
                                       spotLightWithShadowsTechnique,
                                       spotLightWithMaskTechnique,
                                       spotLightTechnique;

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

        #region Constructor

        /// <summary>
        /// Light Pre Pass Spot Light Shader.
        /// </summary>
        private SpotLightShader() : base("LightPrePass\\SpotLight")
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            boundingLightObject = new Sphere(8, 8, 1);
            ContentManager.CurrentContentManager = userContentManager;
            interiorOfBoundingVolumeDepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.Greater,
            };
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
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spLightIntensity = new ShaderParameterFloat("lightIntensity", this);
                spInvLightRadius = new ShaderParameterFloat("invLightRadius", this);
                spFarPlane = new ShaderParameterFloat("farPlane", this);
                spLightOuterConeAngle = new ShaderParameterFloat("lightOuterAngle", this);
                spLightInnerConeAngle = new ShaderParameterFloat("lightInnerAngle", this);
                spLightColor = new ShaderParameterColor("lightColor", this);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
                spNormalTexture = new ShaderParameterTexture("normalTexture", this, SamplerState.PointClamp, 1);
                spShadowTexture = new ShaderParameterTexture("shadowTexture", this, SamplerState.PointClamp, 3);
                spLightMaskTexture = new ShaderParameterTexture("lightMaskTexture", this, SamplerState.LinearClamp, 4);
                spLightPosition = new ShaderParameterVector3("lightPosition", this);
                spLightDirection = new ShaderParameterVector3("lightDirection", this);
                spWorldViewMatrix = new ShaderParameterMatrix("worldView", this);
                spWorldViewProjMatrix = new ShaderParameterMatrix("worldViewProj", this);
                spViewToLightViewProjMatrix = new ShaderParameterMatrix("viewToLightViewProj", this);
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
                spotLightWithShadowsWithMaskTechnique = Resource.Techniques["SpotLightWithShadowsWithMask"];
                spotLightWithShadowsTechnique = Resource.Techniques["SpotLightWithShadows"];
                spotLightWithMaskTechnique = Resource.Techniques["SpotLightWithMask"];
                spotLightTechnique = Resource.Techniques["SpotLight"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

        #endregion

        #region Begin

        /// <summary>
        /// Begins the spot light rendering.
        /// </summary>
        /// <param name="depthTexture">Gbuffer's depth buffer.</param>
        /// <param name="normalTexture">Gbuffer's normal map.</param>
        /// <param name="viewMatrix">Camera view matrix.</param>
        /// <param name="projectionMatrix">Camera projection matrix.</param>
        /// <param name="nearPlane">Camera near plane.</param>
        /// <param name="farPlane">Camera far plane.</param>
        internal void Begin(RenderTarget depthTexture, RenderTarget normalTexture,
                            Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane)
        {
            try
            {
                spDepthTexture.Value = depthTexture;
                spNormalTexture.Value = normalTexture;
                spHalfPixel.Value = new Vector2(0.5f / depthTexture.Width, 0.5f / depthTexture.Height); // I use the depth texture, but I just need the destination render target dimension.
                spFarPlane.Value = farPlane;
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

        #region Render

        /// <summary>
        /// Render the spot light.
        /// </summary>
        public void Render(Color diffuseColor, Vector3 position, Vector3 direction, float intensity,
                           float range, float innerConeAngle, float outerConeAngle, Texture shadowTexture, Texture lightMaskTexture)
        {
            try
            {

                #region Set Parameters
                
                spLightColor.Value = diffuseColor;
                spLightPosition.Value = Vector3.Transform(position, viewMatrix);
                spLightIntensity.Value = intensity;
                spInvLightRadius.Value = 1 / range;
                Vector3 directionVS = Vector3.TransformNormal(direction, viewMatrix);
                directionVS.Normalize();
                spLightDirection.Value = directionVS;
                spLightInnerConeAngle.Value = innerConeAngle * (3.141592f / 180.0f);
                spLightOuterConeAngle.Value = outerConeAngle * (3.141592f / 180.0f);

                if (lightMaskTexture != null)
                {
                    Matrix lightViewMatrix = Matrix.CreateLookAt(position, position + direction, Vector3.Up);
                    Matrix lightProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(outerConeAngle * (float)Math.PI / 180.0f, // field of view
                                                                                       1.0f,   // Aspect ratio
                                                                                       1f,   // Near plane
                                                                                       range); // Far plane
                    spViewToLightViewProjMatrix.Value = Matrix.Invert(viewMatrix) * lightViewMatrix * lightProjectionMatrix;
                    spLightMaskTexture.Value = lightMaskTexture;
                }
                else
                    spLightMaskTexture.Value = Texture.BlackTexture; // To avoid a potential exception.

                // Compute the light world matrix.
                // Scale according to light radius, and translate it to light position.
                Matrix boundingLightObjectWorldMatrix = Matrix.CreateScale(range) * Matrix.CreateTranslation(position);

                spWorldViewProjMatrix.Value = boundingLightObjectWorldMatrix * viewMatrix * projectionMatrix;
                spWorldViewMatrix.Value = boundingLightObjectWorldMatrix * viewMatrix;

                if (shadowTexture != null)
                {
                    spShadowTexture.Value = shadowTexture;
                    Resource.CurrentTechnique = lightMaskTexture != null ? spotLightWithShadowsWithMaskTechnique : spotLightWithShadowsTechnique;
                }
                else
                {
                    spShadowTexture.Value = Texture.BlackTexture; // To avoid a potential exception.
                    Resource.CurrentTechnique = lightMaskTexture != null ? spotLightWithMaskTechnique : spotLightTechnique;
                }

                #endregion

                // Calculate the distance between the camera and light center.
                float cameraToCenter = Vector3.Distance(Matrix.Invert(viewMatrix).Translation, position) - nearPlane;
                // If we are inside the light volume, draw the sphere's inside face.
                if (cameraToCenter <= range) 
                {
                    EngineManager.Device.DepthStencilState = interiorOfBoundingVolumeDepthStencilState;
                    EngineManager.Device.RasterizerState = RasterizerState.CullClockwise;
                }
                else
                {
                    EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
                    EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                }

                Resource.CurrentTechnique.Passes[0].Apply();
                boundingLightObject.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Spot Light: Unable to render.", e);
            }
        } // Render

        #endregion

    } // SpotLightShader
} // XNAFinalEngine.Graphics
