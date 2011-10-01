
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
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Allows to render onto textures, this kind of textures are called Render Targets.
    /// </summary>
    public class RenderTarget : Texture
    {

        #region Structs

        /// <summary>
        /// This structure is used to set multiple render targets without generating garbage in the process.
        /// Using directly something like this GraphicsDevice.SetRenderTargets(diffuseRt, normalRt, depthRt) will generate garbage.
        /// </summary>
        public struct RenderTargetBinding
        {
            internal Microsoft.Xna.Framework.Graphics.RenderTargetBinding[] InternalBinding;
            internal RenderTarget[] renderTargets;
        } // RenderTargetBinding

        #endregion

        #region Enumerates

        /// <summary>
        /// Relative size to screen and shadow sizes.
        /// </summary>
        public enum SizeType
        {
            /// <summary>
            /// Full screen size.
            /// </summary>
            FullScreen,
            /// <summary>
            /// Half the full screen size, e.g. 800x600 becomes 400x300
            /// </summary>
            HalfScreen,
            /// <summary>
            /// Quarter of the full screen size, e.g. 800x600 becomes 200x150
            /// </summary>
            QuarterScreen,
            /// <summary>
            /// 256 x 256 pixels. Good for shadows.
            /// </summary>
            Square256X256,
            /// <summary>
            /// 512 x 512 pixels. Good for shadows.
            /// </summary>
            Square512X512,
            /// <summary>
            /// 1024 x 1024 pixels. Good for shadows.
            /// </summary>
            Square1024X1024,
            /// <summary>
            /// 2048 x 2048 pixels. Good for shadows.
            /// </summary>
            Square2048X2048,
        } // SizeType

        /// <summary>
        /// Antialiasing Type.
        /// </summary>
        public enum AntialiasingType
        {
            /// <summary>
            /// Back Buffer value.
            /// </summary>
            System,
            /// <summary>
            /// No antialiasing.
            /// </summary>
            NoAntialiasing,
            /// <summary>
            /// 2X antialiasing.
            /// </summary>
            TwoSamples,
            /// <summary>
            /// 4X antialiasing.
            /// </summary>
            FourSamples,
            /// <summary>
            /// 8X antialiasing.
            /// </summary>
            EightSamples,
            /// <summary>
            /// 16X antialiasing.
            /// </summary>
            SixtySamples
        } // AntialiasingType

        #endregion
        
        #region Variables

        /// <summary>
        /// XNA Render target.
        /// </summary>
        private RenderTarget2D renderTarget;

        /// <summary>
        /// Make sure we don't call xnaTexture before resolving for the first time!
        /// </summary>
        private bool alreadyResolved;

        /// <summary>
        /// Surface Format.
        /// </summary>
        private readonly SurfaceFormat surfaceFormat;

        /// <summary>
        /// Depth Format.
        /// </summary>
        private readonly DepthFormat depthFormat;

        /// <summary>
        /// Size type.
        /// </summary>
        private readonly SizeType? sizeType;

        /// <summary>
        /// The count of render targets created for naming purposes.
        /// </summary>
        private static int nameNumber = 1;

        /// <summary>
        /// Remember the last render targets we set.
        /// We can enable up to four render targets at once.
        /// </summary>
        private static readonly RenderTarget[] currentRenderTarget = new RenderTarget[4];

        /// <summary>
        /// All the active render targets.
        /// </summary>
        private static readonly List<RenderTarget> renderTargetList = new List<RenderTarget>();

        #endregion

        #region Properties

        /// <summary>
        /// Return the render target texture. In XNA 4.0 the render target it's a texture.
        /// </summary>
        public override Texture2D XnaTexture
        {
            get
            {
                if (alreadyResolved)
                    return renderTarget;
                throw new InvalidOperationException("Unable to return render target: render target not resolved.");
            }
        } // XnaTexture

        /// <summary>
        /// Surface Format.
        /// </summary>
        public SurfaceFormat SurfaceFormat { get { return surfaceFormat; } }

        /// <summary>
        /// Depth Format.
        /// </summary>
        public DepthFormat DepthFormat { get { return depthFormat; } }

        /// <summary>
        /// Multi Sample Quality.
        /// </summary>
        public AntialiasingType Antialiasing { get; private set; }

        /// <summary>
        /// Currently active render targets.
        /// </summary>
        public static RenderTarget[] CurrentRenderTarget { get { return currentRenderTarget; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a render target for render to textures. Use size type constructor for screen relative sizes.
        /// </summary>
        /// <param name="size">Render target size</param>
        /// <param name="_surfaceFormat">Surface format</param>
        /// <param name="_depthFormat">Depth Format</param>
        /// <param name="antialiasingType">Multi sampling type: System value or no antialiasing.</param>
        public RenderTarget(Size size, SurfaceFormat _surfaceFormat, DepthFormat _depthFormat, AntialiasingType antialiasingType = AntialiasingType.NoAntialiasing)
        {
            Name = "Render Target " + nameNumber++;
            Width = size.Width;
            Height = size.Height;

            surfaceFormat = _surfaceFormat;
            depthFormat = _depthFormat;
            Antialiasing = antialiasingType;

            Create();

            renderTargetList.Add(this);
        } // RenderTarget

        /// <summary>
        /// Creates a render target for render to textures. Use size type constructor for screen relative sizes.
        /// </summary>
        /// <param name="size">Render target size</param>
        /// <param name="_surfaceFormat">Surface format</param>
        /// <param name="_hasDepthBuffer">Has depth buffer?</param>
        /// <param name="antialiasingType">Multi sampling type: System value or no antialiasing.</param>
        public RenderTarget(Size size, SurfaceFormat _surfaceFormat = SurfaceFormat.Color, bool _hasDepthBuffer = true, AntialiasingType antialiasingType = AntialiasingType.NoAntialiasing)
        {
            Name = "Render Target " + nameNumber++;
            Width = size.Width;
            Height = size.Height;

            surfaceFormat = _surfaceFormat;
            depthFormat = _hasDepthBuffer ? DepthFormat.Depth24 : DepthFormat.None;
            Antialiasing = antialiasingType;

            Create();

            renderTargetList.Add(this);
        } // RenderTarget

        /// <summary>
        /// Creates a render target for render to textures.
        /// </summary>
        /// <param name="_sizeType">Render target size</param>
        /// <param name="_surfaceFormat">Surface format</param>
        /// <param name="_depthFormat">Depth Format</param>
        /// <param name="antialiasingType">Multi sampling type: System value or no antialiasing.</param>
        public RenderTarget(SizeType _sizeType, SurfaceFormat _surfaceFormat, DepthFormat _depthFormat, AntialiasingType antialiasingType = AntialiasingType.NoAntialiasing)
        {
            Name = "Render Target " + nameNumber++;
            sizeType = _sizeType;
            Size size = CalculateSize(_sizeType);
            Width = size.Width;
            Height = size.Height;

            surfaceFormat = _surfaceFormat;
            depthFormat = _depthFormat;
            Antialiasing = antialiasingType;

            Create();

            renderTargetList.Add(this);
        } // RenderTarget

        /// <summary>
        /// Creates a render target for render to textures.
        /// </summary>
        /// <param name="_sizeType">Render target size</param>
        /// <param name="_surfaceFormat">Surface format</param>
        /// <param name="_hasDepthBuffer">Has depth buffer?</param>
        /// <param name="antialiasingType">Multi sampling type: System value or no antialiasing.</param>
        public RenderTarget(SizeType _sizeType, SurfaceFormat _surfaceFormat = SurfaceFormat.Color, bool _hasDepthBuffer = true, AntialiasingType antialiasingType = AntialiasingType.NoAntialiasing)
        {
            Name = "Render Target " + nameNumber++;
            sizeType = _sizeType;
            Size size = CalculateSize(_sizeType);
            Width = size.Width;
            Height = size.Height;

            surfaceFormat = _surfaceFormat;
            depthFormat = _hasDepthBuffer ? DepthFormat.Depth24 : DepthFormat.None;
            Antialiasing = antialiasingType;

            Create();

            renderTargetList.Add(this);
        } // RenderTarget

        #endregion

        #region Create

        /// <summary>
        /// Creates render target.
        /// </summary>
        private void Create()
        {
            if (sizeType == SizeType.FullScreen || sizeType == SizeType.HalfScreen || sizeType == SizeType.QuarterScreen)
            {
                SystemInformation.WindowSizeChanged += OnWindowSizeChanged;
            }
            try
            {
                // Create render target of specified size.
                // On Xbox 360, the render target will discard contents. On PC, the render target will discard if multisampling is enabled, and preserve the contents if not.
                // I use RenderTargetUsage.PlatformContents to be little more performance friendly with PC.
                // But I assume that the system works in DiscardContents mode so that an XBOX 360 implementation works.
                // What I lose, mostly nothing, because I made my own ZBuffer texture and the stencil buffer is deleted no matter what I do.
                renderTarget = new RenderTarget2D(SystemInformation.Device, Width, Height, false, surfaceFormat, depthFormat, CalculateMultiSampleQuality(Antialiasing), RenderTargetUsage.PlatformContents);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Render target creation failed", e);
            }
        } // Create

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            renderTargetList.Remove(this);
            renderTarget.Dispose();
        } // DisposeManagedResources

        #endregion

        #region On Window Size Changed

        /// <summary>
        /// In older versions of XNA, when a Device was lost you had to manually recreate some resources.
        /// Now there is no need to do it, however some render targets are bound to the window size, so these render targets will be recreated with the correct size.
        /// </summary>
        private void OnWindowSizeChanged(object sender, EventArgs e)
        {
            if (sizeType == SizeType.FullScreen || sizeType == SizeType.HalfScreen || sizeType == SizeType.QuarterScreen)
            {
                // The size changes with the new window size.
                renderTarget.Dispose();
                Width = CalculateSize(sizeType.Value).Width;
                Height = CalculateSize(sizeType.Value).Height;
                Create();
            }
        } // OnWindowSizeChanged

        #endregion

        #region Calculate Size

        /// <summary>
        /// Calculate size from size type.
        /// </summary>
        private static Size CalculateSize(SizeType sizeType)
        {
            int width;
            int height;
            switch (sizeType)
            {
                case SizeType.FullScreen:
                    width = SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth;
                    height = SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight;
                    break;
                case SizeType.HalfScreen:
                    width = SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth / 2;
                    height = SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight / 2;
                    break;
                case SizeType.QuarterScreen:
                    width = SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth / 4;
                    height = SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight / 4;
                    break;
                case SizeType.Square256X256:
                    width = 256;
                    height = 256;
                    break;
                case SizeType.Square512X512:
                    width = 512;
                    height = 512;
                    break;
                case SizeType.Square1024X1024:
                    width = 1024;
                    height = 1024;
                    break;
                case SizeType.Square2048X2048:
                    width = 1024;
                    height = 1024;
                    break;
                default:
                    throw new ArgumentException("Size type does not exist (probably a bug).");
            }
            return new Size(width, height);
        } // CalculateSize

        #endregion
        
        #region Calculate MultiSample Quality

        /// <summary>
        /// Calculate multiSample quality.
        /// </summary>
        private static int CalculateMultiSampleQuality(AntialiasingType antialiasingTypeType)
        {
            switch (antialiasingTypeType)
            {
                case AntialiasingType.NoAntialiasing:
                    return 0;
                case AntialiasingType.System:
                    return SystemInformation.Device.PresentationParameters.MultiSampleCount;
                case AntialiasingType.TwoSamples:
                    return 2;
                case AntialiasingType.FourSamples:
                    return 4;
                case AntialiasingType.EightSamples:
                    return 8;
                case AntialiasingType.SixtySamples:
                    return 16;
                default:
                    throw new ArgumentException("Render Target error. Antialiasing type doesn't exist (probably a bug).");
            }
        } // CalculateMultiSampleQuality

        #endregion

        #region Enable Render Target

        /// <summary>
        /// Set render target for render.
        /// </summary>
        public void EnableRenderTarget()
        {
            if (currentRenderTarget[0] != null)
                throw new InvalidOperationException("Render Target: unable to set render target. Another render target is still set. If you want to set multiple render targets use the static method called EnableRenderTargets.");
            SystemInformation.Device.SetRenderTarget(renderTarget);
            currentRenderTarget[0] = this;
            alreadyResolved = false;
        } // EnableRenderTarget

        /// <summary>
        /// Enable multiple render targets.
        /// </summary>
        /// <param name="renderTargetBinding">
        /// This structure is used to set multiple render targets without generating garbage in the process.
        /// You can create it using the BindRenderTargets method.
        /// </param>
        public static void EnableRenderTargets(RenderTargetBinding renderTargetBinding)
        {
            if (currentRenderTarget[0] != null)
                throw new InvalidOperationException("Render Target: unable to set render target. Another render target is still set.");
            for (int i = 0; i < renderTargetBinding.renderTargets.Length; i++)
            {
                currentRenderTarget[i] = renderTargetBinding.renderTargets[i];
                renderTargetBinding.renderTargets[i].alreadyResolved = false;
            }
            SystemInformation.Device.SetRenderTargets(renderTargetBinding.InternalBinding);
        } // EnableRenderTargets
        
        #endregion

        #region Clear

        /// <summary>
        /// Clear render target.
        /// This method will only work if the render target was set before with SetRenderTarget.
        /// </summary>
        public void Clear(Color clearColor)
        {            
            if (currentRenderTarget[0] != this)
                throw new InvalidOperationException("Render Target: You can't clear a render target without first setting it");
            if (depthFormat == DepthFormat.None)
                SystemInformation.Device.Clear(clearColor);
            else
                SystemInformation.Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearColor, 1.0f, 0);
        } // Clear

        /// <summary>
        /// Clear render target.
        /// This is the same as calling Clear from the first render target.
        /// </summary>
        public static void ClearCurrentRenderTargets(Color clearColor)
        {            
            if (currentRenderTarget[0] == null)
                throw new InvalidOperationException("Render Target: You can't clear a render target without first setting it");
            currentRenderTarget[0].Clear(clearColor);
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
            if (currentRenderTarget[0] != this)
            {
                throw new InvalidOperationException("Render Target: Cannot call disable to a render target without first setting it.");
            }
            if (currentRenderTarget[1] != null)
                throw new InvalidOperationException("Render Target: There are multiple render targets enabled. Use RenderTarget.BackToBackBuffer instead.");
            
            alreadyResolved = true;
            currentRenderTarget[0] = null;
            SystemInformation.Device.SetRenderTarget(null);
        } // DisableRenderTarget

        /// <summary>
        /// Back to back buffer (frame buffer).
        /// </summary>
        public static void DisableCurrentRenderTargets()
        {
            for (int i = 0; i < 4; i++)
            {
                if (currentRenderTarget[i] != null)
                    currentRenderTarget[i].alreadyResolved = true;
                currentRenderTarget[i] = null;
            }
            SystemInformation.Device.SetRenderTarget(null);
        } // DisableCurrentRenderTargets

        #endregion

        #region Binding

        /// <summary>
        /// Bind render targets so that they could be set together without generate garbage in the process.
        /// </summary>
        public static RenderTargetBinding BindRenderTargets(RenderTarget renderTarget1, RenderTarget renderTarget2)
        {
            RenderTargetBinding renderTargetsBinding = new RenderTargetBinding
            {
                InternalBinding = new[]
                {
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding( renderTarget1.renderTarget),
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding( renderTarget2.renderTarget),
                },
                renderTargets = new[] { renderTarget1, renderTarget2 }
            };
            return renderTargetsBinding;
        } // BindRenderTargets

        /// <summary>
        /// Bind render targets so that they could be set together without generate garbage in the process.
        /// </summary>
        public static RenderTargetBinding BindRenderTargets(RenderTarget renderTarget1, RenderTarget renderTarget2, RenderTarget renderTarget3)
        {
            RenderTargetBinding renderTargetsBinding = new RenderTargetBinding
            {
                InternalBinding = new[] 
                {
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget1.renderTarget),
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget2.renderTarget),
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget3.renderTarget),
                },
                renderTargets = new[] {renderTarget1, renderTarget2, renderTarget3}
            };
            return renderTargetsBinding;
        } // BindRenderTargets

        /// <summary>
        /// Bind render targets so that they could be set together without generate garbage in the process.
        /// </summary>
        public static RenderTargetBinding BindRenderTargets(RenderTarget renderTarget1, RenderTarget renderTarget2, RenderTarget renderTarget3, RenderTarget renderTarget4)
        {
            RenderTargetBinding renderTargetsBinding = new RenderTargetBinding
            {
                InternalBinding = new[]
                {
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget1.renderTarget),
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget2.renderTarget),
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget3.renderTarget),
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget4.renderTarget),
                },
                renderTargets = new[] { renderTarget1, renderTarget2, renderTarget3, renderTarget4 }
            };
            return renderTargetsBinding;
        } // BindRenderTargets

        #endregion

    } // RenderTarget
} // XNAFinalEngine.Assets