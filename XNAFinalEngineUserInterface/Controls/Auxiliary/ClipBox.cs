
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.UserInterface
{

    public class ClipBox : Control
    {

        /// <summary>
        /// Clip Box.
        /// </summary>
        public ClipBox()
        {
            Color = Color.Transparent;
            BackgroundColor = Color.Transparent;
            CanFocus = false;
            Passive = true;
        } // ClipBox

    } // ClipBox
} // XNAFinalEngine.UserInterface