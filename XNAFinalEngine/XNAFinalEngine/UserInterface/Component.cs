
#region Using directives
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.UI
{

    /// <summary>
    /// Component.
    /// </summary>
    public class Component : Disposable
    {

        #region Variables

        /// <summary>
        /// Is initialized?
        /// </summary>
        private bool initialized;

        #endregion

        #region Properties

        /// <summary>
        /// Is initialized?
        /// </summary>
        public virtual bool Initialized { get { return initialized; } }

        #endregion

        #region Init

        /// <summary>
        /// Init.
        /// </summary>
        protected internal virtual void Init()
        {
            initialized = true;
        } // Init

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        protected internal virtual void Update() { }

        #endregion

    } // Component
} // XNAFinalEngine.UI