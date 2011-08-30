
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

namespace XnaFinalEngine.Components
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
        public virtual void AddComponent<TComponentType>() where TComponentType : Component, new()
        {
            // Overrite it!!!
        } // AddComponent

        #endregion

    } // GameObject
} // XnaFinalEngine.Components