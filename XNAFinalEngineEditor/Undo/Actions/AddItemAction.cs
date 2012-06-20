
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
    public class AddItemAction<T> : AbstractAction
    {
        
        public Action<T> Adder { get; set; }
        public Action<T> Remover { get; set; }
        public T Item { get; set; }

        public AddItemAction(Action<T> adder, Action<T> remover, T item)
        {
            Adder = adder;
            Remover = remover;
            Item = item;
        } // AddItemAction
        
        protected override void ExecuteCore()
        {
            Adder(Item);
        } // ExecuteCore

        protected override void UnExecuteCore()
        {
            Remover(Item);
        } // UnExecuteCore

    } // AddItemAction
} // XNAFinalEngine.Undo
