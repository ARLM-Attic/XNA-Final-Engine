
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    public class FontArial12 : Font
    {

        #region Variables

        /// <summary>
        /// La fuente sprite.
        /// </summary>
        protected static SpriteFont spriteFont = null;

        #endregion

        #region Load Resources

        /// <summary>
        /// Load font specific resources.
        /// </summary>
        public static void LoadResources()
        {
            if (spriteFont == null)
            {
                spriteFont = EngineManager.SystemContent.Load<SpriteFont>(Directories.FontsDirectory + "\\" + "Arial12");
            }
        } // LoadResources

        #endregion

        #region Render

        /// <summary>
        /// Render the text in a screen-space position, with a color and a shadow.
        /// </summary>
        public static void Render(string text, Vector2 position, Color color, bool shadow = false, Color shadowColor = new Color())
        {
            LoadResources();
            Font.Render(spriteFont, text, position, color, shadow, shadowColor);
        } // Render

        /// <summary>
        /// Render the text in an Y position, centered in X, with a color and a shadow.
        /// </summary>
        public static void RenderCentered(string text, int position, Color color, bool shadow = false, Color shadowColor = new Color())
        {
            LoadResources();
            Font.RenderCentered(spriteFont, text, position, color, shadow, shadowColor);
        } // RenderCentered

        /// <summary>
        /// Render the text in a position, centered within a rectangle, with a color and a shadow.
        /// </summary>
        public static void RenderCentered(string text, Vector2 position, int width, Color color, bool shadow = false, Color shadowColor = new Color())
        {
            LoadResources();
            Font.RenderCentered(spriteFont, text, position, width, color, shadow, shadowColor);
        } // RenderCentered

        #endregion

    } // FontArial12
} // XNA2FinalEngine.GraphicElements
