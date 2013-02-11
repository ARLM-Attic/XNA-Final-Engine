
#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Animations;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Input;
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
    public class AnimationScene : Scene
    {

        #region Variables
        
        // Now every entity is a game object and the entity’s behavior is defined by the components attached to it.
        // There are several types of components, components related to models, to sound, to particles, to physics, etc.
        private GameObject3D floor, dude, rifle,
                             // Lights
                             directionalLight,
                             // Cameras
                             camera;

        private float animating = -1;

        // Input
        private Button[] buttons;
        private Axis axis;
        
        #endregion

        #region Load
        
        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        protected override void LoadContent()
        {

            #region Setup Input Controls

            buttons = new Button[4];

            // Create the virtual buttons to control the scene selection.
            buttons[0] = new Button
            {
                Name = "Attack",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Keys.Space),
            };
            buttons[1] = new Button
            {
                Name = "Attack",
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput,
                KeyButton = new KeyButton(Buttons.RightTrigger),
            };
            axis = new Axis
            {
                Name = "Walk",
                AnalogAxis = AnalogAxes.LeftStickY,
                AxisBehavior = Axis.AxisBehaviors.AnalogInput,
            };
            buttons[2] = new Button
            {
                Name = "WalkPC",
                KeyButton = new KeyButton(Keys.W),
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput
            };
            buttons[3] = new Button
            {
                Name = "RunPC",
                KeyButton = new KeyButton(Keys.LeftShift),
                ButtonBehavior = Button.ButtonBehaviors.DigitalInput
            };

            #endregion

            #region Camera

            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.AddComponent<SoundListener>();
            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 100;
            camera.Camera.NearPlane = 1f;
            camera.Transform.LookAt(new Vector3(-10, 10, 20), new Vector3(0, 5, 0), Vector3.Up);
            //ScriptCustomCameraScript script = (ScriptCustomCameraScript)camera.AddComponent<ScriptCustomCameraScript>();
            //script.SetPosition(new Vector3(0, 10, 20), new Vector3(0, 5, 0));
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180 / 6f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.AutoExposureEnabled = true;
            camera.Camera.PostProcess.MLAA.Enabled = true;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color;
            camera.Camera.PostProcess.Bloom.Threshold = 3.5f;
            camera.Camera.AmbientLight = new AmbientLight
            {
                SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeTexture(new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, }),
                Color = new Color(10, 10, 10),
                Intensity = 5f,
            };
            //camera.Camera.Sky = new Skybox { TextureCube = new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, } };
            
            #endregion

            #region Models

            dude = new GameObject3D(new FileModel("DudeWalk"), new BlinnPhong());
            // One material for mesh part.
            dude.ModelRenderer.MeshMaterials = new Material[5];
            dude.ModelRenderer.MeshMaterials[0] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\head"), NormalTexture = new Texture("Dude\\headN"), SpecularTexture = new Texture("Dude\\headS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterials[2] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\jacket"), NormalTexture = new Texture("Dude\\jacketN"), SpecularTexture = new Texture("Dude\\jacketS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterials[3] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\pants"), NormalTexture = new Texture("Dude\\pantsN"), SpecularTexture = new Texture("Dude\\pantsS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterials[1] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\upBodyC"), NormalTexture = new Texture("Dude\\upbodyN"), SpecularTexture = new Texture("Dude\\upbodyCS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.ModelRenderer.MeshMaterials[4] = new BlinnPhong { DiffuseTexture = new Texture("Dude\\upBodyC"), NormalTexture = new Texture("Dude\\upbodyN"), SpecularTexture = new Texture("Dude\\upbodyCS"), SpecularPowerFromTexture = false, SpecularPower = 300 };
            dude.Transform.LocalScale = new Vector3(0.1f, 0.1f, 0.1f);
            // Add animations.
            dude.AddComponent<ModelAnimations>();
            ModelAnimation modelAnimation;
            // First DudeAttack.
            #if XBOX
                modelAnimation = new ModelAnimation("DudeAttackXbox"); // Be aware to select the correct content processor when you add your fbx files on the solution.
                modelAnimation.WrapMode = WrapMode.ClampForever;
                dude.ModelAnimations.AddAnimationClip(modelAnimation);
            #else
                modelAnimation = new ModelAnimation("DudeAttack"); // Be aware to select the correct content processor when you add your fbx files on the solution.
                modelAnimation.WrapMode = WrapMode.ClampForever;
                dude.ModelAnimations.AddAnimationClip(modelAnimation);
            #endif
            // Then DudeRun.
            #if XBOX
                modelAnimation = new ModelAnimation("DudeRunXbox"); // Be aware to select the correct content processor when you add your fbx files on the solution.
                modelAnimation.WrapMode = WrapMode.Loop;
                dude.ModelAnimations.AddAnimationClip(modelAnimation);
            #else
                modelAnimation = new ModelAnimation("DudeRun"); // Be aware to select the correct content processor when you add your fbx files on the solution.
                modelAnimation.WrapMode = WrapMode.Loop;
                dude.ModelAnimations.AddAnimationClip(modelAnimation);
            #endif
            // Set the parameters from the animation that it is stored in DudeWalk (this animation is added automatically by the system).
            dude.ModelAnimations["Take 001"].WrapMode = WrapMode.Loop;
            
            // Load the rifle model.
            rifle = new GameObject3D(new FileModel("Rifle"), new BlinnPhong { DiffuseColor = new Color(15, 15, 15), SpecularPower = 100, SpecularIntensity = 0.01f});
            
            // And a floor
            floor = new GameObject3D(new FileModel("Terrain/TerrainLOD0Grid"),
                new BlinnPhong
                {
                    SpecularPower = 300,
                    DiffuseColor = new Color(250, 250, 250),
                    SpecularIntensity = 0.0f,
                })
            { Transform = { LocalScale = new Vector3(5, 5, 5) } };
            
            #endregion

            #region Shadows and Lights

            Shadow.DistributeShadowCalculationsBetweenFrames = true;

            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.Color = new Color(250, 250, 140);
            directionalLight.DirectionalLight.Intensity = 25f;
            directionalLight.Transform.LookAt(new Vector3(-0.75f, 0.85f, -1.3f), Vector3.Zero, Vector3.Forward);
            directionalLight.DirectionalLight.Shadow = new CascadedShadow
            {
                Filter = Shadow.FilterType.PcfPosion,
                LightDepthTextureSize = Size.Square1024X1024,
            };
            DirectionalLight.Sun = directionalLight.DirectionalLight;

            #endregion

            #region Dog Fight Studios

            // Dog Fight Studios share their animations and the scene logic.
            GameObject2D dogFightStudios = new GameObject2D();
            dogFightStudios.AddComponent<HudTexture>();
            dogFightStudios.HudTexture.Texture = new Texture("DogFightStudios");
            dogFightStudios.Transform.Position = new Vector3(-55, -80, 0);

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
            // A simple logic to change between the different states.
            if (Button.Pressed("Attack"))
            {
                if (animating <= 0 || animating > 0.05f)
                {
                    animating = 0;
                    #if XBOX
                        dude.ModelAnimations["DudeAttackXbox"].WrapMode = WrapMode.ClampForever;
                        dude.ModelAnimations.CrossFade("DudeAttackXbox", 0.4f);
                        if (dude.ModelAnimations["DudeAttackXbox"].NormalizedTime > 0.5f)
                            dude.ModelAnimations.Rewind("DudeAttackXbox");
                    #else
                        dude.ModelAnimations["DudeAttack"].WrapMode = WrapMode.ClampForever;
                        dude.ModelAnimations.CrossFade("DudeAttack", 0.4f);
                        if (dude.ModelAnimations["DudeAttack"].NormalizedTime > 0.5f)
                            dude.ModelAnimations.Rewind("DudeAttack");
                    #endif
                }
            }
            else
            {
                if (animating <= 0 || animating > 0.35f)
                {
                    if (Axis.Value("Walk") > 0.8 || (Button.Pressed("RunPC") && Button.Pressed("WalkPC")))
                    {
                        animating = 0;
                        #if XBOX
                            dude.ModelAnimations.CrossFade("DudeRunXbox", 0.3f);
                        #else
                            dude.ModelAnimations.CrossFade("DudeRun", 0.3f);
                        #endif
                    }
                    else
                    {
                        if (Axis.Value("Walk") > 0 || Button.Pressed("WalkPC"))
                        {
                            animating = 0;
                            dude.ModelAnimations.CrossFade("Take 001", 0.2f);
                        }
                        else
                        {
                            animating = 0;
                            #if XBOX
                                dude.ModelAnimations.CrossFade("DudeAttackXbox", 0.35f);
                                dude.ModelAnimations["DudeAttackXbox"].WrapMode = WrapMode.ClampForever;
                            #else
                                dude.ModelAnimations.CrossFade("DudeAttack", 0.35f);
                                dude.ModelAnimations["DudeAttack"].WrapMode = WrapMode.ClampForever;
                            #endif
                        }
                    }
                }
            }

            if (animating != -1)
                animating += Time.GameDeltaTime;              
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
        protected override void PreRenderTasks()
        {
            if (Keyboard.KeyPressed(Keys.T))
            {
                dude.ModelAnimations.LocalBoneTransforms[7] = dude.ModelAnimations.LocalBoneTransforms[7] * Matrix.CreateFromYawPitchRoll(0.3f, 0, 0);
                dude.ModelAnimations.UpdateLocalBoneTransforms();
            }
            if (Keyboard.KeyPressed(Keys.G))
            {
                dude.ModelAnimations.LocalBoneTransforms[7] = dude.ModelAnimations.LocalBoneTransforms[7] * Matrix.CreateFromYawPitchRoll(-0.3f, 0, 0);
                dude.ModelAnimations.UpdateLocalBoneTransforms();
            }

            // Rifle placement.
            // The rifle matchs the 30th bone transformation.
            // But before I made some matrix modifications because of a problem with the content creation tool, a problem not related with the engine.
            rifle.Transform.LocalMatrix = Matrix.Identity;
            rifle.Transform.Translate(new Vector3(1.2767f, 0.5312f, 0.0045f));
            rifle.Transform.Rotate(new Vector3(0, 45, 90));
            rifle.Transform.Translate(new Vector3(0, 1.1081f, -0.3243f));
            rifle.Transform.Rotate(new Vector3(0, 45, 0));
            rifle.Transform.LocalScale = new Vector3(16, 16, 16f);
            rifle.Transform.LocalMatrix = rifle.Transform.LocalMatrix * dude.ModelAnimations.WorldBoneTransforms[30] * dude.Transform.WorldMatrix;
        } // PreRenderTasks

        #endregion

        #region Unload Content

        /// <summary>
        /// Called when the scene resources need to be unloaded.
        /// Override this method to unload any game-specific resources.
        /// But remember that every asset load in the scene asset content manager
        /// and every game object load in the scene game object content manager
        /// will be disposed automatically.
        /// </summary>
        protected override void UnloadContent()
        {
            axis.Dispose();
            foreach (Button button in buttons)
            {
                button.Dispose();
            }
        } // UnloadContent

        #endregion

    } // AnimationScene
} // XNAFinalEngineExamples
