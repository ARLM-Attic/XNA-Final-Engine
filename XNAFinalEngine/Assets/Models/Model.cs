
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using XNABoundingBox    = Microsoft.Xna.Framework.BoundingBox;
using XNABoundingSphere = Microsoft.Xna.Framework.BoundingSphere;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Base class that represents mesh assets.
    /// </summary>
    public abstract class Model : Asset
    {

        #region Variables

        /// <summary>The bounding sphere of the model in local space.</summary>
        /// <remarks>In the old versions I used nullable types but the bounding volumes are critical (performance wise) when frustum culling is enabled.</remarks>
        protected BoundingSphere boundingSphere;

        /// <summary>The bounding box of the model in local space.</summary>
        /// <remarks>In the old versions I used nullable types but the bounding volumes are critical (performance wise) when frustum culling is enabled.</remarks>
        protected BoundingBox boundingBox;
                
        #endregion

        #region Properties

        /// <summary>
        /// Get the vertices' positions of the model.
        /// </summary>
        public abstract Vector3[] Vectices { get; }

        /// <summary>
        /// Bounding sphere in local space.
        /// </summary>
        public BoundingSphere BoundingSphere { get { return boundingSphere; } }

        /// <summary>
        /// Axis aligned bounding box in local space.
        /// </summary>
        public BoundingBox BoundingBox { get { return boundingBox; } }

        /// <summary>
        /// The number of meshes of the model.
        /// Each mesh has its transformation.
        /// </summary>
        public int MeshesCount { get; protected set; }

        /// <summary>
        /// The number of mesh parts of the model.
        /// Each part has its own material.
        /// </summary>
        public int MeshPartsCount { get; protected set; }

        #endregion

        #region Constructor

        protected Model()
        {
            MeshesCount = 1;
            MeshPartsCount = 1;
        } // Model

        #endregion

        #region Render

        /// <summary>
        /// Render the model.
        /// </summary>
        /// <remarks>
        /// Don't call it excepting see the model on the screen.
        /// This is public to allow doing some specific tasks not implemented in the engine.
        /// </remarks>
        public abstract void Render();

        /// <summary>
        /// Render a mesh of the model.
        /// </summary>
        /// <remarks>
        /// Don't call it excepting see the model on the screen.
        /// This is public to allow doing some specific tasks not implemented in the engine.
        /// </remarks>
        public abstract void RenderMeshPart(int meshIndex, int meshPart);

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Recreate textures created without using a content manager.
        /// </summary>
        internal static void RecreateModelsWithoutContentManager()
        {
            foreach (Asset loadedModel in LoadedAssets)
            {
                if (loadedModel is Model && loadedModel.ContentManager == null && loadedModel is PrimitiveModel)
                {
                    loadedModel.RecreateResource();
                }
            }
        } // RecreateResource

        #endregion

    } // Model
} // XNAFinalEngine.Assets