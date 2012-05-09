
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
    public class PointLight : Light
    {

        #region Variables

        // Cached transfomr's position.
        internal Vector3 cachedPosition;

        // Light attenuation.
        private float range;

        // Light specular color.
        private Color specularColor;

        #endregion

        #region Properties

        /// <summary>
        /// The range of the light.
        /// </summary>
        /// <remarks>
        /// The light range has to be lower than the camera far plane.
        /// </remarks>
        public float Range
        {
            get { return range; }
            set
            {
                range = value;
                if (range < 0)
                    range = 0;
            }
        } // Range

        /// <summary>
        /// Light specular color.
        /// </summary>
        public Color SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        } // SpecularColor

        /// <summary>
        /// Associated shadow.
        /// </summary>
        public override Shadow Shadow
        {
            get { return shadow; }
            set
            {
                if (value is CascadedShadow)
                    throw new ArgumentException("Point Light: Cascaded shadows does not work with point lights.");
                if (value is BasicShadow)
                    throw new ArgumentException("Point Light: Basic shadows does not work with point lights.");
                shadow = value;
            }
        } // Shadow

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Values
            range = 1;
            specularColor = Color.White;
            cachedPosition = ((GameObject3D)Owner).Transform.Position;
        } // Initialize

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected override void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            cachedPosition = worldMatrix.Translation;
        } // OnWorldMatrixChanged

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<PointLight> componentPool = new Pool<PointLight>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<PointLight> ComponentPool { get { return componentPool; } }

        #endregion

    } // PointLight
} // XNAFinalEngine.Components
