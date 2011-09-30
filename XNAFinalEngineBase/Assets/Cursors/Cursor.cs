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
using System.IO;
using System.Xml.Linq;
#endregion

namespace XNAFinalEngine.Assets
{

#if (WINDOWS)

	/// <summary>
	/// Cursor
	/// </summary>
    public class Cursor : Asset
    {
        
        #region Properties

        /// <summary>
        /// Internal X Document.
        /// </summary>
        public System.Windows.Forms.Cursor SystemCursor { get; private set; }
        
        #endregion

        #region Constructor

		/// <summary>
		/// Load a cursor.
		/// </summary>
        /// <param name="filename">>The filename must be relative and be a valid file in the Cursors directory</param>
        public Cursor(string filename)
		{
            Name = filename;
            string fullFilename = ContentManager.GameDataDirectory + "Cursors\\" + filename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load cursor: File " + fullFilename + " does not exists!", "filename");
            }
            try
            {
                SystemCursor = ContentManager.CurrentContentManager.XnaContentManager.Load<System.Windows.Forms.Cursor>(fullFilename);
            }
            catch (ObjectDisposedException)
            {
                throw new InvalidOperationException("Content Manager: Content manager disposed");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load cursor: " + filename, e);
            }
        } // Cursor

		#endregion

    } // Document
#endif
} // XNAFinalEngine.Assets
