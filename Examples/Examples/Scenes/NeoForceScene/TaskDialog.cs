
// This class came from Neo Force Tutorial and is not formatted correctly.

using XNAFinalEngine.Assets;

namespace XNAFinalEngine.UserInterface
{
    public class TaskDialog : Dialog
    {

        #region Variables
     
        private Button btnClose;
        private Button btnApply;
        private Button btnOk;
        private ImageBox imgTop;
        private TabControl tbcMain;
        private Button btnFirst;
        private Button btnSecond;
        private Button btnThird;
        private GroupPanel grpFirst;

        #endregion

        #region Constructor

        public TaskDialog()
        {   
            Height = 520;
            MinimumWidth = 254;
            MinimumHeight = 160;
            CenterWindow();

            TopPanel.Height = 80;
            TopPanel.BevelStyle = BevelStyle.None;
            TopPanel.BevelBorder = BevelBorder.None;
            Caption.Visible = false;
            Description.Visible = false;
            Text = "Dialog Template";

            imgTop = new ImageBox
            {
                Parent = TopPanel,
                Top = 0,
                Left = 0,
                Width = TopPanel.ClientWidth,
                Height = TopPanel.ClientHeight,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right | Anchors.Bottom,
                SizeMode = SizeMode.Normal,
                Texture = new Texture("Caption")
            };

            tbcMain = new TabControl
            {
                Parent = this,
                Left = 4,
                Top = TopPanel.Height + 4,
                Width = ClientArea.Width - 8,
                Height = ClientArea.Height - 8 - TopPanel.Height - BottomPanel.Height,
                Anchor = Anchors.All
            };
            tbcMain.AddPage();
            tbcMain.TabPages[0].Text = "First";
            tbcMain.AddPage();
            tbcMain.TabPages[1].Text = "Second";
            tbcMain.AddPage();
            tbcMain.TabPages[2].Text = "Third";

            btnFirst = new Button
            {
                Parent = tbcMain.TabPages[0],
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Top = 8,
                Left = 8,
                Text = ">>> First Page Button <<<"
            };
            btnFirst.Width = btnFirst.Parent.ClientWidth - 16;
            

            grpFirst = new GroupPanel
            {
                Parent = tbcMain.TabPages[0],
                Anchor = Anchors.All,
                Left = 8,
                Top = btnFirst.Top + btnFirst.Height + 4,
                Width = btnFirst.Parent.ClientWidth - 16
            };
            grpFirst.Height = btnFirst.Parent.ClientHeight - grpFirst.Top - 8;

            btnSecond = new Button
            {
                Parent = tbcMain.TabPages[1],
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Top = 8,
                Left = 8,
                Text = ">>> Second Page Button <<<"
            };
            btnSecond.Width = btnSecond.Parent.ClientWidth - 16;
            
            btnThird = new Button
                           {
                               Parent = tbcMain.TabPages[2],
                               Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                               Top = 8,
                               Left = 8,
                               Text = ">>> Third Page Button <<<"
                           };
            btnThird.Width = btnThird.Parent.ClientWidth - 16;
            
            btnOk = new Button
            {
                Parent = BottomPanel,
                Anchor = Anchors.Top | Anchors.Right,
                Text = "OK",
                ModalResult = ModalResult.Ok
            };
            btnOk.Top = btnOk.Parent.ClientHeight - btnOk.Height - 8;
            btnOk.Left = btnOk.Parent.ClientWidth - 8 - btnOk.Width * 3 - 8;
            
            btnApply = new Button
            {
                Parent = BottomPanel,
                Anchor = Anchors.Top | Anchors.Right,
                Top = btnOk.Parent.ClientHeight - btnOk.Height - 8,
                Left = btnOk.Parent.ClientWidth - 4 - btnOk.Width*2 - 8,
                Text = "Apply"
            };

            btnClose = new Button
            {
                Parent = BottomPanel,
                Anchor = Anchors.Top | Anchors.Right,
                Text = "Close",
                ModalResult = ModalResult.Cancel
            };
            btnClose.Top = btnOk.Parent.ClientHeight - btnClose.Height - 8;
            btnClose.Left = btnOk.Parent.ClientWidth - btnClose.Width - 8;
            
            btnFirst.Focused = true;
        }

        #endregion

        #region Init
        /*
        protected internal override void Init()
        {
            base.Init();
            MaximumWidth = 654;
        } // Init
        */
        #endregion

    } // TaskDialog
} // XNAFinalEngine.UI.Central
