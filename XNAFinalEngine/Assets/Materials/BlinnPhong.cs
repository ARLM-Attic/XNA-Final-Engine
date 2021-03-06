
#region License
/*
Copyright (c) 2008-2013, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Assets
{

    /// <summary>
    /// The Blinn�Phong shading model is a modification to the Phong reflection model developed by Jim Blinn 
    /// that performs the specular calculations using the half vector instead of the reflection vector.
    /// This is a cheap BRDF that performs well in the majority of the scenarios.
    /// 
    /// The engine implements a deferred lighting pipeline that calculates a partial BRDF in the light pre pass.
    /// The BRDF implemented is Blinn-Phong. Oren Nayar, Cook Torrance and Ward are other good options, but no one is quicker than Blinn Phong.
    /// It�s up to you the selection of the BRDF to implement in your game, but be aware of the parameters needed in the G-Buffer.
    /// 
    /// If you need a particular BRDF to apply in just a small amount of objects then you can always run the shader with the custom BRDF in forward mode.
    /// </summary>
    public class BlinnPhong : Material
    {

        #region Variables

        // The count of materials for naming purposes.
        private static int nameNumber = 1;

        // Default value.
        private Color diffuseColor = Color.Gray; 

        #endregion

        #region Properties

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

        #region Constructor

        public BlinnPhong()
        {
            Name = "Blinn Phong-" + nameNumber;
            nameNumber++;
        } // BlinnPhong

        #endregion

    } // BlinnPhong
} // XNAFinalEngine.Assets

