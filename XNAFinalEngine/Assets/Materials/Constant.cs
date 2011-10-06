
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

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Constant Material.
    /// </summary>
    public class Constant : Material
    {

        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;

        // Surface diffuse color. If a diffuse texture exists this color will be ignored.
        private Color diffuseColor = Color.Gray;

        // Alpha Blending.
        private float alphaBlending = 1.0f;

        #endregion

        #region Properties

        #region Diffuse

        /// <summary>
        /// Surface diffuse color. If a diffuse texture exists this color will be ignored.
        /// </summary>
        public Color DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        } // DiffuseColor

        /// <summary>
        /// Diffuse texture. If it's null then the DiffuseColor value will be used.
        /// </summary>
        public Texture DiffuseTexture { get; set; }

        #endregion

        #region Transparency

        /// <summary>
        /// Alpha Blending.
        /// Default value: 1
        /// </summary>
        public float AlphaBlending
        {
            get { return alphaBlending; }
            set { alphaBlending = value; }
        } // AlphaBlending

        /// <summary>
        /// Render both sides.
        /// I.e. it manages a the culling mode.
        /// Default value: false (CullCounterClockwise)
        /// </summary>
        public bool BothSides { get; set; }

        #endregion

        #endregion
        
        #region Constructor

        /// <summary>
		/// Constant material.
		/// </summary>
        public Constant()
		{
            Name = "Constant-" + nameNumber;
            nameNumber++;
        } // Constant

		#endregion
        
    } // Constant
} // XNAFinalEngine.Assets

