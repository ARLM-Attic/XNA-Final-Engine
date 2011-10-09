
#region License
//-----------------------------------------------------------------------------
// ModelAnimationClip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNAFinalEngineContentPipelineExtensionRuntime.Animations
{
    /// <summary>
    /// A animation clip is the runtime equivalent of the Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single model animation.
    /// </summary>
    public class ModelAnimationClip
    {
        
        /// <summary>
        /// Maximum number of bone matrices we can render in a single pass.
        /// </summary>
        public const int MaxBones = 59;

        /// <summary>
        /// Gets the total length of the model animation clip
        /// </summary>
        [ContentSerializer]
        public float Duration { get; private set; }
        
        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<ModelKeyframe> Keyframes { get; private set; }

        /// <summary>
        /// Constructs a new model animation clip object.
        /// </summary>
        public ModelAnimationClip(float duration, List<ModelKeyframe> keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        } // AnimationClip

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private ModelAnimationClip() { }

    } // ModelAnimationClip
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations
