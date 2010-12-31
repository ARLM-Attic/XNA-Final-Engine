
#region Using directives
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Tao.OpenGl;
using Tao.FreeGlut;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.AugmentedReality
{
    /// <summary>
    /// ARTag fiducial marker system.
    /// Important: For this to work the computer clock needs to be set to a year prior to 2008.
    /// It also unnecessary needs OpenGL.
    /// If the application has administrator privileges, the system year changes automatically to 2007 and will be restore when the application exits.
    /// </summary>
    public class ARTag : Disposable
    {

        #region Constants

        /// <summary>
        /// The name of the file that specify the arrays of markers that can be recognized by ARTag.
        /// This file can be edited or extended.
        /// </summary>
        const string MarkerArrayFilename = "ARTagMarkers.cf";

        /// <summary>
        /// The webcam's capture is in color or in gray scale? 1 color, 0 gray scale.
        /// </summary>
        private const char RGBGreybar = (char)1;

        #endregion

        #region Structs

        /// <summary>
        /// Struct to manage the array of markers.
        /// It needs to be class instead of struct because class is by reference and struct is by value.
        /// </summary>
        public class MarkerArray
        {
            /// <summary>
            /// ARTag ID.
            /// </summary>
            public int id;
            /// <summary>
            /// The name of the array of markers in the marker file.
            /// </summary>
            public string name;
            /// <summary>
            /// The view matrix returned by the ARTag's tracker.
            /// It contains the last view matrix in which the markers were founded.
            /// </summary>
            public Matrix viewMatrix = Matrix.Identity;
            /// <summary>
            /// Was founded by the tracker?
            /// </summary>
            public bool isFound;
        }

        #endregion

        #region Variables

        /// <summary>
        /// Auxiliary matrix.
        /// </summary>
        private readonly float[] modelViewMatrix = new float[16];

        /// <summary>
        /// The webCam used by ARTag.
        /// </summary>
        private readonly WebCam webCam;
            
        /// <summary>
        /// Webcam raw information pointer.
        /// </summary>
        private IntPtr webcamRawInformationPointer;

        /// <summary>
        /// Focal Lengths. Approx 850-1150 for 640x480 Dragonflies, and about 400 for 320x240 webcam.
        /// Logitech WebCam PRO 900 is about 510 for 640x480
        /// </summary>
        private int focalLengthX, focalLengthY;

        /// <summary>
        /// The arrays of markers to be tracked.
        /// </summary>
        protected List<MarkerArray> markerArrayList = new List<MarkerArray>();
                        
        #endregion

        #region Properties

        /// <summary>
        /// Projection Matrix.
        /// </summary>
        public Matrix ProjectionMatrix { get { return Matrix.CreatePerspectiveFieldOfView((float)webCam.Width / (float)(2.0 * focalLengthX), (float)webCam.Width / (float)webCam.Height, ApplicationLogic.Camera.NearPlane, ApplicationLogic.Camera.FarPlane); } }

        /// <summary>
        /// Focal Length X. Approx 850-1150 for 640x480 Dragonflies, and about 400 for 320x240 webcam.
        /// </summary>
        public int FocalLengthX
        {
            get { return focalLengthX; }
            set {
                focalLengthX = value;
                artag_set_camera_params_wrapped(focalLengthX, focalLengthY, webCam.Width / 2.0, webCam.Height / 2.0);
                GenerateProjectionMatrix();
            }
        } // FocalLengthX

        /// <summary>
        ///     Focal Length Y. Approx 850-1150 for 640x480 Dragonflies, and about 400 for 320x240 webcam.
        /// </summary>
        public int FocalLengthY
        {
            get { return focalLengthY; }
            set
            {
                focalLengthY = value;
                artag_set_camera_params_wrapped(focalLengthX, focalLengthY, webCam.Width / 2.0, webCam.Height / 2.0);
                GenerateProjectionMatrix();
            }
        } // FocalLengthY

        #endregion

        #region Constructor

        /// <summary>
        /// Init the ARTag system. 
        /// OpenGL needs to be up and running (an ARTag limitation).
        /// </summary>
        /// <param name="_webCam">A 3 bytes per pixel webCam</param>
        public ARTag(WebCam _webCam)
        {
            webCam = _webCam;
            InitOpenGL();
            InitARTag();
        } // ARTag

        /// <summary>
        /// Init Opengl.
        /// </summary>
        public void InitOpenGL()
        {
            // Init the GLUT
            Glut.glutInit();

            // Creates the OpenGL window. It's necessary because the OpenGL's matrices can't work otherwise.
            Glut.glutInitWindowSize(1, 1);
            Glut.glutInitWindowPosition(1, 1);
            Glut.glutCreateWindow("OpenGL ARTag");

            // Hide this window.
            Glut.glutHideWindow();
        } // InitOpenGL

        /// <summary>
        /// Init ARTag.
        /// </summary>
        public void InitARTag()
        {
            ChangeYear(2007);
            // ARTag initialization 
            if (init_artag_wrapped(webCam.Width, webCam.Height, webCam.BytesPerPixel) == 1)
            {
                throw new Exception("ARTag: Can't start ARTag.");
            }

            // Set default focal length (FX and FY) values.
            // This values are based in the Logitech WebCam PRO 9000.
            switch (webCam.Width)
            {
                case 320: focalLengthX = focalLengthY = 255; break;
                case 640: focalLengthX = focalLengthY = 510; break;
                case 800: focalLengthX = focalLengthY = 640; break;
                case 920: focalLengthX = focalLengthY = 733; break;
                default: focalLengthX = focalLengthY = 510; break;
            }
            artag_set_camera_params_wrapped(focalLengthX, focalLengthY, webCam.Width / 2.0, webCam.Height / 2.0);

            // Loads the ARTag's marker file. The file with the list of posible markers to be used.
            if (load_array_file_wrapped(MarkerArrayFilename) == -1)
            {
                throw new Exception("ARTag: Error loading the array file");
            }
            
            // Generates the OpenGL's projection matrix.
            GenerateProjectionMatrix();
        } // InitARTag
                
        #endregion

        #region Add Artag Marker Array

        /// <summary>
        /// Add the ARTag marker array. It needs to be part of the markers’ array file.
        /// This marker array will be tracked by the ARTag system automatically.
        /// The return variable it’s a struct that will store primarily the viewmatrix calculated every frame (if the ARTag system it’s running).
        /// </summary>
        public MarkerArray AddArtagMarkerArray(string name)
        {
            MarkerArray markerArray = new MarkerArray
            {
                name = name, id = artag_associate_array_wrapped(name)
            };
            if (markerArray.id == -1)
            {
                throw new Exception("ARTag: Error associating the array");
            }
            // It is added to the list of markers' arrays
            markerArrayList.Add(markerArray);
            
            return markerArray;
        } // AddArtagMarkerArray

        #endregion

        #region Generate Projection Matrix

        /// <summary>
        /// Generates the projection matrix from the webcam information.
        /// </summary>
        private void GenerateProjectionMatrix()
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            double dRight = (double)webCam.Width / (double)(2.0 * focalLengthX);
            double dLeft = -dRight;
            double dTop = (double)webCam.Height / (double)(2.0 * focalLengthY);
            double dBottom = -dTop;
            Gl.glFrustum(dLeft, dRight, dBottom, dTop, 1.0, 1000.0f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        } // GenerateProjectionMatrix

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            // close the window and start the loop
            Glut.glutDestroyWindow(1);
            // Shut down ARTag
            close_artag_wrapped();
            ChangeYear(oldYear);
        } // Dispose

        #endregion

        #region Tracking

        /// <summary>
        /// Tracking for markers.
        /// </summary>
        public void Tracking()
        {            
            
            // Call ARTag to look for objects in camera frames
            try
            {
                // Get pointer to frame - using SharperCV wrapper for OpenCV
                webcamRawInformationPointer = webCam.FrameUnmanaged;

                //artag_find_objects() it searches the image for markers, and finds those that belong to defined objects
                artag_find_objects_wrapped(webcamRawInformationPointer, RGBGreybar);
            }
            catch (AccessViolationException)
            {
                throw new Exception("ARTag: The tracking operation fails.\nThe more probably cause is that the webcam's image don´t have the size o bytes per pixel correct.");
            }
            MarkerArray[] marketArrayArray = markerArrayList.ToArray();
            for (int i = 0; i < markerArrayList.Count; i++)
            {   
                char foundResult = artag_is_object_found_wrapped(marketArrayArray[i].id);
                if (foundResult == (char)1) // If this marker was founded.
                {                    
                    artag_set_object_opengl_matrix_wrapped(marketArrayArray[i].id, (char)0);
                    Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, modelViewMatrix);
                    marketArrayArray[i].viewMatrix = Matrix.CreateFromYawPitchRoll(0, 3.1416f/2, 0) // Rotate the matrix 90 degres.
                                                     * ConvertMatrix(modelViewMatrix);
                    if (marketArrayArray[i].viewMatrix == Matrix.Identity)
                    {
                        throw new Exception("ARTag: The openGL's matrices isn´t working");
                    }
                    marketArrayArray[i].isFound = true;
                }
                else
                {
                    marketArrayArray[i].isFound = false;
                }
            }
        } // Tracking

        #endregion

        #region ARTag Wrapper

        // Import the Win32 DLL sample function
        [DllImport("ARTagWrapper.dll", EntryPoint = "fnARTagWrapper")]
        public static extern int fnARTagWrapper();

        // ARTag Wrapper: ARTag Init
        [DllImport("ARTagWrapper.dll", EntryPoint = "init_artag_wrapped")]
        public static extern char init_artag_wrapped(int width, int height, int bpp);

        // ARTag Wrapper: ARTag Close
        [DllImport("ARTagWrapper.dll", EntryPoint = "close_artag_wrapped")]
        public static extern void close_artag_wrapped();

        // ARTag Wrapper: load_array_file
        // int load_array_file(char* filename);   //returns -1 if file not found
        [DllImport("ARTagWrapper.dll", EntryPoint = "load_array_file_wrapped")]
        public static extern int load_array_file_wrapped(string filename);

        // ARTag Wrapper: artag_associate_array
        //-associate an array with an object, this function will return an ID to use in future
        //calls.  "frame_name" is the same as in the array .cf file which must be loaded first.
        //-if the return value is -1, the object could not be initialized
        //example: base_artag_object_id=artag_associate_array("base0");
        //int artag_associate_array(char* frame_name);  //returns artag_object_id
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_associate_array_wrapped")]
        public static extern int artag_associate_array_wrapped(string frameName);

        // ARTag Wrapper: artag_associate_marker
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_associate_marker_wrapped")]
        public static extern int artag_associate_marker_wrapped(int artagId);

        // ARTag Wrapper: artag_find_objects
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_find_objects_wrapped")]
        //public static extern void artag_find_objects_wrapped([In,Out] ref IntPtr rgb_cam_image, char rgb_greybar);
        public static extern int artag_find_objects_wrapped([In, Out] IntPtr rgbCamImage, char rgbGreybar);

        // ARTag Wrapper: artag_is_object_found
        // artag_is_object_found(artag_object_num) returns 1 if object was found from most
        // recent artag_find_objects() call, returns 0 if object was not found 
        // char artag_is_object_found(int artag_object_num);
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_is_object_found_wrapped")]
        public static extern char artag_is_object_found_wrapped(int artagObjectNum);

        // ARTag Wrapper: artag_get_object_opengl_matrix
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_set_object_opengl_matrix_wrapped")]
        public static extern void artag_set_object_opengl_matrix_wrapped(int objectNum, char mirrorOn);

        // ARTag Wrapper: artag_set_camera_params_wrapped
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_set_camera_params_wrapped")]
        public static extern void artag_set_camera_params_wrapped(double cameraFx, double cameraFy, double cameraCx, double cameraCy);

        // ARTag Wrapper: artag_create_marker
        //artag_create_marker() will fill an unsigned char array with 100*scale*scale bytes
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_create_marker_wrapped")]
        public static extern int artag_create_marker_wrapped(int artagId, int scale, [In][Out] ref IntPtr image);

        // ARTag Wrapper: 
        //void artag_set_output_image_mode(void);     //turn on output debug image writing 
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_set_output_image_mode_wrapped")]
        public static extern void artag_set_output_image_mode_wrapped();

        // ARTag Wrapper: 
        //void artag_clear_output_image_mode(void);   //turn off output debug image writing
        [DllImport("ARTagWrapper.dll", EntryPoint = "artag_clear_output_image_mode_wrapped")]
        public static extern void artag_clear_output_image_mode_wrapped();

        // ARTag Utils

        // ARTag Wrapper: write_pgm
        //void write_pgm(char *file_name, char *comment, unsigned char *image,int width,int height)
        [DllImport("ARTagWrapper.dll", EntryPoint = "write_pgm_wrapped")]
        public static extern void write_pgm_wrapped(string fileName, string comment, [In] IntPtr image, int width, int height);

        // ARTag Wrapper: write_ppm
        //void write_ppm_wrapped(char *file_name, char *comment, unsigned char *image,int width,int height)
        [DllImport("ARTagWrapper.dll", EntryPoint = "write_ppm_wrapped")]
        public static extern void write_ppm_wrapped(string fileName, string comment, [In] IntPtr image, int width, int height);

        // ARTag Wrapper: read_ppm_wrapped
        //unsigned char* read_ppm_wrapped(char *file_name, int *width, int *height)
        [DllImport("ARTagWrapper.dll", EntryPoint = "read_ppm_wrapped")]
        public static extern IntPtr read_ppm_wrapped(string fileName, ref int width, ref int height);

        #endregion ARTag Function Import

        #region Change Year
        
        [DllImport("kernel32 ", SetLastError = true)]
        private static extern bool GetSystemTime(out SystemTime systemTime);

        [DllImport("kernel32 ", SetLastError = true)]
        private static extern bool SetSystemTime(ref SystemTime systemTime);
        
        /// <summary>
        /// System time (in UTC time format)
        /// </summary>
        private struct SystemTime
        {
            internal short wYear;
            internal short wMonth;
            internal short wDayOfWeek;
            internal short wDay;
            internal short wHour;
            internal short wMinute;
            internal short wSecond;
            internal short wMilliseconds;
        } // SystemTime

        /// <summary>
        /// Stores the actual system year.
        /// </summary>
        private static short oldYear;

        /// <summary>
        /// Change the system year.
        /// </summary>
        public static void ChangeYear(short newYear)
        {
            SystemTime st;

            GetSystemTime(out st); // Get time
            oldYear = st.wYear;    // Save current year
            st.wYear = newYear;
            SetSystemTime(ref st); // Set time
        } // ChangeYear

        #endregion

        #region Convert Matrix

        /// <summary>
        /// Converts the matrix from WPF to XNA
        /// </summary>
        private static Matrix ConvertMatrix(float[] _matrix)
        {
            Matrix matrix = Matrix.Identity;
            matrix.M11 = _matrix[0];//(float)matrix3D.M11;
            matrix.M12 = _matrix[1];//(float)matrix3D.M12;
            matrix.M13 = _matrix[2];//(float)matrix3D.M13;
            matrix.M14 = _matrix[3];//(float)matrix3D.M14;
            matrix.M21 = _matrix[4];//(float)matrix3D.M21;
            matrix.M22 = _matrix[5];//(float)matrix3D.M22;
            matrix.M23 = _matrix[6];//(float)matrix3D.M23;
            matrix.M24 = _matrix[7];//(float)matrix3D.M24;
            matrix.M31 = _matrix[8];//(float)matrix3D.M31;
            matrix.M32 = _matrix[9];//(float)matrix3D.M32;
            matrix.M33 = _matrix[10];//(float)matrix3D.M33;
            matrix.M34 = _matrix[11];//(float)matrix3D.M34;
            matrix.M41 = _matrix[12];//(float)matrix3D.OffsetX;
            matrix.M42 = _matrix[13];//(float)matrix3D.OffsetY;
            matrix.M43 = _matrix[14];//(float)matrix3D.OffsetZ;
            matrix.M44 = _matrix[15];//(float)matrix3D.M44;
            return matrix;
        } // ConvertMatrix

        #endregion

    } // ARTag
} // XNAFinalEngine.AugmentedReality