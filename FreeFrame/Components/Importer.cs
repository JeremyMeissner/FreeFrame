using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FreeFrame.Components.Shapes;

namespace FreeFrame.Components
{
    static class Importer
    {
        static public void Import()
        {
            //TODO: find a solution for opening a crossplatform file dialog. Do I need a crossplatform dialog?
            //using (OpenFileDialog openFileDialog = new OpenFileDialog())
            //{
            //}
        }
        static public List<Shape> Import(string filename)
        {
            if (!File.Exists(filename))
                throw new ArgumentException($"'{nameof(filename)}' file cannot be found.", nameof(filename)); //TODO: replace by a simple alert window

            List<Shape> shapes = new List<Shape>();

            using (XmlReader reader = XmlReader.Create(filename))
            {
                while(reader.Read())
                {
                    if (reader.HasAttributes)
                    {
                        Console.WriteLine("Attributes of <" + reader.Name + ">");
                        switch (reader.Name)
                        {
                            case "circle":
                                shapes.Add(new SVGCircle(reader));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return (shapes);
        }
    }
}
