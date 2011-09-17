
using Microsoft.Xna.Framework;

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Base class for renderers.
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public abstract class Renderer : Component
    {

        #region Properties

        /// <summary>
        /// Makes the game object visible or not.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Chaded transform's world matrix value.
        /// </summary>
        internal Matrix CachedWorldMatrix { get; set; }

        /// <summary>
        /// Chaded game object's layer mask value.
        /// </summary>
        internal int CachedLayerMask { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            Visible = true;
            // Set Layer
            CachedLayerMask = Owner.Layer.Mask;
            Owner.LayerChanged += OnLayerChanged;
            // Set World Matrix
            if (Owner is GameObject2D)
            {
                CachedWorldMatrix = ((GameObject2D) Owner).Transform.WorldMatrix;
                ((GameObject2D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            }
            else
            {
                CachedWorldMatrix = ((GameObject3D)Owner).Transform.WorldMatrix;
                ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            }
        } // Initialize

        #endregion

        #region Disable

        /// <summary>
        /// Disable the component. 
        /// </summary>
        internal override void Disable()
        {
            base.Disable();
            Owner.LayerChanged -= OnLayerChanged;
            if (Owner is GameObject2D)
            {
                ((GameObject2D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
            }
            else
            {
                ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
            }
        } // Disable

        #endregion

        #region On Layer Changed

        /// <summary>
        /// On game object's layer changed.
        /// </summary>
        private void OnLayerChanged(object sender, int layerMask)
        {
            CachedLayerMask = layerMask;
        } // OnLayerChanged

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected virtual void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            CachedWorldMatrix = worldMatrix;
        } // OnWorldMatrixChanged

        #endregion

    } // Renderer
} // XnaFinalEngine.Components
