
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The application logic is stored in scenes. 
    /// </summary>
    /// <remarks>
    /// The scene code is executed in a very specific order:
    /// When the scene is created:
    ///     * Load Content: load the resources
    ///     * Begin Run (the editor will call this method again when you play a stopped scene or when the user reset the scene).
    /// In each update:
    ///     * UpdateTasks
    ///     * LateUpdateTasks
    /// In each frame draw:
    ///     * PreRenderTasks
    ///     * PostRenderTasks
    /// When the scene is finishing its execution or when the application is closing:
    ///     * EndRun
    ///     * Unload Content.
    /// In case the device was disposed (very rare):
    ///     *  DeviceDisposed
    /// Also, the editor and the user can reset the scene, when this happens the system will execute:
    ///     * Reset.
    ///     * Begin Run.
    /// Dispose:
    ///     * Unload Content.
    /// </remarks>
    public abstract class Scene : Disposable
    {

        #region Properties

        /// <summary>
        /// Current Scenes.
        /// </summary>
        public static List<Scene> CurrentScenes { get; private set; }

        /// <summary>
        /// Indicates if the scene's resources were loaded.
        /// </summary>
        public bool ContentLoaded { get; private set; }

        /// <summary>
        /// Scene Content Manager.
        /// You can create and use another content manager.
        /// </summary>
        public AssetContentManager AssetContentManager { get; private set; }

        /// <summary>
        /// Scene Content Manager.
        /// You can create and use another content manager.
        /// </summary>
        public GameObjectContentManager GameObjectContentManager { get; private set; }

        #endregion

        #region Constructors

        static Scene()
        {
            CurrentScenes = new List<Scene>(1);
        } // Scene

        protected Scene()
        {
            CurrentScenes.Add(this);
        } // Scene

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Unitialize();
            CurrentScenes.Remove(this);
        } // DisposeManagedResources

        #endregion

        #region Initialize

        /// <summary>
        /// Creates the scene content manager and loads the user content.
        /// </summary>
        internal void Initialize()
        {
            AssetContentManager = new AssetContentManager { Name = (GetType().Name + " Content Manager") };
            AssetContentManager.CurrentContentManager = AssetContentManager;
            GameObjectContentManager = new GameObjectContentManager { Name = (GetType().Name + " Content Manager") };
            GameObjectContentManager.CurrentContentManager = GameObjectContentManager;
            LoadContent();
            ContentLoaded = true;
            // A collection of all generations could be a good idea at this point.
            // Besides the used managed memory indicates rational values when this is executed here.
            GarbageCollector.CollectGarbage();
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Unloads the user content and disposes the scene content manager.
        /// </summary>
        internal void Unitialize()
        {
            if (ContentLoaded)
            {
                EndRun();
                UnloadContent();
                if (AssetContentManager.CurrentContentManager == AssetContentManager)
                    AssetContentManager.CurrentContentManager = null;
                if (GameObjectContentManager.CurrentContentManager == GameObjectContentManager)
                    GameObjectContentManager.CurrentContentManager = null;
                AssetContentManager.Dispose();
                GameObjectContentManager.Dispose();
                ContentLoaded = false;
            }
        } // Unitialize

        #endregion

        #region Load Content

        /// <summary>
        /// Load the resources.
        /// </summary>
        protected virtual void LoadContent() { }

        #endregion

        #region Begin Run

        /// <summary>
        /// Called after all components are initialized but before the first update in the game loop.
        /// The editor needs that you place the logic here.
        /// </summary>
        protected virtual void BeginRun() { }

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update before scripts update.
        /// This is the place to put the application logic.
        /// </summary>
        protected virtual void UpdateTasks() { }

        /// <summary>
        /// Tasks executed during the update, but after the scripts update.
        /// This is another place to put the application logic.
        /// </summary>
        protected virtual void LateUpdateTasks() { }

        #endregion

        #region Render Tasks
        
        /// <summary>
        /// Tasks before the engine render.
        /// Some tasks are more related to the frame rendering than the update,
        /// or maybe the update frequency is too high to waste time in this kind of tasks,
        /// for that reason the pre render task exists.
        /// For example, is more correct to update the HUD information here because is related with the rendering.
        /// </summary>
        protected virtual void PreRenderTasks() { }

        /// <summary>
        /// Tasks after the engine render.
        /// Probably you won’t need to place any task here.
        /// </summary>
        protected virtual void PostRenderTasks() { }

        #endregion

        #region End Run

        /// <summary>
        /// Called when the scene is beign destroyed or when the application is exiting.
        /// </summary>
        protected virtual void EndRun() { }

        #endregion

        #region Unload Content

        /// <summary>
        /// Called when the scene resources need to be unloaded.
        /// Override this method to unload any game-specific resources.
        /// But remember that every asset load in the scene asset content manager
        /// and every game object load in the scene game object content manager
        /// will be disposed automatically.
        /// </summary>
        protected virtual void UnloadContent() { }

        #endregion

        #region Reset

        /// <summary>
        /// The editor or the user can reset the scene.
        /// When this happens the system will execute:
        ///     * Resetting.
        ///     * BeginRun.
        /// </summary>
        public void Reset()
        {
            Reseting();
            BeginRun();
        } // Reset

        /// <summary>
        /// When the scene is beign reset and before BeginRun is executed.
        /// </summary>
        protected virtual void Reseting()
        {
            
        } // Reset

        #endregion

        #region Device Disposed

        /// <summary>
        /// Tasks executed during a device dispose condition.
        /// This is a very rare scenario in which the device is disposed in which the system will try to recreate everything to their last state.
        /// Normally the system will take care of everything, but sometimes you still can need to perform some tasks.
        /// Be aware that the sound will stop playing, music and videos will start over and custom textures and render targets can’t be recreated to their last values.
        /// </summary>
        protected virtual void DeviceDisposed() { }

        #endregion

        #region GameLoop

        // With the purpose to disable access to the scene execution from the user I have to do this...

        internal void BeginRunFromGameLoop()
        {
            BeginRun();
        }

        internal void UpdateTasksFromGameLoop()
        {
            UpdateTasks();
        }

        internal void LateUpdateTasksFromGameLoop()
        {
            LateUpdateTasks();
        }

        internal void PreRenderTasksFromGameLoop()
        {
            PreRenderTasks();
        }

        internal void PostRenderTasksFromGameLoop()
        {
            PostRenderTasks();
        }

        internal void DeviceDisposedFromGameLoop()
        {
            DeviceDisposed();
        }

        #endregion
        
    } // Scene
} // XNAFinalEngine.EngineCore
