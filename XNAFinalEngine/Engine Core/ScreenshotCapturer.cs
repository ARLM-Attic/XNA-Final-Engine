
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

#region Using Statements
using System;
#if !XBOX
    using System.Drawing;
    using System.Drawing.Imaging;
#endif
using System.IO;
using XNAFinalEngine.Assets;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.EngineCore
{
	/// <summary>
    /// Creates and stores an image of the current frame when the user press “print screen”.
	/// </summary>
	public static class ScreenshotCapturer
    {

        #region Constants

        /// <summary>
        /// Screenshot file name. Directory\ + screenshotFileName + "-" + number + extention.
        /// </summary>
        private const string screenshotFileName = "Screenshot";

        #endregion

        #region Variables

        /// <summary>
		/// Internal screenshot number (will increase by one each screenshot)
		/// </summary>
        private static int screenshotNumber;

		#endregion

        #region Properties

        /// <summary>
        /// Make a screenshot the next time a frame ends.
        /// </summary>
        public static bool MakeScreenshot { get; set; }

        #endregion

        #region Screenshot Name Builder

        /// <summary>
        /// Screenshot name builder.
        /// </summary>
        private static string ScreenshotNameBuilder(int num)
        {
            return "Content\\Screenshots" + "\\" + screenshotFileName + "-" + num.ToString("0000") + ".jpg";
        } // ScreenshotNameBuilder

        #endregion

        #region Current Screenshot Number

        /// <summary>
        /// Get current screenshot number.
        /// </summary>
        private static int CurrentScreenshotNumber()
        {
            // We must search for last screenshot we can found in list using own fast filesearch.
            int i = 0, j = 0, k = 0, l = -1;
            // First check if at least 1 screenshot exist
            if (File.Exists(ScreenshotNameBuilder(0)))
            {
                // Scan for screenshot number/1000
                for (i = 1; i < 10; i++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000)) == false)
                        break;
                }

                // This i*1000 does not exist, continue scan next level
                // screenshotnr/100
                i--;
                for (j = 1; j < 10; j++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100)) == false)
                        break;
                }

                // This i*1000+j*100 does not exist, continue scan next level
                // screenshotnr/10
                j--;
                for (k = 1; k < 10; k++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100 + k * 10)) == false)
                        break;
                }

                // This i*1000+j*100+k*10 does not exist, continue scan next level
                // screenshotnr/1
                k--;
                for (l = 1; l < 10; l++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100 + k * 10 + l)) == false)
                        break;
                }

                // This i*1000+j*100+k*10+l does not exist, we have now last
                // screenshot nr!!!
                l--;
            }

            return i * 1000 + j * 100 + k * 10 + l;
        } // GetCurrentScreenshotNum

        #endregion

		#region Save Screenshot

        /// <summary>
        /// Save the texture to a file.
        /// </summary>
        internal static void SaveScreenshot(Texture texture)
        {
            try
            {
                // Make sure screenshots directory exists
                if (Directory.Exists(ContentManager.GameDataDirectory + "Screenshots") == false)
                    Directory.CreateDirectory(ContentManager.GameDataDirectory + "Screenshots");

                screenshotNumber = CurrentScreenshotNumber();
                screenshotNumber++;
                
                #if XBOX
                    // It produced a memory leak and it was never fixed by the XNA team.
                    var stream = new FileStream(ScreenshotNameBuilder(screenshotNumber), FileMode.OpenOrCreate);
                    texture.Resource.SaveAsJpeg(stream, texture.Width, texture.Height);
                #else
                    SaveAsJpeg(texture, ScreenshotNameBuilder(screenshotNumber));
                #endif
                
            } // try
            catch (Exception e)
            {
                throw new InvalidOperationException("Screenshot: Failed to save screenshot.", e);
            }
        } // SaveScreenshot

		#endregion

        #region Save As Jpeg

#if !XBOX
        /// <summary>
        /// Saves texture data as a .jpg. 
        /// </summary>
        /// <remarks>
        /// The XNA functions produce a memory leak. JohnU function does not.
        /// http://xboxforums.create.msdn.com/forums/p/65527/515125.aspx
        /// </remarks>
        private static void SaveAsJpeg(Texture texture, String filename)
        {
            Rectangle textureRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            byte[] textureData = new byte[4 * texture.Width * texture.Height];
            Bitmap bitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format32bppArgb);
 
            texture.Resource.GetData(textureData);
            for (int i = 0; i < textureData.Length; i += 4)
            {
                byte blue = textureData[i];
                textureData[i] = textureData[i + 2];
                textureData[i + 2] = blue;
            }

            BitmapData bitmapData = bitmap.LockBits(textureRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr safePtr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bitmap.UnlockBits(bitmapData);
            bitmap.Save(filename, ImageFormat.Jpeg);
        } // SaveAsJpeg
#endif

        #endregion

    } // ScreenshotCapturer
} // XNAFinalEngine.EngineCore
