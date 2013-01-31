
#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Audio;
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
    /// This scene used to load the other scenes.
    /// It also shows an intro video and a selection menu.
    /// </summary>
    public class ExamplesMainScene : Scene
    {

        #region Variables

        // Indicates if the video started.
        private bool videoStarted;

        // Every entity is a game object. Even videos.
        private GameObject2D introVideo, demoLegend;
        
        // Scene information.
        private GameObject2D[] exampleImage, exampleText, exampleTitle;

        // 3D Objects
        private GameObject3D examplesCamera,
                             finalEngineLogo,
                             xnaLogo,
                             examplesLogo,
                             selectOneScene,
                             loading,
                             directionalLight;

        // The scene selected.
        private int currentScene;
        // The scene in execution.
        private Scene loadedScene;

        // Indicates if the main scene is executing.
        private bool mainSceneExecuting;

        // Statistics graph.
        private GameObject2D statistics;

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        protected override void LoadContent()
        {
            mainSceneExecuting = true;

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
                Color = new Color(80, 75, 85),
                Intensity = 5f,
            };

            #endregion

            finalEngineLogo = new GameObject3D(new FileModel("XNAFinalEngine"), new BlinnPhong { DiffuseColor = new Color(250, 250, 250) });
            finalEngineLogo.Transform.Position = new Vector3(-37, 6, 0);
            finalEngineLogo.Transform.LocalScale = new Vector3(1.3f, 1.3f, 1.3f);

            xnaLogo = new GameObject3D(new FileModel("XNA"), new BlinnPhong { DiffuseColor = new Color(250, 120, 0) });
            xnaLogo.Transform.Position = new Vector3(-37, 6, 0);
            xnaLogo.Transform.LocalScale = new Vector3(1.3f, 1.3f, 1.3f);

            examplesLogo = new GameObject3D(new FileModel("Examples"), new BlinnPhong { DiffuseColor = new Color(150, 250, 0) });
            examplesLogo.Transform.Position = new Vector3(40, 5.7f, 0);
            examplesLogo.Transform.LocalScale = new Vector3(1f, 1f, 1f);

            selectOneScene = new GameObject3D(new FileModel("SelectOneScene"), new BlinnPhong { DiffuseColor = new Color(150, 250, 0) });
            selectOneScene.Transform.Position = new Vector3(6, -55, 0);

            loading = new GameObject3D(new FileModel("Loading"), new BlinnPhong { DiffuseColor = new Color(150, 150, 150) });
            loading.Transform.Position = new Vector3(-7, -5, 0);
            loading.ModelRenderer.Enabled = false;

            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.Color = new Color(190, 110, 150);
            directionalLight.DirectionalLight.Intensity = 1.2f;
            directionalLight.Transform.LookAt(new Vector3(0.6f, 0.05f, 0.6f), Vector3.Zero, Vector3.Forward);

            #region Scene Information

            exampleImage = new GameObject2D[4];
            exampleTitle = new GameObject2D[4];
            exampleText = new GameObject2D[4];

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
            exampleText[0].HudText.Text.Append("This scene shows a Lamborghini Murcielago LP640 in a warehouse.\n\nThe car consists of 430.000 polygons. The scene includes an ambient light with spherical\nharmonic lighting and horizon based ambient occlusion (PC only), one directional light\nwith cascade shadows,two point lights and one spot light with a light mask. The post\nprocessing stage uses tone mapping, morphological antialiasing (MLAA), bloom and film\ngrain. Also a particles system that emits soft tiled particles was placed.\n\nAlmost all the car was modeled by me, with the exception of some interior elements, the\nengine and the rear lights because of my lack of texture experience and time.\n\nMehar Gill (Dog Fight Studios) provides me the warehouse model (originally created\nby igorlmax).\n\nThe reflections do not match the environment. I plan to address this soon.");
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
            exampleText[1].HudText.Text.Append("Bepu physics library was integrated on the engine through a simple interface that hides the\ncommunication between the physic and graphic world.\n\nThis example shows the interaction between dynamic and kinematic rigid bodies and static\nmeshes. This example also loads a non-centered sphere, the interface implemented detects\nthe offset between the center of mass of the object and the model space center and match\nboth representations without user action.\n\nGarbage generation could be avoided if the initial pools are configured correctly and no other\nmemory allocation should occur. Bepu physics 1.2 has a bug in witch garbage is generated,\nto avoid this we use the developement version of Bepu that fix this bug.\n\n");
            exampleText[1].HudText.Font = new Font("BellGothicText");
            exampleText[1].Transform.Position = new Vector3(500, 250, 0);

            #endregion

            #region Animation Scene
            
            exampleImage[2] = new GameObject2D();
            exampleImage[2].AddComponent<HudTexture>();
            exampleImage[2].HudTexture.Texture = new Texture("ExamplesImages\\AnimationScene");
            exampleImage[2].Transform.Position = new Vector3(75, 250, 0);
            exampleTitle[2] = new GameObject2D();
            exampleTitle[2].AddComponent<HudText>();
            exampleTitle[2].HudText.Text.Append("Animations");
            exampleTitle[2].HudText.Font = new Font("BellGothicTitle");
            exampleTitle[2].Transform.Position = new Vector3(500, 200, 0);
            exampleText[2] = new GameObject2D();
            exampleText[2].AddComponent<HudText>();
            exampleText[2].HudText.Text.Append("This is the simple scene used to test the animation system. It includes three animations:\nwalk, run and shoot. Animation blending is performed in the transition between different\nactions.\n\nDog Fight Studios have shared with us three of their test animations resources. However\nthe animations should not be used in commercial projects.");
            exampleText[2].HudText.Font = new Font("BellGothicText");
            exampleText[2].Transform.Position = new Vector3(500, 250, 0);
            
            #endregion

            #region Hellow World Scene

            exampleImage[3] = new GameObject2D();
            exampleImage[3].AddComponent<HudTexture>();
            exampleImage[3].HudTexture.Texture = new Texture("ExamplesImages\\HelloWorldScene");
            exampleImage[3].Transform.Position = new Vector3(75, 250, 0);
            exampleTitle[3] = new GameObject2D();
            exampleTitle[3].AddComponent<HudText>();
            exampleTitle[3].HudText.Text.Append("Hello World");
            exampleTitle[3].HudText.Font = new Font("BellGothicTitle");
            exampleTitle[3].Transform.Position = new Vector3(500, 200, 0);
            exampleText[3] = new GameObject2D();
            exampleText[3].AddComponent<HudText>();
            exampleText[3].HudText.Text.Append("This is from the documentation's tutorial that shows you how to create a simple scene\nso that you can understand the basic mechanism involved in the creation of a game world.");
            exampleText[3].HudText.Font = new Font("BellGothicText");
            exampleText[3].Transform.Position = new Vector3(500, 250, 0);

            #endregion

            #region Editor Scene

            /*#if !Xbox
                exampleImage[3] = new GameObject2D();
                exampleImage[3].AddComponent<HudTexture>();
                exampleImage[3].HudTexture.Texture = new Texture("ExamplesImages\\EditorScene");
                exampleImage[3].Transform.Position = new Vector3(75, 250, 0);
                exampleTitle[3] = new GameObject2D();
                exampleTitle[3].AddComponent<HudText>();
                exampleTitle[3].HudText.Text.Append("Editor");
                exampleTitle[3].HudText.Font = new Font("BellGothicTitle");
                exampleTitle[3].Transform.Position = new Vector3(500, 200, 0);
                exampleText[3] = new GameObject2D();
                exampleText[3].AddComponent<HudText>();
                exampleText[3].HudText.Text.Append("Unfortunately the editor is not finished, but most of the key elements are already done and it is\nvery easy to create new windows because everything is parameterized (thanks in part to .NET\nreflection) and its internal code is very clean.\n\nAt the moment it is possible to transform objects in global and local space, configure materials,\nlights, shadows, cameras and the post processing stage. You can also see the scene from\northographic views and perform undo and redo operations over almost all editor commands.\n\nIn the Editor Shortcuts section of the CodePlex documentation there is a list of all useful\nkeyboard shortcuts.");
                exampleText[3].HudText.Font = new Font("BellGothicText");
                exampleText[3].Transform.Position = new Vector3(500, 250, 0);
            #endif*/

            #endregion

            #endregion

            #region Legend

            demoLegend = new GameObject2D();            
            var legend = (HudText) demoLegend.AddComponent<HudText>();
            legend.Color = new Color(0.5f, 0.5f, 0.5f, 1f);
            #if XBOX
                legend.Text.Append("Start to show the statistics\n");
                legend.Text.Append("Back to comeback to this menu");
            #else
                legend.Text.Append("Start or F1 to show the statistics\n");
                legend.Text.Append("Back or Escape to comeback to this menu");
            #endif

            #endregion

            // Set Default Layer.
            Layer.CurrentCreationLayer = Layer.GetLayerByNumber(0);

            MusicManager.LoadAllSong(true);
            MusicManager.Play(1, true);
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        protected override void UpdateTasks()
        {
            // Show or hide statistics graph.
            if (Button.JustPressed("Show Statistics"))
                Layer.GetLayerByNumber(26).Visible = !Layer.GetLayerByNumber(26).Visible;
            // If the scene is executing...
            if (mainSceneExecuting)
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
                    demoLegend.Transform.Position = new Vector3(20, Screen.Height - 35f, 0f);
                    if (finalEngineLogo.Transform.Position.X < -6)
                    {
                        finalEngineLogo.Transform.Translate(Time.SmoothFrameTime * 50, 0, 0);
                        xnaLogo.Transform.Translate(Time.SmoothFrameTime * 50, 0, 0);
                    }
                    else
                    {
                        xnaLogo.Transform.Position = new Vector3(-6, 6, 0);
                        finalEngineLogo.Transform.Position = new Vector3(-6, 6, 0);
                    }
                    if (examplesLogo.Transform.Position.X > 1.3f)
                        examplesLogo.Transform.Translate(-Time.SmoothFrameTime * 70, 0, 0);
                    else
                        examplesLogo.Transform.Position = new Vector3(1.3f, 5.7f, 0);
                    if (selectOneScene.Transform.Position.Y < -5)
                        selectOneScene.Transform.Translate(0, Time.SmoothFrameTime * 65, 0);
                    else
                        selectOneScene.Transform.Position = new Vector3(6, -5, 0);
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
                        loading.ModelRenderer.Enabled = true;
                        mainSceneExecuting = false;
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
                        // Center the scenes information.
                        int offsetx = (Screen.Width - 1100) / 2;
                        int offsety = (Screen.Height - 400) / 2;
                        exampleImage[i].Transform.Position = new Vector3(offsetx, offsety + 50, 0);
                        exampleTitle[i].Transform.Position = new Vector3(425 + offsetx, offsety, 0);
                        exampleText[i].Transform.Position = new Vector3(425 + offsetx, offsety + 50, 0);
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
                Layer.GetLayerByNumber(25).Visible = false;
                if (loadedScene == null)
                {
                    loading.ModelRenderer.Enabled = false;
                    switch (currentScene)
                    {
                        case 0:
                            loadedScene = new WarehouseScene();
                            break;
                        case 1:
                            loadedScene = new PhysicsTestScene();
                            break;
                        case 2:
                            loadedScene = new AnimationScene();
                            break;
                        case 3:
                            loadedScene = new HelloWorldScene();
                            break;
                    }
                }
                if (Button.JustPressed("Back To Menu"))
                {
                    loadedScene.Dispose();
                    loadedScene = null;
                    mainSceneExecuting = true;
                }
            }
        } // UpdateTasks

        #endregion

    } // ExamplesMainScene
} // XNAFinalEngineExamples
