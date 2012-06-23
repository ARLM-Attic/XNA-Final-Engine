
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

#region Using directives
using XNAFinalEngine.Helpers;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.Components
{
    
    /// <summary>
    /// Base class for all entities in the scenes.
    /// </summary>
    public abstract class GameObject : Disposable
    {

        #region Variables
        
        // A simple but effective way of having unique ids.
        // We can have 18.446.744.073.709.551.616 game object creations before the system "collapse". Almost infinite in practice. 
        // If a more robust system is needed (networking/threading) then you can use the guid structure: http://msdn.microsoft.com/en-us/library/system.guid.aspx
        // However this method is slightly simpler, slightly faster and has slightly lower memory requirements.
        // If performance is critical consider the int type (4.294.967.294 unique values).
        private static long uniqueIdCounter = long.MinValue;
        
        // If an object is disabled their components are not processed. 
        // I.e. the game object will not be updated or draw.
        private bool active = true;
        
        // Default layer mask = 1 (the default layer).
        private uint layerMask = 1;

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
        /// If an object is disabled their components are not processed. 
        /// I.e. the game object will not be updated or draw.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                if (ActiveChanged != null)
                    ActiveChanged(this, active);
            }
        } // Active
        
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

        /// <summary>
        /// Loaded game objects
        /// </summary>
        public static List<GameObject> GameObjects { get; private set; }

        #endregion

        #region Events
        
        #region Layer Changed

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        internal delegate void LayerEventHandler(object sender, uint layerMask);

        /// <summary>
        /// Raised when the game object's layer changes.
        /// </summary>
        internal event LayerEventHandler LayerChanged;

        #endregion

        #region Active

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        public delegate void ActiveEventHandler(object sender, bool active);

        /// <summary>
        /// Raised when the game object's is enabled or disabled.
        /// </summary>
        public event ActiveEventHandler ActiveChanged;

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
            Id = uniqueIdCounter;
            uniqueIdCounter++;
            GameObjects.Add(this);
        } // GameObject

        /// <summary>
        /// Static constructor to create the list of created game objects.
        /// </summary>
        static GameObject()
        {
            GameObjects = new List<GameObject>(100);
        } // GameObject
        
        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            GameObjects.Remove(this);
            // A disposed object could be still generating events, because it is alive for a time, in a disposed state, but alive nevertheless.
            LayerChanged = null;
            ActiveChanged = null;
            RemoveAllComponents();
        } // DisposeManagedResources

        #endregion

        #region Add Component

        /// <summary>
        /// Adds a component to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type.</typeparam>
        public virtual Component AddComponent<TComponentType>() where TComponentType : Component, new()
        {
            return null; // Overrite it!!!
        } // AddComponent

        #endregion

        #region Remove Component

        /// <summary>
        /// Removes a component to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type.</typeparam>
        public abstract void RemoveComponent<TComponentType>() where TComponentType : Component;

        /// <summary>
        /// Removes all components from the game object.
        /// </summary>
        public abstract void RemoveAllComponents();

        #endregion

    } // GameObject
} // XNAFinalEngine.Components