
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
using XNAFinalEngine.Helpers;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// The model animation component is used to play back model animations (skinned and rigid).
    /// This kind of animations does not affect the game object's transform.
    /// </summary>
    public class ModelAnimation : Component
    {

        #region Variables

        // Associated animations.
        private readonly Dictionary<string, ModelAnimationClip> modelAnimations = new Dictionary<string, ModelAnimationClip>(0);
        
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

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<ModelAnimation> modelAnimationPool = new Pool<ModelAnimation>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<ModelAnimation> ModelAnimationPool { get { return modelAnimationPool; } }

        #endregion

    } // ModelAnimation
} // XNAFinalEngine.Components
