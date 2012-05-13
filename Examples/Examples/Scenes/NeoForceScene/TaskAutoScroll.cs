
// This class came from Neo Force Tutorial and is not formatted correctly.

#region Using directives
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.UserInterface
{
    public class TaskAutoScroll : Window
    {

        #region Constructor

        public TaskAutoScroll()
        {
            Height = 360;
            MinimumHeight = 99;
            MinimumWidth = 78;
            Text = "Auto Scrolling";
            CenterWindow();

            Panel pnl1 = new Panel
            {
                Parent = this,
                Width = 400,
                Height = 180,
                Left = 20,
                Top = 20,
                BevelBorder = BevelBorder.All,
                BevelStyle = BevelStyle.Flat,
                BevelMargin = 1,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                AutoScroll = true
            };

            Panel pnl2 = new Panel
            {
                Parent = this,
                Width = 400,
                Height = 320,
                Left = 40,
                Top = 80,
                BevelBorder = BevelBorder.All,
                BevelStyle = BevelStyle.Flat,
                BevelMargin = 1,
                Text = "2",
                Anchor = Anchors.Left | Anchors.Top,
                Color = Color.White
            };
        } // TaskAutoScroll

        #endregion

    } // TaskAutoScroll
} // XNAFinalEngine.UI.Central
