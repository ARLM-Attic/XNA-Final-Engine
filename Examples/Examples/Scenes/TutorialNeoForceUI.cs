
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Scenes;
using XNAFinalEngine.UserInterface;
using XNAFinalEngine.UserInterface.Central;
using Console = XNAFinalEngine.UserInterface.Console;
using EventArgs = XNAFinalEngine.UserInterface.EventArgs;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// User Interface Tutorial.
    /// This is the Neo Force UI exampled extended to show the new features like the color dialog.
    /// </summary>
    public class TutorialNeoForceUIScene : Scene
    {

        #region Variables

        /// <summary>
        /// A scene loaded by this scene.
        /// </summary>
        private Scene scene;

        #region Neo Force Variables

        private bool exit;
        private ExitDialog exitDialog;
        private const int tasksCount = 5;
        private readonly string[] tasks = new[] { "Dialog Template", "Controls Test", "Auto Scrolling", "Layout Window", "Events Test" };

        #region Main Window
        
        private Panel sidebar;

        private SideBarPanel pnlRes;
        private RadioButton rdbRes1024;
        private RadioButton rdbRes1280;
        private RadioButton rdbRes1680;
        private CheckBox chkResFull;
        private Button btnApply;
        private Button btnExit;

        private SideBarPanel pnlTasks;
        private Button btnRandom;
        private Button btnClose;

        private Button[] btnTasks;

        private SideBarPanel pnlSkin;
        private RadioButton rdbDefault;
        private RadioButton rdbGreen;

        private SideBarPanel pnlStats;
        public Label lblObjects;
        public Label lblAvgFps;
        public Label lblFps;

        #endregion

        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method at the end.</remarks>
        public override void Load()
        {
            UserInterfaceManager.Initialize();

            //UserInterfaceManager.WindowClosing += Manager_WindowClosing;
            
            InitControls();

            //EngineManager.ShowFramesPerSecond = true;

            base.Load();
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        public override void UpdateTasks()
        {
            UserInterfaceManager.Update();
        } // UpdateTasks

        #endregion

        #region Render Tasks

        /// <summary>
        /// Tasks before the engine render.
        /// Some tasks are more related to the frame rendering than the update,
        /// or maybe the update frequency is too high to waste time in this kind of tasks,
        /// for that reason the pre render task exists.
        /// For example, is more correct to update the HUD information here because is related with the rendering.
        /// </summary>
        public override void PreRenderTasks()
        {
            UserInterfaceManager.DrawToTexture();
        } // Render

        /// <summary>
        /// Tasks after the engine render.
        /// Probably you won’t need to place any task here.
        /// </summary>
        public override void PostRenderTasks()
        {
            UserInterfaceManager.DrawTextureToScreen();
        } // PostRenderTasks

        #endregion

        #region Neo Force Methods

        #region Closing
        /*
        private void Manager_WindowClosing(object sender, WindowClosingEventArgs e)
        {
            e.Cancel = !exit;

            if (!exit && exitDialog == null)
            {
                exitDialog = new ExitDialog();
                exitDialog.Closed += closeDialog_Closed;
                exitDialog.ShowModal();
                UserInterfaceManager.Add(exitDialog);
            }
        } // Manager_WindowClosing

        private void closeDialog_Closed(object sender, WindowClosedEventArgs e)
        {
            if (((Dialog)sender).ModalResult == ModalResult.Yes)
            {
                exit = true;
                EngineManager.ExitApplication();
            }
            else
            {
                exit = false;
                exitDialog.Closed -= closeDialog_Closed;
                exitDialog.Dispose();
                exitDialog = null;
            }
        } // closeDialog_Closed
        */
        #endregion

        #region Init

        private void InitControls()
        {
            sidebar = new Panel
            {
                StayOnBack = true,
                Passive = true,
                Width = 200,
                Height = Screen.Height,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Bottom
            };
            UserInterfaceManager.Add(sidebar);

            InitRes();
            InitTasks();
            InitStats();
            InitSkins();
            InitConsole();
        } // InitControls

        #region Init Res

        private void InitRes()
        {
            pnlRes = new SideBarPanel
            {
                Passive = true,
                Parent = sidebar,
                Left = 16,
                Top = 16,
                Height = 86,
                CanFocus = false
            };
            pnlRes.Width = sidebar.Width - pnlRes.Left;


            rdbRes1024 = new RadioButton
            {
                Parent = pnlRes,
                Left = 8,
                Height = 16,
                Text = "Resolution 1024x768",
                ToolTip =
                    {
                        Text = "Resolution 1024x768",
                        Color = Color.Blue,
                        TextColor = Color.White
                    },
                Top = 8,
                Checked = true
            };
            rdbRes1024.Width = pnlRes.Width - rdbRes1024.Left * 2;

            rdbRes1280 = new RadioButton
            {
                Parent = pnlRes,
                Left = rdbRes1024.Left,
                Width = rdbRes1024.Width,
                Height = rdbRes1024.Height,
                Text = "Resolution 1280x1024",
                Top = 24
            };

            rdbRes1680 = new RadioButton
            {
                Parent = pnlRes,
                Left = rdbRes1024.Left,
                Width = rdbRes1024.Width,
                Height = rdbRes1024.Height,
                Text = "Resolution 1680x1050",
                Top = 40
            };

            chkResFull = new CheckBox
            {
                Parent = pnlRes,
                Left = rdbRes1024.Left,
                Width = rdbRes1024.Width,
                Height = rdbRes1024.Height,
                Text = "Fullscreen Mode",
                Top = 64
            };

            btnApply = new Button
            {
                Width = 80,
                Parent = sidebar,
                Left = pnlRes.Left,
                Top = pnlRes.Top + pnlRes.Height + 8,
                Text = "Apply",
                Enabled = false,
            };

            btnExit = new Button
            {
                Width = 80,
                Parent = sidebar,
                Left = btnApply.Left + btnApply.Width + 8,
                Top = pnlRes.Top + pnlRes.Height + 8,
                Text = "Exit",
                Enabled = false,
            };
            //btnExit.Click += new EventHandler(btnExit_Click);
        }

        #endregion

        #region Init Tasks

        private void InitTasks()
        {
            pnlTasks = new SideBarPanel
            {
                Passive = true,
                Parent = sidebar,
                Left = 16,
                Width = sidebar.Width - pnlRes.Left,
                Height = (tasksCount * 25) + 16,
                Top = btnApply.Top + btnApply.Height + 16,
                CanFocus = false
            };

            btnTasks = new Button[tasksCount];
            for (int i = 0; i < tasksCount; i++)
            {
                btnTasks[i] = new Button
                {
                    Parent = pnlTasks,
                    Left = 8,
                    Width = -8 + btnApply.Width * 2,
                    Text = "Task [" + i + "]"
                };
                btnTasks[i].Top = 8 + i * (btnTasks[i].Height + 1);
                btnTasks[i].Click += btnTask_Click;
                if (tasks.Length >= i - 1 && tasks[i] != "")
                    btnTasks[i].Text = tasks[i];
            }

            btnRandom = new Button
            {
                Parent = sidebar,
                Width = 80,
                Left = 16,
                Top = pnlTasks.Top + pnlTasks.Height + 8,
                Text = "Random"
            };
            btnRandom.Click += btnRandom_Click;

            btnClose = new Button
            {
                Width = 80,
                Parent = sidebar,
                Left = btnRandom.Left + btnRandom.Width + 8,
                Top = pnlTasks.Top + pnlTasks.Height + 8,
                Text = "Close"
            };
            btnClose.Click += btnClose_Click;
        }

        #endregion

        #region Init Skins

        private void InitSkins()
        {
            pnlSkin = new SideBarPanel
                          {
                              Passive = true,
                              Parent = sidebar,
                              Left = 16,
                              Width = sidebar.Width - pnlRes.Left,
                              Height = 44
                          };
            pnlSkin.Top = Screen.Height - 16 - pnlStats.Height - pnlSkin.Height - 16;
            pnlSkin.Anchor = Anchors.Left | Anchors.Bottom;
            pnlSkin.CanFocus = false;

            rdbDefault = new RadioButton();
            //rdbDefault.Init();
            rdbDefault.Parent = pnlSkin;
            rdbDefault.Left = 8;
            rdbDefault.Width = pnlSkin.Width - rdbDefault.Left * 2;
            rdbDefault.Height = 16;
            rdbDefault.Text = "Default Skin";
            rdbDefault.Top = 8;
            //rdbDefault.Checked = Manager.Skin.Name == "Default";
            rdbDefault.Click += new XNAFinalEngine.UserInterface.EventHandler(rdbDefault_Click);

            rdbGreen = new RadioButton();
            //rdbGreen.Init();
            rdbGreen.Parent = pnlSkin;
            rdbGreen.Left = 8;
            rdbGreen.Width = pnlSkin.Width - rdbGreen.Left * 2;
            rdbGreen.Height = 16;
            rdbGreen.Text = "Green Skin";
            rdbGreen.Top = 24;
            //rdbGreen.Checked = Manager.Skin.Name == "Green";
            rdbGreen.Click += new XNAFinalEngine.UserInterface.EventHandler(rdbGreen_Click);
            rdbGreen.Enabled = true;
        }

        #endregion

        #region Init Stats

        private void InitStats()
        {
            pnlStats = new SideBarPanel
            {
                Passive = true,
                Parent = sidebar,
                Left = 16,
                Height = 64,
                Anchor = Anchors.Left | Anchors.Bottom,
                CanFocus = false
            };
            pnlStats.Width = sidebar.Width - pnlStats.Left;
            pnlStats.Top = Screen.Height - 16 - pnlStats.Height;

            lblObjects = new Label
            {
                Parent = pnlStats,
                Left = 8,
                Top = 8,
                Height = 16,
                Alignment = Alignment.MiddleLeft
            };
            lblObjects.Width = pnlStats.Width - lblObjects.Left * 2; ;

            lblAvgFps = new Label
            {
                Parent = pnlStats,
                Left = 8,
                Top = 24,
                Height = 16,
                Width = pnlStats.Width - lblObjects.Left * 2,
                Alignment = Alignment.MiddleLeft
            };

            lblFps = new Label
            {
                Parent = pnlStats,
                Left = 8,
                Top = 40,
                Height = 16,
                Width = pnlStats.Width - lblObjects.Left * 2,
                Alignment = Alignment.MiddleLeft
            };
        }

        #endregion

        #region Init Console

        private static void InitConsole()
        {
            Console con1 = new Console();
            Console con2 = new Console();

            // Setup of TabControl, which will be holding both consoles
            TabControl tbc = new TabControl
            {
                Alpha = 220,
                Left = 220,
                Height = 220,
                Width = 400,
                Movable = true,
                Resizable = true,
                MinimumHeight = 96,
                MinimumWidth = 160
            };
            tbc.Top = Screen.Height - tbc.Height - 32;

            tbc.AddPage("Global");
            tbc.AddPage("Private");
            tbc.TabPages[0].Add(con1);
            tbc.TabPages[1].Add(con2);

            con2.Width = con1.Width = tbc.TabPages[0].ClientWidth;
            con2.Height = con1.Height = tbc.TabPages[0].ClientHeight;
            con2.Anchor = con1.Anchor = Anchors.All;

            con1.Channels.Add(new ConsoleChannel(0, "General", Color.Orange));
            con1.Channels.Add(new ConsoleChannel(1, "Private", Color.White));
            con1.Channels.Add(new ConsoleChannel(2, "System", Color.Yellow));

            // We want to share channels and message buffer in both consoles
            con2.Channels = con1.Channels;
            con2.MessageBuffer = con1.MessageBuffer;

            // In the second console we display only "Private" messages
            con2.ChannelFilter.Add(1);

            // Select default channels for each tab
            con1.SelectedChannel = 0;
            con2.SelectedChannel = 1;

            // Do we want to add timestamp or channel name at the start of every message?
            con1.MessageFormat = ConsoleMessageFormats.All;
            con2.MessageFormat = ConsoleMessageFormats.TimeStamp;

            // We send initial welcome message to System channel
            con1.MessageBuffer.Add(new ConsoleMessage("Welcome to Neoforce!", 2));

            UserInterfaceManager.Add(tbc);
        }

        #endregion

        #region Methods
        
        void btnClose_Click(object sender, EventArgs e)
        {
            ControlsList list = new ControlsList();
            list.AddRange(UserInterfaceManager.RootControls);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Window)
                {
                    if (list[i].Text.Substring(0, 6) == "Window")
                    {
                        (list[i] as Window).Dispose();
                    }
                }
            }
            list.Clear();
        }

        void btnRandom_Click(object sender, EventArgs e)
        {
            Window win = new Window();
            Button btn = new Button();
            TextBox txt = new TextBox();

            win.ClientWidth = 320;
            win.ClientHeight = 160;

            win.MinimumWidth = 128;
            win.MinimumHeight = 128;

            Random r = new Random();
            win.ClientWidth += r.Next(-100, +100);
            win.ClientHeight += r.Next(-100, +100);

            win.Left = r.Next(200, Screen.Width - win.ClientWidth / 2);
            win.Top = r.Next(0, Screen.Height - win.ClientHeight / 2);
            win.Closed += win_Closed;

            btn.Anchor = Anchors.Bottom;
            btn.Left = (win.ClientWidth / 2) - (btn.Width / 2);
            btn.Top = win.ClientHeight - btn.Height - 8;
            btn.Text = "OK";

            win.Text = "Window (" + win.Width + "x" + win.Height + ")";

            txt.Parent = win;
            txt.Left = 8;
            txt.Top = 8;
            txt.Width = win.ClientArea.Width - 16;
            txt.Height = win.ClientArea.Height - 48;
            txt.Anchor = Anchors.All;
            txt.Mode = TextBoxMode.Multiline;
            txt.Text = "This is a Multiline TextBox.\n" +
                       "Allows to edit large texts,\n" +
                       "copy text to and from clipboard,\n" +
                       "select text with mouse or keyboard\n" +
                       "and much more...";

            txt.SelectAll();
            txt.Focused = true;

            txt.ScrollBars = ScrollBarType.Both;

            win.Add(btn, true);
            win.Show();
            UserInterfaceManager.Add(win);
        }

        void win_Closed(object sender, WindowClosedEventArgs e)
        {
            e.Dispose = true;
        }

        void btnTask_Click(object sender, EventArgs e)
        {
            if (sender == btnTasks[0])
            {
                #if (!XBOX)
                    UserInterfaceManager.Cursor = Skin.Cursors["Busy"].Cursor;
                #endif

                btnTasks[0].Enabled = false;
                TaskDialog tmp = new TaskDialog();
                tmp.Closing += WindowClosing;
                tmp.Closed += WindowClosed;
                UserInterfaceManager.Add(tmp);

                Thread.Sleep(2000); // Sleep to demonstrate animated busy cursor

                tmp.Show();
                #if (!XBOX)
                    UserInterfaceManager.Cursor = Skin.Cursors["Default"].Cursor;
                #endif

            }
            else if (sender == btnTasks[1])
            {
                btnTasks[1].Enabled = false;
                TaskControls tmp = new TaskControls();
                tmp.Closing += WindowClosing;
                tmp.Closed += WindowClosed;
                UserInterfaceManager.Add(tmp);
                tmp.ShowModal();
            }
            else if (sender == btnTasks[2])
            {
                btnTasks[2].Enabled = false;
                TaskAutoScroll tmp = new TaskAutoScroll();
                tmp.Closing += WindowClosing;
                tmp.Closed += WindowClosed;
                UserInterfaceManager.Add(tmp);
                tmp.Show();
            }
            else if (sender == btnTasks[3])
            {
                btnTasks[3].Enabled = false;

                Window tmp = (Window)Layout.Load("Window");
                tmp.Closing += WindowClosing;
                tmp.Closed += WindowClosed;
                tmp.SearchChildControlByName("btnOk").Click += Central_Click;
                UserInterfaceManager.Add(tmp);
                tmp.Show();
            }
            else if (sender == btnTasks[4])
            {
                btnTasks[4].Enabled = false;

                TaskEvents tmp = new TaskEvents();
                tmp.Closing += WindowClosing;
                tmp.Closed += WindowClosed;
                UserInterfaceManager.Add(tmp);
                tmp.Show();
            }
        }

        void rdbGreen_Click(object sender, EventArgs e)
        {
            //while (true)
                UserInterfaceManager.SetSkin("Green");
        }

        void rdbDefault_Click(object sender, EventArgs e)
        {
            UserInterfaceManager.SetSkin("Default");
        }

        void Central_Click(object sender, EventArgs e)
        {
            ((sender as Button).Root as Window).Close();
        }

        void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            //e.Cancel = true; 
        }

        void WindowClosed(object sender, WindowClosedEventArgs e)
        {
            if (sender is TaskDialog)
            {
                btnTasks[0].Enabled = true;
                btnTasks[0].Focused = true;
            }
            else if (sender is TaskControls)
            {
                btnTasks[1].Enabled = true;
                btnTasks[1].Focused = true;
            }
            else if (sender is TaskAutoScroll)
            {
                btnTasks[2].Enabled = true;
                btnTasks[2].Focused = true;
            }
            else if (sender is Window && (sender as Window).Name == "frmMain")
            {
                btnTasks[3].Enabled = true;
                btnTasks[3].Focused = true;
            }
            else if (sender is TaskEvents)
            {
                btnTasks[4].Enabled = true;
                btnTasks[4].Focused = true;
            }
            e.Dispose = true;
        }

        #endregion

        #endregion

        #endregion

    } // TutorialNeoForceUIScene
} // XNAFinalEngineExamples