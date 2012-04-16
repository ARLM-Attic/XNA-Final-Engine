
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
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
using XNAFinalEngine.Components;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class Gizmo
    {

        #region Enumerates

        /// <summary>
        /// The gizmos work in local or global space.
        /// </summary>
        public enum SpaceMode
        {
            Local,
            Global,
        };

        #endregion

        #region Constants

        /// <summary>
        /// The size of the cursor region that affects the selection of the different gizmos.
        /// </summary>
        protected const int RegionSize = 15;

        #endregion

        #region Variables
        
        // The selected object.
        // If there are more then the first is used to place the gizmo.
        protected static GameObject selectedObject;
        
        // Indicates what axis is selected.
        protected static bool redAxisSelected,
                              greenAxisSelected,
                              blueAxisSelected;

        // Auxiliary structure.
        protected static Vector3[] vertices = new Vector3[7];

        protected static Picker picker;
        
        // Indica en que proporcion el moviemiento del mouse vertical o horizontal afecta el movimiento del manipulador.
        protected static Vector2 transformationAmount;
        
        // Indica si el manipulador efectivamente movio al objeto.
        protected static bool produceTransformation;
        /*
        /// <summary>
        /// La matrix del objeto seleccionado antes de la manipulacion.
        /// </summary>
        protected static Matrix oldLocalMatrix;
        */
        #endregion

        #region Properties

        /// <summary>
        /// Indicates if the current gizmo is active or not.
        /// </summary>
        public static bool Active { get; protected set; }

        /// <summary>
        /// The camera used for render the gizmos.
        /// </summary>
        internal static GameObject3D GizmoCamera { get; set; }

        /// <summary>
        /// Local or World Space.
        /// </summary>
        public static SpaceMode Space { get; set; }

        /*
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
        */

        #endregion

        #region Gizmo Center And Orientation

        /// <summary>
        ///  Calculate the center and orientation of the gizmo. 
        /// </summary>
        protected static void GizmoCenterAndOrientation(out Vector3 center, out Quaternion orientation)
        {
            center = Vector3.Zero;
            orientation = new Quaternion();
            if (Space == SpaceMode.Local)
            {
                if (selectedObject is GameObject3D && ((GameObject3D)selectedObject).ModelRenderer != null)
                {
                    center = ((GameObject3D)selectedObject).ModelRenderer.BoundingSphere.Center;
                    orientation = ((GameObject3D)selectedObject).Transform.LocalRotation;
                }
            }
            else
            {
                if (selectedObject is GameObject3D && ((GameObject3D)selectedObject).ModelRenderer != null)
                {
                    center = ((GameObject3D)selectedObject).ModelRenderer.BoundingSphere.Center;
                    orientation = Quaternion.CreateFromRotationMatrix(Matrix.Identity);
                }
            }
        } // GizmoCenterAndOrientation

        #endregion

    } // Gizmo
} // XNAFinalEngine.Editor
