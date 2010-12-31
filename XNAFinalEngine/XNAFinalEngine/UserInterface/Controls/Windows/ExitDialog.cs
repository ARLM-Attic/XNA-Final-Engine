
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
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.UI
{

    /// <summary>
    /// Exit Dialog.
    /// </summary>
    public class ExitDialog : Dialog
    {

        #region Variables
              
        private readonly Button buttonYes;
        private readonly Button buttonNo;
        private readonly Label labelMessage;
        private readonly ImageBox imageIcon;

        #endregion

        #region Constructor
    
        /// <summary>
        /// Exit Dialog.
        /// </summary>
        public ExitDialog()
        {
            string msg = "Do you really want to exit " + EngineManager.Title + "?";
            ClientWidth = (int)UIManager.Skin.Controls["Label"].Layers[0].Text.Font.Resource.MeasureString(msg).X + 48 + 16 + 16 + 16;
            ClientHeight = 120;
            TopPanel.Visible = false;
            IconVisible = true;
            Resizable = false;
            StayOnTop = true;
            Text = EngineManager.Title;
            CenterWindow();

            imageIcon = new ImageBox
            {
                Texture = UIManager.Skin.Images["Icon.Question"].Texture,
                Left = 16,
                Top = 16,
                Width = 48,
                Height = 48,
                SizeMode = SizeMode.Stretched
            };

            labelMessage = new Label
                {
                    Left = 80,
                    Top = 16,
                    Height = 48,
                    Alignment = Alignment.TopLeft,
                    Text = msg
                };
            labelMessage.Width = ClientWidth - labelMessage.Left;
            
            buttonYes = new Button
            {
                Top = 8, 
                Text = "Yes",
                ModalResult = ModalResult.Yes
            };
            buttonYes.Left = (BottomPanel.ClientWidth / 2) - buttonYes.Width - 4;
            
            buttonNo = new Button
            {
                Left = (BottomPanel.ClientWidth/2) + 4,
                Top = 8,
                Text = "No",
                ModalResult = ModalResult.No
            };

            Add(imageIcon);
            Add(labelMessage);
            BottomPanel.Add(buttonYes);
            BottomPanel.Add(buttonNo);

            DefaultControl = buttonNo;
        } // ExitDialog

        #endregion

    } // ExitDialog
} // XNAFinalEngine.UI