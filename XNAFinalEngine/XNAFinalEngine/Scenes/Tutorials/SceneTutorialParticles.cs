
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
using XNAFinalEngine.Particles;
#endregion

namespace XNAFinalEngine.Scenes
{

    /// <summary>
    /// Particles tutorial.
    /// The particle system is based in the XNA example: http://create.msdn.com/en-US/education/catalog/tutorial/particle_xml
    /// </summary>
    public class SceneTutorialParticles : Scene
    {

        #region Variables
        
        /// <summary>
        /// The sample can switch between three different visual effects.
        /// </summary>
        enum ParticleState
        {
            SmokePlume,
            RingOfFire,
        };

        /// <summary>
        /// Current particle state.
        /// </summary>
        ParticleState currentState = ParticleState.RingOfFire;
        
        private ParticleSystem smokePlumeParticles,
                               fireParticles;

        private Curve3D curve;

        private ContainerObject container;

        #endregion

        #region Load

        /// <summary>
        /// Load the scene.
        /// </summary>
        public override void Load()
        {
            ApplicationLogic.Camera = new XSICamera(Vector3.Zero, 500) { FarPlane = 1500 };

            smokePlumeParticles = new ParticleSystem("SmokePlume") { ParticleEmitter = new ParticleEmitterSimple() };   
            fireParticles = new ParticleSystem("Fire");

            curve = Curve3D.Circle(30, 25);
            container = new ContainerObject();
            
            fireParticles.ParticleEmitter = new ParticleEmitterFromCurve(curve, container);
            
            smokePlumeParticles.ParticleEmitter = new ParticleEmitterFromCurve(curve, container);
            
            EngineManager.ShowFPS = true;
        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public override void Update()
        {
            switch (currentState)
            {
                case ParticleState.SmokePlume:
                    smokePlumeParticles.ParticleEmitter.ParticleEmisionScale = 1f;
                    smokePlumeParticles.Update();
                    break;

                case ParticleState.RingOfFire:
                    fireParticles.Update();
                    smokePlumeParticles.ParticleEmitter.ParticleEmisionScale = 0.2f;
                    smokePlumeParticles.Update();
                    break;
            }
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public override void Render()
        {
            smokePlumeParticles.Render();
            fireParticles.Render();
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

    } // SceneTutorialParticles
} // XNAFinalEngine.Scenes
