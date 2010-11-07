
#region License
/*

 Based in the class textures.cs from RacingGame.
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.GraphicElements
{

	/// <summary>
	/// Base class for textures.
	/// </summary>
    public class Texture
    {

        #region Variables
                
        /// <summary>
        /// Texture's file name.
        /// </summary>
        protected string textureFilename;

        /// <summary>
        /// XNA Texture.
        /// </summary>
        protected Microsoft.Xna.Framework.Graphics.Texture2D internalXnaTexture = null;

        /// <summary>
        /// Texture's size.
        /// </summary>
        protected int textureWidth, textureHeight;

        /// <summary>
        /// Size of half a pixel, will be calculated when size is set.
        /// </summary>
        private Vector2 precaledHalfPixelSize = Vector2.Zero;

        #endregion

        #region Properties

        /// <summary>
        /// Texture's file name.
        /// </summary>
        public string TextureFilename { get { return textureFilename; } }

        /// <summary>
        /// XNA Texture.
        /// </summary>
        public virtual Microsoft.Xna.Framework.Graphics.Texture2D XnaTexture
        { 
            get { return internalXnaTexture; }
            set
            {
                internalXnaTexture = value; 
                textureWidth = value.Width;
                textureHeight = value.Height;
                CalculateHalfPixelSize();
            }
        }

        /// <summary>
        /// Texture's width.
        /// </summary>
        public int Width { get { return textureWidth; } }

        /// <summary>
        /// Texture's height.
        /// </summary>
        public int Height { get { return textureHeight; } }

        /// <summary>
        /// Rectangle that starts in 0, 0 and finish in the width and height of the texture. 
        /// </summary>
        public Rectangle TextureRectangle { get { return new Rectangle(0, 0, textureWidth, textureHeight); } }

        /// <summary>
        /// Get the size of half a pixel, used to correct texture coordinates when rendering on screen (see Texture.RenderOnScreen).
        /// </summary>
        public Vector2 HalfPixelSize { get { return precaledHalfPixelSize; } }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Dummy texture.
        /// </summary>
        public Texture()
        {
            textureFilename = "";
            internalXnaTexture = null;
        } // Texture()

		/// <summary>
		/// Create texture from given filename.
		/// </summary>
		/// <param name="setFilename">Set filename, must be relative and be a valid file in the textures directory.</param>
        public Texture(string _textureFilename)
		{
            textureFilename = _textureFilename;
            string fullFilename = Directories.TexturesDirectory + "\\" + textureFilename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load texture: File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                if (EngineManager.UsesSystemContent)
                    internalXnaTexture = EngineManager.SystemContent.Load<Texture2D>(fullFilename);
                else
                    internalXnaTexture = EngineManager.CurrentContent.Load<Texture2D>(fullFilename);

                // Get info from the texture directly.
                textureWidth = internalXnaTexture.Width;
                textureHeight = internalXnaTexture.Height;
                CalculateHalfPixelSize();
            } // try
            catch (Exception)
            {
                throw new Exception("Failed to load texture: " + textureFilename);
            }
		} // Texture

		#endregion

        #region Half Pixel Size
                        
        /// <summary>
        /// Calculate half pixel size.
        /// </summary>
        protected void CalculateHalfPixelSize()
        {
            precaledHalfPixelSize = new Vector2((1.0f / (float)textureWidth)  / 2.0f,
                                                (1.0f / (float)textureHeight) / 2.0f);
        } // CalculateHalfPixelSize

        #endregion

        #region Render on screen
        
        /// <summary>
        /// Render the texture on part of the screen.
        /// </summary>
        /// <param name="screenRectangle">Cut the screen</param>
        /// <param name="textureRectangle">Cut the texture</param>
        public virtual void RenderOnScreen(Rectangle screenRectangle, Rectangle textureRectangle)
        {
            SpriteManager.AddSprite(this, screenRectangle, textureRectangle, new Color(255, 255, 255, 255));
        } // RenderOnScreen(screenRectangle, textureRectangle)

        /// <summary>
        /// Render the texture on part of the screen
        /// </summary>
        /// <param name="screenRectangle">Cut the screen</param>
        public virtual void RenderOnScreen(Rectangle screenRectangle)
        {
            RenderOnScreen(screenRectangle, TextureRectangle);
        } // RenderOnScreen(screenRectangle)

        /// <summary>
        /// Render the texture on the screen using the whole texture.
        /// </summary>        
        public virtual void RenderOnFullScreen()
        {
            RenderOnScreen(new Rectangle(0, 0, EngineManager.Width, EngineManager.Height), TextureRectangle);
        } // RenderOnFullScreen

        #endregion

    } // Texture
} // XNAFinalEngine.GraphicElements

