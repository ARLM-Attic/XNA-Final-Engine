
namespace XNAFinalEngine.UI
{

    #region Enumerators
              
    public enum BevelStyle
    {
        None,
        Flat,
        Etched,
        Bumped,
        Lowered,
        Raised
    } //BevelStyle

    public enum BevelBorder
    {
        None,
        Left,
        Top,
        Right,
        Bottom,
        All
    } // BevelBorder

    #endregion

    public class Bevel : Control
    {

        #region Variables
              
        private BevelBorder border = BevelBorder.All;
        private BevelStyle style = BevelStyle.Etched;

        #endregion

        #region Properties

        /// <summary>
        /// Border (top, left, etc.)
        /// </summary>
        public BevelBorder Border
        {
            get { return border; }
            set
            {
                if (border != value)
                {
                    border = value;
                    if (!Suspended) OnBorderChanged(new EventArgs());
                }
            }
        } // Border

        /// <summary>
        /// Border Style
        /// </summary>
        public BevelStyle Style
        {
            get { return style; }
            set
            {
                if (style != value)
                {
                    style = value;
                    if (!Suspended) OnStyleChanged(new EventArgs());
                }
            }
        } // Style

        #endregion

        #region Events
                
        public event EventHandler BorderChanged;
        public event EventHandler StyleChanged;

        #endregion

        #region Construstors
      
        /// <summary>
        /// Bevel.
        /// </summary>
        public Bevel()
        {
            CanFocus = false;
            Passive = true;
            Width = 64;
            Height = 64;
        } // Bevel

        #endregion

        #region On changes
        
        protected virtual void OnBorderChanged(EventArgs e)
        {
            if (BorderChanged != null) BorderChanged.Invoke(this, e);
        } // OnBorderChanged
      
        protected virtual void OnStyleChanged(EventArgs e)
        {
            if (StyleChanged != null) StyleChanged.Invoke(this, e);
        } // OnStyleChanged

        #endregion

    } // Bevel
} // XNAFinalEngine.UI