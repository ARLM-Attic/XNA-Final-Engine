
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
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Audio
{

    /// <summary>
    /// The sound content needs to be updating his parameters every frame, this class does this.
    /// It also manages the master sound volume.
    /// </summary>
    public class SoundManager
    {

        #region Variables
        
        // Default values.
        private static float masterVolume = 1;
        private static float distanceScale = 1f;
        private static float dopplerScale = 1f;
        private static bool catchNoAudioHardwareException = true;

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

        #endregion

        #region Pause Resume
        /*
        /// <summary>
        /// Pause all sounds.
        /// </summary>
        public void PauseAllSounds()
        {
            foreach (SoundInstance soundInstance in sounds)
            {
                soundInstance.Pause();
            }
        } // PauseAllSounds

        /// <summary>
        /// Resume all sounds.
        /// </summary>
        public void ResumeAllSounds()
        {
            foreach (SoundInstance soundInstance in sounds)
            {
                soundInstance.Resume();
            }
        } // ResumeAllSounds
        */
        #endregion
        
        #region Update

        /// <summary>
        /// Update automatically in every frame the sound's parameters of all the sounds currently playing.
        /// </summary>
        public static void UpdateSound()
        {
            //try
            {
                // Update general parameters
                SoundEffect.DistanceScale = DistanceScale;
                SoundEffect.DopplerScale = DopplerScale;
                SoundEffect.MasterVolume = MasterVolume;
                // Remove sound instances that are over.
                //sounds = (from SoundInstance soundInstance in sounds where !soundInstance.IsOver select soundInstance).ToList();
                // Update sounds.
                /*foreach (SoundInstance soundInstance in sounds)
                {
                    soundInstance.Update();
                }*/
            }
            /*catch (Exception)
            {
                if (!EngineManager.IgnoreSoundExceptions)
                    throw new Exception("Sound Manager critical error.");
            }*/
        } // UpdateSound

        #endregion

    } // SoundManager
} // XNAFinalEngine.Audio
