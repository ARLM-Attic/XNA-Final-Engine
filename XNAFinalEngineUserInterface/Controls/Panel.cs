
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
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Panel.
    /// </summary>
    public class Panel : Container
    {

        #region Variables

        private readonly Bevel bevel;
        private BevelStyle bevelStyle = BevelStyle.None;
        private BevelBorder bevelBorder = BevelBorder.None;
        private int bevelMargin;
        private Color bevelColor = Color.Black;

        #endregion

        #region Properties

        /// <summary>
        /// Bevel Style.
        /// </summary>
        public BevelStyle BevelStyle
        {
            get { return bevelStyle; }
            set
            {
                if (bevelStyle != value)
                {
                    bevelStyle = bevel.Style = value;
                    AdjustMargins();
                    if (!Suspended) OnBevelStyleChanged(new EventArgs());
                }
            }
        } // BevelStyle

        /// <summary>
        /// Bevel Border
        /// </summary>
        public BevelBorder BevelBorder
        {
            get { return bevelBorder; }
            set
            {
                if (bevelBorder != value)
                {
                    bevelBorder = bevel.Border = value;
                    bevel.Visible = bevelBorder != BevelBorder.None;
                    AdjustMargins();
                    if (!Suspended) OnBevelBorderChanged(new EventArgs());
                }
            }
        } // BevelBorder

        /// <summary>
        /// Bevel Margin
        /// </summary>
        public int BevelMargin
        {
            get { return bevelMargin; }
            set
            {
                if (bevelMargin != value)
                {
                    bevelMargin = value;
                    AdjustMargins();
                    if (!Suspended) OnBevelMarginChanged(new EventArgs());
                }
            }
        } // BevelMargin

        /// <summary>
        /// Bevel Color.
        /// </summary>
        public virtual Color BevelColor
        {
            get { return bevelColor; }
            set
            {
                bevel.Color = bevelColor = value;
            }
        } // BevelColor

        #endregion

        #region Events

        public event EventHandler BevelBorderChanged;
        public event EventHandler BevelStyleChanged;
        public event EventHandler BevelMarginChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Panel.
        /// </summary>
        public Panel()
        {
            Passive = false;
            CanFocus = false;
            Width = 64;
            Height = 64;

            bevel = new Bevel
            {
                Style = bevelStyle,
                Border = bevelBorder,
                Left = 0,
                Top = 0,
                Width = Width,
                Height = Height,
                Color = bevelColor,
                Visible = (bevelBorder != BevelBorder.None),
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right | Anchors.Bottom
            };
            Add(bevel, false);
            AdjustMargins();
        } // Panel

        #endregion

        #region Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinControlInformation = new SkinControl(Skin.Controls["Panel"]);
        } // InitSkin

        #endregion

        #region Adjust Margins

        protected override void AdjustMargins()
        {
            int l = 0;
            int t = 0;
            int r = 0;
            int b = 0;
            int s = bevelMargin;

            if (bevelBorder != BevelBorder.None)
            {
                if (bevelStyle != BevelStyle.Flat)
                    s += 2;
                else
                    s += 1;

                if (bevelBorder == BevelBorder.Left || bevelBorder == BevelBorder.All)
                    l = s;
                if (bevelBorder == BevelBorder.Top || bevelBorder == BevelBorder.All)
                    t = s;
                if (bevelBorder == BevelBorder.Right || bevelBorder == BevelBorder.All)
                    r = s;
                if (bevelBorder == BevelBorder.Bottom || bevelBorder == BevelBorder.All)
                    b = s;
            }
            ClientMargins = new Margins(SkinControlInformation.ClientMargins.Left + l, SkinControlInformation.ClientMargins.Top + t, SkinControlInformation.ClientMargins.Right + r, SkinControlInformation.ClientMargins.Bottom + b);

            base.AdjustMargins();
        } // AdjustMargins

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            int x = rect.Left;
            int y = rect.Top;
            int w = rect.Width;
            int h = rect.Height;
            int s = bevelMargin;

            if (bevelBorder != BevelBorder.None)
            {
                if (bevelStyle != BevelStyle.Flat)
                {
                    s += 2;
                }
                else
                {
                    s += 1;
                }

                if (bevelBorder == BevelBorder.Left || bevelBorder == BevelBorder.All)
                {
                    x += s;
                    w -= s;
                }
                if (bevelBorder == BevelBorder.Top || bevelBorder == BevelBorder.All)
                {
                    y += s;
                    h -= s;
                }
                if (bevelBorder == BevelBorder.Right || bevelBorder == BevelBorder.All)
                {
                    w -= s;
                }
                if (bevelBorder == BevelBorder.Bottom || bevelBorder == BevelBorder.All)
                {
                    h -= s;
                }
            }

            base.DrawControl(new Rectangle(x, y, w, h));
        } // DrawControl

        #endregion

        #region OnBevelBorderChanged, OnBevelStyleChanged, OnBevelMarginChanged

        protected virtual void OnBevelBorderChanged(EventArgs e)
        {
            if (BevelBorderChanged != null) BevelBorderChanged.Invoke(this, e);
        } // OnBevelBorderChanged

        protected virtual void OnBevelStyleChanged(EventArgs e)
        {
            if (BevelStyleChanged != null) BevelStyleChanged.Invoke(this, e);
        } // OnBevelStyleChanged

        protected virtual void OnBevelMarginChanged(EventArgs e)
        {
            if (BevelMarginChanged != null) BevelMarginChanged.Invoke(this, e);
        } // OnBevelMarginChanged

        #endregion

    } // Panel
} // XNAFinalEngine.UI