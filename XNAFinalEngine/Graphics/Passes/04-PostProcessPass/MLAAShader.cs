
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
                                       epDepthTexture,
                                       epBlurRadius;

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

        #region Pixel Size

        private static Vector2? lastUsedPixelSize;
        private static void SetPixelSize(Vector2 pixelSize)
        {
            if (lastUsedPixelSize != pixelSize)
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
            if (lastUsedThresholdColor != thresholdColor)
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
            if (lastUsedThresholdDepth != thresholdDepth)
            {
                lastUsedThresholdDepth = thresholdDepth;
                epThresholdDepth.SetValue(thresholdDepth);
            }
        } // SetThresholdDepth

        #endregion

        #region Blur Radius

        private static float? lastUsedBlurRadius;
        private static void SetBlurRadius(float blurRadius)
        {
            if (lastUsedBlurRadius != blurRadius)
            {
                lastUsedBlurRadius = blurRadius;
                epBlurRadius.SetValue(blurRadius);
            }
        } // SetBlurRadius

        #endregion

        #region Scene Texture

        private static Texture lastUsedSceneTexture;
        private static void SetSceneTexture(Texture sceneTexture)
        {
            // XNA 4.0 reconstructs automatically the render targets when a device is lost.
            // However the shaders have to re set to the GPU the new render targets to work properly.
            // This problem seems to manifest only with floating point formats.
            // So it's a floating point texture set it every time that is need it.
            if (lastUsedSceneTexture != sceneTexture ||
                (sceneTexture is RenderTarget && ((RenderTarget)sceneTexture).SurfaceFormat != SurfaceFormat.Color))
            {
                lastUsedSceneTexture = sceneTexture;
                epSceneTexture.SetValue(sceneTexture.Resource);
            }
        } // SetSceneTexture

        #endregion

        #region Depth Texture

        private static Texture lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            // XNA 4.0 reconstructs automatically the render targets when a device is lost.
            // However the shaders have to re set to the GPU the new render targets to work properly.
            // This problem seems to manifest only with floating point formats.
            // So it's a floating point texture set it every time that is need it.
            if (lastUsedDepthTexture != depthTexture ||
                (depthTexture is RenderTarget && ((RenderTarget)depthTexture).SurfaceFormat != SurfaceFormat.Color))
            {
                lastUsedDepthTexture = depthTexture;
                epDepthTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthTexture

        #endregion

        #region Edge Texture

        private static Texture lastUsedEdgeTexture;
        private static void SetEdgeTexture(Texture edgeTexture)
        {
            // XNA 4.0 reconstructs automatically the render targets when a device is lost.
            // However the shaders have to re set to the GPU the new render targets to work properly.
            // This problem seems to manifest only with floating point formats.
            // So it's a floating point texture set it every time that is need it.
            if (lastUsedEdgeTexture != edgeTexture ||
                (edgeTexture is RenderTarget && ((RenderTarget)edgeTexture).SurfaceFormat != SurfaceFormat.Color))
            {
                lastUsedEdgeTexture = edgeTexture;
                epEdgeTexture.SetValue(edgeTexture.Resource);
            }
        } // SetEdgeTexture

        #endregion

        #region Blended Weights Texture

        private static Texture lastUsedBlendedWeightsTexture;
        private static void SetBlendedWeightsTexture(Texture blendedWeightsTexture)
        {
            // XNA 4.0 reconstructs automatically the render targets when a device is lost.
            // However the shaders have to re set to the GPU the new render targets to work properly.
            // This problem seems to manifest only with floating point formats.
            // So it's a floating point texture set it every time that is need it.
            if (lastUsedBlendedWeightsTexture != blendedWeightsTexture ||
                (blendedWeightsTexture is RenderTarget && ((RenderTarget)blendedWeightsTexture).SurfaceFormat != SurfaceFormat.Color))
            {
                lastUsedBlendedWeightsTexture = blendedWeightsTexture;
                epBlendedWeightsTexture.SetValue(blendedWeightsTexture.Resource);
            }
        } // SetBlendedWeightsTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Morphological Antialiasing (MLAA).
        /// </summary>
        internal MLAAShader() : base("PostProcessing\\MLAA")
        {
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            // IMPORTANT: Be careful of the content processor properties of this texture
            // Pre multiply alpha: false
            // Texture format: No change.
            Texture areaTexture = new Texture("Shaders\\AreaMap32");
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
                epBlurRadius     = Resource.Parameters["blurRadius"];
                epEdgeTexture    = Resource.Parameters["edgeTexture"];
                epBlendedWeightsTexture = Resource.Parameters["blendedWeightsTexture"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Filter

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

                // Set parameters
                SetHalfPixel(new Vector2(-1f / texture.Width, 1f / texture.Height));
                SetPixelSize(new Vector2(1f / texture.Width, 1f / texture.Height));
                SetSceneTexture(texture);
                SetDepthTexture(depthTexture);
                SetThresholdColor(postProcess.MLAA.ThresholdColor);
                SetThresholdDepth(postProcess.MLAA.ThresholdDepth);
                SetBlurRadius(postProcess.MLAA.BlurRadius);

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
                        SetBlendedWeightsTexture(blendingWeightTexture);
                        destinationTexture.EnableRenderTarget();
                        destinationTexture.Clear(Color.Black);
                    }
                    
                    pass.Apply();
                    RenderScreenPlane();

                    if (pass.Name == "EdgeDetection")
                        destinationTexture.DisableRenderTarget();
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
