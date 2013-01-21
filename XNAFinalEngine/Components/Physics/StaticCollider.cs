
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
using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.DataStructures;
using BEPUutilities;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Physics;
#endregion

namespace XNAFinalEngine.Components
{
    /// <summary>
    /// To representate static physic objects.
    /// If you intend to move the collider around a lot it is recommended to also attach a kinematic rigidbody to it.
    /// </summary>
    public class StaticCollider : Component
    {

        #region Variables

        // Supporting Bepu Static Collidable.
        private StaticCollidable staticCollidable;

        // Cached owner's transform component.
        private Transform3D cachedTransform3D;

        #endregion

        #region Properties

        /// <summary>
        /// Supporting BEPU static collider.
        /// </summary> 
        public StaticCollidable StaticCollidable
        {
            get { return staticCollidable; }
            set
            {
                // Remove previous static collidable.
                if (staticCollidable != null)
                {
                    PhysicsManager.Scene.Remove(staticCollidable);
                    staticCollidable.Tag = null;                    
                }
                // Add the new one.
                staticCollidable = value;
                staticCollidable.Tag = Owner;
                PhysicsManager.Scene.Add(staticCollidable);
                // Set the static collidable's initial position
                OnWorldMatrixChanged(cachedTransform3D.WorldMatrix);
            }
        } // StaticCollidable

        #endregion
        
        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            // Setup events
            base.Initialize(owner);
            ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            Owner.ActiveChanged += OnActiveChanged;

            // We store the transform component to avoid casting and to improve access time.
            cachedTransform3D = ((GameObject3D)Owner).Transform;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            cachedTransform3D = null;

            if (staticCollidable != null)
                PhysicsManager.Scene.Remove(staticCollidable);
            

            // Clean the entity and event reference
            staticCollidable = null;
            ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
            Owner.ActiveChanged -= OnActiveChanged;

            // Call this last because the owner information is needed.
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region Create Static Collidable From Model Filter

        /// <summary>
        /// Creates and assign a static mesh usign the model stored in the model filter component.
        /// </summary>
        /// <param name="triangleSidedness">A triangle can be double sided, or allow one of its sides to let interacting objects through.</param>
        public void CreateStaticCollidableFromModelFilter(TriangleSidedness triangleSidedness = TriangleSidedness.Counterclockwise)
        {
            ModelFilter modelFilter = ((GameObject3D)Owner).ModelFilter;
            if (modelFilter != null && modelFilter.Model != null && modelFilter.Model is FileModel)
            {
                Vector3[] vertices;
                int[] indices;
                TriangleMesh.GetVerticesAndIndicesFromModel(((FileModel)modelFilter.Model).Resource, out vertices, out indices);
                StaticMesh staticMesh = new StaticMesh(vertices, indices) { Sidedness = triangleSidedness };

                StaticCollidable = staticMesh;
            }
            else
            {
                throw new InvalidOperationException("Static Collider: Model filter not present, model not present or model is not a FileModel.");
            }
        } // CreateStaticCollidableFromModelFilter

        #endregion

        #region Event Handlers

        /// <summary>
        /// On transform's world matrix change update the world matrix of the associated entity only in case of a kinematic rigidbody.
        /// </summary>
        private void OnWorldMatrixChanged(Matrix worldMatrix)
        {        
            AffineTransform affineTransform = new AffineTransform(cachedTransform3D.Rotation, cachedTransform3D.Position);
            if (staticCollidable is StaticMesh)
                ((StaticMesh) staticCollidable).WorldTransform = affineTransform;
            else if (staticCollidable is Terrain)
                ((Terrain)staticCollidable).WorldTransform = affineTransform;
            else if (staticCollidable is InstancedMesh)
                ((InstancedMesh)staticCollidable).WorldTransform = affineTransform;
        } // OnWorldMatrixChanged

        /// <summary>
        /// When the gameobject becomes inactive remove the associated entity from the simulation,
        /// and when the gameobject becomes active again add the entity to the simulation.
        /// </summary>
        private void OnActiveChanged(object sender, bool active)
        {
            if (active)
                PhysicsManager.Scene.Add(staticCollidable);
            else
                PhysicsManager.Scene.Remove(staticCollidable);
        } // OnActiveChanged

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<StaticCollider> componentPool = new Pool<StaticCollider>(200);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<StaticCollider> ComponentPool { get { return componentPool; } }

        #endregion

    } // StaticCollider
} // XNAFinalEngine.Components
