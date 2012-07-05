
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
    public static class BlinnPhongControls
    {

        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(BlinnPhong asset, Window owner)
        {

            #region Diffuse

            GroupBox groupDiffuse = CommonControls.Group("Diffuse", owner);
            // Diffuse Color
            var sliderDiffuseColor = CommonControls.SliderColor("Diffuse Color", groupDiffuse, asset.DiffuseColor, asset, "DiffuseColor");
            // Diffuse Texture
            var assetSelectorDiffuseTexture = CommonControls.AssetSelector<Texture>("Diffuse Texture", groupDiffuse, asset, "DiffuseTexture");
            groupDiffuse.AdjustHeightFromChildren();

            #endregion

            #region Specular

            GroupBox groupSpecular = CommonControls.Group("Specular", owner);
            CheckBox checkBoxSpecularPowerFromTexture = null;
            // Specular Intensity
            var sliderSpecularIntensity = CommonControls.SliderNumericFloat("Specular Intensity", groupSpecular, asset.SpecularIntensity, false, true, 0, 2, asset, "SpecularIntensity");
            // Specular Power
            var sliderSpecularPower = CommonControls.SliderNumericFloat("Specular Power", groupSpecular, asset.SpecularPower, true, true, 0, 100, asset, "SpecularPower");
            sliderSpecularPower.Draw += delegate
            {
                sliderSpecularPower.Enabled = !checkBoxSpecularPowerFromTexture.Enabled || (checkBoxSpecularPowerFromTexture.Enabled && !checkBoxSpecularPowerFromTexture.Checked);
            };
            // Specular Texture
            var assetSelectorSpecularTexture = CommonControls.AssetSelector<Texture>("Specular Texture", groupSpecular, asset, "SpecularTexture");
            assetSelectorSpecularTexture.ItemIndexChanged += delegate { checkBoxSpecularPowerFromTexture.Enabled = asset.SpecularTexture != null; };
            // Specular Texture Power Enabled
            checkBoxSpecularPowerFromTexture = CommonControls.CheckBox("Use specular power from Texture", groupSpecular, asset.SpecularPowerFromTexture, 
                asset, "SpecularPowerFromTexture",
                "Indicates if the specular power will be read from the texture (the alpha channel of the specular texture) or from the specular power property.");
            // Reflection Texture
            var assetSelectorReflectionTexture = CommonControls.AssetSelector<TextureCube>("Reflection Texture", groupSpecular, asset, "ReflectionTexture");
            groupSpecular.AdjustHeightFromChildren();

            #endregion

            #region Normals

            GroupBox groupNormals = CommonControls.Group("Normals", owner);
            // Normal Texture
            var assetSelectorNormalTexture = CommonControls.AssetSelector<Texture>("Normal Texture", groupNormals, asset, "NormalTexture");
            // Parallax Enabled
            CheckBox checkBoxParallaxEnabled = CommonControls.CheckBox("Enabled", groupNormals, asset.ParallaxEnabled, asset, "ParallaxEnabled");
            // Parallax Threshold
            var sliderParallaxLodThreshold = CommonControls.SliderNumericInt("Parallax LOD Threshold", groupNormals,
                                             asset.ParallaxLodThreshold, false, false, 0, 10, asset, "ParallaxLodThreshold");
            // Parallax Minimum Number Samples
            var sliderParallaxMinimumNumberSamples = CommonControls.SliderNumericInt("Parallax Minimum Number Samples", groupNormals,
                                                     asset.ParallaxMinimumNumberSamples, false, false, 0, 50, asset, "ParallaxMinimumNumberSamples");
            // Parallax Maximum Number Samples
            var sliderParallaxMaximumNumberSamples = CommonControls.SliderNumericInt("Parallax Maximum Number Samples", groupNormals,
                                                     asset.ParallaxMaximumNumberSamples, false, false, 0, 500, asset, "ParallaxMaximumNumberSamples");
            // Parallax Maximum Number Samples
            var sliderParallaxHeightMapScale = CommonControls.SliderNumericFloat("Parallax Height Map Scale", groupNormals,
                                               asset.ParallaxHeightMapScale, false, false, 0, 1, asset, "ParallaxHeightMapScale");
            checkBoxParallaxEnabled.CheckedChanged += delegate
            {
                asset.ParallaxEnabled = checkBoxParallaxEnabled.Checked;
                sliderParallaxLodThreshold.Enabled = asset.ParallaxEnabled;
                sliderParallaxMinimumNumberSamples.Enabled = asset.ParallaxEnabled;
                sliderParallaxMaximumNumberSamples.Enabled = asset.ParallaxEnabled;
                sliderParallaxHeightMapScale.Enabled = asset.ParallaxEnabled;
            };
            assetSelectorNormalTexture.Draw += delegate
            {
                if (asset.NormalTexture == null)
                    checkBoxParallaxEnabled.Enabled = false;
            };
            groupNormals.AdjustHeightFromChildren();

            #endregion

            owner.AdjustHeightFromChildren();
        } // AddControls
        
    } // BlinnPhongControls
} // XNAFinalEngine.Editor