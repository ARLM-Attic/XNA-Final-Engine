
namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Base class for renderers.
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public abstract class Renderer : Component
    {

        #region Variables

        /// <summary>
        /// 
        /// </summary>
        protected int cachedLayerMask;

        #endregion

        #region Properties

        /// <summary>
        /// Makes the game object visible or not.
        /// </summary>
        public bool Visible { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            Owner.LayerChanged += OnLayerChanged;
        } // Initialize

        #endregion

        #region On Layer Changed

        /// <summary>
        /// On game object's layer changed.
        /// </summary>
        private void OnLayerChanged(object sender, int layerMask)
        {
            cachedLayerMask = layerMask;
        } // OnLayerChanged

        #endregion

    } // Renderer
} // XnaFinalEngine.Components
