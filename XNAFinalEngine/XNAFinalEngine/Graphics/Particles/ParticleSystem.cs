
#region License
/*

 Based in the class ParticleSystem.cs from Microsoft XNA Community
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Particles
{

    /// <summary>
    /// Particle System.
    /// </summary>
    public class ParticleSystem
    {

        #region Variables
      
        /// <summary>
        /// Settings class controls the appearance and animation of this particle system.
        /// </summary>
        private readonly ParticleSettings settings;

        /// <summary>
        /// Count how many times Draw has been called. This is used to know when it is safe to retire old particles back into the free list. 
        /// </summary>
        private int drawCounter;

        /// <summary>
        /// Particle emitter. Can be null.
        /// </summary>
        private ParticleEmitter emitter;

        /// <summary>
        /// Shared random number generator.
        /// </summary>
        private static readonly Random random = new Random();

        #region Parameters

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

        #region Data Structure

        /// <summary>
        /// An array of particles, treated as a circular queue.
        /// </summary>
        private ParticleVertex[] particles;
                        
        /// <summary>
        /// A vertex buffer holding our particles. This contains the same data as
        /// the particles array, but copied across to where the GPU can access it.
        /// </summary>
        private DynamicVertexBuffer vertexBuffer;

        /// <summary>
        /// Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        /// </summary>
        private IndexBuffer indexBuffer;

        // The particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // particles last for the same amount of time, old particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2 
        //      3 - first free particle
        //
        // In this case, particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2 
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste time waiting around every time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in total, our queue contains four different regions:
        //
        // Vertices between firstActiveParticle and firstNewParticle are actively
        // being drawn, and exist in both the managed particles array and the GPU
        // vertex buffer.
        //
        // Vertices between firstNewParticle and firstFreeParticle are newly created,
        // and exist only in the managed particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between firstFreeParticle and firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between firstRetiredParticle and firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.
        private int firstActiveParticle;
        private int firstNewParticle;
        private int firstFreeParticle;
        private int firstRetiredParticle;

        #endregion

        #endregion

        #region Properties

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

        /// <summary>
        /// Particle emitter. Can be null. If it's null then you need to add the particles manually.
        /// </summary>
        public ParticleEmitter ParticleEmitter
        {
            get { return emitter; }
            set
            {
                emitter = value;
                emitter.ParticleSystem = this;
                emitter.TimeBetweenParticles = 1.0f / ((float)MaximumNumberParticles / Duration);
            }
        } // ParticleEmitter

        #endregion

        #region Shader Parameters

        #region Variables

        /// <summary>
        /// Custom effect for drawing particles. This computes the particle
        /// animation entirely in the vertex shader: no per-particle CPU work required!
        /// </summary>
        private Effect effect;

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        private static EffectParameter
                               // Matrices //
                               epView,
                               epProjection,
                               // Surface //   
                               epViewportScale,
                               epCurrentTime,
                               epDuration,
                               epDurationRandomness,
                               epGravity,
                               epEndVelocity,
                               epMinColor,
                               epMaxColor,
                               epRotateSpeed,
                               epStartSize,
                               epEndSize,
                               // Textures //
                               epTexture;

        #endregion

        #region Matrices

        /// <summary>
        /// Last used view matrix
        /// </summary>
        private static Matrix? lastUsedViewMatrix;
        /// <summary>
        /// Set view matrix
        /// </summary>
        private static Matrix ViewMatrix
        {
            set
            {
                if (lastUsedViewMatrix != value)
                {
                    lastUsedViewMatrix = value;
                    epView.SetValue(value);
                }
            }
        } // ViewMatrix

        /// <summary>
        /// Last used projection matrix
        /// </summary>
        private static Matrix? lastUsedProjectionMatrix;
        /// <summary>
        /// Set projection matrix
        /// </summary>
        private static Matrix ProjectionMatrix
        {
            set
            {
                if (lastUsedProjectionMatrix != value)
                {
                    lastUsedProjectionMatrix = value;
                    epProjection.SetValue(value);
                }
            }
        } // ProjectionMatrix

        #endregion

        #region Viewport Scale

        /// <summary>
        /// Last used viewport scale.
        /// </summary>
        private static Vector2? lastUsedViewportScale;
        /// <summary>
        /// Set viewport scale.
        /// </summary>
        private static void SetViewportScale(Vector2 _viewportScale)
        {
            if (lastUsedViewportScale != _viewportScale)
            {
                lastUsedViewportScale = _viewportScale;
                epViewportScale.SetValue(_viewportScale);
            }
        } // SetViewportScale

        #endregion

        #region Current Time

        /// <summary>
        /// Current Time (in seconds)
        /// </summary>
        private float currentTime;
        /// <summary>
        /// Current Time (in seconds)
        /// </summary>
        private float CurrentTime
        {
            get { return currentTime; }
            set { currentTime = value; }
        } // CurrentTime

        /// <summary>
        /// Last used current time
        /// </summary>
        private static float? lastUsedCurrentTime;
        /// <summary>
        /// Set current time (valor mayor igual a 0)
        /// </summary>
        private static void SetCurrentTime(float _currentTime)
        {
            if (lastUsedCurrentTime != _currentTime && _currentTime >= 0.0f)
            {
                lastUsedCurrentTime = _currentTime;
                epCurrentTime.SetValue(_currentTime);
            }
        } // SetCurrentTime

        #endregion

        #region Surface

        #region Duration

        /// <summary>
        /// Duration. How long these particles will last.
        /// </summary>
        private float duration = 10;
        /// <summary>
        /// Duration. How long these particles will last.
        /// </summary>
        public float Duration
        {
            get { return duration; }
            set { duration = value; }
        } // Duration

        /// <summary>
        /// Last used duration
        /// </summary>
        private static float? lastUsedDuration;
        /// <summary>
        /// Set Duration (valor mayor igual a 0)
        /// </summary>
        private static void SetDuration(float _duration)
        {
            if (lastUsedDuration != _duration && _duration >= 0.0f)
            {
                lastUsedDuration = _duration;
                epDuration.SetValue(_duration);
            }
        } // SetDuration

        #endregion

        #region Duration Randomness

        /// <summary>
        /// Duration Randomness. If greater than zero, some particles will last a shorter time than others.
        /// </summary>
        private float durationRandomness;
        /// <summary>
        /// Duration Randomness. If greater than zero, some particles will last a shorter time than others.
        /// </summary>
        public float DurationRandomness
        {
            get { return durationRandomness; }
            set { durationRandomness = value; }
        } // DurationRandomness

        /// <summary>
        /// Last used duration randomness
        /// </summary>
        private static float? lastUsedDurationRandomness;
        /// <summary>
        /// Set Duration Randomness (valor mayor igual a 0)
        /// </summary>
        private static void SetDurationRandomness(float _durationRandomness)
        {
            if (lastUsedDurationRandomness != _durationRandomness && _durationRandomness >= 0.0f)
            {
                lastUsedDurationRandomness = _durationRandomness;
                epDurationRandomness.SetValue(_durationRandomness);
            }
        } // SetDurationRandomness

        #endregion

        #region Gravity

        /// <summary>
        /// Direction and strength of the gravity effect. This can point in any direction, not just down!
        /// The fire effect points it upward to make the flames rise, and the smoke plume points it sideways to simulate wind.
        /// </summary>
        private Vector3 gravity = new Vector3(-20, -5, 0);
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
        /// Last used gravity
        /// </summary>
        private static Vector3? lastUsedGravity;
        /// <summary>
        /// Set gravity
        /// </summary>
        protected void SetGravity(Vector3 _gravity)
        {
            if (lastUsedGravity != _gravity)
            {
                lastUsedGravity = _gravity;
                epGravity.SetValue(_gravity);
            }
        } // SetGravity

        #endregion

        #region End velocity

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
        /// Last used end velocity
        /// </summary>
        private static float? lastUsedEndVelocity;
        /// <summary>
        /// Set End Velocity (valor mayor igual a 0)
        /// </summary>
        private static void SetEndVelocity(float _endVelocity)
        {
            if (lastUsedEndVelocity != _endVelocity && _endVelocity >= 0.0f)
            {
                lastUsedEndVelocity = _endVelocity;
                epEndVelocity.SetValue(_endVelocity);
            }
        } // SetEndVelocity

        #endregion

        #region Minimum Color

        /// <summary>
        /// Minimum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Color minimumColor = new Color(255, 255, 255, 255);
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
        /// Last used minimum color
        /// </summary>
        private static Color? lastUsedMinimumColor;
        /// <summary>
        /// Set minimum color
        /// </summary>
        protected void SetMinimumColor(Color _minimumColor)
        {
            if (lastUsedMinimumColor != _minimumColor)
            {
                lastUsedMinimumColor = _minimumColor;
                epMinColor.SetValue(new Vector4(_minimumColor.R / 255f, _minimumColor.G / 255f, _minimumColor.B / 255f, _minimumColor.A / 255f));
            }
        } // SetMinimumColor

        #endregion

        #region Maximum Color

        /// <summary>
        /// Maximum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Color maximumColor = new Color(255, 255, 255, 255);
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
        /// Last used maximum color
        /// </summary>
        private static Color? lastUsedMaximumColor;
        /// <summary>
        /// Set maximum color
        /// </summary>
        protected void SetMaximumColor(Color _maximumColor)
        {
            if (lastUsedMaximumColor != _maximumColor)
            {
                lastUsedMaximumColor = _maximumColor;

                epMaxColor.SetValue(new Vector4(_maximumColor.R / 255f, _maximumColor.G / 255f, _maximumColor.B / 255f, _maximumColor.A / 255f));
            }
        } // SetMaximumColor

        #endregion

        #region Rotate Speed

        /// <summary>
        /// Rotate Speed. Range of values controlling how fast the particles rotate.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Vector2 rotateSpeed = new Vector2(-1, 1);
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
        /// Last used rotate speed
        /// </summary>
        private static Vector2? lastUsedRotateSpeed;
        /// <summary>
        /// Set rotate speed
        /// </summary>
        private static void SetRotateSpeed(Vector2 _rotateSpeed)
        {
            if (lastUsedRotateSpeed != _rotateSpeed)
            {
                lastUsedRotateSpeed = _rotateSpeed;
                epRotateSpeed.SetValue(_rotateSpeed);
            }
        } // SetRotateSpeed

        #endregion

        #region Start Size

        /// <summary>
        /// Start Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Vector2 startSize = new Vector2(5, 10);
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
        /// Last used start size
        /// </summary>
        private static Vector2? lastUsedStartSize;
        /// <summary>
        /// Set start size
        /// </summary>
        private static void SetStartSize(Vector2 _startSize)
        {
            if (lastUsedStartSize != _startSize)
            {
                lastUsedStartSize = _startSize;
                epStartSize.SetValue(_startSize);
            }
        } // SetStartSize

        #endregion

        #region End Size

        /// <summary>
        /// End Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        private Vector2 endSize = new Vector2(50, 200);
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
        /// Last used end size
        /// </summary>
        private static Vector2? lastUsedEndSize;
        /// <summary>
        /// Set end size
        /// </summary>
        private static void SetEndSize(Vector2 _endSize)
        {
            if (lastUsedEndSize != _endSize)
            {
                lastUsedEndSize = _endSize;
                epEndSize.SetValue(_endSize);
            }
        } // SetEndSize

        #endregion

        #endregion

        #region Texture

        /// <summary>
        /// Texture.
        /// </summary>
        private Graphics.Texture texture = new Graphics.Texture("Particles\\Smoke");
        /// <summary>
        /// Ttexture.
        /// </summary>
        public Graphics.Texture Texture
        {
            get { return texture; }
            set { texture = value; }
        } // Texture

        /// <summary>
        /// Last used texture.
        /// </summary>
        private static Graphics.Texture lastUsedTexture;
        /// <summary>
        /// Set texture.
        /// </summary>
        private static void SetTexture(Graphics.Texture _texture)
        {
            if (lastUsedTexture != _texture)
            {
                lastUsedTexture = _texture;
                epTexture.SetValue(_texture.XnaTexture);
            } // if
        } // SetTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Particle System. The settings are loaded from file.
        /// </summary>
        public ParticleSystem(string particleFilename)
        {

            string fullFilename = Directories.ParticlesDirectory + "\\" + particleFilename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load particle system: File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                settings = EngineManager.SystemContent.Load<ParticleSettings>(fullFilename);
                LoadParticleEffect();
                // Load settings
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
            }
            catch
            {
                throw new Exception("Failed to load particle system: " + particleFilename);
            }            
            try
            {
                Texture = new Graphics.Texture("Particles\\" + settings.TextureName);
            }
            catch
            {
                throw new Exception("Failed to load the particle texture: " + settings.TextureName);
            }

            CreateDataStructure();
            
        } // ParticleSystem

        /// <summary>
        /// Particle System. The particle settings need to be loaded through properties.
        /// The maximum number of particles is essential for the creation of the data structure.
        /// </summary>
        public ParticleSystem(int _maximumNumberParticles)
        {   
            try
            {
                LoadParticleEffect();

                MaximumNumberParticles = _maximumNumberParticles;

                CreateDataStructure();

            } // try
            catch (Exception)
            {
                throw new Exception("Failed to load particle system");
            }
        } // ParticleSystem

        #endregion

        #region Create Data Structure

        /// <summary>
        /// Create data structure.
        /// </summary>
        private void CreateDataStructure()
        {

            // Allocate the particle array, and fill in the corner fields (which never change).
            particles = new ParticleVertex[MaximumNumberParticles * 4];

            for (int i = 0; i < MaximumNumberParticles; i++)
            {
                particles[i * 4 + 0].Corner = new Short2(-1, -1);
                particles[i * 4 + 1].Corner = new Short2(1, -1);
                particles[i * 4 + 2].Corner = new Short2(1, 1);
                particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

            // Create a dynamic vertex buffer.
            vertexBuffer = new DynamicVertexBuffer(EngineManager.Device, ParticleVertex.VertexDeclaration, MaximumNumberParticles * 4, BufferUsage.WriteOnly);

            // Create and populate the index buffer.
            ushort[] indices = new ushort[MaximumNumberParticles * 6];

            for (int i = 0; i < MaximumNumberParticles; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            indexBuffer = new IndexBuffer(EngineManager.Device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);

            indexBuffer.SetData(indices);

        } // CreateDataStructure

        #endregion

        #region Load Particle Effect

        /// <summary>
        /// Loading and initializing the particle effect.
        /// </summary>
        private void LoadParticleEffect()
        {
            // Load shader
            try
            {
                effect = EngineManager.SystemContent.Load<Effect>(Directories.ShadersDirectory + "\\Particle");
            } // try
            catch
            {
                throw new Exception("Unable to load the particle shader");
            } // catch

            GetParameters();

        } // LoadParticleEffect

        #endregion

        #region Get Parameters

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        private void GetParameters()
        {
            try
            {   
                // Matrices //
                epView = effect.Parameters["View"];
                epProjection = effect.Parameters["Projection"];
                // Surface //    
                epViewportScale = effect.Parameters["ViewportScale"];
                epCurrentTime = effect.Parameters["CurrentTime"];
                epDuration = effect.Parameters["Duration"];
                epDurationRandomness = effect.Parameters["DurationRandomness"];
                epGravity = effect.Parameters["Gravity"];
                epEndVelocity = effect.Parameters["EndVelocity"];
                epMinColor = effect.Parameters["MinColor"];
                epMaxColor = effect.Parameters["MaxColor"];
                epRotateSpeed = effect.Parameters["RotateSpeed"];
                epStartSize = effect.Parameters["StartSize"];
                epEndSize = effect.Parameters["EndSize"];
                // Textures //
                epTexture = effect.Parameters["Texture"];
            }
            catch
            {
                throw new Exception("Get the handles from the particle shader failed");
            }
        } // GetParameters

        #endregion

        #region Set Parameters

        /// <summary>
        /// Set the global variables to the shader.
        /// </summary>
        private void SetParameters()
        {
            try
            {
                ViewMatrix = ApplicationLogic.Camera.ViewMatrix;
                ProjectionMatrix = ApplicationLogic.Camera.ProjectionMatrix;
                // needed to convert particle sizes into screen space point sizes.
                SetViewportScale(new Vector2(0.5f / EngineManager.AspectRatio, -0.5f));
                // Set an effect parameter describing the current time. All the vertex shader particle animation is keyed off this value.
                SetCurrentTime(currentTime);
                // Surface
                SetDuration(duration);
                SetDurationRandomness(durationRandomness);
                SetGravity(gravity);
                SetEndVelocity(endVelocity);
                SetMinimumColor(minimumColor);
                SetMaximumColor(maximumColor);
                SetRotateSpeed(rotateSpeed);
                SetStartSize(startSize);
                SetEndSize(endSize);
                // Texture //
                SetTexture(texture);
                }
            catch
            {
                throw new Exception("Unable to set the particle shader's parameters.");
            }
        } // SetParameters

        #endregion

        #region Update

        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public void Update()
        {
            currentTime += (float)EngineManager.FrameTime;

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually run out of floating point precision,
            // at which point the particles would render incorrectly. An easy way to prevent this is to notice that
            // the time value doesn't matter when no particles are being drawn, so we can reset it back to zero any time the active queue is empty.
            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;
            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;

            if (emitter != null)
            {
                emitter.Update();
            }
        } // Update

        /// <summary>
        /// Helper for checking when active particles have reached the end of their life.
        /// It moves old particles from the active area of the queue to the retired section.
        /// </summary>
        private void RetireActiveParticles()
        {
            float particleDuration = Duration;

            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each particle consists of a quad that is made up of four vertices.
                float particleAge = currentTime - particles[firstActiveParticle * 4].DrawCounterWhenDie;

                if (particleAge < particleDuration) // if the current particle is alive the rest will be also.
                    break;

                // Remember the time at which we retired this particle.
                particles[firstActiveParticle * 4].DrawCounterWhenDie = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle++;

                if (firstActiveParticle >= MaximumNumberParticles)
                    firstActiveParticle = 0;
            }
        } // RetireActiveParticles

        /// <summary>
        /// Helper for checking when retired particles have been kept around long enough that we can be sure the GPU is no longer using them.
        /// It moves old particles from the retired area of the queue to the free section.
        /// </summary>
        private void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                // Has this particle been unused long enough that the GPU is sure to be finished with it?
                // We multiply the retired particle index by four, because each particle consists of a quad that is made up of four vertices.
                int age = drawCounter - (int)particles[firstRetiredParticle * 4].DrawCounterWhenDie;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                firstRetiredParticle++;

                if (firstRetiredParticle >= MaximumNumberParticles)
                    firstRetiredParticle = 0;
            }
        } // FreeRetiredParticles

        #endregion

        #region Render
        
        /// <summary>
        /// Draws the particle system.
        /// </summary>
        public void Render()
        {
            // Restore the vertex buffer contents if the graphics device was lost.
            if (vertexBuffer.IsContentLost)
            {
                vertexBuffer.SetData(particles);
            }

            // If there are any particles waiting in the newly added queue, we'd better upload them to the GPU ready for drawing.
            if (firstNewParticle != firstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();
            }

            // If there are any active particles, draw them now!
            if (firstActiveParticle != firstFreeParticle)
            {

                #region Render States

                EngineManager.Device.BlendState = BlendState;
                EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;

                #endregion

                SetParameters();

                try
                {
                    // Set the particle vertex and index buffer.
                    EngineManager.Device.SetVertexBuffer(vertexBuffer);
                    EngineManager.Device.Indices = indexBuffer;

                    // Activate the particle effect.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        if (firstActiveParticle < firstFreeParticle)
                        {
                            // If the active particles are all in one consecutive range, we can draw them all in a single call.
                            EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                                       firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4,
                                                                       firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2);
                        }
                        else
                        {
                            // If the active particle range wraps past the end of the queue back to the start, we must split them over two draw calls.
                            EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                                       firstActiveParticle * 4, (MaximumNumberParticles - firstActiveParticle) * 4,
                                                                       firstActiveParticle * 6, (MaximumNumberParticles - firstActiveParticle) * 2);

                            if (firstFreeParticle > 0)
                            {
                                EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, firstFreeParticle * 4, 0, firstFreeParticle * 2);
                            }
                        }
                    } // foreach
                } // try
                catch (Exception e)
                {
                    throw new Exception("Unable to render the particle shader: " + e.Message);
                } // catch

                #region Render States

                // Reset some of the renderstates that we changed, so as not to mess up any other subsequent drawing.
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;

                #endregion
            }

            drawCounter++;

        } // Render

        /// <summary>
        /// Helper for uploading new particles from our managed array to the GPU vertex buffer.
        /// </summary>
        private void AddNewParticlesToVertexBuffer()
        {
            const int stride = ParticleVertex.SizeInBytes;

            if (firstNewParticle < firstFreeParticle)
            {
                // If the new particles are all in one consecutive range, we can upload them all in a single call.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles, firstNewParticle * 4, (firstFreeParticle - firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue back to the start, we must split them over two upload calls.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles, firstNewParticle * 4, (MaximumNumberParticles - firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);

                if (firstFreeParticle > 0)
                {
                    vertexBuffer.SetData(0, particles, 0, firstFreeParticle * 4, stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            firstNewParticle = firstFreeParticle;
        } // AddNewParticlesToVertexBuffer

        #endregion

        #region Add Particle

        /// <summary>
        /// Adds a new particle to the system.
        /// If the particle system doesn't have an emitter then we need to create the particles manually.
        /// </summary>
        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= MaximumNumberParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= EmitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(MinimumHorizontalVelocity,
                                                       MaximumHorizontalVelocity,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Y += MathHelper.Lerp(MinimumVerticalVelocity,
                                          MaximumVerticalVelocity,
                                          (float)random.NextDouble());

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color((byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                particles[firstFreeParticle * 4 + i].Position = position;
                particles[firstFreeParticle * 4 + i].Velocity = velocity;
                particles[firstFreeParticle * 4 + i].Random = randomValues;
                particles[firstFreeParticle * 4 + i].DrawCounterWhenDie = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
        } // AddParticle

        #endregion

    } // ParticleSystem
} // XNAFinalEngine.Particles
