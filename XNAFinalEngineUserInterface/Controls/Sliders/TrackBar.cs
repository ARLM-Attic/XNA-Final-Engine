
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
#endregion

namespace XNAFinalEngine.UserInterface
{
    
    #region Enumerator

    public enum ScaleColor
    {
        Red,
        Green,
        Blue,
        /// <summary>
        ///  Default color: Blue gray.
        /// </summary>
        Default,
    } // ScaleColor

    #endregion

    /// <summary>
    /// Track Bar (for sliders)
    /// </summary>
    public class TrackBar : Control
    {

        #region Variables

        /// <summary>
        /// Current value. Ranges always between 0 to 100.
        /// </summary>
        private float internalValue;

        /// <summary>
        /// Step size, this value is expressed in percentages.
        /// </summary>
        private int stepSize = 1;
        
        /// <summary>
        /// /// <summary>
        /// Page size, this value is expressed in percentages.
        /// </summary>
        /// </summary>
        private int pageSize = 5;

        /// <summary>
        /// Scale bar color. Default blue.
        /// </summary>
        private ScaleColor scaleColor = ScaleColor.Default;

        /// <summary>
        /// Draw scale bar?
        /// </summary>
        private bool drawScale = true;

        /// <summary>
        /// Slider button.
        /// </summary>
        private readonly Button buttonSlider;

        /// <summary>
        /// Minimum value that can has the slider.
        /// </summary>
        private float minimumValue;

        /// <summary>
        /// Maximum value that can has the slider.
        /// </summary>
        private float maximumValue = 100;

        #endregion

        #region Properties

        /// <summary>
        /// Minimum value that can has the slider.
        /// </summary>
        public virtual float MinimumValue
        {
            get { return minimumValue; }
            set
            {
                minimumValue = value;
                RecalculateParameters();
                if (!Suspended) OnRangeChanged(new EventArgs());
            }
        } // MinimumValue

        /// <summary>
        /// Maximum value that can has the slider.
        /// </summary>
        public virtual float MaximumValue
        {
            get { return maximumValue; }
            set
            {
                maximumValue = value;
                RecalculateParameters();
                if (!Suspended) OnRangeChanged(new EventArgs());
            }
        } // MaximumValue

        /// <summary>
        /// If out of range rescale. In other words the maximum or minimum value changes.
        /// </summary>
        public virtual bool IfOutOfRangeRescale { get; set; }
        
        /// <summary>
        /// Indicates if the value can be out of range.
        /// </summary>
        public virtual bool ValueCanBeOutOfRange { get; set; }
        
        private float InternalValue
        {
            get { return internalValue; }
            set
            {   
                if (internalValue != value)
                {
                    internalValue = value;
                    if (!ValueCanBeOutOfRange) // Then, we need to check that the internal value is between 0 and 100.
                    {
                        if (internalValue < 0)   internalValue = 0;
                        if (internalValue > 100) internalValue = 100;
                    }
                    else if (IfOutOfRangeRescale) // Is posible that we need to rescale. In other words the maximum or minimum value changes.
                    {
                        float tempCurrentRealValue = CalculateRealValue(internalValue);
                        if (internalValue < 0)
                        {
                            minimumValue = minimumValue - (2 * (minimumValue - CalculateRealValue(internalValue)));
                            internalValue = CalculateInternalValue(tempCurrentRealValue);
                        }
                        if (internalValue > 100)
                        {
                            maximumValue = maximumValue + (2 * (CalculateRealValue(internalValue) - maximumValue));
                            internalValue = CalculateInternalValue(tempCurrentRealValue);
                        }
                    }
                    Invalidate();
                    if (!Suspended)
                        OnValueChanged(new EventArgs());
                }
            }
        } // InternalValue

        /// <summary>
        /// Current value.
        /// </summary>
        public virtual float Value
        {
            get
            {
                return CalculateRealValue(internalValue); 
            }
            set
            {
                InternalValue = CalculateInternalValue(value);
            }
        } // Value
 
