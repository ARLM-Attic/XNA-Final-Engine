
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
        /// The count of dummy textures for naming purposes.
        /// </summary>
        private static int nameNumber = 1;
                
        /// <summary>
        /// XNA Texture.
        /// </summary>
        protected Texture2D xnaTexture;

        #endregion

        #region Properties

        /// <summary>
        /// XNA Texture.
        /// </summary>
        public virtual Texture2D Resource
        { 
            get { return xnaTexture; }
            set
            {
                xnaTexture = value; 
                Size = new Size(xnaTexture.Width, xnaTexture.Height);
            }
        } // XnaTexture

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

        #region Constructor

        /// <summary>
        /// Dummy texture.
        /// </summary>
        public Texture()
        {
            Name = "Texture " + nameNumber++;
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
                Size = new Size(xnaTexture.Width, xnaTexture.Height);
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

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Resource.Dispose();
        } // DisposeManagedResources

	    #endregion
        
    } // Texture
} // XNAFinalEngine.Assets

