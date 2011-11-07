
#region License
// Authors: Schneider, José Ignacio (Under XNA Final Engine license)
//          Schefer, Gustavo Martin (Under Microsoft Permisive License)
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using XNAFinalEngine.Assets;
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
        private Pool<RootAnimations>.Accessor rootAnimationAccessor;
        private Pool<ModelAnimations>.Accessor modelAnimationAccessor;
        private Pool<Camera>.Accessor cameraAccessor;
        private Pool<DirectionalLight>.Accessor directionalLightAccessor;
        private Pool<PointLight>.Accessor pointLightAccessor;
        private Pool<SpotLight>.Accessor spotLightAccessor;

        #endregion

        #region Components

        private ModelRenderer modelRenderer;
        private ModelFilter modelFilter;
        private RootAnimations rootAnimation;
        private ModelAnimations modelAnimation;
        private Camera camera;
        private Light light;
        private readonly List<Script> scripts = new List<Script>(2);

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The parent of this game object.
        /// </summary>
        public GameObject3D Parent
        {
            get { return Transform.Parent; }
            set { Transform.Parent = value; }
        } // Parent

        #region Components

        /// <summary>
        /// Associated transform component.
        /// </summary>
        public Transform3D Transform { get; private set; }
        
        /// <summary>
        /// Associated model renderer component.
        /// </summary>
        public ModelRenderer ModelRenderer
        {
            get { return modelRenderer; }
            private set
            {
                ModelRenderer oldValue = modelRenderer;
                modelRenderer = value;
                // Invoke event
                if (ModelRendererChanged != null)
                    ModelRendererChanged(this, oldValue, value);
            }
        } // ModelRenderer
        
        /// <summary>
        /// Associated model filter component.
        /// </summary>
        public ModelFilter ModelFilter
        {
            get { return modelFilter; }
            private set
            {
                ModelFilter oldValue = modelFilter;
                modelFilter = value;
                // Invoke event
                if (ModelFilterChanged != null)
                    ModelFilterChanged(this, oldValue, value);
            }
        } // ModelFilter
        
        /// <summary>
        /// Associated root animation component.
        /// </summary>
        public RootAnimations RootAnimations
        {
            get { return rootAnimation; }
            private set
            {
                RootAnimations oldValue = rootAnimation;
                rootAnimation = value;
                // Invoke event
                if (RootAnimationChanged != null)
                    RootAnimationChanged(this, oldValue, value);
            }
        } // RootAnimations
        
        /// <summary>
        /// Associated model animation component.
        /// </summary>
        public ModelAnimations ModelAnimations
        {
            get { return modelAnimation; }
            private set
            {
                ModelAnimations oldValue = modelAnimation;
                modelAnimation = value;
                // Invoke event
                if (ModelAnimationChanged != null)
                    ModelAnimationChanged(this, oldValue, value);
            }
        } // ModelAnimations

        /// <summary>
        /// Associated camera component.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            private set
            {
                Camera oldValue = camera;
                camera = value;
                // Invoke event
                if (CameraChanged != null)
                    CameraChanged(this, oldValue, value);
            }
        } // Camera

        /// <summary>
        /// Associated light component.
        /// </summary>
        public Light Light
        {
            get { return light; }
            private set
            {
                Light oldValue = light;
                light = value;
                // Invoke event
                if (LightChanged != null)
                    LightChanged(this, oldValue, value);
            }
        } // Light

        /// <summary>
        /// Associated directional light component.
        /// </summary>
        public DirectionalLight DirectionalLight { get; private set; }

        /// <summary>
        /// Associated point light component.
        /// </summary>
        public PointLight PointLight { get; private set; }

        /// <summary>
        /// Associated spot light component.
        /// </summary>
        public SpotLight SpotLight { get; private set; }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Raised when the game object's model renderer changes.
        /// </summary>
        public event ComponentEventHandler ModelRendererChanged;

        /// <summary>
        /// Raised when the game object's model filter changes.
        /// </summary>
        public event ComponentEventHandler ModelFilterChanged;

        /// <summary>
        /// Raised when the game object's root animation changes.
        /// </summary>
        public event ComponentEventHandler RootAnimationChanged;

        /// <summary>
        /// Raised when the game object's model animation changes.
        /// </summary>
        public event ComponentEventHandler ModelAnimationChanged;

        /// <summary>
        /// Raised when the game object's camera changes.
        /// </summary>
        public event ComponentEventHandler CameraChanged;

        /// <summary>
        /// Raised when the game object's light changes.
        /// </summary>
        public event ComponentEventHandler LightChanged;

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
        /// Base class for all entities in the scenes.
        /// </summary>
        public GameObject3D(string name, Model model, Material material)
        {
            Name = name;
            Initialize();
            AddComponent<ModelRenderer>();
            AddComponent<ModelFilter>();
            ModelFilter.Model = model;
            ModelRenderer.Material = material;
        } // GameObject3D

        /// <summary>
        /// Base class for all entities in the scenes.
        /// </summary>
        public GameObject3D(Model model, Material material)
        {
            Name = "GameObject3D-" + nameNumber;
            nameNumber++;
            Initialize();
            AddComponent<ModelRenderer>();
            AddComponent<ModelFilter>();
            ModelFilter.Model = model;
            ModelRenderer.Material = material;
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

            #region Transform

            // Get from a pool or create the component.
            if (typeof(TComponentType) == typeof(Transform3D))
            {
                throw new ArgumentException("Game Object 3D: Unable to create the 3D transform component. The transform component can’t be replaced or removed.");
            }
            if (typeof(TComponentType) == typeof(Transform2D))
            {
                throw new ArgumentException("Game Object 3D: Unable to create the 2D transform component. A 3D Game Object does not work in 2D.");
            }

            #endregion

            #region Model Filter

            if (typeof(TComponentType) == typeof(ModelFilter))
            {
                if (modelFilterAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the model filter component. There is one already.");
                }
                // Search for an empty component in the pool.
                modelFilterAccessor = ModelFilter.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                ModelFilter = ModelFilter.ComponentPool[modelFilterAccessor];
                // Initialize the component to the default values.
                ModelFilter.Initialize(this);
                return ModelFilter;
            }

            #endregion

            #region Model Renderer

            if (typeof(TComponentType) == typeof(ModelRenderer))
            {
                if (modelRendererAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the model renderer component. There is one already.");
                }
                // Search for an empty component in the pool.
                modelRendererAccessor = ModelRenderer.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                ModelRenderer = ModelRenderer.ComponentPool[modelRendererAccessor];
                // Initialize the component to the default values.
                ModelRenderer.Initialize(this);
                return ModelRenderer;
            }

            #endregion

            #region Root Animation

            if (typeof(TComponentType) == typeof(RootAnimations))
            {
                if (rootAnimationAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the root animation component. There is one already.");
                }
                // Search for an empty component in the pool.
                rootAnimationAccessor = RootAnimations.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                RootAnimations = RootAnimations.ComponentPool[rootAnimationAccessor];
                // Initialize the component to the default values.
                RootAnimations.Initialize(this);
                return RootAnimations;
            }

            #endregion

            #region Model Animation

            if (typeof(TComponentType) == typeof(ModelAnimations))
            {
                if (modelAnimationAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the model animation component. There is one already.");
                }
                // Search for an empty component in the pool.
                modelAnimationAccessor = ModelAnimations.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                ModelAnimations = ModelAnimations.ComponentPool[modelAnimationAccessor];
                // Initialize the component to the default values.
                ModelAnimations.Initialize(this);
                return ModelAnimations;
            }

            #endregion

            #region Camera

            if (typeof(TComponentType) == typeof(Camera))
            {
                if (cameraAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the camera component. There is one already.");
                }
                // Search for an empty component in the pool.
                cameraAccessor = Camera.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                Camera = Camera.ComponentPool[cameraAccessor];
                // Initialize the component to the default values.
                Camera.Initialize(this);
                return Camera;
            }

            #endregion

            #region Light

            if (typeof(Light).IsAssignableFrom(typeof(TComponentType)))
            {
                if (Light != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the light component. There is one already.");
                }
                if (typeof(TComponentType) == typeof(DirectionalLight))
                {
                    // Search for an empty component in the pool.
                    directionalLightAccessor = DirectionalLight.ComponentPool.Fetch();
                    // A component is a reference value, so no problem to do this.
                    Light = DirectionalLight.ComponentPool[directionalLightAccessor];
                    DirectionalLight = DirectionalLight.ComponentPool[directionalLightAccessor];
                }
                else if (typeof(TComponentType) == typeof(PointLight))
                {
                    // Search for an empty component in the pool.
                    pointLightAccessor = PointLight.ComponentPool.Fetch();
                    // A component is a reference value, so no problem to do this.
                    Light = PointLight.ComponentPool[pointLightAccessor];
                    PointLight = PointLight.ComponentPool[pointLightAccessor];
                }
                else if (typeof(TComponentType) == typeof(SpotLight))
                {
                    // Search for an empty component in the pool.
                    spotLightAccessor = SpotLight.ComponentPool.Fetch();
                    // A component is a reference value, so no problem to do this.
                    Light = SpotLight.ComponentPool[spotLightAccessor];
                    SpotLight = SpotLight.ComponentPool[spotLightAccessor];
                }
                else
                    throw new ArgumentException("Game Object 3D: Unable to create the light component.");
                // Initialize the component to the default values.
                Light.Initialize(this);
                return Light;
            }

            #endregion

            #region Script
            
            // Hacerlo de otra manera, creo. Algo como add script y remove script. Los script se comportan diferente que el resto de los componentes.
            if (typeof(Script).IsAssignableFrom(typeof(TComponentType)))
            {
                Component script = new TComponentType();
                script.Initialize(this);
                Script.ScriptList.Add((Script)script);
                return script;
            }

            #endregion

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

            #region Transform

            if (typeof(TComponentType) == typeof(Transform3D))
            {
                throw new ArgumentException("Game Object 3D: Unable to remove the 3D transform component. The transform component can’t be replaced or removed.");
            }
            if (typeof(TComponentType) == typeof(Transform2D))
            {
                throw new ArgumentException("Game Object 3D: Unable to remove the 2D transform component. A 3D Game Object does not work in 2D.");
            }

            #endregion

            #region Model Filter

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
                ModelFilter.ComponentPool.Release(modelFilterAccessor);
                ModelFilter = null;
                modelFilterAccessor = null;
            }

            #endregion

            #region Model Renderer

            if (typeof(TComponentType) == typeof(ModelRenderer))
            {
                if (modelRendererAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the model renderer component. There is not one.");
                }
                ModelRenderer.Uninitialize();
                ModelRenderer.ComponentPool.Release(modelRendererAccessor);
                ModelRenderer = null;
                modelRendererAccessor = null;
            }

            #endregion

            #region Animation

            if (typeof(TComponentType) == typeof(RootAnimations))
            {
                if (rootAnimationAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the root animation component. There is not one.");
                }
                RootAnimations.Uninitialize();
                RootAnimations.ComponentPool.Release(rootAnimationAccessor);
                RootAnimations = null;
                rootAnimationAccessor = null;
            }
            if (typeof(TComponentType) == typeof(ModelAnimations))
            {
                if (modelAnimationAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the model animation component. There is not one.");
                }
                ModelAnimations.Uninitialize();
                ModelAnimations.ComponentPool.Release(modelAnimationAccessor);
                ModelAnimations = null;
                modelAnimationAccessor = null;
            }

            #endregion

            #region Camera

            if (typeof(TComponentType) == typeof(Camera))
            {
                if (cameraAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the camera component. There is not one.");
                }
                Camera.Uninitialize();
                Camera.ComponentPool.Release(cameraAccessor);
                Camera = null;
                cameraAccessor = null;
            }

            #endregion

            #region Light

            if (typeof(TComponentType) == typeof(Light))
            {
                if (Light == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the light component. There is not one.");
                }
                Light.Uninitialize();
                if (typeof(TComponentType) == typeof(DirectionalLight))
                {
                    DirectionalLight.ComponentPool.Release(directionalLightAccessor);
                    directionalLightAccessor = null;
                    DirectionalLight = null;
                }
                if (typeof(TComponentType) == typeof(PointLight))
                {
                    PointLight.ComponentPool.Release(pointLightAccessor);
                    pointLightAccessor = null;
                    PointLight = null;
                }
                if (typeof(TComponentType) == typeof(SpotLight))
                {
                    SpotLight.ComponentPool.Release(spotLightAccessor);
                    spotLightAccessor = null;
                    SpotLight = null;
                }
                Light = null;
            }

            #endregion

        } // RemoveComponent

        #endregion
        
    } // GameObject3D
} // XNAFinalEngine.Components
