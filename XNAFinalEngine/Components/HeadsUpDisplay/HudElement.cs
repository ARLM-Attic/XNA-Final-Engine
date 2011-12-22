
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
using System;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Base class for Hud Elements.
    /// </summary>
    public abstract class HudElement : Renderer
    {

        #region Properties

        /// <summary>
        /// Chaded transform2D's world matrix value.
        /// </summary>
        internal Vector3 CachedPosition;

        /// <summary>
        /// Chaded transform2D's rotation value.
        /// </summary>
        internal float CachedRotation { get; set; }

        /// <summary>
        /// Chaded transform2D's scale value.
        /// </summary>
        internal float CachedScale { get; set; }

        /// <summary>
        /// Color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// If the element is in 3D space then it can be automatically rotated so it always faces the camera. Default value: false.
        /// </summary> 
        public bool Billboard { get; set; }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Default values
            Color = Color.White;
            Billboard = false;
        } // Initialize

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected override void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            cachedWorldMatrix = worldMatrix;
            // We could pass directly the calculated values. In this case there are calculated using the world matrix.
            if (Owner is GameObject2D)
            {
                // Decompose in position, rotation and scale.
                Quaternion quaternion;
                Vector3 scale;
                cachedWorldMatrix.Decompose(out scale, out quaternion, out CachedPosition);
                CachedScale = scale.X;
                // Quaternion to rotation angle.
                Vector2 direction = Vector2.Transform(Vector2.UnitX, quaternion);
                CachedRotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        } // OnWorldMatrixChanged

        #endregion

    } // HudElement
} // XNAFinalEngine.Components