
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
using XNAFinalEngine.Sounds;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.Scenes
{

    /// <summary>
    /// Empty scene. It shows a black screen and the mouse cursor. This class is the template for the new scenes.
    /// </summary>
    public class SceneEmpty : Scene
    {

        #region Variables

        #region Cameras
        
        #endregion

        #region Lights

        #endregion

        #region Objects

        #endregion

        #region Materials

        #endregion

        #region PrePostScreenShaders

        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Load the scene
        /// </summary>
        public override void Load()
        {
            EngineManager.ShowFPS = true;
        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public override void Update()
        {
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public override void Render()
        {
            MousePointer.RenderMousePointer();
        } // Render

        #endregion

        #region UnloadContent

        /// <summary>
        /// Unload the content that it isn't unloaded automatically when the scene is over.
        /// </summary>
        public override void UnloadContent()
        {

        } // UnloadContent

        #endregion

    } // SceneEmpty
} // XNAFinalEngine.Scenes
