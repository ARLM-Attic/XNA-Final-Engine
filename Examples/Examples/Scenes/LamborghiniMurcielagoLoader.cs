
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

        private float leftDoorAngle, rightDoorAngle, tiresAngle; 
        
        // Now every entity is a game object and the entity’s behavior is defined by the components attached to it.
        // There are several types of components, components related to models, to sound, to particles, to physics, etc.
        private GameObject3D // Lambo body
                            murcielagoBody, murcielagoLP640AirTakesEngine, murcielagoLP640AirTakes, murcielagoAirTakesDark,
                            murcielagoFrontLightBase, murcielagoLights, murcielagoLightsGlasses,
                            murcielagoWhiteMetal, murcielagoGrayMetal, murcielagoCarbonFiber, murcielagoGlasses,
                            murcielagoEngineGlasses, murcielagoBlackMetal, murcielagoLogo, murcielagoBlackContant, murcielagoFloor,
                            murcielagoLP640Grid, murcielagoLP640Exhaust, murcielagoLP640LeatherPattern,
                            murcielagoInteriorLeather, murcielagoSteeringWheel, murcielagoInteriorDetails, murcielagoBlackPlastic,
                            murcielagoInteriorCostura, murcielagoTablero, murcielagoRedPlastic,
                            // Left Door
                            murcielagoLeftDoorBody, murcielagoLeftDoorBlackMetal, murcielagoLeftDoorGrayMetal, murcielagoLeftDoorLeather,
                            murcielagoLeftDoorSpeakers, murcielagoLeftDoorGlass, murcielagoLeftDoorCostura, murcielagoLeftDoorDetails, murcielagoLeftDoor,
                            // Right Door
                            murcielagoRightDoorBody, murcielagoRightDoorBlackMetal, murcielagoRightDoorGrayMetal, murcielagoRightDoorLeather,
                            murcielagoRightDoorSpeakers, murcielagoRightDoorGlass, murcielagoRightDoorCostura, murcielagoRightDoorDetails, murcielagoRightDoor,
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
                            // Shadow
                            murcielagoShadow, murcielagoShadowLeftDoor, murcielagoShadowRightDoor,
                            murcielagoShadowFrontLeftTyre, murcielagoShadowFrontRightTyre, murcielagoShadowRearLeftTyre, murcielagoShadowRearRightTyre;
        
        #endregion

        #region Properties

        /// <summary>
        /// The game object that contains the whole Murcielago.
        /// </summary>
        public GameObject3D LamborghiniMurcielago { get; private set; }

        /// <summary>
        /// Left door angle.
        /// </summary>
        public float LeftDoorAngle
        {
            get { return leftDoorAngle; }
            set
            {
                leftDoorAngle = value;
                if (leftDoorAngle < 0)
                    leftDoorAngle = 0;
                if (leftDoorAngle > 47)
                    leftDoorAngle = 47;
                float yaw;
                if (leftDoorAngle < 4)
                    yaw = -(float) Math.Pow(leftDoorAngle * 3.1416f / 1800f, 0.85f);
                else
                    yaw = -(float)Math.Pow(4 * 3.1416f / 1800f, 0.85f);
                // Move the model to the world origin, rotate and then move again to its original position.
                murcielagoLeftDoor.Transform.LocalMatrix = Matrix.CreateTranslation(new Vector3(0, -0.2586f, -2.1334f)) *
                                                           Matrix.CreateFromYawPitchRoll(yaw, leftDoorAngle * 3.1416f / 180f, 0) *
                                                           Matrix.CreateTranslation(new Vector3(0, 0.2586f, 2.1334f));
            }
        } // LeftDoorAngle

        /// <summary>
        /// Right door angle.
        /// </summary>
        public float RightDoorAngle
        {
            get { return rightDoorAngle; }
            set
            {
                rightDoorAngle = value;
                if (rightDoorAngle < 0)
                    rightDoorAngle = 0;
                if (rightDoorAngle > 47)
                    rightDoorAngle = 47;
                float yaw;
                if (rightDoorAngle < 4)
                    yaw = (float)Math.Pow(rightDoorAngle * 3.1416f / 1800f, 0.85f);
                else
                    yaw = (float)Math.Pow(4 * 3.1416f / 1800f, 0.85f);
                // Move the model to the world origin, rotate and then move again to its original position.
                murcielagoRightDoor.Transform.LocalMatrix = Matrix.CreateTranslation(new Vector3(0, -0.2586f, -2.1334f)) *
                                                           Matrix.CreateFromYawPitchRoll(yaw, rightDoorAngle * 3.1416f / 180f, 0) *
                                                           Matrix.CreateTranslation(new Vector3(0, 0.2586f, 2.1334f));
            }
        } // RightDoorAngle

        /// <summary>
        /// Tires angle.
        /// </summary>
        public float TiresAngle
        {
            get { return tiresAngle; }
            set
            {
                tiresAngle = value;
                if (tiresAngle < -40)
                    tiresAngle = -40;
                if (tiresAngle > 40)
                    tiresAngle = 40;
                frontLeftRim.Transform.LocalRotation = Quaternion.Identity;
                frontLeftRim.Transform.Rotate(new Vector3(0, tiresAngle, 0), Space.World);
                frontRightRim.Transform.LocalRotation = Quaternion.Identity;
                frontRightRim.Transform.Rotate(new Vector3(0, tiresAngle + 180, 0), Space.World);

                murcielagoSteeringWheel.Transform.LocalMatrix = Matrix.CreateTranslation(new Vector3(-0.7628f, -0.1094f, -1.0251f)) *
                                                                Matrix.CreateFromYawPitchRoll(0, -19 * 3.1416f / 180f, 0) *
                                                                Matrix.CreateFromYawPitchRoll(0, 0, -tiresAngle * 3.1416f / 180f) *
                                                                Matrix.CreateFromYawPitchRoll(0, 19 * 3.1416f / 180f, 0) *
                                                                Matrix.CreateTranslation(new Vector3(0.7628f, 0.1094f, 1.0251f));
            }
        } // LeftDoorAngle

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
                SpecularPower = 1,
                BasePaintColor = new Color(0.79f, 0.45f, 0),
                SecondBasePaintColor = new Color(1f, 0.3f, 0),
                ThirdBasePaintColor = new Color(0.35f, 0.6f, 0.5f),
                FlakesColor = new Color(255, 100, 100),
                ReflectionTexture = reflectionTexture,
            };

            BlinnPhong rimMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(30, 30, 30),
                SpecularPower = 50,
                //NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP670-Rim-Normal"),
                SpecularIntensity = 150f * reflectioTextureMultiplier,
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
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-DiffuseXbox"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-SpecularXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Diffuse"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\LP640-AirTakesEngine-Specular"),
                #endif
                SpecularIntensity = 0.5f,
                SpecularPower = 30,
            };
            BlinnPhong engineGlassesMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(25, 25, 32),
                AlphaBlending = 0.85f,
                SpecularIntensity = 0.2f * reflectioTextureMultiplier,
                SpecularPower = 1,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong lightsMaterial = new BlinnPhong
            {
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LightsXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Lights"),
                #endif
                SpecularIntensity = 6f * reflectioTextureMultiplier,
                SpecularPower = 30,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong lightsGlassesMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(20, 20, 21),
                //DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Lights"),
                AlphaBlending = 0.7f,
                SpecularIntensity = 5f * reflectioTextureMultiplier,
                SpecularPower = 0.5f,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong glassesMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(20, 20, 20),
                AlphaBlending = 0.6f,
                SpecularIntensity = 0.5f * reflectioTextureMultiplier,
                SpecularPower = 1,
                ReflectionTexture = reflectionTexture,
            };

            BlinnPhong brakeCaliperMaterial = new BlinnPhong
            {
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\LamborghiniBrakeCaliperXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\LamborghiniBrakeCaliper"),
                #endif
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
                #if XBOX
                    DiffuseTexture  = new Texture("LamborghiniMurcielago\\BrakeDisc-DiffuseXbox"),
                    NormalTexture   = new Texture("LamborghiniMurcielago\\BrakeDisc-NormalXbox"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-SpecularXbox"),
                #else
                    DiffuseTexture  = new Texture("LamborghiniMurcielago\\BrakeDisc-Diffuse"),
                    NormalTexture   = new Texture("LamborghiniMurcielago\\BrakeDisc-Normal"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\BrakeDisc-Specular"),
                #endif
                SpecularIntensity = 15.0f,
                SpecularPower = 30,
            };
            BlinnPhong rimLogoMaterial = new BlinnPhong
            {
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-LogoXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Lamborghini-Logo"),
                #endif
                SpecularIntensity = 0.75f * reflectioTextureMultiplier,
                SpecularPower = 500,
                ReflectionTexture = reflectionTexture,
            };
            BlinnPhong tyreMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(10, 10, 10),
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-DiffuseXbox"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-NormalXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Diffuse"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\PirelliPZero-Normal"),
                #endif
                SpecularIntensity = 0.2f,
                SpecularPower = 50,
            };
            BlinnPhong tyreMaterial2 = new BlinnPhong
            {
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-DiffuseXbox"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-NormalXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Diffuse"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tyre-Normal"),
                #endif
                SpecularIntensity = 0.05f,
                SpecularPower = 50,
            };
            BlinnPhong blackMetalMaterial = new BlinnPhong
            {
                DiffuseColor = new Color(10, 10, 10),
                SpecularIntensity = 100 * reflectioTextureMultiplier,
                SpecularPower = 150,
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
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Speaker-DiffuseXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Speaker-Diffuse"),
                #endif
                SpecularIntensity = 0f,
                SpecularPower = 50,
            };
            BlinnPhong leatherMaterial = new BlinnPhong
            {
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Leather-DiffuseXbox"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Leather-NormalXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Leather-Diffuse"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Leather-Normal"),
                #endif
                SpecularIntensity = 0.15f,
                SpecularPower = 20,
            };
            BlinnPhong costuraMaterial = new BlinnPhong
            {
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Costura-DiffuseXbox"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Costura-NormalXbox"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\Costura-SpecularXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Costura-Diffuse"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Costura-Normal"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\Costura-Specular"),
                #endif
                SpecularIntensity = 0.15f,
                SpecularPower = 20,
            };
            BlinnPhong detailsMaterial = new BlinnPhong
            {
                #if XBOX
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-DXbox"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-NXbox"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-SXbox"),
                #else
                    DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Diffuse"),
                    NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Normal"),
                    SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-InteriorDetails-Specular"),
                #endif
                SpecularIntensity = 2f,
                SpecularPower = 300,
            };
            
            #endregion
            
            #region Body
            
            // I upload the xnb instead of the fbx and different xnb are generated for Xbox and PC.
            // It is a little mess you won't need to do in your project.
            // But I need to do it because some texture reference I left.

            murcielagoBody                = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Body"
#if XBOX 
    + "Xbox"
#endif
                ),                carPaintMaterial)       { Parent = LamborghiniMurcielago };
            murcielagoLP640AirTakes       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakes"
