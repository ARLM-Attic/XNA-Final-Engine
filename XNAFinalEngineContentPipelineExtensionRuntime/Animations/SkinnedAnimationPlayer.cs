
#region License
//-----------------------------------------------------------------------------
// SkinnedAnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Animations
{
    /// <summary>
    /// The animation player manipulates a skinned model.
    /// </summary>
    public class SkinnedAnimationPlayer : AnimationPlayerBase
    {

        #region Variables

        // Current animation transform matrices.
        Matrix[] boneTransforms;
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;

        List<Matrix> bindPose;
        List<Matrix> inverseBindPose;
        List<int> skeletonHierarchy;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public SkinnedAnimationPlayer(List<Matrix> bindPose, List<Matrix> inverseBindPose, List<int> skeletonHierarchy)
        {
            if (bindPose == null || bindPose.Count == 0)
                throw new Exception("Bad arguments to model animation player");
            
            boneTransforms = new Matrix[bindPose.Count];
            worldTransforms = new Matrix[bindPose.Count];
            skinTransforms = new Matrix[bindPose.Count];

            this.bindPose = bindPose;
            this.inverseBindPose = inverseBindPose;
            this.skeletonHierarchy = skeletonHierarchy;                
        } // SkinnedAnimationPlayer

        #endregion

        #region Init Clip

        /// <summary>
        /// Initializes the animation clip
        /// </summary>
        protected override void InitClip()
        {
            bindPose.CopyTo(boneTransforms);
        } // InitClip

        #endregion

        #region Set Key Frame

        /// <summary>
        /// Sets the key frame for the passed in frame
        /// </summary>
        /// <param name="keyframe"></param>
        protected override void SetKeyframe(Keyframe keyframe)
        {
            boneTransforms[keyframe.Bone] = keyframe.Transform;
        } // SetKeyframe

        #endregion

        #region Update

        /// <summary>
        /// Updates the transformations ultimately needed for rendering
        /// </summary>
        protected override void OnUpdate()
        {
            if (CurrentClip != null)
            {
                // Root bone.
                worldTransforms[0] = boneTransforms[0];
                skinTransforms[0] = inverseBindPose[0] * worldTransforms[0];

                // Child bones.
                for (int bone = 1; bone < worldTransforms.Length; bone++)
                {
                    int parentBone = skeletonHierarchy[bone];

                    worldTransforms[bone] = boneTransforms[bone] * worldTransforms[parentBone];
                    skinTransforms[bone] = inverseBindPose[bone] * worldTransforms[bone];
                }                
            }
        } // OnUpdate

        #endregion

        #region Get Skin Transform

        /// <summary>
        /// Gets the current bone transform matrices, relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        } // GetSkinTransforms

        #endregion

    } // SkinnedAnimationPlayer
} // XNAFinalEngine.Animations
