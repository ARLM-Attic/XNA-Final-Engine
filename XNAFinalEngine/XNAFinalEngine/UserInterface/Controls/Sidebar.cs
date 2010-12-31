
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
    
    public class SideBar : Panel
    {
                         
        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinControlInformation = new SkinControl(UIManager.Skin.Controls["SideBar"]);
        } // InitSkin

    } // SideBar
} // XNAFinalEngine.UI
