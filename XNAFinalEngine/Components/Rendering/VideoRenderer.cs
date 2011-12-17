
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Helpers;
using Video = XNAFinalEngine.Assets.Video;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Video Renderer.
    /// </summary>
    public class VideoRenderer : Renderer
    {

        #region Variables

        // The video player for this video.
        private VideoPlayer videoPlayerInstance;

        // To allow the volume range check.
        private float volume;
        
        // The current frame.
        private readonly Assets.Texture texture = new Assets.Texture();

        #endregion

        #region Properties

        /// <summary>
        /// Chaded transform2D's world matrix value.
        /// </summary>
        internal Vector3 CachedPosition;

        /// <summary>
        /// Video.
        /// </summary>
        public Video Video { get; set; }

        /// <summary>
        /// Gets or sets the muted setting for the video player.
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the player is playing video in a loop.
        /// </summary>
        public bool IsLooped { get; set; }

        /// <summary>
        /// Gets or sets the video player volume.
        /// </summary>
        /// <value>
        /// Video player volume, from 0.0f (silence) to 1.0f (full volume relative to the current device volume).
        /// </value>
        /// <remarks>
        /// Volume adjustment is based on a decibel, not multiplicative, scale. For example,
        /// when the device volume is half of maximum (about 7 in the Windows Phone user interface),
        /// setting Volume to 0.6f or less is silent or nearly so, not volume 4 as you would expect from a multiplicative adjustment. 
        /// 
        /// Setting Volume to 0.0 subtracts 96 dB from the volume.
        /// Setting Volume to 1.0 subtracts 0 dB from the volume.
        /// Values in between 0.0f and 1.0f subtract dB from the volume proportionally.
        /// </remarks>
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (volume < 0)
                    volume = 0;
                if (volume > 1)
                    volume = 1;
            }
        } // Volume

        /// <summary>
        /// Gets the current state (playing, paused, or stopped) 
        /// </summary>
        public MediaState State
        {
            get
            {
                if (videoPlayerInstance == null)
                    return MediaState.Stopped;
                return videoPlayerInstance.State;
            }
        } // State

        /// <summary>
        /// Gets the play position within the currently playing video (in seconds).
        /// </summary>
        public float PlayPosition
        {
            get
            {
                if (videoPlayerInstance != null)
                    return (float)(videoPlayerInstance.PlayPosition.TotalSeconds);
                return 0;
            }
        } // PlayPosition

        /// <summary>
        /// Retrieves a texture containing the current frame of video being played.
        /// </summary>
        public Assets.Texture Texture
        {
            get
            {
                if (State != MediaState.Stopped)
                    return texture;
                return null;
            }
        } // Texture
        
        #endregion

        #region Events

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        public delegate void VideoEventHandler(object sender, Video video);

        /// <summary>
        /// Raised when the video ends.
        /// </summary>
        public event VideoEventHandler VideoEnded;

        #endregion

        #region Play, Pause, Resume, Stop

        /// <summary>
        /// Plays or resumes a video. 
        /// </summary>
        /// <remarks>
        /// An video renderer could only play one video instance at a time.
        /// </remarks>
        public void Play()
        {
            if (videoPlayerInstance == null)
            {
                videoPlayerInstance = new VideoPlayer();// VideoManager.FetchSoundInstance(Video); // TODO
                // If the sound instance could not be created then do nothing.
                if (videoPlayerInstance == null)
                    return;
                videoPlayerInstance.IsMuted = IsMuted;
                videoPlayerInstance.IsLooped = IsLooped;
                videoPlayerInstance.Volume = Volume;
                videoPlayerInstance.Play(Video.Resource);
            }
            else if (videoPlayerInstance.State == MediaState.Paused)
            {
                videoPlayerInstance.Resume();
            }
        } // Play

        /// <summary>
        /// Stops playing the video.
        /// </summary>
        /// <remarks>The sound instance is disposed.</remarks>
        public void Stop()
        {
            if (videoPlayerInstance != null)
            {
                // Dispose sound effect instance to avoid garbage collection.
                //VideoManager.ReleaseSoundInstance(videoPlayerInstance);
                videoPlayerInstance.Dispose(); // TODO!!
                videoPlayerInstance = null;
                // Raise event
                if (VideoEnded != null)
                    VideoEnded(this, Video);
            }
        } // Stop

        /// <summary>
        /// Pauses the video.
        /// </summary>
        public void Pause()
        {
            if (videoPlayerInstance != null && videoPlayerInstance.State == MediaState.Playing)
            {
                videoPlayerInstance.Pause();
            }
        } // Pause

        /// <summary>
        /// Resumes playback for this video.
        /// </summary>
        public void Resume()
        {
            if (videoPlayerInstance != null && videoPlayerInstance.State == MediaState.Paused)
            {
                videoPlayerInstance.Resume();
            }
        } // Resume

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        internal void Update()
        {
            if (videoPlayerInstance != null)
            {
                // If the video ends.
                if (videoPlayerInstance.State == MediaState.Stopped)
                {
                    Stop();
                }
                else
                {
                    videoPlayerInstance.IsMuted = IsMuted;
                    videoPlayerInstance.IsLooped = IsLooped;
                    videoPlayerInstance.Volume = Volume;
                    texture.Resource = videoPlayerInstance.GetTexture();    
                }
            }
        } // Update

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Set default values.
            volume = 1;
            IsMuted = false;
            IsLooped = false;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected override void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            cachedWorldMatrix = worldMatrix;
            // We could pass directly the calculated values. In this case there are calculated using the world matrix.
            CachedPosition = cachedWorldMatrix.Translation;
        } // OnWorldMatrixChanged

        #endregion
        
        #region Pool

        // Pool for this type of components.
        private static readonly Pool<VideoRenderer> componentPool = new Pool<VideoRenderer>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<VideoRenderer> ComponentPool { get { return componentPool; } }

        #endregion

    } // VideoRenderer
} // XNAFinalEngine.Components
