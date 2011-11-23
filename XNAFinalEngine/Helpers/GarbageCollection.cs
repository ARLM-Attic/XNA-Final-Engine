﻿
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
using System.Runtime;
#endregion

namespace XNAFinalEngine.Helpers
{
    /// <summary>
    /// Collects the garbage using appropriate settings for both platforms. 
    /// It also tests if a garbage collection occurs.
    /// </summary>
    public sealed class GarbageCollector
    {

        #region Destructor

        /// <summary>
        /// The garbage collector is working.
        /// </summary>
        ~GarbageCollector()
        {
            throw new Exception("Garbage Collection performed.");
        } // ~TestGarbageCollection

        #endregion

        #region Create Weak Reference

        /// <summary>
        /// Creates a dummy object and immediately it is dereferenced,
        /// so when the garbage collector call its destructor we will know that a garbage collection was called.
        /// </summary>
        public static void CreateWeakReference()
        {
            new GarbageCollector();
        } // CreateWeakReference

        #endregion

        #region Collect Garbage

        /// <summary>
        ///  Collect garbage.
        /// </summary>
        /// <remarks>
        /// In PC WinForms does generate some garbage in its message handling infrastructure, 
        /// but this is all extremely short-lived and unlikely to make it into gen2, so it will be cheap to collect.
        /// </remarks>
        internal static void CollectGarbage()
        {
            // All generations will undergo a garbage collection.
            #if (WINDOWS)
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            #else
                GC.Collect();
            #endif
            // Enables garbage collection that is more conservative in reclaiming objects.
            // Full Collections occur only if the system is under memory pressure while generation 0 and generation 1 collections might occur more frequently.
            // This is the least intrusive mode.
            // If the work is done right, this latency mode is not need really.
            #if (WINDOWS)
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            #endif
        } // CollectGarbage

        #endregion

    } // GarbageCollector
} // XNAFinalEngine.Helpers