
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Base class of graphic-object level animations (not model level).
    /// </summary>
    public abstract class Animation
    {

        #region Variables

        /// <summary>
        /// The animation.
        /// </summary>
        protected Vector3 animation;

        /// <summary>
        /// The duration of the animation in seconds. When reach this limit the animation needs to dispose themself.
        /// Duration equals to zero means infinite duration.
        /// </summary>
        protected double duration;

        /// <summary>
        /// Elapsed time of the duration of the animation in seconds.
        /// </summary>
        protected double elapsed;

        /// <summary>
        /// Used if we want to loop the animation.
        /// </summary>
        protected bool isInfinite;

        /// <summary>
        /// This Frame Animation expresed in a vector3 transformation.
        /// </summary>
        protected Vector3 thisFrameAnimation;

        /// <summary>
        /// The current active animations in use by the aplication.
        /// </summary>
        protected static List<Animation> activeAnimations = new List<Animation>();

        /// <summary>
        /// The objects that this animation has.
        /// </summary>
        private List<XNAFinalEngine.GraphicElements.Object> associatedObjects = new List<XNAFinalEngine.GraphicElements.Object>();
        
        #endregion

        #region Properties

        /// <summary>
        /// This Frame Animation expresed in a vector3 transformation
        /// </summary>
        internal Vector3 ThisFrameAnimation { get { return thisFrameAnimation; } }

        /// <summary>
        /// Is the animation over?
        /// </summary>
        public bool IsOver { get { return duration == elapsed; } }

        /// <summary>
        /// La lista de objetos que tienen asociados esta animacion.
        /// </summary>
        private List<XNAFinalEngine.GraphicElements.Object> AssociatedObjects
        {
            get { return associatedObjects; }
        }
        
        #endregion

        #region Update Animation

        /// <summary>
        /// Update Animation
        /// </summary>
        protected virtual void Update()
        {
            double newElapsed = elapsed + EngineManager.ElapsedTimeThisFrameInSeconds;
            double percentage;
            if (newElapsed < duration)
            {
                // The percentage of the duration of the animation that we need to realice in this frame.
                percentage = EngineManager.ElapsedTimeThisFrameInSeconds / duration;
                elapsed = newElapsed;
            }
            else
            {
                // We do the rest of the animation and no more.
                percentage = (float)(duration - elapsed) / duration;
                elapsed = duration;
            }
            // If the animation is over but is infinite we need to start over again.
            if (elapsed == duration && isInfinite)
                elapsed = 0;

            // Calculate the amount of animation in this frame            
            thisFrameAnimation = animation * (float)percentage;
            // Return the animation
        } // Update
        
        #endregion

        #region Associated Objects

        /// <summary>
        /// Associate Object.
        /// </summary>
        internal void AssociateObject(XNAFinalEngine.GraphicElements.Object obj)
        {
            associatedObjects.Add(obj);
        } // AssociateObject

        /// <summary>
        /// Deassociate object. The object doesn't have this animation anymore.
        /// </summary>
        internal void DeassociateObject(XNAFinalEngine.GraphicElements.Object obj)
        {
            associatedObjects.Remove(obj);
        } // DeassociateObject

        #endregion

        #region Animation Manager (Static)

        /// <summary>
        /// Update all active animations.
        /// </summary>
        public static void UpdateAnimations()
        {
            List<Animation> animationsToErase = new List<Animation>();
            List<XNAFinalEngine.GraphicElements.Object> associatedObjects;
            foreach (Animation animation in activeAnimations)
            {
                // Si la animacion termino es necesario eliminarla y desasociarla
                if (animation.IsOver)
                {
                    associatedObjects = animation.AssociatedObjects;
                    foreach (XNAFinalEngine.GraphicElements.Object associatedObject in associatedObjects)
                    {
                        associatedObject.DeassociateAnimation(animation);
                    }
                    animationsToErase.Add(animation);
                }
                else
                {
                    // Actualizamos la animacion y las transformaciones de los objetos involucrados.
                    animation.Update();
                    associatedObjects = animation.AssociatedObjects;
                    foreach (XNAFinalEngine.GraphicElements.Object associatedObject in associatedObjects)
                    {   
                        if (animation is TranslationAnimation)
                        {
                            associatedObject.TranslateRel(animation.ThisFrameAnimation);
                        }
                        else if (animation is RotationAnimation)
                        {
                            associatedObject.RotateRel(animation.ThisFrameAnimation);
                        }
                        else if (animation is ScaleAnimation)
                        {
                            associatedObject.ScaleRelAdd(animation.ThisFrameAnimation);
                        }
                    }
                }
            }
            // Borramos definitivamente las animaciones que terminaron
            foreach (Animation animation in animationsToErase) // TODO LINQ??
            {
                activeAnimations.Remove(animation);
            }
        } // UpdateAnimations

        #endregion

    } // Animation
} // XNAFinalEngine.GraphicElements
