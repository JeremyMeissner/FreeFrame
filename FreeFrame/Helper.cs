using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    static class Helper
    {
        private static DebugProc _debugProcCallback = DebugCallback;
        static public void CheckErrors()
        {
            ErrorCode errorCode = GL.GetError();

            while (errorCode != ErrorCode.NoError)
            {
                Console.WriteLine(errorCode.ToString());
                errorCode = GL.GetError();
            }
        }
        [DebuggerStepThrough]
        static private void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string messageString = Marshal.PtrToStringAnsi(message, length); // Retrieve the string from the pointer

            Console.WriteLine($"{severity} {type} | {messageString}");

            if (type == DebugType.DebugTypeError)
                throw new Exception("OpenGL error");
        }
        static public void DebugMode()
        {
            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
        }

    }
}
