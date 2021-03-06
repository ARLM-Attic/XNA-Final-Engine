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
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Point Light.
    /// </summary>
    public class SpotLight : Light
    {

        #region Variables

        // Cached transfomr's position.
        internal Vector3 cachedPosition;

        // Cached game object's world matrix.
        internal Matrix cachedWorldMatrix;

        // Cached transform's direction.
        internal Vector3 cachedDirection;

        // Default values.
        private float range;
        private float innerConeAngle;
        private float outerConeAngle;

        #endregion

        #region Properties

        /// <summary>
        /// The inner cone angle (in degrees) that controls the spread attenuation.
        /// </summary>
        public float InnerConeAngle
        {
            get { return innerConeAngle; }
            set
            {
                innerConeAngle = value;
                if (innerConeAngle < 0)
                    innerConeAngle = 0;
                if (innerConeAngle >= 175)
                    innerConeAngle = 175;
                if (innerConeAngle > outerConeAngle)
                    innerConeAngle = outerConeAngle;
            }
        } // InnerConeAngle

        /// <summary>
        /// The outer cone angle (in degrees) that controls the spread attenuation.
        /// </summary>
        public float OuterConeAngle
        {
            get { return outerConeAngle; }
            set
            {
                outerConeAngle = value;
                if (outerConeAngle < 0)
                    outerConeAngle = 0;
                if (outerConeAngle >= 175)
                    outerConeAngle = 175;
                if (outerConeAngle < innerConeAngle)
                    outerConeAngle = innerConeAngle;
            }
        } // OuterConeAngle

        /// <summary>
        /// The range of the light.
        /// </summary>
        public float Range
        {
            get { return range; }
            set
            {
                range = value;
                if (range <= 0.1f)
                    range = 0.1f;
            }
        } // Range

        /// <summary>
        /// This texture will be projected from the light onto the scene, acting like a mask that occludes the emitted light.
        /// </summary>
        public Texture LightMaskTexture { get; set; }

        /// <summary>
        /// Associated shadow.
        /// </summary>
        public override Shadow Shadow
        {
            get { return shadow; }
            set
            {
                if (value is CascadedShadow)
                    throw new ArgumentException("Spot Light: Cascaded shadows does not work with spot lights.");
                shadow = value;
            }
        } // Shadow

        /// <summary>
        /// Clip volumes restricts the area of influence of the light.
        /// </summary>
        public Model ClipVolume { get; set; }

        /// <summary>
        /// Ignores the world matrix of the light rendering the model on the world's origin.
        /// </summary>
        public bool RenderClipVolumeInLocalSpace { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Values
            range = 100;
            innerConeAngle = 20;
            outerConeAngle = 60;
            RenderClipVolumeInLocalSpace = false;
            cachedPosition = ((GameObject3D)Owner).Transform.Position;
            cachedDirection = ((GameObject3D)Owner).Transform.Forward;
            cachedWorldMatrix = ((GameObject3D)Owner).Transform.WorldMatrix;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            LightMaskTexture = null;
            ClipVolume = null;
            // Call this last because the owner information is needed.
            base.Uninitialize();
        } // Uninitialize

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected override void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            cachedDirection = worldMatrix.Forward;
            cachedPosition = worldMatrix.Translation;
            cachedWorldMatrix = worldMatrix;
        } // OnWorldMatrixChanged

        #endregion
        
        #region Pool

        // Pool for this type of components.
        private static readonly Pool<SpotLight> componentPool = new Pool<SpotLight>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<SpotLight> ComponentPool { get { return componentPool; } }

        #endregion

    } // SpotLight
} // XNAFinalEngine.Components
