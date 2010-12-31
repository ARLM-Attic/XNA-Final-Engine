
#region Using directives
using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using DirectShowLib;
using Color = Microsoft.Xna.Framework.Color;
using System.Threading;
#endregion

namespace XNAFinalEngine.AugmentedReality
{

    /// <summary>
    /// Class to capture a webcam frame. It's based in a Stephen Bogner example. 
    /// This class was refactored and some funcitonality was added, but it's difficult to undestand and maybe it can be improved a lot.
    /// 
    /// Se estructuro mejor y se hizo mas flexible. Permitiendo devolver la informacion de varias maneras mas.
    /// Permite configurar la resolucion, los bytes por frame y los fps.
    /// Ademas, con un pequeño cambio se podria tomar varias camaras por separado. 
    /// Pero dado que el proyecto por el momento no lo requiere esta capacidad no ha sido explotada.
    /// Para lograrlo solo debemos asignar un valor distinto de 0 a deviceNum.
    /// </summary>
    public class DirectShowWebCam : WebCam
    {

        #region Auxiliary class

        /// <summary>
        /// This class helps us to get the dispacher. With this avoid multiple hierarchy.
        /// </summary>
        private class AuxiliaryClass : System.Windows.Threading.DispatcherObject
        {
        } // AuxiliaryClass

        #endregion

        #region Variables

        /// <summary>
        /// Variables relacionadas con la captura a "bajo" nivel.
        /// </summary>
        readonly DirectShowBitmapBuffer buf;

        /// <summary>
        /// Putting Panel in HwndHost didnt help with frame rate loss
        /// </summary>
        readonly System.Windows.Forms.Panel panel;
        
        /// <summary>
        /// Dummy object to control the monitor synchronization.
        /// </summary>
        readonly object syncObject = new object();
        
        /// <summary>
        /// Byte per pixel variables. An incorrect value can produce an excepction.
        /// </summary>
        readonly PixelFormat mediaBitmapSourcePixelFormat;

        readonly Guid sampleGrabberSubType;
                
        /// <summary>
        /// Pointer to the capture structure. Unmanaged data.
        /// </summary>
        IntPtr ipImage;

        /// <summary>
        /// Estructuras managed. WebCamInformationAux se utiliza para invertir la informacion obtenida.
        /// </summary>
        private readonly byte[] webCamInformation;
        private readonly byte[] webCamInformationMirror;

        /// <summary>
        /// Objeto base que se encarga del manejo a bajo nivel de la camara.
        /// </summary>
        readonly DirectShowCapture cameraCapture;

        /// <summary>
        /// Clase auxiliar para tener un dispacher asociado a este objeto.
        /// </summary>
        readonly AuxiliaryClass dispacher;

        #endregion

        #region Properties

        /// <summary>
        /// The capture of the frame. Managed structure.
        /// </summary>
        public override byte[] FrameManaged { get { return webCamInformation; } }

        /// <summary>
        /// The capture of the frame. Unmanaged structure.
        /// </summary>
        public override IntPtr FrameUnmanaged { get { return ipImage; } }

