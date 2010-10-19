
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
#endregion

namespace XNAFinalEngine.UI
{
    /// <summary>
    /// Clase base de los manipuladores de transformacion (escalado, traslacion, rotacion)
    /// </summary>
    public abstract class UIManipulators
    {

        #region Variables

        /// <summary>
        /// Estructura auxiliar. Permite guardar temporalmente los vertices tranformados para crear los manipuladores graficamente.
        /// </summary>
        protected static Vector3[] vertices = new Vector3[7];
     
        /// <summary>
        /// Indica que ejes estan seleccionados.
        /// </summary>
        protected static bool redAxisSelected = false,
                              greenAxisSelected = false,
                              blueAxisSelected = false;

        /// <summary>
        /// Nos indica si el manipulador esta activo. O en otras palabras, si el boton izquierdo del mouse esta siento apretado.
        /// </summary>
        protected static bool active = false;

        /// <summary>
        /// Indica en que proporcion el moviemiento del mouse vertical o horizontal afecta el movimiento del manipulador.
        /// </summary>
        protected static Vector2 amout;

        /// <summary>
        /// El tamaño de la region del cursor que afecta a la seleccion de los distintos miembros del manipulador.
        /// </summary>
        protected static int regionSize = 15;

        /// <summary>
        /// El objeto que se manipulara.
        /// </summary>
        protected static XNAFinalEngine.GraphicElements.Object obj;

        /// <summary>
        /// Indica si el manipulador efectivamente movio al objeto.
        /// </summary>
        protected static bool produceTransformation = false;

        /// <summary>
        /// La matrix del objeto seleccionado antes de la manipulacion.
        /// </summary>
        protected static Matrix oldLocalMatrix;

        #endregion

        #region Properties

        /// <summary>
        /// El tamaño de la region del cursor que afecta a la seleccion de los distintos miembros del manipulador.
        /// </summary>
        public static int RegionSize
        {
            get { return regionSize; }
            set { regionSize = value; }
        } // RegionSize

        /// <summary>
        /// Indica si el manipulador efectivamente movio al objeto.
        /// </summary>
        public static bool ProduceTransformation
        {
            get
            {
                bool aux = produceTransformation;
                produceTransformation = false;
                return aux;
            }
        } // ProduceTransformation

        /// <summary>
        /// La matrix del objeto seleccionado antes de la manipulacion.
        /// </summary>
        public static Matrix OldLocalMatrix { get { return oldLocalMatrix; } }

        /// <summary>
        /// Indica si existe algun manipulador activo
        /// </summary>
        public static bool Active { get { return active; } }

        #endregion
        
    } // UIManipulators
} // XNAFinalEngine.UI
