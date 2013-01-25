
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Material.
    /// </summary>
    public abstract class Material : AssetWithoutResource
    {

        #region Variables

        // Alpha Blending.
        private float alphaBlending = 1.0f;

        // The mip level id for transitioning between the full computation for parallax occlusion mapping and the bump mapping computation.
        private int parallaxLodThreshold = 2;

        // The minimum number of samples for sampling the height field profile.
        private int parallaxMinimumNumberSamples = 8;

        // The maximum number of samples for sampling the height field profile.
        private int parallaxMaximumNumberSamples = 50;

        // Describes the useful range of values for the height field
        private float parallaxHeightMapScale = 0.02f;

        // Specular Power.
        private float specularPower = 50;

        // Specular Intensity.
        private float specularIntensity = 1.0f;

        // default material.
        private static Material defaultMaterial;

        #endregion

        #region Properties

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

        /// <summary>
        /// A Normal Map is usually used to fake high-res geometry detail when it's mapped onto a low-res mesh.
        /// The pixels of the normal map each store a normal, a vector that describes the surface slope of the original high-res mesh at that point.
        /// </summary>
        public Texture NormalTexture { get; set; }

        #region Parallax

        /// <summary>
        /// Parallax mapping is an enhancement of the bump mapping or normal mapping techniques. 
        /// To the end user, this means that textures such as stone walls will have more apparent depth and thus greater realism 
        /// with less of an influence on the performance of the simulation.
        /// Parallax mapping is implemented by displacing the texture coordinates at a point on the rendered polygon 
        /// by a function of the view angle in tangent space (the angle relative to the surface normal) and
        /// the value of the height map at that point. At steeper view-angles, the texture coordinates are displaced more,
        /// giving the illusion of depth due to parallax effects as the view changes.
        /// </summary>
        /// <remarks>
        /// Parallax Occlusion Mapping was not added but it could be added easily.
        /// If that's the case then I could recommend ignoring the parallax in the GBuffer stage to improve performance.
        /// </remarks>
        /// <seealso cref="http://developer.amd.com/media/gpu_assets/Tatarchuk-ParallaxOcclusionMapping-Sketch-print.pdf"/>
        public bool ParallaxEnabled { get; set; }

        /// <summary>
        /// The mip level id for transitioning between the full computation for parallax occlusion mapping and the bump mapping computation.
        /// </summary>
        public int ParallaxLodThreshold
        {
            get { return parallaxLodThreshold; }
            set { parallaxLodThreshold = value; }
        } // ParallaxLODThreshold

        /// <summary>
        /// The minimum number of samples for sampling the height field profile.
        /// </summary>
        public int ParallaxMinimumNumberSamples
        {
            get { return parallaxMinimumNumberSamples; }
            set
            {
                parallaxMinimumNumberSamples = value;
                if (parallaxMinimumNumberSamples < 0)
                    parallaxMinimumNumberSamples = 0;
                if (parallaxMinimumNumberSamples > parallaxMaximumNumberSamples)
                    parallaxMinimumNumberSamples = parallaxMaximumNumberSamples;
            }
        } // ParallaxMinimumNumberSamples

        /// <summary>
        /// The maximum number of samples for sampling the height field profile.
        /// </summary>
        public int ParallaxMaximumNumberSamples
        {
            get { return parallaxMaximumNumberSamples; }
            set
            {
                parallaxMaximumNumberSamples = value;
                if (parallaxMaximumNumberSamples < parallaxMinimumNumberSamples)
                    parallaxMaximumNumberSamples = parallaxMinimumNumberSamples;
            }
        } // ParallaxMaximumNumberSamples

        /// <summary>
        /// Describes the useful range of values for the height field.
        /// </summary>
        public float ParallaxHeightMapScale
        {
            get { return parallaxHeightMapScale; }
            set { parallaxHeightMapScale = value; }
        } // ParallaxMaximumNumberSamples

        #endregion

        #region Specular

        /// <summary>
        /// Specular Power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPower; }
            set
            {
                specularPower = value;
                if (specularPower < 0)
                    specularPower = 0;
            }
        } // SpecularPower

        /// <summary>
        /// Specular Intensity.
        /// </summary>
        public float SpecularIntensity
        {
            get { return specularIntensity; }
            set
            {
                specularIntensity = value;
                if (specularPower < 0)
                    specularPower = 0;
            }
        } // SpecularIntensity

        /// <summary>
        /// Specular texture. If it's null then the SpecularIntensity and SpecularPower values will be used.
        /// RGB: specular intensity.
        /// A:   specular power.
        /// </summary>
        public Texture SpecularTexture { get; set; }

        /// <summary>
        /// Indicates if the specular power will be read from the texture (the alpha channel of the specular texture) or from the specular power property.
        /// Default value: false
        /// </summary>
        public bool SpecularPowerFromTexture { get; set; }

        /// <summary>
        /// Reflection Texture.
        /// </summary>
        public TextureCube ReflectionTexture { get; set; }

        #endregion

        #region Default Material

        /// <summary>
        /// Default Material.
        /// </summary>
        public static Material DefaultMaterial
        {
            get
            {
                if (defaultMaterial == null)
                {
                    AssetContentManager userContentManager = AssetContentManager.CurrentContentManager;
                    AssetContentManager.CurrentContentManager = AssetContentManager.SystemContentManager;
                    defaultMaterial = new BlinnPhong { Name = "Default Material", DiffuseColor = Color.Gray };
                    AssetContentManager.CurrentContentManager = userContentManager;
                }
                return defaultMaterial;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (defaultMaterial != null && defaultMaterial.ContentManager == AssetContentManager.SystemContentManager)
                    defaultMaterial.Dispose();
                defaultMaterial = value;
            }
        } // DefaultMaterial

        #endregion

        #endregion
       
    } // Material
} // XNAFinalEngine.Assets