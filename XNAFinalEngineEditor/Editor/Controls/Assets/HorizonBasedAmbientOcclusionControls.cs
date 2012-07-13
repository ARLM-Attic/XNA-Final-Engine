
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
using XNAFinalEngine.UserInterface;
#endregion

namespace XNAFinalEngine.Editor
{
    internal static class HorizonBasedAmbientOcclusionControls
    {
        
        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(HorizonBasedAmbientOcclusion asset, Window owner, ComboBox comboBoxResource)
        {
            GroupBox groupGeneral = CommonControls.Group("General", owner);
            
            #region Quality

            var quality = CommonControls.ComboBox("Quality", groupGeneral);
            // Add textures name
            quality.Items.Add("Low");
            quality.Items.Add("Middle");
            quality.Items.Add("High");
            // Events
            quality.ItemIndexChanged += delegate
            {
                switch (quality.ItemIndex)
                {
                    case 0: asset.Quality = HorizonBasedAmbientOcclusion.QualityType.LowQuality; break;
                    case 1: asset.Quality = HorizonBasedAmbientOcclusion.QualityType.MiddleQuality; break;
                    case 2: asset.Quality = HorizonBasedAmbientOcclusion.QualityType.HighQuality; break;
                }
            };
            quality.Draw += delegate
            {
                if (quality.ListBoxVisible)
                    return;
                switch (asset.Quality)
                {
                    case HorizonBasedAmbientOcclusion.QualityType.LowQuality    : quality.ItemIndex = 0; break;
                    case HorizonBasedAmbientOcclusion.QualityType.MiddleQuality : quality.ItemIndex = 1; break;
                    case HorizonBasedAmbientOcclusion.QualityType.HighQuality   : quality.ItemIndex = 2; break;
                }
            };

            #endregion

            var numberSteps = CommonControls.SliderNumericInt("Number Steps", groupGeneral, asset.NumberSteps, false, true, 0, 36, asset, "NumberSteps");
            var numberDirections = CommonControls.SliderNumericInt("Number Directions", groupGeneral, asset.NumberDirections, false, true, 0, 36, asset, "NumberDirections");
            var contrast = CommonControls.SliderNumericFloat("Contrast", groupGeneral, asset.Contrast, true, true, 0, 2, asset, "Contrast");
            var lineAttenuation = CommonControls.SliderNumericFloat("LineAttenuation", groupGeneral, asset.LineAttenuation, false, false, 0, 2, asset, "LineAttenuation");
            var radius = CommonControls.SliderNumericFloat("Radius", groupGeneral, asset.Radius, false, false, 0, 0.5f, asset, "Radius");
            var angleBias = CommonControls.SliderNumericFloat("AngleBias", groupGeneral, asset.AngleBias, false, false, 0, 90, asset, "AngleBias");

            groupGeneral.AdjustHeightFromChildren();

        } // AddControls       

    } // HorizonBasedAmbientOcclusionControls
} // XNAFinalEngine.Editor