
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System.Text;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace XNAFinalEngine.Sounds
{

    /// <summary>
    /// The sound content needs to be updating his parameters every frame, this class does this.
    /// It also manages the master sound volume.
    /// </summary>
    public class SoundManager
    {

        #region Variables

        /// <summary>
        /// Sounds volume (it doesn't include music). Range between 0.0f to 1.0f.
        /// </summary>
        private static float masterSoundVolume = 1;

        /// <summary>
        /// List of sounds that are currently playing.
        /// </summary>
        private static List<SoundInstance> sounds = new List<SoundInstance>();

        /// <summary>
        /// Adjust the effect of distance calculations on the sounds.
        /// </summary>
        private static float distanceScale = 10f;

        /// <summary>
        /// Master doppler scale.
        /// </summary>
        private static float masterDopplerScale = 1f;

        #endregion

        #region Properties

        /// <summary>
        /// Sounds Volume (doesn't include music). Range between 0.0f to 1.0f.
        /// </summary>
        public static float MasterSoundVolume
        {
            get { return masterSoundVolume; }
            set { masterSoundVolume = value; }
        } // MasterSoundVolume

        /// <summary>
        /// Adjust the effect of distance calculations on the sounds.
        /// </summary>
        public static float DistanceScale
        {
            get { return distanceScale; }
            set { distanceScale = value; }
        } // DistanceScale

        /// <summary>
        /// Master doppler scale.
        /// </summary>
        public static float MasterDopplerScale
        {
            get { return masterDopplerScale; }
            set { masterDopplerScale = value; }
        } // DopplerScale

        #endregion

        #region Add Sounds

        /// <summary>
        /// Add the sound to the list of sounds currently playing.
        /// </summary>
        internal static void AddSound(SoundInstance soundInstance)
        {
            sounds.Add(soundInstance);
        } // AddSound

        #endregion

        #region Pause Resume

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

        #endregion

        #region Update

        /// <summary>
        /// Update automatically in every frame the sound's parameters of all the sounds currently playing.
        /// </summary>
        public static void UpdateSound()
        {
            // Update general parameters
            SoundEffect.DistanceScale = DistanceScale;
            SoundEffect.DopplerScale = MasterDopplerScale;
            SoundEffect.MasterVolume = MasterSoundVolume;
            // Remove sound instances that are over.
            sounds = (from SoundInstance soundInstance in sounds where !soundInstance.IsOver select soundInstance).ToList<SoundInstance>();
            // Update sounds.
            foreach (SoundInstance soundInstance in sounds)
            {
                soundInstance.Update();
            }
        } // UpdateSound

        #endregion

    } // SoundManager
} // XNAFinalEngine.Sounds
