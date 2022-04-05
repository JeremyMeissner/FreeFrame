using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics;
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
                Size = new Vector2i(600, 600),
                Title = "FreeFrame"
            };
            using Window window = new(GameWindowSettings.Default, nativeWindowSettings);
            window.Run();
        }
    }
}