
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{

	/// <summary>
	/// Base class for textures.
    /// Important: Try to dispose only the textures created without the content manager.
    /// If you dispose a texture and then you try to load again using the same content managed an exception will be raised.
    /// In this cases use the Unload method from the Content Manager instead.
	/// </summary>
    public class Texture : Asset
    {

        #region Variables
                
        /// <summary>
        /// XNA Texture.
        /// </summary>
        protected Texture2D xnaTexture;

        // Default value.
        private SamplerState preferedSamplerState = SamplerState.AnisotropicWrap;

        #endregion

        #region Properties

        #region Resource

        /// <summary>
        /// XNA Texture.
        /// </summary>
        public virtual Texture2D Resource
        { 
            get
            {
                // Textures and render targets have a different treatment because textures could be set,
                // because both are persistent shader parameters, and because they could be created without using content managers.
                // For that reason the nullified resources could be accessed.
                //if (xnaTexture != null && xnaTexture.IsDisposed)
                    //xnaTexture = null;
                return xnaTexture;
            }
            // This is only allowed for videos. 
            // Doing something to avoid this “set” is unnecessary and probably will make more complex some classes just for this special case. 
            // Besides, an internal statement elegantly prevents a bad use of this set.
            // Just don’t dispose this texture because the resource is managed by the video.
            internal set 
            {
                xnaTexture = value;
                if (value == null)
                    Size = new Size(0, 0);
                else
                    Size = new Size(xnaTexture.Width, xnaTexture.Height);
            }
        } // Resource

        #endregion

        #region Preferred Sampler State

        /// <summary>
        /// Some shaders allow us to choose how to sample the texture data.
        /// </summary>
        public virtual SamplerState PreferredSamplerState
	    {
	        get { return preferedSamplerState; }
	        set { preferedSamplerState = value; }
	    } // PreferredSamplerState

	    #endregion

        #region Size

        /// <summary>
        /// Texture's width.
        /// </summary>
        public int Width { get { return Size.Width; } }

        /// <summary>
        /// Texture's height.
        /// </summary>
        public int Height { get { return Size.Height; } }

        /// <summary>
        /// Rectangle that starts in 0, 0 and finish in the width and height of the texture. 
        /// </summary>
        public Rectangle TextureRectangle { get { return new Rectangle(0, 0, Width, Height); } }

        /// <summary>
        /// Size.
        /// This value store information about sizes relative to screen.
        /// </summary>
        public Size Size { get; protected set; }

        #endregion

        #region Textures Filenames

        /// <summary>
        ///  A list with all texture' filenames on the texture directory, except cube textures, lookup tables and user interface textures.
        /// </summary>
        /// <remarks>
        /// If there are memory limitations, this list could be eliminated for the release version.
        /// This is use only useful for the editor.
        /// </remarks>
        public static string[] TexturesFilenames { get; private set; }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Empty texture. 
        /// </summary>
        internal Texture()
        {
            Name = "Empty Texture";
        } // Texture

	    /// <summary>
        /// Texture from XNA asset.
        /// </summary>
        public Texture(Texture2D xnaTexture)
        {
            Name = "Texture";
            this.xnaTexture = xnaTexture;
            Size = new Size(xnaTexture.Width, xnaTexture.Height);
        } // Texture

		/// <summary>
		/// Load texture.
		/// </summary>
        /// <param name="filename">The filename must be relative and be a valid file in the textures directory.</param>
        public Texture(string filename)
        {
            Name = filename;
            Filename = ContentManager.GameDataDirectory + "Textures\\" + filename;
            if (File.Exists(Filename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load texture: File " + Filename + " does not exists!", "filename");
            }
            try
            {
                xnaTexture = ContentManager.CurrentContentManager.XnaContentManager.Load<Texture2D>(Filename);
                ContentManager = ContentManager.CurrentContentManager;
                Size = new Size(xnaTexture.Width, xnaTexture.Height);
                Resource.Name = filename;
            }
            catch (ObjectDisposedException)
            {
                throw new InvalidOperationException("Content Manager: Content manager disposed");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load texture: " + filename, e);
            }
		} // Texture

		#endregion

        #region Static Constructor

        /// <summary>
        /// Search the available textures.
        /// </summary>
        static Texture()
        {
            #if XBOX
                TexturesFilenames = new string[0];
            #else
                const string texturesDirectoryPath = ContentManager.GameDataDirectory + "Textures";
                // Search the texture files //
                DirectoryInfo texturesDirectory = new DirectoryInfo(texturesDirectoryPath);
                try
                {
                    FileInfo[] texturesFileInformation = texturesDirectory.GetFiles("*.xnb", SearchOption.AllDirectories);
                    int count = 0, j = 0;
                    // Count the textures, except cube textures, lookup tables and user interface textures.
                    for (int i = 0; i < texturesFileInformation.Length; i++)
                    {
                        FileInfo fileInformation = texturesFileInformation[i];
                        if (!fileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\Skin") &&
                            !fileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\CubeTextures") &&
                            !fileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\LookupTables"))
                        {
                            count++;
                        }
                    }
                    // Create the array of available textures.
                    TexturesFilenames = new string[count];
                    for (int i = 0; i < texturesFileInformation.Length; i++)
                    {
                        FileInfo fileInformation = texturesFileInformation[i];
                        if (!fileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\Skin") &&
                            !fileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\CubeTextures") &&
                            !fileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\LookupTables"))
                        {
                            // Some textures are in a sub directory, in that case we have to know how is called.
                            string[] splitDirectoryName = fileInformation.DirectoryName.Split(new[] { texturesDirectoryPath }, StringSplitOptions.None);
                            string subdirectory = "";
                            // If is in a sub directory
                            if (splitDirectoryName[1] != "")
                            {
                                subdirectory = splitDirectoryName[1].Substring(1, splitDirectoryName[1].Length - 1) + "\\"; // We delete the start \ and add another \ to the end.
                            }
                            TexturesFilenames[j] = subdirectory + fileInformation.Name.Substring(0, fileInformation.Name.Length - 4);
                            j++;
                        }
                    }
                }
                // If there was an error then do nothing.
                catch
                {
                    TexturesFilenames = new string[0];
                }
            #endif
        } // Texture

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            if (xnaTexture != null && !xnaTexture.IsDisposed)
                Resource.Dispose();
        } // DisposeManagedResources

	    #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal override void RecreateResource()
        {
            if (string.IsNullOrEmpty(Filename))
                xnaTexture = new Texture2D(EngineManager.Device, Size.Width, Size.Height);
            else
                xnaTexture = ContentManager.CurrentContentManager.XnaContentManager.Load<Texture2D>(Filename);
        } // RecreateResource

        /// <summary>
        /// Recreate textures created without using a content manager.
        /// </summary>
        internal static void RecreateTexturesWithoutContentManager()
        {
            foreach (Asset loadedTexture in LoadedAssets)
            {
                if (loadedTexture is Texture && loadedTexture.ContentManager == null)
                {
                    if (((Texture)loadedTexture).Resource != null)
                    {
                        ((Texture)loadedTexture).Resource.Dispose();
                        loadedTexture.RecreateResource();
                    }
                }
            }
        } // RecreateTexturesWithoutContentManager

        #endregion

    } // Texture
} // XNAFinalEngine.Assets

