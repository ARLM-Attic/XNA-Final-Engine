
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
using System.Windows.Forms;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using System.Threading;
#endregion

namespace XNAFinalEngine
{

    /// <summary>
    /// The execution starts here, in the main method.
    /// Before the XNA core will be up and running there are a set of task that are (or can be) performed, like updates.
    /// In this place there are also the exceptions managers. 
    /// If a library or an important file isn’t found or an exception was raised in the program’s execution,
    /// this exception will be propagated to this class.
    /// </summary>
    static class Program
    {

        #region Main

        /// <summary>
        /// Here the initial tasks are performed; the engine (and the aplication's logic) start; and the exceptions are managed.
        /// The main method doesn't accept input parameters.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                // User code for initial tasks.
                {
                    // Handle otherwise unhandled exceptions that occur in Windows Forms threads.
                    Application.ThreadException += UnhandledExceptions;
                }
                // End of user code
                StartEngine();
            } // try
            // If some dll will not found
            catch (System.IO.FileNotFoundException ex)
            {
                string filename = ex.FileName != null ? ex.FileName.Split(new char[] { ',' })[0] : "<no file specified>";
                if (filename.EndsWith(".dll") == false &&
                    filename.EndsWith(".exe") == false &&
                    filename != StringHelper.CutExtension(filename))
                    filename += ".dll";
                MessageBox.Show("Important file not found (" + filename + "), unable to execute.",
                                "XNA Final Engine", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            // If another exception occurs...
            catch (Exception ex)
            {
                MessageBox.Show("There was a critical error.\n\nDetails: " + ex.Message,
                                "XNA Final Engine", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        } // Main

        #endregion

        #region UnhandledExceptions

        /// <summary>
        /// This method handle otherwise unhandled exceptions that occur in Windows Forms threads.
        /// In other words, the JIT debugger won’t be load.
        /// </summary>
        private static void UnhandledExceptions(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("There was a critical error.\n\nDetails: " + e.Exception.Message,
                            "Final Engine", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        } // UnhandledExceptions

        #endregion

        #region Start application

        /// <summary>
        /// Start the engine and the aplication's logic.
        /// </summary>
        private static void StartEngine()
        {
            using (EngineManager game = new EngineManager())
            {   
                game.Run();
            }
        } // StartEngine

        #endregion

    } // Program
} // XNAFinalEngine