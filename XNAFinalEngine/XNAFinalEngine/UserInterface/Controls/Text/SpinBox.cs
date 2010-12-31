
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

namespace XNAFinalEngine.UI
{

    #region Enumerators

    public enum SpinBoxMode
    {
        Range,
        List
    } // SpinBoxMode

    #endregion

    public class SpinBox : TextBox
    {

        #region Variables

        private readonly Button btnUp;
        private readonly Button btnDown;
        private SpinBoxMode mode = SpinBoxMode.List;
        private readonly List<object> items = new List<object>();
        private float value;
        private int rounding = 2;
        private int itemIndex = -1;

        #endregion

        #region Properties

        public new virtual SpinBoxMode Mode
        {
            get { return mode; }
            set { mode = value; }
        } // Mode

        public override bool ReadOnly
        {
            get { return base.ReadOnly; }
            set
            {
                base.ReadOnly = value;
                CaretVisible = !value;
                if (value)
                {
                    Cursor = UIManager.Skin.Cursors["Default"].Resource;
                }
                else
                {
                    Cursor = UIManager.Skin.Cursors["Text"].Resource;
                }
            }
        } // ReadOnly

        public virtual List<object> Items
        {
            get { return items; }
        } // Items

        public float Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    Invalidate();
                }
            }
        } // Value

        public float Minimum { get; set; }

        public float Maximum { get; set; }

        public float Step { get; set; }

        public int ItemIndex
        {
            get { return itemIndex; }
            set
            {
                if (mode == SpinBoxMode.List)
                {
                    itemIndex = value;
                    Text = items[itemIndex].ToString();
                }
            }
        } // ItemIndex

        public int Rounding
        {
            get { return rounding; }
            set
            {
                if (rounding != value)
                {
                    rounding = value;
                    Invalidate();
                }
            }
        } // Rounding

        #endregion

        #region Constructor

        public SpinBox(SpinBoxMode mode)
        {
            Step = 0.25f;
            Maximum = 100;
            Minimum = 0;
            this.mode = mode;
            ReadOnly = true;

            Height = 20;
            Width = 64;

            btnUp = new Button { CanFocus = false };
            btnUp.MousePress += Button_MousePress;
            Add(btnUp, false);

            btnDown = new Button { CanFocus = false };
            btnDown.MousePress += Button_MousePress;
            Add(btnDown, false);
        } // SpinBox

        #endregion

        #region Init

        protected internal override void Init()
        {
            base.Init();

            SkinControl sc = new SkinControl(btnUp.SkinControlInformation);
            sc.Layers["Control"] = new SkinLayer(SkinControlInformation.Layers["Button"]);
            sc.Layers["Button"].Name = "Control";
            btnUp.SkinControlInformation = btnDown.SkinControlInformation = sc;

            btnUp.Glyph = new Glyph(UIManager.Skin.Images["Shared.ArrowUp"].Texture)
            {
                SizeMode = SizeMode.Centered,
                Color = UIManager.Skin.Controls["Button"].Layers["Control"].Text.Colors.Enabled
            };

            btnDown.Glyph = new Glyph(UIManager.Skin.Images["Shared.ArrowDown"].Texture)
            {
                SizeMode = SizeMode.Centered,
                Color = UIManager.Skin.Controls["Button"].Layers["Control"].Text.Colors.Enabled
            };
        } // Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinControlInformation = new SkinControl(UIManager.Skin.Controls["SpinBox"]);
        } // InitSkin

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            base.DrawControl(rect);

            if (ReadOnly && Focused)
            {
                SkinLayer lr = SkinControlInformation.Layers[0];
                Rectangle rc = new Rectangle(rect.Left + lr.ContentMargins.Left,
                                             rect.Top + lr.ContentMargins.Top,
                                             Width - lr.ContentMargins.Horizontal - btnDown.Width - btnUp.Width,
                                             Height - lr.ContentMargins.Vertical);
                Renderer.Draw(UIManager.Skin.Images["ListBox.Selection"].Texture.XnaTexture, rc, Color.FromNonPremultiplied(255, 255, 255, 128));
            }
        } // DrawControl

        #endregion

        #region Shift Index

        private void ShiftIndex(bool direction)
        {
            if (mode == SpinBoxMode.List)
            {
                if (items.Count > 0)
                {
                    if (direction)
                        itemIndex += 1;
                    else
                        itemIndex -= 1;

                    if (itemIndex < 0) 
                        itemIndex = 0;
                    if (itemIndex > items.Count - 1) 
                        itemIndex = itemIndex = items.Count - 1;

                    Text = items[itemIndex].ToString();
                }
            }
            else
            {
                if (direction)
                    value += Step;
                else
                    value -= Step;

                if (value < Minimum) 
                    value = Minimum;
                if (value > Maximum) 
                    value = Maximum;

                Text = value.ToString("n" + rounding);
            }
        } // ShiftIndex

        #endregion

        #region Button Mouse Press

        private void Button_MousePress(object sender, MouseEventArgs e)
        {
            Focused = true;
            if (sender == btnUp) ShiftIndex(true);
            else if (sender == btnDown) ShiftIndex(false);
        } // Button_MousePress

        #endregion

        #region OnResize, OnKeyPress

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            if (btnUp != null)
            {
                btnUp.Width = 16;
                btnUp.Height = Height - SkinControlInformation.Layers["Control"].ContentMargins.Vertical;
                btnUp.Top = SkinControlInformation.Layers["Control"].ContentMargins.Top;
                btnUp.Left = Width - 16 - 2 - 16 - 1;
            }
            if (btnDown != null)
            {
                btnDown.Width = 16;
                btnDown.Height = Height - SkinControlInformation.Layers["Control"].ContentMargins.Vertical;
                btnDown.Top = SkinControlInformation.Layers["Control"].ContentMargins.Top; ;
                btnDown.Left = Width - 16 - 2;
            }
        } // OnResize

        protected override void OnKeyPress(KeyEventArgs e)
        {
            if (e.Key == Keys.Up)
            {
                e.Handled = true;
                ShiftIndex(true);
            }
            else if (e.Key == Keys.Down)
            {
                e.Handled = true;
                ShiftIndex(false);
            }
            base.OnKeyPress(e);
        } // OnKeyPress

        #endregion

    } // SpinBox
} // XNAFinalEngine.UI