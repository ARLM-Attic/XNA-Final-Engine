
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
using System;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{
	/// <summary>
	/// Basic shadows.
	/// Could be used on directional lights and spot lights.
    /// Only one directional light could have active shadows so that the data could be arranged better.
	/// </summary>
    public class BasicShadow : Shadow
	{

        #region Variables

        private Size lightDepthTextureSize = Size.Square1024X1024;

        // The count of materials for naming purposes.
        private static int nameNumber = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Light Depth Texture
        /// </summary>
        internal RenderTarget LightDepthTexture;

        /// <summary>
        /// Light depth texture size.
        /// This is a temporal render target but its size is important. 
        /// Greater size equals better results but the performance penalty is significant.
        /// The size has to be square.
        /// </summary>
        public Size LightDepthTextureSize
        {
            get { return lightDepthTextureSize; }
            set
            {
                if (value.Width != value.Height)
                    throw new ArgumentException("Shadow: light depth textures needs to be square.");
                lightDepthTextureSize = value;
            }
        } // LightDepthTextureSize

        #endregion

        #region Constructor

        public BasicShadow()
        {
            Name = "Basic Shadow-" + nameNumber;
            nameNumber++;
        } // BasicShadow

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            if (LightDepthTexture != null)
                RenderTarget.Release(LightDepthTexture);
        } // DisposeUnmanagedResources

        #endregion

        #region Release Light Depth Texture

        /// <summary>
        /// Release Light Depth Texture.
        /// </summary>
        internal override void ReleaseLightDepthTexture()
        {
            if (LightDepthTexture != null)
                RenderTarget.Release(LightDepthTexture);
        } // ReleaseLightDepthTexture

        #endregion

    } // BasicShadow
} // XNAFinalEngine.Assets
