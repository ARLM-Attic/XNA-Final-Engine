
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// This shader generates a depth map, a normal map, a specular power map and a motion vectors map.
    /// It stores the result in several render targets (depth texture, normal texture, and motion vector and specular power texture).
    /// The depth texture has a surface format of 32 bits single channel precision, and the normal has a half vector 2 format (r16f g16f). 
    /// The normals are store with spherical coordinates and the depth is store using the equation: -DepthVS / FarPlane.
    /// </summary>
    internal class GBufferShader : Shader
    {

        #region Variables

        // Cached values to improve performance.
        private Matrix viewMatrix, projectionMatrix;

        // Singleton reference.
        private static GBufferShader instance;

        // This texture has the pre-calculated results of a costly function.
        // The precision is related to texture resolution.
        private static Texture normalsFittingTexture;

        // Shader Parameters.
        private static ShaderParameterFloat spFarPlane, spSpecularPower, spHeightMapScale;
        private static ShaderParameterMatrix spWorldViewProj, spWorldView, spWorldViewIT;
        private static ShaderParameterMatrixArray spBones;
        private static ShaderParameterBool spSpecularTextured;
        private static ShaderParameterInt spLODThreshold, spMinimumNumberSamples, spMaximumNumberSamples;
        private static ShaderParameterVector2 spObjectNormalTextureSize;
        private static ShaderParameterTexture spObjectNormalTexture, spObjectSpecularTexture, spNormalsFittingTexture;

        // Techniques references.
        private static EffectTechnique gBufferSimpleTechnique,
                                       gBufferWithNormalMap,
                                       gBufferWithParallax,
                                       gBufferSkinnedSimple,
                                       gBufferSkinnedWithNormalMap;
        
        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a G-Buffer shader.
        /// </summary>
        public static GBufferShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new GBufferShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructors

        /// <summary>
        /// This shader generates the G-Buffer of the deferred lighting.
        /// The depth texture has a surface format of 32 bits single channel precision. Equation: -DepthVS / FarPlane
        /// The normals are store using best fit normals for maximum compression (24 bits), and are stored in view space, 
        /// but best fit normals works better in world space, this is specially noticed in the presence of big triangles. 
        /// The specular power is stored in 8 bits following Killzone 2 method.
        /// There is room in the depth surface to store a mask for ambient lighting (Crysis 2 and Toy Story 3 method).
        /// </summary>
        internal GBufferShader() : base("GBuffer\\GBuffer")
        {
            AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
            AssetContentManager.CurrentContentManager = AssetContentManager.SystemContentManager;
            // Set the random normal map. Helps to make the samplers more random.
            #if (WINDOWS)
                normalsFittingTexture = new Texture("Shaders\\NormalsFitting1024");
            #else
                normalsFittingTexture = new Texture("Shaders\\NormalsFitting512");
            #endif
            AssetContentManager.CurrentContentManager = userContentManager;
        } // GBufferShader

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
                // Matrices //
                spWorldViewProj = new ShaderParameterMatrix("worldViewProj", this);
                spWorldView = new ShaderParameterMatrix("worldView", this);
                spWorldViewIT = new ShaderParameterMatrix("worldViewIT", this);
                // Floats //
                spFarPlane = new ShaderParameterFloat("farPlane", this);
                spSpecularPower = new ShaderParameterFloat("specularPower", this);
                spHeightMapScale = new ShaderParameterFloat("heightMapScale", this);
                // Bool //
                spSpecularTextured = new ShaderParameterBool("specularTextured", this);
                // Ints //
                spLODThreshold = new ShaderParameterInt("LODThreshold", this);
                spMinimumNumberSamples = new ShaderParameterInt("minimumNumberSamples", this);
                spMaximumNumberSamples = new ShaderParameterInt("maximumNumberSamples", this);
                 // Vector2 //
                spObjectNormalTextureSize = new ShaderParameterVector2("objectNormalTextureSize", this);
                // Textures //
                spObjectNormalTexture = new ShaderParameterTexture("objectNormalTexture", this, SamplerState.AnisotropicWrap, 0);
                spObjectSpecularTexture = new ShaderParameterTexture("objectSpecularTexture", this, SamplerState.LinearWrap, 1);
                spNormalsFittingTexture = new ShaderParameterTexture("normalsFittingTexture", this, SamplerState.LinearClamp, 7);
                // Skinning //
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
                gBufferSimpleTechnique      = Resource.Techniques["GBufferSimple"];
                gBufferWithNormalMap        = Resource.Techniques["GBufferWithNormalMap"];
                gBufferWithParallax         = Resource.Techniques["GBufferWithParallax"];
                gBufferSkinnedSimple        = Resource.Techniques["GBufferSkinnedSimple"];
                gBufferSkinnedWithNormalMap = Resource.Techniques["GBufferSkinnedWithNormalMap"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

        #endregion

        #region Begin

        /// <summary>
        /// Begins the G Buffer rendering.
        /// </summary>
        internal void Begin(Matrix _viewMatrix, Matrix _projectionMatrix, float farPlane)
        {
            try
            {
                viewMatrix = _viewMatrix;
                projectionMatrix = _projectionMatrix;
                spFarPlane.Value = farPlane;
                spNormalsFittingTexture.Value = normalsFittingTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("G-Buffer: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion
        
        #region Render Model

        /// <summary>
        /// Common parameters to all techniques.
        /// </summary>
        private void SetCommonParameters(ref Matrix worldMatrix, Material material)
        {
            // Set Matrices
            // World View
            Matrix worldViewMatrix;
            Matrix.Multiply(ref worldMatrix, ref viewMatrix, out worldViewMatrix);
            spWorldView.QuickSetValue(ref worldViewMatrix);
            // World View IT
            Matrix worldViewITMatrix, worldViewIMatrix;
            Matrix.Invert(ref worldViewMatrix, out worldViewIMatrix);
            Matrix.Transpose(ref worldViewIMatrix, out worldViewITMatrix);
            spWorldViewIT.QuickSetValue(ref worldViewITMatrix);
            // World View Proj
            Matrix worldViewProjMatrix;
            Matrix.Multiply(ref worldViewMatrix, ref projectionMatrix, out worldViewProjMatrix);
            spWorldViewProj.QuickSetValue(ref worldViewProjMatrix);

            // Specular texture
            if (material.SpecularTexture != null && material.SpecularPowerFromTexture)
            {
                spObjectSpecularTexture.Value = material.SpecularTexture;
                spSpecularTextured.Value = true;
            }
            else
            {
                spObjectSpecularTexture.Value = Texture.BlackTexture; // To avoid a potential exception.
                spSpecularPower.Value = material.SpecularPower;
                spSpecularTextured.Value = false;
            }
        } // SetCommonParameters

        /// <summary>
        /// Begins the G Buffer simple technique.
        /// </summary>
        internal void RenderModelSimple(ref Matrix worldMatrix, Assets.Model model, Material material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = gBufferSimpleTechnique; // Does not produce a graphic call.
                SetCommonParameters(ref worldMatrix, material);
                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("G-Buffer: Unable to render model.", e);
            }
        } // RenderModelSimple

        /// <summary>
        /// Begins the G Buffer "with normals" technique.
        /// </summary>
        internal void RenderModelWithNormals(ref Matrix worldMatrix, Assets.Model model, Material material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = gBufferWithNormalMap;
                SetCommonParameters(ref worldMatrix, material);
                spObjectNormalTexture.Value = material.NormalTexture;
                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("G-Buffer: Unable to render model.", e);
            }
        } // RenderModelWithNormals

        /// <summary>
        /// Begins the G Buffer "with parallax" technique.
        /// </summary>
        internal void RenderModelWithParallax(ref Matrix worldMatrix, Assets.Model model, Material material, int meshIndex, int meshPart)
        {
            try
            {
                Resource.CurrentTechnique = gBufferWithParallax;
                SetCommonParameters(ref worldMatrix, material);
                spObjectNormalTexture.Value = material.NormalTexture;
                spObjectNormalTextureSize.Value = new Vector2(material.NormalTexture.Width, material.NormalTexture.Height);
                spLODThreshold.Value = material.ParallaxLodThreshold;
                spMinimumNumberSamples.Value = material.ParallaxMinimumNumberSamples;
                spMaximumNumberSamples.Value = material.ParallaxMaximumNumberSamples;
                spHeightMapScale.Value = material.ParallaxHeightMapScale;
                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("G-Buffer: Unable to render model.", e);
            }
        } // RenderModelWithParallax

        /// <summary>
        /// Begins the G Buffer "skinned simple" technique.
        /// </summary>
        internal void RenderModelSkinnedSimple(ref Matrix worldMatrix, Assets.Model model, Material material, int meshIndex, int meshPart, Matrix[] boneTransform)
        {
            try
            {
                Resource.CurrentTechnique = gBufferSkinnedSimple;
                SetCommonParameters(ref worldMatrix, material);
                spBones.Value = boneTransform;
                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("G-Buffer: Unable to render model.", e);
            }
        } // RenderModelSkinnedSimple

        /// <summary>
        /// Begins the G Buffer "skinned normals" technique.
        /// </summary>
        internal void RenderModelSkinnedWithNormals(ref Matrix worldMatrix, Assets.Model model, Material material, int meshIndex, int meshPart, Matrix[] boneTransform)
        {
            try
            {
                Resource.CurrentTechnique = gBufferSkinnedWithNormalMap;
                SetCommonParameters(ref worldMatrix, material);
                spObjectNormalTexture.Value = material.NormalTexture;
                spBones.Value = boneTransform;
                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("G-Buffer: Unable to render model.", e);
            }
        } // RenderModelSkinnedWithNormals

        #endregion

    } // GBufferShader
} // XNAFinalEngine.Graphics