        /// <summary>
        /// Texture with the last webcam frame.
        /// </summary>
        public override Texture2D XnaTexture { get { return webCamTexture; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Create and initialize the webcam.
        /// </summary>
        /// <param name="deviceNumber">The device number normally is 0, but sometimes there are other device that works like webcam and in that cases maybe the number changes to 1</param>
        /// <param name="_width">Width resolution</param>
        /// <param name="_height">Height resolution</param>
        /// <param name="_bytesPerPixel">Bytes per pixel</param>
        /// <param name="_fps">The desire frame per seconds</param>
        public DirectShowWebCam(int deviceNumber = 0, int _width = 640, int _height = 480, int _bytesPerPixel = 3, int _fps = 30)
        {
            // Coloco la informacion base de la camara //
            width = _width;
            height = _height;
            bytesPerPixel = _bytesPerPixel;
            fps = _fps;
            // Calculo dependiendo los cuadros por segundo el tipo de los canales RGBA.
            switch (bytesPerPixel)
            {
                case 3:
                    mediaBitmapSourcePixelFormat = PixelFormats.Rgb24;
                    sampleGrabberSubType = MediaSubType.RGB24;
                    break;
                case 4:
                    mediaBitmapSourcePixelFormat = PixelFormats.Bgra32; //Rgb24;
                    sampleGrabberSubType = MediaSubType.ARGB32; //RGB24;
                    break;
                default: throw new Exception("Bytes per pixel not suported");
            }

            DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            //These are 'dummy' pixels
            byte[] pixels = new byte[width * height * bytesPerPixel];
            //Create a new bitmap source
            BitmapSource bmpsrc = BitmapSource.Create(width, height, 96, 96, mediaBitmapSourcePixelFormat, null, pixels, width * bytesPerPixel);
            
            //Create our helper class
            buf = new DirectShowBitmapBuffer(bmpsrc);
            //IntPtr buffPointer = buf.BufferPointer;

            panel = new System.Windows.Forms.Panel();
            short bitsPerPixel = (short)(bytesPerPixel * 8);

            cameraCapture = new DirectShowCapture(deviceNumber, width, height, bitsPerPixel, fps, panel, sampleGrabberSubType);
            dispacher = new AuxiliaryClass();
            cameraCapture.Dispatcher = dispacher.Dispatcher;
            cameraCapture.SampleEvent += SampleEvent;

            // Creo las texturas que almacenaran la captura de la webCam
            webCamTexture = new Texture2D(EngineCore.EngineManager.Device, width, height, false, SurfaceFormat.Color);

            // Creo las estructuras managed para colocar las capturas.
            webCamInformation = new byte[Width * Height * BytesPerPixel];
            webCamInformationMirror = new byte[webCamInformation.Length];
        } // DirectShowWebCam

        /// <summary>
        /// Creo e inicializo la Web Cam
        /// </summary>
        public DirectShowWebCam() : this(640, 480, 4, 30)
        {
        } // StephenBognerWebCam

        #endregion

        #region SampleEvent

        /// <summary>
        /// Sincroniza que el buffer de lectura del cuadro no se utilice mientras se esta leyendo.
        /// No tendria que pasar nunca, pero si en caso pasara...
        /// </summary>
        private void SampleEvent(IntPtr pBuffer, int bufferLen)
        {
            if (buf != null && cameraCapture != null)
            {
                try
                {
                    if (Monitor.TryEnter(syncObject))
                    {
                        try
                        {
                            DirectShowBitmapBuffer.CopyMemory(buf.BufferPointer, pBuffer, bufferLen);
                        }
                        finally
                        {
                            Monitor.Exit(syncObject);
                        }
                    }
                }
                catch
                {
                    throw new Exception("There was an error in the capture of the webcam");
                }
            }
        } // SampleEvent

        #endregion

        #region Calculate New Frame

        /// <summary>
        /// Obtiene un nuevo cuadro desde la camara web.
        /// </summary>
        public override void CalculateNewFrame()
        {
            Marshal.FreeCoTaskMem(ipImage);
            ipImage = cameraCapture.Click();
            Marshal.Copy(ipImage, webCamInformationMirror, 0, webCamInformationMirror.Length);
            
            #region Flip
            
            // Flip the image along Y- this has to be done
            int srcPixOffset;
            int tarPixOffset;
            int colbyte = Width * bytesPerPixel;;            
            for (int row = 0; row < Height; row++)
            {
                srcPixOffset = row * Width * bytesPerPixel;
                tarPixOffset = ((Height - row - 1) * Width) * bytesPerPixel;
                for (int i = 0; i < colbyte; i++)
                {
                    webCamInformation[tarPixOffset + i] = webCamInformationMirror[srcPixOffset + i];
                }
            }

            #endregion

            // We need ipImage with the flip version.
            Marshal.Copy(webCamInformation, 0, ipImage, webCamInformation.Length);
            
            TransformToXNATexture();
        } // CalculateNewFrame

        /// <summary>
        /// Transformo la informacion en bruto en una textura XNA.
        /// Utilizo dos texturas por si se generan conflictos. Sin embargo no parecen generarse.
        /// Pero dado que esta informacion ocupa poco en memoria y mas que eso no influye se sigue con este esquema.
        /// </summary>
        public void TransformToXNATexture()
        {   
            Color[] colorData = new Color[width * height];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = new Color(webCamInformation[BytesPerPixel * i + 2],
                                                                          webCamInformation[BytesPerPixel * i + 1],
                                                                          webCamInformation[BytesPerPixel * i]);
            }
            //EngineManager.Device.Textures[0] = null; // Bug de XNA
            webCamTexture.SetData(colorData);
        } // TransformToXNATexture

        #endregion

    } // DirectShowWebCam
} // XNA2FinalEngine.AugmentedReality
