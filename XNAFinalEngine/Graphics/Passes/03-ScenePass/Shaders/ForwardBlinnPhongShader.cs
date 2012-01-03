
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
using XNAFinalEngine.Helpers;
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Forward Blinn Phong Shader.
    /// Alpha blending doesn't work very well with a deferred rendering.
    /// There are a couple of ways to fix this but neither is completely elegant or easy to implement.
    /// Working with a forward rendering for transparent objects is the common solution, K Buffer is another solution but not so common, and there are others.
    /// I choose the forward rendering alternative, but of course the light management curse is back to wreak more horror ;) 
    /// 
    /// Instead of trying to achieve a flexible transparent system,
    /// you can try to identify the game requirement and make a transparent shader that fits better your necessities.
    /// I make one for my requirements and I suppose that it will be helpful for the majority of you. 
    /// 
    /// Thank Søren for the help and tips.
    /// </summary>
    internal class ForwardBlinnPhongShader : Shader
    {

        #region Variables
        
        // Current view and projection matrix. Used to set the shader parameters.
        private Matrix viewMatrix, projectionMatrix;

        // It's an auxiliary structure that helps avoiding garbage.
        private readonly Vector3[] coeficients = new Vector3[9];

        // Singleton reference.
        private static ForwardBlinnPhongShader instance;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a Blinn Phong shader.
        /// </summary>
        public static ForwardBlinnPhongShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new ForwardBlinnPhongShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               epHalfPixel,
                               epCameraPosition,
                               // Matrices //
                               epWorldViewProj,
                               epWorld,
                               epWorldIT,
                               // Surface //
                               epDiffuseColor,
                               epSpecularIntensity,
                               epSpecularPower,
                               epReflectionTextured,
                               epReflectionTexture,
                               epIsRGBM,
                               epMaxRange,
                               epAlphaBlending,
                               // Lights //
                               epAmbientIntensity,
                               epSphericalHarmonicBase,
                               epAmbientColor,
                               epHasAmbientSphericalHarmonics,
                               epDirectionalLightDirection,
                               epDirectionalLightColor,
                               epDirectionalLightIntensity,
                               // Skinning
                               epBones;
                               

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

        #region Camera Position

        private static Vector3? lastUsedCameraPosition;
        private static void SetCameraPosition(Vector3 cameraPosition)
        {
            if (lastUsedCameraPosition != cameraPosition)
            {
                lastUsedCameraPosition = cameraPosition;
                epCameraPosition.SetValue(cameraPosition);
            }
        } // SetCameraPosition

        #endregion

        // Matrices //

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

        #region Transpose Inverse World Matrix

        private static Matrix? lastUsedTransposeInverseWorldMatrix;
        private static void SetTransposeInverseWorldMatrix(Matrix transposeInverseWorldMatrix)
        {
            if (lastUsedTransposeInverseWorldMatrix != transposeInverseWorldMatrix)
            {
                lastUsedTransposeInverseWorldMatrix = transposeInverseWorldMatrix;
                epWorldIT.SetValue(transposeInverseWorldMatrix);
            }
        } // SetTransposeInverseWorldMatrix

        #endregion

        #region World Matrix

        private static Matrix? lastUsedWorldMatrix;
        private static void SetWorldMatrix(Matrix worldMatrix)
        {
            if (lastUsedWorldMatrix != worldMatrix)
            {
                lastUsedWorldMatrix = worldMatrix;
                epWorld.SetValue(worldMatrix);
            }
        } // SetWorldMatrix

        #endregion

        // Surface //
        
        #region Diffuse Color

        private static Color? lastUsedDiffuseColor;
        private static void SetDiffuseColor(Color diffuseColor)
        {
            if (lastUsedDiffuseColor != diffuseColor)
            {
                lastUsedDiffuseColor = diffuseColor;
                epDiffuseColor.SetValue(new Vector3(diffuseColor.R / 255f, diffuseColor.G / 255f, diffuseColor.B / 255f));
            }
        } // SetDiffuseColor

        #endregion

        #region Specular Intensity

        private static float? lastUsedSpecularIntensity;
        private static void SetSpecularIntensity(float specularIntensity)
        {
            if (lastUsedSpecularIntensity != specularIntensity)
            {
                lastUsedSpecularIntensity = specularIntensity;
                epSpecularIntensity.SetValue(specularIntensity);
            }
        } // SetSpecularIntensity

        #endregion

        #region Specular Power

        private static float? lastUsedSpecularPower;
        private static void SetSpecularPower(float specularPower)
        {
            if (lastUsedSpecularPower != specularPower)
            {
                lastUsedSpecularPower = specularPower;
                epSpecularPower.SetValue(specularPower);
            }
        } // SetSpecularPower

        #endregion

        #region Reflection Textured

        private static bool lastUsedReflectionTextured;
        private static void SetReflectionTextured(bool reflectionTextured)
        {
            if (lastUsedReflectionTextured != reflectionTextured)
            {
                lastUsedReflectionTextured = reflectionTextured;
                epReflectionTextured.SetValue(reflectionTextured);
            }
        } // SetReflectionTextured

        #endregion
        
        #region Reflection Texture

        private static Microsoft.Xna.Framework.Graphics.TextureCube lastUsedReflectionTexture;
        private static void SetReflectionTexture(TextureCube reflectionTexture)
        {
            EngineManager.Device.SamplerStates[4] = SamplerState.LinearClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedReflectionTexture != reflectionTexture.Resource)
            {
                lastUsedReflectionTexture = reflectionTexture.Resource;
                if (reflectionTexture.IsRgbm)
                {
                    epIsRGBM.SetValue(true);
                    epMaxRange.SetValue(reflectionTexture.RgbmMaxRange);
                }
                else
                    epIsRGBM.SetValue(false);
                epReflectionTexture.SetValue(reflectionTexture.Resource);
            }
        } // SetReflectionTexture

        #endregion

        #region Alpha Blending

        private static float? lastUsedAlphaBlending;
        private static void SetAlphaBlending(float alphaBlending)
        {
            if (lastUsedAlphaBlending != alphaBlending)
            {
                lastUsedAlphaBlending = alphaBlending;
                epAlphaBlending.SetValue(alphaBlending);
            }
        } // SetAlphaBlending

        #endregion

        // Lights //

        #region Ambient Intensity

        private static float? lastUsedAmbientIntensity;
        private static void SetAmbientIntensity(float ambientIntensity)
        {
            if (lastUsedAmbientIntensity != ambientIntensity)
            {
                lastUsedAmbientIntensity = ambientIntensity;
                epAmbientIntensity.SetValue(ambientIntensity);
            }
        } // SetAmbientIntensity

        #endregion

        #region Spherical Harmonic Base
        
        private static readonly Vector3[] lastUsedSphericalHarmonicBase = new Vector3[9];
        private static void SetSphericalHarmonicBase(Vector3[] sphericalHarmonicBase)
        {
            if (!ArrayHelper.Equals(lastUsedSphericalHarmonicBase, sphericalHarmonicBase))
            {
                //lastUsedSphericalHarmonicBase = (Vector3[])(sphericalHarmonicBase.Clone()); // Produces garbage
                for (int i = 0; i < 9; i++)
                {
                    lastUsedSphericalHarmonicBase[i] = sphericalHarmonicBase[i];
                }
                epSphericalHarmonicBase.SetValue(sphericalHarmonicBase);
            }
        } // SetSphericalHarmonicBase

        #endregion

        #region Ambient Color

        private static Color? lastUsedAmbientColor;
        private static void SetAmbientColor(Color ambientColor)
        {
            if (lastUsedAmbientColor != ambientColor)
            {
                lastUsedAmbientColor = ambientColor;
                epAmbientColor.SetValue(new Vector3(ambientColor.R / 255f, ambientColor.G / 255f, ambientColor.B / 255f));
            }
        } // SetAmbientColor

        #endregion

        #region Has Ambient Spherical Harmonics

        private static bool lastUsedHasAmbientSphericalHarmonics;
        private static void SetHasAmbientSphericalHarmonics(bool hasAmbientSphericalHarmonics)
        {
            if (lastUsedHasAmbientSphericalHarmonics != hasAmbientSphericalHarmonics)
            {
                lastUsedHasAmbientSphericalHarmonics = hasAmbientSphericalHarmonics;
                epHasAmbientSphericalHarmonics.SetValue(hasAmbientSphericalHarmonics);
            }
        } // SetHasAmbientSphericalHarmonics

        #endregion

        #region Directional Light Color

        private static Color? lastUsedDirectionalLightColor;
        private static void SetDirectionalLightColor(Color directionalLightColor)
        {
            if (lastUsedDirectionalLightColor != directionalLightColor)
            {
                lastUsedDirectionalLightColor = directionalLightColor;
                epDirectionalLightColor.SetValue(new Vector3(directionalLightColor.R / 255f, directionalLightColor.G / 255f, directionalLightColor.B / 255f));
            }
        } // SetDirectionalLightColor

        #endregion

        #region Directional Light Direction

        private static Vector3? lastUsedDirectionalLightDirection;
        private static void SetDirectionalLightDirection(Vector3 directionalLightDirection)
        {
            if (lastUsedDirectionalLightDirection != directionalLightDirection)
            {
                lastUsedDirectionalLightDirection = directionalLightDirection;
                epDirectionalLightDirection.SetValue(directionalLightDirection);
            }
        } // SetDirectionalLightDirection

        #endregion

        #region Directional Light Intensity

        private static float? lastUsedDirectionalLightIntensity;
        private static void SetDirectionalLightIntensity(float directionalLightIntensity)
        {
            if (lastUsedDirectionalLightIntensity != directionalLightIntensity)
            {
                lastUsedDirectionalLightIntensity = directionalLightIntensity;
                epDirectionalLightIntensity.SetValue(directionalLightIntensity);
            }
        } // SetDirectionalLightIntensity

        #endregion

        // Skinning

        #region Bones

        //private static Matrix[] lastUsedBones;
        private static void SetBones(Matrix[] bones)
        {
            // The values are probably different and the operation is costly and garbage prone (but this can be avoided).
            /*if (!ArrayHelper.Equals(lastUsedBones, bones))
            {
                lastUsedBones = (Matrix[])(bones.Clone());
                epBones.SetValue(bones);
            }*/
            epBones.SetValue(bones);
        } // SetBones

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Forward Blinn Phong Shader.
		/// </summary>
        internal ForwardBlinnPhongShader() : base("Materials\\ForwardBlinnPhong") { }

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
                // Matrices //
                epWorldViewProj        = Resource.Parameters["worldViewProj"];
                epWorld                = Resource.Parameters["world"];
                epWorldIT              = Resource.Parameters["worldIT"];
                // Parameters //
                epHalfPixel            = Resource.Parameters["halfPixel"];
                epCameraPosition       = Resource.Parameters["cameraPosition"];
                epSpecularIntensity    = Resource.Parameters["specularIntensity"];
                epSpecularPower        = Resource.Parameters["specularPower"];
                epDiffuseColor         = Resource.Parameters["diffuseColor"];
                epReflectionTexture    = Resource.Parameters["reflectionTexture"];
                epReflectionTextured   = Resource.Parameters["reflectionTextured"];
                epIsRGBM               = Resource.Parameters["isRGBM"];
                epMaxRange             = Resource.Parameters["maxRange"];
                epAlphaBlending        = Resource.Parameters["alphaBlending"];
                // Lights //
                epSphericalHarmonicBase        = Resource.Parameters["sphericalHarmonicBase"];
                epAmbientIntensity             = Resource.Parameters["ambientIntensity"];
                epAmbientColor                 = Resource.Parameters["ambientColor"];
                epHasAmbientSphericalHarmonics = Resource.Parameters["hasAmbientSphericalHarmonics"];
                epDirectionalLightDirection    = Resource.Parameters["directionalLightDirection"];
                epDirectionalLightColor        = Resource.Parameters["directionalLightColor"];
                epDirectionalLightIntensity    = Resource.Parameters["directionalLightIntensity"];
                // Skinning //
                epBones                        = Resource.Parameters["Bones"];
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
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix, RenderTarget lightTexture)
        {
            try
            {
                // Set Render States.
                EngineManager.Device.BlendState = BlendState.NonPremultiplied;
                EngineManager.Device.DepthStencilState = DepthStencilState.DepthRead;
                EngineManager.Device.RasterizerState = RasterizerState.CullNone;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.

                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                SetHalfPixel(new Vector2(0.5f / lightTexture.Width, 0.5f / lightTexture.Height));
                SetCameraPosition(Matrix.Invert(viewMatrix).Translation); // Tener cuidado con esto.
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Blinn Phong Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render Model

        /// <summary>
        /// Render a model.
		/// </summary>
        internal void RenderModel(Matrix worldMatrix, Model model, Matrix[] boneTransform, BlinnPhong blinnPhongMaterial, AmbientLight ambientLight)
        {
            try
            {
                bool isSkinned = false;
                if (model is FileModel && ((FileModel)model).IsSkinned) // If it is a skinned model.
                {
                    SetBones(((FileModel)model).SkinTransforms);
                    isSkinned = true;
                }
                SetWorldViewProjMatrix(worldMatrix * viewMatrix * projectionMatrix);
                SetWorldMatrix(worldMatrix);
                SetTransposeInverseWorldMatrix(Matrix.Transpose(Matrix.Invert(worldMatrix)));

                SetSpecularIntensity(blinnPhongMaterial.SpecularIntensity);
                SetSpecularPower(blinnPhongMaterial.SpecularPower);
                SetDiffuseColor(blinnPhongMaterial.DiffuseColor);
                SetAlphaBlending(blinnPhongMaterial.AlphaBlending);
                
                if (blinnPhongMaterial.ReflectionTexture != null)
                {
                    SetReflectionTexture(blinnPhongMaterial.ReflectionTexture);
                    SetReflectionTextured(true);
                }
                else
                    SetReflectionTextured(false);
                // Lights //
                SetAmbientColor(ambientLight.Color);
                SetAmbientIntensity(ambientLight.Intensity);
                
                if (ambientLight.SphericalHarmonicLighting == null)
                {
                    SetHasAmbientSphericalHarmonics(false);
                }
                else
                {
                    ambientLight.SphericalHarmonicLighting.GetCoeficients(coeficients);
                    SetHasAmbientSphericalHarmonics(true);
                    SetSphericalHarmonicBase(coeficients);
                }
                SetDirectionalLightColor(new Color(210, 200, 200));
                SetDirectionalLightDirection(new Vector3(-0.3f, -0.3f, -0.5f));
                SetDirectionalLightIntensity(1.7f);
                /*}
                else
                {
                    SetDirectionalLightColor(Color.Black);
                }*/

                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Forward Bling Phong Material: Unable to render model.", e);
            }
        } // RenderModel

		#endregion        
        
    } // BlinnPhongShader
} // XNAFinalEngine.Graphics

