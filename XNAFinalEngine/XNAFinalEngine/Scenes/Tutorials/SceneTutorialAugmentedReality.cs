
#region Using directives
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using XNAFinalEngine.GraphicElements;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.AugmentedReality;
using XNAFinalEngine.UI;
using XNAFinalEngine.Input;

#endregion

namespace XNAFinalEngine.Scenes
{
    /// <summary>
    /// Augmented reality scene.
    /// </summary>
    public class SceneAugmentedReality : Scene
    {

        #region Variables

        private ContainerObject cShadowObjects;

        private GraphicObject plane, box;

        #region Lights

        /// <summary>
        /// Scene's lights
        /// </summary>
        private Light pointLight1, pointLight2, pointLight3, directionalLight, spotLight;

        #endregion

        #region PrePostScreenShaders

        private bool esssao = false;
        private bool esshadow = false;
        private PreDepthNormal preDepthNormal;
        private ShadowMap shadowMap;
        private SSAOHorizonBased ssaoHB;
        private Blur blurShadow;
        private Blur blurSSAO;
        private CombineShadows combineShadow;

        #endregion

        #region Augmented Reality

        /// <summary>
        /// Artoolkit Plus. A AR tracker.
        /// </summary>
        private ARToolKitPlus artoolkitplus;
        
        /// <summary>
        /// Artag. A AR tracker.
        /// </summary>
        private ARTag artag;

        /// <summary>
        /// The set of markers to track by artag.
        /// </summary>
        private ARTag.MarkerArray markerArray;

        /// <summary>
        /// Webcam.
        /// </summary>
        private WebCam webCam;

