
#region Using directives
using System;
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Base class for all entities in the scenes.
    /// </summary>
    public class GameObject
    {

        #region Variables

        /// <summary>
        /// The count of nameless game objects for naming purposes.
        /// </summary>
        private static int nameNumber = 1;

        /// <summary>
        /// A simple but effective way of having unique ids.
        /// We can have 18.446.744.073.709.551.616 game object creations before the system "collapse". Almost infinite in practice. 
        /// If a more robust system is needed (networking/threading) then you can use the guid structure: http://msdn.microsoft.com/en-us/library/system.guid.aspx
        /// However this method is slightly simpler, slightly faster and has slightly lower memory requirements.
        /// If performance is critical consider the int type (4.294.967.294 unique values).
        /// </summary>
        private static long uniqueIdCounter = long.MinValue;

        #endregion

        #region Properties

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identification number. Every game object has a unique ID.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Associated transform component.
        /// </summary>
        public Transform Transform { get; private set; }
        
        /// <summary>
        /// Associated renderer component.
        /// </summary>
        public Renderer Renderer { get; private set; }
        
        /// <summary>
        /// The parent of this game object.
        /// </summary>
        public GameObject Parent
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
        public GameObject(string name)
        {
            Name = name;
            Initialize();
        } // GameObject

        /// <summary>
        /// Base class for all entities in the scenes.
        /// </summary>
        public GameObject()
        {
            Name = "GameObject" + nameNumber;
            nameNumber++;
            Initialize();
        } // GameObject

        /// <summary>
        /// Set a unique ID and a transform component.
        /// </summary>
        private void Initialize()
        {
            // Create a unique ID
            Id = uniqueIdCounter;
            uniqueIdCounter++;
            // Create a transform component. Every game object has one.
            Transform = new Transform { GameObject = this };
        } // Initialize

        #endregion

        #region Add Component

        /// <summary>
        /// Adds a component of type TComponentType to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public void AddComponent<TComponentType>() where TComponentType : Component, new()
        {
            // Create the component.
            TComponentType component = new TComponentType { GameObject = this };

            // Add it to the corresponded property.
            if (component is Transform)
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

    } // GameObject
} // XnaFinalEngine.Components
