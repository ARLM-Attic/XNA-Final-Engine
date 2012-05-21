
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
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
using Texture = XNAFinalEngine.Assets.Texture;
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Color picker dialog.
    /// </summary>
    public class ColorPickerDialog : Dialog
    {
        
        #region Constants
        
        // Square color lenght.
        const int squareColorlenght = 132;

        // For the square color palette.
        // This is part of the implementation to capture the mouse movement outside the control's border.
        private const int squareColorLeft = 5;
        private const int squareColorTop = 5;

        #endregion

        #region Variables

        // The initial color.
        private readonly Color oldColor;

        // The current square color's position.
        private Point positionSquareColor;

        // Intensity level of the right bar.
        private float intensityLevel = 0.5f;

        // The first position in the color palette when this sub control is changing its value (left mouse pressed and not released).
        private Point positionBeginningMovement;
        
        // The control is updating the values. 
        // When a sub control updates one value of another control we don't want that the updated control update the values of the first one.
        private bool update = true;

        // The first intensity level value when the control is updated (left mouse pressed and not released).
        private float intensityLevelValueBeginningMovement;

        // The texture picker for screen picking.
        //private Picker picker;

        // If the control is in screen picking mode.
        private bool isPicking;
        
        // Controls.
        private readonly Button buttonPick;
        private readonly Button buttonClose;
        private readonly Control squareColorPalette;
        private readonly TextBox textBoxRed, textBoxGreen, textBoxBlue;
        private readonly Control intensityLevelBar, background;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Color picker dialog.
        /// </summary>
        public ColorPickerDialog(Color _oldColor)
        {
            oldColor = _oldColor;
            Color = oldColor;
            ClientWidth = 5 + 132 + 10;
            ClientHeight = 235;
            TopPanel.Visible = false;
            IconVisible = false;
            Resizable = false;
            BorderVisible = false;
            Movable = false;
            StayOnTop = true;
            AutoScroll = false;
            
            positionSquareColor = PositionFromColor(Color);

            #region Background

            // The background object is invisible, it serves input actions.
            background = new Control
            {
                Left = 0,
                Top = 0,
                Width = Screen.Width,
                Height = Screen.Height,
                StayOnTop = true, // To bring it to the second place (first is the main control)
                Color = new Color(0, 0, 0, 0)
            };
            UserInterfaceManager.Add(background);
            // If we click outside the window close it.
            background.MouseDown += delegate(object sender, MouseEventArgs e)
                                         {
                                             if (e.Button == MouseButton.Left)
                                             {
                                                 if (isPicking)
                                                 {
                                                    /*picker.BeginManualRenderPickerTexture();
                                                    //UIManager.BeginDraw(); // Don't want this. It's already do.
                                                    ApplicationLogic.Render();
                                                    SpriteManager.DrawSprites();
                                                    UserInterfaceManager.EndDraw();
                                                    picker.EndManualRenderPickerTexture();
                                                    Color = picker.ManualPickFromCurrentPickerTexture(1)[0];
                                                    positionSquareColor = PositionFromColor(Color);*/
                                                    isPicking = false;
                                                    // The background control takes the first place (z order), now it needs to be in second.
                                                    background.StayOnTop = false;  // We need to change this so that the main control can take first place.
                                                    BringToFront();
                                                 }
                                                 else
                                                     Close();
                                             }
                                         };

            #endregion

            #region Buttons

            ContentManager userContentManager = ContentManager.CurrentContentManager;
            ContentManager.CurrentContentManager = ContentManager.SystemContentManager;
            // Button pick
            buttonPick = new Button
            {
                Top = 8,
                Glyph = new Glyph(new Texture("Skin\\Default\\Dropper")) { SizeMode = SizeMode.Centered }, // It needs to be store in the skin file, I know.
            };
            ContentManager.CurrentContentManager = userContentManager;

            buttonPick.Left = (BottomPanel.ClientWidth / 2) - buttonPick.Width - 4;
            BottomPanel.Add(buttonPick);
            buttonPick.Click += delegate
            {
                //picker = new Picker();
                isPicking = true;
                background.StayOnTop = true;
                background.BringToFront();
            };
            // Button close
            buttonClose = new Button
            {
                Left = (BottomPanel.ClientWidth / 2) + 4,
                Top = 8,
                Text = "Close",
                ModalResult = ModalResult.No
            };
            BottomPanel.Add(buttonClose);
            buttonClose.Click += delegate
            {
                Close();
            };
            DefaultControl = buttonClose;

            #endregion

            #region Square color palette

            // Square color
            squareColorPalette = new Control
            {
                Left = squareColorLeft,
                Top = squareColorTop,
                Width = squareColorlenght,
                Height = squareColorlenght,
                Color = new Color(0, 0, 0, 0),
                Movable = true, // To implement a roboust color picker when you can move the mouse outside the color palette limits.
            };
            Add(squareColorPalette);
            
            #endregion

            #region Intensity level bar

            // Intensity level bar
            intensityLevelBar = new Control
            {
                Left = 5 + squareColorlenght,
                Top = 5,
                Width = 20,
                Height = squareColorlenght,
                Color = new Color(0, 0, 0, 0),
                Movable = true, // To implement a roboust level picker when you can move the mouse outside the intensity level bar limits.
            };
            Add(intensityLevelBar);

            #endregion

            #region R G B Text Boxes

            // R
            var labelRed = new Label
                               {
                Parent = this,
                Text = " R",
                Width = 40,
                Top = 5 + squareColorlenght + 50,
                Left = 5,
            };
            textBoxRed = new TextBox
            {
                Parent = this,
                Left = 5,
                Top = labelRed.Top + labelRed.Height + 2,
                Width = 40,
                Text = "1"
            };
            // G
            var labelGreen = new Label
            {
                Parent = this,
                Text = " G",
                Width = 40,
                Top = 5 + squareColorlenght + 50,
                Left = labelRed.Width + 10,
            };
            textBoxGreen = new TextBox
                                   {
                Parent = this,
                Left = labelRed.Width + 10,
                Top = labelRed.Top + labelRed.Height + 2,
                Width = 40,
                Text = "1"
            };
            // B
            var labelBlue = new Label
                                {
                Parent = this,
                Text = " B",
                Width = 40,
                Top = 5 + squareColorlenght + 50,
                Left = labelRed.Width * 2 + 15,
            };
            textBoxBlue = new TextBox
                                  {
                Parent = this,
                Left = labelRed.Width * 2 + 15,
                Top = labelRed.Top + labelRed.Height + 2,
                Width = 40,
                Text = "1"
            };

            UpdateRGBFromColor();

            #endregion

            background.BringToFront();

        } // ColorPickerDialog

        #endregion

        #region Init

        protected internal override void Init()
        {
            base.Init();

            #region Square Color

            // When the user clicks in the square color control
            squareColorPalette.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                Color = ColorFromPositionWithIntensity(e.Position);
                positionSquareColor = e.Position;
                positionBeginningMovement = e.Position;
                UpdateRGBFromColor();
            };
            // When the user clicks and without releasing it he moves the mouse.
            squareColorPalette.Move += delegate(object sender, MoveEventArgs e)
            {
                if (update)
                {
                    Point position = new Point(positionBeginningMovement.X + (e.Left - squareColorLeft), positionBeginningMovement.Y + (e.Top - squareColorTop));
                    if (position.X < 0)
                        position.X = 0;
                    else if (position.X > squareColorlenght)
                        position.X = squareColorlenght;
                    if (position.Y < 0)
                        position.Y = 0;
                    else if (position.Y > squareColorlenght)
                        position.Y = squareColorlenght;
                    Color = ColorFromPositionWithIntensity(position);
                    positionSquareColor = position;
                    UpdateRGBFromColor();
                }
            };
            squareColorPalette.MoveEnd += delegate
            {
                update = false;
                squareColorPalette.Left = 5;
                squareColorPalette.Top = 5;
                update = true;
            };
            squareColorPalette.KeyPress += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Escape)
                {
                    Close();
                }
            };

            #endregion

            #region Intensity Level

            // Intensity Level
            intensityLevelBar.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                intensityLevel = 1 - (e.Position.Y / (float)squareColorlenght);
                Color = ColorFromPositionWithIntensity(positionSquareColor);
                intensityLevelValueBeginningMovement = intensityLevel;
                UpdateRGBFromColor();
            };
            intensityLevelBar.Move += delegate(object sender, MoveEventArgs e)
            {
                if (update)
                {
                    float intensity = 1 - (intensityLevelValueBeginningMovement - (e.Top - squareColorTop) / (float)squareColorlenght);
                    if (intensity < 0)
                        intensity = 0;
                    else if (intensity > 1)
                        intensity = 1;
                    intensityLevel = 1 - intensity;
                    Color = ColorFromPositionWithIntensity(positionSquareColor);
                    UpdateRGBFromColor();
                }
            };
            intensityLevelBar.MoveEnd += delegate
            {
                update = false;
                intensityLevelBar.Left = 5 + squareColorlenght;
                intensityLevelBar.Top = 5;
                update = true;
            };
            intensityLevelBar.KeyPress += delegate(object sender, KeyEventArgs e)
                                              {
                                                  if (e.Key == Keys.Escape)
                                                  {
                                                      Close();
                                                  }
                                              };

            #endregion

            #region R

            textBoxRed.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    if (textBoxRed.Text.IsNumericFloat())
                    {
                        if ((float)double.Parse(textBoxRed.Text) < 0)
                            textBoxRed.Text = "0";
                        if ((float)double.Parse(textBoxRed.Text) > 1)
                            textBoxRed.Text = "1";
                        UpdateColorFromRGB();
                    }
                    else
                    {
                        UpdateRGBFromColor();
                    }
                }
            };
            // For tabs and other not so common things.
            textBoxRed.FocusLost += delegate
            {
                if (textBoxRed.Text.IsNumericFloat())
                {
                    if ((float)double.Parse(textBoxRed.Text) < 0)
                        textBoxRed.Text = "0";
                    if ((float)double.Parse(textBoxRed.Text) > 1)
                        textBoxRed.Text = "1";
                    UpdateColorFromRGB();
                }
                else
                {
                    UpdateRGBFromColor();
                }
            };

            #endregion

            #region G

            textBoxGreen.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    if (textBoxGreen.Text.IsNumericFloat())
                    {
                        if ((float)double.Parse(textBoxGreen.Text) < 0)
                            textBoxGreen.Text = "0";
                        if ((float)double.Parse(textBoxGreen.Text) > 1)
                            textBoxGreen.Text = "1";
                        UpdateColorFromRGB();
                    }
                    else
                    {
                        UpdateRGBFromColor();
                    }
                }
            };
            // For tabs and other not so common things.
            textBoxGreen.FocusLost += delegate
            {
                if (textBoxGreen.Text.IsNumericFloat())
                {
                    if ((float)double.Parse(textBoxGreen.Text) < 0)
                        textBoxGreen.Text = "0";
                    if ((float)double.Parse(textBoxGreen.Text) > 1)
                        textBoxGreen.Text = "1";
                    UpdateColorFromRGB();
                }
                else
                {
                    UpdateRGBFromColor();
                }
            };

            #endregion

            #region B

            textBoxBlue.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    if (textBoxBlue.Text.IsNumericFloat())
                    {
                        if ((float)double.Parse(textBoxBlue.Text) < 0)
                            textBoxBlue.Text = "0";
                        if ((float)double.Parse(textBoxBlue.Text) > 1)
                            textBoxBlue.Text = "1";
                        UpdateColorFromRGB();
                    }
                    else
                    {
                        UpdateRGBFromColor();
                    }
                }
            };
            // For tabs and other not so common things.
            textBoxBlue.FocusLost += delegate
            {
                if (textBoxBlue.Text.IsNumericFloat())
                {
                    if ((float)double.Parse(textBoxBlue.Text) < 0)
                        textBoxBlue.Text = "0";
                    if ((float)double.Parse(textBoxBlue.Text) > 1)
                        textBoxBlue.Text = "1";
                    UpdateColorFromRGB();
                }
                else
                {
                    UpdateRGBFromColor();
                }
            };

            #endregion

            Focused = true;

        } // Init

        #endregion

        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            base.DrawControl(rect);
            LineManager.Begin2D(PrimitiveType.LineList);

            #region Render Square Color

            // Initial color (left top corner)
            Color color = new Color(255, 0, 0, 255);
            // Relative square position
            Vector2 position = new Vector2(5, 5);
            
            // I divide the problem into six steps.
            for (int i = 0; i < squareColorlenght; i++)
            {
                int j = i % (squareColorlenght / 6); // the position in the current step
                float porcentaje = (j / (squareColorlenght / 6f - 1)); // The porcentaje of advance in the current step
                if (i < squareColorlenght / 6f)                    // Red to Yellow
                {
                    color.G = (byte)(255 * porcentaje);
                }
                else if (i < 2 * squareColorlenght / 6f)           // Yellow to green
                {
                    color.R = (byte)(255 - 255 * porcentaje);
                }
                else if (i < 3 * squareColorlenght / 6f)           // green to cyan
                {
                    color.B = (byte)(255 * porcentaje);
                }
                else if (i < 4 * squareColorlenght / 6f)           // cyan to blue
                {
                    color.G = (byte)(255 - 255 * porcentaje);
                }
                else if (i < 5 * squareColorlenght / 6f)           // blue to violet
                {
                    color.R = (byte)(255 * porcentaje);
                }
                else                                               // violet to red
                {
                    color.B = (byte)(255 - 255 * porcentaje);
                }
                LineManager.AddVertex(new Vector2(i + position.X, position.Y), MultiplyColorByFloat(color, intensityLevel));
                LineManager.AddVertex(new Vector2(i + position.X, position.Y + 132), MultiplyColorByFloat(new Color(255, 255, 255), intensityLevel));
            }

            #endregion

            // Square color pointer
            float colorPointerScale;
            if (intensityLevel < 0.6f)
                colorPointerScale = 1.0f;
            else
                colorPointerScale = 1 - intensityLevel;
            LineManager.Draw2DPlane(new Rectangle(positionSquareColor.X + 2, positionSquareColor.Y + 2, 6, 6), new Color(colorPointerScale, colorPointerScale, colorPointerScale));
            // Color planes
            LineManager.DrawSolid2DPlane(new Rectangle(5, squareColorlenght + 10, 40, 40), oldColor);
            LineManager.DrawSolid2DPlane(new Rectangle(45, squareColorlenght + 10, 40, 40), Color);
            // Intensity Level Bar
            LineManager.DrawSolid2DPlane(new Rectangle(squareColorlenght + 5, 5, 20, squareColorlenght),
                                         positionSquareColor.Y == squareColorlenght ? Color.White : ColorFromPositionWithIntensity(positionSquareColor, 1), Color.Black);

            LineManager.Draw2DPlane(new Rectangle(squareColorlenght + 5, (int)(squareColorlenght * (1 - intensityLevel)) - 3 + 5, 20, 6), new Color(200, 200, 200));

            LineManager.End();
        } // DrawControl

        #endregion

        #region Update Values

        /// <summary>
        /// Update Color from the R G B sliders values.
        /// </summary>
        private void UpdateColorFromRGB()
        {
            Color = new Color((float)double.Parse(textBoxRed.Text), (float)double.Parse(textBoxGreen.Text), (float)double.Parse(textBoxBlue.Text));
            positionSquareColor = PositionFromColor(Color);
        } // UpdateColorFromRGB

        /// <summary>
        /// Update the R G B sliders values with the control color.
        /// </summary>
        private void UpdateRGBFromColor()
        {
            textBoxRed.Text = Math.Round(Color.R / 255f, 3).ToString();
            textBoxGreen.Text = Math.Round(Color.G / 255f, 3).ToString();
            textBoxBlue.Text = Math.Round(Color.B / 255f, 3).ToString();
        } // UpdateRGBFromColor

        #endregion

        #region Color From Position

        /// <summary>
        /// Return the color from the position in the square color.
        /// </summary>
        private Color ColorFromPositionWithIntensity(Point position)
        {
            return ColorFromPositionWithIntensity(position, intensityLevel);
        } // ColorFromPositionWithIntensity

        /// <summary>
        /// Return the color from the position in the square color.
        /// </summary>
        private static Color ColorFromPositionWithIntensity(Point position, float _intensityLevel)
        {
            return Color.Lerp(MultiplyColorByFloat(ColorFromPosition(position), _intensityLevel), MultiplyColorByFloat(new Color(255, 255, 255), _intensityLevel), position.Y / 132f);
        } // ColorFromPositionWithIntensity

        /// <summary>
        /// Return the color from the position in the square color.
        /// </summary>
        private static Color ColorFromPosition(Point position)
        {
            Color color = new Color(0, 0, 0, 255);
            // the position in the step or band (unknown for now)
            int j = position.X % (squareColorlenght / 6);
            float porcentaje = (j / (squareColorlenght / 6f - 1)); // The porcentaje of advance in the step
            if (position.X < squareColorlenght / 6f)               // Red to Yellow
            {
                color.R = 255;
                color.G = (byte)(255 * porcentaje);
                color.B = 0;
            }
            else if (position.X < 2 * squareColorlenght / 6f)      // Yellow to green
            {
                color.R = (byte)(255 - 255 * porcentaje);
                color.G = 255;
                color.B = 0;
            }
            else if (position.X < 3 * squareColorlenght / 6f)      // green to cyan
            {
                color.R = 0;
                color.G = 255;
                color.B = (byte)(255 * porcentaje);
            }
            else if (position.X < 4 * squareColorlenght / 6f)      // cyan to blue
            {
                color.R = 0;
                color.G = (byte)(255 - 255 * porcentaje);
                color.B = 255;
            }
            else if (position.X < 5 * squareColorlenght / 6f)      // blue to violet
            {
                color.R = (byte)(255 * porcentaje);
                color.G = 0;
                color.B = 255;
            }
            else                                        // violet to red
            {
                color.R = 255;
                color.G = 0;
                color.B = (byte)(255 - 255 * porcentaje);
            }
            return color;
        } // ColorFromPosition

        #endregion

        #region Position From Color

        /// <summary>
        /// Return a square color position from a given color.
        /// </summary>
        private Point PositionFromColor(Color color)
        {
            Point position = new Point();
            float percentage;
            // The higher color tells us the intensity level.
            // The lowest color tells us the position Y, but directly, it has to take the intensity level into consideration.
            // The middle one gives us the position X, but the range is between the lowest color to the higher one.
            if (color.R == color.G && color.R == color.B)
            {
                intensityLevel = color.R / 255f;
                position.X = 0;
                position.Y = squareColorlenght;
            }
            else if (color.R >= color.G && color.R >= color.B)
            {
                intensityLevel = color.R/255f;
                if (color.G >= color.B)                                                         // Red to Yellow
                {
                    percentage = (((color.G - color.B) / 255f) / ((color.R - color.B) / 255f)); 
                    position.X = (int)(percentage * (squareColorlenght / 6));
                    position.Y = (int)(color.B / intensityLevel / 255f * squareColorlenght);
                }
                else                                                                            // violet to red
                {
                    percentage = (((color.B - color.G) / 255f) / ((color.R - color.G) / 255f));
                    position.X = (int)(squareColorlenght - (percentage * (squareColorlenght / 6f)));
                    position.Y = (int)(color.G / intensityLevel / 255f * squareColorlenght);
                }
            }
            else if (color.G >= color.R && color.G >= color.B)
            {
                intensityLevel = color.G / 255f;
                if (color.R >= color.B)                                                         // Yellow to green
                {
                    percentage = (((color.R - color.B) / 255f) / ((color.G - color.B) / 255f));
                    position.X = (int)(squareColorlenght / 3f - (percentage * (squareColorlenght / 6f)));
                    position.Y = (int)(color.B/ intensityLevel / 255f * squareColorlenght);
                }
                else                                                                            // green to cyan
                {
                    percentage = (((color.B - color.R) / 255f) / ((color.G - color.R) / 255f));
                    position.X = (int)(percentage * (squareColorlenght / 6) + squareColorlenght / 3f);
                    position.Y = (int)(color.R / intensityLevel / 255f * squareColorlenght);
                }
            }
            else
            {
                intensityLevel = color.B / 255f;
                if (color.G >= color.R)                                                         // cyan to blue
                {
                    percentage = (((color.G - color.R) / 255f) / ((color.B - color.R) / 255f));
                    position.X = (int)(2f * squareColorlenght / 3f - (percentage * (squareColorlenght / 6f)));
                    position.Y = (int)(color.R / intensityLevel / 255f * squareColorlenght);
                }
                else                                                                            // blue to violet
                {
                    percentage = (((color.R - color.G) / 255f) / ((color.B - color.G) / 255f));
                    position.X = (int)(percentage * (squareColorlenght / 6) + 2f * squareColorlenght / 3f);
                    position.Y = (int)(color.G / intensityLevel / 255f * squareColorlenght);
                }
            }
            return position;
        } // PositionFromColor

        #endregion

        #region Multiply Color By Float

        /// <summary>
        /// Multiply a color by a float.
        /// </summary>
        private static Color MultiplyColorByFloat(Color color, float intensityLevel)
        {
            Color result = Color.White;
            result.R = (byte)(color.R * intensityLevel);
            result.G = (byte)(color.G * intensityLevel);
            result.B = (byte)(color.B * intensityLevel);
            return result;
        } // MultiplyColorByFloat

        #endregion

        #region Close

        /// <summary>
        /// Close
        /// </summary>
        public override void Close()
        {
            base.Close();
            UserInterfaceManager.Remove(background);
            background.Dispose();
        } // Close

        #endregion

        #region On Key Press

        protected override void OnKeyPress(KeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                Close();
            }
            base.OnKeyPress(e);
        } // OnKeyPress

        #endregion
        
    } // ColorPickerDialog
} // XNAFinalEngine.UserInterface