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

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Tool Bar
    /// </summary>
    public class ToolBar : Control
    {

        #region Variables
             
        private int row;

        #endregion

        #region Properties

        public virtual int Row
        {
            get { return row; }
            set
            {
                row = value;
                if (row < 0) row = 0;
                if (row > 7) row = 7;
            }
        } // Row

        public virtual bool FullRow { get; set; }

        #endregion

        #region Constructor
      
        public ToolBar()
        {
            FullRow = false;
            Left = 0;
            Top = 0;
            Width = 64;
            Height = 24;
            CanFocus = false;
        } // ToolBar

        #endregion

        #region Init Skin

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinInformation = new SkinControlInformation(Skin.Controls["ToolBar"]);
        } // InitSkin

        #endregion

    } // ToolBar
} // XNAFinalEngine.UserInterface