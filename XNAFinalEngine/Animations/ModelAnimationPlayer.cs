
#region License
//-----------------------------------------------------------------------------
// ModelAnimationPlayerBase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
#endregion

namespace XNAFinalEngine.Animations
{
    /// <summary>
    /// This plays any model animation clip.
    /// This need to be refactored and extended.
    /// </summary>
    internal class ModelAnimationPlayer
    {

        #region Structs

        /// <summary>
        /// Position, rotation and scale applied the bone.
        /// </summary>
        public struct BoneTransformationData
        {
            internal Vector3 position;
            internal Quaternion rotation;
            internal float scale;
        } // BoneTransformationData

        #endregion

        #region Variables

        // This is an array of the transforms to each object/bone in the model.
        public readonly BoneTransformationData[] BoneTransforms = new BoneTransformationData[ModelAnimationClip.MaxBones];

        // Clip currently being played.
        private ModelAnimation currentAnimationClip;

        // Current timeindex and keyframe in the clip.
        private float currentTimeValue;
        private int   currentKeyframe;

        // Speed of playback.
        private float playbackRate = 1.0f;

        // The amount of time for which the animation will play.
        // MaxValue will loop forever. Zero will play once.
        private float duration = float.MaxValue;

        // Amount of time elapsed while playing.
        private float elapsedPlaybackTime = 0;

        // Whether or not playback is paused.
        private bool paused;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public ModelAnimation Animation { get { return currentAnimationClip; } }

        /// <summary>
        /// Get/Set the current key frame index.
        /// </summary>
        public int CurrentKeyFrame
        {
            get { return currentKeyframe; }
            set
            {
                IList<ModelKeyframe> keyframes = currentAnimationClip.Resource.Keyframes;
                float time = keyframes[value].Time;
                CurrentTimeValue = time;
            }
        } // CurrentKeyFrame

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
                    currentKeyframe = 0;
                    InitClip();
                }

                currentTimeValue = time;

                // Read keyframe matrices.
                IList<ModelKeyframe> keyframes = currentAnimationClip.Resource.Keyframes;

                while (currentKeyframe < keyframes.Count)
                {
                    ModelKeyframe keyframe = keyframes[currentKeyframe];

                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > currentTimeValue)
                        break;

                    // Use this keyframe
                    BoneTransforms[keyframe.Bone] = new BoneTransformationData
                    {
                        position = keyframe.Position,
                        rotation = keyframe.Rotation,
                        scale = keyframe.Scale
                    };

                    currentKeyframe++;
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
        public void Play(ModelAnimation clip)
        {
            Play(clip, 1.0f, float.MaxValue);
        } // Play

        /// <summary>
        /// Starts playing a clip
        /// </summary>
        /// <param name="clip">Animation clip to play</param>
        /// <param name="playbackRate">Speed to playback</param>
        /// <param name="duration">Length of time to play (max is looping, 0 is once)</param>
        public void Play(ModelAnimation clip, float playbackRate, float duration)
        {
            if (clip == null)
                throw new ArgumentNullException("Clip required");

            // Store the clip and reset playing data            
            currentAnimationClip = clip;
            currentKeyframe = 0;
            CurrentTimeValue = 0;
            elapsedPlaybackTime = 0;
            paused = false;

            // Store the data about how we want to playback
            this.playbackRate = playbackRate;
            this.duration = duration;

            // Call the virtual to allow initialization of the clip
            InitClip();
        } // Play

        /// <summary>
        /// Initializes the animation clip
        /// </summary>
        private void InitClip()
        {
            for (int i = 0; i < BoneTransforms.Length; i++)
            {
                BoneTransforms[i] = new BoneTransformationData
                {
                    position = Vector3.Zero,
                    rotation = Quaternion.Identity,
                    scale = 1
                };
            }
        } // InitClip

        #endregion

        #region Pause Resume

        /// <summary>
        /// Will pause the playback of the current clip
        /// </summary>
        public void PauseClip()
        {
            paused = true;
        } // PauseClip

        /// <summary>
        /// Will resume playback of the current clip
        /// </summary>
        public void ResumeClip()
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
                if (AnimationCompleted != null)
                    AnimationCompleted(this, EventArgs.Empty);
                currentAnimationClip = null;
                return;
            }

            // Update the animation position.
            time += currentTimeValue;

            // If we reached the end, loop back to the start.
            while (time >= currentAnimationClip.Duration)
                time -= currentAnimationClip.Duration;

            CurrentTimeValue = time;
        } // Update

        #endregion

    } // ModelAnimationPlayer
} // XNAFinalEngine.Animations