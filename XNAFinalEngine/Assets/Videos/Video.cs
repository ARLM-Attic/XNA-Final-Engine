
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
using System.IO;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Video.
    /// </summary>
    public class Video : Asset
    {
        
        #region Properties

        /// <summary>
        /// XNA video.
        /// </summary>
        public Microsoft.Xna.Framework.Media.Video Resource { get; private set; }

        /// <summary>
        /// Gets the duration of the video (in seconds)
        /// </summary>
        public float Duration { get { return (float)Resource.Duration.TotalSeconds; } }

        /// <summary>
        /// Gets the frame rate of this video.
        /// </summary>
        /// <value>The number of frames this video displays per second.</value>
        /// <remarks>24, 30, and 29.97 are common frame rates.</remarks>
        public float FramesPerSecond { get { return Resource.FramesPerSecond; } }

        /// <summary>
        /// Gets the height of this video, in pixels.
        /// </summary>
        public int Height { get { return Resource.Height; } }

        /// <summary>
        /// Gets the width of this video, in pixels.
        /// </summary>
        public int Width { get { return Resource.Width; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Load video.
        /// </summary>
        /// <param name="filename">The filename must be relative and be a valid file in the video directory.</param>
        public Video(string filename)
        {
            Name = filename;
            Filename = AssetContentManager.GameDataDirectory + "Videos\\" + filename;
            if (File.Exists(Filename + ".xnb") == false)
            {
                throw new Exception("Failed to load sound: File " + Filename + " does not exists!");
            }
            try
            {
                Resource = AssetContentManager.CurrentContentManager.XnaContentManager.Load<Microsoft.Xna.Framework.Media.Video>(Filename);
            }
            catch (ObjectDisposedException e)
            {
                throw new Exception("Content Manager: Content manager disposed", e);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load video: " + filename, e);
            }
        } // Video

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal override void RecreateResource()
        {
            Resource = AssetContentManager.CurrentContentManager.XnaContentManager.Load<Microsoft.Xna.Framework.Media.Video>(Filename);
        } // RecreateResource

        #endregion

    } // Video
} // XNAFinalEngineBase.Assets
