
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
    internal static class PointLightControls
    {
        
        /// <summary>
        /// Creates the configuration controls of this component.
        /// </summary>
        public static void AddControls(PointLight pointLight, ClipControl owner)
        {
            // Enabled
            CheckBox enabled = CommonControls.CheckBox("Enabled", owner, pointLight.Enabled, pointLight, "Enabled");
            enabled.Top = 10;
            // Intensity
            var intensity = CommonControls.SliderNumericFloat("Intensity", owner, pointLight.Intensity, false, true, 0, 100, pointLight, "Intensity");
            // Diffuse Color
            var diffuseColor = CommonControls.SliderColor("Color", owner, pointLight.Color, pointLight, "Color");
            // Range
            var range = CommonControls.SliderNumericFloat("Range", owner, pointLight.Range, false, true, 0, 500, pointLight, "Range");
            // Enabled
            enabled.CheckedChanged += delegate
            {
                intensity.Enabled      = pointLight.Enabled;
                diffuseColor.Enabled   = pointLight.Enabled;
                range.Enabled          = pointLight.Enabled;
            };
            owner.AdjustHeightFromChildren();
        } // AddControls

    } // PointLightControls
} // XNAFinalEngine.Editor