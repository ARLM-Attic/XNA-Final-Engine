
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
	/// Skybox.
	/// </summary>
    public class Skybox : Sky
	{

        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;

        // Alpha blending
        private float alphaBlending = 1.0f;

        // Color intensity.
        private float colorIntensity = 1.0f;

        #endregion

        #region Properties

        /// <summary>
        /// Cube map texture.
        /// </summary>
        public TextureCube TextureCube { get; set; }

        /// <summary>
        /// Alpha blending.
        /// </summary>
        public float AlphaBlending
        {
            get { return alphaBlending; }
            set
            {
                alphaBlending = value;
                if (alphaBlending < 0)
                    alphaBlending = 0;
                if (alphaBlending > 1)
                    alphaBlending = 1;
            }
        } // AlphaBlending

        /// <summary>
        /// Color Intensity.
        /// </summary>
        public float ColorIntensity
        {
            get { return colorIntensity; }
            set
            {
                colorIntensity = value;
                if (colorIntensity < 0)
                    colorIntensity = 0;
            }
        } // ColorIntensity

        #endregion

        #region Constructor

        public Skybox()
        {
            Name = "Skybox-" + nameNumber;
            nameNumber++;
        } // Skybox

        #endregion

    } // Skybox
} // XNAFinalEngine.Assets
