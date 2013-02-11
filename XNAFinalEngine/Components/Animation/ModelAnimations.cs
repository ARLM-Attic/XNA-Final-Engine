
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.Animations;
using XNAFinalEngine.Assets;
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

        #region Structs

        /// <summary>
        /// Stores the information of an animation currently being played.
        /// </summary>
        private class AnimationPlayed
        {
            public AnimationState AnimationState;
            public ModelAnimationPlayer ModelAnimationPlayer;
            public float FadeLenght;
            public float ElapsedTime;

            public AnimationPlayed(AnimationState animationState, ModelAnimationPlayer modelAnimationPlayer, float fadeLenght = 0, float elapsedTime = 0)
            {
                AnimationState = animationState;
                ModelAnimationPlayer = modelAnimationPlayer;
                FadeLenght = fadeLenght;
                ElapsedTime = elapsedTime;
            }
        } // AnimationPlayed

        #endregion

        #region Variables

        // Associated animations.
        private readonly Dictionary<string, AnimationState> animationStates = new Dictionary<string, AnimationState>(0);

        // Current bone transform matrices in absolute format.
        // They have to be transform to world space and if the model is skinned they have to be transformed by the inverse bind pose.
        private readonly Matrix[] localBoneTransform = new Matrix[ModelAnimationClip.MaxBones];

        // Current bone transform matrices with parent transformations.
        // If the model is skinned they have to be transformed by the inverse bind pose.
        private readonly Matrix[] worldBoneTransform = new Matrix[ModelAnimationClip.MaxBones];

        // Chaded model filter's model value.
        private Model cachedModel;

        private readonly List<AnimationPlayed> activeAnimations = new List<AnimationPlayed>();

        #endregion

        #region Properties
        
        /// <summary>
        /// Are we playing any animations?
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Returns the animation state named name.
        /// </summary>
        public AnimationState this[string name] { get { return animationStates[name]; } }

        /// <summary>
        /// Get the number of clips currently assigned to this animation component.
        /// </summary>
        public int AnimationsCount { get { return animationStates.Count; } }

        /// <summary>
        /// Current bone transform matrices in absolute format.
        /// They have to be transform to world space and if the model is skinned they have to be transformed by the inverse bind pose.
        /// If you update the local bone transform manually call UpdateLocalBoneTransforms.
        /// </summary>
        public Matrix[] LocalBoneTransforms { get { return localBoneTransform; } }

        /// <summary>
        /// Bone transforms for rigid and animated skinning models.
        /// If you update the world bone transform manually call UpdateWorldBoneTransforms.
        /// </summary>
        public Matrix[] WorldBoneTransforms { get { return worldBoneTransform; } }

        #endregion

        #region Events

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        internal delegate void AnimationEventHandler(object sender, Matrix[] worldBoneTransform);

        /// <summary>
        /// Raised when the model animation's bone transform changes.
        /// </summary>
        internal event AnimationEventHandler WorldBoneTransformChanged;

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
            // Animations
            animationStates.Clear();
            activeAnimations.Clear();

            cachedModel = null;
            WorldBoneTransformChanged = null;
            ((GameObject3D)Owner).ModelFilterChanged -= OnModelFilterChanged;
            if (((GameObject3D)Owner).ModelFilter != null)
                ((GameObject3D)Owner).ModelFilter.ModelChanged -= OnModelChanged;
            // Call this last because the owner information is needed.
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region Play

        /// <summary>
        /// Plays animation without any blending.
        /// </summary>
        /// <remarks>
        /// If the animation is already playing, other animations will be stopped but the animation will not rewind to the beginning.
        /// If the animation is not set to be looping it will be stopped and rewinded after playing.
        /// </remarks>
        /// <param name="name">Animation name</param>
        public void Play(string name)
        {
            if (!animationStates.ContainsKey(name))
                throw new ArgumentException("Model Animation Component: the animation name does not exist.");
            AnimationState animationState = animationStates[name];
            AnimationPlayed modelAnimationPlayer = null;
            // Stop animations on this layer
            foreach (AnimationPlayed activeAnimation in activeAnimations)
            {
                if (activeAnimation.AnimationState.ModelAnimation.Name != name)
                    activeAnimation.ModelAnimationPlayer.Stop();
                else
                    modelAnimationPlayer = activeAnimation;
            }
            activeAnimations.Clear();
            // If the animation was not being played.
            if (modelAnimationPlayer == null)
            {
                // Create the new animation
                modelAnimationPlayer = new AnimationPlayed(animationState, AnimationManager.FetchModelAnimationPlayer());
                modelAnimationPlayer.ModelAnimationPlayer.Play(animationState);
            }
            activeAnimations.Add(modelAnimationPlayer);
        } // Play

        #endregion

        #region Cross Fade

        /// <summary>
        /// Fades the animation with name animation in over a period of time seconds and fades other animations out.
        /// </summary>
        /// <remarks>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing.
        /// </remarks>
        /// <param name="name">Animation name.</param>
        /// <param name="fadeLength">Fade length.</param>
        public void CrossFade(string name, float fadeLength = 0.3f)
        {
            if (!animationStates.ContainsKey(name))
                throw new ArgumentException("Model Animation Component: the animation name does not exist.");
            AnimationPlayed animationPlayed = null;
            AnimationState animationState = animationStates[name];
            // Stop animations on this layer.
            foreach (AnimationPlayed activeAnimation in activeAnimations)
            {
                if (activeAnimation.AnimationState.ModelAnimation.Name == name)
                    animationPlayed = activeAnimation;
            }
            // If the animation was not being played.
            if (animationPlayed == null)
            {
                // Create the new animation.
                animationPlayed = new AnimationPlayed(animationState, AnimationManager.FetchModelAnimationPlayer());
                animationPlayed.ModelAnimationPlayer.Play(animationState);
            }
            else
            {
                // I want to place the animation in the last place.
                activeAnimations.Remove(animationPlayed);
            }
            animationPlayed.FadeLenght = fadeLength;
            if (activeAnimations.Count > 0 && animationPlayed.ElapsedTime != 0)
                animationPlayed.ModelAnimationPlayer.AnimationState.NormalizedTime = activeAnimations[activeAnimations.Count - 1].ModelAnimationPlayer.AnimationState.NormalizedTime;
            activeAnimations.Add(animationPlayed);
        } // CrossFade

        #endregion

        #region Rewind

        /// <summary>
        /// Rewinds the animation named name.
        /// </summary>
        public void Rewind(string name)
        {
            if (!animationStates.ContainsKey(name))
                throw new ArgumentException("Model Animation Component: the animation name does not exist.");
            foreach (AnimationPlayed activeAnimation in activeAnimations)
            {
                if (activeAnimation.AnimationState.ModelAnimation.Name == name)
                    activeAnimation.ModelAnimationPlayer.CurrentTimeValue = 0;
            }
        } // Rewind

        #endregion

        #region Stop

        /// <summary>
        /// Stops all playing animations of this component.
        /// </summary>
        public void Stop()
        {
            // TODO!!
        } // Stop

        /// <summary>
        /// Stops an animation named name.
        /// </summary>
        /// <param name="name">The animation name.</param>
        /// <param name="immediate">
        /// Specifies whether to stop playing immediately, or to break out of the loop region and play the release.
        /// Specify true to stop playing immediately, or false to break out of the loop region and play the release phase (the remainder of the sound).
        /// </param>
        public void Stop(string name, bool immediate = true)
        {
            // TODO!!
        } // Stop

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        internal void Update()
        {
            if (!(cachedModel is FileModel))
                return;
            if (activeAnimations.Count == 0)
                return;

            // Update cross fade elapsed time.
            for (int i = 0; i < activeAnimations.Count; i++)
            {
                if (activeAnimations[i].FadeLenght != 0)
                    activeAnimations[i].ElapsedTime += Time.GameDeltaTime;
            }

            bool stopAnimations = false;
            for (int i = activeAnimations.Count - 1; i >= 0; i--)
            {
                if (stopAnimations)
                    activeAnimations[i].ModelAnimationPlayer.Stop();
                else
                    // If cross fade is completed the rest of the animations are removed.
                    if (activeAnimations[activeAnimations.Count - 1].ElapsedTime > activeAnimations[activeAnimations.Count - 1].FadeLenght)
                    {
                        stopAnimations = true;
                    }
            }
            
            FileModel fileModel = (FileModel)cachedModel;
            if (activeAnimations.Count >= 2)
            {
                // Update bone transform
                for (int bone = 0; bone < fileModel.BoneCount; bone++)
                {
                    float amout = activeAnimations[activeAnimations.Count - 1].ElapsedTime / activeAnimations[activeAnimations.Count - 1].FadeLenght;
                    ModelAnimationPlayer.BoneTransformationData blendedPose = InterpolatePose(activeAnimations[activeAnimations.Count - 2].ModelAnimationPlayer.BoneTransforms[bone],
                                                                                              activeAnimations[activeAnimations.Count - 1].ModelAnimationPlayer.BoneTransforms[bone],
                                                                                              amout);
                    localBoneTransform[bone] = Matrix.CreateScale(blendedPose.scale) *
                                          Matrix.CreateFromQuaternion(blendedPose.rotation) *
                                          Matrix.CreateTranslation(blendedPose.position);
                }
            }
            else
            {
                // Update bone transform
                for (int bone = 0; bone < fileModel.BoneCount; bone++)
                {
                    localBoneTransform[bone] = Matrix.CreateScale(activeAnimations[0].ModelAnimationPlayer.BoneTransforms[bone].scale) *
                                          Matrix.CreateFromQuaternion(activeAnimations[0].ModelAnimationPlayer.BoneTransforms[bone].rotation) *
                                          Matrix.CreateTranslation(activeAnimations[0].ModelAnimationPlayer.BoneTransforms[bone].position);
                }
            }

            // When the local bone transforms change the world bone transforms are updated.
            UpdateLocalBoneTransforms();

            // Remove finished animations from the active animations (Change the list to array TODO)
            for (int i = 0; i < activeAnimations.Count; i++)
            {
                if (activeAnimations[i].ModelAnimationPlayer.State == MediaState.Stopped)
                {
                    activeAnimations.Remove(activeAnimations[i]);
                    i--;
                }
            }
        } // Update

        #region Interpolate Animation

        /// <summary>
        /// Retrieves and interpolates two pose.
        /// </summary>
        private static ModelAnimationPlayer.BoneTransformationData InterpolatePose(ModelAnimationPlayer.BoneTransformationData transformationData1,
                                                                                   ModelAnimationPlayer.BoneTransformationData transformationData2,
                                                                                   float amount)
        {
            Vector3 translation = Vector3.SmoothStep(transformationData1.position, transformationData2.position, amount);
            Quaternion rotation = Quaternion.Slerp(transformationData1.rotation, transformationData2.rotation, amount);
            float scale = transformationData1.scale * (1 - amount) + transformationData2.scale * amount;
            return new ModelAnimationPlayer.BoneTransformationData { position = translation, rotation = rotation, scale = scale };
        } // InterpolatePose

        #endregion

        #endregion

        #region Update Bone Transforms

        /// <summary>
        /// Call this if you update the local bone transform manually.
        /// </summary>
        public void UpdateLocalBoneTransforms()
        {
            if (cachedModel != null && cachedModel is FileModel)
                ((FileModel)cachedModel).UpdateWorldTransforms(localBoneTransform, worldBoneTransform);
            UpdateWorldBoneTransforms();
        } // UpdateBoneTransforms

        /// <summary>
        /// Call this if you update the world bone transform manually.
        /// </summary>
        public void UpdateWorldBoneTransforms()
        {
            if (WorldBoneTransformChanged != null)
                WorldBoneTransformChanged(this, worldBoneTransform);
        } // UpdateBoneTransforms

        #endregion

        #region Contains Animation Clip

        /// <summary>
        /// Determines if the component contains a specific model animation.
        /// </summary>
        public bool ContainsAnimationClip(string name)
        {
            return animationStates.ContainsKey(name);
        } // ContainsAnimationClip

        #endregion

        #region Add Animation Clip

        /// <summary>
        /// Adds a clip to the animation with name newName.
        /// </summary>
        public void AddAnimationClip(ModelAnimation animation, string newName = null)
        {
            if (newName == null)
                newName = animation.Name;
            if (!ContainsAnimationClip(newName))
                animationStates.Add(newName, new AnimationState(animation));
            else
                throw new ArgumentException("Model Animation Component: The animation " + animation.Name + " is already assigned.");
        } // AddAnimationClip

        #endregion

        #region Remove Animation Clip

        /// <summary>
        /// Remove an animation clip to the component.
        /// </summary>
        public void RemoveAnimationClip(string name)
        {
            if (ContainsAnimationClip(name))
                animationStates.Remove(name);
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
                    RemoveAnimationClip(modelAnimation.Name);
                }
            }
            cachedModel = model;
            // If the model is skined initialize the bone transform with the bind pose.
            if (model != null && model is FileModel && model.IsSkinned)
            {
                for (int i = 0; i < ((FileModel)model).BindPose.Count; i++)
                {
                    localBoneTransform[i] = ((FileModel)model).BindPose[i];
                }
            }
            else
            {
                // If not use the identity transformation.
                for (int i = 0; i < localBoneTransform.Length; i++)
                {
                    localBoneTransform[i] = Matrix.Identity;
                }
            }
            UpdateLocalBoneTransforms();
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
                ((ModelFilter)newComponent).ModelChanged += OnModelChanged;
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
