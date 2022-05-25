using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using FreeFrame.Components.Shapes.Path;
using DelaunatorSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FreeFrame.Components.Shapes
{
    public class SVGPath : Shape
    {

        readonly Dictionary<char, Regex> _dAttributeRegex = new()
        {
            { 'm', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'l', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'h', new Regex(@" *(-?\d+) *") },
            { 'v', new Regex(@" *(-?\d+) *") },
            { 'c', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 's', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 'q', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 't', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'a', new Regex(@" *(-?\d+) +(-?\d+) +(-?\d+) +(-?\d) +(-?\d) +(-?\d+) *, *(-?\d+) *") },
            { 'z', new Regex("") },
        };

        List<DrawAttribute> _drawAttributes = new();

        public List<DrawAttribute> DrawAttributes { get => _drawAttributes; set => _drawAttributes = value; }

        public SVGPath(XmlReader reader) //: this()
        {
            IsResizeable = false;
            IsCornerRadiusChangeable = false;

            string d = reader["d"] ?? throw new Exception("d not here"); // TODO: Error handler if d is note here
            Match match;
            int startIndex = 0;

            for (int i = 0; i < d.Length; i++)
            {
                char c = d[i]; // Get current char
                char lowerC = char.ToLower(d[i]); // Get current char

                if (_dAttributeRegex.ContainsKey(lowerC))
                {
                    match = _dAttributeRegex[lowerC].Match(d, startIndex); // Retrieve the associated regular expression
                    switch (lowerC)
                    {
                        case 'm':
                            DrawAttributes.Add(new MoveTo(match.Groups[1], match.Groups[2], c == 'm')); // 'm' is relative and 'M' absolute
                            break;
                        case 'L':
                            DrawAttributes.Add(new LineTo(match.Groups[1], match.Groups[2], c == 'l')); // 'l' is relative and 'L' absolute
                            break;
                        case 'h':
                            DrawAttributes.Add(new HorizontalLineTo(match.Groups[1], c == 'h')); // 'h' is relative and 'H' absolute
                            break;
                        case 'v':
                            DrawAttributes.Add(new VerticalLineTo(match.Groups[1], c == 'v')); // 'v' is relative and 'V' absolute
                            break;
                        case 'c':
                            DrawAttributes.Add(new CurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], match.Groups[5], match.Groups[6], c == 'c')); // 'c' is relative and 'C' absolute
                            break;
                        case 's':
                            DrawAttributes.Add(new SmoothCurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], c == 's')); // 's' is relative and 'S' absolute
                            break;
                        case 'q':
                            DrawAttributes.Add(new QuadraticBezierCurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], c == 'q')); // 'q' is relative and 'Q' absolute
                            break;
                        case 't':
                            DrawAttributes.Add(new SmoothQuadraticBezierCurveTo(match.Groups[1], match.Groups[2], c == 't')); // 't' is relative and 'T' absolute
                            break;
                        case 'a':
                            DrawAttributes.Add(new EllipticalArc(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], match.Groups[5], match.Groups[6], match.Groups[7], c == 'a')); // 'a' is relative and 'A' absolute
                            break;
                        case 'z':
                            DrawAttributes.Add(new ClosePath());
                            break;
                        default:
                            throw new Exception("Unknowed properties in d");
                    }
                    startIndex += match.Groups[0].Length + 1;
                }
            }

            string color = reader["fill"] ?? throw new Exception("color not here"); // TODO: Error handler if d is note here
            Color = Importer.HexadecimalToRGB(color);


            // Update common properties, use the given attributes
            List<Vector2i> points = GetSelectablePoints();
            X = points.Min(i => i.X);
            Y = points.Min(i => i.Y);
            Width = points.Max(i => i.X) - points.Min(i => i.X);
            Height = points.Max(i => i.Y) - points.Min(i => i.Y);

            ImplementObject();
        }
        public override void ImplementObject()
        {
            Move(new Vector2i(X, Y)); // Update the position since the attr doesnt use the X and Y variables directly
        }

        public override float[] GetVertices() => new float[] { };
        public override uint[] GetVerticesIndexes() => new uint[] { };

        public override void Move(Vector2i position)
        {
            int x = 0, y = 0;
            int? deltaX = null, deltaY = null;
            List<Vector2i> points = GetSelectablePoints();

            DeleteObjects(); // Clear the vaos

            if (points.Count > 0)
            {
                x = points.Min(i => i.X); // Get current min X and Y of the path
                y = points.Min(i => i.Y);
            }

            foreach (DrawAttribute attr in DrawAttributes)
            {
                Type attrType = attr.GetType();
                if (attr.IsRelative == false) // Update position of each absolute attr
                {
                    Console.WriteLine("{0} is relative? =>{1}", attr.GetType().Name, attr.IsRelative);
                    if (deltaX == null || deltaY == null)
                    {
                        // Get delta X and Y only one time
                        deltaX = position.X - x;
                        deltaY = position.Y - y;
                    }
                    attr.X += (int)deltaX;
                    attr.Y += (int)deltaY;

                    attr.X1 += (int)deltaX;
                    attr.Y1 += (int)deltaY;

                    if (attrType == typeof(CurveTo))
                    {
                        ((CurveTo)attr).X2 += (int)deltaX;
                        ((CurveTo)attr).Y2 += (int)deltaY;
                    }
                    else if (attrType == typeof(SmoothCurveTo))
                    {
                        
                        Console.WriteLine(attr.GetType().Name + " - " + attr.ToString());
                        Console.WriteLine("x2: {0}, y2: {1}", ((SmoothCurveTo)attr).X2, ((SmoothCurveTo)attr).Y2);
                        Console.WriteLine("x1: {0}, y1: {1}", DrawAttribute.Last.X1, DrawAttribute.Last.Y1);
                        ((SmoothCurveTo)attr).X2 += (int)deltaX;
                        ((SmoothCurveTo)attr).Y2 += (int)deltaY;
                    }
                }
                if (attrType == typeof(CurveTo) ||
                    attrType == typeof(SmoothCurveTo) ||
                    attrType == typeof(QuadraticBezierCurveTo) ||
                    attrType == typeof(SmoothQuadraticBezierCurveTo) ||
                    attrType == typeof(EllipticalArc))
                {
                    Vaos.Add(new Renderer(attr.GetVertices(), attr.GetVerticesIndexes(), PrimitiveType.LineStrip, this));
                }
                else
                {
                    Vaos.Add(new Renderer(attr.GetVertices(), attr.GetVerticesIndexes(), PrimitiveType.Lines, this));
                }
                attr.UpdateLast(); // Update Last's inner variables 
            }

            // Update common properties
            X = position.X;
            Y = position.Y;
        }
        public override void Resize(Vector2i size) => throw new NotImplementedException("Can't resize a path");
        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();

            foreach (DrawAttribute attr in DrawAttributes)
            {
                points.AddRange(attr.GetSelectablePoints());
                attr.UpdateLast(); // Each attr depend on the previous. The previous element is the last element that called GetVertices() 
            }
            return points;
        }

        public override string ToString()
        {
            string output = "<path d=\"";
            DrawAttributes.ForEach(d => output += d.ToString() + " ");
            return output.Trim() + $"\" fill=\"{ColorToHexadecimal(Color)}\"/>";
        }
    }
}