        /// <summary>
        /// Page size, this value is expressed in percentages.
        /// </summary>
        public virtual int PageSize
        {
            get { return pageSize; }
            set
            {
                if (pageSize != value)
                {
                    pageSize = value;
                    if (pageSize > 100) pageSize = 100;
                    RecalculateParameters();
                    if (!Suspended) OnPageSizeChanged(new EventArgs());
                }
            }
        } // PageSize
   
        /// <summary>
        /// Step size, this value is expressed in percentages.
        /// </summary>
        public virtual int StepSize
        {
            get { return stepSize; }
            set
            {
                if (stepSize != value)
                {
                    stepSize = value;
                    if (stepSize > 100) stepSize = 100;
                    if (!Suspended) OnStepSizeChanged(new EventArgs());
                }
            }
        } // StepSize

        /// <summary>
        /// Scale bar color.
        /// </summary>
        public virtual ScaleColor ScaleBarColor
        {
            get { return scaleColor; }
            set { scaleColor = value; }
        } // ScaleBarColor

        /// <summary>
        /// Draw scale bar?
        /// </summary>
        public virtual bool DrawScaleBar
        {
            get { return drawScale; }
            set { drawScale = value; }
        } // DrawScale

        #endregion

        #region Events
         
        public event EventHandler ValueChanged;
        public event EventHandler RangeChanged;
        public event EventHandler StepSizeChanged;
        public event EventHandler PageSizeChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Track Bar (for sliders)
        /// </summary>
        public TrackBar()
        {
            Width = 64;
            Height = 20;
            CanFocus = false;

            buttonSlider = new Button
            {
                Text = "",
                CanFocus = false,
                Parent = this,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Bottom,
                Detached = true,
                Movable = true
            };
        } // TrackBar

        #endregion

        #region Init

        protected internal override void Init()
        {
            base.Init();
            buttonSlider.SkinControlInformation = new SkinControl(Skin.Controls["TrackBar.Button"]);
            buttonSlider.Move += ButtonSlider_Move;
            buttonSlider.KeyPress += ButtonSlider_KeyPress;
        } // Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinControlInformation = new SkinControl(Skin.Controls["TrackBar"]);
        } // InitSkin

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            RecalculateParameters();
            
            SkinLayer p = SkinControlInformation.Layers["Control"];
            SkinLayer l = SkinControlInformation.Layers["ScaleOrange"];

            const float ratio = 0.66f;
            int h = (int)(ratio * rect.Height);
            int t = rect.Top + (Height - h) / 2;

            float px = ((float)internalValue / (float)100);
            int w = (int)Math.Ceiling(px * (rect.Width - p.ContentMargins.Horizontal - buttonSlider.Width)) + 2;

            if (w < l.SizingMargins.Vertical) 
                w = l.SizingMargins.Vertical;
            if (w > rect.Width - p.ContentMargins.Horizontal)
                w = rect.Width - p.ContentMargins.Horizontal;

