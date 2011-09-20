
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

using System.Collections.Generic;
using XNAFinalEngineContentPipelineExtension.Models;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace XNAFinalEngineContentPipelineExtension.Animations
{
    /// <summary>
    /// Custom processor that extract a rigid animation from fbx files.
    /// The hierarchy won't be exported to avoid over complexity.
    /// Of course with this we can implement a system that shares the hierarchy (like Assassins Cred’s animation system).
    /// </summary>
    [ContentProcessor(DisplayName = "Animation Rigid - XNA Final Engine")]
    public class RigidAnimationProcessor : ContentProcessor<NodeContent, ModelAnimationClip>
    {

        /// <summary>The animation name to be retrieved.</summary>
        /// <remarks>If the name is null or empty the first animation will be processed.</remarks>
        public string AnimationName { get; set; }

        /// <summary>
        /// The main Process method converts an intermediate format content pipeline NodeContent tree to an animation data format.
        /// </summary>
        public override ModelAnimationClip Process(NodeContent input, ContentProcessorContext context)
        {
            RigidModelProcessor rigidModelProcessor = new RigidModelProcessor();
            ModelContent model = rigidModelProcessor.Process(input, context);

            if (string.IsNullOrEmpty(AnimationName)) // If no name was set then take the first animation
            {
                foreach (var animation in ((ModelAnimationData)model.Tag).ModelAnimationClips)
                {
                    return animation.Value;
                }
                throw new InvalidContentException("There is no rigid animation present in this model.");
            }
            if (((ModelAnimationData)model.Tag).ModelAnimationClips.ContainsKey(AnimationName))
                return ((ModelAnimationData)model.Tag).ModelAnimationClips[AnimationName];
            throw new InvalidContentException("There is no rigid animation present with this name.");
        } // Process

    } // RigidAnimationProcessor
} // XNAFinalEngineContentPipelineExtension.Animations
