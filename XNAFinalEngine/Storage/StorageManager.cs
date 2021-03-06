
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
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
#endregion

namespace XNAFinalEngine.Storage
{

    public class ContentManagerData
    {
        public long Id;
        public string Name;
        public List<long> AssetsId = new List<long>();
    } // ContentManagerData

    /// <summary>
    /// Asset data.
    /// </summary>
    public class AssetWithoutResourceData
    {
        public long Id;

        [XmlElement(Type = typeof(AmbientLight))]
        [XmlElement(Type = typeof(HorizonBasedAmbientOcclusion))]
        [XmlElement(Type = typeof(RayMarchingAmbientOcclusion))]
/*        [XmlElement(Type = typeof(BlinnPhong))]
        [XmlElement(Type = typeof(Constant))]
        [XmlElement(Type = typeof(CarPaint))]
        [XmlElement(Type = typeof(PostProcess))]
        [XmlElement(Type = typeof(BasicShadow))]
        [XmlElement(Type = typeof(CascadedShadow))]
        [XmlElement(Type = typeof(Skybox))]
        [XmlElement(Type = typeof(Skydome))]*/
        public AssetWithoutResource Asset;

        public List<string> InternalAssetPropertyNames = new List<string>();
        public List<long> InternalAssetsId = new List<long>();
    } // AssetWithoutResourceData

    public class GameObjectData
    {
        public long Id;
        public string Name;
        public bool is3D;
        public bool Active;
        public int LayerNumber;
        public long ParentId;
        public Matrix LocalMatrix;
        
        // Used in loading.
        public GameObject NewGameObject;

        public List<ComponentData> ComponentData = new List<ComponentData>();
    } // ContentManagerData

    /// <summary>
    /// Asset data.
    /// </summary>
    public class ComponentData
    {
        public long Id;

        [XmlElement(Type = typeof(Camera))]
        public Component Component;

        public List<string> InternalAssetPropertyNames = new List<string>();
        public List<long> InternalAssetsId = new List<long>();
    } // ComponentData
    
    public class SceneData
    {
        public List<ContentManagerData> ContentManagersData = new List<ContentManagerData>();
        public List<AssetWithoutResourceData> AssetsWithoutResourceData = new List<AssetWithoutResourceData>();
        public List<GameObjectData> GameObjectData = new List<GameObjectData>();
    } // SceneData

    /// <summary>
    /// 
    /// </summary>
    public static class StorageManager
    {

        #region Create Save Data

