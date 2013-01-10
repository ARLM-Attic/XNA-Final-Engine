
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
using XNAFinalEngine.Helpers;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using Model = XNAFinalEngine.Assets.Model;
using RenderTargetCube = XNAFinalEngine.Assets.RenderTargetCube;
#endregion

namespace XNAFinalEngine.Graphics
{

	/// <summary>
	/// All shadow map techniques requiere to create a depth buffer from the light position.
	/// </summary>
    internal class LightDepthBufferShader : Shader
	{

		#region Variables
        
        // Light Matrices.
        private Matrix lightViewProjectionMatrix;

        // The result is stored here.
        private RenderTarget lightDepthTexture;

        // or here if it is a cube render target.
        private RenderTargetCube lightDepthTextureCube;

        // Singleton reference.
        private static LightDepthBufferShader instance;

        // Shader Parameters.
        private static ShaderParameterFloat spLightRadius;
        private static ShaderParameterVector3 spLightPosition;
        private static ShaderParameterMatrix spWorldMatrix, spWorldViewProjMatrix;
        private static ShaderParameterMatrixArray spBones;

        // Techniques references.
        private static EffectTechnique generateLightDepthBufferSkinnedTechnique,
                                       generateLightDepthBufferTechnique,
                                       generateCubeLightDepthBufferSkinnedTechnique,
                                       generateCubeLightDepthBufferTechnique;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static LightDepthBufferShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new LightDepthBufferShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        /// <summary>
        /// All shadow map techniques requiere to create a depth buffer from the light position.
        /// </summary>
        private LightDepthBufferShader() : base("Shadows\\LightDepthBuffer") { }

		#endregion

		#region Get parameters handles

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
                spLightRadius = new ShaderParameterFloat("lightRadius", this);
                spLightPosition = new ShaderParameterVector3("lightPosition", this);
                spWorldMatrix = new ShaderParameterMatrix("world", this);
                spWorldViewProjMatrix = new ShaderParameterMatrix("worldViewProj", this);
                spBones = new ShaderParameterMatrixArray("Bones", this, ModelAnimationClip.MaxBones);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

		#endregion

        #region Get Techniques Handles

