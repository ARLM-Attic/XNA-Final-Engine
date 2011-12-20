
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
    public class GameObject2D : GameObject
    {

        #region Variables

        /// <summary>
        /// The count of nameless game objects for naming purposes.
        /// </summary>
        private static int nameNumber = 1;

        #region Accessors

        private Pool<HudText>.Accessor hudTextAccessor;
        private Pool<HudTexture>.Accessor hudTextureAccessor;
        private Pool<VideoRenderer>.Accessor videoRendererAccessor;
        private Pool<LineRenderer>.Accessor lineRendererAccessor;

        #endregion

        #region Components

        private HudText hudText;
        private HudTexture hudTexture;
        private VideoRenderer videoRenderer;
        private LineRenderer lineRenderer;

        #endregion

        #endregion

        #region Properties

        #region Transform

        /// <summary>
        /// Associated transform component.
        /// </summary>
        public Transform2D Transform { get; private set; }

        #endregion

        #region HUD Text

        /// <summary>
        /// Associated Hud Text component.
        /// </summary>
        public HudText HudText
        {
            get { return hudText; }
            private set
            {
                HudText oldValue = hudText;
                hudText = value;
                // Invoke event
                if (HudTextChanged != null)
                    HudTextChanged(this, oldValue, value);
            }
        } // HudText

        #endregion

        #region HUD Texture

        /// <summary>
        /// Associated Hud Text component.
        /// </summary>
        public HudTexture HudTexture
        {
            get { return hudTexture; }
            private set
            {
                HudTexture oldValue = hudTexture;
                hudTexture = value;
                // Invoke event
                if (HudTextureChanged != null)
                    HudTextureChanged(this, oldValue, value);
            }
        } // HudTexture

        #endregion

        #region Video Renderer

        /// <summary>
        /// Associated video renderer component.
        /// </summary>
        public VideoRenderer VideoRenderer
        {
            get { return videoRenderer; }
            private set
            {
                VideoRenderer oldValue = videoRenderer;
                videoRenderer = value;
                // Invoke event
                if (VideoRendererChanged != null)
                    VideoRendererChanged(this, oldValue, value);
            }
        } // VideoRenderer

        #endregion

        #region Line Renderer

        /// <summary>
        /// Associated line renderer component.
        /// </summary>
        public LineRenderer LineRenderer
        {
            get { return lineRenderer; }
            private set
            {
                LineRenderer oldValue = lineRenderer;
                lineRenderer = value;
                // Invoke event
                if (LineRendererChanged != null)
                    LineRendererChanged(this, oldValue, value);
            }
        } // LineRenderer

        #endregion

        #region Parent

        /// <summary>
        /// The parent of this game object.
        /// </summary>
        public GameObject2D Parent
        {
            get { return Transform.Parent; }
            set { Transform.Parent = value; }
        } // Parent

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Raised when the game object's HUD text changes.
        /// </summary>
        public event ComponentEventHandler HudTextChanged;

        /// <summary>
        /// Raised when the game object's HUD texture changes.
        /// </summary>
        public event ComponentEventHandler HudTextureChanged;

        /// <summary>
        /// Raised when the game object's video renderer changes.
        /// </summary>
        public event ComponentEventHandler VideoRendererChanged;

        /// <summary>
        /// Raised when the game object's line renderer changes.
        /// </summary>
        public event ComponentEventHandler LineRendererChanged;

        #endregion
        
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public GameObject2D(string name)
        {
            Name = name;
            Initialize();
        } // GameObject2D

        /// <summary>
        /// 
        /// </summary>
        public GameObject2D()
        {
            Name = "GameObject3D-" + nameNumber;
            nameNumber++;
            Initialize();
        } // GameObject2D

        /// <summary>
        /// Set a transform component.
        /// </summary>
        private void Initialize()
        {
            // Create a transform component. Every game object has one.
            Transform = new Transform2D { Owner = this };

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
            if (typeof(TComponentType) == typeof(Transform2D))
            {
                throw new ArgumentException("Game Object exception. Unable to create the 2D transform component. The transform component can’t be replaced or removed.");
            }
            if (typeof(TComponentType) == typeof(Transform3D))
            {
                throw new ArgumentException("Game Object exception. Unable to create the 3D transform component. A 2D Game Object does not work in 3D.");
            }

            #endregion

            #region HUD Text

            if (typeof(TComponentType) == typeof(HudText))
            {
                if (hudTextAccessor != null)
                {
                    throw new ArgumentException("Game Object 2D: Unable to create the HUD text component. There is one already.");
                }
                // Search for an empty component in the pool.
                hudTextAccessor = HudText.ComponentPool2D.Fetch();
                // A component is a reference value, so no problem to do this.
                HudText = HudText.ComponentPool2D[hudTextAccessor];
                // Initialize the component to the default values.
                HudText.Initialize(this);
                return HudText;
            }

            #endregion

            #region HUD Texture

            if (typeof(TComponentType) == typeof(HudTexture))
            {
                if (hudTextureAccessor != null)
                {
                    throw new ArgumentException("Game Object 2D: Unable to create the HUD texture component. There is one already.");
                }
                // Search for an empty component in the pool.
                hudTextureAccessor = HudTexture.ComponentPool2D.Fetch();
                // A component is a reference value, so no problem to do this.
                HudTexture = HudTexture.ComponentPool2D[hudTextureAccessor];
                // Initialize the component to the default values.
                HudTexture.Initialize(this);
                return HudTexture;
            }

            #endregion

            #region Video Renderer

            if (typeof(TComponentType) == typeof(VideoRenderer))
            {
                if (videoRendererAccessor != null)
                {
                    throw new ArgumentException("Game Object 2D: Unable to create the video renderer component. There is one already.");
                }
                // Search for an empty component in the pool.
                videoRendererAccessor = VideoRenderer.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                videoRenderer = VideoRenderer.ComponentPool[videoRendererAccessor];
                // Initialize the component to the default values.
                videoRenderer.Initialize(this);
                return videoRenderer;
            }

            #endregion

            #region Line Renderer

            if (typeof(TComponentType) == typeof(LineRenderer))
            {
                if (lineRendererAccessor != null)
                {
                    throw new ArgumentException("Game Object 2D: Unable to create the line renderer component. There is one already.");
                }
                // Search for an empty component in the pool.
                lineRendererAccessor = LineRenderer.ComponentPool2D.Fetch();
                // A component is a reference value, so no problem to do this.
                lineRenderer = LineRenderer.ComponentPool2D[lineRendererAccessor];
                // Initialize the component to the default values.
                lineRenderer.Initialize(this);
                return lineRenderer;
            }

            #endregion

            throw new ArgumentException("Game Object 2D: Unknown component type.");
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

            if (typeof(TComponentType) == typeof(Transform2D))
            {
                throw new ArgumentException("Game Object 2D: Unable to remove the 2D transform component. The transform component can’t be replaced or removed.");
            }
            if (typeof(TComponentType) == typeof(Transform3D))
            {
                throw new ArgumentException("Game Object 2D: Unable to remove the 3D transform component. A 2D Game Object does not work in 3D.");
            }

            #endregion

            #region HUD Text

            if (typeof(TComponentType) == typeof(HudText))
            {
                if (hudTextAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 2D: Unable to remove the HUD text component. There is not one.");
                }
                HudText.Uninitialize();
                HudText.ComponentPool2D.Release(hudTextAccessor);
                HudText = null;
                hudTextAccessor = null;
            }

            #endregion

            #region HUD Texture

            if (typeof(TComponentType) == typeof(HudTexture))
            {
                if (hudTextureAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 2D: Unable to remove the HUD texture component. There is not one.");
                }
                HudTexture.Uninitialize();
                HudTexture.ComponentPool2D.Release(hudTextureAccessor);
                HudTexture = null;
                hudTextureAccessor = null;
            }

            #endregion

            #region Video Renderer

            if (typeof(TComponentType) == typeof(VideoRenderer))
            {
                if (videoRendererAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 2D: Unable to remove the video renderer component. There is not one.");
                }
                VideoRenderer.Uninitialize();
                VideoRenderer.ComponentPool.Release(videoRendererAccessor);
                VideoRenderer = null;
                videoRendererAccessor = null;
            }

            #endregion

            #region Line Renderer

            if (typeof(TComponentType) == typeof(LineRenderer))
            {
                if (lineRendererAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 2D: Unable to remove the line renderer component. There is not one.");
                }
                LineRenderer.Uninitialize();
                LineRenderer.ComponentPool2D.Release(lineRendererAccessor);
                LineRenderer = null;
                lineRendererAccessor = null;
            }

            #endregion

        } // RemoveComponent

        #endregion

    } // GameObject2D
} // XNAFinalEngine.Components
