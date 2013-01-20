
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
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;
using Space = BEPUphysics.Space;
#endregion

namespace XNAFinalEngine.Physics
{
    /// <summary>
    /// The physic system utilizes BEPU Physics.
    /// The interface implement assures a correct communication between the engine and this library.
    /// However direct manipulation of BEPU elements is needed.
    /// </summary>
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

            //  NOTE:PERFORMANCE
            //  BEPUphysics uses an iterative system to solve constraints.  You can tell it to do more or less iterations.
            //  Less iterations is faster; more iterations makes the result more accurate.
            //
            //  The amount of iterations needed for a simulation varies.  The "Wall" and "Pyramid" simulations are each fairly
            //  solver intensive, but as few as 4 iterations can be used with acceptable results.
            //  The "Jenga" simulation usually needs a few more iterations for stability; 7-9 is a good minimum.
            //
            //  The Dogbot demo shows how accuracy can smoothly increase with more iterations.
            //  With very few iterations (1-3), it has slightly jaggier movement, as if the parts used to construct it were a little cheap.
            //  As you give it a few more iterations, the motors and constraints get more and more robust.
            //  
            //  Many simulations can work perfectly fine with very few iterations, 
            //  and using a low number of iterations can substantially improve performance.
            //
            //  To change the number of iterations used, uncomment and change the following line (10 iterations is the default):
            Scene.Solver.IterationLimit = 10;

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
        /// Indicates if a ray intercepts a game object.
        /// </summary>
        public static GameObject3D Raycast(Vector3 origin, Vector3 direction, float distance, out RayCastResult result)
        {
            GameObject3D go = null;
            var ray = new Ray(origin, direction);
            
            if (Scene.RayCast(ray, distance, out result))
            {
                var entityCollision = result.HitObject as EntityCollidable;
                if (entityCollision != null)
                    go = (GameObject3D) entityCollision.Entity.Tag;
            }
            return go;
        } // Raycast

        /// <summary>
        /// Indicates if a ray intercepts a game object.
        /// </summary>
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
