
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

String extension methods based on the class StringHelper.cs from RacingGame. License: Microsoft_Permissive_License
  
*/
#endregion

#region Using directives
using System;
using System.Globalization;
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

        #region String

        /// <summary>
        /// Is numeric float
        /// </summary>
        public static bool IsNumericFloat(this String str)
        {
            return IsNumericFloat(str, CultureInfo.InvariantCulture.NumberFormat);
        } // IsNumericFloat

        /// <summary>
        /// Checks if string is numeric float value
        /// </summary>
        /// <param name="str">Input string</param>
        /// <param name="numberFormat">Used number format, e.g. CultureInfo.InvariantCulture.NumberFormat</param>
        /// <returns>True if str can be converted to a float, false otherwise</returns>
        public static bool IsNumericFloat(this String str, NumberFormatInfo numberFormat)
        {
            // Can't be a float if string is not valid!
            if (String.IsNullOrEmpty(str))
                return false;

            //not supported by Convert.ToSingle:
            //if (str.EndsWith("f"))
            //	str = str.Substring(0, str.Length - 1);

            // Only 1 decimal point is allowed
            if (AllowOnlyOneDecimalPoint(str, numberFormat) == false)
                return false;

            // + allows in the first,last,don't allow in middle of the string
            // - allows in the first,last,don't allow in middle of the string
            // $ allows in the first,last,don't allow in middle of the string
            // , allows in the last,middle,don't allow in first char of the string
            // . allows in the first,last,middle, allows in all the indexs
            bool retVal = false;

            // If string is just 1 letter, don't allow it to be a sign
            if (str.Length == 1 &&
                "+-$.,".IndexOf(str[0]) >= 0)
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                // For first indexchar
                char pChar =
                    //char.Parse(str.Substring(i, 1));
                    Convert.ToChar(str.Substring(i, 1));

                if (retVal)
                    retVal = false;

                if ((str.IndexOf(pChar) == 0))
                {
                    retVal = ("+-$.0123456789".IndexOf(pChar) >= 0) ? true : false;
                }
                // For middle characters
                if ((!retVal) && (str.IndexOf(pChar) > 0) &&
                    (str.IndexOf(pChar) < (str.Length - 1)))
                {
                    retVal = (",.0123456789".IndexOf(pChar) >= 0) ? true : false;
                }
                // For last characters
                if ((!retVal) && (str.IndexOf(pChar) == (str.Length - 1)))
                {
                    retVal = ("+-$,.0123456789".IndexOf(pChar) >= 0) ? true : false;
                }

                if (!retVal)
                    break;
            }

            return retVal;
        } // IsNumericFloat

        /// <summary>
        /// Check if string is numeric integer. A decimal point is not accepted.
        /// </summary>
        /// <param name="str">String to check</param>
        public static bool IsNumericInt(this string str)
        {
            // Can't be an int if string is not valid!
            if (String.IsNullOrEmpty(str))
                return false;

            // Go through every letter in str
            int strPos = 0;
            foreach (char ch in str)
            {
                // Only 0-9 are allowed
                if ("0123456789".IndexOf(ch) < 0 &&
                    // Allow +/- for first char
                    (strPos > 0 || (ch != '-' && ch != '+')))
                    return false;
                strPos++;
            } // foreach (ch in str)

            // All fine, return true, this is a number!
            return true;
        } // IsNumericInt

        #region Allow Only One Decimal Point

        /// <summary>
        /// Allow only one decimal point, used for IsNumericFloat.
        /// </summary>
        /// <param name="str">Input string to check</param>
        /// <param name="numberFormat">Used number format, e.g. CultureInfo.InvariantCulture.NumberFormat</param>
        /// <return>True if check succeeded, false otherwise</return>
        private static bool AllowOnlyOneDecimalPoint(string str, NumberFormatInfo numberFormat)
        {
            char[] strInChars = str.ToCharArray();
            bool hasGroupSeperator = false;
            int decimalSeperatorCount = 0;
            for (int i = 0; i < strInChars.Length; i++)
            {
                if (numberFormat.CurrencyDecimalSeparator.IndexOf(strInChars[i]) == 0)
                {
                    decimalSeperatorCount++;
                } // if (numberFormat.CurrencyDecimalSeparator.IndexOf)

                // has float group seperators  ?
                if (numberFormat.CurrencyGroupSeparator.IndexOf(strInChars[i]) == 0)
                {
                    hasGroupSeperator = true;
                } // if (numberFormat.CurrencyGroupSeparator.IndexOf)
            } // for (int)

            if (hasGroupSeperator)
            {
                // If first digit is the group seperator or begins with 0,
                // there is something wrong, the group seperator is used as a comma.
                if (str.StartsWith(numberFormat.CurrencyGroupSeparator) ||
                    strInChars[0] == '0')
                    return false;

                // look only at the digits in front of the decimal point
                string[] splittedByDecimalSeperator = str.Split(
                    numberFormat.CurrencyDecimalSeparator.ToCharArray());

                #region Invert the digits for modulo check
                //   ==> 1.000 -> 000.1  ==> only after 3 digits 
                char[] firstSplittedInChars = splittedByDecimalSeperator[0].ToCharArray();
                int arrayLength = firstSplittedInChars.Length;
                char[] firstSplittedInCharsInverted = new char[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    firstSplittedInCharsInverted[i] =
                        firstSplittedInChars[arrayLength - 1 - i];
                } // for (int)
                #endregion

                // group seperators are only allowed between 3 digits -> 1.000.000
                for (int i = 0; i < arrayLength; i++)
                {
                    if (i % 3 != 0 && numberFormat.CurrencyGroupSeparator.IndexOf(
                        firstSplittedInCharsInverted[i]) == 0)
                    {
                        return false;
                    } // if (i)
                } // for (int)
            } // if (hasGroupSeperator)
            if (decimalSeperatorCount > 1)
                return false;

            return true;
        } // AllowOnlyOneDecimalPoint

        #endregion

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