
#region License
//-----------------------------------------------------------------------------
// RootAnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Animations
{
    /// <summary>
    /// The animation player contains a single transformation that is used to move/position/scale something.
    /// </summary>
    public class RootAnimationPlayer : AnimationPlayerBase
    {

        Matrix currentTransform;        
        
        /// <summary>
        /// Initializes the transformation to the identity
        /// </summary>
        protected override void InitClip()
        {
            currentTransform = Matrix.Identity;
        } // InitClip

        /// <summary>
        /// Sets the key frame by storing the current transform
        /// </summary>
        /// <param name="keyframe">Keyframe being set</param>
        protected override void SetKeyframe(Keyframe keyframe)
        {
            currentTransform = keyframe.Transform;
        } // SetKeyframe

        /// <summary>
        /// Gets the current transformation being applied
        /// </summary>
        /// <returns>Transformation matrix</returns>
        public Matrix GetCurrentTransform()
        {
            return currentTransform;
        } // GetCurrentTransform

    } // RootAnimationPlayer
} // XNAFinalEngine.Animations