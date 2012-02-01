
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
        }

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
                return;

            bool assetCreation = asset == null;
            
            if (assetCreation)
            {
                // Create a temporal asset with the first resource in the list.
                asset = new LookupTable(LookupTable.LookupTablesFilenames[0]);
                CurrentCreatedAsset = asset;
            }

            #region Window

            var window = new Window
            {
                Parent = null,
                Text = asset.Name + " : Lookup Table"
            };
            // In creation I don't want that the user mess with other things.
            if (assetCreation)
            {
                window.ShowModal();
            }
            window.Closed += delegate { };

            #endregion

            #region Name

            var nameLabel = new Label
            {
                Parent = window,
                Text = "Name", Left = 10, Top = 10,
            };
            var nameTextBox = new TextBox
            {
                Parent = window,
                Width = window.ClientWidth - nameLabel.Width - 25,
                Text = asset.Name, Left = 60, Top = 10
            };
            nameTextBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    asset.Name = nameTextBox.Text;
                    window.Text = asset.Name + " : Lookup Table";
                }
            };
            nameTextBox.FocusLost += delegate
            {
                asset.Name = nameTextBox.Text;
                window.Text = asset.Name + " : Lookup Table";
            };

            #endregion

            #region Group Resource

            GroupBox groupResource = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Height = 160,
                Left = 8,
                Top = nameLabel.Top + nameLabel.Height + 15,
                Text = "Resource",
                TextColor = Color.Gray,
            };

            #region Resource

            var labelResource = new Label
            {
                Parent = groupResource,
                Left = 10,
                Top = 25,
                Width = 150,
                Text = "Resource"
            };
            var comboBoxResource = new ComboBox
            {
                Parent = groupResource,
                Left = labelResource.Left + labelResource.Width,
                Top = labelResource.Top,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
            };
            comboBoxResource.Width = groupResource.Width - 10 - comboBoxResource.Left;

            #endregion

            #region Content Manager

            var labelContentManager = new Label
            {
                Parent = groupResource,
                Left = 10,
                Top = labelResource.Top + labelResource.Height + 10,
                Width = 150,
                Text = "Content Manager"
            };
            var comboBoxContentManager = new ComboBox
            {
                Parent = groupResource,
                Left = labelContentManager.Left + labelContentManager.Width,
                Top = labelContentManager.Top,
                Height = 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                MaxItemsShow = 25,
                Enabled = false,
            };
            comboBoxContentManager.Width = groupResource.Width - 10 - comboBoxContentManager.Left;
            // Add resources' names
            if (asset.ContentManager == null)
                comboBoxContentManager.Items.Add("Does not use a content manager.");
            else if (asset.ContentManager.Name == "")
                comboBoxContentManager.Items.Add("Does not have name.");
            else
                comboBoxContentManager.Items.Add(asset.ContentManager.Name);
            comboBoxContentManager.ItemIndex = 0;

            #endregion

            groupResource.Height = labelContentManager.Top + labelContentManager.Height + 20;

            #endregion

            #region Group Image

            GroupBox groupImage = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Height = 200,
                Left = 8,
                Top = groupResource.Top + groupResource.Height + 15,
                Text = "Image",
                TextColor = Color.Gray,
            };

            var imageBoxImage = new ImageBox
            {
                Parent = groupImage,
                Top = 25,
                Left = 8,
                Width = groupImage.ClientWidth - 16,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right | Anchors.Bottom,
                SizeMode = SizeMode.Fit,
                Texture = LookupTable.LookupTableToTexture(asset),
                Height = 160
            };

            groupImage.Height = imageBoxImage.Top + imageBoxImage.Height + 20;

            #endregion

            #region Group Properties

            GroupBox groupProperties = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Height = 160,
                Left = 8,
                Top = groupImage.Top + groupImage.Height + 15,
                Text = "Properties",
                TextColor = Color.Gray,
            };

            var sizeLabel = new Label
            {
                Parent = groupProperties,
                Text = "Size",
                Left = 10,
                Top = 25,
                Width = 150,
            };
            var sizeTextBox = new TextBox
            {
                Parent = groupProperties,
                Width = groupProperties.ClientWidth - sizeLabel.Width - 20,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Text = asset.Size.ToString(),
                Left = 160,
                Top = 25,
                Enabled = false,
            };
            sizeTextBox.Draw += delegate { sizeTextBox.Text = asset.Size.ToString(); };

            groupProperties.Height = sizeLabel.Top + sizeLabel.Height + 20;

            #endregion

            if (assetCreation)
            {
                #region Buttons

                var buttonApply = new Button
                {
                    Parent = window,
                    Anchor = Anchors.Top | Anchors.Right,
                    Top = groupProperties.Top + groupProperties.Height + 10,
                    Left = window.ClientWidth - 4 - 70 * 2 - 8,
                    Text = "Create"
                };
                buttonApply.Click += delegate
                {
                    CurrentCreatedAssetChanged = null;
                    window.Close();
                };

                var buttonClose = new Button
                {
                    Parent = window,
                    Anchor = Anchors.Top | Anchors.Right,
                    Text = "Cancel",
                    ModalResult = ModalResult.Cancel,
                    Top = groupProperties.Top + groupProperties.Height + 10,
                    Left = window.ClientWidth - 70 - 8
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
                        nameTextBox.Text = asset.Name;
                        imageBoxImage.Texture.Dispose();
                        imageBoxImage.Texture = LookupTable.LookupTableToTexture(asset);
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

                window.Height = buttonClose.Top + buttonClose.Height + 40;
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
                comboBoxResource.Enabled = false;
                window.Height = groupProperties.Top + groupProperties.Height + 40;
            }
        } // Show

        #endregion

    } // LookupTableWindow
} // XNAFinalEngine.Editor