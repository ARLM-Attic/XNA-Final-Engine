
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Texture = Microsoft.Xna.Framework.Graphics.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Morphological Antialiasing (MLAA).
    /// </summary>
    public static class MLAA
    {

        #region Enumerators

        public enum EdgeDetectionType
        {
            Both,
            Color,
            Depth
        } // EdgeDetectionType

        #endregion

        #region Variables

        /// <summary>
        /// Threshold Color.
        /// </summary>
        private static float thresholdColor = 0.1f;

        /// <summary>
        /// Threshold Depth.
        /// </summary>
        private static float thresholdDepth = 0.1f;

        /// <summary>
        /// Enabled?
        /// </summary>
        private static bool enabled = true;

        /// <summary>
        /// Blur radius.
        /// </summary>
        private static float blurRadius = 2;

        #endregion

        #region Properties

        public static bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        } // Enabled

        /// <summary>
        /// Blending Weight Texture.
        /// </summary>
        public static RenderTarget BlendingWeightTexture { get; set; }

        /// <summary>
        /// Anti Aliased Texture.
        /// </summary>
        public static RenderTarget AntiAliasedTexture { get; set; }

        /// <summary>
        /// Edge detection: both, color or depth.
        /// </summary>
        public static EdgeDetectionType EdgeDetection { get; set; }

        /// <summary>
        /// Threshold Color.
        /// </summary>
        public static float ThresholdColor
        {
            get { return thresholdColor; }
            set { thresholdColor = value; }
        } // ThresholdColor

        /// <summary>
        /// Threshold Depth.
        /// </summary>
        public static float ThresholdDepth
        {
            get { return thresholdDepth; }
            set { thresholdDepth = value; }
        } // ThresholdDepth

        /// <summary>
        /// Blur radius.
        /// </summary>
        public static float BlurRadius
        {
            get { return blurRadius; }
            set { blurRadius = value; }
        } // BlurRadius

        #endregion

        #region Shader Parameters

        /// <summary>
        /// The shader effect.
        /// </summary>
        private static Effect Effect { get; set; }

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
            if (EngineManager.DeviceLostInThisFrame || lastUsedSceneTexture != sceneTexture)
            {
                lastUsedSceneTexture = sceneTexture;
                epSceneTexture.SetValue(sceneTexture.XnaTexture);
            }
        } // SetSceneTexture

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

        #region Edge Texture

        private static Texture lastUsedEdgeTexture;
        private static void SetEdgeTexture(Texture edgeTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedEdgeTexture != edgeTexture)
            {
                lastUsedEdgeTexture = edgeTexture;
                epEdgeTexture.SetValue(edgeTexture.XnaTexture);
            }
        } // SetEdgeTexture

        #endregion

        #region Blended Weights Texture

        private static Texture lastUsedBlendedWeightsTexture;
        private static void SetBlendedWeightsTexture(Texture blendedWeightsTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedBlendedWeightsTexture != blendedWeightsTexture)
            {
                lastUsedBlendedWeightsTexture = blendedWeightsTexture;
                epBlendedWeightsTexture.SetValue(blendedWeightsTexture.XnaTexture);
            }
        } // SetBlendedWeightsTexture

        #endregion

        #endregion

        #region Load Shader

        /// <summary>
        /// Load shader
        /// </summary>
        private static void LoadShader()
        {
            const string filename = "PostProcessing\\MLAA";
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
            Texture areaTexture = new Texture("Shaders\\AreaMap32");
            Effect.Parameters["areaTexture"].SetValue(areaTexture.XnaTexture);
            
            BlendingWeightTexture = new RenderTarget(RenderTarget.SizeType.FullScreen, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
            AntiAliasedTexture = new RenderTarget(RenderTarget.SizeType.FullScreen, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
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
                epHalfPixel      = Effect.Parameters["halfPixel"];
                epPixelSize      = Effect.Parameters["pixelSize"];
                epThresholdColor = Effect.Parameters["thresholdColor"];
                epThresholdDepth = Effect.Parameters["thresholdDepth"];
                epSceneTexture   = Effect.Parameters["sceneTexture"];
                epDepthTexture   = Effect.Parameters["depthTexture"];
                epBlurRadius     = Effect.Parameters["blurRadius"];
                epEdgeTexture = Effect.Parameters["edgeTexture"];
                epBlendedWeightsTexture = Effect.Parameters["blendedWeightsTexture"];
            }
            catch
            {
                throw new Exception("Get the handles from the MLAA shader failed.");
            }
        } // GetParametersHandles

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public static void Render(RenderTarget sceneTexture, RenderTarget depthTexture)
        {
            if (Effect == null)
            {
                LoadShader();
            }
            try
            {
                // Set render states
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;

                // Set parameters
                SetHalfPixel(new Vector2(-1f / sceneTexture.Width, 1f / sceneTexture.Height));
                SetPixelSize(new Vector2(1f / sceneTexture.Width, 1f / sceneTexture.Height));
                SetSceneTexture(sceneTexture);
                SetDepthTexture(depthTexture);
                SetThresholdColor(ThresholdColor);
                SetThresholdDepth(ThresholdDepth);
                SetBlurRadius(BlurRadius);

                switch (EdgeDetection)
                {
                    case EdgeDetectionType.Both: Effect.CurrentTechnique = Effect.Techniques["EdgeDetectionColorDepth"]; break;
                    case EdgeDetectionType.Color: Effect.CurrentTechnique = Effect.Techniques["EdgeDetectionColor"]; break;
                    case EdgeDetectionType.Depth: Effect.CurrentTechnique = Effect.Techniques["EdgeDetectionDepth"]; break;
                }
                
                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    if (pass.Name == "EdgeDetection")
                    {
                        // Store the edge texture into the result texture to reduce memory consumption.
                        AntiAliasedTexture.EnableRenderTarget();
                        AntiAliasedTexture.Clear(Color.Black);
                    }
                    else if (pass.Name == "BlendingWeight")
                    {
                        BlendingWeightTexture.EnableRenderTarget();
                        BlendingWeightTexture.Clear(new Color(0, 0, 0, 0));
                    }
                    else
                    {
                        AntiAliasedTexture.EnableRenderTarget();
                        AntiAliasedTexture.Clear(Color.Black);
                    }
                    
                    pass.Apply();
                    ScreenPlane.Render();

                    if (pass.Name == "EdgeDetection")
                    {
                        AntiAliasedTexture.DisableRenderTarget();
                    }
                    else if (pass.Name == "BlendingWeight")
                    {
                        SetEdgeTexture(AntiAliasedTexture);
                        BlendingWeightTexture.DisableRenderTarget();
                    }
                    else
                    {
                        SetBlendedWeightsTexture(BlendingWeightTexture);
                        AntiAliasedTexture.DisableRenderTarget();
                    }
                }

                EngineManager.SetDefaultRenderStates();
            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the MLAA shader. " + e.Message);
            }
        } // Render

        #endregion

    } // MLAA
} // XNAFinalEngine.Graphics
