
#region Using directives
using System;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XNAFinalEngine.AugmentedReality
{
    /// <summary>
    /// Abstract class for webcam support.
    /// </summary>
    public abstract class WebCam
    {
        
        #region Variables

        /// <summary>
        /// Width resolution.
        /// </summary>
        protected int width = 640;

        /// <summary>
        /// Height resolution.
        /// </summary>
        protected int height = 480;

        /// <summary>
        /// Bytes per pixel.
        /// </summary>
        protected int bytesPerPixel = 4;

        /// <summary>
        /// Frame per seconds.
        /// </summary>
        protected int fps = 30;

        /// <summary>
        /// Texture with the last webcam frame.
        /// </summary>
        protected Texture2D webCamTexture = null;

        #endregion        
        
        #region Properties

        /// <summary>
        /// Width resolution.
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// Height resolution.
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// Bytes per pixel.
        /// </summary>
        public int BytesPerPixel { get { return bytesPerPixel; } }

        /// <summary>
        /// Frame per seconds.
        /// </summary>
        public int FramePerSeconds { get { return fps; } }

        /// <summary>
        /// The capture of the frame. Managed structure.
        /// </summary>
        public virtual byte[] FrameManaged
        {
            get
            {
                // Override it!!!!
                return null;
            }
        } // FrameManaged

        /// <summary>
        /// The capture of the frame. Unmanaged structure.
        /// </summary>
        public virtual IntPtr FrameUnmanaged
        {
            get
            {
                // Override it!!!!
                return IntPtr.Zero;
            }
        } // FrameUnmanaged

        #endregion

        #region Get Texture Information

        /// <summary>
        /// Texture with the last webcam frame.
        /// </summary>
        public virtual Texture2D XNATexture
        {
            get 
            {
                // Override it!!!!
                return null;
            }
        } // XNATexture

        #endregion
                
        #region Dispose

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            // Override it!!!!
        } // Dispose

        #endregion

        #region Calculate New Frame

        /// <summary>
        /// Obtiene un nuevo cuadro desde la camara web.
        /// </summary>
        public virtual void CalculateNewFrame()
        {
            // Override it!!!!
        } // CalculateNewFrame

        #endregion

    } // WebCam
} // XNA2FinalEngine.AugmentedReality