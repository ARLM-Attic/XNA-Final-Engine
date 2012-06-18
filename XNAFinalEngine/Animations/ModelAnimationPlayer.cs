
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
        private AnimationState animationState;

        // Current timeindex and keyframe in the clip.
        //private float currentTimeValue;
        private int   currentKeyframe;

        // This become true when the stop function is called with immediate in false.
        private bool stopWhenCicleFinishes;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationState AnimationState { get { return animationState; } }

        /// <summary>
        /// Get/Set the current key frame index.
        /// </summary>
        public int CurrentKeyFrame
        {
            get { return currentKeyframe; }
            set
            {
                IList<ModelKeyframe> keyframes = animationState.ModelAnimation.Resource.Keyframes;
                float time = keyframes[value].Time;
                CurrentTimeValue = time;
            }
        } // CurrentKeyFrame

        /// <summary>
        /// Gets/set the current play position.
        /// </summary>
        public float CurrentTimeValue
        {
            set
            {
                float time = value;

                // If the position moved backwards, reset the keyframe index.
                if (time < animationState.Time)
                    currentKeyframe = 0;
                
                animationState.Time = time;

                // Read keyframe matrices.
                IList<ModelKeyframe> keyframes = animationState.ModelAnimation.Resource.Keyframes;

                while (currentKeyframe < keyframes.Count)
                {
                    ModelKeyframe keyframe = keyframes[currentKeyframe];

                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > animationState.Time)
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

        /// <summary>
        /// Gets the current state (playing, paused, or stopped) 
        /// </summary>
        public MediaState State { get; private set; }

        #endregion

        #region Play

        /// <summary>
        /// Starts playing a clip.
        /// </summary>
        /// <param name="clip">Animation clip to play.</param>
        public void Play(AnimationState clip)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");
            // Store the clip and reset playing data            
            animationState = clip;
            currentKeyframe = 0;
            CurrentTimeValue = clip.Time;
            stopWhenCicleFinishes = false;
            State = MediaState.Playing;
        } // Play

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
            {
                animationState.Time = 0;
                State = MediaState.Stopped;
            }
                
            else
                stopWhenCicleFinishes = true;
        } // Stop

        #endregion

        #region Pause Resume

        /// <summary>
        /// Will pause the playback of the current clip
        /// </summary>
        public void PauseClip()
        {
            State = MediaState.Paused;
        } // PauseClip

        /// <summary>
        /// Will resume playback of the current clip
        /// </summary>
        public void ResumeClip()
        {
            State = MediaState.Playing;
        } // ResumeClip

        #endregion

        #region Update

        /// <summary>
        /// Called during the update loop to move the animation forward
        /// </summary>        
        public virtual void Update()
        {
            if (State == MediaState.Paused || State == MediaState.Stopped)
                return;

            // Adjust for the rate
            float time = Time.GameDeltaTime * animationState.Speed;

            // Update the animation position.
            time += animationState.Time;

            // See if we should terminate
            if (time >= animationState.ModelAnimation.Duration && animationState.WrapMode == WrapMode.Once)
            {
                Stop();
                return;
            }
            if (time >= animationState.ModelAnimation.Duration && animationState.WrapMode == WrapMode.ClampForever)
            {
                CurrentTimeValue = animationState.ModelAnimation.Duration;
                return;
            }

            // If we reached the end, loop back to the start.
            while (time >= animationState.ModelAnimation.Duration)
            {
                if (stopWhenCicleFinishes)
                {
                    Stop();
                    CurrentTimeValue = time;
                    return;
                }
                time -= animationState.ModelAnimation.Duration;
            }

            CurrentTimeValue = time;
        } // Update

        #endregion

    } // ModelAnimationPlayer
} // XNAFinalEngine.Animations