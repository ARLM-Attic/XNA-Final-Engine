
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
using XNAFinalEngine.GraphicElements;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
using XNAFinalEngine.Sounds;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.Scenes
{

    /// <summary>
    /// Video tutorial.
    /// This tutorial plays a video and when it finishes shows a centered text that tells us that the video is over.
    /// The maximum video size is 1280x720, the highest resolution of the XBOX 360.
    /// http://klucher.com/blog/video-support-in-xna-game-studio-3-1/
    /// http://blogs.msdn.com/b/ashtat/archive/2009/06/20/video-playback-in-xna-game-studio-3-1.aspx
    /// 
    /// Steps:
    /// 
    /// 1.	Define a video variable.
    /// 
    /// 2.	In the load method we need to load the video file and play it.
    /// 
    /// 3.	In the render method we need to render the current frame on screen. We can also stop, pause, resume, ask for its state, etc.
    /// 
    /// </summary>
    public class SceneTutorialVideo : Scene
    {

        #region Variables

        /// <summary>
        /// The video object.
        /// </summary>
        private Video logosVideo = null;

        #endregion

        #region Load

        /// <summary>
        /// Load the scene
        /// </summary>
        public override void Load()
        {
            logosVideo = new Video("LogosIntro");
            logosVideo.Play();
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
            if (logosVideo.State == Microsoft.Xna.Framework.Media.MediaState.Playing)
            {
                if (Keyboard.EscapeJustPressed)
                {
                    logosVideo.Stop();
                }
                else
                    logosVideo.Render();
            }
            else
            {   
                FontBattlefield18.RenderCentered("the video is over", EngineManager.Height / 2, Color.White);
                UIMousePointer.RenderMousePointer();
            }
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

    } // SceneTutorialVideo
} // XNAFinalEngine.Scenes