#if XBOX 
    + "Xbox"
#endif
                ),            airTakesMaterial)       { Parent = LamborghiniMurcielago };
            murcielagoLights              = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Lights"
#if XBOX 
    + "Xbox"
#endif
                ),              lightsMaterial)         { Parent = LamborghiniMurcielago };
            murcielagoLightsGlasses       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LightGlasses"
#if XBOX 
    + "Xbox"
#endif
                ),        lightsGlassesMaterial)  { Parent = LamborghiniMurcielago };
            murcielagoGrayMetal           = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-GrayMetal"
#if XBOX 
    + "Xbox"
#endif
                ),           grayMetalMaterial)      { Parent = LamborghiniMurcielago };
            murcielagoGlasses             = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Glasses"
#if XBOX 
    + "Xbox"
#endif
                ),             glassesMaterial)        { Parent = LamborghiniMurcielago };
            murcielagoEngineGlasses       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-EngineGlasses"
#if XBOX 
    + "Xbox"
#endif
                ), engineGlassesMaterial)  { Parent = LamborghiniMurcielago };
            murcielagoLP640AirTakesEngine = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakesEngine"
#if XBOX 
    + "Xbox"
#endif
                ),      airTakesEngineMaterial) { Parent = LamborghiniMurcielago };
            murcielagoBlackPlastic        = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackPlastic"
