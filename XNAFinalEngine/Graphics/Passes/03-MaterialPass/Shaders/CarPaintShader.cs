
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
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Car paint shader.
    /// 
    /// This material includes some features to imitate the look of tricked-out car paint.
    /// Some car paint appears to change color based on your viewing angle.
    /// The paint looks like one color when you're looking straight at the surface and another color when 
    /// the surface is parallel to your view. This shader imitates that effect.
    /// </summary>
    internal class CarPaintShader : Shader
    {

        #region Variables
        
        // Current view and projection matrix.
        private Matrix viewProjectionMatrix;

        // Singleton reference.
        private static CarPaintShader instance;

        private static Texture sparkleNoiseTexture;

        // Shader Parameters.
        private static ShaderParameterMatrix spViewInverseMatrix, spWorldViewProjMatrix, spWorldMatrix, spWorldITMatrix;
        private static ShaderParameterTexture spNormalTexture, spDiffuseAccumulationTexture, spSpecularAccumulationTexture, spSparkleNoiseTexture;
        private static ShaderParameterTextureCube spReflectionTexture;
        private static ShaderParameterVector3 spCameraPosition;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterFloat spSpecularIntensity, spMaxRange, spMicroflakePerturbation, spMicroflakePerturbationA, spNormalPerturbation, spFlakeScale, spFlakesExponent;
        private static ShaderParameterBool spIsRGBM;
        private static ShaderParameterColor spBasePaintColor, spSecondBasePaintColor, spThridBasePaintColor, spFlakeLayerColor;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a Blinn Phong shader.
        /// </summary>
        public static CarPaintShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new CarPaintShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Constructor

        private CarPaintShader() : base("Materials\\CarPaint")
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            sparkleNoiseTexture = new Texture("Shaders\\SparkleNoiseMap");
            ContentManager.CurrentContentManager = userContentManager;
        } // CarPaintShader

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
                spSpecularIntensity = new ShaderParameterFloat("specularIntensity", this);
                spMicroflakePerturbation = new ShaderParameterFloat("microflakePerturbation", this);
                spMicroflakePerturbationA = new ShaderParameterFloat("microflakePerturbationA", this);
                spNormalPerturbation = new ShaderParameterFloat("normalPerturbation", this);
                spFlakeScale = new ShaderParameterFloat("flakesScale", this);
                spFlakesExponent = new ShaderParameterFloat("flakesExponent", this);
                spMaxRange = new ShaderParameterFloat("maxRange", this);
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spCameraPosition = new ShaderParameterVector3("cameraPosition", this);
                spWorldMatrix = new ShaderParameterMatrix("world", this);
                spWorldITMatrix = new ShaderParameterMatrix("worldIT", this);
                spWorldViewProjMatrix = new ShaderParameterMatrix("worldViewProj", this);
                spViewInverseMatrix = new ShaderParameterMatrix("viewI", this);
                spBasePaintColor = new ShaderParameterColor("basePaintColor1", this);
                spSecondBasePaintColor = new ShaderParameterColor("basePaintColor2", this);
                spThridBasePaintColor = new ShaderParameterColor("basePaintColor3", this);
                spFlakeLayerColor = new ShaderParameterColor("flakeLayerColor", this);
                spNormalTexture = new ShaderParameterTexture("normalTexture", this, SamplerState.PointClamp, 1);
                spDiffuseAccumulationTexture = new ShaderParameterTexture("diffuseAccumulationTexture", this, SamplerState.PointClamp, 4);
                spSpecularAccumulationTexture = new ShaderParameterTexture("specularAccumulationTexture", this, SamplerState.PointClamp, 5);
                spReflectionTexture = new ShaderParameterTextureCube("reflectionTexture", this, SamplerState.LinearClamp, 3);
                spSparkleNoiseTexture = new ShaderParameterTexture("microflakeMap", this, SamplerState.LinearWrap, 0);
                spIsRGBM = new ShaderParameterBool("isRGBM", this);
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
        internal void Begin(Matrix viewMatrix, Matrix projectionMatrix, Texture diffuseAccumulationTexture, Texture specularAccumulationTexture, Texture normalTexture)
        {
            try
            {
                // Set initial parameters
                Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjectionMatrix);
                spHalfPixel.Value = new Vector2(0.5f / diffuseAccumulationTexture.Width, 0.5f / diffuseAccumulationTexture.Height);
                spCameraPosition.Value = Matrix.Invert(viewMatrix).Translation;
                spDiffuseAccumulationTexture.Value = diffuseAccumulationTexture;
                spSpecularAccumulationTexture.Value = specularAccumulationTexture;
                spSparkleNoiseTexture.Value = sparkleNoiseTexture;
                spNormalTexture.Value = normalTexture;
                spViewInverseMatrix.Value = Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix)));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Car Paint Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render

        /// <summary>
        /// Render a model.
        /// </summary>
        internal void RenderModel(ref Matrix worldMatrix, Model model, CarPaint material, int meshIndex, int meshPart)
        {
            try
            {
                // Matrices
                Matrix worldViewProjection;
                Matrix.Multiply(ref worldMatrix, ref viewProjectionMatrix, out worldViewProjection);
                spWorldViewProjMatrix.QuickSetValue(ref worldViewProjection);
                spWorldMatrix.QuickSetValue(ref worldMatrix);
                spWorldITMatrix.Value = Matrix.Transpose(Matrix.Invert(worldMatrix));
                // Surface //
                spSpecularIntensity.Value = material.SpecularIntensity;
                spBasePaintColor.Value = material.BasePaintColor;
                spSecondBasePaintColor.Value = material.SecondBasePaintColor;
                spThridBasePaintColor.Value = material.ThirdBasePaintColor;
                spFlakeLayerColor.Value = material.FlakesColor;
                spNormalPerturbation.Value = material.NormalPerturbation;
                spMicroflakePerturbation.Value = material.MicroflakePerturbation;
                spMicroflakePerturbationA.Value = material.MicroflakePerturbationA;
                spReflectionTexture.Value = material.ReflectionTexture;
                if (material.ReflectionTexture.IsRgbm)
                {
                    spIsRGBM.Value = true;
                    spMaxRange.Value = material.ReflectionTexture.RgbmMaxRange;
                }
                else
                    spIsRGBM.Value = false;
                spFlakeScale.Value = material.FlakesScale;
                spFlakesExponent.Value = material.FlakesExponent;

                Resource.CurrentTechnique.Passes[0].Apply();
                model.RenderMeshPart(meshIndex, meshPart);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Car Paint Material: Unable to render model.", e);
            }
        } // RenderModel

		#endregion        
        
    } // CarPaintShader
} // XNAFinalEngine.Graphics

