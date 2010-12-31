
#region Using directives
using System;
using System.Windows;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
#endregion

namespace XNAFinalEngine.AugmentedReality
{

    /// <summary>
    /// Direct Show Bitmap Buffer. It's from an Stephen Bogner example.
    /// </summary>
    class DirectShowBitmapBuffer
    {   

        #region Variables

        private BitmapSource _bitmapImage = null;

        private object _wicImageHandle = null;

        private object _wicImageLock = null;

        private uint _bufferSize = 0;

        private IntPtr _bufferPointer = IntPtr.Zero;

        private uint _stride = 0;

        private int _width;

        private int _height;

        #endregion

        #region Constructor

        /// <summary>
        /// Direct Show Bitmap Buffer
        /// </summary>
        public DirectShowBitmapBuffer(BitmapSource Image)
        {
            //Keep reference to our bitmap image
            _bitmapImage = Image;

            //Get around the STA deal
            _bitmapImage.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new System.Windows.Threading.DispatcherOperationCallback(delegate
            {
                //Cache our width and height
                _width = _bitmapImage.PixelWidth;
                _height = _bitmapImage.PixelHeight;
                return null;
            }), null);
            
            //Retrieve and store our WIC handle to the bitmap
            SetWICHandle();
            
            //Set the buffer pointer
            SetBufferInfo();
        } // DirectShowBitmapBuffer

        #endregion

        #region Buffer Pointer

        /// <summary>
        /// The pointer to the BitmapImage's native buffer
        /// </summary>
        public IntPtr BufferPointer
        {
            get
            {
                //Set the buffer pointer
                SetBufferInfo();
                return  _bufferPointer;
            }
        } // BufferPointer

        #endregion

        #region SetBufferInfo

        private void SetBufferInfo()
        {
            int hr = 0;

            //Get the internal nested class that holds some of the native functions for WIC
            Type wicBitmapNativeMethodsClass = Type.GetType("MS.Win32.PresentationCore.UnsafeNativeMethods+WICBitmap, PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            //Get the methods of all the static methods in the class
            MethodInfo[] info = wicBitmapNativeMethodsClass.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

            //The rectangle of the buffer we are
            //going to request
            Int32Rect rect = new Int32Rect();

            rect.Width = _width;
            rect.Height = _height;
            
            //Populate the arguments to pass to the function
            object[] args = new object[] { _wicImageHandle, rect, 2, _wicImageHandle };

            try
            {
                //This method looks good
                MethodInfo lockmethod = info[0];

                //Execute our static Lock() method
                hr = (int)lockmethod.Invoke(null, args);
            }
            catch
            { // The code isn't very good. In some case the correct method is in the second field.
                MethodInfo lockmethod = info[1];

                //Execute our static Lock() method
                hr = (int)lockmethod.Invoke(null, args);
            }

            //argument[3] is our "out" pointer to the lock handle
            //it is set by our last method invoke call
            _wicImageLock = args[3];
            
            //Get the internal nested class that holds some of the
            //other native functions for WIC
            Type wicLockMethodsClass = Type.GetType("MS.Win32.PresentationCore.UnsafeNativeMethods+WICBitmapLock, PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            
            //Get all the native methods into our array
            MethodInfo[] lockMethods = wicLockMethodsClass.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

            //Our method to get the stride value of the image
            MethodInfo getStrideMethod = lockMethods[0];

            //Fill in our arguments
            args = new object[] { _wicImageLock, _stride };

            //Execute the stride method
            getStrideMethod.Invoke(null, args);

            //Grab out or byref value for the stride
            _stride = (uint)args[1];

            //This one looks perty...
            //This function will return to us 
            //the buffer pointer and size
            MethodInfo getBufferMethod = lockMethods[1];

            //Fill in our arguments
            args = new object[] { _wicImageLock, _bufferSize, _bufferPointer };

            //Run our method
            hr = (int)getBufferMethod.Invoke(null, args);

            _bufferSize = (uint)args[1];
            _bufferPointer = (IntPtr)args[2];

            DisposeLockHandle();
        } // SetBufferInfo

        private void DisposeLockHandle()
        {
            MethodInfo close = _wicImageLock.GetType().GetMethod("Close");
            MethodInfo dispose = _wicImageLock.GetType().GetMethod("Dispose");

            close.Invoke(_wicImageLock, null);
            dispose.Invoke(_wicImageLock, null);
        } // DisposeLockHandle

        #endregion

        #region SetWICHandle

        private void SetWICHandle()
        {
            //Get the type of bitmap image
            Type bmpType = typeof(BitmapSource);

            //Use reflection to get the private property WicSourceHandle
            FieldInfo fInfo = bmpType.GetField("_wicSource", 
                                               BindingFlags.NonPublic | BindingFlags.Instance);

            //Retrieve the WIC handle from our BitmapImage instance
            _wicImageHandle = fInfo.GetValue(_bitmapImage);
        } // SetWICHandle

        #endregion

        #region APIs
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);
        #endregion

    } // DirectShowBitmapBuffer
} // XNAFinalEngine.AugmentedReality
