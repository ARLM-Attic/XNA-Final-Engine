
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using XNAFinalEngine.Helpers;
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Base class for all entities in the scenes.
    /// </summary>
    public class GameObject2D : GameObject
    {

        #region Variables

        /// <summary>
        /// The count of nameless game objects for naming purposes.
        /// </summary>
        private static int nameNumber = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Associated transform component.
        /// </summary>
        public Transform2D Transform { get; private set; }
        
        /// <summary>
        /// Associated renderer component.
        /// </summary>
        public Renderer Renderer { get; private set; }
        
        /// <summary>
        /// The parent of this game object.
        /// </summary>
        public GameObject2D Parent
        {
            get { return Transform.Parent; }
            set { Transform.Parent = value; }
        } // Parent

        #endregion
        
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public GameObject2D(string name)
        {
            Name = name;
            Initialize();
        } // GameObject2D

        /// <summary>
        /// 
        /// </summary>
        public GameObject2D()
        {
            Name = "GameObject3D-" + nameNumber;
            nameNumber++;
            Initialize();
        } // GameObject2D

        /// <summary>
        /// Set a transform component.
        /// </summary>
        private void Initialize()
        {
            // Create a transform component. Every game object has one.
            Transform = new Transform2D { Owner = this };
        } // Initialize

        #endregion

        #region Add Component

        /// <summary>
        /// Adds a component of type TComponentType to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public override Component AddComponent<TComponentType>()
        {
            Component component = null;
            // Get from a pool or create the component.
            if (typeof(TComponentType) == typeof(Transform2D))
            {
                throw new ArgumentException("Game Object exception. Unable to create the transform component. The transform component can’t be replaced or removed.");
            }
            if (typeof(TComponentType) == typeof(HudText))
            {
                // Search for an empty component in the pool.
                Pool<HudText>.Accessor componentAccesor = HudText.HudTextPool2D.Fetch();
                // A component is a reference value, so no problem to do this.
                component = HudText.HudTextPool2D[componentAccesor];
                // Initialize the component to the default values.
                component.Initialize(this);
            }
            return component;
        } // AddComponent

        #endregion

    } // GameObject2D
} // XnaFinalEngine.Components
