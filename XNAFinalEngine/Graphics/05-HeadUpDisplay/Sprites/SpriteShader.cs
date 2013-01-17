
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Sprite Shader.
    /// It test the depth using the depth buffer from the G-Buffer.
    /// </summary>    
    internal class SpriteShader : Shader
    {

        #region Variables

        /// <summary>
        /// Current view and projection matrix. Used to set the shader parameters.
        /// </summary>
        private Matrix viewMatrix, projectionMatrix;

        // Singleton reference.
        private static SpriteShader instance;

        // Shader Parameters.
        private static ShaderParameterFloat   spFarPlane;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterTexture spDepthTexture;
        private static ShaderParameterMatrix  spWorldViewProj,
                                              spProjectionInverse;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static SpriteShader Instance { get { return instance ?? (instance = new SpriteShader()); } }

        #endregion

        #region Constructor

        /// <summary>
		/// Sprite shader.
        /// It test the depth using the depth buffer from the G-Buffer.
		/// </summary>
        private SpriteShader() : base("Sprites\\SpriteEffect") { }

		#endregion
        
		#region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
		protected override sealed void GetParametersHandles()
		{
			try
			{
                spFarPlane = new ShaderParameterFloat("farPlane", this);
                spWorldViewProj = new ShaderParameterMatrix("worldViewProj", this);
                spProjectionInverse = new ShaderParameterMatrix("projectionInverse", this);
                spDepthTexture = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 1);
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
		} // GetParametersHandles

		#endregion

        #region Begin

        /// <summary>
        /// Begins the render.
        /// </summary>
        internal void BeginLinearSpace(Matrix viewMatrix, Matrix projectionMatrix)
        {
            try
            {
                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                Resource.CurrentTechnique = Resource.Techniques["SpriteBatchLinearSpace"];
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to begin the rendering.", e);
            }
        } // BeginLinearSpace

        /// <summary>
        /// Begins the render.
        /// </summary>
        internal void BeginGammaSpace(Matrix viewMatrix, Matrix projectionMatrix, Texture depthTexture, float farPlane)
        {
            try
            {
                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                spProjectionInverse.Value = Matrix.Invert(projectionMatrix);
                spDepthTexture.Value = depthTexture;
                spFarPlane.Value = farPlane;
                spHalfPixel.Value = new Vector2(0.5f / depthTexture.Width, 0.5f / depthTexture.Height); // The half depth size produce flickering
                Resource.CurrentTechnique = Resource.Techniques["SpriteBatchGammaSpace"];
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Sprite Shader: Unable to begin the rendering.", e);
            }
        } // BeginGammaSpace

        #endregion

        #region Set Parameters

        /// <summary>
        /// Set parameters.
		/// </summary>		
        internal void SetParameters(Matrix worldMatrix)
        {
            try
            {
                spWorldViewProj.Value = worldMatrix * viewMatrix * projectionMatrix;
                Resource.CurrentTechnique.Passes[0].Apply();                
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Sprite Shader: Unable to set parameters.", e);
            }
        } // SetParameters

		#endregion

    } // SpriteShader
} // XNAFinalEngine.Graphics

