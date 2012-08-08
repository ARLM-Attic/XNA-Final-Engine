
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
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Editor;
using DirectionalLight = XNAFinalEngine.Components.DirectionalLight;
using Size = XNAFinalEngine.Helpers.Size;
using Texture = XNAFinalEngine.Assets.Texture;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// This was used to test most of the new version's features.
    /// It’s a mess, really, but it’s the best start point to understand the new version.
    /// </summary>
    public class LamborghiniMurcielagoScene : Scene
    {

        #region Variables
        
        // Now every entity is a game object and the entity’s behavior is defined by the components attached to it.
        // There are several types of components, components related to models, to sound, to particles, to physics, etc.
        private static GameObject3D // Lambo body
                                    murcielagoBody, murcielagoLP640AirTakesEngine, murcielagoLP640AirTakes, murcielagoAirTakesDark,
                                    murcielagoFrontLightBase, murcielagoLights, murcielagoLightsGlasses,
                                    murcielagoWhiteMetal, murcielagoGrayMetal, murcielagoCarbonFiber, murcielagoGlasses,
                                    murcielagoEngineGlasses, murcielagoBlackMetal, murcielagoLogo, murcielagoBlackContant, murcielagoFloor,
                                    murcielagoLP640Grid, murcielagoLP640RearSpoilerDarkPart, murcielagoLP640Exhaust,
                                    murcielagoInteriorLeather, murcielagoSteeringWheel, murcielagoInteriorDetails, murcielagoBlackPlastic,
                                    murcielagoInteriorCostura, murcielagoTablero,
                                    // Left Door
                                    murcielagoLeftDoorBody, murcielagoLeftDoorBlackMetal, murcielagoLeftDoorGrayMetal, murcielagoLeftDoorLeather,
                                    murcielagoLeftDoorSpeakers,
                                    // Right Door
                                    murcielagoRightDoorBody, murcielagoRightDoorBlackMetal, murcielagoRightDoorGrayMetal, murcielagoRightDoorLeather,
                                    murcielagoRightDoorSpeakers,
                                    // Front Left Wheel
                                    murcielagoLP670FrontLeftRim, murcielagoLP640FrontLeftRimBase, murcielagoLP640FrontLeftRimBase02,
                                    murcielagoFrontLeftRimLogo, murcielagoFrontLeftBrakeDisc, murcielagoFrontLeftBrakeCaliper, murcielagoFrontLeftTyre,
                                    murcielagoFrontLeftTyre02, frontLeftRim,
                                    // Front Right Wheel
                                    murcielagoLP670FrontRightRim, murcielagoLP640FrontRightRimBase, murcielagoLP640FrontRightRimBase02,
                                    murcielagoFrontRightRimLogo, murcielagoFrontRightBrakeDisc, murcielagoFrontRightBrakeCaliper, murcielagoFrontRightTyre,
                                    murcielagoFrontRightTyre02, frontRightRim,
                                    // Rear Left Wheel
                                    murcielagoLP670RearLeftRim, murcielagoLP640RearLeftRimBase, murcielagoLP640RearLeftRimBase02,
                                    murcielagoRearLeftRimLogo, murcielagoRearLeftBrakeDisc, murcielagoRearLeftBrakeCaliper, murcielagoRearLeftTyre,
                                    murcielagoRearLeftTyre02, rearLeftRim,
                                    // Rear Right Wheel
                                    murcielagoLP670RearRightRim, murcielagoLP640RearRightRimBase, murcielagoLP640RearRightRimBase02,
                                    murcielagoRearRightRimLogo, murcielagoRearRightBrakeDisc, murcielagoRearRightBrakeCaliper, murcielagoRearRightTyre,
                                    murcielagoRearRightTyre02, rearRightRim,
                                    // Test floors
                                    floor,
                                    // Lights
                                    directionalLight, pointLight, pointLight2, pointLight3, pointLight4, pointLight5, pointLight6, pointLight7, spotLight,
                                    // Cameras
                                    camera, camera2,
                                    skydome;

        private static GameObject2D xnaFinalEngineLogo, videoTest, statistics;
        
        #endregion

        #region Load
        
        /// <summary>
        /// Load the resources.
        /// </summary>
        public override void LoadContent()
        {

            #region Camera
            
            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            //camera.AddComponent<SoundListener>();
            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 500;
            camera.Camera.NearPlane = 0.5f;
            camera.Transform.LookAt(new Vector3(5, 0, 15), Vector3.Zero, Vector3.Up);
            ScriptCustomCamera script = (ScriptCustomCamera)camera.AddComponent<ScriptCustomCamera>();
            // Test script list.
            //camera.RemoveComponent<ScriptCustomCamera>();
            //script = (ScriptCustomCamera)camera.AddComponent<ScriptCustomCamera>();
            script.SetPosition(new Vector3(-8, 0, 9), Vector3.Zero);
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180 / 6f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Both;
            camera.Camera.PostProcess.Bloom.Threshold = 2;
            //camera.Camera.PostProcess.FilmGrain.Enabled = true;
            camera.Camera.AmbientLight = new AmbientLight
            {
                SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, }),
                                                            Color = new Color(10, 10, 10),
                                                            Intensity = 7f,
                                                            AmbientOcclusionStrength = 5f };
            /*camera.Camera.AmbientLight.AmbientOcclusion = new HorizonBasedAmbientOcclusion
            {
                NumberSteps = 8, // Don't change this.
                NumberDirections = 12, // Don't change this.
                Radius = 0.0005f, // Bigger values produce more cache misses and you don’t want GPU cache misses, trust me.
                LineAttenuation = 1f,
                Contrast = 1f,
                AngleBias = 5f,
                Quality = HorizonBasedAmbientOcclusion.QualityType.HighQuality,
                TextureSize = Size.TextureSize.HalfSize,
            };*/

            #endregion
            
            #region Materials

            CarPaint carPaint = new CarPaint
            {
                SpecularIntensity = 10000f,
                SpecularPower = 20,
                BasePaintColor = new Color(250, 250, 250),
                SecondBasePaintColor = new Color(220, 220, 220),
                FlakeLayerColor1 = new Color(170, 170, 170),
                FlakesColor = new Color(100, 100, 100),
                ReflectionTexture = new TextureCube("Showroom"),
                //ReflectionTexture = new TextureCube("FactoryCatwalkRGBM", true, 50),
            };
            
            #endregion
            
            #region Models
            
            #region Body
            
            murcielagoBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Body"), carPaint);
            
            murcielagoTablero = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Tablero"),
                                                             new Constant
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tablero"),
                                                                 //SpecularIntensity = 1f,
                                                                 //SpecularPower = 100,
                                                             });
            murcielagoLights = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Lights"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Lights"),
                                                                 SpecularIntensity = 200f,
                                                                 SpecularPower = 8,
                                                                 ReflectionTexture = new TextureCube("Showroom"),
                                                             });
            murcielagoLightsGlasses = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LightGlasses"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Lights"),
                                                                 AlphaBlending = 0.4f,
                                                                 SpecularIntensity = 5000f,
                                                                 SpecularPower = 1,
                                                                 ReflectionTexture = new TextureCube("Showroom"),
                                                             });
            murcielagoBlackPlastic = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackPlastic"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseColor = new Color(10, 10, 10),
                                                                 SpecularIntensity = 0.1f,
                                                                 SpecularPower = 3,
                                                             });
            murcielagoInteriorCostura = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorCostura"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Costura-Diffuse"),
                                                                 NormalTexture = new Texture("LamborghiniMurcielago\\Costura-Normal"),
                                                                 SpecularTexture = new Texture("LamborghiniMurcielago\\Costura-Specular"),
                                                                 SpecularIntensity = 0.15f,
                                                                 SpecularPower = 20,
                                                             });
            murcielagoInteriorDetails = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorDetails"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Diffuse"),
                                                                 NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Normal"),
                                                                 //SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Specular"),
                                                                 SpecularIntensity = 1f,
                                                                 SpecularPower = 20,
                                                             });
            murcielagoInteriorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorLeather"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Leather-Diffuse"),
                                                                 NormalTexture = new Texture("LamborghiniMurcielago\\Leather-Normal"),
                                                                 SpecularIntensity = 0.15f,
                                                                 SpecularPower = 20,
                                                             });
            murcielagoSteeringWheel = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-SteeringWheel"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture  = new Texture("LamborghiniMurcielago\\SteeringWheel-Diffuse"),
                                                                 NormalTexture   = new Texture("LamborghiniMurcielago\\SteeringWheel-Normal"),
                                                                 //SpecularTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-Specular"),
                                                                 SpecularIntensity = 0.15f,
                                                                 SpecularPower = 20,
                                                             });
            murcielagoLP640AirTakesEngine = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakesEngine"),
                                                             new BlinnPhong
                                                             {
                                                                DiffuseTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Diffuse"),
                                                                SpecularTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Specular"),
                                                                SpecularIntensity = 30f,
                                                                SpecularPower = 50,
                                                             });
            murcielagoLP640AirTakes = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakes"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseColor = new Color(9, 9, 9),
                                                                 SpecularIntensity = 0.2f,
                                                                 SpecularPower = 10,
                                                             });
            murcielagoAirTakesDark = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakesDark"),
                                                             new BlinnPhong // Constant
                                                             {
                                                                 DiffuseColor = new Color(10, 10, 10),
                                                                 SpecularIntensity = 0,
                                                                 AlphaBlending = 0.85f,
                                                             });
            murcielagoFrontLightBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontLightBase"),
                                                        new BlinnPhong
                                                            {
                                                                DiffuseColor = new Color(50, 45, 40),
                                                                SpecularIntensity = 0.0f,
                                                                //SpecularPower = 300,
                                                            });
            murcielagoWhiteMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-WhiteMetal"),
                                                              new BlinnPhong 
                                                                {
                                                                    DiffuseColor = new Color(200, 200, 200),
                                                                    SpecularIntensity = 40,
                                                                    SpecularPower = 5,
                                                                    ReflectionTexture = new TextureCube("Showroom"),
                                                                });
            murcielagoCarbonFiber = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-CarbonFiber"),
                                                              new BlinnPhong
                                                              {
                                                                  DiffuseTexture = new Texture("LamborghiniMurcielago\\CarbonFiber"),
                                                                  SpecularIntensity = 50f,
                                                                  SpecularPower = 7,
                                                                  ReflectionTexture = new TextureCube("Showroom"),
                                                              });
            murcielagoGrayMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-GrayMetal"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseColor = new Color(100, 100, 100),
                                                      SpecularIntensity = 100.0f,
                                                      SpecularPower = 100,
                                                      ReflectionTexture = new TextureCube("Showroom"),
                                                  });
            murcielagoGlasses = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Glasses"),
                                                    new BlinnPhong
                                                    {
                                                        DiffuseColor = new Color(20, 20, 20),
                                                        AlphaBlending = 0.8f,
                                                        SpecularIntensity = 6000,
                                                        SpecularPower = 1,
                                                        ReflectionTexture = new TextureCube("Showroom"),
                                                        //ReflectionTexture = new Graphics.TextureCube("FactoryCatwalkRGBM", true, 50)
                                                    });
            murcielagoEngineGlasses = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-EngineGlasses"),
                                                    new BlinnPhong
                                                    {
                                                        DiffuseColor = new Color(15, 15, 15),
                                                        AlphaBlending = 0.8f,
                                                        SpecularIntensity = 400,
                                                        SpecularPower = 10,
                                                        ReflectionTexture = new TextureCube("Showroom"),
                                                        //ReflectionTexture = new Graphics.TextureCube("FactoryCatwalkRGBM", true, 50)
                                                    });
            murcielagoBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackMetal"),
                                                        new BlinnPhong
                                                        {
                                                            DiffuseColor = new Color(10, 10, 10),
                                                            SpecularIntensity = 100,
                                                            SpecularPower = 10,
                                                            ReflectionTexture = new TextureCube("Showroom"),
                                                        });
            murcielagoLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Logo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Normal"),
                                                     //SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Specular"),
                                                     SpecularIntensity = 300.0f,
                                                     SpecularPower = 15,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoBlackContant = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackConstant"),
                                                 new Constant
                                                 {
                                                     DiffuseColor = Color.Black,
                                                 });

            murcielagoFloor = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Floor"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Floor"),
                                                     SpecularIntensity = 0.05f,
                                                     SpecularPower = 1000,
                                                 });

            murcielagoLP640Grid = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Grid"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Grid"),
                                                     SpecularIntensity = 1.0f,
                                                 });

            murcielagoLP640RearSpoilerDarkPart = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RearSpoilerDarkPart"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularIntensity = 0.05f,
                                                     SpecularPower = 1000,
                                                 });

            murcielagoLP640Exhaust = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"),
                                                     SpecularIntensity = 0.1f,
                                                     SpecularPower = 300,
                                                 });
            
            #endregion
            
            #region Left Door

            murcielagoLeftDoorBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorBody"), carPaint);

            murcielagoLeftDoorBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorBlackMetal"),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(20, 20, 20),
                                                SpecularIntensity = 50,
                                                SpecularPower = 3,
                                                ReflectionTexture = new TextureCube("Showroom"),
                                            });
            murcielagoLeftDoorGrayMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorGrayMetal"),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(100, 100, 100),
                                                SpecularIntensity = 100.0f,
                                                SpecularPower = 100,
                                                ReflectionTexture = new TextureCube("Showroom"),
                                            });
            murcielagoLeftDoorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorLeather"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Leather-Diffuse"),
                                                                 NormalTexture = new Texture("LamborghiniMurcielago\\Leather-Normal"),
                                                                 SpecularIntensity = 0.15f,
                                                                 SpecularPower = 20,
                                                             });
            murcielagoLeftDoorSpeakers = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorSpeakers"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Speaker-Diffuse"),
                                                                 SpecularIntensity = 0f,
                                                                 SpecularPower = 50,
                                                             });

            #endregion

            #region Right Door

            murcielagoRightDoorBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorBody"), carPaint);
                                              
            murcielagoRightDoorBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorBlackMetal"),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(20, 20, 20),
                                                SpecularIntensity = 50,
                                                SpecularPower = 3,
                                                ReflectionTexture = new TextureCube("Showroom"),
                                            });
            murcielagoRightDoorGrayMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorGrayMetal"),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(100, 100, 100),
                                                SpecularIntensity = 100.0f,
                                                SpecularPower = 100,
                                                ReflectionTexture = new TextureCube("Showroom"),
                                            });
            murcielagoRightDoorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorLeather"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Leather-Diffuse"),
                                                                 NormalTexture = new Texture("LamborghiniMurcielago\\Leather-Normal"),
                                                                 SpecularIntensity = 0.15f,
                                                                 SpecularPower = 20,
                                                             });
            murcielagoRightDoorSpeakers = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorSpeakers"),
                                                             new BlinnPhong
                                                             {
                                                                 DiffuseTexture = new Texture("LamborghiniMurcielago\\Speaker-Diffuse"),
                                                                 SpecularIntensity = 0f,
                                                                 SpecularPower = 50,
                                                             });

            #endregion
            
            #region Front Left Wheel

            murcielagoLP670FrontLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-FrontRim"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(25, 25, 25),
                                                     SpecularPower = 15,
                                                     //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                                                     SpecularIntensity = 400f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640FrontLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640FrontLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(80, 80, 80),
                                                     SpecularPower = 5,
                                                     SpecularIntensity = 50f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoFrontLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                                                     SpecularIntensity = 20f,
                                                     SpecularPower = 100,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoFrontLeftBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Normal"),
                                                     SpecularTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Specular"),
                                                     SpecularIntensity = 10.0f,
                                                     SpecularPower = 100,
                                                     //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                                 });

            murcielagoFrontLeftBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\LamborghiniBrakeCaliper"),
                                                     SpecularIntensity = 1000.0f,
                                                     SpecularPower = 5f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });
            murcielagoFrontLeftBrakeCaliper.Transform.Rotate(new Vector3(183, 0, 0), Space.Local);

            murcielagoFrontLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre"),
                                     new BlinnPhong
                                     {
                                         DiffuseColor = new Color(10, 10, 10),
                                         //DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                                         SpecularIntensity = 0.05f,
                                         SpecularPower = 10,
                                         //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                     });

            murcielagoFrontLeftTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre02"),
                                     new BlinnPhong
                                     {
                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Normal"),
                                         SpecularIntensity = 0.01f,
                                         SpecularPower = 2,
                                     });

            frontLeftRim = new GameObject3D();
            frontLeftRim.Transform.LocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            frontLeftRim.Transform.Rotate(new Vector3(0, 40, 0), Space.World);
            frontLeftRim.Transform.Position = new Vector3(1.6f, -0.5f, 2.35f);

            murcielagoLP670FrontLeftRim.Parent = frontLeftRim;
            murcielagoLP640FrontLeftRimBase.Parent = frontLeftRim;
            murcielagoLP640FrontLeftRimBase02.Parent = frontLeftRim;
            murcielagoFrontLeftRimLogo.Parent = frontLeftRim;
            murcielagoFrontLeftBrakeDisc.Parent = frontLeftRim;
            murcielagoFrontLeftBrakeCaliper.Parent = frontLeftRim;
            murcielagoFrontLeftTyre.Parent = frontLeftRim;
            murcielagoFrontLeftTyre02.Parent = frontLeftRim;

            #endregion

            #region Front Right Wheel

            murcielagoLP670FrontRightRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-FrontRim"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(25, 25, 25),
                                                     SpecularPower = 15,
                                                     //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                                                     SpecularIntensity = 400f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640FrontRightRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640FrontRightRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(80, 80, 80),
                                                     SpecularPower = 5,
                                                     SpecularIntensity = 50f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoFrontRightRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                                                     SpecularIntensity = 20f,
                                                     SpecularPower = 100,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoFrontRightBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Normal"),
                                                     SpecularTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Specular"),
                                                     SpecularIntensity = 10.0f,
                                                     SpecularPower = 100,
                                                     //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                                 });

            murcielagoFrontRightBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\LamborghiniBrakeCaliper"),
                                                     SpecularIntensity = 1000.0f,
                                                     SpecularPower = 5f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });
            murcielagoFrontRightBrakeCaliper.Transform.Rotate(new Vector3(7, 0, 0), Space.Local);

            murcielagoFrontRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre"),
                                     new BlinnPhong
                                     {
                                         DiffuseColor = new Color(10, 10, 10),
                                         //DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                                         SpecularIntensity = 0.05f,
                                         SpecularPower = 10,
                                         //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                     });

            murcielagoFrontRightTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre02"),
                                     new BlinnPhong
                                     {
                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Normal"),
                                         SpecularIntensity = 0.01f,
                                         SpecularPower = 2,
                                     });

            frontRightRim = new GameObject3D
                                {
                                    Transform =
                                        {
                                            LocalScale = new Vector3(1.1f, 1.1f, 1.1f),
                                            Position = new Vector3(-1.6f, -0.5f, 2.35f)
                                        }
                                };
            frontRightRim.Transform.Rotate(new Vector3(0, 40 + 180, 0), Space.World);

            murcielagoLP670FrontRightRim.Parent = frontRightRim;
            murcielagoLP640FrontRightRimBase.Parent = frontRightRim;
            murcielagoLP640FrontRightRimBase02.Parent = frontRightRim;
            murcielagoFrontRightRimLogo.Parent = frontRightRim;
            murcielagoFrontRightBrakeDisc.Parent = frontRightRim;
            murcielagoFrontRightBrakeCaliper.Parent = frontRightRim;
            murcielagoFrontRightTyre.Parent = frontRightRim;
            murcielagoFrontRightTyre02.Parent = frontRightRim;

            #endregion

            #region Rear Left Wheel

            murcielagoLP670RearLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-RearRim"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(25, 25, 25),
                                                     SpecularPower = 15,
                                                     //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                                                     SpecularIntensity = 400f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640RearLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640RearLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(80, 80, 80),
                                                     SpecularPower = 5,
                                                     SpecularIntensity = 50f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoRearLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                                                     SpecularIntensity = 20f,
                                                     SpecularPower = 100,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoRearLeftBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Normal"),
                                                     SpecularTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Specular"),
                                                     SpecularIntensity = 10.0f,
                                                     SpecularPower = 100,
                                                     //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                                 });

            murcielagoRearLeftBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\LamborghiniBrakeCaliper"),
                                                     SpecularIntensity = 1000.0f,
                                                     SpecularPower = 5f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });
            murcielagoRearLeftBrakeCaliper.Transform.Rotate(new Vector3(4, 0, 0), Space.Local);

            murcielagoRearLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre"),
                                     new BlinnPhong
                                     {
                                         DiffuseColor = new Color(10, 10, 10),
                                         //DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                                         SpecularIntensity = 0.05f,
                                         SpecularPower = 10,
                                     });

            murcielagoRearLeftTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre02"),
                                     new BlinnPhong
                                     {
                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Normal"),
                                         SpecularIntensity = 0.01f,
                                         SpecularPower = 2,
                                     });

            rearLeftRim = new GameObject3D();
            rearLeftRim.Transform.LocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            rearLeftRim.Transform.Rotate(new Vector3(0, 0, 0), Space.World);
            rearLeftRim.Transform.Position = new Vector3(1.6f, -0.5f, -2.9f);

            murcielagoLP670RearLeftRim.Parent = rearLeftRim;
            murcielagoLP640RearLeftRimBase.Parent = rearLeftRim;
            murcielagoLP640RearLeftRimBase02.Parent = rearLeftRim;
            murcielagoRearLeftRimLogo.Parent = rearLeftRim;
            murcielagoRearLeftBrakeDisc.Parent = rearLeftRim;
            murcielagoRearLeftBrakeCaliper.Parent = rearLeftRim;
            murcielagoRearLeftTyre.Parent = rearLeftRim;
            murcielagoRearLeftTyre02.Parent = rearLeftRim;

            #endregion

            #region Rear Right Wheel

            murcielagoLP670RearRightRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-RearRim"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(25, 25, 25),
                                                     SpecularPower = 15,
                                                     //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                                                     SpecularIntensity = 400f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640RearRightRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoLP640RearRightRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(80, 80, 80),
                                                     SpecularPower = 5,
                                                     SpecularIntensity = 50f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoRearRightRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                                                     SpecularIntensity = 20f,
                                                     SpecularPower = 100,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });

            murcielagoRearRightBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Normal"),
                                                     SpecularTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Specular"),
                                                     SpecularIntensity = 10.0f,
                                                     SpecularPower = 100,
                                                     //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                                 });

            murcielagoRearRightBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\LamborghiniBrakeCaliper"),
                                                     SpecularIntensity = 1000.0f,
                                                     SpecularPower = 5f,
                                                     ReflectionTexture = new TextureCube("Showroom"),
                                                 });
            murcielagoRearRightBrakeCaliper.Transform.Rotate(new Vector3(182, 0, 0), Space.Local);

            murcielagoRearRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre"),
                                     new BlinnPhong
                                     {
                                         DiffuseColor = new Color(10, 10, 10),
                                         //DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                                         SpecularIntensity = 0.05f,
                                         SpecularPower = 10,
                                     });

            murcielagoRearRightTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre02"),
                                     new BlinnPhong
                                     {
                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Normal"),
                                         SpecularIntensity = 0.01f,
                                         SpecularPower = 2,
                                     });

            rearRightRim = new GameObject3D();
            rearRightRim.Transform.LocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            rearRightRim.Transform.Rotate(new Vector3(0, 180, 0), Space.World);
            rearRightRim.Transform.Position = new Vector3(-1.6f, -0.5f, -2.9f);

            murcielagoLP670RearRightRim.Parent = rearRightRim;
            murcielagoLP640RearRightRimBase.Parent = rearRightRim;
            murcielagoLP640RearRightRimBase02.Parent = rearRightRim;
            murcielagoRearRightRimLogo.Parent = rearRightRim;
            murcielagoRearRightBrakeDisc.Parent = rearRightRim;
            murcielagoRearRightBrakeCaliper.Parent = rearRightRim;
            murcielagoRearRightTyre.Parent = rearRightRim;
            murcielagoRearRightTyre02.Parent = rearRightRim;

            #endregion
            
            #region Floor

            floor = new GameObject3D(new FileModel("Terrain/TerrainLOD0Grid"),
                           new BlinnPhong
                           {
                               SpecularPower = 300,
                               DiffuseColor = new Color(200, 200, 200),
                               SpecularIntensity = 0.0f,
                           }) 
            { Transform = { LocalScale = new Vector3(5, 5, 5) } };
            floor.Transform.Position = new Vector3(0, -1.17f, 0);

            #endregion
                   
            #endregion

            #region Shadows and Lights
            
            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.Color = new Color(250, 250, 220);
            directionalLight.DirectionalLight.Intensity = 10f;
            directionalLight.Transform.LookAt(new Vector3(0.5f, 0.65f, 1.3f), Vector3.Zero, Vector3.Forward);
            /*directionalLight.DirectionalLight.Shadow = new CascadedShadow
            {
                Filter = Shadow.FilterType.Pcf5X5,
                LightDepthTextureSize = Size.Square512X512,
                TextureSize = Size.TextureSize.FullSize,
                Range = 50,
            };*/
            
            pointLight = new GameObject3D();
            pointLight.AddComponent<PointLight>();
            pointLight.PointLight.Color = new Color(250, 100, 180);
            pointLight.PointLight.Intensity = 1.5f;
            pointLight.PointLight.Range = 100;
            pointLight.Transform.Position = new Vector3(15, 5, -25);
            
            pointLight2 = new GameObject3D();
            pointLight2.AddComponent<PointLight>();
            pointLight2.PointLight.Color = new Color(170, 150, 255);
            pointLight2.PointLight.Intensity = 1.5f;
            pointLight2.PointLight.Range = 100;
            pointLight2.Transform.Position = new Vector3(-25, 25, -15);
            
            pointLight3 = new GameObject3D();
            pointLight3.AddComponent<PointLight>();
            pointLight3.PointLight.Color = new Color(170, 250, 55);
            pointLight3.PointLight.Intensity = 1.5f;
            pointLight3.PointLight.Range = 100;
            pointLight3.Transform.Position = new Vector3(-15f, 15, -20);
            
            pointLight4 = new GameObject3D();
            pointLight4.AddComponent<PointLight>();
            pointLight4.PointLight.Color = new Color(150, 150, 150);
            pointLight4.PointLight.Intensity = 1.5f;
            pointLight4.PointLight.Range = 100; // I always forget to set the light range lower than the camera far plane.
            pointLight4.Transform.Position = new Vector3(30, -5, 20);
            
            pointLight5 = new GameObject3D();
            pointLight5.AddComponent<PointLight>();
            pointLight5.PointLight.Color = new Color(220, 250, 155);
            pointLight5.PointLight.Intensity = 1.5f;
            pointLight5.PointLight.Range = 200;
            pointLight5.Transform.Position = new Vector3(20f, 20.5f, -50);
            
            pointLight6 = new GameObject3D();
            pointLight6.AddComponent<PointLight>();
            pointLight6.PointLight.Color = new Color(240, 170, 110);
            pointLight6.PointLight.Intensity = 1.5f;
            pointLight6.PointLight.Range = 150; // I always forget to set the light range lower than the camera far plane.
            pointLight6.Transform.Position = new Vector3(0, -30f, -10);
            
            #endregion

            #region Statistics

            statistics = new GameObject2D();
            statistics.AddComponent<ScriptStatisticsDrawer>();

            #endregion
            
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        public override void UpdateTasks()
        {
            base.UpdateTasks();
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

    } // LamborghiniMurcielagoScene
} // XNAFinalEngineExamples
