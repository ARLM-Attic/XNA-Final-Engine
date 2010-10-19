
#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.GraphicElements;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
using XNAFinalEngine.Sounds;
using XNAFinalEngine.Input;
using DirectionalLight = XNAFinalEngine.GraphicElements.DirectionalLight;
#endregion

namespace XNAFinalEngine.Scenes
{

    /// <summary>
    /// Scene that shows the Lamborghini Murcielago LP640. This scene needs a father scene. It doesn't take care of ilumination.
    /// </summary>
    public class SceneLamborghiniLP640 : Scene
    {

        #region Variables

        /// <summary>
        /// Left door angle.
        /// </summary>
        private int leftDoorAngle = 0;
        
        /// <summary>
        /// Right door angle.
        /// </summary>
        private int rightDoorAngle = 0;
        
        /// <summary>
        /// Direction or wheel angle.
        /// </summary>
        private int directionAngle = 30;

        /// <summary>
        /// Indicates if the resources used to render the scene are loaded.
        /// </summary>
        private bool otherResourcesLoaded = false;
                
        #region Objects

        /// <summary>
        /// Graphic objects.
        /// </summary>
        private GraphicObject gBody, gOpaqueBlackMetal, gLP640GlossyBlackMetal, gRedMetal, gLightBlueMetal,
                              gWhiteMetal, gAirIntakes, gCarbonFiber, gLeather, gGlasses, gGlassFrontLight,
                              gBoard, gLogo, gRearLights, gRearLights2, gRearLights3, gLeftLight,
                              gGrid, gExhaust, gExhaustCopper, gFloor,
                              // Tires //
                              gRearTires01,      gRearTires02,      gRearRims,      gRearBrakeDisks,      gRearBrakes,      gRearRimSupports,      gRearRimLogos,
                              gFrontLeftTire01,  gFrontLeftTire02,  gFrontLeftRim,  gFrontLeftBrakeDisk,  gFrontLeftBrake,  gFrontLeftRimSupport,  gFrontLeftRimLogo,
                              gFrontRightTire01, gFrontRightTire02, gFrontRightRim, gFrontRightBrakeDisk, gFrontRightBrake, gFrontRightRimSupport, gFrontRightRimLogo,
                              // Doors //
                              gLeftDoorBody, gLeftDoorGlossyBlackMetal, gLeftDoorWhiteMetal, gLeftDoorLeather,
                              gLeftDoorGlass, gLeftDoorSpeaker, 
                              gRightDoorBody, gRightDoorGlossyBlackMetal, gRightDoorWhiteMetal, gRightDoorLeather,
                              gRightDoorGlass, gRightDoorSpeaker,
                              // Pilot //
                              gPilot01, gPilot02, gPilot03,
                              plane;

        /// <summary>
        /// Container objects.
        /// </summary>
        private ContainerObject cOpaqueObjects, cGlassObjects, cEntireObject, cRearTires,
                                cFrontLeftTires, cFrontRightTires, cLeftDoor, gRightDoor,
                                cFrontLeftTiresSpinning, cFrontRightTiresSpinning, cRearTiresSpinning;

        #endregion
        
        #region Materials

        private CarPaint matCarPaint,
                         matRim,
                         matGlass,
                         matGlassFrontLight,
                         matGlassRearLight,
                         matGlassRearLight2,
                         matGlossyBlack;
        private ParallaxMapping matTire;
                
        #endregion

        #region Lights

        /// <summary>
        /// Scene's lights
        /// </summary>
        private Light pointLight1, pointLight2, pointLight3, directionalLight, spotLight;

        #endregion

        #region PrePostScreenShaders

        private bool esssao = false;
        private bool esshadow = true;
        private PreDepthNormal preDepthShader;
        private PreSkybox skybox;
        private ShadowMapShader shadowMapShader;
        private SSAOHorizonBased ssaoHB;
        private Blur blurShadow;
        private Blur blurSSAO;
        private CombineShadows convineShadow;
        private SSAORayMarching ssaoRM;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The object that has the complete model of the car.
        /// </summary>
        public ContainerObject Model { get { return cEntireObject; } }

        /// <summary>
        /// Left door angle.
        /// </summary>
        public int LeftDoorAngle { set { leftDoorAngle = value; } }

