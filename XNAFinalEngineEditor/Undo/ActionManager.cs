
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
    /// Action Manager is a central class for the Undo Framework.
    /// Your domain model (business objects) will have an ActionManager reference that would 
    /// take care of executing actions.
    /// 
    /// Here's how it works:
    /// 1. You declare a class that implements IAction
    /// 2. You create an instance of it and give it all necessary info that it needs to know
    ///    to apply or rollback a change
    /// 3. You call ActionManager.RecordAction(yourAction)
    /// 
    /// Then you can also call ActionManager.Undo() or ActionManager.Redo()
    /// </summary>
    public static class ActionManager
    {

        #region Variables

        private static readonly Stack<Transaction> mTransactionStack = new Stack<Transaction>();

        private static IActionHistory mHistory;

        #endregion

        #region Properties

        /// <summary>
        /// Defines whether we should record an action to the Undo buffer and then execute,
        /// or just execute it without it becoming a part of history
        /// </summary>
        public static bool ExecuteImmediatelyWithoutRecording { get; set; }

        /// <summary>
        /// Currently running action (during an Undo or Redo process)
        /// </summary>
        /// <remarks>null if no Undo or Redo is taking place</remarks>
        public static IAction CurrentAction { get; internal set; }

        /// <summary>
        /// Checks if we're inside an Undo or Redo operation
        /// </summary>
        public static bool ActionIsExecuting { get { return CurrentAction != null; } }

        public static Stack<Transaction> TransactionStack { get { return mTransactionStack; } }

        public static Transaction RecordingTransaction 
        {
            get
            {
                if (TransactionStack.Count > 0)
                    return TransactionStack.Peek();
                return null;
            }
        } // RecordingTransaction

        public static bool CanUndo { get { return History.CanMoveBack; } }

        public static bool CanRedo { get { return History.CanMoveForward; } }

        internal static IActionHistory History
        {
            get { return mHistory; }
            set
            {
                if (mHistory != null)
                    mHistory.CollectionChanged -= RaiseUndoBufferChanged;
                mHistory = value;
                if (mHistory != null)
                    mHistory.CollectionChanged += RaiseUndoBufferChanged;
            }
        } // History

        #endregion

        #region Constructor

        static ActionManager()
        {
            History = new SimpleHistory();
        } // ActionManager

        #endregion

        #region Events

        /// <summary>
        /// Listen to this event to be notified when a new action is added, executed, undone or redone
        /// </summary>
        public static  event EventHandler CollectionChanged;

        internal static void RaiseUndoBufferChanged(object sender, EventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(null, e);
            }
        } // RaiseUndoBufferChanged

        #endregion

        #region RecordAction

        #region Record Action

        /// <summary>
        /// Central method to add and execute a new action.
        /// </summary>
        /// <param name="action">An action to be recorded in the buffer and executed.</param>
        public static void RecordAction(IAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action", "ActionManager.RecordAction: the action argument is null");

            // make sure we're not inside an Undo or Redo operation
            CheckNotRunningBeforeRecording(action);

            // if we don't want to record actions, just run and forget it
            if (ExecuteImmediatelyWithoutRecording && action.CanExecute())
            {
                action.Execute();
                return;
            }

            // Check if we're inside a transaction that is being recorded
            Transaction currentTransaction = RecordingTransaction;
            if (currentTransaction != null)
            {
                // if we're inside a transaction, just add the action to the transaction's list
                currentTransaction.Add(action);
                if (!currentTransaction.IsDelayed)
                {
                    action.Execute();
                }
            }
            else
            {
                RunActionDirectly(action);
            }
        } // RecordAction

        #endregion

        #region Check Not Running Before Recording

        private static void CheckNotRunningBeforeRecording(IAction candidate)
        {
            if (CurrentAction != null)
            {
                string candidateActionName = candidate != null ? candidate.ToString() : "";
                throw new InvalidOperationException
                (
                    string.Format
                    (
                          "ActionManager.RecordActionDirectly: the ActionManager is currently running "
                        + "or undoing an action ({0}), and this action (while being executed) attempted "
                        + "to recursively record another action ({1}), which is not allowed. "
                        + "You can examine the stack trace of this exception to see what the "
                        + "executing action did wrong and change this action not to influence the "
                        + "Undo stack during its execution. Checking if ActionManager.ActionIsExecuting == true "
                        + "before launching another transaction might help to avoid the problem. Thanks and sorry for the inconvenience.",
                        CurrentAction.ToString(),
                        candidateActionName
                    )
                );
            }
        } // CheckNotRunningBeforeRecording

        #endregion

        #region Run Action Directly

        /// <summary>
        /// Adds the action to the buffer and runs it
        /// </summary>
        private static void RunActionDirectly(IAction actionToRun)
        {
            CheckNotRunningBeforeRecording(actionToRun);

            CurrentAction = actionToRun;
            try
            {
                if (History.AppendAction(actionToRun))
                    History.MoveForward();
            }
            finally
            {
                CurrentAction = null;
            }
        } // RunActionDirectly

        #endregion

        #endregion

        #region Transactions

        public static Transaction CreateTransaction()
        {
            return Transaction.Create();
        } // CreateTransaction

        public static Transaction CreateTransaction(bool delayed)
        {
            return Transaction.Create(delayed);
        } // CreateTransaction

        public static void OpenTransaction(Transaction t)
        {
            TransactionStack.Push(t);
        } // OpenTransaction

        public static void CommitTransaction()
        {
            if (TransactionStack.Count == 0)
            {
                throw new InvalidOperationException(
                    "ActionManager.CommitTransaction was called"
                    + " when there is no open transaction (TransactionStack is empty)."
                    + " Please examine the stack trace of this exception to find code"
                    + " which called CommitTransaction one time too many."
                    + " Normally you don't call OpenTransaction and CommitTransaction directly,"
                    + " but use using(var t = Transaction.Create(Root)) instead.");
            }

            Transaction committing = TransactionStack.Pop();

            if (committing.HasActions())
                RecordAction(committing);
        } // CommitTransaction

        public static void RollBackTransaction()
        {
            if (TransactionStack.Count != 0)
            {
                var topLevelTransaction = TransactionStack.Peek();
                if (topLevelTransaction != null)
                    topLevelTransaction.UnExecute();

                TransactionStack.Clear();
            }
        } // RollBackTransaction

        #endregion

        #region Undo, Redo

        public static void Undo()
        {
            if (!CanUndo)
                return;
            if (ActionIsExecuting)
            {
                throw new InvalidOperationException(string.Format("ActionManager is currently busy"
                    + " executing a transaction ({0}). This transaction has called Undo()"
                    + " which is not allowed until the transaction ends."
                    + " Please examine the stack trace of this exception to see"
                    + " what part of your code called Undo.", CurrentAction));
            }
            CurrentAction = History.CurrentState.PreviousAction;
            History.MoveBack();
            CurrentAction = null;
        } // Undo

        public static void Redo()
        {
            if (!CanRedo)
                return;
            if (ActionIsExecuting)
            {
                throw new InvalidOperationException(string.Format("ActionManager is currently busy"
                    + " executing a transaction ({0}). This transaction has called Redo()"
                    + " which is not allowed until the transaction ends."
                    + " Please examine the stack trace of this exception to see"
                    + " what part of your code called Redo.", CurrentAction));
            }
            CurrentAction = History.CurrentState.NextAction;
            History.MoveForward();
            CurrentAction = null;
        } // Redo

        #endregion

        #region Buffer

        public static void Clear()
        {
            History.Clear();
            CurrentAction = null;
        } // Clear

        public static IEnumerable<IAction> EnumUndoableActions()
        {
            return History.EnumerateUndoableActions();
        } // EnumUndoableActions

        #endregion

        #region Set Property

        public static void SetProperty(object instance, string propertyName, object value)
        {
            SetPropertyAction action = new SetPropertyAction(instance, propertyName, value);
            RecordAction(action);
        } // SetProperty

        public static void CallMethod(Action execute, Action unexecute)
        {
            CallMethodAction action = new CallMethodAction(execute, unexecute);
            RecordAction(action);
        } // SetProperty

        #endregion

    } // ActionManager
} // XNAFinalEngine.Undo