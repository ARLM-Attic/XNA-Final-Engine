
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
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Constant Shader.
    /// </summary>
    internal class ConstantShader : Shader
    {

        #region Variables

        // Current view and projection matrix. Used to set the shader parameters.
        private Matrix viewProjectionMatrix;

        // Singleton reference.
        private static ConstantShader instance;

        // Shader Parameters.
        private static ShaderParameterFloat spAlphaBlending;
        private static ShaderParameterMatrix spWorldViewProjMatrix;
        private static ShaderParameterMatrixArray spBones;
        private static ShaderParameterColor spDiffuseColor;
        private static ShaderParameterTexture spDiffuseTexture;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a Constant shader.
        /// </summary>
        public static ConstantShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new ConstantShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
		/// Constant shader.
		/// </summary>
        private ConstantShader() : base("Materials\\Constant") { }

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
                spAlphaBlending = new ShaderParameterFloat("alphaBlending", this);
                spWorldViewProjMatrix = new ShaderParameterMatrix("worldViewProj", this);
                spDiffuseColor = new ShaderParameterColor("diffuseColor", this);
                spDiffuseTexture = new ShaderParameterTexture("diffuseTexture", this, SamplerState.AnisotropicWrap, 0);
                spBones = new ShaderParameterMatrixArray("Bones", this, ModelAnimationClip.MaxBones);
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
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix)
        {
            try
            {
                // Set initial parameters
                Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjectionMatrix);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model using the simple technique.
		/// </summary>		
        internal void RenderModelSimple(ref Matrix worldMatrix, Model model, Constant material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = Resource.Techniques["ConstantSimple"];
                // Matrix
                Matrix worldViewProjection;
                Matrix.Multiply(ref worldMatrix, ref viewProjectionMatrix, out worldViewProjection);
                spWorldViewProjMatrix.QuickSetValue(ref worldViewProjection);
                // Diffuse
                if (material.DiffuseTexture == null)
                {
                    spDiffuseColor.Value = material.DiffuseColor;
                    spDiffuseTexture.Value = Texture.BlackTexture;
                }
                else
                {
                    spDiffuseColor.Value = Color.Black;
                    spDiffuseTexture.Value = material.DiffuseTexture;
                }
                spAlphaBlending.Value = material.AlphaBlending;

                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to render model.", e);
            }
        } // RenderModelSimple

        /// <summary>
        /// Render a skinned model.
        /// </summary>		
        internal void RenderModelSkinned(ref Matrix worldMatrix, Model model, Matrix[] boneTransform, Constant material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = Resource.Techniques["ConstantSkinned"];
                // Skinned
                spBones.Value = boneTransform;
                // Matrix
                Matrix worldViewProjection;
                Matrix.Multiply(ref worldMatrix, ref viewProjectionMatrix, out worldViewProjection);
                spWorldViewProjMatrix.QuickSetValue(ref worldViewProjection);
                // Diffuse
                if (material.DiffuseTexture == null)
                {
                    spDiffuseColor.Value = material.DiffuseColor;
                    spDiffuseTexture.Value = Texture.BlackTexture;
                }
                else
                {
                    spDiffuseColor.Value = Color.Black;
                    spDiffuseTexture.Value = material.DiffuseTexture;
                }
                spAlphaBlending.Value = material.AlphaBlending;

                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Constant Material: Unable to render model.", e);
            }
        } // RenderModelSkinned

		#endregion

    } // ConstantShader
} // XNAFinalEngine.Graphics

