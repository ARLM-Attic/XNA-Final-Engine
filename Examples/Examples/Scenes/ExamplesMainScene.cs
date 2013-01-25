
#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
using Keyboard = XNAFinalEngine.Input.Keyboard;
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

        private bool videoStarted;

        // Every entity is a game object. Even videos.
        private GameObject2D introVideo;
        
        private GameObject2D[] exampleImage, exampleText, exampleTitle;

        private GameObject3D examplesCamera,
                             xnaFinalEngineLogo;

        private int currentScene;
        private Scene loadedScene;

        private GameObject2D statistics;

        #endregion

        #region Properties

        /// <summary>
        /// Indicates if the main scene is executing.
        /// </summary>
        public bool MainSceneExecuting { get; internal set; }

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        protected override void LoadContent()
        {
            MainSceneExecuting = true;

            #region Setup Input Controls
            
            // Create the virtual buttons to control the scene selection.
            new Button
            {
                Name = "Next Scene",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Keys.Right),
            };
            new Button
            {
                Name = "Next Scene",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Buttons.DPadRight),
            };
            new Button
            {
                Name = "Next Scene",
                ButtonBehavior = Button.ButtonBehaviors.AnalogInput,
                AnalogAxis = AnalogAxes.LeftStickX,
            };
            new Button
            {
                Name = "Previous Scene",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Keys.Left),
            };
            new Button
            {
                Name = "Previous Scene",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Buttons.DPadLeft),
            };
            new Button
            {
                Name = "Previous Scene",
                ButtonBehavior = Button.ButtonBehaviors.AnalogInput,
                AnalogAxis = AnalogAxes.LeftStickX,
                Invert = true,
            };
            new Button
            {
                Name = "Load Scene",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Keys.Enter),
            };
            new Button
            {
                Name = "Load Scene",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Buttons.A),
            };
            new Button
            {
                Name = "Back To Menu",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Keys.Escape),
            };
            new Button
            {
                Name = "Back To Menu",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Buttons.Back),
            };
            new Button
            {
                Name = "Show Statistics",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Keys.F1),
            };
            new Button
            {
                Name = "Show Statistics",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Buttons.Start),
            };

            #endregion

            #region Intro Video

            // Load and play the intro video...
            introVideo = new GameObject2D();
            introVideo.AddComponent<VideoRenderer>();
            introVideo.VideoRenderer.Video = new Video("LogosIntro");

            #endregion

            #region Statistics

            Layer.GetLayerByNumber(26).Name = "Statistics Layer";
            Layer.CurrentCreationLayer = Layer.GetLayerByNumber(26);
            Layer.GetLayerByNumber(26).Visible = false;
            statistics = new GameObject2D();
            statistics.AddComponent<ScriptStatisticsDrawer>();

            #endregion

            // This scene will be assigned to this layer.
            Layer.GetLayerByNumber(25).Name = "Examples Main Scene Layer";
            // But we hide it until the video is over.
            Layer.GetLayerByNumber(25).Visible = false;
            // Creates the scene objects in this layer;
            Layer.CurrentCreationLayer = Layer.GetLayerByNumber(25);

            #region 3D Camera

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

            #endregion

            xnaFinalEngineLogo = new GameObject3D(new FileModel("XNAFinalEngine"), new BlinnPhong());
            xnaFinalEngineLogo.Transform.Position = new Vector3(-17, 6, 0);

            exampleImage = new GameObject2D[3];
            exampleTitle = new GameObject2D[3];
            exampleText = new GameObject2D[3];

            #region Warehouse Scene

            exampleImage[0] = new GameObject2D();
            exampleImage[0].AddComponent<HudTexture>();
            exampleImage[0].HudTexture.Texture = new Texture("ExamplesImages\\WarehouseScene");
            exampleImage[0].Transform.Position = new Vector3(75, 250, 0);
            exampleTitle[0] = new GameObject2D();
            exampleTitle[0].AddComponent<HudText>();
            exampleTitle[0].HudText.Text.Append("Warehouse");
            exampleTitle[0].HudText.Font = new Font("BellGothicTitle");
            exampleTitle[0].Transform.Position = new Vector3(500, 200, 0);
            exampleText[0] = new GameObject2D();
            exampleText[0].AddComponent<HudText>();
            exampleText[0].HudText.Text.Append("This scene shows a Lamborghini Murcielago LP640 in a warehouse.\n\nThe car consists of 430.000 polygons. The scene includes an ambient light with spherical harmonic\nlighting and horizon based ambient occlusion (PC only), one directional light with cascade shadows,\ntwo point lights and one spot light with a light mask. The post processing stage use tone mapping,\nmorphological antialiasing (MLAA), bloom and film grain. Also a particles system that emits 150\nsoft tiled particles was placed.\n\nAlmost all the car was modeled by me, with the exception of some interior elements and the rear lights.\nI actually modeled these parts but because of my lack of texture experience and time I used some\nheavy textured models (and their textures) from a couple of games.\n\nMehar Gill (Dog Fight Studios) provides me the warehouse model (originally created by igorlmax).");
            exampleText[0].HudText.Font = new Font("BellGothicText");
            exampleText[0].Transform.Position = new Vector3(500, 250, 0);

            #endregion

            #region Physics Scene

            exampleImage[1] = new GameObject2D();
            exampleImage[1].AddComponent<HudTexture>();
            exampleImage[1].HudTexture.Texture = new Texture("ExamplesImages\\PhysicsScene");
            exampleImage[1].Transform.Position = new Vector3(75, 250, 0);
            exampleTitle[1] = new GameObject2D();
            exampleTitle[1].AddComponent<HudText>();
            exampleTitle[1].HudText.Text.Append("Physics Demonstration");
            exampleTitle[1].HudText.Font = new Font("BellGothicTitle");
            exampleTitle[1].Transform.Position = new Vector3(500, 200, 0);
            exampleText[1] = new GameObject2D();
            exampleText[1].AddComponent<HudText>();
            exampleText[1].HudText.Text.Append("Bepu physics library was integrated on the engine through a simple interface that hides the\ncommunication between the physic and graphic world.\n\nGarbage generation could be avoided if the initial pools are configured correctly and no other\n memory allocation should occur. Bepu physics 1.2 has a bug in witch garbage is generated,\nto avoid this we use the developed version of Bepu that fix this bug.\n\nThis example shows dynamic and kinematic rigid bodies and static meshes. This example also\nloads a non-centered sphere, the interface implemented detects this offset and address this miss\ncalculations without user action.\n\n");
            exampleText[1].HudText.Font = new Font("BellGothicText");
            exampleText[1].Transform.Position = new Vector3(500, 250, 0);

            #endregion

            #region Hellow World Scene

            exampleImage[2] = new GameObject2D();
            exampleImage[2].AddComponent<HudTexture>();
            exampleImage[2].HudTexture.Texture = new Texture("ExamplesImages\\HelloWorldScene");
            exampleImage[2].Transform.Position = new Vector3(75, 250, 0);
            exampleTitle[2] = new GameObject2D();
            exampleTitle[2].AddComponent<HudText>();
            exampleTitle[2].HudText.Text.Append("Hello World");
            exampleTitle[2].HudText.Font = new Font("BellGothicTitle");
            exampleTitle[2].Transform.Position = new Vector3(500, 200, 0);
            exampleText[2] = new GameObject2D();
            exampleText[2].AddComponent<HudText>();
            exampleText[2].HudText.Text.Append("This is the...");
            exampleText[2].HudText.Font = new Font("BellGothicText");
            exampleText[2].Transform.Position = new Vector3(500, 250, 0);

            #endregion

            // Set Default Layer.
            Layer.CurrentCreationLayer = Layer.GetLayerByNumber(0);
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        protected override void UpdateTasks()
        {
            if (Button.JustPressed("Show Statistics"))
                Layer.GetLayerByNumber(26).Visible = !Layer.GetLayerByNumber(26).Visible;
            if (MainSceneExecuting)
            {
                // Starts the video...
                if (Time.ApplicationTime > 1 && !videoStarted)
                {
                    introVideo.VideoRenderer.Play();
                    videoStarted = true;
                }
                if (videoStarted && Keyboard.KeyPressed(Keys.Escape))
                    introVideo.VideoRenderer.Stop();
                // If the video is over...
                if (introVideo.VideoRenderer.State == MediaState.Stopped && videoStarted)
                {
                    Layer.GetLayerByNumber(25).Visible = true;
                    if (xnaFinalEngineLogo.Transform.Position.X < -7)
                        xnaFinalEngineLogo.Transform.Translate(Time.SmoothFrameTime * 25, 0, 0);
                    else
                        xnaFinalEngineLogo.Transform.Position = new Vector3(-7, 6, 0);
                    // Handle input...
                    if (Button.JustPressed("Next Scene"))
                        currentScene++;
                    if (Button.JustPressed("Previous Scene"))
                        currentScene--;
                    if (currentScene >= exampleImage.Length)
                        currentScene = 0;
                    if (currentScene < 0)
                        currentScene = exampleImage.Length - 1;
                    if (Button.JustPressed("Load Scene"))
                    {
                        Layer.GetLayerByNumber(25).Visible = false;
                        MainSceneExecuting = false;
                        switch (currentScene)
                        {
                            case 0:
                                loadedScene = new WarehouseScene();
                                break;
                            case 1:
                                loadedScene = new PhysicsTestScene();
                                break;
                            case 2:
                                loadedScene = new HelloWorldScene();
                                break;
                        }
                    }
                }
                // Set as visible only the current scene.
                for (int i = 0; i < exampleImage.Length; i++)
                {
                    if (i == currentScene)
                    {
                        exampleImage[i].Active = true;
                        exampleTitle[i].Active = true;
                        exampleText[i].Active  = true;
                    }
                    else
                    {
                        exampleImage[i].Active = false;
                        exampleTitle[i].Active = false;
                        exampleText[i].Active  = false;
                    }
                }
            }
            else
            {
                if (Button.JustPressed("Back To Menu"))
                {
                    loadedScene.Dispose();
                    loadedScene = null;
                    MainSceneExecuting = true;
                }
            }
        } // UpdateTasks

        #endregion
        
    } // EmptyScene
} // XNAFinalEngineExamples
