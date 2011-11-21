
#region Using directives
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngineContentPipelineExtensionRuntime.Particles;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Particle.
    /// </summary>
    public class Particle : Asset
    {

        #region Variables

        /// <summary>
        /// Indicates is the particle system is soft or hard.
        /// In a soft particle system the closer the sprite’s fragment is to the scene, the more it fades out to show more of the background.
        /// Also, if the sprite is behind the background scene then the fragment is discard entirely.
        /// You can see the Shader X7 book or NVIDIA soft particles article for more information about soft particles.
        /// </summary>
        private bool softParticles = true;

        /// <summary>
        /// This value affects how much the fragment particle fade out when a scene object is close.
        /// Soft particles property has to be set to true.
        /// </summary>
        private float fadeDistance = 0.1f;

        /// <summary>
        /// Duration. How long these particles will last.
        /// </summary>
        private float duration = 10;

        /// <summary>
        /// Duration Randomness. If greater than zero, some particles will last a shorter time than others.
        /// </summary>
        private float durationRandomness;

        /// <summary>
        /// Direction and strength of the gravity effect. This can point in any direction, not just down!
        /// The fire effect points it upward to make the flames rise, and the smoke plume points it sideways to simulate wind.
        /// </summary>
        private Vector3 gravity = new Vector3(-20, -5, 0);

        /// <summary>
        /// Minimum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Color minimumColor = new Color(255, 255, 255, 255);

        /// <summary>
        /// Maximum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Color maximumColor = new Color(255, 255, 255, 255);

        /// <summary>
        /// Rotate Speed. Range of values controlling how fast the particles rotate.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Vector2 rotateSpeed = new Vector2(-1, 1);

        /// <summary>
        /// Start Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Vector2 startSize = new Vector2(5, 10);

        /// <summary>
        /// End Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Vector2 endSize = new Vector2(50, 200);

        /// <summary>
        /// Maximum number of particles that can be displayed at one time.
        /// </summary>
        private int maximumNumberParticles = 600;

        /// <summary>
        /// Alpha blending state. 
        /// </summary>
        private BlendState blendState = BlendState.NonPremultiplied;

        /// <summary>
        /// Controls how much particles are influenced by the velocity of the object which created them.
        /// You can see this in action with the explosion effect, where the flames continue to move in the same direction as the source projectile.
        /// The projectile trail particles, on the other hand, set this value very low so they are less affected by the velocity of the projectile.
        /// </summary>
        private float emitterVelocitySensitivity = 1;

        /// <summary>
        /// Minimum horizontal velocity. Range of values controlling how much X and Z axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private float minimumHorizontalVelocity;

        /// <summary>
        /// Maximum horizontal velocity. Range of values controlling how much X and Z axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private float maximumHorizontalVelocity = 15;

        /// <summary>
        /// Minimum vertical velocity. Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private float minimumVerticalVelocity = 10;

        /// <summary>
        /// Maximum vertical velocity. Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private float maximumVerticalVelocity = 20;

        #endregion

        #region Properties

        /// <summary>
        /// Indicates is the particle system is soft or hard.
        /// In a soft particle system the closer the sprite’s fragment is to the scene, the more it fades out to show more of the background.
        /// Also, if the sprite is behind the background scene then the fragment is discard entirely.
        /// You can see the Shader X7 book or NVIDIA soft particles article for more information about soft particles.
        /// </summary>
        public bool SoftParticles
        {
            get { return softParticles; }
            set { softParticles = value; }
        } // SoftParticles

        /// <summary>
        /// This value affects how much the fragment particle fade out when a scene object is close.
        /// Soft particles property has to be set to true.
        /// </summary>
        public float FadeDistance
        {
            get { return fadeDistance; }
            set { fadeDistance = value; }
        } // FadeDistance

        /// <summary>
        /// Duration. How long these particles will last.
        /// </summary>
        public float Duration
        {
            get { return duration; }
            set { duration = value; }
        } // Duration

        /// <summary>
        /// Duration Randomness. If greater than zero, some particles will last a shorter time than others.
        /// </summary>
        public float DurationRandomness
        {
            get { return durationRandomness; }
            set { durationRandomness = value; }
        } // DurationRandomness

        /// <summary>
        /// Direction and strength of the gravity effect. This can point in any direction, not just down!
        /// The fire effect points it upward to make the flames rise, and the smoke plume points it sideways to simulate wind.
        /// </summary>
        public Vector3 Gravity
        {
            get { return gravity; }
            set { gravity = value; }
        } // Gravity

        /// <summary>
        /// Controls how the particle velocity will change over their lifetime. If set to 1, particles will keep going at the same speed as when they were created.
        /// If set to 0, particles will come to a complete stop right before they die. Values greater than 1 make the particles speed up over time.
        /// </summary>
        private float endVelocity = 0.75f;
        /// <summary>
        /// Controls how the particle velocity will change over their lifetime. If set to 1, particles will keep going at the same speed as when they were created.
        /// If set to 0, particles will come to a complete stop right before they die. Values greater than 1 make the particles speed up over time.
        /// </summary>
        public float EndVelocity
        {
            get { return endVelocity; }
            set { endVelocity = value; }
        } // EndVelocity

        /// <summary>
        /// Minimum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Color MinimumColor
        {
            get { return minimumColor; }
            set { minimumColor = value; }
        } // MinimumColor

        /// <summary>
        /// Maximum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Color MaximumColor
        {
            get { return maximumColor; }
            set { maximumColor = value; }
        } // MaximumColor

        /// <summary>
        /// Rotate Speed. Range of values controlling how fast the particles rotate.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Vector2 RotateSpeed
        {
            get { return rotateSpeed; }
            set { rotateSpeed = value; }
        } // RotateSpeed

        /// <summary>
        /// Start Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Vector2 StartSize
        {
            get { return startSize; }
            set { startSize = value; }
        } // StartSize

        /// <summary>
        /// End Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Vector2 EndSize
        {
            get { return endSize; }
            set { endSize = value; }
        } // EndSize

        /// <summary>
        /// Ttexture.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// Controls how much particles are influenced by the velocity of the object which created them.
        /// You can see this in action with the explosion effect, where the flames continue to move in the same direction as the source projectile.
        /// The projectile trail particles, on the other hand, set this value very low so they are less affected by the velocity of the projectile.
        /// </summary>
        public float EmitterVelocitySensitivity
        {
            get { return emitterVelocitySensitivity; }
            set { emitterVelocitySensitivity = value; }
        } // EmitterVelocitySensitivity

        /// <summary>
        /// Minimum horizontal velocity. Range of values controlling how much X and Z axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MinimumHorizontalVelocity
        {
            get { return minimumHorizontalVelocity; }
            set { minimumHorizontalVelocity = value; }
        } // MinimumHorizontalVelocity

        /// <summary>
        /// Maximum horizontal velocity. Range of values controlling how much X and Z axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MaximumHorizontalVelocity
        {
            get { return maximumHorizontalVelocity; }
            set { maximumHorizontalVelocity = value; }
        } // MaximumHorizontalVelocity

        /// <summary>
        /// Minimum vertical velocity. Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MinimumVerticalVelocity
        {
            get { return minimumVerticalVelocity; }
            set { minimumVerticalVelocity = value; }
        } // MinimumVerticalVelocity

        /// <summary>
        /// Maximum vertical velocity. Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public float MaximumVerticalVelocity
        {
            get { return maximumVerticalVelocity; }
            set { maximumVerticalVelocity = value; }
        } // MaximumVerticalVelocity

        /// <summary>
        /// Maximum number of particles that can be displayed at one time.
        /// </summary>
        public int MaximumNumberParticles
        {
            get { return maximumNumberParticles; }
            set { maximumNumberParticles = value; }
        } // MaximumNumberParticles

        /// <summary>
        /// Alpha blending state. 
        /// </summary>
        public BlendState BlendState
        {
            get { return blendState; }
            set { blendState = value; }
        } // BlendState

        #endregion
        
        #region Constructor

        /// <summary>
        /// Particle.
        /// The settings are loaded from file.
        /// </summary>
        public Particle(string filename)
        {
            Name = filename;
            string fullFilename = ContentManager.GameDataDirectory + "Particles\\" + filename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load particle: File " + fullFilename + " does not exists!");
            }
            try
            {
                // Remember user content manager.
                ContentManager userContentManager = ContentManager.CurrentContentManager;
                // Create a temporal content manager.
                ContentManager.CurrentContentManager = new ContentManager();
                // Load settings from file.
                ParticleSettings settings = ContentManager.CurrentContentManager.XnaContentManager.Load<ParticleSettings>(fullFilename);
                // Set settings into properties.
                Duration = (float)settings.Duration.TotalSeconds;
                DurationRandomness = settings.DurationRandomness;
                Gravity = settings.Gravity;
                EndVelocity = settings.EndVelocity;
                MinimumColor = settings.MinColor;
                MaximumColor = settings.MaxColor;
                RotateSpeed = new Vector2(settings.MinRotateSpeed, settings.MaxRotateSpeed);
                StartSize = new Vector2(settings.MinStartSize, settings.MaxStartSize);
                EndSize = new Vector2(settings.MinEndSize, settings.MaxEndSize);
                MaximumNumberParticles = settings.MaxParticles;
                BlendState = settings.BlendState;
                EmitterVelocitySensitivity = settings.EmitterVelocitySensitivity;
                MinimumHorizontalVelocity = settings.MinHorizontalVelocity;
                MaximumHorizontalVelocity = settings.MaxHorizontalVelocity;
                MinimumVerticalVelocity = settings.MinVerticalVelocity;
                MaximumVerticalVelocity = settings.MaxVerticalVelocity;
                // Remember texture name before destroy the variables.
                fullFilename = "Particles\\" + settings.TextureName;
                ContentManager.CurrentContentManager.Unload();
                ContentManager.CurrentContentManager = userContentManager;
                Texture = new Texture(fullFilename);
            }
            catch (ObjectDisposedException e)
            {
                throw new Exception("Content Manager: Content manager disposed", e);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load particle: " + filename, e);
            }
        } // Particle

        /// <summary>
        /// Particle. 
        /// The particle settings need to be loaded through properties.
        /// </summary>
        public Particle() { }

        #endregion

    } // Particle
} // XNAFinalEngine.Assets
