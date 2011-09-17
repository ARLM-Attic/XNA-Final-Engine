
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
using XNAFinalEngine.Assets;
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Model Renderer.
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public class ModelRenderer : Renderer
    {

        #region Variables
        
        /// <summary>
        /// Chaded model filter's model value.
        /// </summary>
        private Model cachedModel;

        #endregion

        #region Properties
        /*
        #region Bounding Volumes

        /// <summary>
        /// The bounding Sphere of the model.
        /// The bounding volume is re calculated when the position of the object changes.
        /// A performance drop will be felt if the vertex count is big.
        /// </summary>
        public override BoundingSphere BoundingSphere
        {
            get
            {                
                if (!matrixWhenBoundingSphereWasCalculated.HasValue || matrixWhenBoundingSphereWasCalculated.Value != worlMatrix)
                {
                    matrixWhenBoundingSphereWasCalculated = worlMatrix;
                    boundingSphere = model.BoundingSphere(worlMatrix);
                }
                return boundingSphere;
            }
        } // BoundingSphere

        /// <summary>
        /// The bounding Sphere of the model.
        /// This bounding volume isn’t recalculated, only is translated and scaled accordly to its world matrix.
        /// The result isn’t perfect but is good enough and is far superior in performance.
        /// </summary>
        public override BoundingSphere BoundingSphereOptimized
        {
            get
            {
                return model.BoundingSphereOptimized(WorldMatrix, WorldScale);
            }
        } // BoundingSphereOptimized

        /// <summary>
        /// The bounding Box of the model. Aligned to the X, Y, Z planes.
        /// The bounding volume is re calculated when the position of the object changes.
        /// A performance drop will be felt if the vertex count is big. 
        /// If a bounding box is still needed and the object is in motion then use a dummy object with few polygons than enclose the object.
        /// A bounding box calculation over a simple object is not so costly.
        /// </summary>
        public override BoundingBox BoundingBox
        {
            get
            {
                Matrix worlMatrix = WorldMatrix;
                if (!matrixWhenBoundingBoxWasCalculated.HasValue || matrixWhenBoundingBoxWasCalculated.Value != worlMatrix)
                {
                    matrixWhenBoundingBoxWasCalculated = worlMatrix;
                    boundingBox = model.BoundingBox(worlMatrix);
                }
                return boundingBox;
            }
        } // BoundingBox

        /// <summary>
        /// The bounding Box of the model. Aligned to the X, Y, Z planes.
        /// This bounding box is not recalculated, it’s only transformed.
        /// Of course a big marge of error could exist if the object is rotated and its bounding box is far from a cube.
        /// </summary>
        public override BoundingBox BoundingBoxOptimized
        {
            get
            {
                return model.BoundingBoxOptimized(WorldMatrix);
            }
        } // BoundingBoxOptimized

        #endregion
        */
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // If there is not a ModelFilter component present then we create one.
            if (((GameObject3D) Owner).ModelFilter == null)
            {
                ((GameObject3D) Owner).AddComponent<ModelFilter>();
                ((GameObject3D) Owner).ModelFilter.ModelChanged += OnModelChanged;
            }
        } // Initialize

        #endregion

        #region Disable

        /// <summary>
        /// Disable the component. 
        /// </summary>
        internal override void Disable()
        {
            base.Disable();
            ((GameObject3D)Owner).ModelFilter.ModelChanged -= OnModelChanged;
        } // Disable

        #endregion

        #region On Model Changed

        /// <summary>
        /// On model filter's model changed.
        /// </summary>
        private void OnModelChanged(object sender, Model model)
        {
            cachedModel = model;
        } // OnLayerChanged

        #endregion

    } // MeshRenderer
} // XnaFinalEngine.Components
