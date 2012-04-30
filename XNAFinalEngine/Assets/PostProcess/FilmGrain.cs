
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

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Film grain.
    /// In movies film grain or granularity is the random optical texture of processed photographic film due to the presence of
    /// small particles of a metallic silver, or dye clouds, developed from silver halide that have received enough photons.
    /// While film grain is a function of such particles (or dye clouds) it is not the same thing as such.
    /// It is an optical effect, the magnitude of which (amount of grain) depends on both the film stock and the definition at which it is observed.
    /// 
    /// Games with film grain: http://www.giantbomb.com/film-grain/92-487/
    /// </summary>
    public class FilmGrain
    {

        #region Variables

        // Fil grain strength.
        private float filmgrainStrength = 0.2f;

        // The film grain effect is a phenomenon mostly seen in analog films that is more notorious in dark areas.
        // This value accentuates the noise in the dark values.
        private float accentuateDarkNoisePower = 2;

        // The noise is both, random and static. With this we can accentuate or reduce the random noise.
        // 1 is half random and half static, 0 is only static, and more than 1 accentuate the random noise.
        private float randomNoiseStrength = 0.5f;

        // Is it enabled?
        private bool enabled = false;

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
        /// Fil grain strength.
        /// 0 = no effect
        /// 1 = full effect
        /// </summary>
        public float Strength
        {
            get { return filmgrainStrength; }
            set
            {
                filmgrainStrength = value;
                if (filmgrainStrength < 0)
                    filmgrainStrength = 0;
                if (filmgrainStrength > 1)
                    filmgrainStrength = 1;
            }
        } // Strength
        
        /// <summary>
        /// The film grain effect is a phenomenon mostly seen in analog films that is more notorious in dark areas.
        /// This value accentuates the noise in the dark values.
        /// Use values greater than 1.
        /// </summary>
        public float AccentuateDarkNoisePower
        {
            get { return accentuateDarkNoisePower; }
            set
            {
                accentuateDarkNoisePower = value;
                if (accentuateDarkNoisePower < 1)
                    accentuateDarkNoisePower = 1;
                if (accentuateDarkNoisePower > 10)
                    accentuateDarkNoisePower = 10;
            }
        } // AccentuateDarkNoisePower

        /// <summary>
        /// The noise is both, random and static. With this we can accentuate or reduce the random noise.
        /// 1 is half random and half static, 0 is only static, and more than 1 accentuate the random noise.
        /// </summary>
        public float RandomNoiseStrength
        {
            get { return randomNoiseStrength; }
            set
            {
                randomNoiseStrength = value;
                if (randomNoiseStrength < 0)
                    randomNoiseStrength = 0;
                if (randomNoiseStrength > 5)
                    randomNoiseStrength = 5;
            }
        } // RandomNoiseStrength

        #endregion

    } // FilmGrain
} // XNAFinalEngine.Assets
