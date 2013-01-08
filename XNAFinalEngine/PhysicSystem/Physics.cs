using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Collidables.MobileCollidables;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Components;
using Space = BEPUphysics.Space;


namespace XNAFinalEngine.PhysicSystem
{
    static public class Physics
    {
        
        #region Properties

        /// <summary>
        /// The gravity applied to all rigid bodies in the scene.
        /// </summary>
        public static Vector3 Gravity {
            get { return Scene.ForceUpdater.Gravity; }
            set { Scene.ForceUpdater.Gravity = value; }
        }

        /// <summary>
        /// Physic scene where the physics simulation occurs.
        /// You can add entities and do raycasting through this.
        /// </summary>
        public static Space Scene { get; private set; }

        #endregion


         /// <summary>
        /// Creates the physics scene and sets the defaults parameters.
        /// </summary>
        internal static void Initialize()
        {
            //TODO: Multithreading
            Scene = new Space();
        } // Initialize


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

    } // Physics
} // XNAFinalEngine.PhysicSystem
