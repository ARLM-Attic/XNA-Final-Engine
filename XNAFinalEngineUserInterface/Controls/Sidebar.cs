
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
    
    public class SideBar : Panel
    {
                         
        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinInformation = new SkinControl(Skin.Controls["SideBar"]);
        } // InitSkin

    } // SideBar
} // XNAFinalEngine.UserInterface
