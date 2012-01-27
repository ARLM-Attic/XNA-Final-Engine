
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
    /// Morphological Antialiasing (MLAA).
    /// </summary>
    public class MLAA
    {

        #region Enumerators

        public enum EdgeDetectionType
        {
            Both,
            Color,
            Depth
        } // EdgeDetectionType

        #endregion

        #region Variables

        // Enabled?
        private bool enabled = true;

        // Threshold Color.
        private float thresholdColor = 0.2f;

        // Threshold Depth.
        private float thresholdDepth = 0.2f;

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
        /// Edge Detection.
        /// Color: uses the color information. Good for texture and geometry aliasing.
        /// Depth: uses the depth buffer. Great for geometry aliasing.
        /// Both: the two at the same time. A little more costly with slightly better results.
        /// Normals: not implemented.
        /// </summary>
        public EdgeDetectionType EdgeDetection { get; set; } 

        /// <summary>
        /// Threshold Color.
        /// </summary>
        public float ThresholdColor
        {
            get { return thresholdColor; }
            set
            {
                thresholdColor = value;
                if (thresholdColor < 0)
                    thresholdColor = 0;
                if (thresholdColor > 1)
                    thresholdColor = 1;
            }
        } // ThresholdColor

        /// <summary>
        /// Threshold Depth.
        /// </summary>
        public float ThresholdDepth
        {
            get { return thresholdDepth; }
            set
            {
                thresholdDepth = value;
                if (thresholdDepth < 0)
                    thresholdDepth = 0;
                if (thresholdDepth > 1)
                    thresholdDepth = 1;
            }
        } // ThresholdDepth

        #endregion
                  
    } // MLAA
} // XNAFinalEngine.Assets
