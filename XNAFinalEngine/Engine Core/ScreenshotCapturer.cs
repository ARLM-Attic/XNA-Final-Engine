
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

#region Using Statements
using System;
using System.IO;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.EngineCore
{
	/// <summary>
    /// Screenshot Capturer.
    /// Creates a png image of the current frame when the user press “print screen”.
    /// Because this key is "special" the call is done by the keyboard hook class. 
	/// </summary>
	public class ScreenshotCapturer
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
		private int screenshotNumber = 1;

        /// <summary>
        /// Render Target used to store the screenshot frame.
        /// </summary>
        private RenderTarget screenshotTexture;

		#endregion

        #region Properties

        /// <summary>
        /// We need to make a screenshot in this frame?
        /// </summary>
        internal bool NeedToMakeScreenshot { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Screenshot Capturer
        /// </summary>
		public ScreenshotCapturer()
		{   
			screenshotNumber = CurrentScreenshotNumber();
            NeedToMakeScreenshot = false;
		} // ScreenshotCapturer

		#endregion

        #region Screenshot name builder

        /// <summary>
        /// Screenshot name builder.
        /// </summary>
        private static string ScreenshotNameBuilder(int num)
        {
            return Directories.ScreenshotsDirectory + "\\" + screenshotFileName + "-" + num.ToString("0000") + ".png";
        } // ScreenshotNameBuilder

        #endregion

        #region Current screenshot number

        /// <summary>
        /// Get current screenshot num
        /// </summary>
        private static int CurrentScreenshotNumber()
        {
            // We must search for last screenshot we can found in list using own fast filesearch
            int i = 0, j = 0, k = 0, l = -1;
            // First check if at least 1 screenshot exist
            if (File.Exists(ScreenshotNameBuilder(1)))
            {
                // First scan for screenshot num/1000
                for (i = 1; i < 10; i++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000)) == false)
                        break;
                } // for (i)

                // This i*1000 does not exist, continue scan next level
                // screenshotnr/100
                i--;
                for (j = 1; j < 10; j++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100)) == false)
                        break;
                } // for (j)

                // This i*1000+j*100 does not exist, continue scan next level
                // screenshotnr/10
                j--;
                for (k = 1; k < 10; k++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100 + k * 10)) == false)
                        break;
                } // for (k)

                // This i*1000+j*100+k*10 does not exist, continue scan next level
                // screenshotnr/1
                k--;
                for (l = 1; l < 10; l++)
                {
                    if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100 + k * 10 + l)) == false)
                        break;
                } // for (l)

                // This i*1000+j*100+k*10+l does not exist, we have now last
                // screenshot nr!!!
                l--;
            } // if (File.Exists)

            return i * 1000 + j * 100 + k * 10 + l;
        } // GetCurrentScreenshotNum

        #endregion

		#region Make screenshot

		/// <summary>
		/// Make png screenshot.
		/// </summary>
		public void MakeScreenshot()
		{
            NeedToMakeScreenshot = true;
        }// MakeScreenshot

        /// <summary>
        /// Begin the screenshot process. Before the frame draw.
        /// </summary>
        internal void BeginScreenshot()
        {
            try
            {       
                // Make sure screenshots directory exists
                if (Directory.Exists(ContentManager.GameDataDirectory + "Screenshots") == false)
                    Directory.CreateDirectory(ContentManager.GameDataDirectory + "Screenshots");

                screenshotTexture = new RenderTarget(RenderTarget.SizeType.FullScreen);
                screenshotTexture.EnableRenderTarget();
            } // try
            catch (Exception ex)
            {
                NeedToMakeScreenshot = false;
                throw new Exception("Failed to save screenshot: " + ex);
            }
        } // BeginScreenshot

        /// <summary>
        /// End the screenshot process. After the frame draw.
        /// </summary>
        internal void EndScreenshot()
        {
            try
            {
                screenshotNumber++;
                var stream = new FileStream(ScreenshotNameBuilder(screenshotNumber), FileMode.OpenOrCreate);
                screenshotTexture.DisableRenderTarget();
                // Don't want a one frame purple screen, we need to show the screenshot frame on screen.
                screenshotTexture.RenderOnFullScreen();
                SpriteManager.DrawSprites();
                screenshotTexture.XnaTexture.SaveAsPng(stream, EngineManager.Width, EngineManager.Height);
            } // try
            catch (Exception ex)
            {
                throw new Exception("Failed to save screenshot: " + ex);
            }
            finally
            {
                NeedToMakeScreenshot = false;
            }
        } // EndScreenshot

		#endregion

	} // ScreenshotCapturer
} // XNAFinalEngine.EngineCore
