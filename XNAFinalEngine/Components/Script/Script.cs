﻿
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
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Script component.
    /// </summary>
    public abstract class Script : Component
    {

        #region Properties

        /// <summary>
        /// Enabled scripts are Updated, disabled scripts are not.
        /// </summary>
        public bool Enabled { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            Enabled = true;
            Load();
        } // Initialize
        
        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            Unload();
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region Load

        /// <summary>
        /// Tasks executed during the creation of this component.
        /// </summary>
        public virtual void Load() { }

        #endregion

        #region Update

        /// <summary>
        /// Tasks executed during the first stage of the update.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Tasks executed during the last stage of the update.
        /// </summary>
        public virtual void LateUpdate() { }

        #endregion

        #region Render

        /// <summary>
        /// Tasks executed during the first stage of the scene render.
        /// </summary>
        public virtual void PreRenderUpdate() { }

        /// <summary>
        /// Tasks executed during the last stage of the scene render.
        /// </summary>
        public virtual void PostRenderUpdate() { }

        #endregion

        #region Unload

        /// <summary>
        /// Tasks executed during the unload of this component.
        /// </summary>
        public virtual void Unload() { }

        #endregion

        #region Script List

        // Script list.
        // A pool does not work with derived types, so a list has to be used.
        private static readonly List<Script> componentList = new List<Script>(50);

        /// <summary>
        /// Script list.
        /// </summary>
        internal static List<Script> ScriptList { get { return componentList; } }

        #endregion

    } // Script
} // XNAFinalEngine.Components