using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using FreeFrame.Components.Shapes.Path;
using DelaunatorSharp;

namespace FreeFrame.Components.Shapes
{
    public class SVGPath : Shape
    {
        readonly Dictionary<char, Regex> _dAttributesRegex = new()
        {
            { 'M', new Regex(@" *(\d+) *, *(\d+) *") },
            { 'm', new Regex(@" *(\d+) *, *(\d+) *") },
            { 'L', new Regex(@" *(\d+) *, *(\d+) *") },
            { 'l', new Regex(@" *(\d+) *, *(\d+) *") },
            { 'H', new Regex(@" *(\d+) *") },
            { 'h', new Regex(@" *(\d+) *") },
            { 'V', new Regex(@" *(\d+) *") },
            { 'v', new Regex(@" *(\d+) *") },
            { 'C', new Regex(@" *(\d+) *, *(\d+) +(\d+) *, *(\d+) +(\d+) *, *(\d+) *") },
            { 'c', new Regex(@" *(\d+) *, *(\d+) +(\d+) *, *(\d+) +(\d+) *, *(\d+) *") },
            { 'S', new Regex(@" *(\d+) *, *(\d+) +(\d+) *, *(\d+) *") },
            { 's', new Regex(@" *(\d+) *, *(\d+) +(\d+) *, *(\d+) *") },
            { 'Q', new Regex(@" *(\d+) *, *(\d+) +(\d+) *, *(\d+) *") },
            { 'q', new Regex(@" *(\d+) *, *(\d+) +(\d+) *, *(\d+) *") },
            { 'T', new Regex(@" *(\d+) *, *(\d+) *") },
            { 't', new Regex(@" *(\d+) *, *(\d+) *") },
            { 'A', new Regex(@" *(\d+) +(\d+) +(\d+) +(\d) +(\d) +(\d+) *, *(\d+) *") },
            { 'a', new Regex(@" *(\d+) +(\d+) +(\d+) +(\d) +(\d) +(\d+) *, *(\d+) *") },
            { 'Z', new Regex("") },
            { 'z', new Regex("") },
        };

        List<DrawAttribute> _drawAttributes = new();

        public List<DrawAttribute> DrawAttributes { get => _drawAttributes; set => _drawAttributes = value; }

        public SVGPath(XmlReader reader) //: this()
        {
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
        }

        public override float[] GetVertices()
        {
            //  Delaunator polygon = new Delaunator(new IPoint[] { new Point(0.0, 0.0) });


            /*
            DrawAttribute? prev = null;
            DrawAttribute? current = null;
            foreach (DrawAttribute next in DrawAttributes)
            {
                if (current != null)
                {
                    if (current.GetType() == typeof(LineTo))
                    {
                        vertices.AddRange(((LineTo)current).GetVertices());
                    }
                    else if (current.GetType() == typeof(HorizontalLineTo))
                    {
                        vertices.AddRange(((HorizontalLineTo)current).GetVertices());
                    }
                    else if (current.GetType() == typeof(VerticalLineTo))
                    {
                        vertices.AddRange(((VerticalLineTo)current).GetVertices());
                    }

                    if (prev != null)
                    {
                        if (current.GetType() == typeof(CurveTo) || current.GetType() == typeof(SmoothCurveTo))
                        {
                            DrawAttribute.LastControlPointX = ((CurveTo)prev).X2; // Update last x and y (for relatives attributes points)
                            DrawAttribute.LastControlPointY = ((CurveTo)prev).Y2;
                        }
                        if (current.GetType() != typeof(MoveTo)) // Avoid trying to get last added vertices but MoveTo doesnt add vertices
                        {
                            DrawAttribute.LastX = vertices[-2]; // Update last x and y (for relatives attributes points)
                            DrawAttribute.LastY = vertices[-1];
                        }
                    }
                }
                (prev, current) = (current, next);
            }

            if (current != null) // Last element
            {

            }
            */

            // Edges
            List<float> vertices = new List<float>();
            DrawAttribute? previous = null;
            foreach (DrawAttribute current in DrawAttributes)
            {
                if (current.GetType() == typeof(LineTo) ||
                    current.GetType() == typeof(HorizontalLineTo) ||
                    current.GetType() == typeof(VerticalLineTo) ||
                    current.GetType() == typeof(CurveTo) ||
                    current.GetType() == typeof(SmoothCurveTo) ||
                    current.GetType() == typeof(QuadraticBezierCurveTo) ||
                    current.GetType() == typeof(SmoothQuadraticBezierCurveTo)) // TODO: Add EllipticalArc support
                {
                    foreach (float item in current.GetVertices())
                    {
                        vertices.Add(item);
                    }
                }

                if (current.GetType() != typeof(MoveTo)) // MoveTo already update the lasts
                {
                    DrawAttribute.LastX = vertices[^2]; // Update last x and y (for relatives attributes points)
                    DrawAttribute.LastY = vertices[^1];
                }
                if (previous != null)
                {
                    if (previous.GetType() == typeof(CurveTo)) // Cubic Bézier Curves
                    {
                        DrawAttribute.LastControlPointX = ((CurveTo)previous).X2;
                        DrawAttribute.LastControlPointY = ((CurveTo)previous).Y2;
                    }
                    else if (previous.GetType() == typeof(QuadraticBezierCurveTo)) // Quadratic Bézier Curves
                    {
                        DrawAttribute.LastControlPointX = ((QuadraticBezierCurveTo)previous).X1;
                        DrawAttribute.LastControlPointY = ((QuadraticBezierCurveTo)previous).Y1;
                    }
                    else if (current.GetType() != typeof(SmoothCurveTo) && current.GetType() != typeof(SmoothQuadraticBezierCurveTo)) // Only reset if we're done with bézier curves
                    {
                        (DrawAttribute.LastControlPointX, DrawAttribute.LastControlPointY) = (0, 0);
                    }
                }
                previous = current; // previous element
            }

            return vertices.ToArray();
        }

        public override uint[] GetVerticesIndexes()
        {
            /*
            List<uint> indexes = new List<uint>();

            foreach (DrawAttribute attr in DrawAttributes)
            {
                if (attr.GetType() == typeof(LineTo))
                {
                    foreach (uint attrIndexes in ((LineTo)attr).GetVerticesIndexes())
                    {
                        indexes.Add(attrIndexes);
                    }
                }
            }
            return indexes.ToArray();
            */

            uint[] indexes = Enumerable.Range(0, GetVertices().Length / 2).Select(i => (uint)i).ToArray();

            return indexes;
        }

        public override Hitbox Hitbox()
        {
            Hitbox hitbox = new Hitbox();

            hitbox.Areas.Add(new Hitbox.Area(0, 0, 0, 0)); // TODO: please dont hardcode

            return hitbox;
        }

        public override string ToString()
        {
            string output = "d: ";
            DrawAttributes.ForEach(d => output += d.ToString() + " ");
            return output.Trim();
        }
    }
}
