
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Modal Containter, for now it's only used for the windows.
    /// </summary>
    public abstract class ModalContainer : Container
    {

        #region Variables

        private ModalResult    modalResult = ModalResult.None;
        private ModalContainer lastModal;

        #endregion

        #region Properties

        /// <summary>
        /// Is visible?
        /// </summary>
        public override bool Visible
        {
            get { return base.Visible; }
            set
            {
                if (value) Focused = true;
                base.Visible = value;
            }
        } // Visible

        /// <summary>
        /// Is the active modal?
        /// </summary>
        public virtual bool IsTheActiveModal { get { return UserInterfaceManager.ModalWindow == this; } }
        
        public virtual ModalResult ModalResult
        {
            get { return modalResult; }
            set { modalResult = value; }
        } // ModalResult

        #endregion

        #region Events

        public event WindowClosingEventHandler Closing;
        public event WindowClosedEventHandler  Closed;

        #endregion

        #region Show Modal

        /// <summary>
        /// Show Modal.
        /// </summary>
        public virtual void ShowModal()
        {
            lastModal = UserInterfaceManager.ModalWindow;
            UserInterfaceManager.ModalWindow = this;
            UserInterfaceManager.InputSystem.KeyDown += Input_KeyDown;
        } // ShowModal

        #endregion

        #region Close

        /// <summary>
        /// Close
        /// </summary>
        public virtual void Close()
        {
            WindowClosingEventArgs ex = new WindowClosingEventArgs();
            OnClosing(ex);
            if (!ex.Cancel)
            {
                UserInterfaceManager.InputSystem.KeyDown -= Input_KeyDown;
                UserInterfaceManager.ModalWindow = lastModal;
                if (lastModal != null)
                    lastModal.Focused = true;
                else
                    UserInterfaceManager.FocusedControl = null;
                Hide();
                WindowClosedEventArgs ev = new WindowClosedEventArgs();
                OnClosed(ev);

                if (ev.Dispose)
                {
                    Dispose();
                }
            }
        } // Close

        /// <summary>
        /// Close
        /// </summary>
        public virtual void Close(ModalResult modalResult)
        {
            ModalResult = modalResult;
            Close();
        } // Close

        #endregion

        #region On Closing, On Closed

        protected virtual void OnClosing(WindowClosingEventArgs e)
        {
            if (Closing != null) Closing.Invoke(this, e);
        } // OnClosing

        protected virtual void OnClosed(WindowClosedEventArgs e)
        {
            if (Closed != null) Closed.Invoke(this, e);
        } // OnClosed

        #endregion

        #region Input_KeyDown

        /// <summary>
        /// If it's modal then with escape can be closed.
        /// </summary>
        void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (Visible && (UserInterfaceManager.FocusedControl != null && UserInterfaceManager.FocusedControl.Root == this) && e.Key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                Close(ModalResult.Cancel);
            }
        }

        #endregion

    } // ModalContainer
} // XNAFinalEngine.UserInterface