#if XBOX 
    + "Xbox"
#endif
                ),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(10, 10, 10),
                                                SpecularIntensity = 0.1f,
                                                SpecularPower = 3,
                                            }) { Parent = LamborghiniMurcielago };
            murcielagoRedPlastic = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RedPlastic"
#if XBOX 
    + "Xbox"
#endif
                ),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = new Color(250, 50, 50),
                                                SpecularIntensity = 0.02f,
                                                SpecularPower = 500,
                                            }) { Parent = LamborghiniMurcielago };
            murcielagoLP640LeatherPattern = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-LeatherPattern"
#if XBOX 
    + "Xbox"
#endif
                ),
                                            new BlinnPhong
                                            {
                                                #if XBOX
                                                    DiffuseTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-DiffuseXbox"),
                                                    NormalTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-NormalXbox"),
                                                #else
                                                    DiffuseTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-Diffuse"),
                                                    NormalTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-Normal"),
                                                #endif
                                                //SpecularTexture = new Texture("LamborghiniMurcielago\\LeatherPattern-Specular"),
                                                SpecularPowerFromTexture = false,
                                                SpecularIntensity = 0.03f,
                                                SpecularPower = 35,
                                                ParallaxEnabled = false,
                                            }) { Parent = LamborghiniMurcielago };
            murcielagoAirTakesDark = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-AirTakesDark"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                      new Constant
                                                      {
                                                          DiffuseColor = new Color(10, 10, 10),
                                                          AlphaBlending = 0.5f,
                                                      }) { Parent = LamborghiniMurcielago };
            murcielagoFrontLightBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontLightBase"
