
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.EngineCore;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Allows to render onto textures, this kind of textures are called Render Targets.
    /// This class is required for the screen shaders and the screenshot.
    /// </summary>
    public class RenderToTexture : Texture
    {

        #region Enumerates

        /// <summary>
        /// Posible size types for creating a RenderToTexture object.
        /// </summary>
        public enum SizeType
        {
            /// <summary>
            /// Uses the full screen size for this texture
            /// </summary>
            FullScreen,
            /// <summary>
            /// Uses half the full screen size, e.g. 800x600 becomes 400x300
            /// </summary>
            HalfScreen,
            /// <summary>
            /// Uses a quarter of the full screen size, e.g. 800x600 becomes 200x150
            /// </summary>
            QuarterScreen,
            /// <summary>
            /// Tamaño personalizado. 256 x 256 pixeles.
            /// </summary>
            Custom256x256,
            /// <summary>
            /// Tamaño personalizado. 512 x 512 pixeles.
            /// </summary>
            Custom512x512,
            /// <summary>
            /// Tamaño personalizado. 1024 x 1024 pixeles.
            /// </summary>
            Custom1024x1024,
            /// <summary>
            /// Tamaño personalizado. 2048 x 2048 pixeles.
            /// </summary>
            Custom2048x2048,
            /// <summary>
            /// Custom Size.
            /// </summary>
            Custom
        }

        public enum AntiAliasingType
        {
            /// <summary>
            /// Utiliza la misma configuracion que el sistema.
            /// </summary>
            System,
            /// <summary>
            /// No utiliza antialiasing.
            /// </summary>
            NoAntialiasing
        }

        #endregion

        #region Variables

        /// <summary>
        /// XNA Render target.
        /// </summary>
        private RenderTarget2D renderTarget;

        /// <summary>
        /// Size type.
        /// </summary>
        private readonly SizeType sizeType;

        /// <summary>
        /// Id for each created RenderToTexture for the generated filename.
        /// </summary>
        private static int globalInstanceId;

        /// <summary>
        /// Make sure we don't call xnaTexture before resolving for the first time!
        /// </summary>
        private bool alreadyResolved;

        /// <summary>
        /// Remember the last render target we set.
        /// </summary>
        private static readonly Stack<RenderToTexture> currentRenderTarget = new Stack<RenderToTexture>();

        /// <summary>
        /// All the active render targets.
        /// </summary>
        private static readonly List<RenderToTexture> renderTargets = new List<RenderToTexture>();

        /// <summary>
        /// Surface Format.
        /// </summary>
        private SurfaceFormat? surfaceFormat;

        /// <summary>
        /// Depth Format.
        /// </summary>
        private DepthFormat? depthFormat;

        #endregion

        #region Properties

        /// <summary>
        /// Currently active render to texture.
        /// </summary>
        public static RenderToTexture CurrentRenderTarget
        {
            get
            {
                if (currentRenderTarget.Count == 0)
                    return null;
                return currentRenderTarget.Peek();
            }
        } // CurrentRenderTarget

        /// <summary>
        /// Return the render target texture. In XNA 4.0 the render target it's a texture.
        /// If we don't resolve (disableRenderTarget) firts we don't obtain the last render target texture.
        /// </summary>
        public override XnaTexture XnaTexture
        {
            get
            {
                if (alreadyResolved)
                    internalXnaTexture = renderTarget;
                return internalXnaTexture;
            }
        } // XnaTexture

        /// <summary>
        /// Does this texture use some high percision format? Better than 8 bit color?
        /// </summary>
        public bool UsesHighPrecisionFormat { get; private set; }

        /// <summary>
        /// Has depth buffer?
        /// </summary>
        public bool HasDepthBuffer { get; private set; }

        /// <summary>
        /// Multi Sample Quality.
        /// </summary>
        protected int multiSampleQuality { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a render target for render to textures.
        /// </summary>
        /// <param name="setSizeType">Render target size</param>
        /// <param name="_usesHighPrecisionFormat">Uses a rgba 8 bit format or a 32 bit single channel format</param>
        /// <param name="_hasDepthBuffer">has depth buffer?</param>
        /// <param name="_multiSampleQuality">MultiSample count. Use EngineManager.MultisamplingQuality for the current multi sample count</param>
        public RenderToTexture(SizeType setSizeType, bool _usesHighPrecisionFormat = false, bool _hasDepthBuffer = true, int _multiSampleQuality = 0)
        {
            sizeType = setSizeType;
            UsesHighPrecisionFormat = _usesHighPrecisionFormat;
            HasDepthBuffer = _hasDepthBuffer;
            textureFilename = "RenderToTexture instance " + globalInstanceId++;
            multiSampleQuality = _multiSampleQuality;

            Create();

            renderTargets.Add(this);
        } // RenderToTexture

        /// <summary>
        /// Creates a render target for render to textures.
        /// </summary>
        /// <param name="setSizeType">Render target size</param>
        /// <param name="_surfaceFormat">Surface format</param>
        /// <param name="_hasDepthBuffer">has depth buffer?</param>
        /// <param name="_multiSampleQuality">MultiSample count. Use EngineManager.MultisamplingQuality for the current multi sample count</param>
        public RenderToTexture(SizeType setSizeType, SurfaceFormat _surfaceFormat, bool _hasDepthBuffer = true, int _multiSampleQuality = 0)
        {
            sizeType = setSizeType;
            surfaceFormat = _surfaceFormat;
            HasDepthBuffer = _hasDepthBuffer;
            textureFilename = "RenderToTexture instance " + globalInstanceId++;
            multiSampleQuality = _multiSampleQuality;

            Create();

            renderTargets.Add(this);
        } // RenderToTexture

        /// <summary>
        /// Creates a render target for render to textures. Use size type constructor for screen relative sizes.
        /// </summary>
        /// <param name="size">Render target size</param>
        /// <param name="_usesHighPrecisionFormat">Uses a rgba 8 bit format or a 32 bit single channel format</param>
        /// <param name="_hasDepthBuffer">has depth buffer?</param>
        /// <param name="_multiSampleQuality">MultiSample count. Use EngineManager.MultisamplingQuality for the current multi sample count</param>
        public RenderToTexture(System.Drawing.Size size, bool _usesHighPrecisionFormat = false, bool _hasDepthBuffer = true, int _multiSampleQuality = 0)
        {
            sizeType = SizeType.Custom;
            textureWidth = size.Width;
            textureHeight = size.Height;

            UsesHighPrecisionFormat = _usesHighPrecisionFormat;
            HasDepthBuffer = _hasDepthBuffer;
            textureFilename = "RenderToTexture instance " + globalInstanceId++;
            multiSampleQuality = _multiSampleQuality;

            Create();

            renderTargets.Add(this);
        } // RenderToTexture

        private void Create()
        {
            // Calculate render target's size.
            CalculateSize();

            #region Surface and depth format

            if (surfaceFormat == null)
                surfaceFormat = UsesHighPrecisionFormat ? SurfaceFormat.Single : SurfaceFormat.Color;

            if (depthFormat == null)
                depthFormat = HasDepthBuffer ? DepthFormat.Depth24Stencil8 : DepthFormat.None;
            
            #endregion

            try
            {
                // Create render target of specified size.
                renderTarget = new RenderTarget2D(EngineManager.Device, textureWidth, textureHeight, false, surfaceFormat.Value, depthFormat.Value, multiSampleQuality, RenderTargetUsage.PlatformContents);
            } // try
            catch (Exception ex)
            {
                throw new Exception("Creating render target failed: " + ex);
            } // catch
        } // Create

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            renderTargets.Remove(this);
            XnaTexture.Dispose();
            renderTarget.Dispose();
        } // DisposeManagedResources

        #endregion

        #region Calculate Size

        /// <summary>
        /// Calculate render target's size.
        /// </summary>
        private void CalculateSize()
        {
            switch (sizeType)
            {
                case SizeType.FullScreen:
                    textureWidth = EngineManager.Width;
                    textureHeight = EngineManager.Height;
                    break;
                case SizeType.HalfScreen:
                    textureWidth = EngineManager.Width / 2;
                    textureHeight = EngineManager.Height / 2;
                    break;
                case SizeType.QuarterScreen:
                    textureWidth = EngineManager.Width / 4;
                    textureHeight = EngineManager.Height / 4;
                    break;
                case SizeType.Custom256x256:
                    textureWidth = 256;
                    textureHeight = 256;
                    break;
                case SizeType.Custom512x512:
                    textureWidth = 512;
                    textureHeight = 512;
                    break;
                case SizeType.Custom1024x1024:
                    textureWidth = 1024;
                    textureHeight = 1024;
                    break;
                case SizeType.Custom2048x2048:
                    textureWidth = 2048;
                    textureHeight = 2048;
                    break;
            }
            CalculateHalfPixelSize();
        } // CalculateSize

        #endregion

        #region Set Render Target

        /// <summary>
        /// Set render target for render.
        /// </summary>
        public void EnableRenderTarget()
        {
            EngineManager.Device.SetRenderTarget(renderTarget);
            currentRenderTarget.Push(this);
        } // EnableRenderTarget

        /// <summary>
        /// Set two render targets at once.
        /// This method can be extended to support more than two render targets.
        /// </summary>
        /// <param name="renderTarget1">First render target.</param>
        /// <param name="renderTarget2">Second render target.</param>
        public static void EnableRenderTargets(RenderToTexture renderTarget1, RenderToTexture renderTarget2)
        {
            EngineManager.Device.SetRenderTargets(renderTarget1.renderTarget, renderTarget2.renderTarget);
            currentRenderTarget.Push(renderTarget1);
            currentRenderTarget.Push(renderTarget2);
        } // EnableRenderTargets

        #endregion

        #region Clear

        /// <summary>
        /// Clear render target.
        /// This method will only work if the render target was set before with SetRenderTarget.
        /// </summary>
        public void Clear(Color clearColor)
        {
            // Make sure this render target is currently set!
            if (currentRenderTarget.Peek() != this)
                throw new Exception("You can't clear a render target without first setting the render target!");
            if (HasDepthBuffer)
                EngineManager.Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearColor, 1.0f, 0);
            // If doesn't have depth buffer then the clear will fail, even if the clear option is set only to target.
            /*else
                EngineManager.Device.Clear(clearColor);*/
        } // Clear

        #endregion

        #region Disable Render Target

        /// <summary>
        /// Resolve render target.
        /// This method will only work if the render target was set before with SetRenderTarget.
        /// </summary>
        public void DisableRenderTarget()
        {
            // Make sure this render target is currently set!
            if (currentRenderTarget.Count == 0 || currentRenderTarget.Peek() != this)
                throw new Exception("You can't call disable the render target without first setting the render target!");

            alreadyResolved = true;

            currentRenderTarget.Pop();
            if (currentRenderTarget.Count == 0)
                BackToSceneRenderTarget();
            else
                EngineManager.Device.SetRenderTarget((RenderTarget2D)(currentRenderTarget.Peek().renderTarget));
        } // DisableRenderTarget

        /// <summary>
        /// Resolve two render targets at once.
        /// </summary>
        public static void DisableMultipleRenderTargets()
        {
            currentRenderTarget.Peek().alreadyResolved = true;
            currentRenderTarget.Pop();
            currentRenderTarget.Peek().alreadyResolved = true;
            currentRenderTarget.Pop();
            if (currentRenderTarget.Count == 0)
                BackToSceneRenderTarget();
            else
            {
                EngineManager.Device.SetRenderTarget((RenderTarget2D)(currentRenderTarget.Peek().renderTarget));
            }
        } // DisableRenderTarget

        #endregion

        #region Render on screen

        /// <summary>
        /// Render the texture on part of the screen. If the render target is has a single channel color then the render it's inmediate.
        /// </summary>
        /// <param name="screenRectangle">Cut the screen</param>
        /// <param name="textureRectangle">Cut the texture</param>
        public override void RenderOnScreen(Rectangle screenRectangle, Rectangle textureRectangle)
        {
            if (UsesHighPrecisionFormat)
            {
                SpriteManager.DrawSingleChannelTexture(XnaTexture, screenRectangle, textureRectangle);
            }
            else
                base.RenderOnScreen(screenRectangle, textureRectangle);
        } // RenderOnScreen

        /// <summary>
        /// Render the texture on part of the screen. If the render target is has a single channel color then the render it's inmediate.
        /// </summary>
        /// <param name="screenRectangle">Cut the screen</param>
        public override void RenderOnScreen(Rectangle screenRectangle)
        {
            if (UsesHighPrecisionFormat)
            {
                SpriteManager.DrawSingleChannelTexture(XnaTexture, screenRectangle, TextureRectangle);
            }
            else
                base.RenderOnScreen(screenRectangle, TextureRectangle);
        } // RenderOnScreen

        /// <summary>
        /// Render the texture on the screen using the whole texture. If the render target is has a single channel color then the render it's inmediate.
        /// </summary>        
        public override void RenderOnFullScreen()
        {
            if (UsesHighPrecisionFormat)
            {
                SpriteManager.DrawSingleChannelTexture(XnaTexture, new Rectangle(0, 0, EngineManager.Width, EngineManager.Height), TextureRectangle);
            }
            else
                base.RenderOnScreen(new Rectangle(0, 0, EngineManager.Width, EngineManager.Height), TextureRectangle);
        } // RenderOnFullScreen

        #endregion

        #region Recreate Render Targets (static)

        public static void RecreateRenderTargets()
        {
            foreach (RenderToTexture renderToTexture in renderTargets)
            {
                if (renderToTexture.sizeType == SizeType.FullScreen || renderToTexture.sizeType == SizeType.HalfScreen || renderToTexture.sizeType == SizeType.QuarterScreen)
                { // The size changes with the new window size.
                    renderToTexture.renderTarget.Dispose();
                    renderToTexture.Create();
                }
            }
        } // RecreateRenderTargets

        #endregion

        #region Back to Scene Render Target (static)

        /// <summary>
        /// Back to scene render target.
        /// </summary>
        public static void BackToSceneRenderTarget()
        {
            while (currentRenderTarget.Count > 0)
            {
                currentRenderTarget.Peek().alreadyResolved = true;
                currentRenderTarget.Pop();
            }
            EngineManager.Device.SetRenderTarget(null);
        } // BackToSceneRenderTarget

        #endregion

    } // RenderToTexture
} // XNAFinalEngine.Graphics