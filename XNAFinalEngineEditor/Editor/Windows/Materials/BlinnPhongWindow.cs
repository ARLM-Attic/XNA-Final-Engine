
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.UserInterface;
#endregion

namespace XNAFinalEngine.Editor
{
    public static class BlinnPhongWindow
    {

        /// <summary>
        /// Creates and shows the configuration window of this material.
        /// </summary>
        public static void Show(BlinnPhong asset)
        {

            #region Window

            var window = new AssetWindow
            {
                AssetName = asset.Name,
                AssetType = "Blinn-Phong"
            };
            window.AssetNameChanged += delegate
            {
                asset.Name = window.AssetName;
                window.AssetName = asset.Name; // If the new name is not unique
            };
            window.Draw += delegate { window.AssetName = asset.Name; };

            #endregion
            
            #region Group Diffuse

            GroupBox groupDiffuse = CommonControls.Group("Diffuse", window);

            #region Diffuse Color

            var sliderDiffuseColor = CommonControls.SliderColor("Diffuse Color", groupDiffuse, asset.DiffuseColor);
            sliderDiffuseColor.ColorChanged += delegate { asset.DiffuseColor = sliderDiffuseColor.Color; };
            sliderDiffuseColor.Draw += delegate { sliderDiffuseColor.Color = asset.DiffuseColor; };

            #endregion

            #region Diffuse Texture

            var assetSelectorDiffuseTexture = CommonControls.AssetSelector("Diffuse Texture", groupDiffuse);
            assetSelectorDiffuseTexture.AssetAdded += delegate
            {
                TextureWindow.CurrentCreatedAssetChanged += delegate
                {
                    asset.DiffuseTexture = TextureWindow.CurrentCreatedAsset;
                    window.Invalidate();
                };
                TextureWindow.Show(null);
            };
            assetSelectorDiffuseTexture.AssetEdited += delegate
            {
                TextureWindow.Show(asset.DiffuseTexture);
            };
            // Events
            assetSelectorDiffuseTexture.ItemIndexChanged += delegate
            {
                if (assetSelectorDiffuseTexture.ItemIndex <= 0)
                    asset.DiffuseTexture = null;
                else
                {
                    foreach (Texture texture in Asset.LoadedAssets)
                    {
                        if (texture.Name == (string)assetSelectorDiffuseTexture.Items[assetSelectorDiffuseTexture.ItemIndex])
                            asset.DiffuseTexture = texture;
                    }
                }
                assetSelectorDiffuseTexture.EditButtonEnabled = asset.DiffuseTexture != null;
                sliderDiffuseColor.Enabled = assetSelectorDiffuseTexture.ItemIndex == 0;
            };
            assetSelectorDiffuseTexture.Draw += delegate
            {
                // Add textures name here because someone could dispose or add new asset.
                assetSelectorDiffuseTexture.Items.Clear();
                assetSelectorDiffuseTexture.Items.Add("No texture");
                foreach (Texture texture in Asset.LoadedAssets)
                {
                    // You can filter some assets here.
                    if (texture.ContentManager == null || !texture.ContentManager.Hidden)
                        assetSelectorDiffuseTexture.Items.Add(texture.Name);
                }

                if (assetSelectorDiffuseTexture.ListBoxVisible)
                    return;
                // Identify current index
                if (asset.DiffuseTexture == null)
                    assetSelectorDiffuseTexture.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < assetSelectorDiffuseTexture.Items.Count; i++)
                    {
                        if ((string)assetSelectorDiffuseTexture.Items[i] == asset.DiffuseTexture.Name)
                        {
                            assetSelectorDiffuseTexture.ItemIndex = i;
                            break;
                        }
                    }
                }
            };

            #endregion

            groupDiffuse.AdjustHeightFromChildren();

            #endregion

            #region Group Specular

            GroupBox groupSpecular = CommonControls.Group("Specular", window);

            CheckBox checkBoxSpecularPowerFromTexture = null;

            #region Specular Intensity

