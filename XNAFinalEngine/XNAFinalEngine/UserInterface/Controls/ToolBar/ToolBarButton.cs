﻿
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

namespace XNAFinalEngine.UI
{

    /// <summary>
    /// Tool bar button.
    /// </summary>
    public class ToolBarButton : Button
    {

        #region Constructor

        /// <summary>
        /// Tool Bar Button.
        /// </summary>
        public ToolBarButton()
        {
            CanFocus = false;
            Text = "";
        }

        #endregion

        #region Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinControlInformation = new SkinControl(UIManager.Skin.Controls["ToolBarButton"]);
        } // InitSkin

        #endregion

    } // ToolBarButton
} // XNAFinalEngine.UI
