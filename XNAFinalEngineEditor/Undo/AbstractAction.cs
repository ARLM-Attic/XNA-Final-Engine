
#region License
/*

 Based in the class Undo Framework (undo.codeplex.com) from KirillOsenkov
 License: Microsoft Public License (Ms-PL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

namespace XNAFinalEngine.Undo
{
    public abstract class AbstractAction : IAction
    {

        #region Properties

        /// <summary>
        /// Defines if the action can be merged with the previous one in the Undo buffer
        /// This is useful for long chains of consecutive operations of the same type,
        /// e.g. dragging something or typing some text
        /// </summary>
        public bool AllowToMergeWithPrevious { get; set; }

        protected int ExecuteCount { get; set; }

        #endregion

        #region Execute

        public virtual void Execute()
        {
            if (!CanExecute())
            {
                return;
            }
            ExecuteCore();
            ExecuteCount++;
        } // Execute

        #endregion

        #region Execute Core

        /// <summary>
        /// Override execute core to provide your logic that actually performs the action
        /// </summary>
        protected abstract void ExecuteCore();

        public virtual void UnExecute()
        {
            if (!CanUnExecute())
            {
                return;
            }
            UnExecuteCore();
            ExecuteCount--;
        } // ExecuteCore

        /// <summary>
        /// Override this to provide the logic that undoes the action
        /// </summary>
        protected abstract void UnExecuteCore();

        #endregion

        #region Can Execute and UnExecute

        public virtual bool CanExecute()
        {
            return ExecuteCount == 0;
        } // CanExecute

        public virtual bool CanUnExecute()
        {
            return !CanExecute();
        } // CanUnExecute

        #endregion

        #region Try To Merge

        /// <summary>
        /// If the last action can be joined with the followingAction,
        /// the following action isn't added to the Undo stack,
        /// but rather mixed together with the current one.
        /// </summary>
        /// <param name="FollowingAction"></param>
        /// <returns>true if the FollowingAction can be merged with the
        /// last action in the Undo stack</returns>
        public virtual bool TryToMerge(IAction followingAction)
        {
            return false;
        } // TryToMerge

        #endregion

    } // AbstractAction
} // XNAFinalEngine.Undo