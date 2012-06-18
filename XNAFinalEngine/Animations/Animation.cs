
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

namespace XNAFinalEngine.Animations
{

    /// <summary>
    /// Determines how time is treated outside of the keyframed range.
    /// </summary>
    public enum WrapMode
    {
        /// <summary>
        /// For animation states, Default means the animation wrap mode. Instead, for animations, Default means Once.
        /// </summary>
        Default,
        /// <summary>
        /// When time reaches the end of the animation clip, the clip will automatically stop playing and time will be reset to beginning of the clip.
        /// </summary>
        Once,
        /// <summary>
        /// When time reaches the end of the animation clip, time will continue at the beginning.
        /// </summary>
        Loop,
        /// <summary>
        /// When time reaches the end of the animation clip, time will ping pong back between beginning and end.
        /// </summary>
        //PingPong,
        /// <summary>
        /// Plays back the animation. When it reaches the end, it will keep playing the last frame and never stop playing.
        /// </summary>
        ClampForever,
    } // AnimationState

    /// <summary>
    /// Used by the Play function of the model animation component.
    /// </summary>
    public enum AnimationBlendMode
    {
        /// <summary>
        /// Animations will be blended.
        /// </summary>
        Blend,
        /// <summary>
        /// Animations will be added.
        /// </summary>
        Additive,
    } // AnimationBlendMode

} // XNAFinalEngine.Animations