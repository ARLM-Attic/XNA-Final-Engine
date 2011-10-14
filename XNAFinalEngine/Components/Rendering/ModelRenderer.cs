
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Model Renderer.
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public class ModelRenderer : Renderer
    {

        #region Variables

        /// <summary>The bounding sphere of the model in world space.</summary>
        /// <remarks>In the old versions I used nullable types but the bounding volumes are critical (performance wise) when frustum culling is enabled.</remarks>
        protected BoundingSphere boundingSphere;

        /// <summary>The bounding box of the model in world space.</summary>
        /// <remarks>In the old versions I used nullable types but the bounding volumes are critical (performance wise) when frustum culling is enabled.</remarks>
        protected BoundingBox boundingBox;

        // Bone transforms for rigid and animated skinning models.
        internal Matrix[] cachedBoneTransforms;

        // Chaded model filter's model value.
        private Model cachedModel;
        
        #endregion

        #region Properties

        /// <summary>
        /// The material applied to the model.
        /// </summary>
        public Material Material { get; set; }
        
        /// <summary>
        /// The bounding Sphere of the model.
        /// This bounding volume isn’t recalculated, only is translated and scaled accordly to its world matrix.
        /// </summary>
        public BoundingSphere BoundingSphere { get { return boundingSphere; } }
        
        /// <summary>
        /// The bounding Box of the model. Aligned to the X, Y, Z planes.
        /// The axis aligned bounding box is only calculated once with the vertex information.
        /// That means that a rotated axis aligned bounding box could probably enclose very rough the model. 
        /// In that case use bounding spheres or recalculates the bounding box manually, but remember: this operation is extremely costly for real time use.
        /// </summary>
        public BoundingBox BoundingBox { get { return boundingBox; } }

        /// <summary>
        /// Chaded model filter's model value.
        /// </summary>
        internal Model CachedModel
        {
            get { return cachedModel; }
            private set
            {
                cachedModel = value;
                CalculateBoundingVolumes();
            }
        } // CachedModel
        
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            
            // Model
            ((GameObject3D)Owner).ModelFilterChanged += OnModelFilterChanged;
            if (((GameObject3D)Owner).ModelFilter != null)
            {
                ((GameObject3D)Owner).ModelFilter.ModelChanged += OnModelChanged;
                CachedModel = ((GameObject3D)Owner).ModelFilter.Model;
            }
            // Model Animations
            ((GameObject3D)Owner).ModelAnimationChanged += OnModelAnimationChanged;
            if (((GameObject3D)Owner).ModelAnimations != null)
            {
                ((GameObject3D)Owner).ModelAnimations.BoneTransformChanged += OnBoneTransformChanged;
                cachedBoneTransforms = ((GameObject3D)Owner).ModelAnimations.BoneTransform;
            }

            Material = null;
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            base.Uninitialize();
            if (((GameObject3D)Owner).ModelFilter != null)
                ((GameObject3D)Owner).ModelFilter.ModelChanged -= OnModelChanged;
            ((GameObject3D)Owner).ModelFilterChanged -= OnModelFilterChanged;
        } // Uninitialize

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected override void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            base.OnWorldMatrixChanged(worldMatrix);
            CalculateBoundingVolumes();
        } // OnWorldMatrixChanged

        #endregion

        #region On Model Changed

        /// <summary>
        /// On model filter's model changed.
        /// </summary>
        private void OnModelChanged(object sender, Model model)
        {
            CachedModel = model;
        } // OnModelChanged

        #endregion

        #region On Model Filter Changed

        /// <summary>
        /// On model filter changed.
        /// </summary>
        private void OnModelFilterChanged(object sender, Component oldComponent, Component newComponent)
        {
            // Remove event association.
            if (oldComponent != null)
                ((ModelFilter)oldComponent).ModelChanged -= OnModelChanged;
            // Add new event association
            if (newComponent != null)
            {
                ((ModelFilter)newComponent).ModelChanged += OnModelChanged;
                CachedModel = ((GameObject3D)Owner).ModelFilter.Model;
            }
            else
            {
                CachedModel = null;
            }
        } // OnModelFilterChanged

        #endregion

        #region On Bone Transform Changed

        /// <summary>
        /// On model animation's bone transfomr changed.
        /// </summary>
        private void OnBoneTransformChanged(object sender, Matrix[] boneTransform)
        {
            cachedBoneTransforms = boneTransform;
        } // OnBoneTransformChanged

        #endregion

        #region On Model Animation Changed

        /// <summary>
        /// On model animation changed.
        /// </summary>
        private void OnModelAnimationChanged(object sender, Component oldComponent, Component newComponent)
        {
            // Remove event association.
            if (oldComponent != null)
                ((ModelAnimations)oldComponent).BoneTransformChanged -= OnBoneTransformChanged;

            if (newComponent != null)
            {
                ((ModelAnimations)newComponent).BoneTransformChanged += OnBoneTransformChanged;
                cachedBoneTransforms = ((GameObject3D)Owner).ModelAnimations.BoneTransform;
            }
            else
            {
                cachedBoneTransforms = null;
            }

        } // OnModelAnimationChanged

        #endregion

        #region Calculate Bounding Volumes

        /// <summary>
        /// Calculate the game object bounding volumes using the transformation information and the model's bounding volumes.
        /// </summary>
        private void CalculateBoundingVolumes()
        {
            if (((GameObject3D)Owner).ModelFilter.Model == null)
            {
                // If no model asset is present then the bounding volumes will be empty.
                boundingSphere = new BoundingSphere();
                boundingBox = new BoundingBox();
            }
            else
            {
                // Bounding Sphere
                BoundingSphere modelBoundingSphere = ((GameObject3D) Owner).ModelFilter.Model.BoundingSphere;
                float maxScale;
                Vector3 scale = ((GameObject3D) Owner).Transform.Scale;
                // This allows us to support non uniform scaling.
                if (scale.X >= scale.Y && scale.X >= scale.Z)
                {
                    maxScale = scale.X;
                }
                else
                {
                    maxScale = scale.Y >= scale.Z ? scale.Y : scale.Z;
                }
                Vector3 center = Vector3.Transform(modelBoundingSphere.Center, cachedWorldMatrix); // Don't use this: boundingSphere.Value.Center + position;
                float radius = modelBoundingSphere.Radius * maxScale;
                boundingSphere = new BoundingSphere(center, radius);

                // Bounding Box
                BoundingBox modelBoudingBox = ((GameObject3D) Owner).ModelFilter.Model.BoundingBox;
                boundingBox = new BoundingBox(Vector3.Transform(modelBoudingBox.Min, cachedWorldMatrix), Vector3.Transform(modelBoudingBox.Max, cachedWorldMatrix));
            }
        } // CalculateBoundingVolumes

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<ModelRenderer> componentPool = new Pool<ModelRenderer>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<ModelRenderer> ComponentPool { get { return componentPool; } }

        #endregion

    } // ModelRenderer
} // XNAFinalEngine.Components
