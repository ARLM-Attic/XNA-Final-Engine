
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
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Light Pre Pass Ambient Light Shader.
    /// </summary>
    internal class AmbientLightShader : Shader
    {

        #region Variables

        // Singleton reference.
        private static AmbientLightShader instance;

        // It's an auxiliary structure that helps avoiding garbage.
        private readonly Vector3[] coeficients = new Vector3[9];

        // Shader Parameters.
        private static ShaderParameterFloat spIntensity, spAmbientOcclusionStrength;
        private static ShaderParameterVector2 spHalfPixel;
        private static ShaderParameterColor spColor;
        private static ShaderParameterTexture spAmbientOcclusionTexture, spNormalTexture;
        private static ShaderParameterMatrix spViewInverseMatrix;
        private static ShaderParameterVector3Array spSphericalHarmonicBase;

        // Techniques references.
        private static EffectTechnique ambientLightSphericalHarmonicsTechnique,
                                       ambientLightSphericalHarmonicsAmbientOcclusionTechnique,
                                       ambientLightTechnique,
                                       ambientLightAmbientOcclusionTechnique;

        // State to avoid calculating lighting over the sky.
        private static DepthStencilState avoidSkyDepthStencilState;

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

        #region Constructor

        /// <summary>
        /// Light Pre Pass Ambient Light Shader.
        /// </summary>
        private AmbientLightShader() : base("LightPrePass\\AmbientLight")
        {
            // If the depth is 1 (sky) then I do not calculate the ambient light in this texels.
            avoidSkyDepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false,
                DepthBufferFunction = CompareFunction.NotEqual,
            };
        } // AmbientLightShader

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
                spIntensity = new ShaderParameterFloat("intensity", this);
                spAmbientOcclusionStrength = new ShaderParameterFloat("ambientOcclusionStrength", this);
                spHalfPixel = new ShaderParameterVector2("halfPixel", this);
                spColor = new ShaderParameterColor("color", this);
                spAmbientOcclusionTexture = new ShaderParameterTexture("ambientOcclusionTexture", this, SamplerState.PointClamp, 3);
                spNormalTexture = new ShaderParameterTexture("normalTexture", this, SamplerState.PointClamp, 1);
                spViewInverseMatrix = new ShaderParameterMatrix("viewI", this);
                spSphericalHarmonicBase = new ShaderParameterVector3Array("sphericalHarmonicBase", this, 9);
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
                ambientLightSphericalHarmonicsTechnique = Resource.Techniques["AmbientLightSphericalHarmonics"];
                ambientLightSphericalHarmonicsAmbientOcclusionTechnique = Resource.Techniques["AmbientLightSphericalHarmonicsAmbientOcclusion"];
                ambientLightTechnique = Resource.Techniques["AmbientLight"];
                ambientLightAmbientOcclusionTechnique = Resource.Techniques["AmbientLightAmbientOcclusion"];
            }
            catch
            {
                throw new InvalidOperationException("The technique's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetTechniquesHandles

        #endregion

        #region Render
        
        /// <summary>
        /// Render the ambient light.
        /// </summary>
        public void Render(RenderTarget normalTexture, AmbientLight ambientLight, Texture ambientOcclusionTexture, Matrix viewMatrix)
        {
            try
            {
                // If the depth is 1 (sky) then I do not calculate the ambient light in this texels.
                EngineManager.Device.DepthStencilState = avoidSkyDepthStencilState;

                // Set common parameters.
                spHalfPixel.Value = new Vector2(-0.5f / (normalTexture.Width / 2), 0.5f / (normalTexture.Height / 2));
                spColor.Value = ambientLight.Color;
                spIntensity.Value = ambientLight.Intensity;
                
                // Select Technique and Set Some Related Parameters
                if (ambientLight.SphericalHarmonicLighting != null)
                {
                    spViewInverseMatrix.Value = Matrix.Invert(Matrix.Transpose(Matrix.Invert(viewMatrix)));
                    spNormalTexture.Value = normalTexture;
                    ambientLight.SphericalHarmonicLighting.GetCoeficients(coeficients);
                    spSphericalHarmonicBase.Value = coeficients;
                    // Spherical Harmonics
                    if (ambientLight.AmbientOcclusion == null || !ambientLight.AmbientOcclusion.Enabled)
                        Resource.CurrentTechnique = ambientLightSphericalHarmonicsTechnique;
                    // Spherical Harmonics and Ambient Occlusion
                    else
                    {
                        Resource.CurrentTechnique = ambientLightSphericalHarmonicsAmbientOcclusionTechnique;
                        spAmbientOcclusionTexture.Value = ambientOcclusionTexture;
                        spAmbientOcclusionStrength.Value = ambientLight.AmbientOcclusionStrength;
                    }    
                }
                else
                {
                    // Only color
                    if (ambientLight.AmbientOcclusion == null || !ambientLight.AmbientOcclusion.Enabled)
                        Resource.CurrentTechnique = ambientLightTechnique;
                    // Ambient Occlusion
                    else
                    {
                        Resource.CurrentTechnique = ambientLightAmbientOcclusionTechnique;
                        spAmbientOcclusionTexture.Value = ambientOcclusionTexture;
                        spAmbientOcclusionStrength.Value = ambientLight.AmbientOcclusionStrength;
                    }
                }

                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Light Pre Pass Ambient Light: Unable to render.", e);
            }
        } // Render

        #endregion

    } // AmbientLightShader
} // XNAFinalEngine.Graphics