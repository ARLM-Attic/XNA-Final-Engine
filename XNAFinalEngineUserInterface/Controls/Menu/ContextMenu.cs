
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
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Context Menu. (right click over control)
    /// </summary>
    public class ContextMenu : MenuBase
    {

        #region Variables

        private long timer;
        private Control sender;

        #endregion

        #region Properties

        protected internal Control Sender { get { return sender; } set { sender = value; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Context Menu. (right click over control)
        /// </summary>
        public ContextMenu()
        {
            Visible = false;
            Detached = true;
            StayOnBack = true;

            UserInterfaceManager.InputSystem.MouseDown += InputMouseDown;
        } // ContextMenu

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            UserInterfaceManager.InputSystem.MouseDown -= InputMouseDown;
        } // DisposeManagedResources

        #endregion

        #region Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinInformation = new SkinControl(Skin.Controls["ContextMenu"]);
        } // InitSkin

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            base.DrawControl(rect);

            SkinLayer l1 = SkinInformation.Layers["Control"];
            SkinLayer l2 = SkinInformation.Layers["Selection"];

            int vsize = LineHeight();
            Color col;

            for (int i = 0; i < Items.Count; i++)
            {
                int mod = i > 0 ? 2 : 0;
                int left = rect.Left + l1.ContentMargins.Left + vsize;
                int h = vsize - mod - (i < (Items.Count - 1) ? 1 : 0);
                int top = rect.Top + l1.ContentMargins.Top + (i * vsize) + mod;


                if (Items[i].Separated && i > 0)
                {
                    Rectangle r = new Rectangle(left, rect.Top + l1.ContentMargins.Top + (i * vsize), LineWidth() - vsize + 4, 1);
                    Renderer.Draw(Skin.Controls["Control"].Layers[0].Image.Texture.XnaTexture, r, l1.Text.Colors.Enabled);
                }
                if (ItemIndex != i)
                {
                    if (Items[i].Enabled)
                    {
                        Rectangle r = new Rectangle(left, top, LineWidth() - vsize, h);
                        Renderer.DrawString(this, l1, Items[i].Text, r, false);
                        col = l1.Text.Colors.Enabled;
                    }
                    else
                    {
                        Rectangle r = new Rectangle(left + l1.Text.OffsetX,
                                                    top + l1.Text.OffsetY,
                                                    LineWidth() - vsize, h);
                        Renderer.DrawString(l1.Text.Font.Font.XnaSpriteFont, Items[i].Text, r, l1.Text.Colors.Disabled, l1.Text.Alignment);
                        col = l1.Text.Colors.Disabled;
                    }
                }
                else
                {
                    if (Items[i].Enabled)
                    {
                        Rectangle rs = new Rectangle(rect.Left + l1.ContentMargins.Left,
                                                     top,
                                                     Width - (l1.ContentMargins.Horizontal - SkinInformation.OriginMargins.Horizontal),
                                                     h);
                        Renderer.DrawLayer(this, l2, rs);

                        Rectangle r = new Rectangle(left,
                                                    top, LineWidth() - vsize, h);

                        Renderer.DrawString(this, l2, Items[i].Text, r, false);
                        col = l2.Text.Colors.Enabled;
                    }
                    else
                    {
                        Rectangle rs = new Rectangle(rect.Left + l1.ContentMargins.Left,
                                                     top,
                                                     Width - (l1.ContentMargins.Horizontal - SkinInformation.OriginMargins.Horizontal),
                                                     vsize);
                        Renderer.DrawLayer(l2, rs, l2.States.Disabled.Color, l2.States.Disabled.Index);

                        Rectangle r = new Rectangle(left + l1.Text.OffsetX,
                                                    top + l1.Text.OffsetY,
                                                    LineWidth() - vsize, h);
                        Renderer.DrawString(l2.Text.Font.Font.XnaSpriteFont, Items[i].Text, r, l2.Text.Colors.Disabled, l2.Text.Alignment);
                        col = l2.Text.Colors.Disabled;
                    }
                }

                if (Items[i].Icon != null)
                {
                    Rectangle r = new Rectangle(rect.Left + l1.ContentMargins.Left + 3, rect.Top + top + 3, LineHeight() - 6, LineHeight() - 6);
                    Renderer.Draw(Items[i].Icon, r, Color.White);
                }

                if (Items[i].ChildrenItems != null && Items[i].ChildrenItems.Count > 0)
                {
                    Renderer.Draw(Skin.Images["Shared.ArrowRight"].Texture.XnaTexture, rect.Left + LineWidth() - 4, rect.Top + l1.ContentMargins.Top + (i * vsize) + 8, col);
                }
            }
        } // DrawControl

        #endregion

        #region Line Height and Width

        private int LineHeight()
        {
            int h = 0;
            if (Items.Count > 0)
            {
                SkinLayer l = SkinInformation.Layers["Control"];
                h = l.Text.Font.Font.LineSpacing + 9;
            }
            return h;
        } // LineHeight

        private int LineWidth()
        {
            int w = 0;
            SkinFont font = SkinInformation.Layers["Control"].Text.Font;
            if (Items.Count > 0)
            {
                foreach (MenuItem t in Items)
                {
                    int wx = (int)font.Font.MeasureString(t.Text).X + 16;
                    if (wx > w) w = wx;
                }
            }
            w += 4 + LineHeight();
            return w;
        } // LineWidth

        #endregion

        #region Auto Size

        /// <summary>
        /// Auto Size
        /// </summary>
        private void AutoSize()
        {
            SkinText font = SkinInformation.Layers["Control"].Text;
            if (Items != null && Items.Count > 0)
            {
                Height = (LineHeight() * Items.Count) + (SkinInformation.Layers["Control"].ContentMargins.Vertical - SkinInformation.OriginMargins.Vertical);
                Width = LineWidth() + (SkinInformation.Layers["Control"].ContentMargins.Horizontal - SkinInformation.OriginMargins.Horizontal) + font.OffsetX;
            }
            else
            {
                Height = 16;
                Width = 16;
            }
        } // AutoSize

        #endregion

        #region Track Item

        private void TrackItem(int y)
        {
            if (Items != null && Items.Count > 0)
            {
                int h = LineHeight();
                y -= SkinInformation.Layers["Control"].ContentMargins.Top;
                int i = (int)((float)y / h);
                if (i < Items.Count)
                {
                    if (i != ItemIndex && Items[i].Enabled)
                    {
                        if (ChildMenu != null)
                        {
                            HideMenu(false);
                        }

                        if (i >= 0 && i != ItemIndex)
                        {
                            Items[i].SelectedInvoke(new EventArgs());
                        }

                        Focused = true;
                        ItemIndex = i;
                        timer = (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMilliseconds;
                    }
                    else if (!Items[i].Enabled && ChildMenu == null)
                    {
                        ItemIndex = -1;
                    }
                }
                Invalidate();
            }
        } // TrackItem

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        protected internal override void Update()
        {
            base.Update();

            AutoSize();

            long time = (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMilliseconds;

            if (timer != 0 && time - timer >= UserInterfaceManager.MenuDelay && ItemIndex >= 0 && Items[ItemIndex].ChildrenItems.Count > 0 && ChildMenu == null)
            {
                OnClick(new MouseEventArgs(new MouseState(), MouseButton.Left, Point.Zero));
            }
        } // Update

        #endregion

        #region OnMouseMove, OnMouseOut, OnClick, OnKeyPress

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            TrackItem(e.Position.Y);
        } // OnMouseMove

        protected override void OnMouseOut(MouseEventArgs e)
        {
            base.OnMouseOut(e);

            if (!CheckArea(e.State.X, e.State.Y) && ChildMenu == null)
            {
                ItemIndex = -1;
            }
        } // OnMouseOut

        protected override void OnClick(EventArgs e)
        {
            if (sender != null && !(sender is MenuBase)) sender.Focused = true;
            base.OnClick(e);
            timer = 0;

            MouseEventArgs ex = (e is MouseEventArgs) ? (MouseEventArgs)e : new MouseEventArgs();

            if (ex.Button == MouseButton.Left || ex.Button == MouseButton.None)
            {
                if (ItemIndex >= 0 && Items[ItemIndex].Enabled)
                {
                    if (ItemIndex >= 0 && Items[ItemIndex].ChildrenItems != null && Items[ItemIndex].ChildrenItems.Count > 0)
                    {
                        if (ChildMenu == null)
                        {
                            ChildMenu = new ContextMenu();
                            (ChildMenu as ContextMenu).RootMenu = RootMenu;
                            (ChildMenu as ContextMenu).ParentMenu = this;
                            (ChildMenu as ContextMenu).sender = sender;
                            ChildMenu.Items.AddRange(Items[ItemIndex].ChildrenItems);
                            (ChildMenu as ContextMenu).AutoSize();
                        }
                        int y = ControlTopAbsoluteCoordinate + SkinInformation.Layers["Control"].ContentMargins.Top + (ItemIndex * LineHeight());
                        ((ContextMenu)ChildMenu).Show(sender, ControlLeftAbsoluteCoordinate + Width - 1, y);
                        if (ex.Button == MouseButton.None) (ChildMenu as ContextMenu).ItemIndex = 0;
                    }
                    else
                    {
                        if (ItemIndex >= 0)
                        {
                            Items[ItemIndex].ClickInvoke(ex);
                        }
                        if (RootMenu is ContextMenu) (RootMenu as ContextMenu).HideMenu(true);
                        else if (RootMenu is MainMenu)
                        {
                            (RootMenu as MainMenu).HideMenu();
                        }
                    }
                }
            }
        } // OnClick

        protected override void OnKeyPress(KeyEventArgs e)
        {
            base.OnKeyPress(e);

            timer = 0;

            if (e.Key == Keys.Down || (e.Key == Keys.Tab && !e.Shift))
            {
                e.Handled = true;
                ItemIndex += 1;
            }

            if (e.Key == Keys.Up || (e.Key == Keys.Tab && e.Shift))
            {
                e.Handled = true;
                ItemIndex -= 1;
            }

            if (ItemIndex > Items.Count - 1) ItemIndex = 0;
            if (ItemIndex < 0) ItemIndex = Items.Count - 1;

            if (e.Key == Keys.Right && Items[ItemIndex].ChildrenItems.Count > 0)
            {
                e.Handled = true;
                OnClick(new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));
            }
            if (e.Key == Keys.Left)
            {
                e.Handled = true;
                if (ParentMenu != null && ParentMenu is ContextMenu)
                {
                    (ParentMenu as ContextMenu).Focused = true;
                    (ParentMenu as ContextMenu).HideMenu(false);
                }
            }
            if (e.Key == Keys.Escape)
            {
                e.Handled = true;
                if (ParentMenu != null) ParentMenu.Focused = true;
                HideMenu(true);
            }
        } // OnKeyPress

        #endregion

        #region Hide Menu

        public virtual void HideMenu(bool hideCurrent)
        {
            if (hideCurrent)
            {
                Visible = false;
                ItemIndex = -1;
            }
            if (ChildMenu != null)
            {
                ((ContextMenu)ChildMenu).HideMenu(true);
                ChildMenu.Dispose();
                ChildMenu = null;
            }
        } // HideMenu

        #endregion

        #region Show

        public override void Show()
        {
            Show(null, Left, Top);
        } // Show

        public virtual void Show(Control sender, int x, int y)
        {
            AutoSize();
            base.Show();
            if (sender != null && sender.Root != null && sender.Root is Container)
            {
                (sender.Root as Container).Add(this, false);
            }
            else
            {
                UserInterfaceManager.Add(this);
            }

            this.sender = sender;

            if (sender != null && sender.Root != null && sender.Root is Container)
            {
                Left = x - Root.ControlLeftAbsoluteCoordinate;
                Top = y - Root.ControlTopAbsoluteCoordinate;
            }
            else
            {
                Left = x;
                Top = y;
            }

            if (ControlLeftAbsoluteCoordinate + Width > SystemInformation.ScreenWidth)
            {
                Left = Left - Width;
                if (ParentMenu != null && ParentMenu is ContextMenu)
                {
                    Left = Left - ParentMenu.Width + 2;
                }
                else if (ParentMenu != null)
                {
                    Left = SystemInformation.ScreenWidth - (Parent != null ? Parent.ControlLeftAbsoluteCoordinate : 0) - Width - 2;
                }
            }
            if (ControlTopAbsoluteCoordinate + Height > SystemInformation.ScreenHeight)
            {
                Top = Top - Height;
                if (ParentMenu != null && ParentMenu is ContextMenu)
                {
                    Top = Top + LineHeight();
                }
                else if (ParentMenu != null)
                {
                    Top = ParentMenu.Top - Height - 1;
                }
            }

            Focused = true;
        } // Show

        #endregion

        #region Input Mouse Down

        private void InputMouseDown(object sender, MouseEventArgs e)
        {
            if ((RootMenu is ContextMenu) && !(RootMenu as ContextMenu).CheckArea(e.Position.X, e.Position.Y) && Visible)
            {
                HideMenu(true);
            }
            else if ((RootMenu is MainMenu) && RootMenu.ChildMenu != null && !((ContextMenu)RootMenu.ChildMenu).CheckArea(e.Position.X, e.Position.Y) && Visible)
            {
                (RootMenu as MainMenu).HideMenu();
            }
        } // InputMouseDown

        #endregion

        #region Check Area

        private bool CheckArea(int x, int y)
        {
            if (Visible)
            {
                if (x <= ControlLeftAbsoluteCoordinate ||
                    x >= ControlLeftAbsoluteCoordinate + Width ||
                    y <= ControlTopAbsoluteCoordinate ||
                    y >= ControlTopAbsoluteCoordinate + Height)
                {
                    bool ret = false;
                    if (ChildMenu != null)
                    {
                        ret = ((ContextMenu)ChildMenu).CheckArea(x, y);
                    }
                    return ret;
                }
                return true;
            }
            return false;
        } // CheckArea

        #endregion

    } // ContextMenu
} // XNAFinalEngine.UserInterface