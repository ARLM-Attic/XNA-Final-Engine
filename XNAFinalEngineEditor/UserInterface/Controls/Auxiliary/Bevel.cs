
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
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.UserInterface
{

    #region Enumerators
              
    public enum BevelStyle
    {
        None,
        Flat,
        Etched,
        Bumped,
        Lowered,
        Raised
    } //BevelStyle

    public enum BevelBorder
    {
        None,
        Left,
        Top,
        Right,
        Bottom,
        All
    } // BevelBorder

    #endregion

    /// <summary>
    /// Bevel.
    /// </summary>
    public class Bevel : Control
    {

        #region Variables

        /// <summary>
        /// Border (none, top, left, right, bottom, all)
        /// </summary>
        private BevelBorder border = BevelBorder.All;

        /// <summary>
        /// Border Style (None, Flat, Etched, Bumped, Lowered, Raised).
        /// </summary>
        private BevelStyle style = BevelStyle.Etched;

        #endregion

        #region Properties

        /// <summary>
        /// Border (none, top, left, right, bottom, all)
        /// </summary>
        public BevelBorder Border
        {
            get { return border; }
            set
            {
                if (border != value)
                {
                    border = value;
                    if (!Suspended) OnBorderChanged(new EventArgs());
                }
            }
        } // Border

        /// <summary>
        /// Border Style (None, Flat, Etched, Bumped, Lowered, Raised).
        /// </summary>
        public BevelStyle Style
        {
            get { return style; }
            set
            {
                if (style != value)
                {
                    style = value;
                    if (!Suspended) OnStyleChanged(new EventArgs());
                }
            }
        } // Style

        #endregion

        #region Events
                
        public event EventHandler BorderChanged;
        public event EventHandler StyleChanged;

        #endregion

        #region Constructor
      
        /// <summary>
        /// Bevel.
        /// </summary>
        public Bevel()
        {
            CanFocus = false;
            Passive = true;
            Width = 64;
            Height = 64;
        } // Bevel

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            // A disposed object could be still generating events, because it is alive for a time, in a disposed state, but alive nevertheless.
            BorderChanged = null;
            StyleChanged = null;
            base.DisposeManagedResources();
        } // DisposeManagedResources

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            if (Border != BevelBorder.None && Style != BevelStyle.None)
            {
                if (Border != BevelBorder.All)
                {
                    DrawPart(rect, Border, Style, false);
                }
                else
                {
                    DrawPart(rect, BevelBorder.Left, Style, true);
                    DrawPart(rect, BevelBorder.Top, Style, true);
                    DrawPart(rect, BevelBorder.Right, Style, true);
                    DrawPart(rect, BevelBorder.Bottom, Style, true);
                }
            }
        } // DrawControl

        private void DrawPart(Rectangle rect, BevelBorder pos, BevelStyle style, bool all)
        {
            SkinLayer layer = SkinInformation.Layers["Control"];
            Color c1 = Utilities.ParseColor(layer.Attributes["LightColor"].Value);
            Color c2 = Utilities.ParseColor(layer.Attributes["DarkColor"].Value);
            Color c3 = Utilities.ParseColor(layer.Attributes["FlatColor"].Value);

            if (Color != UndefinedColor) c3 = Color;

            Texture texture = SkinInformation.Layers["Control"].Image.Texture;

            int x1 = 0; int y1 = 0; int w1 = 0; int h1 = 0;
            int x2 = 0; int y2 = 0; int w2 = 0; int h2 = 0;

            if (style == BevelStyle.Bumped || style == BevelStyle.Etched)
            {
                if (all && (pos == BevelBorder.Top || pos == BevelBorder.Bottom))
                {
                    rect = new Rectangle(rect.Left + 1, rect.Top, rect.Width - 2, rect.Height);
                }
                else if (all && (pos == BevelBorder.Left))
                {
                    rect = new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height - 1);
                }
                switch (pos)
                {
                    case BevelBorder.Left:
                        {
                            x1 = rect.Left; y1 = rect.Top; w1 = 1; h1 = rect.Height;
                            x2 = x1 + 1; y2 = y1; w2 = w1; h2 = h1;
                            break;
                        }
                    case BevelBorder.Top:
                        {
                            x1 = rect.Left; y1 = rect.Top; w1 = rect.Width; h1 = 1;
                            x2 = x1; y2 = y1 + 1; w2 = w1; h2 = h1;
                            break;
                        }
                    case BevelBorder.Right:
                        {
                            x1 = rect.Left + rect.Width - 2; y1 = rect.Top; w1 = 1; h1 = rect.Height;
                            x2 = x1 + 1; y2 = y1; w2 = w1; h2 = h1;
                            break;
                        }
                    case BevelBorder.Bottom:
                        {
                            x1 = rect.Left; y1 = rect.Top + rect.Height - 2; w1 = rect.Width; h1 = 1;
                            x2 = x1; y2 = y1 + 1; w2 = w1; h2 = h1;
                            break;
                        }
                }
            }
            else
            {
                switch (pos)
                {
                    case BevelBorder.Left:
                        {
                            x1 = rect.Left; y1 = rect.Top; w1 = 1; h1 = rect.Height;
                            break;
                        }
                    case BevelBorder.Top:
                        {
                            x1 = rect.Left; y1 = rect.Top; w1 = rect.Width; h1 = 1;
                            break;
                        }
                    case BevelBorder.Right:
                        {
                            x1 = rect.Left + rect.Width - 1; y1 = rect.Top; w1 = 1; h1 = rect.Height;
                            break;
                        }
                    case BevelBorder.Bottom:
                        {
                            x1 = rect.Left; y1 = rect.Top + rect.Height - 1; w1 = rect.Width; h1 = 1;
                            break;
                        }
                }
            }

            switch (Style)
            {
                case BevelStyle.Bumped:
                    {
                        Renderer.Draw(texture.Resource, new Rectangle(x1, y1, w1, h1), c1);
                        Renderer.Draw(texture.Resource, new Rectangle(x2, y2, w2, h2), c2);
                        break;
                    }
                case BevelStyle.Etched:
                    {
                        Renderer.Draw(texture.Resource, new Rectangle(x1, y1, w1, h1), c2);
                        Renderer.Draw(texture.Resource, new Rectangle(x2, y2, w2, h2), c1);
                        break;
                    }
                case BevelStyle.Raised:
                    {
                        Color c = c1;
                        if (pos == BevelBorder.Left || pos == BevelBorder.Top) c = c1;
                        else c = c2;

                        Renderer.Draw(texture.Resource, new Rectangle(x1, y1, w1, h1), c);
                        break;
                    }
                case BevelStyle.Lowered:
                    {
                        Color c = c1;
                        if (pos == BevelBorder.Left || pos == BevelBorder.Top) c = c2;
                        else c = c1;

                        Renderer.Draw(texture.Resource, new Rectangle(x1, y1, w1, h1), c);
                        break;
                    }
                default:
                    {
                        Renderer.Draw(texture.Resource, new Rectangle(x1, y1, w1, h1), c3);
                        break;
                    }
            }
        } // DrawPart

        #endregion

        #region On Border Changed

        protected virtual void OnBorderChanged(EventArgs e)
        {
            if (BorderChanged != null) BorderChanged.Invoke(this, e);
        } // OnBorderChanged

        #endregion

        #region On Style Changed

        protected virtual void OnStyleChanged(EventArgs e)
        {
            if (StyleChanged != null) StyleChanged.Invoke(this, e);
        } // OnStyleChanged

        #endregion

    } // Bevel
} // XNAFinalEngine.UserInterface