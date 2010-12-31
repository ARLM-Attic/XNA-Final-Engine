
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
using Microsoft.Xna.Framework;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Scenes;
#endregion

namespace XNAFinalEngine
{
    /// <summary>
    /// The application logic starts here, and continues only in the scene namespace.
    /// Here we set the scene we want to load, in other words, we only need to change one line of code, “scene = new Scene();“.
    /// The active camera is in this class and can be changed any time we want it.
    /// </summary>
    public static class ApplicationLogic
    {

        #region Variables

         /// <summary>
        /// The active camera. It can be changed any time we want.
        /// Caution: any method that requires the current camera information will be automatically search this variable.
        /// </summary>
        private static Camera camera = new FixedCamera(new Vector3(0, 10, 20));

        /// <summary>
        /// The scene that the application will use.
        /// We promote to use scenes. It is better that this class (applicationLogic) remains mostly unchanged.
        /// </summary>
        private static Scene scene;

        #endregion

        #region Properties

        /// <summary>
        /// The active camera. It can be changed any time we want.
        /// Caution: any method that requires the current camera information will be automatically search this variable.
        /// </summary>
        public static Camera Camera { get { return camera; } set { camera = value; } }

        #endregion

        #region Create Scene

        /// <summary>
        /// Init the aplication logic.
        /// </summary>
        public static void CreateScene()
        {
            //scene = new SceneTutorialMusic();
            //scene = new SceneTutorial3DSound01();
            //scene = new SceneTutorial3DSound02();
            //scene = new SceneTutorialCurves();
            //scene = new SceneTutorialVideo();
            //scene = new SceneTutorialParticles();
            //scene = new SceneTutorialTexturePicker();
            scene = new SceneTutorialNewUI();
            //scene = new SceneDeferredLighting();
            //scene = new SceneEmpty();
            scene.Load();
        } // CreateScene

        #endregion

        #region Update

        /// <summary>
        /// Update aplication. First update the camera then the scene.
        /// </summary>
        public static void Update()
        {
            if (camera != null)
                camera.Update();
            if (scene != null)
                scene.Update();
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render aplication
        ///  </summary>
        public static void Render()
        {
            if (scene != null)
                scene.Render();
        } // Render

        #endregion

        #region Unload Content

        /// <summary>
        /// Unload the content that it isn't unloaded automatically when the application finishes.
        /// </summary>
        public static void UnloadContent()
        {
            if (scene != null)
                scene.UnloadContent();
        } // UnloadContent

        #endregion

    } // ApplicationLogic
} // XNAFinalEngine
