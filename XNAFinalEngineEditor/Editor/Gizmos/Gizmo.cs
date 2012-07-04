
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.EngineCore;
#endregion

namespace XNAFinalEngine.Editor
{
    /// <summary>
    /// Base class for gizmos.
    /// </summary>
    internal abstract class Gizmo
    {

        #region Enumerates

        /// <summary>
        /// The gizmos work in local or global space.
        /// </summary>
        public enum SpaceMode
        {
            Global,
            Local,
        };

        #endregion

        #region Constants

        /// <summary>
        /// The size of the cursor region that affects the selection of the different gizmos.
        /// </summary>
        protected const int RegionSize = 20;

        #endregion

        #region Variables
        
        // The selected object.
        // If there are more then the first is used to place the gizmo.
        protected GameObject3D selectedObject;

        // The selected object.
        // If there are more then the first is used to place the gizmo.
        protected List<GameObject3D> selectedObjects;

        protected static List<Matrix> selectedObjectsLocalMatrix;
        
        // Indicates what axis is selected.
        protected static  bool redAxisSelected,
                               greenAxisSelected,
                               blueAxisSelected;

        // Auxiliary structure.
        protected static Vector3[] vertices = new Vector3[7];

        // Shader to render the gizmo.
        protected static Shader constantShader;

        private static bool active;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Indicates if a gizmo is being manipulated or not.
        /// </summary>
        public static bool Active
        {
            get { return active && (redAxisSelected || greenAxisSelected || blueAxisSelected); }
            protected set { active = value; }
        } // Active

        /// <summary>
        /// Local or World Space.
        /// </summary>
        public static SpaceMode Space { get; set; }

        /// <summary>
        /// Picker to select gizmo axis.
        /// </summary>
        public static Picker Picker { get; set; }

        #endregion

        #region Gizmo Scale Center Orientation

        /// <summary>
        /// Calculate the center, scale and orientation of the gizmo.
        /// </summary>
        protected static void GizmoScaleCenterOrientation(GameObject3D selectedObject, GameObject3D gizmoCamera, out float scale, out Vector3 center, out Quaternion orientation)
        {
            center = Vector3.Zero;
            orientation = new Quaternion();
            // Center
            center = selectedObject.Transform.Position;
            
            // Orientation
            if (Space == SpaceMode.Local)
                orientation = selectedObject.Transform.Rotation;
            else
                orientation = new Quaternion();

            // Calculate the distance from the object to camera position.
            if (gizmoCamera.Camera.OrthographicProjection)
            {
                scale = gizmoCamera.Camera.OrthographicVerticalSize / 5;
            }
            else
            {
                Vector3 cameraToCenter = gizmoCamera.Camera.Position - center;
                float distanceToCamera = cameraToCenter.Length();
                scale = (distanceToCamera / 16) / (gizmoCamera.Camera.RenderTargetSize.Width / 1400f); // Arbitrary number.
            }
        } // GizmoScaleCenterOrientation

        #endregion

        #region Calculate 2D Mouse Direction

        /// <summary>
        /// Calculate how the mouse movement will affect the transformation amount.
        /// </summary>
        /// <param name="selectedObject">Selected object.</param>
        /// <param name="gizmoCamera">The camera.</param>
        /// <param name="direction">The direction indicates the axis.</param>
        /// <param name="transformationAmount">This value will be multiplied by the mouse position to calculate the transformation amount.</param>
        protected static void Calculate2DMouseDirection(GameObject3D selectedObject, GameObject3D gizmoCamera, Vector3 direction, out Vector2 transformationAmount)
        {
            // Calculate the center, scale and orientation of the gizmo.
            Vector3 center;
            Quaternion orientation;
            float scale;
            GizmoScaleCenterOrientation(selectedObject, gizmoCamera, out scale, out center, out orientation);

            // Calculate the gizmo matrix.
            Matrix transformationMatrix = Matrix.CreateScale(scale);
            transformationMatrix *= Matrix.CreateFromQuaternion(orientation);
            transformationMatrix *= Matrix.CreateTranslation(center);

            // Calculate the direction of movement for every possible combination.
            vertices[0] = Vector3.Transform(new Vector3(0, 0, 0), transformationMatrix);
            vertices[1] = Vector3.Transform(direction, transformationMatrix);

            Vector3[] screenPositions = new Vector3[2];
            screenPositions[0] = EngineManager.Device.Viewport.Project(vertices[0], gizmoCamera.Camera.ProjectionMatrix, gizmoCamera.Camera.ViewMatrix, Matrix.Identity);
            screenPositions[1] = EngineManager.Device.Viewport.Project(vertices[1], gizmoCamera.Camera.ProjectionMatrix, gizmoCamera.Camera.ViewMatrix, Matrix.Identity);

            Vector3 aux = screenPositions[1] - screenPositions[0];
            transformationAmount = new Vector2
            {
                X = aux.X/aux.Length(),
                Y = aux.Y/aux.Length()
            };
        } // Calculate2DMouseDirection

        #endregion

    } // Gizmo
} // XNAFinalEngine.Editor
