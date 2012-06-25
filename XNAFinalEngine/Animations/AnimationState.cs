
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Animations
{
    /// <summary>
    /// The AnimationState gives full control over animation blending.
    /// </summary>
    /// <remarks>
    /// In most cases the Animation interface is sufficient and easier to use. 
    /// Use the AnimationState if you need full control over the animation blending any playback process.
    /// The AnimationState interface allows you to modify speed, weight, time and layers while any animation is playing.
    /// You can also setup animation mixing and wrapMode.
    /// </remarks>
    public class AnimationState
    {

        #region Variables

        private bool enabled;
        private readonly ModelAnimation modelAnimation;
        private float weight;
        private float time;
        private float speed;
        private int layer;
        private AnimationBlendMode animationBlendMode;
        private WrapMode wrapMode;

        #endregion

        #region Properties

        /// <summary>
        /// Enables / disables the animation.
        /// </summary>
        /// <remarks>
        /// For the animation to take any effect the weight also needs to be set to a value higher than zero. 
        /// If the animation is disabled, time will be paused until the animation is enabled again.
        /// </remarks>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        } // Enabled

        /// <summary>
        /// The clip that is being played by this animation state.
        /// </summary>
        public ModelAnimation ModelAnimation { get { return modelAnimation; } }

        /// <summary>
        /// The weight of animation.
        /// </summary>
        /// <remarks>
        /// Weights are distributed so that the top layer gets everything.
        /// If it doesn't use the full weight then the next layer gets to distribute the remaining weights and so on. 
        /// Once all weights are used by the top layers, no weights will be available for lower layers anymore.
        /// Unity uses fair weighting, which means if a lower layer wants 80% and 50% have already been used up, 
        /// the layer will NOT use up all weights. instead it will take up 80% of the 50%.
        /// </remarks>
        public float Weight
        {
            get { return weight; }
            set { weight = value; }
        } // Weight
        
        /// <summary>
        /// The current time of the animation.
        /// </summary>
        /// <remarks>
        /// If the time is larger than length it will be wrapped according to wrapMode.
        /// The value can be larger than the animations length. 
        /// In this case playback mode will remap the time before sampling.
        /// This value usually goes from 0 to infinity
        /// </remarks>
        public float Time
        {
            get { return time; }
            set { time = value; }
        } // Time

        /// <summary>
        /// The normalized time of the animation.
        /// </summary>
        /// <remarks>
        /// A value of 1 is the end of the animation. 
        /// A value of 0.5 is the middle of the animation.
        /// </remarks>
        public float NormalizedTime
        {
            get { return Time / modelAnimation.Duration; }
            set
            {
                Time = value * modelAnimation.Duration;
            }
        } // NormalizedTime
        
        /// <summary>
        /// The playback speed of the animation. 1 is normal playback speed.
        /// </summary>
        /// <remarks>
        /// A negative playback speed will play the animation backwards.
        /// </remarks>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        } // Speed

        /// <summary>
        /// The layer of the animation. When calculating the final blend weights, animations in higher layers will get their weights distributed first.
        /// </summary>
        /// <remarks>
        /// Lower layer animations only receive blend weights if the higher layers didn't use up all blend weights.
        /// </remarks>
        public int Layer
        {
            get { return layer; }
            set { layer = value; }
        } // Layer

        /// <summary>
        /// Which blend mode should be used?
        /// </summary>
        public AnimationBlendMode AnimationBlendMode
        {
            get { return animationBlendMode; }
            set { animationBlendMode = value; }
        } // AnimationBlendMode

        /// <summary>
        /// Sets the wrap mode used in the animation state.
        /// </summary>
        public WrapMode WrapMode
        {
            get
            {
                if (wrapMode == WrapMode.Default)
                    return modelAnimation.WrapMode;
                return wrapMode;
            }
            set { wrapMode = value; }
        } // WrapMode

        #endregion

        #region Constructor

        /// <summary>
        /// The AnimationState gives full control over animation blending.
        /// </summary>
        /// <remarks>
        /// In most cases the Animation interface is sufficient and easier to use. 
        /// Use the AnimationState if you need full control over the animation blending any playback process.
        /// The AnimationState interface allows you to modify speed, weight, time and layers while any animation is playing.
        /// You can also setup animation mixing and wrapMode.
        /// </remarks>
        public AnimationState(ModelAnimation modelAnimation)
        {
            this.modelAnimation = modelAnimation;
            enabled = true;
            weight = 1;
            time = 0;
            speed = 1;
            layer = 0;
            animationBlendMode = AnimationBlendMode.Blend;
            wrapMode = WrapMode.Default;
        } // AnimationState

        #endregion

    } // AnimationState
} // XNAFinalEngine.Animations