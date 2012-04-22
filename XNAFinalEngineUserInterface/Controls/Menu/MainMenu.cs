
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Main Menu
    /// </summary>
    public class MainMenu : MenuBase
    {

        #region Variables

        private Rectangle[] rs;
        private int lastIndex = -1;

        #endregion

        #region Constructor

        /// <summary>
        /// Main Menu.
        /// </summary>
        public MainMenu()
        {
            Left = 0;
            Top = 0;
            Height = 24;
            Detached = false;
            DoubleClicks = false;
            StayOnBack = true;
        } // MainMenu

        #endregion

        #region Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinInformation = new SkinControl(Skin.Controls["MainMenu"]);
        } // InitSkin

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            SkinLayer l1 = SkinInformation.Layers["Control"];
            SkinLayer l2 = SkinInformation.Layers["Selection"];
            rs = new Rectangle[Items.Count];

            Renderer.DrawLayer(this, l1, rect, ControlState.Enabled);

            int prev = l1.ContentMargins.Left;
            for (int i = 0; i < Items.Count; i++)
            {
                MenuItem mi = Items[i];

                int tw = (int)l1.Text.Font.Font.MeasureString(mi.Text).X + l1.ContentMargins.Horizontal;
                rs[i] = new Rectangle(rect.Left + prev, rect.Top + l1.ContentMargins.Top, tw, Height - l1.ContentMargins.Vertical);
                prev += tw;

                if (ItemIndex != i)
                {
                    if (mi.Enabled && Enabled)
                    {
                        Renderer.DrawString(this, l1, mi.Text, rs[i], ControlState.Enabled, false);
                    }
                    else
                    {
                        Renderer.DrawString(this, l1, mi.Text, rs[i], ControlState.Disabled, false);
                    }
                }
                else
                {
                    if (Items[i].Enabled && Enabled)
                    {
                        Renderer.DrawLayer(this, l2, rs[i], ControlState.Enabled);
                        Renderer.DrawString(this, l2, mi.Text, rs[i], ControlState.Enabled, false);
                    }
                    else
                    {
                        Renderer.DrawLayer(this, l2, rs[i], ControlState.Disabled);
                        Renderer.DrawString(this, l2, mi.Text, rs[i], ControlState.Disabled, false);
                    }
                }
            }
        } // DrawControl

        #endregion

        #region On Mouse Move

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = lastIndex;

            TrackItem(e.State.X - Root.ControlLeftAbsoluteCoordinate, e.State.Y - Root.ControlTopAbsoluteCoordinate);

            if (ItemIndex >= 0 && (i == -1 || i != ItemIndex) && Items[ItemIndex].ChildrenItems != null && Items[ItemIndex].ChildrenItems.Count > 0 && ChildMenu != null)
            {
                HideSubMenu();
                lastIndex = ItemIndex;
                OnClick(e);
            }
            else if (ChildMenu != null && i != ItemIndex)
            {
                HideSubMenu();
                Focused = true;
            }
        } // OnMouseMove

        private void TrackItem(int x, int y)
        {
            if (Items != null && Items.Count > 0 && rs != null)
            {
                Invalidate();
                for (int i = 0; i < rs.Length; i++)
                {
                    if (rs[i].Contains(x, y))
                    {
                        if (i >= 0 && i != ItemIndex)
                        {
                            Items[i].OnSelected(new EventArgs());
                        }
                        ItemIndex = i;
                        return;
                    }
                }
                if (ChildMenu == null)
                    ItemIndex = -1;
            }
        } // TrackItem

        #endregion

        #region On Mouse Out

        protected override void OnMouseOut(MouseEventArgs e)
        {
            base.OnMouseOut(e);
            OnMouseMove(e);
        } // OnMouseOut

        #endregion

        #region Hide Menu or Sub Menu

        private void HideSubMenu()
        {
            if (ChildMenu != null)
            {
                ((ContextMenu)ChildMenu).HideMenu(true);
                ChildMenu.Dispose();
                ChildMenu = null;
            }
        } // HideSubMenu
        
        public virtual void HideMenu()
        {
            if (ChildMenu != null)
            {
                ((ContextMenu)ChildMenu).HideMenu(true);
                ChildMenu.Dispose();
                ChildMenu = null;
            }
            if (UserInterfaceManager.FocusedControl is MenuBase)
                Focused = true;
            Invalidate();
            ItemIndex = -1;
        } // HideMenu

        #endregion

        #region On Click

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            MouseEventArgs ex = (e is MouseEventArgs) ? (MouseEventArgs)e : new MouseEventArgs();

            if (ex.Button == MouseButton.Left || ex.Button == MouseButton.None)
            {
                if (ItemIndex >= 0 && Items[ItemIndex].Enabled)
                {
                    if (ItemIndex >= 0 && Items[ItemIndex].ChildrenItems != null && Items[ItemIndex].ChildrenItems.Count > 0)
                    {
                        if (ChildMenu != null)
                        {
                            ChildMenu.Dispose();
                            ChildMenu = null;
                        }
                        ChildMenu = new ContextMenu();
                        (ChildMenu as ContextMenu).RootMenu = this;
                        (ChildMenu as ContextMenu).ParentMenu = this;
                        (ChildMenu as ContextMenu).Sender = Root;
                        ChildMenu.Items.AddRange(Items[ItemIndex].ChildrenItems);

                        int y = Root.ControlTopAbsoluteCoordinate + rs[ItemIndex].Bottom + 1;
                        (ChildMenu as ContextMenu).Show(Root, Root.ControlLeftAbsoluteCoordinate + rs[ItemIndex].Left, y);
                        if (ex.Button == MouseButton.None) (ChildMenu as ContextMenu).ItemIndex = 0;
                    }
                    else
                    {
                        if (ItemIndex >= 0)
                        {
                            Items[ItemIndex].OnClick(ex);
                        }
                    }
                }
            }
        } // OnClick

        #endregion

        #region On Key Press

        protected override void OnKeyPress(KeyEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.Key == Keys.Right)
            {
                ItemIndex += 1;
                e.Handled = true;
            }
            if (e.Key == Keys.Left)
            {
                ItemIndex -= 1;
                e.Handled = true;
            }

            if (ItemIndex > Items.Count - 1) ItemIndex = 0;
            if (ItemIndex < 0) ItemIndex = Items.Count - 1;

            if (e.Key == Keys.Down && Items.Count > 0 && Items[ItemIndex].ChildrenItems.Count > 0)
            {
                e.Handled = true;
                OnClick(new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));
            }
            if (e.Key == Keys.Escape)
            {
                e.Handled = true;
                ItemIndex = -1;
            }
        } // OnKeyPress

        #endregion

        #region On Focus Lost and Gained

        /// <summary>
        /// If the control gained focus then...
        /// </summary>
        protected override void OnFocusGained()
        {
            base.OnFocusGained();
            if (ItemIndex < 0 && Items.Count > 0)
                ItemIndex = 0;
        } // OnFocusGained

        /// <summary>
        /// If the control lost focus then...
        /// </summary>
        protected override void OnFocusLost()
        {
            base.OnFocusLost();
            if (ChildMenu == null || !ChildMenu.Visible)
                ItemIndex = -1;
        } // OnFocusLost

        #endregion

    } // MainMenu
} // XNAFinalEngine.UserInterface