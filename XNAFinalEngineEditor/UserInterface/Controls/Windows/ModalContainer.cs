
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
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

        // It stores the previous modal when this modal is activated.
        private ModalContainer lastModal;

        // Default value.
        private ModalResult modalResult = ModalResult.None;

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
                if (value) 
                    Focused = true;
                base.Visible = value;
            }
        } // Visible

        /// <summary>
        /// Is the active modal?
        /// </summary>
        public virtual bool IsTheActiveModal { get { return UserInterfaceManager.ModalWindow == this; } }

        /// <summary>
        /// When the modal window is closed can give diferent results (None, Ok, Cancel, Yes, No, Abort, Retry, Ignore).
        /// This stores the result.
        /// </summary>
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
        /// Show this container in modal mode.
        /// That means that prevent user from interacting with the rest of the controls.
        /// </summary>
        public virtual void ShowModal()
        {
            // You can't activate the modal mode twice at the same time.
            if (UserInterfaceManager.ModalWindow == this)
                return;
            lastModal = UserInterfaceManager.ModalWindow;
            UserInterfaceManager.ModalWindow = this;
            // This allow to close the modal window with the escape key.
            UserInterfaceManager.InputSystem.KeyDown += InputKeyDown;
        } // ShowModal

        #endregion

        #region Close

        /// <summary>
        /// Close
        /// </summary>
        public virtual void Close()
        {
            // Raise on closing event.
            WindowClosingEventArgs ex = new WindowClosingEventArgs();
            OnClosing(ex);
            // If an event does not deny the closing...
            if (!ex.Cancel)
            {
                // Remove the event link to prevent garbage.
                UserInterfaceManager.InputSystem.KeyDown -= InputKeyDown;
                // Restore previous modal window.
                UserInterfaceManager.ModalWindow = lastModal;
                if (lastModal != null)
                    lastModal.Focused = true;
                else
                    UserInterfaceManager.FocusedControl = null;
                Hide();
                // Raise on closed event.
                WindowClosedEventArgs ev = new WindowClosedEventArgs();
                OnClosed(ev);
                // If an event does not change the dispose property.
                if (ev.Dispose)
                    Dispose();
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
            if (Closing != null) 
                Closing.Invoke(this, e);
        } // OnClosing

        protected virtual void OnClosed(WindowClosedEventArgs e)
        {
            if (Closed != null) 
                Closed.Invoke(this, e);
        } // OnClosed

        #endregion

        #region Input KeyDown

        /// <summary>
        /// If it's modal then with escape can be closed.
        /// </summary>
        void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (Visible && UserInterfaceManager.FocusedControl == this && e.Key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                Close(ModalResult.Cancel);
            }
        } // InputKeyDown

        #endregion

    } // ModalContainer
} // XNAFinalEngine.UserInterface