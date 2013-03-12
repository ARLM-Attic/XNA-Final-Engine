
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
    /// Dog Fight Studios Tech Demo scene.
    /// </summary>
    public class TechDemoScene : Scene
    {

        #region Variables
        
        // Now every entity is a game object and the entity’s behavior is defined by the components attached to it.
        // There are several types of components, components related to models, to sound, to particles, to physics, etc.
        private GameObject3D // Models
                             building01, warehouseGround, townhall,
                             // Lights
                             directionalLight, pointLight, pointLight2, spotLight,
                             // Cameras
                             camera,
                             skydome;
        
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
            camera.Camera.FarPlane = 1000;
            camera.Camera.NearPlane = 1f; // Do not place a small value here, you can destroy performance, not just precision.
            camera.Transform.LookAt(new Vector3(0, 0, 15), Vector3.Zero, Vector3.Up);
            ScriptCustomCameraScript script = (ScriptCustomCameraScript)camera.AddComponent<ScriptCustomCameraScript>();
            script.SetPosition(new Vector3(182, -85.47f, 150), new Vector3(0, 26f, 0));
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180 / 7f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.AutoExposureEnabled = true;
            camera.Camera.PostProcess.ToneMapping.LensExposure = -1.5f;
            camera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Both;
            camera.Camera.PostProcess.MLAA.ThresholdColor = 0.1f;
            camera.Camera.PostProcess.MLAA.ThresholdDepth = 0.03f;
            camera.Camera.PostProcess.MLAA.Enabled = true;
            camera.Camera.PostProcess.Bloom.Enabled = true;
            camera.Camera.PostProcess.Bloom.Threshold = 3f;
            camera.Camera.PostProcess.FilmGrain.Enabled = true;
            camera.Camera.PostProcess.FilmGrain.Strength = 0.1f;
            camera.Camera.PostProcess.AnamorphicLensFlare.Enabled = false;
            camera.Camera.PostProcess.ColorCorrection.Enabled = true;
            camera.Camera.PostProcess.ColorCorrection.FirstLookupTable = new LookupTable("LookupTableWarehouse"); // A little more red and blue.
            camera.Camera.AmbientLight = new AmbientLight
            {
                //SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeTexture(new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, }),
                //SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromLatLongTexture(new Texture("CubeTextures\\Arches_E_PineTree_3k")),
                SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromLatLongTexture(new Texture("CubeTextures\\Desert-15_XXLResize")),
                //SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromLatLongTexture(new Texture("CubeTextures\\DH102LL")),
                //SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromLatLongTexture(new Texture("CubeTextures\\Factory_Catwalk_2k")),
                Color = new Color(0, 0, 0),
                Intensity = 5f,
                AmbientOcclusionStrength = 2.5f
            };
            camera.Camera.Sky = new Skydome { Texture = new Texture("HotPursuitSkydome") };
            //camera.Camera.Sky = new Skybox { TextureCube = new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, }, ColorIntensity = 15 };
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
            #endregion
            
            #region Models

            warehouseGround = new GameObject3D(new FileModel("Warehouse\\WarehouseGround"),
                                      new BlinnPhong
                                      {
                                          DiffuseTexture = new Texture("Warehouse\\Ground-Diffuse"),
                                          NormalTexture = new Texture("Warehouse\\Ground-Normals"),
                                          SpecularIntensity = 0.7f
                                      });
            warehouseGround.Transform.LocalScale = new Vector3(10, 10, 10);
            warehouseGround.Transform.Translate(0, -35.5f, 0);

            float specularIntensity = 2;
            float specularPower = 500;

            townhall = new GameObject3D(new FileModel("Tech Demo\\XNA_TownHall"),
                                            new BlinnPhong
                                            {
                                                DiffuseColor = Color.White,
                                                SpecularIntensity = 3,
                                                SpecularPower = 300,
                                            });
            townhall.Transform.Rotate(new Vector3(-90, -90, 0));
            townhall.Transform.Translate(40, -26f, -260, Space.World);
            townhall.ModelRenderer.MeshMaterials = new Material[5];
            townhall.ModelRenderer.MeshMaterials[0] = new BlinnPhong
            {
                DiffuseTexture = new Texture("Tech Demo\\XNA_building02_wall04"),
                NormalTexture = new Texture("Tech Demo\\XNA_building02_wall04_N"),
                SpecularIntensity = specularIntensity,
                SpecularPower = specularPower,
            };
            townhall.ModelRenderer.MeshMaterials[1] = new BlinnPhong
            {
                DiffuseTexture = new Texture("Tech Demo\\XNA_building02_roff02"),
                NormalTexture = new Texture("Tech Demo\\XNA_building02_roff02_N"),
                SpecularIntensity = specularIntensity,
                SpecularPower = specularPower,
            };
            townhall.ModelRenderer.MeshMaterials[2] = new BlinnPhong
            {
                DiffuseTexture = new Texture("Tech Demo\\XNA_building02_wall03"),
                NormalTexture = new Texture("Tech Demo\\XNA_building02_wall03_N"),
                SpecularPowerFromTexture = false,
                SpecularIntensity = specularIntensity,
                SpecularPower = specularPower,
            };
            townhall.ModelRenderer.MeshMaterials[3] = new BlinnPhong
            {
                DiffuseTexture = new Texture("Tech Demo\\XNA_building02_wall05"),
                NormalTexture = new Texture("Tech Demo\\XNA_building02_wall05_N"),
                SpecularPowerFromTexture = false,
                SpecularIntensity = specularIntensity,
                SpecularPower = specularPower,
            };
            townhall.ModelRenderer.MeshMaterials[4] = new BlinnPhong
            {
                DiffuseTexture = new Texture("Tech Demo\\XNA_building02_window01"),
                NormalTexture = new Texture("Tech Demo\\XNA_building02_window01_N"),
                SpecularPowerFromTexture = false,
                SpecularIntensity = specularIntensity,
                SpecularPower = specularPower,
            };

            building01 = new GameObject3D(new FileModel("Tech Demo\\building01"),
                                new BlinnPhong
                                {
                                    DiffuseColor = Color.White,
                                    SpecularIntensity = 3,
                                    SpecularPower = 300,
                                });
            building01.Transform.Rotate(new Vector3(-90, 0, 0));
            building01.ModelRenderer.MeshMaterials = new Material[23];
            building01.ModelRenderer.MeshMaterials[0] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_groundfloor01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_groundfloor01_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_groundfloor01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[1] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_groundfloor02_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_groundfloor02_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_groundfloor02_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[2] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_wall01_D"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[3] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_brick02_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_brick02_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[4] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_brick03_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_brick03_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[5] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_brick01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_brick01_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[6] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_brick04_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_brick01_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[7] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_window03_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_window03_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[8] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_window02_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_window02_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_window02_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[9] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_shop01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_shop01_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[10] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_window01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_window01_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[11] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_shop01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_shop01_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[12] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_Shutter-doors01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_Shutter-doors01_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_Shutter-doors01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[13] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_Shutter-doors03_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_Shutter-doors03_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_Shutter-doors03_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[14] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_Shutter-doors02_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_Shutter-doors02_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_Shutter-doors02_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[15] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_bill01_D"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_bill01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[16] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_Watertower01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_Watertower01_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_Watertower01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[17] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_Shutter-doors01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_Shutter-doors01_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_Shutter-doors01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[18] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_AirCondition01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_AirCondition01_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_AirCondition01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[19] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_flaps01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_flaps01_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_flaps01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[20] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_rainshed01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_rainshed01_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[21] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_iron01_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_iron01_N"),
                                                                SpecularTexture = new Texture("Tech Demo\\building01_iron01_S"),
                                                                SpecularPowerFromTexture = false,
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            building01.ModelRenderer.MeshMaterials[22] = new BlinnPhong
                                                            {
                                                                DiffuseTexture = new Texture("Tech Demo\\building01_rainshed02_D"),
                                                                NormalTexture = new Texture("Tech Demo\\building01_rainshed02_N"),
                                                                SpecularIntensity = specularIntensity,
                                                                SpecularPower = specularPower,
                                                            };
            #endregion

            #region Statistics

            //GameObject2D statistics = new GameObject2D();
            //statistics.AddComponent<ScriptStatisticsDrawer>();

            #endregion
            
            #region Shadows and Lights

            Shadow.DistributeShadowCalculationsBetweenFrames = true;
            
            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.Color = new Color(190, 190, 150);
            directionalLight.DirectionalLight.Intensity = 25.2f;
            directionalLight.Transform.LookAt(new Vector3(0.3f, 0.5f, -0.5f), Vector3.Zero, Vector3.Forward);
            DirectionalLight.Sun = directionalLight.DirectionalLight;
            directionalLight.DirectionalLight.Shadow = new CascadedShadow
            {
                Filter = Shadow.FilterType.Pcf7X7,
                LightDepthTextureSize = Size.Square1024X1024,
                ShadowTextureSize = Size.TextureSize.FullSize, // Lower than this could produce artifacts if the light is too intense.
                DepthBias = 0.0025f,
                FarPlaneSplit1 = 200,
                FarPlaneSplit2 = 300,
                FarPlaneSplit3 = 500,
                FarPlaneSplit4 = 800
            };
            
            #endregion

            #region Particles
            /*
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
            particles.ParticleRenderer.MinimumColor = Color.White;
            particles.ParticleRenderer.MaximumColor = Color.White;
            particles.ParticleRenderer.RotateSpeed = new Vector2(-0.3f, 0.25f);
            particles.ParticleRenderer.StartSize = new Vector2(150, 300);
            particles.ParticleRenderer.EndSize = new Vector2(500, 750);
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
            particles.ParticleRenderer.Gravity = new Vector3(0, 1, -25);
            particles.ParticleRenderer.EndVelocity = 0.75f;
            particles.ParticleRenderer.MinimumColor = Color.White;
            particles.ParticleRenderer.MaximumColor = Color.White;
            particles.ParticleRenderer.RotateSpeed = new Vector2(-0.2f, 0.2f);
            particles.ParticleRenderer.StartSize = new Vector2(150, 500);
            particles.ParticleRenderer.EndSize = new Vector2(500, 750);
            particles.ParticleRenderer.TilesX = 1;
            particles.ParticleRenderer.TilesY = 1;
            particles.ParticleRenderer.AnimationRepetition = 1;
            */
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
            
        } // UpdateTasks

        #endregion

    } // TechDemoScene
} // XNAFinalEngineExamples