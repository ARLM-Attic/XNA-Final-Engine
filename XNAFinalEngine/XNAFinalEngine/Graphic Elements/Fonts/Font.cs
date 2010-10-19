
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
#endregion

namespace XNAFinalEngine.GraphicElements
{
    public abstract class Font
    {

        #region Variables

        /// <summary>
        /// El manejador de sprites de las fuentes. Podria ser conveniente implementar un manejador como el de sprites.
        /// Pero si el desempeño no se ve muy influido con este esquema el codigo resulta levemente mas limpio.
        /// </summary>
        protected static SpriteBatch spritebatch = null;

        #endregion

        #region Render

        /// <summary>
        /// Render the text in this position and with this color. Y adicionalmente renderizar renderizar una sombra
        /// con el color especificado.
        /// </summary>
        protected static void Render(SpriteFont spriteFont, string text, Vector2 position, Color color, bool shadow, Color shadowColor)
        {
            if (spritebatch == null)
            {
                spritebatch = new SpriteBatch(EngineManager.Device);
            }
            spritebatch.Begin();
                if (shadow)
                {
                    spritebatch.DrawString(spriteFont, text, new Vector2(position.X + 1, position.Y + 1), shadowColor);
                }
                spritebatch.DrawString(spriteFont, text, position, color);
            spritebatch.End(); 
        } // Render

        /// <summary>
        /// Render the text in this Y position, centered in the screen, and with this color. Y adicionalmente renderizar renderizar una sombra
        /// con el color especificado.
        /// </summary>
        protected static void RenderCentered(SpriteFont spriteFont, string text, int position, Color color, bool shadow, Color shadowColor)
        {
            if (spritebatch == null)
            {
                spritebatch = new SpriteBatch(EngineManager.Device);
            }
            float middleScreen = EngineManager.Device.Viewport.Width / 2;
            float stringLenght = spriteFont.MeasureString(text).X / 2;
            spritebatch.Begin();
            if (shadow)
            {
                spritebatch.DrawString(spriteFont, text, new Vector2(middleScreen - stringLenght + 2, position + 2), shadowColor);
            }
            spritebatch.DrawString(spriteFont, text, new Vector2(middleScreen - stringLenght, position), color);
            spritebatch.End();
        } // RenderCentered

        /// <summary>
        /// Render the text in this position and with this color. El texto se centrara en un rectangulo generado por la posicion y la variable ancho.
        /// Y adicionalmente renderizar renderizar una sombra con el color especificado.
        /// </summary>
        protected static void RenderCentered(SpriteFont spriteFont, string text, Vector2 position, int width, Color color, bool shadow, Color shadowColor)
        {
            if (spritebatch == null)
            {
                spritebatch = new SpriteBatch(EngineManager.Device);
            }
            float stringLenght = spriteFont.MeasureString(text).X / 2;
            spritebatch.Begin();
            if (shadow)
            {
                spritebatch.DrawString(spriteFont, text, new Vector2(position.X + (width / 2) - stringLenght + 2, position.Y + 2), shadowColor);
            }
            spritebatch.DrawString(spriteFont, text, new Vector2(position.X + (width / 2) - stringLenght, position.Y), color);
            spritebatch.End();
        } // RenderCentered

        #endregion
        
    } // Font
} // XNA2FinalEngine.GraphicElements
