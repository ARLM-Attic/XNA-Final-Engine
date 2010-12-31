
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
using XNAFinalEngine.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.UI
{
    /// <summary>
    /// Renderiza un puntero en las coordenadas de pantalla del mouse
    /// </summary>
    public class MousePointer
    {

        #region Variables

        /// <summary>
        ///  The mouse pointer's texture.
        /// </summary>
        private static XNAFinalEngine.Graphics.Texture mousePointer = null;

        private static bool manipulatorModeActive = false;

        #endregion

        #region Properties

        public static bool ManipulatorMode
        {
            get { return manipulatorModeActive; }
            set { manipulatorModeActive = value; }
        } // ManipulatorMode

        #endregion

        /// <summary>
        /// Render the mouse pointer. No necesita inicializar nada.
        /// </summary>
        public static void RenderMousePointer()
        {
            if (mousePointer == null)
            {
                mousePointer = new XNAFinalEngine.Graphics.Texture("MousePointer");
            }
            if (manipulatorModeActive)
            {
                mousePointer.RenderOnScreen(new Rectangle(Mouse.Position.X + Gizmo.RegionSize / 2, Mouse.Position.Y + Gizmo.RegionSize / 2, 24, 24));
                Primitives.Draw2DPlane(new Rectangle(Mouse.Position.X,
                                                         Mouse.Position.Y,
                                                         Gizmo.RegionSize,
                                                         Gizmo.RegionSize),
                                           Color.White);
            }
            else
            {
                mousePointer.RenderOnScreen(new Rectangle(Mouse.Position.X, Mouse.Position.Y, 24, 24));
            }
        } // RenderMouse

        /// <summary>
        /// Render the mouse pointer. No necesita inicializar nada.
        /// </summary>
        public static void RenderMousePointer(Point MousePos)
        {
            if (mousePointer == null)
            {
                mousePointer = new XNAFinalEngine.Graphics.Texture("MousePointer");
            }
            if (manipulatorModeActive)
            {
                mousePointer.RenderOnScreen(new Rectangle(MousePos.X + Gizmo.RegionSize / 2, MousePos.Y + Gizmo.RegionSize / 2, 24, 24));
                Primitives.Draw2DPlane(new Rectangle(MousePos.X,
                                                         MousePos.Y,
                                                         Gizmo.RegionSize,
                                                         Gizmo.RegionSize),
                                           Color.White);
            }
            else
            {
                mousePointer.RenderOnScreen(new Rectangle(MousePos.X, MousePos.Y, 24, 24));
            }
        } // RenderMouse

    } // MousePointer
} // XNAFinalEngine.UI
