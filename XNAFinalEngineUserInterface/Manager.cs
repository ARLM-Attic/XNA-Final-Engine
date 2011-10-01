
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
using System.Linq;
#if (!XBOX)
    using System.Windows.Forms;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// User Interface Manager.
    /// This user interface is prepared to work for editors and window applications only.
    /// However if gamepad support is added and a cursor is rendered then it could work in XBOX 360 perfectly.
    /// </summary>
    public static class UserInterfaceManager
    {

        #region Structs

        private struct ControlStates
        {
            public Control[] Buttons;
            public int Click;
            public Control Over;
        } // ControlStates

        #endregion

        #region Variables

        #if (!XBOX)
            /// <summary>
            /// Current cursor.
            /// </summary>
            private static Assets.Cursor cursor;

            /// <summary>
            /// Returns the form of the game runs in.
            /// </summary>
            private static Form window;
        #endif

        /// <summary>
        /// Main render target, when the UI will be render.
        /// </summary>
        private static RenderTarget renderTarget;
        private static ControlsList controls;
        private static ControlsList orderList;
        private static Control focusedControl;
        private static ModalContainer modalWindow;
        private static ControlStates states;

        #endregion

        #region Properties

        #if (!XBOX)
            
            /// <summary>
            /// Gets or sets an application cursor.
            /// </summary>
            public static Assets.Cursor Cursor
            {
                get { return cursor; }
                set
                {
                    cursor = value;
                    window.Cursor = value.SystemCursor;
                }
            } // Cursor
        #endif  

        /// <summary>
        /// Returns InputSystem instance responsible for managing user input.
        /// </summary>
        public static Input InputSystem { get; private set; }

        /// <summary>
        /// Returns list of controls added to the manager.
        /// </summary>
        public static ControlsList Controls { get { return controls; } }

        /// <summary>
        /// Gets or sets the time that passes before the ToolTip appears.
        /// </summary>
        public static int ToolTipDelay { get; set; }

        /// <summary>
        /// Gets or sets the time that passes before a submenu appears when hovered over menu item.
        /// </summary>
        public static int MenuDelay { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of milliseconds that can elapse between a first click and a second click to consider the mouse action a double-click.
        /// </summary>
        public static int DoubleClickTime { get; set; }

        /// <summary>
        /// Gets or sets texture size increment in pixel while performing controls resizing.
        /// </summary>
        public static int TextureResizeIncrement { get; set; }

        /// <summary>
        /// Enables or disables showing of tooltips globally.
        /// </summary>
        public static bool ToolTipsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if a control should unfocus if you click outside on the screen.
        /// </summary>
        public static bool AutoUnfocus { get; set; }

        /// <summary>
        /// Returns currently active modal window.
        /// </summary>
        public static ModalContainer ModalWindow
        {
            get
            {
                return modalWindow;
            }
            internal set
            {
                modalWindow = value;

                if (value != null)
                {
                    value.ModalResult = ModalResult.None;

                    value.Visible = true;
                    value.Focused = true;
                }
            }
        } // ModalWindow

        /// <summary>
        /// Returns currently focused control.
        /// </summary>
        public static Control FocusedControl
        {
            get
            {
                return focusedControl;
            }
            internal set
            {
                if (value != null && value.Visible && value.Enabled)
                {
                    if (value.CanFocus)
                    {
                        if (focusedControl == null || (focusedControl != null && value.Root != focusedControl.Root) || !value.IsRoot)
                        {
                            if (focusedControl != null && focusedControl != value)
                            {
                                focusedControl.Focused = false;
                            }
                            focusedControl = value;
                        }
                    }
                    else if (!value.CanFocus)
                    {
                        if (focusedControl != null && value.Root != focusedControl.Root)
                        {
                            if (focusedControl != value.Root)
                            {
                                focusedControl.Focused = false;
                            }
                            focusedControl = value.Root;
                        }
                        else if (focusedControl == null)
                        {
                            focusedControl = value.Root;
                        }
                    }
                    BringToFront(value.Root);
                }
                else if (value == null)
                {
                    focusedControl = value;
                }
            }
        } // FocusedControl

        internal static ControlsList OrderList { get { return orderList; } }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the GraphicsDevice settings are changed.
        /// </summary>
        public static event DeviceEventHandler DeviceSettingsChanged;

        /// <summary>
        /// Occurs when the skin is about to change.
        /// </summary>
        public static event SkinEventHandler SkinChanging;

        /// <summary>
        /// Occurs when the skin changes.
        /// </summary>
        public static event SkinEventHandler SkinChanged;

        /// <summary>
        /// Occurs when game window is about to close.
        /// </summary>
        public static event WindowClosingEventHandler WindowClosing;

        #endregion

        #region Init User Interface Manager

        /// <summary>
        /// Initializes the User Interface Manager.
        /// </summary>
        public static void InitUserInterfaceManager()
        {
            // Set some public parameters.
            TextureResizeIncrement = 32;
            ToolTipDelay = 500;
            AutoUnfocus = true;
            ToolTipsEnabled = true;
            
            #if (WINDOWS)
                MenuDelay = System.Windows.Forms.SystemInformation.MenuShowDelay;
                DoubleClickTime = System.Windows.Forms.SystemInformation.DoubleClickTime;
                window = (Form)System.Windows.Forms.Control.FromHandle(SystemInformation.GameWindow.Handle);
                window.FormClosing += FormClosing;
            #endif

            controls  = new ControlsList();
            orderList = new ControlsList();

            SystemInformation.GraphicsDeviceManager.PreparingDeviceSettings += PrepareGraphicsDevice;

            states.Buttons = new Control[32];
            states.Click = -1;
            states.Over = null;

            InputSystem = new Input();

            InputSystem.MouseDown  += MouseDownProcess;
            InputSystem.MouseUp    += MouseUpProcess;
            InputSystem.MousePress += MousePressProcess;
            InputSystem.MouseMove  += MouseMoveProcess;

            InputSystem.KeyDown    += KeyDownProcess;
            InputSystem.KeyUp      += KeyUpProcess;
            InputSystem.KeyPress   += KeyPressProcess;

            renderTarget = new RenderTarget(RenderTarget.SizeType.FullScreen, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);

            Renderer.Init();

            SetSkin("Default");
        } // Manager

        #endregion

        #region Form Closing

        #if (WINDOWS)
            /// <summary>
            /// If the form is closing
            /// </summary>
            private static void FormClosing(object sender, FormClosingEventArgs e)
            {
                bool ret = false;

                WindowClosingEventArgs ex = new WindowClosingEventArgs();
                if (WindowClosing != null)
                {   
                    SystemInformation.IsApplicationActive = true;
                    WindowClosing.Invoke(null, ex);
                    ret = ex.Cancel;
                }
                e.Cancel = ret;
            } // FormClosing
        #endif

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose controls.
        /// </summary>
        public static void DisposeControls()
        {
            // Recursively disposing all controls added to the manager and its child controls.
            if (controls != null)
            {
                int c = controls.Count;
                for (int i = 0; i < c; i++)
                {
                    if (controls.Count > 0) 
                        controls[0].Dispose();
                }
            }
        } // DisposeControls

        #endregion

        #region Prepare Graphics Device

        /// <summary>
        /// Method used as an event handler for the GraphicsDeviceManager.PreparingDeviceSettings event.
        /// </summary>
        private static void PrepareGraphicsDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            foreach (Control control in Controls)
            {
                SetMaxSize(control, SystemInformation.ScreenWidth, SystemInformation.ScreenHeight);
            }

            if (DeviceSettingsChanged != null) 
                DeviceSettingsChanged.Invoke(new DeviceEventArgs(e));
        } // PrepareGraphicsDevice

        private static void SetMaxSize(Control c, int w, int h)
        {
            if (c.Width > w)
            {
                w -= (c.SkinControlInformation != null) ? c.SkinControlInformation.OriginMargins.Horizontal : 0;
                c.Width = w;
            }
            if (c.Height > h)
            {
                h -= (c.SkinControlInformation != null) ? c.SkinControlInformation.OriginMargins.Vertical : 0;
                c.Height = h;
            }

            foreach (Control cx in c.ChildrenControls)
            {
                SetMaxSize(cx, w, h);
            }
        } // SetMaxSize

        #endregion

        #region Skin

        /// <summary>
        /// Sets a new skin.
        /// </summary>
        public static void SetSkin(string skinFilename)
        {
            if (SkinChanging != null) 
                SkinChanging.Invoke(new EventArgs());
            
            Skin.LoadSkin(skinFilename);

            #if (!XBOX)
                if (Skin.Cursors["Default"] != null)
                {
                    Cursor = Skin.Cursors["Default"].Cursor;
                }
            #endif
            
            // Initializing skins for every control created, even not visible or not added to the manager or another parent.
            foreach (Control control in Control.ControlList)
            {
                control.InitSkin();
            }
            
            if (SkinChanged != null) 
                SkinChanged.Invoke(new EventArgs());
            
            //  Initializing all controls created, even not visible or not added to the manager or another parent.
            foreach (Control control in Control.ControlList)
            {
                control.Init();
            }

        } // SetSkin

        #endregion

        #region Bring to front or back

        /// <summary>
        /// Brings the control to the front (z-order).
        /// </summary>
        /// <param name="control">The control being brought to the front.</param>
        public static void BringToFront(Control control)
        {
            if (control != null && !control.StayOnBack)
            {
                ControlsList cs = (control.Parent == null) ? controls : control.Parent.ChildrenControls;
                if (cs.Contains(control))
                {
                    cs.Remove(control);
                    if (!control.StayOnTop)
                    {
                        int pos = cs.Count;
                        for (int i = cs.Count - 1; i >= 0; i--)
                        {
                            if (!cs[i].StayOnTop)
                            {
                                break;
                            }
                            pos = i;
                        }
                        cs.Insert(pos, control);
                    }
                    else
                    {
                        cs.Add(control);
                    }
                }
            }
        } // BringToFront

        /// <summary>
        /// Sends the control to the back (z-order).
        /// </summary>
        /// <param name="control">The control being sent back.</param>
        public static void SendToBack(Control control)
        {
            if (control != null && !control.StayOnTop)
            {
                ControlsList cs = (control.Parent == null) ? controls : control.Parent.ChildrenControls as ControlsList;
                if (cs.Contains(control))
                {
                    cs.Remove(control);
                    if (!control.StayOnBack)
                    {
                        int pos = 0;
                        for (int i = 0; i < cs.Count; i++)
                        {
                            if (!cs[i].StayOnBack)
                            {
                                break;
                            }
                            pos = i;
                        }
                        cs.Insert(pos, control);
                    }
                    else
                    {
                        cs.Insert(0, control);
                    }
                }
            }
        } // SendToBack

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public static void Update()
        {
            try
            {
                // Init new controls.
                Control.InitializeNewControls();

                InputSystem.Update();

                ControlsList controlList = new ControlsList(controls);
                foreach (Control c in controlList)
                {
                    c.Update();
                }
                OrderList.Clear();
                SortLevel(controls);
            }
            catch (Exception exception)
            {
                throw new Exception("User Interface update failed.\n\n" + exception);
            }
        } // Update

        private static void SortLevel(ControlsList controlList)
        {
            if (controlList != null)
            {
                foreach (Control c in controlList.Where(c => c.Visible))
                {
                    OrderList.Add(c);
                    SortLevel(c.ChildrenControls as ControlsList);
                }
            }
        } // SortLevel

        #endregion

        #region Add or Remove

        /// <summary>
        /// Adds a component or a control to the manager.
        /// </summary>
        /// <param name="control">The control being added.</param>
        public static void Add(Control control)
        {
            if (control != null)
            {
                if (!controls.Contains(control))
                {
                    if (control.Parent != null) 
                        control.Parent.Remove(control);
                    controls.Add(control);
                    control.Parent = null;
                    if (focusedControl == null) 
                        control.Focused = true;
                    DeviceSettingsChanged += control.OnDeviceSettingsChanged;
                    SkinChanging += control.OnSkinChanging;
                    SkinChanged += control.OnSkinChanged;
                }
            }
        } // Add

        /// <summary>
        /// Removes a component or a control from the manager.
        /// </summary>
        /// <param name="control">The control being removed.</param>
        public static void Remove(Control control)
        {
            if (control != null)
            {
                SkinChanging -= control.OnSkinChanging;
                SkinChanged -= control.OnSkinChanged;
                DeviceSettingsChanged -= control.OnDeviceSettingsChanged;
                if (control.Focused) 
                    control.Focused = false;
                controls.Remove(control);
            }
        } // Remove

        #endregion

        #region Draw

        /// <summary>
        /// Renders all controls added to the manager.
        /// </summary>
        public static void BeginDraw()
        {
            if ((controls != null))
            {
                ControlsList list = new ControlsList();
                list.AddRange(controls);

                foreach (Control control in list)
                {
                    control.PreDrawControl();
                }

                renderTarget.EnableRenderTarget();
                SystemInformation.Device.Clear(Color.Transparent);
                foreach (Control control in list)
                {
                    control.Render();
                }
                renderTarget.DisableRenderTarget();
            }
            
        } // BeginDraw

        /// <summary>
        /// Draws texture resolved from RenderTarget to specified rectangle.
        /// </summary>
        public static void EndDraw()
        {
            Renderer.Begin();
                Renderer.Draw(renderTarget.XnaTexture, new Rectangle(0, 0, SystemInformation.ScreenWidth, SystemInformation.ScreenHeight), Color.White);
            Renderer.End();
        } // EndDraw

        #endregion
        
        #region Input

        private static bool CheckParent(Control control, Point pos)
        {
            if (control.Parent != null && !CheckDetached(control))
            {
                Control parent = control.Parent;
                Control root = control.Root;

                Rectangle pr = new Rectangle(parent.ControlLeftAbsoluteCoordinate,
                                             parent.ControlTopAbsoluteCoordinate,
                                             parent.Width,
                                             parent.Height);

                Margins margins = root.SkinControlInformation.ClientMargins;
                Rectangle rr = new Rectangle(root.ControlLeftAbsoluteCoordinate + margins.Left,
                                             root.ControlTopAbsoluteCoordinate + margins.Top,
                                             root.ControlAndMarginsWidth - margins.Horizontal,
                                             root.ControlAndMarginsHeight - margins.Vertical);


                return (rr.Contains(pos) && pr.Contains(pos));
            }

            return true;
        } // CheckParent

        private static bool CheckState(Control control)
        {
            bool modal = (ModalWindow == null) ? true : (ModalWindow == control.Root);

            return (control != null && !control.Passive && control.Visible && control.Enabled && modal);
        } // CheckState

        private static bool CheckOrder(Control control, Point pos)
        {
            if (!CheckPosition(control, pos)) return false;

            for (int i = OrderList.Count - 1; i > OrderList.IndexOf(control); i--)
            {
                Control c = OrderList[i];

                if (!c.Passive && CheckPosition(c, pos) && CheckParent(c, pos))
                {
                    return false;
                }
            }

            return true;
        } // CheckOrder

        private static bool CheckDetached(Control control)
        {
            bool ret = control.Detached;
            if (control.Parent != null)
            {
                if (CheckDetached(control.Parent)) ret = true;
            }
            return ret;
        } // CheckDetached

        private static bool CheckPosition(Control control, Point pos)
        {
            return (control.ControlLeftAbsoluteCoordinate <= pos.X &&
                    control.ControlTopAbsoluteCoordinate <= pos.Y &&
                    control.ControlLeftAbsoluteCoordinate + control.Width >= pos.X &&
                    control.ControlTopAbsoluteCoordinate + control.Height >= pos.Y &&
                    CheckParent(control, pos));
        } // CheckPosition

        private static bool CheckButtons(int index)
        {
            return states.Buttons.Where((t, i) => i != index).All(t => t == null);
        } // CheckButtons

        private static void TabNextControl(Control control)
        {
            int start = OrderList.IndexOf(control);
            int i = start;

            do
            {
                if (i < OrderList.Count - 1) 
                    i++;
                else
                    i = 0;
            }
            while ((OrderList[i].Root != control.Root || !OrderList[i].CanFocus || OrderList[i].IsRoot || !OrderList[i].Enabled) && i != start);

            OrderList[i].Focused = true;
        } // TabNextControl

        private static void TabPrevControl(Control control)
        {
            int start = OrderList.IndexOf(control);
            int i = start;

            do
            {
                if (i > 0) i -= 1;
                else i = OrderList.Count - 1;
            }
            while ((OrderList[i].Root != control.Root || !OrderList[i].CanFocus || OrderList[i].IsRoot || !OrderList[i].Enabled) && i != start);
            OrderList[i].Focused = true;
        } // TabPrevControl

        private static void ProcessArrows(Control control, KeyEventArgs kbe)
        {
            Control c = control;
            if (c.Parent != null && c.Parent.ChildrenControls != null)
            {
                int index = -1;

                if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Left && !kbe.Handled)
                {
                    int miny = int.MaxValue;
                    int minx = int.MinValue;
                    for (int i = 0; i < ((ControlsList)c.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = (c.Parent.ChildrenControls as ControlsList)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cay = c.Top + (c.Height / 2);
                        int cby = cx.Top + (cx.Height / 2);

                        if (Math.Abs(cay - cby) <= miny && (cx.Left + cx.Width) >= minx && (cx.Left + cx.Width) <= c.Left)
                        {
                            miny = Math.Abs(cay - cby);
                            minx = cx.Left + cx.Width;
                            index = i;
                        }
                    }
                }
                else if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Right && !kbe.Handled)
                {
                    int miny = int.MaxValue;
                    int minx = int.MaxValue;
                    for (int i = 0; i < ((ControlsList)c.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = ((ControlsList)c.Parent.ChildrenControls)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cay = c.Top + (c.Height / 2);
                        int cby = cx.Top + (cx.Height / 2);

                        if (Math.Abs(cay - cby) <= miny && cx.Left <= minx && cx.Left >= (c.Left + c.Width))
                        {
                            miny = Math.Abs(cay - cby);
                            minx = cx.Left;
                            index = i;
                        }
                    }
                }
                else if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Up && !kbe.Handled)
                {
                    int miny = int.MinValue;
                    int minx = int.MaxValue;
                    for (int i = 0; i < (c.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = (c.Parent.ChildrenControls)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cax = c.Left + (c.Width / 2);
                        int cbx = cx.Left + (cx.Width / 2);

                        if (Math.Abs(cax - cbx) <= minx && (cx.Top + cx.Height) >= miny && (cx.Top + cx.Height) <= c.Top)
                        {
                            minx = Math.Abs(cax - cbx);
                            miny = cx.Top + cx.Height;
                            index = i;
                        }
                    }
                }
                else if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Down && !kbe.Handled)
                {
                    int miny = int.MaxValue;
                    int minx = int.MaxValue;
                    for (int i = 0; i < (c.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = (c.Parent.ChildrenControls)[i];
                        if (cx == c || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cax = c.Left + (c.Width / 2);
                        int cbx = cx.Left + (cx.Width / 2);

                        if (Math.Abs(cax - cbx) <= minx && cx.Top <= miny && cx.Top >= (c.Top + c.Height))
                        {
                            minx = Math.Abs(cax - cbx);
                            miny = cx.Top;
                            index = i;
                        }
                    }
                }

                if (index != -1)
                {
                    ((ControlsList)c.Parent.ChildrenControls)[index].Focused = true;
                    kbe.Handled = true;
                }
            }
        } // ProcessArrows

        private static void MouseDownProcess(object sender, MouseEventArgs e)
        {
            ControlsList c = new ControlsList();
            c.AddRange(OrderList);

            if (AutoUnfocus && focusedControl != null && focusedControl.Root != modalWindow)
            {
                bool hit = Controls.Any(cx => cx.ControlRectangle.Contains(e.Position));

                if (!hit)
                {
                    if (Control.ControlList.Any(t => t.Visible && t.Detached && t.ControlRectangle.Contains(e.Position)))
                    {
                        hit = true;
                    }
                }
                if (!hit) focusedControl.Focused = false;
            }

            for (int i = c.Count - 1; i >= 0; i--)
            {
                if (CheckState(c[i]) && CheckPosition(c[i], e.Position))
                {
                    states.Buttons[(int)e.Button] = c[i];
                    c[i].SendMessage(Message.MouseDown, e);

                    if (states.Click == -1)
                    {
                        states.Click = (int)e.Button;

                        if (FocusedControl != null)
                        {
                            FocusedControl.Invalidate();
                        }
                        c[i].Focused = true;
                    }
                    return;
                }
            }

            if (ModalWindow != null)
            {
                //SystemSounds.Beep.Play();
            }
            else // If we click the window background. This prevent a bug.
            {
                FocusedControl = null;
            }
        } // MouseDownProcess

        private static void MouseUpProcess(object sender, MouseEventArgs e)
        {
            Control c = states.Buttons[(int)e.Button];
            if (c != null)
            {
                if (CheckPosition(c, e.Position) && CheckOrder(c, e.Position) && states.Click == (int)e.Button && CheckButtons((int)e.Button))
                {
                    c.SendMessage(Message.Click, e);
                }
                states.Click = -1;
                c.SendMessage(Message.MouseUp, e);
                states.Buttons[(int)e.Button] = null;
                MouseMoveProcess(sender, e);
            }
        } // MouseUpProcess

        private static void MousePressProcess(object sender, MouseEventArgs e)
        {
            Control c = states.Buttons[(int)e.Button];
            if (c != null)
            {
                if (CheckPosition(c, e.Position))
                {
                    c.SendMessage(Message.MousePress, e);
                }
            }
        } // MousePressProcess

        private static void MouseMoveProcess(object sender, MouseEventArgs e)
        {
            ControlsList c = new ControlsList();
            c.AddRange(OrderList);

            for (int i = c.Count - 1; i >= 0; i--)
            {
                bool chpos = CheckPosition(c[i], e.Position);
                bool chsta = CheckState(c[i]);

                if (chsta && ((chpos && states.Over == c[i]) || (states.Buttons[(int)e.Button] == c[i])))
                {
                    c[i].SendMessage(Message.MouseMove, e);
                    break;
                }
            }

            for (int i = c.Count - 1; i >= 0; i--)
            {
                bool chpos = CheckPosition(c[i], e.Position);
                bool chsta = CheckState(c[i]) || (!string.IsNullOrEmpty(c[i].ToolTip.Text) && c[i].Visible);

                if (chsta && !chpos && states.Over == c[i] && states.Buttons[(int)e.Button] == null)
                {
                    states.Over = null;
                    c[i].SendMessage(Message.MouseOut, e);
                    break;
                }
            }

            for (int i = c.Count - 1; i >= 0; i--)
            {
                bool chpos = CheckPosition(c[i], e.Position);
                bool chsta = CheckState(c[i]) || (!string.IsNullOrEmpty(c[i].ToolTip.Text) && c[i].Visible);

                if (chsta && chpos && states.Over != c[i] && states.Buttons[(int)e.Button] == null)
                {
                    if (states.Over != null)
                    {
                        states.Over.SendMessage(Message.MouseOut, e);
                    }
                    states.Over = c[i];
                    c[i].SendMessage(Message.MouseOver, e);
                    break;
                }
                if (states.Over == c[i]) break;
            }
        } // MouseMoveProcess

        private static void KeyDownProcess(object sender, KeyEventArgs e)
        {
            Control c = FocusedControl;

            if (c != null && CheckState(c))
            {
                if (states.Click == -1)
                {
                    states.Click = (int)MouseButton.None;
                }
                states.Buttons[(int)MouseButton.None] = c;
                c.SendMessage(Message.KeyDown, e);

                if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
                {
                    c.SendMessage(Message.Click, new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));
                }
            }
        } // KeyDownProcess

        private static void KeyUpProcess(object sender, KeyEventArgs e)
        {
            Control c = states.Buttons[(int)MouseButton.None];

            if (c != null)
            {
                if (e.Key == Microsoft.Xna.Framework.Input.Keys.Space)
                {
                    c.SendMessage(Message.Click, new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));
                }
                states.Click = -1;
                states.Buttons[(int)MouseButton.None] = null;
                c.SendMessage(Message.KeyUp, e);
            }
        } // KeyUpProcess

        private static void KeyPressProcess(object sender, KeyEventArgs e)
        {
            Control c = states.Buttons[(int)MouseButton.None];
            if (c != null)
            {
                c.SendMessage(Message.KeyPress, e);

                if ((e.Key == Microsoft.Xna.Framework.Input.Keys.Right ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Left ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Up ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Down) && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    ProcessArrows(c, e);
                    KeyDownProcess(sender, e);
                }
                else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Tab && !e.Shift && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    TabNextControl(c);
                    KeyDownProcess(sender, e);
                }
                else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Tab && e.Shift && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    TabPrevControl(c);
                    KeyDownProcess(sender, e);
                }
            }
        } // KeyPressProcess

        #endregion

    } // UserInterfaceManager
} // // XNAFinalEngine.UserInterface