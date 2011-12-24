
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
using XNAFinalEngine.Helpers;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Particle System.
    /// </summary>
    public class ParticleShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static ParticleShader instance;

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

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        private static EffectParameter
                               // Matrices //
                               epViewProjection,
                               epProjection,
                               // Surface //   
                               epViewportScale,
                               epCurrentTime,
                               epDuration,
                               epDurationRandomness,
                               epGravity,
                               epEndVelocity,
                               epMinColor,
                               epMaxColor,
                               epRotateSpeed,
                               epStartSize,
                               epEndSize,
                               // Textures //
                               epTexture,
                               // Soft Particles //
                               epProjectionInvert,
                               epFarPlane,
                               epDepthTexture,
                               epHalfPixel,
                               epFadeDistance;

        #region Matrices

        private static Matrix? lastUsedViewProjectionMatrix;
        private static void SetViewProjectionMatrix(Matrix viewProjectionMatrix)
        {
            if (lastUsedViewProjectionMatrix != viewProjectionMatrix)
            {
                lastUsedViewProjectionMatrix = viewProjectionMatrix;
                epViewProjection.SetValue(viewProjectionMatrix);
            }
        } // SetViewProjectionMatrix

        private static Matrix? lastUsedProjectionMatrix;
        private static void SetProjectionMatrix(Matrix projectionMatrix)
        {
            if (lastUsedProjectionMatrix != projectionMatrix)
            {
                lastUsedProjectionMatrix = projectionMatrix;
                epProjection.SetValue(projectionMatrix);
            }
        } // SetProjectionMatrix

        private static Matrix? lastUsedProjectionInvertMatrix;
        private static void SetProjectionInvertMatrix(Matrix projectionInvertMatrix)
        {
            if (lastUsedProjectionInvertMatrix != projectionInvertMatrix)
            {
                lastUsedProjectionInvertMatrix = projectionInvertMatrix;
                epProjectionInvert.SetValue(projectionInvertMatrix);
            }
        } // SetProjectionInvertMatrix

        #endregion

        #region Viewport Scale

        private static Vector2? lastUsedViewportScale;
        private static void SetViewportScale(Vector2 _viewportScale)
        {
            if (lastUsedViewportScale != _viewportScale)
            {
                lastUsedViewportScale = _viewportScale;
                epViewportScale.SetValue(_viewportScale);
            }
        } // SetViewportScale

        #endregion

        #region Current Time

        private static float? lastUsedCurrentTime;
        private static void SetCurrentTime(float _currentTime)
        {
            if (lastUsedCurrentTime != _currentTime && _currentTime >= 0.0f)
            {
                lastUsedCurrentTime = _currentTime;
                epCurrentTime.SetValue(_currentTime);
            }
        } // SetCurrentTime

        #endregion

        #region Surface

        #region Duration

        private static float? lastUsedDuration;
        private static void SetDuration(float _duration)
        {
            if (lastUsedDuration != _duration && _duration >= 0.0f)
            {
                lastUsedDuration = _duration;
                epDuration.SetValue(_duration);
            }
        } // SetDuration

        #endregion

        #region Duration Randomness

        private static float? lastUsedDurationRandomness;
        private static void SetDurationRandomness(float _durationRandomness)
        {
            if (lastUsedDurationRandomness != _durationRandomness && _durationRandomness >= 0.0f)
            {
                lastUsedDurationRandomness = _durationRandomness;
                epDurationRandomness.SetValue(_durationRandomness);
            }
        } // SetDurationRandomness

        #endregion

        #region Gravity

        private static Vector3? lastUsedGravity;
        protected void SetGravity(Vector3 _gravity)
        {
            if (lastUsedGravity != _gravity)
            {
                lastUsedGravity = _gravity;
                epGravity.SetValue(_gravity);
            }
        } // SetGravity

        #endregion

        #region End velocity

        private static float? lastUsedEndVelocity;
        private static void SetEndVelocity(float _endVelocity)
        {
            if (lastUsedEndVelocity != _endVelocity && _endVelocity >= 0.0f)
            {
                lastUsedEndVelocity = _endVelocity;
                epEndVelocity.SetValue(_endVelocity);
            }
        } // SetEndVelocity

        #endregion

        #region Minimum Color

        private static Color? lastUsedMinimumColor;
        protected void SetMinimumColor(Color _minimumColor)
        {
            if (lastUsedMinimumColor != _minimumColor)
            {
                lastUsedMinimumColor = _minimumColor;
                epMinColor.SetValue(new Vector4(_minimumColor.R / 255f, _minimumColor.G / 255f, _minimumColor.B / 255f, _minimumColor.A / 255f));
            }
        } // SetMinimumColor

        #endregion

        #region Maximum Color

        private static Color? lastUsedMaximumColor;
        protected void SetMaximumColor(Color _maximumColor)
        {
            if (lastUsedMaximumColor != _maximumColor)
            {
                lastUsedMaximumColor = _maximumColor;

                epMaxColor.SetValue(new Vector4(_maximumColor.R / 255f, _maximumColor.G / 255f, _maximumColor.B / 255f, _maximumColor.A / 255f));
            }
        } // SetMaximumColor

        #endregion

        #region Rotate Speed

        private static Vector2? lastUsedRotateSpeed;
        private static void SetRotateSpeed(Vector2 _rotateSpeed)
        {
            if (lastUsedRotateSpeed != _rotateSpeed)
            {
                lastUsedRotateSpeed = _rotateSpeed;
                epRotateSpeed.SetValue(_rotateSpeed);
            }
        } // SetRotateSpeed

        #endregion

        #region Start Size

        private static Vector2? lastUsedStartSize;
        private static void SetStartSize(Vector2 _startSize)
        {
            if (lastUsedStartSize != _startSize)
            {
                lastUsedStartSize = _startSize;
                epStartSize.SetValue(_startSize);
            }
        } // SetStartSize

        #endregion

        #region End Size

        private static Vector2? lastUsedEndSize;
        private static void SetEndSize(Vector2 _endSize)
        {
            if (lastUsedEndSize != _endSize)
            {
                lastUsedEndSize = _endSize;
                epEndSize.SetValue(_endSize);
            }
        } // SetEndSize

        #endregion

        #endregion

        #region Texture

        private static Texture2D lastUsedTexture;
        private static void SetTexture(Texture texture)
        {
            EngineManager.Device.SamplerStates[3] = SamplerState.LinearWrap;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedTexture != texture.Resource)
            {
                lastUsedTexture = texture.Resource;
                epTexture.SetValue(texture.Resource);
            }
        } // SetDiffuseTexture

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

        #region Fade Distance

        private static float? lastFadeDistance;
        private static void SetFadeDistance(float fadeDistance)
        {
            if (lastFadeDistance != fadeDistance)
            {
                lastFadeDistance = fadeDistance;
                epFadeDistance.SetValue(fadeDistance);
            }
        } // SetFadeDistance

        #endregion

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
                // Matrices //
                epViewProjection     = Resource.Parameters["viewProjection"];
                epProjection         = Resource.Parameters["Projection"];
                // Surface //    
                epViewportScale      = Resource.Parameters["ViewportScale"];
                epCurrentTime        = Resource.Parameters["CurrentTime"];
                epDuration           = Resource.Parameters["Duration"];
                epDurationRandomness = Resource.Parameters["DurationRandomness"];
                epGravity            = Resource.Parameters["Gravity"];
                epEndVelocity        = Resource.Parameters["EndVelocity"];
                epMinColor           = Resource.Parameters["MinColor"];
                epMaxColor           = Resource.Parameters["MaxColor"];
                epRotateSpeed        = Resource.Parameters["RotateSpeed"];
                epStartSize          = Resource.Parameters["StartSize"];
                epEndSize            = Resource.Parameters["EndSize"];
                // Textures //
                epTexture = Resource.Parameters["particleTexture"];
                // Soft Particles //
                epProjectionInvert = Resource.Parameters["ProjI"];
                epFarPlane         = Resource.Parameters["farPlane"];
                epDepthTexture     = Resource.Parameters["depthTexture"];
                epHalfPixel        = Resource.Parameters["halfPixel"];
                epFadeDistance     = Resource.Parameters["fadeDistance"];
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
                // Set Render States.
                EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                // Set initial parameters
                SetViewProjectionMatrix(viewMatrix * projectionMatrix);
                SetProjectionMatrix(projectionMatrix);
                SetProjectionInvertMatrix(Matrix.Invert(projectionMatrix));

                SetViewportScale(new Vector2(0.5f / aspectRatio, -0.5f));                
                SetFarPlane(farPlane);
                SetHalfPixel(new Vector2(0.5f / cameraSize.Width, 0.5f / cameraSize.Height)); // The half depth size produce flickering.
                SetDepthTexture(depthTexture);
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
                           Vector2 startSize, Vector2 endSize, Texture texture, bool softParticles, float fadeDistance)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = blendState;

                // Set parameters
                // Set an effect parameter describing the current time. All the vertex shader particle animation is keyed off this value.
                SetCurrentTime(particleSystem.CurrentTime);
                // Surface
                SetDuration(duration);
                SetDurationRandomness(durationRandomness);
                SetGravity(gravity);
                SetEndVelocity(endVelocity);
                SetMinimumColor(minimumColor);
                SetMaximumColor(maximumColor);
                SetRotateSpeed(rotateSpeed);
                SetStartSize(startSize);
                SetEndSize(endSize);
                // Texture
                SetTexture(texture);
                // Soft Particles
                if (softParticles)
                {
                    SetFadeDistance(fadeDistance);
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
