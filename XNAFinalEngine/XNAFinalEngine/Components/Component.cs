
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Base class for everything attached to GameObjects.
    /// </summary>
    public class Component
    {

        #region Properties

        /// <summary>
        /// The game object this component is attached to.
        /// A component is always attached to one game object.
        /// </summary>
        public GameObject Owner { get; internal set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new component.
        /// The system does not allow the direct creation of components.
        /// If you want to create a component use the AddComponent method of the gameobject class.
        /// </summary>
        internal Component()
        {
        } // Component

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal virtual void Initialize(GameObject owner)
        {
            Owner = owner;
        } // Initialize

        #endregion

    } // Component
} // XnaFinalEngine.Components
