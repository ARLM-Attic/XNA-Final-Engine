
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.UserInterface;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Editor
{
    public static class ConstantWindow
    {

        /// <summary>
        /// Creates and shows the configuration window of this material.
        /// </summary>
        public static void Show(Constant asset)
        {

            #region Window

            var window = new AssetWindow
            {
                AssetName = asset.Name,
                AssetType = "Constant"
            };
            window.AssetNameChanged += delegate
            {
                asset.Name = window.AssetName;
                window.AssetName = asset.Name; // If the new name is not unique
            };
            window.Draw += delegate { window.AssetName = asset.Name; };

            #endregion
            
            #region Group Surface Parameters

            GroupBox group = CommonControls.Group("Surface Parameters", window);

            #endregion
            
            #region Diffuse Color

            var sliderDiffuseColor = CommonControls.SliderColor("Diffuse Color", group, asset.DiffuseColor);
            sliderDiffuseColor.ColorChanged += delegate { asset.DiffuseColor = sliderDiffuseColor.Color; };
            sliderDiffuseColor.Draw += delegate { sliderDiffuseColor.Color = asset.DiffuseColor; };

            #endregion
            
            #region Diffuse Texture

            var assetSelectorDiffuseTexture = CommonControls.AssetSelector("Diffuse Texture", group);
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
                    // If we have to change the asset...
                    if (asset.DiffuseTexture == null ||
                        asset.DiffuseTexture.Name != (string)assetSelectorDiffuseTexture.Items[assetSelectorDiffuseTexture.ItemIndex])
                    {
                        asset.DiffuseTexture = Texture.LoadedTextures[assetSelectorDiffuseTexture.ItemIndex - 1]; // The first item is the no texture item.
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
                foreach (Texture texture in Texture.LoadedTextures)
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
            
            group.AdjustHeightFromChildren();
            window.AdjustHeightFromChildren();
        } // Show

    } // ConstantWindow
} // XNAFinalEngine.Editor