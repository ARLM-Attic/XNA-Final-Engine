
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
    /// Color correction.
    /// Uses three-dimensional lookup tables for real-time color processing and allows having up to two lookup tables at the same time.
    /// If two lookup tables are active then the system will lerp the colors by the LerpLookupTablesAmount property value. 
    /// If the LerpOriginalColorAmount has a value different to 0 the system will lerp between the color corrected by the lookup tables and the original color.
    /// 
    /// Unreal Engine 3 and Cryengine 3 both use a 16x16x16 lookup table.
    /// 
    /// Lookup tables (LUTs) are an excellent technique for optimizing the evaluation of functions that are expensive to compute and inexpensive to cache.
    /// By precomputing the evaluation of a function over a domain of common inputs, expensive runtime operations can be replaced with inexpensive table lookups.
    /// If the table lookups can be performed faster than computing the results from scratch (or if the function is repeatedly queried at the same input),
    /// then the use of a lookup table will yield significant performance gains. 
    /// For data requests that fall between the table's samples, an interpolation algorithm can generate reasonable approximations by averaging nearby samples.
    /// 
    /// About this subject:
    /// http://http.developer.nvidia.com/GPUGems/gpugems_ch22.html
    /// http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter24.html
    /// 
    /// Maybe this paper is useful: Reducing the Cost of Lookup Table Based Color Transformations (Raja Balasubramanian)
    /// I don’t read it yet because I’m happy with the performance of the current lookup tables.
    /// 
    /// One more thing: a lookup table doesn’t reduce color precision thanks to the GPU’s linear interpolation. However you can lose precision in the transformation itself.
    /// </summary>
    public class ColorCorrection
    {

        #region Variables

        // Indicates how much each of the two lookup table results will affect the final color.
        private float lerpLookupTablesAmount = 0.5f;

        // Indicates how much will be interpolated between the original color and the corrected color.
        private float lerpOriginalColorAmount = 1;

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
        /// First lookup table.
        /// If is null then no color correction will be performed.
        /// </summary>
        public LookupTable FirstLookupTable { get; set; }

        /// <summary>
        /// Second lookup table.
        /// If isn't null then the system will lerp the colors by the LerpLookupTablesAmount property value.
        /// </summary>
        public LookupTable SecondtLookupTable { get; set; }
        
        /// <summary>
        /// Indicates how much each of the two lookup table results will affect the final color.
        /// 0 = first lookup table.
        /// 1 = second lookup table.
        /// default = 0.5
        /// </summary>
        public float LerpLookupTablesAmount
        {
            get { return lerpLookupTablesAmount; }
            set
            {
                lerpLookupTablesAmount = value;
                if (lerpLookupTablesAmount < 0)
                    lerpLookupTablesAmount = 0;
                if (lerpLookupTablesAmount > 1)
                    lerpLookupTablesAmount = 1;
            }
        } // LerpLookupTablesAmount

        /// <summary>
        /// Indicates how much will be interpolated between the original color and the corrected color.
        /// 0 = original color.
        /// 1 = corrected color.
        /// default = 1
        /// </summary>
        public float LerpOriginalColorAmount
        {
            get { return lerpOriginalColorAmount; }
            set
            {
                lerpOriginalColorAmount = value;
                if (lerpOriginalColorAmount < 0)
                    lerpOriginalColorAmount = 0;
                if (lerpOriginalColorAmount > 1)
                    lerpOriginalColorAmount = 1;
            }
        } // LerpOriginalColorAmount

        #endregion

    } // ColorCorrection
} // XNAFinalEngine.Assets