        /// <summary>
        /// Create the structure that will be serialized.
        /// It stores content managers, assets and game objects.
        /// </summary>
        private static SceneData CreateSaveData()
        {
            SceneData sceneData = new SceneData();

            #region Content Managers

            // Save content managers data.
            foreach (var contentManager in AssetContentManager.SortedContentManagers)
            {
                // Hidden content managers are ignored.
                // They carried information only useful for the system to work.
                // Next time you load the scene this information will be (possible) already loaded.
                if (!contentManager.Hidden)
                {
                    ContentManagerData contentManagerData = new ContentManagerData();
                    contentManagerData.Id = contentManager.Id;
                    contentManagerData.Name = contentManager.Name;
                    foreach (var asset in contentManager.Assets)
                    {
                        contentManagerData.AssetsId.Add(asset.Id);
                    }
                    sceneData.ContentManagersData.Add(contentManagerData);
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
                    if (asset is AssetWithoutResource)
                    {
                        AssetWithoutResourceData assetData = new AssetWithoutResourceData
                        {
                            Id = asset.Id,
                            Asset = (AssetWithoutResource)asset,
                        };
                        if (asset is AmbientLight)
                        {
                            if (((AmbientLight)asset).AmbientOcclusion != null)
                            {
                                assetData.InternalAssetsId.Add(((AmbientLight) asset).AmbientOcclusion.Id);
                                assetData.InternalAssetPropertyNames.Add("AmbientOcclusion");
                            }
                        }
                        sceneData.AssetsWithoutResourceData.Add(assetData);    
                    }
                }
            }

            #endregion

            #region Game Objects

            // Save content managers data.
            foreach (var gameObject in GameObject.GameObjects)
            {
                // Hidden content managers are ignored.
                // They carried information only useful for the system to work.
                // Next time you load the scene this information will be (possible) already loaded.
                if (gameObject.Layer.Number != 31 && gameObject.Layer.Number != 30)
                {
                    GameObjectData gameObjectData = new GameObjectData
                    {
                        Id = gameObject.Id,
                        Name = gameObject.Name,
                        is3D = gameObject is GameObject3D,
                        Active = gameObject.Active,
                        LayerNumber = gameObject.Layer.Number
                    };
                    // Parent
                    if (gameObject is GameObject3D)
                    {
                        gameObjectData.ParentId = ((GameObject3D)gameObject).Parent != null ? ((GameObject3D) gameObject).Parent.Id : long.MaxValue;
                        gameObjectData.LocalMatrix = ((GameObject3D)gameObject).Transform.LocalMatrix;
                    }
                    else
                    {
                        gameObjectData.ParentId = ((GameObject2D)gameObject).Parent != null ? ((GameObject2D)gameObject).Parent.Id : long.MaxValue;
                        gameObjectData.LocalMatrix = ((GameObject2D)gameObject).Transform.LocalMatrix;
                    }
                    // Components
                    foreach (var component in gameObject.Components)
                    {
                        ComponentData componentData = new ComponentData
                        {
                            Id = component.Id,
                            Component = component
                        };
                        gameObjectData.ComponentData.Add(componentData);
                    }
                    sceneData.GameObjectData.Add(gameObjectData);
                }
            }

            #endregion

            return sceneData;
        } // CreateSaveData

        #endregion

        #region Load Save Data

        /// <summary>
        /// Create the structure that will be serialized.
        /// It stores content managers, assets and game objects.
        /// </summary>
        private static void LoadSaveData(SceneData sceneData)
        {
            // Generate content managers data.
            foreach (var contentManagerData in sceneData.ContentManagersData)
            {
                // Create Content Manager
                AssetContentManager contentManager = new AssetContentManager { Name = contentManagerData.Name };
                foreach (var assetId in contentManagerData.AssetsId)
                {
                    // The not resourced assets are already created and can change the content manager without recreation.
                    foreach (var assetsWithoutResourceData in sceneData.AssetsWithoutResourceData)
                    {
                        if (assetId == assetsWithoutResourceData.Id)
                        {
                            assetsWithoutResourceData.Asset.ChangeContentManager(contentManager);
                            break;
                        }
                    }
                }
            }
            // Recreate game objects (and the transform component)
            foreach (var gameObjectData in sceneData.GameObjectData)
            {
                GameObject gameObject;
                if (gameObjectData.is3D)
                {
                    gameObject = new GameObject3D();
                    ((GameObject3D)gameObject).Transform.LocalMatrix = gameObjectData.LocalMatrix;
                }
                else
                {
                    gameObject = new GameObject2D();
                    ((GameObject2D)gameObject).Transform.LocalMatrix = gameObjectData.LocalMatrix;
                }
                gameObject.Name = gameObjectData.Name;
                gameObject.Layer = Layer.GetLayerByNumber(gameObjectData.LayerNumber);
                gameObject.Active = gameObjectData.Active;
                gameObjectData.NewGameObject = gameObject; // Use to recreate the hierarchy.
                foreach (var componentData in gameObjectData.ComponentData)
                {
                    // Create the component.
                    // Reflection is needed because we can't know the type in compiler time and I don't want to use an inflexible big switch sentence.
                    gameObject.GetType().GetMethod("AddComponent").MakeGenericMethod(componentData.Component.GetType()).Invoke(gameObject, null);
                    // Each serializable property will be copy to the new component.
                    PropertyInfo componentProperty = gameObject.GetType().GetProperty(componentData.Component.GetType().Name);
                    List<PropertyInfo> propertiesName = GetSerializableProperties(componentData.Component.GetType());
                    for (int i = 0; i < propertiesName.Count; i++)
                    {
                        var property = propertiesName[i];
                        Component component = (Component) componentProperty.GetValue(gameObject, null);
                        Object value = property.GetValue(componentData.Component, null);
                        property.SetValue(component, value, null);
                    }
                }
            }
            // Recreate hierarchy.
            foreach (var gameObjectData in sceneData.GameObjectData)
            {
                // If it is has a parent.
                if (gameObjectData.ParentId != long.MaxValue)
                {
                    // Search all loaded game objects
                    foreach (var searchedGameObjectData in sceneData.GameObjectData)
                    {
                        if (searchedGameObjectData.Id == gameObjectData.ParentId)
                        {
                            if (gameObjectData.is3D)
                                ((GameObject3D)gameObjectData.NewGameObject).Parent = (GameObject3D)searchedGameObjectData.NewGameObject;
                            else
                                ((GameObject2D)gameObjectData.NewGameObject).Parent = (GameObject2D)searchedGameObjectData.NewGameObject;
                            break;
                        }
                    }
                }
            }
        } // CreateSaveData

