
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Context Menu. (right click over control)
    /// </summary>
    /// <remarks>
    /// Internally the context menu is also used to draw the main menu.
    /// </remarks>
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
            StayOnTop = true;

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
            base.DisposeManagedResources();
        } // DisposeManagedResources

        #endregion

        #region Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinInformation = new SkinControlInformation(Skin.Controls["ContextMenu"]);
        } // InitSkin

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            base.DrawControl(rect);

            SkinLayer layerControl   = SkinInformation.Layers["Control"];
            SkinLayer layerSelection = SkinInformation.Layers["Selection"];
            int verticalSize = LineHeight();
            Color color;

            // Find maximum width (not including right side text)
            // This information will be used to render the right side text, if any.
            int maximumWidth = 0;
            foreach (MenuItem menuItem in Items)
            {
                int textWidth = (int)layerControl.Text.Font.Font.MeasureString(menuItem.Text).X + 16;
                if (textWidth > maximumWidth)
                    maximumWidth = textWidth;
            }

            // Render all menu items.
            for (int i = 0; i < Items.Count; i++)
            {
                int mod = i > 0 ? 2 : 0;
                int left = rect.Left + layerControl.ContentMargins.Left + verticalSize;
                int hight = verticalSize - mod - (i < (Items.Count - 1) ? 1 : 0);
                int top = rect.Top + layerControl.ContentMargins.Top + (i * verticalSize) + mod;


                if (Items[i].SeparationLine && i > 0)
                {
                    Rectangle rectangle = new Rectangle(left, rect.Top + layerControl.ContentMargins.Top + (i * verticalSize), LineWidth() - verticalSize + 4, 1);
                    Renderer.Draw(Skin.Controls["Control"].Layers[0].Image.Texture.Resource, rectangle, layerControl.Text.Colors.Enabled);
                }

                #region No Selected
                if (ItemIndex != i)
                {
                    if (Items[i].Enabled)
                    {
                        Rectangle rectangle = new Rectangle(left, top, LineWidth() - verticalSize, hight);
                        // Render Text.
                        Renderer.DrawString(this, layerControl, Items[i].Text, rectangle, false);
                        // Render Right Side Text.
                        if (!string.IsNullOrEmpty(Items[i].RightSideText))
                        {
                            rectangle = new Rectangle(rectangle.Left + maximumWidth, rectangle.Top,
                                                     (int)layerControl.Text.Font.Font.MeasureString(Items[i].RightSideText).X + 16, rectangle.Height);
                            Renderer.DrawString(this, layerControl, Items[i].RightSideText, rectangle, false);
                        }
                        color = layerControl.Text.Colors.Enabled;
                    }
                    else
                    {
                        Rectangle rectangle = new Rectangle(left + layerControl.Text.OffsetX, top + layerControl.Text.OffsetY, LineWidth() - verticalSize, hight);
                        // Render Text.
                        Renderer.DrawString(layerControl.Text.Font.Font.Resource, Items[i].Text, rectangle, layerControl.Text.Colors.Disabled, layerControl.Text.Alignment);
                        // Render Right Side Text.
                        if (!string.IsNullOrEmpty(Items[i].RightSideText))
                        {
                            rectangle = new Rectangle(rectangle.Left + maximumWidth, rectangle.Top,
                                                     (int)layerControl.Text.Font.Font.MeasureString(Items[i].RightSideText).X + 16, rectangle.Height);
                            Renderer.DrawString(layerControl.Text.Font.Font.Resource, Items[i].RightSideText, rectangle, layerControl.Text.Colors.Disabled, layerControl.Text.Alignment);
                        }
                        color = layerControl.Text.Colors.Disabled;
                    }
                }
                #endregion

                #region Selected
                else
                {
                    if (Items[i].Enabled)
                    {
                        Rectangle rs = new Rectangle(rect.Left + layerControl.ContentMargins.Left, 
                                                     top,
                                                     Width - (layerControl.ContentMargins.Horizontal - SkinInformation.OriginMargins.Horizontal),
                                                     hight);
                        Renderer.DrawLayer(this, layerSelection, rs);

                        Rectangle rectangle = new Rectangle(left, top, LineWidth() - verticalSize, hight);
                        // Render String.
                        Renderer.DrawString(this, layerSelection, Items[i].Text, rectangle, false);
                        // Render Right Side Text.
                        if (!string.IsNullOrEmpty(Items[i].RightSideText))
                        {
                            rectangle = new Rectangle(rectangle.Left + maximumWidth, rectangle.Top,
                                                     (int)layerControl.Text.Font.Font.MeasureString(Items[i].RightSideText).X + 16, rectangle.Height);
                            Renderer.DrawString(this, layerSelection, Items[i].RightSideText, rectangle, false);
                        }
                        color = layerSelection.Text.Colors.Enabled;
                    }
                    else
                    {
                        Rectangle rs = new Rectangle(rect.Left + layerControl.ContentMargins.Left,
                                                     top,
                                                     Width - (layerControl.ContentMargins.Horizontal - SkinInformation.OriginMargins.Horizontal),
                                                     verticalSize);
                        Renderer.DrawLayer(layerSelection, rs, layerSelection.States.Disabled.Color, layerSelection.States.Disabled.Index);

                        Rectangle rectangle = new Rectangle(left + layerControl.Text.OffsetX,
                                                    top + layerControl.Text.OffsetY,
                                                    LineWidth() - verticalSize, hight);
                        // Render Text.
                        Renderer.DrawString(layerSelection.Text.Font.Font.Resource, Items[i].Text, rectangle,
                                            layerSelection.Text.Colors.Disabled, layerSelection.Text.Alignment);
                        // Render Right Side Text.
                        if (!string.IsNullOrEmpty(Items[i].RightSideText))
                        {
                            rectangle = new Rectangle(rectangle.Left + maximumWidth, rectangle.Top,
                                                     (int)layerControl.Text.Font.Font.MeasureString(Items[i].RightSideText).X + 16, rectangle.Height);
                            Renderer.DrawString(layerSelection.Text.Font.Font.Resource, Items[i].RightSideText, rectangle,
                                                layerSelection.Text.Colors.Disabled, layerSelection.Text.Alignment);
                        }
                        color = layerSelection.Text.Colors.Disabled;
                    }
                }
                #endregion

                if (Items[i].Icon != null)
                {
                    Rectangle r = new Rectangle(rect.Left + layerControl.ContentMargins.Left + 3, rect.Top + top + 3, LineHeight() - 6, LineHeight() - 6);
                    Renderer.Draw(Items[i].Icon, r, Color.White);
                }

                if (Items[i].Items != null && Items[i].Items.Count > 0)
                {
                    Renderer.Draw(Skin.Images["Shared.ArrowRight"].Texture.Resource, rect.Left + LineWidth() - 4, rect.Top + layerControl.ContentMargins.Top + (i * verticalSize) + 8, color);
                }
            }
        } // DrawControl

        #endregion

        #region Line Height and Width

        private int LineHeight()
        {
            int height = 0;
            if (Items.Count > 0)
            {
                height = SkinInformation.Layers["Control"].Text.Font.Font.LineSpacing + 9;
            }
            return height;
        } // LineHeight

        /// <summary>
        /// Find maximum width from items.
        /// </summary>
        private int LineWidth()
        {
            int maximumWidth = 0;
            int maximumRightSideWidth = 0;
            SkinFont font = SkinInformation.Layers["Control"].Text.Font;
            if (Items.Count > 0)
            {
                foreach (MenuItem item in Items)
                {
                    // Text maximum.
                    int itemWidth = (int)font.Font.MeasureString(item.Text).X + 16;
                    if (itemWidth > maximumWidth) 
                        maximumWidth = itemWidth;
                    // Right side text maximum.
                    int itemRightSideWidth;
                    if (string.IsNullOrEmpty(item.RightSideText))
                        itemRightSideWidth = 0;
                    else
                        itemRightSideWidth = (int)font.Font.MeasureString(item.RightSideText).X + 16;
                    if (itemRightSideWidth > maximumRightSideWidth) 
                        maximumRightSideWidth = itemRightSideWidth;
                }
            }
            maximumWidth += maximumRightSideWidth + 4 + LineHeight();
            return maximumWidth;
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
                            Items[i].OnSelected(new EventArgs());
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

            if (timer != 0 && time - timer >= UserInterfaceManager.MenuDelay && ItemIndex >= 0 && Items[ItemIndex].Items.Count > 0 && ChildMenu == null)
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
                    if (ItemIndex >= 0 && Items[ItemIndex].Items != null && Items[ItemIndex].Items.Count > 0)
                    {
                        if (ChildMenu == null)
                        {
                            ChildMenu = new ContextMenu();
                            (ChildMenu as ContextMenu).RootMenu = RootMenu;
                            (ChildMenu as ContextMenu).ParentMenu = this;
                            (ChildMenu as ContextMenu).sender = sender;
                            ChildMenu.Items.AddRange(Items[ItemIndex].Items);
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
                            Items[ItemIndex].OnClick(ex);
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

            if (e.Key == Keys.Right && Items[ItemIndex].Items.Count > 0)
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
            if (sender != null && sender.Root != null && sender.Root is Window)
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
            
            if (ControlLeftAbsoluteCoordinate + Width > Screen.Width)
            {
                Left = Left - Width;
                if (ParentMenu != null && ParentMenu is ContextMenu)
                {
                    Left = Left - ParentMenu.Width + 2;
                }
                else if (ParentMenu != null)
                {
                    Left = Screen.Width - (Parent != null ? Parent.ControlLeftAbsoluteCoordinate : 0) - Width - 2;
                }
            }
            if (ControlTopAbsoluteCoordinate + Height > Screen.Height)
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