            var sliderSpecularIntensity = CommonControls.SliderNumeric("Specular Intensity", groupSpecular, asset.SpecularIntensity, false, true, 0, 2);
            sliderSpecularIntensity.ValueChanged += delegate { asset.SpecularIntensity = sliderSpecularIntensity.Value; };
            sliderSpecularIntensity.Draw += delegate { sliderSpecularIntensity.Value = asset.SpecularIntensity; };

            #endregion

            #region Specular Power

            var sliderSpecularPower = CommonControls.SliderNumeric("Specular Power", groupSpecular, asset.SpecularPower, true, true, 0, 100);
            sliderSpecularPower.ValueChanged += delegate { asset.SpecularPower = sliderSpecularPower.Value; };
            sliderSpecularPower.Draw += delegate
            {
                sliderSpecularPower.Value = asset.SpecularPower;
                sliderSpecularPower.Enabled = !checkBoxSpecularPowerFromTexture.Enabled || (checkBoxSpecularPowerFromTexture.Enabled && !checkBoxSpecularPowerFromTexture.Checked);
            };

            #endregion

            #region Specular Texture

            var assetSelectorSpecularTexture = CommonControls.AssetSelector("Specular Texture", groupSpecular);
            assetSelectorSpecularTexture.AssetAdded += delegate
            {
                TextureWindow.CurrentCreatedAssetChanged += delegate
                {
                    asset.SpecularTexture = TextureWindow.CurrentCreatedAsset;
                    window.Invalidate();
                };
                TextureWindow.Show(null);
            };
            assetSelectorSpecularTexture.AssetEdited += delegate
            {
                TextureWindow.Show(asset.SpecularTexture);
            };
            // Events
            assetSelectorSpecularTexture.ItemIndexChanged += delegate
            {
                if (assetSelectorSpecularTexture.ItemIndex <= 0)
                    asset.SpecularTexture = null;
                else
                {
                    foreach (Texture texture in Asset.LoadedAssets)
                    {
                        if (texture.Name == (string)assetSelectorSpecularTexture.Items[assetSelectorSpecularTexture.ItemIndex])
                            asset.SpecularTexture = texture;
                    }
                }
                assetSelectorSpecularTexture.EditButtonEnabled = asset.SpecularTexture != null;
                checkBoxSpecularPowerFromTexture.Enabled = asset.SpecularTexture != null;
            };
            assetSelectorSpecularTexture.Draw += delegate
            {
                // Add textures name here because someone could dispose or add new asset.
                assetSelectorSpecularTexture.Items.Clear();
                assetSelectorSpecularTexture.Items.Add("No texture");
                foreach (Texture texture in Asset.LoadedAssets)
                {
                    // You can filter some assets here.
                    if (texture.ContentManager == null || !texture.ContentManager.Hidden)
                        assetSelectorSpecularTexture.Items.Add(texture.Name);
                }

                if (assetSelectorSpecularTexture.ListBoxVisible)
                    return;
                // Identify current index
                if (asset.SpecularTexture == null)
                    assetSelectorSpecularTexture.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < assetSelectorSpecularTexture.Items.Count; i++)
                    {
                        if ((string)assetSelectorSpecularTexture.Items[i] == asset.SpecularTexture.Name)
                        {
                            assetSelectorSpecularTexture.ItemIndex = i;
                            break;
                        }
                    }
                }
            };

            #endregion

            #region Specular Texture Power Enabled
            
            checkBoxSpecularPowerFromTexture = CommonControls.CheckBox("Use specular power from Texture", groupSpecular, asset.SpecularPowerFromTexture,
                "Indicates if the specular power will be read from the texture (the alpha channel of the specular texture) or from the specular power property.");
            checkBoxSpecularPowerFromTexture.CheckedChanged += delegate { asset.SpecularPowerFromTexture = checkBoxSpecularPowerFromTexture.Checked; };
            checkBoxSpecularPowerFromTexture.Draw += delegate {  checkBoxSpecularPowerFromTexture.Checked = asset.SpecularPowerFromTexture; };
            
            #endregion

            #region Reflection Texture

            var assetSelectorReflectionTexture = CommonControls.AssetSelector("Reflection Texture", groupSpecular);
            assetSelectorReflectionTexture.AssetAdded += delegate
            {
                TextureCubeWindow.CurrentCreatedAssetChanged += delegate
                {
                    asset.ReflectionTexture = TextureCubeWindow.CurrentCreatedAsset;
                    window.Invalidate();
                };
                TextureCubeWindow.Show(null);
            };
            assetSelectorReflectionTexture.AssetEdited += delegate
            {
                TextureCubeWindow.Show(asset.ReflectionTexture);
            };
            // Events
            assetSelectorReflectionTexture.ItemIndexChanged += delegate
            {
                if (assetSelectorReflectionTexture.ItemIndex <= 0)
                    asset.ReflectionTexture = null;
                else
                {
                    foreach (Asset texture in Asset.LoadedAssets)
                    {
                        if (texture is TextureCube && texture.Name == (string)assetSelectorReflectionTexture.Items[assetSelectorReflectionTexture.ItemIndex])
                            asset.ReflectionTexture = (TextureCube)texture;
                    }
                }
                assetSelectorReflectionTexture.EditButtonEnabled = asset.ReflectionTexture != null;
            };
            assetSelectorReflectionTexture.Draw += delegate
            {
                // Add textures name here because someone could dispose or add new asset.
                assetSelectorReflectionTexture.Items.Clear();
                assetSelectorReflectionTexture.Items.Add("No texture");
                foreach (Asset texture in Asset.LoadedAssets)
                {
                    // You can filter some assets here.
                    if (texture.ContentManager == null || !texture.ContentManager.Hidden)
                        assetSelectorReflectionTexture.Items.Add(texture.Name);
                }

                if (assetSelectorReflectionTexture.ListBoxVisible)
                    return;
                // Identify current index
                if (asset.ReflectionTexture == null)
                    assetSelectorReflectionTexture.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < assetSelectorReflectionTexture.Items.Count; i++)
                    {
                        if ((string)assetSelectorReflectionTexture.Items[i] == asset.ReflectionTexture.Name)
                        {
                            assetSelectorReflectionTexture.ItemIndex = i;
                            break;
                        }
                    }
                }
            };
            
            #endregion

            groupSpecular.AdjustHeightFromChildren();
            
            #endregion

            #region Normals

            GroupBox groupNormals = CommonControls.Group("Normals", window);

            #region Normal Texture

            var assetSelectorNormalTexture = CommonControls.AssetSelector("Normal Texture", groupNormals);
            assetSelectorNormalTexture.AssetAdded += delegate
            {
                TextureWindow.CurrentCreatedAssetChanged += delegate
                {
                    asset.NormalTexture = TextureWindow.CurrentCreatedAsset;
                    window.Invalidate();
                };
                TextureWindow.Show(null);
            };
            assetSelectorNormalTexture.AssetEdited += delegate { TextureWindow.Show(asset.NormalTexture); };
            // Events
            assetSelectorNormalTexture.ItemIndexChanged += delegate
            {
                if (assetSelectorNormalTexture.ItemIndex <= 0)
                    asset.NormalTexture = null;
                else
                {
                    foreach (Texture texture in Asset.LoadedAssets)
                    {
                        if (texture.Name == (string)assetSelectorNormalTexture.Items[assetSelectorNormalTexture.ItemIndex])
                            asset.NormalTexture = texture;
                    }
                }
                assetSelectorNormalTexture.EditButtonEnabled = asset.NormalTexture != null;
            };
            assetSelectorNormalTexture.Draw += delegate
            {
                // Add textures name here because someone could dispose or add new asset.
                assetSelectorNormalTexture.Items.Clear();
                assetSelectorNormalTexture.Items.Add("No texture");
                foreach (Texture texture in Asset.LoadedAssets)
                {
                    // You can filter some assets here.
                    if (texture.ContentManager == null || !texture.ContentManager.Hidden)
                        assetSelectorNormalTexture.Items.Add(texture.Name);
                }

                if (assetSelectorNormalTexture.ListBoxVisible)
                    return;
                // Identify current index
                if (asset.NormalTexture == null)
                    assetSelectorNormalTexture.ItemIndex = 0;
                else
                {
                    for (int i = 0; i < assetSelectorNormalTexture.Items.Count; i++)
                    {
                        if ((string)assetSelectorNormalTexture.Items[i] == asset.NormalTexture.Name)
                        {
                            assetSelectorNormalTexture.ItemIndex = i;
                            break;
                        }
                    }
                }
            };

            #endregion

            #region Parallax Enabled

            CheckBox checkBoxParallaxEnabled = CommonControls.CheckBox("Enabled", groupNormals, asset.ParallaxEnabled);
            checkBoxParallaxEnabled.Draw += delegate { checkBoxParallaxEnabled.Checked = asset.ParallaxEnabled; };

            #endregion

            #region Parallax Threshold

            var sliderParallaxLodThreshold = CommonControls.SliderNumeric("Parallax LOD Threshold", groupNormals, asset.ParallaxLodThreshold, false, false, 0, 10);
            sliderParallaxLodThreshold.ValueChanged += delegate { asset.ParallaxLodThreshold = (int)sliderParallaxLodThreshold.Value; };
            sliderParallaxLodThreshold.Draw += delegate { sliderParallaxLodThreshold.Value = asset.ParallaxLodThreshold; };

            #endregion

            #region Parallax Minimum Number Samples

            var sliderParallaxMinimumNumberSamples = CommonControls.SliderNumeric("Parallax Minimum Number Samples", groupNormals, asset.ParallaxMinimumNumberSamples, false, false, 0, 50);
            sliderParallaxMinimumNumberSamples.ValueChanged += delegate { asset.ParallaxMinimumNumberSamples = (int)sliderParallaxMinimumNumberSamples.Value; };
            sliderParallaxMinimumNumberSamples.Draw += delegate { sliderParallaxMinimumNumberSamples.Value = asset.ParallaxMinimumNumberSamples; };

            #endregion

            #region Parallax Maximum Number Samples

            var sliderParallaxMaximumNumberSamples = CommonControls.SliderNumeric("Parallax Maximum Number Samples", groupNormals, asset.ParallaxMaximumNumberSamples, false, false, 0, 500);
            sliderParallaxMaximumNumberSamples.ValueChanged += delegate { asset.ParallaxMaximumNumberSamples = (int)sliderParallaxMaximumNumberSamples.Value; };
            sliderParallaxMaximumNumberSamples.Draw += delegate { sliderParallaxMaximumNumberSamples.Value = asset.ParallaxMaximumNumberSamples; };

            #endregion

            #region Parallax Maximum Number Samples

            var sliderParallaxHeightMapScale = CommonControls.SliderNumeric("Parallax Height Map Scale", groupNormals, asset.ParallaxHeightMapScale, false, false, 0, 1);
            sliderParallaxHeightMapScale.ValueChanged += delegate { asset.ParallaxHeightMapScale = sliderParallaxHeightMapScale.Value; };
            sliderParallaxHeightMapScale.Draw += delegate { sliderParallaxHeightMapScale.Value = asset.ParallaxHeightMapScale; };

            #endregion

            checkBoxParallaxEnabled.CheckedChanged += delegate
            {
                asset.ParallaxEnabled = checkBoxParallaxEnabled.Checked;
                sliderParallaxLodThreshold.Enabled = asset.ParallaxEnabled;
                sliderParallaxMinimumNumberSamples.Enabled = asset.ParallaxEnabled;
                sliderParallaxMaximumNumberSamples.Enabled = asset.ParallaxEnabled;
                sliderParallaxHeightMapScale.Enabled = asset.ParallaxEnabled;
            };
            
            groupNormals.AdjustHeightFromChildren();

            #endregion

            window.Height = 500;

        } // Show

    } // BlinnPhongWindow
} // XNAFinalEngine.Editor