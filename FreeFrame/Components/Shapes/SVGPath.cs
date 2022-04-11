using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using FreeFrame.Components.Shapes.Path;

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

        List<DrawAttribute> _drawAttribute = new();

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
                            _drawAttribute.Add(new MoveTo(match.Groups[1], match.Groups[2], c == 'm')); // 'm' is relative and 'M' absolute
                            break;
                        case 'l':
                        case 'L':
                            _drawAttribute.Add(new LineTo(match.Groups[1], match.Groups[2], c == 'l')); // 'l' is relative and 'L' absolute
                            break;
                        case 'H':
                        case 'h':
                            _drawAttribute.Add(new HorizontalLineTo(match.Groups[1], c == 'h')); // 'h' is relative and 'H' absolute
                            break;
                        case 'V':
                        case 'v':
                            _drawAttribute.Add(new VerticalLineTo(match.Groups[1], c == 'v')); // 'v' is relative and 'V' absolute
                            break;
                        case 'C':
                        case 'c':
                            _drawAttribute.Add(new CurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], match.Groups[5], match.Groups[6], c == 'c')); // 'c' is relative and 'C' absolute
                            break;
                        case 'S':
                        case 's':
                            _drawAttribute.Add(new SmoothCurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], c == 's')); // 's' is relative and 'S' absolute
                            break;
                        case 'Q':
                        case 'q':
                            _drawAttribute.Add(new QuadraticBezierCurveTo(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], c == 'q')); // 'q' is relative and 'Q' absolute
                            break;
                        case 'T':
                        case 't':
                            _drawAttribute.Add(new SmoothQuadraticBezierCurveTo(match.Groups[1], match.Groups[2], c == 't')); // 't' is relative and 'T' absolute
                            break;
                        case 'A':
                        case 'a':
                            _drawAttribute.Add(new EllipticalArc(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], match.Groups[5], match.Groups[6], match.Groups[7], c == 'a')); // 'a' is relative and 'A' absolute
                            break;
                        case 'Z':
                        case 'z':
                            _drawAttribute.Add(new ClosePath());
                            break;
                        default:
                            throw new Exception("Unknowed properties in d");
                    }
                    startIndex += match.Groups[0].Length + 1;
                }
            }
        }
        public override string ToString() 
        {
            string output = "d: ";
            _drawAttribute.ForEach(d => output += d.ToString() + " ");
            return output.Trim();
        }

        public override void UpdateProperties()
        {
            throw new NotImplementedException();
        }
    }
}
