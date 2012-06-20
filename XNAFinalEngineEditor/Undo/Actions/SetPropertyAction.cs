
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
using System.Reflection;
#endregion

namespace XNAFinalEngine.Undo
{
    /// <summary>
    /// This is a sample action that can change any property on any object  It can also undo what it did.
    /// </summary>
    public class SetPropertyAction : AbstractAction
    {

        #region Properties

        public object ParentObject { get; set; }
        public PropertyInfo Property { get; set; }
        public object Value { get; set; }
        public object OldValue { get; set; }

        #endregion

        public SetPropertyAction(object parentObject, string propertyName, object value)
        {
            ParentObject = parentObject;
            Property = parentObject.GetType().GetProperty(propertyName);
            Value = value;
        } // SetPropertyAction
        
        protected override void ExecuteCore()
        {
            OldValue = Property.GetValue(ParentObject, null);
            Property.SetValue(ParentObject, Value, null);
        } // ExecuteCore

        protected override void UnExecuteCore()
        {
            Property.SetValue(ParentObject, OldValue, null);
        } // UnExecuteCore

        /// <summary>
        /// Subsequent changes of the same property on the same object are consolidated into one action
        /// </summary>
        /// <param name="followingAction">Subsequent action that is being recorded</param>
        /// <returns>true if it agreed to merge with the next action, 
        /// false if the next action should be recorded separately</returns>
        public override bool TryToMerge(IAction followingAction)
        {
            SetPropertyAction next = followingAction as SetPropertyAction;
            if (next != null && next.ParentObject == this.ParentObject && next.Property == this.Property)
            {
                Value = next.Value;
                Property.SetValue(ParentObject, Value, null);
                return true;
            }
            return false;
        } // TryToMerge

    } // SetPropertyAction
} // XNAFinalEngine.Undo