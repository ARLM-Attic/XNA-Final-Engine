
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
using XNAFinalEngine.Graphics;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Point Light.
    /// </summary>
    public class AmbientLight : Light
    {

        #region Variables
        
        // Ambient Occlusion Strength.
        private static float ambientOcclusionStrength;

        #endregion

        #region Properties

        /// <summary>
        /// Spherical Harmonic Ambient Light.
        /// They are great for store low frequency ambient colors and are very fast.
        /// </summary>
        public static SphericalHarmonicL2 SphericalHarmonicAmbientLight { get; set; }

        /// <summary>
        /// Ambient Occlusion Effect.
        /// If null no ambient occlusion will be used.
        /// </summary>
        public static AmbientOcclusion AmbientOcclusion { get; set; }

        /// <summary>
        /// Ambient Occlusion Strength.
        /// </summary>
        public static float AmbientOcclusionStrength
        {
            get { return ambientOcclusionStrength; }
            set { ambientOcclusionStrength = value; }
        } // AmbientOcclusionStrength

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Redefine light's default values. Ambient lights behave different.
            DiffuseColor = new Color(20, 20, 20);
            Intensity = 0.1f;
            ambientOcclusionStrength = 5;
        } // Initialize

        #endregion

    } // AmbientLight
} // XNAFinalEngine.Components
