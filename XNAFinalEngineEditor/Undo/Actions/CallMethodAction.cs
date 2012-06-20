
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
#endregion

namespace XNAFinalEngine.Undo
{
    public class CallMethodAction : AbstractAction
    {

        public Action ExecuteDelegate { get; set; }
        public Action UnexecuteDelegate { get; set; }

        public CallMethodAction(Action execute, Action unexecute)
        {
            ExecuteDelegate = execute;
            UnexecuteDelegate = unexecute;
        } // CallMethodAction

        protected override void ExecuteCore()
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate();
        } // ExecuteCore

        protected override void UnExecuteCore()
        {
            if (UnexecuteDelegate != null)
                UnexecuteDelegate();
        } // UnExecuteCore

    } // CallMethodAction
} // XNAFinalEngine.Undo