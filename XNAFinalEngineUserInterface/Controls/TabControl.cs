
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.UserInterface
{

    public class TabPage : ClipControl
    {

        #region Variables
              
        private Rectangle headerRectangle = Rectangle.Empty;

        #endregion

        #region Properties
                    
        protected internal Rectangle HeaderRectangle { get { return headerRectangle; } }

        #endregion

        #region Constructor

        public TabPage()
        {
            Color = Color.Transparent;
            Passive = true;
            CanFocus = false;
        } // TabPage

        #endregion

        #region Calculate Rectangle

        protected internal void CalculateRectangle(Rectangle prev, SpriteFont font, Margins margins, Point offset, bool first)
        {
            int size = (int)Math.Ceiling(font.MeasureString(Text).X) + margins.Horizontal;

            if (first) offset.X = 0;

            headerRectangle = new Rectangle(prev.Right + offset.X, prev.Top, size, prev.Height);
        } // CalculateRectangle

        #endregion

    } // TabPage

    public class TabControl : Container
    {

        #region Variables
              
        private readonly List<TabPage> tabPages = new List<TabPage>();
        private int selectedIndex;
        private int hoveredIndex = -1;

        #endregion

        #region Properties

        public TabPage[] TabPages
        {
            get { return tabPages.ToArray(); }
        } // TabPages

        public virtual int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex >= 0 && selectedIndex < tabPages.Count && value >= 0 && value < tabPages.Count)
                {
                    TabPages[selectedIndex].Visible = false;
                }
                if (value >= 0 && value < tabPages.Count)
                {
                    TabPages[value].Visible = true;
                    ControlsList c = TabPages[value].ChildrenControls as ControlsList;
                    if (c.Count > 0) c[0].Focused = true;
                    selectedIndex = value;
                    if (!Suspended) OnPageChanged(new EventArgs());
                }
            }
        } // SelectedIndex

        public virtual TabPage SelectedPage
        {
            get { return tabPages[SelectedIndex]; }
            set
            {
                for (int i = 0; i < tabPages.Count; i++)
                {
                    if (tabPages[i] == value)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }
        } // SelectedPage

        #endregion

        #region Events

        public event EventHandler PageChanged;

        #endregion

        #region Constructor

        public TabControl()
        {
            CanFocus = false;
        } // TabControl

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            SkinLayer l1 = SkinInformation.Layers["Control"];
            SkinLayer l2 = SkinInformation.Layers["Header"];
            Color col = Color != UndefinedColor ? Color : Color.White;

            Rectangle r1 = new Rectangle(rect.Left, rect.Top + l1.OffsetY, rect.Width, rect.Height - l1.OffsetY);
            if (tabPages.Count <= 0)
            {
                r1 = rect;
            }

            base.DrawControl(r1);

            if (tabPages.Count > 0)
            {

                Rectangle prev = new Rectangle(rect.Left, rect.Top + l2.OffsetY, 0, l2.Height);
                for (int i = 0; i < tabPages.Count; i++)
                {
                    SpriteFont font = l2.Text.Font.Font.Resource;
                    Margins margins = l2.ContentMargins;
                    Point offset = new Point(l2.OffsetX, l2.OffsetY);
                    if (i > 0) prev = tabPages[i - 1].HeaderRectangle;

                    tabPages[i].CalculateRectangle(prev, font, margins, offset, i == 0);
                }

                for (int i = tabPages.Count - 1; i >= 0; i--)
                {
                    int li = tabPages[i].Enabled ? l2.States.Enabled.Index : l2.States.Disabled.Index;
                    Color lc = tabPages[i].Enabled ? l2.Text.Colors.Enabled : l2.Text.Colors.Disabled;
                    if (i == hoveredIndex)
                    {
                        li = l2.States.Hovered.Index;
                        lc = l2.Text.Colors.Hovered;
                    }


                    Margins m = l2.ContentMargins;
                    Rectangle rx = tabPages[i].HeaderRectangle;
                    Rectangle sx = new Rectangle(rx.Left + m.Left, rx.Top + m.Top, rx.Width - m.Horizontal, rx.Height - m.Vertical);
                    if (i != selectedIndex)
                    {
                        Renderer.DrawLayer(l2, rx, col, li);
                        Renderer.DrawString(l2.Text.Font.Font.Resource, tabPages[i].Text, sx, lc, l2.Text.Alignment);
                    }
                }

                Margins mi = l2.ContentMargins;
                Rectangle ri = tabPages[selectedIndex].HeaderRectangle;
                Rectangle si = new Rectangle(ri.Left + mi.Left, ri.Top + mi.Top, ri.Width - mi.Horizontal, ri.Height - mi.Vertical);
                Renderer.DrawLayer(l2, ri, col, l2.States.Focused.Index);
                Renderer.DrawString(l2.Text.Font.Font.Resource, tabPages[selectedIndex].Text, si, l2.Text.Colors.Focused, l2.Text.Alignment, l2.Text.OffsetX, l2.Text.OffsetY, false);
            }
        } // DrawControl

        #endregion

        #region Add or Remove Page

        public virtual TabPage AddPage(string text)
        {
            TabPage p = AddPage();
            p.Text = text;

            return p;
        } // AddPage
   
        public virtual TabPage AddPage()
        {
            TabPage page = new TabPage
            {
                Left = 0,
                Top = 0,
                Width = ClientWidth,
                Height = ClientHeight,
                Anchor = Anchors.All,
                Text = "Tab " + (tabPages.Count + 1),
                Visible = false
            };
            Add(page, true);
            tabPages.Add(page);
            tabPages[0].Visible = true;

            return page;
        } // AddPage
   
        public virtual void RemovePage(TabPage page, bool dispose)
        {
            tabPages.Remove(page);
            if (dispose)
            {
                page.Dispose();
            }
            SelectedIndex = 0;
        } // RemovePage
         
        public virtual void RemovePage(TabPage page)
        {
            RemovePage(page, true);
        } // RemovePage

        #endregion

        #region OnMouseDown, OnMouseMove, OnPageChanged

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (tabPages.Count > 1)
            {
                Point p = new Point(e.State.X - Root.ControlLeftAbsoluteCoordinate, e.State.Y - Root.ControlTopAbsoluteCoordinate);
                for (int i = 0; i < tabPages.Count; i++)
                {
                    Rectangle r = tabPages[i].HeaderRectangle;
                    if (p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }
        } // OnMouseDown
   
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (tabPages.Count > 1)
            {
                int index = hoveredIndex;
                Point p = new Point(e.State.X - Root.ControlLeftAbsoluteCoordinate, e.State.Y - Root.ControlTopAbsoluteCoordinate);
                for (int i = 0; i < tabPages.Count; i++)
                {
                    Rectangle r = tabPages[i].HeaderRectangle;
                    if (p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom && tabPages[i].Enabled)
                    {
                        index = i;
                        break;
                    }
                    index = -1;
                }
                if (index != hoveredIndex)
                {
                    hoveredIndex = index;
                    Invalidate();
                }
            }
        } // OnMouseMove

        protected virtual void OnPageChanged(EventArgs e)
        {
            if (PageChanged != null) PageChanged.Invoke(this, e);
        } // OnPageChanged

        #endregion

    } // TabControl
} //  XNAFinalEngine.UserInterface