
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
using Keyboard = XNAFinalEngine.Input.Keyboard;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// Empty scene.
    /// </summary>
    public class HelloWorldScene : Scene
    {

        #region Variables

        private GameObject3D camera, body, directionalLight;
        private GameObject2D helloWorldText;

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method.</remarks>
        public override void LoadContent()
        {
            // Hello World
            helloWorldText = new GameObject2D();
            helloWorldText.AddComponent<HudText>();
            helloWorldText.HudText.Text.Append("Hello World");
            helloWorldText.Transform.LocalPosition = new Vector3(10, 10, 0);
            // Creating a 3D World
            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            /*body = new GameObject3D();
            body.AddComponent<ModelFilter>();
            body.ModelFilter.Model = new FileModel("LamborghiniMurcielago\\Murcielago-Body");
            body.AddComponent<ModelRenderer>();
            body.ModelRenderer.Material = new Constant();*/
            body = new GameObject3D(new FileModel("LamborghiniMurcielago\\Murcielago-Body"), new Constant());
            camera.Transform.LookAt(new Vector3(5, 0, 10), Vector3.Zero, Vector3.Up);
            
            body.ModelRenderer.Material = new BlinnPhong();
            ((BlinnPhong)body.ModelRenderer.Material).DiffuseColor = Color.Yellow;
            directionalLight = new GameObject3D();
            directionalLight.AddComponent<DirectionalLight>();
            directionalLight.DirectionalLight.Color = new Color(250, 250, 140);
            directionalLight.DirectionalLight.Intensity = 1f;
            directionalLight.Transform.LookAt(new Vector3(0.5f, 0.65f, 1.3f), Vector3.Zero, Vector3.Forward);

            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            
            base.LoadContent();
        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        public override void UpdateTasks()
        {
            if (Keyboard.KeyJustPressed(Keys.Space))
            {
                if (directionalLight.DirectionalLight.Intensity == 1)
                    directionalLight.DirectionalLight.Intensity = 4f;
                else
                    directionalLight.DirectionalLight.Intensity = 1f;
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

    } // HelloWorldScene
} // XNAFinalEngineExamples
