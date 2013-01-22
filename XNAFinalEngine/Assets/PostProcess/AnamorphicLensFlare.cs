
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
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Anamorphic Lens Flare.
    /// Lens flare is a photographic artefact caused by various physical interactions between a lens and the light passing through it.
    /// </summary>
    public class AnamorphicLensFlare
    {

        #region Variables
        
        // Default values.
        private bool enabled;
        private float dispersal = 0.1875f;
        private float haloWidth = 0.45f;
        private float intensity = 1.5f;
        private Vector3 distortion = new Vector3(0.94f, 0.97f, 1.00f);

        #endregion

        #region Properties

        /// <summary>
        /// Is it enabled?
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        } // Enabled

        /// <summary>
        /// This control the spreading the light source out along a line through the image centre with a ghost effect.  
        /// </summary>
        public float Dispersal
        {
            get { return dispersal; } 
            set { dispersal = value; }
        } // Dispersal

        /// <summary>
        /// Controls the size of the halo on the center of the screen.
        /// </summary>
        public float HaloWidth
        {
            get { return haloWidth; } 
            set
            {
                haloWidth = value;
                if (haloWidth < 0)
                    haloWidth = 0;
            }
        } // HaloWidth

        /// <summary>
        /// Control the intensity of the lens flare.
        /// </summary>
        public float Intensity
        {
            get { return intensity; }
            set
            {
                intensity = value;
                if (intensity < 0)
                    intensity = 0;
            }
        } // Intensity

        /// <summary>
        /// Control the chromatic distortion effect.
        /// </summary>
        public Vector3 ChromaticDistortion
        {
            get { return distortion; }
            set { distortion = value; }
        } // ChromaticDistortion

        /// <summary>
        /// Dirt Texture.
        /// </summary>
        public Texture DirtTexture { get; set; }

        #endregion

    } // AnamorphicLensFlare
} // XNAFinalEngine.Assets