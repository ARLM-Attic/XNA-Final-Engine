
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
    /// Adjust color levels, just like Photoshop. This adjusts the overall color levels. For R G B independent adjustments use AdjustLevelsIndividualChannels.
    /// By default this feature is disabled. Use the enabled property to enable and disable.
    /// If performance is an issue use lookup tables to accelerate color transformations, besides it allows more color transformations.
    ///
    /// How it works?
    /// 
    /// The Input Levels adjustment allows us to make three basic changes to improve overall image tone.
    /// We can brighten the highlights by setting a new white point (InputWhite),
    /// we can darken the shadows by setting a new black point (InputBlack),
    /// and we can lighten or darken the midtones in the image (InputGamma). 
    /// 
    /// The output levels compress (or clamp) the image's tonal range.
    /// By compressing the image's tonal range, we are forcing pixel tones to be closer together.
    /// This causes a reduction in contrast.
    ///
    /// Information about color transformation:
    /// http://http.developer.nvidia.com/GPUGems/gpugems_ch22.html
    /// http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter24.html
    /// </summary>
    public class AdjustLevels
    {

        #region Variables

        /// <summary>
        /// Darken the shadows by setting a new black point.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 0.9f
        /// </summary>
        private float inputBlack;

        /// <summary>
        /// Brighten the highlights by setting a new white point.
        /// Default value = 1
        /// Minimum value = 0.1f
        /// Maximum value = 1
        /// </summary>
        private float inputWhite = 1;

        /// <summary>
        /// Lighten or darken the midtones in the image.
        /// Default value = 1
        /// Minimum value = 9.99f
        /// Maximum value = 0.01f
        /// </summary>
        private float inputGamma = 1;

        /// <summary>
        /// Clamp the darker colors to this value.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        private float outputBlack;

        /// <summary>
        /// Clamp the brighten colors to this value.
        /// Default value = 1
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        private float outputWhite = 1;

        // Is it enabled?
        private bool enabled = true;

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
        /// Darken the shadows by setting a new black point.
        /// 0 is the default value.
        /// Minimum value = 0
        /// Maximum value = 0.9f
        /// </summary>
        public float InputBlack
        {
            get { return inputBlack; }
            set
            {
                inputBlack = value;
                if (inputBlack < 0)
                    inputBlack = 0;
                if (inputBlack > 0.9f)
                    inputBlack = 0.9f;
            }
        } // InputBlack

        /// <summary>
        /// Brighten the highlights by setting a new white point.
        /// Default value = 1
        /// Minimum value = 0.1f
        /// Maximum value = 1
        /// </summary>
        public float InputWhite
        {
            get { return inputWhite; }
            set
            {
                inputWhite = value;
                if (inputWhite < 0.1f)
                    inputWhite = 0.1f;
                if (inputWhite > 1)
                    inputWhite = 1;
            }
        } // InputWhite

        /// <summary>
        /// Lighten or darken the midtones in the image.
        /// Default value = 1
        /// Minimum value = 0.01f
        /// Maximum value = 9.99f
        /// </summary>
        public float InputGamma
        {
            get { return inputGamma; }
            set
            {
                inputGamma = 1 / value;
                if (inputGamma < 0.01f)
                    inputGamma = 0.01f;
                if (inputGamma > 9.99f)
                    inputGamma = 9.99f;
            }
        } // InputGamma

        /// <summary>
        /// Clamp the darker colors to this value.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputBlack
        {
            get { return outputBlack; }
            set
            {
                outputBlack = value;
                if (outputBlack < 0)
                    outputBlack = 0;
                if (outputBlack > 1)
                    outputBlack = 1;
            }
        } // OutputBlack

        /// <summary>
        /// Clamp the brighten colors to this value.
        /// Default value = 1
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputWhite
        {
            get { return outputWhite; }
            set
            {
                outputWhite = value;
                if (outputWhite < 0)
                    outputWhite = 0;
                if (outputWhite > 1)
                    outputWhite = 1;
            }
        } // OutputWhite

        #endregion

    } // AdjustLevels
} // XNAFinalEngine.Assets
