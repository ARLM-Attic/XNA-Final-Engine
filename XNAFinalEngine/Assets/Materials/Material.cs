
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
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Base class for materials.
    /// </summary>
    public abstract class Material : Asset
    {

        #region Variables

        // Alpha Blending.
        private float alphaBlending = 1.0f;

        // We only sorted if we need to do it. Don't need to wast time in game mode.
        private static bool loadedMaterialsSorted;

        // Loaded assets of this type.
        private static readonly List<Material> loadedMaterials = new List<Material>();

        // default material.
        private static Material defaultMaterial = new BlinnPhong { Name = "Default Material", DiffuseColor = Color.Gray };

        #endregion

        #region Properties

        #region Name

        /// <summary>
        /// The name of the asset.
        /// If the name already exists then we add one to its name and we call it again.
        /// </summary>
        public override string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    // Is the name unique?
                    bool isUnique = LoadedMaterials.All(assetFromList => assetFromList == this || assetFromList.Name != value);
                    if (isUnique)
                    {
                        name = value;
                        loadedMaterialsSorted = false;

                    }
                    // If not then we add one to its name and find out if is unique.
                    else
                        Name = NamePlusOne(value);
                }
            }
        } // Name

        #endregion

        #region Transparency

        /// <summary>
        /// Alpha Blending.
        /// Default value: 1
        /// </summary>
        public float AlphaBlending
        {
            get { return alphaBlending; }
            set
            {
                alphaBlending = value;
                if (alphaBlending > 1)
                    alphaBlending = 1;
                else if (alphaBlending < 0)
                    alphaBlending = 0;
            }
        } // AlphaBlending

        /// <summary>
        /// Render both sides.
        /// I.e. it manages the culling mode.
        /// Default value: false (CullCounterClockwise)
        /// </summary>
        public bool BothSides { get; set; }

        #endregion

        #region Default Material

        /// <summary>
        /// Default Material.
        /// </summary>
        public static Material DefaultMaterial
        {
            get { return defaultMaterial; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                defaultMaterial = value;
            }
        } // DefaultMaterial

        #endregion

        #region Loaded Materials

        /// <summary>
        /// Loaded materials.
        /// </summary>
        public static List<Material> LoadedMaterials
        {
            get
            {
                if (!loadedMaterialsSorted)
                {
                    loadedMaterialsSorted = true;
                    loadedMaterials.Sort(CompareAssets);
                }
                return loadedMaterials;
            }
        } // LoadedMaterials

        #endregion

        #endregion

        #region Constructor

        protected Material()
        {
            loadedMaterials.Add(this);
            loadedMaterialsSorted = false;
        } // Material

        #endregion

    } // Material
} // XNAFinalEngine.Assets