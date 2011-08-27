
#region Using directives
using Microsoft.Xna.Framework;
#endregion

namespace XnaFinalEngine.Components
{

    /// <summary>
    /// Mesh Renderer.
    /// Render Meshs
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public abstract class MeshRenderer : Renderer
    {

        #region Variables
        /*
        /// <summary>
        /// 
        /// </summary>
        private Matrix cachedWorldMatrix;

        /// <summary>
        /// 
        /// </summary>
        private Model cachedModel;

        #endregion

        #region Properties

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

    } // MeshRenderer
} // XnaFinalEngine.Components