        /// <summary>
        /// Right door angle.
        /// </summary>
        public int RightDoorAngle { set { rightDoorAngle = value; } }

        /// <summary>
        /// Direction or wheel angle.
        /// </summary>
        public int DirectionAngle { set { directionAngle = value; } }

        #endregion

        #region Load

        /// <summary>
        /// Load the scene.
        /// </summary>
        public override void Load()
        {   
            String skyboxFilename = "Skybox-GrimNight";

            #region Load the models

            // Body
            matCarPaint = new CarPaint(skyboxFilename)
            {
                Shininess = 5,
                FresnelBias = 1.2f,
                FresnelPower = 1.0f,
                Reflection = 0.8f,
                SurfaceColor = new Color(150, 150, 150),
                EdgeColor = new Color(56, 56, 56),
                MiddleColor = new Color(40, 40, 40)
            };
            gBody = new GraphicObject(new Box(2)/*"LP640Body"*/, matCarPaint);
            ApplicationLogic.Camera = new XSICamera(Vector3.Zero);
            // Light Blue Metal
            /*gLightBlueMetal = new GraphicObject("LP640MetalCeleste", new Constant(Color.LightBlue));
            // White Metal
            gWhiteMetal = new GraphicObject("LP640MetalBlanco", new Blinn(Color.White, 0.5f, 50, 1));
            // Glossy Black
            matGlossyBlack = new CarPaint(skyboxFilename)
            {
                Shininess = 5,
                FresnelBias = 3.0f,
                FresnelPower = 2.5f,
                Reflection = 0.8f,
                SurfaceColor = new Color(10, 10, 10),
                SpecularColor = new Color(250, 250, 250),
                EdgeColor = new Color(10, 10, 10),
                MiddleColor = new Color(15, 15, 15)
            };
            gLP640GlossyBlackMetal = new GraphicObject("LP640MetalNegroBrillante", matGlossyBlack);
            // Opaque Black Metal
            gOpaqueBlackMetal = new GraphicObject("LP640MetalNegroOpaco", new Blinn(Color.Black, 0.0f, 50, 1));
            // Red Metal
            gRedMetal = new GraphicObject("LP640MetalRojo", new Blinn(Color.Red, 0.0f, 50, 1));
            // Air intakes
            gAirIntakes = new GraphicObject("LP640TomasAireMotor", new Blinn("LP640TomasAireMotor", 0.0f, 50, 1));
            // Carbon fiber
            gCarbonFiber = new GraphicObject("LP640FibraCarbono", new Blinn("LP640FibraCarbono", 0.0f, 50, 1));
            // Leather
            gLeather = new GraphicObject("LP640Cuero", new Blinn(new Color(20, 20, 20), 0.3f, 10, 1));
            // Board
            gBoard = new GraphicObject("LP640Tablero", new Constant("LP640Tablero"));
            // Logo
            gLogo = new GraphicObject("LP640Logo", new Blinn("LP640LogoLamborghini", 0.0f, 50, 1));
            // Left light
            gLeftLight = new GraphicObject("LP640GuineLuzDelantera", new Constant("LP640GuineLuzDelantera"));
            // Floor
            gFloor = new GraphicObject("LP640Piso", new Blinn("LP640Piso", 0.0f, 50, 1));
            // Grid
            gGrid = new GraphicObject("LP640Rejilla", new Blinn("LP640Rejilla", 0.0f, 50, 1));
            // Exhaust
            gExhaust = new GraphicObject("LP640CanoEscape", new Blinn("LP640CanoEscape", 0.0f, 50, 1));
            // Exhaust Copper
            gExhaustCopper = new GraphicObject("LP640CanoEscapeCobre", new Blinn("LP640CanoEscapeCobre", 0.0f, 50, 1));

            #region Rear Lights

            gRearLights = new GraphicObject("LP640LucesTraceras", new Blinn("LP640LucesTraceras", 1.5f, 500, 1));
            matGlassRearLight = new CarPaint("Skybox-Miramar")
            {
                Shininess = 15,
                FresnelBias = 0.15f,
                FresnelPower = 0.5f,
                Reflection = 1.0f,
                AlphaBlending = 0.4f,
                SurfaceColor = new Color(255, 100, 100),
                SpecularColor = new Color(255, 0, 0),
                EdgeColor = new Color(50, 200, 200),
                MiddleColor = new Color(255, 175, 175)
            };
            gRearLights2 = new GraphicObject("LP640LucesTraceras2", matGlassRearLight);
            gRearLights2.TranslateAbs(0,0,-0.01f);
            matGlassRearLight2 = new CarPaint("Skybox-Miramar")
            {
                Shininess = 15,
                FresnelBias = 0.15f,
                FresnelPower = 0.5f,
                Reflection = 1.0f,
                AlphaBlending = 0.4f,
                SurfaceColor = new Color(255, 255, 255),
                SpecularColor = new Color(255, 255, 255),
                EdgeColor = new Color(140, 140, 140),
                MiddleColor = new Color(155, 155, 155)
            };
            gRearLights3 = new GraphicObject("LP640LucesTraceras3", matGlassRearLight2);
            gRearLights3.TranslateAbs(0, 0, -0.01f);

            #endregion

            #region Glasses

            // Glasses //
            matGlass = new CarPaint("Skybox-Miramar")
            {
                Shininess = 3,
                FresnelBias = 3.5f,
                FresnelPower = 0.3f,
                Reflection = 0.9f,
                AlphaBlending = 0.3f,
                SurfaceColor = new Color(0, 0, 20),
                SpecularColor = new Color(255, 255, 255),
                EdgeColor = new Color(60, 50, 60),
                MiddleColor = new Color(60, 60, 70)
            };
            gGlasses = new GraphicObject("LP640Vidrios", matGlass);
            // Front Light Glass //
            matGlassFrontLight = new CarPaint("Skybox-Miramar")
            {
                Shininess = 2,
                FresnelBias = 2.0f,
                FresnelPower = 0.5f,
                Reflection = 1.0f,
                AlphaBlending = 0.5f,
                SurfaceColor = new Color(20, 20, 55),
                SpecularColor = new Color(255, 225, 255),
                EdgeColor = new Color(128, 128, 104),
                MiddleColor = new Color(60, 60, 70)
            };
            gGlassFrontLight = new GraphicObject("LP640VidriosLuces", matGlass);

            #endregion

            #region Pilot

            gPilot01 = new GraphicObject("LP640Piloto-01", new Blinn("LP640Piloto-01", 0.0f, 50, 1));
            gPilot02 = new GraphicObject("LP640Piloto-02", new Blinn("LP640Piloto-02", 0.0f, 50, 1));
            gPilot03 = new GraphicObject("LP640Piloto-03", matGlass);

            #endregion

            #region Tires

            // Rear
            gRearTires01     = new GraphicObject("LP640RuedasTracerasCubiertas", new Blinn("LP640PirelliPZero", 2.0f, 200, 1));
            matTire          = new ParallaxMapping("LP640Cubiertas-Diffuse", "LP640Cubiertas-Normal")
            {
                ParallaxAmount = 0,
                SpecularColor = new Color(30, 30, 30),
                Shininess = 10
            };
            gRearTires02     = new GraphicObject("LP640RuedasTracerasCubiertas2", matTire);
            gRearBrakeDisks  = new GraphicObject("LP640RuedasTracerasDiscosDeFreno", new Blinn("LP640DiscosDeFreno", 0.3f, 200, 1));
            gRearBrakes      = new GraphicObject("LP640RuedasTracerasFrenos", new Blinn("LP640Frenos", 0.3f, 200, 1));
            gRearRimSupports = new GraphicObject("LP640RuedasTracerasAgarraLlanta", matGlossyBlack);
            gRearRimLogos    = new GraphicObject("LP640RuedasTracerasLogos", new Blinn("LP640LogoLamborghini", 0.3f, 200, 1));
            matRim           = new CarPaint("Skybox-Miramar")
            {
                Shininess = 5,
                FresnelBias = 1.3f,
                FresnelPower = 1.3f,
                Reflection = 1.0f,
                SurfaceColor = new Color(120, 120, 120),
                EdgeColor = new Color(105, 105, 105),
                MiddleColor = new Color(150, 150, 150)
            };
            gRearRims = new GraphicObject("LP640RuedasTracerasLlantas", matRim);
            // Front Left
            gFrontLeftTire01      = new GraphicObject("LP640RuedasDelanteraIzqCubiertas", new Blinn("LP640PirelliPZero", 2.0f, 200, 1));
            gFrontLeftTire02      = new GraphicObject("LP640RuedasDelanteraIzqCubiertas2", matTire);
            gFrontLeftBrakeDisk   = new GraphicObject("LP640RuedasDelanteraIzqDiscosDeFreno", new Blinn("LP640DiscosDeFreno", 0.3f, 200, 1));
            gFrontLeftBrake       = new GraphicObject("LP640RuedasDelanteraIzqFrenos", new Blinn("LP640Frenos", 0.5f, 20, 1));
            gFrontLeftRimSupport  = new GraphicObject("LP640RuedasDelanteraIzqAgarraLlanta", matGlossyBlack);
            gFrontLeftRimLogo     = new GraphicObject("LP640RuedasDelanteraIzqLogos", new Blinn("LP640LogoLamborghini", 0.3f, 200, 1));
            gFrontLeftRim         = new GraphicObject("LP640RuedasDelanteraIzqLlantas", matRim);
            // Front Right
            gFrontRightTire01     = new GraphicObject("LP640RuedasDelanteraDerCubiertas", new Blinn("LP640PirelliPZero", 2.0f, 200, 1));
            gFrontRightTire02     = new GraphicObject("LP640RuedasDelanteraDerCubiertas2", matTire);
            gFrontRightBrakeDisk  = new GraphicObject("LP640RuedasDelanteraDerDiscosDeFreno", new Blinn("LP640DiscosDeFreno", 0.3f, 200, 1));
            gFrontRightBrake      = new GraphicObject("LP640RuedasDelanteraDerFrenos", new Blinn("LP640Frenos", 0.5f, 20, 1));
            gFrontRightRimSupport = new GraphicObject("LP640RuedasDelanteraDerAgarraLlanta", matGlossyBlack);
            gFrontRightRimLogo    = new GraphicObject("LP640RuedasDelanteraDerLogos", new Blinn("LP640LogoLamborghini", 0.3f, 200, 1));
            gFrontRightRim        = new GraphicObject("LP640RuedasDelanteraDerLlantas", matRim);

            #endregion

            #region Doors

            gLeftDoorBody = new GraphicObject("LP640PuertaIzqCarroceria", matCarPaint);
            gLeftDoorGlossyBlackMetal = new GraphicObject("LP640PuertaIzqMetalNegroBrillante", matGlossyBlack);
            gLeftDoorWhiteMetal = new GraphicObject("LP640PuertaIzqMetalBlanco", new Blinn(Color.White, 0.5f, 50, 1));
            gLeftDoorLeather = new GraphicObject("LP640PuertaIzqCuero", new Blinn(new Color(20, 20, 20), 0.3f, 10, 1));
            gLeftDoorGlass = new GraphicObject("LP640PuertaIzqVidrios", matGlass);
            gLeftDoorSpeaker = new GraphicObject("LP640PuertaIzqParlante", new Blinn("LP640Parlante", 0.3f, 200, 1));

            gRightDoorBody = new GraphicObject("LP640PuertaDerCarroceria", matCarPaint);
            gRightDoorGlossyBlackMetal = new GraphicObject("LP640PuertaDerMetalNegroBrillante", matGlossyBlack);
            gRightDoorWhiteMetal = new GraphicObject("LP640PuertaDerMetalBlanco", new Blinn(Color.White, 0.5f, 50, 1));
            gRightDoorLeather = new GraphicObject("LP640PuertaDerCuero", new Blinn(new Color(20, 20, 20), 0.3f, 10, 1));
            gRightDoorGlass = new GraphicObject("LP640PuertaDerVidrios", matGlass);
            gRightDoorSpeaker = new GraphicObject("LP640PuertaDerParlante", new Blinn("LP640Parlante", 0.3f, 200, 1));

            #endregion
            */
            #endregion

            #region Build container objects

            cOpaqueObjects = new ContainerObject();
            cOpaqueObjects.AddObject(gBody);
            /*cOpaqueObjects.AddGraphicObject(gLightBlueMetal);
            cOpaqueObjects.AddGraphicObject(gWhiteMetal);
            cOpaqueObjects.AddGraphicObject(gLP640GlossyBlackMetal);
            cOpaqueObjects.AddGraphicObject(gOpaqueBlackMetal);
            cOpaqueObjects.AddGraphicObject(gRedMetal);
            cOpaqueObjects.AddGraphicObject(gAirIntakes);
            cOpaqueObjects.AddGraphicObject(gCarbonFiber);
            cOpaqueObjects.AddGraphicObject(gLeather);
            cOpaqueObjects.AddGraphicObject(gBoard);
            cOpaqueObjects.AddGraphicObject(gLogo);
            cOpaqueObjects.AddGraphicObject(gRearLights);
            cOpaqueObjects.AddGraphicObject(gLeftLight);
            cOpaqueObjects.AddGraphicObject(gFloor);
            cOpaqueObjects.AddGraphicObject(gGrid);
            cOpaqueObjects.AddGraphicObject(gExhaust);
            cOpaqueObjects.AddGraphicObject(gExhaustCopper);
            cOpaqueObjects.AddGraphicObject(gPilot01);
            cOpaqueObjects.AddGraphicObject(gPilot02);*/
            // Rear tires
            /*cRearTires = new ContainerObject();
            cOpaqueObjects.AddContainerObject(cRearTires);
            cRearTiresSpinning = new ContainerObject();
            cRearTires.AddContainerObject(cRearTiresSpinning);
            cRearTires.AddGraphicObject(gRearBrakeDisks);
            cRearTires.AddGraphicObject(gRearBrakes);
            cRearTires.AddGraphicObject(gRearRimSupports);
            cRearTires.TranslateAbs(0, -0.5f, -2.95f);
            cRearTiresSpinning.AddGraphicObject(gRearTires01);
            cRearTiresSpinning.AddGraphicObject(gRearTires02);
            cRearTiresSpinning.AddGraphicObject(gRearRims);
            cRearTiresSpinning.AddGraphicObject(gRearRimLogos);
            // Front left tires
            cFrontLeftTires = new ContainerObject();
            cOpaqueObjects.AddContainerObject(cFrontLeftTires);
            cFrontLeftTiresSpinning = new ContainerObject();
            cFrontLeftTires.AddContainerObject(cFrontLeftTiresSpinning);
            cFrontLeftTires.AddGraphicObject(gFrontLeftBrakeDisk);
            cFrontLeftTires.AddGraphicObject(gFrontLeftBrake);
            cFrontLeftTires.AddGraphicObject(gFrontLeftRimSupport);
            cFrontLeftTires.TranslateAbs(1.65f, -0.5f, 2.34f);
            cFrontLeftTiresSpinning.AddGraphicObject(gFrontLeftTire01);
            cFrontLeftTiresSpinning.AddGraphicObject(gFrontLeftTire02);
            cFrontLeftTiresSpinning.AddGraphicObject(gFrontLeftRim);
            cFrontLeftTiresSpinning.AddGraphicObject(gFrontLeftRimLogo);
            // Front right tires
            cFrontRightTires = new ContainerObject();
            cOpaqueObjects.AddContainerObject(cFrontRightTires);
            cFrontRightTiresSpinning = new ContainerObject();
            cFrontRightTires.AddContainerObject(cFrontRightTiresSpinning);
            cFrontRightTires.AddGraphicObject(gFrontRightBrakeDisk);
            cFrontRightTires.AddGraphicObject(gFrontRightBrake);
            cFrontRightTires.AddGraphicObject(gFrontRightRimSupport);
            cFrontRightTires.TranslateAbs(-1.65f, -0.5f, 2.34f);
            cFrontRightTiresSpinning.AddGraphicObject(gFrontRightTire01);
            cFrontRightTiresSpinning.AddGraphicObject(gFrontRightTire02);
            cFrontRightTiresSpinning.AddGraphicObject(gFrontRightRim);
            cFrontRightTiresSpinning.AddGraphicObject(gFrontRightRimLogo);
            // Left door
            cLeftDoor = new ContainerObject();
            cOpaqueObjects.AddContainerObject(cLeftDoor);
            cLeftDoor.AddGraphicObject(gLeftDoorBody);
            cLeftDoor.AddGraphicObject(gLeftDoorGlossyBlackMetal);
            cLeftDoor.AddGraphicObject(gLeftDoorWhiteMetal);
            cLeftDoor.AddGraphicObject(gLeftDoorLeather);
            cLeftDoor.AddGraphicObject(gLeftDoorSpeaker);
            cLeftDoor.TranslateAbs(0, 0.3922f, 1.96f);
            // Right door
            gRightDoor = new ContainerObject();
            cOpaqueObjects.AddContainerObject(gRightDoor);
            gRightDoor.AddGraphicObject(gRightDoorBody);
            gRightDoor.AddGraphicObject(gRightDoorGlossyBlackMetal);
            gRightDoor.AddGraphicObject(gRightDoorWhiteMetal);
            gRightDoor.AddGraphicObject(gRightDoorLeather);
            gRightDoor.AddGraphicObject(gRightDoorSpeaker);
            gRightDoor.TranslateAbs(0, 0.3922f, 1.96f);*/
            // Glasses
            cGlassObjects = new ContainerObject();
            /*cGlassObjects.AddGraphicObject(gRearLights2);
            cGlassObjects.AddGraphicObject(gRearLights3);
            cGlassObjects.AddGraphicObject(gGlasses);
            cGlassObjects.AddGraphicObject(gGlassFrontLight);
            cGlassObjects.AddGraphicObject(gPilot03);
            cGlassObjects.AddGraphicObject(gLeftDoorGlass);
            gLeftDoorGlass.TranslateAbs(0, 0.3922f, 1.96f);
            cGlassObjects.AddGraphicObject(gRightDoorGlass);
            gRightDoorGlass.TranslateAbs(0, 0.3922f, 1.96f);
            */
            cEntireObject = new ContainerObject();
            cEntireObject.AddObject(cOpaqueObjects);
            cEntireObject.AddObject(cGlassObjects);
            
            #endregion
            
            // Doesn't use cEntireObject because maybe the father scene change the local transformation matrix with absolute values.
            /*cGlassObjects.RotateAbs(0, 180, 0);
            cGlassObjects.TranslateAbs(0, 0.77f, 0);
            cGlassObjects.ScaleAbs(0.7f);
            cOpaqueObjects.RotateAbs(0, 180, 0);
            cOpaqueObjects.TranslateAbs(0, 0.77f, 0);*/
            cOpaqueObjects.ScaleAbs(0.7f);

            //cRearTiresSpinning.AssociateAnimation(new RotationAnimation(5f, new Vector3(360, 0, 0), true));
            EngineManager.ShowFPS = true;
        } // Load

