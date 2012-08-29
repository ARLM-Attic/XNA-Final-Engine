
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The application logic is stored in scenes. 
    /// </summary>
    /// <remarks>
    /// The scene code is executed in a very specific order:
    /// When the scene is loading:
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
    /// </remarks>
    public abstract class Scene : Disposable
    {

        #region Properties

        /// <summary>
        /// Indicates if the scene's resources were loaded.
        /// </summary>
        public bool ContentLoaded { get; private set; }

        /// <summary>
        /// Scene Content Manager.
        /// You can create and use another content manager.
        /// </summary>
        public ContentManager ContentManager { get; private set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Creates the scene content manager and loads the user content.
        /// </summary>
        internal void Initialize()
        {
            ContentManager = new ContentManager { Name = (GetType().Name + " Content Manager") };
            ContentManager.CurrentContentManager = ContentManager;
            LoadContent();
            ContentLoaded = true;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Unloads the user content and disposes the scene content manager.
        /// </summary>
        internal void Unitialize()
        {
            UnloadContent();
            ContentManager.Dispose();
            ContentLoaded = false;
        } // Unitialize

        #endregion

        #region Load Content

        /// <summary>
        /// Load the resources.
        /// </summary>
        public virtual void LoadContent() { }

        #endregion

        #region Begin Run

        /// <summary>
        /// Called after all components are initialized but before the first update in the game loop.
        /// The editor needs that you place the logic here.
        /// </summary>
        public virtual void BeginRun() { }

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update before scripts update.
        /// This is the place to put the application logic.
        /// </summary>
        public virtual void UpdateTasks() { }

        /// <summary>
        /// Tasks executed during the update, but after the scripts update.
        /// This is another place to put the application logic.
        /// </summary>
        public virtual void LateUpdateTasks() { }

        #endregion

        #region Render Tasks
        
        /// <summary>
        /// Tasks before the engine render.
        /// Some tasks are more related to the frame rendering than the update,
        /// or maybe the update frequency is too high to waste time in this kind of tasks,
        /// for that reason the pre render task exists.
        /// For example, is more correct to update the HUD information here because is related with the rendering.
        /// </summary>
        public virtual void PreRenderTasks() { }

        /// <summary>
        /// Tasks after the engine render.
        /// Probably you won’t need to place any task here.
        /// </summary>
        public virtual void PostRenderTasks() { }

        #endregion

        #region End Run

        /// <summary>
        /// Called after all components are initialized but before the first update in the game loop.
        /// The editor needs that you place the logic here.
        /// </summary>
        public virtual void EndRun() { }

        #endregion

        #region Unload Content

        /// <summary>
        /// Called when the scene resources need to be unloaded.
        /// Override this method to unload any game-specific resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        public virtual void UnloadContent() { }

        #endregion

        #region Reset

        /// <summary>
        /// The editor and the user can reset the scene.
        /// When this happens the system will execute:
        ///     * Reset.
        ///     * Begin Run.
        /// </summary>
        public virtual void Reset() { }

        #endregion

        #region Device Disposed

        /// <summary>
        /// Tasks executed during a device dispose condition.
        /// This is a very rare scenario in which the device is disposed in which the system will try to recreate everything to their last state.
        /// Normally the system will take care of everything, but sometimes you still can need to perform some tasks.
        /// Be aware that the sound will stop playing, music and videos will start over and custom textures and render targets can’t be recreated to their last values.
        /// </summary>
        public virtual void DeviceDisposed() { }

        #endregion

    } // Scene
} // XNAFinalEngine.EngineCore
