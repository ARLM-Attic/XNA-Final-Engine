
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
using System.Linq;
#endregion

namespace XNAFinalEngine.Undo
{
    public sealed class Transaction : IAction, IDisposable
    {

        #region Variables

        private readonly List<IAction> actions;

        #endregion

        #region Properties

        public bool Aborted { get; set; }

        public bool AllowToMergeWithPrevious { get; set; }

        public bool IsDelayed { get; set; }

        #endregion

        #region Constructor

        private Transaction(bool delayed)
        {
            actions = new List<IAction>();
            ActionManager.OpenTransaction(this);
            IsDelayed = delayed;
        } // Transaction

        #endregion

        #region Create

        public static Transaction Create(bool delayed)
        {
            return new Transaction(delayed);
        } // Create

        /// <summary>
        /// By default, the actions are delayed and executed only after the top-level transaction commits.
        /// </summary>
        /// <remarks>
        /// Make sure to dispose of the transaction once you're done - it will actually call Commit for you.
        /// </remarks>
        /// <example>
        /// Recommended usage: using (Transaction.Create(actionManager)) { DoStuff(); }
        /// </example>
        public static Transaction Create()
        {
            return Create(true);
        } // Create

        #endregion

        #region IAction implementation

        public void Execute()
        {
            if (!IsDelayed)
            {
                IsDelayed = true;
                return;
            }
            foreach (var action in actions)
            {
                action.Execute();
            }
        } // Execute

        public void UnExecute()
        {
            foreach (var action in Enumerable.Reverse(actions))
            {
                action.UnExecute();
            }
        } // UnExecute

        public bool CanExecute()
        {
            foreach (var action in actions)
            {
                if (!action.CanExecute())
                    return false;
            }
            return true;
        } // CanExecute

        public bool CanUnExecute()
        {
            foreach (var action in Enumerable.Reverse(actions))
            {
                if (!action.CanUnExecute())
                    return false;
            }
            return true;
        } // CanUnExecute

        public bool TryToMerge(IAction followingAction)
        {
            return false;
        } // TryToMerge

        #endregion

        #region Commit

        public void Commit()
        {
            ActionManager.CommitTransaction();
        } // Commit

        #endregion

        #region Rollback

        public void Rollback()
        {
            ActionManager.RollBackTransaction();
            Aborted = true;
        } // Rollback

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (!Aborted)
                Commit();
        } // Dispose

        #endregion

        #region Add

        public void Add(IAction actionToAppend)
        {
            if (actionToAppend == null)
                throw new ArgumentNullException("actionToAppend");
            actions.Add(actionToAppend);
        } // Add

        #endregion

        #region Has Actions

        public bool HasActions()
        {
            return actions.Count != 0;
        } // HasActions

        #endregion

        #region Remove

        public void Remove(IAction actionToCancel)
        {
            if (actionToCancel == null)
            {
                throw new ArgumentNullException("actionToCancel");
            }
            actions.Remove(actionToCancel);
        } // Remove

        #endregion

    } // Transaction
} // XNAFinalEngine.Undo
