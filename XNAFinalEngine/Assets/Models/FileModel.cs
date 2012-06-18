
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Support .X and fbx formats.
    /// The internal resource can’t be disposed individually. Use the Unload method from the Content Manager instead.
    /// </summary>
    public class FileModel : Model
    {

        #region Variables

        private ModelAnimation[] modelAnimations;
        private RootAnimation[] rootAnimations;

        #endregion

        #region Properties

        /// <summary>
        /// Underlying xna model object. Loaded with the content system.
        /// </summary>
        public XnaModel Resource { get; private set; }

        #region Vertices

        /// <summary>
        /// Get the vertices' positions of the model.
        /// </summary>   
        /// <remarks>
        /// This is a slow operation that generates garbage. 
        /// We could store the vertices, but there is no need to do this… for now.
        /// So why waste precious memory space? And why fragment the data if I only use this method in the loading?
        /// </remarks>
        public override Vector3[] Vectices
        {
            get
            {

                #region Vertex Count

                int vertexCount = 0;

                foreach (ModelMesh mesh in Resource.Meshes) // Foreach is faster and does not produce garbage.
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        vertexCount += part.NumVertices;
                    }
                }

                #endregion

                // The array of vertices positions
                Vector3[] verticesPosition = new Vector3[vertexCount];

                // Indicates where to write in the vertices array.
                int currentIndex = 0;

                foreach (ModelMesh mesh in Resource.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // Read the format of the vertex buffer  
                        VertexDeclaration declaration = part.VertexBuffer.VertexDeclaration;
                        VertexElement[] vertexElements = declaration.GetVertexElements();
                        // Find the element that holds the position  
                        VertexElement vertexPosition = new VertexElement();
                        foreach (VertexElement vert in vertexElements)
                        {
                            if (vert.VertexElementUsage == VertexElementUsage.Position && vert.VertexElementFormat == VertexElementFormat.Vector3)
                            {
                                vertexPosition = vert;
                                // There should only be one  
                                break;
                            }
                        }
                        // Check the position element found is valid  
                        if (vertexPosition.VertexElementUsage != VertexElementUsage.Position || vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
                        {
                            throw new InvalidOperationException("Model: Model uses unsupported vertex format!");
                        }
                        // This where we store the vertices until transformed  
                        Vector3[] partVertices = new Vector3[part.NumVertices];
                        // Read the vertices from the buffer in to the array  
                        part.VertexBuffer.GetData(part.VertexOffset * declaration.VertexStride + vertexPosition.Offset, partVertices, 0, part.NumVertices, declaration.VertexStride);
                        // Copy part vertices to the array of vertices of the model.
                        partVertices.CopyTo(verticesPosition, currentIndex);
                        currentIndex += part.NumVertices;
                    }
                }
                return verticesPosition;
            }
        } // Vertices

        #endregion

        #region Animations

        /// <summary>
        /// Gets a collection of animation clips that operate on the root of the object.
        /// These are stored by name in a dictionary, so there could for instance be clips for "Walk", "Run", "JumpReallyHigh", etc.
        /// </summary>
        public RootAnimation[] RootAnimations { get { return rootAnimations; } }

        /// <summary>
        /// Gets a collection of model animation clips. These are stored by name in a dictionary, so there could for instance be clips for "Walk", "Run", "JumpReallyHigh", etc.
        /// </summary>
        public ModelAnimation[] ModelAnimations { get { return modelAnimations; } }

        #endregion

        #region Skinning

        /// <summary>
        /// Bindpose matrices for each bone in the skeleton, relative to the parent bone.
        /// </summary>
        public List<Matrix> BindPose
        {
            get
            {
                // If there is no animation information.
                if (Resource.Tag == null || !(Resource.Tag is ModelAnimationData))
                    return null;
                return ((ModelAnimationData)Resource.Tag).BindPose;
            }
        } // BindPose

        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        public List<Matrix> InverseBindPose
        {
            get
            {
                // If there is no animation information.
                if (Resource.Tag == null || !(Resource.Tag is ModelAnimationData))
                    return null;
                return ((ModelAnimationData)Resource.Tag).InverseBindPose;
            }
        } // InverseBindPose

        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        public List<int> BoneHierarchy
        {
            get
            {
                // If there is no animation information.
                if (Resource.Tag == null || !(Resource.Tag is ModelAnimationData))
                    return null;
                return ((ModelAnimationData)Resource.Tag).BoneHierarchy;
            }
        } // BoneHierarchy

        /// <summary>
        /// The name of the bones.
        /// </summary>
        public Dictionary<string, int> BoneIndices
        {
            get
            {
                // If there is no animation information.
                if (Resource.Tag == null || !(Resource.Tag is ModelAnimationData))
                    return null;
                return ((ModelAnimationData)Resource.Tag).BoneIndices;
            }
        } // BonesName

        /// <summary>
        /// Bone count.
        /// </summary>
        public int BoneCount { get; private set; }

        /// <summary>
        /// Is the model skinned?
        /// </summary>
        public bool IsSkinned { get; private set; }

        #endregion
       
        #endregion

        #region Constructor

        /// <summary>
        /// Load a .x or fbx file.
        /// </summary>
        public FileModel(string filename)
        {
            Name = filename;
            Filename = ContentManager.GameDataDirectory + "Models\\" + filename;
            if (File.Exists(Filename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load model: File " + Filename + " does not exists!", "filename");
            }
            try
            {
                Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<XnaModel>(Filename);
                ContentManager = ContentManager.CurrentContentManager;
                // Calcuate bounding volumes
                Vector3[] vectices = Vectices;
                boundingSphere = BoundingSphere.CreateFromPoints(vectices);
                boundingBox    = BoundingBox.CreateFromPoints(vectices);
                // Mesh Count
                MeshPartsCount = 0;
                MeshesCount = Resource.Meshes.Count;
                foreach (ModelMesh mesh in Resource.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts)
                        MeshPartsCount++;
                // Animations
                if (Resource.Tag != null && Resource.Tag is ModelAnimationData && ((ModelAnimationData)Resource.Tag).BoneHierarchy != null)
                {
                    BoneCount = ((ModelAnimationData)Resource.Tag).BoneHierarchy.Count;
                    IsSkinned = ((ModelAnimationData)Resource.Tag).InverseBindPose != null;
                }
                if (Resource.Tag != null && Resource.Tag is ModelAnimationData)
                {
                    // Model Animations.
                    modelAnimations = new ModelAnimation[((ModelAnimationData)Resource.Tag).ModelAnimationClips.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, ModelAnimationClip> modelAnimation in ((ModelAnimationData)Resource.Tag).ModelAnimationClips)
                    {
                        modelAnimations[i] = new ModelAnimation(modelAnimation.Key, modelAnimation.Value);
                        i++;
                    }
                    // Root Animations.
                    rootAnimations = new RootAnimation[((ModelAnimationData)Resource.Tag).RootAnimationClips.Count];
                    i = 0;
                    foreach (KeyValuePair<string, RootAnimationClip> rootAnimation in ((ModelAnimationData)Resource.Tag).RootAnimationClips)
                    {
                        rootAnimations[i] = new RootAnimation(rootAnimation.Key, rootAnimation.Value);
                        i++;
                    }
                }
                else
                {
                    modelAnimations = new ModelAnimation[0];
                    rootAnimations = new RootAnimation[0];
                }
            }
            catch (ObjectDisposedException)
            {
                throw new InvalidOperationException("Content Manager: Content manager disposed");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load model: " + filename, e);
            }
        } // FileModel

        #endregion

        #region Update World and Skin Transforms

        /// <summary>
        /// Calculate and update the world transform matrices.
        /// </summary>
        internal void UpdateWorldTransforms(Matrix[] boneTransform, Matrix[] worldTransforms)
        {
            if (boneTransform == null)
                throw new ArgumentNullException("boneTransform");
            if (worldTransforms == null)
                throw new ArgumentNullException("worldTransforms");
            if (!IsSkinned)
                throw new InvalidOperationException("Model: Unable to perform skinning operation over a non-skinned model.");
            // Root bone.
            worldTransforms[0] = boneTransform[0];
            // Child bones.
            for (int bone = 1; bone < BoneCount; bone++)
            {
                int parentBone = ((ModelAnimationData)Resource.Tag).BoneHierarchy[bone];
                worldTransforms[bone] = boneTransform[bone] * worldTransforms[parentBone];
            }
        } // UpdateWorldTransforms

        /// <summary>
        /// Calculate and update the skin transform matrices.
        /// </summary>
        internal void UpdateSkinTransforms(Matrix[] worldTransforms, Matrix[] skinTransforms)
        {
            if (worldTransforms == null)
                throw new ArgumentNullException("worldTransforms");
            if (skinTransforms == null)
                throw new ArgumentNullException("skinTransforms");
            if (!IsSkinned)
                throw new InvalidOperationException("Model: Unable to perform skinning operation over a non-skinned model.");
            for (int bone = 0; bone < BoneCount; bone++)
            {
                skinTransforms[bone] = ((ModelAnimationData)Resource.Tag).InverseBindPose[bone] * worldTransforms[bone];
            }
        } // UpdateSkinTransforms

        #endregion

        #region Render

        /// <summary>
        /// Render the model.
        /// </summary>
        /// <remarks>
        /// Don't call it excepting see the model on the screen.
        /// This is public to allow doing some specific tasks not implemented in the engine.
        /// </remarks>
        public override void Render()
        {
            // Go through all meshes in the model
            foreach (ModelMesh mesh in Resource.Meshes) // foreach is faster than for because no range checking is performed.
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // Set vertex buffer and index buffer
                    EngineManager.Device.SetVertexBuffer(part.VertexBuffer);
                    EngineManager.Device.Indices = part.IndexBuffer;
                    // And render all primitives
                    EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    // Update statistics
                    Statistics.DrawCalls++;
                    Statistics.TrianglesDrawn += part.PrimitiveCount;
                    Statistics.VerticesProcessed += part.NumVertices;
                }
            }
        } // Render

        /// <summary>
        /// Render a mesh part of the model.
        /// </summary>
        /// <remarks>
        /// Don't call it excepting see the model on the screen.
        /// This is public to allow doing some specific tasks not implemented in the engine.
        /// </remarks>
        public override void RenderMeshPart(int meshIndex, int meshPart)
        {
            if (meshIndex >= MeshesCount || meshIndex < 0)
                throw new IndexOutOfRangeException("meshIndex");
            ModelMeshPart part = Resource.Meshes[meshIndex].MeshParts[meshPart];
            // Set vertex buffer and index buffer
            EngineManager.Device.SetVertexBuffer(part.VertexBuffer);
            EngineManager.Device.Indices = part.IndexBuffer;
            // And render all primitives
            EngineManager.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0,
                                                        part.NumVertices, part.StartIndex, part.PrimitiveCount);
            // Update statistics
            Statistics.DrawCalls++;
            Statistics.TrianglesDrawn += part.PrimitiveCount;
            Statistics.VerticesProcessed += part.NumVertices;
        } // RenderMesh

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            foreach (ModelAnimation modelAnimation in modelAnimations)
            {
                modelAnimation.Dispose();
            }
            foreach (RootAnimation rootAnimation in rootAnimations)
            {
                rootAnimation.Dispose();
            }
        } // DisposeManagedResources

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal override void RecreateResource()
        {
            Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<XnaModel>(Filename);
        } // RecreateResource

        #endregion

    } // FileModel
} // XNAFinalEngine.Assets