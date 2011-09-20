
#region License
//-----------------------------------------------------------------------------
// ModelKeyframe.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNAFinalEngineContentPipelineExtensionRuntime.Animations
{
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public abstract class Keyframe
    {

        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Time { get; protected set; }

    } // Keyframe
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations