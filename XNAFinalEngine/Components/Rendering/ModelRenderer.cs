
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

        #region Constants

        /// <summary>
        /// Maximum Number of Mesh Materials.
        /// </summary>
        public const int MaximumNumberMeshMaterials = 16;

        #endregion

        #region Variables

        private readonly Material[] meshMaterial = new Material[MaximumNumberMeshMaterials];

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
        /// <remarks>
        /// It is possible to use different materials for the different model’s mesh using the MeshMaterial property.
        /// If the mesh material is null the material from them Material property will be used.
        /// </remarks>
        public Material Material { get; set; }
        
        /// <summary>
        /// Allows to use different materials for the different model’s mesh.
        /// If the mesh material is null the material from them Material property will be used.
        /// </summary>
        public Material[] MeshMaterial
        {
            get { return meshMaterial; }
        } // MeshMaterial

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

        /// <summary>
        /// Render bounding box using lines.
        /// This alternative uses the asset bounding box and transforms the points using the transformation matrix.
        /// </summary>
        public bool RenderNonAxisAlignedBoundingBox { get; set; }

        /// <summary>
        /// Render bounding box using lines.
        /// This alternative uses the renderer component bounding box.
        /// </summary>
        public bool RenderAxisAlignedBoundingBox { get; set; }

        /// <summary>
        /// Render bounding sphere using lines.
        /// </summary>
        public bool RenderBoundingSphere { get; set; }
        
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
            Material = null;
            cachedBoneTransforms = null;
            cachedModel = null;
            for (int i = 0; i < MeshMaterial.Length; i++)
            {
                MeshMaterial[i] = null;
            }
            ((GameObject3D)Owner).ModelFilterChanged -= OnModelFilterChanged;
            if (((GameObject3D)Owner).ModelFilter != null)
                ((GameObject3D)Owner).ModelFilter.ModelChanged -= OnModelChanged;
            ((GameObject3D)Owner).ModelAnimationChanged -= OnModelAnimationChanged;
            if (((GameObject3D)Owner).ModelAnimations != null)
                ((GameObject3D)Owner).ModelAnimations.BoneTransformChanged -= OnBoneTransformChanged;
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
            // If it is a skinned model then the skin transform should be updated.
            // Why do it here? Because it should be do it only one time for frame and because the animation component has nothing to do with the model. 
            // The skinning transforms information is only useful for rendering.
            // Also, normally there are few skinned models in a scene and I don’t discriminate them like, for example, Unity does.
            // So it’s difficult in this scheme to update the skinning data using a high level task and producing
            // at the same time few cache misses (the majority of the model are not skinned)
            // However, it’s possible that some skinned models will be updated that will be frustum culled
            // but I doubt that too much animations will be performed on off-screen models.
            // The Unity scheme could be faked using two pools in the ModelFilter component, one for common models and one for skinned models. This is a matter of choice.
            if (cachedModel != null && cachedModel is FileModel && ((FileModel)cachedModel).IsSkinned)
            {
                ((FileModel)cachedModel).UpdateWorldTransforms(boneTransform, boneTransform);
                ((FileModel)cachedModel).UpdateSkinTransforms(boneTransform, boneTransform);
                cachedBoneTransforms = boneTransform;
            }
            else // It is rigid animated.
            {
                cachedBoneTransforms = boneTransform;
            }
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
            if (((GameObject3D)Owner).ModelFilter == null || ((GameObject3D)Owner).ModelFilter.Model == null)
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
                Vector3 center = Vector3.Transform(modelBoundingSphere.Center, CachedWorldMatrix); // Don't use this: boundingSphere.Value.Center + position;
                float radius = modelBoundingSphere.Radius * maxScale;
                boundingSphere = new BoundingSphere(center, radius);

                // Bounding Box
                BoundingBox modelBoudingBox = ((GameObject3D) Owner).ModelFilter.Model.BoundingBox;
                boundingBox = new BoundingBox(Vector3.Transform(modelBoudingBox.Min, CachedWorldMatrix), Vector3.Transform(modelBoudingBox.Max, CachedWorldMatrix));
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
