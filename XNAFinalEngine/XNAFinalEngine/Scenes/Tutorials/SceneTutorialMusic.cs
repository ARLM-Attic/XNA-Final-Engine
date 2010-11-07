
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
using XNAFinalEngine.GraphicElements;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
using XNAFinalEngine.Sounds;
using XNAFinalEngine.Input;
using DirectionalLight = XNAFinalEngine.GraphicElements.DirectionalLight;
#endregion

namespace XNAFinalEngine.Scenes
{
    /// <summary>
    /// Music tutorial.
    /// This tutorial plays the music files compiled in the music directory.
    /// 
    /// The background scene exists only for cosmetics.
    /// 
    /// IMPORTANT!s
    /// 
    /// * Music filename format: index - Artist - song name, where index is the song number in the list.
    /// 
    /// * The shuffle won't work very well with a small number of songs.
    /// </summary>
    public class SceneTutorialMusic : Scene
    {

        #region Variables

        #region Camera
        
        private Camera cameraCursors;

        #endregion

        #region Objects

        private GraphicObject gKeyLeftArrow, gKeyRightArrow, gKeyUpArrow, gKeyDownArrow, gSpaceKey, gPlaneTextCursors, gPlaneTextShuffle, gFloor;

        private ContainerObject cObjects;

        #endregion

        #region Lights

        /// <summary>
        /// Scene's lights
        /// </summary>
        private Light pointLight1, directionalLight;

        #endregion

        #region Render Targets

        /// <summary>
        /// Some text will be render in these render targets, and the result texture will be used in the materials of two objects.
        /// </summary>
        private RenderToTexture rtTextCursors, rtTextShuffle;

        #endregion

        #region Chronometers

        /// <summary>
        /// Chronometers for camera's movements.
        /// </summary>
        private Chronometer chronometer = new Chronometer(), chronometer2 = new Chronometer();

        #endregion

        #region PrePostScreenShaders

