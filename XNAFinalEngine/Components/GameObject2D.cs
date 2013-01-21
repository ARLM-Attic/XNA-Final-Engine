
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
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// 2D Game Objects has a special transform component that works in 2D screen space plus depth [0,1) to sort rendering order.
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
        internal event ComponentEventHandler HudTextChanged;

        /// <summary>
        /// Raised when the game object's HUD texture changes.
        /// </summary>
        internal event ComponentEventHandler HudTextureChanged;

        /// <summary>
        /// Raised when the game object's video renderer changes.
        /// </summary>
        internal event ComponentEventHandler VideoRendererChanged;

        /// <summary>
        /// Raised when the game object's line renderer changes.
        /// </summary>
        internal event ComponentEventHandler LineRendererChanged;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Game Object 2D.
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

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            // A disposed object could be still generating events, because it is alive for a time, in a disposed state, but alive nevertheless.
            HudTextChanged = null;
            HudTextureChanged = null;
            VideoRendererChanged = null;
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
                result = HudText;
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
                result = HudTexture;
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
                VideoRenderer = VideoRenderer.ComponentPool[videoRendererAccessor];
                // Initialize the component to the default values.
                videoRenderer.Initialize(this);
                result = videoRenderer;
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
                LineRenderer = LineRenderer.ComponentPool2D[lineRendererAccessor];
                // Initialize the component to the default values.
                lineRenderer.Initialize(this);
                result = lineRenderer;
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

            if (result == null)
                throw new ArgumentException("Game Object 2D: Unknown component type.");
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
                Components.Remove(HudText);
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
                Components.Remove(HudTexture);
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
                Components.Remove(VideoRenderer);
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
                Components.Remove(LineRenderer);
                LineRenderer.ComponentPool2D.Release(lineRendererAccessor);
                LineRenderer = null;
                lineRendererAccessor = null;
            }

            #endregion

            #region Script

            if (typeof(Script).IsAssignableFrom(typeof(TComponentType)))
            {
                Component script = XNAFinalEngine.Components.Script.ContainScript<TComponentType>(this);
                if (script == null)
                {
                    throw new ArgumentException("Game Object 2D: Unable to remove the script component. There is not one.");
                }
                script.Uninitialize();
                Components.Remove(script);
                scripts.Remove((Script)script);
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
            if (HudText != null)
                RemoveComponent<HudText>();
            if (HudTexture != null)
                RemoveComponent<HudTexture>();
            if (VideoRenderer != null)
                RemoveComponent<VideoRenderer>();
            if (LineRenderer != null)
                RemoveComponent<LineRenderer>();
            foreach (var script in scripts)
                script.Uninitialize();
            scripts.Clear();
        } // RemoveAllComponents

        #endregion

    } // GameObject2D
} // XNAFinalEngine.Components
