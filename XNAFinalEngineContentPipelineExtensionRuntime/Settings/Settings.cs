
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

namespace XNAFinalEngineContentPipelineExtensionRuntime.Settings
{
    /// <summary>
    /// Settings class describes all the tweakable options used to control the engine.
    /// </summary>
    public class MainSettings
    {

        /// <summary>
        /// Window Title.
        /// </summary>
        public string WindowName = "XNA Final Engine";
        
        /// <summary>
        /// Window Width.
        /// Games like Call of Duty Modern Warfare and Oblivion use a resolution of 1024×600
        /// This resolution allow 4 1024x600 32 bits no MSAAed render targets in the EDRAM at the same time.
        /// In other words, this allows us having the three GBuffer's render targets (depth, normal and motion vector/specular power) plus the real depth map in the EDRAM.
        /// However predicate tilling seems to work fast in the test that I made.
        /// </summary>
        public int WindowWidth = 1024;

        /// <summary>
        /// Window Height.
        /// Games like Call of Duty Modern Warfare and Oblivion use a resolution of 1024×600
        /// This resolution allow 4 1024x600 32 bits no MSAAed render targets in the EDRAM at the same time.
        /// In other words, this allows us having the three GBuffer's render targets (depth, normal and motion vector/specular power) plus the real depth map in the EDRAM.
        /// However predicate tilling seems to work fast in the test that I made.
        /// </summary>
        public int WindowHeight = 600;

        /// <summary>
        /// An aspect ratio of 0 means that the aspect ratio will be width/height.
        /// </summary>
        public float AspectRatio = 0;

        /// <summary>
        /// Fullscreen mode enabled?
        /// </summary>
        public bool Fullscreen = false;

        /// <summary>
        /// VSync enabled?
        /// </summary>
        public bool VSync = false;

        /// <summary>
        /// MultiSample Quality. 
        /// It's the level of multisampling, in this case 4 means 4X, and 0 means no multisampling.
        /// Important: Deferred lighting does not work well with MSAA.
        /// </summary>
        public int MultiSampleQuality = 0;

        /// <summary>
        /// Enables the option to resize the application window.
        /// </summary>
        public bool ChangeWindowSize = true;

        /// <summary>
        /// UpdateFrequency allows setting a fixed step updates per second.
        /// If the value is 0 then the engine will try to updates as much as possible.
        /// </summary>
        public int UpdateFrequency = 0;

        /// <summary>
        /// Is Mouse Visible?
        /// </summary>
        public bool IsMouseVisible = true;

    } // MainSettings
} // XNAFinalEngineContentPipelineExtensionRuntime.Settings