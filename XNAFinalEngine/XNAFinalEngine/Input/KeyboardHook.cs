
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
This class was based in the work of Emma Burrows. This work doesn't have a license of any kind.
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endregion

namespace XNAFinalEngine.Input
{
    /// <summary>
    /// Sets up a keyboard hook to trap all keystrokes without passing any to other applications.
    /// It also disables the win key and allows the print screen key to call the screenshot capturer.
    /// </summary>
    public class KeyboardHook : IDisposable
    {

        #region Constants

        /// <summary>
        /// Keyboard API constants
        /// </summary>
        private const int KeyboardHookId = 13;

        #endregion

        #region Variables

        /// <summary>
        /// Variables used in the call to SetWindowsHookEx
        /// </summary>
        private readonly HookHandlerDelegate proc;
        private readonly IntPtr hookId = IntPtr.Zero;
        internal delegate IntPtr HookHandlerDelegate(int nCode, IntPtr wParam, ref KeyboadHookStruct lParam);
        
        /// <summary>
        /// Structure returned by the hook whenever a key is pressed
        /// </summary>
        internal struct KeyboadHookStruct
        {
            public int VkCode;
            public int Flags;
        }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Sets up a keyboard hook to trap all keystrokes without passing any to other applications.
        /// It also disables the win key and allows the print screen key to call the screenshot capturer.
        /// </summary>
        public KeyboardHook()
        {
            proc = new HookHandlerDelegate(HookCallback);
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                hookId = NativeMethods.SetWindowsHookEx(KeyboardHookId, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        } // KeyboardHook

        #endregion

        #region Hook Callback Method

        /// <summary>
        /// Processes the key event captured by the hook.
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, ref KeyboadHookStruct lParam)
        {
            // If the key is disable or has a special function
            if (lParam.VkCode == 91 || lParam.VkCode == 92 ||  // Win
                (lParam.Flags == 32 && lParam.VkCode == 13) ||  // Alt-Enter
                lParam.VkCode == 44) // Print Screen
            {
                if (lParam.Flags == 32 && lParam.VkCode == 13)
                {
                    EngineCore.EngineManager.ToggleFullscreen();
                }
                if (lParam.VkCode == 44)
                {
                    EngineCore.EngineManager.ScreenshotCapturer.MakeScreenshot();
                }
                return (IntPtr)1;
            }
            // if the key is allowed
            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, ref lParam);
        } // HookCallback

        #endregion

        #region Dispose

        /// <summary>
        /// Releases the keyboard hook.
        /// </summary>
        public void Dispose()
        {
            NativeMethods.UnhookWindowsHookEx(hookId);
        } // Dispose

        #endregion

        #region Native methods

        [ComVisibleAttribute(false), System.Security.SuppressUnmanagedCodeSecurity]
        internal class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook,
                HookHandlerDelegate lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref KeyboadHookStruct lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
            public static extern short GetKeyState(int keyCode);

        }

        #endregion

    } // KeyboardHook
} // XNAFinalEngine.Input