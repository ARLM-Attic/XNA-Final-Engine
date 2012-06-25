
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Ray Marching Screen Space Ambient Occlusion.
    /// </summary>
    /// <remarks>
    /// Horizon Based is similar in performance but better in results.
    /// However this is a good example of how to make raymarching shaders, and the results are not bad either.
    /// 
    /// This shader is baded in a Shader X 7 article.
    /// </remarks>
    public class RayMarchingAmbientOcclusion : AmbientOcclusion
    {

        #region Variables

        // The count of assets for naming purposes.
        private static int nameNumber = 1;
        
        // Number of Steps.
        // It’s a sensitive performance parameter.
        private float numberSteps = 4.0f;

        // Number of Rays.
        // It’s a sensitive performance parameter.
        private float numberRays = 4.0f;

        // Number of Directions.
        // It’s a sensitive performance parameter.
        private float numberDirections = 6.0f;

        // Contrast.
        private float contrast = 1;

        // Line Attenuation.
        // The far samplers have a lower effect in the result. This controls how faster their weight decay.
        private float lineAttenuation = 1f;

        // Radius.
        // Bigger the radius more cache misses will occur. Be careful!!
        private float radius = 0.01f;

        #endregion

        #region Properties

        /// <summary>
        /// Number of Steps.
        /// It’s a sensitive performance parameter.
        /// </summary>
        public float NumberSteps
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
        /// Number of Rays.
        /// It’s a sensitive performance parameter.
        /// </summary>
        public float NumberRays
        {
            get { return numberRays; }
            set
            {
                numberRays = value;
                if (numberRays <= 0)
                    numberRays = 0;
            }
        } // NumberRays

        /// <summary>
        /// Number of Directions.
        /// It’s a sensitive performance parameter.
        /// </summary>
        public float NumberDirections
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
        /// The far samplers have a lower effect in the result. This controls how faster their weight decay.
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
        /// Bigger the radius more cache misses will occur. Be careful!!
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                if (radius <= 0)
                    radius = 0;
                if (radius > 1)
                    radius = 1;
            }
        } // Radius

        #endregion

        #region Constructor

        public RayMarchingAmbientOcclusion()
        {
            Name = "Ray Marching Ambient Occlusion-" + nameNumber;
            nameNumber++;
        } // RayMarchingAmbientOcclusion

        #endregion

    } // RayMarchingAmbientOcclusion
} // XNAFinalEngine.Assets
