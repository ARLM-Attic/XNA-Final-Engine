
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Helpers;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Model Renderer.
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public class ParticleRenderer : Renderer
    {

        #region Properties

        /// <summary>
        /// Indicates is the particle system is soft or hard.
        /// In a soft particle system the closer the sprite’s fragment is to the scene, the more it fades out to show more of the background.
        /// Also, if the sprite is behind the background scene then the fragment is discard entirely.
        /// You can see the Shader X7 book or NVIDIA soft particles article for more information about soft particles.
        /// </summary>
        public bool SoftParticles { get; set; }

        /// <summary>
        /// This value affects how much the fragment particle fade out when a scene object is close.
        /// Soft particles property has to be set to true.
        /// </summary>
        public float FadeDistance { get; set; }

        /// <summary>
        /// Duration Randomness. If greater than zero, some particles will last a shorter time than others.
        /// </summary>
        public float DurationRandomness { get; set; }

        /// <summary>
        /// Direction and strength of the gravity effect. This can point in any direction, not just down!
        /// The fire effect points it upward to make the flames rise, and the smoke plume points it sideways to simulate wind.
        /// </summary>
        public Vector3 Gravity { get; set; }

        /// <summary>
        /// Controls how the particle velocity will change over their lifetime. If set to 1, particles will keep going at the same speed as when they were created.
        /// If set to 0, particles will come to a complete stop right before they die. Values greater than 1 make the particles speed up over time.
        /// </summary>
        public float EndVelocity { get; set; }

        /// <summary>
        /// Minimum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Color MinimumColor { get; set; }

        /// <summary>
        /// Maximum Color. Range of values controlling the particle color and alpha.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Color MaximumColor { get; set; }

        /// <summary>
        /// Rotate Speed. Range of values controlling how fast the particles rotate.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Vector2 RotateSpeed { get; set; }

        /// <summary>
        /// Start Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Vector2 StartSize { get; set; }

        /// <summary>
        /// End Size. Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        public Vector2 EndSize { get; set; }

        /// <summary>
        /// Ttexture.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// Alpha blending state. 
        /// </summary>
        public BlendState BlendState { get; set; }
        
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            // Set default values.
            SoftParticles = true;
            FadeDistance = 0.1f;
            DurationRandomness = 0;
            Gravity = new Vector3(-20, -5, 0);
            EndVelocity = 0.75f;
            MinimumColor = new Color(255, 255, 255, 255);
            MaximumColor = new Color(255, 255, 255, 255);
            RotateSpeed = new Vector2(-1, 1);
            StartSize = new Vector2(5, 10);
            EndSize = new Vector2(50, 200);
            Texture = null;
            BlendState = BlendState.Additive;
            base.Initialize(owner);
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

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<ParticleRenderer> componentPool = new Pool<ParticleRenderer>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<ParticleRenderer> ComponentPool { get { return componentPool; } }

        #endregion

    } // ParticleRenderer
} // XNAFinalEngine.Components
