
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
using System.Text.RegularExpressions;
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

        // A reference to the content manager that is always loaded. 
        // This content manager is used for load certain assets that are persistent like shaders and some other minor assets.
        // The user can use it as the current content manager but it can’t be unload or dispose.
        private static ContentManager systemContentManager;

        // Current content manager.
        private static ContentManager currentContentManager;

        // Content Manager name.
        private string name;

        #endregion

        #region Properties

        /// <summary>
        /// A reference to the content manager that is always loaded. 
        /// This content manager is used for load certain assets that are persistent like shaders and some other minor assets.
        /// The user can use it as the current content manager but it can’t be unload or dispose.
        /// </summary>
        public static ContentManager SystemContentManager
        {
            get
            {
                if (systemContentManager == null)
                    systemContentManager = new ContentManager("System Content Manager");
                return systemContentManager;
            }
        } // SystemContentManager
        
        /// <summary>
        /// Current content manager. 
        /// It uses the system content by default but you can assign different content managers (for example one for each level)
        /// </summary>
        public static ContentManager CurrentContentManager
        {
            get
            {
                if (currentContentManager == null)
                    return SystemContentManager;
                return currentContentManager;
            }
            set
            {
                currentContentManager = value;
            }
        } // CurrentContent
        
        /// <summary>
        /// The name of the content manager.
        /// If the name already exists then we add one to its name and we call it again.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    // Is the name unique?
                    bool isUnique = ContentManagers.All(contentManagerFromList => contentManagerFromList == this || contentManagerFromList.Name != value);
                    if (isUnique)
                    {
                        name = value;
                        ContentManagers.Sort(CompareContentManagers);
                    }
                    // If not then we add one to its name and find out if is unique.
                    else
                        Name = NamePlusOne(value);
                }
            }
        } // Name
        
        /// <summary>
        /// The XNA Content Manager.
        /// </summary>
        internal Microsoft.Xna.Framework.Content.ContentManager XnaContentManager { get; private set; }

        /// <summary>
        /// Loaded Content Managers
        /// </summary>
        public static List<ContentManager> ContentManagers { get; private set; }

        /// <summary>
        /// The list of loaded assets in this content manager.
        /// </summary>
        public List<Asset> Assets { get; private set; }

        /// <summary>
        /// If the content manager is hidden then the content loaded is inaccessible outside the owner.
        /// </summary>
        public bool Hidden { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The ContentManager manages the loaded assets.
        /// To use it you have to create an instance of this class and set the CurrentContentManager static
        /// property of this class so that references the newly created ContentManager instance. 
        /// All the assets that you load latter will be automatically managed by this content manager.
        /// You can unload or dispose it. In any case the loaded assets will be disposed.
        /// By default the system content manager is the current content manager.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="hidden">If the content manager is hidden then the content loaded is inaccessible outside the owner.</param>
        public ContentManager(string name, bool hidden = false)
        {
            Hidden = hidden;
            XnaContentManager = new Microsoft.Xna.Framework.Content.ContentManager(EngineManager.GameServices);
            Name = name;
            ContentManagers.Add(this);
            ContentManagers.Sort(CompareContentManagers);
            Assets = new List<Asset>();
        } // ContentManager

        static ContentManager()
        {
            ContentManagers = new List<ContentManager>();
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

        #region Name Plus One

        /// <summary>
        /// Return the name plus one.
        /// For example: name will be returned like name1 and name9 will be returned like name10.
        /// </summary>
        protected static string NamePlusOne(string name)
        {
            Regex regex = new Regex(@"(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            Match match = regex.Match(name);

            if (match.Success)
            {
                int numberPlusOne = (int)double.Parse(match.Value) + 1;
                return regex.Replace(name, numberPlusOne.ToString());
            }
            return name + "1";
        } // NamePlusOne

        #endregion

        #region Sort

        /// <summary>
        /// This comparation allows to sort the content managers by their names.
        /// </summary>
        protected static int CompareContentManagers(ContentManager contentManager1, ContentManager contentManager2)
        {
            string x = contentManager1.Name;
            string y = contentManager2.Name;
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    int retval = x.CompareTo(y);
                    //int retval = x.Length.CompareTo(y.Length);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        //
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        //
                        return x.CompareTo(y);
                    }
                }
            }
        } // CompareContentManagers

        #endregion

    } // ContentManager
} // XNAFinalEngine.Assets
