using System;
using BEPUphysics.Entities;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.PhysicSystem;

namespace XNAFinalEngine.Components
{
    public class RigidBody : Component
    {
        #region Variables

        private Entity entity;

        /// <summary>
        /// Cached contacts count information for the entity. It's used to know if a collision start/end/stay in the current frame.
        /// </summary>
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
        }

        public bool IsDynamic
        {
            get
            {
                if (entity != null)
                    return entity.IsDynamic;
                else
                    throw new InvalidOperationException("Entity not properly setted");
            }
        }

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
            try {
                Physics.Scene.Remove(entity);
            } catch(ArgumentException ae) {
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
        /// Update RigidBody component transform
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

        /// <summary>
        /// Process the collision information of the component and fires the corresponding events if necessary.
        /// </summary>
        internal void ProcessCollisions()
        {      
            // Calculate the current contacts count      
            var currentContactsCount = 0;
            if (entity.CollisionInformation.Pairs.Count > 0)
                foreach (CollidablePairHandler pair in entity.CollisionInformation.Pairs)
                    //TODO: Should verify that the contacts in contacts list have negative depth to count as a 100% collision
                   currentContactsCount += pair.Contacts.Count;            
            
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

        #region Event Handlers

        /// <summary>
        /// On transform's world matrix changed update the world matrix of the associated entity.
        /// </summary>
        protected void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            //Matrix pirulo = Matrix.CreateFromQuaternion(((GameObject3D) Owner).Transform.Rotation);
            //pirulo.Translation = ((GameObject3D) Owner).Transform.Position;
            //entity.WorldTransform = pirulo;
        } // OnWorldMatrixChanged

        /// <summary>
        /// When the gameobject becomes inactive remove the associated entity from the simulation,
        /// and when the gameobject becomes active again add the entity to the simulation
        /// </summary>
        protected void OnActiveChanged(object sender, bool active)
        {
            if (active)
                Physics.Scene.Add(entity);
            else
                Physics.Scene.Remove(entity);
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
} // XNAFinalEngine.Components.Physics
