using OpenTK;
using OpenTK.Graphics;
using FreeFrame.Components;
using FreeFrame.Components.Shapes;

namespace FreeFrame
{
    class Program
    {
        static void Main()
        {
            List<Shape> shapes = Importer.Import("test.svg");

            shapes.ForEach(shape => Console.WriteLine(shape));
        }
    }
}
