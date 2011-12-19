
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
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.UserInterface;
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngineExamples.Editor
{
    public static class ConstantWindow
    {
        /// <summary>
        /// Creates and shows the configuration window of this material.
        /// </summary>
        public static void Show(Constant material)
        {
            #region Window

            var window = new Window
            {
                Text = material.Name + " : Constant"
            };
            UserInterfaceManager.Add(window);
            window.Closed += delegate { };

            #endregion

            #region Name

            var materialNameLabel = new Label {Text = "Name", Left = 10, Top = 10,};
            window.Add(materialNameLabel);
            var materialNameTextBox = new TextBox { Text = material.Name, Left = 60, Top = 10 };
            window.Add(materialNameTextBox);
            materialNameTextBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    material.Name = materialNameTextBox.Text;
                    window.Text = material.Name + " : Constant";
                }
            };
            materialNameTextBox.FocusLost += delegate
            {
                material.Name = materialNameTextBox.Text;
                window.Text = material.Name + " : Constant";
            };

            #endregion

            #region Group Surface Parameters

            GroupBox group = new GroupBox
            {
                Parent = window,
                Anchor = Anchors.Left | Anchors.Top | Anchors.Right,
                Width = window.ClientWidth - 16,
                Height = 160,
                Left = 8,
                Top = materialNameLabel.Top + materialNameLabel.Height + 15,
                Text = "Surface Parameters",
                TextColor = Color.Gray,
            };

            #endregion
            
            #region Diffuse Color

            var sliderColor = new SliderColor
            {
                Left = 10,
                Top = 20,
                Color = material.DiffuseColor,
                Text = "Diffuse Color",
            };
            group.Add(sliderColor);
            sliderColor.ColorChanged += delegate { material.DiffuseColor = sliderColor.Color; };
            sliderColor.Draw += delegate { sliderColor.Color = material.DiffuseColor; };

            #endregion
            
            //group.Height = sliderColor.Top + sliderColor.Height + 20;
            window.Height = group.Top + group.Height + 40;

        } // Show

    } // ConstantWindow
} // XNAFinalEngineExamples.Editor