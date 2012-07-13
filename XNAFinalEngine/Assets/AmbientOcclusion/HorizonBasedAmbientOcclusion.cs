
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
    /// Horizon Based Screen Space Ambient Occlusion.
    /// </summary>
    /// <remarks>
    /// Important: I have to put constants in the shader's for sentences.
    /// If you want to change the number of steps and the number of directions will need to change the properties and the shader code.
    /// 
    /// This shader is baded in a Shader X 7 article with some minor modifications.
    /// </remarks>
    public class HorizonBasedAmbientOcclusion : AmbientOcclusion
    {

        #region Enumerates

        public enum QualityType
        {   
            LowQuality,
            MiddleQuality,
            HighQuality,
        } // QualityType

        #endregion

        #region Variables

        // The count of assets for naming purposes.
        private static int nameNumber = 1;

        // Number of Steps.
        // The far samplers have a lower effect in the result. This controls how faster their weight decay.
        private int numberSteps = 6;

        // Number of Directions.
        // The far samplers have a lower effect in the result. This controls how faster their weight decay.
        private int numberDirections = 10;

        // Contrast.
        private float contrast = 1f;

        // Line Attenuation.
        // The far samplers have a lower effect in the result. This controls how faster their weight decay.
        private float lineAttenuation = 1f;
        
        // Radius.
        // Bigger the radius more cache misses will occur. Be careful!!
        private float radius = 0.003f;

        // Angle Bias (grades).
        private float angleBias = 5f;

        // High quality is the best and the faster I think.
        private QualityType quality = QualityType.HighQuality;

        #endregion

        #region Properties
        
        /// <summary>
        /// Quality.
        /// </summary>
        public QualityType Quality
        {
            get { return quality; }
            set { quality = value; }
        } // Quality

        /// <summary>
        /// Number of Steps
        /// The far samplers have a lower effect in the result. This controls how faster their weight decay.
        /// </summary>
        public int NumberSteps
        {
            get { return numberSteps; }
            set
            {
                numberSteps = value;
                if (numberSteps <= 0)
                    numberSteps = 0;
            }
        } // NumberSteps

        /// <summary>
        /// Number of Directions
        /// The far samplers have a lower effect in the result. This controls how faster their weight decay.
        /// </summary>
        public int NumberDirections
        {
            get { return numberDirections; }
            set
            {
                numberDirections = value;
                if (numberDirections <= 0)
                    numberDirections = 0;
            }
        } // NumberDirections

        /// <summary>
        /// Contrast.
        /// </summary>
        public float Contrast
        {
            get { return contrast; }
            set
            {
                contrast = value;
                if (contrast <= 0)
                    contrast = 0;
            }
        } // Contrast

        /// <summary>
        /// Line Attenuation.
        /// </summary>
        public float LineAttenuation
        {
            get { return lineAttenuation; }
            set
            {
                lineAttenuation = value;
                if (lineAttenuation <= 0)
                    lineAttenuation = 0;
            }
        } // Line Attenuation

        /// <summary>
        /// Radius.
        /// </summary>
        /// <remarks>
        /// Higher the radius more cache misses will occur. Be careful!!
        /// </remarks>
        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                if (radius <= 0)
                    radius = 0;
                if (radius > 0.5f)
                    radius = 0.5f;
            }
        } // Radius

        /// <summary>
        /// Angle Bias (degrees)
        /// </summary>
        public float AngleBias
        {
            get { return angleBias; }
            set
            {
                angleBias = value;
                if (angleBias <= 0)
                    angleBias = 0;
                if (angleBias > 90)
                    angleBias = 90;
            }
        } // AngleBias

        #endregion

        #region Constructor

        public HorizonBasedAmbientOcclusion()
        {
            Name = "Horizon Based Ambient Occlusion-" + nameNumber;
            nameNumber++;
        } // HorizonBasedAmbientOcclusion

        #endregion

    } // HorizonBasedAmbientOcclusion
} // XNAFinalEngine.Assets
