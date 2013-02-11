
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
using BEPUphysics.EntityStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
#if (!XBOX)
    using XNAFinalEngine.Editor;
#endif
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Physics;
using Box = BEPUphysics.Entities.Prefabs.Box;
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
        private GameObject3D camera, cube, obstacle, floor;

        // Materials //
        private Material hitMaterial;
        private Material mat1;
        private Material mat2;

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        protected override void LoadContent()
        {
            hitMaterial = new BlinnPhong { DiffuseColor = new Color(0.10f, 0.10f, 0.85f) };
            mat1 = new BlinnPhong { DiffuseColor = new Color(0.85f, 0.65f, 0f) };
            mat2 = new BlinnPhong { DiffuseColor = new Color(0.5f, 0.9f, 0.5f) };

            #region Physics Simulation Settings

            // Setup gravity.
            PhysicsManager.Gravity = new Vector3(0, -9.81f, 0);

            #endregion

            #region Camera

            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.AddComponent<SoundListener>();
            camera.Transform.LookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            var script = (ScriptCustomCameraScript)camera.AddComponent<ScriptCustomCameraScript>();
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
            // Kinematic rigidbodies are moved through the transform component of the gameobject which it belongs.
            // Kinematic rigidbodies don't respond to collisions and can't collide with other kinematic rigidbodies. However, they can
            // collide with dynamic rigidbodies affecting them.
            var bepuGround = new Box(Vector3.Zero, 50, 1, 50);            
            // Now, we create the game object and we add a rigidbody component to it.
            floor = new GameObject3D(new XNAFinalEngine.Assets.Box(50, 1, 50), new BlinnPhong { SpecularIntensity = 0.3f, SpecularPower = 200 });            
            var floorRb = (RigidBody)floor.AddComponent<RigidBody>();
            // Finally, we set the entity created previously to the rigidbody component
            floorRb.Entity = bepuGround;
            
            // Make a Cube (Dynamic) //
            // Now, we create a 1x1x1 bepu box entity that will appear 20 units above the floor.
            // Notice that this time we specify the entity's mass in order to create a dynamic entity. 
            // Dynamic rigidbodies are affected by gravity and respond to collisions with other rigidbodies.
            // Dynamic rigidbodies are under the control of physics so they don't have to be moved or rotated through the transform component.
            var bepuCube = new Box(new Vector3(0, 20, 2), 1, 1, 1, 1);            
            cube = new GameObject3D(new XNAFinalEngine.Assets.Box(1, 1, 1), new BlinnPhong { DiffuseColor = new Color(0.8f, 0.8f, 0.85f) } );            
            var cubeRb = (RigidBody)cube.AddComponent<RigidBody>();
            cubeRb.Entity = bepuCube;
            // Create a script for test collisions
            var crt = (CollisionResponseTestScript)cube.AddComponent<CollisionResponseTestScript>();
            GameObject2D debugGo = new GameObject2D();
            debugGo.Transform.Position = new Vector3(15f, Screen.Height - 90, 0f);
            crt.DebugText = (HudText)debugGo.AddComponent<HudText>();
            
            // Make an obstacle (Kinematic) //
            var bepuObstacle = new Box(new Vector3(0.5f, 1f, 0.5f), 4, 1, 1.5f);                        
            obstacle = new GameObject3D(new XNAFinalEngine.Assets.Box(4, 1, 1.5f), new BlinnPhong { DiffuseColor = new Color(0.9f, 0.2f, 0.15f), SpecularPower = 20 });
            obstacle.Transform.Position = new Vector3(0.5f, 1f, 0.5f);
            var obstacleRb = (RigidBody) obstacle.AddComponent<RigidBody>();
            obstacleRb.Entity = bepuObstacle;
            
            // Make a wall of boxes //
            MakeWall(6, 9);

            // Make a sphere obstacle (Dynamic) //
            // The sphere model is not center in its model space, instead is displaced 10 units on Z.
            GameObject3D sphere;
            // First we creates this sphere with no physics representation.
            //sphere = new GameObject3D(new FileModel("SphereTransformed"), new BlinnPhong { DiffuseColor = new Color(1f, 0f, 0f), SpecularPower = 20 });
            // Then we creates the same sphere and asign a dynamic physic object representation.
            sphere = new GameObject3D(new FileModel("SphereTransformed"), new BlinnPhong { DiffuseColor = new Color(0.79f, 0.75f, 0.2f), SpecularPower = 20 });
            // The initial motion state place the sphere just a little above.
            MotionState motionState = new MotionState { Position = new Vector3(0f, 10f, 0f), Orientation = Quaternion.Identity };
            // We create the physic object. The offset that creates Bepu is hide transparently.
            // Moreover the initial motion state takes in consideration this offset and places the sphere in the same x, z coordinates as the other sphere.
            ((RigidBody)sphere.AddComponent<RigidBody>()).CreateDynamicEntityFromModelFilter(motionState, 1);

            // Static mesh.
            var lamboBody = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Body"
#if XBOX 
    + "Xbox"
#endif
                ), new BlinnPhong { DiffuseColor = Color.Gray });
            lamboBody.Transform.Position = new Vector3(-3, 3, 3);
            ((StaticCollider)lamboBody.AddComponent<StaticCollider>()).CreateStaticCollidableFromModelFilter();

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
        protected override void UpdateTasks()
        {
            // This is intended to be used in Xbox tests.
            //if (XNAFinalEngine.Input.GamePad.PlayerOne.BackJustPressed)
            //    throw new Exception("Quick exit in Xbox 360 tests.");

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
                var bullet = MakeBox(camera.Transform.Position, 1, 1, 1, 1, hitMaterial);
                var rb = bullet.RigidBody;
                rb.Entity.Orientation = camera.Transform.Rotation;
                rb.Entity.LinearVelocity = camera.Transform.Forward * 45f;
            }

            // Cast a ray and apply an impulse to the first object touched by the ray //
            if (Keyboard.KeyJustPressed(Keys.LeftShift) || XNAFinalEngine.Input.GamePad.PlayerOne.BJustPressed)
            {                
                RayCastResult raycastResult;
                GameObject3D go = PhysicsManager.Raycast(new Ray(camera.Transform.Position, camera.Transform.Forward), 100f, out raycastResult);
                if (go != null && go != floor && go != cube && go != obstacle)
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
        private static GameObject3D MakeBox(Vector3 position, float width, float height, float depth, float mass, Material mat)
        {
            var cubeEntity = new Box(position, width, height, depth, mass);            
            var gameObject = new GameObject3D(new XNAFinalEngine.Assets.Box(width, height, depth), mat);
            var rigidBody = (RigidBody) gameObject.AddComponent<RigidBody>();
            rigidBody.Entity = cubeEntity;

            return gameObject;
        } // MakeBox

        /// <summary>
        /// Makes a wall of boxes that are affected by physics.
        /// </summary>
        private void MakeWall(int rows, int cols)
        {
            for (int i = 0; i <= rows; i++)
                for (int j = 0; j <= cols; j++)
                {
                    MakeBox(new Vector3(5 + j + 0.05f * j, i + 0.05f * i, 0), 1, 1, 1, 1, j % 2 == 0? mat1 : mat2);
                }
        } // MakeWall

        #endregion

    } // PhysicsTestScene    
} // XNAFinalEngineExamples
