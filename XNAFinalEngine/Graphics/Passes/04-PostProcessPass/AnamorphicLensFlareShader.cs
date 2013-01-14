
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
using Model = XNAFinalEngine.Assets.Model;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Anamorphic Lens Flare Shader.
    /// </summary>
    internal class AnamorphicLensFlareShader : Shader
    {

        #region Variables

        // Bounding Light Object.
        private static Model sunObject;

        // Singleton reference.
        private static AnamorphicLensFlareShader instance;

        // Shader Parameters.
        private static ShaderParameterFloat spFarPlane, spDispersal, spHaloWidth, spIntensity;
        private static ShaderParameterVector2 spHalfPixel, spSunPosProj;
        private static ShaderParameterVector3 spDistortion;
        private static ShaderParameterColor spSunColor;
        private static ShaderParameterMatrix spWorldViewProj, spWorldView;
        private static ShaderParameterTexture spDepthTexture, spSceneTexture, spDirtTexture, spHighBlurredSunTexture;

        #endregion

        #region Properties

        /// <summary>
        /// A singleton of this shader.
        /// </summary>
        public static AnamorphicLensFlareShader Instance
        {
            get
            {
                if (instance == null)
                    instance = new AnamorphicLensFlareShader();
                return instance;
            }
        } // Instance

        #endregion
        
        #region Constructor

        /// <summary>
        /// Anamorphic Lens Flare Shader.
        /// </summary>
        private AnamorphicLensFlareShader() : base("PostProcessing\\AnamorphicLensFlare")
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            //sunObject = new Sphere(6, 6, 1);   // Algorithmically generated mesh normally sucks when optimized vertex access is needed.
            sunObject = new FileModel("Sphere"); // Exported models for the contrary are great.
            ContentManager.CurrentContentManager = userContentManager;
        } // AnamorphicLensFlareShader

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
                spHalfPixel             = new ShaderParameterVector2("halfPixel", this);
                spSunPosProj            = new ShaderParameterVector2("sunPosProj", this);
                spDistortion            = new ShaderParameterVector3("distortion", this);
                spDepthTexture          = new ShaderParameterTexture("depthTexture", this, SamplerState.PointClamp, 0);
                spSceneTexture          = new ShaderParameterTexture("sceneTexture", this, SamplerState.PointClamp, 9);
                spHighBlurredSunTexture = new ShaderParameterTexture("highBlurredSunTexture", this, SamplerState.PointClamp, 7);
                spDirtTexture           = new ShaderParameterTexture("dirtTexture", this, SamplerState.PointClamp, 8);
                spFarPlane              = new ShaderParameterFloat("farPlane", this);
                spDispersal             = new ShaderParameterFloat("dispersal", this);
                spHaloWidth             = new ShaderParameterFloat("haloWidth", this);
                spIntensity             = new ShaderParameterFloat("intensity", this);
                spWorldViewProj         = new ShaderParameterMatrix("worldViewProj", this);
                spWorldView             = new ShaderParameterMatrix("worldView", this);
                spSunColor              = new ShaderParameterColor("sunColor", this);
                Resource.CurrentTechnique = Resource.Techniques["LensFlare"];
            }
            catch
            {
                throw new InvalidOperationException("The parameter's handles from the " + Name + " shader could not be retrieved.");
            }
        } // GetParameters

        #endregion

        #region Render

        /// <summary>
        /// Generate Bloom Texture.
        /// </summary>
        internal RenderTarget Render(Texture halfDepthTexture, Texture bloomTexture, PostProcess postProcess, Matrix viewMatrix, Matrix projectionMatrix, float farPlane, Vector3 cameraPosition)
        {
            if (postProcess == null)
                throw new ArgumentNullException("postProcess");
            if (halfDepthTexture == null || halfDepthTexture.Resource == null)
                throw new ArgumentNullException("halfDepthTexture");
            if (postProcess.AnamorphicLensFlare == null)
                throw new ArgumentException("Anamorphic Lens Flare Shader: Anamorphic lens flare properties can not be null.");
            try
            {
                // Fetch auxiliary render target.
                RenderTarget lensFlareTexture = RenderTarget.Fetch(halfDepthTexture.Size, SurfaceFormat.Color, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);

                // Set Render States.
                EngineManager.Device.BlendState = BlendState.Opaque;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                
                #region First pass: sun rendering and occlusion test.
                
                lensFlareTexture.EnableRenderTarget();
                lensFlareTexture.Clear(Color.Black);
                // Set Parameters
                spHalfPixel.Value = new Vector2(0.5f / lensFlareTexture.Width, 0.5f / lensFlareTexture.Height);
                spDepthTexture.Value = halfDepthTexture;
                spFarPlane.Value = farPlane;
                cameraPosition.Z = cameraPosition.Z - (farPlane - 10);
                Matrix worldMatrix = Matrix.CreateScale((farPlane * 2.25f) / 100) * Matrix.CreateTranslation(cameraPosition);
                spWorldViewProj.Value = worldMatrix * viewMatrix * projectionMatrix;
                spWorldView.Value = worldMatrix *viewMatrix;
                spSunColor.Value = new Color(1.0f, 0.9f, 0.70f);
                // Render
                Resource.CurrentTechnique.Passes[0].Apply();
                sunObject.Render();
                lensFlareTexture.DisableRenderTarget();
                
                #endregion

                // The second and third pass were removed to improve performance.

                #region Fourth pass: high blur vertical

                RenderTarget highBlurredSunTextureVertical = RenderTarget.Fetch(halfDepthTexture.Size, SurfaceFormat.Color, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                highBlurredSunTextureVertical.EnableRenderTarget();
                highBlurredSunTextureVertical.Clear(Color.Black);
                spHalfPixel.Value = new Vector2(-0.5f / (lensFlareTexture.Width / 2), 0.5f / (lensFlareTexture.Height / 2));
                spSceneTexture.Value = lensFlareTexture; // bloomTexture;
                Resource.CurrentTechnique.Passes[3].Apply();
                RenderScreenPlane();
                highBlurredSunTextureVertical.DisableRenderTarget();

                #endregion

                #region Fifth pass: high blur horizontal

                RenderTarget highBlurredSunTexture = RenderTarget.Fetch(halfDepthTexture.Size, SurfaceFormat.Color, DepthFormat.None, RenderTarget.AntialiasingType.NoAntialiasing);
                highBlurredSunTexture.EnableRenderTarget();
                highBlurredSunTexture.Clear(Color.Black);
                spSceneTexture.Value = highBlurredSunTextureVertical;
                Resource.CurrentTechnique.Passes[4].Apply();
                RenderScreenPlane();
                highBlurredSunTexture.DisableRenderTarget();
                RenderTarget.Release(highBlurredSunTextureVertical);

                #endregion

                #region Last pass: composite images
                
                lensFlareTexture.EnableRenderTarget();
                lensFlareTexture.Clear(Color.Black);
                // Set Parameters
                spHighBlurredSunTexture.Value = highBlurredSunTexture;
                spDispersal.Value = postProcess.AnamorphicLensFlare.Dispersal;
                spHaloWidth.Value = postProcess.AnamorphicLensFlare.HaloWidth;
                spIntensity.Value = postProcess.AnamorphicLensFlare.Intensity;
                spDistortion.Value = postProcess.AnamorphicLensFlare.ChromaticDistortion;
                spDirtTexture.Value = postProcess.AnamorphicLensFlare.DirtTexture ?? Texture.BlackTexture;
                // uv coordinates of the sun position on the screen.
                Vector4 sunPosProjected = Vector4.Transform(new Vector4(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 1), viewMatrix * projectionMatrix * BiasMatrix());
                sunPosProjected = sunPosProjected / sunPosProjected.W;
                spSunPosProj.Value = new Vector2(sunPosProjected.X, 1 - sunPosProjected.Y);
                // Render
                Resource.CurrentTechnique.Passes[5].Apply();
                RenderScreenPlane();
                lensFlareTexture.DisableRenderTarget();

                #endregion

                // Release the rest of the render targets.
                RenderTarget.Release(highBlurredSunTexture);
                return lensFlareTexture;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Anamorphic Lens Flare Shader: Unable to render.", e);
            }
        } // Render

        /// <summary>
        /// A bias matrix transform from projection space to normalized screen space.
        /// </summary>
        private static Matrix BiasMatrix()
        {
            return new Matrix
            {
                M11 = 0.5f, M12 = 0.0f, M13 = 0.0f, M14 = 0.0f, 
                M21 = 0.0f, M22 = 0.5f, M23 = 0.0f, M24 = 0.0f, 
                M31 = 0.0f, M32 = 0.0f, M33 = 0.5f, M34 = 0.0f, 
                M41 = 0.5f, M42 = 0.5f, M43 = 0.5f, M44 = 1.0f, 
            };
        } // BiasMatrix

        #endregion
        
    } // AnamorphicLensFlareShader
} // XNAFinalEngine.Graphics
