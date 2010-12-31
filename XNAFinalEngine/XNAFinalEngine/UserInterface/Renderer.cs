
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.UI
{

    /// <summary>
    /// User Interface's elements renderer.
    /// </summary>
    public static class Renderer
    {

        #region Blending Mode

        public enum BlendingMode
        {
            Transparent,
            Opaque,
        } // BlendingMode

        #endregion

        #region Variables

        /// <summary>
        /// User interface sprite batch.
        /// </summary>
        private static SpriteBatch spriteBatch;

        /// <summary>
        /// Device states, used by the sprite batch.
        /// </summary>
        private static BlendState blendState;
        private static RasterizerState rasterizerState;
        private static DepthStencilState depthStencilState;
        private static SamplerState samplerState;

        #endregion

        #region Init
        
        /// <summary>
        /// User Interface's elements renderer.
        /// </summary>
        internal static void Init()
        {
            if (spriteBatch == null)
            {
                spriteBatch = new SpriteBatch(EngineManager.Device);
                SetDeviceStates();
            }
        } // Renderer

        private static void SetDeviceStates()
        {
            blendState = BlendState.AlphaBlend;

            rasterizerState = new RasterizerState
            {
                CullMode = RasterizerState.CullNone.CullMode,
                DepthBias = RasterizerState.CullNone.DepthBias,
                FillMode = RasterizerState.CullNone.FillMode,
                MultiSampleAntiAlias = RasterizerState.CullNone.MultiSampleAntiAlias,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = RasterizerState.CullNone.SlopeScaleDepthBias,
            };

            samplerState = SamplerState.AnisotropicClamp;

            depthStencilState = DepthStencilState.None;

        } // SetDeviceStates

        #endregion

        #region Begin End

        /// <summary>
        /// Begin UI render.
        /// </summary>
        /// <param name="mode">Blending mode, transparent or opaque.</param>
        public static void Begin(BlendingMode mode)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate,
                              mode == BlendingMode.Transparent ? blendState : BlendState.Opaque, 
                              samplerState,
                              depthStencilState,
                              rasterizerState);
        } // Begin

        /// <summary>
        /// End UI Render.
        /// </summary>
        public static void End()
        {
            spriteBatch.End();
        } // End

        #endregion

        #region Draw Texture

        public static void Draw(Texture2D texture, Rectangle destination, Color color)
        {
            if (destination.Width > 0 && destination.Height > 0)
            {
                spriteBatch.Draw(texture, destination, null, color, 0.0f, Vector2.Zero, SpriteEffects.None, UIManager.GlobalDepth);
            }
        } // Draw

        public static void Draw(Texture2D texture, Rectangle destination, Rectangle source, Color color)
        {
            if (source.Width > 0 && source.Height > 0 && destination.Width > 0 && destination.Height > 0)
            {
                spriteBatch.Draw(texture, destination, source, color, 0.0f, Vector2.Zero, SpriteEffects.None, UIManager.GlobalDepth);
            }
        } // Draw

        public static void Draw(Texture2D texture, int left, int top, Color color)
        {
            spriteBatch.Draw(texture, new Vector2(left, top), null, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, UIManager.GlobalDepth);
        } // Draw

        public static void Draw(Texture2D texture, int left, int top, Rectangle source, Color color)
        {
            if (source.Width > 0 && source.Height > 0)
            {
                spriteBatch.Draw(texture, new Vector2(left, top), source, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, UIManager.GlobalDepth);
            }
        } // Draw

        #endregion
        
        #region Draw String

        public static void DrawString(SpriteFont font, string text, int left, int top, Color color)
        {
            spriteBatch.DrawString(font, text, new Vector2(left, top), color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, UIManager.GlobalDepth);
        } // DrawString

        public static void DrawString(SpriteFont font, string text, Rectangle rect, Color color, Alignment alignment)
        {
            DrawString(font, text, rect, color, alignment, 0, 0, true);
        } // DrawString

        public static void DrawString(SpriteFont font, string text, Rectangle rect, Color color, Alignment alignment, bool ellipsis)
        {
            DrawString(font, text, rect, color, alignment, 0, 0, ellipsis);
        } // DrawString

        public static void DrawString(Control control, SkinLayer layer, string text, Rectangle rect)
        {
            DrawString(control, layer, text, rect, true, 0, 0, true);
        } // DrawString

        public static void DrawString(Control control, SkinLayer layer, string text, Rectangle rect, bool margins)
        {
            DrawString(control, layer, text, rect, margins, 0, 0, true);
        } // DrawString

        public static void DrawString(Control control, SkinLayer layer, string text, Rectangle rect, ControlState state, bool margins)
        {
            DrawString(control, layer, text, rect, state, margins, 0, 0, true);
        } // DrawString

        public static void DrawString(Control control, SkinLayer layer, string text, Rectangle rect, bool margins, int ox, int oy)
        {
            DrawString(control, layer, text, rect, margins, ox, oy, true);
        } // DrawString

        public static void DrawString(Control control, SkinLayer layer, string text, Rectangle rect, bool margins, int ox, int oy, bool ellipsis)
        {
            DrawString(control, layer, text, rect, control.ControlState, margins, ox, oy, ellipsis);
        } // DrawString

        public static void DrawString(Control control, SkinLayer layer, string text, Rectangle rect, ControlState state, bool margins, int ox, int oy, bool ellipsis)
        {
            if (layer.Text != null)
            {
                if (margins)
                {
                    Margins m = layer.ContentMargins;
                    rect = new Rectangle(rect.Left + m.Left, rect.Top + m.Top, rect.Width - m.Horizontal, rect.Height - m.Vertical);
                }

                Color col;
                if (state == ControlState.Hovered && (layer.States.Hovered.Index != -1))
                {
                    col = layer.Text.Colors.Hovered;
                }
                else if (state == ControlState.Pressed)
                {
                    col = layer.Text.Colors.Pressed;
                }
                else if (state == ControlState.Focused || (control.Focused && state == ControlState.Hovered && layer.States.Hovered.Index == -1))
                {
                    col = layer.Text.Colors.Focused;
                }
                else if (state == ControlState.Disabled)
                {
                    col = layer.Text.Colors.Disabled;
                }
                else
                {
                    col = layer.Text.Colors.Enabled;
                }

                if (!string.IsNullOrEmpty(text))
                {
                    SkinText font = layer.Text;
                    if (control.TextColor != Control.UndefinedColor && control.ControlState != ControlState.Disabled) col = control.TextColor;
                    DrawString(font.Font.Resource, text, rect, col, font.Alignment, font.OffsetX + ox, font.OffsetY + oy, ellipsis);
                }
            }
        } // DrawString

        public static void DrawString(SpriteFont font, string text, Rectangle rect, Color color, Alignment alignment, int offsetX, int offsetY, bool ellipsis)
        {

            if (ellipsis)
            {
                const string elli = "...";
                int size = (int)Math.Ceiling(font.MeasureString(text).X);
                if (size > rect.Width)
                {
                    int es = (int)Math.Ceiling(font.MeasureString(elli).X);
                    for (int i = text.Length - 1; i > 0; i--)
                    {
                        int c = 1;
                        if (char.IsWhiteSpace(text[i - 1]))
                        {
                            c = 2;
                            i--;
                        }
                        text = text.Remove(i, c);
                        size = (int)Math.Ceiling(font.MeasureString(text).X);
                        if (size + es <= rect.Width)
                        {
                            break;
                        }
                    }
                    text += elli;
                }
            }

            if (rect.Width > 0 && rect.Height > 0)
            {
                Vector2 pos = new Vector2(rect.Left, rect.Top);
                Vector2 size = font.MeasureString(text);

                int x = 0; int y = 0;

                switch (alignment)
                {
                    case Alignment.TopLeft:
                        break;
                    case Alignment.TopCenter:
                        x = GetTextCenter(rect.Width, size.X);
                        break;
                    case Alignment.TopRight:
                        x = rect.Width - (int)size.X;
                        break;
                    case Alignment.MiddleLeft:
                        y = GetTextCenter(rect.Height, size.Y);
                        break;
                    case Alignment.MiddleRight:
                        x = rect.Width - (int)size.X;
                        y = GetTextCenter(rect.Height, size.Y);
                        break;
                    case Alignment.BottomLeft:
                        y = rect.Height - (int)size.Y;
                        break;
                    case Alignment.BottomCenter:
                        x = GetTextCenter(rect.Width, size.X);
                        y = rect.Height - (int)size.Y;
                        break;
                    case Alignment.BottomRight:
                        x = rect.Width - (int)size.X;
                        y = rect.Height - (int)size.Y;
                        break;

                    default:
                        x = GetTextCenter(rect.Width, size.X);
                        y = GetTextCenter(rect.Height, size.Y);
                        break;
                }

                pos.X = (int)(pos.X + x);
                pos.Y = (int)(pos.Y + y);

                DrawString(font, text, (int)pos.X + offsetX, (int)pos.Y + offsetY, color);
            }
        } // DrawString

        private static int GetTextCenter(float size1, float size2)
        {
            return (int)Math.Ceiling((size1 / 2) - (size2 / 2));
        } // GetTextCenter

        #endregion

        #region Draw Layer

        public static void DrawLayer(SkinLayer layer, Rectangle rect, Color color, int index)
        {
            Size imageSize = new Size(layer.Image.Texture.Width, layer.Image.Texture.Height);
            Size partSize = new Size(layer.Width, layer.Height);

            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.TopLeft),      GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.TopLeft, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.TopCenter), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.TopCenter, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.TopRight), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.TopRight, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.MiddleLeft), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.MiddleLeft, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.MiddleCenter), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.MiddleCenter, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.MiddleRight), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.MiddleRight, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.BottomLeft), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.BottomLeft, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.BottomCenter), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.BottomCenter, index), color);
            Draw(layer.Image.Texture.XnaTexture, GetDestinationArea(rect, layer.SizingMargins, Alignment.BottomRight), GetSourceArea(imageSize, partSize, layer.SizingMargins, Alignment.BottomRight, index), color);
        } // DrawLayer

        public static void DrawLayer(Control control, SkinLayer layer, Rectangle rect)
        {
            DrawLayer(control, layer, rect, control.ControlState);
        } // DrawLayer

        public static void DrawLayer(Control control, SkinLayer layer, Rectangle rect, ControlState state)
        {
            Color color;
            Color overlayColor = Color.White;
            int index;
            int overlayIndex = -1;

            if (state == ControlState.Hovered && (layer.States.Hovered.Index != -1))
            {
                color = layer.States.Hovered.Color;
                index = layer.States.Hovered.Index;

                if (layer.States.Hovered.Overlay)
                {
                    overlayColor = layer.Overlays.Hovered.Color;
                    overlayIndex = layer.Overlays.Hovered.Index;
                }
            }
            else if (state == ControlState.Focused || (control.Focused && state == ControlState.Hovered && layer.States.Hovered.Index == -1))
            {
                color = layer.States.Focused.Color;
                index = layer.States.Focused.Index;

                if (layer.States.Focused.Overlay)
                {
                    overlayColor = layer.Overlays.Focused.Color;
                    overlayIndex = layer.Overlays.Focused.Index;
                }
            }
            else if (state == ControlState.Pressed)
            {
                color = layer.States.Pressed.Color;
                index = layer.States.Pressed.Index;

                if (layer.States.Pressed.Overlay)
                {
                    overlayColor = layer.Overlays.Pressed.Color;
                    overlayIndex = layer.Overlays.Pressed.Index;
                }
            }
            else if (state == ControlState.Disabled)
            {
                color = layer.States.Disabled.Color;
                index = layer.States.Disabled.Index;

                if (layer.States.Disabled.Overlay)
                {
                    overlayColor = layer.Overlays.Disabled.Color;
                    overlayIndex = layer.Overlays.Disabled.Index;
                }
            }
            else
            {
                color = layer.States.Enabled.Color;
                index = layer.States.Enabled.Index;

                if (layer.States.Enabled.Overlay)
                {
                    overlayColor = layer.Overlays.Enabled.Color;
                    overlayIndex = layer.Overlays.Enabled.Index;
                }
            }

            if (control.Color != Control.UndefinedColor) color = control.Color * (control.Color.A / 255f);
            DrawLayer(layer, rect, color, index);

            if (overlayIndex != -1)
            {
                DrawLayer(layer, rect, overlayColor, overlayIndex);
            }
        } // DrawLayer

        private static Rectangle GetSourceArea(Size imageSize, Size partSize, Margins margins, Alignment alignment, int index)
        {
            Rectangle rect = new Rectangle();
            int xc = (int)((float)imageSize.Width / partSize.Width);

            int xm = (index) % xc;
            int ym = (index) / xc;

            const int adj = 1;
            margins.Left += margins.Left > 0 ? adj : 0;
            margins.Top += margins.Top > 0 ? adj : 0;
            margins.Right += margins.Right > 0 ? adj : 0;
            margins.Bottom += margins.Bottom > 0 ? adj : 0;

            margins = new Margins(margins.Left, margins.Top, margins.Right, margins.Bottom);
            switch (alignment)
            {
                case Alignment.TopLeft:
                    {
                        rect = new Rectangle((0 + (xm * partSize.Width)),
                                             (0 + (ym * partSize.Height)),
                                             margins.Left,
                                             margins.Top);
                        break;
                    }
                case Alignment.TopCenter:
                    {
                        rect = new Rectangle((0 + (xm * partSize.Width)) + margins.Left,
                                             (0 + (ym * partSize.Height)),
                                             partSize.Width - margins.Left - margins.Right,
                                             margins.Top);
                        break;
                    }
                case Alignment.TopRight:
                    {
                        rect = new Rectangle((partSize.Width + (xm * partSize.Width)) - margins.Right,
                                             (0 + (ym * partSize.Height)),
                                             margins.Right,
                                             margins.Top);
                        break;
                    }
                case Alignment.MiddleLeft:
                    {
                        rect = new Rectangle((0 + (xm * partSize.Width)),
                                             (0 + (ym * partSize.Height)) + margins.Top,
                                             margins.Left,
                                             partSize.Height - margins.Top - margins.Bottom);
                        break;
                    }
                case Alignment.MiddleCenter:
                    {
                        rect = new Rectangle((0 + (xm * partSize.Width)) + margins.Left,
                                             (0 + (ym * partSize.Height)) + margins.Top,
                                             partSize.Width - margins.Left - margins.Right,
                                             partSize.Height - margins.Top - margins.Bottom);
                        break;
                    }
                case Alignment.MiddleRight:
                    {
                        rect = new Rectangle((partSize.Width + (xm * partSize.Width)) - margins.Right,
                                             (0 + (ym * partSize.Height)) + margins.Top,
                                             margins.Right,
                                             partSize.Height - margins.Top - margins.Bottom);
                        break;
                    }
                case Alignment.BottomLeft:
                    {
                        rect = new Rectangle((0 + (xm * partSize.Width)),
                                             (partSize.Height + (ym * partSize.Height)) - margins.Bottom,
                                             margins.Left,
                                             margins.Bottom);
                        break;
                    }
                case Alignment.BottomCenter:
                    {
                        rect = new Rectangle((0 + (xm * partSize.Width)) + margins.Left,
                                             (partSize.Height + (ym * partSize.Height)) - margins.Bottom,
                                             partSize.Width - margins.Left - margins.Right,
                                             margins.Bottom);
                        break;
                    }
                case Alignment.BottomRight:
                    {
                        rect = new Rectangle((partSize.Width + (xm * partSize.Width)) - margins.Right,
                                             (partSize.Height + (ym * partSize.Height)) - margins.Bottom,
                                             margins.Right,
                                             margins.Bottom);
                        break;
                    }
            }

            return rect;
        } // GetSourceArea

        public static Rectangle GetDestinationArea(Rectangle area, Margins margins, Alignment alignment)
        {
            Rectangle rect = new Rectangle();

            int adj = 1;
            margins.Left += margins.Left > 0 ? adj : 0;
            margins.Top += margins.Top > 0 ? adj : 0;
            margins.Right += margins.Right > 0 ? adj : 0;
            margins.Bottom += margins.Bottom > 0 ? adj : 0;

            margins = new Margins(margins.Left, margins.Top, margins.Right, margins.Bottom);

            switch (alignment)
            {
                case Alignment.TopLeft:
                    {
                        rect = new Rectangle(area.Left + 0,
                                             area.Top + 0,
                                             margins.Left,
                                             margins.Top);
                        break;

                    }
                case Alignment.TopCenter:
                    {
                        rect = new Rectangle(area.Left + margins.Left,
                                             area.Top + 0,
                                             area.Width - margins.Left - margins.Right,
                                             margins.Top);
                        break;

                    }
                case Alignment.TopRight:
                    {
                        rect = new Rectangle(area.Left + area.Width - margins.Right,
                                             area.Top + 0,
                                             margins.Right,
                                             margins.Top);
                        break;

                    }
                case Alignment.MiddleLeft:
                    {
                        rect = new Rectangle(area.Left + 0,
                                             area.Top + margins.Top,
                                             margins.Left,
                                             area.Height - margins.Top - margins.Bottom);
                        break;
                    }
                case Alignment.MiddleCenter:
                    {
                        rect = new Rectangle(area.Left + margins.Left,
                                             area.Top + margins.Top,
                                             area.Width - margins.Left - margins.Right,
                                             area.Height - margins.Top - margins.Bottom);
                        break;
                    }
                case Alignment.MiddleRight:
                    {
                        rect = new Rectangle(area.Left + area.Width - margins.Right,
                                             area.Top + margins.Top,
                                             margins.Right,
                                             area.Height - margins.Top - margins.Bottom);
                        break;
                    }
                case Alignment.BottomLeft:
                    {
                        rect = new Rectangle(area.Left + 0,
                                             area.Top + area.Height - margins.Bottom,
                                             margins.Left,
                                             margins.Bottom);
                        break;
                    }
                case Alignment.BottomCenter:
                    {
                        rect = new Rectangle(area.Left + margins.Left,
                                             area.Top + area.Height - margins.Bottom,
                                             area.Width - margins.Left - margins.Right,
                                             margins.Bottom);
                        break;
                    }
                case Alignment.BottomRight:
                    {
                        rect = new Rectangle(area.Left + area.Width - margins.Right,
                                             area.Top + area.Height - margins.Bottom,
                                             margins.Right,
                                             margins.Bottom);
                        break;
                    }
            }

            return rect;
        } // GetDestinationArea

        #endregion

        #region Draw Glyph

        public static void DrawGlyph(Glyph glyph, Rectangle rect)
        {
            Size imageSize = new Size(glyph.Texture.Width, glyph.Texture.Height);

            if (!glyph.SourceRectangle.IsEmpty)
            {
                imageSize = new Size(glyph.SourceRectangle.Width, glyph.SourceRectangle.Height);
            }

            if (glyph.SizeMode == SizeMode.Centered)
            {
                rect = new Rectangle((rect.X + (rect.Width - imageSize.Width) / 2) + glyph.Offset.X,
                                     (rect.Y + (rect.Height - imageSize.Height) / 2) + glyph.Offset.Y,
                                     imageSize.Width,
                                     imageSize.Height);
            }
            else if (glyph.SizeMode == SizeMode.Normal)
            {
                rect = new Rectangle(rect.X + glyph.Offset.X, rect.Y + glyph.Offset.Y, imageSize.Width, imageSize.Height);
            }
            else if (glyph.SizeMode == SizeMode.Auto)
            {
                rect = new Rectangle(rect.X + glyph.Offset.X, rect.Y + glyph.Offset.Y, imageSize.Width, imageSize.Height);
            }

            if (glyph.SourceRectangle.IsEmpty)
            {
                Draw(glyph.Texture.XnaTexture, rect, glyph.Color);
            }
            else
            {
                Draw(glyph.Texture.XnaTexture, rect, glyph.SourceRectangle, glyph.Color);
            }
        } // DrawGlyph

        #endregion

    } // Renderer
} // XNAFinalEngine.UI