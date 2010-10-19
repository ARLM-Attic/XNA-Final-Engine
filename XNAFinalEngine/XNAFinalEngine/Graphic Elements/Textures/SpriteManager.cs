
#region License
/*

 Based in the class SpriteHelper.cs from RacingGame.
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Sprite helper class to manage and render sprites.
    /// </summary>
    public class SpriteManager
    {        

        #region Variables

        /// <summary>
        /// A list of all sprites we have to render this frame.
        /// </summary>
        static List<SpriteToRender> sprites = new List<SpriteToRender>();

        /// <summary>
        /// Sprite batch for rendering.
        /// </summary>
        static SpriteBatch spriteBatch = null;

        #endregion

        #region AddSprite

        /// <summary>
        /// Add the texture to the list of sprites that will be render in screen.
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="screenRectangle">Cut the screen</param>
        /// <param name="textureRectangle">Cut the texture</param>
        /// <param name="color">Color</param>
        public static void AddSprite(Texture texture, Rectangle screenRectangle, Rectangle textureRectangle, Color color)
        {
            sprites.Add(new SpriteToRender(texture.XnaTexture, screenRectangle, textureRectangle, color));
        } // AddSprite 

        #endregion

        #region DrawSprites

        /// <summary>
        /// Render the sprites  in the list onto part of the screen.
        /// We can flip vertically to fix some webcam problems.
        /// Call SpriteSortMode.BackToFront for sprites like trees or so.
        /// </summary>
        public static void DrawSprites(SpriteSortMode spriteSortMode = SpriteSortMode.Immediate)
        {
            // No need to render if we got no sprites this frame
            if (sprites.Count == 0)
                return;

            // Create sprite batch if we have not done it yet.
            // Use device from texture to create the sprite batch.
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(sprites[0].texture.GraphicsDevice);

            // Start rendering sprites
            spriteBatch.Begin(spriteSortMode, BlendState.AlphaBlend);

            // Render all sprites
            foreach (SpriteToRender sprite in sprites)
            {
                spriteBatch.Draw(sprite.texture, sprite.screenRectangle, sprite.sourceRectangle, sprite.color);
            }

            // We are done, draw everything on screen with help of the end method.
            spriteBatch.End();

            // Kill list of remembered sprites
            sprites.Clear();

        } // DrawSprites

        /// <summary>
        /// Render a single channel texture onto screen.
        /// </summary>
        public static void DrawSingleChannelTexture(Texture2D texture, Rectangle screenRectangle, Rectangle textureRectangle)
        {
            // Create sprite batch if we have not done it yet.
            // Use device from texture to create the sprite batch.
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(sprites[0].texture.GraphicsDevice);

            // Start rendering sprites
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            
            spriteBatch.Draw(texture, screenRectangle, textureRectangle, Color.White);

            // We are done, draw everything on screen with help of the end method.
            spriteBatch.End();

        } // DrawSprites

        #endregion

        #region Clear the list of sprites

        /// <summary>
        /// Clear the list of sprites to render in this frame.
        /// </summary>
        public static void ClearListSprites()
        {
            sprites.Clear();
        } // ClearListSprites

        #endregion

        #region SpriteToRender helper class

        /// <summary>
        /// Auxiliary class that is used in the list of sprite.
        /// </summary>
        private class SpriteToRender
        {
            public Texture2D texture;
            public Rectangle screenRectangle;
            public Rectangle? sourceRectangle;
            public Color color;

            public SpriteToRender(Texture2D _texture, Rectangle _screenRectangle, Rectangle? _sourceRectangle, Color _color)
            {
                texture = _texture;
                screenRectangle = _screenRectangle;
                sourceRectangle = _sourceRectangle;
                color = _color;
            } // SpriteToRender

        } // SpriteToRender

        #endregion

    } // SpriteManager
} // XNAFinalEngine.GraphicElements