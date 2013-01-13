
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
using System.Xml.Serialization;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Base class for everything attached to GameObjects.
    /// </summary>
    public class Component
    {

        #region Variables

        // A simple but effective way of having unique ids.
        // We can have 18.446.744.073.709.551.616 game object creations before the system "collapse". Almost infinite in practice. 
        // If a more robust system is needed (networking/threading) then you can use the guid structure: http://msdn.microsoft.com/en-us/library/system.guid.aspx
        // However this method is slightly simpler, slightly faster and has slightly lower memory requirements.
        // If performance is critical consider the int type (4.294.967.294 unique values).
        private static long uniqueIdCounter = long.MinValue;

        /// <summary>
        /// Chaded game object's layer mask value.
        /// Every component tests if its layer mask is currently valid.
        /// </summary>
        private uint cachedLayerMask;

        /// <summary>
        /// Chaded game object's active value.
        /// Every component tests if its owner is currently active.
        /// </summary>
        private bool cachedOwnerActive;

        // Makes the componet active or not.
        private bool enabled;

        #endregion

        #region Properties

        /// <summary>
        /// Identification number. Every game object has a unique ID.
        /// </summary>
        [XmlIgnore]
        public long Id { get; private set; }

        /// <summary>
        /// The game object this component is attached to.
        /// A component is always attached to one game object.
        /// </summary>
        [XmlIgnore]
        public GameObject Owner { get; internal set; }

        /// <summary>
        /// Makes the componet active or not.
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
        /// Indicates if the layer is active.
        /// I.e. if it is enable, the owner is active and its layer is active.
        /// </summary>
        [XmlIgnore]
        public bool IsActive { get { return Layer.IsActive(cachedLayerMask) && cachedOwnerActive && Enabled; } }

        /// <summary>
        /// Indicates if the layer is visible.
        /// I.e. if it is enable, the owner is active and its layer is visible (includes the current camera culling mask in the answer).
        /// </summary>
        [XmlIgnore]
        public bool IsVisible { get { return Layer.IsVisible(cachedLayerMask) && cachedOwnerActive && Enabled; } }

        #endregion

        #region Events

        #region Enable

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        public delegate void EnableEventHandler(object sender, bool enable);

        /// <summary>
        /// Raised when the game object's is enabled or disabled.
        /// </summary>
        public event EnableEventHandler EnabledChanged;

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new component.
        /// The system does not allow the direct creation of components.
        /// If you want to create a component use the AddComponent method of the gameobject class.
        /// </summary>
        internal Component()
        {
            // Create a unique ID
            Id = uniqueIdCounter;
            uniqueIdCounter++;
        } // Component

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal virtual void Initialize(GameObject owner)
        {
            Owner = owner;
            Enabled = true;
            // Set Owner's layer.
            cachedLayerMask = Owner.Layer.Mask;
            Owner.LayerChanged += OnLayerChanged;
            // Set Owner's active state.
            cachedOwnerActive = Owner.Active;
            Owner.ActiveChanged += OnActiveChanged;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal virtual void Uninitialize()
        {
            Owner.LayerChanged -= OnLayerChanged;
            Owner.ActiveChanged -= OnActiveChanged;
            Owner = null;
        } // Uninitialize

        #endregion

        #region On Layer Changed

        /// <summary>
        /// On game object's layer changed.
        /// </summary>
        private void OnLayerChanged(object sender, uint layerMask)
        {
            cachedLayerMask = layerMask;
        } // OnLayerChanged

        #endregion

        #region On Active Changed

        /// <summary>
        /// On game object's active changed.
        /// </summary>
        private void OnActiveChanged(object sender, bool active)
        {
            cachedOwnerActive = active;
        } // OnActiveChanged

        #endregion

    } // Component
} // XNAFinalEngine.Components
