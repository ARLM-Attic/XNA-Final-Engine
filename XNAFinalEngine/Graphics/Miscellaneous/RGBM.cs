
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
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// RGBM Helper.
    /// http://xnafinalengine.codeplex.com/wikipage?title=HDR%20Internal%20Representation&referringTitle=Documentation
    /// </summary>
    public static class RgbmHelper
    {

        #region Float To RGBM

        /// <summary>
        /// Encode from float (linear) to RGBM (gamma).
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="maxRange">Maximum float value</param>
        public static Color FloatLinearToRgbmGamma(float r, float g, float b, float maxRange)
        {
            // Catch negative numbers.
            r = Math.Max(0, r);
            g = Math.Max(0, g);
            b = Math.Max(0, b);
            float maxRgb = Math.Max(r, Math.Max(g, b));
            float m = Math.Min(maxRgb / maxRange, 1.0f); // Catch max above MaxRange.
            
            r = r / (m * maxRange); // between 0 to 1
            g = g / (m * maxRange); // between 0 to 1
            b = b / (m * maxRange); // between 0 to 1

            // From linear to gamma space. Gamma space stores better lower values.
            return GammaLinearSpaceHelper.LinearToGamma(new Vector4(r, g, b, m));
        } // FloatToRgbm

        #endregion

        #region RGBM To Float

        /// <summary>
        /// Decode from RGBM to float.
        /// </summary>
        /// <param name="rgbm">RGBM Gamma Color</param>
        /// <param name="maxRange">Maximum float value</param>
        public static Vector3 RgbmGammaToFloatLinear(Color rgbm, float maxRange)
        {
            Vector4 rgbmLinear = GammaLinearSpaceHelper.GammaToLinear(rgbm);

            rgbmLinear.W *= maxRange;

            rgbmLinear.X *= rgbmLinear.W;
            rgbmLinear.Y *= rgbmLinear.W;
            rgbmLinear.Z *= rgbmLinear.W;

            return new Vector3(rgbmLinear.X, rgbmLinear.Y, rgbmLinear.Z);
        } // RgbmToFloat

        #endregion

        #region RGBE To Float

        /// <summary>
        /// Encode from RGBE to float. Not tested.
        /// </summary>
        /// <param name="red">Red</param>
        /// <param name="green">Green</param>
        /// <param name="blue">Blue</param>
        /// <param name="exponent">Exponent</param>
        public static Vector3 RgbeToFloat(float red, float green, float blue, float exponent)
        {
            Vector3 result = new Vector3();
            float floatExponent = exponent - 128 - 8;
            result.X = red   * (float)(Math.Pow(2, floatExponent));
            result.Y = green * (float)(Math.Pow(2, floatExponent));
            result.Z = blue  * (float)(Math.Pow(2, floatExponent));
            return result;
        } // RgbeToFloat

        #endregion

        #region Convert HDR Texture To RGBM

        /// <summary>
        /// Convert a HDR Texture To Rgbm format.
        /// </summary>
        /// <param name="hdrTexture">HDR Texture</param>
        /// <param name="maxRange">Maximum float value for RGBM encoding. Not necessary the maximum r, g, or b intensity value of the texture.</param>
        public static Texture ConvertHdrTextureToRgbm(Texture hdrTexture, float maxRange)
        {
            float[] floatArray = new float[hdrTexture.Width * hdrTexture.Height * 4]; // hdr textures stores an alpha value, at least xna returns to me the alpha component.
            Color[] colorArray = new Color[hdrTexture.Width * hdrTexture.Height];

            // Get float values from texture.
            hdrTexture.Resource.GetData<float>(floatArray);

            // For each color component we create the RGBM counterpart.
            for (int i = 0; i < hdrTexture.Width * hdrTexture.Height; i++)
            {
                // Vector3 rgb = rgbe2float(floatArray[i*4], floatArray[i*4 + 1], floatArray[i*4 + 2], floatArray[i*4 + 3]); // If the texture is in rgbe format.
                colorArray[i] = FloatLinearToRgbmGamma(floatArray[i * 4], floatArray[i * 4 + 1], floatArray[i * 4 + 2], maxRange);
            }
            Texture result = new Texture
            {
                Resource = new Texture2D(EngineManager.Device, hdrTexture.Width, hdrTexture.Height)
            };
            result.Resource.SetData(colorArray);
            return result;
        } // ConvertHdrTextureToRgbm

        #endregion

        #region Convert HDR Texture To RGBM (only RGB)

        /// <summary>
        /// Convert a HDR Texture To Rgbm format. Only returns the RGB components. This is useful for programs (like Bixorama) that don’t work well with the alpha channel.
        /// </summary>
        /// <param name="hdrTexture">HDR Texture</param>
        /// <param name="maxRange">Maximum float value for RGBM encoding. Not necessary the maximum r, g, or b intensity value of the texture.</param>
        public static Texture ConvertHdrTextureToRgbmOnlyRgb(Texture hdrTexture, float maxRange)
        {
            float[] floatArray = new float[hdrTexture.Width * hdrTexture.Height * 4]; // hdr textures stores an alpha value, at least xna returns to me the alpha component.
            Color[] colorArray = new Color[hdrTexture.Width * hdrTexture.Height];

            // Get float values from texture.
            hdrTexture.Resource.GetData<float>(floatArray);

            // For each color component we create the RGBM counterpart.
            for (int i = 0; i < hdrTexture.Width * hdrTexture.Height; i++)
            {
                // Vector3 rgb = rgbe2float(floatArray[i*4], floatArray[i*4 + 1], floatArray[i*4 + 2], floatArray[i*4 + 3]); // If the texture is in rgbe format.
                colorArray[i] = FloatLinearToRgbmGamma(floatArray[i * 4], floatArray[i * 4 + 1], floatArray[i * 4 + 2], maxRange);
                colorArray[i] = new Color(colorArray[i].R, colorArray[i].G, colorArray[i].B);
            }
            Texture result = new Texture
            {
                Resource = new Texture2D(EngineManager.Device, hdrTexture.Width, hdrTexture.Height)
            };
            result.Resource.SetData(colorArray);
            return result;
        } // ConvertHdrTextureToRgbm

        #endregion

        #region Convert HDR Texture To RGBM (only M)

        /// <summary>
        /// Convert a HDR Texture To Rgbm format. Only returns the RGB components. This is useful for programs (like Bixorama) that don’t work well with the alpha channel.
        /// </summary>
        /// <param name="hdrTexture">HDR Texture</param>
        /// <param name="maxRange">Maximum float value for RGBM encoding. Not necessary the maximum r, g, or b intensity value of the texture.</param>
        public static Texture ConvertHdrTextureToRgbmOnlyM(Texture hdrTexture, float maxRange)
        {
            float[] floatArray = new float[hdrTexture.Width * hdrTexture.Height * 4]; // hdr textures stores an alpha value, at least xna returns to me the alpha component.
            Color[] colorArray = new Color[hdrTexture.Width * hdrTexture.Height];

            // Get float values from texture.
            hdrTexture.Resource.GetData<float>(floatArray);

            // For each color component we create the RGBM counterpart.
            for (int i = 0; i < hdrTexture.Width * hdrTexture.Height; i++)
            {
                // Vector3 rgb = rgbe2float(floatArray[i*4], floatArray[i*4 + 1], floatArray[i*4 + 2], floatArray[i*4 + 3]); // If the texture is in rgbe format.
                colorArray[i] = FloatLinearToRgbmGamma(floatArray[i * 4], floatArray[i * 4 + 1], floatArray[i * 4 + 2], maxRange);
                colorArray[i] = new Color(colorArray[i].A, colorArray[i].A, colorArray[i].A);
            }
            Texture result = new Texture
            {
                Resource = new Texture2D(EngineManager.Device, hdrTexture.Width, hdrTexture.Height)
            };
            result.Resource.SetData(colorArray);
            return result;
        } // ConvertHdrTextureToRgbm

        #endregion

    } // RgbmHelper
} // XNAFinalEngine.Graphics