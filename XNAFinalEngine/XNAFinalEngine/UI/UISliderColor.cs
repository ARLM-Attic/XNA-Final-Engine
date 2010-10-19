
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
using Keys = Microsoft.Xna.Framework.Input.Keys;
#endregion

namespace XNAFinalEngine.UI
{
    public class UISliderColor : UISlider
    {

        #region Variables

        /// <summary>
        /// El valor actual del slider de color, separados en cada canal de color.
        /// </summary>
        protected float currentValueRed,
                        currentValueGreen,
                        currentValueBlue;

        /// <summary>
        /// El porcentaje de hubicacion del manipulador del slider en el mismo.
        /// </summary>
        protected float barPorcentRed,
                        barPorcentGreen,
                        barPorcentBlue;

        /// <summary>
        /// Indica si se esta manipulando el slider con el mouse.
        /// </summary>
        protected bool sliderActiveRed = false,
                       sliderActiveGreen = false,
                       sliderActiveBlue = false;

        #endregion

        #region Properties

        /// <summary>
        /// El valor actual del slider.
        /// </summary>
        public Color CurrentValue
        {
            get { return new Color(currentValueRed, currentValueGreen, currentValueBlue); }
            set
            {
                currentValueRed = (float)(value.R) / 255.0f;
                currentValueGreen = (float)(value.G) / 255.0f;
                currentValueBlue = (float)(value.B) / 255.0f;
            }
        } // CurrentValue

        #endregion

        #region Constructor

        /// <summary>
        /// Slider para colores.
        /// </summary>
        public UISliderColor(string _name, Vector2 _position, Color firstValue)
        {
            name = _name;
            position = _position;
            currentValueRed = (float)(firstValue.R) / 255.0f;
            currentValueGreen = (float)(firstValue.G) / 255.0f;
            currentValueBlue = (float)(firstValue.B) / 255.0f;
            bottomLimit = 0.0f;
            topLimit = 1.0f;
            step = 0.004f;
            LoadTextures();
        } // UISliderColor

        #endregion

        #region Update and Render

