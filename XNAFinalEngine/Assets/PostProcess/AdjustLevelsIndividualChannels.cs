
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

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Adjust color levels, just like Photoshop. This adjusts each channel individually.
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
    public class AdjustLevelsIndividualChannels
    {

        #region Variables

        /// <summary>
        /// Darken the shadows by setting a new black point.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 0.9f
        /// </summary>
        private Vector3 inputBlack = Vector3.Zero;

        /// <summary>
        /// Brighten the highlights by setting a new white point.
        /// Default value = 1
        /// Minimum value = 0.1f
        /// Maximum value = 1
        /// </summary>
        private Vector3 inputWhite = new Vector3(1, 1, 1);

        /// <summary>
        /// Lighten or darken the midtones in the image.
        /// Default value = 1
        /// Minimum value = 9.99f
        /// Maximum value = 0.01f
        /// </summary>
        private Vector3 inputGamma = new Vector3(1, 1, 1);

        /// <summary>
        /// Clamp the darker colors to this value.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        private Vector3 outputBlack = Vector3.Zero;

        /// <summary>
        /// Clamp the brighten colors to this value.
        /// Default value = 1
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        private Vector3 outputWhite = new Vector3(1, 1, 1);

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

        #region All channels

        /// <summary>
        /// Darken the shadows by setting a new black point.
        /// 0 is the default value.
        /// Minimum value = 0
        /// Maximum value = 0.9f
        /// </summary>
        public Vector3 InputBlack
        {
            get { return inputBlack; }
            set
            {
                inputBlack = value;
                CheckRanges();
            }
        } // InputBlack

        /// <summary>
        /// Brighten the highlights by setting a new white point.
        /// Default value = 1
        /// Minimum value = 0.1f
        /// Maximum value = 1
        /// </summary>
        public Vector3 InputWhite
        {
            get { return inputWhite; }
            set
            {
                inputWhite = value;
                CheckRanges();
            }
        } // InputWhite

        /// <summary>
        /// Lighten or darken the midtones in the image.
        /// Default value = 1
        /// Minimum value = 0.01f
        /// Maximum value = 9.99f
        /// </summary>
        public Vector3 InputGamma
        {
            get { return inputGamma; }
            set
            {
                inputGamma = new Vector3(1 / value.X, 1 / value.Y, 1 / value.Z);
                CheckRanges();
            }
        } // InputGamma

        /// <summary>
        /// Clamp the darker colors to this value.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public Vector3 OutputBlack
        {
            get { return outputBlack; }
            set
            {
                outputBlack = value;
                CheckRanges();
            }
        } // OutputBlack

        /// <summary>
        /// Clamp the brighten colors to this value.
        /// Default value = 1
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public Vector3 OutputWhite
        {
            get { return outputWhite; }
            set
            {
                outputWhite = value;
                CheckRanges();
            }
        } // OutputWhite

        #endregion

        #region Red Channel

        /// <summary>
        /// Darken the shadows by setting a new black point.
        /// 0 is the default value.
        /// Minimum value = 0
        /// Maximum value = 0.9f
        /// </summary>
        public float InputBlackRedChannel
        {
            get { return inputBlack.X; }
            set
            {
                inputBlack.X = value;
                CheckRanges();
            }
        } // InputBlackRedChannel

        /// <summary>
        /// Brighten the highlights by setting a new white point.
        /// Default value = 1
        /// Minimum value = 0.1f
        /// Maximum value = 1
        /// </summary>
        public float InputWhiteRedChannel
        {
            get { return inputWhite.X; }
            set
            {
                inputWhite.X = value;
                CheckRanges();
            }
        } // InputWhiteRedChannel

        /// <summary>
        /// Lighten or darken the midtones in the image.
        /// Default value = 1
        /// Minimum value = 0.01f
        /// Maximum value = 9.99f
        /// </summary>
        public float InputGammaRedChannel
        {
            get { return inputGamma.X; }
            set
            {
                inputGamma.X = value;
                CheckRanges();
            }
        } // InputGammaRedChannel

        /// <summary>
        /// Clamp the darker colors to this value.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputBlackRedChannel
        {
            get { return outputBlack.X; }
            set
            {
                outputBlack.X = value;
                CheckRanges();
            }
        } // OutputBlackRedChannel

        /// <summary>
        /// Clamp the brighten colors to this value.
        /// Default value = 1
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputWhiteRedChannel
        {
            get { return outputWhite.X; }
            set
            {
                outputWhite.X = value;
                CheckRanges();
            }
        } // OutputWhiteRedChannel

        #endregion

        #region Green Channel

        /// <summary>
        /// Darken the shadows by setting a new black point.
        /// 0 is the default value.
        /// Minimum value = 0
        /// Maximum value = 0.9f
        /// </summary>
        public float InputBlackGreenChannel
        {
            get { return inputBlack.Y; }
            set
            {
                inputBlack.Y = value;
                CheckRanges();
            }
        } // InputBlackGreenChannel

        /// <summary>
        /// Brighten the highlights by setting a new white point.
        /// Default value = 1
        /// Minimum value = 0.1f
        /// Maximum value = 1
        /// </summary>
        public float InputWhiteGreenChannel
        {
            get { return inputWhite.Y; }
            set
            {
                inputWhite.Y = value;
                CheckRanges();
            }
        } // InputWhiteGreenChannel

        /// <summary>
        /// Lighten or darken the midtones in the image.
        /// Default value = 1
        /// Minimum value = 0.01f
        /// Maximum value = 9.99f
        /// </summary>
        public float InputGammaGreenChannel
        {
            get { return inputGamma.Y; }
            set
            {
                inputGamma.Y = value;
                CheckRanges();
            }
        } // InputGammaGreenChannel

        /// <summary>
        /// Clamp the darker colors to this value.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputBlackGreenChannel
        {
            get { return outputBlack.Y; }
            set
            {
                outputBlack.Y = value;
                CheckRanges();
            }
        } // OutputBlackGreenChannel

        /// <summary>
        /// Clamp the brighten colors to this value.
        /// Default value = 1
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputWhiteGreenChannel
        {
            get { return outputWhite.Y; }
            set
            {
                outputWhite.Y = value;
                CheckRanges();
            }
        } // OutputWhiteGreenChannel

        #endregion

        #region Blue Channel

        /// <summary>
        /// Darken the shadows by setting a new black point.
        /// 0 is the default value.
        /// Minimum value = 0
        /// Maximum value = 0.9f
        /// </summary>
        public float InputBlackBlueChannel
        {
            get { return inputBlack.Z; }
            set
            {
                inputBlack.Z = value;
                CheckRanges();
            }
        } // InputBlackBlueChannel

        /// <summary>
        /// Brighten the highlights by setting a new white point.
        /// Default value = 1
        /// Minimum value = 0.1f
        /// Maximum value = 1
        /// </summary>
        public float InputWhiteBlueChannel
        {
            get { return inputWhite.Z; }
            set
            {
                inputWhite.Z = value;
                CheckRanges();
            }
        } // InputWhiteBlueChannel

        /// <summary>
        /// Lighten or darken the midtones in the image.
        /// Default value = 1
        /// Minimum value = 0.01f
        /// Maximum value = 9.99f
        /// </summary>
        public float InputGammaBlueChannel
        {
            get { return inputGamma.Z; }
            set
            {
                inputGamma.Z = value;
                CheckRanges();
            }
        } // InputGammaBlueChannel

        /// <summary>
        /// Clamp the darker colors to this value.
        /// Default value = 0
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputBlackBlueChannel
        {
            get { return outputBlack.Z; }
            set
            {
                outputBlack.Z = value;
                CheckRanges();
            }
        } // OutputBlackBlueChannel

        /// <summary>
        /// Clamp the brighten colors to this value.
        /// Default value = 1
        /// Minimum value = 0
        /// Maximum value = 1
        /// </summary>
        public float OutputWhiteBlueChannel
        {
            get { return outputWhite.Z; }
            set
            {
                outputWhite.Z = value;
                CheckRanges();
            }
        } // OutputWhiteBlueChannel

        #endregion

        #endregion

        #region Check Ranges

        /// <summary>
        /// Check ranges and saturate incorrect values.
        /// </summary>
        private void CheckRanges()
        {

            #region Input Black

            if (inputBlack.X < 0)
                inputBlack.X = 0;
            if (inputBlack.X > 0.9f)
                inputBlack.X = 0.9f;
            if (inputBlack.Y < 0)
                inputBlack.Y = 0;
            if (inputBlack.Y > 0.9f)
                inputBlack.Y = 0.9f;
            if (inputBlack.Z < 0)
                inputBlack.Z = 0;
            if (inputBlack.Z > 0.9f)
                inputBlack.Z = 0.9f;

            #endregion

            #region Input White

            if (inputWhite.X < 0.1f)
                inputWhite.X = 0.1f;
            if (inputWhite.X > 1)
                inputWhite.X = 1;
            if (inputWhite.Y < 0.1f)
                inputWhite.Y = 0.1f;
            if (inputWhite.Y > 1)
                inputWhite.Y = 1;
            if (inputWhite.Z < 0.1f)
                inputWhite.Z = 0.1f;
            if (inputWhite.Z > 1)
                inputWhite.Z = 1;

            #endregion

            #region Input Gamma

            if (inputGamma.X < 0.01f)
                inputGamma.X = 0.01f;
            if (inputGamma.X > 9.99f)
                inputGamma.X = 9.99f;
            if (inputGamma.Y < 0.01f)
                inputGamma.Y = 0.01f;
            if (inputGamma.Y > 9.99f)
                inputGamma.Y = 9.99f;
            if (inputGamma.Z < 0.01f)
                inputGamma.Z = 0.01f;
            if (inputGamma.Z > 9.99f)
                inputGamma.Z = 9.99f;

            #endregion

            #region Output Black

            if (outputBlack.X < 0)
                outputBlack.X = 0;
            if (outputBlack.X > 1)
                outputBlack.X = 1;
            if (outputBlack.Y < 0)
                outputBlack.Y = 0;
            if (outputBlack.Y > 1)
                outputBlack.Y = 1;
            if (outputBlack.Z < 0)
                outputBlack.Z = 0;
            if (outputBlack.Z > 1)
                outputBlack.Z = 1;

            #endregion

            #region Output White

            if (outputWhite.X < 0)
                outputWhite.X = 0;
            if (outputWhite.X > 1)
                outputWhite.X = 1;
            if (outputWhite.Y < 0)
                outputWhite.Y = 0;
            if (outputWhite.Y > 1)
                outputWhite.Y = 1;
            if (outputWhite.Z < 0)
                outputWhite.Z = 0;
            if (outputWhite.Z > 1)
                outputWhite.Z = 1;

            #endregion

        } // CheckRanges

        #endregion

    } // AdjustLevelsIndividualChannels
} // XNAFinalEngine.Assets
