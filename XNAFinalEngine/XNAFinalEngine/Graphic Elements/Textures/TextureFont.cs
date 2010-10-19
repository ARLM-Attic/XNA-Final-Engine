/// Fuentes implementadas con texturas (sprite fonts)
/// Este esquema solo usa una fuente. Se puede implementar multiples fuentes utilizando proyectos como el de Ziggyware.
/// El cual tiene un creador de sprites fonts, el cual anexa un xml con las coodenadas UV de los distintos simbolos/letras/numeros.
/// Este esquema se dejara de usar en el proyecto dado que resulta mas util utilizar el esquema de Microsoft.

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
    public class TextureFont : Texture
    {

        #region FontToRender helper class

        /// <summary>
        /// TextureFont to render
        /// </summary>
        internal class FontToRender
        {

            #region Variables

            /// <summary>
            /// X and y position
            /// </summary>
            public int x, y;
            /// <summary>
            /// Text
            /// </summary>
            public string text;
            /// <summary>
            /// Color
            /// </summary>
            public Color color;
            /// <summary>
            /// Scale, usually just 1
            /// </summary>
            public float scale;

            #endregion

            #region Constructor
            /// <summary>
            /// Create font to render
            /// </summary>
            /// <param name="setX">Set x</param>
            /// <param name="setY">Set y</param>
            /// <param name="setText">Set text</param>
            /// <param name="setColor">Set color</param>
            public FontToRender(int setX, int setY, string setText, Color setColor)
            {
                x = setX;
                y = setY;
                text = setText;
                color = setColor;
                scale = 1.0f;
            }

            /// <summary>
            /// Create font to render
            /// </summary>
            /// <param name="setX">Set x</param>
            /// <param name="setY">Set y</param>
            /// <param name="setText">Set text</param>
            /// <param name="setColor">Set color</param>
            public FontToRender(int setX, int setY, string setText, Color setColor,
                float setScale)
            {
                x = setX;
                y = setY;
                text = setText;
                color = setColor;
                scale = setScale;
            }
            #endregion

        } // FontToRender

        #endregion

        #region Constants
        /// <summary>
        /// Game font filename for our bitmap.
        /// </summary>
        const string fontFilename = "Font";

        /// <summary>
        /// Font height
        /// </summary>
        const int fontHeight = 36;

        /// <summary>
        /// Substract this value from the y postion when rendering.
        /// Most letters start below the CharRects, this fixes that issue.
        /// </summary>
        const int subRenderHeight = 5;

        /// <summary>
        /// Char rectangles, goes from space (32) to ~ (126).
        /// Height is not used (always the same), instead we save the actual used width for rendering in the height value!
        /// This are the characters:
        ///  !"#$%&'()*+,-./0123456789:;<=>?@
        /// ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`
        /// abcdefghijklmnopqrstuvwxyz{|}~
        /// Then we also got 4 extra rects for the XBox Buttons: A, B, X, Y
        /// </summary>
        static Rectangle[] CharRects = new Rectangle[126 - 32 + 1]
        {
            new Rectangle(0, 0, 1, 8), // space
            new Rectangle(1, 0, 11, 10),
            new Rectangle(12, 0, 14, 13),
            new Rectangle(26, 0, 20, 18),
            new Rectangle(46, 0, 20, 18),
            new Rectangle(66, 0, 24, 22),
            new Rectangle(90, 0, 25, 23),
            new Rectangle(115, 0, 8, 7),
            new Rectangle(124, 0, 10, 9),
            new Rectangle(136, 0, 10, 9),
            new Rectangle(146, 0, 20, 18),
            new Rectangle(166, 0, 20, 18),
            new Rectangle(186, 0, 10, 8),
            new Rectangle(196, 0, 10, 9),
            new Rectangle(207, 0, 10, 8),
            new Rectangle(217, 0, 18, 16),
            new Rectangle(235, 0, 20, 19),

            new Rectangle(0, 36, 20, 18), // 1
            new Rectangle(20, 36, 20, 18),
            new Rectangle(40, 36, 20, 18),
            new Rectangle(60, 36, 21, 19),
            new Rectangle(81, 36, 20, 18),
            new Rectangle(101, 36, 20, 18),
            new Rectangle(121, 36, 20, 18),
            new Rectangle(141, 36, 20, 18),
            new Rectangle(161, 36, 20, 18), // 9
            new Rectangle(181, 36, 10, 8),
            new Rectangle(191, 36, 10, 8),
            new Rectangle(201, 36, 20, 18),
            new Rectangle(221, 36, 20, 18),

            new Rectangle(0, 72, 20, 18), // >
            new Rectangle(20, 72, 19, 17),
            new Rectangle(39, 72, 26, 24),
            new Rectangle(65, 72, 22, 20),
            new Rectangle(87, 72, 22, 20),
            new Rectangle(109, 72, 22, 20),
            new Rectangle(131, 72, 23, 21),
            new Rectangle(154, 72, 20, 18),
            new Rectangle(174, 72, 19, 17),
            new Rectangle(193, 72, 23, 21),
            new Rectangle(216, 72, 23, 21),
            new Rectangle(239, 72, 11, 10),

            new Rectangle(0, 108, 15, 13), // J
            new Rectangle(15, 108, 22, 20),
            new Rectangle(37, 108, 19, 17),
            new Rectangle(56, 108, 29, 26),
            new Rectangle(85, 108, 23, 21),
            new Rectangle(108, 108, 24, 22), // O
            new Rectangle(132, 108, 22, 20),
            new Rectangle(154, 108, 24, 22),
            new Rectangle(178, 108, 24, 22),
            new Rectangle(202, 108, 21, 19),
            new Rectangle(223, 108, 17, 15), // T

            new Rectangle(0, 144, 22, 20), // U
            new Rectangle(22, 144, 22, 20),
            new Rectangle(44, 144, 30, 28),
            new Rectangle(74, 144, 22, 20),
            new Rectangle(96, 144, 20, 18),
            new Rectangle(116, 144, 20, 18),
            new Rectangle(136, 144, 10, 9),
            new Rectangle(146, 144, 18, 16),
            new Rectangle(167, 144, 10, 9),
            new Rectangle(177, 144, 17, 16),
            new Rectangle(194, 144, 17, 16),
            new Rectangle(211, 144, 17, 16),
            new Rectangle(228, 144, 20, 18),

            new Rectangle(0, 180, 20, 18), // b
            new Rectangle(20, 180, 18, 16),
            new Rectangle(38, 180, 20, 18),
            new Rectangle(58, 180, 20, 18), // e
            new Rectangle(79, 180, 14, 12), // f
            new Rectangle(93, 180, 20, 18), // g
            new Rectangle(114, 180, 19, 18), // h
            new Rectangle(133, 180, 11, 10),
            new Rectangle(145, 180, 11, 10), // j
            new Rectangle(156, 180, 20, 18),
            new Rectangle(176, 180, 11, 9),
            new Rectangle(187, 180, 29, 27),
            new Rectangle(216, 180, 20, 18),
            new Rectangle(236, 180, 20, 19),

            new Rectangle(0, 216, 20, 18), // p
            new Rectangle(20, 216, 20, 18),
            new Rectangle(40, 216, 13, 12), // r
            new Rectangle(53, 216, 17, 16),
            new Rectangle(70, 216, 14, 11), // t
            new Rectangle(84, 216, 19, 18),
            new Rectangle(104, 216, 17, 16),
            new Rectangle(122, 216, 25, 23),
            new Rectangle(148, 216, 19, 17),
            new Rectangle(168, 216, 18, 16),
            new Rectangle(186, 216, 16, 15),
            new Rectangle(203, 216, 10, 9),
            new Rectangle(214, 216, 12, 11), // |
            new Rectangle(227, 216, 10, 9),
            new Rectangle(237, 216, 18, 17),
        };
        #endregion

        #region Properties

        /// <summary>
        /// Height. Calcula la altura considerando la resolucion actual
        /// </summary>
        public static int FontHeight
        {
            get
            {
                return YToRes1050(fontHeight - subRenderHeight);
            }
        }

        /// <summary>
        /// XTo res 1400
        /// </summary>
        protected static int XToRes1400(int xIn1400px)
        {
            return (int)Math.Round(xIn1400px * DeviceManager.Width / 1400.0f);
        }

        /// <summary>
        /// YTo res 1200
        /// </summary>
        protected static int YToRes1050(int yIn1050px)
        {
            return (int)Math.Round(yIn1050px * DeviceManager.Height / 1050.0f);
        }

        #endregion
        
        #region Constructor

        public TextureFont() : base(fontFilename)
        {

        } // Font

        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            // TODO    
        }

        #endregion

        #region Get text width
        /// <summary>
        /// Get the text width of a given text.
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Width (in pixels) of the text</returns>
        public static int GetTextWidth(string text)
        {
            int width = 0;
            //foreach (char c in text)
            char[] chars = text.ToCharArray();
            for (int num = 0; num < chars.Length; num++)
            {
                int charNum = (int)chars[num];
                if (charNum >= 32 && charNum - 32 < CharRects.Length)
                    width += XToRes1400(CharRects[charNum - 32].Height);
            }
            return width;
        }
        #endregion

        #region Write methods

        /// <summary>
        /// Remember font texts to render to render them all at once
        /// in our Render method (beween rest of the ui and the mouse cursor).
        /// </summary>
        static List<FontToRender> remTexts = new List<FontToRender>();

        /// <summary>
        /// Sprite batch for rendering
        /// </summary>
        static SpriteBatch spriteBatch = null;

        /// <summary>
        /// Write the given text at the specified position.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="text">Text</param>
        /// <param name="color">Color</param>
        public static void WriteText(int x, int y, string text, Color color)
        {
            remTexts.Add(new FontToRender(x, y, text, color));
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="text">Text</param>
        public static void WriteText(int x, int y, string text)
        {
            remTexts.Add(new FontToRender(x, y, text, Color.White));
        }

        /// <summary>
        /// Write text centered
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="text">Text</param>
        public static void WriteTextCentered(int x, int y, string text)
        {
            WriteText(x - GetTextWidth(text) / 2, y - FontHeight / 2, text);
        }

        /// <summary>
        /// Write text centered with scale factor
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="text">Text</param>
        /// <param name="color">Color</param>
        public static void WriteTextCentered(int x, int y, string text, Color color, float scale)
        {
            int width = GetTextWidth(text);
            remTexts.Add(new FontToRender(x - (int)Math.Round(width * scale / 2), y - (int)Math.Round(FontHeight * scale / 2), text, color, scale));
        }

        /// <summary>
        /// Write time
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="timeMilliseconds">Time Milliseconds</param>
        /// <param name="col">Color</param>
        public static void WriteTime(int x, int y, int timeMilliseconds, Color col)
        {
            WriteText(x, y,
                // negative?
                (timeMilliseconds < 0 ? "-" : "") +
                // Minutes
                ((Math.Abs(timeMilliseconds) / 1000) / 60) + ":" +
                // Seconds
                ((Math.Abs(timeMilliseconds) / 1000) % 60).ToString("00") + "." +
                // Milliseconds
                ((Math.Abs(timeMilliseconds) / 10) % 100).ToString("00"),
                col);
        }
        #endregion

        #region Draw Text
        /// <summary>
        /// Draw all the text
        /// </summary>
        public void DrawText()
        {
            if (remTexts.Count == 0)
                return;

            // Create sprite batch if we have not done it yet.
            // Use device from texture to create the sprite batch.
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(DeviceManager.Device);

            // Start rendering
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            // Draw each character in the text
            //foreach (UIRenderer.FontToRender fontText in texts)
            for (int textNum = 0; textNum < remTexts.Count; textNum++)
            {
                FontToRender fontText = remTexts[textNum];

                int x = fontText.x;
                int y = fontText.y;
                Color color = fontText.color;
                //foreach (char c in fontText.text)
                char[] chars = fontText.text.ToCharArray();
                for (int num = 0; num < chars.Length; num++)
                {
                    int charNum = (int)chars[num];
                    if (charNum >= 32 &&
                        charNum - 32 < CharRects.Length)
                    {
                        // Draw this char
                        Rectangle rect = CharRects[charNum - 32];
                        // Reduce height to prevent overlapping pixels
                        rect.Y += 1;
                        rect.Height = fontHeight;
                        Rectangle destRect = new Rectangle(x, y - YToRes1050(subRenderHeight), rect.Width, rect.Height);

                        // Scale destRect (1600x1200 is the base size)
                        destRect.Width = XToRes1400( (int)Math.Round(destRect.Width * fontText.scale));
                        destRect.Height = YToRes1050( (int)Math.Round(destRect.Height * fontText.scale));

                        // Since we want upscaling, we use the modified destRect
                        spriteBatch.Draw(XnaTexture, destRect, rect, color);

                        // Increase x pos by width we use for this character
                        int charWidth = CharRects[charNum - 32].Height;
                        x += XToRes1400( (int)Math.Round(charWidth * fontText.scale));
                    }
                }
            }

            // End rendering
            spriteBatch.End();

            remTexts.Clear();

            DeviceManager.Device.RenderState.DepthBufferEnable = true;
            DeviceManager.Device.RenderState.AlphaBlendEnable = false;
            DeviceManager.Device.RenderState.AlphaTestEnable = false;
        }
        #endregion

    } // Font
} // XNAFinalEngine.GraphicElements
