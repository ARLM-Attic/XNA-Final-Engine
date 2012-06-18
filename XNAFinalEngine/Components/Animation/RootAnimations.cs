
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// The root animation component is used to play back root animations.
    /// This kind of animations affects the game object's transform.
    /// </summary>
    public class RootAnimations : Component
    {

        #region Variables

        // Associated animations.
        private readonly Dictionary<string, RootAnimationClip> rootAnimations = new Dictionary<string, RootAnimationClip>(0);

        // Chaded model filter's model value.
        private Model cachedModel;

        #region Current Animation

        // Clip currently being played
        RootAnimationClip currentAnimationClip;
        // The animations are absolute, we need the previous animation's transformation matrix to make them relative.
        private Matrix previousAnimationTransform;
        // Current timeindex and keyframe in the clip
        float currentTimeValue;
        int currentKeyFrameIndex;
        private RootKeyframe currentKeyFrame;
        // Speed of playback
        float playbackRate = 1.0f;
        // The amount of time for which the animation will play.
        // TimeSpan.MaxValue will loop forever. TimeSpan.Zero will play once. 
        float duration = float.MaxValue;
        // Amount of time elapsed while playing
        float elapsedPlaybackTime = 0;
        // Whether or not playback is paused
        bool paused;
        // This become true when the stop function is called with immediate in false.
        private bool stopWhenCicleFinishes;

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
                    currentKeyFrame = currentAnimationClip.Keyframes[0];
                }

                currentTimeValue = time;

                // Read keyframe matrices.
                IList<RootKeyframe> keyframes = currentAnimationClip.Keyframes;
                while (currentKeyFrameIndex < keyframes.Count)
                {
                    RootKeyframe keyframe = keyframes[currentKeyFrameIndex];
                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > currentTimeValue)
                        break;
                    // Use this keyframe
                    currentKeyFrame = keyframe;
                    currentKeyFrameIndex++;
                }
            }
        } // CurrentTimeValue

        /// <summary>
        /// Gets the current state (playing, paused, or stopped) 
        /// </summary>
        public MediaState State
        {
            get
            {
                if (currentAnimationClip == null)
                    return MediaState.Stopped;
                if (paused)
                    return MediaState.Paused;
                return MediaState.Playing;
            }
        } // State
        
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
            if (!rootAnimations.ContainsKey(name))
                throw new ArgumentException("Root Animation Component: the animation name does not exist.");

            // Store the clip and reset playing data            
            currentAnimationClip = rootAnimations[name];
            currentKeyFrameIndex = 0;
            currentKeyFrame = currentAnimationClip.Keyframes[0];
            CurrentTimeValue = 0;
            elapsedPlaybackTime = 0;
            paused = false;

            // Store the data about how we want to playback
            this.playbackRate = playbackRate;
            this.duration = duration;

            previousAnimationTransform = ((GameObject3D)Owner).Transform.LocalMatrix;
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

        #region Stop

        /// <summary>
        /// Stops playing the sound.
        /// </summary>
        /// <param name="immediate">
        /// Specifies whether to stop playing immediately, or to break out of the loop region and play the release.
        /// Specify true to stop playing immediately, or false to break out of the loop region and play the release phase (the remainder of the sound).
        /// </param>
        public void Stop(bool immediate = true)
        {
            if (immediate)
                currentAnimationClip = null;
            else
                stopWhenCicleFinishes = true;
        } // Stop

        #endregion

        #region Update

        /// <summary>
        /// Called during the update loop to move the animation forward
        /// </summary>        
        public virtual void Update()
        {
            if (currentAnimationClip == null)
                return;
            if (paused)
                return;

            // Adjust for the rate
            float time = Time.GameDeltaTime * playbackRate;
            
            elapsedPlaybackTime += time;

            // See if we should terminate
            if (elapsedPlaybackTime > duration && duration != 0 || elapsedPlaybackTime > currentAnimationClip.Duration && duration == 0)
            {
                // Animation Completed
                currentAnimationClip = null;
                return;
            }

            // Update the animation position.
            time += currentTimeValue;
            // If we reached the end, loop back to the start.
            while (time >= currentAnimationClip.Duration)
            {
                if (stopWhenCicleFinishes)
                {
                    currentAnimationClip = null;
                    return;
                }
                time -= currentAnimationClip.Duration;
            }
                
            CurrentTimeValue = time;

            // Update transform Matrix.
            // The animation information is absolute but we need relative information so that different transformations could be applied to the transform at the same time.
            ((GameObject3D)Owner).Transform.LocalMatrix = currentKeyFrame.Transform * Matrix.Invert(previousAnimationTransform) * ((GameObject3D)Owner).Transform.LocalMatrix;
            previousAnimationTransform = currentKeyFrame.Transform;
        } // Update

        #endregion
      
        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Model
            OnModelChanged(null, ((GameObject3D)Owner).ModelFilter == null ? null : ((GameObject3D)Owner).ModelFilter.Model);
            ((GameObject3D)Owner).ModelFilterChanged += OnModelFilterChanged;
            if (((GameObject3D)Owner).ModelFilter != null)
            {
                ((GameObject3D)Owner).ModelFilter.ModelChanged += OnModelChanged;
            }
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            currentAnimationClip = null;
            cachedModel = null;
            ((GameObject3D)Owner).ModelFilterChanged -= OnModelFilterChanged;
            if (((GameObject3D)Owner).ModelFilter != null)
                ((GameObject3D)Owner).ModelFilter.ModelChanged -= OnModelChanged;
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region Contains Animation Clip

        /// <summary>
        /// Determines if the component contains a specific root animation.
        /// </summary>
        /// <remarks>Checks both, the name and the clip.</remarks>
        public bool ContainsAnimationClip(RootAnimation animation)
        {
            return rootAnimations.ContainsValue(animation.Resource) || rootAnimations.ContainsKey(animation.Name);
        } // ContainsAnimationClip

        /// <summary>
        /// Determines if the component contains a specific model animation.
        /// </summary>
        public bool ContainsAnimationClip(string name)
        {
            return rootAnimations.ContainsKey(name);
        } // ContainsAnimationClip

        #endregion

        #region Add Animation Clip

        /// <summary>
        /// Adds an animation clip to the component.
        /// </summary>
        public void AddAnimationClip(Assets.RootAnimation animation)
        {
            if (!ContainsAnimationClip(animation))
                rootAnimations.Add(animation.Name, animation.Resource);
            else
                throw new ArgumentException("Root Animation Component: The animation " + animation.Name + " is already assigned.");
        } // AddAnimationClip

        #endregion

        #region Remove Animation Clip

        /// <summary>
        /// Remove an animation clip to the component.
        /// </summary>
        public void RemoveAnimationClip(RootAnimation animation)
        {
            if (ContainsAnimationClip(animation))
                rootAnimations.Remove(animation.Name);
            else
                throw new ArgumentException("Model Animation Component: The animation " + animation.Name + " does not exist.");
        } // RemoveAnimationClip

        /// <summary>
        /// Remove an animation clip to the component.
        /// </summary>
        public void RemoveAnimationClip(string name)
        {
            if (ContainsAnimationClip(name))
                rootAnimations.Remove(name);
            else
                throw new ArgumentException("Model Animation Component: The animation " + name + " does not exist.");
        } // RemoveAnimationClip

        #endregion

        #region On Model Changed

        /// <summary>
        /// On model filter's model changed.
        /// </summary>
        private void OnModelChanged(object sender, Model model)
        {
            if (cachedModel != null && cachedModel is FileModel)
            {
                foreach (var rootAnimation in ((FileModel)cachedModel).RootAnimations)
                {
                    RemoveAnimationClip(rootAnimation);
                }
            }
            cachedModel = model;
            if (cachedModel != null && cachedModel is FileModel)
            {
                foreach (var rootAnimation in ((FileModel)cachedModel).RootAnimations)
                {
                    AddAnimationClip(rootAnimation);
                }
            }
        } // OnModelChanged

        #endregion

        #region On Model Filter Changed

        /// <summary>
        /// On model filter changed.
        /// </summary>
        private void OnModelFilterChanged(object sender, Component oldComponent, Component newComponent)
        {
            // Remove event association.
            if (oldComponent != null)
                ((ModelFilter)oldComponent).ModelChanged -= OnModelChanged;
            // Add new event association
            if (newComponent != null)
            {
                ((ModelFilter)newComponent).ModelChanged += OnModelChanged;
            }
            OnModelChanged(null, ((GameObject3D)Owner).ModelFilter == null ? null : ((GameObject3D)Owner).ModelFilter.Model);
        } // OnModelFilterChanged

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<RootAnimations> componentPool = new Pool<RootAnimations>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<RootAnimations> ComponentPool { get { return componentPool; } }

        #endregion

    } // RootAnimation
} // XNAFinalEngine.Components
