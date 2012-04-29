
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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Texture = XNAFinalEngine.Assets.Texture;
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
        
        private static SamplerState samplerState2D = SamplerState.LinearClamp;
        private static SamplerState samplerState3D = SamplerState.AnisotropicClamp;

        // To control the good use of begin-end.
        private static bool begined = false, begin2D = false;

        #endregion

        #region Properties

        /// <summary>
        /// Sampler State for all 2D sprites. 
        /// Default value: LinearClamp
        /// </summary>
        /// <remarks>If there is no rotation or scale perform over any HUD 2D element then use point sampling to improve performance.</remarks>
        public static SamplerState SamplerState2D 
        {
            get { return samplerState2D; } 
            set { samplerState2D = value; }
        } // SamplerState2D

        /// <summary>
        /// Sampler State for all 3D sprites.
        /// Default value: AnisotropicClamp
        /// </summary>
        public static SamplerState SamplerState3D
        {
            get { return samplerState3D; }
            set { samplerState3D = value; }
        } // SamplerState3D

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the Sprite Manager subsystem.
        /// </summary>
        internal static void Initialize()
        {            
            spriteBatch = new SpriteBatch(EngineManager.Device);
        } // Initialize

        #endregion

        #region Begin

        /// <summary>
        /// Begins a sprite batch operation in 2D.
        /// </summary>
        public static void Begin2D()
        {
            if (spriteBatch == null)
                throw new Exception("The Sprite Manager is not initialized.");
            if (begined)
                throw new InvalidOperationException("Sprite Manager: Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called.");

            // In PC BlendState.AlphaBlend is a little more expensive than BlendState.Opaque when alpha = 1.
            // But PC is the powerful platform so no need to choose between the two.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, samplerState2D, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            
            begined = true;
            begin2D = true;
        } // Begin2D

        /// <summary>
        /// Begins a sprite batch operation in 3D.
        /// </summary>
        public static void Begin3DLinearSpace(Matrix viewMatrix, Matrix projectionMatrix)
        {
            if (spriteBatch == null)
                throw new Exception("The Sprite Manager is not initialized.");
            if (begined)
                throw new InvalidOperationException("Sprite Manager: Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called.");

            // In PC BlendState.AlphaBlend is a little more expensive than BlendState.Opaque when alpha = 1.
            // But PC is the powerful platform so no need to choose between the two.
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState3D, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, SpriteShader.Instance.Resource);

            SpriteShader.Instance.BeginLinearSpace(viewMatrix, projectionMatrix);

            begined = true;
            begin2D = false;
        } // Begin3D

        /// <summary>
        /// Begins a sprite batch operation in 3D.
        /// </summary>
        public static void Begin3DGammaSpace(Matrix viewMatrix, Matrix projectionMatrix, Texture depthTexture, float farPlane)
        {
            if (spriteBatch == null)
                throw new Exception("The Sprite Manager is not initialized.");
            if (begined)
                throw new InvalidOperationException("Sprite Manager: Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called.");

            // In PC BlendState.AlphaBlend is a little more expensive than BlendState.Opaque when alpha = 1.
            // But PC is the powerful platform so no need to choose between the two.
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState3D, DepthStencilState.None, RasterizerState.CullCounterClockwise, SpriteShader.Instance.Resource);

            SpriteShader.Instance.BeginGammaSpace(viewMatrix, projectionMatrix, depthTexture, farPlane);

            begined = true;
            begin2D = false;
        } // Begin3D

        #endregion

        #region Draw Text

        /// <summary>
        /// Adds a string to a batch of sprites for rendering using the specified font, text, position, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="font">A font for diplaying text.</param>
        /// <param name="text">Text string.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite. It also includes the depth. By default, 0 represents the front layer and 1 represents a back layer.</param>
        /// <param name="color">The color to tint the font.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the upper-left corner.</param>
        /// <param name="scale">Scale factor.</param>
        public static void Draw2DText(Font font, StringBuilder text, Vector3 position, Color color, float rotation, Vector2 origin, float scale)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (!begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 3D mode.");

            spriteBatch.DrawString(font.Resource, text, new Vector2(position.X, position.Y), color, rotation, origin, scale, SpriteEffects.None, position.Z);
            // Update statistics
            Statistics.TrianglesDrawn += 2 * text.Length;
            Statistics.VerticesProcessed += 4 * text.Length;
        } // Draw2DText

        /// <summary>
        /// Draws a text in 3D space.
        /// </summary>
        /// <param name="font">A font for diplaying text.</param>
        /// <param name="text">Text string.</param>
        /// <param name="worldMatrix">The location to draw the sprite.</param>
        /// <param name="color">The color to tint the font.</param>
        public static void Draw3DText(Font font, StringBuilder text, Matrix worldMatrix, Color color)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 2D mode.");

            SpriteShader.Instance.SetParameters(Matrix.CreateFromYawPitchRoll(0, 3.1416f, 0) * worldMatrix);
            spriteBatch.DrawString(font.Resource, text, Vector2.Zero, color);
            // Update statistics
            Statistics.TrianglesDrawn += 2 * text.Length;
            Statistics.VerticesProcessed += 4 * text.Length;
        } // Draw3DText

        /// <summary>
        /// Draws a text in 3D space in billboard fashion.
        /// </summary>
        /// <param name="font">A font for diplaying text.</param>
        /// <param name="text">Text string.</param>
        /// <param name="worldMatrix">The location to draw the sprite.</param>
        /// <param name="color">The color to tint the font.</param>
        public static void Draw3DBillboardText(Font font, StringBuilder text, Matrix worldMatrix, Color color, Vector3 cameraPosition, Vector3 cameraUp, Vector3 cameraForward)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 2D mode.");

            float scale = new Vector3(worldMatrix.M11, worldMatrix.M12, worldMatrix.M13).Length();
            worldMatrix = Matrix.CreateBillboard(worldMatrix.Translation, cameraPosition, cameraUp, null);
            SpriteShader.Instance.SetParameters(Matrix.CreateScale(scale) * Matrix.CreateFromYawPitchRoll(0, 0, 3.1416f) * worldMatrix);
            spriteBatch.DrawString(font.Resource, text, Vector2.Zero, color);
            // Update statistics
            Statistics.TrianglesDrawn += 2 * text.Length;
            Statistics.VerticesProcessed += 4 * text.Length;
        } // Draw3DBillboardText

        #endregion

        #region Draw Texture

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture, position, source rectangle, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The location (in screen coordinates) to draw the sprite. It also includes the depth. By default, 0 represents the front layer and 1 represents a back layer.</param>
        /// <param name="sourceRectangle">A rectangle that specifies (in texels) the source texels from a texture. Use null to draw the entire texture.</param>
        /// <param name="color">The color to tint the font.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the upper-left corner.</param>
        /// <param name="scale">Scale factor.</param>
        public static void Draw2DTexture(Texture texture, Vector3 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (!begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 3D mode.");

            spriteBatch.Draw(texture.Resource, new Vector2(position.X, position.Y), sourceRectangle, color, rotation, origin, scale, SpriteEffects.None, position.Z);
            // Update statistics
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;
        } // Draw2DTexture

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture, position, source rectangle, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="depth">The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.</param>
        /// <param name="destinationRectangle">A rectangle that specifies (in screen coordinates) the destination for drawing the sprite. If this rectangle is not the same size as the source rectangle, the sprite will be scaled to fit.</param>
        /// <param name="sourceRectangle">A rectangle that specifies (in texels) the source texels from a texture. Use null to draw the entire texture.</param>
        /// <param name="color">The color to tint the font.</param>
        /// <param name="rotation">Specifies the angle (in radians) to rotate the sprite about its center.</param>
        /// <param name="origin">The sprite origin; the default is (0,0) which represents the upper-left corner.</param>
        public static void Draw2DTexture(Texture texture, float depth, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (!begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 3D mode.");

            spriteBatch.Draw(texture.Resource, destinationRectangle, sourceRectangle, color, rotation, origin, SpriteEffects.None, depth);
            // Update statistics
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;
        } // Draw2DTexture

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture, position, source rectangle, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="worldMatrix">The location to draw the sprite.</param>
        /// <param name="sourceRectangle">A rectangle that specifies (in texels) the source texels from a texture. Use null to draw the entire texture.</param>
        /// <param name="color">The color to tint the font.</param>
        public static void Draw3DTexture(Texture texture, Matrix worldMatrix, Rectangle? sourceRectangle, Color color)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 2D mode.");

            SpriteShader.Instance.SetParameters(worldMatrix * Matrix.CreateFromYawPitchRoll(0, 3.1416f, 0));
            spriteBatch.Draw(texture.Resource, Vector2.Zero, sourceRectangle, color);
            // Update statistics
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;
        } // Draw3DTexture

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture, position, source rectangle, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="worldMatrix">The location to draw the sprite.</param>
        /// <param name="color">The color to tint the font.</param>
        public static void Draw3DTexture(Texture texture, Matrix worldMatrix, Color color)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 2D mode.");

            SpriteShader.Instance.SetParameters(worldMatrix * Matrix.CreateFromYawPitchRoll(0, 3.1416f, 0));
            spriteBatch.Draw(texture.Resource, Vector2.Zero, color);
            // Update statistics
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;
        } // Draw3DTexture

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture, position, source rectangle, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="worldMatrix">The location to draw the sprite.</param>
        /// <param name="sourceRectangle">A rectangle that specifies (in texels) the source texels from a texture. Use null to draw the entire texture.</param>
        /// <param name="color">The color to tint the font.</param>
        public static void Draw3DBillboardTexture(Texture texture, Matrix worldMatrix, Rectangle? sourceRectangle, Color color, Vector3 cameraPosition, Vector3 cameraUp, Vector3 cameraForward)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 2D mode.");

            float scale = new Vector3(worldMatrix.M11, worldMatrix.M12, worldMatrix.M13).Length();
            worldMatrix = Matrix.CreateBillboard(worldMatrix.Translation, cameraPosition, cameraUp, cameraForward);
            SpriteShader.Instance.SetParameters(Matrix.CreateScale(scale) * Matrix.CreateFromYawPitchRoll(0, 0, 3.1416f) * worldMatrix);
            spriteBatch.Draw(texture.Resource, Vector2.Zero, sourceRectangle, color);
            // Update statistics
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;
        } // Draw3DBillboardTexture

        /// <summary>
        /// Adds a sprite to a batch of sprites for rendering using the specified texture, position, source rectangle, color, rotation, origin, scale, effects and layer.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="worldMatrix">The location to draw the sprite.</param>
        /// <param name="color">The color to tint the font.</param>
        public static void Draw3DBillboardTexture(Texture texture, Matrix worldMatrix, Color color, Vector3 cameraPosition, Vector3 cameraUp, Vector3 cameraForward)
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
            if (begin2D)
                throw new InvalidOperationException("Sprite Manager: Begin was called in 2D mode.");

            float scale = new Vector3(worldMatrix.M11, worldMatrix.M12, worldMatrix.M13).Length();
            worldMatrix = Matrix.CreateBillboard(worldMatrix.Translation, cameraPosition, cameraUp, cameraForward);
            SpriteShader.Instance.SetParameters(Matrix.CreateScale(scale) * Matrix.CreateFromYawPitchRoll(0, 0, 3.1416f) * worldMatrix);
            spriteBatch.Draw(texture.Resource, Vector2.Zero, color);
            // Update statistics
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;
        } // Draw3DBillboardTexture

        #endregion

        #region End

        /// <summary>
        /// Flushes the sprite batch and restores the device state to how it was before Begin was called.
        /// </summary>
        /// <remarks>
        /// SpriteBatch lazily grows its buffers, so it will allocate whenever a single batch draws more sprites 
        /// than any previous batch has seen, but it does not allocate anything once you reach a steady state and
        /// are no longer increasing your sprite count.
        /// Unfortunately the pre draw is not enough to avoid a garbage collection. 
        /// </remarks>
        public static void End()
        {
            if (!begined)
                throw new InvalidOperationException("Sprite Manager: End was called, but Begin has not yet been called. You must call Begin successfully before you can call End.");
            spriteBatch.End();
            begined = false;
            // Update statistics
            Statistics.DrawCalls++;
        } // End

        #endregion

        #region Draw Texture To Full Screen

        /// <summary>
        /// Draw textures onto fullscreen.
        /// This is useful for quick tests related to render targets.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <param name="alphaBlend">Some surface formats do not allow alpha blending, but in some scenarios alpha blending is need.</param>
        public static void DrawTextureToFullScreen(Texture texture, bool alphaBlend = false)
        {
            if (spriteBatch == null)
                throw new Exception("The Sprite Manager is not initialized.");

            // This is not a batch operation, for that reason the immediate mode is selected.
            // Floating point textures only works in point filtering.
            // Besides, we don’t need more than this because the render target will match the screen resolution.
            // Also there is no need for alpha blending.
            spriteBatch.Begin(SpriteSortMode.Immediate, alphaBlend ? BlendState.AlphaBlend : BlendState.Opaque,
                              SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            spriteBatch.Draw(texture.Resource,
                             new Rectangle(0, 0, EngineManager.Device.Viewport.Width, EngineManager.Device.Viewport.Height),
                             Color.White);
            // Update statistics
            Statistics.DrawCalls++;
            Statistics.TrianglesDrawn += 2;
            Statistics.VerticesProcessed += 4;

            spriteBatch.End();
        } // DrawTextureToFullScreen

        #endregion

    } // SpriteManager
} // XNAFinalEngine.Graphics
