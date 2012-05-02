
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
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
    public class Shader : Asset
    {

        #region Variables

        /// <summary>
        /// Vertex buffer for the screen plane.
        /// </summary>
        private static VertexBuffer vertexBufferScreenPlane;

        #endregion

        #region Properties

        /// <summary>
        /// The shader effect.
        /// </summary>
        public Effect Resource { get; private set; }

        #endregion

        #region Load Shader

        /// <summary>
        /// Load the shader.
        /// </summary>
        /// <remarks>
        /// All shaders are loaded only once and into the System Component Manager.
        /// </remarks>
        public Shader(string filename)
        {            
            Name = filename;
            Filename = ContentManager.GameDataDirectory + "Shaders\\" + filename;
            if (File.Exists(Filename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load shader: File " + Filename + " does not exists!", "filename");
            }
            try
            {
                Resource = ContentManager.SystemContentManager.XnaContentManager.Load<Effect>(Filename);
                ContentManager = ContentManager.SystemContentManager;
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

        #region Render Screen Plane

        /// <summary>
        /// Render a screen plane using this shader.
        /// A screen plane is useful for most screen space shaders.
        /// </summary>
        public void RenderScreenPlane()
        {
            if (vertexBufferScreenPlane != null && vertexBufferScreenPlane.GraphicsDevice.IsDisposed)
            {
                vertexBufferScreenPlane.Dispose();
                vertexBufferScreenPlane = null;
            }
            if (vertexBufferScreenPlane == null)
            {
                VertexPositionTexture[] vertices = new[]
                {
                    new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 0f), new Vector2(0, 1)),
                    new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 0f),  new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(1.0f, -1.0f, 0f),  new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(1.0f, 1.0f, 0f),   new Vector2(1, 0)),
                };
                vertexBufferScreenPlane = new VertexBuffer(EngineManager.Device, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
                vertexBufferScreenPlane.SetData(vertices);
            }
            EngineManager.Device.SetVertexBuffer(vertexBufferScreenPlane);
            EngineManager.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            // Update statistics
            Statistics.DrawCalls++;
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;
        } // RenderScreenPlane

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal override void RecreateResource()
        {
            Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<Effect>(Filename);
            GetParametersHandles();
        } // RecreateResource

        #endregion

    } // Shader
} // XNAFinalEngine.Assets