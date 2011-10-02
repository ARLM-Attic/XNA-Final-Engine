
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
using XNAFinalEngine.Components;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
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

        #endregion

        #region Load Content

        /// <summary>
        /// Load Content.
        /// </summary>
        internal static void LoadContent()
        {
            // Create the 32 layers.
            Layer.InitLayers();
            SpriteManager.Init();

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

            #region Scene Pre Render Tasks

            if (CurrentScene != null && CurrentScene.Loaded)
            {
                CurrentScene.PreRenderTasks();
            }

            #endregion

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

        } // UnloadContent

        #endregion

    } // GameLoop
} // XNAFinalEngine.EngineCore