
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
using Microsoft.Xna.Framework.Graphics;
using XNAFinalEngine.Assets;
using XNAFinalEngine.UserInterface;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.Editor
{
    internal static class LookupTableControls
    {

        #region Show

        /// <summary>
        /// Creates the configuration controls of this asset.
        /// </summary>
        public static void AddControls(LookupTable asset, Window owner, ComboBox comboBoxResource)
        {
            // In asset creation I need to look on the CurrentCreatedAsset property to have the last asset.
            // I can't use CurrentCreatedAsset in edit mode.
            // However I can use asset for creation (maybe in a disposed state but don't worry) and edit mode,
            // and only update the values when I know that CurrentCreatedAsset changes.
            
            #region Group Image

            var groupImage = CommonControls.Group("Image", owner);
            AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
            AssetContentManager.CurrentContentManager = UserInterfaceManager.UserInterfaceContentManager;
            var imageBoxImage = CommonControls.ImageBox(LookupTable.LookupTableToTexture(asset), groupImage);
            AssetContentManager.CurrentContentManager = userContentManager;
            groupImage.AdjustHeightFromChildren();

            #endregion
            
            #region Group Properties

            GroupBox groupProperties = CommonControls.Group("Properties", owner);

            var sizeTextBox = CommonControls.TextBox("Size", groupProperties, asset.Size.ToString());
            sizeTextBox.Enabled = false;

            groupProperties.AdjustHeightFromChildren();

            #endregion

            // If it is asset creation time.
            if (comboBoxResource != null)
            {
                comboBoxResource.ItemIndexChanged += delegate
                {
                    imageBoxImage.Texture.Dispose();
                    userContentManager = AssetContentManager.CurrentContentManager;
                    AssetContentManager.CurrentContentManager = UserInterfaceManager.UserInterfaceContentManager;
                    imageBoxImage.Texture = LookupTable.LookupTableToTexture((LookupTable)AssetWindow.CurrentCreatedAsset);
                    AssetContentManager.CurrentContentManager = userContentManager;
                    sizeTextBox.Text = ((LookupTable)AssetWindow.CurrentCreatedAsset).Size.ToString();
                };
                // If the user creates the asset (press the create button) then update the changeable properties.
                owner.Closed += delegate
                {
                    imageBoxImage.Texture.Dispose();
                };
            }
        } // AddControls

        #endregion
        
    } // LookupTableControls
} // XNAFinalEngine.Editor