
#region Using directives
using System;
using System.Runtime.InteropServices;
using ARTKPManagedWrapper;
using System.Windows.Media.Media3D;
using Matrix = Microsoft.Xna.Framework.Matrix;
#endregion

namespace XNAFinalEngine.AugmentedReality
{

    /// <summary>
    /// Tracking usando ARToolKit Plus
    /// Se implemento la detección de un solo arreglo de marcadores. No hay flexibilidad en este apartado.
    /// </summary>
    public class ARToolKitPlus
    { 

        #region Variables

        /// <summary>
        /// Tracker
        /// </summary>
        private IntPtr tracker;

        /// <summary>
        /// View Matrix (calculada usando realidad aumentada)
        /// </summary>        
        private Matrix viewMatrix = Matrix.Identity;

        /// <summary>
        /// Projection Matrix (calculada usando realidad aumentada)
        /// </summary>
        private Matrix projectionMatrix = Matrix.Identity;

        /// <summary>
        /// WebCam
        /// </summary>
        private WebCam webCam = null;

        /// <summary>
        /// Pixel Format
        /// </summary>
        private ArManWrap.PIXEL_FORMAT arPixelFormat = ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_ABGR; //.PIXEL_FORMAT_RGB;

        #endregion

        #region Properties

        /// <summary>
        /// View Matrix (calculada usando realidad aumentada)
        /// </summary>
        public Matrix ViewMatrix { get { return viewMatrix; } }

        /// <summary>
        /// Projection Matrix (calculada usando realidad aumentada)
        /// </summary>
        public Matrix ProjectionMatrix { get { return projectionMatrix; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa el sistema de trackeo
        /// </summary>
        public ARToolKitPlus(WebCam _webCam)
        {
            webCam = _webCam;
            
            //create the AR Tracker for finding a single marker
            tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, webCam.Width, webCam.Height);
            if (tracker == IntPtr.Zero)
            {
                throw new Exception("ARToolKitPlus: Tracker construction failed.");
            }
            //get the Tracker description
            IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
            string desc = Marshal.PtrToStringAnsi(ipDesc);
            int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)arPixelFormat); //MOD

            string cameraCalibrationPath = "Augmented Reality/no_distortion.cal";

            string multiPath = "Augmented Reality/markerboard_480-499.cfg";

            int retVal = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multiPath, 1.0f, 2000.0f);
            if (retVal != 0)
            {
                throw new Exception("ARToolKitPlus: Tracker not initialized.");
            }

            bool use_id_bch = false;
            if (use_id_bch == true)
            {
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_BCH);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.250f);
            }
            else //id_simple (supposed to be robust)
            {
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
            }

            //ArManWrap.ARTKPSetThreshold(tracker, 160);
            bool autoThresh = ArManWrap.ARTKPIsAutoThresholdActivated(tracker);
            ArManWrap.ARTKPActivateAutoThreshold(tracker, true);

            ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
            ArManWrap.ARTKPSetUseDetectLite(tracker, false);
        } // Tracker

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
        } // Dispose

        #endregion

        #region Tracking

        /// <summary>
        /// Calculo la posicion de la camara segun el cuadro actual capturado por la camara web.
        /// </summary>
        public void Tracking()
        {
            try
            {
                float[] modelViewMatrix = new float[16];
                float[] projMatrix = new float[16];

                int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, webCam.FrameManaged); //uses ArDetectMarker internally (unless set to Lite)
                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);

                Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                viewMatrix = ConvertMatrix(wpfModelViewMatrix);
                Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);
                projectionMatrix = ConvertMatrix(wpfProjMatrix);
            }
            catch
            {
                throw new Exception("ARToolKitPlus: The tracking operation fails.");
            }
        } // Tracking

        #endregion

        #region Convert Matrix

        /// <summary>
        /// Convierte la matriz de WPF a XNA
        /// </summary>
        public Matrix ConvertMatrix(Matrix3D matrix3D)
        {
            Matrix matrix = Matrix.Identity;
            matrix.M11 = (float)matrix3D.M11;
            matrix.M12 = (float)matrix3D.M12;
            matrix.M13 = (float)matrix3D.M13;
            matrix.M14 = (float)matrix3D.M14;
            matrix.M21 = (float)matrix3D.M21;
            matrix.M22 = (float)matrix3D.M22;
            matrix.M23 = (float)matrix3D.M23;
            matrix.M24 = (float)matrix3D.M24;
            matrix.M31 = (float)matrix3D.M31;
            matrix.M32 = (float)matrix3D.M32;
            matrix.M33 = (float)matrix3D.M33;
            matrix.M34 = (float)matrix3D.M34;
            matrix.M41 = (float)matrix3D.OffsetX;
            matrix.M42 = (float)matrix3D.OffsetY;
            matrix.M43 = (float)matrix3D.OffsetZ;
            matrix.M44 = (float)matrix3D.M44;
            return matrix;
        } // ConvertMatrix

        #endregion

    } // ARToolKitPlus
} // XNA2FinalEngine.AugmentedReality
