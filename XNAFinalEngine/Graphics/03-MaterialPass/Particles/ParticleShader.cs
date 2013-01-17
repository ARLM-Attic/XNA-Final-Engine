
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
using XNAFinalEngine.Helpers;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Particle System.
    /// </summary>
    internal class ParticleShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static ParticleShader instance;
        
        // Shader Parameters.
        private static ShaderParameterMatrix spViewProjection, spProjection, spProjectionInvert;
        private static ShaderParameterFloat spCurrentTime, spDuration, spDurationRandomness, spEndVelocity, spFarPlane, spFadeDistance;
        private static ShaderParameterVector2 spViewportScale, spRotateSpeed, spStartSize, spEndSize, spHalfPixel, spTiles;
        private static ShaderParameterVector3 spGravity;
        private static ShaderParameterColorWithAlpha spMinColor, spMaxColor;
        private static ShaderParameterTexture spTexture, spDepthTexture;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static ParticleShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new ParticleShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Particle System. The settings are loaded from file.
        /// </summary>
        public ParticleShader() : base("Particles\\Particles") { }

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override sealed void GetParametersHandles()
        {
            try
            {
                spViewProjection = new ShaderParameterMatrix("viewProjection", this);
                spProjection = new ShaderParameterMatrix("Projection", this);
                spProjectionInvert = new ShaderParameterMatrix("ProjI", this);
                spCurrentTime = new ShaderParameterFloat("CurrentTime", this);
                spDuration = new ShaderParameterFloat("Duration", this);
                spDurationRandomness = new ShaderParameterFloat("DurationRandomness", this);
                spEndVelocity = new ShaderParameterFloat("EndVelocity", this);
                spFarPlane = new ShaderParameterFloat("farPlane", this);
                spFadeDistance = new ShaderParameterFloat("fadeDistance", this);
                spViewportScale = new ShaderParameterVector2("ViewportScale", this);
                spRotateSpeed = new ShaderParameterVector2("RotateSpeed", this);
                spStartSize = new ShaderParameterVector2("StartSize", this);
                spEndSize = new ShaderParameterVector2("EndSize", this);
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spTiles = new ShaderParameterVector2("tiles", this);
                spGravity = new ShaderParameterVector3("Gravity", this);
                spMinColor = new ShaderParameterColorWithAlpha("MinColor", this);
                spMaxColor = new ShaderParameterColorWithAlpha("MaxColor", this);
                spTexture = new ShaderParameterTexture("particleTexture", this, SamplerState.LinearClamp, 3);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParametersHandles

        #endregion

        #region Begin

        /// <summary>
        /// Begins the render.
        /// </summary>
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix, float aspectRatio, float farPlane, Size cameraSize, Texture depthTexture)
        {
            try
            {
                // Set initial parameters
                spViewProjection.Value = viewMatrix * projectionMatrix;
                spProjection.Value = projectionMatrix;
                spProjectionInvert.Value = Matrix.Invert(projectionMatrix);

                spViewportScale.Value = new Vector2(0.5f / aspectRatio, -0.5f);
                spFarPlane.Value = farPlane;
                spHalfPixel.Value = new Vector2(0.5f / cameraSize.Width, 0.5f / cameraSize.Height); // The half depth size produce flickering.
                spDepthTexture.Value = depthTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Particle Shader: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render
        
        /// <summary>
        /// Draws a particle system.
        /// </summary>
        public void Render(ParticleSystem particleSystem, float duration, BlendState blendState, float durationRandomness,
                                    Vector3 gravity, float endVelocity, Color minimumColor, Color maximumColor, Vector2 rotateSpeed,
                                    Vector2 startSize, Vector2 endSize, Texture texture, int tilesX, int tilesY, bool softParticles, float fadeDistance)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = blendState;

                // Set parameters
                // Set an effect parameter describing the current time. All the vertex shader particle animation is keyed off this value.
                spCurrentTime.Value = particleSystem.CurrentTime;
                // Surface
                spDuration.Value = duration;
                spDurationRandomness.Value = durationRandomness;
                spGravity.Value = gravity;
                spEndVelocity.Value = endVelocity;
                spMinColor.Value = minimumColor;
                spMaxColor.Value = maximumColor;
                spRotateSpeed.Value = rotateSpeed;
                spStartSize.Value = startSize;
                spEndSize.Value = endSize;
                // Texture
                spTexture.Value = texture;
                spTiles.Value = new Vector2(tilesX, tilesY);
                // Soft Particles
                if (softParticles)
                {
                    spFadeDistance.Value = fadeDistance;
                }

                Resource.CurrentTechnique = softParticles ? Resource.Techniques["SoftParticles"] : Resource.Techniques["HardParticles"];
                Resource.CurrentTechnique.Passes[0].Apply();

                // Render particle system.
                particleSystem.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Particle Shader: Unable to render the particle system.", e);
            }
        } // Render

        #endregion
        
    } // ParticleShader
} // XNAFinalEngine.Graphics
