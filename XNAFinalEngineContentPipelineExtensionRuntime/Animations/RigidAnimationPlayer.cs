
#region License
//-----------------------------------------------------------------------------
// RigidAnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Animations
{
    /// <summary>
    /// This animation player knows how to play an animation on a rigid model, applying transformations to each of the objects in the model over time.
    /// </summary>
    public class RigidAnimationPlayer : AnimationPlayerBase
    {        
        
        // This is an array of the transforms to each object in the model
        Matrix[] boneTransforms;        
               
        /// <summary>
        /// Create a new rigid animation player
        /// </summary>
        /// <param name="count">Number of bones (objects) in the model</param>
        public RigidAnimationPlayer(int count)
        {
            if (count <= 0)
                throw new Exception("Bad arguments to model animation player");
            
            boneTransforms = new Matrix[count];
        } // RigidAnimationPlayer

        /// <summary>
        /// Initializes all the bone transforms to the identity
        /// </summary>
        protected override void InitClip()
        {
            for (int i = 0; i < boneTransforms.Length; i++)
                boneTransforms[i] = Matrix.Identity;
        } // InitClip

        /// <summary>
        /// Sets the key frame for a bone to a transform
        /// </summary>
        /// <param name="keyframe">Keyframe to set</param>
        protected override void SetKeyframe(Keyframe keyframe)
        {
            boneTransforms[keyframe.Bone] = keyframe.Transform;
        } // SetKeyframe

        /// <summary>
        /// Gets the current bone transform matrices for the animation
        /// </summary>
        public Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        } // GetBoneTransforms

    } // RigidAnimationPlayer
} // XNAFinalEngine.Animations
