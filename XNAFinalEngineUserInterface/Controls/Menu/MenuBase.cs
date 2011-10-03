
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.UserInterface
{

    public abstract class MenuBase : Control
    {

        #region Variables
              
        private int itemIndex = -1;
        private readonly List<MenuItem> items = new List<MenuItem>();

        #endregion

        #region Properties

        protected internal int ItemIndex { get { return itemIndex; } set { itemIndex = value; } }
        protected internal MenuBase ChildMenu { get; set; }
        protected internal MenuBase RootMenu { get; set; }
        protected internal MenuBase ParentMenu { get; set; }
        public List<MenuItem> Items { get { return items; } }

        #endregion

        #region Constructor

        protected MenuBase()
        {
            RootMenu = this;
        } // MenuBase

        #endregion

    } // MenuBase
} // XNAFinalEngine.UserInterface