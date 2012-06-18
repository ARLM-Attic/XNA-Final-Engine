
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
using Microsoft.Xna.Framework;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary> 
    /// This emitter implementation solves two related problems:
    /// 
    /// If an object wants to create particles very slowly, less than once per frame,
    /// it can be a pain to keep track of which updates ought to create a new particle versus which should not.
    /// 
    /// If an object is moving quickly and is creating many particles per frame, it will look ugly if these particles are all bunched up together.
    /// Much better if they can be spread out along a line between where the object is now and where it was on the previous frame.
    /// This is particularly important for leaving trails behind fast moving objects such as rockets.
    /// </summary>
    public class ParticleEmitter : Component
    {

        #region Variables

        // Maximum number of particles that can be displayed at one time.
        private int maximumNumberParticles;
                        
        // Time left over from the previous frame.
        protected float timeLeftOver;

        // A value between 0 and 1 means fewer particles, and a value greater than 1 indicates more particles.
        private float particleEmisionScale = 1;

        // Time between particles.
        private float timeBetweenParticles;

        // Previous and current position.
        private Vector3 previousPosition, currentPosition;

        // Particle velocity.
        private Vector3 velocity;

        // Particle duration.
        private float duration;

        #endregion

        #region Properties
        
        /// <summary>
        /// The "vertex buffer" of the particle system.
        /// </summary>
        internal ParticleSystem ParticleSystem { get; set; }

        /// <summary>
        /// Duration. How long these particles will last.
        /// </summary>
        public float Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                TimeBetweenParticles = 1.0f / (MaximumNumberParticles / Duration);
            }
        } // Duration

        /// <summary>
        /// Controls how much particles are influenced by the velocity of the object which created them.
        /// You can see this in action with the explosion effect, where the flames continue to move in the same direction as the source projectile.
        /// The projectile trail particles, on the other hand, set this value very low so they are less affected by the velocity of the projectile.
        /// </summary>
        public float EmitterVelocitySensitivity { get; set; }

        /// <summary>
        /// Minimum horizontal velocity. Range of values controlling how much X and Z axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MinimumHorizontalVelocity { get; set; }

        /// <summary>
        /// Maximum horizontal velocity. Range of values controlling how much X and Z axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MaximumHorizontalVelocity { get; set; }

        /// <summary>
        /// Minimum vertical velocity. Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MinimumVerticalVelocity { get; set; }

        /// <summary>
        /// Maximum vertical velocity. Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MaximumVerticalVelocity { get; set; }
        
        /// <summary>
        /// Maximum number of particles that can be displayed at one time.
        /// </summary>
        public int MaximumNumberParticles
        {
            get { return maximumNumberParticles; }
            set
            {
                if (value > 0)
                {
                    maximumNumberParticles = value;
                    if (ParticleSystem != null)
                        ParticleSystem.Dispose();
                    ParticleSystem = new ParticleSystem(value);
                    TimeBetweenParticles = 1.0f / (MaximumNumberParticles / Duration);
                }
            }
        } // MaximumNumberParticles

        /// <summary>
        /// Time between particles.
        /// </summary>
        protected float TimeBetweenParticles
        {
            get { return timeBetweenParticles / ParticleEmisionScale; }
            set { timeBetweenParticles = value; }
        } // TimeBetweenParticles

        /// <summary>
        /// A value between 0 and 1 means fewer particles, and a value greater than 1 indicates more particles.
        /// </summary>
        public float ParticleEmisionScale
        {
            get { return particleEmisionScale; }
            set
            {
                if (value > 0)
                    particleEmisionScale = value;
            }
        } // ParticleEmisionScale

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Default values.
            MaximumNumberParticles = 400;
            Duration = 10;
            EmitterVelocitySensitivity = 1;
            MinimumHorizontalVelocity = 0;
            MaximumHorizontalVelocity = 15;
            MinimumVerticalVelocity = 10;
            MaximumVerticalVelocity = 20;
            previousPosition = ((GameObject3D)Owner).Transform.Position;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        internal virtual void Update()
        {
            ParticleSystem.Update(Duration);

            Vector3 lastPosition = currentPosition;
            currentPosition = ((GameObject3D)Owner).Transform.Position; // Cache this one. TODO
            velocity = (currentPosition - lastPosition) / Time.FrameTime;
            
            if (Time.FrameTime > 0)
            {
                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + Time.FrameTime;

                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > TimeBetweenParticles)
                {
                    currentTime += TimeBetweenParticles;
                    timeToSpend -= TimeBetweenParticles;

                    // Work out the optimal position for this particle.
                    // This will produce evenly spaced particles regardless of the object speed, particle creation frequency, or game update rate.
                    float step = currentTime / Time.FrameTime;

                    // Create the particle.
                    AddParticle(step);
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }

            // Update previous positions.
            previousPosition = currentPosition;
        } // Update

        /// <summary>
        /// Add particle.
        /// </summary>
        /// <param name="step">Step from last position to current position</param>
        protected virtual void AddParticle(float step)
        {
            ParticleSystem.AddParticle(Vector3.Lerp(previousPosition, currentPosition, step), velocity, EmitterVelocitySensitivity,
                                       MinimumHorizontalVelocity, MaximumHorizontalVelocity, MinimumVerticalVelocity, MaximumVerticalVelocity);
        } // AddParticle

        #endregion

        #region Pool
        
        // Pool for this type of components.
        private static readonly Pool<ParticleEmitter> componentPool = new Pool<ParticleEmitter>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<ParticleEmitter> ComponentPool { get { return componentPool; } }

        #endregion

    } // ParticleEmitter
} // XNAFinalEngine.Components