#if XBOX 
    + "Xbox"
#endif
                ),
                                             new BlinnPhong
                                             {
                                                 DiffuseColor = new Color(50, 45, 40),
                                                 SpecularIntensity = 0.0f,
                                                 //SpecularPower = 300,
                                             }) { Parent = LamborghiniMurcielago };
            murcielagoWhiteMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-WhiteMetal"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                              new BlinnPhong
                                                              {
                                                                  DiffuseColor = new Color(200, 200, 200),
                                                                  SpecularIntensity = 4 * reflectioTextureMultiplier,
                                                                  SpecularPower = 5,
                                                                  ReflectionTexture = reflectionTexture,
                                                              }) { Parent = LamborghiniMurcielago };
            murcielagoBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackMetal"
#if XBOX 
    + "Xbox"
#endif
                ), blackMetalMaterial) { Parent = LamborghiniMurcielago };
            murcielagoLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Logo"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                 new BlinnPhong
                                                 {
                                                    #if XBOX
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-DiffuseXbox"),
                                                        NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-NormalXbox"),
                                                        SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-DiffuseXbox"),
                                                    #else
                                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Diffuse"),
                                                         NormalTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Normal"),
                                                         SpecularTexture = new Texture("LamborghiniMurcielago\\Murcielago-Logo-Diffuse"),
                                                     #endif
                                                     SpecularIntensity = 10f * reflectioTextureMultiplier,
                                                     SpecularPower = 10,
                                                     ReflectionTexture = reflectionTexture,
                                                 }) { Parent = LamborghiniMurcielago };
            murcielagoFloor = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Floor"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                 new BlinnPhong
                                                 {
                                                     #if XBOX
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-FloorXbox"),
                                                     #else
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Floor"),
                                                     #endif
                                                     SpecularIntensity = 0.05f,
                                                     SpecularPower = 1000,
                                                 }) { Parent = LamborghiniMurcielago };
            murcielagoBlackContant = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BlackConstant"
#if XBOX 
    + "Xbox"
#endif
                ),
                                     new Constant
                                     {
                                         DiffuseColor = Color.Black,
                                     }) { Parent = LamborghiniMurcielago };
            murcielagoLP640Exhaust = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"
#if XBOX 
    + "Xbox"
#endif
                ),
                                     new BlinnPhong
                                     {
                                         #if XBOX
                                            DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-ExhaustXbox"),
                                         #else
                                            DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Exhaust"),
                                         #endif
                                         SpecularIntensity = 0.1f,
                                         SpecularPower = 300,
                                     }) { Parent = LamborghiniMurcielago };
            murcielagoLP640Grid = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-Grid"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                 new BlinnPhong
                                                 {
                                                      #if XBOX
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-GridXbox"),
                                                      #else
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-LP640-Grid"),
                                                      #endif
                                                     SpecularIntensity = 1.0f,
                                                 }) { Parent = LamborghiniMurcielago };
            
            murcielagoCarbonFiber = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-CarbonFiber"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                  new BlinnPhong
                                                  {
                                                      #if XBOX
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\CarbonFiberXbox"),
                                                      #else
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\CarbonFiber"),
                                                      #endif
                                                      SpecularIntensity = 1f,
                                                      SpecularPower = 50,
                                                      ReflectionTexture = reflectionTexture,
                                                  }) { Parent = LamborghiniMurcielago };
            murcielagoSteeringWheel = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-SteeringWheel"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                 new BlinnPhong
                                                 {
                                                     #if XBOX
                                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-DiffuseXbox"),
                                                         NormalTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-NormalXbox"),
                                                     #else
                                                         DiffuseTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-Diffuse"),
                                                         NormalTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-Normal"),
                                                     #endif
                                                     //SpecularTexture = new Texture("LamborghiniMurcielago\\SteeringWheel-Specular"),
                                                     SpecularIntensity = 0.15f,
                                                     SpecularPower = 10,
                                                 }) { Parent = LamborghiniMurcielago };
            murcielagoInteriorDetails = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorDetails"
