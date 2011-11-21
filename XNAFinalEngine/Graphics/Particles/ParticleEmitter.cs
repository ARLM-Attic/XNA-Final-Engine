
#region License
/*

 Based in the class ParticleEmitter.cs from Microsoft XNA Community
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using XNAFinalEngine.EngineCore;
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
    /// </summary>
    public abstract class ParticleEmitter
    {

        #region Variables
       
        /// <summary>
        /// Time left over from the previous frame.
        /// </summary>
        protected float timeLeftOver;

        /// <summary>
        /// A value between 0 and 1 means fewer particles, and a value greater than 1 indicates more particles.
        /// </summary>
        private float particleEmisionScale = 1;
        
        /// <summary>
        /// Time between particles.
        /// </summary>
        private float timeBetweenParticles;

        #endregion

        #region Properties

        /// <summary>
        /// The particle system that uses this emitter.
        /// </summary>
        internal ParticleSystem ParticleSystem { get; set; }

        /// <summary>
        /// Time between particles.
        /// </summary>
        internal float TimeBetweenParticles
        {
            get
            {
                if (ParticleEmisionScale == 0) { timeBetweenParticles = float.MaxValue;  }
                return timeBetweenParticles / ParticleEmisionScale;
            }
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

        #region Update

        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles in the appropriate positions.
        /// </summary>
        internal virtual void Update()
        {   
            if (EngineManager.FrameTime > 0)
            {
                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + (float)EngineManager.FrameTime;
                
                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > TimeBetweenParticles)
                {
                    currentTime += TimeBetweenParticles;
                    timeToSpend -= TimeBetweenParticles;

                    // Work out the optimal position for this particle.
                    // This will produce evenly spaced particles regardless of the object speed, particle creation frequency, or game update rate.
                    float step = currentTime / (float)EngineManager.FrameTime;

                    // Create the particle.
                    AddParticle(step);
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }
        } // Update

        /// <summary>
        /// Add particle.
        /// </summary>
        /// <param name="step">Step from last position to current position</param>
        protected virtual void AddParticle(float step)
        {
            throw new Exception();
        } // AddParticle

        #endregion

    } // ParticleEmitter
} // XNAFinalEngine.Graphics