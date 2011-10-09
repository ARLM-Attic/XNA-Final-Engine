
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
using XNAFinalEngine.Animations;
using XNAFinalEngine.Components;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using RootAnimation = XNAFinalEngine.Components.RootAnimations;
using XNAFinalEngine.Scenes;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The manager of all managers.
    /// </summary>
    public static class GameLoop
    {

        #region Variables

        private static GBuffer gbuffer;
        private static EditorCamera camera;
        private static ModelAnimation testAnimation;
        private static ModelAnimationPlayer animationPlayer;
        private static FileModel dude;
        private static SkinnedEffect effect;
        private static Matrix[] boneTransforms = new Matrix[58];
        private static Matrix[] worldTransforms = new Matrix[58];
        private static Matrix[] skinTransforms = new Matrix[58];

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

            gbuffer = new GBuffer(RenderTarget.SizeType.FullScreen);
            
            camera = new EditorCamera(new Vector3(0, 30, 0), 200, 0, 0);
            camera.FarPlane = 20000;

            testAnimation = new ModelAnimation("dude");
            animationPlayer = new ModelAnimationPlayer();

            dude = new FileModel("DudeWalk");
            effect = new SkinnedEffect(EngineManager.Device);

            animationPlayer.Play(testAnimation);

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

            #region Scene Update Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.UpdateTasks();
            }

            #endregion

            fpsText.Transform.LocalPosition = new Vector3(Screen.Width - 100, 20, 0);
 
            Input.InputManager.Update();
            camera.Update();

            for (int i = 0; i < RootAnimation.RootAnimationPool.Count; i++)
            {
                RootAnimation.RootAnimationPool.Elements[i].Update();
            }
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

            #region Scene Pre Render Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.PreRenderTasks();
            }

            #endregion

            #region Model Animation Processing

            // Update every active animation.
            // The output is a skeletal/rigid pose in local space for each active clip.
            // The pose might contain information for every joint in the skeleton (a full-body pose),
            // for only a subset of joints (partial pose), or it might be a difference pose for use in additive blending.
            // TODO!!! foreach animation in poolanimationsplayers { Update }
            animationPlayer.Update();

            // Sometimes the final pose is a composition of a number of animation clips. In this stage the animations are blended.
            // The blending includes: lerp, additive blending and cross fading blending. 
            // TODO!! foeach modelAnimationComponent blend active animations according to a blend tree or something similar.

            // The global pose (world space) is generated.
            // However, if no post processing exist (IK, ragdolls, etc.) this stage could be merge with
            // the inverse bind pose multiplication stage in the mesh draw code. And for now the engine will do this.

            #endregion

            #region Graphics

            for (int bone = 0; bone < worldTransforms.Length; bone++)
            {
                boneTransforms[bone] = Matrix.CreateScale(animationPlayer.BoneTransforms[bone].scale) *
                                       Matrix.CreateFromQuaternion(animationPlayer.BoneTransforms[bone].rotation) *
                                       Matrix.CreateTranslation(animationPlayer.BoneTransforms[bone].position);
            }
            // Root bone.
            worldTransforms[0] = boneTransforms[0] * Matrix.Identity;
            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = ((ModelAnimationData)dude.Resource.Tag).SkeletonHierarchy[bone];
                worldTransforms[bone] = boneTransforms[bone] * worldTransforms[parentBone];
            }
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = ((ModelAnimationData)dude.Resource.Tag).InverseBindPose[bone] * worldTransforms[bone];
            }
            
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;
            effect.EnableDefaultLighting();
            effect.DiffuseColor = new Vector3(255, 255, 255);
            effect.SpecularColor = new Vector3(0.25f);
            effect.SpecularPower = 1;
            effect.SetBoneTransforms(skinTransforms);
            effect.CurrentTechnique.Passes[0].Apply();
            
            foreach (ModelMesh mesh in dude.Resource.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // Set vertex buffer and index buffer
                    EngineManager.Device.SetVertexBuffer(part.VertexBuffer);
                    EngineManager.Device.Indices = part.IndexBuffer;
                    // And render all primitives
                    EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
            
            /*
            gbuffer.Begin(camera.ViewMatrix, camera.ProjectionMatrix, 100);
                ModelRenderer currentModelRenderer; 
                for (int i = 0; i < ModelRenderer.ModelRendererPool.Count; i++)
                {
                    currentModelRenderer = ModelRenderer.ModelRendererPool.Elements[i];
                    if (currentModelRenderer.CachedModel != null && currentModelRenderer.Visible) // && currentModelRenderer.CachedLayerMask)
                    {
                        gbuffer.RenderModel(currentModelRenderer.cachedWorldMatrix, currentModelRenderer.CachedModel);
                    }
                }
            gbuffer.End();
            
            SpriteManager.DrawTextureToFullScreen(gbuffer.NormalTexture);
            */

            #endregion

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

            #region Scene Post Render Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.PostRenderTasks();
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