#if XBOX 
    + "Xbox"
#endif
                ), detailsMaterial) { Parent = LamborghiniMurcielago };
            murcielagoTablero = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Tablero"
#if XBOX 
    + "Xbox"
#endif
                ),
                                                new BlinnPhong
                                                {
                                                    #if XBOX
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-TableroXbox"),
                                                    #else
                                                        DiffuseTexture = new Texture("LamborghiniMurcielago\\Murcielago-Tablero"),
                                                    #endif
                                                    SpecularIntensity = 0.1f,
                                                    SpecularPower = 500,
                                                }) { Parent = LamborghiniMurcielago };
            
            murcielagoInteriorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorLeather"
#if XBOX 
    + "Xbox"
#endif
                ), leatherMaterial) { Parent = LamborghiniMurcielago };
            murcielagoInteriorCostura = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-InteriorCostura"
#if XBOX 
    + "Xbox"
#endif
                ), costuraMaterial) { Parent = LamborghiniMurcielago };
            
            // 20k polys shadow.
            murcielagoShadow = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Shadow"
#if XBOX 
    + "Xbox"
#endif
                ), null) { Parent = LamborghiniMurcielago };
            murcielagoBody.ModelRenderer.CastShadows = false;
            murcielagoLP640AirTakesEngine.ModelRenderer.CastShadows = false;
            murcielagoLP640AirTakes.ModelRenderer.CastShadows = false;
            murcielagoAirTakesDark.ModelRenderer.CastShadows = false;
            murcielagoFrontLightBase.ModelRenderer.CastShadows = false;
            murcielagoLights.ModelRenderer.CastShadows = false;
            murcielagoLightsGlasses.ModelRenderer.CastShadows = false;
            murcielagoWhiteMetal.ModelRenderer.CastShadows = false;
            murcielagoGrayMetal.ModelRenderer.CastShadows = false;
            murcielagoCarbonFiber.ModelRenderer.CastShadows = false;
            murcielagoGlasses.ModelRenderer.CastShadows = false;
            murcielagoEngineGlasses.ModelRenderer.CastShadows = false;
            murcielagoBlackMetal.ModelRenderer.CastShadows = false;
            murcielagoLogo.ModelRenderer.CastShadows = false;
            murcielagoBlackContant.ModelRenderer.CastShadows = false;
            murcielagoFloor.ModelRenderer.CastShadows = false;
            murcielagoLP640Grid.ModelRenderer.CastShadows = false;
            murcielagoLP640Exhaust.ModelRenderer.CastShadows = false;
            murcielagoLP640LeatherPattern.ModelRenderer.CastShadows = false;
            murcielagoInteriorLeather.ModelRenderer.CastShadows = false;
            murcielagoSteeringWheel.ModelRenderer.CastShadows = false;
            murcielagoInteriorDetails.ModelRenderer.CastShadows = false;
            murcielagoBlackPlastic.ModelRenderer.CastShadows = false;
            murcielagoInteriorCostura.ModelRenderer.CastShadows = false;
            murcielagoTablero.ModelRenderer.CastShadows = false;
            murcielagoRedPlastic.ModelRenderer.CastShadows = false;
            
            #endregion
            
            #region Left Door

            murcielagoLeftDoor = new GameObject3D { Parent = LamborghiniMurcielago};           

            murcielagoLeftDoorBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorBody"
#if XBOX 
    + "Xbox"
#endif
                ), carPaintMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorBlackMetal"
#if XBOX 
    + "Xbox"
#endif
                ), blackMetalMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorGrayMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorGrayMetal"
