
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Tool Bar Panel.
    /// </summary>
    public class ToolBarPanel : Control
    {

        #region Constructor
   
        /// <summary>
        /// Tool Bar Panel.
        /// </summary>
        public ToolBarPanel()
        {
            Width = 64;
            Height = 25;
        } // ToolBarPanel

        #endregion

        #region Init
                      
        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinInformation = new SkinControl(Skin.Controls["ToolBarPanel"]);
        } // InitSkin

        #endregion

        #region Update

        protected internal override void Update()
        {
            base.Update();
            AlignBars();
        } // Update

        private void AlignBars()
        {
            int[] rx = new int[8];
            int h = 0;
            int rm = -1;

            foreach (Control c in ChildrenControls)
            {
                if (c is ToolBar)
                {
                    ToolBar t = c as ToolBar;
                    if (t.FullRow) t.Width = Width;
                    t.Left = rx[t.Row];
                    t.Top = (t.Row * t.Height) + (t.Row > 0 ? 1 : 0);
                    rx[t.Row] += t.Width + 1;

                    if (t.Row > rm)
                    {
                        rm = t.Row;
                        h = t.Top + t.Height + 1;
                    }
                }
            }
            Height = h;
        } // AlignBars

        #endregion

    } // ToolBarPanel
} // XNAFinalEngine.UserInterface