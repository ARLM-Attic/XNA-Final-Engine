
#region License
/*
Copyright (c) 2008-2013, Schneider, José Ignacio.
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
Author: Schneider, José Ignacio (jischneider@hotmail.com)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
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

        // Syncronization object used in multithreading.
        private static readonly object syncObj = new object();
        
        // The threads that perform the frustum culling over model renderer components.
        private static MultiThreadingTask<FrustumCullingParameters> modelRendererFrustumCullingThreads;

        private static BoundingFrustum[] boundingFrustumList;
        private static List<ModelRenderer>[] modelsToRenderList;

        #endregion

        #region Model Renderer Frustum Culling

        /// <summary>
        /// Perform frustum culling over the model renderer components.
        /// </summary>
        public static void ModelRendererFrustumCulling(BoundingFrustum boundingFrustum, List<ModelRenderer> modelsToRender)
        {
            // If the number of objects is high then the task is devided in threads.
            if (ModelRenderer.ComponentPool.Count > 50)
            {
                // If it is the first execution then the threads are created.
                if (modelRendererFrustumCullingThreads == null)
                {
                    boundingFrustumList = new BoundingFrustum[ProcessorsInformation.AvailableProcessors];
                    modelsToRenderList = new List<ModelRenderer>[ProcessorsInformation.AvailableProcessors];
                    for (int i = 0; i < ProcessorsInformation.AvailableProcessors; i++)
                    {
                        boundingFrustumList[i] = new BoundingFrustum(Matrix.Identity);
                        modelsToRenderList[i] = new List<ModelRenderer>(50);
                    }
                    modelRendererFrustumCullingThreads = new MultiThreadingTask<FrustumCullingParameters>(ModelRendererFrustumCullingTask, ProcessorsInformation.AvailableProcessors);
                }
                int objectsProcessedByThread = (int)(ModelRenderer.ComponentPool.Count / (ProcessorsInformation.AvailableProcessors + 1.7f)); // The application thread makes double the work.
                for (int i = 0; i < ProcessorsInformation.AvailableProcessors; i++)
                {
                    boundingFrustumList[i].Matrix = boundingFrustum.Matrix;
                    modelRendererFrustumCullingThreads.Start(i, new FrustumCullingParameters(boundingFrustumList[i], modelsToRenderList[i], i * objectsProcessedByThread, objectsProcessedByThread));
                }
                ModelRendererFrustumCullingTask(new FrustumCullingParameters(boundingFrustum, modelsToRender,
                                                                             ProcessorsInformation.AvailableProcessors * objectsProcessedByThread,
                                                                             ModelRenderer.ComponentPool.Count - (ProcessorsInformation.AvailableProcessors * objectsProcessedByThread)));
                modelRendererFrustumCullingThreads.WaitForTaskCompletition();
                for (int i = 0; i < ProcessorsInformation.AvailableProcessors; i++)
                {
                    foreach (var modelR in modelsToRenderList[i])
                    {
                        modelsToRender.Add(modelR);
                    }
                    //modelsToRender.AddRange(modelsToRenderList[i]);
                    modelsToRenderList[i].Clear();
                }
            }
            else
                ModelRendererFrustumCullingTask(new FrustumCullingParameters(boundingFrustum, modelsToRender, 0, ModelRenderer.FrustumCullingDataPool.Count));
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

            // I improved memory locality with FrustumCullingDataPool.
            // However, in order to do that I had to make the code a little more complicated and error prone.
            // There is a performance gain, but it is small. 
            // I recommend to perform this kind of optimizations is the game/application has a CPU bottleneck.
            // Besides, the old code was half data oriented.
            for (int i = startPosition; i < (count + startPosition); i++)
            {
                ModelRenderer.FrustumCullingData frustumCullingData = ModelRenderer.FrustumCullingDataPool.Elements[i];
                if (//component.CachedModel != null && // Not need to waste cycles in this, how many ModelRenderer components will not have a model?
                    // Is Visible?
                    Layer.IsVisible(frustumCullingData.layerMask) && frustumCullingData.ownerActive && frustumCullingData.enabled)
                {
                    if (boundingFrustum.Intersects(frustumCullingData.boundingSphere))
                    {
                        //lock (syncObj)
                        {
                            modelsToRender.Add(frustumCullingData.component);
                        }
                    }
                }
            }
        } // ModelRendererFrustumCullingTask

        #endregion

    } // FrustumCulling
} // XNAFinalEngine.Graphics
