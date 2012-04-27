
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
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using Screen = XNAFinalEngine.EngineCore.Screen;
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

        #region Script Class

        /// <summary>
        /// Used to call the UI's update and render methods in the correct order without explicit calls. 
        /// </summary>
        /// <remarks>
        /// Most XNA Final Engine managers don’t work this way because the GameLoop class controls their functionality.
        /// This manager is in a higher level because of the GPL license (the code is based in Neo Force Controls) and because is not garbage free.
        /// I do baste improvement in the code and functionality of this UI but I’m not disposed to waste more time in this endeavor.
        /// The UI has a purpose and in its current state can accomplished.
        /// </remarks>
        private sealed class ScripUserInterface : Script
        {

            /// <summary>
            /// Update.
            /// </summary>
            public override void Update()
            {
                UserInterfaceManager.Update();
            }

            /// <summary>
            /// Tasks executed during the first stage of the scene render.
            /// </summary>
            public override void PreRenderUpdate()
            {
                UserInterfaceManager.DrawToTexture();
            }

            /// <summary>
            /// Tasks executed during the last stage of the scene render.
            /// </summary>
            public override void PostRenderUpdate()
            {
                UserInterfaceManager.DrawTextureToScreen();
            }

        } // ScripUserInterface

        #endregion

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

        // Main render target, when the UI will be render.
        private static RenderTarget renderTarget;

        private static Control focusedControl;
        private static ModalContainer modalWindow;
        private static ControlStates states;

        // To avoid more than one initialization.
        private static bool initialized;

        // Used to call the update and render method in the correct order without explicit calls.
        private static GameObject userInterfaceGameObject;

        // Used to generate the resize event.
        private static int oldScreenWidth, oldScreenHeight;

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
                    window.Cursor = value.Resource;
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
        public static ControlsList RootControls { get; private set; }

        internal static ControlsList OrderList { get; private set; }

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
            get { return modalWindow; }
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
            get { return focusedControl; }
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
        
        #endregion

        #region Events

        /// <summary>
        /// Occurs when the GraphicsDevice settings are changed.
        /// </summary>
        internal static event DeviceEventHandler DeviceSettingsChanged;

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

        /// <summary>
        /// Occurs with the window change its size.
        /// </summary>
        public static event ResizeEventHandler WindowResize;

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes the User Interface Manager.
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
                return;
            try
            {
                initialized = true;
                // Set some public parameters.
                TextureResizeIncrement = 32;
                ToolTipDelay = 500;
                AutoUnfocus = true;
                ToolTipsEnabled = true;
            
                #if (WINDOWS)
                    MenuDelay = SystemInformation.MenuShowDelay;
                    DoubleClickTime = SystemInformation.DoubleClickTime;
                    window = (Form)System.Windows.Forms.Control.FromHandle(EngineManager.GameWindow.Handle);
                    window.FormClosing += FormClosing;
                #endif

                RootControls  = new ControlsList();
                OrderList = new ControlsList();

                EngineManager.GraphicsDeviceManager.PreparingDeviceSettings += OnPrepareGraphicsDevice;

                states.Buttons = new Control[32];
                states.Click = -1;
                states.Over = null;

                // Input events
                InputSystem = new Input();
                InputSystem.MouseDown  += MouseDownProcess;
                InputSystem.MouseUp    += MouseUpProcess;
                InputSystem.MousePress += MousePressProcess;
                InputSystem.MouseMove  += MouseMoveProcess;
                InputSystem.KeyDown    += KeyDownProcess;
                InputSystem.KeyUp      += KeyUpProcess;
                InputSystem.KeyPress   += KeyPressProcess;

                // Final render target.
                renderTarget = new RenderTarget(Helpers.Size.FullScreen, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing)
                {
                    Name = "User Interface Render Target"
                };

                // Init User Interface Renderer.
                Renderer.Init();

                // Set Default skin.
                SetSkin("Default");

                // Window resize.
                oldScreenWidth = Screen.Width;
                oldScreenHeight = Screen.Height;
                Screen.ScreenSizeChanged += OnScreenSizeChanged;

                // To automatically update and render.
                userInterfaceGameObject = new GameObject2D();
                userInterfaceGameObject.AddComponent<ScripUserInterface>();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("User Interface Manager: Error occurred during initialization. Was the engine started?", e);
            }
        } // Initialize

        #endregion

        #region On Prepare Graphics Device

        /// <summary>
        /// If the device is recreated then the controls have to be invalidated so that they redraw.
        /// </summary>
        private static void OnPrepareGraphicsDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if (DeviceSettingsChanged != null)
                DeviceSettingsChanged.Invoke(new DeviceEventArgs(e));
        } // OnPrepareGraphicsDevice

        #endregion

        #region On Screen Size Changed

        /// <summary>
        /// Raised when the window size changes.
        /// </summary>
        private static void OnScreenSizeChanged(object sender, System.EventArgs e)
        {
            if (WindowResize != null)
                WindowResize.Invoke(null, new ResizeEventArgs(EngineManager.Device.PresentationParameters.BackBufferWidth,
                                                              EngineManager.Device.PresentationParameters.BackBufferHeight,
                                                              oldScreenWidth, oldScreenHeight));
            oldScreenWidth = EngineManager.Device.PresentationParameters.BackBufferWidth;
            oldScreenHeight = EngineManager.Device.PresentationParameters.BackBufferHeight;
        } // OnScreenSizeChanged

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
                    WindowClosing.Invoke(null, ex);
                    ret = ex.Cancel;
                }
                e.Cancel = ret;
            } // FormClosing

        #endif

        #endregion

        #region Dispose Controls

        /// <summary>
        /// Dispose all controls added to the manager and its child controls.
        /// </summary>
        public static void DisposeControls()
        {
            try
            {
                for (int i = 0; i < RootControls.Count; i++)
                {
                    RootControls[i].Dispose();
                }
                RootControls.Clear();
                OrderList.Clear();
                FocusedControl = null;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("User Interface Manager: Unable to dispose controls. Was the User Interface Manager started?", e);
            }
        } // DisposeControls

        #endregion
        
        #region Set Skin

        /// <summary>
        /// Sets a new skin.
        /// </summary>
        public static void SetSkin(string skinName)
        {
            if (SkinChanging != null) 
                SkinChanging.Invoke(new EventArgs());
            
            Skin.LoadSkin(skinName);

            #if (!XBOX)
                if (Skin.Cursors["Default"] != null)
                {
                    Cursor = Skin.Cursors["Default"].Cursor;
                }
            #endif
            
            // Initializing skins for every control created, even not visible or not added to the manager or another control.
            foreach (Control control in Control.ControlList)
            {
                control.InitSkin();
            }
            
            if (SkinChanged != null) 
                SkinChanged.Invoke(new EventArgs());

            //  Initializing all controls created, even not visible or not added to the manager or another control.
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
        internal static void BringToFront(Control control)
        {
            if (control != null && !control.StayOnBack)
            {
                // We search for the control's brothers.
                ControlsList brotherControls = (control.Parent == null) ? RootControls : control.Parent.ChildrenControls;
                if (brotherControls.Contains(control)) // The only case in which is false is when the control was not added to anything.
                {
                    brotherControls.Remove(control);
                    if (!control.StayOnTop)
                    {
                        // We try to insert the control the higher that we can in the sorted list
                        int newControlPosition = brotherControls.Count;
                        for (int i = brotherControls.Count - 1; i >= 0; i--)
                        {
                            if (!brotherControls[i].StayOnTop) // If there is a control that has to be in top then we won't go any further.
                                break;
                            newControlPosition = i;
                        }
                        brotherControls.Insert(newControlPosition, control);
                    }
                    else
                    {
                        brotherControls.Add(control);
                    }
                }
            }
        } // BringToFront

        /// <summary>
        /// Sends the control to the back (z-order).
        /// </summary>
        /// <param name="control">The control being sent back.</param>
        internal static void SendToBack(Control control)
        {
            if (control != null && !control.StayOnTop)
            {
                ControlsList brotherControls = (control.Parent == null) ? RootControls : control.Parent.ChildrenControls;
                if (brotherControls.Contains(control))
                {
                    brotherControls.Remove(control);
                    if (!control.StayOnBack)
                    {
                        int newControlPosition = 0;
                        for (int i = 0; i < brotherControls.Count; i++)
                        {
                            if (!brotherControls[i].StayOnBack)
                                break;
                            newControlPosition = i;
                        }
                        brotherControls.Insert(newControlPosition, control);
                    }
                    else
                    {
                        brotherControls.Insert(0, control);
                    }
                }
            }
        } // SendToBack

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        private static void Update()
        {
            try
            {
                // Init new controls.
                Control.InitializeNewControls();

                InputSystem.Update();

                // In the control's update the Root Control list could be modified so we need to create an auxiliary list.
                ControlsList controlList = new ControlsList(RootControls);
                foreach (Control control in controlList)
                {
                    control.Update();
                }
                OrderList.Clear();
                SortLevel(RootControls);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("User Interface Manager: Update failed.", exception);
            }
        } // Update

        /// <summary>
        /// Sort the control and their children.
        /// </summary>
        /// <param name="controlList"></param>
        private static void SortLevel(ControlsList controlList)
        {
            if (controlList != null)
            {
                for (int i = 0; i < controlList.Count; i++)
                {
                    if (controlList[i].Visible)
                    {
                        OrderList.Add(controlList[i]);
                        SortLevel(controlList[i].ChildrenControls);
                    }
                }
            }
        } // SortLevel

        #endregion

        #region Add or Remove

        /// <summary>
        /// Adds a control to the manager.
        /// </summary>
        /// <param name="control">The control being added.</param>
        internal static void Add(Control control)
        {
            if (control != null)
            {
                if (!RootControls.Contains(control))
                {
                    if (control.Parent != null) 
                        control.Parent.Remove(control);
                    RootControls.Add(control);
                    control.Parent = null;
                    if (focusedControl == null) 
                        control.Focused = true;
                    DeviceSettingsChanged += control.OnDeviceSettingsChanged;
                    SkinChanging += control.OnSkinChanging;
                    SkinChanged += control.OnSkinChanged;
                    WindowResize += control.OnParentResize;
                }
            }
        } // Add

        /// <summary>
        /// Removes a component or a control from the manager.
        /// </summary>
        /// <param name="control">The control being removed.</param>
        internal static void Remove(Control control)
        {
            if (control != null)
            {
                SkinChanging -= control.OnSkinChanging;
                SkinChanged -= control.OnSkinChanged;
                DeviceSettingsChanged -= control.OnDeviceSettingsChanged;
                WindowResize -= control.OnParentResize;
                if (control.Focused) 
                    control.Focused = false;
                RootControls.Remove(control);
            }
        } // Remove

        #endregion

        #region Draw

        /// <summary>
        /// Renders all controls added to the manager to a render target.
        /// </summary>
        private static void DrawToTexture()
        {
            if ((RootControls != null))
            {
                // Render each control in its own render target.
                foreach (Control control in RootControls)
                {
                    control.PreDrawControlOntoOwnTexture();
                }
                // Draw user interface texture.
                renderTarget.EnableRenderTarget();
                    EngineManager.Device.Clear(Color.Transparent);
                    foreach (Control control in RootControls)
                    {
                        control.DrawControlOntoMainTexture();
                    }
                renderTarget.DisableRenderTarget();
            }
        } // DrawToTexture

        /// <summary>
        /// Draws User Interface's render target to screen.
        /// </summary>
        private static void DrawTextureToScreen()
        {
            if ((RootControls != null))
            {
                Renderer.Begin();
                Renderer.Draw(renderTarget.Resource, new Rectangle(0, 0, EngineCore.Screen.Width, EngineCore.Screen.Height), Color.White);
                Renderer.End();
            }
        } // DrawTextureToScreen

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

                Margins margins = root.SkinInformation.ClientMargins;
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
            if (!CheckPosition(control, pos)) 
                return false;

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
            //Control control = control;
            if (control.Parent != null && control.Parent.ChildrenControls != null)
            {
                int index = -1;

                if (kbe.Key == Microsoft.Xna.Framework.Input.Keys.Left && !kbe.Handled)
                {
                    int miny = int.MaxValue;
                    int minx = int.MinValue;
                    for (int i = 0; i < ((ControlsList)control.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = (control.Parent.ChildrenControls as ControlsList)[i];
                        if (cx == control || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cay = control.Top + (control.Height / 2);
                        int cby = cx.Top + (cx.Height / 2);

                        if (Math.Abs(cay - cby) <= miny && (cx.Left + cx.Width) >= minx && (cx.Left + cx.Width) <= control.Left)
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
                    for (int i = 0; i < ((ControlsList)control.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = ((ControlsList)control.Parent.ChildrenControls)[i];
                        if (cx == control || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cay = control.Top + (control.Height / 2);
                        int cby = cx.Top + (cx.Height / 2);

                        if (Math.Abs(cay - cby) <= miny && cx.Left <= minx && cx.Left >= (control.Left + control.Width))
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
                    for (int i = 0; i < (control.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = (control.Parent.ChildrenControls)[i];
                        if (cx == control || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cax = control.Left + (control.Width / 2);
                        int cbx = cx.Left + (cx.Width / 2);

                        if (Math.Abs(cax - cbx) <= minx && (cx.Top + cx.Height) >= miny && (cx.Top + cx.Height) <= control.Top)
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
                    for (int i = 0; i < (control.Parent.ChildrenControls).Count; i++)
                    {
                        Control cx = (control.Parent.ChildrenControls)[i];
                        if (cx == control || !cx.Visible || !cx.Enabled || cx.Passive || !cx.CanFocus) continue;

                        int cax = control.Left + (control.Width / 2);
                        int cbx = cx.Left + (cx.Width / 2);

                        if (Math.Abs(cax - cbx) <= minx && cx.Top <= miny && cx.Top >= (control.Top + control.Height))
                        {
                            minx = Math.Abs(cax - cbx);
                            miny = cx.Top;
                            index = i;
                        }
                    }
                }

                if (index != -1)
                {
                    control.Parent.ChildrenControls[index].Focused = true;
                    kbe.Handled = true;
                }
            }
        } // ProcessArrows

        private static void MouseDownProcess(object sender, MouseEventArgs e)
        {
            ControlsList controlList = new ControlsList();
            controlList.AddRange(OrderList);

            if (AutoUnfocus && focusedControl != null && focusedControl.Root != modalWindow)
            {
                bool hit = RootControls.Any(cx => cx.ControlRectangle.Contains(e.Position));

                if (!hit)
                {
                    if (Control.ControlList.Any(t => t.Visible && t.Detached && t.ControlRectangle.Contains(e.Position)))
                    {
                        hit = true;
                    }
                }
                if (!hit) focusedControl.Focused = false;
            }

            for (int i = controlList.Count - 1; i >= 0; i--)
            {
                if (CheckState(controlList[i]) && CheckPosition(controlList[i], e.Position))
                {
                    states.Buttons[(int)e.Button] = controlList[i];
                    controlList[i].SendMessage(Message.MouseDown, e);

                    if (states.Click == -1)
                    {
                        states.Click = (int)e.Button;

                        if (FocusedControl != null)
                        {
                            FocusedControl.Invalidate();
                        }
                        controlList[i].Focused = true;
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
            Control control = states.Buttons[(int)e.Button];
            if (control != null)
            {
                if (CheckPosition(control, e.Position) && CheckOrder(control, e.Position) && states.Click == (int)e.Button && CheckButtons((int)e.Button))
                {
                    control.SendMessage(Message.Click, e);
                }
                states.Click = -1;
                control.SendMessage(Message.MouseUp, e);
                states.Buttons[(int)e.Button] = null;
                MouseMoveProcess(sender, e);
            }
        } // MouseUpProcess

        private static void MousePressProcess(object sender, MouseEventArgs e)
        {
            Control control = states.Buttons[(int)e.Button];
            if (control != null)
            {
                if (CheckPosition(control, e.Position))
                {
                    control.SendMessage(Message.MousePress, e);
                }
            }
        } // MousePressProcess

        private static void MouseMoveProcess(object sender, MouseEventArgs e)
        {
            ControlsList controlList = new ControlsList();
            controlList.AddRange(OrderList);

            for (int i = controlList.Count - 1; i >= 0; i--)
            {
                bool checkPosition = CheckPosition(controlList[i], e.Position);
                bool checkState = CheckState(controlList[i]);

                if (checkState && ((checkPosition && states.Over == controlList[i]) || (states.Buttons[(int)e.Button] == controlList[i])))
                {
                    controlList[i].SendMessage(Message.MouseMove, e);
                    break;
                }
            }

            for (int i = controlList.Count - 1; i >= 0; i--)
            {
                bool checkPosition = CheckPosition(controlList[i], e.Position);
                bool checkState = CheckState(controlList[i]) || (!string.IsNullOrEmpty(controlList[i].ToolTip.Text) && controlList[i].Visible);

                if (checkState && !checkPosition && states.Over == controlList[i] && states.Buttons[(int)e.Button] == null)
                {
                    states.Over = null;
                    controlList[i].SendMessage(Message.MouseOut, e);
                    break;
                }
            }

            for (int i = controlList.Count - 1; i >= 0; i--)
            {
                bool checkPosition = CheckPosition(controlList[i], e.Position);
                bool checkState = CheckState(controlList[i]) || (!string.IsNullOrEmpty(controlList[i].ToolTip.Text) && controlList[i].Visible);

                if (checkState && checkPosition && states.Over != controlList[i] && states.Buttons[(int)e.Button] == null)
                {
                    if (states.Over != null)
                    {
                        states.Over.SendMessage(Message.MouseOut, e);
                    }
                    states.Over = controlList[i];
                    controlList[i].SendMessage(Message.MouseOver, e);
                    break;
                }
                if (states.Over == controlList[i])
                    break;
            }
        } // MouseMoveProcess

        private static void KeyDownProcess(object sender, KeyEventArgs e)
        {
            Control focusedControl = FocusedControl;

            if (focusedControl != null && CheckState(focusedControl))
            {
                if (states.Click == -1)
                {
                    states.Click = (int)MouseButton.None;
                }
                states.Buttons[(int)MouseButton.None] = focusedControl;
                focusedControl.SendMessage(Message.KeyDown, e);

                if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
                {
                    focusedControl.SendMessage(Message.Click, new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));
                }
            }
        } // KeyDownProcess

        private static void KeyUpProcess(object sender, KeyEventArgs e)
        {
            Control control = states.Buttons[(int)MouseButton.None];

            if (control != null)
            {
                if (e.Key == Microsoft.Xna.Framework.Input.Keys.Space)
                {
                    control.SendMessage(Message.Click, new MouseEventArgs(new MouseState(), MouseButton.None, Point.Zero));
                }
                states.Click = -1;
                states.Buttons[(int)MouseButton.None] = null;
                control.SendMessage(Message.KeyUp, e);
            }
        } // KeyUpProcess

        private static void KeyPressProcess(object sender, KeyEventArgs e)
        {
            Control control = states.Buttons[(int)MouseButton.None];
            if (control != null)
            {
                control.SendMessage(Message.KeyPress, e);

                if ((e.Key == Microsoft.Xna.Framework.Input.Keys.Right ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Left ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Up ||
                     e.Key == Microsoft.Xna.Framework.Input.Keys.Down) && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    ProcessArrows(control, e);
                    KeyDownProcess(sender, e);
                }
                else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Tab && !e.Shift && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    TabNextControl(control);
                    KeyDownProcess(sender, e);
                }
                else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Tab && e.Shift && !e.Handled && CheckButtons((int)MouseButton.None))
                {
                    TabPrevControl(control);
                    KeyDownProcess(sender, e);
                }
            }
        } // KeyPressProcess

        #endregion

    } // UserInterfaceManager
} // // XNAFinalEngine.UserInterface