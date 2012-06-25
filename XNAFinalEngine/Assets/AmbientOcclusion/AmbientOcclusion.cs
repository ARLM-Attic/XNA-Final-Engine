
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

#region Using directives
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Base class for ambient occlusion effects.
    /// </summary>
    /// <remarks>
    /// There are two options where to apply them: in the ambient light or in the final result, previous to the post process.
    /// The first is more "correct" but subtle. I choose the first to achieve more photorealistic results.
    /// </remarks>
    public abstract class AmbientOcclusion : Asset
    {

        #region Variables

        // Is it enabled?
        private bool enabled = true;

        // Ambient occlusion resolution, relative to the camera's render target.
        private Size.TextureSize textureSize = Size.TextureSize.QuarterSize;

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
        /// Ambient occlusion resolution, relative to the camera's render target.
        /// Ambient occlusion is a costly technique but it produces a low frequency result.
        /// So there is no need to use a render target of the same dimension as the frame buffer.
        /// Normally a half resolution buffer produces very good results and if the performance is critical you could use a quarter size buffer.
        /// </summary>
        public Size.TextureSize TextureSize
        {
            get { return textureSize; }
            set { textureSize = value; }
        } // TextureSize

        #endregion

    } // AmbientOcclusion
} // XNAFinalEngine.Assets

