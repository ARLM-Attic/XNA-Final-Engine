
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
using System.Collections.Generic;
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
    /// Bloom shader.
    /// The effect produces fringes (or feathers) of light around very bright objects in an image.
    /// Basically, if an object has a light behind it, the light will look more realistic and will somewhat overlap the front of the object from the 3rd person perspective. 
    /// http://en.wikipedia.org/wiki/Bloom_(shader_effect)
    /// </summary>
    /// <remarks>
    /// To avoid wasting space in temporal render targets we could create a blur auxiliary render target with enough size to work with all our render targets.
    /// The system will use a viewport to match the sizes.
    /// The problem is that in the second pass the shader has to know about the viewport so that it won’t read more than it should.
    /// </remarks>
    internal class BloomShader : Shader
    {

        #region Properties

        /// <summary>
        /// Bloom Texture.
        /// </summary>
        public RenderTarget BloomTexture { get; private set; }

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
            // XNA 4.0 reconstructs automatically the render targets when a device is lost.
            // However the shaders have to re set to the GPU the new render targets to work properly.
            // This problem seems to manifest only with floating point formats.
            // So it's a floating point texture set it every time that is need it.
            if (lastUsedSceneTexture != sceneTexture ||
                (sceneTexture is RenderTarget && ((RenderTarget)sceneTexture).SurfaceFormat != SurfaceFormat.Color))
            {
                lastUsedSceneTexture = sceneTexture;
                epSceneTexture.SetValue(sceneTexture.Resource);
            }
        } // SetSceneTexture

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Bloom shader.
        /// </summary>
        private BloomShader(Size size) : base("PostProcessing\\Bloom")
        {
            BloomTexture = new RenderTarget(size, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
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
        internal void Render(Texture sceneTexture, PostProcess postProcess)
        {
            if (postProcess == null)
                throw new ArgumentNullException("postProcess");
            if (sceneTexture == null || sceneTexture.Resource == null)
                throw new ArgumentNullException("sceneTexture");
            if (postProcess.Bloom == null)
                throw new ArgumentException("Bloom Shader: Bloom property can not be null.");
            try
            {
                // Set Render States
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineManager.Device.SamplerStates[8] = SamplerState.PointClamp;

                // Set Parameters
                SetHalfPixel(new Vector2(-1f / BloomTexture.Width, 1f / BloomTexture.Height));
                SetLensExposure(postProcess.LensExposure);
                SetBloomThreshold(postProcess.Bloom.BloomThreshold);
                SetSceneTexture(sceneTexture);

                BloomTexture.EnableRenderTarget();
                
                Resource.CurrentTechnique.Passes[0].Apply();
                RenderScreenPlane();

                BloomTexture.DisableRenderTarget();

                // Blur it.
                BlurShader.GetShader(BloomTexture.Size).Filter(BloomTexture, false, 1); // Quarter size blur
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Bloom Shader: Unable to render.", e);
            }
        } // Render

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            BloomTexture.Dispose();
        } // DisposeManagedResources

        #endregion

        #region Stored Shaders

        // A pool of all bloom shaders.
        private static readonly Dictionary<Size, BloomShader> shaders = new Dictionary<Size, BloomShader>(1);
        
        /// <summary>
        /// Returns a bloom shader for this size.
        /// The shaders are stored in a pool.
        /// </summary>
        public static BloomShader GetShader(Size size)
        {
            if (shaders.ContainsKey(size))
                return shaders[size];
            // If not return a new one.
            shaders[size] = new BloomShader(size);
            return shaders[size];
        } // GetShader

        /// <summary>
        /// Dispose stored shaders.
        /// </summary>
        public static void DisposeShaders()
        {
            foreach (var shader in shaders)
            {
                shader.Value.Dispose();
            }
            shaders.Clear();
        } // DisposeShaders

        #endregion

    } // Bloom
} // XNAFinalEngine.Graphics
