
#region License
//-----------------------------------------------------------------------------
// AnimatedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using XNAFinalEngine.Animations;
#endregion

namespace XNAFinalEngineContentPipelineExtension.Animations
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class, adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName = "Animated Model - XNA Final Engine")]
    public class AnimatedModelProcessor : ModelProcessor
    {

        #region Process

        /// <summary> 
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelConte nt object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        { 
            ValidateMesh(input, context, null);
                        
            List<int> boneHierarchy = new List<int>();
             
            // Chain to the base ModelProcessor class so it can convert the model data. 
            ModelContent model = base.Process(input, context);           

            // Add each of the bones
            foreach (ModelBoneContent bone in model.Bones)
            {                
                boneHierarchy.Add(model.Bones.IndexOf(bone.Parent));
            }
            
            // Animation clips inside the object (mesh)
            Dictionary<string, AnimationClip> animationClips = new Dictionary<string, AnimationClip>();
            
            // Animation clips at the root of the object
            Dictionary<string, AnimationClip> rootClips = new Dictionary<string, AnimationClip>();

            // Process the animations
            ProcessAnimations(input, model, animationClips, rootClips);
                        
            // Store the data for the model
            model.Tag = new ModelAnimationData(animationClips, rootClips, null, null, boneHierarchy);

            return model;
        } // Process

        #endregion

        #region Process Animations

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary object to our runtime AnimationClip format.
        /// </summary>
        static void ProcessAnimations(NodeContent input, ModelContent model, 
                                      Dictionary<string, AnimationClip> animationClips,
                                      Dictionary<string, AnimationClip> rootClips)
        {            
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();
            for (int i = 0; i < model.Bones.Count; i++)
            {
                string boneName = model.Bones[i].Name;
            
                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in the root of the object            
            foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
            {
                AnimationClip processed = ProcessRootAnimation(animation.Value, model.Bones[0].Name);

                rootClips.Add(animation.Key, processed);
            }

            // Get the unique names of the animations on the mesh children
            List<string> animationNames = new List<string>();
            AddAnimationNodes(animationNames, input);

            // Now create those animations
            foreach (string key in animationNames)
            {
                AnimationClip processed = ProcessRigidAnimation(key, boneMap, input, model);
                
                animationClips.Add(key, processed);
            }
        } // ProcessAnimations

        static void AddAnimationNodes(List<string> animationNames, NodeContent node)
        {            
            foreach (NodeContent childNode in node.Children)
            {
                // If this node doesn't have keyframes for this animation we should just skip it
                foreach (string key in childNode.Animations.Keys)
                {
                    if (!animationNames.Contains(key))
                        animationNames.Add(key);
                }

                AddAnimationNodes(animationNames, childNode);
            }
        } // AddAnimationNodes

        #endregion

        #region Process Root Animation

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        internal static AnimationClip ProcessRootAnimation(AnimationContent animation, string name)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            // The root animation is controlling the root of the bones
            AnimationChannel channel = animation.Channels[name];
            
            // Add the transformations on the root of the model
            foreach (AnimationKeyframe keyframe in channel)
            {
                keyframes.Add(new Keyframe(0, keyframe.Time, keyframe.Transform));
            }            

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new AnimationClip(animation.Duration, keyframes);
        } // ProcessRootAnimation

        #endregion

        #region Process Rigid Animation

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent object to our runtime AnimationClip format.
        /// </summary>
        static AnimationClip ProcessRigidAnimation(string animationName, Dictionary<string, int> boneMap, NodeContent input, ModelContent model)
        {
            List<Keyframe> keyframes = new List<Keyframe>();
            TimeSpan duration = TimeSpan.Zero;

            AddTransformationNodes(animationName, boneMap, input, keyframes, ref duration);

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new AnimationClip(duration, keyframes);
        } // ProcessRigidAnimation

        static void AddTransformationNodes(string animationName, Dictionary<string, int> boneMap, NodeContent input, List<Keyframe> keyframes, ref TimeSpan duration)
        {
            // Add the transformation on each of the meshes
            foreach (NodeContent childNode in input.Children)
            {
                // If this node doesn't have keyframes for this animation we should just skip it
                if (childNode.Animations.ContainsKey(animationName))
                {
                    AnimationChannel childChannel = childNode.Animations[animationName].Channels[childNode.Name];
                    if (childNode.Animations[animationName].Duration != duration)
                    {
                        if (duration < childNode.Animations[animationName].Duration)
                            duration = childNode.Animations[animationName].Duration;
                    }

                    int boneIndex;
                    if (!boneMap.TryGetValue(childNode.Name, out boneIndex))
                    {
                        throw new InvalidContentException(string.Format("Found animation for bone '{0}', which is not part of the model.", childNode.Name));
                    }

                    foreach (AnimationKeyframe keyframe in childChannel)
                    {
                        keyframes.Add(new Keyframe(boneIndex, keyframe.Time, keyframe.Transform));
                    }
                }

                AddTransformationNodes(animationName, boneMap, childNode, keyframes, ref duration);
            }
        } // AddTransformationNodes

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
                                              "Mesh {0} is a child of bone {1}. AnimatedModelProcessor does not correctly handle meshes that are children of bones.",
                                              mesh.Name, parentBoneName);
                }               
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        } // ValidateMesh

        #endregion

        #region Flatten Transforms

        /// <summary>
        /// Bakes unwanted transforms into the model geometry, so everything ends up in the same coordinate system.
        /// </summary>
        /*static void FlattenTransforms(NodeContent node, BoneContent skeleton)
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
        }*/

        #endregion

    } // AnimatedModelProcessor
} // XNAFinalEngineContentPipelineExtension.Animations