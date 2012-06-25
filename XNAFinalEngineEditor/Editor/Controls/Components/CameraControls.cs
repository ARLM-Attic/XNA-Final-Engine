﻿
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.UserInterface;
#endregion

namespace XNAFinalEngine.Editor
{
    public static class CameraControls
    {

        /// <summary>
        /// Creates the configuration controls of this component.
        /// </summary>
        public static void AddControls(Camera camera, ClipControl owner)
        {
            // Enabled
            var enabled = CommonControls.CheckBox("Enabled", owner, camera.Enabled, camera, "Enabled");
            enabled.Top = 10;
            // Clear Color
            var clearColor = CommonControls.SliderColor("Clear Color", owner, camera.ClearColor, camera, "ClearColor");
            var sky = CommonControls.AssetSelector<Sky>("Sky", owner, camera, "Sky");
            var postProcess = CommonControls.AssetSelector<PostProcess>("Post Process", owner, camera, "PostProcess");
            var ambientLight = CommonControls.AssetSelector<AmbientLight>("Ambient Light", owner, camera, "AmbientLight");
            
            enabled.CheckedChanged += delegate
            {
                clearColor.Enabled = enabled.Checked;
                sky.Enabled = enabled.Checked;
                postProcess.Enabled = enabled.Checked;
                ambientLight.Enabled = enabled.Checked;
            };
        } // AddControls
        
    } // CameraControls
} // XNAFinalEngine.Editor