
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Image Box.
    /// </summary>
    public class ImageBox : Control
    {

        #region Variables

        /// <summary>
        /// Texture.
        /// </summary>
        private Texture texture;

        /// <summary>
        /// Size Mode (Normal, Streched, Centered and Auto).
        /// Auto mode changes the control's width and height to the texture's dimentions.
        /// </summary>
        private SizeMode sizeMode = SizeMode.Normal;

        /// <summary>
        /// Allows to cut the texture.
        /// </summary>
        private Rectangle sourceRectangle = Rectangle.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Texture.
        /// </summary>
        public Texture Texture
        {
            get { return texture; }
            set
            {
                texture = value;
                sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                Invalidate();
                if (!Suspended) OnImageChanged(new EventArgs());
            }
        } // Texture

        /// <summary>
        /// Allows to cut the texture.
        /// </summary>
        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
            set
            {
                if (texture != null)
                {
                    int left   = value.Left;
                    int top    = value.Top;
                    int width  = value.Width;
                    int height = value.Height;

                    if (left < 0) left = 0;
                    if (top < 0)  top = 0;
                    if (width  > texture.Width)  width = texture.Width;
                    if (height > texture.Height) height = texture.Height;
                    if (left + width  > texture.Width)  width  = (texture.Width  - left);
                    if (top  + height > texture.Height) height = (texture.Height - top);

                    sourceRectangle = new Rectangle(left, top, width, height);
                }
                else
                {
                    sourceRectangle = Rectangle.Empty;
                }
                Invalidate();
            }
        } // SourceRectangle

        /// <summary>
        /// Size Mode (Normal, Streched, Centered and Auto).
        /// Auto mode changes the control's width and height to the texture's dimentions.
        /// </summary>
        public SizeMode SizeMode
        {
            get { return sizeMode; }
            set
            {
                if (value == SizeMode.Auto && texture != null)
                {
                    Width  = texture.Width;
                    Height = texture.Height;
                }
                sizeMode = value;
                Invalidate();
                if (!Suspended) OnSizeModeChanged(new EventArgs());
            }
        } // SizeMode

        #endregion

        #region Events

        public event EventHandler ImageChanged;
        public event EventHandler SizeModeChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Image Box.
        /// </summary>
        public ImageBox()
        {
            CanFocus = false;
            Color = Color.White;
        } // ImageBox

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            if (texture != null)
            {
                switch (sizeMode)
                {
                    case SizeMode.Normal:
                    case SizeMode.Auto:
                        Renderer.Draw(texture.XnaTexture, rect.X, rect.Y, sourceRectangle, Color);
                        break;
                    case SizeMode.Stretched:
                        Renderer.Draw(texture.XnaTexture, rect, sourceRectangle, Color);
                        break;
                    case SizeMode.Centered:
                        int x = (rect.Width  / 2) - (texture.Width  / 2);
                        int y = (rect.Height / 2) - (texture.Height / 2);
                        Renderer.Draw(texture.XnaTexture, x, y, sourceRectangle, Color);
                        break;
                }
            }
        } // DrawControl

        #endregion

        #region On Image Changed, On Size Mode Changed

        protected virtual void OnImageChanged(EventArgs e)
        {
            if (ImageChanged != null) ImageChanged.Invoke(this, e);
        } // OnImageChanged

        protected virtual void OnSizeModeChanged(EventArgs e)
        {
            if (SizeModeChanged != null) SizeModeChanged.Invoke(this, e);
        } // OnSizeModeChanged

        #endregion

    } // ImageBox
} // XNAFinalEngine.UI
