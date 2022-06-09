using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

            switch (severity)
            {
                case DebugSeverity.DontCare:
                case DebugSeverity.DebugSeverityNotification:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case DebugSeverity.DebugSeverityHigh:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case DebugSeverity.DebugSeverityMedium:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case DebugSeverity.DebugSeverityLow:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }
            Console.WriteLine($"{severity} {type} | {messageString}");
            Console.ResetColor();

            if (type == DebugType.DebugTypeError)
                throw new Exception("OpenGL error");
        }
        /// <summary>
        /// Enable the debug mode
        /// </summary>
        static public void EnableDebugMode()
        {
            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
        }
    }
}
