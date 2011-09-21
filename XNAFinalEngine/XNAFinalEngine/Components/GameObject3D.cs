
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

#region Using directives
using System;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Base class for all entities in the scenes.
    /// </summary>
    public class GameObject3D : GameObject
    {

        #region Variables

        /// <summary>
        /// The count of nameless game objects for naming purposes.
        /// </summary>
        private static int nameNumber = 1;

        #region Accessors

        private Pool<ModelFilter>.Accessor modelFilterAccessor;
        private Pool<ModelRenderer>.Accessor modelRendererAccessor;
        private Pool<RootAnimation>.Accessor rootAnimationAccessor;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Associated transform component.
        /// </summary>
        public Transform3D Transform { get; private set; }
        
        /// <summary>
        /// Associated model renderer component.
        /// </summary>
        public ModelRenderer ModelRenderer { get; private set; }

        /// <summary>
        /// Associated model filter component.
        /// </summary>
        public ModelFilter ModelFilter { get; private set; }

        /// <summary>
        /// Associated root animation component.
        /// </summary>
        public RootAnimation RootAnimation { get; private set; }
        
        /// <summary>
        /// The parent of this game object.
        /// </summary>
        public GameObject3D Parent
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
        public GameObject3D(string name)
        {
            Name = name;
            Initialize();
        } // GameObject3D

        /// <summary>
        /// Base class for all entities in the scenes.
        /// </summary>
        public GameObject3D()
        {
            Name = "GameObject3D-" + nameNumber;
            nameNumber++;
            Initialize();
        } // GameObject3D

        /// <summary>
        /// Set a unique ID and a transform component.
        /// </summary>
        private void Initialize()
        {
            // Create a transform component. Every game object has one.
            Transform = new Transform3D { Owner = this };
        } // Initialize

        #endregion

        #region Add Component

        /// <summary>
        /// Adds a component of type TComponentType to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public override Component AddComponent<TComponentType>()
        {
            // Get from a pool or create the component.
            if (typeof(TComponentType) == typeof(Transform3D))
            {
                throw new ArgumentException("Game Object 3D: Unable to create the 3D transform component. The transform component can’t be replaced or removed.");
            }
            if (typeof(TComponentType) == typeof(Transform2D))
            {
                throw new ArgumentException("Game Object 3D: Unable to create the 2D transform component. A 3D Game Object does not work in 2D.");
            }
            if (typeof(TComponentType) == typeof(ModelFilter))
            {
                if (modelFilterAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the model filter component. There is one already.");
                }
                // Search for an empty component in the pool.
                modelFilterAccessor = ModelFilter.ModelFilterPool.Fetch();
                // A component is a reference value, so no problem to do this.
                ModelFilter = ModelFilter.ModelFilterPool[modelFilterAccessor];
                // Initialize the component to the default values.
                ModelFilter.Initialize(this);
                return ModelFilter;
            }
            if (typeof(TComponentType) == typeof(ModelRenderer))
            {
                if (modelRendererAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the model renderer component. There is one already.");
                }
                // Search for an empty component in the pool.
                modelRendererAccessor = ModelRenderer.ModelRendererPool.Fetch();
                // A component is a reference value, so no problem to do this.
                ModelRenderer = ModelRenderer.ModelRendererPool[modelRendererAccessor];
                // Initialize the component to the default values.
                ModelRenderer.Initialize(this);
                return ModelRenderer;
            }
            if (typeof(TComponentType) == typeof(RootAnimation))
            {
                if (rootAnimationAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the animation root component. There is one already.");
                }
                // Search for an empty component in the pool.
                rootAnimationAccessor = RootAnimation.RootAnimationPool.Fetch();
                // A component is a reference value, so no problem to do this.
                RootAnimation = RootAnimation.RootAnimationPool[rootAnimationAccessor];
                // Initialize the component to the default values.
                RootAnimation.Initialize(this);
                return RootAnimation;
            }
            throw new ArgumentException("Game Object 3D: Unknown component type.");
        } // AddComponent

        #endregion

        #region Remove Component

        /// <summary>
        /// Removes a component to the game object. 
        /// </summary>
        /// <remarks>
        /// A component is not really destroyed, is recycled, it returns to the component pool.
        /// </remarks>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public override void RemoveComponent<TComponentType>()
        {
            // Get from a pool or create the component.
            if (typeof(TComponentType) == typeof(Transform3D))
            {
                throw new ArgumentException("Game Object 3D: Unable to remove the 3D transform component. The transform component can’t be replaced or removed.");
            }
            if (typeof(TComponentType) == typeof(Transform2D))
            {
                throw new ArgumentException("Game Object 3D: Unable to remove the 2D transform component. A 3D Game Object does not work in 2D.");
            }
            if (typeof(TComponentType) == typeof(ModelFilter))
            {
                if (modelFilterAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the model filter component. There is not one.");
                }
                if (ModelRenderer != null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the model filter component. There are still components that use it.");
                }
                ModelFilter.Uninitialize();
                ModelFilter.ModelFilterPool.Release(modelFilterAccessor);
                ModelFilter = null;
                modelFilterAccessor = null;
            }
            if (typeof(TComponentType) == typeof(ModelRenderer))
            {
                if (modelRendererAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the model renderer component. There is not one.");
                }
                ModelRenderer.Uninitialize();
                ModelRenderer.ModelRendererPool.Release(modelRendererAccessor);
                ModelRenderer = null;
                modelRendererAccessor = null;
            }
            if (typeof(TComponentType) == typeof(RootAnimation))
            {
                if (rootAnimationAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the root animation component. There is not one.");
                }
                RootAnimation.Uninitialize();
                RootAnimation.RootAnimationPool.Release(rootAnimationAccessor);
                ModelRenderer = null;
                modelRendererAccessor = null;
            }
        } // RemoveComponent

        #endregion

    } // GameObject3D
} // XNAFinalEngine.Components
