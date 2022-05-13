using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FreeFrame.Components.Shapes;
using OpenTK.Mathematics;

namespace FreeFrame.Components
{
    public static class Importer
    {
        static public (List<Shape>, bool) ImportFromStream(Stream pStream)
        {
            List<Shape> shapes = new List<Shape>();
            bool compatibilityFlag = false;

            using (XmlReader reader = XmlReader.Create(pStream))
            {
                while (reader.Read())
                {
                    //if (reader.HasAttributes)
                    {
                        //Console.WriteLine("Attributes of <" + reader.Name + ">");
                        switch (reader.Name)
                        {
                            case "xml":
                            case "svg":
                                break; // Skip knowned elements
                            case "polygon":
                                shapes.Add(new SVGPolygon(reader));
                                break;
                            case "path":
                                shapes.Add(new SVGPath(reader));
                                break;
                            case "rect":
                                shapes.Add(new SVGRectangle(reader));
                                break;
                            case "circle":
                                shapes.Add(new SVGCircle(reader));
                                break;
                            case "line":
                                shapes.Add(new SVGLine(reader));
                                break;
                            default:
                                compatibilityFlag = true; // If an element is unknow, the flag is trigger
                                break;
                        }
                    }
                }
            }
            return (shapes, compatibilityFlag);
        }
        static public (List<Shape>, bool) ImportFromFile(string pFilename)
        {
            if (!File.Exists(pFilename))
                throw new ArgumentException($"'{pFilename}' file cannot be found.", nameof(pFilename)); // TODO: replace by a simple alert window

            byte[] byteArray = Encoding.UTF8.GetBytes(File.ReadAllText(pFilename));

            return ImportFromStream(new MemoryStream(byteArray));
        }
        static public (List<Shape>, bool) ImportFromString(string pString)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(pString);

            return ImportFromStream(new MemoryStream(byteArray));
        }

        static public void ExportToFile(List<Shape> shapes, Vector2i clientSize)
        {

            using (FileStream fs = new FileStream("output.svg", FileMode.Create))
            {
                byte[] bytes = Encoding.ASCII.GetBytes($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1"" width=""{clientSize.X}"" height=""{clientSize.Y}"" >" + Environment.NewLine);

                for (int i = 0; i < bytes.Length; i++)
                    fs.WriteByte(bytes[i]);

                foreach (Shape shape in shapes)
                {
                    bytes = Encoding.ASCII.GetBytes(shape.ToString() + Environment.NewLine);
                    for (int i = 0; i < bytes.Length; i++)
                        fs.WriteByte(bytes[i]);
                }
                bytes = Encoding.ASCII.GetBytes("</svg>");
                for (int i = 0; i < bytes.Length; i++)
                    fs.WriteByte(bytes[i]);
            }
        }
    }
}
