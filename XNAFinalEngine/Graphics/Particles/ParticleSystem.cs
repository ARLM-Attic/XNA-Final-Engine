
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Particle System.
    /// </summary>
    internal class ParticleSystem : Disposable
    {

        #region Variables

        // Count how many times Draw has been called. This is used to know when it is safe to retire old particles back into the free list. 
        private int drawCounter;

        private readonly int maximumNumberParticles;

        // Shared random number generator.
        private static readonly Random random = new Random();
        
        #region Data Structure

        /// <summary>
        /// An array of particles, treated as a circular queue.
        /// </summary>
        private readonly ParticleVertex[] particles;
                        
        /// <summary>
        /// A vertex buffer holding our particles. This contains the same data as
        /// the particles array, but copied across to where the GPU can access it.
        /// </summary>
        private readonly DynamicVertexBuffer vertexBuffer;

        /// <summary>
        /// Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        /// </summary>
        private readonly IndexBuffer indexBuffer;

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
        /// Current Time (in seconds)
        /// </summary>
        public float CurrentTime { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Particle System.
        /// </summary>
        public ParticleSystem(int maximumNumberParticles)
        {
            this.maximumNumberParticles = maximumNumberParticles;

            // Allocate the particle array, and fill in the corner fields (which never change).
            particles = new ParticleVertex[maximumNumberParticles * 4];

            for (int i = 0; i < maximumNumberParticles; i++)
            {
                particles[i * 4 + 0].Corner = new Short2(-1, -1);
                particles[i * 4 + 1].Corner = new Short2(1, -1);
                particles[i * 4 + 2].Corner = new Short2(1, 1);
                particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

            // Create a dynamic vertex buffer.
            vertexBuffer = new DynamicVertexBuffer(EngineManager.Device, ParticleVertex.VertexDeclaration, maximumNumberParticles * 4, BufferUsage.WriteOnly);

            // Create and populate the index buffer.
            ushort[] indices = new ushort[maximumNumberParticles * 6];

            for (int i = 0; i < maximumNumberParticles; i++)
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
        } // ParticleSystem

        #endregion

        #region Update

        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public void Update(float particleDuration)
        {
            CurrentTime += Time.FrameTime;

            RetireActiveParticles(particleDuration);
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually run out of floating point precision,
            // at which point the particles would render incorrectly. An easy way to prevent this is to notice that
            // the time value doesn't matter when no particles are being drawn, so we can reset it back to zero any time the active queue is empty.
            if (firstActiveParticle == firstFreeParticle)
                CurrentTime = 0;
            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;
        } // Update

        /// <summary>
        /// Helper for checking when active particles have reached the end of their life.
        /// It moves old particles from the active area of the queue to the retired section.
        /// </summary>
        private void RetireActiveParticles(float particleDuration)
        {
            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each particle consists of a quad that is made up of four vertices.
                float particleAge = CurrentTime - particles[firstActiveParticle * 4].DrawCounterWhenDie;

                if (particleAge < particleDuration) // if the current particle is alive the rest will be also.
                    break;

                // Remember the time at which we retired this particle.
                particles[firstActiveParticle * 4].DrawCounterWhenDie = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle++;

                if (firstActiveParticle >= maximumNumberParticles)
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

                if (firstRetiredParticle >= maximumNumberParticles)
                    firstRetiredParticle = 0;
            }
        } // FreeRetiredParticles

        #endregion

        #region Render
        
        /// <summary>
        /// Draws the particle system.
        /// I recommend using the SceneManager.AddParticleSystemToRender method instanced of render it manually so that the particles are post processed.
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
                try
                {
                    // Set the particle vertex and index buffer.
                    EngineManager.Device.SetVertexBuffer(vertexBuffer);
                    EngineManager.Device.Indices = indexBuffer;

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
                                                                   firstActiveParticle * 4, (maximumNumberParticles - firstActiveParticle) * 4,
                                                                   firstActiveParticle * 6, (maximumNumberParticles - firstActiveParticle) * 2);

                        if (firstFreeParticle > 0)
                        {
                            EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, firstFreeParticle * 4, 0, firstFreeParticle * 2);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Particle System: Unable to render the particle system.", e);
                }
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
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles, firstNewParticle * 4, (maximumNumberParticles - firstNewParticle) * 4, stride, SetDataOptions.NoOverwrite);

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
        public void AddParticle(Vector3 position, Vector3 velocity, float emitterVelocitySensitivity,
                                float minimumHorizontalVelocity, float maximumHorizontalVelocity,
                                float minimumVerticalVelocity, float maximumVerticalVelocity)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= maximumNumberParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= emitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(minimumHorizontalVelocity,
                                                       maximumHorizontalVelocity,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Y += MathHelper.Lerp(minimumVerticalVelocity,
                                          maximumVerticalVelocity,
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
                particles[firstFreeParticle * 4 + i].DrawCounterWhenDie = CurrentTime;
            }

            firstFreeParticle = nextFreeParticle;
        } // AddParticle

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        } // DisposeManagedResources

        #endregion

    } // ParticleSystem
} // XNAFinalEngine.Graphics
