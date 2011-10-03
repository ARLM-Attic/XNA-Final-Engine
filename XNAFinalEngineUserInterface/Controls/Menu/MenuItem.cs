
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

    /// <summary>
    /// Menu Item.
    /// </summary>
    public class MenuItem
    {

        #region Variables

        /// <summary>
        /// Showing text.
        /// </summary>
        private string text = "MenuItem";

        /// <summary>
        /// Children Items.
        /// </summary>
        public List<MenuItem> childrenItems = new List<MenuItem>();

        /// <summary>
        /// Enabled?
        /// </summary>
        private bool enabled = true;

        #endregion

        #region Properties

        /// <summary>
        /// Showing text.
        /// </summary>
        public string Text { get { return text; } set { text = value; } }

        /// <summary>
        /// Icon that can be placed to the left of the text.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// Separated?
        /// </summary>
        public bool Separated { get; set; }

        /// <summary>
        /// Enabled?
        /// </summary>
        public bool Enabled { get { return enabled; } set { enabled = value; } }

        /// <summary>
        /// Children Items.
        /// </summary>
        public List<MenuItem> ChildrenItems { get { return childrenItems; } set { childrenItems = value; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Menu Item.
        /// </summary>
        public MenuItem() { }

        /// <summary>
        /// Menu Item.
        /// </summary>
        public MenuItem(string text) : this()
        {
            Text = text;
        } // MenuItem

        /// <summary>
        /// Menu Item.
        /// </summary>
        public MenuItem(string text, bool separated) : this(text)
        {
            Separated = separated;
        } // MenuItem

        #endregion

        #region Events

        public event EventHandler Click;
        public event EventHandler Selected;

        #endregion

        #region Invoke

        internal void ClickInvoke(EventArgs e)
        {
            if (Click != null) Click.Invoke(this, e);
        } // ClickInvoke

        internal void SelectedInvoke(EventArgs e)
        {
            if (Selected != null) Selected.Invoke(this, e);
        } // SelectedInvoke

        #endregion

    } // MenuItem
} // XNAFinalEngine.UserInterface