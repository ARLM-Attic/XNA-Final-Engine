
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Animations;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Audio;
using XNAFinalEngine.Components;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using DirectionalLight = XNAFinalEngine.Components.DirectionalLight;
using Model = XNAFinalEngine.Assets.Model;
using RootAnimation = XNAFinalEngine.Components.RootAnimations;
using XNAFinalEngine.Physics;
using RenderTargetCube = XNAFinalEngine.Assets.RenderTargetCube;
using Texture = Microsoft.Xna.Framework.Graphics.Texture;

#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The XNA Final Engine pipeline is defined here. 
    /// There is no need for user input, all operations are carried automatically.
    /// </summary>
    /// <remarks>
    /// This class seems complex and in the pass I could agree.
    /// However to have the engine pipeline code in one class has its advantages.
    /// Besides, I don’t put everything in here, just the callers to the specifics tasks.
    /// </remarks>
    public static class GameLoop
    {

        #region Structs
        
        // Used to render the mesh parts more efficently.
        private struct MeshPartToRender
        {
            public Matrix WorldMatrix;
            public Model Model;
            public Matrix[] BoneTransform;
            public Material Material;
            public int MeshIndex;
            public int MeshPart;
        } // MeshPartToRender

        private struct DirectionalLightDepthInformation
        {
            // To avoid garbage I do not store an array of matrices.
            // I can't copy the array reference either, I need to clone the values.
            public Matrix lightViewMatrix;
            public Matrix lightViewMatrix2;
            public Matrix lightViewMatrix3;
            public Matrix lightViewMatrix4;
            public Matrix lightProjectionMatrix;
            public Matrix lightProjectionMatrix2;
            public Matrix lightProjectionMatrix3;
            public Matrix lightProjectionMatrix4;
            public RenderTarget lightDepthTexture;
            public DirectionalLight light;
            public Camera camera;
        } // DirectionalLightDepthInformation

        private struct SpotLightDepthInformation
        {
            public Matrix lightViewMatrix;
            public Matrix lightProjectionMatrix;
            public SpotLight light;
            public RenderTarget lightDepthTexture;
            public bool calculatedThisFrame;
        } // SpotLightDepthInformation

        private struct PointLightDepthInformation
        {
            public PointLight light;
            public RenderTargetCube lightDepthTexture;
            public bool calculatedThisFrame;
        } // PointLightDepthInformation

        #endregion

        #region Variables

        // Used in the transparent sorting.
        private static Vector3 cameraPosition;

        // Render Targets used in the deferred lighting pipeline.
        private static RenderTarget.RenderTargetBinding gbufferTextures;
        private static RenderTarget.RenderTargetBinding gbufferHalfTextures;
        private static RenderTarget.RenderTargetBinding gbufferQuarterTextures;
        private static RenderTarget.RenderTargetBinding lightTextures;
        private static RenderTarget sceneTexture;
        private static RenderTarget ambientOcclusionTexture;

        // They are auxiliary values that helps avoiding garbage.
        private static readonly Vector3[] cornersViewSpace = new Vector3[4];
        private static readonly BoundingFrustum cameraBoundingFrustum = new BoundingFrustum(Matrix.Identity);

        // The system can have 0, 1, 2 and 3 audio lister.
        // These fields are used to avoid garbage in each sound update.
        private static AudioListener oneAudioListener;
        private static readonly AudioListener[] twoAudioListener   = new AudioListener[2];
        private static readonly AudioListener[] threeAudioListener = new AudioListener[3];
        private static readonly AudioListener[] fourAudioListener  = new AudioListener[4];

        // Frustum Culling.
        private static readonly List<ModelRenderer> modelsToRender      = new List<ModelRenderer>(100);
        private static readonly List<ModelRenderer> modelsToRenderShadows = new List<ModelRenderer>(100);
        private static readonly List<ModelRenderer> modelsToRenderShadowsSimple = new List<ModelRenderer>(100);
        private static readonly List<ModelRenderer> modelsToRenderShadowsSkinned = new List<ModelRenderer>(10);
        private static readonly List<PointLight>    pointLightsToRender = new List<PointLight>(50);
        private static readonly List<SpotLight>     spotLightsToRender  = new List<SpotLight>(20);

        // G-Buffer ordered lists.
        private static readonly List<MeshPartToRender> gbufferSimple               = new List<MeshPartToRender>(50);
        private static readonly List<MeshPartToRender> gBufferWithNormalMap        = new List<MeshPartToRender>(50);
        private static readonly List<MeshPartToRender> gBufferWithParallax         = new List<MeshPartToRender>(50);
        private static readonly List<MeshPartToRender> gBufferSkinnedSimple        = new List<MeshPartToRender>(50);
        private static readonly List<MeshPartToRender> gBufferSkinnedWithNormalMap = new List<MeshPartToRender>(50);
        private static readonly List<MeshPartToRender> transparentObjects          = new List<MeshPartToRender>(50);
        private static readonly List<MeshPartToRender> transparentSkinnedObjects   = new List<MeshPartToRender>(5);

        // Shadows
        private static DirectionalLightDepthInformation[] activeDirectionalLightShadows = new DirectionalLightDepthInformation[10];
        private static SpotLightDepthInformation[] activeSpotLightShadows = new SpotLightDepthInformation[10];
        private static PointLightDepthInformation[] activePointLightShadows = new PointLightDepthInformation[5];
        private static List<RenderTargetCube> lightDepthTextureCubeArray = new List<RenderTargetCube>(20) ;
        private static int activeCamerasCount;
        
        #endregion

        #region Properties

        /// <summary>
        /// This indicates the cameras to render and their order, ignoring the camera component settings.
        /// The camera component provides an interface to set this information. 
        /// The editor and maybe some user testing will benefit with this functionality.
        /// But do not use it for the final game.
        /// </summary>
        public static List<Camera> CamerasToRender { get; set; }
        
        /// <summary>
        /// You can avoid the rendering of the main camera to the back buffer.
        /// </summary>
        public static bool RenderMainCameraToScreen { get; set; }

        #endregion

        #region Load Content

        /// <summary>
        /// Called when the application starts and when the device is disposed. 
        /// </summary>
        /// <remarks> 
        /// In application start up this method is called before Begin Run.
        /// </remarks>
        internal static void LoadContent()
        {
            RenderMainCameraToScreen = true;

            // Initialize managers that are related to the device.
            SpriteManager.Initialize();
            LineManager.Initialize();
            SoundManager.Initialize();
            
            // Recreate assets.
            AssetContentManager.RecreateContentManagers();

            // Call the DeviceDisposed method only when the the device was disposed.
            foreach (Scene scene in Scene.CurrentScenes)
            {
                if (scene.ContentLoaded)
                    scene.DeviceDisposedFromGameLoop();
            }

            // Collect all garbage.
            // Garbage collections are performed in XBOX 360 between 1 Mb of created data.
            // Collecting the garbage gives a little more room to have a little garbage periodically.
            GarbageCollector.CollectGarbage();
        } // LoadContent

        #endregion

        #region Begin Run

        /// <summary>
        /// Called when the application starts. 
        /// </summary>
        /// <remarks> 
        /// In application start up this method is called after Load Content.
        /// </remarks>
        internal static void BeginRun()
        {
            // Initialize managers that are not related to the device.
            InputManager.Initialize();
            MusicManager.Initialize();
            // Initialize the physics simulation for our scene
            PhysicsManager.Initialize();
            
            // Begin run the scene.
            foreach (Scene scene in Scene.CurrentScenes)
            {
                scene.Initialize();
                scene.BeginRunFromGameLoop();
            }
            
            // Start scripts.
            for (int i = 0; i < Script.ScriptList.Count; i++) // The for sentence is needed because the script list could be modified by some script.
            {
                var script = Script.ScriptList[i];
                if (script.assignedToAGameObject && script.IsActive)
                {
                    if (!script.Started)
                    {
                        script.Start();
                        script.Started = true;
                    }
                }
            }

            // Init statistics counting.
            Statistics.InitStatistics();

            // Try to recover memory when the screen size changes.
            // Render Targets relative to the old screen resolution are not longer need it.
            Screen.ScreenSizeChanged += delegate
            {
                RenderTarget.ClearRenderTargetPool();
                RenderTarget.ClearMultpleRenderTargetPool();
            };

            // Collect all garbage.
            // Garbage collections are performed in XBOX 360 between 1 Mb of created data.
            // Collecting the garbage gives a little more room to have a little garbage periodically.
            GarbageCollector.CollectGarbage();

        } // BeginRun

        #endregion

        #region Update

        /// <summary>
        /// Called when the game has determined that game logic needs to be processed.
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

            #region Physics

            // Run the simulation
            PhysicsManager.Scene.Update(Time.GameDeltaTime);

            // Update physics components, i.e. update the game object's transform
            for (int i = 0; i < RigidBody.ComponentPool.Count; i++)
            {
                if (RigidBody.ComponentPool.Elements[i].IsActive)
                    RigidBody.ComponentPool.Elements[i].Update();
            }

            #endregion

            #region Special Keys

            if (Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.PrintScreen))
            {
                ScreenshotCapturer.MakeScreenshot = true;
            }
            if ((Keyboard.KeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftAlt) ||
                 Keyboard.KeyPressed(Microsoft.Xna.Framework.Input.Keys.RightAlt)) &&
                 Keyboard.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                Screen.ToggleFullscreen();
            }

            #endregion

            #region Root Animation Processing

            for (int i = 0; i < RootAnimation.ComponentPool.Count; i++)
            {
                RootAnimation component = RootAnimation.ComponentPool.Elements[i];
                if (component.IsActive)
                    component.Update();
            }

            #endregion

            #region Model Animation Processing

            // Update the individual active model animation players.
            AnimationManager.UpdateModelAnimationPlayers();

            // Compose the active model animations.
            for (int i = 0; i < ModelAnimations.ComponentPool.Count; i++)
            {
                ModelAnimations component = ModelAnimations.ComponentPool.Elements[i];
                if (component.IsActive)
                    component.Update();
            }

            // The global pose (world space) is generated.
            // However, if no post processing exist (IK, ragdolls, etc.) this stage could be merge with
            // the inverse bind pose multiplication stage in the mesh draw code. And for now the engine will do this.
            // TODO!!

            // The animation players of the individual animations that were finished in the model animation player update,
            // the individual animations that were discarded by the compose operation and the individual animations that were stopped
            // are release to be used by other future individual animations.
            AnimationManager.ReleaseUnusedAnimationPlayers();
            
            #endregion
            
            #region Logic Update
            
            // Update the scene
            for (int i = 0; i < Scene.CurrentScenes.Count; i++)
            {
                Scene scene = Scene.CurrentScenes[i];
                if (!scene.ContentLoaded)
                    scene.Initialize();
                scene.UpdateTasksFromGameLoop();
            }
            // Update the scripts
            for (int i = 0; i < Script.ScriptList.Count; i++) // The for sentence is needed because the script list could be modified by some script.
            {
                var script = Script.ScriptList[i];
                if (script.assignedToAGameObject && script.IsActive)
                {
                    if (!script.Started)
                    {
                        script.Start();
                        script.Started = true;
                    }
                    script.Update();
                }
            }
            // Perform the late update of the scene.
            for (int i = 0; i < Scene.CurrentScenes.Count; i++)
            {
                Scene scene = Scene.CurrentScenes[i];
                if (scene.ContentLoaded)
                    scene.LateUpdateTasksFromGameLoop();
            }
            // Perform the late update of the scripts.
            for (int i = 0; i < Script.ScriptList.Count; i++) // The for sentence is needed because the script list could be modified by some script.
            {
                var script = Script.ScriptList[i];
                if (script.assignedToAGameObject && script.IsActive)
                {
                    if (!script.Started)
                    {
                        script.Start();
                        script.Started = true;
                    }
                    script.LateUpdate();
                }
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
                if (SoundListener.ComponentPool.Elements[i].IsActive)
                    audioListenerCount++;
            }
            if (audioListenerCount > 4)
                throw new InvalidOperationException("Sound Manager: The maximum number of active audio listener is 4");
            // Update and put into a list.
            int arrayindex = 0;
            for (int i = 0; i < SoundListener.ComponentPool.Count; i++)
            {
                SoundListener component = SoundListener.ComponentPool.Elements[i];
                if (component.IsActive)
                {
                    component.UpdateListenerProperties();
                    if (audioListenerCount == 1)
                        oneAudioListener = component.audioListener;
                    else if (audioListenerCount == 2)
                        twoAudioListener[arrayindex] = component.audioListener;
                    else if (audioListenerCount == 3)
                        threeAudioListener[arrayindex] = component.audioListener;
                    else if (audioListenerCount == 4)
                        fourAudioListener[arrayindex] = component.audioListener;
                    arrayindex++;
                }
            }

            #endregion
            
            #region Emitters
            
            // Update sound emitters.
            if (audioListenerCount <= 1)
                for (int i = 0; i < SoundEmitter.ComponentPool.Count; i++)
                {
                    if (SoundEmitter.ComponentPool.Elements[i].IsActive)
                        SoundEmitter.ComponentPool.Elements[i].Update(oneAudioListener);   
                }
            else
            {
                AudioListener[] audioListeners;
                if (audioListenerCount == 2)
                    audioListeners = twoAudioListener;
                else if (audioListenerCount == 3)   
                    audioListeners = threeAudioListener;
                else
                    audioListeners = fourAudioListener;
                for (int i = 0; i < SoundEmitter.ComponentPool.Count; i++)
                {
                    if (SoundEmitter.ComponentPool.Elements[i].IsActive)
                        SoundEmitter.ComponentPool.Elements[i].Update(audioListeners);   
                }
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
            // Reset Frame Statistics
            Statistics.ReserFrameStatistics();
            // Update the chronometers that work in frame time space.
            Chronometer.UpdateFrameTimeChronometers();

            #region Logic Update

            // Perform the pre draw update of the scene and scripts.
            for (int i = 0; i < Scene.CurrentScenes.Count; i++)
            {
                Scene scene = Scene.CurrentScenes[i];
                if (scene.ContentLoaded)
                    scene.PreRenderTasksFromGameLoop();
            }
            for (int i = 0; i < Script.ScriptList.Count; i++) // The for sentence is needed because the script list could be modified by some script.
            {
                var script = Script.ScriptList[i];
                if (script.assignedToAGameObject && script.IsActive)
                {
                    if (!script.Started)
                    {
                        script.Start();
                        script.Started = true;
                    }
                    script.PreRenderUpdate();
                }
            }

            #endregion 
            
            #region Particles Emitters

            // Update particle emitters.
            for (int i = 0; i < ParticleEmitter.ComponentPool.Count; i++)
            {
                if (ParticleEmitter.ComponentPool.Elements[i].IsActive)
                    ParticleEmitter.ComponentPool.Elements[i].Update();
            }

            #endregion

            Camera mainCamera;

            #region Set shadows To Not calculated This Frame

            // Used to avoid calculating the same shadow map in split screen mode.
            for (int i = 0; i < activeSpotLightShadows.Length; i++)
            {
                activeSpotLightShadows[i].calculatedThisFrame = false;
            }
            // Used to avoid calculating the same shadow map in split screen mode.
            for (int i = 0; i < activePointLightShadows.Length; i++)
            {
                activePointLightShadows[i].calculatedThisFrame = false;
            }

            #endregion

            if (CamerasToRender == null || CamerasToRender.Count == 0)
            {

                #region Free Innecesary Shadow Resources

                if (Shadow.DistributeShadowCalculationsBetweenFrames)
                {
                    // Search each camera
                    for (int cameraIndex = 0; cameraIndex < Camera.ComponentPool.Count; cameraIndex++)
                    {
                        Camera currentCamera = Camera.ComponentPool.Elements[cameraIndex];
                        // If the camera is not longer active then we need to remove it.
                        if (!currentCamera.IsVisible)
                        {
                            // We search each active shadows members and we try to release resources.
                            for (int i = 0; i < activeDirectionalLightShadows.Length; i++)
                            {
                                if (activeDirectionalLightShadows[i].camera == currentCamera)
                                {
                                    activeDirectionalLightShadows[i].camera = null;
                                    activeDirectionalLightShadows[i].light = null;
                                    RenderTarget.Release(activeDirectionalLightShadows[i].lightDepthTexture);
                                    activeDirectionalLightShadows[i].lightDepthTexture = null;
                                }
                                else if (activeDirectionalLightShadows[i].camera == null)
                                    break;
                            }
                        }
                    }
                }

                #endregion

                #region Track Active Cameras

                // This is useful to liberate resource in non split-screen render.
                activeCamerasCount = 0;
                for (int cameraIndex = 0; cameraIndex < Camera.ComponentPool.Count; cameraIndex++)
                {
                    Camera currentCamera = Camera.ComponentPool.Elements[cameraIndex];
                    // Only active master cameras are renderer.
                    if (currentCamera.MasterCamera == null && currentCamera.IsVisible)
                        activeCamerasCount++;
                }

                #endregion

                #region Render Each Camera

                // For each camera we render the scene in it
                for (int cameraIndex = 0; cameraIndex < Camera.ComponentPool.Count; cameraIndex++)
                {
                    Camera currentCamera = Camera.ComponentPool.Elements[cameraIndex];
                    // Only active master cameras are renderer.
                    if (currentCamera.MasterCamera == null && currentCamera.IsVisible)
                        RenderMasterCamera(currentCamera);
                }
            
                #endregion

                mainCamera = Camera.MainCamera;
            }
            else // You can manually set what cameras want to render. This is mostly for the editor. In this case...
            {

                #region Free Innecesary Shadow Resources

                if (Shadow.DistributeShadowCalculationsBetweenFrames)
                {
                    // Search each camera
                    for (int cameraIndex = 0; cameraIndex < Camera.ComponentPool.Count; cameraIndex++)
                    {
                        Camera currentCamera = Camera.ComponentPool.Elements[cameraIndex];
                        // If the camera won't be rendered...
                        if (!CamerasToRender.Contains(currentCamera))
                        {
                            // We search each active shadows members and we try to release resources.
                            for (int i = 0; i < activeDirectionalLightShadows.Length; i++)
                            {
                                if (activeDirectionalLightShadows[i].camera == currentCamera)
                                {
                                    activeDirectionalLightShadows[i].camera = null;
                                    activeDirectionalLightShadows[i].light = null;
                                    RenderTarget.Release(activeDirectionalLightShadows[i].lightDepthTexture);
                                    activeDirectionalLightShadows[i].lightDepthTexture = null;
                                }
                                else if (activeDirectionalLightShadows[i].camera == null)
                                    break;
                            }
                        }
                    }
                }

                #endregion
                
                #region Track Active Cameras

                // This is useful to liberate resource in non split-screen render.
                activeCamerasCount = 0;
                for (int cameraIndex = 0; cameraIndex < CamerasToRender.Count; cameraIndex++)
                {
                    Camera currentCamera = CamerasToRender[cameraIndex];
                    // Only active master cameras are renderer.
                    if (currentCamera.MasterCamera == null && currentCamera.IsVisible)
                        activeCamerasCount++;
                }

                #endregion

                #region Render CamerasToRender Cameras

                // For each camera we render the scene in it
                for (int cameraIndex = 0; cameraIndex < CamerasToRender.Count; cameraIndex++)
                {
                    Camera currentCamera = CamerasToRender[cameraIndex];
                    // Only active master cameras are renderer.
                    if (currentCamera.MasterCamera == null && currentCamera.IsVisible)
                        RenderMasterCamera(currentCamera);
                }

                #endregion

                mainCamera = CamerasToRender[CamerasToRender.Count - 1];
            }

            #region Release Spot Shadows Resources
            
            // If memory constrains are too high, then it is possible to improve this mechanism.
            // A spot and point shadow render target pool could be implemented.
            // The frustum culling of spot and point lights could be performed before rendering the cameras and thus we can liberate shadows resources more eficiently.
            for (int i = 0; i < activeSpotLightShadows.Length; i++)
            {
                if (!activeSpotLightShadows[i].calculatedThisFrame)
                {
                    activeSpotLightShadows[i].light = null;
                    RenderTarget.Release(activeSpotLightShadows[i].lightDepthTexture);
                    activeSpotLightShadows[i].lightDepthTexture = null;
                }
            }
            for (int i = 0; i < activePointLightShadows.Length; i++)
            {
                if (!activePointLightShadows[i].calculatedThisFrame || (!Shadow.DistributeShadowCalculationsBetweenFrames && activeCamerasCount == 1))
                {
                    activePointLightShadows[i].light = null;
                    RenderTargetCube.Release(activePointLightShadows[i].lightDepthTexture);
                    activePointLightShadows[i].lightDepthTexture = null;
                }
            }

            #endregion

            #region Screenshot Preparations

            RenderTarget screenshotRenderTarget = null;
            if (ScreenshotCapturer.MakeScreenshot)
            {
                // Instead of render into the back buffer we render into a render target.
                AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
                AssetContentManager.CurrentContentManager = AssetContentManager.SystemContentManager;
                screenshotRenderTarget = new RenderTarget(Size.FullScreen, SurfaceFormat.Color, false);
                AssetContentManager.CurrentContentManager = userContentManager;
                screenshotRenderTarget.EnableRenderTarget();
            }

            #endregion

            #region Render Main Camera to Back Buffer

            if (RenderMainCameraToScreen)
            {
                EngineManager.Device.Clear(Color.Black);
                // Render the main camera onto back buffer.
                if (mainCamera != null && mainCamera.RenderTarget != null)
                    SpriteManager.DrawTextureToFullScreen(Camera.MainCamera.RenderTarget);
                RenderHeadsUpDisplay();
            }

            #endregion

            #region Logic Update

            for (int i = 0; i < Scene.CurrentScenes.Count; i++)
            {
                Scene scene = Scene.CurrentScenes[i];
                if (scene.ContentLoaded)
                    scene.PostRenderTasksFromGameLoop();
            }
            for (int i = 0; i < Script.ScriptList.Count; i++) // The for sentence is needed because the script list could be modified by some script.
            {
                var script = Script.ScriptList[i];
                if (script.assignedToAGameObject && script.IsActive)
                {
                    if (!script.Started)
                    {
                        script.Start();
                        script.Started = true;
                    }
                    script.PostRenderUpdate();
                }
            }

            #endregion 

            #region Screenshot Saving

            if (ScreenshotCapturer.MakeScreenshot)
            {
                screenshotRenderTarget.DisableRenderTarget();
                ScreenshotCapturer.MakeScreenshot = false;
                ScreenshotCapturer.SaveScreenshot(screenshotRenderTarget);
                SpriteManager.DrawTextureToFullScreen(screenshotRenderTarget);
                screenshotRenderTarget.Dispose();
            }

            #endregion
            
        } // Draw

        #region Render Master Camera

        /// <summary>
        /// Render a master camera and its slaves. 
        /// </summary>
        private static void RenderMasterCamera(Camera currentCamera)
        {
            // If the camera does not have a render target we create one for the user.
            if (currentCamera.RenderTarget == null)
            {
                AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
                AssetContentManager.CurrentContentManager = AssetContentManager.SystemContentManager;
                currentCamera.RenderTarget = new RenderTarget(currentCamera.RenderTargetSize, SurfaceFormat.Color, DepthFormat.None);
                AssetContentManager.CurrentContentManager = userContentManager;
            }
            // If it does not have slaves cameras and it occupied the whole render target...
            if (currentCamera.slavesCameras.Count == 0 && currentCamera.NormalizedViewport == new RectangleF(0, 0, 1, 1))
                RenderCamera(currentCamera, currentCamera.RenderTarget);
            else
            {

                #region Render Cameras

                // Render each camera to a render target and then merge.
                currentCamera.PartialRenderTarget = RenderTarget.Fetch(CalculatePartialRenderTargetSize(currentCamera), SurfaceFormat.Color, DepthFormat.None, 
                                                                       RenderTarget.AntialiasingType.NoAntialiasing);
                RenderCamera(currentCamera, currentCamera.PartialRenderTarget);
                foreach (Camera slaveCamera in currentCamera.slavesCameras)
                {
                    if (slaveCamera.IsActive)
                    {
                        // I store the render of the camera to a partial render target.
                        // This helps reduce the memory consumption (GBuffer, Light Pass, HDR pass)
                        // at the expense of a pass that copy this texture to a bigger render target
                        // and a last pass that copy the cameras’ render target to the back buffer.
                        // If the performance is critical and there is more memory you should change this behavior.
                        // It also simplified the render of one camera. 
                        slaveCamera.PartialRenderTarget = RenderTarget.Fetch(CalculatePartialRenderTargetSize(currentCamera), SurfaceFormat.Color, DepthFormat.None,
                                                                             RenderTarget.AntialiasingType.NoAntialiasing);
                        RenderCamera(slaveCamera, slaveCamera.PartialRenderTarget);
                    }
                }

                #endregion

                #region Composite Cameras

                // Composite cameras
                currentCamera.RenderTarget.EnableRenderTarget();
                currentCamera.RenderTarget.Clear(currentCamera.ClearColor);

                // Composite using the rendering order
                bool masterCamerawasRendered = false;
                foreach (Camera slaveCamera in currentCamera.slavesCameras)
                {
                    // If the master camera needs to be rendered.
                    if (!masterCamerawasRendered && slaveCamera.RenderingOrder > currentCamera.RenderingOrder)
                    {
                        EngineManager.Device.Viewport = new Viewport(currentCamera.Viewport.X, currentCamera.Viewport.Y, currentCamera.Viewport.Width, currentCamera.Viewport.Height);
                        SpriteManager.DrawTextureToFullScreen(currentCamera.PartialRenderTarget, true);
                        RenderTarget.Release(currentCamera.PartialRenderTarget);
                        masterCamerawasRendered = true;
                    }
                    // Render slaves cameras (they are already ordered).
                    if (slaveCamera.IsActive)
                    {
                        EngineManager.Device.Viewport = new Viewport(slaveCamera.Viewport.X, slaveCamera.Viewport.Y, slaveCamera.Viewport.Width, slaveCamera.Viewport.Height);
                        SpriteManager.DrawTextureToFullScreen(slaveCamera.PartialRenderTarget, true);
                        RenderTarget.Release(slaveCamera.PartialRenderTarget);
                    }
                }
                // If the master camera was not rendered then we do it here.
                if (!masterCamerawasRendered)
                {
                    EngineManager.Device.Viewport = new Viewport(currentCamera.Viewport.X, currentCamera.Viewport.Y, currentCamera.Viewport.Width, currentCamera.Viewport.Height);
                    SpriteManager.DrawTextureToFullScreen(currentCamera.PartialRenderTarget, true);
                    RenderTarget.Release(currentCamera.PartialRenderTarget);
                }
                currentCamera.RenderTarget.DisableRenderTarget();

                #endregion

            }
        } // RenderMainCamera

        #endregion
        
        #region Calculate Render Target Size

        /// <summary>
        /// Calculate partial render target size.
        /// </summary>
        private static Size CalculatePartialRenderTargetSize(Camera camera)
        {
            Size targetSize;
            if (camera.NeedViewport)
            {
                targetSize = new Size(camera.Viewport.Width, camera.Viewport.Height);
                targetSize.MakeRelativeIfPosible();
            }
            else
                targetSize = camera.RenderTargetSize;
            return targetSize;
        } // CalculatePartialRenderTargetSize

        #endregion

        #region Testing

        /// <summary>
        /// Renders a texture and the Head Up Display to the camera's render target and free the resources.
        /// It is only used for testing.
        /// </summary>
        private static void FinishRendering(Camera currentCamera, RenderTarget renderTarget, Assets.Texture showToScreen = null)
        {
            renderTarget.EnableRenderTarget();
            if (showToScreen != null)
                SpriteManager.DrawTextureToFullScreen(showToScreen);
            renderTarget.DisableRenderTarget();
            Layer.CurrentCameraCullingMask = uint.MaxValue;
            ReleaseUnusedRenderTargets();
        } // FinishRendering

        #endregion

        #region Render Camera
        
        /// <summary>
        /// Deferred lighting pipeline for one camera. 
        /// </summary>
        private static void RenderCamera(Camera currentCamera, RenderTarget renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");

            // Store a the camera position for the tranparent object sorting.
            cameraPosition = currentCamera.Position;

            // Calculate view space bounding frustum.
            currentCamera.BoundingFrustumViewSpace(cornersViewSpace);
            // Set camera culling mask.
            Layer.CurrentCameraCullingMask = currentCamera.CullingMask;

            #region Frustum Culling

            cameraBoundingFrustum.Matrix = currentCamera.ViewMatrix * currentCamera.ProjectionMatrix;
            modelsToRender.Clear();
            FrustumCulling.ModelRendererFrustumCulling(cameraBoundingFrustum, modelsToRender);

            // Testing
            //FinishRendering(currentCamera, renderTarget); return;

            #endregion

            #region GBuffer Pass

            // In XNA we can’t recover the GPU Z-Buffer, so we have to store it in an additional render target.
            // That means more work to do and less space to store the render targets in the Xbox's EDRAM. 
            // This could be worse if we want to store additional information, like surface’s albedo.
            // To avoid a predicated tilling we can do a Z Pre Pass, i.e. decupling the G-Buffer generation.
            // This has one big advantage and one big disadvantage. 
            // The disadvantage is that we need to render one more time the geometry but
            // (advantage) we can use this pre-rendering as an occlusion culling for the G-Buffer,
            // however, if the poly count is high, the vertex processing could be to high to handle.

            GBufferPass.Begin(renderTarget.Size);
            GBufferShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane);

            // Batching is everything, in both CPU (data oriented programming) and GPU (less preparation).
            // The problem is that I do not want to know about materials in the G-Buffer, but I can’t avoid it completely.
            // Therefore I have all the G-Buffer possible accessed data as common properties of all materials.
            // That means if I add new materials in the engine, then there is no need to change the G-Buffer code.

            #region  Sorting

            // There are five different techniques in the G-Buffer shader, therefore there are five lists.
            // A better sorting could arrange also by texture or by other properties. 
            // This sorting could help sort the HDR pass faster.
            gBufferSkinnedSimple.Clear();
            gBufferSkinnedWithNormalMap.Clear();
            gBufferWithParallax.Clear();
            gbufferSimple.Clear();
            gBufferWithNormalMap.Clear();
            // These two are to avoid sorting transparent objects.
            transparentObjects.Clear();
            transparentSkinnedObjects.Clear();

            // Sort by G-Buffer techniques.
            // It is possible to this using multithreading.
            foreach (ModelRenderer modelRenderer in modelsToRender)
            {
                int currentMeshPart = 0;
                // Each mesh is sorted individually.
                for (int mesh = 0; mesh < modelRenderer.CachedModel.MeshesCount; mesh++)
                {
                    int meshPartsCount = modelRenderer.CachedModel.MeshPartsCountPerMesh[mesh];
                    // Each mesh part is sorted individiually.
                    for (int meshPart = 0; meshPart < meshPartsCount; meshPart++)
                    {
                        // Find material (the mesh part could have a custom material or use the model material)
                        Material material = null;
                        if (modelRenderer.MeshMaterials != null && currentMeshPart < modelRenderer.MeshMaterials.Length && modelRenderer.MeshMaterials[currentMeshPart] != null)
                            material = modelRenderer.MeshMaterials[currentMeshPart];
                        else if (modelRenderer.Material != null)
                            material = modelRenderer.Material;
                        // Once the material is felt then the classification begins.
                        if (material != null)
                        {
                            // If it is a skinned model.
                            if (modelRenderer.CachedModel.IsSkinned && modelRenderer.cachedBoneTransforms != null)
                            {
                                MeshPartToRender meshPartToRender = new MeshPartToRender
                                {
                                    WorldMatrix = modelRenderer.CachedWorldMatrix,
                                    Model = modelRenderer.CachedModel,
                                    BoneTransform = modelRenderer.cachedBoneTransforms,
                                    Material = material,
                                    MeshIndex = mesh,
                                    MeshPart = meshPart,
                                };
                                if (material.AlphaBlending == 1) // Only opaque models are rendered on the G-Buffer.
                                {
                                    if (material.NormalTexture == null)
                                        gBufferSkinnedSimple.Add(meshPartToRender);
                                    else
                                        gBufferSkinnedWithNormalMap.Add(meshPartToRender);
                                }
                                else
                                    transparentSkinnedObjects.Add(meshPartToRender);
                            }
                            else
                            {
                                MeshPartToRender meshPartToRender = new MeshPartToRender
                                {
                                    Model = modelRenderer.CachedModel,
                                    BoneTransform = null,
                                    Material = material,
                                    MeshIndex = mesh,
                                    MeshPart = meshPart,
                                };
                                if (modelRenderer.cachedBoneTransforms != null)
                                    meshPartToRender.WorldMatrix = modelRenderer.cachedBoneTransforms[mesh + 1] * modelRenderer.CachedWorldMatrix;
                                else
                                    meshPartToRender.WorldMatrix = modelRenderer.CachedWorldMatrix;
                                if (material.AlphaBlending == 1) // Only opaque models are rendered on the G-Buffer.
                                {
                                    if (material.NormalTexture == null)
                                        gbufferSimple.Add(meshPartToRender);
                                    else if (material.ParallaxEnabled)
                                        gBufferWithParallax.Add(meshPartToRender);
                                    else
                                        gBufferWithNormalMap.Add(meshPartToRender);
                                }
                                else
                                {
                                    transparentObjects.Add(meshPartToRender);
                                }
                            }
                        }
                        currentMeshPart++;
                    }
                }
            }

            #endregion
            
            #region Render

            // Render with batchs (each list is a G-Buffer technique)
            for (int i = 0; i < gbufferSimple.Count; i++)
            {
                MeshPartToRender meshPartToRender = gbufferSimple[i];
                GBufferShader.Instance.RenderModelSimple(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                         meshPartToRender.Material,
                                                         meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferWithNormalMap.Count; i++)
            {
                MeshPartToRender meshPartToRender = gBufferWithNormalMap[i];
                GBufferShader.Instance.RenderModelWithNormals(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                              meshPartToRender.Material,
                                                              meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferWithParallax.Count; i++)
            {
                MeshPartToRender meshPartToRender = gBufferWithParallax[i];
                GBufferShader.Instance.RenderModelWithParallax(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                               meshPartToRender.Material,
                                                               meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferSkinnedSimple.Count; i++)
            {
                MeshPartToRender meshPartToRender = gBufferSkinnedSimple[i];
                GBufferShader.Instance.RenderModelSkinnedSimple(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                                meshPartToRender.Material,
                                                                meshPartToRender.MeshIndex, meshPartToRender.MeshPart,
                                                                meshPartToRender.BoneTransform);
            }
            for (int i = 0; i < gBufferSkinnedWithNormalMap.Count; i++)
            {
                MeshPartToRender meshPartToRender = gBufferSkinnedWithNormalMap[i];
                GBufferShader.Instance.RenderModelSkinnedWithNormals(ref meshPartToRender.WorldMatrix,
                                                                     meshPartToRender.Model, meshPartToRender.Material,
                                                                     meshPartToRender.MeshIndex,
                                                                     meshPartToRender.MeshPart,
                                                                     meshPartToRender.BoneTransform);
            }

            #endregion
            
            gbufferTextures = GBufferPass.End();
            
            // Downsample GBuffer
            gbufferHalfTextures = DownsamplerGBufferShader.Instance.Render(gbufferTextures.RenderTargets[0], gbufferTextures.RenderTargets[1]);
            gbufferQuarterTextures = DownsamplerGBufferShader.Instance.Render(gbufferHalfTextures.RenderTargets[0], gbufferHalfTextures.RenderTargets[1]);

            // Testing
            //FinishRendering(currentCamera, renderTarget, gbufferTextures.RenderTargets[1]); return;
            
            #endregion

            #region Light Pre Pass
            
            // Frustum Culling
            pointLightsToRender.Clear();
            spotLightsToRender.Clear();
            cameraBoundingFrustum.Matrix = currentCamera.ViewMatrix * currentCamera.ProjectionMatrix;
            // They are multithreading candidates.
            FrustumCulling.PointLightFrustumCulling(cameraBoundingFrustum, pointLightsToRender);
            FrustumCulling.SpotLightFrustumCulling(cameraBoundingFrustum, spotLightsToRender);
            
            #region Ambient Occlusion
            
            // If the ambient occlusion pass is requested...
            if (currentCamera.AmbientLight != null && currentCamera.AmbientLight.Intensity > 0 &&
                currentCamera.AmbientLight.AmbientOcclusion != null && currentCamera.AmbientLight.AmbientOcclusion.Enabled)
            {
                RenderTarget aoDepthTexture, aoNormalTexture;
                // Select downsampled version or full version of the gbuffer textures.
                if (currentCamera.AmbientLight.AmbientOcclusion.TextureSize == Size.TextureSize.FullSize)
                {
                    aoDepthTexture = gbufferTextures.RenderTargets[0];
                    aoNormalTexture = gbufferTextures.RenderTargets[1];
                }
                else if (currentCamera.AmbientLight.AmbientOcclusion.TextureSize == Size.TextureSize.HalfSize)
                {
                    aoDepthTexture = gbufferHalfTextures.RenderTargets[0];
                    aoNormalTexture = gbufferHalfTextures.RenderTargets[1];
                }
                else
                {
                    aoDepthTexture = gbufferQuarterTextures.RenderTargets[0];
                    aoNormalTexture = gbufferQuarterTextures.RenderTargets[1];
                }
                // Now the occlusion texture is generated. The result will be used in the light pass with the ambient light.
                if (currentCamera.AmbientLight.AmbientOcclusion is HorizonBasedAmbientOcclusion)
                {
                    ambientOcclusionTexture = HorizonBasedAmbientOcclusionShader.Instance.Render(aoDepthTexture,
                                                                                                 aoNormalTexture,
                                                                                                 (HorizonBasedAmbientOcclusion)currentCamera.AmbientLight.AmbientOcclusion,
                                                                                                 currentCamera.FieldOfView, renderTarget.Size,
                                                                                                 gbufferTextures.RenderTargets[0]);
                }
                if (currentCamera.AmbientLight.AmbientOcclusion is RayMarchingAmbientOcclusion)
                {
                    ambientOcclusionTexture = RayMarchingAmbientOcclusionShader.Instance.Render(aoDepthTexture,
                                                                                                aoNormalTexture,
                                                                                                (RayMarchingAmbientOcclusion)currentCamera.AmbientLight.AmbientOcclusion,
                                                                                                currentCamera.FieldOfView);
                }
                // Testing
                //FinishRendering(currentCamera, renderTarget, ambientOcclusionTexture); RenderTarget.Release(ambientOcclusionTexture); return;
            }
            
            #endregion
            
            #region Shadow Maps

            // Big TODO!!
            // There are several techniques that could be added. 
            // * Black Rock Studios’ shadow edge detection (Rendering Techniques in Split Second). 
            // * Spot shadow results grouped in one texture and filtered in just one pass (actually two).
            // * A better shadow resource management that includes a pool of shadow textures and
            //   a quick release of unnecessary render targets before the cameras' processing stage.
            // * A better transition between cascades in the cascaded shadows.
            // * Killzone 2 method in cascaded shadows to avoid rasterization artifacts when the camera rotates.

            // Set Render States.
            EngineManager.Device.BlendState = BlendState.Opaque;
            EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineManager.Device.DepthStencilState = DepthStencilState.Default;

            #region Free Innecesary Shadow Resources

            for (int i = 0; i < activeDirectionalLightShadows.Length; i++)
            {
                if (activeDirectionalLightShadows[i].camera == currentCamera)
                {
                    if (activeDirectionalLightShadows[i].light != DirectionalLight.Sun)
                    {
                        activeDirectionalLightShadows[i].camera = null;
                        activeDirectionalLightShadows[i].light = null;
                        RenderTarget.Release(activeDirectionalLightShadows[i].lightDepthTexture);
                        activeDirectionalLightShadows[i].lightDepthTexture = null;
                    }
                }
                else if (activeDirectionalLightShadows[i].camera == null)
                    break;
            }

            // If the light was removed and its data reused for another new light then the system will show an incorrect shadow map for one frame.
            // But it's better to avoid wasting resources in detecting this case because this it is very strange and it an error that occur for just one frame.

            #endregion

            #region Directional Light Shadows

            // Only one directional light, the sun, could generate shadows. This is done to arrange the data better.
            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                DirectionalLight directionalLight = DirectionalLight.ComponentPool.Elements[i];
                // If there is a shadow map and the light is visible.
                if (directionalLight == DirectionalLight.Sun && directionalLight.Shadow != null && directionalLight.Shadow.Enabled && directionalLight.IsVisible && directionalLight.Intensity > 0)
                {

                    #region Search Correct Depth Texture

                    // Select downsampled version or full version of the gbuffer textures.
                    RenderTarget depthTexture;
                    if (directionalLight.Shadow.ShadowTextureSize == Size.TextureSize.FullSize)
                        depthTexture = gbufferTextures.RenderTargets[0];
                    else if (directionalLight.Shadow.ShadowTextureSize == Size.TextureSize.HalfSize)
                        depthTexture = gbufferHalfTextures.RenderTargets[0];
                    else
                        depthTexture = gbufferQuarterTextures.RenderTargets[0];

                    #endregion

                    #region Cascaded Shadow

                    if (directionalLight.Shadow is CascadedShadow)
                    {
                        CascadedShadow shadow = (CascadedShadow)directionalLight.Shadow;
                        
                        // The texture that contains the visibility information (depth map) from the light point of view.
                        RenderTarget lightDepthTexture = null;
                        
                        // If the shadows are updated one frame but not the other
                        // then the light depth texture and some other information related to the frame when it was generated is stored.
                        // We try to find this information here.
                        int shadowIndex = -1;
                        if (Shadow.DistributeShadowCalculationsBetweenFrames)
                        {
                            shadowIndex = GetIndexActiveDirectionalLightShadows(currentCamera, directionalLight);
                            // If there is in the array the light depth texture is used.
                            if (shadowIndex != -1)
                                lightDepthTexture = activeDirectionalLightShadows[shadowIndex].lightDepthTexture;
                            // If there was destroyed because the screen was resized or something similar...
                            if (lightDepthTexture != null && lightDepthTexture.IsDisposed)
                                lightDepthTexture = null;
                        }

                        // The light depth texture is created or updated only if...
                        if (Shadow.DistributeShadowCalculationsBetweenFrames == false || Time.TotalFramesCount % 2 != 0 || lightDepthTexture == null)
                        {

                            #region Generate Light Depth Texture

                            // Determines the size of the frustum needed to cover the viewable area, then creates the light view matrix and an appropriate orthographic projection.
                            CascadedShadowMapShader.Instance.SetLight(directionalLight.cachedDirection, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix,
                                                                      currentCamera.NearPlane, currentCamera.FarPlane, cornersViewSpace,
                                                                      shadow.FarPlaneSplit1, shadow.FarPlaneSplit2, shadow.FarPlaneSplit3, shadow.FarPlaneSplit4);
                            
                            // Feth light depth texture and enable it for render.
                            if (lightDepthTexture == null)
                                LightDepthBufferShader.Instance.Begin(new Size(shadow.LightDepthTextureSize.Width * CascadedShadowMapShader.NumberSplits, shadow.LightDepthTextureSize.Height));
                            else
                                LightDepthBufferShader.Instance.Begin(lightDepthTexture);

                            // A shadow map is generated for each split or frustum.
                            for (int splitNumber = 0; splitNumber < CascadedShadowMapShader.NumberSplits; splitNumber++)
                            {
                                LightDepthBufferShader.Instance.SetLightMatrices(CascadedShadowMapShader.Instance.LightViewMatrix[splitNumber],
                                                                                 CascadedShadowMapShader.Instance.LightProjectionMatrix[splitNumber]);
                                // The complete shadow map is stored as atlas.
                                LightDepthBufferShader.Instance.SetViewport(splitNumber);

                                // Frustum Culling
                                cameraBoundingFrustum.Matrix = CascadedShadowMapShader.Instance.LightViewMatrix[splitNumber] * CascadedShadowMapShader.Instance.LightProjectionMatrix[splitNumber];
                                modelsToRenderShadows.Clear();
                                FrustumCulling.ModelRendererFrustumCulling(cameraBoundingFrustum, modelsToRenderShadows);

                                // Sort by skinned and not skinned
                                modelsToRenderShadowsSimple.Clear();
                                modelsToRenderShadowsSkinned.Clear();
                                foreach (ModelRenderer modelRenderer in modelsToRenderShadows)
                                {
                                    if (modelRenderer.CastShadows && (modelRenderer.Material == null || modelRenderer.Material.AlphaBlending == 1))
                                    {
                                        if (modelRenderer.CachedModel.IsSkinned)
                                            modelsToRenderShadowsSkinned.Add(modelRenderer);
                                        else
                                            modelsToRenderShadowsSimple.Add(modelRenderer);
                                    }
                                }
                                // Render simple objects.
                                foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSimple)
                                {
                                    LightDepthBufferShader.Instance.RenderModel(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                                }
                                // Render skinned objects.
                                foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSkinned)
                                {
                                    LightDepthBufferShader.Instance.RenderModel(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                                }
                            }
                            // Resolve and return the render target with the depth information from the light point of view.
                            lightDepthTexture = LightDepthBufferShader.Instance.End();

                            #endregion

                            #region Update Active Shadows Array

                            // If the information needs to be stored in the array...
                            if (Shadow.DistributeShadowCalculationsBetweenFrames)
                            {
                                // and the entry was not created...
                                if (shadowIndex == -1)
                                {
                                    // We create the entry...
                                    shadowIndex = GetFreeIndexActiveDirectionalLightShadows();
                                    activeDirectionalLightShadows[shadowIndex].camera = currentCamera;
                                    activeDirectionalLightShadows[shadowIndex].light = directionalLight;
                                }
                                // And update the values.
                                activeDirectionalLightShadows[shadowIndex].lightViewMatrix = CascadedShadowMapShader.Instance.LightViewMatrix[0];
                                activeDirectionalLightShadows[shadowIndex].lightViewMatrix2 = CascadedShadowMapShader.Instance.LightViewMatrix[1];
                                activeDirectionalLightShadows[shadowIndex].lightViewMatrix3 = CascadedShadowMapShader.Instance.LightViewMatrix[2];
                                activeDirectionalLightShadows[shadowIndex].lightViewMatrix4 = CascadedShadowMapShader.Instance.LightViewMatrix[3];
                                activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix = CascadedShadowMapShader.Instance.LightProjectionMatrix[0];
                                activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix2 = CascadedShadowMapShader.Instance.LightProjectionMatrix[1];
                                activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix3 = CascadedShadowMapShader.Instance.LightProjectionMatrix[2];
                                activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix4 = CascadedShadowMapShader.Instance.LightProjectionMatrix[3];
                                activeDirectionalLightShadows[shadowIndex].lightDepthTexture = lightDepthTexture;
                            }

                            #endregion

                        }
                        else // When the light depth textures is not updated it is need to set some global variables to the shader so that it can perform the work accordingly.
                        {
                            // View matrix information has to be updated to avoid an incorrect transformation.
                            CascadedShadowMapShader.Instance.SetLight(currentCamera.ViewMatrix, 
                                                                      cornersViewSpace,
                                                                      activeDirectionalLightShadows[shadowIndex].lightViewMatrix,
                                                                      activeDirectionalLightShadows[shadowIndex].lightViewMatrix2,
                                                                      activeDirectionalLightShadows[shadowIndex].lightViewMatrix3,
                                                                      activeDirectionalLightShadows[shadowIndex].lightViewMatrix4,
                                                                      activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix,
                                                                      activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix2,
                                                                      activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix3,
                                                                      activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix4);
                        }
                        
                        // Calculate a deferred shadow map.
                        // I use a downsampled depth texture to improve performance.
                        directionalLight.ShadowTexture = CascadedShadowMapShader.Instance.Render(lightDepthTexture, depthTexture, shadow.DepthBias, shadow.Filter);
                        
                        // If the depth light texture is not longer needed then we can released.
                        if (Shadow.DistributeShadowCalculationsBetweenFrames == false)
                            RenderTarget.Release(lightDepthTexture);
                    }

                    #endregion
                        
                    #region Basic Shadow
                        
                    else if (directionalLight.Shadow is BasicShadow)
                    {
                        BasicShadow shadow = (BasicShadow)directionalLight.Shadow;

                        // The texture that contains the visibility information (depth map) from the light point of view.
                        RenderTarget lightDepthTexture = null;
                        
                        // If the shadows are updated one frame but not the other
                        // then the light depth texture and some other information related to the frame when it was generated is stored.
                        // We try to find this information here.
                        int shadowIndex = -1;
                        if (Shadow.DistributeShadowCalculationsBetweenFrames)
                        {
                            shadowIndex = GetIndexActiveDirectionalLightShadows(currentCamera, directionalLight);
                            // If there is in the array the light depth texture is used.
                            if (shadowIndex != -1)
                                lightDepthTexture = activeDirectionalLightShadows[shadowIndex].lightDepthTexture;
                            // If there was destroyed because the screen was resized or something similar...
                            if (lightDepthTexture != null && lightDepthTexture.IsDisposed)
                                lightDepthTexture = null;
                        }

                        if (Shadow.DistributeShadowCalculationsBetweenFrames == false || Time.TotalFramesCount % 2 != 0 || lightDepthTexture == null)
                        {

                            #region Generate Light Depth Texture

                            BasicShadowMapShader.Instance.SetLight(directionalLight.cachedDirection, currentCamera.ViewMatrix, shadow.Range, cornersViewSpace);
                            
                            // Feth shadow texture and enable it for render.
                            if (lightDepthTexture == null)
                                LightDepthBufferShader.Instance.Begin(shadow.LightDepthTextureSize);
                            else
                                LightDepthBufferShader.Instance.Begin(lightDepthTexture);

                            LightDepthBufferShader.Instance.SetLightMatrices(BasicShadowMapShader.Instance.LightViewMatrix, BasicShadowMapShader.Instance.LightProjectionMatrix);

                            // Frustum Culling
                            cameraBoundingFrustum.Matrix = BasicShadowMapShader.Instance.LightViewMatrix * BasicShadowMapShader.Instance.LightProjectionMatrix;
                            modelsToRenderShadows.Clear();
                            FrustumCulling.ModelRendererFrustumCulling(cameraBoundingFrustum, modelsToRenderShadows);

                            // Sort by skinned and not skinned
                            modelsToRenderShadowsSimple.Clear();
                            modelsToRenderShadowsSkinned.Clear();
                            foreach (ModelRenderer modelRenderer in modelsToRenderShadows)
                            {
                                if (modelRenderer.CastShadows && (modelRenderer.Material == null || modelRenderer.Material.AlphaBlending == 1))
                                {
                                    if (modelRenderer.CachedModel.IsSkinned)
                                        modelsToRenderShadowsSkinned.Add(modelRenderer);
                                    else
                                        modelsToRenderShadowsSimple.Add(modelRenderer);
                                }
                            }
                            // Render simple objects.
                            foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSimple)
                            {
                                LightDepthBufferShader.Instance.RenderModel(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                            }
                            // Render skinned objects.
                            foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSkinned)
                            {
                                LightDepthBufferShader.Instance.RenderModel(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                            }
                            // Resolve and return the render target with the depth information from the light point of view.
                            lightDepthTexture = LightDepthBufferShader.Instance.End();

                            #endregion

                            #region Update Active Shadows Array

                            // If the information needs to be stored in the array...
                            if (Shadow.DistributeShadowCalculationsBetweenFrames)
                            {
                                // and the entry was not created...
                                if (shadowIndex == -1)
                                {
                                    // We create the entry...
                                    shadowIndex = GetFreeIndexActiveDirectionalLightShadows();
                                    activeDirectionalLightShadows[shadowIndex].camera = currentCamera;
                                    activeDirectionalLightShadows[shadowIndex].light = directionalLight;
                                }
                                // And update the values.
                                activeDirectionalLightShadows[shadowIndex].lightViewMatrix = BasicShadowMapShader.Instance.LightViewMatrix;
                                activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix = BasicShadowMapShader.Instance.LightProjectionMatrix;
                                activeDirectionalLightShadows[shadowIndex].lightDepthTexture = lightDepthTexture;
                            }

                            #endregion

                        }
                        else // When the light depth textures is not updated it is need to set some global variables to the shader so that it can perform the work accordingly.
                        {
                            // View matrix information has to be updated to avoid an incorrect transformation.
                            BasicShadowMapShader.Instance.SetLight(currentCamera.ViewMatrix,
                                                                   cornersViewSpace,
                                                                   activeDirectionalLightShadows[shadowIndex].lightViewMatrix,
                                                                   activeDirectionalLightShadows[shadowIndex].lightProjectionMatrix);
                        }
                        // Calculate a deferred shadow map.
                        directionalLight.ShadowTexture = BasicShadowMapShader.Instance.Render(lightDepthTexture, depthTexture, shadow.DepthBias, shadow.Filter);
                        // If the depth light texture is not longer needed then we can released.
                        if (Shadow.DistributeShadowCalculationsBetweenFrames == false)
                            RenderTarget.Release(lightDepthTexture);
                    }
                    
                    #endregion
                    
                }
            }

            #endregion
            
            #region Spot Light Shadows
            
            foreach (SpotLight spotLight in spotLightsToRender)
            {
                // If there is a shadow map...
                if (spotLight.Shadow != null && spotLight.Shadow.Enabled)
                {

                    #region Search Correct Depth Texture

                    // Select downsampled version or full version of the gbuffer textures.
                    RenderTarget depthTexture;
                    if (spotLight.Shadow.ShadowTextureSize == Size.TextureSize.FullSize)
                        depthTexture = gbufferTextures.RenderTargets[0];
                    else if (spotLight.Shadow.ShadowTextureSize == Size.TextureSize.HalfSize)
                        depthTexture = gbufferHalfTextures.RenderTargets[0];
                    else
                        depthTexture = gbufferQuarterTextures.RenderTargets[0];

                    #endregion

                    // If the shadow map is a cascaded shadow map...
                    if (spotLight.Shadow is BasicShadow)
                    {
                        BasicShadow shadow = (BasicShadow)spotLight.Shadow;

                        // The texture that contains the visibility information (depth map) from the light point of view.
                        RenderTarget lightDepthTexture = null;
                        bool calculatedThisFrame = false;

                        // If the shadows are updated one frame but not the other
                        // then the light depth texture and some other information related to the frame when it was generated is stored.
                        // We try to find this information here.
                        int shadowIndex = 0;
                        if (Shadow.DistributeShadowCalculationsBetweenFrames || activeCamerasCount > 1)
                        {
                            shadowIndex = GetIndexActiveSpotLightShadows(spotLight);
                            // If there is in the array the light depth texture is used.
                            if (shadowIndex != -1)
                            {
                                lightDepthTexture = activeSpotLightShadows[shadowIndex].lightDepthTexture;
                                calculatedThisFrame = activeSpotLightShadows[shadowIndex].calculatedThisFrame;
                            }

                            // If there was destroyed because the screen was resized or something similar...
                            if (lightDepthTexture != null && lightDepthTexture.IsDisposed)
                                lightDepthTexture = null;
                        }

                        if (((!Shadow.DistributeShadowCalculationsBetweenFrames || Time.TotalFramesCount % 4 != 1) && !calculatedThisFrame) || lightDepthTexture == null)
                        {

                            #region Generate Light Depth Texture
                            
                            BasicShadowMapShader.Instance.SetLight(spotLight.cachedPosition, spotLight.cachedDirection, currentCamera.ViewMatrix, spotLight.OuterConeAngle, spotLight.Range, cornersViewSpace);

                            // Feth shadow texture and enable it for render.
                            if (lightDepthTexture == null)
                                LightDepthBufferShader.Instance.Begin(shadow.LightDepthTextureSize);
                            else
                                LightDepthBufferShader.Instance.Begin(lightDepthTexture);

                            LightDepthBufferShader.Instance.SetLightMatrices(BasicShadowMapShader.Instance.LightViewMatrix, BasicShadowMapShader.Instance.LightProjectionMatrix);

                            // Frustum Culling
                            cameraBoundingFrustum.Matrix = BasicShadowMapShader.Instance.LightViewMatrix * BasicShadowMapShader.Instance.LightProjectionMatrix;
                            modelsToRenderShadows.Clear();
                            FrustumCulling.ModelRendererFrustumCulling(cameraBoundingFrustum, modelsToRenderShadows);

                            // Sort by skinned and not skinned
                            modelsToRenderShadowsSimple.Clear();
                            modelsToRenderShadowsSkinned.Clear();
                            foreach (ModelRenderer modelRenderer in modelsToRenderShadows)
                            {
                                if (modelRenderer.CastShadows && (modelRenderer.Material == null || modelRenderer.Material.AlphaBlending == 1))
                                {
                                    if (modelRenderer.CachedModel.IsSkinned)
                                        modelsToRenderShadowsSkinned.Add(modelRenderer);
                                    else
                                        modelsToRenderShadowsSimple.Add(modelRenderer);
                                }
                            }
                            // Render simple objects.
                            foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSimple)
                            {
                                LightDepthBufferShader.Instance.RenderModel(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                            }
                            // Render skinned objects.
                            foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSkinned)
                            {
                                LightDepthBufferShader.Instance.RenderModel(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                            }
                            // Resolve and return the render target with the depth information from the light point of view.
                            lightDepthTexture = LightDepthBufferShader.Instance.End();

                            #endregion

                            #region Update Active Shadows Array

                            if (Shadow.DistributeShadowCalculationsBetweenFrames || activeCamerasCount > 1)
                            {
                                // If the information needs to be stored in the array and the entry was not created...
                                if (shadowIndex == -1)
                                {
                                    // We create the entry...
                                    shadowIndex = GetFreeIndexActiveSpotLightShadows();
                                    activeSpotLightShadows[shadowIndex].light = spotLight;
                                }
                                // And update the values.
                                activeSpotLightShadows[shadowIndex].lightViewMatrix = BasicShadowMapShader.Instance.LightViewMatrix;
                                activeSpotLightShadows[shadowIndex].lightProjectionMatrix = BasicShadowMapShader.Instance.LightProjectionMatrix;
                                activeSpotLightShadows[shadowIndex].lightDepthTexture = lightDepthTexture;
                                activeSpotLightShadows[shadowIndex].calculatedThisFrame = true;
                            }

                            #endregion

                        }
                        else // When the light depth textures is not updated it is need to set some global variables to the shader so that it can perform the work accordingly.
                        {
                            // View matrix information has to be updated to avoid an incorrect transformation.
                            BasicShadowMapShader.Instance.SetLight(currentCamera.ViewMatrix, cornersViewSpace,
                                                                   activeSpotLightShadows[shadowIndex].lightViewMatrix, 
                                                                   activeSpotLightShadows[shadowIndex].lightProjectionMatrix);
                        }
                        // Calculate a deferred shadow map.
                        spotLight.ShadowTexture = BasicShadowMapShader.Instance.Render(lightDepthTexture, depthTexture, shadow.DepthBias, shadow.Filter);
                        if (!Shadow.DistributeShadowCalculationsBetweenFrames && activeCamerasCount == 1)
                            RenderTarget.Release(lightDepthTexture);
                        // Testing.
                        //RenderTarget.Release(spotLight.ShadowTexture);
                        //FinishRendering(currentCamera, renderTarget, spotLight.ShadowTexture); return;

                    }
                }
            }
            
            #endregion
            
            #region Point Light Shadows
            
            foreach (PointLight pointLight in pointLightsToRender)
            {
                // If there is a shadow map...
                if (pointLight.Shadow != null && pointLight.Shadow.Enabled)
                {
                    // If the shadow map is a cascaded shadow map...
                    if (pointLight.Shadow is CubeShadow)
                    {
                        CubeShadow shadow = (CubeShadow)pointLight.Shadow;

                        // The texture that contains the visibility information (depth map) from the light point of view.
                        RenderTargetCube lightDepthTextureCube = null;
                        bool calculatedThisFrame = false;

                        // If the shadows are updated one frame but not the other
                        // then the light depth texture and some other information related to the frame when it was generated is stored.
                        // We try to find this information here.
                        int shadowIndex = GetIndexActivePointLightShadows(pointLight);
                        // If there is in the array the light depth texture is used.
                        if (shadowIndex != -1)
                        {
                            lightDepthTextureCube = activePointLightShadows[shadowIndex].lightDepthTexture;
                            calculatedThisFrame = activeSpotLightShadows[shadowIndex].calculatedThisFrame;
                        }
                                
                        // If there was destroyed because the screen was resized or something similar...
                        if (lightDepthTextureCube != null && lightDepthTextureCube.IsDisposed)
                            lightDepthTextureCube = null;

                        if (((Shadow.DistributeShadowCalculationsBetweenFrames == false || Time.TotalFramesCount % 4 != 3) && !calculatedThisFrame) || lightDepthTextureCube == null)
                        {
                            #region Generate Light Depth Texture

                            CubeShadowMapShader.Instance.SetLight(pointLight.cachedPosition, pointLight.Range);
                            // Feth shadow texture and enable it for render.
                            if (lightDepthTextureCube == null)
                                LightDepthBufferShader.Instance.Begin(shadow.LightDepthTextureSize, pointLight.cachedPosition, pointLight.Range);
                            else
                                LightDepthBufferShader.Instance.Begin(lightDepthTextureCube, pointLight.cachedPosition, pointLight.Range);
                            
                            for (int faceNumber = 0; faceNumber < 6; faceNumber++)
                            {
                                LightDepthBufferShader.Instance.SetLightMatrices(CubeShadowMapShader.Instance.LightViewMatrix[faceNumber], CubeShadowMapShader.Instance.LightProjectionMatrix);
                                LightDepthBufferShader.Instance.SetFace((CubeMapFace)faceNumber);

                                // Frustum Culling
                                cameraBoundingFrustum.Matrix = CubeShadowMapShader.Instance.LightViewMatrix[faceNumber] * CubeShadowMapShader.Instance.LightProjectionMatrix;
                                modelsToRenderShadows.Clear();
                                FrustumCulling.ModelRendererFrustumCulling(cameraBoundingFrustum, modelsToRenderShadows);

                                // Sort by skinned and not skinned
                                modelsToRenderShadowsSimple.Clear();
                                modelsToRenderShadowsSkinned.Clear();
                                foreach (ModelRenderer modelRenderer in modelsToRenderShadows)
                                {
                                    if (modelRenderer.CastShadows && (modelRenderer.Material == null || modelRenderer.Material.AlphaBlending == 1))
                                    {
                                        if (modelRenderer.CachedModel.IsSkinned)
                                            modelsToRenderShadowsSkinned.Add(modelRenderer);
                                        else
                                            modelsToRenderShadowsSimple.Add(modelRenderer);
                                    }
                                }
                                // Render simple objects.
                                foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSimple)
                                {
                                    LightDepthBufferShader.Instance.RenderModelCubeShadows(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                                }
                                // Render skinned objects.
                                foreach (ModelRenderer modelRenderer in modelsToRenderShadowsSkinned)
                                {
                                    LightDepthBufferShader.Instance.RenderModelCubeShadows(ref modelRenderer.CachedWorldMatrix, modelRenderer.CachedModel, modelRenderer.cachedBoneTransforms);
                                }
                                LightDepthBufferShader.Instance.UnsetCurrentFace();
                            }
                            lightDepthTextureCube = LightDepthBufferShader.Instance.EndCube();

                            #endregion

                            #region Update Active Shadows Array

                            // If the information needs to be stored in the array and the entry was not created...
                            if (shadowIndex == -1)
                            {
                                // We create the entry...
                                shadowIndex = GetFreeIndexActivePointLightShadows();
                                activePointLightShadows[shadowIndex].light = pointLight;
                            }
                            // And update the values.
                            activePointLightShadows[shadowIndex].lightDepthTexture = lightDepthTextureCube;
                            
                            #endregion
                        }

                        lightDepthTextureCubeArray.Add(lightDepthTextureCube);
                    }
                }
                else
                    lightDepthTextureCubeArray.Add(null);
            }
            
            #endregion
            
            #endregion
            
            #region Light Texture

            LightPrePass.Begin(renderTarget.Size);

            // Put the depth information from the G-Buffer to the hardware Z-Buffer of the light pre pass render targets.
            // The depth information will be used for the stencil operations that allow an important optimization and
            // the possibility to use light clip volumes (to limit the range of influence of a light using convex volumes)
            ReconstructZBufferShader.Instance.Render(gbufferTextures.RenderTargets[0], currentCamera.FarPlane, currentCamera.ProjectionMatrix);

            // Set states common to most lights.
            LightPrePass.SetRenderStates();
            
            #region Ambient Light
            
            // Render ambient light
            if (currentCamera.AmbientLight != null && currentCamera.AmbientLight.Intensity > 0)
            {
                AmbientLightShader.Instance.Render(gbufferTextures.RenderTargets[1], // Normal Texture
                                                   currentCamera.AmbientLight,
                                                   ambientOcclusionTexture,
                                                   currentCamera.ViewMatrix);
            }
            RenderTarget.Release(ambientOcclusionTexture);

            #endregion
            
            #region Directional Lights

            DirectionalLightShader.Instance.Begin(gbufferTextures.RenderTargets[0], // Depth Texture
                                                  gbufferTextures.RenderTargets[1], // Normal Texture
                                                  currentCamera.ViewMatrix,
                                                  cornersViewSpace);
            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                DirectionalLight directionalLight = DirectionalLight.ComponentPool.Elements[i];
                if (directionalLight.Intensity > 0 && directionalLight.IsVisible)
                {
                    DirectionalLightShader.Instance.Render(directionalLight.Color, directionalLight.cachedDirection, directionalLight.Intensity, directionalLight.ShadowTexture);
                }
            }

            #endregion
            
            #region Point Lights
            
            PointLightShader.Instance.Begin(gbufferTextures.RenderTargets[0], // Depth Texture
                                            gbufferTextures.RenderTargets[1], // Normal Texture
                                            currentCamera.ViewMatrix,
                                            currentCamera.ProjectionMatrix,
                                            currentCamera.NearPlane,
                                            currentCamera.FarPlane,
                                            currentCamera.FieldOfView);
            for (int i = 0; i < pointLightsToRender.Count; i++)
            {
                PointLight pointLight = pointLightsToRender[i];
                PointLightShader.Instance.Render(pointLight.Color, pointLight.cachedPosition, pointLight.Intensity, pointLight.Range, lightDepthTextureCubeArray[i],
                                                 pointLight.cachedWorldMatrix, pointLight.RenderClipVolumeInLocalSpace, pointLight.ClipVolume);
            }

            #endregion
            
            #region Spot Lights

            SpotLightShader.Instance.Begin(gbufferTextures.RenderTargets[0], // Depth Texture
                                           gbufferTextures.RenderTargets[1], // Normal Texture
                                           currentCamera.ViewMatrix,
                                           currentCamera.ProjectionMatrix,
                                           currentCamera.NearPlane,
                                           currentCamera.FarPlane);
            foreach (SpotLight spotLight in spotLightsToRender)
            {
                SpotLightShader.Instance.Render(spotLight.Color, spotLight.cachedPosition, spotLight.cachedDirection, spotLight.Intensity, spotLight.Range, 
                                                spotLight.InnerConeAngle, spotLight.OuterConeAngle, spotLight.ShadowTexture, spotLight.LightMaskTexture,
                                                spotLight.cachedWorldMatrix, spotLight.RenderClipVolumeInLocalSpace, spotLight.ClipVolume);
            }

            #endregion
            
            lightTextures = LightPrePass.End();

            #endregion

            #region Release Shadow Textures

            // We can do this from time to time to reduce calculations.
            for (int i = 0; i < SpotLight.ComponentPool.Count; i++)
            {
                SpotLight spotLight = SpotLight.ComponentPool.Elements[i];
                if (spotLight.ShadowTexture != null)
                {
                    RenderTarget.Release(spotLight.ShadowTexture);
                    spotLight.ShadowTexture = null;
                }
            }
            for (int i = 0; i < DirectionalLight.ComponentPool.Count; i++)
            {
                DirectionalLight directionalLight = DirectionalLight.ComponentPool.Elements[i];
                if (directionalLight.ShadowTexture != null)
                {
                    RenderTarget.Release(directionalLight.ShadowTexture);
                    directionalLight.ShadowTexture = null;
                }
            }

            #endregion

            lightDepthTextureCubeArray.Clear();

            // Testing
            //FinishRendering(currentCamera, renderTarget, lightTextures.RenderTargets[0]); return;
            
            #endregion
            
            #region Material Pass

            MaterialPass.Begin(renderTarget.Size, currentCamera.ClearColor);
            
            // Similar to Z-Pre Pass, the previously generated Z-Buffer is used to avoid the generation of invisible fragments.
            // In XNA we lost the GPU Z-Buffer, therefore a regeneration pass is needed.
            // It is possible that the cost of this regeneration could be higher than the performance improvement, particularly in small scenes.
            ReconstructZBufferShader.Instance.Render(gbufferTextures.RenderTargets[0], currentCamera.FarPlane, currentCamera.ProjectionMatrix);
            EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
            EngineManager.Device.BlendState = BlendState.Opaque;

            #region Opaque Objects
          
            #region Blinn Phong

            BlinnPhongShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTextures.RenderTargets[0], lightTextures.RenderTargets[1], gbufferTextures.RenderTargets[1]);
            // Blinn Phong Simple
            for (int i = 0; i < gbufferSimple.Count; i++)
            {
                var meshPartToRender = gbufferSimple[i];
                if (meshPartToRender.Material is BlinnPhong)
                    BlinnPhongShader.Instance.RenderModelSimple(ref meshPartToRender.WorldMatrix, meshPartToRender.Model, 
                                                                (BlinnPhong) meshPartToRender.Material, 
                                                                meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferWithNormalMap.Count; i++)
            {
                var meshPartToRender = gBufferWithNormalMap[i];
                if (meshPartToRender.Material is BlinnPhong)
                    BlinnPhongShader.Instance.RenderModelSimple(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                                (BlinnPhong)meshPartToRender.Material,
                                                                meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            // Blinn Phong Skinned
            for (int i = 0; i < gBufferSkinnedSimple.Count; i++)
            {
                var meshPartToRender = gBufferSkinnedSimple[i];
                if (meshPartToRender.Material is BlinnPhong)
                    BlinnPhongShader.Instance.RenderModelSkinned(ref meshPartToRender.WorldMatrix, meshPartToRender.Model, meshPartToRender.BoneTransform,
                                                                 (BlinnPhong)meshPartToRender.Material,
                                                                 meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferSkinnedWithNormalMap.Count; i++)
            {
                var meshPartToRender = gBufferSkinnedWithNormalMap[i];
                if (meshPartToRender.Material is BlinnPhong)
                    BlinnPhongShader.Instance.RenderModelSkinned(ref meshPartToRender.WorldMatrix, meshPartToRender.Model, meshPartToRender.BoneTransform,
                                                                 (BlinnPhong)meshPartToRender.Material,
                                                                 meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            // Blinn Phong Parallax
            // This has to be after the other techniques because it unlink the G-Buffer normal texture that is used in the other techniques.
            for (int i = 0; i < gBufferWithParallax.Count; i++)
            {
                var meshPartToRender = gBufferWithParallax[i];
                if (meshPartToRender.Material is BlinnPhong)
                    BlinnPhongShader.Instance.RenderModelParallax(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                                  (BlinnPhong)meshPartToRender.Material,
                                                                  meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }

            #endregion

            #region Constant

            ConstantShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
            // Simple
            for (int i = 0; i < gbufferSimple.Count; i++)
            {
                var meshPartToRender = gbufferSimple[i];
                if (meshPartToRender.Material is Constant)
                    ConstantShader.Instance.RenderModelSimple(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                              (Constant)meshPartToRender.Material,
                                                              meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferWithNormalMap.Count; i++)
            {
                var meshPartToRender = gBufferWithNormalMap[i];
                if (meshPartToRender.Material is Constant)
                    ConstantShader.Instance.RenderModelSimple(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                              (Constant)meshPartToRender.Material,
                                                              meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferWithParallax.Count; i++)
            {
                var meshPartToRender = gBufferWithParallax[i];
                if (meshPartToRender.Material is Constant)
                    ConstantShader.Instance.RenderModelSimple(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                              (Constant)meshPartToRender.Material,
                                                              meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            // Skinned
            for (int i = 0; i < gBufferSkinnedSimple.Count; i++)
            {
                var meshPartToRender = gBufferSkinnedSimple[i];
                if (meshPartToRender.Material is Constant)
                    ConstantShader.Instance.RenderModelSkinned(ref meshPartToRender.WorldMatrix, meshPartToRender.Model, 
                                                               meshPartToRender.BoneTransform,
                                                               (Constant)meshPartToRender.Material,
                                                               meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferSkinnedWithNormalMap.Count; i++)
            {
                var meshPartToRender = gBufferSkinnedWithNormalMap[i];
                if (meshPartToRender.Material is Constant)
                    ConstantShader.Instance.RenderModelSkinned(ref meshPartToRender.WorldMatrix, meshPartToRender.Model, 
                                                               meshPartToRender.BoneTransform,
                                                               (Constant)meshPartToRender.Material,
                                                               meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }

            #endregion

            #region Car Paint

            CarPaintShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, lightTextures.RenderTargets[0], lightTextures.RenderTargets[1], gbufferTextures.RenderTargets[1]);
            for (int i = 0; i < gbufferSimple.Count; i++)
            {
                var meshPartToRender = gbufferSimple[i];
                if (meshPartToRender.Material is CarPaint)
                    CarPaintShader.Instance.RenderModel(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                        (CarPaint)meshPartToRender.Material,
                                                        meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }
            for (int i = 0; i < gBufferWithNormalMap.Count; i++)
            {
                var meshPartToRender = gBufferWithNormalMap[i];
                if (meshPartToRender.Material is CarPaint)
                    CarPaintShader.Instance.RenderModel(ref meshPartToRender.WorldMatrix, meshPartToRender.Model,
                                                        (CarPaint)meshPartToRender.Material,
                                                        meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
            }

            #endregion

            #endregion

            // Set Render States.

            EngineManager.Device.BlendState = BlendState.NonPremultiplied;
            EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;

            #region Sky

            // The sky is render later so that the GPU can avoid fragment processing. But it has to be done before the transparent objects.
            if (currentCamera.Sky != null)
            {
                if (currentCamera.Sky is Skybox && ((Skybox)currentCamera.Sky).TextureCube != null)
                {
                    SkyboxShader.Instance.Render(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane, (Skybox)(currentCamera.Sky));
                }
                if (currentCamera.Sky is Skydome && ((Skydome)currentCamera.Sky).Texture != null)
                {
                    if (DirectionalLight.Sun != null)
                        SkydomeShader.Instance.Render(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane, DirectionalLight.Sun.cachedDirection, (Skydome)(currentCamera.Sky));
                    else
                        SkydomeShader.Instance.Render(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane, Vector3.Forward, (Skydome)(currentCamera.Sky));
                }
            }

            #endregion

            #region Particles

            ParticleShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.AspectRatio, currentCamera.FarPlane,
                                          new Size(currentCamera.Viewport.Width, currentCamera.Viewport.Height), gbufferTextures.RenderTargets[0]);
            for (int i = 0; i < ParticleRenderer.ComponentPool.Count; i++)
            {
                ParticleRenderer particleRenderer = ParticleRenderer.ComponentPool.Elements[i];

                if (particleRenderer.cachedParticleSystem != null && particleRenderer.Texture != null && particleRenderer.IsVisible)
                    ParticleShader.Instance.Render(particleRenderer.cachedParticleSystem, particleRenderer.cachedDuration,
                                                   particleRenderer.BlendState, particleRenderer.DurationRandomness, particleRenderer.Gravity,
                                                   particleRenderer.EndVelocity, particleRenderer.MinimumColor, particleRenderer.MaximumColor,
                                                   particleRenderer.RotateSpeed, particleRenderer.StartSize, particleRenderer.EndSize,
                                                   particleRenderer.Texture, particleRenderer.TilesX, particleRenderer.TilesY, particleRenderer.AnimationRepetition, 
                                                   particleRenderer.SoftParticles, particleRenderer.FadeDistance);
            }

            #endregion

            #region Transparent Objects

            // Sorting from back to front.
            // The mesh parts compares the bounding sphere of the entire model.
            // If you need a better sorting separate the mesh parts into models or store and update a bounding sphere per mesh part.
            //transparentObjects.Sort(CompareMeshParts); // Produces garbage.
            BubbleSort(transparentObjects);

            #region Blinn Phong

            if (DirectionalLight.Sun != null)
                ForwardBlinnPhongShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.AmbientLight,
                                                       DirectionalLight.Sun.Color, DirectionalLight.Sun.cachedDirection, DirectionalLight.Sun.Intensity, null, renderTarget.Size);
            else
                ForwardBlinnPhongShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.AmbientLight, Color.Black, Vector3.UnitZ, 0, null, Size.FullScreen);
            // Blinn Phong Simple
            for (int i = 0; i < transparentObjects.Count; i++)
            {
                var meshPartToRender = transparentObjects[i];
                if (meshPartToRender.Material is BlinnPhong)
                {
                    // Search closer lights.

                    #region Spot Light

                    SpotLight closerSpotLight = null;
                    float closerDistantance = currentCamera.FarPlane;
                    foreach (SpotLight spotLight in spotLightsToRender)
                    {
                        float spotLightDistance = Vector3.Distance(meshPartToRender.Model.BoundingSphere.Center, spotLight.cachedPosition);
                        if (spotLightDistance < closerDistantance)
                        {
                            closerDistantance = spotLightDistance;
                            closerSpotLight = spotLight;
                        }
                    }
                    Vector3 spotLightPosition = Vector3.Zero;
                    Vector3 spotLightDirection = Vector3.UnitX;
                    Color spotLightColor = Color.Black;
                    float spotLightIntensity = 0;
                    float spotLightInnerAngle = 0;
                    float spotLightOuterAngle = 0;
                    float spotLightRange = 0;
                    if (closerSpotLight != null)
                    {
                        spotLightPosition = closerSpotLight.cachedPosition;
                        spotLightDirection = closerSpotLight.cachedDirection;
                        spotLightColor = closerSpotLight.Color;
                        spotLightIntensity = closerSpotLight.Intensity;
                        spotLightInnerAngle = closerSpotLight.InnerConeAngle;
                        spotLightOuterAngle = closerSpotLight.OuterConeAngle;
                        spotLightRange = closerSpotLight.Range;
                    }

                    #endregion

                    #region Point Lights

                    PointLight closerPointLight1 = null;
                    PointLight closerPointLight2 = null;
                    float closerDistantance1 = currentCamera.FarPlane;
                    float closerDistantance2 = currentCamera.FarPlane;
                    foreach (PointLight pointLight in pointLightsToRender)
                    {
                        float pointLightDistance = Vector3.Distance(meshPartToRender.Model.BoundingSphere.Center, pointLight.cachedPosition);
                        if (pointLightDistance < closerDistantance2)
                        {
                            closerDistantance2 = pointLightDistance;
                            closerPointLight2 = pointLight;
                            if (pointLightDistance < closerDistantance1)
                            {
                                float swapCloserDistance= closerDistantance2;
                                PointLight swapCloserPointLight = closerPointLight2;
                                closerDistantance2 = closerDistantance1;
                                closerPointLight2 = closerPointLight1;
                                closerDistantance1 = swapCloserDistance;
                                closerPointLight1 = swapCloserPointLight;
                            }
                        }
                    }
                    Vector3 pointLightPos1 = Vector3.Zero;
                    Color pointLightColor1 = Color.Black;
                    float pointLightIntensity1 = 0;
                    float pointLightRange1 = 0;
                    Vector3 pointLightPos2 = Vector3.Zero;
                    Color pointLightColor2 = Color.Black;
                    float pointLightIntensity2 = 0;
                    float pointLightRange2 = 0;
                    if (closerPointLight1 != null)
                    {
                        pointLightPos1 = closerPointLight1.cachedPosition;
                        pointLightColor1 = closerPointLight1.Color;
                        pointLightIntensity1 = closerPointLight1.Intensity;
                        pointLightRange1 = closerPointLight1.Range;
                    }
                    if (closerPointLight2 != null)
                    {
                        pointLightPos2 = closerPointLight2.cachedPosition;
                        pointLightColor2 = closerPointLight2.Color;
                        pointLightIntensity2 = closerPointLight2.Intensity;
                        pointLightRange2 = closerPointLight2.Range;
                    }

                    #endregion

                    ForwardBlinnPhongShader.Instance.RenderModel(ref meshPartToRender.WorldMatrix,
                                                                 meshPartToRender.Model,
                                                                 (BlinnPhong) meshPartToRender.Material,
                                                                 meshPartToRender.MeshIndex, meshPartToRender.MeshPart,
                                                                 spotLightPosition, spotLightDirection, spotLightColor,
                                                                 spotLightIntensity, spotLightInnerAngle, spotLightOuterAngle, spotLightRange,
                                                                 pointLightPos1, pointLightColor1, pointLightIntensity1, pointLightRange1,
                                                                 pointLightPos2, pointLightColor2, pointLightIntensity2, pointLightRange2);
                }
            }

            #endregion

            #region Constant

            ConstantShader.Instance.Begin(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
            // Constant Simple
            for (int i = 0; i < transparentObjects.Count; i++)
            {
                var meshPartToRender = transparentObjects[i];
                if (meshPartToRender.Material is Constant)
                {
                    // Search closer lights.
                    ConstantShader.Instance.RenderModelSimple(ref meshPartToRender.WorldMatrix,
                                                              meshPartToRender.Model,
                                                              (Constant)meshPartToRender.Material,
                                                              meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
                }
            }
            for (int i = 0; i < transparentSkinnedObjects.Count; i++)
            {
                var meshPartToRender = transparentSkinnedObjects[i];
                if (meshPartToRender.Material is Constant)
                {
                    // Search closer lights.
                    ConstantShader.Instance.RenderModelSkinned(ref meshPartToRender.WorldMatrix,
                                                               meshPartToRender.Model, meshPartToRender.BoneTransform,
                                                               (Constant)meshPartToRender.Material,
                                                               meshPartToRender.MeshIndex, meshPartToRender.MeshPart);
                }
            }

            #endregion
            
            #endregion

            // The 3D Heads Up Displays that needs to be calculated in linear space are processed here.

            #region Textures and Text

            if (HudText.ComponentPool3D.Count != 0 || HudTexture.ComponentPool3D.Count != 0)
            {
                SpriteManager.Begin3DLinearSpace(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);
                for (int i = 0; i < HudTexture.ComponentPool3D.Count; i++)
                {
                    HudTexture hudTexture = HudTexture.ComponentPool3D.Elements[i];
                    if (hudTexture.Texture != null && hudTexture.PostProcessed && hudTexture.IsVisible)
                    {
                        if (hudTexture.Billboard)
                        {
                            if (hudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DBillboardTexture(hudTexture.Texture,
                                                                     hudTexture.CachedWorldMatrix,
                                                                     hudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                            else
                                SpriteManager.Draw3DBillboardTexture(hudTexture.Texture,
                                                                     hudTexture.CachedWorldMatrix,
                                                                     hudTexture.SourceRectangle,
                                                                     hudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                        }
                        else
                        {
                            if (hudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DTexture(hudTexture.Texture,
                                                          hudTexture.CachedWorldMatrix,
                                                          hudTexture.SourceRectangle,
                                                          hudTexture.Color);
                            else
                                SpriteManager.Draw3DTexture(hudTexture.Texture,
                                                          hudTexture.CachedWorldMatrix,
                                                          hudTexture.Color);
                        }
                    }
                }
                for (int i = 0; i < HudText.ComponentPool3D.Count; i++)
                {
                    HudText hudText = HudText.ComponentPool3D.Elements[i];
                    if (hudText.PostProcessed && hudText.IsVisible)
                    {
                        if (hudText.Billboard)
                            SpriteManager.Draw3DBillboardText(hudText.Font ?? Font.DefaultFont,
                                                              hudText.Text,
                                                              hudText.CachedWorldMatrix,
                                                              hudText.Color,
                                                              currentCamera.Position,
                                                              currentCamera.Up,
                                                              currentCamera.Forward);
                        else
                            SpriteManager.Draw3DText(hudText.Font ?? Font.DefaultFont, hudText.Text, hudText.CachedWorldMatrix, hudText.Color);

                    }
                }
                SpriteManager.End();
            }

            #endregion

            #region 3D Lines (Line List)

            LineManager.Begin3D(PrimitiveType.LineList, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);

            for (int i = 0; i < LineRenderer.ComponentPool3D.Count; i++)
            {
                LineRenderer lineRenderer = LineRenderer.ComponentPool3D.Elements[i];
                if (lineRenderer.Vertices != null && lineRenderer.IsVisible && lineRenderer.PostProcessed && lineRenderer.PrimitiveType == PrimitiveType.LineList)
                {
                    for (int j = 0; j < lineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(Vector3.Transform(lineRenderer.Vertices[j].Position, lineRenderer.CachedWorldMatrix), lineRenderer.Vertices[j].Color);
                }
            }

            LineManager.End();

            #endregion
            
            sceneTexture = MaterialPass.End();
            RenderTarget.Release(lightTextures);
            // Testing
            //FinishRendering(currentCamera, renderTarget, sceneTexture); return;

            #endregion
            
            #region Post Process Pass

            PostProcessingPass.BeginAndProcess(currentCamera.PostProcess, sceneTexture, gbufferTextures.RenderTargets[0], gbufferHalfTextures.RenderTargets[0], ref currentCamera.LuminanceTexture,
                                               renderTarget, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, currentCamera.FarPlane, currentCamera.Position, cornersViewSpace);
            
            // Render in gamma space

            #region 3D Textures and Text

            if (HudText.ComponentPool3D.Count != 0 || HudTexture.ComponentPool3D.Count != 0)
            {
                SpriteManager.Begin3DGammaSpace(currentCamera.ViewMatrix, currentCamera.ProjectionMatrix, gbufferTextures.RenderTargets[0], currentCamera.FarPlane);
                for (int i = 0; i < HudTexture.ComponentPool3D.Count; i++)
                {
                    HudTexture hudTexture = HudTexture.ComponentPool3D.Elements[i];
                    if (hudTexture.IsVisible && hudTexture.Texture != null && !hudTexture.PostProcessed)
                    {
                        if (hudTexture.Billboard)
                        {
                            if (hudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DBillboardTexture(hudTexture.Texture,
                                                                     hudTexture.CachedWorldMatrix,
                                                                     hudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                            else
                                SpriteManager.Draw3DBillboardTexture(hudTexture.Texture,
                                                                     hudTexture.CachedWorldMatrix,
                                                                     hudTexture.SourceRectangle,
                                                                     hudTexture.Color,
                                                                     currentCamera.Position,
                                                                     currentCamera.Up,
                                                                     currentCamera.Forward);
                        }
                        else
                        {
                            if (hudTexture.DestinationRectangle != null)
                                SpriteManager.Draw3DTexture(hudTexture.Texture,
                                                          hudTexture.CachedWorldMatrix,
                                                          hudTexture.SourceRectangle,
                                                          hudTexture.Color);
                            else
                                SpriteManager.Draw3DTexture(hudTexture.Texture,
                                                          hudTexture.CachedWorldMatrix,
                                                          hudTexture.Color);
                        }
                    }
                }
                for (int i = 0; i < HudText.ComponentPool3D.Count; i++)
                {
                    HudText hudText = HudText.ComponentPool3D.Elements[i];
                    if (hudText.IsVisible && !hudText.PostProcessed)
                    {
                        if (hudText.Billboard)
                            SpriteManager.Draw3DBillboardText(hudText.Font ?? Font.DefaultFont,
                                                              hudText.Text,
                                                              hudText.CachedWorldMatrix,
                                                              hudText.Color,
                                                              currentCamera.Position,
                                                              currentCamera.Up,
                                                              currentCamera.Forward);
                        else
                            SpriteManager.Draw3DText(hudText.Font ?? Font.DefaultFont, hudText.Text, hudText.CachedWorldMatrix, hudText.Color);

                    }
                }
                SpriteManager.End();
            }

            #endregion

            #region 3D Lines (Line List)

            LineManager.Begin3D(PrimitiveType.LineList, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);

            #region Bounding Volumes

            for (int i = 0; i < modelsToRender.Count; i++)
            {
                ModelRenderer modelRenderer = modelsToRender[i];
                if (modelRenderer.CachedModel != null && (modelRenderer.RenderNonAxisAlignedBoundingBox || modelRenderer.RenderBoundingSphere || modelRenderer.RenderAxisAlignedBoundingBox))
                {
                    if (modelRenderer.RenderNonAxisAlignedBoundingBox)
                    {
                        // Doing this allows to show a more correct bounding box.
                        // But be aware that the axis aligned bounding box calculated in the model renderer component does not match this.
                        LineManager.DrawBoundingBox(modelRenderer.CachedModel.BoundingBox, Color.Gray, modelRenderer.CachedWorldMatrix);
                    }
                    if (modelRenderer.RenderAxisAlignedBoundingBox)
                    {
                        LineManager.DrawBoundingBox(modelRenderer.BoundingBox, Color.Gray);
                    }
                    if (modelRenderer.RenderBoundingSphere)
                    {
                        LineManager.DrawBoundingSphere(modelRenderer.BoundingSphere, Color.Gray);
                    }
                }
            }

            #endregion

            #region Lines

            for (int i = 0; i < LineRenderer.ComponentPool3D.Count; i++)
            {
                LineRenderer lineRenderer = LineRenderer.ComponentPool3D.Elements[i];
                if (lineRenderer.Vertices != null && lineRenderer.IsVisible && lineRenderer.PrimitiveType == PrimitiveType.LineList && !lineRenderer.PostProcessed)
                {
                    for (int j = 0; j < lineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(Vector3.Transform(lineRenderer.Vertices[j].Position, lineRenderer.CachedWorldMatrix), lineRenderer.Vertices[j].Color);
                }
            }

            #endregion

            LineManager.End();

            #endregion

            #region 3D Lines (Triangle List)

            LineManager.Begin3D(PrimitiveType.TriangleList, currentCamera.ViewMatrix, currentCamera.ProjectionMatrix);

            for (int i = 0; i < LineRenderer.ComponentPool3D.Count; i++)
            {
                LineRenderer lineRenderer = LineRenderer.ComponentPool3D.Elements[i];
                if (lineRenderer.Vertices != null && lineRenderer.IsVisible && lineRenderer.PrimitiveType == PrimitiveType.TriangleList)
                {
                    for (int j = 0; j < lineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(Vector3.Transform(lineRenderer.Vertices[j].Position, lineRenderer.CachedWorldMatrix), lineRenderer.Vertices[j].Color);
                }
            }

            LineManager.End();

            #endregion

            PostProcessingPass.End();

            #endregion

            ReleaseUnusedRenderTargets();

            // Reset Camera Culling Mask
            Layer.CurrentCameraCullingMask = uint.MaxValue;
            
        } // RenderCamera

        #region Active Shadows Methods

        /// <summary>
        /// Returns the index of the array that contains the light depth texture (and other data) previously generated.
        /// To work with split screen we need to store this map for each active camera.
        /// -1 indicates that is was not founded.
        /// </summary>
        public static int GetIndexActiveDirectionalLightShadows(Camera camera, DirectionalLight light)
        {
            for (int i = 0; i < activeDirectionalLightShadows.Length; i++)
            {
                if (activeDirectionalLightShadows[i].camera == camera && activeDirectionalLightShadows[i].light == light)
                    return i;
            }
            return -1;
        } // GetIndexActiveDirectionalLightShadows

        /// <summary>
        /// Gets the index of the first free slot in the active directiona light shadows array.
        /// </summary>
        public static int GetFreeIndexActiveDirectionalLightShadows()
        {
            for (int i = 0; i < activeDirectionalLightShadows.Length; i++)
            {
                if (activeDirectionalLightShadows[i].camera == null)
                    return i;
            }
            // If there is no free slot (almost impossible)...
            DirectionalLightDepthInformation[] newArray = new DirectionalLightDepthInformation[activeDirectionalLightShadows.Length + 5];
            for (int i = 0; i < activeDirectionalLightShadows.Length; i++)
            {
                newArray[i] = activeDirectionalLightShadows[i];
            }
            activeDirectionalLightShadows = newArray;
            return activeDirectionalLightShadows.Length - 5;
        } // GetFreeIndexActiveDirectionalLightShadows

        /// <summary>
        /// Returns the index of the array that contains the light depth texture (and other data) previously generated.
        /// -1 indicates that is was not founded.
        /// </summary>
        public static int GetIndexActiveSpotLightShadows(SpotLight light)
        {
            for (int i = 0; i < activeSpotLightShadows.Length; i++)
            {
                if (activeSpotLightShadows[i].light == light)
                    return i;
            }
            return -1;
        } // GetIndexActiveSpotLightShadows

        /// <summary>
        /// Gets the index of the first free slot in the active spot light shadows array.
        /// </summary>
        public static int GetFreeIndexActiveSpotLightShadows()
        {
            for (int i = 0; i < activeSpotLightShadows.Length; i++)
            {
                if (activeSpotLightShadows[i].light == null)
                    return i;
            }
            // If there is no free slot...
            SpotLightDepthInformation[] newArray = new SpotLightDepthInformation[activeDirectionalLightShadows.Length + 5];
            for (int i = 0; i < activeSpotLightShadows.Length; i++)
            {
                newArray[i] = activeSpotLightShadows[i];
            }
            activeSpotLightShadows = newArray;
            return activeSpotLightShadows.Length - 5;
        } // GetFreeIndexActiveSpotLightShadows

        /// <summary>
        /// Returns the index of the array that contains the light depth texture (and other data) previously generated.
        /// -1 indicates that is was not founded.
        /// </summary>
        public static int GetIndexActivePointLightShadows(PointLight light)
        {
            for (int i = 0; i < activePointLightShadows.Length; i++)
            {
                if (activePointLightShadows[i].light == light)
                    return i;
            }
            return -1;
        } // GetIndexActivePointLightShadows

        /// <summary>
        /// Gets the index of the first free slot in the active point light shadows array.
        /// </summary>
        public static int GetFreeIndexActivePointLightShadows()
        {
            for (int i = 0; i < activePointLightShadows.Length; i++)
            {
                if (activePointLightShadows[i].light == null)
                    return i;
            }
            // If there is no free slot...
            PointLightDepthInformation[] newArray = new PointLightDepthInformation[activeDirectionalLightShadows.Length + 5];
            for (int i = 0; i < activePointLightShadows.Length; i++)
            {
                newArray[i] = activePointLightShadows[i];
            }
            activePointLightShadows = newArray;
            return activePointLightShadows.Length - 5;
        } // GetFreeIndexActivePointLightShadows

        #endregion

        #region Sort Mesh Parts

        /// <summary>
        /// Simple Bubble Sort.
        /// </summary>
        private static void BubbleSort(List<MeshPartToRender> list)
        {
            for (int pass = 1; pass < list.Count; pass++)
                for (int i = 0; i < list.Count - 1; i++)
                {
                    float objectDistance1 = Vector3.Distance(list[i].Model.BoundingSphere.Center, cameraPosition);
                    float objectDistance2 = Vector3.Distance(list[i + 1].Model.BoundingSphere.Center, cameraPosition);
                    if (objectDistance1 > objectDistance2)
                    {
                        // Swap
                        MeshPartToRender temp = list[i];
                        list[i] = list[i + 1];
                        list[i + 1] = temp;
                    }
                }
        } // BubbleSort

        /*/// <summary> 
        /// TODO: it seems it is broken. I have to replace it.
        /// Average case: O(n log n)
        /// Best case: O(n log n)
        /// Worst case: O(n log n)
        /// </summary>
        private static void HeapSort(List<MeshPartToRender> list)
        {
            for (int i = (list.Count / 2) - 1; i >= 0; i--)
                HeapInternalFunction(list, i, list.Count);
            for (int i = list.Count - 1; i >= 1; i--)
            {
                // Swap
                MeshPartToRender temp = list[0];
                list[0] = list[i];
                list[i] = temp;

                HeapInternalFunction(list, 0, i - 1);
            }
        } // HeapSort

        private static void HeapInternalFunction(List<MeshPartToRender> list, int root, int bottom)
        {
            bool completed = false;

            while ((root * 2 <= bottom) && (!completed))
            {
                float objectDistance1 = Vector3.Distance(list[root * 2].Model.BoundingSphere.Center, cameraPosition);
                float objectDistance2 = Vector3.Distance(list[root * 2 + 1].Model.BoundingSphere.Center, cameraPosition);

                int maxChild;
                if (root * 2 == bottom)
                    maxChild = root * 2;
                else if (objectDistance1 > objectDistance2)
                    maxChild = root * 2;
                else
                    maxChild = root * 2 + 1;

                objectDistance1 = Vector3.Distance(list[root].Model.BoundingSphere.Center, cameraPosition);
                objectDistance2 = Vector3.Distance(list[maxChild].Model.BoundingSphere.Center, cameraPosition);

                if (objectDistance1 < objectDistance2)
                {
                    // Swap
                    MeshPartToRender temp = list[root];
                    list[root] = list[maxChild];
                    list[maxChild] = temp;

                    root = maxChild;
                }
                else
                {
                    completed = true;
                }
            }
        } // Heapify*/

        #endregion

        #region Release Unused Render Targets

        /// <summary>
        /// Release Unused Render Targets.
        /// </summary>
        private static void ReleaseUnusedRenderTargets()
        {
            RenderTarget.Release(gbufferTextures);
            RenderTarget.Release(gbufferHalfTextures);
            RenderTarget.Release(gbufferQuarterTextures);
            RenderTarget.Release(lightTextures);
            if (sceneTexture != null)
            {
                RenderTarget.Release(sceneTexture);
                sceneTexture = null;
            }
            if (ambientOcclusionTexture != null)
            {
                RenderTarget.Release(ambientOcclusionTexture);
                ambientOcclusionTexture = null;
            }
        } // ReleaseUnusedRenderTargets

        #endregion

        #endregion

        #region Render Heads Up Display

        /// <summary>
        /// Render the Head Up Display
        /// </summary>
        private static void RenderHeadsUpDisplay()
        {
            SpriteManager.Begin2D();
            {

                #region Videos

                VideoRenderer video;
                for (int i = 0; i < VideoRenderer.ComponentPool.Count; i++)
                {
                    video = VideoRenderer.ComponentPool.Elements[i];
                    video.Update();
                    if (video.IsVisible && video.State != MediaState.Stopped)
                    {
                        // Aspect ratio
                        Rectangle screenRectangle;
                        float videoAspectRatio = (float)video.Texture.Width / (float)video.Texture.Height,
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
                        SpriteManager.Draw2DTexture(video.Texture, video.CachedPosition.Z, screenRectangle, null, Color.White, 0, Vector2.Zero);
                    }
                }

                #endregion

            }
            SpriteManager.End();

            #region 2D Lines

            LineManager.Begin2D(PrimitiveType.TriangleList);
            for (int i = 0; i < LineRenderer.ComponentPool2D.Count; i++)
            {
                LineRenderer lineRenderer = LineRenderer.ComponentPool2D.Elements[i];
                if (lineRenderer.Vertices != null && lineRenderer.IsVisible && lineRenderer.PrimitiveType == PrimitiveType.TriangleList)
                {
                    for (int j = 0; j < lineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(lineRenderer.Vertices[j].Position, lineRenderer.Vertices[j].Color);
                }
            }
            LineManager.End();

            LineManager.Begin2D(PrimitiveType.LineList);
            for (int i = 0; i < LineRenderer.ComponentPool2D.Count; i++)
            {
                LineRenderer currentLineRenderer = LineRenderer.ComponentPool2D.Elements[i];
                if (currentLineRenderer.Vertices != null && currentLineRenderer.IsVisible && currentLineRenderer.PrimitiveType == PrimitiveType.LineList)
                {
                    for (int j = 0; j < currentLineRenderer.Vertices.Length; j++)
                        LineManager.AddVertex(currentLineRenderer.Vertices[j].Position, currentLineRenderer.Vertices[j].Color);
                }
            }
            LineManager.End();

            #endregion

            SpriteManager.Begin2D();
            {

                #region HUD Text

                HudText hudText;
                for (int i = 0; i < HudText.ComponentPool2D.Count; i++)
                {
                    hudText = HudText.ComponentPool2D.Elements[i];
                    if (hudText.IsVisible)
                    {
                        SpriteManager.Draw2DText(hudText.Font ?? Font.DefaultFont, hudText.Text, hudText.CachedPosition, hudText.Color, hudText.CachedRotation, Vector2.Zero, hudText.CachedScale);
                    }
                }

                #endregion

                #region HUD Texture

                HudTexture hudTexture;
                for (int i = 0; i < HudTexture.ComponentPool2D.Count; i++)
                {
                    hudTexture = HudTexture.ComponentPool2D.Elements[i];
                    if (hudTexture.IsVisible && hudTexture.Texture != null)
                    {
                        if (hudTexture.DestinationRectangle != null)
                            SpriteManager.Draw2DTexture(hudTexture.Texture, hudTexture.CachedPosition.Z, hudTexture.DestinationRectangle.Value, hudTexture.SourceRectangle,
                                                        hudTexture.Color, hudTexture.CachedRotation, Vector2.Zero);
                        else
                            SpriteManager.Draw2DTexture(hudTexture.Texture, hudTexture.CachedPosition, hudTexture.SourceRectangle, hudTexture.Color, hudTexture.CachedRotation,
                                                        Vector2.Zero, hudTexture.CachedScale);
                    }
                }

                #endregion

            }
            SpriteManager.End();
        } // RenderHeadsUpDisplay

        #endregion

        #endregion

        #region End Run

        internal static void EndRun()
        {
            foreach (Scene scene in Scene.CurrentScenes)
            {
                scene.Unitialize();
            }
            // Disable wiimote and keyboard hook.
            InputManager.UnloadInputDevices();
        } // UnloadContent

        #endregion

        #region Remove Unused Resources

        /// <summary>
        /// Remove Unused Resources.
        /// This is intended to be used when you load a level.
        /// </summary>
        public static void RemoveUnusedResources()
        {
            SoundManager.RemoveNotReservedUnusedSoundInstances();
            GarbageCollector.CollectGarbage();
        } // RemoveUnusedResources

        #endregion

    } // GameLoop
} // XNAFinalEngine.EngineCore