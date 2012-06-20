
#region License
/*

 Based in the class Undo Framework (undo.codeplex.com) from KirillOsenkov
 License: Microsoft Public License (Ms-PL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.Undo
{
    /// <summary>
    /// IActionHistory represents a recorded list of actions undertaken by user.

    /// This class implements a usual, linear action sequence. You can move back and forth
    /// changing the state of the respective document. When you move forward, you execute
    /// a respective action, when you move backward, you Undo it (UnExecute).

    /// Implemented through a double linked-list of SimpleHistoryNode objects.
    /// ====================================================================
    /// </summary>
    internal class SimpleHistory : IActionHistory
    {

        #region Variables

        private SimpleHistoryNode mCurrentState = new SimpleHistoryNode();

        #endregion

        #region Properties

        /// <summary>
        /// "Iterator" to navigate through the sequence, "Cursor"
        /// </summary>
        public SimpleHistoryNode CurrentState
        {
            get { return mCurrentState; }
            set
            {
                if (value != null)
                    mCurrentState = value;
                else
                    throw new ArgumentNullException("value");
            }
        } // CurrentState

        public SimpleHistoryNode Head { get; set; }

        public IAction LastAction { get; set; }

        public bool CanMoveForward { get { return CurrentState.NextAction != null && CurrentState.NextNode != null; } }

        public bool CanMoveBack { get { return CurrentState.PreviousAction != null && CurrentState.PreviousNode != null; } }

        /// <summary>
        /// The length of Undo buffer (total number of undoable actions)
        /// </summary>
        public int Length { get; set; }

        #endregion

        #region Constructor

        public SimpleHistory()
        {
            Initialize();
        } // SimpleHistory

        #endregion

        #region Events

        public event EventHandler CollectionChanged;
        protected void RaiseUndoBufferChanged()
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new EventArgs());
            }
        }

        #endregion

        #region Append Action

        /// <summary>
        /// Adds a new action to the tail after current state. If 
        /// there exist more actions after this, they're lost (Garbage Collected).
        /// This is the only method of this class that actually modifies the linked-list.
        /// </summary>
        /// <param name="newAction">Action to be added.</param>
        /// <returns>true if action was appended, false if it was merged with the previous one</returns>
        public bool AppendAction(IAction newAction)
        {
            if (CurrentState.PreviousAction != null && CurrentState.PreviousAction.TryToMerge(newAction))
            {
                RaiseUndoBufferChanged();
                return false;
            }
            CurrentState.NextAction = newAction;
            CurrentState.NextNode = new SimpleHistoryNode(newAction, CurrentState);
            return true;
        } // AppendAction

        #endregion

        #region Clear

        /// <summary>
        /// All existing Nodes and Actions are garbage collected.
        /// </summary>
        public void Clear()
        {
            Initialize();
            RaiseUndoBufferChanged();
        } // Clear

        #endregion

        #region Initialize

        private void Initialize()
        {
            CurrentState = new SimpleHistoryNode();
            Head = CurrentState;
        } // Initialize

        #endregion

        #region Enumerate Undoable Actions

        public IEnumerable<IAction> EnumerateUndoableActions()
        {
            SimpleHistoryNode current = Head;
            while (current != null && current != CurrentState && current.NextAction != null)
            {
                yield return current.NextAction;
                current = current.NextNode;
            }
        } // EnumerateUndoableActions

        #endregion

        #region Move Forward and Backward

        public void MoveForward()
        {
            if (!CanMoveForward)
            {
                throw new InvalidOperationException(
                    "History.MoveForward() cannot execute because"
                    + " CanMoveForward returned false (the current state"
                    + " is the last state in the undo buffer.");
            }
            CurrentState.NextAction.Execute();
            CurrentState = CurrentState.NextNode;
            Length += 1;
            RaiseUndoBufferChanged();
        } // MoveForward

        public void MoveBack()
        {
            if (!CanMoveBack)
            {
                throw new InvalidOperationException(
                    "History.MoveBack() cannot execute because"
                    + " CanMoveBack returned false (the current state"
                    + " is the last state in the undo buffer.");
            }
            CurrentState.PreviousAction.UnExecute();
            CurrentState = CurrentState.PreviousNode;
            Length -= 1;
            RaiseUndoBufferChanged();
        } // MoveBack

        #endregion

        #region Get Enumerator

        public IEnumerator<IAction> GetEnumerator()
        {
            return EnumerateUndoableActions().GetEnumerator();
        } // GetEnumerator

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        } // GetEnumerator

        #endregion

    } // SimpleHistory
} // XNAFinalEngine.Undo