#if XBOX 
    + "Xbox"
#endif
                ), grayMetalMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorSpeakers = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorSpeakers"
#if XBOX 
    + "Xbox"
#endif
                ), leftDoorSpeakersMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorGlass = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorGlass"
#if XBOX 
    + "Xbox"
#endif
                ), glassesMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorLeather"
#if XBOX 
    + "Xbox"
#endif
                ), leatherMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorCostura = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorCostura"
#if XBOX 
    + "Xbox"
#endif
                ), costuraMaterial) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorDetails = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LeftDoorDetails"
#if XBOX 
    + "Xbox"
#endif
                ), detailsMaterial) { Parent = murcielagoLeftDoor };

            // 2k polys shadow.
            murcielagoShadowLeftDoor = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-ShadowLeftDoor"
#if XBOX 
    + "Xbox"
#endif
                ), null) { Parent = murcielagoLeftDoor };
            murcielagoLeftDoorBody.ModelRenderer.CastShadows = false;
            murcielagoLeftDoorBlackMetal.ModelRenderer.CastShadows = false;
            murcielagoLeftDoorGrayMetal.ModelRenderer.CastShadows = false;
            murcielagoLeftDoorSpeakers.ModelRenderer.CastShadows = false;
            murcielagoLeftDoorGlass.ModelRenderer.CastShadows = false;
            murcielagoLeftDoorLeather.ModelRenderer.CastShadows = false;
            murcielagoLeftDoorCostura.ModelRenderer.CastShadows = false;
            murcielagoLeftDoorDetails.ModelRenderer.CastShadows = false;

            #endregion

            #region Right Door

            murcielagoRightDoor = new GameObject3D { Parent = LamborghiniMurcielago };

            murcielagoRightDoorBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorBody"
#if XBOX 
    + "Xbox"
#endif
                ), carPaintMaterial) { Parent = murcielagoRightDoor };
            murcielagoRightDoorBlackMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorBlackMetal"
#if XBOX 
    + "Xbox"
#endif
                ), blackMetalMaterial) { Parent = murcielagoRightDoor };
            murcielagoRightDoorGrayMetal = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorGrayMetal"
#if XBOX 
    + "Xbox"
#endif
                ), grayMetalMaterial) { Parent = murcielagoRightDoor };
            murcielagoRightDoorSpeakers = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorSpeakers"
#if XBOX 
    + "Xbox"
#endif
                ), leftDoorSpeakersMaterial) { Parent = murcielagoRightDoor };
            murcielagoRightDoorGlass = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorGlass"
#if XBOX 
    + "Xbox"
#endif
                ), glassesMaterial) { Parent = murcielagoRightDoor };
            murcielagoRightDoorLeather = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorLeather"
#if XBOX 
    + "Xbox"
#endif
                ), leatherMaterial) { Parent = murcielagoRightDoor };
            murcielagoRightDoorCostura = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorCostura"
#if XBOX 
    + "Xbox"
#endif
                ), costuraMaterial) { Parent = murcielagoRightDoor };
            murcielagoRightDoorDetails = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RightDoorDetails"
#if XBOX 
    + "Xbox"
#endif
                ), detailsMaterial) { Parent = murcielagoRightDoor };

            // 2k polys shadow.
            murcielagoShadowRightDoor = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-ShadowRightDoor"
#if XBOX 
    + "Xbox"
#endif
                ), null) { Parent = murcielagoRightDoor };
            murcielagoRightDoorBody.ModelRenderer.CastShadows = false;
            murcielagoRightDoorBlackMetal.ModelRenderer.CastShadows = false;
            murcielagoRightDoorGrayMetal.ModelRenderer.CastShadows = false;
            murcielagoRightDoorSpeakers.ModelRenderer.CastShadows = false;
            murcielagoRightDoorGlass.ModelRenderer.CastShadows = false;
            murcielagoRightDoorLeather.ModelRenderer.CastShadows = false;
            murcielagoRightDoorCostura.ModelRenderer.CastShadows = false;
            murcielagoRightDoorDetails.ModelRenderer.CastShadows = false;
            
            #endregion
            
            #region Front Left Wheel

            murcielagoLP670FrontLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-FrontRim"
#if XBOX 
    + "Xbox"
#endif
                ), rimMaterial);
            murcielagoLP640FrontLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial);
            murcielagoLP640FrontLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial2);
            murcielagoFrontLeftBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"