        /// <summary>
        /// Texture used to render the webCam frame into screen.
        /// </summary>
        private GraphicElements.Texture webCamTexture;

        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        public override void Load()
        {
            float scale = 10;

            ApplicationLogic.Camera = new FixedCamera(new Vector3(-2 * scale, 30 * scale, 20 * scale), new Vector3(0, 0, 0));
            //ApplicationLogic.Camera = new XSICamera(Vector3.Zero, 20 * scale);

            box = new GraphicObject(new Box(2), new CarPaint());

            plane = new GraphicObject(new GraphicElements.Plane(10, 10), new Constant(Color.Red));

            cShadowObjects = new ContainerObject();
            cShadowObjects.AddObject(box);
            cShadowObjects.AddObject(plane);

            plane.RotateAbs(-90, 0, 0);
            plane.TranslateAbs(0, -1, 0);
            
            cShadowObjects.ScaleAbs(scale);
            
            #region Lights

            AmbientLight.LightColor = new Color(150, 150, 150);

            directionalLight = new GraphicElements.DirectionalLight(new Vector3(-0.1f, -1.0f, 1.1f), new Color(100, 120, 130));
            pointLight1 = new PointLight(new Vector3(15 * scale, 12 * scale, -30 * scale), new Color(200, 100, 100));
            pointLight2 = new PointLight(new Vector3(-30 * scale, 50 * scale, 1 * scale), new Color(0, 250, 0));
            pointLight3 = new PointLight(new Vector3(-20 * scale, 3.1f * scale, -20 * scale), new Color(0, 0, 250));
            spotLight = new SpotLight(new Vector3(-20 * scale, 10 * scale, 0), new Vector3(2, -1, 0), new Color(100, 100, 150), 30, 1);

            cShadowObjects.AssociateLight(pointLight1);
            cShadowObjects.AssociateLight(pointLight2);
            cShadowObjects.AssociateLight(pointLight3);
            cShadowObjects.AssociateLight(directionalLight);

            #endregion

            #region Post Screen Shaders

            preDepthNormal = new PreDepthNormal(true) { FarPlane = 50 * scale };

            shadowMap = new ShadowMap(RenderToTexture.SizeType.Custom512x512);
            shadowMap.AssociatedLight = directionalLight;
            shadowMap.ShadowColor = new Color(130, 130, 130);
            shadowMap.VirtualLightDistance = 20 * scale;
            shadowMap.FarPlane = 30 * scale;

            blurShadow = new Blur() { BlurWidth = 5f };
            blurSSAO = new Blur() { BlurWidth = 1.5f };
            ssaoHB = new SSAOHorizonBased(RenderToTexture.SizeType.HalfScreen)
            {
                NumberSteps = 32,
                NumberDirections = 15,
                Radius = 0.2f,
                LineAttenuation = 1.0f,
                Contrast = 2.0f,
                AngleBias = 30
            };
            combineShadow = new CombineShadows();

            #endregion
            
            #region Augmented Reality

            webCamTexture = new GraphicElements.Texture();

            // ARToolkit Plus
            //webCam = new DirectShowWebCam(800, 600, 4, 30);
            //artoolkitplus = new ARToolKitPlus(webCam);
            
            // ARTAG
            webCam = new DirectShowWebCam(0, 640, 480, 3, 30);
            artag = new ARTag(webCam);
            markerArray = artag.AddArtagMarkerArray("base0");

            #endregion

            EngineManager.ShowFPS = true;
            
        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update the scene.
        /// </summary>
        public override void Update()
        {
            // Grab a new frame from the webcam.
            webCam.CalculateNewFrame();
            if (artag != null)
            {
                artag.Tracking();
                //if (markerArray.isFound)
                {
                    ApplicationLogic.Camera.ViewMatrix = markerArray.viewMatrix;
                    ApplicationLogic.Camera.ProjectionMatrix = artag.ProjectionMatrix;
                }
            }
            if (artoolkitplus != null)
            {
                artoolkitplus.Tracking();
                ApplicationLogic.Camera.ViewMatrix = artoolkitplus.ViewMatrix;
                ApplicationLogic.Camera.ProjectionMatrix = artoolkitplus.ProjectionMatrix;
            }
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render the scene.
        /// </summary>
        public override void Render()
        {

            #region Pre Work

            if (Keyboard.LeftJustPressed)
            {
                esshadow = !esshadow;
            }
            if (Keyboard.RightJustPressed)
            {
                esssao = !esssao;
            }
            if (esshadow)
            {
                shadowMap.GenerateLightDepthBuffer(cShadowObjects);
                shadowMap.GenerateShadows(cShadowObjects);
                blurShadow.GenerateBlur(shadowMap.ShadowMapTexture);
            }
            if (esssao)
            {
                preDepthNormal.GenerateDepthNormalMap(cShadowObjects);
                ssaoHB.GenerateSSAO(preDepthNormal.HighPrecisionDepthMapTexture.XnaTexture, preDepthNormal.NormalDepthMapTexture.XnaTexture);
                blurSSAO.GenerateBlur(ssaoHB.SSAOTexture);
            }

            #endregion

            EngineManager.ClearTargetAndDepthBuffer(Color.Blue);

            webCamTexture.XnaTexture = webCam.XNATexture;
            webCamTexture.RenderOnFullScreen();
            SpriteManager.DrawSprites();
            box.Render();

            #region Post Work

            if (esshadow)
            {
                combineShadow.GenerateCombineShadows(blurShadow.BlurMapTexture);
                //shadowMap.lightDepthBufferTexture.RenderOnFullScreen();
                SpriteManager.DrawSprites();
            }
            if (esssao)
            {
                combineShadow.GenerateCombineShadows(blurSSAO.BlurMapTexture);
                //ssaoHB.SSAOTexture.RenderOnFullScreen();
                SpriteManager.DrawSprites();
            }
            //ssaoHB.Test();
            //shadowMap.Test();
            UIMousePointer.RenderMousePointer();

            #endregion

        } // Render

        #endregion

        #region Unload Content
        
        /// <summary>
        /// Codigo que se ejecuta cada vez que se necesita descargar elementos
        /// </summary>
        public override void UnloadContent()
        {   
            if (artag != null)
            {
                artag.Dispose();
            }
            if (artoolkitplus != null)
            {
                artoolkitplus.Dispose();
            }
            if (webCam != null)
            {
                webCam.Dispose();
            }
        } // UnloadContent

        #endregion

    } // AugmentedRealityScene
} // XNAFinalEngine.Scenes
