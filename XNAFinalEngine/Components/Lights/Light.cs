﻿
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Base class for lights.
    /// </summary>
    public abstract class Light : Component
    {

        #region Variables

        // Light diffuse color.
        private Color color;
        
        // The Intensity of a light is multiplied with the Light color.
        private float intensity;

        protected Shadow shadow;

        #endregion

        #region Properties

        /// <summary>
        /// Light color.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        } // Color

        /// <summary>
        /// The Intensity of a light is multiplied with the Light color.
        /// </summary>
        public float Intensity
        {
            get { return intensity; } 
            set { intensity = value; }
        } // Intensity

        /// <summary>
        /// Associated shadow.
        /// </summary>
        public virtual Shadow Shadow
        {
            get { return shadow;}
            set { shadow = value; }
        } // Shadow

        /// <summary>
        /// Shadow Texture.
        /// </summary>
        internal RenderTarget ShadowTexture { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Values
            intensity = 1;
            color = Color.White;
            // Transformation
            if (Owner is GameObject2D)
            {
                throw new InvalidOperationException("Lights does not work in 2D space.");
            }
            ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            Shadow = null;
            ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
            // Call this last because the owner information is needed.
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected abstract void OnWorldMatrixChanged(Matrix worldMatrix);

        #endregion
        
    } // Light
} // XNAFinalEngine.Components
