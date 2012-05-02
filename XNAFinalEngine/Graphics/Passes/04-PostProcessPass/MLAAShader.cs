﻿
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Morphological Antialiasing (MLAA).
    /// </summary>
    internal class MLAAShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static MLAAShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a MLAA shader.
        /// </summary>
        public static MLAAShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new MLAAShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        #region Effect Handles

        /// <summary>
        /// Effect handles.
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epPixelSize,
                                       epThresholdColor,
                                       epThresholdDepth,
                                       epSceneTexture,
                                       epEdgeTexture,
                                       epBlendedWeightsTexture,
                                       epDepthTexture;

        #endregion

        #region Half Pixel

        private static Vector2? lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region Pixel Size

        private static Vector2? lastUsedPixelSize;
        private static void SetPixelSize(Vector2 pixelSize)
        {
            if (lastUsedPixelSize != pixelSize || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedPixelSize = pixelSize;
                epPixelSize.SetValue(pixelSize);
            }
        } // SetPixelSize

        #endregion

        #region Threshold Color

        private static float? lastUsedThresholdColor;
        private static void SetThresholdColor(float thresholdColor)
        {
            if (lastUsedThresholdColor != thresholdColor || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedThresholdColor = thresholdColor;
                epThresholdColor.SetValue(thresholdColor);
            }
        } // SetThresholdColor

        #endregion

        #region Threshold Depth

        private static float? lastUsedThresholdDepth;
        private static void SetThresholdDepth(float thresholdDepth)
        {
            if (lastUsedThresholdDepth != thresholdDepth || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedThresholdDepth = thresholdDepth;
                epThresholdDepth.SetValue(thresholdDepth);
            }
        } // SetThresholdDepth

        #endregion

        #region Scene Texture

        private static Texture2D lastUsedSceneTexture;
        private static void SetSceneTexture(Texture sceneTexture)
        {
            EngineManager.Device.SamplerStates[10] = SamplerState.PointClamp;
            EngineManager.Device.SamplerStates[11] = SamplerState.LinearClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedSceneTexture != sceneTexture.Resource || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedSceneTexture = sceneTexture.Resource;
                epSceneTexture.SetValue(sceneTexture.Resource);
            }
        } // SetSceneTexture

        #endregion

        #region Depth Texture

        private static Texture2D lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[14] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDepthTexture != depthTexture.Resource || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedDepthTexture = depthTexture.Resource;
                epDepthTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthTexture

        #endregion

        #region Edge Texture

        private static Texture2D lastUsedEdgeTexture;
        private static void SetEdgeTexture(Texture edgeTexture)
        {
            EngineManager.Device.SamplerStates[12] = SamplerState.LinearClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedEdgeTexture != edgeTexture.Resource || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedEdgeTexture = edgeTexture.Resource;
                epEdgeTexture.SetValue(edgeTexture.Resource);
            }
        } // SetEdgeTexture

        #endregion

        #region Blended Weights Texture

        private static Texture2D lastUsedBlendedWeightsTexture;
        private static void SetBlendedWeightsTexture(Texture blendedWeightsTexture)
        {
            EngineManager.Device.SamplerStates[13] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedBlendedWeightsTexture != blendedWeightsTexture.Resource || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedBlendedWeightsTexture = blendedWeightsTexture.Resource;
                epBlendedWeightsTexture.SetValue(blendedWeightsTexture.Resource);
            }
        } // SetBlendedWeightsTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Morphological Antialiasing (MLAA).
        /// </summary>
        private MLAAShader() : base("PostProcessing\\MLAA")
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            // IMPORTANT: Be careful of the content processor properties of this texture
            // Pre multiply alpha: false
            // Texture format: No change.
            Texture areaTexture = new Texture("Shaders\\AreaMap32");
            ContentManager.CurrentContentManager = userContentManager;
            
            EngineManager.Device.SamplerStates[15] = SamplerState.PointWrap;
            Resource.Parameters["areaTexture"].SetValue(areaTexture.Resource);
        } // MLAAShader

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
                epHalfPixel      = Resource.Parameters["halfPixel"];
                epPixelSize      = Resource.Parameters["pixelSize"];
                epThresholdColor = Resource.Parameters["thresholdColor"];
                epThresholdDepth = Resource.Parameters["thresholdDepth"];
                epSceneTexture   = Resource.Parameters["sceneTexture"];
                epDepthTexture   = Resource.Parameters["depthTexture"];
                epEdgeTexture    = Resource.Parameters["edgeTexture"];
                epBlendedWeightsTexture = Resource.Parameters["blendedWeightsTexture"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public RenderTarget Render(RenderTarget texture, Texture depthTexture, PostProcess postProcess)
        {
            if (texture == null || texture.Resource == null)
                throw new ArgumentNullException("texture");
            if (postProcess == null)
                throw new ArgumentNullException("postProcess");
            if (postProcess.MLAA == null || !(postProcess.MLAA.Enabled))
                throw new ArgumentException("MLAA Shader: MLAA properties can not be null.");
            try
            {
                // Fetch render targets.
                RenderTarget blendingWeightTexture = RenderTarget.Fetch(texture.Size, SurfaceFormat.Color, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                RenderTarget destinationTexture = RenderTarget.Fetch(texture.Size, SurfaceFormat.Color, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                
                // Set render states
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                // If someone changes the sampler state of the area texture could be a problem… in a form of an exception.
                EngineManager.Device.SamplerStates[15] = SamplerState.PointWrap;

                // Set parameters
                SetHalfPixel(new Vector2(-1f / texture.Width, 1f / texture.Height));
                SetPixelSize(new Vector2(1f / texture.Width, 1f / texture.Height));
                SetSceneTexture(texture);
                SetDepthTexture(depthTexture);
                SetThresholdColor(postProcess.MLAA.ThresholdColor);
                SetThresholdDepth(postProcess.MLAA.ThresholdDepth);

                switch (postProcess.MLAA.EdgeDetection)
                {
                    case MLAA.EdgeDetectionType.Both:  Resource.CurrentTechnique = Resource.Techniques["EdgeDetectionColorDepth"]; break;
                    case MLAA.EdgeDetectionType.Color: Resource.CurrentTechnique = Resource.Techniques["EdgeDetectionColor"]; break;
                    case MLAA.EdgeDetectionType.Depth: Resource.CurrentTechnique = Resource.Techniques["EdgeDetectionDepth"]; break;
                }

                foreach (EffectPass pass in Resource.CurrentTechnique.Passes)
                {
                    if (pass.Name == "EdgeDetection")
                    {
                        // Store the edge texture into the result texture to reduce memory consumption.
                        destinationTexture.EnableRenderTarget();
                        destinationTexture.Clear(Color.Black);
                    }
                    else if (pass.Name == "BlendingWeight")
                    {
                        SetEdgeTexture(destinationTexture);
                        blendingWeightTexture.EnableRenderTarget();
                        blendingWeightTexture.Clear(new Color(0, 0, 0, 0));
                    }
                    else
                    {
                        // For testing
                        //RenderTarget.Release(blendingWeightTexture);
                        //return destinationTexture;
                        SetBlendedWeightsTexture(blendingWeightTexture);
                        destinationTexture.EnableRenderTarget();
                        destinationTexture.Clear(Color.Black);
                    }
                    
                    pass.Apply();
                    RenderScreenPlane();

                    if (pass.Name == "EdgeDetection")
                    {
                        destinationTexture.DisableRenderTarget();
                    }
                    else if (pass.Name == "BlendingWeight")
                        blendingWeightTexture.DisableRenderTarget();
                    else
                        destinationTexture.DisableRenderTarget();
                }
                // It's not used anymore.
                RenderTarget.Release(blendingWeightTexture);
                return destinationTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("MLAA Shader: Unable to render.", e);
            }
        } // Render

        #endregion

    } // MLAAShader
} // XNAFinalEngine.Graphics