#if XBOX 
    + "Xbox"
#endif
                ), brakeCaliperMaterial);
            murcielagoFrontLeftBrakeCaliper.Transform.Rotate(new Vector3(183, 0, 0), Space.Local);
            murcielagoFrontLeftBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"
#if XBOX 
    + "Xbox"
#endif
                ), brakeDiscMaterial);
            murcielagoFrontLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"
#if XBOX 
    + "Xbox"
#endif
                ), rimLogoMaterial);
            murcielagoFrontLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial);
            murcielagoFrontLeftTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre02"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial2);

            frontLeftRim = new GameObject3D();
            frontLeftRim.Transform.LocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            frontLeftRim.Transform.Rotate(new Vector3(0, -tiresAngle, 0), Space.World);
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

            // 2k polys shadow.
            murcielagoShadowFrontLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-ShadowFrontTyre"
#if XBOX 
    + "Xbox"
#endif
                ), null) { Parent = frontLeftRim };
            murcielagoLP670FrontLeftRim.ModelRenderer.CastShadows = false;
            murcielagoLP640FrontLeftRimBase.ModelRenderer.CastShadows = false;
            murcielagoLP640FrontLeftRimBase02.ModelRenderer.CastShadows = false;
            murcielagoFrontLeftRimLogo.ModelRenderer.CastShadows = false;
            murcielagoFrontLeftBrakeDisc.ModelRenderer.CastShadows = false;
            murcielagoFrontLeftBrakeCaliper.ModelRenderer.CastShadows = false;
            murcielagoFrontLeftTyre.ModelRenderer.CastShadows = false;
            murcielagoFrontLeftTyre02.ModelRenderer.CastShadows = false;

            #endregion

            #region Front Right Wheel

            murcielagoLP670FrontRightRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-FrontRim"
#if XBOX 
    + "Xbox"
#endif
                ), rimMaterial);
            murcielagoLP640FrontRightRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial);
            murcielagoLP640FrontRightRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial2);
            murcielagoFrontRightBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"
#if XBOX 
    + "Xbox"
#endif
                ), brakeCaliperMaterial);
            murcielagoFrontRightBrakeCaliper.Transform.Rotate(new Vector3(7, 0, 0), Space.Local);
            murcielagoFrontRightBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"
#if XBOX 
    + "Xbox"
#endif
                ), brakeDiscMaterial);
            murcielagoFrontRightRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"
#if XBOX 
    + "Xbox"
#endif
                ), rimLogoMaterial);
            murcielagoFrontRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial);
            murcielagoFrontRightTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontTyre02"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial2);

            frontRightRim = new GameObject3D();
            frontRightRim.Transform.LocalScale = new Vector3(1.1f, 1.1f, 1.1f);
            frontRightRim.Transform.Rotate(new Vector3(0, tiresAngle + 180, 0));
            frontRightRim.Transform.Position = new Vector3(-1.6f, -0.5f, 2.35f);

            murcielagoLP670FrontRightRim.Parent = frontRightRim;
            murcielagoLP640FrontRightRimBase.Parent = frontRightRim;
            murcielagoLP640FrontRightRimBase02.Parent = frontRightRim;
            murcielagoFrontRightRimLogo.Parent = frontRightRim;
            murcielagoFrontRightBrakeDisc.Parent = frontRightRim;
            murcielagoFrontRightBrakeCaliper.Parent = frontRightRim;
            murcielagoFrontRightTyre.Parent = frontRightRim;
            murcielagoFrontRightTyre02.Parent = frontRightRim;

            frontRightRim.Parent = LamborghiniMurcielago;

            // 2k polys shadow.
            murcielagoShadowFrontRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-ShadowFrontTyre"
#if XBOX 
    + "Xbox"
#endif
                ), null) { Parent = frontLeftRim };
            murcielagoLP670FrontRightRim.ModelRenderer.CastShadows = false;
            murcielagoLP640FrontRightRimBase.ModelRenderer.CastShadows = false;
            murcielagoLP640FrontRightRimBase02.ModelRenderer.CastShadows = false;
            murcielagoFrontRightRimLogo.ModelRenderer.CastShadows = false;
            murcielagoFrontRightBrakeDisc.ModelRenderer.CastShadows = false;
            murcielagoFrontRightBrakeCaliper.ModelRenderer.CastShadows = false;
            murcielagoFrontRightTyre.ModelRenderer.CastShadows = false;
            murcielagoFrontRightTyre02.ModelRenderer.CastShadows = false;

            #endregion

            #region Rear Left Wheel

            murcielagoLP670RearLeftRim = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-RearRim"
