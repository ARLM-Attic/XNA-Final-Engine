
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Play windows media video (wmv) files. The maximum video size is 1280x720, the highest resolution of the XBOX 360.
    /// We can obtain XNA textures of each frame, or we can show them automatically in the screen.
    /// http://klucher.com/blog/video-support-in-xna-game-studio-3-1/
    /// http://blogs.msdn.com/b/ashtat/archive/2009/06/20/video-playback-in-xna-game-studio-3-1.aspx
    /// </summary>
    public class Video
    {

        #region Variables

        /// <summary>
        /// The name of the video file.
        /// </summary>
        private String videoFilename = "";

        /// <summary>
        /// The video.
        /// </summary>
        private Microsoft.Xna.Framework.Media.Video video;

        /// <summary>
        /// The player of this video.
        /// </summary>
        private VideoPlayer videoPlayer;

        /// <summary>
        /// Each video frame is transform into a sprite. This class uses a particular sprite batch.
        /// </summary>
        private static SpriteBatch spritebatch = null;

        /// <summary>
        /// It’s an auxiliary that indicates how to cut the screen space to maintain the video’s aspect ratio.
        /// </summary>
        private Rectangle screenRectangle;

        #endregion

        #region Properties

        /// <summary>
        /// Get the texture of the current frame.
        /// </summary>
        public Texture2D xnaTexture { get { return videoPlayer.GetTexture(); } }

        /// <summary>
        /// Mute or unmute the video's sound. 
        /// </summary>
        public bool IsMuted
        {
            get { return videoPlayer.IsMuted;  }
            set { videoPlayer.IsMuted = value;  }
        } // IsMuted

        /// <summary>
        /// Indicates whether the playing video is in a loop.
        /// </summary>
        public bool IsLooped
        {
            get { return videoPlayer.IsLooped; }
            set { videoPlayer.IsLooped = value; }
        } // IsLooped

        /// <summary>
        /// Video volumen.
        /// </summary>
        public float Volume
        {
            get { return videoPlayer.Volume; }
            set { videoPlayer.Volume = value; }
        } // Volume

        /// <summary>
        /// Gets the XNA video playback state.
        /// </summary>
        public MediaState State { get { return videoPlayer.State; } }

        /// <summary>
        /// Gets the play position within the video.
        /// </summary>
        public TimeSpan PlayPosition { get { return videoPlayer.PlayPosition; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Load the video.
        /// </summary>
        public Video(string _videoFilename, bool _isMuted, bool _isLooped)
        {   
            videoFilename = _videoFilename;
            string fullFilename = Directories.VideosDirectory + "\\" + videoFilename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load video: File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                video = EngineManager.Content.Load<Microsoft.Xna.Framework.Media.Video>(fullFilename);
            } // try
            catch (Exception)
            {
                throw new Exception("Failed to load video");
            }
            videoPlayer = new VideoPlayer();
            videoPlayer.IsMuted = _isMuted;
            videoPlayer.IsLooped = _isLooped;
            
            // Correccion de aspect ratio
            float videoAspectRatio = (float)video.Width / (float)video.Height,
                  screenAspectRatio = (float)EngineManager.Width / (float)EngineManager.Height;
            
            if (videoAspectRatio > screenAspectRatio)
            {
                float vsAspectRatio = videoAspectRatio / screenAspectRatio;
                int blackStripe = (int)((EngineManager.Height - (EngineManager.Height / vsAspectRatio)) / 2);
                screenRectangle = new Rectangle(0, 0 + blackStripe, EngineManager.Width, EngineManager.Height - blackStripe * 2);
            }
            else
            {
                float vsAspectRatio = screenAspectRatio / videoAspectRatio;
                int blackStripe = (int)((EngineManager.Width - (EngineManager.Width / vsAspectRatio)) / 2);
                screenRectangle = new Rectangle(0 + blackStripe, 0, EngineManager.Width - blackStripe * 2, EngineManager.Height);
            }
        } // Video

        /// <summary>
        /// Load the video. Assumed that isn’t muted and it doesn’t repeat.
        /// </summary>
        public Video(string _videoFilename) : this(_videoFilename, false, false) { }

        #endregion

        #region Stop Play Pause

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            videoPlayer.Stop();
        } // Stop

        /// <summary>
        /// Play from the start.
        /// </summary>
        public void Play()
        {
            videoPlayer.Play(video);
        } // Play

        /// <summary>
        /// Pause.
        /// </summary>
        public void Pause()
        {
            videoPlayer.Pause();
        } // Pause

        /// <summary>
        /// Resume.
        /// </summary>
        public void Resume()
        {
            videoPlayer.Resume();
        } // Resume

        #endregion

        #region Render

        /// <summary>
        /// Render the video into full screen.
        /// </summary>
        public void Render()
        {
            if (videoPlayer.State == MediaState.Playing)
            {
                EngineManager.Device.Clear(Color.Black);
                
                if (spritebatch == null)
                {
                    spritebatch = new SpriteBatch(EngineManager.Device);
                }
                spritebatch.Begin();

                spritebatch.Draw(videoPlayer.GetTexture(), screenRectangle, Color.White);

                spritebatch.End();
            }
        } // Render

        #endregion

    } // Video
} // XNAFinalEngine.GraphicElements
