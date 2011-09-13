
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
Authors: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
#endregion

namespace XNAFinalEngine.Helpers
{

    #region Enumerates

    /// <summary>
    /// Relative size to screen and shadow sizes.
    /// </summary>
    [Flags]
    public enum SizeType
    {
        /// <summary>
        /// Full screen size.
        /// </summary>
        FullScreen,
        /// <summary>
        /// Half the full screen size, e.g. 800x600 becomes 400x300
        /// </summary>
        HalfScreen,
        /// <summary>
        /// Quarter of the full screen size, e.g. 800x600 becomes 200x150
        /// </summary>
        QuarterScreen,
        /// <summary>
        /// 256 x 256 pixels. Good for shadows.
        /// </summary>
        Square256X256,
        /// <summary>
        /// 512 x 512 pixels. Good for shadows.
        /// </summary>
        Square512X512,
        /// <summary>
        /// 1024 x 1024 pixels. Good for shadows.
        /// </summary>
        Square1024X1024,
        /// <summary>
        /// 2048 x 2048 pixels. Good for shadows.
        /// </summary>
        Square2048X2048,
    } // SizeType

    #endregion

    /// <summary>
    /// Stores an ordered pair of integers, which specify a Height and Width.
    /// </summary>
    public struct Size
    {        

        #region Variables

        private int width, height;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the horizontal component of this Size structure.
        /// </summary>
        public int Width { get { return width; } set { width = value; } }
                
        /// <summary>
        /// Gets or sets the vertical component of this Size structure.
        /// </summary>
        public int Height { get { return height; } set { height = value; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Stores an ordered pair of integers, which specify a Height and Width.
        /// </summary>
        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        } // Size

        #endregion

        #region Equal
        
        public static bool operator ==(Size x, Size y)
        {
            return x.width == y.width && x.height == y.height;
        } // Equal

        public static bool operator !=(Size x, Size y)
        {
            return x.width != y.width || x.height != y.height;
        } // Not Equal

        public override bool Equals(System.Object obj)
        {
            return obj is Size && this == (Size)obj;
        } // Equals

        public override int GetHashCode()
        {
            return width.GetHashCode() ^ height.GetHashCode();
        } // GetHashCode
        
        #endregion

        #region Calculate Size

        /// <summary>
        /// Calculate size from size type.
        /// </summary>
        private static Size CalculateSize(SizeType sizeType)
        {
            int width;
            int height;
            switch (sizeType)
            {
                case SizeType.FullScreen:
                    width = SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth;
                    height = SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight;
                    break;
                case SizeType.HalfScreen:
                    width = SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth / 2;
                    height = SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight / 2;
                    break;
                case SizeType.QuarterScreen:
                    width = SystemInformation.GraphicsDeviceManager.PreferredBackBufferWidth / 4;
                    height = SystemInformation.GraphicsDeviceManager.PreferredBackBufferHeight / 4;
                    break;
                case SizeType.Square256X256:
                    width = 256;
                    height = 256;
                    break;
                case SizeType.Square512X512:
                    width = 512;
                    height = 512;
                    break;
                case SizeType.Square1024X1024:
                    width = 1024;
                    height = 1024;
                    break;
                case SizeType.Square2048X2048:
                    width = 1024;
                    height = 1024;
                    break;
                default:
                    throw new ArgumentException("Size type does not exist (probably a bug).");
            }
            return new Size(width, height);
        } // CalculateSize

        #endregion

    } // Size 
} // XNAFinalEngine.Helpers