        /// <summary>
        /// Variables for ambient oclussion.
        /// </summary>
        private PreDepthNormal preDepthShader;
        private SSAOHorizonBased ssaoHB;
        private Blur blurSSAO;
        private CombineShadows convineShadow;

        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Load the scene
        /// </summary>
        public override void Load()
        {
            // Play from the first song.
            MusicManager.Play(0);
            MusicManager.Volume = 0.5f; // Half volume

            #region Load background scene

            cameraCursors = new FixedCamera(new Vector3(-5, 45, 4), new Vector3(-7, 0, -1));

            #region Load Models

            gKeyLeftArrow = new GraphicObject("Tutorials\\key", new Blinn("Tutorials\\KeyLeftArrow") { SurfaceColor = new Color(50, 50, 50) });
            gKeyLeftArrow.TranslateAbs(-1.5f, 0, 0);
            gKeyRightArrow = new GraphicObject("Tutorials\\key", new Blinn("Tutorials\\KeyRightArrow") { SurfaceColor = new Color(50, 50, 50) });
            gKeyRightArrow.TranslateAbs(1.5f,0,0);
            gKeyUpArrow = new GraphicObject("Tutorials\\key", new Blinn("Tutorials\\KeyUpArrow") { SurfaceColor = new Color(50, 50, 50) });
            gKeyUpArrow.TranslateAbs(0, 0, -1.75f);
            gKeyDownArrow = new GraphicObject("Tutorials\\key", new Blinn("Tutorials\\KeyDownArrow") { SurfaceColor = new Color(50, 50, 50) });
            gSpaceKey = new GraphicObject("Tutorials\\SpaceKey", new Blinn() { SurfaceColor = new Color(50, 50, 50) });
            gSpaceKey.TranslateAbs(-10, 0, 0);
            
            gPlaneTextCursors = new GraphicObject(new GraphicElements.Plane(5, 5), new Constant(new XNAFinalEngine.GraphicElements.Texture(), 1));
            gPlaneTextCursors.RotateAbs(-90, 0, 0);
            gPlaneTextShuffle = new GraphicObject(new GraphicElements.Plane(5, 5), new Constant(new XNAFinalEngine.GraphicElements.Texture(), 1));
            gPlaneTextShuffle.RotateAbs(-90, 0, 0);
            gPlaneTextShuffle.TranslateAbs(-10, 0, 0);

            gFloor = new GraphicObject(new GraphicElements.Plane(50, 50), new Blinn(new Color(20, 20, 20)));
            gFloor.RotateAbs(-90, 0, 0);
            gFloor.TranslateAbs(0, -0.001f, 0);

            cObjects = new ContainerObject();
            cObjects.TranslateAbs(0, 0, 1);

            cObjects.AddObject(gKeyLeftArrow);
            cObjects.AddObject(gKeyRightArrow);
            cObjects.AddObject(gKeyUpArrow);
            cObjects.AddObject(gKeyDownArrow);
            cObjects.AddObject(gSpaceKey);
            cObjects.AddObject(gFloor);

            #endregion

            #region Lights

            AmbientLight.LightColor = new Color(100, 100, 100);

            directionalLight = new DirectionalLight(new Vector3(-0.1f, -1.0f, 0.1f), new Color(100, 120, 130));
            pointLight1 = new PointLight(new Vector3(15, 12, -30), new Color(100, 100, 100));

            cObjects.AssociateLight(pointLight1);
            cObjects.AssociateLight(directionalLight);

            #endregion

            rtTextCursors = new RenderToTexture(RenderToTexture.SizeType.Custom512x512, false, false, 0);
            rtTextShuffle = new RenderToTexture(RenderToTexture.SizeType.Custom512x512, false, false, 0);

            chronometer.Start();
            chronometer2.Start();

            #region Post Screen Shaders

            preDepthShader = new PreDepthNormal(true) { FarPlane = 100 };

            blurSSAO = new Blur() { BlurWidth = 1.5f };
            ssaoHB = new SSAOHorizonBased(RenderToTexture.SizeType.HalfScreen)            
            {
                NumberSteps = 32,
                NumberDirections = 15,
                Radius = 0.05f,
                LineAttenuation = 1.0f,
                Contrast = 2.0f,
                AngleBias = 10
            };

            convineShadow = new CombineShadows();

            #endregion

            #endregion

        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update.
        /// </summary>
        public override void Update()
        {

            #region Camera movement

            if (chronometer.ElapsedTime < 10)
            {
                cameraCursors.Position = new Vector3(cameraCursors.Position.X, cameraCursors.Position.Y, cameraCursors.Position.Z - (float)EngineManager.FrameTime * 0.1f);
            }
            else
            {
                if (chronometer.ElapsedTime > 20)
                {
                    chronometer.Reset();
                }
                else
                {
                    cameraCursors.Position = new Vector3(cameraCursors.Position.X, cameraCursors.Position.Y, cameraCursors.Position.Z + (float)EngineManager.FrameTime * 0.1f);
                }
            }
            if (chronometer2.ElapsedTime > 5 && chronometer2.ElapsedTime < 15)
            {
                cameraCursors.Position = new Vector3(cameraCursors.Position.X + (float)EngineManager.FrameTime * 0.1f, cameraCursors.Position.Y, cameraCursors.Position.Z);
            }
            else
            {
                if (chronometer2.ElapsedTime >= 15 && chronometer2.ElapsedTime < 25)
                {
                    cameraCursors.Position = new Vector3(cameraCursors.Position.X - (float)EngineManager.FrameTime * 0.1f, cameraCursors.Position.Y, cameraCursors.Position.Z);
                }
                else
                {
                    if (chronometer2.ElapsedTime >= 25)
                        chronometer2.Reset();
                }
            }

            #endregion

            if (Input.Keyboard.RightJustPressed)
            {
                MusicManager.Next();
            }
            if (Input.Keyboard.LeftJustPressed)
            {
                MusicManager.Previous();
            }
            if (Input.Keyboard.UpPressed)
            {
                MusicManager.Volume = MusicManager.Volume + 0.5f * (float)(EngineManager.FrameTime);
                if (MusicManager.Volume > 1) MusicManager.Volume = 1;
            }
            if (Input.Keyboard.DownPressed)
            {
                MusicManager.Volume = MusicManager.Volume - 0.5f * (float)(EngineManager.FrameTime);
                if (MusicManager.Volume < 0) MusicManager.Volume = 0;
            }
            if (Input.Keyboard.SpaceJustPressed)
            {
                MusicManager.Shuffle = !MusicManager.Shuffle;
            }
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Render.
        /// </summary>
        public override void Render()
        {

            #region Render background scene

            #region Pre Work

            preDepthShader.GenerateDepthNormalMap(cObjects);
            ssaoHB.GenerateSSAO(preDepthShader.HighPrecisionDepthMapTexture.XnaTexture, preDepthShader.NormalDepthMapTexture.XnaTexture);
            blurSSAO.GenerateBlur(ssaoHB.SSAOTexture);

            #endregion

            #region Render cursor text

            rtTextCursors.EnableRenderTarget();
                rtTextCursors.Clear(new Color(0, 0, 0, 0));
                FontArial14.Render("Previous Song", new Vector2(32, 450), Color.LightBlue);
                FontArial14.Render("Next Song", new Vector2(365, 450), Color.LightBlue);
                FontArial14.Render("Volume Down", new Vector2(192, 450), Color.LightBlue);
                FontArial14.Render("Volume Up", new Vector2(205, 60), Color.LightBlue);
            rtTextCursors.DisableRenderTarget();

            #endregion

            #region Render shuffle text

            rtTextShuffle.EnableRenderTarget();
                rtTextShuffle.Clear(new Color(0, 0, 0, 0));
                FontArial14.Render("Shufle", new Vector2(250, 450), Color.LightBlue);
            rtTextShuffle.DisableRenderTarget();

            #endregion

            EngineManager.ClearTargetAndDepthBuffer(Color.Black);

            ApplicationLogic.Camera = cameraCursors;

            cObjects.Render();

            #region Post Work

            convineShadow.GenerateCombineShadows(blurSSAO.BlurMapTexture);
            SpriteManager.DrawSprites();

            #endregion

            ((Constant)(gPlaneTextCursors.Material)).DiffuseTexture = rtTextCursors;
            gPlaneTextCursors.Render();

            ((Constant)(gPlaneTextShuffle.Material)).DiffuseTexture = rtTextShuffle;
            gPlaneTextShuffle.Render();

            #endregion

            FontBattlefield18.RenderCentered("Artist: " + MusicManager.CurrentSongArtistName, 20, Color.White);
            FontBattlefield18.RenderCentered("Song: " + MusicManager.CurrentSongName, 20 + 30, Color.White);
            FontArial14.RenderCentered("Volume: " + MusicManager.Volume.ToString("N2"), 20 + 80, Color.White);

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

    } // SceneTutorialMusic
} // XNAFinalEngine.Scenes
