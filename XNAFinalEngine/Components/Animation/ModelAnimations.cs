﻿
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
using XNAFinalEngine.Animations;
using XNAFinalEngine.Assets;
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
        
        #region Variables

        // Associated animations.
        private readonly Dictionary<string, ModelAnimation> modelAnimations = new Dictionary<string, ModelAnimation>(0);

        // Current bone transform matrices in absolute format.
        // They have to be transform to world space and if the model is skinned they have to be transformed by the inverse bind pose.
        private readonly Matrix[] boneTransform = new Matrix[ModelAnimationClip.MaxBones];

        private readonly ModelAnimationPlayer animationPlayer = new ModelAnimationPlayer();

        // Chaded model filter's model value.
        private Model cachedModel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/set the current play position.
        /// </summary>
        public float CurrentTimeValue
        {
            get { return animationPlayer.CurrentTimeValue; }
            set { animationPlayer.CurrentTimeValue = value; }
        } // CurrentTimeValue

        /// <summary>
        /// Gets the current state (playing, paused, or stopped) 
        /// </summary>
        public AnimationState State { get { return animationPlayer.State; } }
        
        /// <summary>
        /// Current bone transform matrices in absolute format.
        /// They have to be transform to world space and if the model is skinned they have to be transformed by the inverse bind pose.
        /// </summary>
        public Matrix[] BoneTransform { get { return boneTransform; } }

        #endregion

        #region Events

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        internal delegate void AnimationEventHandler(object sender, Matrix[] boneTransform);

        /// <summary>
        /// Raised when the model animation's bone transform changes.
        /// </summary>
        internal event AnimationEventHandler BoneTransformChanged;

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
            // This is provisory, the animation system is in diapers.
            animationPlayer.Play(modelAnimations[name]);
        } // Play

        #endregion

        #region Pause Resume

        /// <summary>
        /// Will pause the playback of the current clip
        /// </summary>
        public void Pause()
        {
            animationPlayer.PauseClip();
        } // PauseClip

        /// <summary>
        /// Will resume playback of the current clip
        /// </summary>
        public void Resume()
        {
            animationPlayer.ResumeClip();
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
            animationPlayer.Stop(immediate);
        } // Stop

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        internal void Update()
        {
            if (animationPlayer.State == AnimationState.Playing)
            {
                animationPlayer.Update();
                for (int bone = 0; bone < ModelAnimationClip.MaxBones; bone++)
                {
                    boneTransform[bone] = Matrix.CreateScale(animationPlayer.BoneTransforms[bone].scale) *
                                          Matrix.CreateFromQuaternion(animationPlayer.BoneTransforms[bone].rotation) *
                                          Matrix.CreateTranslation(animationPlayer.BoneTransforms[bone].position);
                }
                if (BoneTransformChanged != null)
                    BoneTransformChanged(this, boneTransform);
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
            base.Uninitialize();
            cachedModel = null;
            BoneTransformChanged = null;
            ((GameObject3D)Owner).ModelFilterChanged -= OnModelFilterChanged;
            if (((GameObject3D)Owner).ModelFilter != null)
                ((GameObject3D)Owner).ModelFilter.ModelChanged -= OnModelChanged;
        } // Uninitialize

        #endregion

        #region Contains Animation Clip

        /// <summary>
        /// Determines if the component contains a specific model animation.
        /// </summary>
        /// <remarks>Checks both, the name and the clip.</remarks>
        public bool ContainsAnimationClip(ModelAnimation animation)
        {
            return modelAnimations.ContainsValue(animation) || modelAnimations.ContainsKey(animation.Name);
        } // ContainsAnimationClip
        
        /// <summary>
        /// Determines if the component contains a specific model animation.
        /// </summary>
        public bool ContainsAnimationClip(string name)
        {
            return modelAnimations.ContainsKey(name);
        } // ContainsAnimationClip

        #endregion

        #region Add Animation Clip

        /// <summary>
        /// Adds an animation clip to the component.
        /// </summary>
        public void AddAnimationClip(ModelAnimation animation)
        {
            if (!ContainsAnimationClip(animation))
                modelAnimations.Add(animation.Name, animation);
            else
                throw new ArgumentException("Model Animation Component: The animation " + animation.Name + " is already assigned.");
        } // AddAnimationClip

        #endregion

        #region Remove Animation Clip

        /// <summary>
        /// Remove an animation clip to the component.
        /// </summary>
        public void RemoveAnimationClip(ModelAnimation animation)
        {
            if (ContainsAnimationClip(animation))
                modelAnimations.Remove(animation.Name);
            else
                throw new ArgumentException("Model Animation Component: The animation " + animation.Name + " does not exist.");
        } // RemoveAnimationClip

        /// <summary>
        /// Remove an animation clip to the component.
        /// </summary>
        public void RemoveAnimationClip(string name)
        {
            if (ContainsAnimationClip(name))
                modelAnimations.Remove(name);
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
                foreach (var modelAnimation in ((FileModel)cachedModel).ModelAnimations)
                {
                    RemoveAnimationClip(modelAnimation);
                }
            }
            cachedModel = model;
            // If the model is skined initialize the bone transform with the bind pose.
            if (model != null && model is FileModel && ((FileModel)model).IsSkinned)
            {
                for (int i = 0; i < ((FileModel)model).BindPose.Count; i++)
                {
                    boneTransform[i] = ((FileModel)model).BindPose[i];
                }
            }
            else
            {
                // If not use the identity transformation.
                for (int i = 0; i < boneTransform.Length; i++)
                {
                    boneTransform[i] = Matrix.Identity;
                }
            }
            if (BoneTransformChanged != null)
                BoneTransformChanged(this, boneTransform);
            if (cachedModel != null && cachedModel is FileModel)
            {
                foreach (var modelAnimation in ((FileModel)cachedModel).ModelAnimations)
                {
                    AddAnimationClip(modelAnimation);
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
        private static readonly Pool<ModelAnimations> componentPool = new Pool<ModelAnimations>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<ModelAnimations> ComponentPool { get { return componentPool; } }

        #endregion
        
    } // ModelAnimation
} // XNAFinalEngine.Components
