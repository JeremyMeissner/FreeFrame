using FreeFrame.Components.Shapes;
using OpenTK.Mathematics;
using System.Text;
using System.Xml;

namespace FreeFrame.Components
{
    public static class Importer
    {
        /// <summary>
        /// Import shapes list and timeline from a stream
        /// </summary>
        /// <param name="pStream">Given stream</param>
        /// <returns>Shape list and timeline</returns>
        /// <exception cref="Exception">If something went wrong in the importation</exception>
        static private (List<Shape>, SortedDictionary<int, List<Shape>>, bool) ImportFromStream(Stream pStream)
        {
            List<Shape> shapes = new List<Shape>();
            SortedDictionary<int, List<Shape>> timeline = new SortedDictionary<int, List<Shape>>();
            bool compatibilityFlag = false;

            Shape? previous = null;

            using (XmlReader reader = XmlReader.Create(pStream))
            {
                try
                {
                    while (reader.Read())
                    {
                        if (reader.HasAttributes)
                        {
                            switch (reader.Name)
                            {
                                case "xml":
                                case "svg":
                                    break; // Skip knowned elements
                                case "polygon":
                                    shapes.Add(new SVGPolygon(reader));
                                    previous = shapes.Last();
                                    break;
                                case "path":
                                    shapes.Add(new SVGPath(reader));
                                    previous = shapes.Last();
                                    break;
                                case "rect":
                                    shapes.Add(new SVGRectangle(reader));
                                    previous = shapes.Last();
                                    break;
                                case "circle":
                                    shapes.Add(new SVGCircle(reader));
                                    previous = shapes.Last();
                                    break;
                                case "line":
                                    shapes.Add(new SVGLine(reader));
                                    previous = shapes.Last();
                                    break;
                                default:
                                    compatibilityFlag = true; // If an element is unknow, the flag is trigger
                                    break;
                            }
                        }

                    }
                }
                catch (Exception)
                {
                    throw new Exception("Error while importing");
                }
            }
            return (shapes, new SortedDictionary<int, List<Shape>>(), compatibilityFlag);
        }

        /// <summary>
        /// Import shapes list and timeline from a file path
        /// </summary>
        /// <param name="pFilename">File path</param>
        /// <returns>Shape list and timeline</returns>
        /// <exception cref="ArgumentException">If file cannot be found</exception>
        static public (List<Shape>, SortedDictionary<int, List<Shape>>, bool) ImportFromFile(string pFilename)
        {
            if (!File.Exists(pFilename))
                throw new ArgumentException($"'{pFilename}' file cannot be found.", nameof(pFilename));

            byte[] byteArray = Encoding.UTF8.GetBytes(File.ReadAllText(pFilename));

            return ImportFromStream(new MemoryStream(byteArray));
        }

        /// <summary>
        /// Import shapes list and timeline from a string
        /// </summary>
        /// <param name="pString">Given string</param>
        /// <returns>Shape list and timeline</returns>
        static public (List<Shape>, SortedDictionary<int, List<Shape>>, bool) ImportFromString(string pString)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(pString);

            return ImportFromStream(new MemoryStream(byteArray));
        }

        /// <summary>
        /// Export a given shape list into an svg file
        /// </summary>
        /// <param name="shapes">Shape list</param>
        /// <param name="clientSize">Window size</param>
        /// <param name="path">File path</param>
        static public void ExportToFile(List<Shape> shapes, Vector2i clientSize, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
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

        /// <summary>
        /// Convert from a string to an Color4 value
        /// </summary>
        /// <param name="hexadecimal">hexadecimal string</param>
        /// <returns>Color4 value</returns>
        static public Color4 HexadecimalToRGB(string hexadecimal)
        {
            float r = Convert.ToInt32(hexadecimal.Substring(1, 2), 16) / 255f;
            float g = Convert.ToInt32(hexadecimal.Substring(3, 2), 16) / 255f;
            float b = Convert.ToInt32(hexadecimal.Substring(5, 2), 16) / 255f;
            float a = Convert.ToInt32(hexadecimal.Substring(7, 2), 16) / 255f;
            return new Color4(r, g, b, a);
        }
    }
}
