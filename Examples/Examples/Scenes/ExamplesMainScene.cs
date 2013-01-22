
#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.Editor;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using Video = XNAFinalEngine.Assets.Video;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// This scene loads the other scenes.
    /// It also shows an intro video and a selection menu.
    /// </summary>
    public class ExamplesMainScene : Scene
    {

        #region Variables

        private static bool videoStarted;

        // Every entity is a game object. Even videos.
        private static GameObject2D introVideo;

        private static GameObject3D examplesCamera,
                                    xnaFinalEngineLogo;

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        public override void LoadContent()
        {
            // This scene will be assigned to this layer.
            Layer.GetLayerByNumber(25).Name = "Examples Main Scene Layer";
            // But we hide it until the video is over.
            Layer.GetLayerByNumber(25).Active = false;
            // Creates the scene objects in this layer;
            Layer.CurrentCreationLayer = Layer.GetLayerByNumber(25);

            // Load and play the intro video...
            introVideo = new GameObject2D();
            introVideo.AddComponent<VideoRenderer>();
            introVideo.VideoRenderer.Video = new Video("LogosIntro");

            // Camera
            examplesCamera = new GameObject3D();
            examplesCamera.AddComponent<Camera>();
            examplesCamera.AddComponent<SoundListener>();
            examplesCamera.Camera.RenderTargetSize = Size.FullScreen;
            examplesCamera.Camera.FarPlane = 100;
            examplesCamera.Camera.NearPlane = 1f; // Do not place a small value here, you can destroy performance, not just precision.
            examplesCamera.Transform.LookAt(new Vector3(0, 0, 25), Vector3.Zero, Vector3.Up);
            examplesCamera.Camera.ClearColor = Color.Black;
            examplesCamera.Camera.FieldOfView = 180 / 6f;
            examplesCamera.Camera.PostProcess = new PostProcess();
            examplesCamera.Camera.PostProcess.ToneMapping.AutoExposureEnabled = false;
            examplesCamera.Camera.PostProcess.ToneMapping.LensExposure = 0f;
            examplesCamera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            examplesCamera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color;
            examplesCamera.Camera.PostProcess.MLAA.Enabled = true;
            examplesCamera.Camera.AmbientLight = new AmbientLight
            {
                Color = new Color(50, 50, 55),
                Intensity = 5f,
            };

            xnaFinalEngineLogo = new GameObject3D(new FileModel("XNAFinalEngine"), new BlinnPhong());
            xnaFinalEngineLogo.Transform.Position = new Vector3(-7, -10, 0);

            // Set Default Layer.
            Layer.CurrentCreationLayer = Layer.GetLayerByNumber(0);
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        public override void UpdateTasks()
        {
            if (Time.ApplicationTime > 1 && !videoStarted)
            {
                introVideo.VideoRenderer.Play();
                videoStarted = true;
            }
            // If the video is over...
            if (introVideo.VideoRenderer.State == MediaState.Stopped && videoStarted)
            {
                if (xnaFinalEngineLogo.Transform.Position.Y < 6)
                    xnaFinalEngineLogo.Transform.Translate(0, Time.SmoothFrameTime * 25, 0);
                Layer.GetLayerByNumber(25).Active = true;
            }
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
            
        } // PreRenderTasks

        /// <summary>
        /// Tasks after the engine render.
        /// Probably you won’t need to place any task here.
        /// </summary>
        public override void PostRenderTasks()
        {
            
        } // PostRenderTasks

        #endregion

    } // EmptyScene
} // XNAFinalEngineExamples
