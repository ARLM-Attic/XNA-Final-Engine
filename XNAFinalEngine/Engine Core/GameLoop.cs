
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
using System.Runtime;
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

        // TODO!!! Esto no va.
        private static GBuffer gbuffer;
        private static LightPrePass lightPrePass;
        private static DirectionalLightShader directionalLightShader;
        private static ScenePass hdrLinearSpacePass;
        private static ConstantShader constantShader;
        private static BlinnPhongShader blinnPhongShader;
        private static PostProcessingPass postProcessPass;
        private static PostProcess postProcess;
        private static MLAAShader mlaaShader;

        /// <summary>
        /// This game object will show the frames per second onto screen.
        /// </summary>
        private static GameObject2D fpsText;
        
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
            
            gbuffer = new GBuffer(Size.FullScreen);
            lightPrePass = new LightPrePass(Size.FullScreen);
            directionalLightShader = new DirectionalLightShader();
            hdrLinearSpacePass = new ScenePass(Size.FullScreen);
            constantShader = new ConstantShader();
            blinnPhongShader = new BlinnPhongShader();
            postProcessPass = new PostProcessingPass(Size.FullScreen);
            postProcess = new PostProcess
                              {
                                  FilmGrain = new FilmGrain(),
                                  Bloom = new Bloom(),
                                  AdjustLevels = new AdjustLevels(),
                                  MLAA = new MLAA { EdgeDetection = MLAA.EdgeDetectionType.Color, BlurRadius = 2}
                              };
            mlaaShader = new MLAAShader(Size.FullScreen);
            
            if (CurrentScene != null)
            {
                CurrentScene.Load();
            }

            #region Garbage Collection

            // All generations will undergo a garbage collection.
            #if (WINDOWS)
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            #else
                GC.Collect();
            #endif
            // Enables garbage collection that is more conservative in reclaiming objects.
            // Full Collections occur only if the system is under memory pressure while generation 0 and generation 1 collections might occur more frequently.
            // This is the least intrusive mode.
            // If the work is done right, this latency mode is not need really.
            #if (WINDOWS)
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            #endif
            //TestGarbageCollection.CreateWeakReference();

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
            
            InputManager.Update();

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
                // If does not have a render target
                if (currentCamera.RenderTarget == null)
                    currentCamera.RenderTarget = new RenderTarget(currentCamera.RenderTargetSize, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
                if (currentCamera.PartialRenderTarget == null)
                    currentCamera.PartialRenderTarget = new RenderTarget(currentCamera.RenderTargetSize, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);

                #region GBuffer Pass
                
                gbuffer.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane);
                for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
                {
                    ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                    if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                    {
                        gbuffer.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, currentModelRenderer.Material);
                    }
                }
                gbuffer.End();

                #endregion
                
                #region Light Pre Pass
                    
                lightPrePass.Begin(currentCamera.AmbientLight.Color);

                // Render ambient light for every camera.

                // Render directional lights for every camera.
                directionalLightShader.Begin(gbuffer.DepthTexture, gbuffer.NormalTexture, gbuffer.MotionVectorsSpecularPowerTexture, currentCamera.ViewMatrix, currentCamera.BoundingFrustum());
                for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
                {
                    DirectionalLight currentDirectionalLight = DirectionalLight.ComponentPool.Elements[i];
                    directionalLightShader.RenderLight(currentDirectionalLight.DiffuseColor, currentDirectionalLight.cachedDirection, currentDirectionalLight.Intensity);
                }

                // Render point lights for every camera.

                // Render spot lights for every camera.

                lightPrePass.End();
                    
                #endregion
                
                #region HDR Linear Space Pass

                hdrLinearSpacePass.Begin(currentCamera.ClearColor);

                // Render all the opaque objects);
                for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
                {
                    ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                    if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                    {
                        if (currentModelRenderer.Material is Constant)
                        {
                            constantShader.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
                            constantShader.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, (Constant)currentModelRenderer.Material);
                        }
                        else if (currentModelRenderer.Material is BlinnPhong)
                        {
                            blinnPhongShader.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightPrePass.LightTexture);
                            blinnPhongShader.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, (BlinnPhong)currentModelRenderer.Material);
                        }
                    }
                }

                // The sky is render latter so that the GPU can avoid fragment processing. But it has to be before the transparent objects.

                // The particle systems

                // The transparent objects will be render in forward fashion.

                hdrLinearSpacePass.End();

                #endregion
                
                #region Post Process Pass

                postProcessPass.Render(hdrLinearSpacePass.SceneTexture, postProcess);
                mlaaShader.Filter(postProcessPass.PostProcessedSceneTexture, gbuffer.DepthTexture, postProcess);

                #endregion

            }

            // If it is a master camera and it does not have slaves...
            /*if (currentCamera.MasterCamera == null && currentCamera.slavesCameras.Count == 0)
            {
                SpriteManager.DrawTextureToFullScreen(postProcessPass.PostProcessedSceneTexture);
            }*/
            
            #endregion

            currentCamera.RenderTarget.EnableRenderTarget();
            currentCamera.RenderTarget.Clear(currentCamera.ClearColor);
            SpriteManager.DrawTextureToFullScreen(postProcessPass.PostProcessedSceneTexture);
            // Composite the different viewports
            /*for (int i = 0; i < currentCamera.slavesCameras.Count; i++)
                currentCamera.slavesCameras[i];*/
            currentCamera.RenderTarget.DisableRenderTarget();

            #region Screenshot Preparations

            RenderTarget screenshotRenderTarget = null;
            if (ScreenshotCapturer.MakeScreenshot)
            {
                // Instead of render into the back buffer we render into a render target.
                screenshotRenderTarget = new RenderTarget(Size.FullScreen, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
                screenshotRenderTarget.EnableRenderTarget();
            }

            #endregion

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

            #region Screenshot

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

        #endregion

        #region Unload Content

        internal static void UnloadContent()
        {
            InputManager.DisableKeyboardHook();
        } // UnloadContent

        #endregion

    } // GameLoop
} // XNAFinalEngine.EngineCore