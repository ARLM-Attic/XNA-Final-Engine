
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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Components;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using RootAnimation = XNAFinalEngine.Components.RootAnimations;
using XNAFinalEngine.Scenes;
using Camera = XNAFinalEngine.Components.Camera;
using DirectionalLight = XNAFinalEngine.Components.DirectionalLight;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The manager of all managers.
    /// </summary>
    public static class GameLoop
    {

        #region Variables
        
        /// <summary>
        /// This game object will show the frames per second onto screen.
        /// </summary>
        private static GameObject2D fpsText;

        // It's an auxiliary value that helps avoiding garbage.
        private static Vector3[] cornersViewSpace = new Vector3[4];
        
        #endregion

        #region Properties

        /// <summary>
        /// Current Scene.
        /// </summary>
        public static Scene CurrentScene { get; set; }

        /// <summary>
        /// Show frames per second onto screen.
        /// </summary>
        public static bool ShowFramesPerSecond { get; set; }

        #endregion

        #region Load Content

        /// <summary>
        /// Load Content.
        /// </summary>
        internal static void LoadContent()
        {
            // Create the 32 layers.
            Layer.InitLayers();
            // Graphics
            SpriteManager.Init();
            // Input
            InputManager.Initialize();
            InputManager.EnableKeyboardHook();

            #region FPS

            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            fpsText = new GameObject2D();
            fpsText.AddComponent<HudText>();
            fpsText.HudText.Font = new Font("Arial12");
            fpsText.HudText.Color = Color.Yellow;
            fpsText.HudText.Text.Insert(0, "FPS ");
            fpsText.Transform.LocalRotation = 0f;

            #endregion
            
            if (CurrentScene != null)
            {
                CurrentScene.Load();
            }

            // Pre draw to avoid the first frame's garbage and to place everything into memory.
            Draw(new GameTime());

            #region Garbage Collection

            // Collect all garbage.
            GarbageCollector.CollectGarbage();
            // Test the garbage collector.
            //GarbageCollector.CreateWeakReference();

            #endregion

        } // LoadContent
        
        #endregion
        
        #region Update

        /// <summary>
        /// Update
        /// </summary>
        internal static void Update(GameTime gameTime)
        {
            Time.GameDeltaTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            #region Input
            
            InputManager.Update();

            #endregion

            #region Scene Update Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.UpdateTasks();
            }

            #endregion

            #region Scripts Update

            foreach (var script in Script.ScriptList)
            {
                script.Update();
            }

            #endregion 
            
            #region Scripts Late Update

            foreach (var script in Script.ScriptList)
            {
                script.LateUpdate();
            }

            #endregion

        } // Update

        #endregion

        #region Draw
        
        /// <summary>
        /// Draw
        /// </summary>        
        internal static void Draw(GameTime gameTime)
        {
            // Update frame time
            Time.FrameTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);
            // Update frames per second visibility.
            fpsText.HudText.Visible = ShowFramesPerSecond;
            fpsText.Transform.LocalPosition = new Vector3(Screen.Width - 100, 20, 0);
            
            #region Scene Pre Render Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.PreRenderTasks();
            }

            #endregion
            
            #region Scripts Pre Render Update

            foreach (var script in Script.ScriptList)
            {
                script.PreRenderUpdate();
            }

            #endregion 
            
            #region Root Animation Processing

            for (int i = 0; i < RootAnimation.ComponentPool.Count; i++)
            {
                RootAnimation.ComponentPool.Elements[i].Update();
            }

            #endregion

            #region Model Animation Processing

            // Update every active animation.
            // The output is a skeletal/rigid pose in local space for each active clip.
            // The pose might contain information for every joint in the skeleton (a full-body pose),
            // for only a subset of joints (partial pose), or it might be a difference pose for use in additive blending.
            for (int i = 0; i < ModelAnimations.ComponentPool.Count; i++)
            {
                ModelAnimations.ComponentPool.Elements[i].Update();
            }

            // Sometimes the final pose is a composition of a number of animation clips. In this stage the animations are blended.
            // The blending includes: lerp, additive blending and cross fading blending. 
            // TODO!! foeach modelAnimationComponent blend active animations according to a blend tree or something similar.

            // The global pose (world space) is generated.
            // However, if no post processing exist (IK, ragdolls, etc.) this stage could be merge with
            // the inverse bind pose multiplication stage in the mesh draw code. And for now the engine will do this.

            #endregion
            
            #region Graphics

            Camera currentCamera = null;
            // For each camera we render the scene in it
            for (int cameraIndex = 0; cameraIndex < Camera.ComponentPool.Count; cameraIndex++)
            {
                currentCamera = Camera.ComponentPool.Elements[cameraIndex];
                // If is a master camera
                if (currentCamera.MasterCamera == null)
                {
                    if (currentCamera.RenderTarget != null)
                        RenderTarget.Release(currentCamera.RenderTarget);
                    // If it does not have slaves cameras...
                    if (currentCamera.slavesCameras.Count == 0)
                        currentCamera.RenderTarget = RenderCamera(currentCamera);
                    else
                    {
                        // Render each camera to a render target and then merge.
                        currentCamera.PartialRenderTarget = RenderCamera(currentCamera);
                        for (int i = 0; i < currentCamera.slavesCameras.Count; i++)
                        {
                            currentCamera.slavesCameras[i].PartialRenderTarget = RenderCamera(currentCamera.slavesCameras[i]);
                        }
                        // Composite cameras
                        currentCamera.RenderTarget = RenderTarget.Fetch(currentCamera.RenderTargetSize, SurfaceFormat.Color, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                        currentCamera.RenderTarget.EnableRenderTarget();
                        currentCamera.RenderTarget.Clear(Color.Black);
                        EngineManager.Device.Viewport = new Viewport(currentCamera.Viewport.X, currentCamera.Viewport.Y, currentCamera.Viewport.Width, currentCamera.Viewport.Height);
                        SpriteManager.DrawTextureToFullScreen(currentCamera.PartialRenderTarget);
                        RenderTarget.Release(currentCamera.PartialRenderTarget);
                        for (int i = 0; i < currentCamera.slavesCameras.Count; i++)
                        {
                            EngineManager.Device.Viewport = new Viewport(currentCamera.slavesCameras[i].Viewport.X, currentCamera.slavesCameras[i].Viewport.Y, currentCamera.slavesCameras[i].Viewport.Width, currentCamera.slavesCameras[i].Viewport.Height);
                            SpriteManager.DrawTextureToFullScreen(currentCamera.slavesCameras[i].PartialRenderTarget);
                            RenderTarget.Release(currentCamera.slavesCameras[i].PartialRenderTarget);
                        }
                        currentCamera.RenderTarget.DisableRenderTarget();
                    }
                }
            }

            #endregion

            #region Screenshot Preparations

            RenderTarget screenshotRenderTarget = null;
            if (ScreenshotCapturer.MakeScreenshot)
            {
                // Instead of render into the back buffer we render into a render target.
                screenshotRenderTarget = new RenderTarget(Size.FullScreen, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
                screenshotRenderTarget.EnableRenderTarget();
            }

            #endregion

            EngineManager.Device.Clear(Color.Black);
            // Render onto back buffer the main camera and the HUD.
            SpriteManager.DrawTextureToFullScreen(currentCamera.RenderTarget);

            #region Heads Up Display

            // Draw 2D Heads Up Display
            SpriteManager.Begin();
            {
                HudText currentHudText;
                for (int i = 0; i < HudText.HudTextPool2D.Count; i++)
                {
                    currentHudText = HudText.HudTextPool2D.Elements[i];
                    if (currentHudText.Visible)
                    {
                        currentHudText.Text.Length = 4;
                        currentHudText.Text.AppendWithoutGarbage(Time.FramesPerSecond);
                        SpriteManager.DrawText(currentHudText.Font,
                                               currentHudText.Text,
                                               currentHudText.CachedPosition,
                                               currentHudText.Color,
                                               currentHudText.CachedRotation,
                                               Vector2.Zero,
                                               currentHudText.CachedScale);
                    }
                }
            }
            SpriteManager.End();

            #endregion

            

            #region Screenshot);

            if (ScreenshotCapturer.MakeScreenshot)
            {
                screenshotRenderTarget.DisableRenderTarget();
                ScreenshotCapturer.MakeScreenshot = false;
                ScreenshotCapturer.SaveScreenshot(screenshotRenderTarget);
                SpriteManager.DrawTextureToFullScreen(screenshotRenderTarget);
                screenshotRenderTarget.Dispose();
            }

            #endregion

            #region Scene Post Render Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.PostRenderTasks();
            }

            #endregion

            #region Scripts Post Render Update

            foreach (var script in Script.ScriptList)
            {
                script.PostRenderUpdate();
            }

            #endregion 

        } // Draw

        private static RenderTarget RenderCamera(Camera currentCamera)
        {
            // This is here to allow the rendering of these render targets for testing.
            RenderTarget.RenderTargetBinding gbufferTextures = new RenderTarget.RenderTargetBinding();
            RenderTarget lightTexture = null;
            RenderTarget sceneTexture = null;
            RenderTarget postProcessedSceneTexture = null;
            RenderTarget ambientOcclusionTexture = null;
            RenderTarget halfNormalTexture = null;
            RenderTarget halfDepthTexture = null;
            RenderTarget quarterNormalTexture = null;
            RenderTarget quarterDepthTexture = null;

            // Calculate view space bounding frustum.
            currentCamera.BoundingFrustum(cornersViewSpace);
            
            #region Calculate Size

            Size destinationSize;
            if (currentCamera.NeedViewport)
            {
                destinationSize = new Size(currentCamera.Viewport.Width, currentCamera.Viewport.Height);
                destinationSize.MakeRelativeIfPosible();
            }
            else
                destinationSize = currentCamera.RenderTargetSize;

            #endregion

            #region GBuffer Pass

            GBufferPass.Begin(destinationSize);
            GBufferShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane);
            for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
            {
                ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                {
                    GBufferShader.Instance.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, currentModelRenderer.Material);
                }
            }
            gbufferTextures = GBufferPass.End();

            #endregion
            
            #region DownSample GBuffer

            halfDepthTexture = DepthDownsamplerShader.Instance.Render(gbufferTextures.RenderTargets[0]);
            quarterDepthTexture = DepthDownsamplerShader.Instance.Render(halfDepthTexture);

            // Donwsample normal map. Applying typical color filters in normal maps is not the best way course of action, but is simple, fast and the resulting error is subtle.
            // In this case the buffer is downsampled using the sprite manager. Is important that the filter type is linear, not point.
            // If some error occurs them probably the surfaceformat does not support linear filter.)
            try
            {
                halfNormalTexture = RenderTarget.Fetch(destinationSize.HalfSize(),
                                                       gbufferTextures.RenderTargets[1].SurfaceFormat,
                                                       gbufferTextures.RenderTargets[1].DepthFormat,
                                                       gbufferTextures.RenderTargets[1].Antialiasing);
                // Downsampled half size normal map
                halfNormalTexture.EnableRenderTarget();
                SpriteManager.DrawTextureToFullScreen(gbufferTextures.RenderTargets[1]);
                halfNormalTexture.DisableRenderTarget();
                // Downsampled quarter size normal map
                quarterNormalTexture = RenderTarget.Fetch(destinationSize.HalfSize().HalfSize(),
                                                          gbufferTextures.RenderTargets[1].SurfaceFormat,
                                                          gbufferTextures.RenderTargets[1].DepthFormat,
                                                          gbufferTextures.RenderTargets[1].Antialiasing);
                quarterNormalTexture.EnableRenderTarget();
                SpriteManager.DrawTextureToFullScreen(halfNormalTexture);
                quarterNormalTexture.DisableRenderTarget();
            }
            catch (Exception e)
            {
                // Maybe an error could occur if the surface format that XNA gives you doesn't support linear filtering. It needs further testing.
                throw new InvalidOperationException("Unable to downsample the normal map. ", e);
            }

            #endregion

            #region Light Pre Pass
            
            if (currentCamera.AmbientLight.AmbientOcclusion != null && currentCamera.AmbientLight.AmbientOcclusion.Enabled)
            {
                RenderTarget aoDepthTexture, aoNormalTexture;
                if (currentCamera.AmbientLight.AmbientOcclusion.Resolution == AmbientOcclusion.AmbientOcclusionResolution.FullSize)
                {
                    aoDepthTexture = gbufferTextures.RenderTargets[0];
                    aoNormalTexture = gbufferTextures.RenderTargets[1];
                }
                else if (currentCamera.AmbientLight.AmbientOcclusion.Resolution == AmbientOcclusion.AmbientOcclusionResolution.HalfSize)
                {
                    aoDepthTexture = halfDepthTexture;
                    aoNormalTexture = halfNormalTexture;
                }
                else
                {
                    aoDepthTexture = quarterDepthTexture;
                    aoNormalTexture = quarterNormalTexture;
                }
                
                if (currentCamera.AmbientLight.AmbientOcclusion is HorizonBasedAmbientOcclusion)
                {
                    ambientOcclusionTexture = HorizonBasedAmbientOcclusionShader.Instance.Render(aoDepthTexture,
                                                                                                 aoNormalTexture,
                                                                                                 (HorizonBasedAmbientOcclusion)currentCamera.AmbientLight.AmbientOcclusion,
                                                                                                 currentCamera.FieldOfView);
                }
                if (currentCamera.AmbientLight.AmbientOcclusion is RayMarchingAmbientOcclusion)
                {
                    ambientOcclusionTexture = RayMarchingAmbientOcclusionShader.Instance.Render(aoDepthTexture,
                                                                                                aoNormalTexture,
                                                                                                (RayMarchingAmbientOcclusion)currentCamera.AmbientLight.AmbientOcclusion,
                                                                                                currentCamera.FieldOfView);
                }
            }
            
            LightPrePass.Begin(destinationSize, currentCamera.AmbientLight.Color);
            
            // Render ambient light for every camera.
            if (currentCamera.AmbientLight != null)
            {
                AmbientLightShader.Instance.RenderLight(gbufferTextures.RenderTargets[1], // Normal Texture
                                                        currentCamera.AmbientLight,
                                                        ambientOcclusionTexture,
                                                        currentCamera.ViewMatrix);
            }
            RenderTarget.Release(ambientOcclusionTexture);
            
            // Render directional lights for every camera.
            DirectionalLightShader.Instance.Begin(gbufferTextures.RenderTargets[0], // Depth Texture
                                            gbufferTextures.RenderTargets[1], // Normal Texture
                                            gbufferTextures.RenderTargets[2], // Motion Vector Specular Power
                                            currentCamera.ViewMatrix,
                                            cornersViewSpace);
            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                DirectionalLight currentDirectionalLight = DirectionalLight.ComponentPool.Elements[i];
                DirectionalLightShader.Instance.RenderLight(currentDirectionalLight.DiffuseColor, currentDirectionalLight.cachedDirection, currentDirectionalLight.Intensity);
            }
            
            // Render point lights for every camera.
            PointLightShader.Instance.Begin(gbufferTextures.RenderTargets[0], // Depth Texture
                                            gbufferTextures.RenderTargets[1], // Normal Texture
                                            gbufferTextures.RenderTargets[2], // Motion Vector Specular Power
                                            currentCamera.ViewMatrix,
                                            currentCamera.ProjectionMatrix,
                                            currentCamera.NearPlane,
                                            currentCamera.FarPlane);
            for (int i = 0; i < PointLight.ComponentPool.Count; i++)
            {
                PointLight currentPointLight = PointLight.ComponentPool.Elements[i];
                PointLightShader.Instance.RenderLight(currentPointLight.DiffuseColor, currentPointLight.cachedPosition, currentPointLight.Intensity, currentPointLight.Range);
            }
            
            // Render spot lights for every camera.

            lightTexture = LightPrePass.End();

            #endregion

            #region HDR Linear Space Pass

            ScenePass.Begin(destinationSize, currentCamera.ClearColor);

            // Render all the opaque objects);
            for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
            {
                ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Material.AlphaBlending == 1 && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                {
                    if (currentModelRenderer.Material is Constant)
                    {
                        ConstantShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
                        ConstantShader.Instance.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, (Constant)currentModelRenderer.Material);
                    }
                    else if (currentModelRenderer.Material is BlinnPhong)
                    {
                        BlinnPhongShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        BlinnPhongShader.Instance.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, (BlinnPhong)currentModelRenderer.Material);
                    }
                    else if (currentModelRenderer.Material is CarPaint)
                    {
                        CarPaintShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        CarPaintShader.Instance.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, (CarPaint)currentModelRenderer.Material);
                    }
                }
            }

            // The sky is render latter so that the GPU can avoid fragment processing. But it has to be before the transparent objects.

            // The particle systems

            // The transparent objects will be render in forward fashion.
            for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
            {
                ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Material.AlphaBlending != 1 && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                {
                    if (currentModelRenderer.Material is Constant)
                    {
                        ConstantShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
                        ConstantShader.Instance.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, (Constant)currentModelRenderer.Material);
                    }
                    else if (currentModelRenderer.Material is BlinnPhong)
                    {
                        ForwardBlinnPhongShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        ForwardBlinnPhongShader.Instance.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, (BlinnPhong)currentModelRenderer.Material, currentCamera.AmbientLight);
                    }
                    else if (currentModelRenderer.Material is CarPaint)
                    {
                        CarPaintShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        CarPaintShader.Instance.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, (CarPaint)currentModelRenderer.Material);
                    }
                }
            }

            sceneTexture = ScenePass.End();
            RenderTarget.Release(lightTexture);

            #endregion

            #region Post Process Pass

            postProcessedSceneTexture = PostProcessingPass.Process(sceneTexture, gbufferTextures.RenderTargets[0], currentCamera.PostProcess);
            RenderTarget.Release(sceneTexture); // It is not need anymore.
            RenderTarget.Release(gbufferTextures); // It is not need anymore.
            RenderTarget.Release(halfNormalTexture);
            RenderTarget.Release(halfDepthTexture);
            RenderTarget.Release(quarterNormalTexture);
            RenderTarget.Release(quarterDepthTexture);

            #endregion

            return postProcessedSceneTexture;
            //RenderTarget.Release(postProcessedSceneTexture);
            //return gbufferTextures.RenderTargets[1];
            //return lightTexture;
            //return ambientOcclusionTexture;
        } // RenderCamera

        #endregion

        #region Unload Content

        internal static void UnloadContent()
        {
            InputManager.DisableKeyboardHook();
        } // UnloadContent

        #endregion

    } // GameLoop
} // XNAFinalEngine.EngineCore