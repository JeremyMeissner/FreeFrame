using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace FreeFrame.Components.Shapes
{
    public class SVGPolygon : Shape
    {
        readonly Regex _pointsRegex = new(@" *(\d+) *, *(\d+) *"); // https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/points

        #region Geometry properties
        List<(int, int)> _points = new();
        #endregion

        // TODO: Also add Polyline https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/points

        public SVGPolygon(XmlReader reader)
        {
            string points = reader["points"] ?? throw new Exception("points not here"); // TODO: Error handler if points is note here
            MatchCollection matches = _pointsRegex.Matches(points); // Retrieve every points

            foreach (Match match in matches)
                _points.Add((Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value)));
        }

        public override float[] GetVertices()
        {
            throw new NotImplementedException();
        }

        public override uint[] GetVerticesIndexes()
        {
            throw new NotImplementedException();
        }

        public override Hitbox Hitbox()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string output = "points: ";
            foreach ((int x, int y) point in _points)
                output += string.Format("{0},{1} ", point.x, point.y);
            return output.Trim();
        }
    }
}
