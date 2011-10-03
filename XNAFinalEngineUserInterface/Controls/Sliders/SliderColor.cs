
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Slider for color values.
    /// </summary>
    public class SliderColor : Control
    {

        #region Variables

        private TextBox textBoxR, textBoxG, textBoxB;

        private TrackBar sliderR, sliderG, sliderB;

        /// <summary>
        /// If the system is updating the color, then the color won't be updated by the RGB sliders.
        /// </summary>
        private bool updatingColor;

        /// <summary>
        /// If the system is updating the RGB sliders, then the RGB sliders won't be updated by the color.
        /// </summary>
        private bool updatingRGB;

        #endregion

        #region Constructor

        /// <summary>
        /// Slider for numeric values.
        /// </summary>
        public SliderColor()
        {

            Anchor = Anchors.Left | Anchors.Right | Anchors.Top;
            CanFocus = false;
            Passive = true;
            Width = 420;
            Height = 75;
            var label = new Label
            {
                Parent = this,
                Width = 150,
                Top = 25,
            };
            TextChanged += delegate { label.Text = Text; };

            #region Square Color

            // Square color
            Control squareColor = new Panel
            {
                Left = label.Left + label.Width + 5,
                Top = 17,
                Width = 40,
                Height = 40,
                Color = Color,
                BevelBorder = BevelBorder.All,
                BevelStyle = BevelStyle.Etched,
                BevelColor = Color.Black,
            };
            Add(squareColor);
            squareColor.MouseDown += delegate
                                    {/*
                                        var colorPickerDialog = new ColorPickerDialog(Color);
                                        UserInterfaceManager.Add(colorPickerDialog);

                                        colorPickerDialog.Closed += delegate
                                        {
                                            Focused = true;
                                        };
                                        
                                        #region Color Picker Position
                                        
                                        int left = squareColor.ControlLeftAbsoluteCoordinate;
                                        if (left + colorPickerDialog.Width > SystemInformation.ScreenWidth)
                                            left -= colorPickerDialog.Width;
                                        int top = squareColor.ControlTopAbsoluteCoordinate + squareColor.Height;
                                        if (top + colorPickerDialog.Height > SystemInformation.ScreenHeight)
                                            top -= colorPickerDialog.Height + squareColor.Height;
                                        colorPickerDialog.SetPosition(left, top);
                                        
                                        #endregion

                                        colorPickerDialog.ColorChanged += delegate
                                        {
                                            Color = colorPickerDialog.Color;
                                        };*/ // TODO!!!!
                                    };

            #endregion

            #region R

            textBoxR = new TextBox
            {
                Parent = this,
                Top = 4,
                Width = 40,
                Left = label.Left + label.Width + 5 + 45,
                Text = "1",
            };
            sliderR = new TrackBar
            {
                Parent = this,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Left = textBoxR.Left + textBoxR.Width + 4,
                Top = 6,
                MinimumWidth = 100,
                Height = 15,
                MinimumValue = 0,
                MaximumValue = 1,
                Width = 176,
                ValueCanBeOutOfRange = false,
                IfOutOfRangeRescale = false,
                ScaleBarColor = ScaleColor.Red,
            };
            sliderR.ValueChanged += delegate
                                    {
                                        updatingRGB = true;
                                        textBoxR.Text = Math.Round(sliderR.Value, 3).ToString();
                                        if (!updatingColor)
                                        {
                                            if (XNAFinalEngine.Input.Keyboard.KeyPressed(Keys.LeftControl))
                                            {
                                                sliderG.Value = sliderR.Value;
                                                sliderB.Value = sliderR.Value;
                                            }
                                            UpdateColorFromRGB();
                                        }
                                        updatingRGB = false;
                                    };
            textBoxR.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    updatingRGB = true;
                    try
                    {
                        sliderR.Value = (float)double.Parse(textBoxR.Text);
                        UpdateColorFromRGB();
                    }
                    catch // If not numeric
                    {
                        textBoxR.Text = sliderR.Value.ToString();
                    }
                    updatingRGB = false;
                }
            };
            // For tabs and other not so common things.
            textBoxR.FocusLost += delegate
            {
                updatingRGB = true;
                try
                {
                    sliderR.Value = (float)double.Parse(textBoxR.Text);
                    UpdateColorFromRGB();
                }
                catch // If not numeric
                {
                    textBoxR.Text = sliderR.Value.ToString();
                }
                updatingRGB = false;
            };

            #endregion

            #region G

            textBoxG = new TextBox
            {
                Parent = this,
                Top = 4 + textBoxR.Top + textBoxR.Height,
                Width = 40,
                Left = label.Left + label.Width + 4 + 45,
                Text = "1"
            };
            sliderG = new TrackBar
            {
                Parent = this,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Left = textBoxG.Left + textBoxG.Width + 4,
                Top = 6 + textBoxR.Top + textBoxR.Height,
                Height = 15,
                MinimumWidth = 100,
                MinimumValue = 0,
                MaximumValue = 1,
                Width = 176,
                ValueCanBeOutOfRange = false,
                IfOutOfRangeRescale = false,
                ScaleBarColor = ScaleColor.Green,
            };
            sliderG.ValueChanged += delegate
                                        {
                                            updatingRGB = true;
                                            textBoxG.Text = Math.Round(sliderG.Value, 3).ToString();
                                            if (!updatingColor)
                                            {
                                                if (XNAFinalEngine.Input.Keyboard.KeyPressed(Keys.LeftControl))
                                                {
                                                    sliderR.Value = sliderG.Value;
                                                    sliderB.Value = sliderG.Value;
                                                }
                                                UpdateColorFromRGB();
                                            }
                                            updatingRGB = false;
                                        };
            textBoxG.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    updatingRGB = true;
                    try
                    {
                        sliderG.Value = (float)double.Parse(textBoxG.Text);
                        UpdateColorFromRGB();
                    }
                    catch // If not numeric
                    {
                        textBoxG.Text = sliderG.Value.ToString();
                    }
                    updatingRGB = false;
                }
            };
            // For tabs and other not so common things.
            textBoxG.FocusLost += delegate
            {
                updatingRGB = true;
                try
                {
                    sliderG.Value = (float)double.Parse(textBoxG.Text);
                    UpdateColorFromRGB();
                }
                catch // If not numeric
                {
                    textBoxG.Text = sliderG.Value.ToString();
                }
                updatingRGB = false;
            };

            #endregion

            #region B

            textBoxB = new TextBox
            {
                Parent = this,
                Top = 4 + textBoxG.Top + textBoxG.Height,
                Width = 40,
                Left = label.Left + label.Width + 4 + 45,
                Text = "1"
            };
            sliderB = new TrackBar
            {
                Parent = this,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Left = textBoxB.Left + textBoxB.Width + 4,
                Top = 6 + textBoxG.Top + textBoxG.Height,
                Height = 15,
                MinimumWidth = 100,
                MinimumValue = 0,
                MaximumValue = 1,
                Width = 176,
                ValueCanBeOutOfRange = false,
                IfOutOfRangeRescale = false,
                ScaleBarColor = ScaleColor.Blue,
            };
            sliderB.ValueChanged += delegate
                                    {
                                        updatingRGB = true;
                                        textBoxB.Text = Math.Round(sliderB.Value, 3).ToString();
                                        if (!updatingColor)
                                        {
                                            if (XNAFinalEngine.Input.Keyboard.KeyPressed(Keys.LeftControl))
                                            {
                                                sliderR.Value = sliderB.Value;
                                                sliderG.Value = sliderB.Value;
                                            }
                                            UpdateColorFromRGB();
                                        }
                                        updatingRGB = false;
                                    };
            textBoxB.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    updatingRGB = true;
                    try
                    {
                        sliderB.Value = (float)double.Parse(textBoxB.Text);
                        UpdateColorFromRGB();
                    }
                    catch // If not numeric
                    {
                        textBoxB.Text = sliderB.Value.ToString();
                    }
                    updatingRGB = false;
                }
            };
            // For tabs and other not so common things.
            textBoxB.FocusLost += delegate
            {
                updatingRGB = true;
                try
                {
                    sliderB.Value = (float)double.Parse(textBoxB.Text);
                    UpdateColorFromRGB();
                }
                catch // If not numeric
                {
                    textBoxB.Text = sliderB.Value.ToString();
                }
                updatingRGB = false;
            };

            #endregion

            ColorChanged += delegate
            {
                updatingColor = true;
                    squareColor.Color = Color;
                    if (!updatingRGB)
                        UpdateRGBFromColor();
                updatingColor = false;
            };

            // To init all values with a color
            Color = Color.Gray;

        } // SliderColor

        #endregion

        #region Update Values

        /// <summary>
        /// Update Color from the R G B sliders values.
        /// </summary>
        private void UpdateColorFromRGB()
        {
            Color = new Color(sliderR.Value, sliderG.Value, sliderB.Value);
        } // UpdateColorFromRGB

        /// <summary>
        /// Update the R G B sliders values with the control color.
        /// </summary>
        private void UpdateRGBFromColor()
        {
            sliderR.Value = Color.R / 255f;
            sliderG.Value = Color.G / 255f;
            sliderB.Value = Color.B / 255f;
        } // UpdateRGBFromColor

        #endregion

        #region Draw Control

        protected override void DrawControl(Rectangle rect)
        {
            // Only the children will be rendered.
        } // DrawControl

        #endregion

    } // SliderColor
} // XNAFinalEngine.UserInterface