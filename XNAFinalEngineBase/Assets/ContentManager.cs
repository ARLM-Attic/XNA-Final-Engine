
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
using System;
using Microsoft.Xna.Framework;
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

        /// <summary>
        /// Dummy Services to allow the creation of new content managers.
        /// </summary>
        private static readonly GameServiceContainer dummyServices = new GameServiceContainer();

        /// <summary>
        /// A reference to the content manager that is always loaded. 
        /// This content manager is used for load certain assets that are persistent like shaders and some other minor assets.
        /// The user can use it as the current content manager but it can’t be unload or dispose.
        /// </summary>
        private static readonly ContentManager systemContentManager = new ContentManager();

        /// <summary>
        /// Current content manager.
        /// </summary>
        private static ContentManager currentContentManager;

        #endregion

        #region Properties

        /// <summary>
        /// A reference to the content manager that is always loaded. 
        /// This content manager is used for load certain assets that are persistent like shaders and some other minor assets.
        /// The user can use it as the current content manager but it can’t be unload or dispose.
        /// </summary>
        public static ContentManager SystemContentManager { get { return systemContentManager; } }
        
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
        /// The XNA Content Manager.
        /// </summary>
        internal Microsoft.Xna.Framework.Content.ContentManager XnaContentManager { get; private set; }

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
        public ContentManager()
        {
            XnaContentManager = new Microsoft.Xna.Framework.Content.ContentManager(dummyServices);
        } // ContentManager

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (systemContentManager == this)
            {
                throw new Exception("Content Manager: System Content Manager can not be disposed.");
            }
            XnaContentManager.Dispose();
        } // DisposeManagedResources

        #endregion

        #region Unload

        /// <summary>
        /// Disposes all data that was loaded by this ContentManager.
        /// </summary>
        protected void Unload()
        {
            if (systemContentManager == this)
            {
                throw new Exception("Content Manager: System Content Manager can not be unloaded.");
            }
            XnaContentManager.Unload();
        } // Unload
        
        #endregion

    } // ContentManager
} // XNAFinalEngine.Assets