        #endregion

        #region Load Other resources

        /// <summary>
        /// Load the resources used to render the scene.
        /// </summary>
        private void LoadOtherResources()
        {
            ApplicationLogic.Camera = new XSICamera(new Vector3(0, 0, 0), 13, 0.1f, 0);

            plane = new GraphicObject(new GraphicElements.Plane(50, 50), new Blinn(Color.White, 2, 30, 1));
            plane.RotateAbs(-90, 0, 0);
            plane.TranslateAbs(0, -1, 0);
            cEntireObject.AddObject(plane);

            #region Lights

            AmbientLight.LightColor = new Color(180, 180, 180);

            directionalLight = new DirectionalLight(new Vector3(-0.1f, -1.0f, 0.1f), new Color(100, 120, 130));
            pointLight1 = new PointLight(new Vector3(15, 12, -30), new Color(100, 100, 100));
            pointLight2 = new PointLight(new Vector3(-30, 50, 1), new Color(180, 180, 180));
            pointLight3 = new PointLight(new Vector3(-20, 3.1f, 50), new Color(140, 140, 140));
            spotLight = new SpotLight(new Vector3(-5, 20, 2), new Vector3(.5f, -1, -.2f), new Color(100, 100, 150), 30, 1);

            cEntireObject.AssociateLight(pointLight1);
            cEntireObject.AssociateLight(pointLight2);
            cEntireObject.AssociateLight(pointLight3);
            cEntireObject.AssociateLight(directionalLight);

            #endregion

            #region Post Screen Shaders

            preDepthShader = new PreDepthNormal(true);
            preDepthShader.FarPlane = 100;

            skybox = new PreSkybox("Miramar");
            skybox.AlphaBlending = 0.8f;
            shadowMapShader = new ShadowMapShader(RenderToTexture.SizeType.Custom512x512);
            shadowMapShader.LightDirection = new Vector3(-0.1f, -0.7f, -0.1f);
            shadowMapShader.AssociatedLight = directionalLight;// spotLight;
            shadowMapShader.ShadowColor = new Color(130, 130, 130);
            shadowMapShader.VirtualLightDistance = 50;
            shadowMapShader.FarPlane = 100;
            blurShadow = new Blur();
            blurShadow.BlurWidth = 5;
            blurSSAO = new Blur();
            blurSSAO.BlurWidth = 1.5f;
            ssaoHB = new SSAOHorizonBased(RenderToTexture.SizeType.HalfScreen);
            ssaoHB.NumberSteps = 32;
            ssaoHB.NumberDirections = 15;
            ssaoHB.Radius = 0.015f;
            ssaoHB.LineAttenuation = 1.0f;
            ssaoHB.Contrast = 1.5f;
            ssaoRM = new SSAORayMarching(RenderToTexture.SizeType.HalfScreen);

            convineShadow = new CombineShadows();

            #endregion

        } // LoadOtherResources

