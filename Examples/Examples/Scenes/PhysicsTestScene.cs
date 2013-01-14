#region License

/*
Copyright (c) 2008-2013, Schefer, Gustavo Martín.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

• Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

• The names of its contributors cannot be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schefer, Gustavo Martín (gusschefer@hotmail.com)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

using System;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nebula.Scripts;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Physics;
using Box = BEPUphysics.Entities.Prefabs.Box;
using DirectionalLight = XNAFinalEngine.Components.DirectionalLight;
using Sphere = BEPUphysics.Entities.Prefabs.Sphere;
using Keyboard = XNAFinalEngine.Input.Keyboard;
using Size = XNAFinalEngine.Helpers.Size;
using Space = XNAFinalEngine.Components.Space;


namespace XNAFinalEngineExamples
{
    class PhysicsTestScene : Scene
    {
        #region Varaibles
         
        private static GameObject3D camera, cube, obstacle;        
        private static GameObject2D statistics;

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        public override void LoadContent()
        {

            #region Physics Simulation Settings

            // Setup gravity
            PhysicsManager.Gravity = new Vector3(0, -9.81f, 0);

            #endregion

            #region Camera
            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.AddComponent<SoundListener>();
            camera.Transform.LookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            var script = (ScriptCustomCamera)camera.AddComponent<ScriptCustomCamera>();
            script.SetPosition(new Vector3(0, 15, 45), new Vector3(0, 5, 0));

            #region Camera Settings

            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 5000;
            camera.Camera.NearPlane = 2f;
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180f / 7f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            camera.Camera.PostProcess.ToneMapping.AutoExposureEnabled = false;
            camera.Camera.PostProcess.ToneMapping.LensExposure = 0f;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Both;
            camera.Camera.PostProcess.MLAA.Enabled = false;
            camera.Camera.PostProcess.Bloom.Threshold = 2;
            camera.Camera.PostProcess.FilmGrain.Enabled = false;
            camera.Camera.PostProcess.FilmGrain.Strength = 0.1f;
            camera.Camera.AmbientLight = new AmbientLight
            {
                //SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("FactoryCatwalkRGBM") { IsRgbm = true, RgbmMaxRange = 50, }),
                Color = new Color(30, 30, 30),
                Intensity = 6f,
                AmbientOcclusionStrength = 4f
            };
            //camera.Camera.Sky = new Skybox { TextureCube = new TextureCube("PurpleNebulaComplex") { IsRgbm = false, RgbmMaxRange = 50, }, ColorIntensity = 1f, };
            /*camera.Camera.AmbientLight.AmbientOcclusion = new HorizonBasedAmbientOcclusion
            {
                NumberSteps = 18, //8, // Don't change this.
                NumberDirections = 16, // 12, // Don't change this.
                Radius = 0.01f, // Bigger values produce more cache misses and you don’t want GPU cache misses, trust me.
                LineAttenuation = 1f,
                Contrast = 1f,
                AngleBias = 5f,
                Quality = HorizonBasedAmbientOcclusion.QualityType.HighQuality,
                TextureSize = Size.TextureSize.HalfSize,
            };*/

            #endregion

            #endregion
            
            // Make the floor (Kinematic) //
            // First, we create a bepu box entity that we will be used in our rigidbody component. 
            // Notice that we don't specify the mass, so we are creating a kinematic entity.
            // Kinematic rigidbodies are moved using through the transform component of the gameobject the rigidbody belongs to.
            // Kinematic rigidbodies don't respond to collisions and can't collide with other kinematic rigidbodies. However, they can
            // collide with dynamic rigidbodies affecting them.
            var bepuGround = new Box(Vector3.Zero, 100, 1, 100);
            // We add the entity to the scene so as the entity can be simulated
            PhysicsManager.Scene.Add(bepuGround);
            // Now, we create the game object and we add a rigidbody component to it.
            var floor = new GameObject3D(new XNAFinalEngine.Assets.Box(100, 1, 100), new BlinnPhong());            
            var floorRb = (RigidBody)floor.AddComponent<RigidBody>();
            // Finally, we set the entity created previously to the rigidbody component
            floorRb.Entity = bepuGround;
            

            // Make a Cube (Dynamic) //
            // Now, we create a 1x1x1 bepu box entity that will appear 20 units above the floor.
            // Notice that this time we specify the entity's mass in order to create a dynamic entity. 
            // Dynamic rigidbodies are affected by gravity and respond to collisions with other rigidbodies.
            // Dynamic rigidbodies are under the control of physics so they don't have to be moved or rotated through the transform component.
            var bepuCube = new Box(new Vector3(0, 20, 2), 1, 1, 1, 1);
            PhysicsManager.Scene.Add(bepuCube);            
            cube = new GameObject3D(new XNAFinalEngine.Assets.Box(1, 1, 1), new BlinnPhong { DiffuseColor = new Color(1.0f, 1.0f, 1.0f) });            
            var cubeRb = (RigidBody)cube.AddComponent<RigidBody>();
            cubeRb.Entity = bepuCube;
            // Create a script for test collisions
            var crt = (CollisionResponseTest)cube.AddComponent<CollisionResponseTest>();
            GameObject2D debugGo = new GameObject2D();
            debugGo.Transform.Position = new Vector3(15f, Screen.Height - 90, 0f);
            crt.DebugText = (HudText)debugGo.AddComponent<HudText>();
            

            // Make an obstacle (Kinematic) //
            var bepuObstacle = new Box(new Vector3(0.5f, 1f, 0.5f), 4, 1, 1.5f);            
            PhysicsManager.Scene.Add(bepuObstacle);
            obstacle = new GameObject3D(new XNAFinalEngine.Assets.Box(4, 1, 1.5f), new BlinnPhong { DiffuseColor = new Color(0.9f, 0.0f, 0.0f) });
            obstacle.Transform.Position = new Vector3(0.5f, 1f, 0.5f);
            var obstacleRb = (RigidBody) obstacle.AddComponent<RigidBody>();
            obstacleRb.Entity = bepuObstacle;            


            // Make a wall of boxes //
            MakeWall(20, 20);


            // Make a sphere obstacle (Dynamic) //
            var bepuSphere = new Sphere(new Vector3(-6f, 1f, -3f), 1f, 2.0f);
            PhysicsManager.Scene.Add(bepuSphere);
            var sphere = new GameObject3D(new XNAFinalEngine.Assets.Sphere(25, 25, 1f), new BlinnPhong { DiffuseColor = new Color(0.75f, 0.75f, 0.0f) });
            ((RigidBody) sphere.AddComponent<RigidBody>()).Entity = bepuSphere;  


            // Very Simple Crosshair //
            GameObject2D crosshair = new GameObject2D();
            crosshair.Transform.Position = new Vector3(Screen.Width / 2f, Screen.Height / 2f, 0f);
            var crossText = (HudText) crosshair.AddComponent<HudText>();
            crossText.Text.Append("+");

            #region Shadows and Lights

            var directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.Color = new Color(250, 250, 220);
            directionalLight.DirectionalLight.Intensity = 5;
            directionalLight.Transform.LookAt(new Vector3(0.0f, 0.0f, -0.99f), Vector3.Zero, Vector3.Up);
            /*directionalLight.DirectionalLight.Shadow = new CascadedShadow
            {
                Filter = Shadow.FilterType.Pcf3X3,
                LightDepthTextureSize = Size.Square1024X1024,
                ShadowTextureSize = Size.TextureSize.FullSize, // Lower than this could produce artifacts is the light is to intense.
                DepthBias = 0.0025f,
                FarPlaneSplit1 = 15,
                FarPlaneSplit2 = 40,
                FarPlaneSplit3 = 100,
                //FarPlaneSplit4 = 1000,
                ApplyBilateralFilter = false,
            };*/
            Shadow.DistributeShadowCalculationsBetweenFrames = true;

            var pointLight = new GameObject3D();
            pointLight.AddComponent<PointLight>();
            pointLight.PointLight.Color = new Color(200, 200, 230); // new Color(240, 235, 200);
            pointLight.PointLight.Intensity = 5f;
            pointLight.PointLight.Range = 60;
            pointLight.Transform.Position = new Vector3(4.8f, 10f, 10); // new Vector3(8f, -1f, 10);
            //pointLight.PointLight.Shadow = new CubeShadow { LightDepthTextureSize = 1024, };

            var pointLight2 = new GameObject3D();
            pointLight2.AddComponent<PointLight>();
            pointLight2.PointLight.Color = new Color(200, 170, 130);
            pointLight2.PointLight.Intensity = 2f;
            pointLight2.PointLight.Range = 30;
            pointLight2.Transform.Position = new Vector3(-12f, 10, -3);

            #endregion

            #region Statistics
            
            statistics = new GameObject2D();
            statistics.AddComponent<ScriptStatisticsDrawer>();
            
            #endregion

            #region Demo Legend

            #region Legend Background 
            
            Texture2D bg = new Texture2D(EngineManager.Device, 1, 1, false, SurfaceFormat.Color);
            Color[] bgColor = new Color[] { new Color(0f, 0f, 0f, 0.55f) };
            bg.SetData(bgColor);

            var bgTexture = new XNAFinalEngine.Assets.Texture(bg);
            var backgroundLegend = new GameObject2D();            
            backgroundLegend.Transform.Position = new Vector3(0f, 0f, 1f);
            var bgHudTex = (HudTexture) backgroundLegend.AddComponent<HudTexture>();
            bgHudTex.Texture = bgTexture;
            bgHudTex.DestinationRectangle = new Rectangle(0, Screen.Height - 115, Screen.Width, Screen.Height);
            
            #endregion

            var demoLegend = new GameObject2D();            
            demoLegend.Transform.Position = new Vector3(Screen.Width - 380f, Screen.Height - 105f, 0f);
            var legend = (HudText) demoLegend.AddComponent<HudText>();
            legend.Color = new Color(1f, 1f, 1f, 1f);
            legend.Text.Append(" Controls\n");
            legend.Text.Append("\n");
            legend.Text.Append("- Press Left Ctrl to fire a Blue box.\n");
            legend.Text.Append("- Press Space to apply an Up impulse to the white box.\n");
            legend.Text.Append("- Press Left Shift to cast a ray that paints yellow and \n");
            legend.Text.Append("  applies an Up impulse to the GO that hits.\n");
            legend.Text.Append("- Press keys A, S, W, D, Q, E to move and rotate the red box");

            #endregion

        } // LoadContent

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        public override void UpdateTasks()
        {
            if (XNAFinalEngine.Input.GamePad.PlayerOne.BackJustPressed)
                throw new Exception("Quick exit in Xbox 360 tests.");
            // Move the dynamic box //
            if (Keyboard.KeyJustPressed(Keys.Space) || XNAFinalEngine.Input.GamePad.PlayerOne.AJustPressed)
                cube.RigidBody.Entity.ApplyImpulse(cube.Transform.Position, Vector3.Up * 5);

            // Move the obstacle //
            if (Keyboard.KeyPressed(Keys.W) || XNAFinalEngine.Input.GamePad.PlayerOne.DPadUpPressed)
                obstacle.Transform.Translate(Vector3.Forward * Time.GameDeltaTime * 2, Space.World);
            if (Keyboard.KeyPressed(Keys.S) || XNAFinalEngine.Input.GamePad.PlayerOne.DPadDownPressed)
                obstacle.Transform.Translate(Vector3.Backward * Time.GameDeltaTime * 2, Space.World);
            if (Keyboard.KeyPressed(Keys.A) || XNAFinalEngine.Input.GamePad.PlayerOne.DPadLeftPressed)
                obstacle.Transform.Translate(Vector3.Left * Time.GameDeltaTime * 2, Space.World);
            if (Keyboard.KeyPressed(Keys.D) || XNAFinalEngine.Input.GamePad.PlayerOne.DPadRightPressed)
                obstacle.Transform.Translate(Vector3.Right * Time.GameDeltaTime * 2, Space.World);
            if (Keyboard.KeyPressed(Keys.Q) || XNAFinalEngine.Input.GamePad.PlayerOne.LeftButtonPressed)
                obstacle.Transform.Rotate(new Vector3(0, Time.GameDeltaTime * 35, 0));
            if (Keyboard.KeyPressed(Keys.E) || XNAFinalEngine.Input.GamePad.PlayerOne.RightButtonPressed)
                obstacle.Transform.Rotate(new Vector3(0, -Time.GameDeltaTime * 35, 0));

            // Fire a box //
            if (Keyboard.KeyJustPressed(Keys.LeftControl) || XNAFinalEngine.Input.GamePad.PlayerOne.XJustPressed)
            {
                var bullet = MakeBox(camera.Transform.Position, 1, 1, 1, 1, new Color(0.10f, 0.10f, 0.85f));
                var rb = bullet.RigidBody;
                rb.Entity.Orientation = camera.Transform.Rotation;
                rb.Entity.LinearVelocity = camera.Transform.Forward * 45f;
            }

            // Cast a ray and apply an impulse to the first object touched by the ray //
            if (Keyboard.KeyJustPressed(Keys.LeftShift) || XNAFinalEngine.Input.GamePad.PlayerOne.BJustPressed)
            {                
                RayCastResult raycastResult;
                GameObject3D go = PhysicsManager.Raycast(new Ray(camera.Transform.Position, camera.Transform.Forward), 100f, out raycastResult);
                if (go != null)
                {
                    // Change the color of the hitted GO                   
                    go.ModelRenderer.Material = new BlinnPhong { DiffuseColor = new Color(1.0f, 1.0f, 0.0f) };

                    // Apply Up impulse to the GO
                    go.RigidBody.Entity.LinearMomentum = Vector3.Up * 25f;
                }
            }
            
            base.UpdateTasks();
        } // UpdateTasks

        #endregion

        #region Helper Methods

        private GameObject3D MakeBox(Vector3 position, float width, float height, float depth, float mass, Color color)
        {
            var cubeEntity = new Box(position, width, height, depth, mass);
            PhysicsManager.Scene.Add(cubeEntity);
            var go = new GameObject3D(new XNAFinalEngine.Assets.Box(width, height, depth), new BlinnPhong { DiffuseColor = color });
            var cubeRb = (RigidBody) go.AddComponent<RigidBody>();
            cubeRb.Entity = cubeEntity;

            return go;
        } // MakeBox


        private void MakeWall(int rows, int cols)
        {
            for (int i = 0; i <= rows; i++)
                for (int j = 0; j <= cols; j++)
                {
                    MakeBox(new Vector3(5 + j + 0.05f * j, i + 0.05f * i, 0), 1, 1, 1, 1, j % 2 == 0? new Color(0.85f, 0.65f, 0f) : new Color(0.5f, 0.9f, 0.5f));
                }
        } // MakeWall

        #endregion

    } // PhysicsTestScene    
} // XNAFinalEngineExamples
