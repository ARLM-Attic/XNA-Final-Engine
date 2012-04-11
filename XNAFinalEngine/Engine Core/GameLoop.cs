
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Components;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using XNAFinalEngine.Audio;
using RootAnimation = XNAFinalEngine.Components.RootAnimations;
using XNAFinalEngine.Scenes;
using Camera = XNAFinalEngine.Components.Camera;
using DirectionalLight = XNAFinalEngine.Components.DirectionalLight;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The XNA Final Engine pipeline is defined here.
    /// </summary>
    /// <remarks>
    /// This class seems complex and in the pass I could agree.
    /// However to have the engine pipeline code in one class is not that terrible.
    /// Besides, I don’t put everything in here, just the callers to the specifics tasks.
    /// </remarks>
    public static class GameLoop
    {

        #region Variables
        
        /// <summary>
        /// This game object will show the frames per second onto screen.
        /// </summary>
        private static GameObject2D fpsText;

        // It's an auxiliary value that helps avoiding garbage.
        private static Vector3[] cornersViewSpace = new Vector3[4];

        private static readonly List<ModelRenderer> modelsToRender = new List<ModelRenderer>(50);
        private static readonly List<ModelRenderer> modelsToRenderShadow = new List<ModelRenderer>(50);

        private static readonly BoundingFrustum cameraBoundingFrustum = new BoundingFrustum(Matrix.Identity);

        private static AudioListener oneAudioListener;
        private static AudioListener[] twoAudioListener = new AudioListener[2];
        private static AudioListener[] threeAudioListener = new AudioListener[3];
        private static AudioListener[] fourAudioListener = new AudioListener[4];
        
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

            #region Load Managers

            // Create the 32 layers.
            Layer.Initialize();
            // Graphics
            SpriteManager.Initialize();
            // Input
            InputManager.Initialize();
            // Music
            MusicManager.Initialize();
            // Lines
            LineManager.Initialize();

            #endregion

            #region FPS

            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            fpsText = new GameObject2D();
            fpsText.AddComponent<HudText>();
            fpsText.HudText.Font = new Font("Arial12");
            fpsText.HudText.Color = Color.Yellow;
            fpsText.HudText.Text.Insert(0, "FPS ");
            fpsText.Transform.LocalRotation = 0f;

            #endregion

            #region Load Scene

            if (CurrentScene != null)
            {
                CurrentScene.Load();
            }

            #endregion

            #region Pre Draw

            // Pre draw to avoid the first frame's garbage and to place everything into memory.
            Draw(new GameTime());

            #endregion

            #region Garbage Collection

            // Collect all garbage.
            GarbageCollector.CollectGarbage();

            #endregion

            #region Statistics

            Statistics.InitStatistics();

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

            #region Managers

            InputManager.Update();
            MusicManager.Update();
            // Update the chronometers that work in game delta time space.
            Chronometer.UpdateGameDeltaTimeChronometers();
            
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

            #region Scene Late Update Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.LateUpdateTasks();
            }

            #endregion
            
            #region Scripts Late Update

            foreach (var script in Script.ScriptList)
            {
                script.LateUpdate();
            }

            #endregion

            #region Sound
            
            // Update the sound's general parameters.
            SoundManager.Update();

            #region Sound Listener

            // Count the active sound listeners.
            int audioListenerCount = 0;
            for (int i = 0; i < SoundListener.ComponentPool.Count; i++)
            {
                if (SoundListener.ComponentPool.Elements[i].Enabled)
                {
                    audioListenerCount++;
                }
            }
            if (audioListenerCount > 4)
            {
                throw new InvalidOperationException("Sound Manager: The maximum number of active audio listener is 4");
            }
            // Update and put into a list.
            int arrayindex = 0;
            for (int i = 0; i < SoundListener.ComponentPool.Count; i++)
            {
                if (SoundListener.ComponentPool.Elements[i].Enabled)
                {
                    SoundListener.ComponentPool.Elements[i].UpdateListenerProperties();
                    if (audioListenerCount == 1)
                        oneAudioListener = SoundListener.ComponentPool.Elements[i].audioListener;
                    else if (audioListenerCount == 2)
                        twoAudioListener[arrayindex] = SoundListener.ComponentPool.Elements[i].audioListener;
                    else if (audioListenerCount == 3)
                        threeAudioListener[arrayindex] = SoundListener.ComponentPool.Elements[i].audioListener;
                    else if (audioListenerCount == 4)
                        fourAudioListener[arrayindex] = SoundListener.ComponentPool.Elements[i].audioListener;
                    arrayindex++;
                }
            }

            #endregion
            
            #region Emitters
            
            // Update sound emitters.
            for (int i = 0; i < SoundEmitter.ComponentPool.Count; i++)
            {
                if (audioListenerCount <= 1)
                    SoundEmitter.ComponentPool.Elements[i].Update(oneAudioListener);
                else if (audioListenerCount == 2)
                    SoundEmitter.ComponentPool.Elements[i].Update(twoAudioListener);
                else if (audioListenerCount == 3)
                    SoundEmitter.ComponentPool.Elements[i].Update(threeAudioListener);
                else if (audioListenerCount == 4)
                    SoundEmitter.ComponentPool.Elements[i].Update(fourAudioListener);
            }
            
            #endregion

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

            #region Reser Frame Statistics

            // Reset Frame Statistics
            Statistics.ReserFrameStatistics();

            #endregion

            #region Update Chronometers

            // Update the chronometers that work in frame time space.
            Chronometer.UpdateFrameTimeChronometers();

            #endregion

            #region Update Frames Per Second Text

            // Update frames per second visibility.
            fpsText.HudText.Visible = ShowFramesPerSecond;
            fpsText.Transform.LocalPosition = new Vector3(Screen.Width - 100, 20, 0);
            fpsText.HudText.Text.Length = 4;
            fpsText.HudText.Text.AppendWithoutGarbage(Time.FramesPerSecond);

            #endregion

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
            
            #region Particles Emitters

            // Update particle emitters.
            for (int i = 0; i < ParticleEmitter.ComponentPool.Count; i++)
            {
                ParticleEmitter.ComponentPool.Elements[i].Update();
            }

            #endregion
            
            #region Render Each Camera
            
            // For each camera we render the scene in it
            for (int cameraIndex = 0; cameraIndex < Camera.ComponentPool.Count; cameraIndex++)
            {
                Camera currentCamera = Camera.ComponentPool.Elements[cameraIndex];
                // If is a master camera
                if (currentCamera.MasterCamera == null && currentCamera.Visible && Layer.IsActive(currentCamera.CachedLayerMask))
                {
                    if (currentCamera.RenderTarget != null)
                        RenderTarget.Release(currentCamera.RenderTarget);
                    // If it does not have slaves cameras...
                    if (currentCamera.slavesCameras.Count == 0 && currentCamera.NormalizedViewport == new RectangleF(0, 0, 1, 1))
                        currentCamera.RenderTarget = RenderCamera(currentCamera);
                    else
                    {
                        // Render each camera to a render target and then merge.
                        currentCamera.PartialRenderTarget = RenderCamera(currentCamera);
                        for (int i = 0; i < currentCamera.slavesCameras.Count; i++)
                        {
                            if (currentCamera.slavesCameras[i].Visible && Layer.IsActive(currentCamera.slavesCameras[i].CachedLayerMask))
                                // I store the render of the camera to a partial render target.
                                // This helps reduce the memory consumption (GBuffer, Light Pass, HDR pass)
                                // at the expense of a pass that copy this texture to a bigger render target
                                // and a last pass that copy the cameras’ render target to the back buffer.
                                // If the performance is critical and there is more memory you should change this behavior.
                                // It also simplified the render of one camera. 
                                currentCamera.slavesCameras[i].PartialRenderTarget = RenderCamera(currentCamera.slavesCameras[i]);
                        }
                        // Composite cameras
                        currentCamera.RenderTarget = RenderTarget.Fetch(currentCamera.RenderTargetSize, SurfaceFormat.Color, DepthFormat.None,
                                                                        RenderTarget.AntialiasingType.NoAntialiasing);
                        currentCamera.RenderTarget.EnableRenderTarget();
                        currentCamera.RenderTarget.Clear(currentCamera.ClearColor);
                        EngineManager.Device.Viewport = new Viewport(currentCamera.Viewport.X, currentCamera.Viewport.Y,
                                                                     currentCamera.Viewport.Width, currentCamera.Viewport.Height);
                        SpriteManager.DrawTextureToFullScreen(currentCamera.PartialRenderTarget, true);
                        RenderTarget.Release(currentCamera.PartialRenderTarget);
                        for (int i = 0; i < currentCamera.slavesCameras.Count; i++)
                        {
                            if (currentCamera.slavesCameras[i].Visible && Layer.IsActive(currentCamera.slavesCameras[i].CachedLayerMask))
                            {
                                EngineManager.Device.Viewport = new Viewport(currentCamera.slavesCameras[i].Viewport.X, currentCamera.slavesCameras[i].Viewport.Y,
                                                                             currentCamera.slavesCameras[i].Viewport.Width, currentCamera.slavesCameras[i].Viewport.Height);
                                SpriteManager.DrawTextureToFullScreen(currentCamera.slavesCameras[i].PartialRenderTarget, true);
                                RenderTarget.Release(currentCamera.slavesCameras[i].PartialRenderTarget);
                            }
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
                screenshotRenderTarget = new RenderTarget(Size.FullScreen, SurfaceFormat.Color, false);
                screenshotRenderTarget.EnableRenderTarget();
            }

            #endregion

            #region Render Main Camera to Back Buffer

            EngineManager.Device.Clear(Color.Black);
            // Render onto back buffer the main camera and the HUD.
            if (Camera.MainCamera != null && Camera.MainCamera.RenderTarget != null)
                SpriteManager.DrawTextureToFullScreen(Camera.MainCamera.RenderTarget);

            #endregion

            #region Heads Up Display

            // Draw 2D Heads Up Display
            SpriteManager.Begin2D();
            {

                #region HUD Text

                HudText currentHudText;
                for (int i = 0; i < HudText.ComponentPool2D.Count; i++)
                {
                    currentHudText = HudText.ComponentPool2D.Elements[i];
                    if (currentHudText.Visible && Layer.IsActive(currentHudText.CachedLayerMask))
                    {
                        SpriteManager.Draw2DText(currentHudText.Font ?? Font.DefaultFont,
                                               currentHudText.Text,
                                               currentHudText.CachedPosition,
                                               currentHudText.Color,
                                               currentHudText.CachedRotation,
                                               Vector2.Zero,
                                               currentHudText.CachedScale);
                    }
                }

                #endregion

                #region HUD Texture

                HudTexture currentHudTexture;
                for (int i = 0; i < HudTexture.ComponentPool2D.Count; i++)
                {
                    currentHudTexture = HudTexture.ComponentPool2D.Elements[i];
                    if (currentHudTexture.Visible && currentHudTexture.Texture != null && Layer.IsActive(currentHudTexture.CachedLayerMask))
                    {
                        if (currentHudTexture.DestinationRectangle != null)
                            SpriteManager.Draw2DTexture(currentHudTexture.Texture,
                                                      currentHudTexture.CachedPosition.Z,
                                                      currentHudTexture.DestinationRectangle.Value,
                                                      currentHudTexture.SourceRectangle,
                                                      currentHudTexture.Color,
                                                      currentHudTexture.CachedRotation,
                                                      Vector2.Zero);
                        else
                            SpriteManager.Draw2DTexture(currentHudTexture.Texture,
                                                      currentHudTexture.CachedPosition,
                                                      currentHudTexture.SourceRectangle,
                                                      currentHudTexture.Color,
                                                      currentHudTexture.CachedRotation,
                                                      Vector2.Zero,
                                                      currentHudTexture.CachedScale);
                    }
                }

                #endregion

                #region 2D Lines

                LineManager.Begin2D(PrimitiveType.TriangleList);
                for (int i = 0; i < LineRenderer.ComponentPool2D.Count; i++)
                {
                    LineRenderer currentLineRenderer = LineRenderer.ComponentPool2D.Elements[i];
                    if (currentLineRenderer.Vertices != null && currentLineRenderer.Visible &&
                        currentLineRenderer.PrimitiveType == PrimitiveType.TriangleList && Layer.IsActive(currentLineRenderer.CachedLayerMask))
                    {
                        for (int j = 0; j < currentLineRenderer.Vertices.Length; j++)
                            LineManager.AddVertex(currentLineRenderer.Vertices[j].Position, currentLineRenderer.Vertices[j].Color);
                    }
                }
                LineManager.End();

                LineManager.Begin2D(PrimitiveType.LineList);
                for (int i = 0; i < LineRenderer.ComponentPool2D.Count; i++)
                {
                    LineRenderer currentLineRenderer = LineRenderer.ComponentPool2D.Elements[i];
                    if (currentLineRenderer.Vertices != null && currentLineRenderer.Visible &&
                        currentLineRenderer.PrimitiveType == PrimitiveType.LineList && Layer.IsActive(currentLineRenderer.CachedLayerMask))
                    {
                        for (int j = 0; j < currentLineRenderer.Vertices.Length; j++)
                            LineManager.AddVertex(currentLineRenderer.Vertices[j].Position, currentLineRenderer.Vertices[j].Color);
                    }
                }
                LineManager.End();

                #endregion

                #region Videos

                VideoRenderer currentVideo;
                for (int i = 0; i < VideoRenderer.ComponentPool.Count; i++)
                {
                    currentVideo = VideoRenderer.ComponentPool.Elements[i];
                    currentVideo.Update();
                    if (currentVideo.Visible && currentVideo.Texture != null && Layer.IsActive(currentVideo.CachedLayerMask))
                    {
                        // Aspect ratio
                        Rectangle screenRectangle;
                        float videoAspectRatio = (float)currentVideo.Texture.Width / (float)currentVideo.Texture.Height,
                              screenAspectRatio = (float)Screen.Width / (float)Screen.Height;

                        if (videoAspectRatio > screenAspectRatio)
                        {
                            float vsAspectRatio = videoAspectRatio / screenAspectRatio;
                            int blackStripe = (int)((Screen.Height - (Screen.Height / vsAspectRatio)) / 2);
                            screenRectangle = new Rectangle(0, 0 + blackStripe, Screen.Width, Screen.Height - blackStripe * 2);
                        }
                        else
                        {
                            float vsAspectRatio = screenAspectRatio / videoAspectRatio;
                            int blackStripe = (int)((Screen.Width - (Screen.Width / vsAspectRatio)) / 2);
                            screenRectangle = new Rectangle(0 + blackStripe, 0, Screen.Width - blackStripe * 2, Screen.Height);
                        }
                        SpriteManager.Draw2DTexture(currentVideo.Texture,
                                                  currentVideo.CachedPosition.Z,
                                                  screenRectangle,
                                                  null,
                                                  Color.White,
                                                  0,
                                                  Vector2.Zero);
                    }
                }

                #endregion

            }
            SpriteManager.End();

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

            #region Release Shadow Light Depth Textures

            // We can do this from time to time to reduce calculations.
            // The problem is that I have to store the result for each camera.
            // And how much cameras do the game will have?
            for (int i = 0; i < SpotLight.ComponentPool.Count; i++)
            {
                if (SpotLight.ComponentPool.Elements[i].Shadow != null)
                {
                    RenderTarget.Release(SpotLight.ComponentPool.Elements[i].Shadow.LightDepthTexture);
                    SpotLight.ComponentPool.Elements[i].Shadow.LightDepthTexture = null;
                }
            }
            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                if (DirectionalLight.ComponentPool.Elements[i].Shadow != null)
                {
                    RenderTarget.Release(DirectionalLight.ComponentPool.Elements[i].Shadow.LightDepthTexture);
                    DirectionalLight.ComponentPool.Elements[i].Shadow.LightDepthTexture = null;
                }
            }

            #endregion
            
        } // Draw

        #endregion

        #region Frustum Culling

        /// <summary>
        /// Frustum Culling.
        /// </summary>
        /// <param name="boundingFrustum">Bounding Frustum.</param>
        /// <param name="modelsToRender">The result.</param>
        private static void FrustumCulling(BoundingFrustum boundingFrustum, List<ModelRenderer> modelsToRender)
        {
            for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
            {
                ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                {
                    if (boundingFrustum.Intersects(currentModelRenderer.BoundingSphere))
                    {
                        modelsToRender.Add(currentModelRenderer);
                    }
                }
            }
        } // FrustumCulling

        #endregion

        #region Render Camera

        private static RenderTarget RenderCamera(Camera currentCamera)
        {
            
            #region Buffers Declarations

            // This is here to allow the rendering of these render targets for testing.
            RenderTarget.RenderTargetBinding gbufferTextures;
            RenderTarget lightTexture = null;
            RenderTarget sceneTexture = null;
            RenderTarget postProcessedSceneTexture = null;
            RenderTarget ambientOcclusionTexture = null;
            RenderTarget halfNormalTexture = null;
            RenderTarget halfDepthTexture = null;
            RenderTarget quarterNormalTexture = null;
            RenderTarget quarterDepthTexture = null;

            #endregion

            #region Create Camera Bounding Frustum

            // Calculate view space bounding frustum.
            currentCamera.BoundingFrustum(cornersViewSpace);

            #endregion

            #region Camera Culling Mask

            Layer.CurrentCameraCullingMask = currentCamera.CullingMask;

            #endregion

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

            #region Frustum Culling

            // The objective is implementing a culling management in a limited time framework.
            // In DICE’s presentation (Culling the Battlefield Data Oriented Design in Practice)
            // they find that a slightly modified simple frustum culling could work better than 
            // a tree based structure if a data oriented design is followed. 
            // The question is if C# could bring me the possibility to arrange data the way I need it or not.
            // Then they apply a software occlusion culling technique, an interesting approach
            // but unfortunately I don’t think I can make it work in the time that I have.

            // I will try to make a simple version and then optimized.
            // First I will try to do frustum culling with bounding spheres.
            // DICE talk about grids, I still do not understand why. It is to separate better the data send to the cores?
            // DICE also stores in an array only the bounding and entity information. This is something that I already find necessary to do some months before ago.
            // They also store AABB information to perform the software occlusion culling in the next pass.
            // They also improve a lot the performance of the intersect operation, however I will relay in the XNA implementation, at least for the time being.
            // Finally, I should implement a multi frustum culling (cameras and lights) to improve performance. 
            // Another reference about this: http://blog.selfshadow.com/publications/practical-visibility/
            // Reading this last link I concluded that probably understood incorrectly some part of the method.

            // CHC++ is a technique very used. In ShaderX7 there are a good article about it (it also includes the source code).

            // First Version (very simple)
            cameraBoundingFrustum.Matrix = currentCamera.ViewMatrix*currentCamera.ProjectionMatrix;
            modelsToRender.Clear();
            FrustumCulling(cameraBoundingFrustum, modelsToRender);

            #endregion

            #region GBuffer Pass

            GBufferPass.Begin(destinationSize);
            GBufferShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane);
            for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
            {
                ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Material.AlphaBlending == 1 && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                {
                    GBufferShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, currentModelRenderer.Material);
                }
            }
            gbufferTextures = GBufferPass.End();
            
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
            
            #endregion
            
            #region Light Pre Pass

            #region Ambient Occlusion

            if (currentCamera.AmbientLight.AmbientOcclusion != null && currentCamera.AmbientLight.AmbientOcclusion.Enabled)
            {
                RenderTarget aoDepthTexture, aoNormalTexture;
                if (currentCamera.AmbientLight.AmbientOcclusion.TextureSize == Size.TextureSize.FullSize)
                {
                    aoDepthTexture = gbufferTextures.RenderTargets[0];
                    aoNormalTexture = gbufferTextures.RenderTargets[1];
                }
                else if (currentCamera.AmbientLight.AmbientOcclusion.TextureSize == Size.TextureSize.HalfSize)
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

            #endregion

            #region Shadow Maps

            #region Directional Light Shadows

            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                DirectionalLight currentDirectionalLight = DirectionalLight.ComponentPool.Elements[i];
                // If there is a shadow map...
                if (currentDirectionalLight.Shadow != null && currentDirectionalLight.Shadow.Enabled && currentDirectionalLight.Visible &&
                    currentDirectionalLight.Intensity > 0 && Layer.IsActive(currentDirectionalLight.CachedLayerMask))
                {
                    RenderTarget shadowDepthTexture;
                    if (currentDirectionalLight.Shadow.TextureSize == Size.TextureSize.FullSize)
                        shadowDepthTexture = gbufferTextures.RenderTargets[0];
                    else if (currentDirectionalLight.Shadow.TextureSize == Size.TextureSize.HalfSize)
                        shadowDepthTexture = halfDepthTexture;
                    else
                        shadowDepthTexture = quarterDepthTexture;

                    // If the shadow map is a cascaded shadow map...
                    if (currentDirectionalLight.Shadow is CascadedShadow)
                    {
                        CascadedShadow shadow = (CascadedShadow)currentDirectionalLight.Shadow;
                        // TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        if (shadow.LightDepthTexture != null)
                            RenderTarget.Release(shadow.LightDepthTexture);
                        CascadedShadowMapShader.Instance.Begin(shadow.LightDepthTextureSize, shadowDepthTexture, shadow.DepthBias, shadow.Filter);
                        CascadedShadowMapShader.Instance.SetLight(currentDirectionalLight.cachedDirection, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix,
                                                                    currentCamera.NearPlane, currentCamera.FarPlane, cornersViewSpace);
                        //FrustumCulling(new BoundingFrustum(), modelsToRenderShadow);
                        // Render all the opaque objects...
                        for (int j = 0; j < ModelRenderer.ComponentPool.Count; j++)
                        {
                            ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[j];
                            if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Material.AlphaBlending == 1 && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                            {
                                CascadedShadowMapShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms);
                            }
                        }
                        currentDirectionalLight.ShadowTexture = CascadedShadowMapShader.Instance.End(ref shadow.LightDepthTexture);
                    }
                    // If the shadow map is a basic shadow map...
                    else if (currentDirectionalLight.Shadow is BasicShadow)
                    {
                        BasicShadow shadow = (BasicShadow)currentDirectionalLight.Shadow;
                        // TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        if (shadow.LightDepthTexture != null)
                            RenderTarget.Release(shadow.LightDepthTexture);
                        BasicShadowMapShader.Instance.Begin(shadow.LightDepthTextureSize, shadowDepthTexture, shadow.DepthBias, shadow.Filter);
                        BasicShadowMapShader.Instance.SetLight(currentDirectionalLight.cachedDirection, currentCamera.ViewMatrix, shadow.Range, cornersViewSpace);
                        //FrustumCulling(new BoundingFrustum(), modelsToRenderShadow);
                        // Render all the opaque objects...
                        for (int j = 0; j < ModelRenderer.ComponentPool.Count; j++)
                        {
                            ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[j];
                            if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Material.AlphaBlending == 1 && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                            {
                                BasicShadowMapShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms);
                            }
                        }
                        currentDirectionalLight.ShadowTexture = BasicShadowMapShader.Instance.End(ref shadow.LightDepthTexture);
                    }
                }
            }

            #endregion

            #region Spot Light Shadows

            for (int i = 0; i < SpotLight.ComponentPool.Count; i++)
            {
                SpotLight currentSpotLight = SpotLight.ComponentPool.Elements[i];
                // If there is a shadow map...
                if (currentSpotLight.Shadow != null && currentSpotLight.Shadow.Enabled && currentSpotLight.Visible &&
                    currentSpotLight.Intensity > 0 && Layer.IsActive(currentSpotLight.CachedLayerMask))
                {
                    RenderTarget shadowDepthTexture;
                    if (currentSpotLight.Shadow.TextureSize == Size.TextureSize.FullSize)
                        shadowDepthTexture = gbufferTextures.RenderTargets[0];
                    else if (currentSpotLight.Shadow.TextureSize == Size.TextureSize.HalfSize)
                        shadowDepthTexture = halfDepthTexture;
                    else
                        shadowDepthTexture = quarterDepthTexture;

                    // If the shadow map is a cascaded shadow map...
                    if (currentSpotLight.Shadow is BasicShadow)
                    {
                        BasicShadow shadow = (BasicShadow)currentSpotLight.Shadow;
                        if (shadow.LightDepthTexture == null)
                        {
                            BasicShadowMapShader.Instance.Begin(shadow.LightDepthTextureSize, shadowDepthTexture, shadow.DepthBias, shadow.Filter);
                            BasicShadowMapShader.Instance.SetLight(currentSpotLight.cachedPosition, currentSpotLight.cachedDirection, currentCamera.ViewMatrix, currentSpotLight.OuterConeAngle,
                                                                   currentSpotLight.Range, cornersViewSpace);
                            //FrustumCulling(new BoundingFrustum(), modelsToRenderShadow);
                            // Render all the opaque objects...
                            for (int j = 0; j < ModelRenderer.ComponentPool.Count; j++)
                            {
                                ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[j];
                                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Material.AlphaBlending == 1 && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                                {
                                    BasicShadowMapShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms);
                                }
                            }
                            currentSpotLight.ShadowTexture = BasicShadowMapShader.Instance.End(ref shadow.LightDepthTexture);
                        }
                        else
                        {
                            currentSpotLight.ShadowTexture = BasicShadowMapShader.Instance.ProcessWithPrecalculedLightDepthTexture(shadow.LightDepthTexture);
                        }
                    }
                }
            }

            #endregion

            #endregion

            #region Light Texture

            LightPrePass.Begin(destinationSize, currentCamera.AmbientLight.Color);

            #region Ambient Light

            // Render ambient light for every camera.
            if (currentCamera.AmbientLight != null)
            {
                AmbientLightShader.Instance.RenderLight(gbufferTextures.RenderTargets[1], // Normal Texture
                                                        currentCamera.AmbientLight,
                                                        ambientOcclusionTexture,
                                                        currentCamera.ViewMatrix);
            }
            RenderTarget.Release(ambientOcclusionTexture);

            #endregion

            #region Directional Lights

            DirectionalLightShader.Instance.Begin(gbufferTextures.RenderTargets[0], // Depth Texture
                                            gbufferTextures.RenderTargets[1], // Normal Texture
                                            gbufferTextures.RenderTargets[2], // Motion Vector Specular Power
                                            currentCamera.ViewMatrix,
                                            cornersViewSpace);
            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                DirectionalLight currentDirectionalLight = DirectionalLight.ComponentPool.Elements[i];
                if (currentDirectionalLight.Visible && currentDirectionalLight.Intensity > 0 && Layer.IsActive(currentDirectionalLight.CachedLayerMask))
                {
                    DirectionalLightShader.Instance.RenderLight(currentDirectionalLight.DiffuseColor, currentDirectionalLight.cachedDirection,
                                                                currentDirectionalLight.Intensity, currentDirectionalLight.ShadowTexture);
                }
            }

            #endregion

            #region Point Lights
            
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
                if (currentPointLight.Visible && currentPointLight.Intensity > 0 && Layer.IsActive(currentPointLight.CachedLayerMask))
                {
                    PointLightShader.Instance.RenderLight(currentPointLight.DiffuseColor, currentPointLight.cachedPosition, currentPointLight.Intensity, currentPointLight.Range);
                }
            }

            #endregion

            #region Spot Lights
            
            SpotLightShader.Instance.Begin(gbufferTextures.RenderTargets[0], // Depth Texture
                                           gbufferTextures.RenderTargets[1], // Normal Texture
                                           gbufferTextures.RenderTargets[2], // Motion Vector Specular Power
                                           currentCamera.ViewMatrix,
                                           currentCamera.ProjectionMatrix,
                                           currentCamera.NearPlane,
                                           currentCamera.FarPlane);
            for (int i = 0; i < SpotLight.ComponentPool.Count; i++)
            {
                SpotLight currentSpotLight = SpotLight.ComponentPool.Elements[i];
                if (currentSpotLight.Visible && currentSpotLight.Intensity > 0 && Layer.IsActive(currentSpotLight.CachedLayerMask))
                {
                    SpotLightShader.Instance.RenderLight(currentSpotLight.DiffuseColor, currentSpotLight.cachedPosition,
                                                         currentSpotLight.cachedDirection, currentSpotLight.Intensity,
                                                         currentSpotLight.Range, currentSpotLight.InnerConeAngle,
                                                         currentSpotLight.OuterConeAngle, currentSpotLight.ShadowTexture, currentSpotLight.LightMaskTexture);
                }
            }

            #endregion

            lightTexture = LightPrePass.End();

            #endregion

            #region Release Shadow Textures

            // We can do this from time to time to reduce calculations.
            for (int i = 0; i < SpotLight.ComponentPool.Count; i++)
            {
                if (SpotLight.ComponentPool.Elements[i].ShadowTexture != null)
                {
                    RenderTarget.Release(SpotLight.ComponentPool.Elements[i].ShadowTexture);
                    SpotLight.ComponentPool.Elements[i].ShadowTexture = null;
                }
            }
            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                if (DirectionalLight.ComponentPool.Elements[i].ShadowTexture != null)
                {
                    RenderTarget.Release(DirectionalLight.ComponentPool.Elements[i].ShadowTexture);
                    DirectionalLight.ComponentPool.Elements[i].ShadowTexture = null;
                }
            }
            for (int i = 0; i < PointLight.ComponentPool.Count; i++)
            {
                if (PointLight.ComponentPool.Elements[i].ShadowTexture != null)
                {
                    RenderTarget.Release(PointLight.ComponentPool.Elements[i].ShadowTexture);
                    PointLight.ComponentPool.Elements[i].ShadowTexture = null;
                }
            }

            #endregion

            #endregion

            #region HDR Linear Space Pass

            ScenePass.Begin(destinationSize, currentCamera.ClearColor);
            
            #region Opaque Objects

            // Render all the opaque objects...
            for (int i = 0; i < modelsToRender.Count; i++)
            {
                ModelRenderer currentModelRenderer = modelsToRender[i];
                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null && currentModelRenderer.Material.AlphaBlending == 1 &&
                    currentModelRenderer.Visible && Layer.IsActive(currentModelRenderer.CachedLayerMask))
                {
                    if (currentModelRenderer.Material is Constant)
                    {
                        ConstantShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
                        ConstantShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, (Constant)currentModelRenderer.Material);
                    }
                    else if (currentModelRenderer.Material is BlinnPhong)
                    {
                        BlinnPhongShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        BlinnPhongShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, (BlinnPhong)currentModelRenderer.Material);
                    }
                    else if (currentModelRenderer.Material is CarPaint)
                    {
                        CarPaintShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        CarPaintShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, (CarPaint)currentModelRenderer.Material);
                    }
                }
            }

            #endregion
            
            #region Sky

            // The sky is render latter so that the GPU can avoid fragment processing. But it has to be done before the transparent objects.
            if (currentCamera.Sky != null)
            {
                if (currentCamera.Sky is Skybox && ((Skybox)currentCamera.Sky).TextureCube != null)
                {
                    SkyboxShader.Instance.Render(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane, (Skybox)(currentCamera.Sky));
                }
                if (currentCamera.Sky is Skydome && ((Skydome)currentCamera.Sky).Texture != null)
                {
                    SkydomeShader.Instance.Render(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane, (Skydome)(currentCamera.Sky));
                }
            }

            #endregion

            #region Particles
            
            ParticleShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.AspectRatio, currentCamera.FarPlane,
                                          new Size(currentCamera.Viewport.Width, currentCamera.Viewport.Height), gbufferTextures.RenderTargets[0]);
            for (int i = 0; i < ParticleRenderer.ComponentPool.Count; i++)
            {
                ParticleRenderer currentParticleRenderer = ParticleRenderer.ComponentPool.Elements[i];
                if (currentParticleRenderer.cachedParticleSystem != null && currentParticleRenderer.Texture != null && 
                    currentParticleRenderer.Visible && Layer.IsActive(currentParticleRenderer.CachedLayerMask))
                    ParticleShader.Instance.Render(currentParticleRenderer.cachedParticleSystem, currentParticleRenderer.Duration,
                                                   currentParticleRenderer.BlendState, currentParticleRenderer.DurationRandomness, currentParticleRenderer.Gravity,
                                                   currentParticleRenderer.EndVelocity, currentParticleRenderer.MinimumColor, currentParticleRenderer.MaximumColor,
                                                   currentParticleRenderer.RotateSpeed, currentParticleRenderer.StartSize, currentParticleRenderer.EndSize,
                                                   currentParticleRenderer.Texture, currentParticleRenderer.SoftParticles, currentParticleRenderer.FadeDistance);
            }
            
            #endregion

            #region Transparent Objects
            
            // The transparent objects will be render in forward fashion.
            for (int i = 0; i < modelsToRender.Count; i++)
            {
                ModelRenderer currentModelRenderer = modelsToRender[i];
                if (currentModelRenderer.CachedModel != null && currentModelRenderer.Material != null &&
                    currentModelRenderer.Material.AlphaBlending != 1 && currentModelRenderer.Visible &&
                    Layer.IsActive(currentModelRenderer.CachedLayerMask))
                {
                    if (currentModelRenderer.Material is Constant)
                    {
                        ConstantShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
                        ConstantShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, (Constant)currentModelRenderer.Material);
                    }
                    else if (currentModelRenderer.Material is BlinnPhong)
                    {
                        ForwardBlinnPhongShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        ForwardBlinnPhongShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, currentModelRenderer.cachedBoneTransforms, (BlinnPhong)currentModelRenderer.Material, currentCamera.AmbientLight);
                    }
                    else if (currentModelRenderer.Material is CarPaint)
                    {
                        CarPaintShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTexture);
                        CarPaintShader.Instance.RenderModel(currentModelRenderer.CachedWorldMatrix, currentModelRenderer.CachedModel, (CarPaint)currentModelRenderer.Material);
                    }
                }
            }
            
            #endregion

            #region Textures and Text

            if (HudText.ComponentPool3D.Count != 0 || HudTexture.ComponentPool3D.Count != 0)
            {
                SpriteManager.Begin3DLinearSpace(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
                for (int i = 0; i < HudTexture.ComponentPool3D.Count; i++)
                {
                    HudTexture currentHudTexture = HudTexture.ComponentPool3D.Elements[i];
                    if (currentHudTexture.Visible && currentHudTexture.Texture != null && currentHudTexture.PostProcessed && Layer.IsActive(currentHudTexture.CachedLayerMask))
                    {
                        if (currentHudTexture.Billboard)
                        {
                            if (currentHudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DBillboardTexture(currentHudTexture.Texture,
                                                                     currentHudTexture.CachedWorldMatrix,
                                                                     currentHudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                            else
                                SpriteManager.Draw3DBillboardTexture(currentHudTexture.Texture,
                                                                     currentHudTexture.CachedWorldMatrix,
                                                                     currentHudTexture.SourceRectangle,
                                                                     currentHudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                        }
                        else
                        {
                            if (currentHudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DTexture(currentHudTexture.Texture,
                                                          currentHudTexture.CachedWorldMatrix,
                                                          currentHudTexture.SourceRectangle,
                                                          currentHudTexture.Color);
                            else
                                SpriteManager.Draw3DTexture(currentHudTexture.Texture,
                                                          currentHudTexture.CachedWorldMatrix,
                                                          currentHudTexture.Color);    
                        }
                    }
                }
                for (int i = 0; i < HudText.ComponentPool3D.Count; i++)
                {
                    HudText currentHudText = HudText.ComponentPool3D.Elements[i];
                    if (currentHudText.Visible && currentHudText.PostProcessed && Layer.IsActive(currentHudText.CachedLayerMask))
                    {
                        if (currentHudText.Billboard)
                            SpriteManager.Draw3DBillboardText(currentHudText.Font ?? Font.DefaultFont,
                                                              currentHudText.Text,
                                                              currentHudText.CachedWorldMatrix,
                                                              currentHudText.Color,
                                                              currentCamera.Position,
                                                              currentCamera.Up,
                                                              currentCamera.Forward);
                        else
                            SpriteManager.Draw3DText(currentHudText.Font ?? Font.DefaultFont, currentHudText.Text, currentHudText.CachedWorldMatrix, currentHudText.Color);
                        
                    }
                }
                SpriteManager.End();
            }

            #endregion

            sceneTexture = ScenePass.End();
            RenderTarget.Release(lightTexture);

            #endregion
            
            #region Post Process Pass

            PostProcessingPass.BeginAndProcess(sceneTexture, gbufferTextures.RenderTargets[0], currentCamera.PostProcess);
            // Render in gamma space
            
            #region Textures and Text

            if (HudText.ComponentPool3D.Count != 0 || HudTexture.ComponentPool3D.Count != 0)
            {
                SpriteManager.Begin3DGammaSpace(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, gbufferTextures.RenderTargets[0], currentCamera.FarPlane);
                for (int i = 0; i < HudTexture.ComponentPool3D.Count; i++)
                {
                    HudTexture currentHudTexture = HudTexture.ComponentPool3D.Elements[i];
                    if (currentHudTexture.Visible && currentHudTexture.Texture != null && !currentHudTexture.PostProcessed && Layer.IsActive(currentHudTexture.CachedLayerMask))
                    {
                        if (currentHudTexture.Billboard)
                        {
                            if (currentHudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DBillboardTexture(currentHudTexture.Texture,
                                                                     currentHudTexture.CachedWorldMatrix,
                                                                     currentHudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                            else
                                SpriteManager.Draw3DBillboardTexture(currentHudTexture.Texture,
                                                                     currentHudTexture.CachedWorldMatrix,
                                                                     currentHudTexture.SourceRectangle,
                                                                     currentHudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                        }
                        else
                        {
                            if (currentHudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DTexture(currentHudTexture.Texture,
                                                          currentHudTexture.CachedWorldMatrix,
                                                          currentHudTexture.SourceRectangle,
                                                          currentHudTexture.Color);
                            else
                                SpriteManager.Draw3DTexture(currentHudTexture.Texture,
                                                          currentHudTexture.CachedWorldMatrix,
                                                          currentHudTexture.Color);
                        }
                    }
                }
                for (int i = 0; i < HudText.ComponentPool3D.Count; i++)
                {
                    HudText currentHudText = HudText.ComponentPool3D.Elements[i];
                    if (currentHudText.Visible && !currentHudText.PostProcessed && Layer.IsActive(currentHudText.CachedLayerMask))
                    {
                        if (currentHudText.Billboard)
                            SpriteManager.Draw3DBillboardText(currentHudText.Font ?? Font.DefaultFont,
                                                              currentHudText.Text,
                                                              currentHudText.CachedWorldMatrix,
                                                              currentHudText.Color,
                                                              currentCamera.Position,
                                                              currentCamera.Up,
                                                              currentCamera.Forward);
                        else
                            SpriteManager.Draw3DText(currentHudText.Font ?? Font.DefaultFont, currentHudText.Text, currentHudText.CachedWorldMatrix, currentHudText.Color);

                    }
                }
                SpriteManager.End();
            }

            #endregion

            #region Lines (Line List)

            LineManager.Begin3D(PrimitiveType.LineList, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
            
            #region Bounding Volumes

            for (int i = 0; i < ModelRenderer.ComponentPool.Count; i++)
            {
                ModelRenderer currentModelRenderer = ModelRenderer.ComponentPool.Elements[i];
                if (currentModelRenderer.CachedModel != null && (currentModelRenderer.RenderBoundingBox || currentModelRenderer.RenderBoundingSphere) &&
                    Layer.IsActive(currentModelRenderer.CachedLayerMask))
                {
                    if (currentModelRenderer.RenderBoundingBox)
                    {
                        LineManager.DrawBoundingBox(currentModelRenderer.BoundingBox, Color.Red);
                    }
                    if (currentModelRenderer.RenderBoundingSphere)
                    {
                        LineManager.DrawBoundingSphere(currentModelRenderer.BoundingSphere, Color.Green);
                    }
                }
            }

            #endregion

            #region 3D Lines (Line List)

            for (int i = 0; i < LineRenderer.ComponentPool3D.Count; i++)
            {
                LineRenderer currentLineRenderer = LineRenderer.ComponentPool3D.Elements[i];
                if (currentLineRenderer.Vertices != null && currentLineRenderer.Visible && currentLineRenderer.PrimitiveType == PrimitiveType.LineList &&
                    Layer.IsActive(currentLineRenderer.CachedLayerMask))
                {
                    for (int j = 0; j < currentLineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(currentLineRenderer.Vertices[j].Position, currentLineRenderer.Vertices[j].Color);
                }
            }

            #endregion

            LineManager.End();

            #endregion

            #region 3D Lines (Triangle List)

            LineManager.Begin3D(PrimitiveType.TriangleList, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);

            for (int i = 0; i < LineRenderer.ComponentPool3D.Count; i++)
            {
                LineRenderer currentLineRenderer = LineRenderer.ComponentPool3D.Elements[i];
                if (currentLineRenderer.Vertices != null && currentLineRenderer.Visible &&
                    currentLineRenderer.PrimitiveType == PrimitiveType.TriangleList && Layer.IsActive(currentLineRenderer.CachedLayerMask))
                {
                    for (int j = 0; j < currentLineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(currentLineRenderer.Vertices[j].Position, currentLineRenderer.Vertices[j].Color);
                }
            }

            LineManager.End();

            #endregion
            
            postProcessedSceneTexture = PostProcessingPass.End();

            // They are not needed anymore.
            RenderTarget.Release(sceneTexture);
            RenderTarget.Release(gbufferTextures);
            RenderTarget.Release(halfNormalTexture);
            RenderTarget.Release(halfDepthTexture);
            RenderTarget.Release(quarterNormalTexture);
            RenderTarget.Release(quarterDepthTexture);

            #endregion

            #region Reset Camera Culling Mask

            Layer.CurrentCameraCullingMask = uint.MaxValue;

            #endregion

            return postProcessedSceneTexture;

            #region For Testing
            //RenderTarget.Release(postProcessedSceneTexture);
            //return gbufferTextures.RenderTargets[1];
            //return lightTexture;
            #endregion

        } // RenderCamera

        #endregion

        #region Unload Content

        internal static void UnloadContent()
        {
            // Disable wiimote and keyboard hook.
            InputManager.UnloadInputDevices();
        } // UnloadContent

        #endregion

    } // GameLoop
} // XNAFinalEngine.EngineCore