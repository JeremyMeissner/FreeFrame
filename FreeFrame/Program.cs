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
            List<Shape> shapes = Importer.ImportFromFile("test.svg");
            shapes.ForEach(shape => Console.WriteLine(shape));

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