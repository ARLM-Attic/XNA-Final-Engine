
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
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Song.
    /// </summary>
    /// <remarks>
    /// Because a content processor bug the song information (name, artist) can't be extracted. 
    /// Because of that it uses the file name to extract this information. 
    /// Filename format: index - artist - song name, where index is the song number in the list.
    /// However the filename format: "artist - song name" works.
    /// </remarks>
    public class Song : Asset
    {
        
        #region Properties

        /// <summary>
        /// XNA song.
        /// </summary>
        public Microsoft.Xna.Framework.Media.Song Resource { get; private set; }
        
        /// <summary>
        /// Gets the Artist of the Song.
        /// Resource.Artist.Name doesn't work. It's a content processor compilation bug.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets the duration of the Song (in seconds)
        /// </summary>
        public float Duration { get { return (float)Resource.Duration.TotalSeconds; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Load song.
        /// </summary>
        /// <param name="filename">The filename must be relative and be a valid file in the music directory.</param>
        public Song(string filename)
        {            
            string fullFilename = ContentManager.GameDataDirectory + "Music\\" + filename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load song: File " + fullFilename + " does not exists!");
            }
            try
            {
                Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<Microsoft.Xna.Framework.Media.Song>(fullFilename);
                ContentManager = ContentManager.CurrentContentManager;
            }
            catch (ObjectDisposedException e)
            {
                throw new Exception("Content Manager: Content manager disposed", e);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load song: " + filename, e);
            }
            // Resource.Artist.Name and Resource.Name don't always work.
            // It’s better to use the song filename but the format has to be respected.
            // File name format: index - Artist - song name, where index is the song number in the list.
            try
            {
                Artist = filename.Split('-')[1];
                Name = filename.Split('-')[2];
            }
            catch // If there is an error maybe the index is not set.
            {
                try
                {
                    Artist = filename.Split('-')[0];
                    Name = filename.Split('-')[1];
                }
                catch // If there is an error we are...
                {
                    Artist = "Unknown";
                    Name = filename;
                }
            }
        } // Song

        #endregion
        
    } // Song
} // XNAFinalEngineBase.Assets