        /// <summary>
        /// Get the handles of the techniques from the shader.
        /// </summary>
        /// <remarks>
        /// Creating and assigning a EffectParameter instance for each technique in your Effect is significantly faster than using the Parameters indexed property on Effect.
        /// </remarks>
        protected override void GetTechniquesHandles()
        {
            try
            {
                generateLightDepthBufferSkinnedTechnique = Resource.Techniques["GenerateLightDepthBufferSkinned"];
                generateLightDepthBufferTechnique = Resource.Techniques["GenerateLightDepthBuffer"];
                generateCubeLightDepthBufferSkinnedTechnique = Resource.Techniques["GenerateCubeLightDepthBufferSkinned"];
                generateCubeLightDepthBufferTechnique = Resource.Techniques["GenerateCubeLightDepthBuffer"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

        #endregion

        #region Begin

        /// <summary>
        /// Begins the rendering of the depth information from the light point of view.
        /// </summary>
        internal void Begin(Size lightDepthTextureSize)
        {
            try
            {
                // Fetch the render target.
                lightDepthTexture = RenderTarget.Fetch(lightDepthTextureSize, SurfaceFormat.HalfSingle, DepthFormat.Depth16, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                
                // Enable first render target.
                lightDepthTexture.EnableRenderTarget();
                lightDepthTexture.Clear(Color.White);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to begin the rendering.", e);
            }
        } // Begin

        /// <summary>
        /// Begins the rendering of the depth information from the light point of view over a cube render target. 
        /// </summary>
        internal void Begin(int lightDepthTextureSize, Vector3 lightPosition, float lightRadius)
        {
            try
            {
                // Creates the render target textures
                lightDepthTextureCube = RenderTargetCube.Fetch(lightDepthTextureSize, SurfaceFormat.HalfSingle, DepthFormat.Depth16, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                
                spLightPosition.Value = lightPosition;
                spLightRadius.Value = lightRadius;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Shadow Map Shader: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Set Light Matrices

        /// <summary>
        /// Set light view and projection matrices.
        /// </summary>
        public void SetLightMatrices(Matrix lightViewMatrix, Matrix lightProjectionMatrix)
        {
            lightViewProjectionMatrix = lightViewMatrix * lightProjectionMatrix;
        } // SetLightMatrices

        #endregion

        #region Set Viewport

        /// <summary>
        /// Set viewport for cascaded shadows.
        /// </summary>
        internal void SetViewport(int splitNumber)
        {
            // Set the viewport for the current split.
            Viewport splitViewport = new Viewport
            {
                MinDepth = 0,
                MaxDepth = 1,
                X = splitNumber * lightDepthTexture.Height,
                Y = 0,
                Width = lightDepthTexture.Height,
                Height = lightDepthTexture.Height
            };
            EngineManager.Device.Viewport = splitViewport;
        } // SetViewport

        #endregion    

        #region Set Face

        /// <summary>
        /// Set face for cube shadows.
        /// </summary>
        internal void SetFace(CubeMapFace cubeMapFace)
        {
            // Enable first render target.
            lightDepthTextureCube.EnableRenderTarget(cubeMapFace);
            lightDepthTextureCube.Clear(Color.White);
        } // SetViewport

        /// <summary>
        /// Unset current face.
        /// </summary>
        internal void UnsetCurrentFace()
        {
            lightDepthTextureCube.DisableRenderTarget();
        } // UnsetCurrentFace

        #endregion

        #region Render Model

        /// <summary>
        /// Render objects in light space.
        /// </summary>
        internal void RenderModel(ref Matrix worldMatrix, Model model, Matrix[] boneTransform)
        {
            if (model.IsSkinned) // If it is a skinned model.
            {
                spBones.Value = boneTransform;
                Resource.CurrentTechnique = generateLightDepthBufferSkinnedTechnique;
            }
            else
                Resource.CurrentTechnique = generateLightDepthBufferTechnique;

            Matrix worldLightProjectionMatrix;
            Matrix.Multiply(ref worldMatrix, ref lightViewProjectionMatrix, out worldLightProjectionMatrix);

            if (boneTransform == null || model.IsSkinned)
            {
                spWorldViewProjMatrix.Value = worldLightProjectionMatrix;
                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
            else
            {
                for (int mesh = 0; mesh < model.MeshesCount; mesh++)
                {
                    Matrix boneTransformedMatrix;
                    Matrix.Multiply(ref boneTransform[mesh + 1], ref worldLightProjectionMatrix, out boneTransformedMatrix);
                    Resource.CurrentTechnique.Passes[0].Apply();
                    // Render the model's mesh.
                    int meshPartsCount = model.MeshPartsCountPerMesh[mesh];
                    for (int meshPart = 0; meshPart < meshPartsCount; meshPart++)
                    {
                        model.RenderMeshPart(mesh, meshPart);
                    }
                }
            }
        } // RenderModel

        /// <summary>
        /// Render objects in light space.
        /// </summary>
        internal void RenderModelCubeShadows(Matrix worldMatrix, Model model, Matrix[] boneTransform)
        {
            if (model.IsSkinned) // If it is a skinned model.
            {
                spBones.Value = boneTransform;
                Resource.CurrentTechnique = generateCubeLightDepthBufferSkinnedTechnique;
            }
            else
                Resource.CurrentTechnique = generateCubeLightDepthBufferTechnique;

            Matrix worldLightProjectionMatrix;
            Matrix.Multiply(ref worldMatrix, ref lightViewProjectionMatrix, out worldLightProjectionMatrix);

            // Simple and skinned.
            if (boneTransform == null || model.IsSkinned)
            {
                spWorldViewProjMatrix.Value = worldLightProjectionMatrix;
                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
            // Animated Rigid Models.
            else
            {
                
                for (int mesh = 0; mesh < model.MeshesCount; mesh++)
                {
                    Matrix boneTransformedMatrix;
                    Matrix.Multiply(ref boneTransform[mesh + 1], ref worldLightProjectionMatrix, out boneTransformedMatrix);
                    Resource.CurrentTechnique.Passes[0].Apply();
                    // Render the model's mesh.
                    int meshPartsCount = model.MeshPartsCountPerMesh[mesh];
                    for (int meshPart = 0; meshPart < meshPartsCount; meshPart++)
                    {
                        model.RenderMeshPart(mesh, meshPart);
                    }
                }
            }
        } // RenderModel

        #endregion
        
        #region End

        /// <summary>
        /// Resolve and return the render target with the depth information from the light point of view.
        /// </summary>
        internal RenderTarget End()
        {
            try
            {
                lightDepthTexture.DisableRenderTarget();
                return lightDepthTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Depth Buffer Shader: Unable to end the rendering.", e);
            }
        } // End

        /// <summary>
        /// Resolve and return the render target with the depth information from the light point of view.
        /// </summary>
        internal RenderTargetCube EndCube()
        {
            try
            {
                return lightDepthTextureCube;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Depth Buffer Shader: Unable to end the rendering.", e);
            }
        } // EndCube

        #endregion

    } // LightDepthBufferShader
} // XNAFinalEngine.Graphics
