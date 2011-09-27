
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Display a list of items.
    /// </summary>
    public class ListBox : Control
    {

        #region Variables
        
        private List<object> items = new List<object>();
        private readonly ScrollBar sbVert;
        private readonly ClipBox pane;
        private int itemIndex = -1;
        private bool hotTrack;
        private int itemsCount;
        private bool hideSelection = true;
        
        #endregion

        #region Properties

        public virtual List<object> Items
        {
            get { return items; }
            internal set { items = value; }
        } // Items

        public virtual bool HotTrack
        {
            get { return hotTrack; }
            set
            {
                if (hotTrack != value)
                {
                    hotTrack = value;
                    if (!Suspended) OnHotTrackChanged(new EventArgs());
                }
            }
        } // HotTrack

        public virtual int ItemIndex
        {
            get { return itemIndex; }
            set
            {
                if (value >= 0 && value < items.Count)
                {
                    itemIndex = value;
                }
                else
                {
                    itemIndex = -1;
                }
                ScrollTo(itemIndex);

                if (!Suspended) OnItemIndexChanged(new EventArgs());

            }
        } // ItemIndex

        public virtual bool HideSelection
        {
            get { return hideSelection; }
            set
            {
                if (hideSelection != value)
                {
                    hideSelection = value;
                    Invalidate();
                    if (!Suspended) OnHideSelectionChanged(new EventArgs());
                }
            }
        } // HideSelection

        #endregion

        #region Events

        public event EventHandler HotTrackChanged;
        public event EventHandler ItemIndexChanged;
        public event EventHandler HideSelectionChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Display a list of items.
        /// </summary>
        public ListBox()
        {
            Width = 64;
            Height = 64;
            MinimumHeight = 16;

            sbVert = new ScrollBar(Orientation.Vertical)
            {
                Parent = this, 
                Range = 1, 
                PageSize = 1,
                StepSize = 10
            };
            sbVert.Left = Left + Width - sbVert.Width - SkinControlInformation.Layers["Control"].ContentMargins.Right;
            sbVert.Top = Top + SkinControlInformation.Layers["Control"].ContentMargins.Top;
            sbVert.Height = Height - SkinControlInformation.Layers["Control"].ContentMargins.Vertical;
            sbVert.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;
            
            pane = new ClipBox
            {
                Parent = this,
                Top = SkinControlInformation.Layers["Control"].ContentMargins.Top,
                Left = SkinControlInformation.Layers["Control"].ContentMargins.Left,
                Width = Width - sbVert.Width - SkinControlInformation.Layers["Control"].ContentMargins.Horizontal - 1,
                Height = Height - SkinControlInformation.Layers["Control"].ContentMargins.Vertical,
                Anchor = Anchors.All,
                Passive = true,
                CanFocus = false
            };
            pane.Draw += DrawPane;

            CanFocus = true;
            Passive = false;
        } // ListBox

        #endregion

        #region Auto Height

        public virtual void AutoHeight(int maxItems)
        {
            if (items != null && items.Count < maxItems) maxItems = items.Count;
            if (maxItems < 3)
            {
                sbVert.Visible = false;
                pane.Width = Width - SkinControlInformation.Layers["Control"].ContentMargins.Horizontal - 1;
            }
            else
            {
                pane.Width = Width - sbVert.Width - SkinControlInformation.Layers["Control"].ContentMargins.Horizontal - 1;
                sbVert.Visible = true;
            }

            SkinText font = SkinControlInformation.Layers["Control"].Text;
            if (items != null && items.Count > 0)
            {
                int h = (int)font.Font.Resource.MeasureString(items[0].ToString()).Y;
                Height = (h * maxItems) + (SkinControlInformation.Layers["Control"].ContentMargins.Vertical);// - Skin.OriginMargins.Vertical);
            }
            else
            {
                Height = 32;
            }
        } // AutoHeight

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            sbVert.Invalidate();
            pane.Invalidate();
            base.DrawControl(rect);
        } // DrawControl

        private void DrawPane(object sender, DrawEventArgs e)
        {
            if (items != null && items.Count > 0)
            {
                SkinText font = SkinControlInformation.Layers["Control"].Text;
                SkinLayer sel = SkinControlInformation.Layers["ListBox.Selection"];
                int h = (int)font.Font.Resource.MeasureString(items[0].ToString()).Y;
                int v = (sbVert.Value / 10);
                int p = (sbVert.PageSize / 10);
                int d = (int)(((sbVert.Value % 10) / 10f) * h);
                int c = items.Count;
                int s = itemIndex;

                for (int i = v; i <= v + p + 1; i++)
                {
                    if (i < c)
                    {
                        Renderer.DrawString(this, SkinControlInformation.Layers["Control"], items[i].ToString(), new Rectangle(e.Rectangle.Left, e.Rectangle.Top - d + ((i - v) * h), e.Rectangle.Width, h), false);
                    }
                }
                if (s >= 0 && s < c && (Focused || !hideSelection))
                {
                    int pos = -d + ((s - v) * h);
                    if (pos > -h && pos < (p + 1) * h)
                    {
                        Renderer.DrawLayer(this, sel, new Rectangle(e.Rectangle.Left, e.Rectangle.Top + pos, e.Rectangle.Width, h));
                        Renderer.DrawString(this, sel, items[s].ToString(), new Rectangle(e.Rectangle.Left, e.Rectangle.Top + pos, e.Rectangle.Width, h), false);
                    }
                }
            }
        } // DrawPane

        #endregion

        #region Update

        protected internal override void Update()
        {
            base.Update();

            if (Visible && items != null && items.Count != itemsCount)
            {
                itemsCount = items.Count;
                ItemsChanged();
            }
        } // Update

        #endregion

        #region Track Item

        private void TrackItem(int x, int y)
        {
            if (items != null && items.Count > 0 && (pane.ControlRectangleRelativeToParent.Contains(new Point(x, y))))
            {
                SkinText font = SkinControlInformation.Layers["Control"].Text;
                int h = (int)font.Font.Resource.MeasureString(items[0].ToString()).Y;
                int i = (int)Math.Floor((sbVert.Value / 10f) + ((float)y / h));
                if (i >= 0 && i < Items.Count && i >= (int)Math.Floor((float)sbVert.Value / 10f) && i < (int)Math.Ceiling((float)(sbVert.Value + sbVert.PageSize) / 10f)) ItemIndex = i;
                Focused = true;
            }
        } // TrackItem

        #endregion

        #region Items Changed

        private void ItemsChanged()
        {
            if (items != null && items.Count > 0)
            {
                SkinText font = SkinControlInformation.Layers["Control"].Text;
                int h = (int)font.Font.Resource.MeasureString(items[0].ToString()).Y;

                int sizev = Height - SkinControlInformation.Layers["Control"].ContentMargins.Vertical;
                sbVert.Range = items.Count * 10;
                sbVert.PageSize = (int)Math.Floor((float)sizev * 10 / h);
                Invalidate();
            }
            else if (items == null || items.Count <= 0)
            {
                sbVert.Range = 1;
                sbVert.PageSize = 1;
                Invalidate();
            }
        } // ItemsChanged

        #endregion

        #region Scroll To

        public virtual void ScrollTo(int index)
        {
            ItemsChanged();
            if ((index * 10) < sbVert.Value)
            {
                sbVert.Value = index * 10;
            }
            else if (index >= (int)Math.Floor(((float)sbVert.Value + sbVert.PageSize) / 10f))
            {
                sbVert.Value = ((index + 1) * 10) - sbVert.PageSize;
            }
        } // ScrollTo

        #endregion

        #region OnMouseDown, OnMouseMove, OnKeyPress, OnResize, OnItemIndexChanged, OnHotTrackChanged, OnHideSelectionChanged

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left || e.Button == MouseButton.Right)
            {
                TrackItem(e.Position.X, e.Position.Y);
            }
        } // OnMouseDown

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (hotTrack)
            {
                TrackItem(e.Position.X, e.Position.Y);
            }
        } // OnMouseMove

        protected override void OnKeyPress(KeyEventArgs e)
        {
            if (e.Key == Keys.Down)
            {
                e.Handled = true;
                itemIndex += sbVert.StepSize / 10;
            }
            else if (e.Key == Keys.Up)
            {
                e.Handled = true;
                itemIndex -= sbVert.StepSize / 10;
            }
            else if (e.Key == Keys.PageDown)
            {
                e.Handled = true;
                itemIndex += sbVert.PageSize / 10;
            }
            else if (e.Key == Keys.PageUp)
            {
                e.Handled = true;
                itemIndex -= sbVert.PageSize / 10;
            }
            else if (e.Key == Keys.Home)
            {
                e.Handled = true;
                itemIndex = 0;
            }
            else if (e.Key == Keys.End)
            {
                e.Handled = true;
                itemIndex = items.Count - 1;
            }

            if (itemIndex < 0) itemIndex = 0;
            else if (itemIndex >= Items.Count) itemIndex = Items.Count - 1;

            ItemIndex = itemIndex;

            base.OnKeyPress(e);
        } // OnKeyPress

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            ItemsChanged();
        } // OnResize
        
        protected virtual void OnItemIndexChanged(EventArgs e)
        {
            if (ItemIndexChanged != null) ItemIndexChanged.Invoke(this, e);
        } // OnItemIndexChanged

        protected virtual void OnHotTrackChanged(EventArgs e)
        {
            if (HotTrackChanged != null) HotTrackChanged.Invoke(this, e);
        } // OnHotTrackChanged

        protected virtual void OnHideSelectionChanged(EventArgs e)
        {
            if (HideSelectionChanged != null) HideSelectionChanged.Invoke(this, e);
        } // OnHideSelectionChanged

        #endregion

    } // ListBox
} // XNAFinalEngine.UI
