
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace XNAFinalEngine.GraphicElements
{
    /// <summary>
    /// Point Light.
    /// </summary>
    public class PointLight : Light
    {

        #region Variables

        /// <summary>
        /// Light position.
        /// </summary>
        protected Vector3 position;

        #endregion

        #region Properties

        /// <summary>
        /// The point light position.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Point Light
        /// </summary>
        public PointLight(Vector3 _position, Color _color)
        {
            position = _position;
            color = _color;
            //lightObject = new GraphicObject(new Sphere(20, 20, 0.5f), new Constant(color));
            //lightObject.TranslateAbs(position);
        } // PointLight

        #endregion

        #region Render

        /// <summary>
        /// Render the graphic representation of the light. It�s a tool to see and change light�s parameters in real time.
        /// </summary>
        public override void Render()
        {
            position = lightObject.WorldPosition;
            ((Constant)lightObject.Material).SurfaceColor = color;
            lightObject.Render();
        } //Render

        #endregion

    } // PointLight
} // XNAFinalEngine.GraphicElements