using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using FreeFrame.Components.Shapes;
using OpenTK.Mathematics;

namespace FreeFrame.Components
{
    public static class Importer
    {
        static private (List<Shape>, SortedDictionary<int, List<Shape>>, bool) ImportFromStream(Stream pStream)
        {
            List<Shape> shapes = new List<Shape>();
            SortedDictionary<int, List<Shape>> timeline = new SortedDictionary<int, List<Shape>>();
            bool compatibilityFlag = false;

            Shape? previous = null;

            using (XmlReader reader = XmlReader.Create(pStream))
            {
                while (reader.Read())
                {
                    if (reader.HasAttributes)
                    {
                        //Console.WriteLine("Attributes of <" + reader.Name + ">");
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
                            case "animate":
                                if (previous != null)
                                {
                                    string attr = reader["attributeName"].ToString();
                                    int nbrKeyFrames = reader["values"].ToString().Count(c => c == ';');
                                    // regex
                                    Regex rx = new Regex(@"\d|#\d{8}");

                                    MatchCollection matches = rx.Matches(reader["values"]);

                                    for (int i = 0; i < nbrKeyFrames + 1; i++)
                                    {
                                        int positionInTimeline = i * (Timeline.MAX_TIMELINE / (nbrKeyFrames + 1));
                                        if (timeline.ContainsKey(positionInTimeline) == false)
                                            timeline.Add(positionInTimeline, null);
                                        if (timeline[positionInTimeline] == null)
                                            timeline[positionInTimeline] = new List<Shape>();
                                        Shape shape = previous.ShallowCopy();

                                        string matchResult = matches[i].Value;

                                        switch (attr)
                                        {
                                            case "rx":
                                            case "ry":
                                                shape.CornerRadius = Convert.ToInt32(matchResult);
                                                break;
                                            case "width":
                                                shape.Width = Convert.ToInt32(matchResult);
                                                break;
                                            case "height":
                                                shape.Height = Convert.ToInt32(matchResult);
                                                break;
                                            case "x":
                                                shape.X = Convert.ToInt32(matchResult);
                                                break;
                                            case "y":
                                                shape.Y = Convert.ToInt32(matchResult);
                                                break;
                                            case "fill":
                                                shape.Color = HexadecimalToRGB(matchResult);
                                                break;
                                            default:
                                                break;
                                        }
                                        timeline[positionInTimeline].Add(shape);
                                    }


                                    //reader["dur"]; //TODO: fps base on duration ?
                                }
                                break;
                            case "animateTransform":
                                if (reader["attributeName"].ToString() == "transform" && reader["type"].ToString() == "rotate")
                                {
                                    // Only read first one Z from
                                    // from="z _ _"
                                    // to="z _ _"
                                    // get angle
                                }
                                break;
                            default:
                                compatibilityFlag = true; // If an element is unknow, the flag is trigger
                                break;
                        }
                    }

                }
                try //TODO: fix trycatch
                {
                }
                catch (Exception)
                {
                    throw new Exception("Error while importing");
                }
            }
            return (shapes, new SortedDictionary<int, List<Shape>>(), compatibilityFlag);
        }
        static public (List<Shape>, SortedDictionary<int, List<Shape>>, bool) ImportFromFile(string pFilename)
        {
            if (!File.Exists(pFilename))
                throw new ArgumentException($"'{pFilename}' file cannot be found.", nameof(pFilename)); // TODO: replace by a simple alert window

            byte[] byteArray = Encoding.UTF8.GetBytes(File.ReadAllText(pFilename));

            return ImportFromStream(new MemoryStream(byteArray));
        }
        static public (List<Shape>, SortedDictionary<int, List<Shape>>, bool) ImportFromString(string pString)
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
