
#region License
/*

 Based in the class VBScreen.cs from RacingGame.
 License: Microsoft_Permissive_License

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// A screen plane used for most screen shaders.
    /// </summary>
    public static class ScreenPlane
    {

        /// <summary>
        /// VBScreen holds all data for the vbScreens list to reuse existing VBScreens. Handles also the VB, creation and rendering.
        /// </summary>
        private class VertexBufferScreenPlane
        {

            #region Variables

            /// <summary>
            /// Vertex buffer to render stuff on screen.
            /// </summary>
            private readonly VertexBuffer vertexBufferScreen;

            #endregion

            #region Constructor

            /// <summary>
            /// Create Vertex Buffer screen
            /// </summary>
            internal VertexBufferScreenPlane()
            {
                VertexPositionTexture[] vertices = new []
                {
                    new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 1f), new Vector2(0, 1)),
                    new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 1f),  new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(1.0f, -1.0f, 1f),  new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1f),   new Vector2(1, 0)),
                };
                vertexBufferScreen = new VertexBuffer(EngineManager.Device, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);

                vertexBufferScreen.SetData(vertices);

            } // VertexBufferScreenPlane

            #endregion

            #region Render

            /// <summary>
            /// Render
            /// </summary>
            internal void Render()
            {
                // Rendering is pretty straight forward (if you know how anyway).
                EngineManager.Device.SetVertexBuffer(vertexBufferScreen);
                EngineManager.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            } // Render

            #endregion

        } // VertexBufferScreenPlane
        
        #region Render

        /// <summary>
        /// Vb screen instance
        /// </summary>
        static VertexBufferScreenPlane vertexBufferScreenPlane;

        /// <summary>
        /// Render a screen plane.
        /// </summary>
        public static void Render()
        {
            if (vertexBufferScreenPlane == null)
                vertexBufferScreenPlane = new VertexBufferScreenPlane();

            vertexBufferScreenPlane.Render();
        } // Render

        #endregion
        
    } // ScreenPlane
} // XNAFinalEngine.Graphics
