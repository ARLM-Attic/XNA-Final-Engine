
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

        /// <summary>
        /// The bounding sphere of the model.
        /// In the old versions I used nullable types but the bounding volumes are critical (performance wise) when frustum culling is enabled.
        /// </summary>
        protected BoundingSphere boundingSphere;

        /// <summary>
        /// The bounding box of the model.
        /// In the old versions I used nullable types but the bounding volumes are critical (performance wise) when frustum culling is enabled.
        /// </summary>
        protected BoundingBox boundingBox;
                
        #endregion

        #region Properties

        /// <summary>
        /// Get the vertices' positions of the model.
        /// </summary>
        public virtual Vector3[] Vectices
        {
            get
            {
                // Overrite it!!
                return null;
            }
        } // Vectices

        /// <summary>
        /// Bounding sphere in local space.
        /// </summary>
        public BoundingSphere BoundingSphere { get { return boundingSphere; } }

        /// <summary>
        /// Axis aligned bounding box in local space.
        /// </summary>
        public BoundingBox BoundingBox { get { return boundingBox; } }

        #endregion

        #region Bounding Volumens
        /*
        /// <summary>
        /// Creates a bounding sphere from this object.
        /// This code only calculates the bounding sphere the first time, then transforms this sphere with the world matrix.
        /// </summary>
        public virtual XNABoundingSphere BoundingSphereOptimized(Matrix worldMatrix, Vector3 scale)
        {
            float maxScale;

            if (!boundingSphere.HasValue)
            {
                boundingSphere = XNABoundingSphere.CreateFromPoints(Vertices(Matrix.Identity));
            }
            // This allows us to support non uniform scaling.
            if (scale.X >= scale.Y && scale.X >= scale.Z)
            {
                maxScale = scale.X;
            }
            else
            {
                if (scale.Y >= scale.Z)
                    maxScale = scale.Y;
                else
                    maxScale = scale.Z;
            }
            Vector3 center = Vector3.Transform(boundingSphere.Value.Center, worldMatrix); // Don't use this: boundingSphere.Value.Center + position;
            float radius = boundingSphere.Value.Radius * maxScale;
            return new XNABoundingSphere(center, radius);
        } // BoundingSphereOptimized

        /// <summary>
        /// Creates a bounding box from this object.
        /// This code only calculates the bounding box the first time, then transforms this box with the world matrix.
        /// </summary>
        public virtual XNABoundingBox BoundingBoxOptimized(Matrix worldMatrix)
        {
            if (!boundingBox.HasValue)
            {
                boundingBox = XNABoundingBox.CreateFromPoints(Vertices(Matrix.Identity));
            }
            return new XNABoundingBox(Vector3.Transform(boundingBox.Value.Min, worldMatrix), Vector3.Transform(boundingBox.Value.Max, worldMatrix));
        } // BoundingBoxOptimized

        /// <summary>
        /// Creates a bounding sphere from this object.
        /// </summary>
        public virtual XNABoundingSphere BoundingSphere(Matrix worldMatrix)
        {
            return XNABoundingSphere.CreateFromPoints(Vertices(worldMatrix));
        } // BoundingSphere

        /// <summary>
        /// Creates a axis-oriented bounding box from this object.
        /// No podemos obtimizar mucho el codigo dado que las bounding box se asumen orientadas con los ejes de coordenadas.
        /// Podriamos hacer una optimizacion por escalado y transformacion, pero no por rotacion. En ese caso conviene usar bounding spheres.
        /// La optimizacion se hace un nivel mas arriba.
        /// </summary>
        public virtual XNABoundingBox BoundingBox(Matrix worldMatrix)
        {
            return XNABoundingBox.CreateFromPoints(Vertices(worldMatrix));
        } // BoundingBox
        */
        #endregion
                
    } // Model
} // XNAFinalEngine.Assets