        #endregion

        #region Update

        /// <summary>
        /// Update the scene. The car has some movement like direction, and doors aperture.
        /// </summary>
        public override void Update()
        {
            /*gRightDoorGlass.RotateAbs(rightDoorAngle, 0, 0);
            gLeftDoorGlass.RotateAbs(leftDoorAngle, 0, 0);
            gRightDoor.RotateAbs(rightDoorAngle, 0, 0);
            cLeftDoor.RotateAbs(leftDoorAngle, 0, 0);
            cFrontLeftTires.RotateAbs(0, directionAngle, 0);
            cFrontRightTires.RotateAbs(0, directionAngle, 0);*/
            /*
            if (Keyboard.DownPressed)
            {
                glassRearLightMaterial.AlphaBlending = 1;
                glassRearLightMaterial.Shininess = 0;
            }
            else
            {
                glassRearLightMaterial.AlphaBlending = 0.5f;
                glassRearLightMaterial.Shininess = 5;
            }*/
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render. It doesn't clear the screen.
        /// </summary>
        public override void Render()
        {

            if (!otherResourcesLoaded)
            {
                otherResourcesLoaded = true;
                LoadOtherResources();
            }

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
                shadowMapShader.GenerateLightDepthBuffer(cEntireObject);
                shadowMapShader.GenerateShadows(cEntireObject);
                blurShadow.GenerateBlur(shadowMapShader.ShadowMapTexture);
            }
            if (esssao)
            {
                preDepthShader.GenerateDepthNormalMap(cEntireObject);
                ssaoHB.GenerateSSAO(preDepthShader.HighPrecisionDepthMapTexture.XnaTexture, preDepthShader.NormalDepthMapTexture.XnaTexture);
                blurSSAO.GenerateBlur(ssaoHB.SSAOTexture);
            }

            #endregion
            
            EngineManager.ClearTargetAndDepthBuffer(Color.Black);
            
            cEntireObject.Render();
            
            #region Post Work

            if (esshadow)
            {
                convineShadow.GenerateCombineShadows(blurShadow.BlurMapTexture);
                //shadowMapShader.ShadowMapTexture.RenderOnFullScreen();
                //blurShadow.BlurMapTexture.RenderOnFullScreen();
                //shadowMapShader.lightDepthBufferTexture.RenderOnFullScreen();
                SpriteManager.DrawSprites();
            }
            if (esssao)
            {
                convineShadow.GenerateCombineShadows(blurSSAO.BlurMapTexture);
                //ssaoHB.SSAOTexture.RenderOnFullScreen();
                //preDepthShader.NormalDepthMapTexture.RenderOnFullScreen();
                //preDepthShader.HighPrecisionDepthMapTexture.RenderOnFullScreen();
                //blurSSAO.BlurMapTexture.RenderOnFullScreen();
                SpriteManager.DrawSprites();
            }

            #endregion

            matCarPaint.Test();
            UIMousePointer.RenderMousePointer();
            
        } // Render

        #endregion

    } // SceneLamborghiniLP640
} // XNAFinalEngine.Scenes
