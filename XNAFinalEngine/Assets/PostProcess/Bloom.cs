
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

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Bloom.
    /// The effect produces fringes (or feathers) of light around very bright objects in an image.
    /// Basically, if an object has a light behind it, the light will look more realistic and will somewhat overlap the front of the object from the 3rd person perspective. 
    /// http://en.wikipedia.org/wiki/Bloom_(shader_effect)
    /// </summary>
    public class Bloom
    {

        #region Variables
        
        // Bloom Threshold.
        private float threshold = 3f;
        
        // Is it enabled?
        private bool enabled = true;
        
        // Bloom Scale.
        private float scale = 1f;

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
        /// Bloom Scale.
        /// </summary>
        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                if (scale < 0)
                    scale = 0;
            }
        } // BloomScale

        /// <summary>
        /// Bloom Threshold.
        /// </summary>
        public float Threshold
        {
            get { return threshold; } 
            set
            {
                threshold = value;
                if (threshold < 0)
                    threshold = 0;
            }
        } // Threshold

        #endregion

    } // Bloom
} // XNAFinalEngine.Assets