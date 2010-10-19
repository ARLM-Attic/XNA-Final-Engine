
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
        private static List<Sound> sounds = new List<Sound>();

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

        #endregion

        #region Add and Remove Sounds

        /// <summary>
        /// Remove the sound from the list of sounds currently playing.
        /// </summary>
        public static void RemoveSound(Sound sound)
        {
            sounds.Remove(sound);
        } // RemoveSound

        /// <summary>
        /// Add the sound to the list of sounds currently playing.
        /// </summary>
        public static void AddSound(Sound sound)
        {
            sounds.Add(sound);
        } // AddSound

        #endregion

        #region Update

        /// <summary>
        /// Update automatically in every frame the sound's parameters of all the sounds currently playing.
        /// </summary>
        public static void UpdateSoundParameters()
        {
            foreach (Sound sound in sounds)
            {
                sound.Update();
            }
        } // UpdateSoundParameters

        #endregion

    } // SoundManager
} // XNAFinalEngine.Sounds
