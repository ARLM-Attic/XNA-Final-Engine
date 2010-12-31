
#region Using directives
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.UI.Central
{
    public class TaskAutoScroll : Window
    {

        #region Variables
         
        private Panel pnl1;
        private Panel pnl2;

        #endregion

        #region Constructor

        public TaskAutoScroll()
        {
            Height = 360;
            MinimumHeight = 99;
            MinimumWidth = 78;
            Text = "Auto Scrolling";
            CenterWindow();

            pnl1 = new Panel
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

            pnl2 = new Panel
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
        }

        #endregion

    } // TaskAutoScroll
} // XNAFinalEngine.UI.Central
