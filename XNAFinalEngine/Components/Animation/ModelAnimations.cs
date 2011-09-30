
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// The model animation component is used to play back model animations (skinned and rigid).
    /// This kind of animations does not affect the game object's transform.
    /// </summary>
    public class ModelAnimations : Component
    {
        /*
        #region Variables

        // Associated animations.
        private readonly Dictionary<string, ModelAnimationClip> modelAnimations = new Dictionary<string, ModelAnimationClip>(0);

        #region Current Animation

        // Clip currently being played
        ModelAnimationClip currentClip;
        // Current timeindex and keyframe in the clip
        float currentTimeValue;
        int currentKeyFrameIndex;
        // Speed of playback
        float playbackRate = 1.0f;
        // The amount of time for which the animation will play.
        // TimeSpan.MaxValue will loop forever. TimeSpan.Zero will play once. 
        float duration = float.MaxValue;
        // Amount of time elapsed while playing
        float elapsedPlaybackTime = 0;
        // Whether or not playback is paused
        bool paused;

        #endregion
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets/set the current play position.
        /// </summary>
        public float CurrentTimeValue
        {
            get { return currentTimeValue; }
            set
            {
                float time = value;

                // If the position moved backwards, reset the keyframe index.
                if (time < currentTimeValue)
                {
                    currentKeyFrameIndex = 0;
                    InitClip();
                }

                currentTimeValue = time;

                // Read keyframe matrices.
                IList<ModelKeyframe> keyframes = currentClip.Keyframes;
                while (currentKeyFrameIndex < keyframes.Count)
                {
                    ModelKeyframe keyframe = keyframes[currentKeyFrameIndex];
                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > currentTimeValue)
                        break;
                    // Use this keyframe

                    // Use this keyframe
                    SetKeyframe(keyframe);

                    currentKeyFrameIndex++;
                }
            }
        } // CurrentTimeValue

        #endregion

        #region Events

        /// <summary>
        /// Invoked when playback has completed.
        /// </summary>
        public event EventHandler AnimationCompleted;

        #endregion

        #region Play

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>        
        public void Play(string name)
        {
            Play(name, 1.0f, float.MaxValue);
        } // Play

        /// <summary>
        /// Starts playing a clip
        /// </summary>
        /// <param name="name">Animation name</param>
        /// <param name="playbackRate">Speed to playback</param>
        /// <param name="duration">Length of time to play in seconds (max is looping, 0 is once)</param>
        public void Play(string name, float playbackRate, float duration)
        {
            if (!modelAnimations.ContainsKey(name))
                throw new ArgumentException("Root Animation Component: the animation name does not exist.");

            // Store the clip and reset playing data            
            currentClip = modelAnimations[name];
            currentKeyFrameIndex = 0;
            CurrentTimeValue = 0;
            elapsedPlaybackTime = 0;
            paused = false;

            // Store the data about how we want to playback
            this.playbackRate = playbackRate;
            this.duration = duration;

            InitClip();
        } // Play

        #endregion

        #region Pause Resume

        /// <summary>
        /// Will pause the playback of the current clip
        /// </summary>
        public void Pause()
        {
            paused = true;
        } // PauseClip

        /// <summary>
        /// Will resume playback of the current clip
        /// </summary>
        public void Resume()
        {
            paused = false;
        } // ResumeClip

        #endregion

        #region Update

        /// <summary>
        /// Called during the update loop to move the animation forward
        /// </summary>        
        public virtual void Update()
        {
            if (currentClip == null)
                return;
            if (paused)
                return;

            // Adjust for the rate
            float time = Time.GameDeltaTime * playbackRate;

            elapsedPlaybackTime += time;

            // See if we should terminate
            if (elapsedPlaybackTime > duration && duration != 0 || elapsedPlaybackTime > currentClip.Duration && duration == 0)
            {
                if (AnimationCompleted != null)
                    AnimationCompleted(this, EventArgs.Empty);
                currentClip = null;
                return;
            }

            // Update the animation position.
            time += currentTimeValue;
            // If we reached the end, loop back to the start.
            while (time >= currentClip.Duration)
                time -= currentClip.Duration;
            CurrentTimeValue = time;

            
        } // Update

        #endregion
        
        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
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

        #region Contains Animation Clip

        /// <summary>
        /// Determines if the component contains a specific model animation.
        /// </summary>
        /// <remarks>Checks both, the name and the clip.</remarks>
        public bool ContainsAnimationClip(Assets.ModelAnimation animation)
        {
            return modelAnimations.ContainsValue(animation.AnimationClip) || modelAnimations.ContainsKey(animation.Name);
        } // ContainsAnimationClip

        #endregion

        #region Add Animation Clip

        /// <summary>
        /// Adds an animation clip to the component.
        /// </summary>
        public void AddAnimationClip(Assets.ModelAnimation animation)
        {
            if (!ContainsAnimationClip(animation))
                modelAnimations.Add(animation.Name, animation.AnimationClip);
            else
                throw new ArgumentException("Model Animation Component: The animation " + animation.Name + " is already assigned.");
        } // AddAnimationClip

        #endregion
        */
        #region Pool

        // Pool for this type of components.
        private static readonly Pool<ModelAnimations> modelAnimationPool = new Pool<ModelAnimations>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<ModelAnimations> ModelAnimationPool { get { return modelAnimationPool; } }

        #endregion
        
    } // ModelAnimation
} // XNAFinalEngine.Components
