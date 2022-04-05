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
            List<Shape> shapes = Importer.ImportFromFile("test.svg");

            shapes.ForEach(shape => Console.WriteLine(shape));
        }
    }
}
