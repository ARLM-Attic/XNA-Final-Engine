
#region License
/*
Copyright (c) 2008-2013, Schneider, José Ignacio.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	The names of its contributors cannot be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jischneider@hotmail.com)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Frustum Culling.
    /// </summary>
    /// <remarks>
    /// The objective is implementing a simple but effective culling management.
    /// In DICE’s presentation (Culling the Battlefield Data Oriented Design in Practice)
    /// they find that a slightly modified simple frustum culling could work better than 
    /// a tree based structure if a data oriented design is followed. 
    /// The question is if C# could bring us the possibility to arrange data the way we need it. I think it can.
    /// 
    /// Then they apply a software occlusion culling technique, an interesting approach
    /// but unfortunately I don’t think I can make it work in the time that I have.
    /// Moreover, a Z-Pre Pass and a simple LOD scheme could be a good alternative. Yes, I know, with a Z-Pre Pass the geometry is still processed. 
    /// CHC++ is a technique very used. In ShaderX7 there is a good article about it (it also includes the source code). But I do not have plans to implement it.
    /// </remarks>
    internal static class FrustumCulling
    {

        #region Structs

        /// <summary>
        /// Used in multitheading to pass the parameters to the task.
        /// </summary>
        private struct FrustumCullingParameters
        {
            public readonly BoundingFrustum boundingFrustum;
            public readonly List<ModelRenderer> modelsToRender;
            public readonly int startPosition, count;

            public FrustumCullingParameters(BoundingFrustum boundingFrustum, List<ModelRenderer> modelsToRender, int startPosition, int count)
            {
                this.boundingFrustum = boundingFrustum;
                this.modelsToRender = modelsToRender;
                this.startPosition = startPosition;
                this.count = count;
            }

        } // FrustumCullingParameters

        #endregion

        #region Variables
        
        // The threads that perform the frustum culling over model renderer components.
        private static MultiThreadingTask<FrustumCullingParameters> modelRendererFrustumCullingThreads;
        
        // All list are generated and added later to the main list.
        private static List<ModelRenderer>[] modelToRenderPartialLists;

        #endregion

        #region Model Renderer Frustum Culling

        /// <summary>
        /// Perform frustum culling over the model renderer components.
        /// </summary>
        public static void ModelRendererFrustumCulling(BoundingFrustum boundingFrustum, List<ModelRenderer> modelToRenderList)
        {
            // If the number of objects is high then the task is devided in threads.
            if (ModelRenderer.ComponentPool.Count > 50 && ProcessorsInformation.AvailableProcessors > 0)
            {
                // If it is the first execution then the threads are created.
                if (modelRendererFrustumCullingThreads == null)
                {
                    modelToRenderPartialLists = new List<ModelRenderer>[ProcessorsInformation.AvailableProcessors];
                    // The next variables help to avoid a lock operation.
                    for (int i = 0; i < ProcessorsInformation.AvailableProcessors; i++)
                    {
                        modelToRenderPartialLists[i] = new List<ModelRenderer>(50);
                    }
                    modelRendererFrustumCullingThreads = new MultiThreadingTask<FrustumCullingParameters>(ModelRendererFrustumCullingTask, ProcessorsInformation.AvailableProcessors);
                }
                int objectsProcessedByThread = (int)(ModelRenderer.ComponentPool.Count / (ProcessorsInformation.AvailableProcessors + 1.15f)); // The application thread makes a little more work.
                for (int i = 0; i < ProcessorsInformation.AvailableProcessors; i++)
                {
                    
                    modelRendererFrustumCullingThreads.Start(i, new FrustumCullingParameters(boundingFrustum, modelToRenderPartialLists[i], i * objectsProcessedByThread, objectsProcessedByThread));
                }
                ModelRendererFrustumCullingTask(new FrustumCullingParameters(boundingFrustum, modelToRenderList,
                                                                             ProcessorsInformation.AvailableProcessors * objectsProcessedByThread,
                                                                             ModelRenderer.ComponentPool.Count - (ProcessorsInformation.AvailableProcessors * objectsProcessedByThread)));
                // Merge the partial list with the main list.
                for (int i = 0; i < ProcessorsInformation.AvailableProcessors; i++)
                {
                    // Wait for the threads to finish.
                    modelRendererFrustumCullingThreads.WaitForTaskCompletition(i);
                    foreach (var modelToRender in modelToRenderPartialLists[i])
                    {
                        modelToRenderList.Add(modelToRender);
                    }
                    modelToRenderPartialLists[i].Clear();
                }
            }
            else
                ModelRendererFrustumCullingTask(new FrustumCullingParameters(boundingFrustum, modelToRenderList, 0, ModelRenderer.FrustumCullingDataPool.Count));
        } // ModelRendererFrustumCulling

        /// <summary>
        /// This is task that's is performed over the threads.
        /// </summary>
        private static void ModelRendererFrustumCullingTask(FrustumCullingParameters parameters)
        {
            BoundingFrustum boundingFrustum = parameters.boundingFrustum;
            List<ModelRenderer> modelsToRender = parameters.modelsToRender;
            int startPosition = parameters.startPosition;
            int count = parameters.count;

            FastBoundingFrustum fastBoundingFrustum = new FastBoundingFrustum(boundingFrustum);

            // I improved memory locality with FrustumCullingDataPool.
            // However, in order to do that I had to make the code a little more complicated and error prone.
            // There is a performance gain, but it is small. 
            // I recommend to perform this kind of optimizations is the game/application has a CPU bottleneck.
            // Besides, the old code was half data oriented.
            for (int i = startPosition; i < (count + startPosition); i++)
            {
                ModelRenderer.FrustumCullingData frustumCullingData = ModelRenderer.FrustumCullingDataPool.Elements[i];
                if (frustumCullingData.model != null && // Not need to waste cycles in this, how many ModelRenderer components will not have a model?
                    // Is Visible?
                    Layer.IsVisible(frustumCullingData.layerMask) && frustumCullingData.ownerActive && frustumCullingData.enabled)
                {
                    if (fastBoundingFrustum.Intersects(ref frustumCullingData.boundingSphere))
                    {
                        modelsToRender.Add(frustumCullingData.component);
                    }
                }
            }
        } // ModelRendererFrustumCullingTask

        #endregion

        #region Light Frustum Culling

        /// <summary>
        /// Perform frustum culling over the point lights components.
        /// </summary>
        public static void PointLightFrustumCulling(BoundingFrustum boundingFrustum, List<PointLight> pointLightsToRender)
        {
            FastBoundingFrustum fastBoundingFrustum = new FastBoundingFrustum(boundingFrustum);

            for (int i = 0; i < PointLight.ComponentPool.Count; i++)
            {
                PointLight component = PointLight.ComponentPool.Elements[i];
                if (component.Intensity > 0 && component.IsVisible)
                {
                    BoundingSphere boundingSphere = new BoundingSphere(component.cachedPosition, component.Range);
                    if (fastBoundingFrustum.Intersects(ref boundingSphere))
                    //if (boundingFrustum.Intersects(frustumCullingData.boundingSphere)) // It is not thread safe and is slow as hell.
                        pointLightsToRender.Add(component);
                }
            }
        } // PointLightFrustumCulling

        /// <summary>
        /// Frustum Culling.
        /// </summary>
        /// <param name="boundingFrustum">Bounding Frustum.</param>
        /// <param name="spotLightsToRender">The result.</param>
        public static void SpotLightFrustumCulling(BoundingFrustum boundingFrustum, List<SpotLight> spotLightsToRender)
        {
            FastBoundingFrustum fastBoundingFrustum = new FastBoundingFrustum(boundingFrustum);

            for (int i = 0; i < SpotLight.ComponentPool.Count; i++)
            {
                SpotLight component = SpotLight.ComponentPool.Elements[i];
                if (component.Intensity > 0 && component.IsVisible)
                {
                    BoundingSphere boundingSphere = new BoundingSphere(component.cachedPosition, component.Range);
                    if (fastBoundingFrustum.Intersects(ref boundingSphere))
                    //if (boundingFrustum.Intersects(frustumCullingData.boundingSphere)) // It is not thread safe and is slow as hell.
                        spotLightsToRender.Add(component);
                }
            }
        } // FrustumCulling

        #endregion

        #region Fast Bounding Frustum

        /// <summary>
        /// A bounding frustum class that perform Intersects faster.
        /// </summary>
        /// <remarks>
        /// The XNA implementation is not thread safe and is slow as hell.
        /// Bitphase Entertainment makes a far better implementation.
        /// http://xboxforums.create.msdn.com/forums/p/81153/490425.aspx
        /// </remarks>
        public struct FastBoundingFrustum
        {

            #region Variables

            // This variables are catched to improve performance.
            private readonly Vector3 nearNormal, farNormal, leftNormal, rightNormal, bottomNormal, topNormal;
            private readonly float nearD, farD, leftD, rightD, bottomD, topD;

            #endregion

            #region Constructors

            public FastBoundingFrustum(BoundingFrustum source)
            {

                nearNormal = source.Near.Normal; nearD = source.Near.D;
                leftNormal = source.Left.Normal; leftD = source.Left.D;
                rightNormal = source.Right.Normal; rightD = source.Right.D;
                bottomNormal = source.Bottom.Normal; bottomD = source.Bottom.D;
                topNormal = source.Top.Normal; topD = source.Top.D;
                farNormal = source.Far.Normal; farD = source.Far.D;
            } // FastBoundingFrustum
            
            public FastBoundingFrustum(ref Matrix cameraMatrix)
            {

                float x = -cameraMatrix.M14 - cameraMatrix.M11;
                float y = -cameraMatrix.M24 - cameraMatrix.M21;
                float z = -cameraMatrix.M34 - cameraMatrix.M31;
                float scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
                leftNormal = new Vector3(x * scale, y * scale, z * scale);
                leftD = (-cameraMatrix.M44 - cameraMatrix.M41) * scale;

                x = -cameraMatrix.M14 + cameraMatrix.M11;
                y = -cameraMatrix.M24 + cameraMatrix.M21;
                z = -cameraMatrix.M34 + cameraMatrix.M31;
                scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
                rightNormal = new Vector3(x * scale, y * scale, z * scale);
                rightD = (-cameraMatrix.M44 + cameraMatrix.M41) * scale;

                x = -cameraMatrix.M14 + cameraMatrix.M12;
                y = -cameraMatrix.M24 + cameraMatrix.M22;
                z = -cameraMatrix.M34 + cameraMatrix.M32;
                scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
                topNormal = new Vector3(x * scale, y * scale, z * scale);
                topD = (-cameraMatrix.M44 + cameraMatrix.M42) * scale;

                x = -cameraMatrix.M14 - cameraMatrix.M12;
                y = -cameraMatrix.M24 - cameraMatrix.M22;
                z = -cameraMatrix.M34 - cameraMatrix.M32;
                scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
                bottomNormal = new Vector3(x * scale, y * scale, z * scale);
                bottomD = (-cameraMatrix.M44 - cameraMatrix.M42) * scale;

                x = -cameraMatrix.M13;
                y = -cameraMatrix.M23;
                z = -cameraMatrix.M33;
                scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
                nearNormal = new Vector3(x * scale, y * scale, z * scale);
                nearD = (-cameraMatrix.M43) * scale;

                z = -cameraMatrix.M14 + cameraMatrix.M13;
                y = -cameraMatrix.M24 + cameraMatrix.M23;
                z = -cameraMatrix.M34 + cameraMatrix.M33;
                scale = 1.0f / ((float) Math.Sqrt((x * x) + (y * y) + (z * z)));
                farNormal = new Vector3(x * scale, y * scale, z * scale);
                farD      = (-cameraMatrix.M44 + cameraMatrix.M43) * scale;
            } // FastBoundingFrustum

            #endregion

            #region Intersects

            public bool Intersects(ref BoundingSphere sphere)
            {

                Vector3 p = sphere.Center; float radius = sphere.Radius;

                if (nearD + (nearNormal.X * p.X) + (nearNormal.Y * p.Y) + (nearNormal.Z * p.Z) > radius) return false;
                if (leftD + (leftNormal.X * p.X) + (leftNormal.Y * p.Y) + (leftNormal.Z * p.Z) > radius) return false;
                if (rightD + (rightNormal.X * p.X) + (rightNormal.Y * p.Y) + (rightNormal.Z * p.Z) > radius) return false;
                if (bottomD + (bottomNormal.X * p.X) + (bottomNormal.Y * p.Y) + (bottomNormal.Z * p.Z) > radius) return false;
                if (topD + (topNormal.X * p.X) + (topNormal.Y * p.Y) + (topNormal.Z * p.Z) > radius) return false;
                // Can ignore far plane when distant object culling is handled by another mechanism
                if(farD + (farNormal.X * p.X) + (farNormal.Y * p.Y) + (farNormal.Z * p.Z) > radius) return false;

                return true;
            } // Intersects

            public bool Intersects(ref BoundingBox box)
            {
                Vector3 p;

                p.X = (nearNormal.X >= 0 ? box.Min.X : box.Max.X);
                p.Y = (nearNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
                p.Z = (nearNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
                if (nearD + (nearNormal.X * p.X) + (nearNormal.Y * p.Y) + (nearNormal.Z * p.Z) > 0) return false;

                p.X = (leftNormal.X >= 0 ? box.Min.X : box.Max.X);
                p.Y = (leftNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
                p.Z = (leftNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
                if (leftD + (leftNormal.X * p.X) + (leftNormal.Y * p.Y) + (leftNormal.Z * p.Z) > 0) return false;

                p.X = (rightNormal.X >= 0 ? box.Min.X : box.Max.X);
                p.Y = (rightNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
                p.Z = (rightNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
                if (rightD + (rightNormal.X * p.X) + (rightNormal.Y * p.Y) + (rightNormal.Z * p.Z) > 0) return false;

                p.X = (bottomNormal.X >= 0 ? box.Min.X : box.Max.X);
                p.Y = (bottomNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
                p.Z = (bottomNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
                if (bottomD + (bottomNormal.X * p.X) + (bottomNormal.Y * p.Y) + (bottomNormal.Z * p.Z) > 0) return false;

                p.X = (topNormal.X >= 0 ? box.Min.X : box.Max.X);
                p.Y = (topNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
                p.Z = (topNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
                if (topD + (topNormal.X * p.X) + (topNormal.Y * p.Y) + (topNormal.Z * p.Z) > 0) return false;

                // Can ignore far plane when distant object culling is handled by another mechanism
                p.X = (farNormal.X >= 0 ? box.Min.X : box.Max.X);
                p.Y = (farNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
                p.Z = (farNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
                if(farD + (farNormal.X * p.X) + (farNormal.Y * p.Y) + (farNormal.Z * p.Z) > 0) return false;
                
                return true;
            } // Intersects

            #endregion

        } // FastBoundingFrustum

        #endregion

    } // FrustumCulling
} // XNAFinalEngine.Graphics