        /// <summary>
        /// Render the slider
        /// </summary>
        public override void UpdateAndRender()
        {

            #region Update Red

            // Actualizamos el manipulador de la barra
            if (Mouse.LeftButtonJustPressed && Mouse.Position.X >= (position.X + valueOffset) && Mouse.Position.X <= (position.X + rightButtonOffset) &&
                Mouse.Position.Y >= (position.Y + 0) && Mouse.Position.Y <= (position.Y + 6))
            {
                sliderActiveRed = true;
            }
            if (sliderActiveRed)
            {
                if (Keyboard.KeyPressed(Keys.LeftControl))
                {
                    sliderActiveRed = true;
                    sliderActiveGreen = true;
                    sliderActiveBlue = true;
                }
                if (!Mouse.LeftButtonPressed)
                {
                    sliderActiveRed = false;
                    sliderActiveGreen = false;
                    sliderActiveBlue = false;
                }
                else
                {
                    barPorcentRed = (Mouse.Position.X - position.X - valueOffset - 10) / 100;
                    if (barPorcentRed < 0) barPorcentRed = 0;
                    if (barPorcentRed > 1) barPorcentRed = 1;
                    // Diferencia / paso * porcentaje de barra
                    float steps = (topLimit - bottomLimit) / step * barPorcentRed;
                    steps = (int)Math.Round(steps, MidpointRounding.ToEven); // Redondeamos para ajustarnos al paso
                    currentValueRed = bottomLimit + steps * step;
                }
            }
            barPorcentRed = (currentValueRed - bottomLimit) * 100 / (topLimit - bottomLimit);
            // Boton izquierdo
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValueRed -= step;
                    if (currentValueRed < bottomLimit)
                        currentValueRed = bottomLimit;
                }
            }
            // Boton Derecho
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValueRed += step;
                    if (currentValueRed > topLimit)
                        currentValueRed = topLimit;
                }
            }
            currentValueRed = (float)Math.Round(currentValueRed, 4); // Redondeamos para evitar valor indeseados por el trunqueo entre double y float, Valores como 0.00001

            #endregion

            #region Update Green

            // Actualizamos el manipulador de la barra
            if (Mouse.LeftButtonJustPressed && Mouse.Position.X >= (position.X + valueOffset) && Mouse.Position.X <= (position.X + rightButtonOffset) &&
                Mouse.Position.Y >= (position.Y + 10) && Mouse.Position.Y <= (position.Y + 16))
            {
                sliderActiveGreen = true;
            }
            if (sliderActiveGreen)
            {
                if (Keyboard.KeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                {
                    sliderActiveRed = true;
                    sliderActiveGreen = true;
                    sliderActiveBlue = true;
                }
                if (!Mouse.LeftButtonPressed)
                {
                    sliderActiveRed = false;
                    sliderActiveGreen = false;
                    sliderActiveBlue = false;
                }
                else
                {
                    barPorcentGreen = (Mouse.Position.X - position.X - valueOffset - 10) / 100;
                    if (barPorcentGreen < 0) barPorcentGreen = 0;
                    if (barPorcentGreen > 1) barPorcentGreen = 1;
                    // Diferencia / paso * porcentaje de barra
                    float steps = (topLimit - bottomLimit) / step * barPorcentGreen;
                    steps = (int)Math.Round(steps, MidpointRounding.ToEven); // Redondeamos para ajustarnos al paso
                    currentValueGreen = bottomLimit + steps * step;
                }
            }
            barPorcentGreen = (currentValueGreen - bottomLimit) * 100 / (topLimit - bottomLimit);
            // Boton izquierdo
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValueGreen -= step;
                    if (currentValueGreen < bottomLimit)
                        currentValueGreen = bottomLimit;
                }
            }
            // Boton Derecho
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValueGreen += step;
                    if (currentValueGreen > topLimit)
                        currentValueGreen = topLimit;
                }
            }
            currentValueGreen = (float)Math.Round(currentValueGreen, 4); // Redondeamos para evitar valor indeseados por el trunqueo entre double y float, Valores como 0.00001

            #endregion

            #region Update Blue
            
            // Actualizamos el manipulador de la barra
            if (Mouse.LeftButtonJustPressed && Mouse.Position.X >= (position.X + valueOffset) && Mouse.Position.X <= (position.X + rightButtonOffset) &&
                Mouse.Position.Y >= (position.Y + 20) && Mouse.Position.Y <= (position.Y + 26))
            {
                sliderActiveBlue = true;
            }
            if (sliderActiveBlue)
            {
                if (Keyboard.KeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                {
                    sliderActiveRed = true;
                    sliderActiveGreen = true;
                    sliderActiveBlue = true;
                }
                if (!Mouse.LeftButtonPressed)
                {
                    sliderActiveRed = false;
                    sliderActiveGreen = false;
                    sliderActiveBlue = false;
                }
                else
                {
                    barPorcentBlue = (Mouse.Position.X - position.X - valueOffset - 10) / 100;
                    if (barPorcentBlue < 0) barPorcentBlue = 0;
                    if (barPorcentBlue > 1) barPorcentBlue = 1;
                    // Diferencia / paso * porcentaje de barra
                    float steps = (topLimit - bottomLimit) / step * barPorcentBlue;
                    steps = (int)Math.Round(steps, MidpointRounding.ToEven); // Redondeamos para ajustarnos al paso
                    currentValueBlue = bottomLimit + steps * step;
                }
            }
            barPorcentBlue = (currentValueBlue - bottomLimit) * 100 / (topLimit - bottomLimit);
            // Boton izquierdo
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValueBlue -= step;
                    if (currentValueBlue < bottomLimit)
                        currentValueBlue = bottomLimit;
                }
            }
            // Boton Derecho
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3)))
            {
                if (Mouse.LeftButtonJustPressed)
                {
                    currentValueBlue += step;
                    if (currentValueBlue > topLimit)
                        currentValueBlue = topLimit;
                }
            }
            currentValueBlue = (float)Math.Round(currentValueBlue, 4); // Redondeamos para evitar valor indeseados por el trunqueo entre double y float, Valores como 0.00001

            #endregion

            #region Render

            // Nombre
            FontArial12.Render(name, new Vector2(position.X, position.Y), Color.White);
            FontArial8.Render("( R: " + Math.Round(currentValueRed, 2).ToString() + " G: " + Math.Round(currentValueGreen, 2).ToString() + " B: " + Math.Round(currentValueBlue, 2).ToString() + " )", new Vector2(position.X, position.Y + 17), Color.White);
            // Panel de color (TODO!!! Hacer un plano 2D en primitivas 2D)
            Primitives.Begin(PrimitiveType.LineList);
                for (int i = -15; i < 15; i++)
                {
                    Primitives.AddVertex(new Vector2(position.X + valueOffset - 42  + i, position.Y + 2), new Color(currentValueRed, currentValueGreen, currentValueBlue));
                    Primitives.AddVertex(new Vector2(position.X + valueOffset - 42 + i, position.Y + 25), new Color(currentValueRed, currentValueGreen, currentValueBlue));
                }
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 59, position.Y + 1), Color.White);
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 59, position.Y + 26), Color.Gray);
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 26, position.Y + 1), Color.White);
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 26, position.Y + 26), Color.Gray);
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 59, position.Y + 1), Color.White);
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 26, position.Y + 1), Color.White);
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 59, position.Y + 26), Color.Gray);
                Primitives.AddVertex(new Vector2(position.X + valueOffset - 26, position.Y + 26), Color.Gray);
            Primitives.End();
            // Barra (tiene 100 pixeles de longitud)
            Primitives.Begin(PrimitiveType.LineList);
                Primitives.AddVertex(new Vector2(position.X + valueOffset + 10, position.Y + 3), new Color(100, 100, 100));
                Primitives.AddVertex(new Vector2(position.X + rightButtonOffset - 10, position.Y + 3), Color.Red);
                Primitives.AddVertex(new Vector2(position.X + valueOffset + 10, position.Y + 13), new Color(100, 100, 100));
                Primitives.AddVertex(new Vector2(position.X + rightButtonOffset - 10, position.Y + 13), Color.Green);
                Primitives.AddVertex(new Vector2(position.X + valueOffset + 10, position.Y + 23), new Color(100, 100, 100));
                Primitives.AddVertex(new Vector2(position.X + rightButtonOffset - 10, position.Y + 23), Color.Blue);
            Primitives.End();
            // Renderizamos el manipulador de la barra
            Primitives.Begin(PrimitiveType.LineList);
                for (int i = -10; i < 10; i++)
                {
                    Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcentRed + i, position.Y + 0), Color.White);
                    Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcentRed + i, position.Y + 6), new Color(50, 50, 70));
                }
                for (int i = -10; i < 10; i++)
                {
                    Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcentGreen + i, position.Y + 10), Color.White);
                    Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcentGreen + i, position.Y + 16), new Color(50, 50, 70));
                }
                for (int i = -10; i < 10; i++)
                {
                    Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcentBlue + i, position.Y + 20), Color.White);
                    Primitives.AddVertex(new Vector2(position.X + valueOffset + 10 + barPorcentBlue + i, position.Y + 26), new Color(50, 50, 70));
                }
            Primitives.End();
            // Boton izquierdo Red
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3), new Rectangle(0, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3), new Rectangle(0, 0, buttonWidth, buttonWidth));
            // Boton Derecho Red
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3), new Rectangle(buttonWidth, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y, buttonWidth, buttonWidth / 3), new Rectangle(buttonWidth, 0, buttonWidth, buttonWidth));

            // Boton izquierdo Green
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3), new Rectangle(0, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3), new Rectangle(0, 0, buttonWidth, buttonWidth));
            // Boton Derecho Green
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3), new Rectangle(buttonWidth, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 10, buttonWidth, buttonWidth / 3), new Rectangle(buttonWidth, 0, buttonWidth, buttonWidth));

            // Boton izquierdo Blue
            if (Mouse.MouseInBox(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3), new Rectangle(0, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + leftButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3), new Rectangle(0, 0, buttonWidth, buttonWidth));
            // Boton Derecho Blue
            if (Mouse.MouseInBox(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3)))
            {
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3), new Rectangle(buttonWidth, buttonWidth, buttonWidth, buttonWidth));
            }
            else
                bottonTexture.RenderOnScreen(new Rectangle((int)position.X + rightButtonOffset, (int)position.Y + 20, buttonWidth, buttonWidth / 3), new Rectangle(buttonWidth, 0, buttonWidth, buttonWidth));

            #endregion

        } // UpdateAndRender

        #endregion

    } // UISliderNumeric
} // XNA2FinalEngine.UI