#if XBOX 
    + "Xbox"
#endif
                ), rimMaterial);
            murcielagoLP640RearLeftRimBase = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial);
            murcielagoLP640RearLeftRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial2);
            murcielagoRearLeftBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"
#if XBOX 
    + "Xbox"
#endif
                ), brakeDiscMaterial);
            murcielagoRearLeftBrakeCaliper = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"
#if XBOX 
    + "Xbox"
#endif
                ), brakeCaliperMaterial);
            murcielagoRearLeftBrakeCaliper.Transform.Rotate(new Vector3(4, 0, 0), Space.Local);
            murcielagoRearLeftRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"
#if XBOX 
    + "Xbox"
#endif
                ), rimLogoMaterial);
            murcielagoRearLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial);
            murcielagoRearLeftTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre02"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial2);

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

            // 2k polys shadow.
            murcielagoShadowRearLeftTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-ShadowRearTyre"
#if XBOX 
    + "Xbox"
#endif
                ), null) { Parent = frontLeftRim };
            murcielagoLP670RearLeftRim.ModelRenderer.CastShadows = false;
            murcielagoLP640RearLeftRimBase.ModelRenderer.CastShadows = false;
            murcielagoLP640RearLeftRimBase02.ModelRenderer.CastShadows = false;
            murcielagoRearLeftRimLogo.ModelRenderer.CastShadows = false;
            murcielagoRearLeftBrakeDisc.ModelRenderer.CastShadows = false;
            murcielagoRearLeftBrakeCaliper.ModelRenderer.CastShadows = false;
            murcielagoRearLeftTyre.ModelRenderer.CastShadows = false;
            murcielagoRearLeftTyre02.ModelRenderer.CastShadows = false;

            #endregion

            #region Rear Right Wheel

            murcielagoLP670RearRightRim       = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP670-RearRim"
#if XBOX 
    + "Xbox"
#endif
                ), rimMaterial);
            murcielagoLP640RearRightRimBase   = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial);
            murcielagoLP640RearRightRimBase02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-LP640-RimBase02"
#if XBOX 
    + "Xbox"
#endif
                ), rimBaseMaterial2);
            murcielagoRearRightBrakeCaliper   = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeCaliper"
#if XBOX 
    + "Xbox"
#endif
                ), brakeCaliperMaterial);
            murcielagoRearRightBrakeCaliper.Transform.Rotate(new Vector3(182, 0, 0), Space.Local);
            murcielagoRearRightBrakeDisc = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-BrakeDisc"
#if XBOX 
    + "Xbox"
#endif
                ), brakeDiscMaterial);
            murcielagoRearRightRimLogo = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-FrontRimLogo"
#if XBOX 
    + "Xbox"
#endif
                ), rimLogoMaterial);
            murcielagoRearRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial);
            murcielagoRearRightTyre02 = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-RearTyre02"
#if XBOX 
    + "Xbox"
#endif
                ), tyreMaterial2);

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

            // 2k polys shadow.
            murcielagoShadowRearRightTyre = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-ShadowRearTyre"
#if XBOX 
    + "Xbox"
#endif
                ), null) { Parent = frontLeftRim };
            murcielagoLP670RearRightRim.ModelRenderer.CastShadows = false;
            murcielagoLP640RearRightRimBase.ModelRenderer.CastShadows = false;
            murcielagoLP640RearRightRimBase02.ModelRenderer.CastShadows = false;
            murcielagoRearRightRimLogo.ModelRenderer.CastShadows = false;
            murcielagoRearRightBrakeDisc.ModelRenderer.CastShadows = false;
            murcielagoRearRightBrakeCaliper.ModelRenderer.CastShadows = false;
            murcielagoRearRightTyre.ModelRenderer.CastShadows = false;
            murcielagoRearRightTyre02.ModelRenderer.CastShadows = false;
            
            #endregion             
            
        } // Load

        #endregion

    } // LamborghiniMurcielagoLoader
} // XNAFinalEngineExamples
