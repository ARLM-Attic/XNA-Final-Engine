
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

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Base class for materials.
    /// A material can be render; to do this job it takes an object model, its associated lights, and its matrices.
    /// It also has the possibility of being modified in real time with the help of the engine UI.
    /// </summary>
    public abstract class Material
    {

        #region Variables

        /// <summary>
        /// The count of materials for naming porpouses.
        /// </summary>
        private static int nameNumber = 1;

        /// <summary>
        /// Is the configuration window open?
        /// </summary>
        protected bool configurationWindowOpen;

        #endregion

        #region Properties

        /// <summary>
        /// The XNA shader effect.
        /// </summary>
        public Effect Effect { get; protected set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Load Shader

        /// <summary>
        /// Load the shader.
        /// </summary>
        protected Effect LoadShader(string filename)
        {
            Name = "Material" + nameNumber;
            nameNumber++;
            // Load shader
            try
            {
                return EngineManager.SystemContent.Load<Effect>(Path.Combine(Directories.ShadersDirectory, filename));
            } // try
            catch
            {
                throw new Exception("Unable to load the shader " + filename);
            } // catch            
        } // LoadShader

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
        protected virtual void GetParametersHandles()
        {
            // Overrite it //
        } // GetParametersHandles

        #endregion

        #region Render

        /// <summary>
        /// Render this shader/material; to do this job it takes an object model, its associated lights, and its matrices.
        /// </summary>
        internal virtual void Render(Matrix worldMatrix, PointLight[] pointLight, DirectionalLight[] directionalLight, SpotLight[] spotLight, Model model)
        {
            // Overrite it //
        } // Render

        /// <summary>
        /// Render this shader/material. This method is used by this class's children
        /// </summary>
        protected void Render(Model model)
        {
            try
            {
                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    model.Render();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to render the shader. " + e.Message);
            }
        } // Render

        #endregion

        #region Configuration Window

        /// <summary>
        /// Open the configuration window of this material.
        /// </summary>
        public virtual void OpenConfigurationWindow()
        {
            // Overrite it!!
        } // OpenConfigurationWindow

        #endregion

    } // Material
} // XNAFinalEngine.Graphics