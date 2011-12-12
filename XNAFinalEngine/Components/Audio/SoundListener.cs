
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Sound Listener.
    /// </summary>
    public class SoundListener : Component
    {
        
        #region Variables

        /// <summary>
        /// This is the cached world matrix from the transform component.
        /// This matrix represents the view matrix.
        /// </summary>
        internal Matrix cachedWorldMatrix;

        // XNA audio emitter, used for 3D sounds.
        private readonly AudioListener audioListener = new AudioListener();

        // For velocity calculations.
        private Vector3 oldPosition;

        #endregion

        #region Properties

        /// <summary>
        /// Enabled.
        /// </summary>
        public bool Enabled { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Default values.
            Enabled = true;
            // Cache transform matrix. It will be the view matrix.
            cachedWorldMatrix = ((GameObject3D)Owner).Transform.WorldMatrix;
            ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            oldPosition = cachedWorldMatrix.Translation;
        } // Initialize
        
        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            base.Uninitialize();
            ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
        } // Uninitialize

        #endregion

        #region Update Sound Effect Instance

        /// <summary>
        /// Update listener properties.
        /// </summary>
        internal void UpdateListenerProperties()
        {
            audioListener.Forward = cachedWorldMatrix.Forward;
            audioListener.Up = cachedWorldMatrix.Up;
            audioListener.Position = cachedWorldMatrix.Translation;
            audioListener.Velocity = (audioListener.Position - oldPosition) / Time.SmoothFrameTime; // Distance / Time
            oldPosition = audioListener.Position;
        } // UpdateListenerProperties

        #endregion
        
        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected virtual void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            // The view matrix is the invert
            cachedWorldMatrix = Matrix.Invert(worldMatrix);            
        } // OnWorldMatrixChanged

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<SoundListener> componentPool = new Pool<SoundListener>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<SoundListener> ComponentPool { get { return componentPool; } }

        #endregion

    } // SoundListener
} // XNAFinalEngine.Components