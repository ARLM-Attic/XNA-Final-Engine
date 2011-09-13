
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Base class for shaders.
    /// </summary>
    /// <remarks>
    /// Shader assets are not useful for the user. 
    /// We could easily do a version that does not load the shader twice if the shader is already loaded, just like the XNA Final Engine old versions.
    /// But now the shaders are managed internally by the Graphic System and few creations are need.
    /// </remarks>    
    public abstract class Shader : Asset
    {

        #region Properties

        /// <summary>
        /// The shader effect.
        /// </summary>
        public Effect Effect { get; private set; }

        #endregion

        #region Load Shader

        /// <summary>
        /// Load the shader.
        /// </summary>
        /// <remarks>
        /// All shaders are loaded only once and into the System Component Manager.
        /// </remarks>
        protected Shader(string filename)
        {            
            Name = filename;
            string fullFilename = ContentManager.GameDataDirectory + "Shaders\\" + filename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load shader: File " + fullFilename + " does not exists!", "filename");
            }
            try
            {
                Effect = ContentManager.SystemContentManager.XnaContentManager.Load<Effect>(fullFilename);
            }
            catch (ObjectDisposedException)
            {
                throw new InvalidOperationException("Content Manager: Content manager disposed");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load shader: " + filename, e);
            }
            // Get the handles of the parameters from the shader.
            GetParametersHandles();
        } // Shader

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected virtual void GetParametersHandles()
        {
            try
            {
                // Overrite it!!
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParametersHandles

        #endregion

    } // Shader
} // XNAFinalEngine.Assets