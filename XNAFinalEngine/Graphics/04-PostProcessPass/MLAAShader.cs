
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

        private static Texture areaTexture;

        // Shader Parameters.
        private static ShaderParameterFloat   spThresholdColor,
                                              spThresholdDepth;
        private static ShaderParameterVector2 spHalfPixel,
                                              spPixelSize;
        private static ShaderParameterTexture spSceneTexture,
                                              spDepthTexture,
                                              spEdgeTexture,
                                              spBlendedWeightsTexture,
                                              spAreaTexture;

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

        #region Constructor

        /// <summary>
        /// Morphological Antialiasing (MLAA).
        /// </summary>
        private MLAAShader() : base("PostProcessing\\MLAA")
        {
            AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
            AssetContentManager.CurrentContentManager = AssetContentManager.SystemContentManager;
            // IMPORTANT: Be careful of the content processor properties of this texture
            // Pre multiply alpha: false
            // Texture format: No change.
            areaTexture = new Texture("Shaders\\AreaMap32");
            AssetContentManager.CurrentContentManager = userContentManager;
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
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spPixelSize = new ShaderParameterVector2("pixelSize", this);
                spThresholdColor = new ShaderParameterFloat("thresholdColor", this);
                spThresholdDepth = new ShaderParameterFloat("thresholdDepth", this);
                spSceneTexture = new ShaderParameterTexture("sceneTexture", this, SamplerState.PointClamp, 10);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 14);
                spEdgeTexture = new ShaderParameterTexture("edgeTexture", this, SamplerState.LinearClamp, 12);
                spBlendedWeightsTexture = new ShaderParameterTexture("blendedWeightsTexture", this, SamplerState.PointClamp, 13);
                spAreaTexture = new ShaderParameterTexture("areaTexture", this, SamplerState.PointClamp, 15);
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
        public RenderTarget Render(RenderTarget texture, Texture depthTexture, PostProcess postProcess, RenderTarget destinationTexture)
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
                
                // Set render states
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;

                // Set parameters
                spHalfPixel.Value = new Vector2(-0.5f / (texture.Width / 2), 0.5f / (texture.Height / 2));
                spPixelSize.Value = new Vector2(1f / texture.Width, 1f / texture.Height);
                spSceneTexture.Value = texture; EngineManager.Device.SamplerStates[11] = SamplerState.LinearClamp; // The scene texture has two samplers.
                spDepthTexture.Value = depthTexture;
                spThresholdColor.Value = postProcess.MLAA.ThresholdColor;
                spThresholdDepth.Value = postProcess.MLAA.ThresholdDepth;
                spAreaTexture.Value = areaTexture;

                switch (postProcess.MLAA.EdgeDetection)
                {
                    case MLAA.EdgeDetectionType.Both:  Resource.CurrentTechnique = Resource.Techniques["EdgeDetectionColorDepth"]; break;
                    case MLAA.EdgeDetectionType.Color: Resource.CurrentTechnique = Resource.Techniques["EdgeDetectionColor"]; break;
                    case MLAA.EdgeDetectionType.Depth: Resource.CurrentTechnique = Resource.Techniques["EdgeDetectionDepth"]; break;
                }

                // To avoid an exception that is produced with adaptative exposure is enable in the first camera rendered.
                // It seems the lastLuminanceTexture is still link to this sampler and when the technique is apply did not like this situation.
                // I am not sure why.
                // A possible solution is to reservate a small number of samplers for linear and anisotropic access and the rest for point.
                EngineManager.Device.SamplerStates[12] = SamplerState.PointClamp;

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
                        spEdgeTexture.Value = destinationTexture;
                        blendingWeightTexture.EnableRenderTarget();
                        blendingWeightTexture.Clear(new Color(0, 0, 0, 0));
                    }
                    else
                    {
                        // For testing
                        /*RenderTarget.Release(blendingWeightTexture);
                        return destinationTexture;*/
                        spBlendedWeightsTexture.Value = blendingWeightTexture;
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
