
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Ambient Light.
    /// </summary>
    internal class AmbientLightShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static AmbientLightShader instance;

        // It's an auxiliary structure that helps avoiding garbage.
        private readonly Vector3[] coeficients = new Vector3[9];

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of a ambient light shader.
        /// </summary>
        public static AmbientLightShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new AmbientLightShader();
                return instance;
            }
        } // Instance

        #endregion

        #region Shader Parameters
        
        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epColor,
                                       epAmbientOcclusionTexture,
                                       epNormalTexture,
                                       epIntensity,
                                       epViewI,
                                       epSphericalHarmonicBase,
                                       epAmbientOcclusionStrength;

        #region View Projection Matrix

        private static Matrix lastUsedViewInverseMatrix;
        private static void SetViewInverseMatrix(Matrix viewInverseMatrix)
        {
            if (lastUsedViewInverseMatrix != viewInverseMatrix)
            {
                lastUsedViewInverseMatrix = viewInverseMatrix;
                epViewI.SetValue(viewInverseMatrix);
            }
        } // SetViewInverseMatrix

        #endregion

        #region Color

        private static Color lastUsedColor;
        private static void SetColor(Color color)
        {
            if (lastUsedColor != color)
            {
                lastUsedColor = color;
                epColor.SetValue(new Vector3(color.R / 255f, color.G / 255f, color.B / 255f));
            }
        } // SetColor

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

        #region Half Pixel

        private static Vector2 lastUsedHalfPixel;
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

        private static Texture2D lastUsedAmbientOcclusionTexture;
        private static void SetAmbientOcclusionTexture(Texture ambientOcclusionTexture)
        {
            EngineManager.Device.SamplerStates[3] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedAmbientOcclusionTexture != ambientOcclusionTexture.Resource)
            {
                lastUsedAmbientOcclusionTexture = ambientOcclusionTexture.Resource;
                epAmbientOcclusionTexture.SetValue(ambientOcclusionTexture.Resource);
            }
        } // SetAmbientOcclusionTexture

        #endregion

        #region Normal Texture

        private static Texture2D lastUsedNormalTexture;
        private static void SetNormalTexture(Texture normalTexture)
        {
            EngineManager.Device.SamplerStates[1] = SamplerState.PointClamp;
            // It’s not enough to compare the assets, the resources has to be different because the resources could be regenerated when a device is lost.
            if (lastUsedNormalTexture != normalTexture.Resource)
            {
                lastUsedNormalTexture = normalTexture.Resource;
                epNormalTexture.SetValue(normalTexture.Resource);
            }
        } // SetNormalTexture

        #endregion

        #region Intensity

        private static float lastUsedIntensity;
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

        private static float lastUsedAmbientOcclusionStrength;
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

        #region Constructor

        /// <summary>
        /// Light Pre Pass Directional Light Shader.
        /// </summary>
        private AmbientLightShader() : base("LightPrePass\\AmbientLight") { }

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
                epHalfPixel                = Resource.Parameters["halfPixel"];
                    epHalfPixel.SetValue(lastUsedHalfPixel);
                epColor                    = Resource.Parameters["color"];
                epColor.SetValue(new Vector3(lastUsedColor.R / 255f, lastUsedColor.G / 255f, lastUsedColor.B / 255f));
                epNormalTexture            = Resource.Parameters["normalTexture"];
                    if (lastUsedNormalTexture != null && !lastUsedNormalTexture.IsDisposed)
                        epNormalTexture.SetValue(lastUsedNormalTexture);
                epAmbientOcclusionTexture  = Resource.Parameters["ambientOcclusionTexture"];
                    if (lastUsedAmbientOcclusionTexture != null && !lastUsedAmbientOcclusionTexture.IsDisposed)
                        epAmbientOcclusionTexture.SetValue(lastUsedAmbientOcclusionTexture);
                epSphericalHarmonicBase    = Resource.Parameters["sphericalHarmonicBase"];
                    epSphericalHarmonicBase.SetValue(lastUsedSphericalHarmonicBase);
                epIntensity                = Resource.Parameters["intensity"];
                    epIntensity.SetValue(lastUsedIntensity);
                epViewI                    = Resource.Parameters["viewI"];
                    epViewI.SetValue(lastUsedViewInverseMatrix);
                epAmbientOcclusionStrength = Resource.Parameters["ambientOcclusionStrength"];
                    epAmbientOcclusionStrength.SetValue(lastUsedAmbientOcclusionStrength);
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Render Light
        
        /// <summary>
        /// Render to the light pre pass texture.
        /// </summary>
        public void RenderLight(RenderTarget normalTexture, AmbientLight ambientLight, Texture ambientOcclusionTexture, Matrix viewMatrix)
        {
            
            try
            {

                #region Set Parameters
                
                SetHalfPixel(new Vector2(-1f / normalTexture.Width, 1f / normalTexture.Height));
                SetColor(ambientLight.Color);
                SetIntensity(ambientLight.Intensity);
                
                #endregion

                #region Select Technique and Set Some Related Parameters

                if (ambientLight.SphericalHarmonicLighting != null)
                {
                    SetViewInverseMatrix(Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix))));
                    SetNormalTexture(normalTexture);
                    ambientLight.SphericalHarmonicLighting.GetCoeficients(coeficients);
                    SetSphericalHarmonicBase(coeficients);
                    // Spherical Harmonics
                    if (ambientLight.AmbientOcclusion == null || !ambientLight.AmbientOcclusion.Enabled)
                        Resource.CurrentTechnique = Resource.Techniques["AmbientLightSphericalHarmonics"];
                    // Spherical Harmonics and Ambient Occlusion
                    else
                    {
                        Resource.CurrentTechnique = Resource.Techniques["AmbientLightSphericalHarmonicsAmbientOcclusion"];
                        SetAmbientOcclusionTexture(ambientOcclusionTexture);
                        SetAmbientOcclusionStrength(ambientLight.AmbientOcclusionStrength);
                    }    
                }
                else
                {
                    // Only color
                    if (ambientLight.AmbientOcclusion == null || !ambientLight.AmbientOcclusion.Enabled)
                        Resource.CurrentTechnique = Resource.Techniques["AmbientLight"];
                    // Ambient Occlusion
                    else
                    {
                        Resource.CurrentTechnique = Resource.Techniques["AmbientLightAmbientOcclusion"];
                        SetAmbientOcclusionTexture(ambientOcclusionTexture);
                        SetAmbientOcclusionStrength(ambientLight.AmbientOcclusionStrength);
                    }
                }

                #endregion

                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Ambient Light: Unable to render.", e);
            }
        } // RenderLight

        #endregion

    } // AmbientLightShader
} // XNAFinalEngine.Graphics
