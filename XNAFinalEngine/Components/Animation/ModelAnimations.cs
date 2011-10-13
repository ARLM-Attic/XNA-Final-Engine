
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

        #endregion

        #region Properties
        
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
        public delegate void AnimationEventHandler(object sender, Matrix[] boneTransform);

        /// <summary>
        /// Raised when the model animation's bone transform changes.
        /// </summary>
        public event AnimationEventHandler BoneTransformChanged;

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

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        internal void Update()
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
        public bool ContainsAnimationClip(ModelAnimation animation)
        {
            return modelAnimations.ContainsValue(animation) || modelAnimations.ContainsKey(animation.Name);
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
