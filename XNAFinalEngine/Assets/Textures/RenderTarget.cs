
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
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Allows to render onto textures, this kind of textures are called Render Targets.
    /// </summary>
    public sealed class RenderTarget : Texture
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
        /// The count of render targets created for naming purposes.
        /// </summary>
        private static int nameNumber = 1;

        /// <summary>
        /// Remember the last render targets we set.
        /// We can enable up to four render targets at once.
        /// </summary>
        private static readonly RenderTarget[] currentRenderTarget = new RenderTarget[4];

        #endregion

        #region Properties

        /// <summary>
        /// Return the render target texture. In XNA 4.0 the render target it's a texture.
        /// </summary>
        public override Texture2D Resource
        {
            get
            {
                if (alreadyResolved)
                    return renderTarget;
                throw new InvalidOperationException("Render Target: Unable to return render target. Render target not resolved.");
            }
        } // Resource

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
            Size = size;

            surfaceFormat = _surfaceFormat;
            depthFormat = _depthFormat;
            Antialiasing = antialiasingType;

            Create();
            Screen.ScreenSizeChanged += OnWindowSizeChanged;
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
            Size = size;

            surfaceFormat = _surfaceFormat;
            depthFormat = _hasDepthBuffer ? DepthFormat.Depth24 : DepthFormat.None;
            Antialiasing = antialiasingType;

            Create();
            Screen.ScreenSizeChanged += OnWindowSizeChanged;
        } // RenderTarget

        #endregion

        #region Create

        /// <summary>
        /// Creates render target.
        /// </summary>
        private void Create()
        {
            try
            {
                // Create render target of specified size.
                // On Xbox 360, the render target will discard contents. On PC, the render target will discard if multisampling is enabled, and preserve the contents if not.
                // I use RenderTargetUsage.PlatformContents to be little more performance friendly with PC.
                // But I assume that the system works in DiscardContents mode so that an XBOX 360 implementation works.
                // What I lose, mostly nothing, because I made my own ZBuffer texture and the stencil buffer is deleted no matter what I do.
                renderTarget = new RenderTarget2D(EngineManager.Device, Width, Height, false, surfaceFormat, depthFormat, CalculateMultiSampleQuality(Antialiasing), RenderTargetUsage.PlatformContents);
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
            Screen.ScreenSizeChanged -= OnWindowSizeChanged;
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
            if (Size == Size.FullScreen || Size == Size.HalfScreen || Size == Size.QuarterScreen ||
                Size == Size.SplitFullScreen || Size == Size.SplitHalfScreen || Size == Size.SplitQuarterScreen)
            {
                // The size changes with the new window size.
                renderTarget.Dispose();
                Create();
            }
        } // OnWindowSizeChanged

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
                    return Screen.MultiSampleQuality;
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
            EngineManager.Device.SetRenderTarget(renderTarget);
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
            try
            {
                EngineManager.Device.SetRenderTargets(renderTargetBinding.InternalBinding);
            }
            catch
            {
                // Maybe the render targets where recreated.
                // When the device is lost the RenderTargetBinding are not recreated because they are structures (garbage considerations). So I do it here.
                try
                {
                    for (int i = 0; i < renderTargetBinding.InternalBinding.Length; i++)
                    {
                        renderTargetBinding.InternalBinding[i] = new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTargetBinding.renderTargets[i].renderTarget);
                    }
                    EngineManager.Device.SetRenderTargets(renderTargetBinding.InternalBinding);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Render Target. Unable to bind the render targets.", e);
                }
            }
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
                EngineManager.Device.Clear(clearColor);
            else
                EngineManager.Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearColor, 1.0f, 0);
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
            EngineManager.Device.SetRenderTarget(null);
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
            EngineManager.Device.SetRenderTarget(null);
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
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget1.renderTarget),
                    new Microsoft.Xna.Framework.Graphics.RenderTargetBinding(renderTarget2.renderTarget),
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

        #region Pool

        // A pool of all render targets.
        private static readonly RenderTarget[] renderTargets = new RenderTarget[0];

        /// <summary>
        /// There is a pool of render targets to avoid wasting unnecessary graphic memory.
        /// The idea is that a render target has also a flag that tell us if the content is still need or not.
        /// So, when a shader needs a render target it search in the pool for an unused render target with the right characteristics (size, surface format, etc.)
        /// The problem if someone has to turn the flag false when the render target’s content is unnecessary and this could be somehow ugly. 
        /// But the graphic pipeline performance is critical, it’s not an area for the user and its complexity was diminished thanks to the new code’s restructuring.
        /// The pool should be used in the filters, shadow maps and similar shaders. Not everything.
        /// Use the Release method to return a render target to the pool.
        /// </summary>
        public static RenderTarget Fetch(Size size, SurfaceFormat surfaceFormat, DepthFormat depthFormat, AntialiasingType antialiasingType)
        {
            for (int i = 0; i < renderTargets.Length; i++)
            {
                RenderTarget renderTarget = renderTargets[i];
                if (renderTarget.Size == size && renderTarget.SurfaceFormat == surfaceFormat && renderTarget.DepthFormat == depthFormat && renderTarget.Antialiasing == antialiasingType)
                {
                    
                }
            }
            return null;
        } // Fetch

        #endregion

    } // RenderTarget
} // XNAFinalEngine.Assets