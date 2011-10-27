
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Ambient Light.
    /// </summary>
    public static class AmbientLight
    {

        #region Variables

        /// <summary>
        /// The ambient light color
        /// When the light pre pass is cleared this color is used to fill the buffer.
        /// The intensity property doesn’t affect this color.
        /// </summary>         
        private static Color color = new Color(0, 0, 0);

        /// <summary>
        /// Light intensity.
        /// The intensity doesn’t affect the color property.
        /// </summary>
        private static float intensity = 0.1f;

        /// <summary>
        /// Ambient Occlusion Strength.
        /// </summary>
        private static float ambientOcclusionStrength = 5;

        #endregion

        #region Properties

        /// <summary>
        /// Spherical Harmonic Ambient Light.
        /// They are great for store low frequency ambient colors and are very fast.
        /// </summary>
        public static SphericalHarmonicL2 SphericalHarmonicAmbientLight { get; set; }

        /// <summary>
        /// Ambient Occlusion Effect.
        /// If null no ambient occlusion will be used.
        /// </summary>
        public static AmbientOcclusion AmbientOcclusion { get; set; }

        /// <summary>
        /// Ambient Occlusion Strength.
        /// </summary>
        public static float AmbientOcclusionStrength
        {
            get { return ambientOcclusionStrength; }
            set { ambientOcclusionStrength = value; }
        } // AmbientOcclusionStrength

        /// <summary>
        /// Light intensity.
        /// The intensity doesn’t affect the color property.
        /// </summary>
        public static float Intensity
        {
            get { return intensity; }
            set
            {
                intensity = value;
                if (intensity < 0)
                    intensity = 0;
            }
        } // Intensity

        /// <summary>
        /// The ambient light color.
        /// When the light pre pass is cleared this color is used to fill the buffer. The intensity property doesn’t affect this color.
        /// </summary>        
        public static Color Color
        {
            get { return color; }
            set { color = value; }
        } // Color

        #endregion

        #region Shader Parameters

        /// <summary>
        /// The shader effect.
        /// </summary>
        private static Effect Effect { get; set; }

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epAmbientOcclusionTexture,
                                       epNormalTexture,
                                       epIntensity,
                                       epViewI,
                                       epSphericalHarmonicBase,
                                       epAmbientOcclusionStrength;

        #region View Projection Matrix

        private static Matrix? lastUsedViewInverseMatrix;
        private static void SetViewInverseMatrix(Matrix viewInverseMatrix)
        {
            if (lastUsedViewInverseMatrix != viewInverseMatrix)
            {
                lastUsedViewInverseMatrix = viewInverseMatrix;
                epViewI.SetValue(viewInverseMatrix);
            }
        } // SetViewInverseMatrix

        #endregion

        #region Spherical Harmonic Base

        private static Vector3[] lastUsedSphericalHarmonicBase;
        private static void SetSphericalHarmonicBase(Vector3[] sphericalHarmonicBase)
        {
            if (!ArrayHelper.Equals(lastUsedSphericalHarmonicBase, sphericalHarmonicBase))
            {
                lastUsedSphericalHarmonicBase = (Vector3[])(sphericalHarmonicBase.Clone());
                epSphericalHarmonicBase.SetValue(sphericalHarmonicBase);
            }
        } // SetSphericalHarmonicBase

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

        #region Ambient Occlusion Texture

        private static Texture lastUsedAmbientOcclusionTexture;
        private static void SetAmbientOcclusionTexture(Texture ambientOcclusionTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedAmbientOcclusionTexture != ambientOcclusionTexture)
            {
                lastUsedAmbientOcclusionTexture = ambientOcclusionTexture;
                epAmbientOcclusionTexture.SetValue(ambientOcclusionTexture.XnaTexture);
            }
        } // SetAmbientOcclusionTexture

        #endregion

        #region Normal Texture

        private static Texture lastUsedNormalTexture;
        private static void SetNormalTexture(Texture normalTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedNormalTexture != normalTexture)
            {
                lastUsedNormalTexture = normalTexture;
                epNormalTexture.SetValue(normalTexture.XnaTexture);
            }
        } // SetNormalTexture

        #endregion

        #region Intensity

        private static float? lastUsedIntensity;
        private static void SetIntensity(float _intensity)
        {
            if (lastUsedIntensity != _intensity)
            {
                lastUsedIntensity = _intensity;
                epIntensity.SetValue(_intensity);
            }
        } // SetIntensity

        #endregion

        #region Ambient Occlusion Strength

        private static float? lastUsedAmbientOcclusionStrength;
        private static void SetAmbientOcclusionStrength(float ambientOcclusionStrength)
        {
            if (lastUsedAmbientOcclusionStrength != ambientOcclusionStrength)
            {
                lastUsedAmbientOcclusionStrength = ambientOcclusionStrength;
                epAmbientOcclusionStrength.SetValue(ambientOcclusionStrength);
            }
        } // SetAmbientOcclusionStrength

        #endregion

        #endregion

        #region Load Shader

        /// <summary>
        /// Load shader
        /// </summary>
        private static void LoadShader()
        {
            const string filename = "LightPrePass\\AmbientLight";
            // Load shader
            try
            {
                Effect = EngineManager.SystemContent.Load<Effect>(Path.Combine(Directories.ShadersDirectory, filename));
            } // try
            catch
            {
                throw new Exception("Unable to load the shader " + filename);
            } // catch
            GetParametersHandles();
        } // LoadShader

        #endregion

        #region Get Parameters Handles

        /// <summary>
        /// Get the handles of the parameters from the shader.
        /// </summary>
        private static void GetParametersHandles()
        {
            try
            {
                epHalfPixel                = Effect.Parameters["halfPixel"];
                epNormalTexture            = Effect.Parameters["normalTexture"];
                epAmbientOcclusionTexture  = Effect.Parameters["ambientOcclusionTexture"];
                epSphericalHarmonicBase    = Effect.Parameters["sphericalHarmonicBase"];
                epIntensity                = Effect.Parameters["intensity"];
                epViewI                    = Effect.Parameters["viewI"];
                epAmbientOcclusionStrength = Effect.Parameters["ambientOcclusionStrength"];
            }
            catch
            {
                throw new Exception("Get the handles from the light pre pass ambient light shader failed.");
            }
        } // GetParameters

        #endregion

        #region Render

        /// <summary>
        /// Render to the light pre pass texture.
        /// </summary>
        public static void Render(RenderTarget normalMap, RenderTarget lightPrePassMap)
        {
            if (Effect == null)
            {
                LoadShader();
                if (SphericalHarmonicAmbientLight == null)
                {
                    SphericalHarmonicAmbientLight = new SphericalHarmonicL2();
                    SphericalHarmonicAmbientLight.Fill(1, 1, 1);
                }
            }
            try
            {

                #region Set Parameters

                SetHalfPixel(new Vector2(-1f / lightPrePassMap.Width, 1f / lightPrePassMap.Height));
                SetNormalTexture(normalMap);
                SetSphericalHarmonicBase(SphericalHarmonicAmbientLight.Coeficients);
                SetIntensity(Intensity);
                SetViewInverseMatrix(Matrix.Invert(Matrix.CreateFromQuaternion(ApplicationLogic.Camera.Orientation)));
                SetAmbientOcclusionStrength(AmbientOcclusionStrength);

                #endregion

                if (AmbientOcclusion == null)
                    Effect.CurrentTechnique = Effect.Techniques["AmbientLightSH"];
                else
                {
                    Effect.CurrentTechnique = Effect.Techniques["AmbientLightSHSSAO"];
                    // And finally to the ambient light shader.
                    SetAmbientOcclusionTexture(AmbientOcclusion.AmbientOcclusionTexture);
                }

                Effect.CurrentTechnique.Passes[0].Apply();
                ScreenPlane.Render();
            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the light pre pass ambient light shader. " + e.Message);
            }
        } // Render

        #endregion

    } // AmbientLight
} // XNAFinalEngine.Graphics
