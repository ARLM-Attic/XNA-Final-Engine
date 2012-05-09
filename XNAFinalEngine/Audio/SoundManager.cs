
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
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Audio
{

    /// <summary>
    /// The sound manager.
    /// </summary>
    /// <remarks>
    /// A Sound Instance is associated to one and only sound asset. 
    /// To avoid garbage generation when playing a sound we need to have the sound instance created in loading time,
    /// but it is impossible that the system knows in anticipation how much instances will need.
    /// By default the engine will be create the instance and deleted when it finish and thus generating garbage in the process
    /// (avoiding the dispose operation is not need it because the creation of classes is what the garbage collector tracks).
    /// However, you can create a pool of instance in loading time (see the Sound class) if you know how much instances you will need. 
    /// If all sound instances are used you could return the next as null or create a new one outside the pool system.
    /// 
    /// About XACT:
    /// Shawn Hargreaves said: "All our future investment in audio technology will be in the SoundEffect, Song, and MediaPlayer APIs, as opposed to XACT."
    /// Moreover it seems that XACT has problems with garbage (similar to SounEffect). But I’m not so sure, I didn’t make tests to corroborate this.
    /// But, what it is better? Make an implementation that supports XACT? Or ignore it?
    /// It seems that even if the XACT’s tools are nice they don’t compensate the garbage problems and complexity.
    /// Besides the sound’s components are there, if XACT is need then it is possible to change the SoundEffect and Song code with the XACT code.
    /// It's not trivial, but not impossible either.
    /// </remarks>
    public static class SoundManager
    {

        #region Constants

        /// <summary>
        /// Maximum number of sound instances.
        /// 10 are reserved for fire and forget sounds.
        /// </summary>
        private const int maximumNumberOfSoundInstances = 290;

        #endregion

        #region Variables

        // Default values.
        private static float masterVolume = 1;
        private static float distanceScale = 1f;
        private static float dopplerScale = 1f;
        private static bool catchNoAudioHardwareException = true;

        private static int soundInstanceCount;

        // The active sound instances.
        private static readonly SoundEffectInstance[] soundEffectInstances = new SoundEffectInstance[maximumNumberOfSoundInstances];
        // The sound resources of the active sound instances.
        private static readonly Sound[] soundEffectInstancesResource = new Sound[maximumNumberOfSoundInstances];

        #endregion

        #region Properties

        /// <summary>
        /// Sounds Master Volume (doesn't include music).
        /// </summary>
        /// <value>Volume, ranging from 0.0f (silence) to 1.0f (full volume).</value>
        public static float MasterVolume
        {
            get { return masterVolume; }
            set
            {
                masterVolume = value;
                if (masterVolume < 0)
                    masterVolume = 0;
                if (masterVolume > 1)
                    masterVolume = 1;
            }
        } // MusicVolume

        /// <summary>
        /// Gets or sets a value that adjusts the effect of distance calculations on the sound (emitter).
        /// </summary>
        /// <value>Value that scales distance calculations on the sound (emitter).</value>
        /// <remarks>
        /// If sounds are attenuating too fast, which means that the sounds get quiet too quickly as they move away from the listener, you need to increase the DistanceScale.
        /// If sounds are not attenuating fast enough, decrease the DistanceScale. 
        /// This property will also affect Doppler sound.
        /// 
        /// If the values is in the (0, 1) range the effect of distance attenuation increases.
        /// If the distance is greater thant 1 the distance attenuation decreases.
        /// </remarks>
        public static float DistanceScale
        {
            get { return distanceScale; }
            set
            {
                distanceScale = value;
                if (distanceScale < 0)
                    distanceScale = 0;
            }
        } // DistanceScale

        /// <summary>
        /// Gets or sets a value that adjusts the effect of doppler calculations on the sound (emitter).
        /// </summary>
        /// <value>Value that scales doppler calculations on the sound (emitter).</value>
        /// <remarks>
        /// DopplerScale changes the relative velocities of emitters and listeners.
        /// If sounds are shifting (pitch) too much for the given relative velocity of the emitter and listener, decrease the DopplerScale.
        /// If sounds are not shifting enough for the given relative velocity of the emitter and listener, increase the DopplerScale.
        /// 
        /// 0.0 < DopplerScale <= 1.0 --> Decreases the effect of Doppler
        /// DopplerScale > 1.0 --> Increase the effect of Doppler
        /// </remarks>
        public static float DopplerScale
        {
            get { return dopplerScale; }
            set
            {
                dopplerScale = value;
                if (dopplerScale < 0)
                    dopplerScale = 0;
            }
        } // DopplerScale
        
        /// <summary>
        /// Indicates if the system ignores or not the NoAudioHardwareException.
        /// </summary>
        /// <remarks>
        /// The exception that is thrown when no audio hardware is present, or when audio hardware is installed,
        /// but the device drivers for the audio hardware are not present or enabled.
        /// </remarks>
        public static bool CatchNoAudioHardwareException
        {
            get { return catchNoAudioHardwareException; }
            set { catchNoAudioHardwareException = value; }
        } // CatchNoAudioHardwareException

        /// <summary>
        /// The number of sound instances in memory.
        /// </summary>
        public static int SoundInstanceCount
        {
            get { return soundInstanceCount; }
            internal set
            {
                if (soundInstanceCount > maximumNumberOfSoundInstances)
                    throw new InvalidOperationException("Sound Manager: The number of sound instances is too high.");
                soundInstanceCount = value;
            }
        } // SoundInstanceCount

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the Sprite Manager subsystem.
        /// </summary>
        internal static void Initialize()
        {
            for (int i = 0; i < soundEffectInstances.Length; i++)
            {
                if (soundEffectInstances[i] == null)
                    return;
                soundEffectInstances[i].Dispose();
                soundEffectInstances[i] = soundEffectInstancesResource[i].Resource.CreateInstance();
            }
        } // Initialize

        #endregion

        #region Pause Resume
        
        /// <summary>
        /// Pause all sounds.
        /// </summary>
        public static void PauseAllSounds()
        {
            for (int i = 0; i < soundEffectInstances.Length; i++)
            {
                soundEffectInstances[i].Pause();
            }
        } // PauseAllSounds

        /// <summary>
        /// Resume all sounds.
        /// </summary>
        public static void ResumeAllSounds()
        {
            for (int i = 0; i < soundEffectInstances.Length; i++)
            {
                soundEffectInstances[i].Resume();
            }
        } // ResumeAllSounds
        
        #endregion

        #region Fetch Sound Instance

        /// <summary>
        /// Give a sound instance of a sound asset.
        /// </summary>
        /// <remarks>
        /// In the XBOX 360 only 300 sound instances could be exist at the same time.
        /// The sound system will not allow more than 290 instances at the same time so that you can use some fire and forget sounds.
        /// </remarks>
        internal static SoundEffectInstance FetchSoundInstance(Sound sound)
        {
            // If the sound has a pool of sound effect instances.
            if (sound.SoundEffectInstances != null)
            {
                for (int j = 0; j < sound.SoundEffectInstances.Length; j++)
                {
                    if (sound.SoundEffectInstances[j].State == SoundState.Stopped)
                        return sound.SoundEffectInstances[j];
                }
            }
            // Maybe we have one available in the central pool.
            int i;
            for (i = 0; i < soundEffectInstances.Length; i++)
            {
                if (soundEffectInstances[i] == null)
                    break;
                if (soundEffectInstancesResource[i] == sound && soundEffectInstances[i].State == SoundState.Stopped)
                    return soundEffectInstances[i];
            }
            // If we have space in the sound instances array...
            if (soundInstanceCount < maximumNumberOfSoundInstances)
            {
                // i is the first available position.
                soundEffectInstances[i] = sound.Resource.CreateInstance();
                soundEffectInstances[i].IsLooped = sound.IsLooped;
                soundEffectInstancesResource[i] = sound;
                soundInstanceCount++;
                return soundEffectInstances[i];
            }
            // If there is no space maybe some instance is stopped. We use their place.
            for (int j = 0; j < soundEffectInstances.Length; j++)
            {
                if (soundEffectInstances[j] == null)
                    return null;
                if (soundEffectInstances[j].State == SoundState.Stopped)
                {
                    soundEffectInstances[j].Dispose();
                    soundEffectInstances[j] = sound.Resource.CreateInstance();
                    soundEffectInstances[j].IsLooped = sound.IsLooped;
                    soundEffectInstancesResource[j] = sound;
                    return soundEffectInstances[j];
                }
            }
            throw new InvalidOperationException("Sound Manager: Fetch Sound Instance produce an unexpected exception.");
        } // FetchSoundInstance

        #endregion

        #region Remove Unused Sound Instances

        /// <summary>
        /// Remove unused sound instance from the general array of instances.
        /// </summary>
        internal static void RemoveUnusedSoundInstances()
        {
            // We find the position of the last created instance.
            int firstNullPosition;
            for (firstNullPosition = 0; firstNullPosition < soundEffectInstances.Length; firstNullPosition++)
            {
                if (soundEffectInstances[firstNullPosition] == null)
                    break;
            }
            for (int i = 0; i < soundEffectInstances.Length; i++)
            {
                if (soundEffectInstances[i] == null)
                    return;
                if (soundEffectInstances[i].State == SoundState.Stopped)
                {
                    soundEffectInstances[i].Dispose();
                    soundEffectInstancesResource[i] = null;
                    soundInstanceCount--;
                    // Swap this value with the last used sound instance.
                    soundEffectInstances[i]         = soundEffectInstances[firstNullPosition - 1];
                    soundEffectInstancesResource[i] = soundEffectInstancesResource[firstNullPosition - 1];
                    soundEffectInstances[firstNullPosition - 1] = null;
                    soundEffectInstancesResource[firstNullPosition - 1] = null;
                    firstNullPosition--;
                }
            }
        } // RemoveUnusedSoundInstances

        #endregion

        #region Update

        /// <summary>
        /// Update the sound's global parameters.
        /// </summary>
        public static void Update()
        {
            // Update general parameters
            SoundEffect.DistanceScale = DistanceScale;
            SoundEffect.DopplerScale = DopplerScale;
            SoundEffect.MasterVolume = MasterVolume;
        } // Update

        #endregion

    } // SoundManager
} // XNAFinalEngine.Audio
