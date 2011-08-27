
#region License
//-----------------------------------------------------------------------------
// AnimatedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using XNAFinalEngine.Animations;
#endregion

namespace XNAFinalEngineContentPipelineExtension.Animations
{
    /// <summary>
    /// Custom processor that extract a root animation from fbx files.
    /// </summary>
    [ContentProcessor(DisplayName = "Animation - XNA Final Engine")]
    public class AnimationProcessor : ContentProcessor<NodeContent, AnimationClip>
    {

        #region Process

        /// <summary> 
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override AnimationClip Process(NodeContent input, ContentProcessorContext context) 
        { 
            ModelProcessor modelProcessor = new ModelProcessor();
            // Chain to the base ModelProcessor class so it can convert the model data. 
            ModelContent model = modelProcessor.Process(input, context);

            return ProcessAnimations(input, model);
        } // Process

        #endregion

        #region Process Animations

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary object to our runtime AnimationClip format.
        /// </summary>
        static AnimationClip ProcessAnimations(NodeContent input, ModelContent model)
        {            
            // Convert the first animation in the root of the object
            foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
            {
                AnimationClip processed = AnimatedModelProcessor.ProcessRootAnimation(animation.Value, model.Bones[0].Name);                
                return processed;
            }
            throw new InvalidContentException("Input file does not contain any animations.");
        } // ProcessAnimations

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

    } // AnimationProcessor
} // XNAFinalEngineContentPipelineExtension.Animations