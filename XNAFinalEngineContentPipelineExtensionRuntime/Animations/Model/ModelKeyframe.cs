
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
    public class ModelKeyframe : Keyframe
    {

        #region Variables

        /// <summary>
        /// Gets the index of the target bone that is animated by this keyframe.
        /// </summary>
        [ContentSerializer]
        public int Bone { get; private set; }
        
        /// <summary>
        /// Gets the positon transform for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Gets the rotation transform for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// Gets the scale transform for this keyframe.
        /// </summary>
        /// <remarks>
        /// The scale could use the Vector3 type for non-uniform scaling or even ignore the scale values altogether. It depends of the game necessities.
        /// </remarks>
        [ContentSerializer]
        public float Scale { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new ModelKeyframe object.
        /// </summary>
        public ModelKeyframe(int bone, float time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            // Decompose Matrix into separete values. 
            // This reduces memory consumption and improves performance because there will be less cache misses
            // and animation blending works better with the quaternions than matrices.
            Vector3 position, scale;
            Quaternion rotation;
            transform.Decompose(out scale, out rotation, out position);
            Scale = scale.X;
            Rotation = rotation;
            Position = position;
        } // Keyframe

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private ModelKeyframe() { }

        #endregion

    } // ModelKeyframe
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations