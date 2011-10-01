
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Window
    /// </summary>
    public class Window : ModalContainer
    {

        #region Constants

        private const string skinWindow = "Window";
        private const string layerWindow = "Control";
        private const string layerCaption = "Caption";
        private const string layerFrameTop = "FrameTop";
        private const string layerFrameLeft = "FrameLeft";
        private const string layerFrameRight = "FrameRight";
        private const string layerFrameBottom = "FrameBottom";
        private const string layerIcon = "Icon";
        private const string skinButton = "Window.CloseButton";
        private const string layerButton = "Control";
        private const string skinShadow = "Window.Shadow";
        private const string layerShadow = "Control";

        #endregion

        #region Variables

        /// <summary>
        /// Close Button.
        /// </summary>
        private readonly Button buttonClose;

        /// <summary>
        /// Is the close button visible?
        /// </summary>
        private bool closeButtonVisible = true;

        /// <summary>
        /// Is the icon visible?
        /// </summary>
        private bool iconVisible = true;

        /// <summary>
        /// Has shadow?
        /// </summary>
        private bool shadow = true;

        /// <summary>
        /// Is caption visible?
        /// </summary>
        private bool captionVisible = true;

        /// <summary>
        /// Is the window's border visible?
        /// </summary>
        private bool borderVisible = true;

        /// <summary>
        /// The alpha intensity when the window is dragged.
        /// </summary>
        private byte dragAlpha = 200;
        private byte oldAlpha  = 255;

        #endregion

        #region Properties

        /// <summary>
        /// Top-left icon.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// Has shadow?
        /// </summary>
        public virtual bool Shadow
        {
            get { return shadow; }
            set { shadow = value; }
        } // Shadow

        /// <summary>
        /// Is the close button visible?
        /// </summary>
        public virtual bool CloseButtonVisible
        {
            get
            {
                return closeButtonVisible;
            }
            set
            {
                closeButtonVisible = value;
                if (buttonClose != null) 
                    buttonClose.Visible = value;
            }
        } // CloseButtonVisible

        /// <summary>
        /// Is the icon visible?
        /// </summary>
        public virtual bool IconVisible
        {
            get
            {
                return iconVisible;
            }
            set
            {
                iconVisible = value;
            }
        } // IconVisible

        /// <summary>
        /// Is caption visible?
        /// </summary>
        public virtual bool CaptionVisible
        {
            get { return captionVisible; }
            set
            {
                captionVisible = value;
                AdjustMargins();
            }
        } // CaptionVisible

        /// <summary>
        /// Is the window's border visible?
        /// </summary>
        public virtual bool BorderVisible
        {
            get { return borderVisible; }
            set
            {
                borderVisible = value;
                AdjustMargins();
            }
        } // BorderVisible

        /// <summary>
        /// The alpha intensity when the window is dragged.
        /// </summary>
        public virtual byte DragAlpha
        {
            get { return dragAlpha; }
            set { dragAlpha = value; }
        } // DragAlpha

        #endregion

        #region Constructor

        /// <summary>
        /// Window
        /// </summary>
        public Window()
        {
            SetDefaultSize(640, 480);
            SetMinimumSize(100, 75);

            buttonClose = new Button
            {
                SkinControlInformation = new SkinControl(Skin.Controls[skinButton]),
                Detached = true,
                CanFocus = false,
                Text = null,
            };
            buttonClose.Click       += ButtonClose_Click;
            buttonClose.SkinChanged += ButtonClose_SkinChanged;

            AdjustMargins();

            AutoScroll = true;
            Movable = true;
            Resizable = true;
            CenterWindow();

            Add(buttonClose, false);
            
            oldAlpha = Alpha;
        } // Window

        #endregion

        #region Init

        protected internal override void Init()
        {
            base.Init();
            SkinLayer skinLayer = buttonClose.SkinControlInformation.Layers[layerButton];
            buttonClose.Width  = skinLayer.Width  - buttonClose.SkinControlInformation.OriginMargins.Horizontal;
            buttonClose.Height = skinLayer.Height - buttonClose.SkinControlInformation.OriginMargins.Vertical;
            buttonClose.Left   = ControlAndMarginsWidth - SkinControlInformation.OriginMargins.Right - buttonClose.Width + skinLayer.OffsetX;
            buttonClose.Top    = SkinControlInformation.OriginMargins.Top + skinLayer.OffsetY;
            buttonClose.Anchor = Anchors.Top | Anchors.Right;
        } // Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinControlInformation = new SkinControl(Skin.Controls[skinWindow]);
            AdjustMargins();

            CheckLayer(SkinControlInformation, layerWindow);
            CheckLayer(SkinControlInformation, layerCaption);
            CheckLayer(SkinControlInformation, layerFrameTop);
            CheckLayer(SkinControlInformation, layerFrameLeft);
            CheckLayer(SkinControlInformation, layerFrameRight);
            CheckLayer(SkinControlInformation, layerFrameBottom);
            CheckLayer(Skin.Controls[skinButton], layerButton);
            CheckLayer(Skin.Controls[skinShadow], layerShadow);
        } // InitSkin

        #endregion

        #region Events

        /// <summary>
        /// When the button skin changed.
        /// </summary>
        private void ButtonClose_SkinChanged(object sender, EventArgs e)
        {
            buttonClose.SkinControlInformation = new SkinControl(Skin.Controls[skinButton]);
        } // ButtonClose_SkinChanged

        /// <summary>
        /// When a click on the button happen.
        /// </summary>
        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close(ModalResult = ModalResult.Cancel);
        } // ButtonClose_Click

        #endregion

        #region Draw and Render

        /// <summary>
        /// Render the control to the main render target.
        /// </summary>
        internal override void Render()
        {

            #region Shadow

            if (Visible && Shadow)
            {
                SkinControl skinControlShadow = Skin.Controls[skinShadow];
                SkinLayer   skinLayerShadow   = skinControlShadow.Layers[layerShadow];

                Color shadowColor = Color.FromNonPremultiplied(skinLayerShadow.States.Enabled.Color.R, skinLayerShadow.States.Enabled.Color.G, skinLayerShadow.States.Enabled.Color.B, Alpha);

                Renderer.Begin();
                    Renderer.DrawLayer(skinLayerShadow,
                                       new Rectangle(Left - skinControlShadow.OriginMargins.Left, Top - skinControlShadow.OriginMargins.Top, Width + skinControlShadow.OriginMargins.Horizontal, Height + skinControlShadow.OriginMargins.Vertical),
                                       shadowColor, 0);
                Renderer.End();
            }

            #endregion

            base.Render();
        } // Render

        /// <summary>
        /// Get the rectangle that contains the icon.
        /// </summary>
        private Rectangle GetIconRectangle()
        {
            SkinLayer skinLayerCaption = SkinControlInformation.Layers[layerCaption];
            SkinLayer skinLayerIcon    = SkinControlInformation.Layers[layerIcon];

            int iconHeight = skinLayerCaption.Height - skinLayerCaption.ContentMargins.Vertical;
            return new Rectangle(DrawingRectangle.Left + skinLayerCaption.ContentMargins.Left + skinLayerIcon.OffsetX,
                                 DrawingRectangle.Top  + skinLayerCaption.ContentMargins.Top  + skinLayerIcon.OffsetY,
                                 iconHeight, iconHeight);

        } // GetIconRectangle

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            SkinLayer skinLayerFrameTop    = captionVisible ? SkinControlInformation.Layers[layerCaption] : SkinControlInformation.Layers[layerFrameTop];
            SkinLayer skinLayerFrameLeft   = SkinControlInformation.Layers[layerFrameLeft];
            SkinLayer skinLayerFrameRight  = SkinControlInformation.Layers[layerFrameRight];
            SkinLayer skinLayerFrameBottom = SkinControlInformation.Layers[layerFrameBottom];
            SkinLayer skinLayerIcon        = SkinControlInformation.Layers[layerIcon];
            LayerStates layerStateFrameTop, layerStateFrameLeft, layerStateFrameRight, layerStateFrameButtom;
            SpriteFont font = skinLayerFrameTop.Text.Font.Font.XnaSpriteFont;
            Color color;

            if ((Focused || (UserInterfaceManager.FocusedControl != null && UserInterfaceManager.FocusedControl.Root == Root)) && ControlState != ControlState.Disabled)
            {
                layerStateFrameTop = skinLayerFrameTop.States.Focused;
                layerStateFrameLeft = skinLayerFrameLeft.States.Focused;
                layerStateFrameRight = skinLayerFrameRight.States.Focused;
                layerStateFrameButtom = skinLayerFrameBottom.States.Focused;
                color = skinLayerFrameTop.Text.Colors.Focused;
            }
            else if (ControlState == ControlState.Disabled)
            {
                layerStateFrameTop = skinLayerFrameTop.States.Disabled;
                layerStateFrameLeft = skinLayerFrameLeft.States.Disabled;
                layerStateFrameRight = skinLayerFrameRight.States.Disabled;
                layerStateFrameButtom = skinLayerFrameBottom.States.Disabled;
                color = skinLayerFrameTop.Text.Colors.Disabled;
            }
            else
            {
                layerStateFrameTop = skinLayerFrameTop.States.Enabled;
                layerStateFrameLeft = skinLayerFrameLeft.States.Enabled;
                layerStateFrameRight = skinLayerFrameRight.States.Enabled;
                layerStateFrameButtom = skinLayerFrameBottom.States.Enabled;
                color = skinLayerFrameTop.Text.Colors.Enabled;
            }
            // Render Background plane
            Renderer.DrawLayer(SkinControlInformation.Layers[layerWindow], rect, SkinControlInformation.Layers[layerWindow].States.Enabled.Color, SkinControlInformation.Layers[layerWindow].States.Enabled.Index);
            // Render border
            if (borderVisible)
            {
                Renderer.DrawLayer(skinLayerFrameTop, new Rectangle(rect.Left, rect.Top, rect.Width, skinLayerFrameTop.Height), layerStateFrameTop.Color, layerStateFrameTop.Index);
                Renderer.DrawLayer(skinLayerFrameLeft, new Rectangle(rect.Left, rect.Top + skinLayerFrameTop.Height, skinLayerFrameLeft.Width, rect.Height - skinLayerFrameTop.Height - skinLayerFrameBottom.Height), layerStateFrameLeft.Color, layerStateFrameLeft.Index);
                Renderer.DrawLayer(skinLayerFrameRight, new Rectangle(rect.Right - skinLayerFrameRight.Width, rect.Top + skinLayerFrameTop.Height, skinLayerFrameRight.Width, rect.Height - skinLayerFrameTop.Height - skinLayerFrameBottom.Height), layerStateFrameRight.Color, layerStateFrameRight.Index);
                Renderer.DrawLayer(skinLayerFrameBottom, new Rectangle(rect.Left, rect.Bottom - skinLayerFrameBottom.Height, rect.Width, skinLayerFrameBottom.Height), layerStateFrameButtom.Color, layerStateFrameButtom.Index);

                if (iconVisible && (Icon != null || skinLayerIcon != null) && captionVisible)
                {
                    Texture2D i = Icon ?? skinLayerIcon.Image.Texture.XnaTexture;
                    Renderer.Draw(i, GetIconRectangle(), Color.White);
                }

                int icosize = 0;
                if (skinLayerIcon != null && iconVisible && captionVisible)
                {
                    icosize = skinLayerFrameTop.Height - skinLayerFrameTop.ContentMargins.Vertical + 4 + skinLayerIcon.OffsetX;
                }
                int closesize = 0;
                if (buttonClose.Visible)
                {
                    closesize = buttonClose.Width - (buttonClose.SkinControlInformation.Layers[layerButton].OffsetX);
                }

                Rectangle r = new Rectangle(rect.Left + skinLayerFrameTop.ContentMargins.Left + icosize,
                                            rect.Top + skinLayerFrameTop.ContentMargins.Top,
                                            rect.Width - skinLayerFrameTop.ContentMargins.Horizontal - closesize - icosize,
                                            skinLayerFrameTop.Height - skinLayerFrameTop.ContentMargins.Top - skinLayerFrameTop.ContentMargins.Bottom);
                int ox = skinLayerFrameTop.Text.OffsetX;
                int oy = skinLayerFrameTop.Text.OffsetY;
                Renderer.DrawString(font, Text, r, color, skinLayerFrameTop.Text.Alignment, ox, oy, true);
            }
        } // DrawControl

        #endregion

        #region Center Window

        /// <summary>
        /// Center of the main window.
        /// </summary>
        public virtual void CenterWindow()
        {
            Left = (SystemInformation.ScreenWidth / 2) - (Width / 2);
            Top = (SystemInformation.ScreenHeight - Height) / 2;
        } // Center

        #endregion

        #region On Resize, On Move Begin, On Move End, On Double Click

        protected override void OnResize(ResizeEventArgs e)
        {
            SetMovableArea();
            base.OnResize(e);
        } // OnResize

        protected override void OnMoveBegin(EventArgs e)
        {
            base.OnMoveBegin(e);
            oldAlpha = Alpha;
            Alpha = dragAlpha;
        } // OnMoveBegin

        protected override void OnMoveEnd(EventArgs e)
        {
            base.OnMoveEnd(e);
            Alpha = oldAlpha;
        } // OnMoveEnd

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            MouseEventArgs ex = (e is MouseEventArgs) ? (MouseEventArgs)e : new MouseEventArgs();

            if (IconVisible && ex.Button == MouseButton.Left)
            {
                Rectangle r = GetIconRectangle();
                r.Offset(ControlLeftAbsoluteCoordinate, ControlTopAbsoluteCoordinate);
                if (r.Contains(ex.Position))
                {
                    Close();
                }
            }
        } // OnDoubleClick

        #endregion

        #region Adjust Margins

        protected override void AdjustMargins()
        {

            if (captionVisible && borderVisible)
            {
                ClientMargins = new Margins(SkinControlInformation.ClientMargins.Left,  SkinControlInformation.Layers[layerCaption].Height, 
                                            SkinControlInformation.ClientMargins.Right, SkinControlInformation.ClientMargins.Bottom);
            }
            else if (!captionVisible && borderVisible)
            {
                ClientMargins = new Margins(SkinControlInformation.ClientMargins.Left,  SkinControlInformation.ClientMargins.Top,
                                            SkinControlInformation.ClientMargins.Right, SkinControlInformation.ClientMargins.Bottom);
            }
            else if (!borderVisible)
            {
                ClientMargins = new Margins(0, 0, 0, 0);
            }

            if (buttonClose != null)
            {
                buttonClose.Visible = closeButtonVisible && captionVisible && borderVisible;
            }

            SetMovableArea();

            base.AdjustMargins();
        } // AdjustMargins

        #endregion

        #region Set Movable Area

        private void SetMovableArea()
        {
            if (captionVisible && borderVisible)
            {
                MovableArea = new Rectangle(SkinControlInformation.OriginMargins.Left, SkinControlInformation.OriginMargins.Top, Width, SkinControlInformation.Layers[layerCaption].Height - SkinControlInformation.OriginMargins.Top);
            }
            else if (!captionVisible)
            {
                MovableArea = new Rectangle(0, 0, Width, Height);
            }
        } // SetMovableArea

        #endregion

    } // Window
} // XNAFinalEngine.UI