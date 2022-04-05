using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FreeFrame.Components.Shapes;

namespace FreeFrame.Components
{
    public static class Importer
    {
        static public void Import()
        {
            //TODO: find a solution for opening a crossplatform file dialog. Do I need a crossplatform dialog?
            //using (OpenFileDialog openFileDialog = new OpenFileDialog())
            //{
            //}
        }
        static public List<Shape> ImportFromStream(Stream pStream)
        {
            List<Shape> shapes = new List<Shape>();

            using (XmlReader reader = XmlReader.Create(pStream))
            {
                while (reader.Read())
                {
                    if (reader.HasAttributes)
                    {
                        Console.WriteLine("Attributes of <" + reader.Name + ">");
                        switch (reader.Name)
                        {
                            case "xml":
                            case "svg":
                            case "rect":
                            case "path":
                                break;
                            case "circle":
                                shapes.Add(new SVGCircle(reader));
                                break;
                            default:
                                // TODO: show all the elemnt are not valid, be careful
                                break;
                        }
                    }
                }
            }
            return (shapes);
        }
        static public List<Shape> ImportFromFile(string pFilename)
        {
            if (!File.Exists(pFilename))
                throw new ArgumentException($"'{nameof(pFilename)}' file cannot be found.", nameof(pFilename)); //TODO: replace by a simple alert window

            byte[] byteArray = Encoding.UTF8.GetBytes(File.ReadAllText(pFilename));

            return ImportFromStream(new MemoryStream(byteArray));
        }
        static public List<Shape> ImportFromString(string pString)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(pString);

            return ImportFromStream(new MemoryStream(byteArray));
        }
    }
}
