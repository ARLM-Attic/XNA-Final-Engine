
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
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.Assets
{
    /// <summary>
    /// Font.
    /// </summary>
    public class Font : Asset
    {

        #region Variables

        // default material.
        private static Font defaultFont;

        #endregion

        #region Properties

        /// <summary>
        /// Default Font.
        /// </summary>
        public static Font DefaultFont
        {
            get { return defaultFont; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                defaultFont = value;
            }
        } // DefaultFont

        /// <summary>
        /// XNA Sprite font.
        /// </summary>
        public SpriteFont Resource { get; private set; }

        /// <summary>
        /// Gets or sets the vertical distance (in pixels) between the base lines of two consecutive lines of text.
        /// Line spacing includes the blank space between lines as well as the height of the characters.
        /// </summary>
        public int LineSpacing
        {
            get { return Resource.LineSpacing; }
            set { Resource.LineSpacing = value; }
        } // LineSpacing

        /// <summary>
        /// Gets or sets the spacing of the font characters.
        /// </summary>
        public float Spacing
        {
            get { return Resource.Spacing; }
            set { Resource.Spacing = value; }
        } // Spacing

        /// <summary>
        /// Gets a collection of all the characters that are included in the font.
        /// </summary>
        public ReadOnlyCollection<char> Characters { get { return Resource.Characters; } }

        /// <summary>
        /// Gets or sets the default character for the font.
        /// </summary>
        public char? DefaultCharacter
        {
            get { return Resource.DefaultCharacter; }
            set { Resource.DefaultCharacter = value; }
        } // DefaultCharacter

        #endregion

        #region Constructor

        /// <summary>
        /// Load font.
        /// </summary>
        /// <param name="filename">The filename must be relative and be a valid file in the font directory.</param>
        public Font(string filename)
        {
            Name = filename;
            Filename = ContentManager.GameDataDirectory + "Fonts\\" + filename;
            if (File.Exists(Filename + ".xnb") == false)
            {
                throw new Exception("Failed to load font: File " + Filename + " does not exists!");
            }
            try
            {
                Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<SpriteFont>(Filename);
                ContentManager = ContentManager.CurrentContentManager;
            }
            catch (ObjectDisposedException e)
            {
                throw new Exception("Content Manager: Content manager disposed", e);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load font: " + filename, e);
            }
        } // Font

        /// <summary>
        /// This execute the first time a font is required.
        /// </summary>
        static Font()
        {
            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            defaultFont = new Font("Default");
            ContentManager.CurrentContentManager = userContentManager;
        } // Font

        #endregion

        #region Measure String

        /// <summary>
        /// Returns the width and height of a string as a Vector2.
        /// </summary>
        public Vector2 MeasureString(string text)
        {
            return Resource.MeasureString(text);
        } // MeasureString

        /// <summary>
        /// Returns the width and height of a string as a Vector2.
        /// http://msdn.microsoft.com/en-us/library/system.text.stringbuilder.aspx
        /// StringBuilder is good for avoid garbage.
        /// </summary>
        public Vector2 MeasureString(StringBuilder text)
        {
            return Resource.MeasureString(text);
        } // MeasureString

        #endregion

        #region Recreate Resource

        /// <summary>
        /// Useful when the XNA device is disposed.
        /// </summary>
        internal override void RecreateResource()
        {
            Resource = ContentManager.CurrentContentManager.XnaContentManager.Load<SpriteFont>(Filename);
        } // RecreateResource

        #endregion

    } // Font
} // XNAFinalEngineBase.Assets
