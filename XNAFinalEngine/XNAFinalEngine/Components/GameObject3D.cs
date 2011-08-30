
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

#region Using directives
using System;
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Base class for all entities in the scenes.
    /// </summary>
    public class GameObject3D : GameObject
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
        public Transform3D Transform { get; private set; }
        
        /// <summary>
        /// Associated renderer component.
        /// </summary>
        public Renderer Renderer { get; private set; }
        
        /// <summary>
        /// The parent of this game object.
        /// </summary>
        public GameObject3D Parent
        {
            get { return Transform.Parent; }
            set { Transform.Parent = value; }
        } // Parent

        #endregion

        #region Events

        #endregion

        #region Constructor

        /// <summary>
        /// Base class for all entities in the scenes.
        /// </summary>
        public GameObject3D(string name)
        {
            Name = name;
            Initialize();
        } // GameObject3D

        /// <summary>
        /// Base class for all entities in the scenes.
        /// </summary>
        public GameObject3D()
        {
            Name = "GameObject3D-" + nameNumber;
            nameNumber++;
            Initialize();
        } // GameObject3D

        /// <summary>
        /// Set a unique ID and a transform component.
        /// </summary>
        private void Initialize()
        {
            // Create a transform component. Every game object has one.
            Transform = new Transform3D { Owner = this };
        } // Initialize

        #endregion

        #region Add Component

        /// <summary>
        /// Adds a component of type TComponentType to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public override void AddComponent<TComponentType>()
        {
            // Create the component.
            TComponentType component = new TComponentType { Owner = this };

            // Add it to the corresponded property.
            if (component is Transform3D)
            {
                throw new Exception("Game object exception. Unable to create the transform component. The transform component can’t be replaced or removed.");
            }
            if (component is Renderer)
            {
                Renderer = (Renderer)(Component)component;
            }
            // Add it to the component list. The component list allows the development of new components.
            // TODO!!
        } // AddComponent

        #endregion

    } // GameObject3D
} // XnaFinalEngine.Components
