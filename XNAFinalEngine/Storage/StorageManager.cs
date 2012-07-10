
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
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Storage
{

    public struct ContentManagerData
    {
        public long Id;
        public string Name;
        public List<long> AssetsId;
    } // ContentManagerData

    /// <summary>
    /// Asset data.
    /// </summary>
    public abstract class AssetData
    {
        public long Id;
        public string Name;
        public Type AssetType;
    } // AssetData

    /// <summary>
    /// Assets that have XNB resources.
    /// </summary>
    public class AssetWithResourceData : AssetData
    {
        public string FileName;
    } // ResourcedAssetData

    [Serializable]
    public struct SceneData
    {
        public List<ContentManagerData> ContentManagers;
        public List<Asset> Assets;
    } // SceneData

    /// <summary>
    /// 
    /// </summary>
    public static class StorageManager
    {

        /// <summary>
        /// Create the structure that will be serialized.
        /// It stores content managers, assets and game objects.
        /// </summary>
        private static SceneData CreateSaveData()
        {
            SceneData sceneData = new SceneData
            {
                ContentManagers = new List<ContentManagerData>(),
                Assets = new List<Asset>(),
            };

            #region Content Managers

            // Save content managers data.
            foreach (var contentManager in ContentManager.SortedContentManagers)
            {
                // Hidden content managers are ignored.
                // They carried information only useful for the system to work.
                // Next time you load the scene this information will be (possible) already loaded.
                if (!contentManager.Hidden)
                {
                    ContentManagerData contentManagerData = new ContentManagerData();
                    contentManagerData.Id = contentManager.Id;
                    contentManagerData.Name = contentManager.Name;
                    contentManagerData.AssetsId = new List<long>();
                    foreach (var asset in contentManager.Assets)
                    {
                        contentManagerData.AssetsId.Add(asset.Id);
                    }
                    sceneData.ContentManagers.Add(contentManagerData);
                }
            }

            #endregion

            #region Assets

            // Save assets data.
            foreach (var asset in Asset.SortedLoadedAssets)
            {
                // Hidden assets are ignored.
                // They carried information only useful for the system to work.
                // Next time you load the scene this information will be (possible) already loaded.
                if (!asset.Hidden)
                {
                    sceneData.Assets.Add(asset);
                    break;
                    /*AssetData assetData = new AssetData();

                    assetData.Id = asset.Id;
                    assetData.Name = asset.Name;
                    assetData.AssetType = asset.GetType();

                    sceneData.Assets.Add(assetData);*/
                }
            }

            #endregion

            return sceneData;
        } // CreateSaveData

        #region Save Scene

        public static void SaveScene()
        {
            // If a save is pending, save as soon as the storage device is chosen.
            IAsyncResult result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
            while (!result.IsCompleted) {}
            StorageDevice device = StorageDevice.EndShowSelector(result);

            // Create the data to save.
            SceneData data = CreateSaveData();

            // Open a storage container.
            result = device.BeginOpenContainer("StorageDemo", null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";

            // Check to see whether the save exists.
            if (container.FileExists(filename))
                // Delete it so that we can create one fresh.
                container.DeleteFile(filename);

            // Create the file.
            Stream stream = container.CreateFile(filename);

            // Convert the object to XML data and put it in the stream.
            XmlSerializer serializer = new XmlSerializer(typeof(SceneData));
            serializer.Serialize(stream, data);

            // Close the file.
            stream.Close();

            // Dispose the container, to commit changes.
            container.Dispose();
            
        } // Save Scene

        #endregion

    } // StorageManager
} // XNAFinalEngine.Storage