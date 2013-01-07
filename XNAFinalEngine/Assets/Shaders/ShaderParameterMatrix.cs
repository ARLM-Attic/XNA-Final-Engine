
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
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Shader Parameter.
    /// </summary>
    public class ShaderParameterMatrix : ShaderParameter
    {

        #region Variables

        private Matrix lastUsedValue;

        #endregion

        #region Properties

        /// <summary>
        /// Current value.
        /// </summary>
        public Matrix Value
        {
            get { return lastUsedValue; }
            set
            {
                if (!Equals(ref lastUsedValue, ref value)) // Faster
                //if (lastUsedValue != value)
                {
                    lastUsedValue = value;
                    Resource.SetValue(value);
                }
            }
        } // Value

        #endregion

        #region Constructor

        /// <summary>
        /// Shader Parameter for Matrix type.
        /// </summary>
        public ShaderParameterMatrix(string name, Shader shader) : base(name, shader)
        {
            Resource.SetValue(Value);
        } // ShaderParameterMatrix

        #endregion

        #region Quick Set Value

        /// <summary>
        /// This is faster than Value because the matrix is not copy.
        /// </summary>
        /// <param name="matrix"></param>
        public void QuickSetValue(ref Matrix matrix)
        {
            if (!Equals(ref lastUsedValue, ref matrix)) // Also faster
            {
                lastUsedValue = matrix;
                Resource.SetValue(matrix);
            }
        } // QuickSetValue

        #endregion

        #region Equals

        /// <summary>
        /// Compares two matrix without copy the values to registers.
        /// </summary>
        private static bool Equals(ref Matrix a, ref Matrix b)
        {
            // Kudos to Kris Nye. Probably is better to have this in extension methods.
            // The diagonal is checked first for quick fails.
            return a.M11 == b.M11
            && a.M22 == b.M22
            && a.M33 == b.M33
            && a.M44 == b.M44
            && a.M12 == b.M12
            && a.M13 == b.M13
            && a.M14 == b.M14
            && a.M21 == b.M21
            && a.M23 == b.M23
            && a.M24 == b.M24
            && a.M31 == b.M31
            && a.M32 == b.M32
            && a.M34 == b.M34
            && a.M41 == b.M41
            && a.M42 == b.M42
            && a.M43 == b.M43;
        } // Equals

        #endregion

    } // ShaderParameterMatrix
} // XNAFinalEngine.Assets