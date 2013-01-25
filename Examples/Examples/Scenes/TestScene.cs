
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
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// I use it to make quick tests.
    /// </summary>
    public class TestScene : Scene
    {

        #region Variables
        
        private static SoundEffectInstance[] soundEffectInstances;

        #endregion

        #region Load
        
        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        protected override void LoadContent()
        {

            #region Sound Instance Test

            // Just a test to see exactly how it works internally.
            Sound sound = new Sound("Tutorials\\Dog");
            soundEffectInstances = new SoundEffectInstance[400];
            for (int i = 0; i < 400; i++)
            {
                soundEffectInstances[i] = sound.Resource.CreateInstance();
            }

            #endregion

            base.LoadContent();
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        protected override void UpdateTasks()
        {
            
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

            #region XBOX Matrix test
            /*
            Matrix toto = Matrix.Identity;
            Matrix pepe = Matrix.Identity;
            for (int i = 0; i < 10000; i++)
            {                
                Matrix.Multiply(ref toto, ref pepe, out pepe);
            }/*
            Matrix toto = Matrix.Identity;
            Matrix pepe = Matrix.Identity;
            for (int i = 0; i < 10000; i++)
            {
                pepe = Matrix.Multiply(toto, pepe);
            }*/

            #endregion

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

        } // PreRenderTasks

        #endregion

    } // TestScene
} // XNAFinalEngineExamples
