
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
using System.Linq;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Base class for assets.
    /// They have a name and can be disposed.
    /// However, some resources could be managed by the XNA content pipeline and the content manager does not allow individual disposes in all type of assets.
    /// If you want to dispose this unmanaged resource use the unload method of the content manager.
    /// </summary>
    public abstract class Asset : Disposable
    {

        #region Variables

        // A simple but effective way of having unique ids.
        // We can have 18.446.744.073.709.551.616 game object creations before the system "collapse". Almost infinite in practice. 
        // If a more robust system is needed (networking/threading) then you can use the guid structure: http://msdn.microsoft.com/en-us/library/system.guid.aspx
        // However this method is slightly simpler, slightly faster and has slightly lower memory requirements.
        // If performance is critical consider the int type (4.294.967.294 unique values).
        private static long uniqueIdCounter = long.MinValue;

        // The asset name.
        protected string name;

        // The content manager that stores this asset.
        private ContentManager contentManager;

        private bool hidden;

        // Loaded assets of this type.
        private static readonly List<Asset> loadedAssets = new List<Asset>();

        // We only sorted if we need to do it. Don't need to wast time in game mode.
        private static bool areLoadedAssetsSorted;

        #endregion

        #region Properties

        /// <summary>
        /// Identification number. Every asset has a unique ID.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Asset Filename (if any).
        /// </summary>
        public string Filename { get; protected set; }

        /// <summary>
        /// The name of the asset.
        /// </summary>
        /// <remarks>
        /// The name is not unique. 
        /// Consequently it can be used to identify the asset, use Id instead.
        /// </remarks>
        public virtual string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrEmpty(value) && name != value)
                {
                    name = value;
                    areLoadedAssetsSorted = false;
                }
            }
        } // Name

        /// <summary>
        /// The content manager that stores this asset.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return contentManager; }
            internal set
            {
                contentManager = value;
                if (value != null)
                    value.Assets.Add(this);
            }
        } // ContentManager
        
        /// <summary>
        /// This is a flag that tells to the editor that it has to be hiding from the user.
        /// Hidden objects are not saved.
        /// </summary>
        /// <remarks>
        /// If the assets is managed by a content manager then the asset has the visibility of it.
        /// </remarks>
        public bool Hidden
        {
            get
            {
                if (ContentManager != null)
                    return ContentManager.Hidden;
                return hidden;
            }
            set { hidden = value; }
        } // Hidden

        #region Loaded Assets

        /// <summary>
        /// Loaded textures.
        /// </summary>
        public static List<Asset> LoadedAssets { get { return loadedAssets; } }

        /// <summary>
        /// Sorted loaded assets list.
        /// If the list is already sorted this operation is O(c).
        /// </summary>
        public static List<Asset> SortedLoadedAssets
        {
            get
            {
                if (!areLoadedAssetsSorted)
                {
                    // The assets are sorted by name.
                    // But they are only sorted when it is needed .
                    // This won't affect game performance, just the editor performance.
                    areLoadedAssetsSorted = true;
                    loadedAssets.Sort(CompareAssets);
                }
                return loadedAssets;
            }
        } // SortedLoadedAssets

        #endregion

        #endregion

        #region Constructor

        protected Asset()
        {
            // Create a unique ID
            Id = uniqueIdCounter;
            uniqueIdCounter++;
            LoadedAssets.Add(this);
            areLoadedAssetsSorted = false;
        } // Asset

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (ContentManager != null)
                throw new InvalidOperationException("Assets loaded with content managers cannot be disposed individually.");
            LoadedAssets.Remove(this);
            areLoadedAssetsSorted = false;
        } // DisposeManagedResources

        #endregion

        #region Search Assets Filename

        /// <summary>
        /// Search for available assets.
        /// </summary>
        protected static string[] SearchAssetsFilename(string directoryPath)
        {
            string[] filenames;
            #if XBOX
                return new string[0];
            #else
                // Search the texture files //
                DirectoryInfo texturesDirectory = new DirectoryInfo(directoryPath);
                try
                {
                    FileInfo[] filesInformation = texturesDirectory.GetFiles("*.xnb", SearchOption.AllDirectories);
                    // Count the textures, except cube textures and user interface textures.
                    filenames = new string[filesInformation.Length];
                    for (int i = 0; i < filesInformation.Length; i++)
                    {
                        FileInfo fileInformation = filesInformation[i];
                        // Some textures are in a sub directory, in that case we have to know how is called.
                        string[] splitDirectoryName = fileInformation.DirectoryName.Split(new[] { directoryPath }, StringSplitOptions.None);
                        string subdirectory = "";
                        // If is in a sub directory
                        if (splitDirectoryName[1] != "")
                        {
                            subdirectory = splitDirectoryName[1].Substring(1, splitDirectoryName[1].Length - 1) + "\\"; // We delete the start \ and add another \ to the end.
                        }
                        filenames[i] = subdirectory + fileInformation.Name.Substring(0, fileInformation.Name.Length - 4);

                    }
                }
                // If there was an error then do nothing.
                catch
                {
                    filenames = new string[0];
                }
                return filenames;
            #endif
        } // SearchAssetsFilename

        #endregion

        #region Sort

        /// <summary>
        /// This comparation allows to sort the assets by their names.
        /// </summary>
        protected static int CompareAssets(Asset asset1, Asset asset2)
        {
            // If they are the same asset then return equals.
            if (asset1 == asset2)
                return 0;

            string x = asset1.Name;
            string y = asset2.Name;
            if (x == null)
            {
                if (y == null)
                    // If x is null and y is null, they're equal. 
                    return 0;
                else
                    // If x is null and y is not null, y is greater. 
                    return -1;
            }
            else
            {
                // If x is not null...
                if (y == null)
                // ...and y is null, x is greater.
                    return 1;
                else
                {
                    // ...and y is not null, compare the two strings.
                    int retval = x.CompareTo(y);

                    if (retval != 0)
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        return retval;
                    else
                    {
                        // Create a new unique name for the second asset and do a comparation again.
                        asset2.SetUniqueName(y);
                        y = asset2.Name;
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        return x.CompareTo(y);
                    }
                }
            }
        } // CompareAssets

        #endregion

        #region Set Unique Name

        /// <summary>
        /// Set a unique texture name.
        /// </summary>
        public void SetUniqueName(string newName)
        {
            // Is the name unique?
            bool isUnique = LoadedAssets.All(assetFromList => assetFromList == this || assetFromList.Name != newName);
            if (isUnique)
            {
                if (name != newName)
                {
                    name = newName;
                    areLoadedAssetsSorted = false;
                }
            }
            // If not then we add one to its name and search again to see if is unique.
            else
                SetUniqueName(newName.PlusOne());
        } // SetUniqueName

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal virtual void RecreateResource()
        {
            // Override if necessary.
        } // RecreateResource

        #endregion

    } // Asset
}  // XNAFinalEngine.Assets