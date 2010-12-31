
namespace XNAFinalEngine.UI.Central
{
    public class TaskEvents : Window
    {

        #region Variables
         
        private Button btn;
        private ListBox lst;
        private ListBox txt;

        #endregion

        #region Constructor

        public TaskEvents()
        {
            Height = 360;
            MinimumHeight = 99;
            MinimumWidth = 78;
            Text = "Events Test";
            CenterWindow();

            btn = new Button {Parent = this, Left = 20, Top = 20};
            btn.MouseMove += btn_MouseMove;
            btn.MouseDown += btn_MouseDown;
            btn.MouseUp += btn_MouseUp;
            btn.MouseOver += btn_MouseOver;
            btn.MouseOut += btn_MouseOut;
            btn.MousePress += btn_MousePress;
            btn.Click += btn_Click;

            lst = new ListBox {Parent = this, Left = 20, Top = 60, Width = 128, Height = 128};
            lst.MouseMove += btn_MouseMove;
            lst.MouseDown += btn_MouseDown;
            lst.MouseUp += btn_MouseUp;
            lst.MouseOver += btn_MouseOver;
            lst.MouseOut += btn_MouseOut;
            lst.MousePress += btn_MousePress;
            lst.Click += btn_Click;

            txt = new ListBox {Parent = this, Left = 200, Top = 8, Width = 160, Height = 300};
        }

        void btn_Click(object sender, EventArgs e)
        {
            MouseEventArgs ex = (e is MouseEventArgs) ? (MouseEventArgs)e : new MouseEventArgs();
            txt.Items.Add((sender == btn ? "Button" : "List") + ": Click " + ex.Button);
            txt.ItemIndex = txt.Items.Count - 1;
        }

        static void btn_MousePress(object sender, MouseEventArgs e)
        {
            //  txt.Items.Add((sender == btn ? "Button" : "List") + ": Press");
            //  txt.ItemIndex = txt.Items.Count - 1;
        }

        void btn_MouseOut(object sender, MouseEventArgs e)
        {
            txt.Items.Add((sender == btn ? "Button" : "List") + ": Out");
            txt.ItemIndex = txt.Items.Count - 1;
        }

        void btn_MouseOver(object sender, MouseEventArgs e)
        {
            txt.Items.Add((sender == btn ? "Button" : "List") + ": Over");
            txt.ItemIndex = txt.Items.Count - 1;
        }

        void btn_MouseUp(object sender, MouseEventArgs e)
        {
            txt.Items.Add((sender == btn ? "Button" : "List") + ": Up " + e.Button);
            txt.ItemIndex = txt.Items.Count - 1;
        }

        void btn_MouseDown(object sender, MouseEventArgs e)
        {
            txt.Items.Add((sender == btn ? "Button" : "List") + ": Down " + e.Button);
            txt.ItemIndex = txt.Items.Count - 1;
        }

        void btn_MouseMove(object sender, MouseEventArgs e)
        {
            txt.Items.Add((sender == btn ? "Button" : "List") + ": Move");
            txt.ItemIndex = txt.Items.Count - 1;
        }

        #endregion

    } // TaskEvents
} // XNAFinalEngine.UI.Central