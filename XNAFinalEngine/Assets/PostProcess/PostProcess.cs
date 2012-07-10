
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
    /// This is used to add post process effects like tone mapping, bloom, MLAA, film grain, and color grading.
    /// </summary>
    public class PostProcess : AssetWhitoutResource
    {

        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;
        
        private readonly ToneMapping toneMapping = new ToneMapping();
        private readonly Bloom bloom = new Bloom();
        private readonly MLAA mlaa = new MLAA();
        private readonly FilmGrain filmGrain = new FilmGrain();
        private readonly AdjustLevels adjustLevels = new AdjustLevels();
        private readonly AdjustLevelsIndividualChannels adjustLevelsIndividualChannels = new AdjustLevelsIndividualChannels();
        private readonly ColorCorrection colorCorrection = new ColorCorrection();

        #endregion

        #region Properties

        /// <summary>
        /// Tone Mapping.
        /// </summary>
        public ToneMapping ToneMapping { get { return toneMapping; } }

        /// <summary>
        /// Bloom.
        /// </summary>
        public Bloom Bloom { get { return bloom; } }

        /// <summary>
        /// Morphological Antialiasing (MLAA).
        /// </summary>
        public MLAA MLAA { get { return mlaa; } }

        /// <summary>
        /// Film Grain.
        /// </summary>
        public FilmGrain FilmGrain { get { return filmGrain; } }

        /// <summary>
        /// Color Correction.
        /// </summary>
        public ColorCorrection ColorCorrection { get { return colorCorrection; } }

        /// <summary>
        /// Adjust color levels, just like Photoshop.
        /// </summary>
        public AdjustLevels AdjustLevels { get { return adjustLevels; } }

        /// <summary>
        /// Adjust color levels, just like Photoshop. This adjusts each channel individually.
        /// </summary>
        public AdjustLevelsIndividualChannels AdjustLevelsIndividualChannels { get { return adjustLevelsIndividualChannels; } }
        
        #endregion

        #region Constructor

        /// <summary>
        /// This is used to add post process effects like tone mapping, bloom, MLAA, film grain, and color grading.
        /// </summary>
        public PostProcess()
        {
            Name = "Post Process-" + nameNumber;
            nameNumber++;
        } // PostProcess

        #endregion

    } // PostProcess
} // XNAFinalEngine.Assets