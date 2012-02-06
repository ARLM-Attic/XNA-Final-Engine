
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        #endregion

        #region Properties

        /// <summary>
        /// The name of the asset.
        /// If a name already exists then we add one to its name and we call it again.
        /// </summary>
        public override string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    // Is the name unique?
                    bool isUnique = LoadedTextures.All(assetFromList => assetFromList == this || assetFromList.Name != value);
                    if (isUnique)
                    {
                        name = value;
                        LoadedTextures.Sort(CompareAssets);
                    }
                    // If not then we add one to its name and find out if is unique.
                    else
                        Name = NamePlusOne(value);
                }
            }
        } // Name

        /// <summary>
        /// XNA Texture.
        /// </summary>
        public virtual Texture2D Resource
        { 
            get
            {
                if (xnaTexture.IsDisposed)
                    return null;
                return xnaTexture;
            }
            // This is only allowed for videos. Doing something to avoid this “set” is unnecessary and probably will make more complex some classes 
            // just for this special case. Besides, an internal statement elegantly prevents a bad use of this set.
            // Just don’t dispose this texture because the resource is managed by the video.
            internal set 
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                xnaTexture = value;
                Size = new Size(xnaTexture.Width, xnaTexture.Height);
            }
        } // Resource

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

        /// <summary>
        /// Loaded Textures.
        /// </summary>
        public static List<Texture> LoadedTextures { get; private set; }

        /// <summary>
        ///  A list with all texture' filenames on the texture directory, except cube textures and user interface textures.
        /// </summary>
        public static string[] TexturesFilenames { get; private set; }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Empty texture. 
        /// </summary>
        internal Texture()
        {
            Name = "Empty Texture";
            LoadedTextures.Add(this);
            LoadedTextures.Sort(CompareAssets);
        } // Texture

	    /// <summary>
        /// Texture from XNA asset.
        /// </summary>
        public Texture(Texture2D xnaTexture)
        {
            Name = "Texture";
            this.xnaTexture = xnaTexture;
            Size = new Size(xnaTexture.Width, xnaTexture.Height);
            LoadedTextures.Add(this);
            LoadedTextures.Sort(CompareAssets);
        } // Texture

		/// <summary>
		/// Load texture.
		/// </summary>
        /// <param name="filename">The filename must be relative and be a valid file in the textures directory.</param>
        public Texture(string filename)
		{
            Name = filename;
            string fullFilename = ContentManager.GameDataDirectory + "Textures\\" + filename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load texture: File " + fullFilename + " does not exists!", "filename");
            }
            try
            {
                xnaTexture = ContentManager.CurrentContentManager.XnaContentManager.Load<Texture2D>(fullFilename);
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
            LoadedTextures.Add(this);
            LoadedTextures.Sort(CompareAssets);
		} // Texture

		#endregion

        #region Static Constructor

        /// <summary>
        /// Search the available textures.
        /// </summary>
        static Texture()
        {
            const string texturesDirectoryPath = ContentManager.GameDataDirectory + "Textures";
            // Search the texture files //
            DirectoryInfo texturesDirectory = new DirectoryInfo(texturesDirectoryPath);
            try
            {
                FileInfo[] texturesFileInformation = texturesDirectory.GetFiles("*.xnb", SearchOption.AllDirectories);
                int count = 0, j = 0;
                // Count the textures, except cube textures and user interface textures.
                for (int i = 0; i < texturesFileInformation.Length; i++)
                {
                    FileInfo songFileInformation = texturesFileInformation[i];
                    if (!songFileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\Skin") &&
                        !songFileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\CubeTextures") &&
                        !songFileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\LookupTables"))
                    {
                        count++;
                    }
                }
                // Create the array of available textures.
                TexturesFilenames = new string[count];
                for (int i = 0; i < texturesFileInformation.Length; i++)
                {
                    FileInfo songFileInformation = texturesFileInformation[i];
                    if (!songFileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\Skin") &&
                        !songFileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\CubeTextures") &&
                        !songFileInformation.DirectoryName.Contains(texturesDirectoryPath + "\\LookupTables"))
                    {
                        // Some textures are in a sub directory, in that case we have to know how is called.
                        string[] splitDirectoryName = songFileInformation.DirectoryName.Split(new[] { texturesDirectoryPath }, StringSplitOptions.None);
                        string subdirectory = "";
                        // If is in a sub directory
                        if (splitDirectoryName[1] != "")
                        {
                            subdirectory = splitDirectoryName[1].Substring(1, splitDirectoryName[1].Length - 1) + "\\"; // We delete the start \ and add another \ to the end.
                        }
                        TexturesFilenames[j] = subdirectory + songFileInformation.Name.Substring(0, songFileInformation.Name.Length - 4);
                        j++;
                    }
                }
            }
            // If there was an error then do nothing.
            catch
            {
                TexturesFilenames = new string[0];
            }
            LoadedTextures = new List<Texture>();
        } // Texture

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            if (Resource != null)
                Resource.Dispose();
            LoadedTextures.Remove(this);
        } // DisposeManagedResources

	    #endregion

    } // Texture
} // XNAFinalEngine.Assets

