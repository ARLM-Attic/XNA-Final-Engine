
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
    /// Tone Mapping.
    /// Map one set of colors to another in order to approximate the appearance of high dynamic range images in a medium that has a more limited dynamic range.
    /// </summary>
    /// <remarks>
    /// References:
    ///   http://mynameismjp.wordpress.com/2010/04/30/a-closer-look-at-tone-mapping/
    ///   http://content.gpwiki.org/index.php/D3DBook:High-Dynamic_Range_Rendering
    ///   http://filmicgames.com/archives/75
    /// </remarks>
    public class ToneMapping
    {

        #region Variables
        
        // Is auto exposure enabled?
        private bool autoExposureEnabled = true;

        // Lens exposure.
        private float lensExposure = 1;
      
        #endregion

        #region Properties

        /// <summary>
        /// Lens exposure.
        /// </summary>
        public float LensExposure
        {
            get { return lensExposure; }
            set
            {
                lensExposure = value;
                if (lensExposure < 0)
                    lensExposure = 0;
            }
        } // LensExposure

        /// <summary>
        /// Is auto exposure enabled?
        /// </summary>
        public bool AutoExposureEnabled
        {
            get { return autoExposureEnabled; }
            set { autoExposureEnabled = value; }
        } // AutoExposureEnabled

        /// <summary>
        /// Luminance texture. Used in the adaptation pass.
        /// </summary>
        internal RenderTarget LuminanceTexture { get; set; }

        #endregion

    } // ToneMapping
} // XNAFinalEngine.Assets