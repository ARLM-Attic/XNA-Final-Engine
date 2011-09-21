
#region License
//-----------------------------------------------------------------------------
// ModelAnimationClip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNAFinalEngineContentPipelineExtensionRuntime.Animations
{
    /// <summary>
    /// A animation clip is the runtime equivalent of the Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single root animation.
    /// </summary>
    public class RootAnimationClip
    {

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
        public List<RootKeyframe> Keyframes { get; private set; }

        /// <summary>
        /// Constructs a new root animation clip object.
        /// </summary>
        public RootAnimationClip(float duration, List<RootKeyframe> keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        } // RootAnimationClip

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private RootAnimationClip() { }

    } // RootAnimationClip
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations
