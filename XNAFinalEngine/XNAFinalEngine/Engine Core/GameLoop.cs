
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaFinalEngine.Components;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
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

        private static FileModel model;
        
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
                testText.AddComponent<HudText>();
                testText.HudText.Font = new Font("Arial12");
                testText.HudText.Color = Color.White;
                testText.HudText.Text.Insert(0, "FPS ");
                testText.Transform.LocalPosition = new Vector3(100, 100, 0);
                testText.Transform.LocalRotation = 0f;
            }

            model = new FileModel("LamborghiniMurcielago\\Murcielago-Body");
            
            #region Garbage Collection

            // All generations will undergo a garbage collection.
            #if (!XBOX)
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            #else
                GC.Collect();
            #endif
            // Enables garbage collection that is more conservative in reclaiming objects.
            // Full Collections occur only if the system is under memory pressure while generation 0 and generation 1 collections might occur more frequently.
            // This is the least intrusive mode.
            // If the work is done right, this latency mode is not need really.
            #if (!XBOX)
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            #endif
            //TestGarbageCollection.CreateWeakReference();

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
            // Update frame time
            Time.FrameTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);
            
            // Draw 2D Heads Up Display
            SpriteManager.Begin();
            {
                HudText currentHudText;
                for (int i = 0; i < HudText.HudTextPool2D.Count; i++)
                {
                    currentHudText = HudText.HudTextPool2D.Elements[i];
                    if (currentHudText.Visible)
                    {
                        currentHudText.Text.Length = 4;
                        currentHudText.Text.AppendWithoutGarbage(Time.FramesPerSecond);
                        SpriteManager.DrawText(currentHudText.Font,
                                               currentHudText.Text,
                                               currentHudText.CachedPosition,
                                               currentHudText.Color,
                                               currentHudText.CachedRotation,
                                               Vector2.Zero,
                                               currentHudText.CachedScale);
                    }
                }
            }
            SpriteManager.End();

            //model = new FileModel("LamborghiniMurcielago\\Murcielago-Body");

            #region Foreach vs. for test

            //int vertexCount = 0;
            /*
            for (int i = 0; i < 10000; i++)
            {
                foreach (ModelMesh mesh in model.XnaModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        vertexCount += part.NumVertices;
                    }
                }
            }/*
            for (int i = 0; i < 100000; i++)
            {
                for (int j = 0; j < model.XnaModel.Meshes.Count; j++)
                {
                    ModelMesh mesh = model.XnaModel.Meshes[j];
                    for (int k = 0; k < mesh.MeshParts.Count; k++)
                    {
                        ModelMeshPart part = mesh.MeshParts[k];
                        vertexCount += part.NumVertices;
                    }
                }
            }*/
            /*
            for (int i = 0; i < 100000; i++)
            {
                for (int j = 0; j < model.XnaModel.Meshes.Count; j++)
                {
                    for (int k = 0; k < model.XnaModel.Meshes[j].MeshParts.Count; k++)
                    {
                        vertexCount += model.XnaModel.Meshes[j].MeshParts[k].NumVertices;
                    }
                }
            }*/
            /*
            int count = model.XnaModel.Meshes.Count;
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    ModelMesh mesh = model.XnaModel.Meshes[j];
                    int countparts = mesh.MeshParts.Count;
                    for (int k = 0; k < countparts; k++)
                    {
                        vertexCount += mesh.MeshParts[k].NumVertices;
                    }
                }
            }*/

            #endregion

        } // Draw

        #endregion

        #region Unload Content

        internal static void UnloadContent()
        {

        } // UnloadContent

        #endregion

    } // GameLoop
} // XNAFinalEngine.EngineCore