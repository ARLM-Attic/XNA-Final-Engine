
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
    /// 3D Sound tutorial.
    /// This tutorial plays a single sound on an object in movement.
    /// </summary>
    public class SceneTutorial3DSound02 : Scene
    {

        #region Variables

        /// <summary>
        /// The emitter object.
        /// </summary>
        private GraphicObject gRotateObject;

        /// <summary>
        /// The sound file.
        /// </summary>
        private Sound soundNoice;

        /// <summary>
        /// Current sound instance.
        /// </summary>
        private SoundInstance soundInstance;

        #endregion

        #region Load

        /// <summary>
        /// Load the scene
        /// </summary>
        public override void Load()
        {
            ApplicationLogic.Camera = new FixedCamera(new Vector3(0, 0, -10), Vector3.Zero);
            //ApplicationLogic.Camera = new XSICamera(Vector3.Zero);

            gRotateObject = new GraphicObject(new GraphicElements.Plane(2, 2), new Constant("Tutorials\\Dog"));

            soundNoice = new Sound("Tutorials\\Dog");
            // Indicate to the sound manager the scene scale, master volume and master doppler scale.
            SoundManager.DistanceScale = 5;
            SoundManager.MasterSoundVolume = 1.0f;
            SoundManager.MasterDopplerScale = 1.0f;
        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public override void Update()
        {
            // Position
            float dx = (float)-Math.Cos(EngineManager.TotalTime);
            float dz = (float)-Math.Sin(EngineManager.TotalTime);
            gRotateObject.TranslateAbs(dx * 10, 0, dz * 10 -10);
            // Rotation
            Quaternion rotation;
            Vector3 position, scale;
            Matrix.CreateLookAt(new Vector3(gRotateObject.WorldPosition.X, gRotateObject.WorldPosition.Y, gRotateObject.WorldPosition.Z),
                                            new Vector3(0, 0, -10), Vector3.Up).Decompose(out scale, out rotation, out position);
            gRotateObject.RotateAbs(Matrix.Invert(Matrix.CreateFromQuaternion(rotation))); // To view matrix is inverse to the world and we need world coordinates.
            gRotateObject.RotateRel(0, 180, 0); // The plane is backward
            // Sound
            if (soundInstance == null)
            {
                soundInstance = soundNoice.Play(ApplicationLogic.Camera, gRotateObject, 1, 1);
            }
            else if (soundInstance.IsOver)
            {
                soundInstance = null;
            }
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public override void Render()
        {
            gRotateObject.Render();
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

    } // SceneTutorial3DSound02
} // XNAFinalEngine.Scenes
