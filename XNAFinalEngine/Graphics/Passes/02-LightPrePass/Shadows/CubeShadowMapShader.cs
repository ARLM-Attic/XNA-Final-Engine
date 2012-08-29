
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
using System;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Render point light shadows from all directions using the cube shadow technique.
    /// </summary>
    internal class CubeShadowMapShader
	{

		#region Variables

        /// <summary>
        /// Light Projection Matrix.
        /// </summary>
        private Matrix lightProjectionMatrix;

        /// <summary>
        /// Light View Matrices (one for each cube face).
        /// </summary>
        private readonly Matrix[] lightViewMatrix = new Matrix[6];

        // Singleton reference.
        private static CubeShadowMapShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// Light Projection Matrix.
        /// </summary>
        public Matrix LightProjectionMatrix { get { return lightProjectionMatrix; } }

        /// <summary>
        /// Light Projection Matrices (one for each face).
        /// </summary>
        public Matrix[] LightViewMatrix { get { return lightViewMatrix; } }

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static CubeShadowMapShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new CubeShadowMapShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Render point light shadows from all directions using the cube shadow technique. 
        /// </summary>
        private CubeShadowMapShader() { }

		#endregion
        
        #region Set Light

        /// <summary>
        /// Determines the size of the frustum needed to cover the viewable area, then creates the light view matrix and an appropriate orthographic projection.
        /// </summary>
        internal void SetLight(Vector3 position, float range)
        {
            // Calculate all six face direction light-view-projection matrix for cube.
            lightProjectionMatrix = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 1, 1, range);

            // Loop through the six faces of the cube map.
            for (int i = 0; i < 6; ++i)
            {
                // Standard view that will be overridden below.
                Vector3 eyePosition = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 lookatPosition, upVector;

                switch (i)
                {
                    case 0: // X+
                        lookatPosition = new Vector3(1.0f, 0.0f, 0.0f);
                        upVector    = new Vector3(0.0f, 1.0f, 0.0f);
                        break;
                    case 1: // X-
                        lookatPosition = new Vector3(-1.0f, 0.0f, 0.0f);
                        upVector    = new Vector3(0.0f, 1.0f, 0.0f);
                        break;
                    case 2: // Y+
                        lookatPosition = new Vector3(0.0f, 1.0f, 0.0f);
                        upVector    = new Vector3(0.0f, 0.0f, -1.0f);
                        break;
                    case 3: // Y-
                        lookatPosition = new Vector3(0.0f, -1.0f, 0.0f);
                        upVector    = new Vector3(0.0f, 0.0f, 1.0f);
                        break;
                    case 4: // Z-
                        lookatPosition = new Vector3(0.0f, 0.0f, 1.0f);
                        upVector    = new Vector3(0.0f, 1.0f, 0.0f);
                        break;
                    default: // Z+
                        lookatPosition = new Vector3(0.0f, 0.0f, -1.0f);
                        upVector    = new Vector3(0.0f, 1.0f, 0.0f);
                        break;
                }
                lightViewMatrix[i] = Matrix.CreateLookAt(eyePosition + position, lookatPosition + position, upVector);
            }
        } // SetLight

        #endregion
        
    } // CubeShadowMapShader
} // XNAFinalEngine.Graphics
