
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System.Runtime;
using System.Text;
using Microsoft.Xna.Framework;
using XnaFinalEngine.Components;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using XNAFinalEngineBase.Helpers;

#endregion

namespace XNAFinalEngine.EngineCore
{

    /// <summary>
    /// The manager of all managers.
    /// </summary>
    public static class GameLoop
    {

        #region Variables

        private static GameObject2D testText;
        private static Font font;

        #endregion

        #region Load Content
        
        /// <summary>
        /// Load Content.
        /// </summary>
        public static void LoadContent()
        {
            // Create the 32 layers.
            Layer.InitLayers();
            SpriteManager.Init();

            for (int i = 0; i < HudText.HudTextPool2D.Capacity; i++)
            {
                testText = new GameObject2D();
                HudText textComponent = ((HudText)testText.AddComponent<HudText>());
                textComponent.Font = new Font("Arial12");
                textComponent.Color = Color.White;
                textComponent.Text.Insert(0, "FPS ");
                testText.Transform.LocalPosition = new Vector3(100, 100, 0);
            }

            #region Garbage Collection

            // All generations will undergo a garbage collection.
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            // Enables garbage collection that is more conservative in reclaiming objects.
            // Full Collections occur only if the system is under memory pressure while generation 0 and generation 1 collections might occur more frequently.
            // This is the least intrusive mode.
            // If the work is done right, this latency mode is not need really.
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            TestGarbageCollection.CreateWeakReference();

            #endregion

        } // LoadContent
        
        #endregion

        #region Update

        /// <summary>
        /// Update
        /// </summary>
        internal static void Update(GameTime gameTime)
        {
            Time.GameDeltaTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);            
        } // Update

        #endregion

        #region Draw
        
        /// <summary>
        /// Draw
        /// </summary>        
        internal static void Draw(GameTime gameTime)
        {
            Time.FrameTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);
            
            // Update frame time            
            // Draw auxiliary cameras in forward.
            // Draw main deferred lighting cameras (one for each viewport)
            // Draw 2D Hud            
            SpriteManager.Begin();
            {
                for (int i = 0; i < HudText.HudTextPool2D.Count; i++)
                {
                    HudText.HudTextPool2D.elements[i].Text.Length = 4;
                    HudText.HudTextPool2D.elements[i].Text.AppendWithoutGarbage(Time.FramesPerSecond);
                    SpriteManager.DrawText(HudText.HudTextPool2D.elements[i].Font,
                                           HudText.HudTextPool2D.elements[i].Text,
                                           new Vector3(100, 100, 0),
                                           HudText.HudTextPool2D.elements[i].Color,
                                           0, Vector2.Zero, 1);
                }
            }
            SpriteManager.End();
            
        } // Draw

        #endregion

        #region Unload Content

        internal static void UnloadContent()
        {

        } // UnloadContent

        #endregion

    } // GameLoop
} // XNAFinalEngine.EngineCore