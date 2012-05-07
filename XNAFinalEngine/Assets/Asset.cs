
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
using System.IO;
using System.Text.RegularExpressions;
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

        // The asset name.
        protected string name;

        // The content manager that stores this asset.
        private ContentManager contentManager;

        #endregion

        #region Properties

        /// <summary>
        /// Asset Filename.
        /// </summary>
        public string Filename { get; protected set; }

        /// <summary>
        /// The name of the asset.
        /// </summary>
        public virtual string Name { get; set; } // TODO: change to abstract.
        
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

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (ContentManager != null)
                throw new InvalidOperationException("Assets loaded with content managers cannot be disposed individually.");
        } // DisposeManagedResources

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
                int numberPlusOne = (int) double.Parse(match.Value) + 1;
                return regex.Replace(name, numberPlusOne.ToString());
            }
            return name + "1";
        } // NamePlusOne

        #endregion

        #region Search Assets Filename

        /// <summary>
        /// Search for available assets.
        /// </summary>
        protected static string[] SearchAssetsFilename(string directoryPath)
        {
            string[] filenames;
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
        } // SearchAssetsFilename

        #endregion

        #region Sort

        /// <summary>
        /// This comparation allows to sort the assets by their names.
        /// </summary>
        protected static int CompareAssets(Asset asset1, Asset asset2)
        {
            string x = asset1.Name;
            string y = asset2.Name;
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
        } // CompareAssets

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