
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
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Assets;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Base class for renderers.
    /// A renderer is what makes an object appear on the screen.
    /// </summary>
    public class ModelFilter : Component
    {

        #region Variables

        // Model
        private Model model;

        #endregion

        #region Properties

        /// <summary>
        /// Model.
        /// </summary>
        public Model Model
        {
            get { return model;}
            set
            {
                model = value;
                if (ModelChanged != null)
                    ModelChanged(this, model);
            }
        } // Model

        #endregion

        #region Events

        /// <summary>
        /// http://xnafinalengine.codeplex.com/wikipage?title=Improving%20performance&referringTitle=Documentation
        /// </summary>
        internal delegate void ModelEventHandler(object sender, Model model);

        /// <summary>
        /// Raised when the model filter's model changes.
        /// </summary>
        internal event ModelEventHandler ModelChanged;

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
        } // Initialize

        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            Model = null;
            ModelChanged = null;
            // Call this last because the owner information is needed.
            base.Uninitialize();
        } // Uninitialize

        #endregion
        
        #region Pool
        
        // Pool for this type of components.
        private static readonly Pool<ModelFilter> componentPool = new Pool<ModelFilter>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<ModelFilter> ComponentPool { get { return componentPool; } }

        #endregion

    } // ModelFilter
} // XNAFinalEngine.Components
