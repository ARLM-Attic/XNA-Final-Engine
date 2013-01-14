
#region License
/*
Copyright (c) 2008-2013, Schefer, Gustavo Martín.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

• Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

• The names of its contributors cannot be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schefer, Gustavo Martín (gusschefer@hotmail.com)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System.Threading;
using BEPUphysics;
using BEPUphysics.Collidables.MobileCollidables;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;
using Space = BEPUphysics.Space;
#endregion

namespace XNAFinalEngine.Physics
{
    public static class PhysicsManager
    {
        
        #region Properties

        /// <summary>
        /// The gravity applied to all rigid bodies in the scene.
        /// </summary>
        public static Vector3 Gravity 
        {
            get { return Scene.ForceUpdater.Gravity; }
            set { Scene.ForceUpdater.Gravity = value; }
        } // Gravity

        /// <summary>
        /// Physic scene where the physics simulation occurs.
        /// You can add entities and do raycasting through this.
        /// </summary>
        public static Space Scene { get; private set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Creates the physics scene and sets the defaults parameters.
        /// </summary>
        internal static void Initialize()
        {          
            Scene = new Space();

            // Setup multithreading
            #if XBOX360
                Scene.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new int[] { 1 }); }, null);
                Scene.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new int[] { 3 }); }, null);
                Scene.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 }); }, null);
                Scene.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 }); }, null);
            #else
                if (ProcessorsInformation.AvailableProcessors > 0)
                {
                    // On windows, just throw a thread at every processor. 
                    // The thread scheduler will take care of where to put them.
                    for (int i = 0; i < ProcessorsInformation.AvailableProcessors + 1; i++)
                    {
                        Scene.ThreadManager.AddThread();
                    }
                }
            #endif
        } // Initialize

        #endregion

        #region Raycast

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static GameObject3D Raycast(Vector3 origin, Vector3 direction, float distance, out RayCastResult result)
        {
            GameObject3D go = null;
            var r = new Ray(origin, direction);
            
            if (Scene.RayCast(r, distance, out result))
            {
                var entityCollision = result.HitObject as EntityCollidable;
                if (entityCollision != null)
                    go = (GameObject3D) entityCollision.Entity.Tag;
            }
            return go;
        } // Raycast
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="distance"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static GameObject3D Raycast(Ray ray, float distance, out RayCastResult result)
        {
            GameObject3D go = null;            

            if (Scene.RayCast(ray, distance, out result))
            {
                var entityCollision = result.HitObject as EntityCollidable;
                if (entityCollision != null)
                    go = (GameObject3D)entityCollision.Entity.Tag;
            }
            return go;
        } // Raycast

        #endregion

    } // PhysicsManager
} // XNAFinalEngine.Physics
