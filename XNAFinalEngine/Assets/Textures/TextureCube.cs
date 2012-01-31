
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
using System.IO;
#endregion

namespace XNAFinalEngine.Assets
{

	/// <summary>
	/// LDR or HDR Cube Maps.
	/// HDR can be stored in the RGBM format.
	/// </summary>
    public class TextureCube : Asset
    {

        #region Variables
                
        /// <summary>
        /// XNA Texture.
        /// </summary>
        protected Microsoft.Xna.Framework.Graphics.TextureCube xnaTextureCube;

	    /// <summary>
        /// The size of this texture resource, in pixels.
	    /// </summary>
	    protected int size;

        #endregion

        #region Properties

        /// <summary>
        /// XNA Texture.
        /// </summary>
        public virtual Microsoft.Xna.Framework.Graphics.TextureCube Resource { get { return xnaTextureCube; } }

        /// <summary>
        /// The size of this texture resource, in pixels.
        /// </summary>
        public int Size { get { return size; } }

        /// <summary>
        /// Is it in RGBM format?
        /// </summary>
        public bool IsRgbm { get; set; }

        /// <summary>
        /// RGBM Max Range.
        /// </summary>
        public float RgbmMaxRange { get; set; }

        /// <summary>
        ///  A list with all texture' filenames on the cube texture directory.
        /// </summary>
        public static string[] TexturesFilename { get; private set; }
    
        #endregion

        #region Constructor

	    /// <summary>
	    /// Create cube map from given filename.
	    /// </summary>
	    /// <param name="filename">Set filename, must be relative and be a valid file in the textures directory.</param>
	    /// <param name="isRgbm">is in RGBM format?</param>
        /// <param name="rgbmMaxRange">RGBM Max Range.</param>
	    public TextureCube(string filename, bool isRgbm = false, float rgbmMaxRange = 50.0f)
		{
            Name = filename;
		    IsRgbm = isRgbm;
	        RgbmMaxRange = rgbmMaxRange;
            string fullFilename = ContentManager.GameDataDirectory + "Textures\\CubeTextures\\" + filename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load cube map: File " + fullFilename + " does not exists!");
            }
            try
            {
                xnaTextureCube = ContentManager.CurrentContentManager.XnaContentManager.Load<Microsoft.Xna.Framework.Graphics.TextureCube>(fullFilename);
                ContentManager = ContentManager.CurrentContentManager;
                size = xnaTextureCube.Size;
            }
            catch (ObjectDisposedException)
            {
                throw new InvalidOperationException("Content Manager: Content manager disposed");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load cube map: " + filename, e);
            }
		} // TextureCube
        
        /// <summary>
        /// Search the available cube texture.
        /// </summary>
        static TextureCube()
        {
            string texturesDirectoryPath = ContentManager.GameDataDirectory + "Textures\\CubeTextures";
            // Search the texture files //
            DirectoryInfo texturesDirectory = new DirectoryInfo(texturesDirectoryPath);
            try
            {
                FileInfo[] texturesFileInformation = texturesDirectory.GetFiles("*.xnb", SearchOption.AllDirectories);
                // Count the textures, except cube textures and user interface textures.
                TexturesFilename = new string[texturesFileInformation.Length];
                for (int i = 0; i < texturesFileInformation.Length; i++)
                {
                    FileInfo fileInformation = texturesFileInformation[i];
                    // Some textures are in a sub directory, in that case we have to know how is called.
                    string[] splitDirectoryName = fileInformation.DirectoryName.Split(new[] { texturesDirectoryPath }, StringSplitOptions.None);
                    string subdirectory = "";
                    // If is in a sub directory
                    if (splitDirectoryName[1] != "")
                    {
                        subdirectory = splitDirectoryName[1].Substring(1, splitDirectoryName[1].Length - 1) + "\\"; // We delete the start \ and add another \ to the end.
                    }
                    TexturesFilename[i] = subdirectory + fileInformation.Name.Substring(0, fileInformation.Name.Length - 4);

                }
            }
            // If there was an error then do nothing.
            catch
            {
                TexturesFilename = new string[0];
            }
        } // TextureCube

		#endregion

    } // TextureCube
} // XNAFinalEngine.Assets

