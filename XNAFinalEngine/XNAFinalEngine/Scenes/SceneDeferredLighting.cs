
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
using XNAFinalEngine.Sounds;
using XNAFinalEngine.Input;
using DirectionalLight = XNAFinalEngine.Graphics.DirectionalLight;

#endregion

namespace XNAFinalEngine.Scenes
{

    /// <summary>
    /// Test.
    /// </summary>
    public class SceneDeferredLighting : Scene
    {

        #region Variables

        #region Cameras
        
        #endregion

        #region Lights

        #endregion

        #region Objects

        #region Objects

        private GraphicObject gFloor, gController;

        private ContainerObject cObjects;

        #endregion

        #endregion

        #region Materials

        #endregion

        #region PrePostScreenShaders

        private PreDepthNormal preDepthShader;

        #endregion

        private RenderToTexture LightPrePass;
        private Effect fxLight, mat;
        private BlendState blendState;

        private SSAOHorizonBased ssaoHB;
        private Blur blurSSAO;
        private CombineShadows convineShadow;

        #endregion

        #region Load

        /// <summary>
        /// Load the scene
        /// </summary>
        public override void Load()
        {

            ApplicationLogic.Camera = new XSICamera(new Vector3(0, 0, 0), 50, 1, 0.2f); //
            //ApplicationLogic.Camera = new FreeCamera(new Vector3(0, 0, 20), Vector3.Zero);


            #region Load Models

            gController = new GraphicObject("xbox360Controller", new Blinn(Color.Blue));
            
            gFloor = new GraphicObject(new Graphics.Plane(50, 50), new Blinn(Color.White));

            cObjects = new ContainerObject();
            cObjects.AddObject(gController);
            cObjects.AddObject(gFloor);

            //gController.TranslateAbs(20, 0, 0);

            gController.AssociateLight(new DirectionalLight(Vector3.Forward, Color.Red));
            ((Blinn)gController.Material).SpecularIntensity = 5;
            ((Blinn)gController.Material).SpecularExponent = 30;

            #endregion
            
            preDepthShader = new PreDepthNormal();

            LightPrePass = new RenderToTexture(RenderToTexture.SizeType.FullScreen, false, true);

            fxLight = EngineManager.CurrentContent.Load<Effect>(@"Content\Shaders\LightDirectional");
            fxLight.CurrentTechnique = fxLight.Techniques["DirectionalLight"];

            blendState = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.One,
            };

