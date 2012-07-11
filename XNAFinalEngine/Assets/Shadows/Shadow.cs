
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
using System;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{
	/// <summary>
	/// Shadows.
	/// </summary>
    public abstract class Shadow : AssetWithoutResource
	{

        #region Enumerates

        /// <summary>
        /// Filter Type.
        /// There are four PCF standard filters and one with poison distribution.
        /// In the future it can be expected a variance and exponential filters.
        /// </summary>
        public enum FilterType
        {
            Pcf2X2,
            Pcf3X3,
            Pcf5X5,
            Pcf7X7,
            PcfPosion
        } // FilterType

        #endregion

		#region Variables
        
        // Default values.
        private Size lightDepthTextureSize = Size.Square1024X1024;
        private FilterType filter = FilterType.Pcf5X5;
        private float depthBias = 0.0025f;
        private Size.TextureSize textureSize = Size.TextureSize.HalfSize;
	    private float range = 50;

        // Is it enabled?
        private bool enabled = true;

	    /// <summary>
	    /// Light Depth Texture
	    /// </summary>
        internal RenderTarget LightDepthTexture;

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
        /// Filter type.
        /// There are four PCF standard filters and one with poison distribution.
        /// In the future it can be expected a variance and exponential filters.
        /// </summary>
        public FilterType Filter
	    {
	        get { return filter; }
	        set { filter = value; }
	    } // Filter

	    /// <summary>
        /// Depth Bias (value between 0 to 0.1)
        /// </summary>
        public float DepthBias
        {
            get { return depthBias; }
            set
            {
                depthBias = value;
                if (value < 0f)
                    depthBias = 0;
                if (value > 0.1f)
                    depthBias = 0.1f;
            }
        } // DepthBias
        
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
        } // ShadowMapSize

	    /// <summary>
        /// Shadow Texture Size.
        /// This is a low frequency result therefore you don’t have to use a full screen size buffer.
	    /// </summary>
	    public Size.TextureSize TextureSize
	    {
	        get { return textureSize; }
	        set { textureSize = value; }
        } // TextureSize

        /// <summary>
        /// Shadow's far plane.
        /// </summary>
        /// <remarks>This value is ignored when the shadow is associated with a spot light.</remarks>
        public float Range
        {
            get { return range; }
            set
            {
                range = value;
                if (range < 0.0f)
                    range = 0;
            }
        } // Range

	    #endregion

    } // Shadow
} // XNAFinalEngine.Assets
