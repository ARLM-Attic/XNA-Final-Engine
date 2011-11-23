
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Scenes;
using XNAFinalEngine.Audio;
using Keyboard = XNAFinalEngine.Input.Keyboard;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// Test scene
    /// </summary>
    public class LamborghiniMurcielagoScene : Scene
    {

        #region Variables

        private static GameObject3D murcielagoBody, murcielagoLP640AirTakesEngine, murcielagoFrontLightBase, murcielagoFrontLightWhiteMetal, murcielagoGlasses,
                                    murcielagoEngineGlasses, murcielagoBlackMetal, murcielagoLogo, murcielagoWheelBlackContant, murcielagoloor,
                                    murcielagoLP640Grid, murcielagoLP640RearSpoilerDarkPart, murcielagoLP640Exhaust,
                                    // Front Left Wheel
                                    murcielagoLP670FrontLeftRim, murcielagoLP640FrontLeftRimBase, murcielagoLP640FrontLeftRimBase02,
                                    murcielagoFrontLeftRimLogo, murcielagoFrontLeftBrakeDisc, murcielagoFrontLeftBrakeCaliper, murcielagoFrontLeftTyre,
                                    murcielagoFrontLeftTyre02, frontLeftRim,
                                    // Rear Left Wheel
                                    murcielagoLP670RearLeftRim, murcielagoLP640RearLeftRimBase, murcielagoLP640RearLeftRimBase02,
                                    murcielagoRearLeftRimLogo, murcielagoRearLeftBrakeDisc, murcielagoRearLeftBrakeCaliper, murcielagoRearLeftTyre,
                                    murcielagoRearLeftTyre02, rearLeftRim,
                                    floor, floor2, floor3,
                                    directionalLight, pointLight, pointLight2, pointLight3,
                                    camera, camera2,
                                    particleSystem;

        #endregion

        #region Load
        
        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method at the end.</remarks>
        public override void Load()
        {

            #region Camera

            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.Camera.Renderer = Camera.RenderingType.DeferredLighting;
            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 1000;
            ScriptEditorCamera script = (ScriptEditorCamera)camera.AddComponent<ScriptEditorCamera>();
            script.SetPosition(new Vector3(5, 0, 15), Vector3.Zero);
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180 / 8.0f;
            camera.Camera.PostProcess = new PostProcess
            {
                FilmGrain = new FilmGrain { FilmgrainStrength = 0.2f }, // Don't overuse it. PLEASE!!!
                Bloom = new Bloom(),
                AdjustLevels = new AdjustLevels(),
                MLAA = new MLAA { EdgeDetection = MLAA.EdgeDetectionType.Both, BlurRadius = 1f, ThresholdDepth = 0.2f, ThresholdColor = 0.2f },
                LensExposure = 1.0f,
            };
            camera.Camera.AmbientLight = new AmbientLight { //SphericalHarmonicAmbientLight = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("Showroom", false)),
                                                            //SphericalHarmonicAmbientLight = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("FactoryCatwalkRGBM", true, 50)),
                                                            SphericalHarmonicAmbientLight = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("Colors", false)),
                                                            Color = new Color(30, 30, 30),
                                                            Intensity = 1.0f,
                                                            AmbientOcclusionStrength = 5};
            camera.Camera.AmbientLight.AmbientOcclusion = new HorizonBasedAmbientOcclusion
            {
                NumberSteps = 8, // Don't change this.
                NumberDirections = 12, // Don't change this.
                Radius = 0.00015f, // Bigger values produce more cache misses and you don’t want GPU cache misses, trust me.
                LineAttenuation = 0.5f,
                Contrast = 0.9f,
                AngleBias = 0.25f,
                Quality = HorizonBasedAmbientOcclusion.QualityType.HighQuality,
                TextureSize = Size.TextureSize.QuarterSize,
            };
            /*camera.Camera.AmbientLight.AmbientOcclusion = new RayMarchingAmbientOcclusion
            {
                NumberSteps = 12,
                NumberDirections = 6,
                NumberRays = 4,
                Radius = 0.005f, // Bigger values produce more cache misses and you don’t want GPU cache misses, trust me.
                LineAttenuation = 1.5f,
                Contrast = 1.5f,
            };*/
            /*
            camera.Camera.NormalizedViewport = new RectangleF(0, 0, 1, 0.5f);
            camera2 = new GameObject3D();
            camera2.AddComponent<Camera>();
            camera2.Camera.MasterCamera = camera.Camera;
            camera2.Camera.ClearColor = Color.Black;
            camera2.Camera.FieldOfView = 180 / 8.0f;
            camera2.Camera.NormalizedViewport = new RectangleF(0, 0.5f, 1, 0.5f);
            camera2.Transform.LookAt(new Vector3(0, 0, 20), new Vector3(0, -2, 0), Vector3.Up);*/
            
            #endregion

            #region Models
            
            #region Body

            murcielagoBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Body"),
                                              new CarPaint
                                                    {
                                                        SpecularIntensity = 20.5f,
                                                        SpecularPower = 2,
                                                        BasePaintColor = new Color(240, 210, 50),
                                                        SecondBasePaintColor = new Color(100, 100, 50),
                                                        MiddlePaintColor = new Color(150, 100, 100),
                                                        FlakeLayerColor = new Color(100, 100, 100),
                                                        ReflectionTexture = new TextureCube("Showroom", false),
                                                        //ReflectionTexture = new Graphics.TextureCube("FactoryCatwalkRGBM", true, 50)
                                                    });
            murcielagoLP640AirTakesEngine = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakesEngine"),
                                                             new BlinnPhong
                                                             {
                                                                DiffuseTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Diffuse"),
                                                                SpecularTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Specular"),
                                                                SpecularIntensity = 0.1f,
                                                                SpecularPower = 300,
                                                             });
            murcielagoFrontLightBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontLightBase"),
                                                        new BlinnPhong { DiffuseColor = new Color(30, 30, 30), SpecularIntensity = 0.1f });
            murcielagoFrontLightWhiteMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontLightWhiteMetal"),
                                                              new BlinnPhong { DiffuseColor = new Color(230, 230, 230) });
            murcielagoGlasses = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Glasses"),
                                                    new BlinnPhong
                                                    {
                                                        DiffuseColor = new Color(20, 20, 20),
                                                        AlphaBlending = 0.95f,
                                                        SpecularIntensity = 200,
                                                        SpecularPower = 5,
                                                        ReflectionTexture = new TextureCube("Showroom", false),
                                                        //ReflectionTexture = new Graphics.TextureCube("FactoryCatwalkRGBM", true, 50)
                                                    });
            murcielagoEngineGlasses = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-EngineGlasses"),
                                                    new BlinnPhong
                                                    {
                                                        DiffuseColor = new Color(20, 20, 20),
                                                        AlphaBlending = 0.6f,
                                                        SpecularIntensity = 50,
                                                        SpecularPower = 1,
                                                        ReflectionTexture = new TextureCube("Showroom", false),
                                                        //ReflectionTexture = new Graphics.TextureCube("FactoryCatwalkRGBM", true, 50)
                                                    });
            murcielagoBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackMetal"),
                                                        new BlinnPhong
                                                        {
                                                            DiffuseColor = new Color(20, 20, 20),
                                                            SpecularIntensity = 50,
                                                            SpecularPower = 3,
                                                            ReflectionTexture = new TextureCube("Showroom", false),
                                                        });
            murcielagoLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Logo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Normal"),
                                                     SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Specular"),
                                                     SpecularIntensity = 0.1f,
                                                 });

            murcielagoWheelBlackContant = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-WheelBlackConstant"),
                                                 new Constant
                                                 {
                                                     DiffuseColor = Color.Black,
                                                 });

            murcielagoloor = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Floor"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Floor"),
                                                     SpecularIntensity = 0.3f,
                                                 });

            murcielagoLP640Grid = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Grid"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Grid"),
                                                     SpecularIntensity = 0.3f,
                                                 });

            murcielagoLP640RearSpoilerDarkPart = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RearSpoilerDarkPart"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(40, 40, 40),
                                                     SpecularIntensity = 0.1f,
                                                 });

            murcielagoLP640Exhaust = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"),
                                                     SpecularIntensity = 1f,
                                                 });

            #endregion

            #region Front Left Wheel

            murcielagoLP670FrontLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-FrontRim"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

            murcielagoLP640FrontLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

            murcielagoLP640FrontLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(80, 80, 80),
                                                     SpecularPower = 5,
                                                     SpecularIntensity = 50f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

            murcielagoFrontLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                                                     SpecularIntensity = 20f,
                                                     SpecularPower = 100,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
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
                                                     SpecularIntensity = 300.0f,
                                                     SpecularPower = 5f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

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
                                         DiffuseColor = new Color(20, 20, 20),
                                         //DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                                         //NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                                         SpecularIntensity = 0.01f,
                                         SpecularPower = 2,
                                         //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                     });

            frontLeftRim = new GameObject3D();
            frontLeftRim.Transform.LocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            frontLeftRim.Transform.Rotate(new Vector3(0, -30, 0), Space.World);
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

            #region Rear Left Wheel

            murcielagoLP670RearLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-RearRim"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

            murcielagoLP640RearLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularPower = 1,
                                                     SpecularIntensity = 10f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

            murcielagoLP640RearLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(80, 80, 80),
                                                     SpecularPower = 5,
                                                     SpecularIntensity = 50f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

            murcielagoRearLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                                                     SpecularIntensity = 20f,
                                                     SpecularPower = 100,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
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
                                                     SpecularIntensity = 300.0f,
                                                     SpecularPower = 5f,
                                                     ReflectionTexture = new TextureCube("Showroom", false),
                                                 });

            murcielagoRearLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre"),
                                     new BlinnPhong
                                     {
                                         DiffuseColor = new Color(10, 10, 10),
                                         //DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                                         NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                                         SpecularIntensity = 0.05f,
                                         SpecularPower = 10,
                                         //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
                                     });

            murcielagoRearLeftTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre02"),
                                     new BlinnPhong
                                     {
                                         DiffuseColor = new Color(20, 20, 20),
                                         //DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                                         //NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                                         SpecularIntensity = 0.01f,
                                         SpecularPower = 2,
                                         //ReflectionTexture = new Graphics.TextureCube("Showroom", false),
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

            #region Floor

            floor = new GameObject3D(new FileModel("Terrain/TerrainLOD0Grid"),
                           new BlinnPhong
                           {
                               SpecularPower = 300,
                               DiffuseTexture = new Texture("Stones-Diffuse"),
                               //DiffuseColor = new Color(27, 27, 25),
                               NormalTexture = new Texture("Stones-NormalHeightMap"),
                               SpecularIntensity = 1.0f,
                               ParallaxEnabled = true,
                               ReflectionTexture = new TextureCube("Showroom", false),
                           });
            floor.Transform.LocalScale = new Vector3(2, 2, 2);
            floor.Transform.Position = new Vector3(0, -1.17f, 0);
            floor2 = new GameObject3D(new FileModel("Terrain/TerrainLOD0Grid"),
               new BlinnPhong
               {
                   SpecularPower = 30,
                   DiffuseTexture = new Texture("Stones-Diffuse"),
                   //DiffuseColor = new Color(27, 27, 25),
                   //NormalTexture = new Texture("Stones-NormalHeightMap"),
                   SpecularIntensity = 10.0f,
                   //ParallaxEnabled = true,
                   ReflectionTexture = new TextureCube("Showroom", false),
               });
            floor2.Transform.LocalScale = new Vector3(2, 2, 2);
            floor2.Transform.Position = new Vector3(0, -1.17f, 0);
            floor2.Transform.Rotate(new Vector3(180, 0, 0));
            floor3 = new GameObject3D(new FileModel("Terrain/TerrainLOD0Grid"),
               new BlinnPhong
               {
                   SpecularPower = 30,
                   DiffuseTexture = new Texture("Stones-Diffuse"),
                   //DiffuseColor = new Color(27, 27, 25),
                   NormalTexture = new Texture("Stones-NormalHeightMap"),
                   SpecularIntensity = 10.0f,
                   //ParallaxEnabled = true,
                   ReflectionTexture = new TextureCube("Showroom", false),
               });
            floor3.Transform.LocalScale = new Vector3(2, 2, 2);
            floor3.Transform.Position = new Vector3(0, 10, 0);
            floor3.Transform.Rotate(new Vector3(180, 0, 0));

            #endregion
            
            #endregion

            #region Shadows and Lights
            
            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.DiffuseColor = new Color(210, 200, 200);
            directionalLight.DirectionalLight.Intensity = 1.7f;
            directionalLight.Transform.LookAt(new Vector3(0.3f, 0.5f, 0.5f), Vector3.Zero, Vector3.Forward);
            directionalLight.DirectionalLight.Shadow = new CascadedShadow
            {
                Filter = Shadow.FilterType.PCF3x3,
                LightDepthTextureSize = Size.Square512X512,
                TextureSize = Size.TextureSize.HalfSize
            };

            pointLight = new GameObject3D();
            pointLight.AddComponent<PointLight>();
            pointLight.PointLight.DiffuseColor = new Color(250, 200, 180);
            pointLight.PointLight.Intensity = 0.4f;
            pointLight.PointLight.Range = 100;
            pointLight.PointLight.SpecularColor = Color.White;
            pointLight.Transform.Position = new Vector3(-25, 5, 10);

            pointLight2 = new GameObject3D();
            pointLight2.AddComponent<PointLight>();
            pointLight2.PointLight.DiffuseColor = new Color(70, 150, 255);
            pointLight2.PointLight.Intensity = 0.4f;
            pointLight2.PointLight.Range = 100;
            pointLight2.PointLight.SpecularColor = Color.White;
            pointLight2.Transform.Position = new Vector3(45, 2, -2);

            pointLight3 = new GameObject3D();
            pointLight3.AddComponent<PointLight>();
            pointLight3.PointLight.DiffuseColor = new Color(70, 250, 55);
            pointLight3.PointLight.Intensity = 0.5f;
            pointLight3.PointLight.Range = 100;
            pointLight3.PointLight.SpecularColor = Color.White;
            pointLight3.Transform.Position = new Vector3(0.1f, 5, -50);
            
            #endregion

            #region Particles

            particleSystem = new GameObject3D();
            particleSystem.AddComponent<ParticleEmitter>();
            particleSystem.AddComponent<ParticleRenderer>();
            particleSystem.ParticleRenderer.Texture = new Texture("Particles\\Fire");
            particleSystem.ParticleRenderer.SoftParticles = true;
            particleSystem.ParticleRenderer.FadeDistance = 5f;

            #endregion

            GameLoop.ShowFramesPerSecond = true;

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
            /*if (Keyboard.KeyJustPressed(Keys.Left))
                camera.Camera.AmbientLight.AmbientOcclusion.Enabled = !camera.Camera.AmbientLight.AmbientOcclusion.Enabled;
            if (Keyboard.RightJustPressed)
                camera.Camera.PostProcess.MLAA.Enabled = !camera.Camera.PostProcess.MLAA.Enabled;
            if (Keyboard.UpJustPressed)
                directionalLight.DirectionalLight.Shadow.Enabled = !directionalLight.DirectionalLight.Shadow.Enabled;*/
            if (Keyboard.KeyJustPressed(Keys.Left))
                MusicManager.Previous();
            if (Keyboard.RightJustPressed)
                MusicManager.Next();

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

        #endregion

    } // LamborghiniMurcielagoScene
} // XNAFinalEngineExamples
