
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

#region Using directives
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Car paint material.
    /// 
    /// This material includes some features to imitate the look of tricked-out car paint.
    /// Some car paint appears to change color based on your viewing angle.
    /// The paint looks like one color when you're looking straight at the surface and another color when 
    /// the surface is parallel to your view. This shader imitates that effect.
    /// </summary>
    public class CarPaint : Material
    {

        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;

        // Base paint color.
        private Color basePaintColor = new Color(0.3f,  0.3f,  0.4f);

        // Second base paint color.
        private Color secondBasePaintColor = new Color(0.2f,  0.2f,  0.3f);

        // Middle color.
        private Color thirdBasePaintColor = new Color(0.35f, 0.35f, 0.3f);
        
        // Flake layer color.
        private Color flakesColor = new Color(0.4f,  0.4f,  0.4f);
        
        // Microflake Perturbation. Value between -1 and 1.
        private float microflakePerturbation = 1.0f;

        // Microflake Perturbation A. Value between 0 and 1.
        private float microflakePerturbationA = 0.1f;
        
        // Normal Perturbation. Value between -1 and 1.
        private float normalPerturbation = 1.0f;

        private float flakesScale = 50;
        private float flakesExponent = 16;

        #endregion

        #region Properties
        
        #region Colors

        /// <summary>
        /// Base paint color.
        /// </summary>
        public Color BasePaintColor
        {
            get { return basePaintColor; }
            set { basePaintColor = value; }
        } // BasePaintColor

        /// <summary>
        /// Second base paint color.
        /// </summary>
        public Color SecondBasePaintColor
        {
            get { return secondBasePaintColor; }
            set { secondBasePaintColor = value; }
        } // LightedPaintColor

        /// <summary>
        /// Third Base Paint Color.
        /// </summary>
        public Color ThirdBasePaintColor
        {
            get { return thirdBasePaintColor; }
            set { thirdBasePaintColor = value; }
        } // ThirdBasePaintColor

        #endregion
        
        #region Flakes

        /// <summary>
        /// Flake layer color.
        /// </summary>
        public Color FlakesColor
        {
            get { return flakesColor; }
            set { flakesColor = value; }
        } // FlakesColor
        
        public float FlakesScale
        {
            get { return flakesScale; }
            set { flakesScale = value; }
        }

        public float FlakesExponent
        {
            get { return flakesExponent; }
            set { flakesExponent = value; }
        }

        /// <summary>
        /// Microflake Perturbation. Value between -1 and 1.
        /// </summary>
        public float MicroflakePerturbation
        {
            get { return microflakePerturbation; }
            set
            {
                if (value >= -1 && value <= 1)
                    microflakePerturbation = value;
            }
        } // MicroflakePerturbation

        /// <summary>
        /// Microflake Perturbation A. Value between 0 and 1.
        /// </summary>
        public float MicroflakePerturbationA
        {
            get { return microflakePerturbationA; }
            set
            {
                if (value >= 0 && value <= 1)
                    microflakePerturbationA = value;
            }
        } // MicroflakePerturbationA

        /// <summary>
        /// Normal Perturbation. Value between -1 and 1.
        /// </summary>
        public float NormalPerturbation
        {
            get { return normalPerturbation; }
            set
            {
                if (value >= -1 && value <= 1)
                    normalPerturbation = value;
            }
        } // NormalPerturbation

        #endregion

        #endregion

        #region Constructor

        public CarPaint()
        {
            Name = "Car Paint-" + nameNumber;
            nameNumber++;
        } // CarPaint

        #endregion
        
    } // CarPaint
} // XNAFinalEngine.Assets

