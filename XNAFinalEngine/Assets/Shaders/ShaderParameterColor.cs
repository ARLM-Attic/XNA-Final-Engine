
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
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Shader Parameter.
    /// </summary>
    public class ShaderParameterColor : ShaderParameter
    {

        #region Variables

        private Color lastUsedValue;

        #endregion

        #region Properties

        /// <summary>
        /// Current value.
        /// </summary>
        public Color Value
        {
            get { return lastUsedValue; }
            set
            {
                if (lastUsedValue != value)
                {
                    lastUsedValue = value;
                    Resource.SetValue(new Vector3(value.R / 255f, value.G / 255f, value.B / 255f));
                }
            }
        } // Value

        #endregion

        #region Constructor

        /// <summary>
        /// Shader Parameter for Color type.
        /// </summary>
        public ShaderParameterColor(string name, Shader shader) : base(name, shader)
        {
            Resource.SetValue(new Vector3(Value.R / 255f, Value.G / 255f, Value.B / 255f));
        } // ShaderParameterColor

        #endregion
        
    } // ShaderParameterColor
} // XNAFinalEngine.Assets