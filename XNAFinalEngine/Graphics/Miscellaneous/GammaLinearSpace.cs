
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
using System;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Gamma and linear space converter.
    /// </summary>
    public static class GammaLinearSpaceHelper
    {

        /// <summary>
        /// Linear to gamma space.
        /// </summary>
        public static Color LinearToGamma(Vector4 color)
        {
            return new Color((float)Math.Sqrt(color.X), (float)Math.Sqrt(color.Y), (float)Math.Sqrt(color.Z), (float)Math.Sqrt(color.W));
        } // LinearToGamma

        /// <summary>
        /// Linear to gamma space.
        /// </summary>
        public static Color LinearToGamma(Vector3 color)
        {
            return new Color((float)Math.Sqrt(color.X), (float)Math.Sqrt(color.Y), (float)Math.Sqrt(color.Z));
        } // LinearToGamma

        /// <summary>
        /// Gamma to linear space.
        /// </summary>
        public static Vector4 GammaToLinear(Color color)
        {
            return new Vector4((color.R / 255f) * (color.R / 255f),
                               (color.G / 255f) * (color.G / 255f),
                               (color.B / 255f) * (color.B / 255f),
                               (color.A / 255f) * (color.A / 255f));
        } // GammaToLinear

    } // GammaLinearSpaceHelper
} // XNAFinalEngine.Graphics