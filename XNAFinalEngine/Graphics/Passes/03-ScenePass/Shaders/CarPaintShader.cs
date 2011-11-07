
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
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
using TextureCube = XNAFinalEngine.Assets.TextureCube;
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
    public class CarPaintShader : Shader
    {

        #region Variables
        
        // Current view and projection matrix. Used to set the shader parameters.
        private Matrix viewMatrix, projectionMatrix;

        // Singleton reference.
        private static CarPaintShader instance;

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

        #region Shader Parameters

        /// <summary>
        /// Effect handles for this shader.
        /// </summary>
        protected static EffectParameter
                               epHalfPixel,
                               epLightMapTexture,
                               epCameraPosition,
                               // Matrices //
                               epWorldViewProj,
                               epWorld,
                               epWorldIT,
                               // Parameters //
                               epSpecularIntensity,
                               epBasePaintColor,
                               epLightedPaintColor,
                               epMiddlePaintColor,
                               epFlakeLayerColor,
                               epMicroflakePerturbation,
                               epMicroflakePerturbationA,
                               epNormalPerturbation,
                               epReflectionTexture,
                               epIsRGBM,
                               epMaxRange;

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

        #region Light Texture

        private static Texture2D lastUsedLightTexture;
        private static void SetLightTexture(Texture lightTexture)
        {
            EngineManager.Device.SamplerStates[0] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedLightTexture != lightTexture.Resource)
            {
                lastUsedLightTexture = lightTexture.Resource;
                epLightMapTexture.SetValue(lightTexture.Resource);
            }
        } // SetLightTexture

        #endregion

        #region World View Projection Matrix

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjMatrix)
            {
                lastUsedWorldViewProjMatrix = worldViewProjMatrix;
                epWorldViewProj.SetValue(worldViewProjMatrix);
            }
        } // SetWorldViewProjMatrix

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
        } // SetWorld

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

        #region Base Paint Color

        private static Color? lastUsedBasePaintColor;
        private static void SetBasePaintColor(Color basePaintColor)
        {
            if (lastUsedBasePaintColor != basePaintColor)
            {
                lastUsedBasePaintColor = basePaintColor;
                epBasePaintColor.SetValue(new Vector3(basePaintColor.R / 255f, basePaintColor.G / 255f, basePaintColor.B / 255f));
            }
        } // SetBasePaintColor

        #endregion

        #region Lighted Paint Color

        private static Color? lastUsedLightedPaintColor;
        private static void SetLightedPaintColor(Color lightedPaintColor)
        {
            if (lastUsedLightedPaintColor != lightedPaintColor)
            {
                lastUsedLightedPaintColor = lightedPaintColor;
                epLightedPaintColor.SetValue(new Vector3(lightedPaintColor.R / 255f, lightedPaintColor.G / 255f, lightedPaintColor.B / 255f));
            }
        } // SetLightedPaintColor

        #endregion

        #region Middle Paint Color

        private static Color? lastUsedMiddlePaintColor;
        private static void SetMiddlePaintColor(Color middlePaintColor)
        {
            if (lastUsedMiddlePaintColor != middlePaintColor)
            {
                lastUsedMiddlePaintColor = middlePaintColor;
                epMiddlePaintColor.SetValue(new Vector3(middlePaintColor.R / 255f, middlePaintColor.G / 255f, middlePaintColor.B / 255f));
            }
        } // SetMiddlePaintColor

        #endregion

        #region Flake Layer Color

        private static Color? lastUsedFlakeLayerColor;
        private static void SetFlakeLayerColor(Color flakeLayerColor)
        {
            if (lastUsedFlakeLayerColor != flakeLayerColor)
            {
                lastUsedFlakeLayerColor = flakeLayerColor;
                epFlakeLayerColor.SetValue(new Vector3(flakeLayerColor.R / 255f, flakeLayerColor.G / 255f, flakeLayerColor.B / 255f));
            }
        } // SetFlakeLayerColor

        #endregion

        #region Microflake Perturbation

        private static float? lastUsedMicroflakePerturbation;
        private static void SetMicroflakePerturbation(float microflakePerturbation)
        {
            if (lastUsedMicroflakePerturbation != microflakePerturbation)
            {
                lastUsedMicroflakePerturbation = microflakePerturbation;
                epMicroflakePerturbation.SetValue(microflakePerturbation);
            }
        } // SetMicroflakePerturbation

        #endregion

        #region Microflake Perturbation A

        private static float? lastUsedMicroflakePerturbationA;
        private static void SetMicroflakePerturbationA(float microflakePerturbationA)
        {
            if (lastUsedMicroflakePerturbationA != microflakePerturbationA)
            {
                lastUsedMicroflakePerturbationA = microflakePerturbationA;
                epMicroflakePerturbationA.SetValue(microflakePerturbationA);
            }
        } // SetMicroflakePerturbationA

        #endregion

        #region Normal Perturbation

        private static float? lastUsedNormalPerturbation;
        private static void SetNormalPerturbation(float normalPerturbation)
        {
            if (lastUsedNormalPerturbation != normalPerturbation)
            {
                lastUsedNormalPerturbation = normalPerturbation;
                epNormalPerturbation.SetValue(normalPerturbation);
            }
        } // SetNormalPerturbation

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

        #endregion

        #region Constructor

        public CarPaintShader() : base("Materials\\CarPaint")
        {
            Texture sparkleNoiseMap = new Texture("Shaders\\SparkleNoiseMap");
            Resource.Parameters["microflakeMap"].SetValue(sparkleNoiseMap.Resource);
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
                epHalfPixel         = Resource.Parameters["halfPixel"];
                epLightMapTexture   = Resource.Parameters["lightMap"];
                epCameraPosition    = Resource.Parameters["cameraPosition"];
                // Matrices
                epWorldViewProj     = Resource.Parameters["worldViewProj"];
                epWorld             = Resource.Parameters["world"];
                epWorldIT           = Resource.Parameters["worldIT"];
                // Parameters
                epSpecularIntensity       = Resource.Parameters["specularIntensity"];
                epBasePaintColor          = Resource.Parameters["basePaintColor"];
                epLightedPaintColor       = Resource.Parameters["lightedPaintColor"];
                epMiddlePaintColor        = Resource.Parameters["middlePaintColor"];
                epFlakeLayerColor         = Resource.Parameters["flakeLayerColor"];
                epMicroflakePerturbation  = Resource.Parameters["microflakePerturbation"];
                epMicroflakePerturbationA = Resource.Parameters["microflakePerturbationA"];
                epNormalPerturbation      = Resource.Parameters["normalPerturbation"];
                epReflectionTexture       = Resource.Parameters["reflectionTexture"];
                epIsRGBM                  = Resource.Parameters["isRGBM"];
                epMaxRange                = Resource.Parameters["maxRange"];
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
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.DepthStencilState = DepthStencilState.Default;
                // If I set the sampler states here and no texture is set then this could produce exceptions 
                // because another texture from another shader could have an incorrect sampler state when this shader is executed.
                // But, if someone changes the sampler state of the sparkle noise texture could be a problem… in a form of an exception.
                EngineManager.Device.SamplerStates[0] = SamplerState.LinearWrap;

                // Set initial parameters
                this.viewMatrix = viewMatrix;
                this.projectionMatrix = projectionMatrix;
                SetHalfPixel(new Vector2(0.5f / lightTexture.Width, 0.5f / lightTexture.Height));
                SetCameraPosition(Matrix.Invert(viewMatrix).Translation);
                SetLightTexture(lightTexture);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Blinn Phong Material: Unable to begin the rendering.", e);
            }
        } // Begin

        #endregion

        #region Render

        /// <summary>
        /// Render a model.
        /// </summary>
        internal void RenderModel(Matrix worldMatrix, Model model, CarPaint carPaintMaterial)
        {
            try
            {
                #region Set Parameters

                // Matrices //
                SetWorldViewProjMatrix(worldMatrix * viewMatrix * projectionMatrix);
                SetWorldMatrix(worldMatrix);
                SetTransposeInverseWorldMatrix(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                // Parameters //
                SetSpecularIntensity(carPaintMaterial.SpecularIntensity);
                SetBasePaintColor(carPaintMaterial.BasePaintColor);
                SetLightedPaintColor(carPaintMaterial.SecondBasePaintColor);
                SetMiddlePaintColor(carPaintMaterial.MiddlePaintColor);
                SetFlakeLayerColor(carPaintMaterial.FlakeLayerColor);
                SetNormalPerturbation(carPaintMaterial.NormalPerturbation);
                SetMicroflakePerturbation(carPaintMaterial.MicroflakePerturbation);
                SetMicroflakePerturbationA(carPaintMaterial.MicroflakePerturbationA);
                SetReflectionTexture(carPaintMaterial.ReflectionTexture);

                #endregion

                Resource.CurrentTechnique.Passes[0].Apply();
                model.Render();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Car Pain Material: Unable to render model.", e);
            }
        } // RenderModel

		#endregion        
        
    } // CarPaintShader
} // XNAFinalEngine.Graphics

