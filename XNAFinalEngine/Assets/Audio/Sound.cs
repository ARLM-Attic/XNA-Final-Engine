
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
using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.Audio;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Sound.
    /// </summary>
    /// <remarks>
    /// A Sound Instance is associated to one and only sound asset. 
    /// To avoid garbage generation when playing a sound we need to have the sound instance created in loading time,
    /// but it is impossible that the system knows in anticipation how much instances will need.
    /// By default the engine will be create the instance and deleted when it finish and thus generating garbage in the process
    /// (avoiding the dispose operation is not need it because the creation of classes is what the garbage collector tracks).
    /// However, you can reserve sound effect instance in the general sound instances pool.
    /// If all sound instances are used a new but temporal sound instance will be created in the pool system.
    /// </remarks>
    public class Sound : Asset
    {

        #region Enumerates

        /// <summary>
        /// Indicates if the sound is 2D or 3D.
        /// </summary>
        public enum SoundType
        {
            /// <summary>
            /// They sound the same no matter the listener and emitter properties.
            /// </summary>
            Sound2D,
            /// <summary>
            /// 3D sounds take in consideration the emitter and a listener properties, like position, orientation, velocity and distance. 
            /// The sound source should be mono.
            /// </summary>
            Sound3D
        } // SoundType

        #endregion

        #region Properties

        /// <summary>
        /// XNA sound effect.
        /// </summary>
        public SoundEffect Resource { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether looping is enabled.
        /// </summary>
        public bool IsLooped { get; private set; }

        /// <summary>
        /// Indicates if the sound is 2D or 3D.
        /// </summary>
        public SoundType Type { get; private set; }

        /// <summary>
        /// Gets the duration of the sound (in seconds)
        /// </summary>
        public float Duration { get { return (float)Resource.Duration.TotalSeconds; } }

        /// <summary>
        /// Returns the speed of sound: 343.5 meters per second.
        /// </summary>
        public static float SpeedOfSound { get { return SoundEffect.SpeedOfSound; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Load sound.
        /// </summary>
        /// <param name="filename">The filename must be relative and be a valid file in the sound directory.</param>
        /// <param name="isLooped">XNA impose to set and don’t change the “is looped” property.</param>
        /// <param name="type">2D or 3D sound. XNA impose to set and don’t change the “type” property.</param>
        public Sound(string filename, bool isLooped = false, SoundType type = SoundType.Sound3D)
        {
            Name = filename;
            IsLooped = isLooped;
            Type = type;
            Filename = AssetContentManager.GameDataDirectory + "Sounds\\" + filename;
            if (File.Exists(Filename + ".xnb") == false)
            {
                throw new Exception("Failed to load sound: File " + Filename + " does not exists!");
            }
            try
            {
                Resource = AssetContentManager.CurrentContentManager.XnaContentManager.Load<SoundEffect>(Filename);
            }
            catch (ObjectDisposedException e)
            {
                throw new Exception("Content Manager: Content manager disposed", e);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load sound: " + filename, e);
            }
        } // Sound

        #endregion

        #region Play

        /// <summary>
        /// Plays a sound.
        /// </summary>
        /// <remarks>
        /// Play returns false if too many sounds already are playing. 
        /// 
        /// To loop a sound or apply 3D effects, call CreateInstance instead of Play, and SoundEffectInstance.Play. 
        /// 
        /// Sounds play in a "fire and forget" fashion with Play.
        /// These sounds will play once, and then stop. They cannot be looped or 3D positioned.
        /// To loop a sound or apply 3D effects, use the sound components.
        /// 
        /// In the XBOX 360 only 300 sound instances could be exist at the same time.
        /// The sound system will not allow more than 290 instances at the same time, so you can throw some fire and forget sounds, but be careful.
        /// 
        /// With the sound’s duration you can implement a system that checks fire and forget sound, but is unnecessary and it hurts the performance a little.
        /// Besides, the sound emitter brings everything you need. Just use fire and forget sounds in testing or something similar.
        /// </remarks>
        /// <returns>true if the sound is playing back successfully; otherwise, false.</returns>
        public bool Play ()
        {
            return Resource.Play();
        } // Play

        /// <summary>
        /// Plays a sound.
        /// </summary>
        /// <remarks>
        /// Play returns false if too many sounds already are playing. 
        /// 
        /// To loop a sound or apply 3D effects, call CreateInstance instead of Play, and SoundEffectInstance.Play. 
        /// 
        /// Sounds play in a "fire and forget" fashion with Play.
        /// These sounds will play once, and then stop. They cannot be looped or 3D positioned.
        /// To loop a sound or apply 3D effects, use the sound components.
        /// 
        /// In the XBOX 360 only 300 sound instances could be exist at the same time.
        /// The sound system will not allow more than 290 instances at the same time, so you can throw some fire and forget sounds, but be careful.
        /// 
        /// With the sound’s duration you can implement a system that checks fire and forget sound, but is unnecessary and it hurts the performance a little.
        /// Besides, the sound emitter brings everything you need. Just use fire and forget sounds in testing or something similar.
        /// </remarks>
        /// <param name="volume">Volume, ranging from 0.0f (silence) to 1.0f (full volume). </param>
        /// <param name="pitch">Pitch adjustment, ranging from -1.0f (down one octave) to 1.0f (up one octave). 0.0f is unity (normal) pitch.</param>
        /// <param name="pan">Panning, ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered.</param>
        /// <returns>true if the sound is playing back successfully; otherwise, false.</returns>
        public bool Play(float volume, float pitch, float pan)
        {
            return Resource.Play(volume, pitch, pan);
        } // Play

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            SoundManager.RemoveSoundInstances(this);
        } // DisposeManagedResources

	    #endregion

        #region Create Sound Instance Pool

        /// <summary>
        /// A Sound Instance is associated to one and only sound asset. 
        /// To avoid garbage generation when playing a sound we need to have the sound instance created in loading time,
        /// but it is impossible that the system knows in anticipation how much instances will need.
        /// By default the engine will be create the instance and deleted when it finish and thus generating garbage in the process
        /// (avoiding the dispose operation is not need it because the creation of classes is what the garbage collector tracks).
        /// However, you can reserve sound effect instance in the general sound instances pool.
        /// If all sound instances are used a new but temporal sound instance will be created in the pool system.
        /// </summary>
        /// <param name="count">The number of instances to add.</param>
        public void ReserveSoundInstances(int count)
        {
            if (count <= 0 || count > 290)
                throw new ArgumentOutOfRangeException("count");
            SoundManager.ReserveSoundInstance(this, count);
        } // CreateSoundInstancePool

        /// <summary>
        /// Release reserved instances.
        /// </summary>
        /// <param name="count">You can indicate a higher number of instances of what they are.</param>
        public void ReleaseSoundInstances(int count = 290)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");
            SoundManager.ReleaseSoundInstance(this, count);
        } // ReleaseSoundInstances

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal override void RecreateResource()
        {
            Resource = AssetContentManager.CurrentContentManager.XnaContentManager.Load<SoundEffect>(Filename);
        } // RecreateResource

        #endregion

    } // Sound
} // XNAFinalEngineBase.Assets
