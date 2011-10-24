
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

namespace XNAFinalEngine.Components
{
    
    /// <summary>
    /// Base class for all entities in the scenes.
    /// </summary>
    public abstract class GameObject
    {

        #region Variables

        /// <summary>
        /// A simple but effective way of having unique ids.
        /// We can have 18.446.744.073.709.551.616 game object creations before the system "collapse". Almost infinite in practice. 
        /// If a more robust system is needed (networking/threading) then you can use the guid structure: http://msdn.microsoft.com/en-us/library/system.guid.aspx
        /// However this method is slightly simpler, slightly faster and has slightly lower memory requirements.
        /// If performance is critical consider the int type (4.294.967.294 unique values).
        /// </summary>
        private static long uniqueIdCounter = long.MinValue;

        /// <summary>
        /// If an object is disabled their components are not processed. 
        /// I.e. the game object will not be updated or draw.
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// Default layer mask = 1 (the default layer).
        /// </summary>
        private int layerMask = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identification number. Every game object has a unique ID.
        /// </summary>
        public long ID { get; private set; }
        
        /// <summary>
        /// If an object is disabled their components are not processed. 
        /// I.e. the game object will not be updated or draw.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (EnabledChanged != null)
                    EnabledChanged(this, enabled);
            }
        } // Enabled
        
        /// <summary>
        /// The layer the game object is in.
        /// </summary>
        public Layer Layer
        {
            get { return Layer.GetLayerByMask(layerMask); }
            set
            {
                layerMask = value.Mask;
                if (LayerChanged != null)
                    LayerChanged(this, layerMask);
            }
        } // Layer

        #endregion

        #region Events
        
        #region Layer Changed

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        public delegate void LayerEventHandler(object sender, int layerMask);

        /// <summary>
        /// Raised when the game object's layer changes.
        /// </summary>
        public event LayerEventHandler LayerChanged;

        #endregion

        #region Enabled

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        public delegate void EnabledEventHandler(object sender, bool enabled);

        /// <summary>
        /// Raised when the game object's is enabled or disabled.
        /// </summary>
        public event EnabledEventHandler EnabledChanged;

        #endregion

        #region Component Changed

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        public delegate void ComponentEventHandler(object sender, Component oldComponent, Component newComponent);

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// This constructor is called by all objects and set the ID.
        /// </summary>
        protected GameObject()
        {
            // Create a unique ID
            ID = uniqueIdCounter;
            uniqueIdCounter++;
        } // GameObject
        
        #endregion

        #region Add Component

        /// <summary>
        /// Adds a component to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public virtual Component AddComponent<TComponentType>() where TComponentType : Component, new()
        {
            return null; // Overrite it!!!
        } // AddComponent

        #endregion

        #region Remove Component

        /// <summary>
        /// Removes a component to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public virtual void RemoveComponent<TComponentType>() where TComponentType : Component
        {
            // Overrite it!!!
        } // AddComponent

        #endregion

    } // GameObject
} // XNAFinalEngine.Components