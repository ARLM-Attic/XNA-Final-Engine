
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
using Microsoft.Xna.Framework;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Helper for objects that want to leave particles behind them as they move around the world.
    /// This emitter implementation solves two related problems:
    /// 
    /// If an object wants to create particles very slowly, less than once per frame,
    /// it can be a pain to keep track of which updates ought to create a new particle versus which should not.
    /// 
    /// If an object is moving quickly and is creating many particles per frame, it will look ugly if these particles are all bunched up together.
    /// Much better if they can be spread out along a line between where the object is now and where it was on the previous frame.
    /// This is particularly important for leaving trails behind fast moving objects such as rockets.
    /// 
    /// This emitter class keeps track of a moving object, remembering its previous position so it can calculate the velocity of the object.
    /// It works out the perfect locations for creating particles at any frequency you specify,
    /// regardless of whether this is faster or slower than the game update rate.
    /// 
    /// The particles will born from a graphic object.
    /// </summary>
    public class ParticleEmitterFromObject : ParticleEmitter
    {

        #region Variables

        /// <summary>
        /// Previous position
        /// </summary>
        private Vector3 previousPosition;

        /// <summary>
        /// Velocity.
        /// </summary>
        private Vector3 velocity;

        /// <summary>
        /// Shared random number generator.
        /// </summary>
        private static readonly Random random = new Random();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new particle emitter object.
        /// The particles will born from a curve that is on a graphic object.
        /// </summary>
        public ParticleEmitterFromObject(Graphics.Object _object)
        {
            Object = _object;
            previousPosition = _object.WorldPosition;
        } // ParticleEmitter

        #endregion

        #region Update

        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles in the appropriate positions.
        /// </summary>
        internal override void Update()
        {
            velocity = (Object.WorldPosition - previousPosition) / (float)EngineManager.FrameTime;

            // Generate the particles. But the generating process uses AddParticle from this class.
            base.Update();

            // Update previous positions.
            previousPosition = Object.WorldPosition;
        } // Update
        
        /// <summary>
        /// Add particle.
        /// </summary>
        /// <param name="step">Step from last position to current position</param>
        protected override void AddParticle(float step)
        {
            Vector3 particlePosition = GetRandomPointFromBoundingBox(Object.BoundingBoxOptimized);
            // Add the particle.
            ParticleSystem.AddParticle(particlePosition, velocity);
        } // AddParticle

        private static Vector3 GetRandomPointFromBoundingBox(BoundingBox boundingBox)
        {
            return new Vector3(boundingBox.Min.X + (boundingBox.Max.X - boundingBox.Min.X) * (float) random.NextDouble(),
                               boundingBox.Min.Y + (boundingBox.Max.Y - boundingBox.Min.Y) * (float) random.NextDouble(),
                               boundingBox.Min.Z + (boundingBox.Max.Z - boundingBox.Min.Z) * (float) random.NextDouble());
        } // GetRandomPointFromBoundingBox

        #endregion

    } // ParticleEmitterFromObject
} // XNAFinalEngine.Graphics