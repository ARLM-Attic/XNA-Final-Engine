
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
using Texture = Microsoft.Xna.Framework.Graphics.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Bloom shader.
    /// The effect produces fringes (or feathers) of light around very bright objects in an image.
    /// Basically, if an object has a light behind it, the light will look more realistic and will somewhat overlap the front of the object from the 3rd person perspective. 
    /// http://en.wikipedia.org/wiki/Bloom_(shader_effect)
    /// </summary>
    public class BloomShader : Shader
    {

        #region Variables

        /// <summary>
        /// Bloom Texture.
        /// </summary>
        private RenderTarget bloomTexture;

        #endregion

        #region Properties

        /// <summary>
        /// Bloom Texture.
        /// </summary>
        public RenderTarget BloomTexture { get { return bloomTexture; } }

        #endregion

        #region Shader Parameters

        /// <summary>
        /// Effect handles
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epBloomThreshold,
                                       epLensExposure,
                                       epSceneTexture;


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

        #region Bloom Threshold

        private static float? lastUsedBloomThreshold;
        private static void SetBloomThreshold(float bloomThreshold)
        {
            if (lastUsedBloomThreshold != bloomThreshold)
            {
                lastUsedBloomThreshold = bloomThreshold;
                epBloomThreshold.SetValue(bloomThreshold);
            }
        } // SetBloomThreshold

        #endregion

        #region Lens Exposure
        
        private static float? lastUsedLensExposure;
        private static void SetLensExposure(float lensExposure)
        {
            if (lastUsedLensExposure != lensExposure)
            {
                lastUsedLensExposure = lensExposure;
                epLensExposure.SetValue(lensExposure);
            }
        } // SetLensExposure

        #endregion

        #region Scene Texture
        
        private static Texture lastUsedSceneTexture;
        private static void SetSceneTexture(Texture sceneTexture)
        {
            if (EngineManager.DeviceLostInThisFrame || lastUsedSceneTexture != sceneTexture)
            {
                lastUsedSceneTexture = sceneTexture;
                epSceneTexture.SetValue(sceneTexture.XnaTexture);
            }
        } // SetSceneTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Bloom shader.
        /// </summary>
        internal BloomShader() : base("PostProcessing\\Bloom")
        {
            bloomTexture = new RenderTarget(RenderTarget.SizeType.QuarterScreen, SurfaceFormat.Color);
            Resource.CurrentTechnique = Resource.Techniques["Bloom"];
        } // BloomShader

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
                epHalfPixel      = Resource.Parameters["halfPixel"];
                epBloomThreshold = Resource.Parameters["bloomThreshold"];
                epLensExposure   = Resource.Parameters["lensExposure"];
                epSceneTexture   = Resource.Parameters["sceneTexture"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Generate Bloom

        /// <summary>
        /// Generate Bloom Texture.
        /// </summary>
        public void GenerateBloom(Texture sceneTexture)
        {
            try
            {
                // Set Render States
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                // Set Parameters
                SetHalfPixel(new Vector2(-1f / BloomTexture.Width, 1f / BloomTexture.Height));
                SetLensExposure(ToneMapping.LensExposure);
                SetBloomThreshold(BloomThreshold);
                SetSceneTexture(sceneTexture);

                bloomTexture.EnableRenderTarget();
                
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();

                bloomTexture.DisableRenderTarget();

                // Blur it.
                DeferredLightingManager.QuarterScreenBlur.GenerateBlur(bloomTexture, false);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Bloom: Unable to render.", e);
            }
        } // GenerateBloom

        #endregion

    } // Bloom
} // XNAFinalEngine.Graphics
