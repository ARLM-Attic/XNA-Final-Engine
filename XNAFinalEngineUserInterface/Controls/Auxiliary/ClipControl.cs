
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

using Microsoft.Xna.Framework;

namespace XNAFinalEngine.UserInterface
{

    public class ClipControl : Control
    {

        #region Properties

        /// <summary>
        /// Gives the upper position inside the control that does not have any child control.
        /// This is useful for placing new controls, but be careful, you have to call it before the parent is set.
        /// </summary>
        public int AvailablePositionInsideControl
        {
            get
            {
                int max = 0;
                foreach (var childrenControl in ClientArea.ChildrenControls)
                {
                    if (childrenControl.Top + childrenControl.Height > max)
                    {
                        max = childrenControl.Top + childrenControl.Height + 1;
                    }
                }
                return max;
            }
        } // AvailablePositionInsideControl
        
        /// <summary>
        /// Client Area.
        /// </summary>
        public virtual ClipBox ClientArea { get; set; }

        /// <summary>
        /// Get and set the control's client margins.
        /// </summary>
        public override Margins ClientMargins
        {
            get { return base.ClientMargins; }
            set
            {
                base.ClientMargins = value;
                if (ClientArea != null)
                {
                    ClientArea.Left   = ClientLeft;
                    ClientArea.Top    = ClientTop;
                    ClientArea.Width  = ClientWidth;
                    ClientArea.Height = ClientHeight;
                }
            }
        } // ClientMargins

        #endregion

        #region Constructor

        public ClipControl()
        {
            ClientArea = new ClipBox
            {
                MinimumWidth = 0,
                MinimumHeight = 0,
                Left = ClientLeft,
                Top = ClientTop,
                Width = ClientWidth,
                Height = ClientHeight
            };

            base.Add(ClientArea);
        } // ClipControl

        #endregion

        #region Add and Remove

        internal virtual void Add(Control control, bool client)
        {
            if (client)
                ClientArea.Add(control);
            else
                base.Add(control);
        } // Add

        internal override void Add(Control control)
        {
            Add(control, true);
        } // Add

        internal override void Remove(Control control)
        {
            base.Remove(control);
            ClientArea.Remove(control);
        } // Remove

        #endregion

        #region On Resize

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            if (ClientArea != null)
            {
                ClientArea.Left = ClientLeft;
                ClientArea.Top = ClientTop;
                ClientArea.Width = ClientWidth;
                ClientArea.Height = ClientHeight;
            }
        } // OnResize

        #endregion

        #region Adjust Height From Children

        /// <summary>
        /// Adjust the height to the available position inside the control.
        /// </summary>
        public virtual void AdjustHeightFromChildren()
        {
            Height = AvailablePositionInsideControl + ClientMargins.Vertical + 5;
        } // AdjustHeightFromChildren

        #endregion

        #region Adjust Margins

        protected virtual void AdjustMargins()
        {
            // Overrite it!!
        } // AdjustMargins

        #endregion

    } // ClipControl
} // XNAFinalEngine.UserInterface
