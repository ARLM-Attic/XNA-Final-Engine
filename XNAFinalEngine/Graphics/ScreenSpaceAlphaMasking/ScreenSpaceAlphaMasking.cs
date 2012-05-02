
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Screen space alpha masking (SSAM)
    /// Based in the Black Rock Studio article (GPU Pro)
    /// 
    /// From this article:
    /// 
    /// This is a solution to the alpha blending of foliage, a solution that increases the quality
    /// of a wide range of alphatest-class renderings, giving them the appaearance of true alpha blending.
    /// It yielded a soft natural look without sacrificing any of the detail and contrast present in the source artwork.
    /// 
    /// Pros:
    /// 
    ///  * Foliage adeges are blended smoothly with the surrounding environment.
    ///  
    ///  * Internally overlapping and interpenetrating primitives are sorted on a per pixel basis using alpha testing techniques.
    ///  
    ///  * The effect is implemented using simple, low-cost rendering techniques that do not require any geometry sorting of splitting
    ///    (only consistency in primitive dispatch order is required).
    ///    
    ///  * The final blending operations are performed at a linear cost (once per pixel) regardless of scene complexity and over-draw.
    ///  
    ///  * The effect integrates well with other alpha-blending stages in the rendering pipeline (particles, etc.)
    ///  
    ///  * When combined with other optimizations such as moving lighting to the vertex shader, and optimizing the shaders for each pass,
    ///    overall performance can be higher than that of MSAA-based techniques.
    ///    
    /// Cons:
    /// 
    ///  * The overhead of rendering the extra passes.
    ///  
    ///  * Memory requirements are higher, as we need to store thee images (but still better than Order-independent transparency)
    ///  
    ///  * The technique cannot be used to sort large collections of semi-transparent glass-like surfaces
    ///    (of soft alpha gradients that span large portions of the screen) without potentially exhibiting visual artifacts.
    ///    
    /// </summary>
    internal static class ScreenSpaceAlphaMasking
    {

        #region Variables
        
        /// <summary>
        /// Render state for the mask creation.
        /// </summary>
        private static BlendState ssamBlendState = new BlendState
                                                    {
                                                        AlphaBlendFunction = BlendFunction.Add,
                                                        AlphaSourceBlend = Blend.One,
                                                        AlphaDestinationBlend = Blend.One,
                                                        ColorBlendFunction = BlendFunction.Max,
                                                        ColorSourceBlend = Blend.One,
                                                        ColorDestinationBlend = Blend.One,
                                                    };

        #endregion

        #region Properties

        /// <summary>
        /// The Screen Space Alpha Mask.
        /// </summary>
        public static RenderTarget Mask { get; private set; }

        #endregion

        #region Shader Parameters

        /// <summary>
        /// The shader effect.
        /// </summary>
        private static Effect Effect { get; set; }

        #region Effect Handles

        /// <summary>
        /// Effect handles.
        /// </summary>
        private static EffectParameter epHalfPixel,
                                       epWorldViewProj,
                                       epWorldView,
                                       epFarPlane,
                                       epDiffuseTexture,
                                       epDepthTexture,
                                       epMaskTexture,
                                       epFoliageColorTexture,
                                       epLightMapTexture;

        #endregion

        #region Half Pixel

        private static Vector2? lastUsedHalfPixel;
        private static void SetHalfPixel(Vector2 _halfPixel)
        {
            if (lastUsedHalfPixel != _halfPixel || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedHalfPixel = _halfPixel;
                epHalfPixel.SetValue(_halfPixel);
            }
        } // SetHalfPixel

        #endregion

        #region World View Projection Matrix

        private static Matrix? lastUsedWorldViewProjMatrix;
        private static void SetWorldViewProjMatrix(Matrix worldViewProjMatrix)
        {
            if (lastUsedWorldViewProjMatrix != worldViewProjMatrix || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedWorldViewProjMatrix = worldViewProjMatrix;
                epWorldViewProj.SetValue(worldViewProjMatrix);
            }
        } // WorldViewProjMatrix

        #endregion

        #region World View Matrix

        private static Matrix? lastUsedWorldViewMatrix;
        private static void SetWorldViewMatrix(Matrix worldViewMatrix)
        {
            if (lastUsedWorldViewMatrix != worldViewMatrix || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedWorldViewMatrix = worldViewMatrix;
                epWorldView.SetValue(worldViewMatrix);
            }
        } // SetWorldViewMatrix

        #endregion

        #region Far Plane

        private static float? lastUsedFarPlane;
        private static void SetFarPlane(float farPlane)
        {
            if (lastUsedFarPlane != farPlane || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedFarPlane = farPlane;
                epFarPlane.SetValue(farPlane);
            }
        } // SetFarPlane

        #endregion

        #region Diffuse Texture

        private static Texture lastUsedDiffuseTexture;
        private static void SetDiffuseTexture(Texture _diffuseTexture)
        {
            if (lastUsedDiffuseTexture != _diffuseTexture || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedDiffuseTexture = _diffuseTexture;
                epDiffuseTexture.SetValue(_diffuseTexture.XnaTexture);
            }
        } // SetDiffuseTexture

        #endregion

        #region Depth Texture

        private static Texture lastUsedDepthTexture;
        private static void SetDepthTexture(Texture depthTexture)
        {
            if (lastUsedDepthTexture != depthTexture  || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedDepthTexture = depthTexture;
                epDepthTexture.SetValue(depthTexture.XnaTexture);
            }
        } // SetDepthTexture

        #endregion

        #region Mask Texture

        private static Texture lastUsedMaskTexture;
        private static void SetMaskTexture(Texture maskTexture)
        {
            if (lastUsedMaskTexture != maskTexture || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedMaskTexture = maskTexture;
                epMaskTexture.SetValue(maskTexture.XnaTexture);
            }
        } // SetMaskTexture

        #endregion

        #region Foliage Color Texture

        private static Texture lastUsedFoliageColorTexture;
        private static void SetFoliageColorTexture(Texture foliageColorTexture)
        {
            if (lastUsedFoliageColorTexture != foliageColorTexture || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedFoliageColorTexture = foliageColorTexture;
                epFoliageColorTexture.SetValue(foliageColorTexture.XnaTexture);
            }
        } // SetFoliageColorTexture

        #endregion

        #region Light Map Texture

        private static Texture lastUsedLightMapTexture;
        private static void SetLightMapTexture(Texture lightMapTexture)
        {
            if (lastUsedLightMapTexture != lightMapTexture  || EngineManager.DeviceDisposedThisFrame)
            {
                lastUsedLightMapTexture = lightMapTexture;
                epLightMapTexture.SetValue(lightMapTexture.XnaTexture);
            }
        } // SetLightMapTexture

        #endregion

        #endregion

        #region Load Shader

        /// <summary>
        /// Load shader
        /// </summary>
        private static void LoadShader()
        {
            const string filename = "Transparency\\ScreenSpaceAlphaMasking";
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
            Mask = new RenderTarget(RenderTarget.SizeType.FullScreen, SurfaceFormat.Color, false, RenderTarget.AntialiasingType.NoAntialiasing);
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
                epHalfPixel           = Effect.Parameters["halfPixel"];
                epFarPlane            = Effect.Parameters["farPlane"];
                epDiffuseTexture      = Effect.Parameters["diffuseTexture"];
                epWorldViewProj       = Effect.Parameters["worldViewProj"];
                epWorldView           = Effect.Parameters["worldView"];
                epDepthTexture        = Effect.Parameters["depthTexture"];
                epMaskTexture         = Effect.Parameters["maskTexture"];
                epFoliageColorTexture = Effect.Parameters["foliageColorTexture"];
                epLightMapTexture = Effect.Parameters["lightMap"];
            }
            catch
            {
                throw new Exception("Get the handles from the Screen Space Alpha Masking shader failed.");
            }
        } // GetParametersHandles

        #endregion

        #region Generate Mask

        /// <summary>
        /// Render the object without taking care of the illumination information.
        /// </summary>
        private static void RenderFoliageObjects(Object renderObjects)
        {
            if (renderObjects is GraphicObject)
            {
                //SetDiffuseTexture(((Foliage)((GraphicObject)renderObjects).Material).DiffuseTexture);
                SetWorldViewProjMatrix(renderObjects.WorldMatrix * ApplicationLogic.Camera.ViewMatrix * ApplicationLogic.Camera.ProjectionMatrix);
                SetWorldViewMatrix(renderObjects.WorldMatrix * ApplicationLogic.Camera.ViewMatrix);
                // Render
                Effect.CurrentTechnique.Passes[0].Apply();
                ((GraphicObject)renderObjects).Model.Render();
            }
            else // if is a container object
            {
                foreach (GraphicObject graphicObj in ((ContainerObject)renderObjects).GraphicObjectsChildren)
                {
                    RenderFoliageObjects(graphicObj);
                }
                foreach (ContainerObject containerObject in ((ContainerObject)renderObjects).ContainerObjectsChildren)
                {
                    RenderFoliageObjects(containerObject);
                }
            }
        } // RenderFoliageObjects

        /// <summary>
        /// Generate Mask.
        /// </summary>
        public static void GenerateMask(List<Object> objectsToRender, RenderTarget depthTexture)
        {
            if (Effect == null)
            {
                LoadShader();
            }
            try
            {
                Effect.CurrentTechnique = Effect.Techniques["SSAM"];

                EngineManager.Device.BlendState = ssamBlendState;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                EngineManager.Device.RasterizerState = RasterizerState.CullNone;

                Mask.EnableRenderTarget();
                Mask.Clear(new Color(0, 0, 0, 0));

                // Set parameters
                SetHalfPixel(new Vector2(0.5f / DeferredLightingManager.LightMap.Width, 0.5f / DeferredLightingManager.LightMap.Height));
                SetFarPlane(ApplicationLogic.Camera.FarPlane);
                SetDepthTexture(depthTexture);

                foreach (var objectToRender in objectsToRender)
                {
                    RenderFoliageObjects(objectToRender);
                }

                Mask.DisableRenderTarget();
                
                EngineManager.SetDefaultRenderStates();
            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the Screen Space Alpha Masking shader" + e.Message);
            }
        } // GenerateMask

        #endregion

        #region Blend Foliage

        /// <summary>
        /// Blend Foliage with the opaque objects.
        /// </summary>
        public static void BlendFoliage(RenderTarget foliageColorTexture, RenderTarget lightMap)
        {
            if (Effect == null)
            {
                LoadShader();
            }
            try
            {
                Effect.CurrentTechnique = Effect.Techniques["FinalColor"];
                // Set render states
                EngineManager.Device.BlendState = BlendState.NonPremultiplied;
                EngineManager.Device.DepthStencilState = DepthStencilState.None;
                // Set parameters
                SetHalfPixel(new Vector2(-1f / foliageColorTexture.Width, 1f / foliageColorTexture.Height));
                SetFoliageColorTexture(foliageColorTexture);
                SetMaskTexture(Mask);
                SetLightMapTexture(lightMap);

                Effect.CurrentTechnique.Passes[0].Apply();
                ScreenPlane.Render();

                EngineManager.SetDefaultRenderStates();
            } // try
            catch (Exception e)
            {
                throw new Exception("Unable to render the screen space alpha masking shader" + e.Message);
            }
        } // BlendFoliage

        #endregion

    } // ScreenSpaceAlphaMasking
} // XNAFinalEngine.Graphics
