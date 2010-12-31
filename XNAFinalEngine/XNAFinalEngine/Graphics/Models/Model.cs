
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.Helpers;
using XNABoundingBox = Microsoft.Xna.Framework.BoundingBox;
using XNABoundingSphere = Microsoft.Xna.Framework.BoundingSphere;
#endregion

namespace XNAFinalEngine.Graphics
{

    /// <summary>
    /// Base class to represent the geometrical information of objects.
    /// </summary>
    public abstract class Model
    {

        #region Variables
                
        /// <summary>
        /// The bounding Sphere of the model.
        /// </summary>
        private BoundingSphere? boundingSphere;
                
        #endregion

        #region Properties

        /// <summary>
        /// The name of the model file.
        /// </summary>
        public string ModelFilename { get; protected set; }

        /// <summary>
        /// Get the vertices' positions of the model.
        /// </summary>
        protected virtual List<Vector3> Vertices(Matrix _worldMatrix)
        {
            // Overrite it!!
            return null;
        } // Vertices

        #endregion

        #region Bounding Volumens

        /// <summary>
        /// Creates a bounding sphere from this object.
        /// This code only calculates the bounding sphere the first time, then transforms this sphere with the world matrix.
        /// </summary>
        public virtual XNABoundingSphere BoundingSphereOptimized(Matrix worldMatrix)
        {

            #region Variables
            Quaternion quaternion;
            Vector3 scale;
            Vector3 position;
            float maxScale;
            #endregion

            if (!boundingSphere.HasValue)
            {
                boundingSphere = XNABoundingSphere.CreateFromPoints(Vertices(Matrix.Identity));
            }

            worldMatrix.Decompose(out scale, out quaternion, out position);
            // Obtengo cual es la compnenete de scale mas grande
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
            // XNA Bug. At least in the version 2.
            Vector3 center = Vector3.Transform(boundingSphere.Value.Center, worldMatrix);
            float radius = boundingSphere.Value.Radius * maxScale;
            return new XNABoundingSphere(center, radius);

        } // BoundingSphereOptimized

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

        #endregion
        
        #region Render

        /// <summary>
        /// Render the model
        /// </summary>
        public virtual void Render()
        {
            // Overrite it!
        } // Render
        
        #endregion
                
    } // Model
} // XNAFinalEngine.Graphics
