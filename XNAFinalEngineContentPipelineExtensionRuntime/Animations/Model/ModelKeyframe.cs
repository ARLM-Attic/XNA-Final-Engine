
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
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public struct ModelKeyframe
    {

        #region Variables

        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        [ContentSerializer] public float Time;

        /// <summary>
        /// Gets the index of the target bone that is animated by this keyframe.
        /// </summary>
        [ContentSerializer] public ushort Bone;

        /// <summary>
        /// Gets the positon transform for this keyframe.
        /// </summary>
        [ContentSerializer] public Vector3 Position;

        /// <summary>
        /// Gets the rotation transform for this keyframe.
        /// </summary>
        [ContentSerializer] public Quaternion Rotation;

        /// <summary>
        /// Gets the scale transform for this keyframe.
        /// </summary>
        /// <remarks>
        /// The scale could use the Vector3 type for non-uniform scaling or even ignore the scale values altogether. It depends of the game necessities.
        /// </remarks>
        [ContentSerializer] public float Scale;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new ModelKeyframe object.
        /// </summary>
        public ModelKeyframe(ushort bone, float time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            // Decompose Matrix into separete values. 
            // This reduces memory consumption and improves performance because there will be less cache misses
            // and animation blending works better with quaternions than matrices.
            Vector3 position, scale;
            Quaternion rotation;
            transform.Decompose(out scale, out rotation, out position);
            Scale = scale.X;
            Rotation = rotation;
            Position = position;
        } // ModelKeyframe

        #endregion

    } // ModelKeyframe
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations