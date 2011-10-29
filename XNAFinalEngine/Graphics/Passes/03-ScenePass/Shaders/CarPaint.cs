
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
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Car paint material.
    /// 
    /// This material includes some features to imitate the look of tricked-out car paint.
    /// Some car paint appears to change color based on your viewing angle.
    /// The paint looks like one color when you're looking straight at the surface and another color when 
    /// the surface is parallel to your view. This shader imitates that effect.
    /// </summary>
    public class CarPaint : SpecularMaterial
    {

        #region Variables

        /// <summary>
        /// The XNA shader effect.
        /// </summary>
        private static Effect effect;

        /// <summary>
        /// Base paint color.
        /// </summary>
        private Color basePaintColor = new Color(0.3f,  0.3f,  0.4f);

        /// <summary>
        /// Second base paint color.
        /// </summary>
        private Color secondBasePaintColor = new Color(0.2f,  0.2f,  0.3f);

        /// <summary>
        /// Middle color.
        /// </summary>
        private Color middlePaintColor = new Color(0.35f, 0.35f, 0.3f);

        /// <summary>
        /// Flake layer color.
        /// </summary>
        private Color flakeLayerColor = new Color(0.4f,  0.4f,  0.4f);
        
        /// <summary>
        /// Microflake Perturbation. Value between -1 and 1.
        /// </summary>
        private float microflakePerturbation = 1.0f;

        /// <summary>
        /// Microflake Perturbation A. Value between 0 and 1.
        /// </summary>
        private float microflakePerturbationA = 0.1f;
        
        /// <summary>
        /// Normal Perturbation. Value between -1 and 1.
        /// </summary>
        private float normalPerturbation = 1.0f;

        #endregion

        #region Properties

        /// <summary>
        /// Microflake Perturbation. Value between -1 and 1.
        /// </summary>
        public float MicroflakePerturbation
        {
            get { return microflakePerturbation; }
            set { microflakePerturbation = value; }
        } // MicroflakePerturbation

        /// <summary>
        /// Microflake Perturbation A. Value between 0 and 1.
        /// </summary>
        public float MicroflakePerturbationA
        {
            get { return microflakePerturbationA; }
            set { microflakePerturbationA = value; }
        } // MicroflakePerturbationA

        /// <summary>
        /// Normal Perturbation. Value between -1 and 1.
        /// </summary>
        public float NormalPerturbation
        {
            get { return normalPerturbation; }
            set { normalPerturbation = value; }
        } // NormalPerturbation

        /// <summary>
        /// Base paint color.
        /// </summary>
        public Color BasePaintColor
        {
            get { return basePaintColor; }
            set { basePaintColor = value; }
        } // BasePaintColor

        /// <summary>
        /// Second base paint color.
        /// </summary>
        public Color SecondBasePaintColor
        {
            get { return secondBasePaintColor; }
            set { secondBasePaintColor = value; }
        } // LightedPaintColor

        /// <summary>
        /// Middle paint color.
        /// </summary>
        public Color MiddlePaintColor
        {
            get { return middlePaintColor; }
            set { middlePaintColor = value; }
        } // MiddlePaintColor

        /// <summary>
        /// Flake layer color.
        /// </summary>
        public Color FlakeLayerColor
        {
            get { return flakeLayerColor; }
            set { flakeLayerColor = value; }
        } // FlakeLayerColor

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

        #region Light Map Texture

        private static Texture lastUsedLightMapTexture;
        private static void SetLightMapTexture(Texture lightMapTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedLightMapTexture != lightMapTexture)
            {
                lastUsedLightMapTexture = lightMapTexture;
                epLightMapTexture.SetValue(lightMapTexture.XnaTexture);
            }
        } // SetLightMapTexture

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

        private static TextureCube lastUsedReflectionTexture;
        private static void SetReflectionTexture(TextureCube reflectionTexture)
        {
            if (lastUsedReflectionTexture != reflectionTexture)
            {
                lastUsedReflectionTexture = reflectionTexture;
                if (reflectionTexture.IsRgbm)
                {
                    epIsRGBM.SetValue(true);
                    epMaxRange.SetValue(reflectionTexture.RgbmMaxRange);
                }
                else
                    epIsRGBM.SetValue(false);
                epReflectionTexture.SetValue(reflectionTexture.XnaTexture);
            }
        } // SetReflectionTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Car Paint Material.
		/// </summary>
		public CarPaint()
		{
            LoadShader("Materials\\CarPaint", ref effect);
        } // CarPaint

		#endregion
        
		#region Get Parameters Handles
		
        /// <summary>
        /// Get the handles of the parameters from the shader.
		/// </summary>
		protected override sealed void GetParametersHandles()
		{
			try
			{
                epHalfPixel         = effect.Parameters["halfPixel"];
                epLightMapTexture   = effect.Parameters["lightMap"];
                epCameraPosition    = effect.Parameters["cameraPosition"];
                // Matrices
                epWorldViewProj     = effect.Parameters["worldViewProj"];
                epWorld             = effect.Parameters["world"];
                epWorldIT           = effect.Parameters["worldIT"];
                // Parameters
                epSpecularIntensity       = effect.Parameters["specularIntensity"];
                epBasePaintColor          = effect.Parameters["basePaintColor"];
                epLightedPaintColor       = effect.Parameters["lightedPaintColor"];
                epMiddlePaintColor        = effect.Parameters["middlePaintColor"];
                epFlakeLayerColor         = effect.Parameters["flakeLayerColor"];
                epMicroflakePerturbation  = effect.Parameters["microflakePerturbation"];
                epMicroflakePerturbationA = effect.Parameters["microflakePerturbationA"];
                epNormalPerturbation      = effect.Parameters["normalPerturbation"];
                epReflectionTexture       = effect.Parameters["reflectionTexture"];
                epIsRGBM                  = effect.Parameters["isRGBM"];
                epMaxRange                = effect.Parameters["maxRange"];

                Texture sparkleNoiseMap = new Texture("Shaders\\SparkleNoiseMap");
                effect.Parameters["microflakeMap"].SetValue(sparkleNoiseMap.XnaTexture);
			}
			catch
			{
                throw new Exception("Get the handles from the car paint material failed.");
			}
		} // GetParametersHandles

		#endregion

        #region Render

        /// <summary>
        /// Render this shader/material.
		/// </summary>		
        internal override void Render(Matrix worldMatrix, Model model)
        {

            #region Set Parameters

            try
            {
                SetHalfPixel(new Vector2(0.5f / DeferredLightingManager.LightMap.Width, 0.5f / DeferredLightingManager.LightMap.Height));
                SetLightMapTexture(DeferredLightingManager.LightMap);
                SetCameraPosition(ApplicationLogic.Camera.Position);
                // Matrices //
                SetWorldViewProjMatrix(worldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix);
                SetWorldMatrix(worldMatrix);
                SetTransposeInverseWorldMatrix(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                // Parameters //
                SetSpecularIntensity(SpecularIntensity);
                SetBasePaintColor(BasePaintColor);
                SetLightedPaintColor(SecondBasePaintColor);
                SetMiddlePaintColor(middlePaintColor);
                SetFlakeLayerColor(flakeLayerColor);
                SetNormalPerturbation(NormalPerturbation);
                SetMicroflakePerturbation(MicroflakePerturbation);
                SetMicroflakePerturbationA(MicroflakePerturbationA);
                SetReflectionTexture(ReflectionTexture);
            }
            catch
            {
                throw new Exception("Unable to set the car paint shader parameters.");
            }
            
            #endregion

            Render(model, effect);
        } // Render

		#endregion        
        
    } // CarPaint
} // XNAFinalEngine.Graphics

