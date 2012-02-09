
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
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// Blinn Phong Material.
    /// Accept a diffuse color or a diffuse texture, but not both.
    /// It has specular highlight that can be controlled with power (shininess) and intensity.
    /// There is also normal mapping, parallax mapping and reflections.
    /// </summary>
    public class BlinnPhong : Material
    {

        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;
        
        // Surface diffuse color. If a diffuse texture exists this color will be ignored.
        private Color diffuseColor = Color.Gray;
        
        // Specular Power.
        private float specularPower = 50;
        
        // Specular Intensity.
        private float specularIntensity = 1.0f;
        
        // The mip level id for transitioning between the full computation for parallax occlusion mapping and the bump mapping computation. 
        private int parallaxLodThreshold = 2;
        
        // The minimum number of samples for sampling the height field profile.
        private int parallaxMinimumNumberSamples = 8;
        
        // The maximum number of samples for sampling the height field profile.
        private int parallaxMaximumNumberSamples = 50;

        // Describes the useful range of values for the height field
        private float parallaxHeightMapScale = 0.02f;

        #endregion

        #region Properties

        #region Diffuse

        /// <summary>
        /// Surface diffuse color. If a diffuse texture exists this color will be ignored.
        /// </summary>
        public Color DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        } // DiffuseColor

        /// <summary>
        /// Diffuse texture. If it's null then the DiffuseColor value will be used.
        /// </summary>
        public Texture DiffuseTexture { get; set; }

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

        #region Normals

        /// <summary>
        /// Normal map texture.
        /// </summary>
        public Texture NormalTexture { get; set; }

        #endregion

        #region Parallax

        /// <summary>
        /// Is parallax enabled? The Height Map is stored in Normal Map's alpha channel.
        /// </summary>
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
        
        #endregion

        #region Constructor

        public BlinnPhong()
        {
            Name = "Bling Phong-" + nameNumber;
            nameNumber++;
        } // BlinnPhong

        #endregion

    } // BlinnPhong
} // XNAFinalEngine.Assets

