
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
#endregion

namespace XNAFinalEngineContentPipelineExtension.Models
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class, adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName = "Model - XNA Final Engine")]
    public class RigidModelProcessor : SimplifiedModelProcessor
    {

        #region Process

        /// <summary> 
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        { 
            ValidateMesh(input, context, null);
                        
            List<int> boneHierarchy = new List<int>();
            Dictionary<string, int> boneIndices = new Dictionary<string, int>();
             
            // Chain to the base ModelProcessor class so it can convert the model data. 
            ModelContent model = base.Process(input, context);           

            // Add each of the bones
            foreach (ModelBoneContent bone in model.Bones)
            {                
                boneHierarchy.Add(model.Bones.IndexOf(bone.Parent));
                boneIndices.Add(bone.Name, boneIndices.Count);
            }
            
            // Animation clips inside the object (mesh)
            Dictionary<string, ModelAnimationClip> modelAnimationClips = new Dictionary<string, ModelAnimationClip>();
            
            // Animation clips at the root of the object
            Dictionary<string, RootAnimationClip> rootAnimationClips = new Dictionary<string, RootAnimationClip>();

            // Process the animations
            ProcessAnimations(input, model, modelAnimationClips, rootAnimationClips);

            // If there is no animation information...
            if (modelAnimationClips.Count == 0 && rootAnimationClips.Count == 0 && model.Bones.Count == 1)
            {
                model.Tag = null;
            }
            else
            {
                // Store the animation data for the model.
                model.Tag = new ModelAnimationData(modelAnimationClips, rootAnimationClips, null, null, boneHierarchy, boneIndices);
            }

            return model;
        } // Process

        #endregion

        #region Process Vertex Channel

        /// <summary>
        /// Processes geometry content vertex channels at the specified index.
        /// </summary>
        protected override void ProcessVertexChannel(GeometryContent geometry, int vertexChannelIndex, ContentProcessorContext context)
        {
            VertexChannelCollection channels = geometry.Vertices.Channels;

            // If the model has only position and normals a UV channel is added.
            // http://xnafinalengine.codeplex.com/wikipage?title=Compressed%20Vertex%20Data
            if (channels.Count == 1 && channels.Contains(VertexChannelNames.Normal()))
            {
                channels.Add<Vector2>(VertexChannelNames.TextureCoordinate(0), null);
            }

            // If the model has position, normal and UV then the data is packed on 32 bytes aliagned vertex data.
            if (channels.Count == 2 && channels.Contains(VertexChannelNames.Normal()) && channels.Contains(VertexChannelNames.TextureCoordinate(0)))
            {
                // No compressed Vertex Data
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);    
            }
            else // If not then the data is compressed.
            {
                string name = channels[vertexChannelIndex].Name;

                if (name == VertexChannelNames.Normal())
                {
                    channels.ConvertChannelContent<NormalizedShort4>(vertexChannelIndex);
                }
                else if (name == VertexChannelNames.TextureCoordinate(0))
                {
                    // If the resource has texture coordinates outside the range [-1, 1] the values will be clamped.
                    channels.ConvertChannelContent<NormalizedShort2>(vertexChannelIndex);
                }
                else if (name == VertexChannelNames.TextureCoordinate(1))
                    channels.Remove(VertexChannelNames.TextureCoordinate(1));
                else if (name == VertexChannelNames.TextureCoordinate(2))
                    channels.Remove(VertexChannelNames.TextureCoordinate(2));
                else if (name == VertexChannelNames.TextureCoordinate(3))
                    channels.Remove(VertexChannelNames.TextureCoordinate(3));
                else if (name == VertexChannelNames.TextureCoordinate(4))
                    channels.Remove(VertexChannelNames.TextureCoordinate(4));
                else if (name == VertexChannelNames.TextureCoordinate(5))
                    channels.Remove(VertexChannelNames.TextureCoordinate(5));
                else if (name == VertexChannelNames.TextureCoordinate(6))
                    channels.Remove(VertexChannelNames.TextureCoordinate(6));
                else if (name == VertexChannelNames.TextureCoordinate(7))
                    channels.Remove(VertexChannelNames.TextureCoordinate(7));
                else if (name == VertexChannelNames.Color(0))
                    channels.Remove(VertexChannelNames.Color(0));
                else if (name == VertexChannelNames.Tangent(0))
                {
                    channels.ConvertChannelContent<NormalizedShort4>(vertexChannelIndex);
                }
                else if (name == VertexChannelNames.Binormal(0))
                {
                    channels.ConvertChannelContent<NormalizedShort4>(vertexChannelIndex);
                    // If the binormal is removed then the position, the normal,
                    // the tangent and one texture coordinate can be fetched in one single block of 32 bytes.
                    // Still, it is more fast to just pass the value. At least on the test I made.
                    //channels.Remove(VertexChannelNames.Binormal(0));
                }
                else
                {
                    base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
                }
            }
        } // ProcessVertexChannel

        #endregion

        #region Process Animations

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary object to our runtime AnimationClip format.
        /// </summary>
        static void ProcessAnimations(NodeContent input, ModelContent model,
                                      Dictionary<string, ModelAnimationClip> modelAnimationClips,
                                      Dictionary<string, RootAnimationClip> rootAnimationClips)
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
                RootAnimationClip processed = ProcessRootAnimation(animation.Value, model.Bones[0].Name);

                rootAnimationClips.Add(animation.Key, processed);
            }

            // Get the unique names of the animations on the mesh children
            List<string> animationNames = new List<string>();
            AddAnimationNodes(animationNames, input);

            // Now create those animations
            foreach (string key in animationNames)
            {
                ModelAnimationClip processed = ProcessRigidAnimation(key, boneMap, input, model);
                
                modelAnimationClips.Add(key, processed);
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
        internal static RootAnimationClip ProcessRootAnimation(AnimationContent animation, string name)
        {
            List<RootKeyframe> keyframes = new List<RootKeyframe>();

            // The root animation is controlling the root of the bones
            AnimationChannel channel = animation.Channels[name];
            
            // Add the transformations on the root of the model
            foreach (AnimationKeyframe keyframe in channel)
            {
                keyframes.Add(new RootKeyframe((float)(keyframe.Time.TotalSeconds), keyframe.Transform));
            }            

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            #region Key Frame Reduction

            // We drop key frame data where the bone transformation is equal to the previous key frame.
            List<RootKeyframe> keyframesReduced = new List<RootKeyframe>();
            keyframesReduced.Add(keyframes[0]);
            for (int i = 1; i < keyframes.Count; i++)
            {
                if (keyframes[i - 1].Transform != keyframes[i].Transform)
                {
                    keyframesReduced.Add(keyframes[i]);
                }
            }
            keyframes = keyframesReduced;
            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            #endregion

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            RootKeyframe[] keyframesArray = new RootKeyframe[keyframes.Count];
            for (int i = 0; i < keyframes.Count; i++)
            {
                keyframesArray[i] = keyframes[i];
            }
            return new RootAnimationClip((float)(animation.Duration.TotalSeconds), keyframesArray);
        } // ProcessRootAnimation

        #endregion

        #region Process Rigid Animation

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent object to our runtime AnimationClip format.
        /// </summary>
        static ModelAnimationClip ProcessRigidAnimation(string animationName, Dictionary<string, int> boneMap, NodeContent input, ModelContent model)
        {
            List<ModelKeyframe> keyframes = new List<ModelKeyframe>();
            TimeSpan duration = TimeSpan.Zero;

            AddTransformationNodes(animationName, boneMap, input, keyframes, ref duration);

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            ModelKeyframe[] keyframesArray = new ModelKeyframe[keyframes.Count];
            for (int i = 0; i < keyframes.Count; i++)
            {
                keyframesArray[i] = keyframes[i];
            }
            return new ModelAnimationClip((float)(duration.TotalSeconds), keyframesArray);
        } // ProcessRigidAnimation

        static void AddTransformationNodes(string animationName, Dictionary<string, int> boneMap, NodeContent input, List<ModelKeyframe> keyframes, ref TimeSpan duration)
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
                        keyframes.Add(new ModelKeyframe((ushort)boneIndex, (float)(keyframe.Time.TotalSeconds), keyframe.Transform));
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
        static int CompareKeyframeTimes(ModelKeyframe a, ModelKeyframe b)
        {
            return a.Time.CompareTo(b.Time);
        } // CompareKeyframeTimes

        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(RootKeyframe a, RootKeyframe b)
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

    } // RigidModelProcessor
} // XNAFinalEngineContentPipelineExtension.Models