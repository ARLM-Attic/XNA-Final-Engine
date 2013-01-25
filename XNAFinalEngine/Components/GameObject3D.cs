
#region License
/*
Copyright (c) 2008-2013, Schneider, José Ignacio and Schefer, Gustavo Martin
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	The names of its contributors cannot be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Authors: Schneider, José Ignacio (jischneider@hotmail.com)
         Schefer, Gustavo Martin
-----------------------------------------------------------------------------------------------------------------------------------------------
*/
#endregion

#region Using directives
using System;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// 3D Game Objects has a special transform component that works in 3D world space.
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
        private Pool<ParticleEmitter>.Accessor particleEmitterAccessor;
        private Pool<ParticleRenderer>.Accessor particleRendererAccessor;
        private Pool<SoundEmitter>.Accessor soundEmitterAccessor;
        private Pool<SoundListener>.Accessor soundListenerAccessor;
        private Pool<HudText>.Accessor hudTextAccessor;
        private Pool<HudTexture>.Accessor hudTextureAccessor;        
        private Pool<LineRenderer>.Accessor lineRendererAccessor;
        private Pool<RigidBody>.Accessor rigidBodyAccessor;
        private Pool<StaticCollider>.Accessor staticColliderAccessor;

        #endregion

        #region Components

        private ModelRenderer modelRenderer;
        private ModelFilter modelFilter;
        private RootAnimations rootAnimation;
        private ModelAnimations modelAnimation;
        private Camera camera;
        private Light light;
        private ParticleEmitter particleEmitter;
        private ParticleRenderer particleRenderer;
        private SoundEmitter soundEmitter;
        private SoundListener soundListener;
        private HudText hudText;
        private HudTexture hudTexture;        
        private LineRenderer lineRenderer;
        private RigidBody rigidBody;
        private StaticCollider staticCollider;

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

        #region Transform

        /// <summary>
        /// Associated transform component.
        /// </summary>
        public Transform3D Transform { get; private set; }

        #endregion

        #region Model Renderer

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

        #endregion

        #region Model Filter

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

        #endregion

        #region Root Animations

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

        #endregion

        #region Model Animations

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

        #endregion

        #region Camera

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

        #endregion

        #region Lights

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

        #region Particle Emitter

        /// <summary>
        /// Associated particle emitter component.
        /// </summary>
        public ParticleEmitter ParticleEmitter
        {
            get { return particleEmitter; }
            private set
            {
                ParticleEmitter oldValue = particleEmitter;
                particleEmitter = value;
                // Invoke event
                if (ParticleEmitterChanged != null)
                    ParticleEmitterChanged(this, oldValue, value);
            }
        } // ParticleEmitter

        #endregion

        #region Particle Renderer

        /// <summary>
        /// Associated particle renderer component.
        /// </summary>
        public ParticleRenderer ParticleRenderer
        {
            get { return particleRenderer; }
            private set
            {
                ParticleRenderer oldValue = particleRenderer;
                particleRenderer = value;
                // Invoke event
                if (ParticleRendererChanged != null)
                    ParticleRendererChanged(this, oldValue, value);
            }
        } // ParticleRenderer

        #endregion

        #region Sound Emitter

        /// <summary>
        /// Associated sound emitter component.
        /// </summary>
        public SoundEmitter SoundEmitter
        {
            get { return soundEmitter; }
            private set
            {
                SoundEmitter oldValue = soundEmitter;
                soundEmitter = value;
                // Invoke event
                if (SoundEmitterChanged != null)
                    SoundEmitterChanged(this, oldValue, value);
            }
        } // SoundEmitter

        #endregion

        #region Sound Listener

        /// <summary>
        /// Associated sound listener component.
        /// </summary>
        public SoundListener SoundListener
        {
            get { return soundListener; }
            private set
            {
                SoundListener oldValue = soundListener;
                soundListener = value;
                // Invoke event
                if (SoundListenerChanged != null)
                    SoundListenerChanged(this, oldValue, value);
            }
        } // SoundListener

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

        #region Rigid Body

        /// <summary>
        /// Associated rigid body component.
        /// </summary>
        public RigidBody RigidBody
        {
            get { return rigidBody; }
            private set 
            {
                RigidBody oldValue = rigidBody;
                rigidBody = value; 
                // Invoke event
                if (RigidBodyChanged != null)
                    RigidBodyChanged(this, oldValue, value);
            }
        }

        #endregion

        #region Static Collider

        /// <summary>
        /// Associated static collider component.
        /// </summary>
        public StaticCollider StaticCollider
        {
            get { return staticCollider; }
            private set
            {
                StaticCollider oldValue = staticCollider;
                staticCollider = value;
                // Invoke event
                if (StaticColliderChanged != null)
                    StaticColliderChanged(this, oldValue, value);
            }
        }

        #endregion
        
        #endregion

        #region Events

        /// <summary>
        /// Raised when the game object's model renderer changes.
        /// </summary>
        internal event ComponentEventHandler ModelRendererChanged;

        /// <summary>
        /// Raised when the game object's model filter changes.
        /// </summary>
        internal event ComponentEventHandler ModelFilterChanged;

        /// <summary>
        /// Raised when the game object's root animation changes.
        /// </summary>
        internal event ComponentEventHandler RootAnimationChanged;

        /// <summary>
        /// Raised when the game object's model animation changes.
        /// </summary>
        internal event ComponentEventHandler ModelAnimationChanged;

        /// <summary>
        /// Raised when the game object's camera changes.
        /// </summary>
        internal event ComponentEventHandler CameraChanged;

        /// <summary>
        /// Raised when the game object's light changes.
        /// </summary>
        internal event ComponentEventHandler LightChanged;

        /// <summary>
        /// Raised when the game object's particle emitter changes.
        /// </summary>
        internal event ComponentEventHandler ParticleEmitterChanged;

        /// <summary>
        /// Raised when the game object's particle renderer changes.
        /// </summary>
        internal event ComponentEventHandler ParticleRendererChanged;

        /// <summary>
        /// Raised when the game object's sound emitter changes.
        /// </summary>
        internal event ComponentEventHandler SoundEmitterChanged;

        /// <summary>
        /// Raised when the game object's sound listener changes.
        /// </summary>
        internal event ComponentEventHandler SoundListenerChanged;

        /// <summary>
        /// Raised when the game object's HUD text changes.
        /// </summary>
        internal event ComponentEventHandler HudTextChanged;

        /// <summary>
        /// Raised when the game object's HUD texture changes.
        /// </summary>
        internal event ComponentEventHandler HudTextureChanged;

        /// <summary>
        /// Raised when the game object's line renderer changes.
        /// </summary>
        internal event ComponentEventHandler LineRendererChanged;

        /// <summary>
        /// Raised when the game object's rigid body changes.
        /// </summary>
        internal event ComponentEventHandler RigidBodyChanged;

        /// <summary>
        /// Raised when the game object's static collider changes.
        /// </summary>
        internal event ComponentEventHandler StaticColliderChanged;

        #endregion

        #region Constructor

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
        /// Set a transform component for this game object.
        /// </summary>
        private void Initialize()
        {
            // Create a transform component. Every game object has one.
            Transform = new Transform3D { Owner = this };
        } // Initialize

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            // A disposed object could be still generating events, because it is alive for a time, in a disposed state, but alive nevertheless.
            ModelRendererChanged = null;
            ModelFilterChanged = null;
            RootAnimationChanged = null;
            ModelAnimationChanged = null;
            CameraChanged = null;
            LightChanged = null;
            ParticleEmitterChanged = null;
            ParticleRendererChanged = null;
            SoundEmitterChanged = null;
            SoundListenerChanged = null;
            HudTextChanged = null;
            HudTextureChanged = null;
            LineRendererChanged = null;
            Transform.Parent = null;
            base.DisposeManagedResources();
        } // DisposeManagedResources

        #endregion

        #region Add Component

        /// <summary>
        /// Adds a component of type TComponentType to the game object.
        /// </summary>
        /// <typeparam name="TComponentType">Component Type</typeparam>
        public override Component AddComponent<TComponentType>()
        {
            Component result = null;

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
                result = ModelFilter;
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
                result = ModelRenderer;
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
                result = RootAnimations;
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
                result = ModelAnimations;
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
                result = Camera;
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
                result = Light;
            }

            #endregion

            #region Particle Emitter

            if (typeof(ParticleEmitter).IsAssignableFrom(typeof(TComponentType)))
            {
                if (ParticleEmitter != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the particle emitter component. There is one already.");
                }
                // Search for an empty component in the pool.
                particleEmitterAccessor = ParticleEmitter.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                ParticleEmitter = ParticleEmitter.ComponentPool[particleEmitterAccessor];
                // Initialize the component to the default values.
                ParticleEmitter.Initialize(this);
                result = ParticleEmitter;
            }

            #endregion

            #region Particle Renderer

            if (typeof(TComponentType) == typeof(ParticleRenderer))
            {
                if (particleRendererAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the particle renderer component. There is one already.");
                }
                // Search for an empty component in the pool.
                particleRendererAccessor = ParticleRenderer.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                ParticleRenderer = ParticleRenderer.ComponentPool[particleRendererAccessor];
                // Initialize the component to the default values.
                ParticleRenderer.Initialize(this);
                result = ParticleRenderer;
            }

            #endregion

            #region Script
            
            if (typeof(Script).IsAssignableFrom(typeof(TComponentType)))
            {
                Component script = XNAFinalEngine.Components.Script.ContainScript<TComponentType>(this);
                if (script != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the script component. There is one already.");
                }
                script = XNAFinalEngine.Components.Script.FetchScript<TComponentType>();
                if (script == null)
                {
                    script = new TComponentType();
                    XNAFinalEngine.Components.Script.ScriptList.Add((Script)script);
                }
                script.Initialize(this);
                scripts.Add((Script)script);
                result = script;
            }

            #endregion

            #region Sound Emitter

            if (typeof(TComponentType) == typeof(SoundEmitter))
            {
                if (soundEmitterAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the sound emitter component. There is one already.");
                }
                // Search for an empty component in the pool.
                soundEmitterAccessor = SoundEmitter.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                SoundEmitter = SoundEmitter.ComponentPool[soundEmitterAccessor];
                // Initialize the component to the default values.
                SoundEmitter.Initialize(this);
                result = SoundEmitter;
            }

            #endregion

            #region Sound Listener

            if (typeof(TComponentType) == typeof(SoundListener))
            {
                if (soundListenerAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the sound listener component. There is one already.");
                }
                // Search for an empty component in the pool.
                soundListenerAccessor = SoundListener.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                SoundListener = SoundListener.ComponentPool[soundListenerAccessor];
                // Initialize the component to the default values.
                SoundListener.Initialize(this);
                result = SoundListener;
            }

            #endregion

            #region HUD Text

            if (typeof(TComponentType) == typeof(HudText))
            {
                if (hudTextAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the HUD text component. There is one already.");
                }
                // Search for an empty component in the pool.
                hudTextAccessor = HudText.ComponentPool3D.Fetch();
                // A component is a reference value, so no problem to do this.
                HudText = HudText.ComponentPool3D[hudTextAccessor];
                // Initialize the component to the default values.
                HudText.Initialize(this);
                result = HudText;
            }

            #endregion

            #region HUD Texture

            if (typeof(TComponentType) == typeof(HudTexture))
            {
                if (hudTextureAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the HUD texture component. There is one already.");
                }
                // Search for an empty component in the pool.
                hudTextureAccessor = HudTexture.ComponentPool3D.Fetch();
                // A component is a reference value, so no problem to do this.
                HudTexture = HudTexture.ComponentPool3D[hudTextureAccessor];
                // Initialize the component to the default values.
                HudTexture.Initialize(this);
                result = HudTexture;
            }

            #endregion

            #region Line Renderer

            if (typeof(TComponentType) == typeof(LineRenderer))
            {
                if (lineRendererAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the line renderer component. There is one already.");
                }
                // Search for an empty component in the pool.
                lineRendererAccessor = LineRenderer.ComponentPool3D.Fetch();
                // A component is a reference value, so no problem to do this.
                LineRenderer = LineRenderer.ComponentPool3D[lineRendererAccessor];
                // Initialize the component to the default values.
                lineRenderer.Initialize(this);
                result = lineRenderer;
            }

            #endregion

            #region Rigid Body

            if (typeof(TComponentType) == typeof(RigidBody))
            {
                if (rigidBodyAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the rigid body component. There is one already.");
                }
                // Search for an empty component in the pool.
                rigidBodyAccessor = RigidBody.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                RigidBody = RigidBody.ComponentPool[rigidBodyAccessor];
                // Initialize the component to the default values.
                rigidBody.Initialize(this);
                result = rigidBody;
            }

            #endregion

            #region Static Collider

            if (typeof(TComponentType) == typeof(StaticCollider))
            {
                if (staticColliderAccessor != null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to create the static collider component. There is one already.");
                }
                // Search for an empty component in the pool.
                staticColliderAccessor = StaticCollider.ComponentPool.Fetch();
                // A component is a reference value, so no problem to do this.
                StaticCollider = StaticCollider.ComponentPool[staticColliderAccessor];
                // Initialize the component to the default values.
                staticCollider.Initialize(this);
                result = staticCollider;
            }

            #endregion

            if (result == null)
                throw new ArgumentException("Game Object 3D: Unknown component type.");
            Components.Add(result);
            return result;
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
                ModelFilter.Uninitialize();
                Components.Remove(ModelFilter);
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
                Components.Remove(ModelRenderer);
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
                Components.Remove(RootAnimations);
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
                Components.Remove(ModelAnimations);
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
                Components.Remove(Camera);
                Camera.ComponentPool.Release(cameraAccessor);
                Camera = null;
                cameraAccessor = null;
                Camera.SortCamerasByRenderingOrder();
            }

            #endregion

            #region Light

            if (typeof(Light).IsAssignableFrom(typeof(TComponentType)))
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
                else if (typeof(TComponentType) == typeof(PointLight))
                {
                    PointLight.ComponentPool.Release(pointLightAccessor);
                    pointLightAccessor = null;
                    PointLight = null;
                }
                else if (typeof(TComponentType) == typeof(SpotLight))
                {
                    SpotLight.ComponentPool.Release(spotLightAccessor);
                    spotLightAccessor = null;
                    SpotLight = null;
                }
                else
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the light component.");
                }
                Components.Remove(Light);
                Light = null;
            }

            #endregion

            #region Particle Emitter

            if (typeof(TComponentType) == typeof(ParticleEmitter))
            {
                if (particleEmitterAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the particle emitter component. There is not one.");
                }
                ParticleEmitter.Uninitialize();
                Components.Remove(ParticleEmitter);
                ParticleEmitter.ComponentPool.Release(particleEmitterAccessor);
                ParticleEmitter = null;
                particleEmitterAccessor = null;
            }

            #endregion

            #region Particle Renderer

            if (typeof(TComponentType) == typeof(ParticleRenderer))
            {
                if (particleRendererAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the particle renderer component. There is not one.");
                }
                ParticleRenderer.Uninitialize();
                Components.Remove(ParticleRenderer);
                ParticleRenderer.ComponentPool.Release(particleRendererAccessor);
                ParticleRenderer = null;
                particleRendererAccessor = null;
            }

            #endregion

            #region Script

            if (typeof(Script).IsAssignableFrom(typeof(TComponentType)))
            {
                Component script = XNAFinalEngine.Components.Script.ContainScript<TComponentType>(this);
                if (script == null)
                {
                    throw new ArgumentException("Game Object 3D: Unable to remove the script component. There is not one.");
                }
                script.Uninitialize();
                Components.Remove(script);
                scripts.Remove((Script)script);
            }

            #endregion

            #region Sound Emitter

            if (typeof(TComponentType) == typeof(SoundEmitter))
            {
                if (soundEmitterAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the sound emitter component. There is not one.");
                }
                SoundEmitter.Uninitialize();
                Components.Remove(SoundEmitter);
                SoundEmitter.ComponentPool.Release(soundEmitterAccessor);
                SoundEmitter = null;
                soundEmitterAccessor = null;
            }

            #endregion

            #region Sound Listener

            if (typeof(TComponentType) == typeof(SoundListener))
            {
                if (soundListenerAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the sound listener component. There is not one.");
                }
                SoundListener.Uninitialize();
                Components.Remove(SoundListener);
                SoundListener.ComponentPool.Release(soundListenerAccessor);
                SoundListener = null;
                soundListenerAccessor = null;
            }

            #endregion

            #region HUD Text

            if (typeof(TComponentType) == typeof(HudText))
            {
                if (hudTextAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the HUD text component. There is not one.");
                }
                HudText.Uninitialize();
                Components.Remove(HudText);
                HudText.ComponentPool3D.Release(hudTextAccessor);
                HudText = null;
                hudTextAccessor = null;
            }

            #endregion

            #region HUD Texture

            if (typeof(TComponentType) == typeof(HudTexture))
            {
                if (hudTextureAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the HUD texture component. There is not one.");
                }
                HudTexture.Uninitialize();
                Components.Remove(HudTexture);
                HudTexture.ComponentPool3D.Release(hudTextureAccessor);
                HudTexture = null;
                hudTextureAccessor = null;
            }

            #endregion

            #region Line Renderer

            if (typeof(TComponentType) == typeof(LineRenderer))
            {
                if (lineRendererAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the line renderer component. There is not one.");
                }
                LineRenderer.Uninitialize();
                Components.Remove(LineRenderer);
                LineRenderer.ComponentPool3D.Release(lineRendererAccessor);
                LineRenderer = null;
                lineRendererAccessor = null;
            }

            #endregion

            #region Rigid Body

            if (typeof(TComponentType) == typeof(RigidBody))
            {
                if (rigidBodyAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the rigid body component. There is not one.");
                }
                RigidBody.Uninitialize();
                Components.Remove(RigidBody);
                RigidBody.ComponentPool.Release(rigidBodyAccessor);
                RigidBody = null;
                rigidBodyAccessor = null;
            }

            #endregion

            #region Static Collider

            if (typeof(TComponentType) == typeof(StaticCollider))
            {
                if (staticColliderAccessor == null)
                {
                    throw new InvalidOperationException("Game Object 3D: Unable to remove the static collider component. There is not one.");
                }
                StaticCollider.Uninitialize();
                Components.Remove(StaticCollider);
                StaticCollider.ComponentPool.Release(staticColliderAccessor);
                StaticCollider = null;
                staticColliderAccessor = null;
            }

            #endregion

        } // RemoveComponent

        #endregion

        #region Remove All Components

        /// <summary>
        /// Removes all components from the game object.
        /// </summary>
        public override void RemoveAllComponents()
        {
            if (ModelFilter != null)
                RemoveComponent<ModelFilter>();
            if (ModelRenderer != null)
                RemoveComponent<ModelRenderer>();
            if (RootAnimations != null)
                RemoveComponent<RootAnimations>();
            if (ModelAnimations != null)
                RemoveComponent<ModelAnimations>();
            if (Camera != null)
                RemoveComponent<Camera>();
            if (DirectionalLight != null)
                RemoveComponent<DirectionalLight>();
            if (PointLight != null)
                RemoveComponent<PointLight>();
            if (SpotLight != null)
                RemoveComponent<SpotLight>();
            if (ParticleEmitter != null)
                RemoveComponent<ParticleEmitter>();
            if (ParticleRenderer != null)
                RemoveComponent<ParticleRenderer>();
            if (SoundEmitter != null)
                RemoveComponent<SoundEmitter>();
            if (SoundListener != null)
                RemoveComponent<SoundListener>();
            if (HudText != null)
                RemoveComponent<HudText>();
            if (HudTexture != null)
                RemoveComponent<HudTexture>();
            if (LineRenderer != null)
                RemoveComponent<LineRenderer>();
            if (RigidBody != null)
                RemoveComponent<RigidBody>();
            if (StaticCollider != null)
                RemoveComponent<StaticCollider>();
            foreach (var script in scripts)
                script.Uninitialize();
            scripts.Clear();
        } // RemoveAllComponents

        #endregion

    } // GameObject3D
} // XNAFinalEngine.Components
