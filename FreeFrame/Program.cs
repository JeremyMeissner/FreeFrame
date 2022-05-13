using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics;
using FreeFrame.Components;
using FreeFrame.Components.Shapes;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace FreeFrame
{
    class Program
    {
        static void Main()
        {
            NativeWindowSettings nativeWindowSettings = new()
            {
                Size = new Vector2i(800, 600),
                Title = "FreeFrame",
                NumberOfSamples = 8,
            };
            using Window window = new(GameWindowSettings.Default, nativeWindowSettings); // Create window context (GLFW, OpenGL)

            window.Run();

        }
    }
}