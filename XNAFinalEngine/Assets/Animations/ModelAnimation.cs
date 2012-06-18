
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
using XNAFinalEngine.Animations;
using XNAFinalEngineContentPipelineExtensionRuntime.Animations;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Animation.
    /// </summary>
    public class ModelAnimation : Asset
    {

        #region Variables

        private WrapMode wrapMode;

        #endregion

        #region Properties

        /// <summary>
        /// Model Animation Data.
        /// </summary>
        public ModelAnimationClip Resource { get; private set; }

        /// <summary>
        /// Animation duration.
        /// </summary>
        public float Duration { get { return Resource.Duration; } }

        

        /// <summary>
        /// Sets the default wrap mode used in the animation state.
        /// </summary>
        public WrapMode WrapMode
        {
            get
            {
                if (wrapMode == WrapMode.Default)
                    return WrapMode.Once;
                return wrapMode;
            }
            set { wrapMode = value; }
        } // WrapMode
        
        #endregion

        #region Constructor

        /// <summary>
        /// Internal Constructor for File Model assets.
        /// </summary>
        internal ModelAnimation(string name, ModelAnimationClip resource)
        {
            Name = name;
            Resource = resource;
        } // ModelAnimation

        /// <summary>
        /// Load model animation data (rigid or skinned) from a .x or fbx file.
        /// </summary>
        public ModelAnimation(string filename)
        {
            Name = filename;
            Filename = ContentManager.GameDataDirectory + "Animations\\" + filename;
            if (File.Exists(Filename + ".xnb") == false)
            {
                throw new ArgumentException("Failed to load animation data: File " + Filename + " does not exists!", "filename");
            }
            try
            {
                Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<ModelAnimationClip>(Filename);
                ContentManager = ContentManager.CurrentContentManager;
            }
            catch (ObjectDisposedException)
            {
                throw new InvalidOperationException("Content Manager: Content manager disposed");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to load animation data: " + filename, e);
            }
        } // ModelAnimation

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal override void RecreateResource()
        {
            if (ContentManager != null)
                Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<ModelAnimationClip>(Filename);
        } // RecreateResource

        #endregion
        
    } // ModelAnimation
} // XNAFinalEngine.Assets