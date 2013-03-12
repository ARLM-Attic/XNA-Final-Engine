
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

using System.IO;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;

#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// Compress a HDR texture in RGBM format, and stores the result in two textures, one for the RGB channels and one for the M channel.
    /// Remember to set the content processor's texture format of the HDR texture to NoChange.
    /// </summary>
    public class HDRToRGBMScene : Scene
    {

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        protected override void LoadContent()
        {
            Texture texture = RgbmHelper.ConvertHdrTextureToRgbmOnlyRgb(new Texture("CubeTextures\\Desert-15_XXLResize"), 17);
            var stream = new FileStream("Desert-15_XXLResizeRGB.jpg", FileMode.OpenOrCreate);
            texture.Resource.SaveAsJpeg(stream, texture.Width, texture.Height);
            texture = RgbmHelper.ConvertHdrTextureToRgbmOnlyM(new Texture("CubeTextures\\Desert-15_XXLResize"), 17);
            stream = new FileStream("Desert-15_XXLResizeM.jpg", FileMode.OpenOrCreate);
            texture.Resource.SaveAsJpeg(stream, texture.Width, texture.Height);
        } // Load

        #endregion

    } // EmptyScene
} // XNAFinalEngineExamples
