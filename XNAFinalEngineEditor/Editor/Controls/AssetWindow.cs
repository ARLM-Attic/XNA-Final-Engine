
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
using System;
using System.Reflection;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Undo;
using XNAFinalEngine.UserInterface;
using EventArgs = XNAFinalEngine.UserInterface.EventArgs;
using EventHandler = XNAFinalEngine.UserInterface.EventHandler;
#endregion

namespace XNAFinalEngine.Editor
{
    internal static class AssetWindow
    {

        #region Variables

        // This is a copy of the asset in its current state of creation.
        private static Asset currentCreatedAsset;

        #endregion

        #region Properties

        /// <summary>
        /// This is a copy of the asset in its current state of creation.
        /// </summary>
        public static Asset CurrentCreatedAsset
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
        public static Window Show<TAssetType>(Asset asset) where TAssetType : Asset
        {
            ContentManager temporalContentManager = null;
            ContentManager userContentManager = null;

            bool assetCreation = asset == null;
            PropertyInfo filenamesProperty = typeof(TAssetType).GetProperty("Filenames"); // Search for the Filenames property, not all assets have it.
            bool resourcedAsset = filenamesProperty != null; // Indicates if the asset has a resource (textures, models, etc.) or if it is a property asset (shadows, ambient light, etc.)
            string[] filenames = null;
            if (assetCreation)
            {
                // If the asset has an internal XNA Resource (textures, models, sounds)
                if (resourcedAsset)
                {
                    filenames = (string[])filenamesProperty.GetValue(asset, null);
                    // If there is no asset to create then return                
                    if (filenames.Length == 0)
                    {
                        CurrentCreatedAsset = null; // To avoid unwanted event references.
                        return null;
                    }
                    userContentManager = ContentManager.CurrentContentManager;
                    temporalContentManager = new ContentManager { Name = "Temporal Content Manager", Hidden = true };
                    ContentManager.CurrentContentManager = temporalContentManager;

                    // Create a temporal asset with the first resource in the list.
                    asset = (Asset)typeof(TAssetType).GetConstructor(new[] { typeof(string) }).Invoke(new object[] { filenames[0] });
                }
                // If not... (ambient light, post process, shadows, etc.)
                else
                    asset = (Asset)typeof(TAssetType).GetConstructor(new Type[] { }).Invoke(null);
                
                CurrentCreatedAsset = asset;
            }

            #region Window

            var window = new UserInterface.AssetWindow
            {
                AssetName = asset.Name,
                AssetType = (typeof(TAssetType)).Name,
            };
            window.AssetNameChanged += delegate
            {
                string oldName = asset.Name;
                asset.SetUniqueName(window.AssetName);
                if (asset.Name != oldName)
                {
                    window.AssetName = asset.Name; // The window name could be change if the name was not unique.
                    asset.Name = oldName; // This is done for the undo.
                    using (Transaction.Create())
                    {
                        // Apply the command and store for the undo feature.
                        ActionManager.SetProperty(asset, "Name", window.AssetName);
                        ActionManager.CallMethod(// Redo
                                                 UserInterfaceManager.Invalidate,
                                                 // Undo
                                                 UserInterfaceManager.Invalidate);
                    }
                   
                }
            };
            window.Draw += delegate { window.AssetName = asset.Name; };
            // In creation I don't want that the user mess with other things.)
            if (assetCreation)
                window.ShowModal();

            #endregion

            #region Group Resource

            GroupBox groupResource;
            ComboBox comboBoxResource = null;
            ComboBox comboBoxContentManager = null;

            if (resourcedAsset)
            {
                groupResource = CommonControls.Group("Resource", window);
                comboBoxResource = CommonControls.ComboBox("Resource", groupResource);
                comboBoxContentManager = CommonControls.ComboBox("Content Manager", groupResource);
                groupResource.AdjustHeightFromChildren();
            }

            #endregion

            #region Creation Mode

            if (assetCreation)
            {
                
                #region Combo Box Resource
                
                if (resourcedAsset)
                {
                    comboBoxResource.Items.AddRange(filenames);
                    comboBoxResource.ItemIndex = 0;
                    // Events
                    comboBoxResource.ItemIndexChanged += delegate
                    {
                        if (filenames[comboBoxResource.ItemIndex] != asset.Filename)
                        {
                            // This is a disposable asset so...
                            temporalContentManager.Unload();
                            // To contemplate some assets like Lookup Tables and some textures.
                            if (!asset.IsDisposed)
                                asset.Dispose();
                            asset = (Asset)typeof(TAssetType).GetConstructor(new[] { typeof(string) }).Invoke(new object[] { filenames[comboBoxResource.ItemIndex] });
                            CurrentCreatedAsset = asset;
                            window.AssetName = asset.Name;
                        }
                    };
                    comboBoxResource.Draw += delegate
                    {
                        if (comboBoxResource.ListBoxVisible)
                            return;
                        for (int i = 0; i < comboBoxResource.Items.Count; i++)
                        {
                            if ((string)comboBoxResource.Items[i] == asset.Filename)
                            {
                                if (comboBoxResource.ItemIndex != i)
                                {
                                    comboBoxResource.ItemIndex = i;
                                    break;
                                }
                            }
                        }
                    };

                }
                
                #endregion

                #region Combo Box Content Manager

                if (resourcedAsset)
                {
                    // The names of the content manager are added here because we want to place the item index in the current content manager.
                    comboBoxContentManager.Items.Clear();
                    // Add content names.
                    foreach (ContentManager contentManager in ContentManager.SortedContentManagers)
                    {
                        if (!contentManager.Hidden)
                            comboBoxContentManager.Items.Add(contentManager.Name);
                    }
                    // Find the current content manager.
                    comboBoxContentManager.ItemIndex = 0;
                    for (int i = 0; i < comboBoxContentManager.Items.Count; i++)
                    {
                        if (ContentManager.SortedContentManagers[i] == userContentManager)
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
                        foreach (ContentManager contentManager in ContentManager.SortedContentManagers)
                        {
                            if (!contentManager.Hidden)
                                comboBoxContentManager.Items.Add(contentManager.Name);
                        }
                    };
                }

                #endregion

                #region Window Closed
                
                window.Closed += delegate
                {
                    if (resourcedAsset)
                        temporalContentManager.Dispose();
                    if (window.ModalResult == ModalResult.Cancel)
                    {
                        // Returns null.
                        CurrentCreatedAsset = null;
                        if (!asset.IsDisposed)
                            asset.Dispose();
                    }
                    else
                    {
                        if (resourcedAsset)
                        {
                            if (!asset.IsDisposed) // To contemplate some assets like Lookup Tables and some textures.
                                asset.Dispose();
                            // Search the content manager reference using its name.
                            foreach (ContentManager contenManager in ContentManager.SortedContentManagers)
                            {
                                if (contenManager.Name == (string)(comboBoxContentManager.Items[comboBoxContentManager.ItemIndex]))
                                {
                                    ContentManager.CurrentContentManager = contenManager;
                                    break;
                                }
                            }
                            // And create the asset with this content manager.
                            CurrentCreatedAsset = (Asset)typeof(TAssetType).GetConstructor(new[] { typeof(string) }).Invoke(new object[] { filenames[comboBoxResource.ItemIndex] });
                        }
                        CurrentCreatedAsset.Name = window.AssetName;
                    }
                    // Restore user content manager.
                    if (resourcedAsset)
                        ContentManager.CurrentContentManager = userContentManager;
                    // Remove references to the event.
                    CurrentCreatedAssetChanged = null;
                };

                #endregion

            }

            #endregion

            #region Edit Mode

            else // If it is in edit mode...
            {
                // Fill Resource Combo Box.
                if (resourcedAsset)
                {
                    comboBoxResource.Items.Add(asset.Filename);
                    comboBoxResource.ItemIndex = 0;
                    comboBoxResource.Enabled = false;
                    // Fill Content Manager Combo Box.
                    if (asset.ContentManager != null)
                        comboBoxContentManager.Items.Add(asset.ContentManager.Name);
                    else
                        comboBoxContentManager.Items.Add("Does not have a Content Manager");
                    comboBoxContentManager.ItemIndex = 0;
                    comboBoxContentManager.Enabled = false;
                }
            }
            #endregion

            #region Specific Controls for Specific Assets

            if (typeof(TAssetType) == typeof(Texture))
                TextureControls.AddControls((Texture)asset, window, comboBoxResource);
            else if (typeof(TAssetType) == typeof(PostProcess))
                PostProcessControls.AddControls((PostProcess) asset, window, comboBoxResource);
            else if (typeof(TAssetType) == typeof(LookupTable))
                LookupTableControls.AddControls((LookupTable)asset, window, comboBoxResource);
            else if (typeof(TAssetType) == typeof(AmbientLight))
                AmbientLightControls.AddControls((AmbientLight)asset, window, comboBoxResource);
            else if (typeof(TAssetType) == typeof(HorizonBasedAmbientOcclusion))
                HorizonBasedAmbientOcclusionControls.AddControls((HorizonBasedAmbientOcclusion)asset, window, comboBoxResource);
            else if (typeof(TAssetType) == typeof(BasicShadow))
                ShadowControls.AddControls((BasicShadow)asset, window, comboBoxResource);
            else if (typeof(TAssetType) == typeof(Sphere))
                PrimitiveControls.AddControls((Sphere)asset, window, assetCreation);
            else if (typeof(TAssetType) == typeof(Box))
                PrimitiveControls.AddControls((Box)asset, window, assetCreation);
            else if (typeof(TAssetType) == typeof(Plane))
                PrimitiveControls.AddControls((Plane)asset, window, assetCreation);
            else if (typeof(TAssetType) == typeof(Cylinder))
                PrimitiveControls.AddControls((Cylinder)asset, window, assetCreation);
            else if (typeof(TAssetType) == typeof(Cone))
                PrimitiveControls.AddControls((Cone)asset, window, assetCreation);
            else if (typeof(TAssetType) == typeof(BlinnPhong))
                BlinnPhongControls.AddControls((BlinnPhong)asset, window);
            else if (typeof(TAssetType) == typeof(Constant))
                ConstantControls.AddControls((Constant)asset, window);
            else if (typeof(TAssetType) == typeof(CarPaint))
                CarPaintControls.AddControls((CarPaint)asset, window);

            #endregion);

            if (assetCreation)
            {
                #region Buttons

                window.StatusBar = new StatusBar();
                var buttonApply = new Button
                {
                    Anchor = Anchors.Top | Anchors.Right,
                    //Top = window.AvailablePositionInsideControl + CommonControls.ControlSeparation,
                    Top = 5,
                    Left = window.ClientWidth - 4 - 70 * 2 - 8,
                    Text = "Create",
                    Parent = window.StatusBar,
                };
                buttonApply.Click += delegate { window.Close(); };

                var buttonClose = new Button
                {
                    Anchor = Anchors.Top | Anchors.Right,
                    Text = "Cancel",
                    ModalResult = ModalResult.Cancel,
                    Top = buttonApply.Top,
                    Left = window.ClientWidth - 70 - 8,
                    Parent = window.StatusBar
                };
                window.StatusBar.Height = buttonApply.Top + buttonApply.Height + 5;

                #endregion
            }

            window.AdjustHeightFromChildren();
            window.Height += 5;
            if (window.Height > 600)
                window.Height = 600;
            return window;
        } // Show

        #endregion

    } // AssetWindow
} // XNAFinalEngine.Editor