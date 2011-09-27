
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
using System;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Dialog window. It has a top panel (with description and caption) and a bottom panel.
    /// </summary>
    public class Dialog : Window
    {

        #region Properties

        /// <summary>
        /// Top panel. Stores the caption and description.
        /// </summary>
        public Panel TopPanel { get; private set; }

        /// <summary>
        /// Bottom panel, good for buttons.
        /// </summary>
        public Panel BottomPanel { get; private set; }

        /// <summary>
        /// Caption. It shows in the top panel.
        /// </summary>
        public Label Caption { get; private set; }

        /// <summary>
        /// Description. It shows in the top panel.
        /// </summary>
        public Label Description { get; private set; }

        #endregion

        #region Constructor
  
        /// <summary>
        /// Dialog window. It has a top panel (with description and caption) and a bottom panel.
        /// </summary>
        public Dialog()
        {
            TopPanel = new Panel
            {
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Height = 64,
                BevelBorder = BevelBorder.Bottom,
                Parent = this,
                Width = ClientWidth
            };
                Caption = new Label
                {
                    Parent = TopPanel,
                    Text = "Caption",
                    Left = 8,
                    Top = 8,
                    Alignment = Alignment.TopLeft,
                    Anchor = Anchors.Left | Anchors.Top | Anchors.Right
                };
                Caption.Width = Caption.Parent.ClientWidth - 16;

                Description = new Label
                {
                    Parent = TopPanel,
                    Left = 8,
                    Text = "Description text.",
                    Alignment = Alignment.TopLeft,
                    Anchor = Anchors.Left | Anchors.Top | Anchors.Right
                };
                Description.Width = Description.Parent.ClientWidth - 16;
            
            BottomPanel = new Panel
            {
                Parent = this,
                Width = ClientWidth,
                Height = 24 + 16,
                BevelBorder = BevelBorder.Top,
                Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right
            };
            BottomPanel.Top = ClientHeight - BottomPanel.Height;
        } // Dialog

        #endregion

        #region Init

        protected internal override void Init()
        {
            base.Init();
            
            SkinLayer lc = new SkinLayer(Caption.SkinControlInformation.Layers[0]);
            lc.Text.Font.Resource = Skin.Fonts[Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["CaptFont"].Value].Resource;
            lc.Text.Colors.Enabled = Utilities.ParseColor(Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["CaptFontColor"].Value);

            SkinLayer ld = new SkinLayer(Description.SkinControlInformation.Layers[0]);
            ld.Text.Font.Resource = Skin.Fonts[Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["DescFont"].Value].Resource;
            ld.Text.Colors.Enabled = Utilities.ParseColor(Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["DescFontColor"].Value);

            TopPanel.Color = Utilities.ParseColor(Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["Color"].Value);
            TopPanel.BevelMargin = int.Parse(Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["BevelMargin"].Value);
            TopPanel.BevelStyle = ParseBevelStyle(Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["BevelStyle"].Value);

            Caption.SkinControlInformation = new SkinControl(Caption.SkinControlInformation);
            Caption.SkinControlInformation.Layers[0] = lc;
            Caption.Height = Skin.Fonts[Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["CaptFont"].Value].Height;

            Description.SkinControlInformation = new SkinControl(Description.SkinControlInformation);
            Description.SkinControlInformation.Layers[0] = ld;
            Description.Height = Skin.Fonts[Skin.Controls["Dialog"].Layers["TopPanel"].Attributes["DescFont"].Value].Height;
            Description.Top = Caption.Top + Caption.Height + 4;
            Description.Height = Description.Parent.ClientHeight - Description.Top - 8;

            BottomPanel.Color = Utilities.ParseColor(Skin.Controls["Dialog"].Layers["BottomPanel"].Attributes["Color"].Value);
            BottomPanel.BevelMargin = int.Parse(Skin.Controls["Dialog"].Layers["BottomPanel"].Attributes["BevelMargin"].Value);
            BottomPanel.BevelStyle = ParseBevelStyle(Skin.Controls["Dialog"].Layers["BottomPanel"].Attributes["BevelStyle"].Value);
        } // Init

        private static BevelStyle ParseBevelStyle(string str)
        {
            return (BevelStyle)Enum.Parse(typeof(BevelStyle), str, true);
        } // ParseBevelStyle

        #endregion

    } // Dialog
} // XNAFinalEngine.UI