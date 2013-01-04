
#region License
/*
Copyright (c) 2008-2013, Schneider, José Ignacio.
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
Author: Schneider, José Ignacio (jischneider@hotmail.com)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace XNAFinalEngine.Helpers
{
    /// <summary>
    /// This helps track the available processors.
    /// In PC, Windows manages the threads, but in Xbox 360 its necesarry to define the affinity.
    /// </summary>
    /// <remarks>
    /// Threading in XNA Final Engine is fairly simple. 
    /// Thanks to the data oriented design, the task could be performed in serial mode spreading the work into different cores and therefore avoiding asynchronic updates.
    /// 
    /// The game loop asks for the hardware threads available and divides the work in them. 
    /// In Xbox 360 the engine uses the core 1, 3, 4 and 5; 0 and 2 are used by XNA (or the operative system).
    /// In PC, there is a hardware thread for each available core. I assume that the cores are not being used;
    /// however, the core that runs the application makes a little more work than the others to avoid a potential bottleneck,
    /// moreover, small test performed shows me that this is better for performance. 
    /// </remarks>
    public static class ProcessorsInformation
    {

        #region Processor

        public class Processor
        {
            #if XBOX
                /// <summary>
                /// Indicate the affinity that has to be set to this virtual processor.
                /// </summary>
                public int Affinity { get; internal set; }
            #endif

            internal Processor() { }
        } // Processor

        #endregion

        #region Variables

        /// <summary>
        /// List of virtual processors.
        /// </summary>
        private static readonly List<Processor> availableProccesors;

        #endregion

        #region Properties

        /// <summary>
        /// The number of processors availables. 
        /// The application thread is not included.
        /// </summary>
        public static int AvailableProcessors { get; private set; }     
        
        #endregion

        #region Constructor

        /// <summary>
        /// Obtain the number of logical cores available for multithreading.
        /// </summary>
        static ProcessorsInformation()
        {
            #if XBOX 
                // http://msdn.microsoft.com/en-us/library/microsoft.xna.net_cf.system.threading.thread.setprocessoraffinity.aspx
                AvailableProcessors = 3;
            #else
                AvailableProcessors = Environment.ProcessorCount - 1; // Minus the application thread.
            #endif

            availableProccesors = new List<Processor>(AvailableProcessors);

            for (int i = 0; i < AvailableProcessors; i++)
            {
                Processor processor = new Processor();
                availableProccesors.Add(processor);
            }
            #if XBOX
                availableProccesors[0].Affinity = 3;
                availableProccesors[0].Affinity = 4;
                availableProccesors[0].Affinity = 5;
            #endif
        } // ProcessorsInformation

        #endregion

        #region Fetch

        /// <summary>
        /// Return a token that correspond to an available processor.
        /// </summary>
        public static Processor Fetch()
        {
            if (AvailableProcessors > 0)
            {
                AvailableProcessors--;
                Processor processor = availableProccesors[AvailableProcessors];
                availableProccesors.Remove(processor);
                return processor;
            }
            return null;
        } // Fetch

        #endregion
        
        #region Release

        /// <summary>
        /// Set the processor to available.
        /// </summary>
        public static void Release(Processor processor)
        {
            availableProccesors.Add(processor);
            AvailableProcessors++;
        } // Release

        #endregion

    } // ProcessorsInformation
} // XNAFinalEngine.Helpers