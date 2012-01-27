
// This class came from Neo Force Tutorial and is not formatted correctly.

#region Using directives
using System;
using Microsoft.Xna.Framework;
using Controls = XNAFinalEngine.UserInterface;
#endregion

namespace XNAFinalEngine.UserInterface.Central
{
    public class TaskControls : Dialog
    {

        #region Variables
   
        private GroupPanel grpEdit;
        private Panel pnlControls;
        private Label lblEdit;
        private TextBox txtEdit;
        private RadioButton rdbNormal;
        private RadioButton rdbPassword;
        private CheckBox chkReadOnly;
        private CheckBox chkBorders;
        private TrackBar trkMain;
        private Label lblTrack;

        private ListBox lstMain;
        private ComboBox cmbMain;
        private SpinBox spnMain;
        private ProgressBar prgMain;
        private Button btnDisable;
        private Button btnProgress;
        private ContextMenu mnuListBox;
        private MainMenu mnuMain;

        #endregion

        #region Constructor

        public TaskControls()
        {
            MinimumWidth = 340;
            MinimumHeight = 140;
            Height = 480;
            CenterWindow();
            Text = "Controls Test";

            TopPanel.Visible = true;
            Caption.Text = "Information";
            Description.Text = "Demonstration of various controls available in Neoforce Controls library.";
            Caption.TextColor = Description.TextColor = new Color(96, 96, 96);

            grpEdit = new GroupPanel
            {
                Parent = this,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = ClientWidth - 200,
                Height = 160,
                Left = 8,
                Top = TopPanel.Height + 8,
                Text = "EditBox"
            };
            
            pnlControls = new Panel
            {
                Passive = true,
                Parent = this,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Left = 8,
                Top = grpEdit.Top + grpEdit.Height + 8,
                Width = ClientWidth - 200,
                BevelBorder = BevelBorder.All,
                BevelMargin = 1,
                BevelStyle = BevelStyle.Etched,
                Color = Color.Transparent
            };
            pnlControls.Height = BottomPanel.Top - 32 - pnlControls.Top;
            
            lblEdit = new Label
            {
                Parent = grpEdit,
                Left = 16,
                Top = 8,
                Text = "Testing field:",
                Width = 128,
                Height = 16,
            };

            txtEdit = new TextBox
            {
                Parent = grpEdit,
                Left = 16,
                Top = 24,
                Width = grpEdit.ClientWidth - 32,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right | Anchors.Bottom,
                Text = "Text"
            };

            rdbNormal = new RadioButton
            {
                Parent = grpEdit,
                Left = 16,
                Top = 52,
                Width = grpEdit.ClientWidth - 32,
                Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right,
                Checked = true,
                Text = "Normal mode",
                ToolTip = {Text = "Enables normal mode for TextBox control."}
            };
            rdbNormal.CheckedChanged += ModeChanged;

            rdbPassword = new RadioButton
            {
                Parent = grpEdit,
                Left = 16,
                Top = 68,
                Width = grpEdit.ClientWidth - 32,
                Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right,
                Checked = false,
                Text = "Password mode",
                ToolTip = {Text = "Enables password mode for TextBox control."}
            };
            rdbPassword.CheckedChanged += ModeChanged;

            chkBorders = new CheckBox
            {
                Parent = grpEdit,
                Left = 16,
                Top = 96,
                Width = grpEdit.ClientWidth - 32,
                Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right,
                Checked = false,
                Text = "Borderless mode",
                ToolTip = {Text = "Enables or disables borderless mode for TextBox control."}
            };
            chkBorders.CheckedChanged += chkBorders_CheckedChanged;

            chkReadOnly = new CheckBox
            {
                Parent = grpEdit,
                Left = 16,
                Top = 110,
                Width = grpEdit.ClientWidth - 32,
                Anchor = Anchors.Left | Anchors.Bottom | Anchors.Right,
                Checked = false,
                Text = "Read only mode",
                ToolTip =
                    {
                        Text = "Enables or disables read only mode for TextBox control.\nThis mode is necessary to enable explicitly."
                    }
            };
            chkReadOnly.CheckedChanged += chkReadOnly_CheckedChanged;

            string[] colors = new string[] {"Red", "Green", "Blue", "Yellow", "Orange", "Purple", "White", "Black", "Magenta", "Cyan",
                                      "Brown", "Aqua", "Beige", "Coral", "Crimson", "Gray", "Azure", "Ivory", "Indigo", "Khaki",
                                      "Orchid", "Plum", "Salmon", "Silver", "Gold", "Pink", "Linen", "Lime", "Olive", "Slate"};

            spnMain = new SpinBox(SpinBoxMode.List)
            {
                Parent = pnlControls,
                Left = 16,
                Top = 16,
                Width = pnlControls.Width - 32,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Mode = SpinBoxMode.Range,
                ItemIndex = 0
            };
            spnMain.Items.AddRange(colors);
            
            cmbMain = new ComboBox
            {
                Parent = pnlControls,
                Left = 16,
                Top = 44,
                Width = pnlControls.Width - 32,
                Height = 20,
                ReadOnly = true,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                ItemIndex = 0,
                MaxItemsShow = 5,
                ToolTip = {Color = Color.Yellow}
            };
            cmbMain.Movable = cmbMain.Resizable = true;
            cmbMain.OutlineMoving = cmbMain.OutlineResizing = true;
            cmbMain.Items.AddRange(colors);
            
            trkMain = new TrackBar
            {
                Parent = pnlControls,
                Left = 16,
                Top = 72,
                Width = pnlControls.Width - 32,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Value = 16
            };
            trkMain.ValueChanged += trkMain_ValueChanged;

            lblTrack = new Label
            {
                Parent = pnlControls,
                Left = 16,
                Top = 96,
                Width = pnlControls.Width - 32,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Alignment = Alignment.TopRight,
                TextColor = new Color(32, 32, 32)
            };
            trkMain_ValueChanged(this, null); // forcing label redraw with init values

            mnuListBox = new ContextMenu();

            MenuItem i1 = new MenuItem("This is very long text");
            MenuItem i2 = new MenuItem("Menu", true);
            MenuItem i3 = new MenuItem("Item", false);
            MenuItem i4 = new MenuItem("Separated", true);

            MenuItem i11 = new MenuItem();
            MenuItem i12 = new MenuItem();
            MenuItem i13 = new MenuItem();
            MenuItem i14 = new MenuItem();

            MenuItem i111 = new MenuItem();
            MenuItem i112 = new MenuItem();
            MenuItem i113 = new MenuItem();
            
            mnuListBox.Items.AddRange(new MenuItem[] { i1, i2, i3, i4 });
            i2.ChildrenItems.AddRange(new MenuItem[] { i11, i12, i13, i14 });
            i13.ChildrenItems.AddRange(new MenuItem[] { i111, i112, i113 });
            
            lstMain = new ListBox
            {
                Parent = this,
                Top = TopPanel.Height + 8,
                Left = grpEdit.Left + grpEdit.Width + 8,
                Height = ClientHeight - 16 - BottomPanel.Height - TopPanel.Height,
                Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom,
                HideSelection = false,
                ContextMenu = mnuListBox
            };
            lstMain.Width = ClientWidth - lstMain.Left - 8;
            lstMain.Items.AddRange(colors);
            

            prgMain = new ProgressBar
            {
                Parent = BottomPanel,
                Left = lstMain.Left,
                Top = 10,
                Width = lstMain.Width,
                Height = 16,
                Anchor = Anchors.Top | Anchors.Right,
                Mode = ProgressBarMode.Infinite,
                Passive = false
            };

            btnDisable = new Button
            {
                Parent = BottomPanel,
                Left = 8,
                Top = 8,
                Text = "Disable",
                TextColor = Color.FromNonPremultiplied(255, 64, 32, 200)
            };
            btnDisable.Click += btnDisable_Click;
            

            btnProgress = new Button
            {
                Parent = BottomPanel,
                Left = prgMain.Left - 16,
                Top = prgMain.Top,
                Height = 16,
                Width = 16,
                Text = "!",
                Anchor = Anchors.Top | Anchors.Right
            };
            btnProgress.Click += btnProgress_Click;

            mnuMain = new MainMenu {Width = Width, Anchor = Anchors.Left | Anchors.Top };
            
            mnuMain.Items.Add(i2);
            mnuMain.Items.Add(i13);
            mnuMain.Items.Add(i3);
            mnuMain.Items.Add(i4);
            //Add(mnuMain);

            ToolBarPanel tlp = new ToolBarPanel();
            ToolBarPanel = tlp;

            ToolBar tlb = new ToolBar
                              {
                                  Parent = tlp,
                                  Movable = true
                              };
            ToolBar tlbx = new ToolBar
                               {
                                   Parent = tlp,
                                   Movable = true
                               };
            tlb.FullRow = true;
            tlbx.Row = 1;
            tlbx.FullRow = false;

            StatusBar stb = new StatusBar();
            StatusBar = stb;
            Label t = new Label
                          {
                              Parent = stb,
                              Text = "Status Bar", Left = 5, Top = 5
                          };

            DefaultControl = txtEdit;

            OutlineMoving = true;
            OutlineResizing = true;

            BottomPanel.BringToFront();

            SkinChanged += TaskControls_SkinChanged;
            TaskControls_SkinChanged(null, null);
        }


