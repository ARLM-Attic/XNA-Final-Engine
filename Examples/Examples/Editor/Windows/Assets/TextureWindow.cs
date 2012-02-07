
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
    public static class TextureWindow
    {

        #region Variables

        // This is a copy of the asset in its current state of creation.
        private static Texture currentCreatedAsset;

        #endregion

        #region Properties

        /// <summary>
        /// This is a copy of the asset in its current state of creation.
        /// </summary>
        public static Texture CurrentCreatedAsset
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
        public static void Show(Texture asset)
        {
            ContentManager temporalContentManager = null;
            ContentManager userContentManager = null;
            // If there is no asset to create then return
            if (asset == null && Texture.TexturesFilenames.Length == 0)
            {
                CurrentCreatedAsset = null; // To avoid unwanted event references.
                return;
            }

            bool assetCreation = asset == null;
            
            if (assetCreation)
            {
                userContentManager = ContentManager.CurrentContentManager;
                temporalContentManager = new ContentManager("Temporal Content Manager", true);
                ContentManager.CurrentContentManager = temporalContentManager;
                // Create a temporal asset with the first resource in the list.
                asset = new Texture(Texture.TexturesFilenames[0]);
                CurrentCreatedAsset = asset;
            }

            #region Window

            var window = new AssetWindow
            {
                AssetName = asset.Name,
                AssetType = "Texture"
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
            var comboBoxResource = CommonControls.ComboBox("Resource", groupResource);
            var comboBoxContentManager = CommonControls.ComboBox("Content Manager", groupResource);
            groupResource.AdjustHeightFromChildren();

            #endregion

            #region Group Image

            var groupImage = CommonControls.Group("Image", window);

            var imageBoxImage = CommonControls.ImageBox(asset, groupImage);

            groupImage.AdjustHeightFromChildren();

            #endregion
            /*
            #region Group Properties

            GroupBox groupProperties = CommonControls.Group("Properties", window);

            var sizeTextBox = CommonControls.TexteBox("Size", groupProperties, asset.Size.ToString());
            sizeTextBox.Enabled = false;
            sizeTextBox.Draw += delegate { sizeTextBox.Text = asset.Size.ToString(); };

            groupProperties.AdjustHeightFromChildren();

            #endregion
            */
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
                buttonApply.Click += delegate { window.Close(); };

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
                comboBoxResource.Items.AddRange(Texture.TexturesFilenames);
                // Events
                comboBoxResource.ItemIndexChanged += delegate
                {
                    if (Texture.TexturesFilenames[comboBoxResource.ItemIndex] != asset.Resource.Name)
                    {
                        // This is a disposable asset so...
                        temporalContentManager.Unload();
                        asset = new Texture(Texture.TexturesFilenames[comboBoxResource.ItemIndex]);
                        CurrentCreatedAsset = asset;
                        imageBoxImage.Texture = asset;
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

                #region Combo Box Content Manager

                // The names of the content manager are added here because someone could dispose or add a new one.
                comboBoxContentManager.Items.Clear();
                // Add names
                foreach (ContentManager contentManager in ContentManager.ContentManagers)
                {
                    if (!contentManager.Hidden)
                        comboBoxContentManager.Items.Add(contentManager.Name);
                }
                comboBoxContentManager.ItemIndex = 0;
                for (int i = 0; i < ContentManager.ContentManagers.Count; i++)
                {
                    if (!ContentManager.ContentManagers[i].Hidden && ContentManager.ContentManagers[i] == userContentManager)
                    {
                        comboBoxContentManager.ItemIndex = i;
                        break;
                    }
                }
                comboBoxResource.Draw += delegate
                {
                    // The names of the content manager are added here because someone could dispose or add a new one.
                    comboBoxContentManager.Items.Clear();
                    // Add names
                    foreach (ContentManager contentManager in ContentManager.ContentManagers)
                    {
                        if (!contentManager.Hidden)
                            comboBoxContentManager.Items.Add(contentManager.Name);
                    }
                };

                #endregion

                #region Window Closed
                
                window.Closed += delegate
                {
                    temporalContentManager.Dispose();
                    if (window.ModalResult == ModalResult.Cancel)
                    {
                        CurrentCreatedAsset = null;
                    }
                    else
                    {
                        // Search the content manager reference using its name.
                        foreach (ContentManager contenManager in ContentManager.ContentManagers)
                        {
                            if (contenManager.Name == (string)(comboBoxContentManager.Items[comboBoxContentManager.ItemIndex]))
                            {
                                ContentManager.CurrentContentManager = contenManager;
                                break;
                            }
                        }
                        CurrentCreatedAsset = new Texture(Texture.TexturesFilenames[comboBoxResource.ItemIndex]) { Name = window.AssetName };
                    }
                    // Restore user content manager.
                    ContentManager.CurrentContentManager = userContentManager;
                    CurrentCreatedAssetChanged = null;
                };

                #endregion
            }
            else // If it is in edit mode...
            {
                comboBoxResource.Items.Add(asset.Resource.Name);
                comboBoxResource.ItemIndex = 0;
                comboBoxResource.Enabled = false;
                comboBoxContentManager.Enabled = false;
                if (asset.ContentManager != null)
                    comboBoxContentManager.Items.Add(asset.ContentManager.Name);
                else
                    comboBoxContentManager.Items.Add("Does not have a Content Manager");
                comboBoxContentManager.ItemIndex = 0;
            }
            window.AdjustHeightFromChildren();
        } // Show

        #endregion

    } // TextureWindow
} // XNAFinalEngine.Editor