            mat = EngineManager.CurrentContent.Load<Effect>(@"Content\Shaders\BlinnLPP");
            mat.CurrentTechnique = mat.Techniques["BlinnPhong"];
            /*
            blurSSAO = new Blur { BlurWidth = 0.4f };
            ssaoHB = new SSAOHorizonBased(RenderToTexture.SizeType.HalfScreen)
            {
                NumberSteps = 24,
                NumberDirections = 15,
                Radius = 0.05f,
                LineAttenuation = 1.5f,
                Contrast = 1.0f,
                AngleBias = 30
            };

            convineShadow = new CombineShadows();
            */
            EngineManager.ShowFPS = true;
            
        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public override void Update()
        {
            
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public override void Render()
        {
            EngineManager.Device.BlendState = BlendState.Opaque;
            EngineManager.Device.DepthStencilState = DepthStencilState.Default;
            EngineManager.Device.RasterizerState = RasterizerState.CullNone;
            preDepthShader.GenerateDepthNormalMap(cObjects);
            /*
            ssaoHB.GenerateSSAO(preDepthShader.DepthMapTexture.XnaTexture, preDepthShader.NormalMapTexture.XnaTexture);
            blurSSAO.GenerateBlur(ssaoHB.SSAOTexture);
            */
            LightPrePass.EnableRenderTarget();
            LightPrePass.Clear(Color.Transparent);
            EngineManager.Device.BlendState = blendState;
            
            // Draw all lights
            
            // Pass in the far frustum corners in View Space so that position can be cheaply reconstructed from a Depth value.
            BoundingFrustum boundingFrustum = new BoundingFrustum(ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix);
            Vector3[] cornersWS = boundingFrustum.GetCorners();
            Vector3[] corners = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                corners[i] = cornersWS[i + 4];
            }
            Vector3 temp = corners[3];
            corners[3] = corners[2];
            corners[2] = temp;
            
            fxLight.Parameters["ViewPosition"].SetValue(ApplicationLogic.Camera.Position);
            fxLight.Parameters["ScreenRes"].SetValue(new Vector2(EngineManager.Width, EngineManager.Height));
            fxLight.Parameters["FrustumCorners"].SetValue(corners);
            fxLight.Parameters["DepthTexture"].SetValue(preDepthShader.DepthMapTexture.XnaTexture);
            fxLight.Parameters["NormalMap"].SetValue(preDepthShader.NormalMapTexture.XnaTexture);
            /*
            fxLight.Parameters["LightColor"].SetValue(new Vector3(0.8f, 1, 0.8f));
            fxLight.Parameters["LightDir"].SetValue(Vector3.Down);
            fxLight.Parameters["SpecPower"].SetValue(100f);
            fxLight.CurrentTechnique.Passes[0].Apply();
            ScreenPlane.Render();
            
            fxLight.Parameters["LightColor"].SetValue(new Vector3(1, 0.8f, 0.8f));
            fxLight.Parameters["LightDir"].SetValue(Vector3.Right);
            fxLight.Parameters["SpecPower"].SetValue(100f);
            fxLight.CurrentTechnique.Passes[0].Apply();
            ScreenPlane.Render();
            */
            fxLight.Parameters["LightColor"].SetValue(new Vector3(0.8f, 0.8f, 1f));
            fxLight.Parameters["LightDir"].SetValue(Vector3.Forward);
            //fxLight.Parameters["LightDir"].SetValue(new Vector3(0.5f, -1, -0.5f));
            fxLight.Parameters["SpecPower"].SetValue(1000.0f);
            fxLight.CurrentTechnique.Passes[0].Apply();
            ScreenPlane.Render();
            
            LightPrePass.DisableRenderTarget();

            EngineManager.Device.BlendState = BlendState.AlphaBlend;
            EngineManager.Device.DepthStencilState = DepthStencilState.Default;
            
            EngineManager.ClearTargetAndDepthBuffer(Color.DarkGray);
            
            mat.CurrentTechnique.Passes[0].Apply();

            mat.Parameters["ViewProjection"].SetValue(ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix);
            mat.Parameters["ScreenRes"].SetValue(new Vector2(EngineManager.Width, EngineManager.Height));
            mat.Parameters["lightMap"].SetValue(LightPrePass.XnaTexture);
            
            mat.Parameters["World"].SetValue(gController.WorldMatrix);
            mat.CurrentTechnique.Passes[0].Apply();
            gController.Model.Render();
            mat.Parameters["World"].SetValue(gFloor.WorldMatrix);
            mat.CurrentTechnique.Passes[0].Apply();
            gFloor.Model.Render();

            //preDepthShader.DepthMapTexture.RenderOnScreen(new Rectangle(0, 0, (int)(200 * 1.6f), 200));
            //preDepthShader.NormalMapTexture.RenderOnScreen(new Rectangle((int)(200 * 1.6f), 0, (int)(200 * 1.6f), 200));
            //LightPrePass.RenderOnScreen(new Rectangle((int)(200 * 1.6f) * 2, 0, (int)(400 * 1.6f), 400));
            
            //convineShadow.GenerateCombineShadows(blurSSAO.BlurMapTexture);
            //convineShadow.GenerateCombineShadows(ssaoHB.SSAOTexture);
            //ssaoHB.SSAOTexture.RenderOnFullScreen();
            //LightPrePass.RenderOnFullScreen();

        } // Render

        #endregion

        #region UnloadContent

        /// <summary>
        /// Unload the content that it isn't unloaded automatically when the scene is over.
        /// </summary>
        public override void UnloadContent()
        {

        } // UnloadContent

        #endregion

    } // SceneEmpty
} // XNAFinalEngine.Scenes
