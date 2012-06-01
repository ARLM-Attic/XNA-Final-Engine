
#region License
//-----------------------------------------------------------------------------
// ModelKeyframe.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNAFinalEngineContentPipelineExtensionRuntime.Animations
{
    /// <summary>
    /// Describes the position at a single point in time.
    /// </summary>
    public struct RootKeyframe
    {

        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        [ContentSerializer]
        public float Time;

        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        [ContentSerializer] public Matrix Transform;

        #region Constructors

        /// <summary>
        /// Constructs a new ModelKeyframe object.
        /// </summary>
        public RootKeyframe(float time, Matrix transform)
        {
            Time = time;
            Transform = transform;
        } // RootKeyframe

        #endregion

    } // RootKeyframe
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations