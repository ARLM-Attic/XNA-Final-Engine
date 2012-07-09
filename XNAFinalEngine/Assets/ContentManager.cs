
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
using System.Linq;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// The ContentManager manages the loaded assets.
    /// To use it you have to create an instance of this class and set the CurrentContentManager static
    /// property of this class so that references the newly created ContentManager instance. 
    /// All the assets that you load latter will be automatically managed by this content manager.
    /// You can unload or dispose it. In any case the loaded assets will be disposed.
    /// By default the system content manager is the current content manager.
    /// </summary>
    public class ContentManager : Disposable
    {

        #region Game data directory

        /// <summary>
        /// We can use this to relocate the whole data of the game to another location.
        /// </summary>
        public const string GameDataDirectory = "Content\\";

        #endregion

        #region Variables

        // A simple but effective way of having unique ids.
        // We can have 18.446.744.073.709.551.616 game object creations before the system "collapse". Almost infinite in practice. 
        // If a more robust system is needed (networking/threading) then you can use the guid structure: http://msdn.microsoft.com/en-us/library/system.guid.aspx
        // However this method is slightly simpler, slightly faster and has slightly lower memory requirements.
        // If performance is critical consider the int type (4.294.967.294 unique values).
        private static long uniqueIdCounter = long.MinValue;

        // Content Manager name.
        private string name;

        // A reference to the content manager that is always loaded. 
        // This content manager is used for load certain assets that are persistent like shaders and some other minor assets.
        private static ContentManager systemContentManager;

        // We only sorted if we need to do it. Don't need to wast time in game mode.
        private static bool areContentManagersSorted;

        private static List<ContentManager> contentManagers = new List<ContentManager>();

        #endregion

        #region Properties

        /// <summary>
        /// A reference to the content manager that is always loaded. 
        /// This content manager is used for load certain assets that are persistent like shaders and some other minor assets.
        /// </summary>
        internal static ContentManager SystemContentManager
        {
            get
            {
                if (systemContentManager == null)
                    systemContentManager = new ContentManager { Name = "System Content Manager", Hidden = true };
                return systemContentManager;
            }
        } // SystemContentManager

        /// <summary>
        /// Current content manager. 
        /// It uses the system content by default but you can assign different content managers (for example one for each level)
        /// </summary>
        public static ContentManager CurrentContentManager { get; set; }

        /// <summary>
        /// Identification number. Every asset has a unique ID.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The name of the content manager.
        /// </summary>
        /// <remarks>
        /// The name is not unique. 
        /// Consequently it can be used to identify the content manager, use Id instead.
        /// </remarks>
        public virtual string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrEmpty(value) && name != value)
                {
                    name = value;
                    areContentManagersSorted = false;
                }
            }
        } // Name
        
        /// <summary>
        /// The XNA Content Manager.
        /// </summary>
        internal Microsoft.Xna.Framework.Content.ContentManager XnaContentManager { get; private set; }

        /// <summary>
        /// The list of loaded assets in this content manager.
        /// </summary>
        public List<Asset> Assets { get; private set; }

        /// <summary>
        /// This is a flag that tells to the editor that it has to be hiding from the user.
        /// Hidden objects are not saved.
        /// </summary>
        public bool Hidden { get; set; }

        #region Content Managers

        /// <summary>
        /// Loaded Content Managers.
        /// </summary>
        public static List<ContentManager> ContentManagers { get { return contentManagers; } }

        /// <summary>
        /// Sorted content manager list.
        /// If the list is already sorted this operation is O(c).
        /// </summary>
        public static List<ContentManager> SortedContentManagers
        {
            get
            {
                if (!areContentManagersSorted)
                {
                    // The assets are sorted by name.
                    // But they are only sorted when it is needed .
                    // This won't affect game performance, just the editor performance.
                    areContentManagersSorted = true;
                    ContentManagers.Sort(CompareContentManagers);
                }
                return ContentManagers;
            }
        } // SortedContentManagers

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// The ContentManager manages the loaded assets.
        /// To use it you have to create an instance of this class and set the CurrentContentManager static
        /// property of this class so that references the newly created ContentManager instance. 
        /// All the assets that you load latter will be automatically managed by this content manager.
        /// You can unload or dispose it. In any case the loaded assets will be disposed.
        /// </summary>
        public ContentManager()
        {
            // Create a unique ID
            Id = uniqueIdCounter;
            uniqueIdCounter++;
            XnaContentManager = new Microsoft.Xna.Framework.Content.ContentManager(EngineManager.GameServices);
            Name = "Content Manager";
            ContentManagers.Add(this);
            Assets = new List<Asset>();
            areContentManagersSorted = false;
        } // ContentManager

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (systemContentManager == this)
                throw new InvalidOperationException("Content Manager: System Content Manager can not be disposed.");
            XnaContentManager.Dispose();
            ContentManagers.Remove(this);
            areContentManagersSorted = false;
            // Dispose assets
            foreach (Asset asset in Assets)
            {
                asset.ContentManager = null;
                asset.Dispose();
            }
            Assets.Clear();
        } // DisposeManagedResources

        #endregion

        #region Unload

        /// <summary>
        /// Disposes all data that was loaded by this ContentManager.
        /// </summary>
        public void Unload()
        {
            if (systemContentManager == this)
                throw new InvalidOperationException("Content Manager: System Content Manager can not be unloaded.");
            XnaContentManager.Unload();
            // Dispose assets
            foreach (Asset asset in Assets)
            {
                asset.ContentManager = null;
                asset.Dispose();
            }
            Assets.Clear();
        } // Unload
        
        #endregion

        #region Sort

        /// <summary>
        /// This comparation allows to sort the content managers by their names.
        /// </summary>
        protected static int CompareContentManagers(ContentManager contentManager1, ContentManager contentManager2)
        {
            // If they are the same asset then return equals.
            if (contentManager1 == contentManager2)
                return 0;

            string x = contentManager1.Name;
            string y = contentManager2.Name;
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
                        contentManager2.SetUniqueName(y);
                        y = contentManager2.Name;
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        return x.CompareTo(y);
                    }
                }
            }
        } // CompareContentManagers

        #endregion

        #region Set Unique Name

        /// <summary>
        /// Set a unique texture name.
        /// </summary>
        public void SetUniqueName(string newName)
        {
            // Is the name unique?
            bool isUnique = ContentManagers.All(contentManagerFromList => contentManagerFromList == this || contentManagerFromList.Name != newName);
            if (isUnique)
            {
                if (name != newName)
                {
                    name = newName;
                    areContentManagersSorted = false;
                }
            }
            // If not then we add one to its name and search again to see if is unique.
            else
                SetUniqueName(newName.PlusOne());
        } // SetUniqueName

        #endregion
        
        #region Recreate Content Managers

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal static void RecreateContentManagers()
        {
            ContentManager temp = CurrentContentManager;
            foreach (ContentManager contentManager in ContentManagers)
            {
                CurrentContentManager = contentManager;
                // Unload resources.
                contentManager.XnaContentManager.Unload();
                // Load them again.
                foreach (Asset asset in contentManager.Assets)
                {
                    if (!(asset is Shader))
                        asset.RecreateResource();
                }
                // Some shaders create its own textures, I want that this textures are recreated first.
                foreach (Asset asset in contentManager.Assets)
                {
                    if (asset is Shader)
                        asset.RecreateResource();
                }
            }
            CurrentContentManager = temp;
        } // RecreateContentManagers

        #endregion

    } // ContentManager
} // XNAFinalEngine.Assets
