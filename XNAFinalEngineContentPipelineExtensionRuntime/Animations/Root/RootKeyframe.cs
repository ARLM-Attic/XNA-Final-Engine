
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNAFinalEngineContentPipelineExtensionRuntime.Animations
{
    /// <summary>
    /// Describes the position at a single point in time.
    /// </summary>
    public class RootKeyframe : Keyframe
    {

        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Matrix Transform { get; private set; }

        #region Constructors

        /// <summary>
        /// Constructs a new ModelKeyframe object.
        /// </summary>
        public RootKeyframe(TimeSpan time, Matrix transform)
        {
            Time = time;
            Transform = transform;
        } // RootKeyframe

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private RootKeyframe() { }

        #endregion

    } // RootKeyframe
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations