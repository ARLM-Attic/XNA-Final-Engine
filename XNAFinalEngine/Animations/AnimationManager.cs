
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
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Animations
{

    /// <summary>
    /// The Animation Manager.
    /// </summary>
    public static class AnimationManager
    {

        #region Variables

        // A pool for the current played animations.
        private static readonly PoolWithoutAccessor<ModelAnimationPlayer> modelAnimationPlayerPool = new PoolWithoutAccessor<ModelAnimationPlayer>(20);

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the Sprite Manager subsystem.
        /// </summary>
        internal static void Initialize()
        {
            
        } // Initialize

        #endregion

        #region Pause Resume
        
        /// <summary>
        /// Pause all animations.
        /// </summary>
        public static void PauseAllAnimations()
        {
            
        } // PauseAllAnimations

        /// <summary>
        /// Resume all animations.
        /// </summary>
        public static void ResumeAllAnimations()
        {
            
        } // ResumeAllAnimations
        
        #endregion

        #region Fetch and Release Model Animation Player

        /// <summary>
        /// Give an available animation player.
        /// </summary>
        internal static ModelAnimationPlayer FetchModelAnimationPlayer()
        {
            return modelAnimationPlayerPool.Fetch();
        } // FetchModelAnimationPlayer

        /// <summary>
        /// Release an unused animation player.
        /// </summary>
        internal static void ReleaseModelAnimationPlayer(ModelAnimationPlayer modelAnimationPlayer)
        {
            modelAnimationPlayerPool.Release(modelAnimationPlayer);
        } // ReleaseModelAnimationPlayer

        #endregion

        #region Update

        /// <summary>
        /// Update the individual active model animations players.
        /// </summary>
        public static void UpdateModelAnimationPlayers()
        {
            for (int i = 0; i < modelAnimationPlayerPool.Count; i++)
            {
                modelAnimationPlayerPool.Elements[i].Update();
            }
        } // UpdateModelAnimationPlayers

        /// <summary>
        /// Release unused model animation players.
        /// </summary>
        public static void ReleaseUnusedAnimationPlayers()
        {
            // Release unused model animation players.
            for (int i = 0; i < modelAnimationPlayerPool.Count; i++)
            {
                if (modelAnimationPlayerPool.Elements[i].State == MediaState.Stopped)
                {
                    ReleaseModelAnimationPlayer(modelAnimationPlayerPool.Elements[i]);
                    // We need to control the swaped element.
                    i--;
                }
            }
        } // ReleaseUnusedAnimationPlayers

        #endregion

    } // AnimationManager
} // XNAFinalEngine.Animations
