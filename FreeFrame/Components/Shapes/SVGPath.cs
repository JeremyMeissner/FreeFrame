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

        readonly Dictionary<char, Regex> _dAttributesRegex = new()
        {
            { 'M', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'm', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'L', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'l', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'H', new Regex(@" *(-?\d+) *") },
            { 'h', new Regex(@" *(-?\d+) *") },
            { 'V', new Regex(@" *(-?\d+) *") },
            { 'v', new Regex(@" *(-?\d+) *") },
            { 'C', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 'c', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 'S', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 's', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 'Q', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 'q', new Regex(@" *(-?\d+) *, *(-?\d+) +(-?\d+) *, *(-?\d+) *") },
            { 'T', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 't', new Regex(@" *(-?\d+) *, *(-?\d+) *") },
            { 'A', new Regex(@" *(-?\d+) +(-?\d+) +(-?\d+) +(-?\d) +(-?\d) +(-?\d+) *, *(-?\d+) *") },
            { 'a', new Regex(@" *(-?\d+) +(-?\d+) +(-?\d+) +(-?\d) +(-?\d) +(-?\d+) *, *(-?\d+) *") },
            { 'Z', new Regex("") },
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

                if (_dAttributesRegex.ContainsKey(c))
                {
                    MatchCollection matches = _dAttributesRegex[c].Matches(d, startIndex); // Retrieve the associated regular expression
                    match = _dAttributesRegex[c].Match(d, startIndex); // Retrieve the associated regular expression
                    switch (c)
                    {
                        case 'M':
                        case 'm':
                            DrawAttributes.Add(new MoveTo(match.Groups[1], match.Groups[2], c == 'm')); // 'm' is relative and 'M' absolute
                            break;
                        case 'l':
                        case 'L':
                            DrawAttributes.Add(new LineTo(match.Groups[1], match.Groups[2], c == 'l')); // 'l' is relative and 'L' absolute
                            break;
                        case 'H':
                        case 'h':
                            DrawAttributes.Add(new HorizontalLineTo(match.Groups[1], c == 'h')); // 'h' is relative and 'H' absolute
                            break;
                        case 'V':
                        case 'v':
                            DrawAttributes.Add(new VerticalLineTo(match.Groups[1], c == 'v')); // 'v' is relative and 'V' absolute
                            break;
                        case 'C':
                        case 'c':
                            DrawAttributes.Add(new CurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], match.Groups[5], match.Groups[6], c == 'c')); // 'c' is relative and 'C' absolute
                            break;
                        case 'S':
                        case 's':
                            DrawAttributes.Add(new SmoothCurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], c == 's')); // 's' is relative and 'S' absolute
                            break;
                        case 'Q':
                        case 'q':
                            DrawAttributes.Add(new QuadraticBezierCurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], c == 'q')); // 'q' is relative and 'Q' absolute
                            break;
                        case 'T':
                        case 't':
                            DrawAttributes.Add(new SmoothQuadraticBezierCurveTo(match.Groups[1], match.Groups[2], c == 't')); // 't' is relative and 'T' absolute
                            break;
                        case 'A':
                        case 'a':
                            DrawAttributes.Add(new EllipticalArc(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], match.Groups[5], match.Groups[6], match.Groups[7], c == 'a')); // 'a' is relative and 'A' absolute
                            break;
                        case 'Z':
                        case 'z':
                            DrawAttributes.Add(new ClosePath());
                            break;
                        default:
                            throw new Exception("Unknowed properties in d");
                    }
                    startIndex += match.Groups[0].Length + 1;
                }
            }

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
                if (attrType == typeof(MoveTo) && attr.IsRelative == false) // Update position of each absolute MoveTo
                {
                    if (deltaX == null || deltaY == null)
                    {
                        // Get delta X and Y only one time
                        deltaX = position.X - x;
                        deltaY = position.Y - y;
                    }
                    attr.X += (int)deltaX;
                    attr.Y += (int)deltaY;
                }
                if (attrType == typeof(CurveTo) ||
                    attrType == typeof(SmoothCurveTo) ||
                    attrType == typeof(QuadraticBezierCurveTo) ||
                    attrType == typeof(SmoothQuadraticBezierCurveTo) ||
                    attrType == typeof(EllipticalArc))
                {
                    _vaos.Add(new VertexArrayObject(attr.GetVertices(), attr.GetVerticesIndexes(), PrimitiveType.LineStrip, this));
                }
                else
                {
                    _vaos.Add(new VertexArrayObject(attr.GetVertices(), attr.GetVerticesIndexes(), PrimitiveType.Lines, this));
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
            string output = "d: ";
            DrawAttributes.ForEach(d => output += d.ToString() + " ");
            return output.Trim();
        }
    }
}