        /// <summary>
        /// Get the serializable properties of a type.
        /// </summary>
        public static List<PropertyInfo> GetSerializableProperties(Type type)
        {
            List<PropertyInfo> serializableProperties = new List<PropertyInfo>();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.IsDefined(typeof(XmlIgnoreAttribute), false) && property.CanWrite && property.CanRead && !property.GetSetMethod().IsStatic)
                    serializableProperties.Add(property);
            }

            return serializableProperties;
        } // GetSerializableProperties

        #endregion

        #region Save Scene

        public static void SaveScene()
        {
            // If a save is pending, save as soon as the storage device is chosen.
            IAsyncResult result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);

            result.AsyncWaitHandle.WaitOne();
            
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

        #region Clear Scene

        public static void ClearScene()
        {
            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(GameObject.GameObjects);
            foreach (var gameObject in gameObjects)
            {
                // If it is not a reserved game object.
                if (gameObject.Layer != Layer.GetLayerByNumber(30) && gameObject.Layer != Layer.GetLayerByNumber(31))
                {
                    gameObject.Dispose();
                }
            }
            List<AssetContentManager> contentManagers = new List<AssetContentManager>();
            contentManagers.AddRange(AssetContentManager.ContentManagers);
            foreach (var contentManager in contentManagers)
            {
                if (!contentManager.Hidden)
                    contentManager.Dispose();
            }
        }

        #endregion

        #region Load Scene

        /// <summary>
        /// This method loads a serialized data object
        /// from the StorageContainer for this game.
        /// </summary>
        public static void LoadScene()
        {
            // If a save is pending, save as soon as the storage device is chosen.
            IAsyncResult result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);

            result.AsyncWaitHandle.WaitOne();

            StorageDevice device = StorageDevice.EndShowSelector(result);

            // Open a storage container.
            result = device.BeginOpenContainer("StorageDemo", null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";

            // Check to see whether the save exists.
            if (!container.FileExists(filename))
            {
                // If not, dispose of the container and return.
                container.Dispose();
                return;
            }

            // Open the file.
            Stream stream = container.OpenFile(filename, FileMode.Open);

            // Read the data from the file.
            XmlSerializer serializer = new XmlSerializer(typeof(SceneData));
            SceneData data = (SceneData)serializer.Deserialize(stream);

            // Close the file.
            stream.Close();

            // Dispose the container.
            container.Dispose();

            LoadSaveData(data);
        }

        #endregion

    } // StorageManager
} // XNAFinalEngine.Storage