        #endregion

        #region Methods

        void TaskControls_SkinChanged(object sender, EventArgs e)
        {
            #if (!XBOX)
                prgMain.Cursor = Skin.Cursors["Busy"].Cursor;
            #endif

        }

        void ModeChanged(object sender, EventArgs e)
        {
            if (sender == rdbNormal)
            {
                txtEdit.Mode = TextBoxMode.Normal;
            }
            else if (sender == rdbPassword)
            {
                txtEdit.Mode = TextBoxMode.Password;
            }
        }

        void chkReadOnly_CheckedChanged(object sender, EventArgs e)
        {
            txtEdit.ReadOnly = chkReadOnly.Checked;
        }

        void chkBorders_CheckedChanged(object sender, EventArgs e)
        {
            txtEdit.DrawBorders = !chkBorders.Checked;
        }

        void btnDisable_Click(object sender, EventArgs e)
        {
            if (txtEdit.Enabled)
            {
                btnDisable.Text = "Enable";
                btnDisable.TextColor = Color.FromNonPremultiplied(64, 255, 32, 200);
            }
            else
            {
                btnDisable.Text = "Disable";
                btnDisable.TextColor = Color.FromNonPremultiplied(255, 64, 32, 200);
            }
            ClientArea.Enabled = !ClientArea.Enabled;

            BottomPanel.Enabled = true;

            prgMain.Enabled = ClientArea.Enabled;
        }

        void btnProgress_Click(object sender, EventArgs e)
        {
            if (prgMain.Mode == ProgressBarMode.Default) prgMain.Mode = ProgressBarMode.Infinite;
            else prgMain.Mode = ProgressBarMode.Default;

            lstMain.Items.Add(new Random().Next().ToString());
            lstMain.ItemIndex = lstMain.Items.Count - 1;
            cmbMain.Text = "!!!";
        }

        void trkMain_ValueChanged(object sender, EventArgs e)
        {/*
            if (lblTrack != null)
            {
                lblTrack.Text = trkMain.Value + "/" + trkMain.Range;
            }*/
        }

        #endregion

    } // TaskControls
} // XNAFinalEngine.UI.Central
