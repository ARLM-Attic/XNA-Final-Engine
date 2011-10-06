
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
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Base class to represent the geometrical information of primitive models (spheres, cubes, cylinders, cones)
    /// Limitation: the model created doesn't have the tangent information.
    /// In this case the best is to create the primitives in any 3D program and import them with "create tangent" activated in the content pipeline property.
    /// </summary>
    public abstract class PrimitiveModel : Model
    {

        #region Variables

        /// <summary>
        /// Number of vertices.
        /// </summary>
        protected int numberVertices;

        /// <summary>
        /// Vertex Buffer.
        /// </summary>
        protected VertexBuffer vertexBuffer;

        /// <summary>
        /// Number of indices.
        /// </summary>
        protected int numberIndices;

        /// <summary>
        /// Index Buffer.
        /// </summary>
        protected IndexBuffer indexBuffer;
                
        #endregion

        #region Properties

        /// <summary>
        /// Get the vertices' positions of the model.
        /// </summary>        
        /// <remarks>This is a slow operation that generates garbage. We could store the vertices here, but there is no need to do this… for now.</remarks>
        public override Vector3[] Vectices
        {
            get
            {
                Vector3[] verticesPosition = new Vector3[vertexBuffer.VertexCount];

                VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[vertexBuffer.VertexCount];
                vertexBuffer.GetData(vertices);

                for (int index = 0; index < vertices.Length; index++)
                {
                    verticesPosition[index] = vertices[index].Position;
                }

                return verticesPosition;
            }
        } // Vertices

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        } // DisposeManagedResources

        #endregion
                        
    } // PrimitiveModel

    #region Sphere Clas

    public class Sphere : PrimitiveModel
    {

        #region Constructor

        /// <summary>
        /// Creates a sphere model.
        /// </summary>
        /// <param name="stacks">Stacks</param>
        /// <param name="slices">Slices</param>
        /// <param name="radius">Radius</param>
        public Sphere(int stacks, int slices, float radius)
        {

            Name = "Sphere Primitive";

            // Calculates the resulting number of vertices and indices
            numberVertices = (stacks + 1) * (slices + 1);
            numberIndices = (3 * stacks * (slices + 1)) * 2;
            int[] indices = new int[numberIndices];
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numberVertices];
               
            float stackAngle = MathHelper.Pi / stacks;  
            float sliceAngle = (float)(Math.PI * 2.0) / slices;  
             
            // Generate the group of Stacks for the sphere  
            int wVertexIndex = 0;  
            int vertexCount = 0;  
            int indexCount = 0;  
 
            for (int stack = 0; stack < (stacks+1); stack++)  
            {  
 
                float r = (float)Math.Sin(stack * stackAngle);  
                float y = (float)Math.Cos(stack * stackAngle);  
 
                // Generate the group of segments for the current Stack  
                for (int slice = 0; slice < (slices+1); slice++)  
                {  
                    float x = r * (float)Math.Sin(slice * sliceAngle);  
                    float z = r * (float)Math.Cos(slice * sliceAngle);  
                    vertices[vertexCount].Position = new Vector3(x * radius, y * radius, z * radius);
 
                    vertices[vertexCount].Normal = Vector3.Normalize(new Vector3(x, y, z));  
 
                    vertices[vertexCount].TextureCoordinate = new Vector2((float)slice / (float)slices, (float)stack / (float)stacks);
                    vertexCount++;  
                    if (stack != (stacks - 1))  
                    {  
                        // First Face
                        indices[indexCount++] = wVertexIndex + (slices + 1);
                        
                        indices[indexCount++] = wVertexIndex;

                        indices[indexCount++] = wVertexIndex + 1;
                        
                        // Second Face
                        indices[indexCount++] = wVertexIndex + (slices);

                        indices[indexCount++] = wVertexIndex;  
                        
                        indices[indexCount++] = wVertexIndex + (slices + 1);  
                        
                        wVertexIndex++;  
                    }  
                }  
            }
            vertexBuffer = new VertexBuffer(EngineManager.Device, typeof(VertexPositionNormalTexture), numberVertices, BufferUsage.None);
            vertexBuffer.SetData(vertices, 0, vertices.Length);
            indexBuffer = new IndexBuffer(EngineManager.Device, typeof(int), numberIndices, BufferUsage.None);
            indexBuffer.SetData(indices, 0, indices.Length);
        } // Sphere

        #endregion

    } // Sphere

    #endregion

    #region Box Clas

    public class Box : PrimitiveModel
    {

        #region Constructors

        /// <summary>
        /// Creates a box model.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        public Box(float width, float height, float depth)
        {
            Name = "Box Primitive";

            // Calculates the resulting number of vertices and indices  
            numberVertices = 36;
            numberIndices = 36;
            int[] indices = new int[numberIndices];
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numberVertices];
                        
            // Because the box is centered at the origin, we need to divide by two to find the + and - offsets
            float halfWidth  = width  / 2.0f;
            float halfHeight = height / 2.0f;
            float halfDepth  = depth  / 2.0f;

            Vector3 topLeftFront     = new Vector3(-halfWidth, halfHeight, halfDepth);
            Vector3 bottomLeftFront  = new Vector3(-halfWidth, -halfHeight, halfDepth);
            Vector3 topRightFront    = new Vector3(halfWidth, halfHeight, halfDepth);
            Vector3 bottomRightFront = new Vector3(halfWidth, -halfHeight, halfDepth);
            Vector3 topLeftBack      = new Vector3(-halfWidth, halfHeight, -halfDepth);
            Vector3 topRightBack     = new Vector3(halfWidth, halfHeight, -halfDepth);
            Vector3 bottomLeftBack   = new Vector3(-halfWidth, -halfHeight, -halfDepth);
            Vector3 bottomRightBack  = new Vector3(halfWidth, -halfHeight, -halfDepth);

            Vector2 textureTopLeft     = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight    = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft  = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            Vector3 frontNormal  = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 backNormal   = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 topNormal    = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 leftNormal   = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightNormal  = new Vector3(1.0f, 0.0f, 0.0f);

            // Front face.
            vertices[1] = new VertexPositionNormalTexture(topLeftFront,     frontNormal, textureTopLeft);
            vertices[0] = new VertexPositionNormalTexture(bottomLeftFront,  frontNormal, textureBottomLeft);
            vertices[2] = new VertexPositionNormalTexture(topRightFront,    frontNormal, textureTopRight);
            vertices[4] = new VertexPositionNormalTexture(bottomLeftFront,  frontNormal, textureBottomLeft);
            vertices[3] = new VertexPositionNormalTexture(bottomRightFront, frontNormal, textureBottomRight);
            vertices[5] = new VertexPositionNormalTexture(topRightFront,    frontNormal, textureTopRight);

            // Back face.
            vertices[7] = new VertexPositionNormalTexture(topLeftBack,      backNormal, textureTopRight);
            vertices[6] = new VertexPositionNormalTexture(topRightBack,     backNormal, textureTopLeft);
            vertices[8] = new VertexPositionNormalTexture(bottomLeftBack,   backNormal, textureBottomRight);
            vertices[10] = new VertexPositionNormalTexture(bottomLeftBack,  backNormal, textureBottomRight);
            vertices[9] = new VertexPositionNormalTexture(topRightBack,     backNormal, textureTopLeft);
            vertices[11] = new VertexPositionNormalTexture(bottomRightBack, backNormal, textureBottomLeft);

            // Top face.
            vertices[13] = new VertexPositionNormalTexture(topLeftFront,  topNormal, textureBottomLeft);
            vertices[12] = new VertexPositionNormalTexture(topRightBack,  topNormal, textureTopRight);
            vertices[14] = new VertexPositionNormalTexture(topLeftBack,   topNormal, textureTopLeft);
            vertices[16] = new VertexPositionNormalTexture(topLeftFront,  topNormal, textureBottomLeft);
            vertices[15] = new VertexPositionNormalTexture(topRightFront, topNormal, textureBottomRight);
            vertices[17] = new VertexPositionNormalTexture(topRightBack,  topNormal, textureTopRight);

            // Bottom face. 
            vertices[19] = new VertexPositionNormalTexture(bottomLeftFront,  bottomNormal, textureTopLeft);
            vertices[18] = new VertexPositionNormalTexture(bottomLeftBack,   bottomNormal, textureBottomLeft);
            vertices[20] = new VertexPositionNormalTexture(bottomRightBack,  bottomNormal, textureBottomRight);
            vertices[22] = new VertexPositionNormalTexture(bottomLeftFront,  bottomNormal, textureTopLeft);
            vertices[21] = new VertexPositionNormalTexture(bottomRightBack,  bottomNormal, textureBottomRight);
            vertices[23] = new VertexPositionNormalTexture(bottomRightFront, bottomNormal, textureTopRight);

            // Left face.
            vertices[25] = new VertexPositionNormalTexture(topLeftFront,    leftNormal, textureTopRight);
            vertices[24] = new VertexPositionNormalTexture(bottomLeftBack,  leftNormal, textureBottomLeft);
            vertices[26] = new VertexPositionNormalTexture(bottomLeftFront, leftNormal, textureBottomRight);
            vertices[28] = new VertexPositionNormalTexture(topLeftBack,     leftNormal, textureTopLeft);
            vertices[27] = new VertexPositionNormalTexture(bottomLeftBack,  leftNormal, textureBottomLeft);
            vertices[29] = new VertexPositionNormalTexture(topLeftFront,    leftNormal, textureTopRight);

            // Right face. 
            vertices[31] = new VertexPositionNormalTexture(topRightFront,    rightNormal, textureTopLeft);
            vertices[30] = new VertexPositionNormalTexture(bottomRightFront, rightNormal, textureBottomLeft);
            vertices[32] = new VertexPositionNormalTexture(bottomRightBack,  rightNormal, textureBottomRight);
            vertices[34] = new VertexPositionNormalTexture(topRightBack,     rightNormal, textureTopRight);
            vertices[33] = new VertexPositionNormalTexture(topRightFront,    rightNormal, textureTopLeft);
            vertices[35] = new VertexPositionNormalTexture(bottomRightBack,  rightNormal, textureBottomRight);

            for (int i = 0; i < 36; i++)
            {
                indices[i] = i;
            }

            vertexBuffer = new VertexBuffer(EngineManager.Device, typeof(VertexPositionNormalTexture), numberVertices, BufferUsage.None);
            vertexBuffer.SetData(vertices, 0, vertices.Length);
            indexBuffer = new IndexBuffer(EngineManager.Device, typeof(int), numberIndices, BufferUsage.None);
            indexBuffer.SetData(indices, 0, indices.Length);
        } // Box

        /// <summary>
        /// Creates a box model.
        /// </summary>
        /// <param name="size">Size</param>
        public Box(float size) : this(size, size, size)
        {
        } // Box        

        #endregion

    } // Box

    #endregion

    #region Plane Clas

    public class Plane : PrimitiveModel
    {

        #region Constructor
                
        /// <summary>
        /// Creates a XY plane model.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Plane(float width, float height)
        {
            // Because the plane is centered at the origin, need to divide by two to find the + and - offsets
            width = width / 2.0f;
            height = height / 2.0f;

            Vector3 topLeft = new Vector3(-width, 0, height);
            Vector3 bottomLeft = new Vector3(-width, 0, -height);
            Vector3 topRight = new Vector3(width, 0, height);
            Vector3 bottomRight = new Vector3(width, 0, -height);

            CreatePlane(topLeft, bottomLeft, topRight, bottomRight);

        } // Plane

        /// <summary>
        /// Creates a XY plane model.
        /// </summary>
        /// <param name="size">Size</param>
        public Plane(float size) : this(size, size)
        {
        } // Plane

        /// <summary>
        /// Creates a plane model.
        /// </summary>
        /// <param name="topLeft">Top left vertex's position</param>
        /// <param name="bottomLeft">Bottom left vertex's position</param>
        /// <param name="topRight">Top right vertex's position</param>
        /// <param name="bottomRight">Bottom right vertex's position</param>
        public Plane(Vector3 topLeft, Vector3 bottomLeft, Vector3 topRight, Vector3 bottomRight) // TODO: A constructor with only three vectors.
        {   
            CreatePlane(topLeft, bottomLeft, topRight, bottomRight);
        } // Plane

        #endregion

        #region Create Plane

        /// <summary>
        /// Creates a XY plane model.
        /// </summary>
        /// <param name="topLeft">Top left vertex's position</param>
        /// <param name="bottomLeft">Bottom left vertex's position</param>
        /// <param name="topRight">Top right vertex's position</param>
        /// <param name="bottomRight">Bottom right vertex's position</param>
        private void CreatePlane(Vector3 topLeft, Vector3 bottomLeft, Vector3 topRight, Vector3 bottomRight)
        {
            Name = "Plane Primitive";

            // Calculates the resulting number of vertices and indices  
            numberVertices = 4;
            numberIndices = 6;
            int[] indices = new int[numberIndices];
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numberVertices];

            Vector2 textureTopLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 1.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 0.0f);

            Vector3 normal = Vector3.Cross(topLeft - bottomLeft, bottomRight - bottomLeft);
            normal.Normalize();

            vertices[0] = new VertexPositionNormalTexture(topLeft, normal, textureTopLeft);
            vertices[1] = new VertexPositionNormalTexture(bottomLeft, normal, textureBottomLeft);
            vertices[2] = new VertexPositionNormalTexture(topRight, normal, textureTopRight);
            vertices[3] = new VertexPositionNormalTexture(bottomRight, normal, textureBottomRight);
            
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;

            vertexBuffer = new VertexBuffer(EngineManager.Device, typeof(VertexPositionNormalTexture), numberVertices, BufferUsage.None);
            vertexBuffer.SetData(vertices, 0, vertices.Length);
            indexBuffer = new IndexBuffer(EngineManager.Device, typeof(int), numberIndices, BufferUsage.None);
            indexBuffer.SetData(indices, 0, indices.Length);
        } // CreatePlane

        #endregion

    } // Plane

    #endregion

    #region Cylinder Clas

    public class Cylinder : PrimitiveModel
    {

        #region Constructor

        /// <summary>
        /// Creates a cylinder model.
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="length">Length</param>
        /// <param name="slices">Slices</param>
        public Cylinder(float radius, float length, int slices)
        {
            Name = "Cylinder Primitive";
            
            float sliceStep = MathHelper.TwoPi / slices;
            float textureStep = 1.0f / slices;
            // Calculates the resulting number of vertices and indices
            numberVertices = 2 + (slices * 4) + 2;
            numberIndices = slices * 3 * 2 + slices * 3 * 2;
            int[] indices = new int[numberIndices];
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numberVertices];

            // The center top and center bottom vertices //
            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, length / 2.0f, 0),  Vector3.Up,   new Vector2(0.5f, 0.5f));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, -length / 2.0f, 0), Vector3.Down, new Vector2(0.5f, 0.5f));
            
            // The other vertices
            int currentVertex = 2;
            int indexCount = 0;
                        
            float sliceAngle = 0;
            for (int i = 0; i < slices; i++)
            {
                float x = (float)Math.Cos(sliceAngle);
                float z = (float)Math.Sin(sliceAngle);

                #region Top
                vertices[currentVertex] = new VertexPositionNormalTexture(new Vector3(radius * x, length / 2, radius * z),
                                                                          Vector3.Up,
                                                                          new Vector2(x / 2.0f + 0.5f, z / 2.0f + 0.5f));

                indices[indexCount++] = 0;
                indices[indexCount++] = currentVertex;
                if (i == slices - 1)
                    indices[indexCount++] = 2;
                else
                    indices[indexCount++] = currentVertex + 1;
                #endregion

                #region Bottom
                
                vertices[currentVertex + slices] = new VertexPositionNormalTexture(new Vector3(radius * x, -length / 2, radius * z),
                                                                                   Vector3.Down,
                                                                                   new Vector2(-x / 2.0f + 0.5f, z / 2.0f + 0.5f));

                indices[indexCount++] = 1;
                if (i == slices - 1)
                    indices[indexCount++] = slices + 2;
                else
                    indices[indexCount++] = currentVertex + slices + 1;
                indices[indexCount++] = currentVertex + slices;
                
                #endregion

                #region Side

                vertices[currentVertex + 2 * slices]    = new VertexPositionNormalTexture(new Vector3(radius * x, length / 2, radius * z),
                                                                                          new Vector3(x, 0, z),
                                                                                          new Vector2(textureStep * i, 1));

                vertices[currentVertex + 3 * slices] = new VertexPositionNormalTexture(new Vector3(radius * x, -length / 2, radius * z),
                                                                                       new Vector3(x, 0, z),
                                                                                       new Vector2(textureStep * i, 0));
                // First Face
                indices[indexCount++] = currentVertex + 2 * slices;
                indices[indexCount++] = currentVertex + 3 * slices;
                if (i == slices - 1)
                {
                    vertices[currentVertex + 3 * slices + 1] = new VertexPositionNormalTexture(new Vector3(radius, length / 2, 0),
                                                                                                  new Vector3(x, 0, z),
                                                                                                  new Vector2(1, 1));

                    vertices[currentVertex + 3 * slices + 2] = new VertexPositionNormalTexture(new Vector3(radius, -length / 2, 0),
                                                                                               new Vector3(x, 0, z),
                                                                                               new Vector2(1, 0));
                    indices[indexCount++] = currentVertex + 3 * slices + 1;
                }
                else
                    indices[indexCount++] = currentVertex + 2 * slices + 1;
                // Second Face                
                indices[indexCount++] = currentVertex + 3 * slices;
                if (i == slices - 1)
                {                   
                    indices[indexCount++] = currentVertex + 3 * slices + 2;
                    indices[indexCount++] = currentVertex + 3 * slices + 1;
                }
                else
                {
                    indices[indexCount++] = currentVertex + 3 * slices + 1;
                    indices[indexCount++] = currentVertex + 2 * slices + 1;
                }
                #endregion
                                
                currentVertex++;
                sliceAngle += sliceStep;
            }

            vertexBuffer = new VertexBuffer(EngineManager.Device, typeof(VertexPositionNormalTexture), numberVertices, BufferUsage.None);
            vertexBuffer.SetData(vertices, 0, vertices.Length);
            indexBuffer = new IndexBuffer(EngineManager.Device, typeof(int), numberIndices, BufferUsage.None);
            indexBuffer.SetData(indices, 0, indices.Length);
        } // Cylinder

        #endregion

    } // Cylinder

    #endregion

    #region Cone Class

    public class Cone : PrimitiveModel
    {

        #region Constructor

        /// <summary>
        /// Creates a cone model
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="length">Length</param>
        /// <param name="slices">Slices</param>
        public Cone(float radius, float length, int slices)
        {
            Name = "Cylinder Primitive";

            float sliceStep = MathHelper.TwoPi / slices;
            // Calculates the resulting number of vertices and indices  
            numberVertices = 2 + (slices * 2);// +2;
            numberIndices = slices * 3 * 2;
            int[] indices = new int[numberIndices];
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numberVertices];

            // The center top and center bottom vertices //
            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, length, 0), Vector3.Up, new Vector2(0.5f, 0.5f));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), Vector3.Down, new Vector2(0.5f, 0.5f));

            // The other vertices
            int currentVertex = 2;
            int indexCount = 0;
            float sliceAngle = 0;

            for (int i = 0; i < slices; i++)
            {
                float x = (float)Math.Cos(sliceAngle);
                float z = (float)Math.Sin(sliceAngle);

                #region Top

                vertices[currentVertex] = new VertexPositionNormalTexture(new Vector3(radius * x, 0, radius * z),
                                                                          Vector3.Up,
                                                                          new Vector2(x / 2.0f + 0.5f, z / 2.0f + 0.5f));

                indices[indexCount++] = 0;
                indices[indexCount++] = currentVertex;
                if (i == slices - 1)
                    indices[indexCount++] = 2;
                else
                    indices[indexCount++] = currentVertex + 1;

                #endregion

                #region Bottom

                vertices[currentVertex + slices] = new VertexPositionNormalTexture(new Vector3(radius * x, 0, radius * z),
                                                                                   Vector3.Down,
                                                                                   new Vector2(-x / 2.0f + 0.5f, z / 2.0f + 0.5f));

                indices[indexCount++] = 1;
                if (i == slices - 1)
                    indices[indexCount++] = slices + 2;
                else
                    indices[indexCount++] = currentVertex + slices + 1;
                indices[indexCount++] = currentVertex + slices;

                #endregion
                
                currentVertex++;
                sliceAngle += sliceStep;
            }

            vertexBuffer = new VertexBuffer(EngineManager.Device, typeof(VertexPositionNormalTexture), numberVertices, BufferUsage.None);
            vertexBuffer.SetData(vertices, 0, vertices.Length);
            indexBuffer = new IndexBuffer(EngineManager.Device, typeof(int), numberIndices, BufferUsage.None);
            indexBuffer.SetData(indices, 0, indices.Length);
        } // Cone

        #endregion

    } // Cone

    #endregion

} // XNAFinalEngine.Assets
