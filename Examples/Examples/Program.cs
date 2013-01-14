
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
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// The execution starts here, in the main method.
    /// Before the Engine core will be up and running there are a set of task that are (or can be) performed, like network updates.
    /// </summary>
    static class Program
    {

        #region Main

        /// <summary>
        /// Here the initial tasks are performed; the engine (and the aplication's logic) start; and the exceptions are managed.
        /// The main method doesn't accept input parameters.
        /// </summary>
        #if (!XBOX)
            [MTAThread]
        #endif
        private static void Main()
        {
            // User initial code. Like network updates or some checking.

            // Now the engine will start.
            //EngineManager.StartEngine(new DudeEditableScene(), false);
            //EngineManager.StartEngine(new PrototypeScene(), false);
            EngineManager.StartEngine(new WarehouseScene(), false);
            //EngineManager.StartEngine(new PhysicsTestScene(), false);
            //EngineManager.StartEngine(new LamborghiniMurcielagoScene(), false);
            //EngineManager.StartEngine(new NeoForceTestScene());
            //EngineManager.StartEngine(new HelloWorldScene(), false);
        } // Main

        #endregion

    } // Program
} // XNAFinalEngineExamples