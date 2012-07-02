
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UserInterface;
using Size = XNAFinalEngine.Helpers.Size;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// Editor Viewport.
    /// </summary>
    internal class EditorViewport : Disposable
    {

        #region Enumerates

        /// <summary>
        /// This indicate the mode of the viewport.
        /// </summary>
        public enum ViewportMode
        {
            /// <summary>
            /// Perspective.
            /// </summary>
            Perspective,
            /// <summary>
            /// Orthographic from top.
            /// </summary>
            Top,
            /// <summary>
            /// Orthographic from front.
            /// </summary>
            Front,
            /// <summary>
            /// Orthographic from right.
            /// </summary>
            Right,
            /// <summary>
            /// The game main camera.
            /// </summary>
            Game,
        } // ViewportModeType

        #endregion

        #region Variables

        // The editor and gizmo camera.
        private readonly GameObject3D viewportCamera, helperCamera;
        private readonly ScriptEditorCamera editorCameraScript;
        
        private bool enabled = true;

        private readonly Container viewportControl;

        private RectangleF normalizedViewport;

        private ViewportMode mode;

        // This value allow to restore the camera transformation of each mode.
        private readonly Vector3[] lookAtPosition = new Vector3[4];
        private readonly float[] distance  = new float[4];
        private readonly float[] roll = new float[4];
        private readonly float[] pitch = new float[4];
        private readonly float[] yaw = new float[4];
        private readonly AmbientLight ambientLight;
        private readonly PostProcess postProcess;

        #endregion

        #region Properties

        /// <summary>
        /// Makes the componet active or not.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    Camera.Enabled = value;
                    viewportControl.Visible = value;
                    if (!value && Camera.RenderTarget != null)
                    {
                        Camera.RenderTarget.Dispose();
                        Camera.RenderTarget = null;
                    }
                }
            }
        } // Enabled

        /// <summary>
        /// This indicate the mode of the viewport (scene, game, etc.).
        /// </summary>
        public ViewportMode Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    StoreModeValues();
                    mode = value;
                    RestoreModeValues();
                }
            }
        } // Mode

        /// <summary>
        /// Viewport Camera.
        /// </summary>
        public Camera Camera { get { return viewportCamera.Camera; } }

        /// <summary>
        /// A helper camera that draw editor elements..
        /// </summary>
        public Camera HelperCamera { get { return helperCamera.Camera; } }

        /// <summary>
        /// Absolute left position of viewport.
        /// </summary>
        public int ClientLeft { get { return viewportControl.ControlLeftAbsoluteCoordinate + viewportControl.ClientArea.Left; } }

        /// <summary>
        ///  Absolute top position of viewport.
        /// </summary>
        public int ClientTop { get { return viewportControl.ControlTopAbsoluteCoordinate + viewportControl.ClientArea.Top; } }

        /// <summary>
        /// Client width.
        /// </summary>
        public int ClientWidth { get { return viewportControl.ClientArea.ControlAndMarginsWidth; } }

        /// <summary>
        /// Client height
        /// </summary>
        public int ClientHeight { get { return viewportControl.ClientArea.ControlAndMarginsHeight; } }

        /// <summary>
        /// Client area.
        /// </summary>
        public Control ClientArea { get { return viewportControl.ClientArea; } }

        /// <summary>
        /// The normalized dimensions relative to the render space area.
        /// </summary>
        public RectangleF NormalizedViewport
        {
            get { return normalizedViewport; }
            set
            {
                if (normalizedViewport != value)
                {
                    normalizedViewport = value;
                    OnScreenSizeChanged(null, null);
                }
            }
        } // NormalizedViewport

        #endregion

        #region Constructor

        /// <summary>
        /// Editor Viewport.
        /// </summary>
        public EditorViewport(RectangleF normalizedViewport, ViewportMode mode)
        {

            #region Controls

            viewportControl = new Container
            {
                Parent = MainWindow.ViewportsSpace,
                Anchor = Anchors.Left | Anchors.Top,
                CanFocus = false,
            };
            // Stores the normalized viewport to adapt to screen size changes.
            NormalizedViewport = normalizedViewport;

            ToolBarPanel topPanel = new ToolBarPanel { Width = viewportControl.Width };
            viewportControl.ToolBarPanel = topPanel;
            ToolBar toolBarTopPanel = new ToolBar
            {
                Parent = topPanel,
                Movable = true,
                FullRow = true,
            };
            var modeComboBox = new ComboBox
            {
                Parent = topPanel,
                Width = 200,
                Top = 2,
            };
            modeComboBox.Items.AddRange(new [] {"Perpective", "Top", "Front", "Right", "Game"});
            modeComboBox.ItemIndex = 0;
            modeComboBox.ItemIndexChanged += delegate
            {
                switch (modeComboBox.ItemIndex)
                {
                    case 0: Mode = ViewportMode.Perspective; break;
                    case 1: Mode = ViewportMode.Top; break;
                    case 2: Mode = ViewportMode.Front; break;
                    case 3: Mode = ViewportMode.Right; break;
                    case 4: Mode = ViewportMode.Game; break;
                }
            };
            modeComboBox.Draw += delegate
            {
                if (modeComboBox.ListBoxVisible)
                    return;
                switch (Mode)
                {
                    case ViewportMode.Perspective: modeComboBox.ItemIndex = 0; break;
                    case ViewportMode.Top: modeComboBox.ItemIndex = 1; break;
                    case ViewportMode.Front: modeComboBox.ItemIndex = 2; break;
                    case ViewportMode.Right: modeComboBox.ItemIndex = 3; break;
                    case ViewportMode.Game: modeComboBox.ItemIndex = 4; break;
                }
            };

            #endregion

            #region Cameras

            // Assets
            ambientLight = new AmbientLight { Name = "Editor Camara Ambient Light "};
            postProcess = new PostProcess   { Name = "Editor Camera Post Process" };
            postProcess.Bloom.Enabled = false;

            // Editor Camera
            viewportCamera = new GameObject3D();
            viewportCamera.AddComponent<Camera>();
            viewportCamera.Camera.Enabled = true;
            viewportCamera.Layer = Layer.GetLayerByNumber(31);
            viewportCamera.Camera.RenderingOrder = int.MinValue;
            editorCameraScript = (ScriptEditorCamera)viewportCamera.AddComponent<ScriptEditorCamera>();
            editorCameraScript.Mode = ScriptEditorCamera.ModeType.Maya;
            editorCameraScript.ClientArea = viewportControl.ClientArea;
            // Camera to render the gizmos and other editor elements.
            // This is done because the gizmos need to be in front of everything and I can't clear the ZBuffer wherever I want.
            helperCamera = new GameObject3D();
            helperCamera.AddComponent<Camera>();
            helperCamera.Camera.Enabled = true;
            helperCamera.Camera.CullingMask = Layer.GetLayerByNumber(31).Mask; // The editor layer.
            helperCamera.Camera.ClearColor = Color.Transparent;
            helperCamera.Layer = Layer.GetLayerByNumber(31);
            // Set the viewport camara as master of the gizmo camera.
            helperCamera.Camera.MasterCamera = viewportCamera.Camera;

            #endregion

            // Set default camera transformation in each mode.
            Mode = ViewportMode.Perspective;
            Reset();
            Mode = ViewportMode.Top;
            Reset();
            Mode = ViewportMode.Front;
            Reset();
            Mode = ViewportMode.Right;
            Reset();

            // Set current mode.
            Mode = mode;

            // Adjust viewport dimentions and render target size when the size changes.
            Screen.ScreenSizeChanged += OnScreenSizeChanged;
        } // EditorViewport

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            viewportCamera.Dispose();
            helperCamera.Dispose();
            viewportControl.Dispose();
            Screen.ScreenSizeChanged -= OnScreenSizeChanged;
        } // DisposeManagedResources

        #endregion

        #region Update
        
        /// <summary>
        /// Update.
        /// </summary>
        public void Update()
        {
            if (Mode == ViewportMode.Game)
                CloneMainCamera();
            helperCamera.Transform.WorldMatrix = viewportCamera.Transform.WorldMatrix;
        } // Update

        #endregion

        #region Reset Viewport Camera

        /// <summary>
        /// Reset camera to default position and orientation.
        /// </summary>
        public void Reset()
        {
            editorCameraScript.LookAtPosition = new Vector3(0, 0, 0);
            editorCameraScript.Distance = 30;
            editorCameraScript.Roll = 0;
            if (mode == ViewportMode.Perspective)
            {
                editorCameraScript.OrthographicMode = false;
                viewportCamera.Camera.OrthographicProjection = false;
                editorCameraScript.Pitch = 0.3f;
                editorCameraScript.Yaw = 0;
            }
            else
            {
                editorCameraScript.OrthographicMode = true;
                viewportCamera.Camera.OrthographicProjection = true;
                if (mode == ViewportMode.Top)
                {
                    editorCameraScript.Yaw = 0;
                    editorCameraScript.Pitch = MathHelper.Pi / 2;
                }
                if (mode == ViewportMode.Right)
                {
                    editorCameraScript.Yaw = MathHelper.Pi / 2;
                    editorCameraScript.Pitch = 0;
                }
                if (mode == ViewportMode.Front)
                {
                    editorCameraScript.Yaw = 0;
                    editorCameraScript.Pitch = 0;
                }
            }
        } // ResetViewportCamera

        #endregion

        #region Store and Restore Mode Values

         /// <summary>
        /// Store current camera transformation to restore it in the future.
        /// </summary>
        private void StoreModeValues()
        {
            if (mode != ViewportMode.Game)
            {
                lookAtPosition[(int)mode] = editorCameraScript.LookAtPosition;
                distance[(int)mode] = editorCameraScript.Distance;
                yaw[(int)mode] = editorCameraScript.Yaw;
                pitch[(int)mode] = editorCameraScript.Pitch;
                roll[(int)mode] = editorCameraScript.Roll;
            }
        } // StoreModeValues

        /// <summary>
        /// Restore previous camera transformation.
        /// </summary>
        private void RestoreModeValues()
        {
            if (mode == ViewportMode.Game)
            {
                editorCameraScript.Enabled = false;
                helperCamera.Camera.Enabled = false;
                return;
            }
            // Viewport camera configuration.
            editorCameraScript.Enabled = true;
            helperCamera.Camera.Enabled = true;
            viewportCamera.Camera.AmbientLight = ambientLight;
            viewportCamera.Camera.PostProcess = postProcess;
            viewportCamera.Camera.ClearColor = Color.Black;
            viewportCamera.Camera.Sky = null;
            viewportCamera.Camera.NormalizedViewport = new RectangleF(0, 0, 1, 1);
            viewportCamera.Camera.CullingMask = uint.MaxValue & ~(uint)(Math.Pow(2, 31));
            viewportCamera.Camera.RenderingOrder = int.MinValue;
            viewportCamera.Camera.AspectRatio = ClientWidth / (float)ClientHeight;
            viewportCamera.Camera.FieldOfView = 36;
            viewportCamera.Camera.NearPlane = 1;
            viewportCamera.Camera.FarPlane = 2000;
            viewportCamera.Camera.RenderHeadUpDisplay = false;
            viewportCamera.Camera.ResetProjectionMatrix();
            // Set the camera size to the client are of the viewport.
            viewportCamera.Camera.RenderTargetSize = new Size(ClientWidth, ClientHeight);

            if (mode == ViewportMode.Perspective)
            {
                editorCameraScript.OrthographicMode = false;
                viewportCamera.Camera.OrthographicProjection = false;
            }
            else
            {
                editorCameraScript.OrthographicMode = true;
                viewportCamera.Camera.OrthographicProjection = true;
            }
            editorCameraScript.LookAtPosition = lookAtPosition[(int)mode];
            editorCameraScript.Distance = distance[(int)mode];
            editorCameraScript.Yaw = yaw[(int)mode];
            editorCameraScript.Pitch = pitch[(int)mode];
            editorCameraScript.Roll = roll[(int)mode];
        } // RestoreModeValues
        
        #endregion

        #region Clone Main Camera

        /// <summary>
        /// Clone Main Camera
        /// </summary>
        private void CloneMainCamera()
        {
            // Find out game camera.
            Layer.ActiveLayers = EditorManager.GameActiveMask;
            Camera mainCamera = Camera.MainCamera;
            Layer.ActiveLayers = Layer.GetLayerByNumber(31).Mask; // Update just the editor

            viewportCamera.Camera.RenderHeadUpDisplay = mainCamera.RenderHeadUpDisplay;
            viewportCamera.Camera.ClearColor = mainCamera.ClearColor;
            viewportCamera.Camera.AmbientLight = mainCamera.AmbientLight;
            viewportCamera.Camera.Sky = mainCamera.Sky;
            viewportCamera.Camera.PostProcess = mainCamera.PostProcess;
            viewportCamera.Transform.WorldMatrix = ((GameObject3D)mainCamera.Owner).Transform.WorldMatrix; // ViewMatrix
            viewportCamera.Camera.ProjectionMatrix = mainCamera.ProjectionMatrix;
            viewportCamera.Camera.AspectRatio = mainCamera.AspectRatio;
            viewportCamera.Camera.NormalizedViewport = mainCamera.NormalizedViewport;
            viewportCamera.Camera.CullingMask = mainCamera.CullingMask;
            Size size;
            float renderTargetAspectRatio = Camera.AspectRatio,
                  renderSpaceAspectRatio = ClientArea.Width / (float)ClientArea.Height;
            if (renderTargetAspectRatio > renderSpaceAspectRatio)
                size = new Size(ClientArea.Width, (int)(ClientArea.Width / renderTargetAspectRatio));
            else
                size = new Size((int)(renderTargetAspectRatio * ClientArea.Height), ClientArea.Height);
            viewportCamera.Camera.RenderTargetSize = size;
        } // CloneMainCamera

        #endregion

        #region On Screen Size Changed

        /// <summary>
        /// Reset the viewport dimensions and adjust the viewport camera's render target size.
        /// </summary>
        private void OnScreenSizeChanged(object caller, System.EventArgs eventArgs)
        {
            viewportControl.Left   = (int)(MainWindow.ViewportArea.Width  * normalizedViewport.X + MainWindow.ViewportArea.X);
            viewportControl.Top    = (int)(MainWindow.ViewportArea.Height * normalizedViewport.Y + MainWindow.ViewportArea.Y);
            viewportControl.Width  = (int)(MainWindow.ViewportArea.Width  * normalizedViewport.Width);
            viewportControl.Height = (int)(MainWindow.ViewportArea.Height * normalizedViewport.Height);
            if (viewportCamera != null)
            {
                editorCameraScript.ClientArea = viewportControl.ClientArea;
                if (mode != ViewportMode.Game)
                {
                    viewportCamera.Camera.RenderTargetSize = new Size(ClientWidth, ClientHeight);
                    viewportCamera.Camera.AspectRatio = ClientWidth / (float)ClientHeight;
                }
                else
                {
                    Size size;
                    float renderTargetAspectRatio = Camera.AspectRatio,
                          renderSpaceAspectRatio = ClientArea.Width / (float)ClientArea.Height;
                    if (renderTargetAspectRatio > renderSpaceAspectRatio)
                        size = new Size(ClientArea.Width, (int)(ClientArea.Width / renderTargetAspectRatio));
                    else
                        size = new Size((int)(renderTargetAspectRatio * ClientArea.Height), ClientArea.Height);
                    viewportCamera.Camera.RenderTargetSize = size;
                }
                    
            }
        } // OnScreenSizeChanged

        #endregion

    } // EditorViewport
} // XNAFinalEngine.Editor
