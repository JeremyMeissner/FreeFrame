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
        // First point x,y of the path
        int _x0, _y0 = 0;
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
        public SVGPath() { }
        public SVGPath(XmlReader reader)
        {
            IsResizeable = false;
            IsCornerRadiusChangeable = false;

            string d = reader["d"] ?? throw new Exception("d not here");
            Match match;
            int startIndex = 0;

            for (int i = 0; i < d.Length; i++)
            {
                char c = d[i]; // Get current char
                char lowerC = char.ToLower(d[i]);

                if (_dAttributeRegex.ContainsKey(lowerC))
                {
                    match = _dAttributeRegex[lowerC].Match(d, startIndex); // Retrieve the associated regular expression

                    if (startIndex == 0) // Save default location point for z
                    {
                        _x0 = Convert.ToInt32(match.Groups[1].Value);
                        _y0 = Convert.ToInt32(match.Groups[2].Value);
                    }

                    switch (lowerC)
                    {
                        case 'm':
                            DrawAttributes.Add(new MoveTo(match.Groups[1], match.Groups[2], c == 'm')); // 'm' is relative and 'M' absolute
                            break;
                        case 'l':
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
                            DrawAttributes.Add(new LineTo(_x0, _y0));
                            break;
                        default:
                            throw new Exception("Unknowed properties in d");
                    }
                    startIndex += match.Groups[0].Length + 1;
                }
            }

            string color = reader["fill"] ?? DefaultColor;
            Color = Importer.HexadecimalToRGB(color);


            // Update common properties, use the given attributes
            List<Vector2i> points = GetSelectablePoints();
            X = points.Min(i => i.X);
            Y = points.Min(i => i.Y);
            Width = points.Max(i => i.X) - points.Min(i => i.X);
            Height = points.Max(i => i.Y) - points.Min(i => i.Y);

            ImplementObject();
        }

        public override float[] GetVertices() => new float[] { };
        public override uint[] GetVerticesIndexes() => new uint[] { };

        /// <summary>
        /// Reset the renderers and create new ones (use when update any properties of the shape)
        /// </summary>
        public override void ImplementObject()
        {
            Move(new Vector2i(X, Y)); // Update the position since the attr doesnt use the X and Y variables directly
        }

        /// <summary>
        /// Retrieve the points that made the shape detectable
        /// </summary>
        /// <returns>Position of all the points</returns>
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

        /// <summary>
        /// Move the current shape to the given position
        /// </summary>
        /// <param name="position">New position</param>
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
                    //Console.WriteLine("{0} is relative? =>{1}", attr.GetType().Name, attr.IsRelative);
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
                        ((SmoothCurveTo)attr).X2 += (int)deltaX;
                        ((SmoothCurveTo)attr).Y2 += (int)deltaY;
                    }
                }
                if (attrType == typeof(CurveTo) ||
                    attrType == typeof(SmoothCurveTo) ||
                    attrType == typeof(QuadraticBezierCurveTo) ||
                    attrType == typeof(SmoothQuadraticBezierCurveTo) ||
                    attrType == typeof(EllipticalArc)) // Only thoses are LineStrip type
                {
                    Renderers.Add(new Renderer(attr.GetVertices(), attr.GetVerticesIndexes(), PrimitiveType.LineStrip, this));
                }
                else
                {
                    Renderers.Add(new Renderer(attr.GetVertices(), attr.GetVerticesIndexes(), PrimitiveType.Lines, this));
                }
                attr.UpdateLast(); // Update Last's inner variables 
            }

            // Update common properties
            X = position.X;
            Y = position.Y;
        }

        /// <summary>
        /// Resize the current shape to the given size
        /// </summary>
        /// <param name="size">New size</param>
        public override void Resize(Vector2i size) => throw new NotImplementedException("Can't resize a path");

        /// <summary>
        /// Retrieve the Shape in the SVG format
        /// </summary>
        /// <returns>string of the SVG format</returns>
        public override string ToString()
        {
            string output = "<path d=\"";
            DrawAttributes.ForEach(d => output += d.ToString() + " ");
            return output.Trim() + $"\" fill=\"{ColorToHexadecimal(Color)}\"/>";
        }
    }
}
