
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
using Microsoft.Xna.Framework;
#endregion

namespace XNAFinalEngine.Graphics
{
    /// <summary>
    /// Directional Light
    /// </summary>
    public class DirectionalLight : Light
    {

        #region Variables

        /// <summary>
        /// Light direction
        /// </summary>
        protected Vector3 direction;

        #endregion

        #region Properties

        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = Vector3.Normalize(value); }
        }
                
        #endregion

        #region Constructor

        /// <summary>
        /// Directional Light
        /// </summary>
        public DirectionalLight(Vector3 _direction, Color _color)
        {
            Direction = _direction;
            Color = _color;
            //lightObject = new GraphicObject("Directional", new Constant(color));
        } // DirectionalLight

        #endregion

        #region Render

        /// <summary>
        /// Render the graphic representation of the light. It’s a tool to see and change light’s parameters in real time.
        /// </summary>
        public override void Render()
        {
           ((Constant)lightObject.Material).SurfaceColor = color;
           // If the direction is Up or Down then we can't the up vector for the rotation matrix
           lightObject.LocalMatrix = Matrix.Identity;
           lightObject.ScaleAbs(0.5f);
           if(direction.Y == 1 || direction.Y == -1)
                lightObject.RotateAbs(Matrix.CreateWorld(Vector3.Zero, -direction, Vector3.Right));
           else
               lightObject.RotateAbs(Matrix.CreateWorld(Vector3.Zero, -direction, Vector3.Up));
           lightObject.TranslateRelLocal(0, 0, -5);
           lightObject.Render();
        } // Render

        #endregion

    } // DirectionalLight
} // XNAFinalEngine.Graphics


