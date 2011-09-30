
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
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Components
{

    /// <summary>
    /// Camera component.
    /// </summary>
    public class Camera : Component
    {

        #region Variables

        /// <summary>
        /// This is the cached world matrix from the transform component.
        /// This matrix represents the view matrix.
        /// </summary>
        private Matrix cachedWorldMatrix;

        /// <summary>
        /// Use transform matrix or user view matrix.
        /// </summary>
        private bool useTransformMatrix = true;

        #region Projection

        /// <summary>
        /// Aspect Ratio. O means system aspect ratio.
        /// </summary>
        private float aspectRatio = 0;

        /// <summary>
        /// Field of view, near plane and far plane.
        /// </summary>
        private float nearPlane = 0.1f,
                      farPlane = 1000.0f,
                      fieldOfView = 36;

        /// <summary>
        /// Camera's vertical size when in orthographic mode.
        /// </summary>
        private int orthographicVerticalSize = 10;

        /// <summary>
        ///  Where on the screen is the camera rendered in normalized coordinates.
        /// </summary>
        private RectangleF normalizedViewport = new RectangleF(0, 0, 1, 1);

        private Rectangle viewport = Rectangle.Empty;

        /// <summary>
        /// Is the camera orthographic (true) or perspective (false)?
        /// </summary>
        private bool orthographic;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Destination render texture.
        /// XNA Final Engine works in linear space and in High Dynamic Range.
        /// If you want proper results use a floating point texture.
        /// </summary>
        public RenderTarget RenderTarget { get; set; }

        #region View

        /// <summary>
        /// View Matrix.
        /// If you change this matrix, the camera no longer updates its rendering based on its Transform.
        /// This lasts until you call ResetWorldToCameraMatrix.
        /// </summary>
        public Matrix ViewMatrix
        {
            get { return cachedWorldMatrix; }
            set 
            {
                cachedWorldMatrix = value;
                useTransformMatrix = false;
            }
        } // ViewMatrix

        #endregion

        #region Projection

        /// <summary> 
        /// We can set the projection matrix.
        /// However if a parameter changes or the projection matrix is manually updated then this value will be replaced.
        /// </summary>
        public Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// The camera's aspect ratio (width divided by height).
        /// Default value: system aspect ratio.
        /// If you modify the aspect ratio of the camera, the value will stay until you call camera.ResetAspect(); which resets the aspect to the screen's aspect ratio.
        /// If the aspect ratio is set to system aspect ratio then the result will consider the viewport selected.
        /// </summary>
        public float AspectRatio
        {
            get
            {
                if (aspectRatio == 0)
                {
                    return Screen.AspectRatio;
                }
                return aspectRatio;
            }
            set
            {
                if (value <= 0)
                    throw new Exception("Camera: the aspect ratio has to be a positive real number.");
                aspectRatio = value;
                ResetProjectionMatrix();
            }
        } // AspectRatio

        /// <summary>
        /// Field of View.
        /// This is the vertical field of view; horizontal FOV varies depending on the viewport's aspect ratio.
        /// Field of view is ignored when camera is orthographic 
        /// Unit: degrees.
        /// Default value: 36
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                fieldOfView = value;
                ResetProjectionMatrix();
            }
        } // FieldOfView

        /// <summary>
        /// Near Plane.
        /// Default Value: 0.1
        /// </summary>
        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                nearPlane = value;
                ResetProjectionMatrix();
            }
        } // NearPlane

        /// <summary>
        /// Far Plane.
        /// Defautl Value: 1000
        /// </summary>
        public float FarPlane
        {
            get { return farPlane; }
            set
            {
                farPlane = value;
                ResetProjectionMatrix();
            }
        } // FarPlane

        /// <summary> 
        /// Is the camera orthographic (true) or perspective (false)?
        /// Unlike perspective projection, in orthographic projection there is no perspective foreshortening.
        /// </summary>
        public bool OrthographicProjection
        {
            get { return orthographic; }
            set
            {
                orthographic = value;
                ResetProjectionMatrix();
            }
        } // OrthographicProjection

        /// <summary>
        /// Camera's vertical size when in orthographic mode. 
        /// The horizontal value} is calculated automaticaly with the aspect ratio property. 
        /// </summary>
        public int OrthographicVerticalSize
        {
            get { return orthographicVerticalSize; }
            set
            {
                orthographicVerticalSize = value;
                ResetProjectionMatrix();
            }
        } // OrthographicVerticalSize

        #endregion

        #region Viewport

        /// <summary>
        /// Where on the screen is the camera rendered in clip space.
        /// Values: left, bottom, width, height.
        /// </summary>
        public RectangleF NormalizedViewport { get; set; }

        /// <summary>
        /// Where on the screen is the camera rendered in screen space.
        /// Values: left, bottom, width, height.
        /// </summary>
        public RectangleF Viewport { get; set; }

        #endregion

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {     
            // Generate the projection matrix.
            ResetProjectionMatrix();            
        } // Initialize

        #endregion

        #region Reset Values

        /// <summary>
        /// Resets the aspect to the screen's aspect ratio.
        /// </summary>
        public void ResetAspectRatio()
        {
            aspectRatio = 0;
            ResetProjectionMatrix();
        } // ResetAspectRatio

        /// <summary>
        /// Make the transform world matrix reflect the camera's position in the scene.
        /// </summary>
        public void ResetViewMatrix()
        {            
            cachedWorldMatrix = ((GameObject3D)Owner).Transform.WorldMatrix;
            useTransformMatrix = true;
        } // ResetViewMatrix

        /// <summary>
        /// Update projection matrix based in the camera's projection properties.
        /// </summary>
        protected void ResetProjectionMatrix()
        {
            if (OrthographicProjection)
                ProjectionMatrix = Matrix.CreateOrthographic(OrthographicVerticalSize * AspectRatio, OrthographicVerticalSize, NearPlane, FarPlane);
            else
                ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(3.1416f * FieldOfView / 180.0f, AspectRatio, NearPlane, FarPlane);
        } // ResetProjectionMatrix

        #endregion

    } // Camera
} // XNAFinalEngine.Components