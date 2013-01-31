
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
#if !XBOX
    using XNAFinalEngine.Editor;
#endif
using XNAFinalEngine.EngineCore;
using DirectionalLight = XNAFinalEngine.Components.DirectionalLight;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Size = XNAFinalEngine.Helpers.Size;
using Texture = XNAFinalEngine.Assets.Texture;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// Warehouse scene.
    /// </summary>
    public class WarehouseScene : Scene
    {

        #region Variables
        
        // Now every entity is a game object and the entity’s behavior is defined by the components attached to it.
        // There are several types of components, components related to models, to sound, to particles, to physics, etc.
        private GameObject3D // Models
                             warehouseWalls, warehouseRoof, warehouseRoof1, warehouseWood, warehouseWood2, warehouseBrokenWindow, warehouseWindow, warehouseGround,
                             // Lights
                             directionalLight, pointLight, pointLight2, spotLight, pointLightClipVolumeTest,
                             // Cameras
                             camera, camera2,
                             skydome,
                             dude, rifle;

        private GameObject2D demoLegend;

        private LamborghiniMurcielagoLoader lamborghiniMurcielagoLoader;

        private bool openingDoors = false;
        
        #endregion

        #region Load
        
        /// <summary>
        /// Load the resources.
        /// </summary>
        protected override void LoadContent()
        {

            #region Camera
            
            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.AddComponent<SoundListener>();
            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 300;
            camera.Camera.NearPlane = 1f; // Do not place a small value here, you can destroy performance, not just precision.
            camera.Transform.LookAt(new Vector3(0, 0, 15), Vector3.Zero, Vector3.Up);
            WarehouseCameraScript script = (WarehouseCameraScript)camera.AddComponent<WarehouseCameraScript>();
            script.SetPosition(new Vector3(0, 3, 18), new Vector3(0, 1.5f, 2.2f));
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180 / 7f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.AutoExposureEnabled = false;
            camera.Camera.PostProcess.ToneMapping.LensExposure = -0.5f;
            camera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color;
            camera.Camera.PostProcess.MLAA.Enabled = true;
            camera.Camera.PostProcess.Bloom.Enabled = true;
            camera.Camera.PostProcess.Bloom.Threshold = 3f;
            camera.Camera.PostProcess.FilmGrain.Enabled = true;
            camera.Camera.PostProcess.FilmGrain.Strength = 0.15f;
            /*camera.Camera.PostProcess.AdjustLevels.Enabled = true;
            camera.Camera.PostProcess.AdjustLevels.InputGamma = 0.9f;
            camera.Camera.PostProcess.AdjustLevels.InputWhite = 0.95f;*/
            camera.Camera.PostProcess.AnamorphicLensFlare.Enabled = false;
            camera.Camera.PostProcess.ColorCorrection.Enabled = true;
            camera.Camera.PostProcess.ColorCorrection.FirstLookupTable = new LookupTable("LookupTableWarehouse"); // A little more red and blue.
            camera.Camera.AmbientLight = new AmbientLight
            {
                SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, }),
                Color = new Color(10, 10, 10),
                Intensity = 8f,
                AmbientOcclusionStrength = 1.5f
            };
            //camera.Camera.Sky = new Skydome { Texture = new Texture("HotPursuitSkydome") };
            //camera.Camera.Sky = new Skybox { TextureCube = new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, } };
            #if !XBOX
                camera.Camera.AmbientLight.AmbientOcclusion = new HorizonBasedAmbientOcclusion
                {
                    NumberSteps = 12,
                    NumberDirections = 12,
                    Radius = 0.003f, // Bigger values produce more cache misses and you don’t want GPU cache misses, trust me.
                    LineAttenuation = 1.0f,
                    Contrast = 1.1f,
                    AngleBias = 0.1f,
                    Quality = HorizonBasedAmbientOcclusion.QualityType.HighQuality,
                    TextureSize = Size.TextureSize.HalfSize,
                };
            #endif

            #region Test Split Screen
            /*
            camera.Camera.NormalizedViewport = new RectangleF(0, 0, 1, 0.5f);
            camera2 = new GameObject3D();
            camera2.AddComponent<Camera>();
            camera2.Camera.MasterCamera = camera.Camera;
            camera2.Camera.ClearColor = Color.Black;
            camera2.Camera.FieldOfView = 180 / 8.0f;
            camera2.Camera.NormalizedViewport = new RectangleF(0, 0.5f, 1, 0.5f);
            camera2.Transform.LookAt(new Vector3(-5, 5, -15), new Vector3(0, 3, 0), Vector3.Up);
            camera2.Camera.AmbientLight = camera.Camera.AmbientLight;
            camera2.Camera.PostProcess = new PostProcess();
            camera2.Camera.PostProcess.ToneMapping.AutoExposureEnabled = true;
            camera2.Camera.PostProcess.ToneMapping.LensExposure = 0.5f;
            camera2.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            camera2.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color;
            camera2.Camera.PostProcess.MLAA.Enabled = true;
            camera2.Camera.PostProcess.Bloom.Enabled = true;
            camera2.Camera.PostProcess.Bloom.Threshold = 3f;
            camera2.Camera.PostProcess.FilmGrain.Enabled = true;
            camera2.Camera.PostProcess.FilmGrain.Strength = 0.15f;
            */
            #endregion

            #endregion
            
            #region Models
            
            #if XBOX
                warehouseWalls = new GameObject3D(new FileModel("Warehouse\\WarehouseWallsXbox"),
                                                  new BlinnPhong
                                                      {
                                                          DiffuseTexture = new Texture("Warehouse\\Warehouse-DiffuseXbox"),
                                                          SpecularTexture = new Texture("Warehouse\\Warehouse-SpecularXbox"),
                                                          SpecularIntensity = 3,
                                                          SpecularPower = 30000,
                                                      });
                warehouseRoof  = new GameObject3D(new FileModel("Warehouse\\WarehouseRoofXbox"),  
                                                  new BlinnPhong
                                                      {
                                                          DiffuseTexture = new Texture("Warehouse\\MetalRoof-DiffuseXbox")
                                                      });
                warehouseRoof1 = new GameObject3D(new FileModel("Warehouse\\WarehouseRoof1Xbox"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\MetalRoof2-DiffuseXbox")
                                                  });

                warehouseWood = new GameObject3D(new FileModel("Warehouse\\WarehouseWoodXbox"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("Warehouse\\Wood-DiffuseXbox")
                                                 });

                warehouseWood2 = new GameObject3D(new FileModel("Warehouse\\WarehouseWood2Xbox"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\Wood2-DiffuseXbox")
                                                  });

                warehouseBrokenWindow = new GameObject3D(new FileModel("Warehouse\\WarehouseBrokenWindowXbox"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("Warehouse\\Window-DiffuseXbox")
                                                 });

                warehouseWindow = new GameObject3D(new FileModel("Warehouse\\WarehouseWindowXbox"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\Window-DiffuseXbox")
                                                  });
                warehouseGround = new GameObject3D(new FileModel("Warehouse\\WarehouseGroundXbox"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\Ground-DiffuseXbox"),
                                                      NormalTexture = new Texture("Warehouse\\Ground-NormalsXbox"),
                                                      SpecularIntensity = 0.7f
                                                  });
            #else
                warehouseWalls = new GameObject3D(new FileModel("Warehouse\\WarehouseWalls"),
                                                  new BlinnPhong
                                                      {
                                                          DiffuseTexture = new Texture("Warehouse\\Warehouse-Diffuse"),
                                                          SpecularTexture = new Texture("Warehouse\\Warehouse-Specular"),
                                                          SpecularIntensity = 3,
                                                          SpecularPower = 30000,
                                                      });
                warehouseRoof  = new GameObject3D(new FileModel("Warehouse\\WarehouseRoof"),  
                                                  new BlinnPhong
                                                      {
                                                          DiffuseTexture = new Texture("Warehouse\\MetalRoof-Diffuse")
                                                      });
                warehouseRoof1 = new GameObject3D(new FileModel("Warehouse\\WarehouseRoof1"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\MetalRoof2-Diffuse")
                                                  });

                warehouseWood = new GameObject3D(new FileModel("Warehouse\\WarehouseWood"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("Warehouse\\Wood-Diffuse")
                                                 });

                warehouseWood2 = new GameObject3D(new FileModel("Warehouse\\WarehouseWood2"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\Wood2-Diffuse")
                                                  });

                warehouseBrokenWindow = new GameObject3D(new FileModel("Warehouse\\WarehouseBrokenWindow"),
                                                 new BlinnPhong
                                                 {
                                                     DiffuseTexture = new Texture("Warehouse\\Window-Diffuse")
                                                 });

                warehouseWindow = new GameObject3D(new FileModel("Warehouse\\WarehouseWindow"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\Window-Diffuse")
                                                  });
                warehouseGround = new GameObject3D(new FileModel("Warehouse\\WarehouseGround"),
                                                  new BlinnPhong
                                                  {
                                                      DiffuseTexture = new Texture("Warehouse\\Ground-Diffuse"),
                                                      NormalTexture = new Texture("Warehouse\\Ground-Normals"),
                                                      SpecularIntensity = 0.7f
                                                  });
            #endif

            #endregion
            
            #region Shadows and Lights

            Shadow.DistributeShadowCalculationsBetweenFrames = true;
            
            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.Color = new Color(190, 190, 150);
            directionalLight.DirectionalLight.Intensity = 9.2f;
            directionalLight.Transform.LookAt(new Vector3(0.3f, 0.95f, -0.3f), Vector3.Zero, Vector3.Forward);
            DirectionalLight.Sun = directionalLight.DirectionalLight;
            directionalLight.DirectionalLight.Shadow = new CascadedShadow
            {
                Filter = Shadow.FilterType.PcfPosion,
                LightDepthTextureSize = Size.Square512X512,
                ShadowTextureSize = Size.TextureSize.FullSize, // Lower than this could produce artifacts if the light is too intense.
                DepthBias = 0.0025f,
                FarPlaneSplit1 = 15,
                FarPlaneSplit2 = 40,
                FarPlaneSplit3 = 100,
                //FarPlaneSplit4 = 150
            };
            /*directionalLight.DirectionalLight.Shadow = new BasicShadow
            {
                Filter = Shadow.FilterType.PcfPosion,
                LightDepthTextureSize = Size.Square512X512,
                ShadowTextureSize = Size.TextureSize.FullSize, // Lower than this could produce artifacts if the light is to intense.
                DepthBias = 0.0025f,
                Range = 100,
            };*/
            
            pointLight = new GameObject3D();
            pointLight.AddComponent<PointLight>();
            pointLight.PointLight.Color = new Color(180, 190, 230);
            pointLight.PointLight.Intensity = 0.3f;
            pointLight.PointLight.Range = 60;
            pointLight.Transform.Position = new Vector3(4.8f, 1.5f, 10);
            //pointLight.PointLight.Shadow = new CubeShadow { LightDepthTextureSize = 512, };

            pointLight = new GameObject3D();
            pointLight.AddComponent<PointLight>();
            pointLight.PointLight.Color = new Color(130, 130, 190);
            pointLight.PointLight.Intensity = 0.7f;
            pointLight.PointLight.Range = 60;
            pointLight.Transform.Position = new Vector3(-4.8f, 0, -4);

            /*pointLightClipVolumeTest = new GameObject3D();
            pointLightClipVolumeTest.AddComponent<PointLight>();
            pointLightClipVolumeTest.PointLight.Color = new Color(250, 20, 20);
            pointLightClipVolumeTest.PointLight.Intensity = 2f;
            pointLightClipVolumeTest.PointLight.Range = 60;
            pointLightClipVolumeTest.PointLight.ClipVolume = new FileModel("ClipVolume");
            pointLightClipVolumeTest.PointLight.RenderClipVolumeInLocalSpace = true;
            pointLightClipVolumeTest.Transform.Position = new Vector3(26f, 7, 35);*/

            spotLight = new GameObject3D();
            spotLight.AddComponent<SpotLight>();
            spotLight.SpotLight.Color = new Color(0, 250, 0);
            spotLight.SpotLight.Intensity = 0.5f;
            spotLight.SpotLight.Range = 40; // I always forget to set the light range lower than the camera far plane.
            spotLight.Transform.Position = new Vector3(0, 16, 15);
            spotLight.Transform.Rotate(new Vector3(-80, 0, 0));
            spotLight.SpotLight.LightMaskTexture = new Texture("LightMasks\\TestLightMask");
            /*spotLight.SpotLight.Shadow = new BasicShadow
            {
                Filter = Shadow.FilterType.PcfPosion,
                LightDepthTextureSize = Size.Square512X512,
                ShadowTextureSize = Size.TextureSize.FullSize,
                DepthBias = 0.0005f,
            };*/
            
            #endregion
            
            #region Lamborghini Murcielago LP640
           
            lamborghiniMurcielagoLoader = new LamborghiniMurcielagoLoader();
            lamborghiniMurcielagoLoader.LoadContent();
            lamborghiniMurcielagoLoader.LamborghiniMurcielago.Transform.LocalScale = new Vector3(1.2f, 1.2f, 1.2f);
            lamborghiniMurcielagoLoader.LamborghiniMurcielago.Transform.LocalPosition = new Vector3(0, 1.4f, 2.35f);
            lamborghiniMurcielagoLoader.TiresAngle = -40;
            
            #endregion

            #region Particles
            
            // Pit particles
            GameObject3D particles = new GameObject3D();
            particles.AddComponent<ParticleEmitter>();
            particles.AddComponent<ParticleRenderer>();
            particles.ParticleEmitter.MaximumNumberParticles = 75;
            particles.Transform.LocalPosition = new Vector3(-1.2f, 0, -13);
            particles.ParticleEmitter.Duration = 50f;
            particles.ParticleEmitter.EmitterVelocitySensitivity = 1;
            particles.ParticleEmitter.MinimumHorizontalVelocity = 1;
            particles.ParticleEmitter.MaximumHorizontalVelocity = 2f;
            particles.ParticleEmitter.MinimumVerticalVelocity = 1;
            particles.ParticleEmitter.MaximumVerticalVelocity = 2;
            particles.ParticleRenderer.Texture = new Texture("Particles\\Smoke");
            particles.ParticleRenderer.SoftParticles = true;
            particles.ParticleRenderer.FadeDistance = 25.0f;
            particles.ParticleRenderer.BlendState = BlendState.NonPremultiplied;
            particles.ParticleRenderer.DurationRandomness = 0.1f;
            particles.ParticleRenderer.Gravity = new Vector3(0, 1, 5);
            particles.ParticleRenderer.EndVelocity = 0.75f;
            particles.ParticleRenderer.MinimumColor = Color.Red;
            particles.ParticleRenderer.MaximumColor = Color.White;
            particles.ParticleRenderer.RotateSpeed = new Vector2(-0.3f, 0.25f);
            particles.ParticleRenderer.StartSize = new Vector2(25, 30);
            particles.ParticleRenderer.EndSize = new Vector2(75, 100);
            particles.ParticleRenderer.TilesX = 1;
            particles.ParticleRenderer.TilesY = 1;
            particles.ParticleRenderer.AnimationRepetition = 1;

            particles = new GameObject3D();
            particles.AddComponent<ParticleEmitter>();
            particles.AddComponent<ParticleRenderer>();
            particles.ParticleEmitter.MaximumNumberParticles = 75;
            particles.Transform.LocalPosition = new Vector3(-3.2f, 0, 13);
            particles.ParticleEmitter.Duration = 50f;
            particles.ParticleEmitter.EmitterVelocitySensitivity = 1;
            particles.ParticleEmitter.MinimumHorizontalVelocity = 1;
            particles.ParticleEmitter.MaximumHorizontalVelocity = 2f;
            particles.ParticleEmitter.MinimumVerticalVelocity = 1;
            particles.ParticleEmitter.MaximumVerticalVelocity = 2;
            particles.ParticleRenderer.Texture = new Texture("Particles\\Smoke");
            particles.ParticleRenderer.SoftParticles = true;
            particles.ParticleRenderer.FadeDistance = 25.0f;
            particles.ParticleRenderer.BlendState = BlendState.NonPremultiplied;
            particles.ParticleRenderer.DurationRandomness = 0.1f;
            particles.ParticleRenderer.Gravity = new Vector3(0, 1, -5);
            particles.ParticleRenderer.EndVelocity = 0.75f;
            particles.ParticleRenderer.MinimumColor = Color.Red;
            particles.ParticleRenderer.MaximumColor = Color.White;
            particles.ParticleRenderer.RotateSpeed = new Vector2(-0.2f, 0.2f);
            particles.ParticleRenderer.StartSize = new Vector2(25, 50);
            particles.ParticleRenderer.EndSize = new Vector2(75, 100);
            particles.ParticleRenderer.TilesX = 1;
            particles.ParticleRenderer.TilesY = 1;
            particles.ParticleRenderer.AnimationRepetition = 1;
            
            #endregion

            #region Legend

            demoLegend = new GameObject2D();
            var legend = (HudText)demoLegend.AddComponent<HudText>();
            legend.Color = new Color(0.1f, 0.1f, 0.1f, 1f);
            #if XBOX
                legend.Text.Append("A to open the doors\n");
                legend.Text.Append("Left stick to rotate the wheels");
            #else
                legend.Text.Append("Space or A to open the doors\n");
                legend.Text.Append("Left and right cursors or left stick to rotate the wheels\n");
                legend.Text.Append("Left mouse button or right stick to move the camera");
            #endif

            #endregion
            
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        protected override void UpdateTasks()
        {
            // This is intended to be used in Xbox tests.
            //if (XNAFinalEngine.Input.GamePad.PlayerOne.BackJustPressed)
            //    throw new Exception("Quick exit in Xbox 360 tests.");

            demoLegend.Transform.Position = new Vector3(20, Screen.Height - 50f, 0f);
            
            #region Doors

            if (Keyboard.KeyJustPressed(Keys.Space) || XNAFinalEngine.Input.GamePad.PlayerOne.AJustPressed)
            {
                openingDoors = !openingDoors;
            }
            if (openingDoors)
            {
                lamborghiniMurcielagoLoader.LeftDoorAngle += Time.SmoothFrameTime * 30;
                lamborghiniMurcielagoLoader.RightDoorAngle += Time.SmoothFrameTime * 30;
            }
            else
            {
                lamborghiniMurcielagoLoader.LeftDoorAngle -= Time.SmoothFrameTime * 30;
                lamborghiniMurcielagoLoader.RightDoorAngle -= Time.SmoothFrameTime * 30;
            }

            #endregion

            #region Tires

            if (Keyboard.KeyPressed(Keys.Left) || XNAFinalEngine.Input.GamePad.PlayerOne.LeftStickX < 0)
            {
                lamborghiniMurcielagoLoader.TiresAngle += Time.SmoothFrameTime * 80;
            }
            if (Keyboard.KeyPressed(Keys.Right) || XNAFinalEngine.Input.GamePad.PlayerOne.LeftStickX > 0)
            {
                lamborghiniMurcielagoLoader.TiresAngle -= Time.SmoothFrameTime * 80;
            }

            #endregion
            
        } // UpdateTasks

        #endregion
        
    } // WarehouseScene
} // XNAFinalEngineExamples