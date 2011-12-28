
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
using System.Collections.Generic;
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
    /// <remarks>
    /// Viewport implementation:
    /// There are two main approaches: viewports vs. multiple render targets.
    /// The multiple render targets do not change the shader code. Only the C# has to now about the split screen.
    /// Using viewports changes the shader code because when you read a previous calculation, for instance the GBuffer, you have to read only half the buffer.
    /// This is easy to implement, but it cost processing power and it adds a little of complexity.
    /// The viewport approach has its advantages thought. If you want that the viewport changes its size over time
    /// the viewport approach does not have any performance penalty, but maybe the penalty could be absorbed in the render target approach. 
    /// </remarks>
    public class Camera : Component
    {

        #region Enumerates

        public enum RenderingType
        {
            /// <summary>
            /// Deferred lighting (or Light Pre-Pass) rendering.
            /// The default rendering type.
            /// Use it for the rendering of the scene, or any king of rendering.
            /// </summary>
            DeferredLighting,
            /// <summary>
            /// Classic forward rendering.
            /// Some graphic effects and material don't work in this renderer.
            /// Use it for auxiliary rendering, like reflections.
            /// </summary>
            ForwardRendering,
        } // RenderingType

        #endregion

        #region Variables

        /// <summary>
        /// This is the cached world matrix from the transform component.
        /// This matrix represents the view matrix.
        /// </summary>
        internal Matrix cachedWorldMatrix;
        
        // Where on the screen is the camera rendered in clip space.
        private RectangleF normalizedViewport;

        // Where on the screen is the camera rendered in screen space.
        private Rectangle viewport;

        // The viewport is expressed in clip space or screen space?
        private bool viewportExpressedInClipSpace;

        // The master of this slave camera. A camera can't be master and slave simultaneity.
        private Camera masterCamera;

        // The slaves of this camera. A camera can't be master and slave simultaneity.
        internal readonly List<Camera> slavesCameras = new List<Camera>();

        // Destination render texture.
        private RenderTarget renderTarget;

        // Deferred lighting or forward rendering?
        private RenderingType renderer;
        
        #region Projection

        // Projection matrix.
        private Matrix projectionMatrix;

        // Use the projection matrix that the user set. 
        // If a projection's value changes there will no change in the projection matrix until the user call the reset projection matrix method.
        private bool useUserProjectionMatrix;

        /// <summary>
        /// Aspect Ratio. O means system aspect ratio.
        /// </summary>
        private float aspectRatio;

        /// <summary>
        /// Field of view, near plane and far plane.
        /// </summary>
        private float nearPlane,
                      farPlane,
                      fieldOfView;
        
        // Is the camera orthographic (true) or perspective (false)?
        private bool orthographic;

        // Camera's vertical size when in orthographic mode.
        private int orthographicVerticalSize;

        private AmbientLight ambientLight;

        private PostProcess postProcess;

        private Size renderTargetSize;
        
        #endregion

        #endregion

        #region Properties

        #region Clear Color

        /// <summary>
        /// The color with which the screen will be cleared.
        /// </summary>
        public Color ClearColor { get; set; }

        // ClearColor

        #endregion

        #region Ambient Light

        /// <summary>
        /// Ambient light.
        /// </summary>
        public AmbientLight AmbientLight
        {
            get { return ambientLight; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                ambientLight = value;
            }
        } // AmbientLight

        #endregion

        #region Renderer

        /// <summary>
        /// Deferred lighting or forward rendering?
        /// </summary>
        public RenderingType Renderer
        {
            get { return renderer; }
            set  { renderer = value; }
        } // Renderer

        #endregion

        #region Sky

        /// <summary>
        /// The background sky shader.
        /// </summary>
        public Sky Sky { get; set; }

        #endregion

        #region Post Process

        /// <summary>
        /// Post process effects applied to this camera.
        /// </summary>
        public PostProcess PostProcess
        {
            get { return postProcess; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                postProcess = value;
            }
        } // PostProcess

        #endregion

        #region Render Target

        /// <summary>
        /// The image with the rendering of the scene. 
        /// This render target stores both, the master camera’s result and its slaves camera’s results.
        /// The render target is automatically set.
        /// </summary>
        public RenderTarget RenderTarget
        {
            get { return renderTarget; }
            internal set
            {
                if (masterCamera != null)
                    return;
                renderTarget = value;
                for (int i = 0; i < slavesCameras.Count; i++)
                    slavesCameras[i].renderTarget = value;
            }
        } // RenderTarget

        /// <summary>
        /// 
        /// </summary>
        internal RenderTarget PartialRenderTarget { get; set; }

        /// <summary>
        /// Render Target Size.
        /// </summary>
        public Size RenderTargetSize
        {
            get { return renderTargetSize; }
            set
            {
                if (masterCamera != null)
                    return;
                renderTargetSize = value;
                for (int i = 0; i < slavesCameras.Count; i++)
                    slavesCameras[i].renderTargetSize = value;
            }
        } // RenderTargetSize

        #endregion

        #region Master Camera

        /// <summary>
        /// If you want to render the scene using more than one camera in a single render target (for example: split screen) you have to relate the cameras.
        /// It does not matter who is the master, however, the rendering type, clear color and render target values come from the master camera.
        /// </summary>
        public Camera MasterCamera
        {
            get { return masterCamera; }
            set
            {
                if (slavesCameras.Count != 0 || (value != null && value.masterCamera != null))
                    throw new InvalidOperationException("Camera: A camera can't be master and slave simultaneity.");
                // Remove this camera from the old master.
                if (masterCamera != null)
                    masterCamera.slavesCameras.Remove(this);
                // Set new master.
                masterCamera = value;
                // Update master with its new slave.
                if (value != null)
                {
                    value.slavesCameras.Add(this);
                    // Just to be robust...
                    // I update the children values so that, in the case of a unparent, the values remain the same as the father.
                    renderTarget = value.RenderTarget;
                    renderTargetSize = value.RenderTargetSize;
                }
            }
        } // MasterCamera

        #endregion

        #region View

        /// <summary>
        /// View Matrix.
        /// If you change this matrix, the camera no longer updates its rendering based on its Transform.
        /// This lasts until you call ResetWorldToCameraMatrix.
        /// </summary>
        public Matrix ViewMatrix { get { return cachedWorldMatrix; } }

        /// <summary>Camera position.</summary>
        /// <remarks>The view matrix is the inverse of the camera's world matrix. Consequently the view matrix can't be used directly.</remarks>
        public Vector3 Position { get; private set; }

        /// <summary>Camera up vector.</summary>
        /// <remarks>The view matrix is the inverse of the camera's world matrix. Consequently the view matrix can't be used directly.</remarks>
        public Vector3 Up { get; private set; }

        /// <summary>Camera forward vector.</summary>
        /// <remarks>The view matrix is the inverse of the camera's world matrix. Consequently the view matrix can't be used directly.</remarks>
        public Vector3 Forward { get; private set; }

        #endregion

        #region Projection

        /// <summary> 
        /// We can set the projection matrix.
        /// If a projection's value changes there will no change in the projection matrix until the user call the reset projection matrix method.
        /// </summary>
        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            set
            {
                projectionMatrix = value;
                useUserProjectionMatrix = true;
            }
        } // ProjectionMatrix

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
                    return ((float)Viewport.Width / Viewport.Height);
                return aspectRatio;
            }
            set
            {
                if (value <= 0)
                    throw new Exception("Camera: the aspect ratio has to be a positive real number.");
                if (aspectRatio == 0)
                    Screen.AspectRatioChanged -= OnAspectRatioChanged;
                if (value == 0)
                    Screen.AspectRatioChanged += OnAspectRatioChanged;
                aspectRatio = value;
                CalculateProjectionMatrix();
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
                CalculateProjectionMatrix();
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
                CalculateProjectionMatrix();
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
                CalculateProjectionMatrix();
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
                CalculateProjectionMatrix();
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
                CalculateProjectionMatrix();
            }
        } // OrthographicVerticalSize

        #endregion

        #region Viewport

        /// <summary>
        /// Where on the screen is the camera rendered in clip space.
        /// Values: left, bottom, width, height.
        /// The normalized values should update with screen size changes.
        /// </summary>
        public RectangleF NormalizedViewport
        {
            get
            {
                if (viewportExpressedInClipSpace)
                    return normalizedViewport;
                return new RectangleF((float)viewport.X / (float)Screen.Width, (float)viewport.Y / (float)Screen.Height,
                                      (float)viewport.Width / (float)Screen.Width, (float)viewport.Height / (float)Screen.Height);
            }
            set
            {
                if (value.X < 0 || value.Y < 0 || (value.X + value.Width) > 1 || (value.Y + value.Height) > 1)
                    throw new ArgumentException("Camera: viewport size invalid.", "value");
                viewportExpressedInClipSpace = true;
                normalizedViewport = value;
                CalculateProjectionMatrix(); // The viewport could affect the aspect ratio.
            }
        } // NormalizedViewport

        /// <summary>
        /// Where on the screen is the camera rendered in screen space.
        /// Values: left, bottom, width, height.
        /// These values won't be updated with screen size changes.
        /// </summary>
        public Rectangle Viewport
        {
            get
            {
                if (viewportExpressedInClipSpace)
                    return new Rectangle((int)(normalizedViewport.X * Screen.Width), (int)(normalizedViewport.Y * Screen.Height),
                                         (int)(normalizedViewport.Width * Screen.Width), (int)(normalizedViewport.Height * Screen.Height));
                // Check for viewport dimensions?
                return viewport;
            }
            set
            {
                if (value == Rectangle.Empty || value.X < 0 || value.Y < 0 || (value.X + value.Width) > RenderTarget.Width || (value.Y + value.Height) > RenderTarget.Height)
                    throw new ArgumentException("Camera: viewport size invalid.", "value");
                viewportExpressedInClipSpace = false;
                viewport = value;
                CalculateProjectionMatrix(); // The viewport could affect the aspect ratio.
            }
        } // Viewport

        /// <summary>
        /// True if the camera needs a viewport (split screen) for render.
        /// </summary>
        public bool NeedViewport { get { return NormalizedViewport != new RectangleF(0, 0, 1, 1); } }

        #endregion
        
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the component. 
        /// </summary>
        internal override void Initialize(GameObject owner)
        {
            base.Initialize(owner);
            // Values //
            postProcess = new PostProcess();
            ambientLight = new AmbientLight();
            nearPlane = 0.1f;
            farPlane = 1000.0f;
            fieldOfView = 36;
            aspectRatio = 0;
            useUserProjectionMatrix = false;
            renderer = RenderingType.DeferredLighting;
            Sky = null;
            masterCamera = null;
            normalizedViewport = new RectangleF(0, 0, 1, 1);
            viewportExpressedInClipSpace = true;
            viewport = Rectangle.Empty;
            slavesCameras.Clear();
            renderTargetSize = Size.FullScreen;
            ClearColor = new Color(20, 20, 20, 255);
            orthographicVerticalSize = 10;
            // Generate the projection matrix.
            CalculateProjectionMatrix();
            Screen.AspectRatioChanged += OnAspectRatioChanged;
            // Cache transform matrix. It will be the view matrix.
            ((GameObject3D)Owner).Transform.WorldMatrixChanged += OnWorldMatrixChanged;
            OnWorldMatrixChanged(((GameObject3D) Owner).Transform.WorldMatrix);
        } // Initialize
        
        #endregion

        #region Uninitialize

        /// <summary>
        /// Uninitialize the component.
        /// Is important to remove event associations and any other reference.
        /// </summary>
        internal override void Uninitialize()
        {
            base.Uninitialize();
            if (aspectRatio == 0)
                Screen.AspectRatioChanged -= OnAspectRatioChanged;
            ((GameObject3D)Owner).Transform.WorldMatrixChanged -= OnWorldMatrixChanged;
        } // Uninitialize

        #endregion

        #region Calculate and Reset Projection Matrix

        /// <summary>
        /// Update projection matrix based in the camera's projection properties.
        /// </summary>
        public void ResetProjectionMatrix()
        {
            useUserProjectionMatrix = false;
            CalculateProjectionMatrix();
        } // ResetProjectionMatrix

        /// <summary>
        /// Update projection matrix based in the camera's projection properties.
        /// This is only executed if the user does not set a projection matrix.
        /// </summary>
        private void CalculateProjectionMatrix()
        {
            if (!useUserProjectionMatrix)
            {
                if (OrthographicProjection)
                    projectionMatrix = Matrix.CreateOrthographic(OrthographicVerticalSize * AspectRatio, OrthographicVerticalSize, NearPlane, FarPlane);
                else
                    projectionMatrix = Matrix.CreatePerspectiveFieldOfView(3.1416f * FieldOfView / 180.0f, AspectRatio, NearPlane, FarPlane);
            }
        } // CalculateProjectionMatrix

        #endregion
        
        #region Bounding Frustum

        // To avoid garbage use always the same values.
        private static Vector3[] cornersWorldSpace = new Vector3[8];
        private static BoundingFrustum boundingFrustum = new BoundingFrustum(Matrix.Identity);

        /// <summary>
        /// Camera Far Plane Bounding Frustum (in view space). 
        /// With the help of the bounding frustum, the position can be cheaply reconstructed from a depth value.
        /// 
        /// </summary>
        public void BoundingFrustum(Vector3[] cornersViewSpace)
        {
            if (cornersViewSpace.Length != 4)
                throw new ArgumentOutOfRangeException("cornersViewSpace");
            boundingFrustum.Matrix = ViewMatrix * ProjectionMatrix;
            boundingFrustum.GetCorners(cornersWorldSpace);
            // Transform form world space to view space
            for (int i = 0; i < 4; i++)
            {
                cornersViewSpace[i] = Vector3.Transform(cornersWorldSpace[i + 4], ViewMatrix);
            }

            // Swap the last 2 values.
            Vector3 temp = cornersViewSpace[3];
            cornersViewSpace[3] = cornersViewSpace[2];
            cornersViewSpace[2] = temp;
        } // BoundingFrustum

        #endregion

        #region On Aspect Ratio Changed

        /// <summary>
        /// When the system aspect ratio changes then the projection matrix has to be recalculated.
        /// </summary>
        private void OnAspectRatioChanged(object sender, EventArgs e)
        {
            CalculateProjectionMatrix();
        } // OnAspectRatioChanged

        #endregion

        #region On World Matrix Changed

        /// <summary>
        /// On transform's world matrix changed.
        /// </summary>
        protected virtual void OnWorldMatrixChanged(Matrix worldMatrix)
        {
            // The view matrix is the invert
            cachedWorldMatrix = Matrix.Invert(worldMatrix);
            Position = worldMatrix.Translation;
            Up = worldMatrix.Up;
            Forward = worldMatrix.Forward;
        } // OnWorldMatrixChanged

        #endregion

        #region Pool

        // Pool for this type of components.
        private static readonly Pool<Camera> componentPool = new Pool<Camera>(20);

        /// <summary>
        /// Pool for this type of components.
        /// </summary>
        internal static Pool<Camera> ComponentPool { get { return componentPool; } }

        #endregion

    } // Camera
} // XNAFinalEngine.Components