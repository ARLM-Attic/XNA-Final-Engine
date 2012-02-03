
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
    public static class LookupTableWindow
    {

        #region Variables

        // This is a copy of the asset in its current state of creation.
        private static LookupTable currentCreatedAsset;

        #endregion

        #region Properties

        /// <summary>
        /// This is a copy of the asset in its current state of creation.
        /// </summary>
        public static LookupTable CurrentCreatedAsset
        {
            get { return currentCreatedAsset; }
            private set
            {
                currentCreatedAsset = value;
                if (CurrentCreatedAssetChanged != null)
                    CurrentCreatedAssetChanged.Invoke(null, new EventArgs());
            }
        } // CurrentCreatedAsset

        #endregion

        #region Events

        public static event EventHandler CurrentCreatedAssetChanged;

        #endregion

        #region Show

        /// <summary>
        /// Creates and shows the configuration window of this asset.
        /// </summary>
        public static void Show(LookupTable asset)
        {
            // If there is no asset to create then return
            if (asset == null && LookupTable.LookupTablesFilenames.Length == 0)
            {
                CurrentCreatedAsset = null; // To avoid unwanted event references.
                return;
            }

            bool assetCreation = asset == null;
            
            if (assetCreation)
            {
                // Create a temporal asset with the first resource in the list.
                asset = new LookupTable(LookupTable.LookupTablesFilenames[0]);
                CurrentCreatedAsset = asset;
            }

            #region Window

            var window = new AssetWindow
            {
                AssetName = asset.Name,
                AssetType = "Lookup Table"
            };
            window.AssetNameChanged += delegate
            {
                asset.Name = window.AssetName;
                window.AssetName = asset.Name; // If the new name is not unique
            };
            window.Draw += delegate { window.AssetName = asset.Name; };
            // In creation I don't want that the user mess with other things.)
            if (assetCreation)
                window.ShowModal();

            #endregion

            #region Group Resource

            GroupBox groupResource = CommonControls.Group("Resource", window);

            #region Resource

            var comboBoxResource = CommonControls.ComboBox("Resource", groupResource);

            #endregion

            #region Content Manager

            var comboBoxContentManager = CommonControls.ComboBox("Content Manager", groupResource);
            // Add resources' names
            if (asset.ContentManager == null)
            {
                comboBoxContentManager.Items.Add("Does not use a content manager.");
                comboBoxContentManager.Enabled = false;
            }
            else if (asset.ContentManager.Name == "")
                comboBoxContentManager.Items.Add("Does not have name.");
            else
                comboBoxContentManager.Items.Add(asset.ContentManager.Name);
            comboBoxContentManager.ItemIndex = 0;

            #endregion

            groupResource.AdjustHeightFromChildren();

            #endregion

            #region Group Image

            var groupImage = CommonControls.Group("Image", window);

            var imageBoxImage = CommonControls.ImageBox(LookupTable.LookupTableToTexture(asset), groupImage);

            groupImage.AdjustHeightFromChildren();

            #endregion

            #region Group Properties

            GroupBox groupProperties = CommonControls.Group("Properties", window);

            var sizeTextBox = CommonControls.TexteBox("Size", groupProperties, asset.Size.ToString());
            sizeTextBox.Enabled = false;
            sizeTextBox.Draw += delegate { sizeTextBox.Text = asset.Size.ToString(); };

            groupProperties.AdjustHeightFromChildren();

            #endregion

            if (assetCreation)
            {
                #region Buttons

                var buttonApply = new Button
                {
                    Anchor = Anchors.Top | Anchors.Right,
                    Top = window.AvailablePositionInsideControl + CommonControls.ControlSeparation,
                    Left = window.ClientWidth - 4 - 70 * 2 - 8,
                    Text = "Create",
                    Parent = window,
                };
                buttonApply.Click += delegate
                {
                    CurrentCreatedAssetChanged = null;
                    window.Close();
                };

                var buttonClose = new Button
                {
                    Anchor = Anchors.Top | Anchors.Right,
                    Text = "Cancel",
                    ModalResult = ModalResult.Cancel,
                    Top = buttonApply.Top,
                    Left = window.ClientWidth - 70 - 8,
                    Parent = window,
                };

                #endregion

                #region Combo Box Resource

                // Add textures name
                comboBoxResource.Items.AddRange(LookupTable.LookupTablesFilenames);
                // Events
                comboBoxResource.ItemIndexChanged += delegate
                {
                    if (LookupTable.LookupTablesFilenames[comboBoxResource.ItemIndex] != asset.Resource.Name)
                    {
                        // This is a disposable asset so...
                        asset.Dispose();
                        asset = new LookupTable(LookupTable.LookupTablesFilenames[comboBoxResource.ItemIndex]);
                        CurrentCreatedAsset = asset;
                        //nameTextBox.Text = asset.Name;
                        imageBoxImage.Texture.Dispose();
                        imageBoxImage.Texture = LookupTable.LookupTableToTexture(asset);
                        window.AssetName = asset.Name;
                    }
                };
                comboBoxResource.Draw += delegate
                {
                    if (comboBoxResource.ListBoxVisible)
                        return;
                    for (int i = 0; i < comboBoxResource.Items.Count; i++)
                        if ((string)comboBoxResource.Items[i] == asset.Resource.Name)
                        {
                            comboBoxResource.ItemIndex = i;
                            break;
                        }
                };

                #endregion

                #region Window
                
                window.Closed += delegate
                {
                    if (window.ModalResult == ModalResult.Cancel)
                    {
                        CurrentCreatedAsset = null;
                        asset.Dispose();
                    }
                    CurrentCreatedAssetChanged = null;
                };

                #endregion
            }
            else
            {
                comboBoxResource.Items.Add(asset.Resource.Name);
                comboBoxResource.ItemIndex = 0;
                comboBoxResource.Enabled = false;
            }
            window.AdjustHeightFromChildren();
        } // Show

        #endregion

    } // LookupTableWindow
} // XNAFinalEngine.Editor