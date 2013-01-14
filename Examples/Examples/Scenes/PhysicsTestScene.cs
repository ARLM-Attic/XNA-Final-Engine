
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

#region Using directives
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
#endregion

namespace XNAFinalEngineExamples
{
    /// <summary>
    /// This scene test the BEPU physic system.
    /// The physic interface was not completed in both functionality and ease of use.
    /// However it implements the key elements to perform a proper adaptation of Bepu Physics.
    /// </summary>
    class PhysicsTestScene : Scene
    {

        #region Variables
         
        // Camera, cube and obstacles.
        private static GameObject3D camera, cube, obstacle;        
        // Used to show the engine statistics onto screen.
        private static GameObject2D statistics;

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        public override void LoadContent()
        {

            #region Physics Simulation Settings

            // Setup gravity.
            PhysicsManager.Gravity = new Vector3(0, -9.81f, 0);

            #endregion

            #region Camera

            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.AddComponent<SoundListener>();
            camera.Transform.LookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            var script = (ScriptCustomCamera)camera.AddComponent<ScriptCustomCamera>();
            script.SetPosition(new Vector3(0, 15, 45), new Vector3(0, 5, 0));
            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 500;
            camera.Camera.NearPlane = 1f;
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180f / 7f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            camera.Camera.PostProcess.ToneMapping.AutoExposureEnabled = false;
            camera.Camera.PostProcess.ToneMapping.LensExposure = 0f;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Both;
            camera.Camera.PostProcess.MLAA.Enabled = false;
            camera.Camera.PostProcess.Bloom.Threshold = 2;
            camera.Camera.PostProcess.FilmGrain.Enabled = true;
            camera.Camera.PostProcess.FilmGrain.Strength = 0.03f;
            camera.Camera.PostProcess.AnamorphicLensFlare.Enabled = false;
            camera.Camera.AmbientLight = new AmbientLight
            {
                Color = new Color(20, 20, 25),
                Intensity = 0.5f,
                AmbientOcclusionStrength = 1f
            };

            #endregion

            #region Models and Physics

            // Make the floor (Kinematic) //
            // First, we create a bepu box entity that we will be used in our rigidbody component. 
            // Notice that we don't specify the mass, so we are creating a kinematic entity.
            // Kinematic rigidbodies are moved using through the transform component of the gameobject the rigidbody belongs to.
            // Kinematic rigidbodies don't respond to collisions and can't collide with other kinematic rigidbodies. However, they can
            // collide with dynamic rigidbodies affecting them.
            var bepuGround = new Box(Vector3.Zero, 50, 1, 50);
            // We add the entity to the scene so as the entity can be simulated
            PhysicsManager.Scene.Add(bepuGround);
            // Now, we create the game object and we add a rigidbody component to it.
            var floor = new GameObject3D(new XNAFinalEngine.Assets.Box(50, 1, 50), new BlinnPhong { SpecularIntensity = 0.3f, SpecularPower = 200 });            
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
            cube = new GameObject3D(new XNAFinalEngine.Assets.Box(1, 1, 1), new BlinnPhong { DiffuseColor = new Color(0.8f, 0.8f, 0.85f) } );            
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
            obstacle = new GameObject3D(new XNAFinalEngine.Assets.Box(4, 1, 1.5f), new BlinnPhong { DiffuseColor = new Color(0.9f, 0.2f, 0.15f), SpecularPower = 20 });
            obstacle.Transform.Position = new Vector3(0.5f, 1f, 0.5f);
            var obstacleRb = (RigidBody) obstacle.AddComponent<RigidBody>();
            obstacleRb.Entity = bepuObstacle;            
            
            // Make a wall of boxes //
            MakeWall(20, 20);

            // Make a sphere obstacle (Dynamic) //
            var bepuSphere = new Sphere(new Vector3(-6f, 1f, -3f), 1f, 2.0f);
            PhysicsManager.Scene.Add(bepuSphere);
            var sphere = new GameObject3D(new XNAFinalEngine.Assets.Sphere(25, 25, 1f), new BlinnPhong { DiffuseColor = new Color(0.79f, 0.75f, 0.2f), SpecularPower = 20 });
            ((RigidBody) sphere.AddComponent<RigidBody>()).Entity = bepuSphere;  
            
            // Very Simple Crosshair //
            GameObject2D crosshair = new GameObject2D();
            crosshair.Transform.Position = new Vector3(Screen.Width / 2f, Screen.Height / 2f, 0f);
            var crossText = (HudText) crosshair.AddComponent<HudText>();
            crossText.Text.Append("+");

            #endregion

            #region Shadows and Lights

            Shadow.DistributeShadowCalculationsBetweenFrames = true;

            var pointLight = new GameObject3D();
            pointLight.AddComponent<PointLight>();
            pointLight.PointLight.Color = new Color(180, 200, 250);
            pointLight.PointLight.Intensity = 4f;
            pointLight.PointLight.Range = 60;
            pointLight.Transform.Position = new Vector3(4.8f, 10f, 10);
            pointLight.PointLight.Shadow = new CubeShadow { LightDepthTextureSize = 1024 };

            var pointLight2 = new GameObject3D();
            pointLight2.AddComponent<PointLight>();
            pointLight2.PointLight.Color = new Color(100, 110, 170);
            pointLight2.PointLight.Intensity = 1f;
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
            #if XBOX
                // This is intended to be used in Xbox tests.
                //if (XNAFinalEngine.Input.GamePad.PlayerOne.BackJustPressed)
                //    throw new Exception("Quick exit in Xbox 360 tests.");
            #endif

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
                    go.ModelRenderer.Material = new BlinnPhong { DiffuseColor = new Color(1.0f, 1.0f, 0.0f), SpecularPower = 500, SpecularIntensity = 0.5f };

                    // Apply Up impulse to the GO
                    go.RigidBody.Entity.LinearMomentum = Vector3.Up * 25f;
                }
            }
            
            base.UpdateTasks();
        } // UpdateTasks

        #endregion

        #region Helper Methods

        /// <summary>
        /// Makes a box that is affected by physics.
        /// </summary>
        private static GameObject3D MakeBox(Vector3 position, float width, float height, float depth, float mass, Color color)
        {
            var cubeEntity = new Box(position, width, height, depth, mass);
            PhysicsManager.Scene.Add(cubeEntity);
            var gameObject = new GameObject3D(new XNAFinalEngine.Assets.Box(width, height, depth), new BlinnPhong { DiffuseColor = color });
            var rigidBody = (RigidBody) gameObject.AddComponent<RigidBody>();
            rigidBody.Entity = cubeEntity;

            return gameObject;
        } // MakeBox

        /// <summary>
        /// Makes a wall of boxes that are affected by physics.
        /// </summary>
        private static void MakeWall(int rows, int cols)
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
