
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
using XNAFinalEngine.Components;
using XNAFinalEngine.Editor;
using XNAFinalEngine.UserInterface;
#endregion

namespace XNAFinalEngine.Scenes
{

    /// <summary>
    /// Base class for scenes.
    /// Here will be the application logic.
    /// </summary>
    public abstract class EditableScene : Scene
    {

        #region Variables

        private GameObject3D camera;

        #endregion

        #region Properties

        /// <summary>
        /// TODO: La pongo aca o la main camara se destaca en otro lado? Como lo hice en el pasado. Ya se vera.
        /// </summary>
        public GameObject3D MainCamera
        {
            get { return camera; }
            set
            {
                if (value != null && value.Camera == null)
                    throw new ArgumentException("Scene: the camera object does not have a camera component attached.", "value");
                camera = value;

            }
        } // MainCamera

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method at the end.</remarks>
        public override void Load()
        {
            EditorManager.Initialize();
            UserInterfaceManager.Initialize();
            base.Load();
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update before scripts update.
        /// This is the place to put the application logic.
        /// </summary>
        public virtual void UpdateTasks()
        {
            // Overrite it!!
        } // UpdateTasks

        /// <summary>
        /// Tasks executed during the update, but after the scripts update.
        /// This is the place to put the application logic.
        /// </summary>
        public virtual void LateUpdateTasks()
        {
            // Overrite it!!
        } // LateUpdateTasks

        #endregion

        #region Render Tasks
        
        /// <summary>
        /// Tasks before the engine render.
        /// Some tasks are more related to the frame rendering than the update,
        /// or maybe the update frequency is too high to waste time in this kind of tasks,
        /// for that reason the pre render task exists.
        /// For example, is more correct to update the HUD information here because is related with the rendering.
        /// </summary>
        public virtual void PreRenderTasks()
        {
            // Overrite it!!
        } // PreRenderTasks

        /// <summary>
        /// Tasks after the engine render.
        /// Probably you won’t need to place any task here.
        /// </summary>
        public virtual void PostRenderTasks()
        {
            // Overrite it!!
        } // PostRenderTasks

        #endregion

    } // Scene
} // XNAFinalEngine.Scenes
