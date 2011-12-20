
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
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Draws primitives (points, lines, triangles and curves) in world space and/or screen space. 
    /// 
    /// To render a primitive:
    /// Begin(PrimitiveType): It only supports list (strips and fan not supported).
    /// AddVertex(position, color): if you use position as Vertex2 the coordinates will be in screen space,
    ///                             if you use position as Vertex3 the coordinates will be in world space.
    /// End(): to finally render the primitives.
    /// 
    /// This class can be extended by using sprites as alternative to the vertex render. 
    /// Moreover it could uses different shaders or render techniques (like using planes or spheres).
    /// 
    /// Helpers:
    /// 
    /// * Draw 2D plane (solid or wireframe).
    /// 
    /// * Draw 3D plane (wireframe). The solid version is in primitive models.
    /// 
    /// * Draw bounding box and bounding sphere.
    /// 
    /// If you want to draw in clipping space you can do it using the screen space methods and multiplying the value (between 0 and 1) by the screen dimensions.
    /// </summary>
    public static class LineManager
    {

        #region Constant

        /// <summary>
        /// This constant controls how large the vertices buffer is.
        /// Larger buffers will require flushing less often, which can increase performance.
        /// However, having buffer that is unnecessarily large will waste memory.
        /// </summary>
        private const int defaultBufferSize = 500;

        #endregion

        #region Variables

        // Camera matrices.
        private static Matrix viewMatrix, projectionMatrix;

        // XNA basic shader. We use it to render simple lines.
        private static readonly BasicEffect basicEffect;

        // A block of vertices that calling AddVertex will fill. 
        // Flush will draw using this array, and will determine how many primitives to draw from positionInBuffer.
        private static readonly VertexPositionColor[] vertices = new VertexPositionColor[defaultBufferSize]; 
        
        // Keeps track of how many vertices have been added. this value increases until we run out of space in the buffer, at which time Flush is automatically called.
        private static int positionInBuffer;

        // This value is set by Begin, and is the type of primitives that we are drawing.
        private static PrimitiveType primitiveType;

        // How many verts does each of these primitives take up? points are 1, lines are 2, and triangles are 3.
        private static int numVertsPerPrimitive;

        // HasBegun is flipped to true once Begin is called, and is used to make sure users don't call End before Begin is called.
        private static bool hasBegun;

        // Indicates if the basic effect shader values were set or not.
        private static bool shaderValuesSeted;

        // Vertex buffer to render stuff on screen.
        private static VertexBuffer vbScreen;

        #endregion

        #region Constructor

        /// <summary>
        /// Starts the primitive system. This operation it’s automatic.
        /// </summary>
        static LineManager()
        {
            // Set up a new basic effect, and enable vertex colors.
            basicEffect = new BasicEffect(EngineManager.Device) { VertexColorEnabled = true };
        } // Primitives

        #endregion
                
        #region Begin

        /// <summary>
        /// Begin is called to tell the PrimitiveBatch what kind of primitives will be drawn,
        /// and to prepare the graphics card to render those primitives.
        /// It only supports list (strips and fan not supported).
        /// </summary>
        /// <param name="_primitiveType">Only support triangle list and line list</param>
        public static void Begin(PrimitiveType _primitiveType)
        {
            if (hasBegun)
            {
                throw new InvalidOperationException("End must be called before Begin can be called again.");
            }

            // These three types reuse vertices, so we can't flush properly without more complex logic.
            // Since that's a bit too complicated for this sample, we'll simply disallow them.
            if (primitiveType == PrimitiveType.LineStrip || primitiveType == PrimitiveType.TriangleStrip)
            {
                throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
            }

            EngineManager.Device.DepthStencilState = DepthStencilState.None;

            primitiveType = _primitiveType;

            // how many verts will each of these primitives require?
            numVertsPerPrimitive = NumberVerticesPerPrimitive(primitiveType);

            // Set the world matrix. The matrix could be changed after the begin by other class method.
            basicEffect.World = Matrix.Identity;

            // Flip the error checking boolean. It's now ok to call AddVertex, Flush, and End.
            hasBegun = true;
            shaderValuesSeted = false;
        } // Begin

        #endregion

        #region Set General Parameters

        /// <summary>
        /// Set Camera Matrices.
        /// </summary>        
        public static void SetCameraMatrices(Matrix _viewMatrix, Matrix _projectionMatrix)
        {
            viewMatrix = _viewMatrix;
            projectionMatrix = _projectionMatrix;
        } // SetCameraMatrices

        #endregion

        #region Add Vertex

        /// <summary>
        /// AddVertex is called to add another vertex to be rendered. 
        /// To draw a point, AddVertex must be called once. for lines, twice, and for triangles 3 times.
        /// This function can only be called once begin has been called.
        /// If there is not enough room in the vertices buffer, Flush is called automatically.
        /// It works with screen space coordinates.
        /// </summary>
        public static void AddVertex(Vector2 position, Color color)
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
            }

            if (!shaderValuesSeted)
            {
                // Work in screen coordinates.
                if (RenderTarget.CurrentRenderTarget[0] != null)
                    basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, RenderTarget.CurrentRenderTarget[0].Width, RenderTarget.CurrentRenderTarget[0].Height, 0, 0, 1);
                else
                    basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Screen.Width, Screen.Height, 0, 0, 1);
                basicEffect.View = Matrix.Identity;
                shaderValuesSeted = true;
            }

            // are we starting a new primitive? if so, and there will not be enough room for a whole primitive, flush.
            bool newPrimitive = ((positionInBuffer % numVertsPerPrimitive) == 0);

            if (newPrimitive &&  (positionInBuffer + numVertsPerPrimitive) >= vertices.Length)
            {
                Flush();
            }

            // Once we know there's enough room, set the vertex in the buffer, and increase position.
            vertices[positionInBuffer].Position = new Vector3(position, 0);
            vertices[positionInBuffer].Color = color;
           
            positionInBuffer++;
        } // AddVertex2D

        /// <summary>
        /// AddVertex is called to add another vertex to be rendered. 
        /// To draw a point, AddVertex must be called once. for lines, twice, and for triangles 3 times.
        /// This function can only be called once begin has been called.
        /// If there is not enough room in the vertices buffer, Flush is called automatically.
        /// It works with world space coordinates.
        /// </summary>
        public static void AddVertex(Vector3 position, Color color)
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
            }

            if (!shaderValuesSeted)
            {
                // Work in world space.
                basicEffect.Projection = projectionMatrix;
                basicEffect.View = viewMatrix;
                shaderValuesSeted = true;
            }

            // Are we starting a new primitive? if so, and there will not be enough room for a whole primitive, flush.
            bool newPrimitive = ((positionInBuffer % numVertsPerPrimitive) == 0);

            if (newPrimitive && (positionInBuffer + numVertsPerPrimitive) >= vertices.Length)
            {
                Flush();
            }

            // Once we know there's enough room, set the vertex in the buffer, and increase position.
            vertices[positionInBuffer].Position = position;
            vertices[positionInBuffer].Color = color;

            positionInBuffer++;
        } // AddVertex3D

        #endregion

        #region End

        /// <summary>
        /// End is called once all the primitives have been drawn using AddVertex.
        /// It will call Flush to actually submit the draw call to the graphics card, and then tell the basic effect to end.
        /// </summary>
        public static void End()
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before End can be called.");
            }

            // Draw whatever the user wanted us to draw
            Flush();
            hasBegun = false;
        } // End
        
        /// <summary>
        /// Flush is called to issue the draw call to the graphics card. Once the draw call is made, positionInBuffer is reset,
        /// so that AddVertex can start over at the beginning. End will call this to draw the primitives that the user
        /// requested, and AddVertex will call this if there is not enough room in the buffer.
        /// </summary>
        private static void Flush()
        {
            if (!hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }

            // no work to do
            if (positionInBuffer == 0)
            {
                return;
            }

            // how many primitives will we draw?
            int primitiveCount = positionInBuffer / numVertsPerPrimitive;

            basicEffect.CurrentTechnique.Passes[0].Apply();

            // submit the draw call to the graphics card
            EngineManager.Device.DrawUserPrimitives<VertexPositionColor>(primitiveType, vertices, 0, primitiveCount);

            // now that we've drawn, it's ok to reset positionInBuffer back to zero,
            // and write over any vertices that may have been set previously.
            positionInBuffer = 0;
        } // Flush

        #endregion

        #region Number Vertices Per Primitive

        /// <summary>
        /// Tells how many vertices it will take to draw each kind of primitive.
        /// </summary>
        private static int NumberVerticesPerPrimitive(PrimitiveType primitive)
        {
            switch (primitive)
            {
                case PrimitiveType.LineList:
                    return numVertsPerPrimitive = 2;
                case PrimitiveType.TriangleList:
                    return numVertsPerPrimitive = 3;
                default:
                    throw new InvalidOperationException("primitive is not valid");
            }
        } // NumberVerticesPerPrimitive

        #endregion

        #region Draw 2D Plane

        /// <summary>
        /// Render a 2D wireframe plane. It works with world space coordinates.
        /// </summary>
        public static void Draw2DPlane(Rectangle screenRect, Color color)
        {
            Begin(PrimitiveType.LineList);
                AddVertex(new Vector2(screenRect.X, screenRect.Y), color);
                AddVertex(new Vector2(screenRect.X, screenRect.Y + screenRect.Height), color);

                AddVertex(new Vector2(screenRect.X, screenRect.Y + screenRect.Height), color);
                AddVertex(new Vector2(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height), color);

                AddVertex(new Vector2(screenRect.X + screenRect.Width, screenRect.Y), color);
                AddVertex(new Vector2(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height), color);
                
                AddVertex(new Vector2(screenRect.X + screenRect.Width, screenRect.Y), color);                
                AddVertex(new Vector2(screenRect.X, screenRect.Y), color);
            End();
        } // Draw2DPlane
        
        /// <summary>
        /// Render a 2D solid plane. It works with world space coordinates.
        /// </summary>
        public static void DrawSolid2DPlane(Rectangle screenRect, Color color)
        {
            DrawSolid2DPlane(screenRect, color, color, color, color);
        } // DrawSolid2DPlane

        /// <summary>
        /// Render a 2D solid plane with a top and a bottom color. It works with world space coordinates.
        /// </summary>
        public static void DrawSolid2DPlane(Rectangle screenRect, Color topColor, Color bottomColor)
        {
            DrawSolid2DPlane(screenRect, topColor, topColor, bottomColor, bottomColor);
        } // DrawSolid2DPlane

        /// <summary>
        /// Render a 2D solid plane. It works with world space coordinates.
        /// </summary>
        /// <param name="screenRect">Rectangle</param>
        /// <param name="topLeftColor">Top Left Color</param>
        /// <param name="topRightColor">Top Right Color</param>
        /// <param name="bottomLeftColor">Bottom Left Color</param>
        /// <param name="bottomRightColor">Bottom Right Color</param>
        public static void DrawSolid2DPlane(Rectangle screenRect, Color topLeftColor, Color topRightColor, Color bottomLeftColor, Color bottomRightColor)
        {
            // Seteo la matriz de mundo. Va a ser la misma para todos los casos. Por supuesto que el programa puede modificarla despues del begin.
            basicEffect.World = Matrix.Identity;
            // Trabajamos con coordenadas de pantalla
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, EngineManager.Device.Viewport.Width, EngineManager.Device.Viewport.Height, 0, 0, 1);
            basicEffect.View = Matrix.Identity;
            EngineManager.Device.RasterizerState = new RasterizerState { CullMode = CullMode.CullClockwiseFace};
            basicEffect.CurrentTechnique.Passes[0].Apply();

            VertexPositionColor[] vertices = new []
            {
                new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y, 0f), topLeftColor),
                new VertexPositionColor(new Vector3(screenRect.X, screenRect.Y + screenRect.Height, 0f),  bottomLeftColor),
                new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y, 0f),  topRightColor),
                new VertexPositionColor(new Vector3(screenRect.X + screenRect.Width, screenRect.Y + screenRect.Height, 0f),   bottomRightColor),
            };
            vbScreen = new VertexBuffer(EngineManager.Device, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);

            vbScreen.SetData(vertices);
            
            EngineManager.Device.SetVertexBuffer(vbScreen);
            EngineManager.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        } // DrawSolid2DPlane
        
        #endregion

        #region Draw Billboard Plane

        /// <summary>
        /// Draw a Billboard Plane.
        /// </summary>
        public static void Draw3DBillboardPlane(Vector3 center, float size, Color color, Vector3 cameraPosition, Vector3 cameraUp)
        {
            Draw3DBillboardPlane(center, size, size, color, cameraPosition, cameraUp);
        } // Draw3DBillboardPlane

        /// <summary>
        /// Draw a Billboard Plane.
        /// </summary>
        public static void Draw3DBillboardPlane(Vector3 center, float width, float height, Color color, Vector3 cameraPosition, Vector3 cameraUp)
        {
            width = width / 2.0f;
            height = height / 2.0f;

            vertices[0].Position = new Vector3(-width, height, 0);
            vertices[1].Position = new Vector3(-width, -height, 0);
            vertices[2].Position = new Vector3( width, -height, 0);
            vertices[3].Position = new Vector3( width,  height, 0);

            Begin(PrimitiveType.LineList);

            basicEffect.World = Matrix.CreateBillboard(center, cameraPosition, cameraUp, null);

                AddVertex(vertices[0].Position, color);
                AddVertex(vertices[1].Position, color);
                            
                AddVertex(vertices[2].Position, color);
                AddVertex(vertices[3].Position, color);

                AddVertex(vertices[3].Position, color);
                AddVertex(vertices[0].Position, color);

                AddVertex(vertices[1].Position, color);
                AddVertex(vertices[2].Position, color);

            End();
        } // Draw3DBillboardPlane

        #endregion

        #region Draw Bounding Box

        /// <summary>
        /// Draw a Bounding Box in the world center.
        /// </summary>
        public static void DrawBoundingBox(BoundingBox boundingBox, Color color)
        {
            DrawBoundingBox(boundingBox, color, Matrix.Identity);
        } // DrawBoundingBox

        /// <summary>
        /// Draw a Bounding Box wherever you want.
        /// </summary>
        public static void DrawBoundingBox(BoundingBox boundingBox, Color color, Matrix worldMatrix)
        {
            vertices[0].Position = new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z);
            vertices[1].Position = new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z);
            vertices[2].Position = new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z);
            vertices[3].Position = new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z);
            vertices[4].Position = new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z);
            vertices[5].Position = new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z);
            vertices[6].Position = new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z);
            vertices[7].Position = new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z);

            Begin(PrimitiveType.LineList);

                basicEffect.World = worldMatrix;

                AddVertex(vertices[0].Position, color);
                AddVertex(vertices[1].Position, color);
                AddVertex(vertices[2].Position, color);
                AddVertex(vertices[3].Position, color);

                AddVertex(vertices[4].Position, color);
                AddVertex(vertices[5].Position, color);
                AddVertex(vertices[6].Position, color);
                AddVertex(vertices[7].Position, color);

                AddVertex(vertices[0].Position, color);
                AddVertex(vertices[3].Position, color);
                AddVertex(vertices[1].Position, color);
                AddVertex(vertices[2].Position, color);

                AddVertex(vertices[4].Position, color);
                AddVertex(vertices[7].Position, color);
                AddVertex(vertices[5].Position, color);
                AddVertex(vertices[6].Position, color);

                AddVertex(vertices[0].Position, color);
                AddVertex(vertices[4].Position, color);
                AddVertex(vertices[1].Position, color);
                AddVertex(vertices[5].Position, color);

                AddVertex(vertices[2].Position, color);
                AddVertex(vertices[6].Position, color);
                AddVertex(vertices[3].Position, color);
                AddVertex(vertices[7].Position, color);
            End();
        } // DrawBoundingBox

        #endregion

        #region Draw Bounding Sphere

        /// <summary>
        /// Draw a Bounding Sphere in the world center.
        /// </summary>
        public static void DrawBoundingSphere(BoundingSphere sphere, Color color)
        {
            DrawBoundingSphere(sphere, color, Matrix.Identity);
        } // DrawBoundingSphere

        /// <summary>
        /// Draw a Bounding Sphere wherever you want.
        /// </summary>
        public static void DrawBoundingSphere(BoundingSphere sphere, Color color, Matrix worldMatrix)
        {   
            const int numCircleSegments = 36;

            const float step = 2.0f * (float)Math.PI / numCircleSegments;
                        
            Begin(PrimitiveType.LineList);

                basicEffect.World = worldMatrix;
                for (int i = 0; i < numCircleSegments; ++i)
                {
                    float u0 = (float)Math.Cos(step * i) * sphere.Radius;
                    float v0 = (float)Math.Sin(step * i) * sphere.Radius;
                    float u1 = (float)Math.Cos(step * (i + 1)) * sphere.Radius;
                    float v1 = (float)Math.Sin(step * (i + 1)) * sphere.Radius;

                    // xy
                    AddVertex(new Vector3(u0, v0, 0) + sphere.Center, color);
                    AddVertex(new Vector3(u1, v1, 0) + sphere.Center, color);

                    // xz
                    AddVertex(new Vector3(u0, 0, v0) + sphere.Center, color);
                    AddVertex(new Vector3(u1, 0, v1) + sphere.Center, color);

                    // yz
                    AddVertex(new Vector3(0, u0, v0) + sphere.Center, color);
                    AddVertex(new Vector3(0, u1, v1) + sphere.Center, color);
                }
            End();
        } // DrawBoundingSphere

        #endregion

        #region Draw curve

        /// <summary>
        /// Draws a curve in world space.
        /// </summary>
        public static void Draw3DCurve(Curve3D curve, Color color, int step = 50)
        {
            Draw3DCurve(curve, color, Matrix.Identity, step);
        } // Draw3DCurve

        /// <summary>
        /// Draws a curve in world space.
        /// </summary>
        public static void Draw3DCurve(Curve3D curve, Color color, Matrix worldMatrix, int step = 50)
        {               
            Begin(PrimitiveType.LineList);

                basicEffect.World = worldMatrix;
                AddVertex(curve.GetPoint(0), color);
                for (float i = curve.CurveTotalTime / step; i < curve.CurveTotalTime; i = i + (curve.CurveTotalTime / step))
                {
                    AddVertex(curve.GetPoint(i), color);
                    AddVertex(curve.GetPoint(i), color);
                }
                AddVertex(curve.GetPoint(curve.CurveTotalTime), color);

            End();
        } // Draw3DCurve

        /// <summary>
        /// Draws a curve in screen space.
        /// </summary>
        public static void Draw2DCurve(Curve3D curve, Color color, Vector2 position, int step = 50)
        {
            Begin(PrimitiveType.LineList);

            AddVertex(new Vector2(curve.GetPoint(0).X, curve.GetPoint(0).Y) + position, color);
            for (float i = curve.CurveTotalTime / step; i < curve.CurveTotalTime; i = i + (curve.CurveTotalTime / step))
            {
                AddVertex(new Vector2(curve.GetPoint(i).X, curve.GetPoint(i).Y) + position, color);
                AddVertex(new Vector2(curve.GetPoint(i).X, curve.GetPoint(i).Y) + position, color);
            }
            AddVertex(new Vector2(curve.GetPoint(curve.CurveTotalTime).X, curve.GetPoint(curve.CurveTotalTime).Y) + position, color);

            End();
        } // Draw2DCurve

        #endregion

    } // LineManager
} // XNAFinalEngine.Graphics
