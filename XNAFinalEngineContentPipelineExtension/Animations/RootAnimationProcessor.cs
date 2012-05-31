
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
using System.Collections.Generic;
using System.ComponentModel;
using XNAFinalEngineContentPipelineExtension.Models;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace XNAFinalEngineContentPipelineExtension.Animations
{
    /// <summary>
    /// Custom processor that extract a root animation from fbx files.
    /// </summary>
    [ContentProcessor(DisplayName = "Animation Root - XNA Final Engine")]
    public class RootAnimationProcessor : ContentProcessor<NodeContent, RootAnimationClip>
    {

        /// <summary>The animation name to be retrieved.</summary>
        /// <remarks>If the name is null or empty the first animation will be processed.</remarks>
        [Description("The animation name to be retrieved. If the name is null or empty the first animation will be processed.")]
        public string AnimationName { get; set; }

        /// <summary>
        /// The main Process method converts an intermediate format content pipeline NodeContent tree to an animation data format.
        /// </summary>
        public override RootAnimationClip Process(NodeContent input, ContentProcessorContext context)
        {
            SimplifiedModelProcessor modelProcessor = new SimplifiedModelProcessor();
            ModelContent model = modelProcessor.Process(input, context);

            Dictionary<string, RootAnimationClip> rootClips = new Dictionary<string, RootAnimationClip>();
            foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
            {
                RootAnimationClip processed = RigidModelProcessor.ProcessRootAnimation(animation.Value, model.Bones[0].Name);
                rootClips.Add(animation.Key, processed);
            }

            if (string.IsNullOrEmpty(AnimationName)) // If no name was set then take the first animation
            {
                foreach (var animation in rootClips)
                {
                    return animation.Value;
                }
                throw new InvalidContentException("There is no root animation present in this model.");
            }
            if (rootClips.ContainsKey(AnimationName))
                return rootClips[AnimationName];
            throw new InvalidContentException("There is no root animation present with this name.");
        } // Process

    } // RootAnimationProcessor
} // XNAFinalEngineContentPipelineExtension.Animations
