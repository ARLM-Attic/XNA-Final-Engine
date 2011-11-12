
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

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Spherical Harmonics for RGB colors.
    /// A spherical harmonic approximates a spherical function using only a few values.
    /// They are great for store low frequency ambient colors and are very fast.
    /// </summary>
    public abstract class SphericalHarmonic
    {

        #region Variables

        /// <summary>
        /// Accumulated weighting value. This value is provided as a helper to make averaging easier. It stores accumulated weighting values from calls to AddLight.
        /// See AddLight method for further details.
        /// </summary>
        protected float weighting;

        #endregion
        
        #region Fill constants

        /// <summary>
        /// Fill the constants with a color.
        /// </summary>
        public virtual void Fill(float red, float green, float blue)
        {
            // Overrite it!!
        } // Fill

        #endregion

        #region Add Light

        /// <summary>
        /// Add light from a given direction to the SH function. See the class remarks for further details on how an SH can be used to approximate lighting.
        /// Input light inputRGB will be multiplied by weight, and weight will be added to weighting.
        /// </summary>
        /// <param name="inputRgb">Input light intensity in RGB (usually gamma space) format for the given direction</param>
        /// <param name="direction">direction of the incoming light</param>
        /// <param name="weight">Weighting for this light, usually 1.0f. Use this value, and weighting if averaging a large number of lighting samples (eg, when converting a cube map to an SH)</param>
        public virtual void AddLight(Vector3 inputRgb, Vector3 direction, float weight = 1.0f)
        {
            // Overrite it!!
        } // AddLight

        #endregion

        #region Sample Direction

        /// <summary>
        /// Sample the spherical harmonic in the given direction.
        /// </summary>
        public virtual Vector3 SampleDirection(Vector3 direction)
        {
            // Overrite it!!
            return Vector3.Zero;
        } // SampleDirection

        #endregion
        
    } // SphericalHarmonic
} // XNAFinalEngine.Graphics
