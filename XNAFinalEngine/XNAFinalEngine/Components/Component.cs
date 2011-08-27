
namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Base class for everything attached to GameObjects.
    /// </summary>
    public class Component
    {

        #region Properties

        /// <summary>
        /// The game object this component is attached to. A component is always attached to a game object. 
        /// </summary>
        public GameObject GameObject { get; internal set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new component.
        /// The system does not allow the direct creation of components.
        /// If you want to create a component use the AddComponent method of the gameobject class.
        /// </summary>
        internal Component()
        {
            Initialize();
        } // Component

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal virtual void Initialize()
        {
            // Overrite it!!!!
        } // Initialize

        #endregion

    } // Component
} // XnaFinalEngine.Components
