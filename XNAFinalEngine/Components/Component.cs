
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Base class for everything attached to GameObjects.
    /// </summary>
    public class Component
    {

        #region Variables

        /// <summary>
        /// Chaded game object's layer mask value.
        /// Every component tests if its layer mask is currently valid.
        /// </summary>
        internal uint CachedLayerMask;

        /// <summary>
        /// Chaded game object's active value.
        /// Every component tests if its owner is currently active.
        /// </summary>
        internal bool CachedOwnerActive;

        #endregion

        #region Properties

        /// <summary>
        /// The game object this component is attached to.
        /// A component is always attached to one game object.
        /// </summary>
        public GameObject Owner { get; internal set; }

        /// <summary>
        /// Makes the componet active or not.
        /// </summary>
        public bool Enabled { get; set; }

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
            Enabled = true;
            // Set Owner's layer.
            CachedLayerMask = Owner.Layer.Mask;
            Owner.LayerChanged += OnLayerChanged;
            // Set Owner's active state.
            CachedOwnerActive = Owner.Active;
            Owner.ActiveChanged += OnActiveChanged;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal virtual void Uninitialize()
        {
            Owner.LayerChanged -= OnLayerChanged;
            Owner.ActiveChanged -= OnActiveChanged;
            Owner = null;
        } // Uninitialize

        #endregion

        #region On Layer Changed

        /// <summary>
        /// On game object's layer changed.
        /// </summary>
        private void OnLayerChanged(object sender, uint layerMask)
        {
            CachedLayerMask = layerMask;
        } // OnLayerChanged

        #endregion

        #region On Active Changed

        /// <summary>
        /// On game object's active changed.
        /// </summary>
        private void OnActiveChanged(object sender, bool active)
        {
            CachedOwnerActive = active;
        } // OnActiveChanged

        #endregion

    } // Component
} // XNAFinalEngine.Components
