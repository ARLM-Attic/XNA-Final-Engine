﻿
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
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Base class for renderers.
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public abstract class Renderer : Component
    {

        #region Variables

        /// <summary>
        /// Chaded transform's world matrix value.
        /// </summary>
        internal Matrix cachedWorldMatrix;

        /// <summary>
        /// Chaded game object's layer mask value.
        /// </summary>
        internal int cachedLayerMask;

        #endregion

        #region Properties

        /// <summary>
        /// Makes the game object visible or not.
        /// </summary>
        public bool Visible { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            Visible = true;
            // Set Layer
            cachedLayerMask = Owner.Layer.Mask;
            Owner.LayerChanged += OnLayerChanged;
            // Set World Matrix
            if (Owner is GameObject2D)
            {
                cachedWorldMatrix = ((GameObject2D) Owner).Transform.WorldMatrix;
                ((GameObject2D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            }
            else
            {
                cachedWorldMatrix = ((GameObject3D)Owner).Transform.WorldMatrix;
                ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            }
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
            Owner.LayerChanged -= OnLayerChanged;
            if (Owner is GameObject2D)
            {
                ((GameObject2D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
            }
            else
            {
                ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
            }
        } // Uninitialize

        #endregion

        #region On Layer Changed

        /// <summary>
        /// On game object's layer changed.
        /// </summary>
        private void OnLayerChanged(object sender, int layerMask)
        {
            cachedLayerMask = layerMask;
        } // OnLayerChanged

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected virtual void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            cachedWorldMatrix = worldMatrix;
        } // OnWorldMatrixChanged

        #endregion

    } // Renderer
} // XNAFinalEngine.Components