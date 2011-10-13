
#region License
//-----------------------------------------------------------------------------
// SkinningData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace XNAFinalEngineContentPipelineExtensionRuntime.Animations
{
    /// <summary>
    /// Combines all the data needed to render and animate a skinned or rigid object.
    /// This is typically stored in the Tag property of the Model being animated.
    /// </summary>
    public class ModelAnimationData
    {

        #region Properties

        /// <summary>
        /// Gets a collection of animation clips that operate on the root of the object.
        /// These are stored by name in a dictionary, so there could for instance be clips for "Walk", "Run", "JumpReallyHigh", etc.
        /// </summary>
        [ContentSerializer]
        public Dictionary<string, RootAnimationClip> RootAnimationClips { get; private set; }

        /// <summary>
        /// Gets a collection of model animation clips. These are stored by name in a dictionary, so there could for instance be clips for "Walk", "Run", "JumpReallyHigh", etc.
        /// </summary>
        [ContentSerializer]
        public Dictionary<string, ModelAnimationClip> ModelAnimationClips { get; private set; }

        /// <summary>
        /// Bindpose matrices for each bone in the skeleton, relative to the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> BindPose { get; private set; }

        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> InverseBindPose { get; private set; }

        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<int> BoneHierarchy { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        public ModelAnimationData(Dictionary<string, ModelAnimationClip> modelAnimationClips,
                                  Dictionary<string, RootAnimationClip> rootAnimationClips,
                                  List<Matrix> bindPose,
                                  List<Matrix> inverseBindPose,
                                  List<int> skeletonHierarchy)
        {
            ModelAnimationClips = modelAnimationClips;
            RootAnimationClips = rootAnimationClips;
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            BoneHierarchy = skeletonHierarchy;
        } // ModelAnimationData

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private ModelAnimationData() { }

        #endregion

    } // ModelAnimationData
} // XNAFinalEngineContentPipelineExtensionRuntime.Animations