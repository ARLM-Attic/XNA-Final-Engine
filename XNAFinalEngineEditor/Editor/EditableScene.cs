
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using Keyboard = XNAFinalEngine.Input.Keyboard;
#endregion

namespace XNAFinalEngine.Editor
{

    /// <summary>
    /// If you want to work with the XNA Final Engine Editor the main scene needs to inherit from this class.
    /// </summary>
    public abstract class EditableScene : Scene
    {

        #region Script

        /// <summary>
        /// This script is used so that the user does not need to call the base methods of this class.
        /// </summary>
        private class HiddenEditorUpdateScript : Script
        {

            private bool beginInEditorMode = true;

            /// <summary>
            /// Load the Editor Manager.
            /// </summary>
            public override void Start()
            {
                EditorManager.Initialize();
            } // Start

            /// <summary>
            /// Tasks executed during the update.
            /// This is the place to put the application logic.
            /// </summary>
            public override void Update()
            {
                if (beginInEditorMode)
                {
                    EditorManager.EnableEditorMode();
                    beginInEditorMode = false;
                }
                // Enable editor mode
                if (Keyboard.KeyJustPressed(Keys.E) && Keyboard.KeyPressed(Keys.LeftControl))
                {
                    if (EditorManager.EditorModeEnabled)
                        EditorManager.DisableEditorMode();
                    else
                        EditorManager.EnableEditorMode();
                }
            } // UpdateTasks

        } // HiddenEditorUpdateScript

        #endregion

        #region Variables

        // This script is used so that the user does not need to call the base methods of this class.
        private readonly GameObject3D editorHiddenUpdateScript;

        #endregion

        #region Constructor

        /// <summary>
        /// If you want to work with the XNA Final Engine Editor the main scene needs to inherit from this class.
        /// </summary>
        protected EditableScene()
        {
            // The engine is not initialized yet but it works.
            editorHiddenUpdateScript = new GameObject3D();
            editorHiddenUpdateScript.AddComponent<HiddenEditorUpdateScript>();
        } // EditableScene

        #endregion

    } // EditableScene
} // XNAFinalEngine.Editor
