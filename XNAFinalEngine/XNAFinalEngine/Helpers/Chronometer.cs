
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
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Helpers
{

    /// <summary>
    /// Chronometer in seconds, starts in pause.
    /// </summary>
    public class Chronometer : IDisposable
    {

        #region Variables

        /// <summary>
        /// The current active chronometers in use by the aplication.
        /// </summary>
        private static List<Chronometer> chronometers = new List<Chronometer>();

        #endregion

        #region Properties

        /// <summary>
        /// Elapsed time in seconds.
        /// </summary>
        public double ElapsedTime { get; private set; }

        /// <summary>
        /// It indicates if the chronometer is running or not. Pause is the default state.
        /// </summary>
        public bool IsPause { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Chronometer in seconds, starts in pause.
        /// </summary>
        public Chronometer()
        {
            ElapsedTime = 0;
            chronometers.Add(this);
        } // Chronometer

        #endregion

        #region Change State

        /// <summary>
        /// Resume chronometer.
        /// </summary>
        public void Start()
        {
            IsPause = false;
        } // Start

        /// <summary>
        /// Pause chronometer.
        /// </summary>
        public void Pause()
        {
            IsPause = true;
        } // Pause

        #endregion

        #region Reset

        /// <summary>
        /// Reset counter. The state doesn't change.
        /// </summary>
        public void Reset()
        {
            ElapsedTime = 0;
        }

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            if (!IsPause)
            {
                ElapsedTime += EngineManager.FrameTime;
            }
        } // Update

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            chronometers.Remove(this);
        } // Dispose

        #endregion

        #region Static Managment (for all chronometers)

        /// <summary>
        /// Pause all chronometers.
        /// </summary>
        public static void PauseAllChronometers()
        {
            foreach (Chronometer chronometer in chronometers)
            {
                 chronometer.Pause();
            }
        } // PauseAllChronometers

        /// <summary>
        /// Resume all chronometers.
        /// </summary>
        public static void StartAllChronometers()
        {
            foreach (Chronometer chronometer in chronometers)
            {
                chronometer.Start();
            }
        } // StartAllChronometers

        /// <summary>
        /// Update all chronometers.
        /// </summary>
        public static void UpdateAllChronometers()
        {
            foreach (Chronometer chronometer in chronometers)
            {
                chronometer.Update();
            }
        } // UpdateAllChronometers

        #endregion

    } // Chronometer
} // XNAFinalEngine.Helpers
