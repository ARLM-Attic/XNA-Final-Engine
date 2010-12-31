
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.UI
{

    /// <summary>
    /// Group Panel. Group controls that will be enclosed by a bevel and it will be a title in the top title bar.
    /// </summary>
    public class GroupPanel : Container
    {

        #region Constructor

        /// <summary>
        /// Group Panel. Group controls with and title a bar.
        /// </summary>
        public GroupPanel()
        {
            CanFocus = false;
            Passive = true;
            Width = 64;
            Height = 64;
        } // GroupPanel

        #endregion

        #region Draw Control

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            SkinLayer layer = SkinControlInformation.Layers["Control"];
            SpriteFont font = (layer.Text != null && layer.Text.Font != null) ? layer.Text.Font.Resource : null;
            Point offset = new Point(layer.Text.OffsetX, layer.Text.OffsetY);

            Renderer.DrawLayer(this, layer, rect);

            if (font != null && !string.IsNullOrEmpty(Text))
            {
                Renderer.DrawString(this, layer, Text, new Rectangle(rect.Left, rect.Top + layer.ContentMargins.Top, rect.Width, SkinControlInformation.ClientMargins.Top - layer.ContentMargins.Horizontal), false, offset.X, offset.Y, false);
            }
        } // DrawControl

        #endregion

    } // GroupPanel
} // XNAFinalEngine.UI