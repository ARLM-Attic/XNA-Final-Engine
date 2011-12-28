
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

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// An ambient light source represents the light that is not directly influenced by local light sources (point, directional and spot lights).
    /// This includes spherical harmonic lighting, ambient occlusion and
    /// clear color (a fixed-intensity and fixed-color light source that affects all objects in the scene equally).
    /// </summary>
    public class AmbientLight : Asset
    {
        
        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;

        // Light diffuse color.
        private Color color = new Color(20, 20, 20);

        // The Intensity of a light is multiplied with the Light color.
        private float intensity = 0.1f;
        
        // Ambient Occlusion Strength.
        private static float ambientOcclusionStrength = 5;

        #endregion

        #region Properties

        /// <summary>
        /// Light diffuse color.
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
            set
            {
                if (intensity > 0)
                    intensity = value;
            }
        } // Intensity

        /// <summary>
        /// Spherical Harmonic Ambient Light.
        /// </summary>
        /// <remarks>
        /// They are great for store low frequency ambient colors and are very fast.
        /// You can generate a spherical harmonic lighting from a cubemap texture (either RGBM or RGB format).
        /// </remarks>
        public SphericalHarmonicL2 SphericalHarmonicLighting { get; set; }

        /// <summary>
        /// Ambient Occlusion Effect.
        /// If null no ambient occlusion will be used.
        /// </summary>
        public AmbientOcclusion AmbientOcclusion { get; set; }

        /// <summary>
        /// Ambient Occlusion Strength.
        /// </summary>
        public float AmbientOcclusionStrength
        {
            get { return ambientOcclusionStrength; }
            set { ambientOcclusionStrength = value; }
        } // AmbientOcclusionStrength

        #endregion

        #region Constructor

        public AmbientLight()
        {
            Name = "Post Process-" + nameNumber;
            nameNumber++;
            SphericalHarmonicLighting = new SphericalHarmonicL2();
            SphericalHarmonicLighting.Fill(0.5f, 0.5f, 0.5f);
        } // AmbientLight

        #endregion

    } // AmbientLight
} // XNAFinalEngine.Assets
