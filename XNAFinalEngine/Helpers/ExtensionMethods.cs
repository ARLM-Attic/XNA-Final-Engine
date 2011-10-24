
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
Authors: Digital Jellyfish Design Ltd (http://forums.create.msdn.com/forums/p/16395/132030.aspx)
         deadlydog (http://www.danskingdom.com)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Helpers
{
    /// <summary> 
    /// Class used to extend other classes. 
    /// </summary> 
    public static class ExtensionMethods
    {

        #region List

        #if (XBOX)

        /// <summary> 
        /// Removes all elements from the List that match the conditions defined by the specified predicate. 
        /// </summary> 
        /// <typeparam name="T">The type of elements held by the List.</typeparam> 
        /// <param name="list">The List to remove the elements from.</param> 
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param> 
        public static int RemoveAll<T>(this System.Collections.Generic.List<T> list, Predicate<T> match)
        {
            int numberRemoved = 0;

            // Loop through every element in the List, in reverse order since we are removing items. 
            for (int i = (list.Count - 1); i >= 0; i--)
            {
                // If the predicate function returns true for this item, remove it from the List. 
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                    numberRemoved++;
                }
            }

            // Return how many items were removed from the List. 
            return numberRemoved;
        } // RemoveAll

        /// <summary> 
        /// Returns true if the List contains elements that match the conditions defined by the specified predicate. 
        /// </summary> 
        /// <typeparam name="T">The type of elements held by the List.</typeparam> 
        /// <param name="list">The List to search for a match in.</param> 
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to match against.</param> 
        public static bool Exists<T>(this System.Collections.Generic.List<T> list, Func<T, bool> match)
        {
            // Loop through every element in the List, until a match is found. 
            for (int i = 0; i < list.Count; i++)
            {
                // If the predicate function returns true for this item, return that at least one match was found. 
                if (match(list[i]))
                    return true;
            }

            // Return that no matching elements were found in the List. 
            return false;
        } // Exists

        #endif

        #endregion

        #region String Builder

        private static readonly char[] digits = new[] { '9', '8', '7', '6', '5', '4', '3', '2', '1', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, int number)
        {
            if (number < 0)
            {
                stringBuilder.Append('-');
            }

            int index = stringBuilder.Length;
            do
            {
                // StringBuilder.Insert(Int32, Char) calls ToString() internally
                // http://www.gavpugh.com/2010/04/01/xnac-avoiding-garbage-when-working-with-stringbuilder/
                stringBuilder.Insert(index, digits, (number % 10) + 9, 1);
                number /= 10;
            }
            while (number != 0);

            return stringBuilder;
        } // AppendWithoutGarbage

        #endregion

        #region Quaternion
        
        /// <summary>
        /// Return yaw, pitch and roll from this quaternion.
        /// </summary>
        /// <returns>X = yaw, Y = pitch, Z = roll</returns>
        public static Vector3 GetYawPitchRoll(this Quaternion quaternion)
        {
            Vector3 yawPitchRoll = new Vector3
            {
                X = (float)(Math.Asin(-2 * (quaternion.X * quaternion.Z - quaternion.W * quaternion.Y))),
                Y = (float)(Math.Atan2(2 * (quaternion.Y * quaternion.Z + quaternion.W * quaternion.X),
                                            quaternion.W * quaternion.W - quaternion.X * quaternion.X -
                                            quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z)),
                Z = (float)(Math.Atan2(2 * (quaternion.X * quaternion.Y + quaternion.W * quaternion.Z),
                                            quaternion.W * quaternion.W + quaternion.X * quaternion.X -
                                            quaternion.Y * quaternion.Y - quaternion.Z * quaternion.Z))
            };
            return yawPitchRoll;
        } // GetYawPitchRoll

        #endregion

    } // ExtensionMethods
} // XNAFinalEngineBase.Helpers