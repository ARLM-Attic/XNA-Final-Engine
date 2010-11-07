
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.GraphicElements
{

    /// <summary>
    /// Skybox.
    /// </summary>
    public class PreSkybox : ScreenShader
    {

        #region Variables

        /// <summary>
        /// A box that is the geometry used by the shader.
        /// </summary>
        private Box skybox = new Box(200);

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        protected EffectParameter epViewProjection,
                                  epSkyboxIntensity,
                                  epCubeMapTexture,
                                  epAlphaBlending;

        #region Matrices

        /// <summary>
        /// Last used view projection matrix
        /// </summary>
        private Matrix? lastUsedViewProjectionMatrix = null;
        /// <summary>
        /// Set view projection matrix
        /// </summary>
        private void SetViewProjectionMatrix(Matrix viewProjectionMatrix)
        {
            if (lastUsedViewProjectionMatrix != viewProjectionMatrix)
            {
                lastUsedViewProjectionMatrix = viewProjectionMatrix;
                epViewProjection.SetValue(viewProjectionMatrix);
            }
        } // ViewProjectionMatrix

        #endregion

        #region SkyboxIntesity

        /// <summary>
        /// Skybox Intesity
        /// </summary>
        private float skyboxIntesity = 1.0f;
        /// <summary>
        /// Skybox Intesity
        /// </summary>
        public float SkyboxIntesity
        {
            get { return skyboxIntesity; }
            set { skyboxIntesity = value; }
        } // SkyboxIntesity

        /// <summary>
        /// Last used Skybox Intesity
        /// </summary>
        private static float? lastUsedSkyboxIntesity = null;
        /// <summary>
        /// Set Skybox Intesity (value between 0 and 1)
        /// </summary>
        private void SetSkyboxIntesity(float _skyboxIntesity)
        {
            if (lastUsedSkyboxIntesity != _skyboxIntesity && _skyboxIntesity >= 0.0f && _skyboxIntesity <= 1.0f)
            {
                lastUsedSkyboxIntesity = _skyboxIntesity;
                epSkyboxIntensity.SetValue(_skyboxIntesity);
            }
        } // SetSkyboxIntesity

        #endregion

        #region AlphaBlending
        
        /// <summary>
        /// Alpha blending
        /// </summary>
        private float alphaBlending = 1.0f;

        /// <summary>
        /// Alpha blending
        /// </summary>
        public float AlphaBlending
        {
            get { return alphaBlending; }
            set { alphaBlending = value; }
        } // AlphaBlending

        /// <summary>
        /// Last used alpha blending
        /// </summary>
        private static float lastUsedAlphaBlending = 1.0f;
        /// <summary>
        /// Set surface's alpha blending (value between 0 and 1)
        /// </summary>
        private void SetAlphaBlending(float _alphaBlending)
        {
            if (lastUsedAlphaBlending != _alphaBlending && _alphaBlending >= 0.0f && _alphaBlending <= 1.0f)
            {
                lastUsedAlphaBlending = _alphaBlending;
                epAlphaBlending.SetValue(_alphaBlending);
            }
        } // SetAlphaBlending

        #endregion

        /// <summary>
        /// Last used cube map texture
        /// </summary>
        private TextureCube lastUsedCubeMapTexture = null;
        /// <summary>
        /// Set cube map texture
        /// </summary>
        private void SetCubeMapTexture(TextureCube cubeMapTexture)
        {
            if (cubeMapTexture != null && lastUsedCubeMapTexture != cubeMapTexture)
            {
                lastUsedCubeMapTexture = cubeMapTexture;
                epCubeMapTexture.SetValue(cubeMapTexture);
            }
        } // CubeMapTexture

        #endregion

        #region Constructor

        /// <summary>
        /// Skybox.
        /// </summary>
        /// <param name="_textureFilename">Final texture name is: "Skybox-" + user texture file name</param>
        public PreSkybox(string _textureFilename)
        {
            Effect = LoadShader("PreSkybox");
            Effect.CurrentTechnique = Effect.Techniques["ModelShader30"];
            
            GetParametersHandles();

            // Load the cubic texture
            string fullFilename = Directories.TexturesDirectory + "\\Skybox\\" + _textureFilename;

            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load the skybox texture: File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                if (EngineManager.UsesSystemContent)
                    SetCubeMapTexture(EngineManager.SystemContent.Load<TextureCube>(fullFilename));
                else
                    SetCubeMapTexture(EngineManager.CurrentContent.Load<TextureCube>(fullFilename));     

            } // try
            catch (Exception)
            {
                throw new Exception("Failed to load the skybox texture");
            }
        } // PreSkybox

        #endregion

        #region Get parameters handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        protected virtual void GetParametersHandles()
        {
            try
			{
                epViewProjection = Effect.Parameters["ViewProj"];
                epSkyboxIntensity = Effect.Parameters["SkyboxIntensity"];
                epCubeMapTexture = Effect.Parameters["CubeMapTexture"];
                epAlphaBlending = Effect.Parameters["AlphaBlending"];
            }
            catch
            {
                throw new Exception("Get the handles from the skybox shader failed.");
            }
        } // GetParametersHandles

        #endregion

        #region Set Parameters

        /// <summary>
        /// Set to the shader the specific atributes of this effect.
        /// </summary>
        public virtual void SetParameters()
        {
            // Matrices
            SetViewProjectionMatrix(ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix);
            // Others
            SetSkyboxIntesity(skyboxIntesity);
            SetAlphaBlending(alphaBlending);
        } // SetParameters

        #endregion

        #region Render
        
        /// <summary>
        /// Render the skybox.
        /// </summary>
        public void Render()
        {
            SetParameters();
            try
            {
                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    skybox.Render();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to render the skybox: " + ex.ToString());
            }		
        } // Render

        #endregion

    } // PreSkybox
} // XNAFinalEngine.GraphicElements
