
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

        private SceneLamborghiniLP640 lp640Scene;

        private ContainerObject cShadowObjects;

        private GraphicObject plane;

        #region Lights

        /// <summary>
        /// Scene's lights
        /// </summary>
        private Light pointLight1, pointLight2, pointLight3, directionalLight, spotLight;

        #endregion

        #region PrePostScreenShaders

        private bool esssao = true;
        private bool esshadow = true;
        private PreDepthNormalShader preDepthShader;
        private ShadowMapShader shadowMapShader;
        private SSAOHorizonBased ssaoHB;
        private SSAORayMarching ssaoRM;
        private Blur blurShadow;
        private Blur blurSSAO;
        private ConvineShadows convineShadow;

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
            ApplicationLogic.Camera = new FixedCamera(new Vector3(0, 0, 20), new Vector3(0, 0, 0));

            lp640Scene = new SceneLamborghiniLP640();
            lp640Scene.Load();
            lp640Scene.DirectionAngle = -30;

            plane = new GraphicObject(new GraphicElements.Plane(50,50), new Constant());

            cShadowObjects = new ContainerObject();
            cShadowObjects.AddContainerObject(lp640Scene.Model);
            cShadowObjects.AddGraphicObject(plane);

            plane.RotateAbs(90, 0, 0);

            float scale = 20;
            cShadowObjects.ScaleAbs(scale);

            #region Lights

            AmbientLight.LightColor = new Color(150, 150, 150);

            directionalLight = new DirectionalLight(new Vector3(-0.1f, -1.0f, 0.1f), new Color(100, 120, 130));
            pointLight1 = new PointLight(new Vector3(15 * scale, 12 * scale, -30 * scale), new Color(100, 100, 100));
            pointLight2 = new PointLight(new Vector3(-30 * scale, 50 * scale, 1 * scale), new Color(180, 180, 180));
            pointLight3 = new PointLight(new Vector3(-20 * scale, 3.1f * scale, -20 * scale), new Color(140, 140, 140));
            spotLight = new SpotLight(new Vector3(-20 * scale, 10 * scale, 0), new Vector3(2, -1, 0), new Color(100, 100, 150), 30, 1);

            cShadowObjects.AssociateLight(pointLight1);
            cShadowObjects.AssociateLight(pointLight2);
            cShadowObjects.AssociateLight(pointLight3);
            cShadowObjects.AssociateLight(directionalLight);

            #endregion

            #region Post Screen Shaders

            preDepthShader = new PreDepthNormalShader(true);
            preDepthShader.FarPlane = 200;

            shadowMapShader = new ShadowMapShader(RenderToTexture.SizeType.Custom512x512);
            shadowMapShader.LightDirection = new Vector3(-0.1f, -0.7f, -0.1f);
            shadowMapShader.AssociatedLight = directionalLight;
            shadowMapShader.ShadowColor = new Color(130, 130, 130);
            shadowMapShader.VirtualLightDistance = 150;
            shadowMapShader.FarPlane = 200;
            blurShadow = new Blur();
            blurShadow.BlurWidth = 3;
            blurSSAO = new Blur();
            blurSSAO.BlurWidth = 2.0f;
            ssaoHB = new SSAOHorizonBased(RenderToTexture.SizeType.FullScreen);
            ssaoHB.NumberSteps = 32;
            ssaoHB.NumberDirections = 15;
            ssaoHB.Radius = 0.1f;
            ssaoHB.LineAttenuation = 1.5f;
            ssaoHB.Contrast = 1.5f;

            ssaoRM = new SSAORayMarching();
            convineShadow = new ConvineShadows();

            #endregion

            #region Augmented Reality

            webCamTexture = new GraphicElements.Texture();

            // ARToolkit Plus
            //webCam = new DirectShowWebCam(800, 600, 4, 30);
            //artoolkitplus = new ARToolKitPlus(webCam);
            
            // ARTAG
            webCam = new DirectShowWebCam(800, 600, 3, 30);
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
            lp640Scene.Update();
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
                shadowMapShader.GenerateLightDepthBuffer(cShadowObjects);
                shadowMapShader.GenerateShadowMap(cShadowObjects);
                blurShadow.GenerateBlur(shadowMapShader.ShadowMapTexture);
            }
            if (esssao)
            {
                preDepthShader.GenerateDepthNormalMap(cShadowObjects);
                ssaoHB.GenerateSSAO(PreDepthNormalShader.HighPrecisionDepthMapTexture.XnaTexture, PreDepthNormalShader.NormalDepthMapTexture.XnaTexture);
                blurSSAO.GenerateBlur(ssaoHB.SSAOTexture);
            }

            #endregion

            EngineManager.ClearTargetAndDepthBuffer(Color.Blue);

            webCamTexture.XnaTexture = webCam.XNATexture;
            webCamTexture.RenderOnFullScreen();
            SpriteManager.DrawSprites();
            lp640Scene.Model.Render();

            #region Post Work

            if (esshadow)
            {
                convineShadow.GenerateConvineShadows(blurShadow.BlurMapTexture);
                SpriteManager.DrawSprites();
            }
            if (esssao)
            {
                convineShadow.GenerateConvineShadows(blurSSAO.BlurMapTexture);
                SpriteManager.DrawSprites();
            }
            //ssaoHB.Test();
            //UIMousePointer.RenderMousePointer();

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
