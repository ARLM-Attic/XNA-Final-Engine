
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Sprite Shader.
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

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a Constant shader.
        /// </summary>
        public static SpriteShader Instance { get { return instance ?? (instance = new SpriteShader()); } }

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        private static EffectParameter epWorldViewProj,
                                       epDepthTexture,
                                       epProjectionInverse,
                                       epHalfPixel,
                                       epFarPlane;

        #region World View Projection Matrix

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjMatrix;
                epWorldViewProj.SetValue(worldViewProjMatrix);
            }
        } // WorldViewProjMatrix

        #endregion

        #region Projection Inverse Matrix

        private static Matrix? lastUsedProjectionInverseMatrix;
        private static void SetProjectionInverseMatrix(Matrix projectionInverseMatrix)
        {
            if (lastUsedProjectionInverseMatrix != projectionInverseMatrix)
            {
                lastUsedProjectionInverseMatrix = projectionInverseMatrix;
                epProjectionInverse.SetValue(projectionInverseMatrix);
            }
        } // SetProjectionInverseMatrix

        #endregion

        #region Depth Texture

        private static Texture2D lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            EngineManager.Device.SamplerStates[1] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedDepthTexture != depthTexture.Resource)
            {
                lastUsedDepthTexture = depthTexture.Resource;
                epDepthTexture.SetValue(depthTexture.Resource);
            }
        } // SetDepthTexture

        #endregion

        #region Half Pixel

        private static Vector2? lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region Far Plane

        private static float lastUsedFarPlane;
        private static void SetFarPlane(float _farPlane)
        {
            if (lastUsedFarPlane != _farPlane)
            {
                lastUsedFarPlane = _farPlane;
                epFarPlane.SetValue(_farPlane);
            }
        } // SetFarPlane

        #endregion

        #endregion

        #region Constructor

        /// <summary>
		/// Sprite shader.
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
                epWorldViewProj  = Resource.Parameters["worldViewProj"];
                epProjectionInverse = Resource.Parameters["projectionInverse"];
                epFarPlane = Resource.Parameters["farPlane"];
                epDepthTexture = Resource.Parameters["depthTexture"];
                epHalfPixel = Resource.Parameters["halfPixel"];
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
                SetProjectionInverseMatrix(Matrix.Invert(projectionMatrix));
                SetDepthTexture(depthTexture);
                SetFarPlane(farPlane);
                SetHalfPixel(new Vector2(0.5f / depthTexture.Width, 0.5f / depthTexture.Height)); // The half depth size produce flickering
                Resource.CurrentTechnique = Resource.Techniques["SpriteBatchGammaSpace"];
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to begin the rendering.", e);
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
                SetWorldViewProjMatrix(worldMatrix * viewMatrix * projectionMatrix);
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

