
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
using XNAFinalEngine.EngineCore;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Deferred Lighting Manager.
    /// Important: XNA 4.0 doesn’t let me use stencil optimizations.
    /// In other words I can’t make some sky and light optimizations with the stencil buffer.
    /// </summary>
    public static class DeferredLightingManager
    {

        #region Variables

        /// <summary>
        /// Render targets.
        /// </summary>
        private static RenderTarget halfDepthMap, halfNormaMap, quarterDepthMap, quarterNormalMap, sceneHDRTexture, sceneFinalTexture;

        /// <summary>
        /// Additive blending state.
        /// The resulting color will be added to current render target color.
        /// </summary>
        private static BlendState additiveBlendingState;
        
        #endregion

        #region Properties

        #region Render Targets
        
        /// <summary>
        /// Light Pre Pass Map.
        /// </summary>
        public static RenderTarget LightMap { get; private set; }

        /// <summary>
        /// Scene texture in HDR format (before tone mapping)
        /// </summary>
        public static RenderTarget SceneHDRTexture { get { return sceneHDRTexture; } }

        /// <summary>
        /// Scene Final Texture (post processed)
        /// </summary>
        public static RenderTarget SceneFinalTexture { get { return sceneFinalTexture; } }

        /// <summary>
        /// Half Depth Map.
        /// </summary>
        public static RenderTarget HalfDepthMap { get { return halfDepthMap; } }

         /// <summary>
        /// Quarter Depth Map.
        /// </summary>
        public static RenderTarget QuarterDepthMap { get { return quarterDepthMap; } }

        /// <summary>
        /// Half Normal Map.
        /// </summary>
        public static RenderTarget HalfNormaMap { get { return halfNormaMap; } }

        /// <summary>
        /// Quarter Normal Map.
        /// </summary>
        public static RenderTarget QuarterNormalMap { get { return quarterNormalMap; } }
        
        #endregion

        #region Filters

        /// <summary>
        /// Half screen blur.
        /// One for every effect so that improve memory consumption.
        /// </summary>
        public static Blur HalfScreenBlur { get; private set; }

        /// <summary>
        /// Quarter screen blur.
        /// One for every effect so that improve memory consumption.
        /// </summary>
        public static Blur QuarterScreenBlur { get; private set; }

        /// <summary>
        /// Half screen dilate.
        /// One for every effect so that improve memory consumption.
        /// </summary>
        public static Dilate HalfScreenDilate { get; private set; }

        #endregion

        #endregion

        #region Init

        /// <summary>
        /// Init Deferred Lighting Manager.
        /// </summary>
        public static void InitDeferredLightingManager()
        {
            // Half size g buffer
            halfDepthMap = new RenderTarget(RenderTarget.SizeType.HalfScreen, true, false);
            halfNormaMap = new RenderTarget(RenderTarget.SizeType.HalfScreen, SurfaceFormat.Rg32, false);
            // Quarter size g buffer. You could use it or not. They take little memory but is better not to have them.
            quarterDepthMap = new RenderTarget(RenderTarget.SizeType.QuarterScreen, true, false);
            quarterNormalMap = new RenderTarget(RenderTarget.SizeType.QuarterScreen, SurfaceFormat.Rg32, false);

            // In PC the HDR Blendable format is implemented using HalfVector4. However in XBOX 360 is implemented using
            // an Xbox specific 7e3 (32 bits per pixel) EDRAM format (or 10101002). That means less precision, more speed,
            // less memory space used and the lack of the fourth channel (2 bits doesn’t help much).

            // The lack of precision is not a big deal and we have better performance.
            // Unfortunately this deferred pipeline implementation needs the fourth channel for monochromatic specular highlights.
            // A possible solution for the XBOX 360 if to use a second HDR Blendable render target for specular highlight.
            // With this we have the same memory requisites but at least we could have color specular highlights,
            // in practice this is not an incredible gain, though (see Crytek’s presentation).

            // In this case we don’t need MSAA or a depth buffer for the render targets and I think the XBOX 360's EDRAM will be fine.
            // If the EDRAM can hold these buffers then, maybe, is a good time to see a workarounds like this:
            // http://msdn.microsoft.com/en-us/library/bb464139.aspx (Predicated Tiling)

            // One more thing, I can't use a color surface format because the RGBM format doesn't work with additive blending.
            LightMap = new RenderTarget(RenderTarget.SizeType.FullScreen, SurfaceFormat.HdrBlendable, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

            additiveBlendingState = new BlendState
            {
                AlphaBlendFunction    = BlendFunction.Add,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend      = Blend.One,
                ColorBlendFunction    = BlendFunction.Add,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend      = Blend.One,
            };

            // It's in linear space. In this same render target the transparent object will be rendered. Maybe an RGBM encoding could work, but how?
            // Multisampling could generate indeseable artifacts. Be careful!
            sceneHDRTexture = new RenderTarget(RenderTarget.SizeType.FullScreen, SurfaceFormat.HdrBlendable, DepthFormat.Depth24, RenderTarget.AntialiasingType.NoAntialiasing);

            sceneFinalTexture = new RenderTarget(RenderTarget.SizeType.FullScreen, SurfaceFormat.Color, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
            
            HalfScreenBlur = new Blur(RenderTarget.SizeType.HalfScreen);
            QuarterScreenBlur = new Blur(RenderTarget.SizeType.QuarterScreen);
            HalfScreenDilate = new Dilate(RenderTarget.SizeType.HalfScreen);

        } // Init

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public static void Update()
        {
            ToneMapping.Update();
        } // Update

        #endregion

        #region Fill G Buffer

        /// <summary>
        /// Fill the G buffer.
        /// For the moment only depth and normals.
        /// Also in this method the downsampled version of the g buffer is created.
        /// </summary>
        private static void FillGBuffer()
        {
            // Depth and Normal map generation
            GBuffer.GenerateGBuffer(SceneManager.OpaqueObjectsToRenderThisFrame);

            // Downsample depth map.
            DepthDownsampler.Downsample(GBuffer.DepthTexture, halfDepthMap);

            // Downsample depth map.
            DepthDownsampler.Downsample(halfDepthMap, quarterDepthMap);

            // Donwsample normal map. Applying typical color filters in normal maps is not the best way course of action, but is simple, fast and the resulting error is subtle.
            // In this case the buffer is downsampled using the sprite manager. Is important that the filter type is linear, not point.
            // If some error occurs them probably the surfaceformat does not support linear filter.
            try
            {
                // Downsampled half size normal map
                halfNormaMap.EnableRenderTarget();
                GBuffer.NormalTexture.RenderOnScreen(new Rectangle(0, 0, halfNormaMap.Width, halfNormaMap.Height));
                SpriteManager.SamplerState = SamplerState.LinearClamp;
                SpriteManager.AlphaBlendEnable = false;
                SpriteManager.DrawSprites();
                SpriteManager.SamplerState = SamplerState.PointWrap;
                SpriteManager.AlphaBlendEnable = true;
                halfNormaMap.DisableRenderTarget();
                // Downsampled quarter size normal map
                quarterNormalMap.EnableRenderTarget();
                halfNormaMap.RenderOnScreen(new Rectangle(0, 0, halfNormaMap.Width, halfNormaMap.Height));
                SpriteManager.SamplerState = SamplerState.LinearClamp;
                SpriteManager.AlphaBlendEnable = false;
                SpriteManager.DrawSprites();
                SpriteManager.SamplerState = SamplerState.PointWrap;
                SpriteManager.AlphaBlendEnable = true;
                quarterNormalMap.DisableRenderTarget();
            }
            catch (Exception e)
            {
                // Maybe an error could occur if the surface format that XNA gives you doesn't support linear filtering. It needs further testing.
                throw new Exception("Unable to downsample the normal map. " + e.Message);
            }
        } // FillGBuffer

        #endregion

        #region Fill Light Map

        /// <summary>
        /// Fill Light Map. This is the light pre pass.
        /// </summary>
        private static void FillLightMap()
        {

            #region Ambient Occlusion

            // Generate screen space ambient occlusion texture
            // I made this here to avoid a little performance hit in the XBOX 360 when enable and disable render targets. 
            // Yes, the engine doesn’t support the XBOX 360 yet, but is better to think in the future, right?
            if (AmbientLight.AmbientOcclusion != null)
            {
                // Generates the ambient occlusion texture
                AmbientLight.AmbientOcclusion.GenerateAmbientOcclusion(halfDepthMap, halfNormaMap);
                // Dilate the result
                //HalfScreenDilate.GenerateDilate(AmbientLight.AmbientOcclusion.AmbientOcclusionTexture);
                // Then blur it.
                HalfScreenBlur.Width = 3;
                HalfScreenBlur.GenerateBlur(AmbientLight.AmbientOcclusion.AmbientOcclusionTexture);
                HalfScreenBlur.Width = 1;
                HalfScreenBlur.GenerateBlur(AmbientLight.AmbientOcclusion.AmbientOcclusionTexture);
            }

            #endregion

            #region Shadows

            // Directional Lights shadows
            foreach (var directionalLight in SceneManager.DirectionalLightForOpaqueObjects)
            {
                if (directionalLight.ShadowMap != null)
                {
                    directionalLight.ShadowMap.Render(SceneManager.OpaqueObjectsToRenderThisFrame, halfDepthMap);
                    HalfScreenBlur.GenerateBlur(directionalLight.ShadowMap.ShadowTexture);
                    HalfScreenBlur.GenerateBlur(directionalLight.ShadowMap.ShadowTexture);
                }
            }

            #endregion

            #region Opaque Objects

            LightMap.EnableRenderTarget();
            // We need all the four channels to black because the alpha channel will be used for the specular color.
            //lightPrePassMap.Clear(Color.Transparent);
            LightMap.Clear(new Color(AmbientLight.Color.R, AmbientLight.Color.G, AmbientLight.Color.B, 0));
            // Render States
            EngineManager.Device.BlendState = additiveBlendingState; // The resulting color will be added to current render target color.
            EngineManager.Device.DepthStencilState = DepthStencilState.None;
            
            // Ambient Light
            // A half size normal map is enough for a low frequency color pass.
            AmbientLight.Render(GBuffer.NormalTexture, LightMap);

            // Directional Lights
            foreach (var directionalLight in SceneManager.DirectionalLightForOpaqueObjects)
            {
                LightPrePassDirectionalLight.Render(directionalLight, GBuffer.DepthTexture, GBuffer.NormalTexture, GBuffer.MotionVectorsSpecularPowerTexture, LightMap);
            }
            // Point Lights
            foreach (var pointLight in SceneManager.PointLightListForOpaqueObjects)
            {
                LightPrePassPointLight.Render(pointLight, GBuffer.DepthTexture, GBuffer.NormalTexture, GBuffer.MotionVectorsSpecularPowerTexture, LightMap);
            }
            // Spot Lights
            foreach (var spotLight in SceneManager.SpotLightListForOpaqueObjects)
            {
                //LightPrePassSpotLight.Render(spotLight, gBuffer.DepthMapTexture, gBuffer.NormalMapTexture, lightPrePassMap);
            }
            
            EngineManager.SetDefaultRenderStates();

            LightMap.DisableRenderTarget();

            #endregion

        } // FillLightMap

        #endregion

        #region Fill Scene Map

        /// <summary>
        /// Fill Scene Map (material pass)
        /// </summary>
        private static void FillSceneMap()
        {
            sceneHDRTexture.EnableRenderTarget();

            sceneHDRTexture.Clear(SceneManager.ClearColor);

            // Render all the opaque objects
            foreach (var obj in SceneManager.OpaqueObjectsToRenderThisFrame)
            {
                obj.Render();
            }
          
            // The sky is render latter so that the GPU can avoid fragment processing. But it has to be before the transparent objects.
            if (SceneManager.Sky != null)
                SceneManager.Sky.Render();

            // The particle systems
            foreach (var obj in SceneManager.ParticlesToRender)
            {
                obj.Render();
            }

            // The transparent objects will be render in forward fashion.
            foreach (var obj in SceneManager.TransparentToRenderThisFrame)
            {
                obj.Render();
            }

            sceneHDRTexture.DisableRenderTarget();
        } // FillSceneMap

        #endregion

        #region Post Process

        /// <summary>
        /// Render to frame buffer and post process.
        /// </summary>
        private static void PostProcess()
        {
            SceneFinalTexture.EnableRenderTarget();
                SceneFinalTexture.Clear(Color.Black);
                PostProcessing.Render(sceneHDRTexture);
            SceneFinalTexture.DisableRenderTarget();
            if (MLAA.Enabled)
            {
                MLAA.Render(SceneFinalTexture, GBuffer.DepthTexture);
            }
        } // PostProcess

        #endregion

        #region Render

        /// <summary>
        /// Render the added objects using the deferred lighting system.
        /// </summary>
        public static void Render()
        {
            //if (SceneManager.ObjectsToRenderThisFrame.Count != 0)
            {
                FillGBuffer();
                FillLightMap();
                FillSceneMap();
                PostProcess();
                EngineManager.ClearTargetAndDepthBuffer(Color.Black);
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                SpriteManager.AlphaBlendEnable = false;
                SpriteManager.SamplerState = SamplerState.PointWrap;
                if (MLAA.Enabled)
                    MLAA.AntiAliasedTexture.RenderOnFullScreen();
                else
                    SceneFinalTexture.RenderOnFullScreen();
                EngineManager.SetDefaultRenderStates();
            }
        } // Render

        #endregion
        
    } // DeferredLightingManager
} // XNAFinalEngine.Graphics
