
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Place the depth information into a GPU’s Z-Buffer.
    /// </summary>
    internal class ReconstructZBufferShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static ReconstructZBufferShader instance;
        
        // Shader Parameters.
        private static ShaderParameterFloat spFarPlane;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterMatrix spProjectionMatrix;
        private static ShaderParameterTexture spDepthTexture;

        private static DepthStencilState depthStencilState;
        private static BlendState blendState;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static ReconstructZBufferShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new ReconstructZBufferShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// Place the depth information into a real GPU’s Z buffer.
        /// </summary>
        private ReconstructZBufferShader() : base("GBuffer\\ReconstructZBuffer")
        {
            blendState = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.None,
            };
            depthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = true,
                DepthBufferFunction = CompareFunction.Always,
            };
        }

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override void GetParametersHandles()
        {
            try
            {
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spFarPlane = new ShaderParameterFloat("farPlane", this);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
                spProjectionMatrix = new ShaderParameterMatrix("projection", this);
                
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Render

        /// <summary>
        /// Place the depth information into the current Z buffer.
        /// </summary>
        internal void Render(RenderTarget depthTexture, float farPlane, Matrix projectionMatrix)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = blendState;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = depthStencilState;

                // Set Parameters
                spHalfPixel.Value = new Vector2(-0.5f / (depthTexture.Width / 2), 0.5f / (depthTexture.Height / 2)); // Use size of destinantion render target.
                spFarPlane.Value = farPlane;
                spProjectionMatrix.Value = projectionMatrix;
                spDepthTexture.Value = depthTexture;

                Resource.CurrentTechnique = Resource.Techniques["ReconstructZBuffer"];
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Reconstruct Z Buffer Shader: Unable to render.", e);
            }
        } // Render

        #endregion

    } // ReconstructZBufferShader
} // XNAFinalEngine.Graphics