
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Combo Box
    /// </summary>
    public class ComboBox : TextBox
    {

        #region Variables

        private readonly Button buttonDown;
        private readonly List<object> items = new List<object>();
        private readonly ListBox listCombo;

        /// <summary>
        /// Maximum Number of items showed by time.
        /// </summary>
        private int maxItems = 5;

        /// <summary>
        /// Draw Selection?
        /// </summary>
        private bool drawSelection = true;

        #endregion

        #region Properties

        /// <summary>
        /// Read Only?
        /// </summary>
        public override bool ReadOnly
        {
            get { return base.ReadOnly; }
            set
            {
                base.ReadOnly = value;
                CaretVisible = !value;
                #if (WINDOWS)
                    Cursor = value ? Skin.Cursors["Default"].Resource : Skin.Cursors["Text"].Resource;
                #endif
            }
        } // ReadOnly

        /// <summary>
        /// Draw Selection?
        /// </summary>
        public bool DrawSelection
        {
            get { return drawSelection; }
            set { drawSelection = value; }
        } // DrawSelection

        /// <summary>
        /// Text
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                if (!items.Contains(value))
                {
                    ItemIndex = -1;
                }
            }
        } // Text

        /// <summary>
        /// List of Items.
        /// </summary>
        public virtual List<object> Items
        {
            get { return items; }
        } // Items

        /// <summary>
        /// Maximum Number of items showed by time.
        /// </summary>
        public int MaxItemsShow
        {
            get { return maxItems; }
            set
            {
                if (maxItems != value)
                {
                    maxItems = value;
                    if (!Suspended) OnMaxItemsChanged(new EventArgs());
                }
            }
        } // MaxItems

        /// <summary>
        /// Item index.
        /// </summary>
        public int ItemIndex
        {
            get { return listCombo.ItemIndex; }
            set
            {
                if (listCombo != null)
                {
                    if (value >= 0 && value < items.Count)
                    {
                        listCombo.ItemIndex = value;
                        Text = listCombo.Items[value].ToString();
                    }
                    else
                    {
                        listCombo.ItemIndex = -1;
                    }
                }
                if (!Suspended) OnItemIndexChanged(new EventArgs());
            }
        } // ItemIndex

        #endregion

        #region Events

        public event EventHandler MaxItemsChanged;
        public event EventHandler ItemIndexChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Combo Box
        /// </summary>
        public ComboBox()
        {
            Height = 20;
            Width = 64;
            ReadOnly = true;

            buttonDown = new Button
            {
                SkinControlInformation = new SkinControl(Skin.Controls["ComboBox.Button"]),
                CanFocus = false
            };
            buttonDown.Click += ButtonDownClick;
            Add(buttonDown, false);

            listCombo = new ListBox
            {
                HotTrack = true,
                Detached = true,
                Visible = false,
                Items = items
            };
        } // ComboBox

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            // We added the listbox to another parent than this control, so we dispose it manually
            if (listCombo != null)
            {
                listCombo.Dispose();
            }
            UserInterfaceManager.InputSystem.MouseDown -= InputMouseDown;
        } // DisposeManagedResources

        #endregion

        #region Init

        protected internal override void Init()
        {
            base.Init();

            listCombo.Click += ListComboClick;
            listCombo.FocusLost += ListComboFocusLost;
            UserInterfaceManager.InputSystem.MouseDown += InputMouseDown;

            listCombo.SkinControlInformation = new SkinControl(Skin.Controls["ComboBox.ListBox"]);
            buttonDown.Glyph = new Glyph(Skin.Images["Shared.ArrowDown"].Texture)
            {
                Color =
                    Skin.Controls["ComboBox.Button"].Layers["Control"].Text.Colors.
                    Enabled,
                SizeMode = SizeMode.Centered
            };
        } // Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinControlInformation = new SkinControl(Skin.Controls["ComboBox"]);
            AdjustMargins();
            ReadOnly = ReadOnly; // To init the right cursor
        } // InitSkin

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            base.DrawControl(rect);

            if (ReadOnly && (Focused || listCombo.Focused) && drawSelection)
            {
                SkinLayer lr = SkinControlInformation.Layers[0];
                Rectangle rc = new Rectangle(rect.Left + lr.ContentMargins.Left,
                                             rect.Top + lr.ContentMargins.Top,
                                             Width - lr.ContentMargins.Horizontal - buttonDown.Width,
                                             Height - lr.ContentMargins.Vertical);
                Renderer.Draw(Skin.Images["ListBox.Selection"].Texture.XnaTexture, rc, Color.FromNonPremultiplied(255, 255, 255, 128));
            }
        } // DrawControl

        #endregion

        #region On Resize

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            if (buttonDown != null)
            {
                buttonDown.Width = 16;
                buttonDown.Height = Height - SkinControlInformation.Layers[0].ContentMargins.Vertical;
                buttonDown.Top = SkinControlInformation.Layers[0].ContentMargins.Top;
                buttonDown.Left = Width - buttonDown.Width - 2;
            }
        } // OnResize

        #endregion

        #region List Event

        private void ListComboClick(object sender, EventArgs e)
        {
            MouseEventArgs ex = (e is MouseEventArgs) ? (MouseEventArgs)e : new MouseEventArgs();

            if (ex.Button == MouseButton.Left || ex.Button == MouseButton.None)
            {
                listCombo.Visible = false;
                if (listCombo.ItemIndex >= 0)
                {
                    Text = listCombo.Items[listCombo.ItemIndex].ToString();
                    Focused = true;
                    ItemIndex = listCombo.ItemIndex;
                }
            }
        } // ListComboClick

        void ListComboFocusLost(object sender, EventArgs e)
        {
            Invalidate();
        } // ListComboFocusLost

        #endregion

        #region Button Down Click

        private void ButtonDownClick(object sender, EventArgs e)
        {
            if (items != null && items.Count > 0)
            {
                if (Root != null && Root is Container)
                {
                    (Root as Container).Add(listCombo, false);
                    listCombo.Alpha = Root.Alpha;
                    listCombo.Left = ControlLeftAbsoluteCoordinate - Root.Left;
                    listCombo.Top = ControlTopAbsoluteCoordinate - Root.Top + Height + 1;
                }
                else
                {
                    UserInterfaceManager.Add(listCombo);
                    listCombo.Alpha = Alpha;
                    listCombo.Left = ControlLeftAbsoluteCoordinate;
                    listCombo.Top = ControlTopAbsoluteCoordinate + Height + 1;
                }

                listCombo.AutoHeight(maxItems);
                if (listCombo.ControlTopAbsoluteCoordinate + listCombo.Height > SystemInformation.ScreenHeight)
                {
                    listCombo.Top = listCombo.Top - Height - listCombo.Height - 2;
                }

                listCombo.Visible = !listCombo.Visible;
                listCombo.Focused = true;
                listCombo.Width = Width;
                listCombo.AutoHeight(maxItems);
            }
        } // ButtonDownClick

        #endregion

        #region Input Mouse Down

        private void InputMouseDown(object sender, MouseEventArgs e)
        {
            if (ReadOnly &&
                (e.Position.X >= ControlLeftAbsoluteCoordinate &&
                 e.Position.X <= ControlLeftAbsoluteCoordinate + Width &&
                 e.Position.Y >= ControlTopAbsoluteCoordinate &&
                 e.Position.Y <= ControlTopAbsoluteCoordinate + Height)) return;

            if (listCombo.Visible &&
               (e.Position.X < listCombo.ControlLeftAbsoluteCoordinate ||
                e.Position.X > listCombo.ControlLeftAbsoluteCoordinate + listCombo.Width ||
                e.Position.Y < listCombo.ControlTopAbsoluteCoordinate ||
                e.Position.Y > listCombo.ControlTopAbsoluteCoordinate + listCombo.Height) &&
               (e.Position.X < buttonDown.ControlLeftAbsoluteCoordinate ||
                e.Position.X > buttonDown.ControlLeftAbsoluteCoordinate + buttonDown.Width ||
                e.Position.Y < buttonDown.ControlTopAbsoluteCoordinate ||
                e.Position.Y > buttonDown.ControlTopAbsoluteCoordinate + buttonDown.Height))
            {
                ButtonDownClick(sender, e);
            }
        } // InputMouseDown

        #endregion

        #region OnKeyDown, OnMouseDown, OnMaxItemsChanged, OnItemIndexChanged

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Keys.Down)
            {
                e.Handled = true;
                ButtonDownClick(this, new MouseEventArgs());
            }
            base.OnKeyDown(e);
        } // OnKeyDown

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (ReadOnly && e.Button == MouseButton.Left)
            {
                ButtonDownClick(this, new MouseEventArgs());
            }
        } // OnMouseDown

        protected virtual void OnMaxItemsChanged(EventArgs e)
        {
            if (MaxItemsChanged != null) MaxItemsChanged.Invoke(this, e);
        } // OnMaxItemsChanged

        protected virtual void OnItemIndexChanged(EventArgs e)
        {
            if (ItemIndexChanged != null) ItemIndexChanged.Invoke(this, e);
        } // OnItemIndexChanged

        #endregion

        #region Adjust Margins

        protected override void AdjustMargins()
        {
            base.AdjustMargins();
            ClientMargins = new Margins(ClientMargins.Left, ClientMargins.Top, ClientMargins.Right + 16, ClientMargins.Bottom);
        } // AdjustMargins

        #endregion

    } // ComboBox
} // XNAFinalEngine.UI