            // Draw control
            base.DrawControl(new Rectangle(rect.Left, t, rect.Width, h));
            // Draw progress line.
            Rectangle r1 = new Rectangle(rect.Left + p.ContentMargins.Left, t + p.ContentMargins.Top, w, h - p.ContentMargins.Vertical);
            if (drawScale)
            {
                switch (scaleColor)
                {
                    case ScaleColor.Red     : Renderer.DrawLayer(this, SkinControlInformation.Layers["ScaleRed"], r1); break;
                    case ScaleColor.Green   : Renderer.DrawLayer(this, SkinControlInformation.Layers["ScaleGreen"], r1); break;
                    case ScaleColor.Blue    : Renderer.DrawLayer(this, SkinControlInformation.Layers["ScaleBlue"], r1); break;
                    case ScaleColor.Default : Renderer.DrawLayer(this, l, r1); break;
                }
            }
        } // DrawControl

        #endregion

        #region Transform Value

        /// <summary>
        /// Calculate real value from an internal value.
        /// </summary>
        private float CalculateRealValue(float value)
        {
            return (value * (maximumValue - minimumValue) / 100f) + minimumValue;
        } // CalculateRealValue

        /// <summary>
        /// Calculate internal value from a real value.
        /// </summary>
        private float CalculateInternalValue(float value)
        {
            return (value - minimumValue) * 100f / (maximumValue - minimumValue);
        } // CalculateInternalValue

        #endregion

        #region Event

        private void ButtonSlider_Move(object sender, MoveEventArgs e)
        {
            SkinLayer p = SkinControlInformation.Layers["Control"];
            int size = buttonSlider.Width;
            int w = Width - p.ContentMargins.Horizontal - size;
            int pos = e.Left;
            
            if (pos < p.ContentMargins.Left) pos = p.ContentMargins.Left;
            if (pos > w + p.ContentMargins.Left) pos = w + p.ContentMargins.Left;

            buttonSlider.SetPosition(pos, 0);
            float px = (float)100 / (float)w;

            // Update value. But in this case the value can't be out of range.
            bool temp = ValueCanBeOutOfRange;
            ValueCanBeOutOfRange = false;
            InternalValue = (pos - p.ContentMargins.Left) * px;
            ValueCanBeOutOfRange = temp;
        } // ButtonSlider_Move

        void ButtonSlider_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Left || e.Key == Keys.Down) InternalValue -= stepSize;
            else if (e.Key == Keys.Right || e.Key == Keys.Up) InternalValue += stepSize;
            else if (e.Key == Keys.PageDown) InternalValue -= pageSize;
            else if (e.Key == Keys.PageUp) InternalValue += pageSize;
            else if (e.Key == Keys.Home) InternalValue = 0;
            else if (e.Key == Keys.End) InternalValue = 100;
        } // ButtonSlider_KeyPress

        #endregion

        #region Recalculate Parameters

        /// <summary>
        /// Recalculate some parameters, like button size, slider position, etc.
        /// </summary>
        private void RecalculateParameters()
        {
            if (buttonSlider != null)
            {
                if (buttonSlider.Width > 12)
                {
                    buttonSlider.Glyph = new Glyph(Skin.Images["Shared.Glyph"].Texture) { SizeMode = SizeMode.Centered };
                }
                else
                {
                    buttonSlider.Glyph = null;
                }

                SkinLayer p = SkinControlInformation.Layers["Control"];
                buttonSlider.Width = (int)(Height * 0.8);
                buttonSlider.Height = Height;
                int size = buttonSlider.Width;
                int w = Width - p.ContentMargins.Horizontal - size;

                float px = (float)100 / (float)w;
                int pos = p.ContentMargins.Left + (int)(Math.Ceiling(internalValue / (float)px));
                
                if (pos < p.ContentMargins.Left) 
                    pos = p.ContentMargins.Left;
                if (pos > w + p.ContentMargins.Left) 
                    pos = w + p.ContentMargins.Left;

                buttonSlider.SetPosition(pos, 0);
            }
        } // RecalculateParameters

        #endregion
        
        #region On Mouse Press, On Resize, On Value Changed, On Range Changed, On Paige Size Changed, On Step Size Changed

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left)
            {
                buttonSlider.Left = e.Position.X - buttonSlider.Width / 2;
                // We try to recreate a mouse click in the button slider. Is not elegant, but it works.
                //XNAFinalEngine.Input.Mouse.MouseState = new MouseState(); // TODO!!!
                //UserInterfaceManager.InputSystem.Update();
            }
        } // OnMousePress

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            RecalculateParameters();
        } // OnResize

        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null) ValueChanged.Invoke(this, e);
        } // OnValueChanged

        protected virtual void OnRangeChanged(EventArgs e)
        {
            if (RangeChanged != null) RangeChanged.Invoke(this, e);
        } // OnRangeChanged

        protected virtual void OnPageSizeChanged(EventArgs e)
        {
            if (PageSizeChanged != null) PageSizeChanged.Invoke(this, e);
        } // OnPageSizeChanged

        protected virtual void OnStepSizeChanged(EventArgs e)
        {
            if (StepSizeChanged != null) StepSizeChanged.Invoke(this, e);
        } // OnStepSizeChanged

        #endregion

    } // TrackBar
} // XNAFinalEngine.UI