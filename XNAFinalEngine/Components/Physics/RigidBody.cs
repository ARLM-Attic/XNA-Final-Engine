
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
using BEPUphysics.Entities;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Physics;
#endregion

namespace XNAFinalEngine.Components
{
    /// <summary>
    /// Basic physic component. Rigidbodies enable GameObjects to act under the control of physics.
    /// There are two kinds of rigidbodies: Dynamic and Kinematic
    /// - Dynamic rigidbodies have finite mass and respond to collisions with other dynamic and kinematic rigidbodies. Dynamic rigidbodies
    ///   are under the control of physics and must be manipulated applying forces and torque only. 
    ///   If you use a dynamic body, set the position/orientation of the BEPU entity once and then the position and orientation of the gameobject 
    ///   is under the control of physics. You should never set the position nor orientation of a dynamic rigidbody after that.
    /// - Kinematic rigidbodies have infinite mass and are not affected by gravity or collisions. They can collide with dynamic rigidbodies (affecting
    ///   them) but can't collide with other kinematic rigidbodies. Kinematic rigidbodies must be manipulated trough the transform component
    ///   setting the position or rotation.     
    /// Remember that when you set the mass in a BEPU entity, you're creating a dynamic rigidbody and when no mass is set you're creating a kinematic one.
    /// </summary>
    public class RigidBody : Component
    {

        #region Variables

        private Entity entity;
        
        // Cached contacts count information for the entity. It's used to know whether a collision start/end/stay in the current frame.
        private int contactsCount;        

        #endregion

        #region Properties

        /// <summary>
        /// Supporting BEPU entity.
        /// </summary> 
        public Entity Entity
        {
            get { return entity; }
            set 
            {
                if (entity == null)
                {
                    entity = value;
                    entity.Tag = this.Owner;
                    foreach (CollidablePairHandler pair in entity.CollisionInformation.Pairs)
                        //TODO: Should verify that the contacts in contacts list have negative depth to count as a collision
                        contactsCount += pair.Contacts.Count;                     
                }
                else
                    throw new InvalidOperationException("Entity already setted");
            }
        } // Entity

        public bool IsDynamic
        {
            get
            {
                if (entity != null)
                    return entity.IsDynamic;
                else
                    throw new InvalidOperationException("Entity not properly setted");
            }
        } // IsDynamic

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);                                    
            ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            Owner.ActiveChanged += OnActiveChanged;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {     
            // Remove the entity from the simulation
            try
            {
                PhysicsManager.Scene.Remove(entity);
            } 
            catch(ArgumentException ae) 
            {
                // The entity was already removed
            }
            
            // Clean the entity and event reference
            entity = null;
            ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
            
            // Call this last because the owner information is needed.
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region Update

        /// <summary>
        /// Update RigidBody component transform in case of a dynamic rigidbody.
        /// </summary>
        internal void Update()
        {
            if (entity.IsDynamic)
            {                
                ((GameObject3D) Owner).Transform.Position = entity.Position;
                ((GameObject3D) Owner).Transform.Rotation = entity.Orientation;
            }
        } // Update

        #endregion

        #region Process Collisions

        /// <summary>
        /// Process the collision information of the component and fires the corresponding events if necessary.
        /// </summary>
        internal void ProcessCollisions()
        {      
            // Calculate the current contacts count      
            var currentContactsCount = 0;
            if (entity.CollisionInformation.Pairs.Count > 0)
            {              
                foreach (CollidablePairHandler pair in entity.CollisionInformation.Pairs)
                {                    
                    //TODO: Should verify that the contacts in contacts list have negative depth to count as a 100% collision                    
                    currentContactsCount += pair.Contacts.Count;
                }
            }

            // Test if collision initiated
            if (contactsCount == 0 && currentContactsCount > 0)                
                // Get the list of scripts attached to the game object and invoke OnCollisionEnter on each one
                foreach (var script in Owner.Scripts)
                    script.OnCollisionEnter(entity.CollisionInformation);
            // Test if collision ended
            if (contactsCount > 0 && currentContactsCount == 0)
                // Get the list of scripts attached to the owner game object and invoke OnCollisionExit on each one
                foreach (var script in Owner.Scripts)
                    script.OnCollisionExit(entity.CollisionInformation);
            // Test if collision stays on
            if (contactsCount > 0 && currentContactsCount > 0)
                // Get the list of scripts attached to the owner game object and invoke OnCollisionStay on each one
                foreach (var script in Owner.Scripts)
                    script.OnCollisionStay(entity.CollisionInformation);
            
            // Update the contacts count
            contactsCount = currentContactsCount;
        } // ProcessCollisions

        #endregion

        #region Event Handlers

        /// <summary>
        /// On transform's world matrix change update the world matrix of the associated entity only in case of a kinematic rigidbody.
        /// </summary>
        protected void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            if (!IsDynamic)
            {
                Matrix wm = Matrix.CreateFromQuaternion(((GameObject3D) Owner).Transform.Rotation);
                wm.Translation = ((GameObject3D) Owner).Transform.Position;
                entity.WorldTransform = wm;
            }
        } // OnWorldMatrixChanged

        /// <summary>
        /// When the gameobject becomes inactive remove the associated entity from the simulation,
        /// and when the gameobject becomes active again add the entity to the simulation
        /// </summary>
        protected void OnActiveChanged(object sender, bool active)
        {
            if (active)
                PhysicsManager.Scene.Add(entity);
            else
                PhysicsManager.Scene.Remove(entity);
        } // OnActiveChanged

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<RigidBody> componentPool = new Pool<RigidBody>(200);
        
        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<RigidBody> ComponentPool { get { return componentPool; } }

        #endregion

    } // RigidBody
} // XNAFinalEngine.Components
