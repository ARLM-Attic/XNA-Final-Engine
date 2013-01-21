
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
using Texture = XNAFinalEngine.Assets.Texture;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// This scene load the murcielago models, textures and materials.
    /// </summary>
    public class LamborghiniMurcielagoLoader
    {

        #region Variables
        
        // Now every entity is a game object and the entity’s behavior is defined by the components attached to it.
        // There are several types of components, components related to models, to sound, to particles, to physics, etc.
        private static GameObject3D // Lambo body
                                    murcielagoBody, murcielagoLP640AirTakesEngine, murcielagoLP640AirTakes, murcielagoAirTakesDark,
                                    murcielagoFrontLightBase, murcielagoLights, murcielagoLightsGlasses,
                                    murcielagoWhiteMetal, murcielagoGrayMetal, murcielagoCarbonFiber, murcielagoGlasses,
                                    murcielagoEngineGlasses, murcielagoBlackMetal, murcielagoLogo, murcielagoBlackContant, murcielagoFloor,
                                    murcielagoLP640Grid, murcielagoLP640RearSpoilerDarkPart, murcielagoLP640Exhaust, murcielagoLP640LeatherPattern,
                                    murcielagoInteriorLeather, murcielagoSteeringWheel, murcielagoInteriorDetails, murcielagoBlackPlastic,
                                    murcielagoInteriorCostura, murcielagoTablero, murcielagoRedPlastic,
                                    // Left Door
                                    murcielagoLeftDoorBody, murcielagoLeftDoorBlackMetal, murcielagoLeftDoorGrayMetal, murcielagoLeftDoorLeather,
                                    murcielagoLeftDoorSpeakers, murcielagoLeftDoorGlass, murcielagoLeftDoorCostura, murcielagoLeftDoorDetails, murcielagoLeftDoor,
                                    // Right Door
                                    murcielagoRightDoorBody, murcielagoRightDoorBlackMetal, murcielagoRightDoorGrayMetal, murcielagoRightDoorLeather,
                                    murcielagoRightDoorSpeakers, murcielagoRightDoorGlass, murcielagoRightDoorCostura, murcielagoRightDoorDetails,
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
                                    murcielagoRearRightTyre02, rearRightRim;
        
        #endregion

        #region Properties

        public GameObject3D LamborghiniMurcielago { get; private set; }

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        public void LoadContent()
        {

            LamborghiniMurcielago = new GameObject3D();
            
            #region Materials

            TextureCube reflectionTexture =
                 new TextureCube("Showroom");
                //new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50 };
            const float reflectioTextureMultiplier = 25;

            CarPaint carPaintMaterial = new CarPaint
            {
                SpecularIntensity = 2f * reflectioTextureMultiplier,
                SpecularPower = 5,
                BasePaintColor = new Color(0.79f, 0.45f, 0),
                SecondBasePaintColor = new Color(1f, 0.3f, 0),
                ThirdBasePaintColor = new Color(0.35f, 0.6f, 0.5f),
                FlakesColor = new Color(255, 100, 100),
                ReflectionTexture = reflectionTexture,
            };

            BlinnPhong rimMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(35, 35, 35),
                SpecularPower = 75,
                //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                SpecularIntensity = 15f * reflectioTextureMultiplier,
                ReflectionTexture = reflectionTexture,
            };

            BlinnPhong airTakesMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(30, 30, 30),
                SpecularIntensity = 0.3f,
                SpecularPower = 10,
            };

            BlinnPhong airTakesEngineMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Diffuse"),
                SpecularTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Specular"),
                SpecularIntensity = 10f,
                SpecularPower = 200,
            };
            BlinnPhong engineGlassesMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(15, 15, 18),
                AlphaBlending = 0.85f,
                SpecularIntensity = 0.2f * reflectioTextureMultiplier,
                SpecularPower = 1,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong lightsMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Lights"),
                SpecularIntensity = 6f * reflectioTextureMultiplier,
                SpecularPower = 30,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong lightsGlassesMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(20, 20, 21),
                //DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Lights"),
                AlphaBlending = 0.6f,
                SpecularIntensity = 0.4f * reflectioTextureMultiplier,
                SpecularPower = 0.5f,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong glassesMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(20, 20, 20),
                AlphaBlending = 0.6f,
                SpecularIntensity = 0.3f * reflectioTextureMultiplier,
                SpecularPower = 1,
                ReflectionTexture = reflectionTexture,
            };

            BlinnPhong brakeCaliperMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\LamborghiniBrakeCaliper"),
                SpecularIntensity = 10 * reflectioTextureMultiplier,
                SpecularPower = 25f,
                ReflectionTexture = reflectionTexture,
            };

            BlinnPhong rimBaseMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(30, 30, 30),
                SpecularPower = 1,
                SpecularIntensity = 0.1f * reflectioTextureMultiplier,
                ReflectionTexture = reflectionTexture,
            };

            BlinnPhong rimBaseMaterial2 = new BlinnPhong
            {
                DiffuseColor = new Color(120, 120, 120),
                SpecularPower = 150,
                SpecularIntensity = 70 * reflectioTextureMultiplier,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong brakeDiscMaterial = new BlinnPhong
            {
                DiffuseTexture  = new Texture("LamborghiniMurcielago\\BrakeDisc-Diffuse"),
                NormalTexture   = new Texture("LamborghiniMurcielago\\BrakeDisc-Normal"),
                SpecularTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Specular"),
                SpecularIntensity = 15.0f,
                SpecularPower = 30,
            };
            BlinnPhong rimLogoMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                SpecularIntensity = 0.75f * reflectioTextureMultiplier,
                SpecularPower = 500,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong tyreMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(10, 10, 10),
                DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                SpecularIntensity = 0.2f,
                SpecularPower = 50,
            };
            BlinnPhong tyreMaterial2 = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Diffuse"),
                NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Normal"),
                SpecularIntensity = 0.05f,
                SpecularPower = 50,
            };
            BlinnPhong blackMetalMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(10, 10, 10),
                SpecularIntensity = 2 * reflectioTextureMultiplier,
                SpecularPower = 50,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong grayMetalMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(70, 70, 70),
                SpecularIntensity = 20f,
                SpecularPower = 200,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong leftDoorSpeakersMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\Speaker-Diffuse"),
                SpecularIntensity = 0f,
                SpecularPower = 50,
            };
            BlinnPhong leatherMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\Leather-Diffuse"),
                NormalTexture = new Texture("LamborghiniMurcielago\\Leather-Normal"),
                SpecularIntensity = 0.15f,
                SpecularPower = 20,
            };
            BlinnPhong costuraMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\Costura-Diffuse"),
                NormalTexture = new Texture("LamborghiniMurcielago\\Costura-Normal"),
                SpecularTexture = new Texture("LamborghiniMurcielago\\Costura-Specular"),
                SpecularIntensity = 0.15f,
                SpecularPower = 20,
            };
            BlinnPhong detailsMaterial = new BlinnPhong
            {
                DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Diffuse"),
                NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Normal"),
                SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Specular"),
                SpecularIntensity = 2f,
                SpecularPower = 300,
            };
            
            #endregion
            
            #region Body

            murcielagoBody                = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Body"),                carPaintMaterial)       { Parent = LamborghiniMurcielago };
            murcielagoLP640AirTakes       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakes"),            airTakesMaterial)       { Parent = LamborghiniMurcielago };
            murcielagoLights              = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Lights"),              lightsMaterial)         { Parent = LamborghiniMurcielago };
            murcielagoLightsGlasses       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LightGlasses"),        lightsGlassesMaterial)  { Parent = LamborghiniMurcielago };
            murcielagoGrayMetal           = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-GrayMetal"),           grayMetalMaterial)      { Parent = LamborghiniMurcielago };
            murcielagoGlasses             = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Glasses"),             glassesMaterial)        { Parent = LamborghiniMurcielago };
            murcielagoEngineGlasses       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-EngineGlasses"), engineGlassesMaterial)  { Parent = LamborghiniMurcielago };
            murcielagoLP640AirTakesEngine = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakesEngine"),      airTakesEngineMaterial) { Parent = LamborghiniMurcielago };
            murcielagoBlackPlastic        = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackPlastic"),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(10, 10, 10),
                                                SpecularIntensity = 0.1f,
                                                SpecularPower = 3,
                                            }) { Parent = LamborghiniMurcielago };
            murcielagoRedPlastic = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RedPlastic"),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(250, 50, 50),
                                                SpecularIntensity = 0.02f,
                                                SpecularPower = 500,
                                            }) { Parent = LamborghiniMurcielago };
            murcielagoLP640LeatherPattern = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-LeatherPattern"),
                                            new BlinnPhong
                                            {
                                                DiffuseTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-Diffuse"),
                                                NormalTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-Normal"),
                                                //SpecularTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-Specular"),
                                                SpecularPowerFromTexture = false,
                                                SpecularIntensity = 0.1f,
                                                SpecularPower = 35,
                                            }) { Parent = LamborghiniMurcielago };
            murcielagoAirTakesDark = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakesDark"),
                                                      new Constant
                                                      {
                                                          DiffuseColor = new Color(10, 10, 10),
                                                          AlphaBlending = 0.5f,
                                                      }) { Parent = LamborghiniMurcielago };
            murcielagoFrontLightBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontLightBase"),
                                             new BlinnPhong
                                             {
                                                 DiffuseColor = new Color(50, 45, 40),
                                                 SpecularIntensity = 0.0f,
                                                 //SpecularPower = 300,
                                             }) { Parent = LamborghiniMurcielago };
            murcielagoWhiteMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-WhiteMetal"),
                                                              new BlinnPhong
                                                              {
                                                                  DiffuseColor = new Color(200, 200, 200),
                                                                  SpecularIntensity = 4 * reflectioTextureMultiplier,
                                                                  SpecularPower = 5,
                                                                  ReflectionTexture = reflectionTexture,
                                                              }) { Parent = LamborghiniMurcielago };
            murcielagoBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackMetal"), blackMetalMaterial) { Parent = LamborghiniMurcielago };
            murcielagoLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Logo"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Normal"),
                                                     SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Diffuse"),
                                                     SpecularIntensity = 10f * reflectioTextureMultiplier,
                                                     SpecularPower = 10,
                                                     ReflectionTexture = reflectionTexture,
                                                 }) { Parent = LamborghiniMurcielago };
            murcielagoFloor = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Floor"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Floor"),
                                                     SpecularIntensity = 0.05f,
                                                     SpecularPower = 1000,
                                                 }) { Parent = LamborghiniMurcielago };
            murcielagoBlackContant = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackConstant"),
                                     new Constant
                                     {
                                         DiffuseColor = Color.Black,
                                     }) { Parent = LamborghiniMurcielago };
            murcielagoLP640Exhaust = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"),
                                     new BlinnPhong
                                     {
                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"),
                                         SpecularIntensity = 0.1f,
                                         SpecularPower = 300,
                                     }) { Parent = LamborghiniMurcielago };
            murcielagoLP640Grid = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Grid"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Grid"),
                                                     SpecularIntensity = 1.0f,
                                                 }) { Parent = LamborghiniMurcielago };
            
            murcielagoCarbonFiber = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-CarbonFiber"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("LamborghiniMurcielago\\CarbonFiber"),
                                                      SpecularIntensity = 1f,
                                                      SpecularPower = 50,
                                                      ReflectionTexture = reflectionTexture,
                                                  }) { Parent = LamborghiniMurcielago };
            murcielagoSteeringWheel = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-SteeringWheel"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-Diffuse"),
                                                     NormalTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-Normal"),
                                                     //SpecularTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-Specular"),
                                                     SpecularIntensity = 0.15f,
                                                     SpecularPower = 10,
                                                 }) { Parent = LamborghiniMurcielago };
            murcielagoInteriorDetails = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorDetails"), detailsMaterial) { Parent = LamborghiniMurcielago };
            murcielagoTablero = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Tablero"),
                                                new BlinnPhong
                                                {
                                                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tablero"),
                                                    SpecularIntensity = 0.1f,
                                                    SpecularPower = 500,
                                                }) { Parent = LamborghiniMurcielago };
            
            murcielagoInteriorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorLeather"), leatherMaterial) { Parent = LamborghiniMurcielago };
            murcielagoInteriorCostura = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorCostura"), costuraMaterial) { Parent = LamborghiniMurcielago };

            /*                  
            murcielagoLP640RearSpoilerDarkPart = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RearSpoilerDarkPart"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseColor = new Color(30, 30, 30),
                                                     SpecularIntensity = 0.05f,
                                                     SpecularPower = 1000,
                                                 }) { Parent = LamborghiniMurcielago };*/
            
            #endregion
            
            #region Left Door

            murcielagoLeftDoor = new GameObject3D { Parent = LamborghiniMurcielago};
            murcielagoLeftDoor.Transform.Rotate(new Vector3(10, 0, 0), Space.World);
            murcielagoLeftDoor.Transform.Position = new Vector3(0, 0.2708f, 2.2335f);

            murcielagoLeftDoorBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorBody"), carPaintMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorBlackMetal"), blackMetalMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorGrayMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorGrayMetal"), grayMetalMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorSpeakers = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorSpeakers"), leftDoorSpeakersMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorGlass = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorGlass"), glassesMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorLeather"), leatherMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorCostura = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorCostura"), costuraMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorDetails = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorDetails"), detailsMaterial) { Parent = murcielagoLeftDoor };

            #endregion

            #region Right Door

            murcielagoRightDoorBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorBody"), carPaintMaterial) { Parent = LamborghiniMurcielago };
            murcielagoRightDoorBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorBlackMetal"), blackMetalMaterial) { Parent = LamborghiniMurcielago };
            murcielagoRightDoorGrayMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorGrayMetal"), grayMetalMaterial) { Parent = LamborghiniMurcielago };
            murcielagoRightDoorSpeakers = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorSpeakers"), leftDoorSpeakersMaterial) { Parent = LamborghiniMurcielago };
            murcielagoRightDoorGlass = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorGlass"), glassesMaterial) { Parent = LamborghiniMurcielago };
            murcielagoRightDoorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorLeather"), leatherMaterial) { Parent = LamborghiniMurcielago };
            murcielagoRightDoorCostura = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorCostura"), costuraMaterial) { Parent = LamborghiniMurcielago };
            murcielagoRightDoorDetails = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorDetails"), detailsMaterial) { Parent = LamborghiniMurcielago };
            
            #endregion
            
            #region Front Left Wheel

            murcielagoLP670FrontLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-FrontRim"), rimMaterial);
            murcielagoLP640FrontLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"), rimBaseMaterial);
            murcielagoLP640FrontLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"), rimBaseMaterial2);
            murcielagoFrontLeftBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"), brakeCaliperMaterial);
            murcielagoFrontLeftBrakeCaliper.Transform.Rotate(new Vector3(183, 0, 0), Space.Local);
            murcielagoFrontLeftBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"), brakeDiscMaterial);
            murcielagoFrontLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"), rimLogoMaterial);
            murcielagoFrontLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre"), tyreMaterial);
            murcielagoFrontLeftTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre02"), tyreMaterial2);

            frontLeftRim = new GameObject3D();
            frontLeftRim.Transform.LocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            frontLeftRim.Transform.Rotate(new Vector3(0, -40, 0), Space.World);
            frontLeftRim.Transform.Position = new Vector3(1.6f, -0.5f, 2.35f);

            murcielagoLP670FrontLeftRim.Parent = frontLeftRim;
            murcielagoLP640FrontLeftRimBase.Parent = frontLeftRim;
            murcielagoLP640FrontLeftRimBase02.Parent = frontLeftRim;
            murcielagoFrontLeftRimLogo.Parent = frontLeftRim;
            murcielagoFrontLeftBrakeDisc.Parent = frontLeftRim;
            murcielagoFrontLeftBrakeCaliper.Parent = frontLeftRim;
            murcielagoFrontLeftTyre.Parent = frontLeftRim;
            murcielagoFrontLeftTyre02.Parent = frontLeftRim;

            frontLeftRim.Parent = LamborghiniMurcielago;

            #endregion

            #region Front Right Wheel

            murcielagoLP670FrontRightRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-FrontRim"), rimMaterial);
            murcielagoLP640FrontRightRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"), rimBaseMaterial);
            murcielagoLP640FrontRightRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"), rimBaseMaterial2);
            murcielagoFrontRightBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"), brakeCaliperMaterial);
            murcielagoFrontRightBrakeCaliper.Transform.Rotate(new Vector3(7, 0, 0), Space.Local);
            murcielagoFrontRightBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"), brakeDiscMaterial);
            murcielagoFrontRightRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"), rimLogoMaterial);
            murcielagoFrontRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre"), tyreMaterial);
            murcielagoFrontRightTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre02"), tyreMaterial2);

            frontRightRim = new GameObject3D
                                {
                                    Transform =
                                        {
                                            LocalScale = new Vector3(1.1f, 1.1f, 1.1f),
                                            Position = new Vector3(-1.6f, -0.5f, 2.35f)
                                        }
                                };
            frontRightRim.Transform.Rotate(new Vector3(0, -40 + 180, 0), Space.World);

            murcielagoLP670FrontRightRim.Parent = frontRightRim;
            murcielagoLP640FrontRightRimBase.Parent = frontRightRim;
            murcielagoLP640FrontRightRimBase02.Parent = frontRightRim;
            murcielagoFrontRightRimLogo.Parent = frontRightRim;
            murcielagoFrontRightBrakeDisc.Parent = frontRightRim;
            murcielagoFrontRightBrakeCaliper.Parent = frontRightRim;
            murcielagoFrontRightTyre.Parent = frontRightRim;
            murcielagoFrontRightTyre02.Parent = frontRightRim;

            frontRightRim.Parent = LamborghiniMurcielago;

            #endregion

            #region Rear Left Wheel

            murcielagoLP670RearLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-RearRim"), rimMaterial);
            murcielagoLP640RearLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"), rimBaseMaterial);
            murcielagoLP640RearLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"), rimBaseMaterial2);
            murcielagoRearLeftBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"), brakeDiscMaterial);
            murcielagoRearLeftBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"), brakeCaliperMaterial);
            murcielagoRearLeftBrakeCaliper.Transform.Rotate(new Vector3(4, 0, 0), Space.Local);
            murcielagoRearLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"), rimLogoMaterial);
            murcielagoRearLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre"), tyreMaterial);
            murcielagoRearLeftTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre02"), tyreMaterial2);

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

            rearLeftRim.Parent = LamborghiniMurcielago;

            #endregion

            #region Rear Right Wheel

            murcielagoLP670RearRightRim       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-RearRim"), rimMaterial);
            murcielagoLP640RearRightRimBase   = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"), rimBaseMaterial);
            murcielagoLP640RearRightRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"), rimBaseMaterial2);
            murcielagoRearRightBrakeCaliper   = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"), brakeCaliperMaterial);
            murcielagoRearRightBrakeCaliper.Transform.Rotate(new Vector3(182, 0, 0), Space.Local);
            murcielagoRearRightBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"), brakeDiscMaterial);
            murcielagoRearRightRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"), rimLogoMaterial);
            murcielagoRearRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre"), tyreMaterial);
            murcielagoRearRightTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre02"), tyreMaterial2);

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
            
            rearRightRim.Parent = LamborghiniMurcielago;
            
            #endregion             
            
        } // Load

        #endregion

    } // LamborghiniMurcielagoLoader
} // XNAFinalEngineExamples
