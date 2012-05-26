
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
using XNAFinalEngine.Animations;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Editor;
using XNAFinalEngine.Helpers;
using DirectionalLight = XNAFinalEngine.Components.DirectionalLight;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Size = XNAFinalEngine.Helpers.Size;
using Texture = XNAFinalEngine.Assets.Texture;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// Test scene to show skinning animation capabilities.
    /// </summary>
    public class DudeEditableScene : EditableScene
    {

        #region Variables
        
        // Now every entity is a game object and the entity’s behavior is defined by the components attached to it.
        // There are several types of components, components related to models, to sound, to particles, to physics, etc.
        private static GameObject3D 
                                    // Models
                                    floor,
                                    dude, animetedCube,
                                    // Lights
                                    directionalLight, pointLight, spotLight,
                                    // Cameras
                                    camera, camera2,
                                    skydome;

        private static GameObject2D statistics;
        
        #endregion

        #region Load
        
        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        public override void Load()
        {
            // Call it before anything.
            base.Load();

            #region Camera

            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.AddComponent<SoundListener>();
            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 5000;
            camera.Camera.NearPlane = 0.1f;
            camera.Transform.LookAt(new Vector3(5, 0, 15), Vector3.Zero, Vector3.Up);
            ScriptEditorCamera script = (ScriptEditorCamera)camera.AddComponent<ScriptEditorCamera>();
            script.SetPosition(new Vector3(0, 10, 20), new Vector3(0, 5, 0));
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180 / 6f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.MLAA.Enabled = true;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color;
            camera.Camera.PostProcess.Bloom.Threshold = 2;
            camera.Camera.AmbientLight = new AmbientLight
            {
                SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("FactoryCatwalkRGBM", true, 50)),
                //SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("Colors", false)),
                Color = new Color(50, 50, 50),
                Intensity = 5f,
                AmbientOcclusionStrength = 5f
            };
            //camera.Camera.Sky = new Skydome { Texture = new Texture("HotPursuitSkydome") };
            camera.Camera.Sky = new Skybox { TextureCube = new TextureCube("FactoryCatwalkRGBM", true, 50) };
            camera.Camera.AmbientLight.AmbientOcclusion = new HorizonBasedAmbientOcclusion
            {
                NumberSteps = 8, // Don't change this.
                NumberDirections = 12, // Don't change this.
                Radius = 0.000002f, // Bigger values produce more cache misses and you don’t want GPU cache misses, trust me.
                LineAttenuation = 0.75f,
                Contrast = 1.0f,
                AngleBias = 1.25f,
                Quality = HorizonBasedAmbientOcclusion.QualityType.HighQuality,
                TextureSize = Size.TextureSize.QuarterSize,
            };
            camera.Camera.PostProcess.FilmGrain.Enabled = false;

            #region Test Split Screen
            /*
            camera.Camera.NormalizedViewport = new RectangleF(0, 0, 1, 0.5f);
            camera2 = new GameObject3D();
            camera2.AddComponent<Camera>();
            camera2.Camera.MasterCamera = camera.Camera;
            camera2.Camera.ClearColor = Color.Black;
            camera2.Camera.FieldOfView = 180 / 8.0f;
            camera2.Camera.NormalizedViewport = new RectangleF(0, 0.5f, 1, 0.5f);
            camera2.Transform.LookAt(new Vector3(0, 0, 20), new Vector3(0, -2, 0), Vector3.Up);
            */
            #endregion

            #endregion

            #region Models

            #region Dude

            dude = new GameObject3D(new FileModel("DudeWalk"), null);
            dude.ModelRenderer.MeshMaterial = new Material[5];
            dude.ModelRenderer.MeshMaterial[0] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\head"), NormalTexture = new Texture("Dude\\headN"), SpecularTexture = new Texture("Dude\\headS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterial[1] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\jacket"), NormalTexture = new Texture("Dude\\jacketN"), SpecularTexture = new Texture("Dude\\jacketS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterial[2] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\pants"), NormalTexture = new Texture("Dude\\pantsN"), SpecularTexture = new Texture("Dude\\pantsS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterial[3] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\upBodyC"), NormalTexture = new Texture("Dude\\upbodyN"), SpecularTexture = new Texture("Dude\\upbodyCS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterial[4] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\upBodyC"), NormalTexture = new Texture("Dude\\upbodyN"), SpecularTexture = new Texture("Dude\\upbodyCS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.Transform.LocalScale = new Vector3(0.1f, 0.1f, 0.1f);
            dude.AddComponent<ModelAnimations>();
            //ModelAnimation modelAnimation = new ModelAnimation("dude"); // Be aware to select the correct content processor.
            //dude.ModelAnimations.AddAnimationClip(modelAnimation);
            dude.ModelAnimations.Play("Take 001");
            dude.Transform.Rotate(new Vector3(0, 180, 0));

            #endregion

            #region Floor

            floor = new GameObject3D(new FileModel("Terrain/TerrainLOD0Grid"),
                           new BlinnPhong
                           {
                               SpecularPower = 300,
                               DiffuseColor = new Color(125, 125, 125),
                               SpecularIntensity = 0.0f,
                           }) { Transform = { LocalScale = new Vector3(5, 5, 5) } };

            #endregion

            #endregion

            #region Shadows and Lights

            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.DiffuseColor = new Color(250, 250, 140);
            directionalLight.DirectionalLight.Intensity = 5f;
            directionalLight.Transform.LookAt(new Vector3(0.5f, 0.65f, 1.3f), Vector3.Zero, Vector3.Forward);
            directionalLight.DirectionalLight.Shadow = new CascadedShadow
            {
                Filter = Shadow.FilterType.PCF3x3,
                LightDepthTextureSize = Size.Square1024X1024,
                TextureSize = Size.TextureSize.HalfSize,
                Range = 50,
            };

            pointLight = new GameObject3D();
            pointLight.AddComponent<PointLight>();
            pointLight.PointLight.DiffuseColor = new Color(250, 0, 180);
            pointLight.PointLight.Intensity = 1f;
            pointLight.PointLight.Range = 100;
            pointLight.Transform.Position = new Vector3(-15, 50, 5);

            spotLight = new GameObject3D();
            spotLight.AddComponent<SpotLight>();
            spotLight.SpotLight.DiffuseColor = Color.Green;
            spotLight.SpotLight.Intensity = 300f;
            spotLight.SpotLight.Range = 40; // I always forget to set the light range lower than the camera far plane.
            spotLight.Transform.Position = new Vector3(0, 15f, -10);
            spotLight.Transform.Rotate(new Vector3(-45, 180, 0));
            spotLight.SpotLight.LightMaskTexture = new Texture("LightMasks\\Crysis2TestLightMask");
            /*spotLight.SpotLight.Shadow = new BasicShadow
            {
                Filter = Shadow.FilterType.PCF3x3,
                LightDepthTextureSize = Size.Square1024X1024,
                TextureSize = Size.TextureSize.HalfSize
            };*/

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
            if (Keyboard.SpaceJustPressed)
            {
                if (dude.ModelAnimations.State == AnimationState.Playing)
                    dude.ModelAnimations.Pause();
                else
                    dude.ModelAnimations.Resume();   
            }
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

    } // DudeEditableScene
} // XNAFinalEngineExamples
