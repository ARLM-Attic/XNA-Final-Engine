
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.GraphicElements;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.UI
{
    public abstract class UISlider
    {

        #region Constants

        /// <summary>
        /// Conjunto de constantes que indican el desplazamiento necesario para hubicar los elementos del slider en pantalla.
        /// </summary>
        protected const int leftButtonOffset = 210;
        protected const int valueOffset = 240;
        protected const int rightButtonOffset = 360;

        /// <summary>
        /// Ancho de los elementos del slider.
        /// </summary>
        protected const int buttonWidth = 25;
        protected const int valueWidth = rightButtonOffset - valueOffset;

        #endregion

        #region Variables

        /// <summary>
        /// Los posibles limites del slider
        /// </summary>
        protected float topLimit,
                        bottomLimit;

        /// <summary>
        /// El nombre del slider. Tambien se usara para reconocer el slider en pantalla.
        /// </summary>
        protected string name = "";

        /// <summary>
        /// Texturas que almacenan los botones del slider.
        /// </summary>
        protected XNAFinalEngine.GraphicElements.Texture bottonTexture = null;

        /// <summary>
        /// La posicion de comienzo del slider en pantalla.
        /// </summary>
        protected Vector2 position;
        
        /// <summary>
        /// El porcentaje de hubicacion del manipulador del slider en el mismo.
        /// </summary>
        protected float barPorcent;

        /// <summary>
        /// Indica si se esta manipulando el slider con el mouse.
        /// </summary>
        protected bool sliderActive = false;

        /// <summary>
        /// El paso en el cual avanzara el slider.
        /// </summary>
        protected float step;

        /// <summary>
        /// El valor actual del slider.
        /// </summary>
        protected float currentValue;
                
        #endregion

        #region LoadTextures

        /// <summary>
        /// Carga las texturas necesarias para renderizar el slider.
        /// </summary>
        public void LoadTextures()
        {
            bottonTexture = new XNAFinalEngine.GraphicElements.Texture("UIElements");
        } // LoadTextures

        #endregion

        #region Render

        /// <summary>
        /// Render the slider
        /// </summary>
        public virtual void UpdateAndRender()
        {

            #region Update

            // Actualizamos el manipulador de la barra
            if (sliderActive == false && Mouse.LeftButtonJustPressed && Mouse.Position.X >= (position.X + valueOffset) && Mouse.Position.X <= (position.X + rightButtonOffset) &&
                Mouse.Position.Y >= (position.Y + 15) && Mouse.Position.Y <= (position.Y + 27))
            {
                sliderActive = true;
            }
            if (sliderActive)
            {
                if (!Mouse.LeftButtonPressed)
                    sliderActive = false;
                else
                {
                    barPorcent = (Mouse.Position.X - position.X - valueOffset - 10) / 100;
                    if (barPorcent < 0) barPorcent = 0;
                    if (barPorcent > 1) barPorcent = 1;
                    // Diferencia / paso * porcentaje de barra
                    float steps = (topLimit - bottomLimit) / step * barPorcent;
                    steps = (int)Math.Round(steps, MidpointRounding.ToEven); // Redondeamos para ajustarnos al paso
                    currentValue = bottomLimit + steps * step;
                }
            }
            barPorcent = (currentValue - bottomLimit) * 100 / (topLimit - bottomLimit);
            // Boton izquierdo
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValue -= step;
                    if (currentValue < bottomLimit)
                        currentValue = bottomLimit;
                }
            }
            // Boton Derecho
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValue += step;
                    if (currentValue > topLimit)
                        currentValue = topLimit;
                }
            }
            currentValue = (float)Math.Round(currentValue, 4); // Redondeamos para evitar valor indeseados por el trunqueo entre double y float, Valores como 0.00001

            #endregion

            #region Render

            // Nombre
            FontArial12.Render(name, new Vector2(position.X, position.Y + 6), Color.White);
            // Barra (tiene 100 pixeles de longitud)
            Primitives.Begin(PrimitiveType.LineList);
                Primitives.AddVertex(new Vector2(position.X + valueOffset + 10, position.Y + 23), new Color(100,100,100));
                Primitives.AddVertex(new Vector2(position.X + rightButtonOffset - 10, position.Y + 23), new Color(100, 100, 100));
            Primitives.End();
            // Renderizamos el manipulador de la barra
            Primitives.Begin(PrimitiveType.LineList);
            for (int i = -10; i < 10; i++)
            {
                Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcent + i, position.Y + 20), Color.White);
                Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcent + i, position.Y + 26), new Color(50, 50, 70));
            }
            Primitives.End();
            // Boton izquierdo
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth), new Rectangle(0, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth), new Rectangle(0, 0, buttonWidth, buttonWidth));
            // Boton Derecho
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth), new Rectangle(buttonWidth, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth), new Rectangle(buttonWidth, 0, buttonWidth, buttonWidth));
            
            #endregion

        } // UpdateAndRender

        #endregion

    } // UISlider
} // XNA2FinalEngine.UI
