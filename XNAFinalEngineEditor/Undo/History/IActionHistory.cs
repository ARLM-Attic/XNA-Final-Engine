
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
    /// A notion of the buffer. Instead of two stacks, it's a state machine
    /// with the current state. It can move one state back or one state forward.
    /// Allows for non-linear buffers, where you can choose one of several actions to redo.
    /// </summary>
    internal interface IActionHistory : IEnumerable<IAction>
    {

        #region Properties

        bool CanMoveBack { get; }
        bool CanMoveForward { get; }
        int Length { get; }
        SimpleHistoryNode CurrentState { get; }

        #endregion

        #region Events

        event EventHandler CollectionChanged;

        #endregion

        /// <summary>
        /// Appends an action to the end of the Undo buffer.
        /// </summary>
        /// <param name="newAction">An action to append.</param>
        /// <returns>false if merged with previous, else true</returns>
        bool AppendAction(IAction newAction);

        void Clear();

        void MoveBack();

        void MoveForward();

        IEnumerable<IAction> EnumerateUndoableActions();

    } // IActionHistory
} // XNAFinalEngine.Undo