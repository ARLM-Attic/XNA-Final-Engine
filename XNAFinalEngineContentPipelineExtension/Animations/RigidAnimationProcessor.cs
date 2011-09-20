
#region License
/*
Copyright (c) 2008-2011, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace XNAFinalEngineContentPipelineExtension.Animations
{
    /// <summary>
    /// Custom processor that extract a rigid and root animation from fbx files.
    /// The hierarchy won't be exported to avoid over complexity.
    /// Of course with this we can implement a system that shares the hierarchy (like Assassins Cred�s animation system).
    /// </summary>
    [ContentProcessor(DisplayName = "Rigid Animation - XNA Final Engine")]
    public class RigidAnimationProcessor : ContentProcessor<NodeContent, AnimationData>
    {

        /// <summary>
        /// The main Process method converts an intermediate format content pipeline NodeContent tree to an animation data format.
        /// </summary>
        public override AnimationData Process(NodeContent input, ContentProcessorContext context)
        {
            RigidModelProcessor rigidModelProcessor = new RigidModelProcessor();
            ModelContent model = rigidModelProcessor.Process(input, context);
            return new AnimationData(((AnimationData)model.Tag).ModelAnimationClips, ((AnimationData)model.Tag).RootAnimationClips, null, null, null);
        } // Process

    } // RigidAnimationProcessor
} // XNAFinalEngineContentPipelineExtension.Animations
