
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
using XNAFinalEngine.Helpers;

#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// Useful statistics to profile the game quickly.
    /// But when you need to deeply profile your game use a real profiler.
    /// PIX and Visual Studio’s profiler are powerful tools. You should use them.
    /// </summary>
    /// <remarks>
    /// The time measures (frame per seconds, update time and frame time) were left in the Time class for organization sake.
    /// </remarks>
    public static class Statistics
    {

        #region Properties

        /// <summary>
        /// The number of triangles processed in the current frame.
        /// </summary>
        public static int TrianglesDrawn { get; internal set; }
        
        /// <summary>
        /// The number of vertices processed in the current frame.
        /// </summary>
        public static int VerticesProcessed { get; internal set; }

        /// <summary>
        /// The number of draw calls performed in the current frame.
        /// </summary>
        /// <remarks>
        /// A draw call occurs every time you call one of the GraphicsDevice.Draw.
        /// When using Model, you get one draw call per mesh part.
        /// </remarks>
        public static int DrawCalls { get; internal set; }

        /// <summary>
        /// The number of garbage collections performed in execution time.
        /// </summary>
        public static int GarbageCollections { get; internal set; }

        /// <summary>
        /// Currently allocated memory in the managed heap.
        /// </summary>
        /// <remarks>
        /// If no C++ library is used then probably all the memory will be managed except the data allocated in the GPU.
        /// </remarks>
        public static long ManagedMemoryUsed { get { return GC.GetTotalMemory(false); } }

        #endregion

        #region Init Statistcis

        /// <summary>
        /// Init the statistics. Call it after the first garbage collection.
        /// </summary>
        internal static void InitStatistics()
        {
            GarbageCollector.CreateWeakReference();
        } // InitStatistics

        #endregion

        #region Reset Frame Statistics

        /// <summary>
        /// Reset to 0 the values that are measured frame by frame.
        /// </summary>
        internal static void ReserFrameStatistics()
        {
            TrianglesDrawn = 0;
            VerticesProcessed = 0;
            DrawCalls = 0;
        } // ReserFrameStatistics

        #endregion

    } // Statistics
} // XNAFinalEngine.EngineCore