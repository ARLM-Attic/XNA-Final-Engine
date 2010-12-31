
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
    /// 3D Sound tutorial.
    /// This tutorial plays five different sounds in five different locations.
    /// </summary>
    public class SceneTutorial3DSound01 : Scene
    {

        #region Variables

        /// <summary>
        /// The emitter objects.
        /// </summary>
        private GraphicObject gEmitterFrontLeft, gEmitterFrontRight, gEmitterFrontCenter, gEmitterRearLeft, gEmitterRearRight;

        /// <summary>
        /// The sound files
        /// </summary>
        private Sound sFrontLeft, sFrontRight, sFrontCenter, sRearLeft, sRearRight;

        /// <summary>
        /// Current sound instance.
        /// </summary>
        private SoundInstance soundInstance;

        /// <summary>
        /// Current sound file. 0 = front left, 1 front center, ...
        /// </summary>
        private int currentSoundFile = 0;

        #endregion

        #region Load

        /// <summary>
        /// Load the scene
        /// </summary>
        public override void Load()
        {
            // Set camera
            ApplicationLogic.Camera = new FixedCamera(new Vector3(0, 0, -10), Vector3.Zero);
            // Create emitter objects
            gEmitterFrontLeft = new GraphicObject(new Box(1), new Blinn(Color.Red));
            gEmitterFrontLeft.TranslateAbs(10, 0, 0);
            gEmitterFrontRight = new GraphicObject(new Box(1), new Blinn(Color.Blue));
            gEmitterFrontRight.TranslateAbs(-10, 0, 0);
            gEmitterFrontCenter = new GraphicObject(new Box(1), new Blinn());
            gEmitterFrontCenter.TranslateAbs(0, 0, 0);
            gEmitterRearLeft = new GraphicObject(new Box(1), new Blinn(Color.Yellow));
            gEmitterRearLeft.TranslateAbs(10, 0, -20);
            gEmitterRearRight = new GraphicObject(new Box(1), new Blinn(Color.Green));
            gEmitterRearRight.TranslateAbs(-10, 0, -20);
            // Load sounds
            sFrontLeft = new Sound("Tutorials\\FrontLeft");
            sFrontRight = new Sound("Tutorials\\FrontRight");
            sFrontCenter = new Sound("Tutorials\\FrontCenter");
            sRearLeft = new Sound("Tutorials\\RearLeft");
            sRearRight = new Sound("Tutorials\\RearRight");
            // Indicate to the sound manager the scene scale and master volume
            SoundManager.DistanceScale = 10;
            SoundManager.MasterSoundVolume = 1.0f;
        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public override void Update()
        {
            if (soundInstance == null)
            {
                switch (currentSoundFile)
                {
                    case 0:
                        soundInstance = sFrontLeft.Play(ApplicationLogic.Camera, gEmitterFrontLeft);
                        break;
                    case 1:
                        soundInstance = sFrontCenter.Play(ApplicationLogic.Camera, gEmitterFrontCenter);
                        break;
                    case 2:
                        soundInstance = sFrontRight.Play(ApplicationLogic.Camera, gEmitterFrontRight);
                        break;
                    case 3:
                        soundInstance = sRearRight.Play(ApplicationLogic.Camera, gEmitterRearRight);
                        break;
                    case 4:
                        soundInstance = sRearLeft.Play(ApplicationLogic.Camera, gEmitterRearLeft);
                        break;
                }
            }
            else if (soundInstance.IsOver)
            {
                currentSoundFile++;
                soundInstance = null;
            }
            if (currentSoundFile == 5) currentSoundFile = 0;
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public override void Render()
        {
            FontBattlefield18.RenderCentered("3D Sound Test", EngineManager.Height / 2, Color.White);
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

    } // SceneTutorial3DSound01
} // XNAFinalEngine.Scenes
