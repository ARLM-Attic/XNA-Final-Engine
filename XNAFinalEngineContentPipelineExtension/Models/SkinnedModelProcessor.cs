
#region License
//-----------------------------------------------------------------------------
// SkinnedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace XNAFinalEngineContentPipelineExtension.Models
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class, adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName = "Model Skinned - XNA Final Engine")]
    public class SkinnedModelProcessor : IgnoreTexturesModelProcessor
    {

        #region Process

        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ValidateMesh(input, context, null);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                throw new InvalidContentException("Input skeleton not found.");

            // We don't want to have to worry about different parts of the model being in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            if (bones.Count > ModelAnimationClip.MaxBones)
            {
                throw new InvalidContentException(string.Format("Skeleton has {0} bones, but the maximum supported is {1}.", bones.Count, ModelAnimationClip.MaxBones));
            }

            List<Matrix> bindPose        = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy  = new List<int>();
             
            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            // Convert animation data to our runtime format.
            Dictionary<string, ModelAnimationClip> animationClips = ProcessAnimations(skeleton.Animations, bones);

            Dictionary<string, RootAnimationClip> rootClips = new Dictionary<string, RootAnimationClip>();
            
            // Chain to the base ModelProcessor class so it can convert the model data.
            ModelContent model = base.Process(input, context);

            // Convert each animation in the root of the object            
            foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
            {
                RootAnimationClip processed = RigidModelProcessor.ProcessRootAnimation(animation.Value, model.Bones[0].Name);

                rootClips.Add(animation.Key, processed);
            }            
 
            // Store our custom animation data in the Tag property of the model.
            model.Tag = new ModelAnimationData(animationClips, rootClips, bindPose, inverseBindPose, skeletonHierarchy);

            return model;
        } // Process

        #endregion

        #region Process Vertex Channel

        protected override void ProcessVertexChannel(GeometryContent geometry, int vertexChannelIndex, ContentProcessorContext context)
        {
            bool isWeights = geometry.Vertices.Channels[vertexChannelIndex].Name == VertexChannelNames.Weights();

            base.ProcessVertexChannel(geometry, vertexChannelIndex, context);

            if (isWeights)
            {
                geometry.Vertices.Channels.ConvertChannelContent<Vector4>("BlendIndices0");
                geometry.Vertices.Channels.ConvertChannelContent<Vector4>("BlendWeight0");
            }
        } // ProcessVertexChannel

        #endregion

        #region Process Animations

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Dictionary<string, ModelAnimationClip> ProcessAnimations(AnimationContentDictionary animations, IList<BoneContent> bones)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, ModelAnimationClip> animationClips;
            animationClips = new Dictionary<string, ModelAnimationClip>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                ModelAnimationClip processed = ProcessAnimation(animation.Value, boneMap);
                
                animationClips.Add(animation.Key, processed);
            }

            if (animationClips.Count == 0)
            {
                throw new InvalidContentException("Input file does not contain any animations.");
            }

            return animationClips;
        } // ProcessAnimations

        #endregion

        #region Process Animation

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent object to our runtime AnimationClip format.
        /// </summary>
        static ModelAnimationClip ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneMap)
        {
            List<ModelKeyframe> keyframes = new List<ModelKeyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in
                animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                 if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    throw new InvalidContentException(string.Format("Found animation for bone '{0}', which is not part of the skeleton.", channel.Key));
                }

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframes.Add(new ModelKeyframe(boneIndex, (float)(keyframe.Time.TotalSeconds), keyframe.Transform));
                }
            }

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new ModelAnimationClip((float)(animation.Duration.TotalSeconds), keyframes);
        } // ProcessAnimation

        #endregion

        #region Compare Keyframe Times

        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(Keyframe a, Keyframe b)
        {
            return a.Time.CompareTo(b.Time);
        } // CompareKeyframeTimes

        #endregion

        #region Validate Mesh

        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context, string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                                              "Mesh {0} is a child of bone {1}. SkinnedModelProcessor does not correctly handle meshes that are children of bones.",
                                              mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null, "Mesh {0} has no skinning information, so it has been deleted.", mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection, because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        } // ValidateMesh

        #endregion

        #region Mesh Has Skinning

        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        } // MeshHasSkinning

        #endregion

        #region Flatten Transforms

        /// <summary>
        /// Bakes unwanted transforms into the model geometry, so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        } // FlattenTransforms

        #endregion

    } // SkinnedModelProcessor
} // XNAFinalEngineContentPipelineExtension.Models
