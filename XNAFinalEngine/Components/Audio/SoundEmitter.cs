
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
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Sound Listener.
    /// </summary>
    public class SoundEmitter : Component
    {
        
        #region Variables

        /// <summary>
        /// This is the cached world matrix from the transform component.
        /// This matrix represents the view matrix.
        /// </summary>
        internal Matrix cachedWorldMatrix;

        // Sound properties.
        private float volume, pan, pitch;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Sound Effect Instance.
        /// </summary>
        internal SoundEffectInstance SoundEffectInstance { get; private set; }

        /// <summary>
        /// Mutes the sound.
        /// </summary>
        public bool Mute { get; set; }

        /// <summary>
        /// Sound Volume.
        /// </summary>
        /// <value>Volume, ranging from 0.0f (silence) to 1.0f (full volume).</value>
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (volume < 0)
                    volume = 0;
                if (volume > 1)
                    volume = 1;
            }
        } // Volume

        /// <summary>
        /// Sound asset.
        /// </summary>
        public Sound Sound { get; set; }

        /// <summary>
        /// Gets a value that indicates whether looping is enabled.
        /// </summary>
        /// <remarks>
        /// If you want to make a sound play continuously until stopped, be sure to set IsLooped to true before you call Play.
        /// </remarks>
        public virtual bool IsLooped { get; set; }

        /// <summary>
        /// Gets or sets the panning.
        /// </summary>
        /// <value>Panning, ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered.</value>
        public float Pan
        {
            get { return pan; }
            set
            {
                pan = value;
                if (pan < -1)
                    pan = -1;
                if (pan > 1)
                    pan = 1;
            }
        } // Pan

        /// <summary>
        /// Gets or sets the pitch.
        /// </summary>
        /// <value>Pitch adjustment, ranging from -1.0f (down one octave) to 1.0f (up one octave). 0.0f is unity (normal) pitch.</value>
        public float Pitch
        {
            get { return pitch; }
            set
            {
                pitch = value;
                if (pitch < -1)
                    pitch = -1;
                if (pitch > 1)
                    pitch = 1;
            }
        } // Pitch
        
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Values //
            volume = 1;
            Sound = null;
            IsLooped = false;
            Pan = 0;
            Pitch = 0;
            // Cache transform matrix. It will be the view matrix.
            cachedWorldMatrix = ((GameObject3D)Owner).Transform.WorldMatrix;
            ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
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
            ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
        } // Uninitialize

        #endregion

        #region Play, Pause, Resume, Stop

        public void Play()
        {
            SoundEffectInstance = Sound.Resource.CreateInstance();
        } // Play

        public void Play3D()
        {
            SoundEffectInstance = Sound.Resource.CreateInstance();
        } // Play3D

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected virtual void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            // The view matrix is the invert
            cachedWorldMatrix = Matrix.Invert(worldMatrix);            
        } // OnWorldMatrixChanged

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<SoundEmitter> componentPool = new Pool<SoundEmitter>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<SoundEmitter> ComponentPool { get { return componentPool; } }

        #endregion

    } // SoundEmitter
} // XNAFinalEngine.Components