
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
#endregion

namespace XNAFinalEngine.UserInterface
{

    public class StackPanel : Container
    {

        #region Variables

        private readonly Orientation orientation;

        #endregion

        #region Constructor

        public StackPanel(Orientation orientation)
        {
            this.orientation = orientation;
            Color = Color.Transparent;
        } // StackPanel

        #endregion

        #region Calculate Layout

        private void CalculateLayout()
        {
            int top = Top;
            int left = Left;

            foreach (Control c in ClientArea.ChildrenControls)
            {
                Margins m = c.DefaultDistanceFromAnotherControl;

                if (orientation == Orientation.Vertical)
                {
                    top += m.Top;
                    c.Top = top;
                    top += c.Height;
                    top += m.Bottom;
                    c.Left = left;
                }

                if (orientation == Orientation.Horizontal)
                {
                    left += m.Left;
                    c.Left = left;
                    left += c.Width;
                    left += m.Right;
                    c.Top = top;
                }
            }
        } // CalculateLayout

        #endregion

        #region On Resize

        protected override void OnResize(ResizeEventArgs e)
        {
            CalculateLayout();
            base.OnResize(e);
        } // OnResize

        #endregion

    } // StackPanel
} //  XNAFinalEngine.UserInterface