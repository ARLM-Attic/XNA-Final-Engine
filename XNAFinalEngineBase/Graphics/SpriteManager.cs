
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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Draw a 2D texture or a 2D text directly to the screen.
    /// </summary>
    /// <remarks>
    /// The new version is similar in interface to the XNA version. However it has some constrains and some extensions.
    /// The 2D entities are drawing in gamma space and not post process is applied.
    /// They are drawing last in the pipeline so the GPU depth buffer is already gone. 
    /// However we have stored the depth information in a render target so depth calculations could still be done.
    /// </remarks>
    public class SpriteManager
    {

        #region Variables

        /// <summary>
        /// XNA Sprite batch for rendering.
        /// </summary>
        private static SpriteBatch spriteBatch;
        
        private static SamplerState samplerState2D = SamplerState.PointClamp;
        private static SamplerState samplerState3D = SamplerState.AnisotropicClamp;

        #endregion

        #region Properties

        /// <summary>
        /// Sampler State for all 2D sprites.
        /// </summary>
        public static SamplerState SamplerState2D 
        {
            get { return samplerState2D; } 
            set { samplerState2D = value; }
        } // SamplerState2D

        /// <summary>
        /// Sampler State for all 3D sprites.
        /// </summary>
        public static SamplerState SamplerState3D
        {
            get { return samplerState3D; }
            set { samplerState3D = value; }
        } // SamplerState3D

        #endregion

        #region Init

        /// <summary>
        /// Init Sprite Manager subsystem.
        /// </summary>
        public static void Init()
        {            
            spriteBatch = new SpriteBatch(SystemInformation.Device);
        } // Init

        #endregion

        #region Begin

        /// <summary>
        /// Begins a sprite batch operation.
        /// </summary>
        public static void Begin()
        {
            if (spriteBatch == null)
                throw new Exception("The Sprite Manager not initialized.");

            // In PC BlendState.AlphaBlend is a little more expensive than BlendState.Opaque when alpha = 1.
            // But PC is the powerful platform so no need to choose between the two.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, samplerState2D, DepthStencilState.None, RasterizerState.CullCounterClockwise);
        } // Begin

        #endregion

        #region Draw

        public static void DrawText(Font font, StringBuilder text, Vector2 position, Color color)
        {
            // SpriteBatch.DrawString (SpriteFont, StringBuilder, Vector2, Color, Single, Vector2, Vector2, SpriteEffects, Single)
            spriteBatch.DrawString(font.XnaSpriteFont, text, position, color, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        } // DrawText

        #endregion

        #region End

        /// <summary>
        /// Flushes the sprite batch and restores the device state to how it was before Begin was called.
        /// </summary>
        public static void End()
        {            
            spriteBatch.End();
        } // End

        #endregion

        #region Draw Texture To Full Screen

        /// <summary>
        /// Draw textures onto fullscreen.
        /// This is great for quick tests related to render targets.
        /// </summary>
        public static void DrawTextureToFullScreen(XNAFinalEngine.Assets.Texture renderTarget)
        {
            if (spriteBatch == null)
                throw new Exception("The Sprite Manager not initialized.");

            //  This is not a batch operation, for that reason the immediate mode is selected.
            // Floating point textures only works in point filtering.
            // Besides, we don’t need more than this because the render target will match the screen resolution.
            // Also there is no need for alpha blending.
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            spriteBatch.Draw(renderTarget.XnaTexture, 
                             new Rectangle(0, 0, 
                                           SystemInformation.Device.PresentationParameters.BackBufferWidth,
                                           SystemInformation.Device.PresentationParameters.BackBufferHeight),
                             Color.White);
                        
            spriteBatch.End();
        } // DrawRenderTarget

        #endregion

    } // SpriteManager
} // XNAFinalEngine.Graphics
