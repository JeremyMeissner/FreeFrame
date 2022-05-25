﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace FreeFrame.Components.Shapes
{
    public class SVGPolygon : Shape
    {
        readonly Regex _pointsAttributeRegex = new(@" *(\d+) *, *(\d+) *"); // https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/points

        #region Geometry properties
        List<Vector2i> _points = new();
        #endregion

        // TODO: Also add Polyline https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/points

        public SVGPolygon(XmlReader reader)
        {
            IsResizeable = false;
            IsCornerRadiusChangeable = false;

            string points = reader["points"] ?? throw new Exception("points not here"); // TODO: Error handler if points is note here
            MatchCollection matches = _pointsAttributeRegex.Matches(points); // Retrieve every points

            foreach (Match match in matches)
                _points.Add((Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value)));

            string color = reader["fill"] ?? throw new Exception("color not here"); // TODO: Error handler if d is note here
            Color = Importer.HexadecimalToRGB(color);

            // Fill geometry values
            X = _points.Min(i => i.X);
            Y = _points.Min(i => i.Y);
            Width = _points.Max(i => i.X) - _points.Min(i => i.X);
            Height = _points.Max(i => i.Y) - _points.Min(i => i.Y);

            // Made points relatives
            for (int i = 0; i < _points.Count; i++)
                _points[i] = new Vector2i(_points[i].X - X, _points[i].Y - Y); // delta

            if (_points.Count == 3)
                IsResizeable = true;

            ImplementObject();
        }
        public SVGPolygon(int x, int y, int width, int height, string color)
        {
            IsCornerRadiusChangeable = false;

            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = Importer.HexadecimalToRGB(color);

            _points.Add(new Vector2i(X + Width / 2, Y));
            _points.Add(new Vector2i(X, Y + Height));
            _points.Add(new Vector2i(X + Width, Y + Height));

            ImplementObject();
        }

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> selectablePoints = new List<Vector2i>();
            if (_points.Count == 3) // Because a triangle is also base on the size
            {
                selectablePoints.Add(new Vector2i(X, Y));
                selectablePoints.Add(new Vector2i(X + Width, Y));
            }
            foreach (Vector2i point in _points)
                selectablePoints.Add(new Vector2i(X + point.X, Y + point.Y));
            return selectablePoints;
        }

        public override float[] GetVertices()
        {
            if (_points.Count == 3) // Because a triangle is also base on the size
                return new float[] { X + Width / 2, Y, X, Y + Height, X + Width, Y + Height }; // TODO: Use Select, Cast, whatever                                                                   
            else // Any other polygon
            {
                List<float> vertices = new List<float>();
                foreach (Vector2i point in _points)
                {
                    vertices.Add(X + point.X);
                    vertices.Add(Y + point.Y);
                }
                return vertices.ToArray();
            }
        }

        public override uint[] GetVerticesIndexes()
        {
            if (_points.Count == 3) // Triangle
                return new uint[] { 0, 1, 2 };
            else // Any other polygon
                return Enumerable.Range(0, _points.Count).Select(i => (uint)i).ToArray();
        }

        public override void ImplementObject()
        {
            foreach (Renderer vao in Vaos)
                vao.DeleteObjects();
            Vaos.Clear();

            if (_points.Count == 3) // Triangle
                Vaos.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.Triangles, this));
            else
                Vaos.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.LineLoop, this));

        }

        public override void Move(Vector2i position)
        {
            X = position.X;
            Y = position.Y;
            ImplementObject();
        }

        public override void Resize(Vector2i size)
        {
            Width = size.X;
            Height = size.Y;
            ImplementObject();
        }

        public override string ToString()
        {
            string output = "<polygon points=\"";
            if (_points.Count == 3) // Because a triangle is also base on the size
                output += String.Format("{0},{1} {2},{3} {4},{5}", X + Width / 2, Y, X, Y + Height, X + Width, Y + Height);
            else
                foreach (Vector2i point in _points)
                    output += string.Format("{0},{1} ", X + point.X, Y + point.Y);
            return output.Trim() + $"\" fill=\"{ColorToHexadecimal(Color)}\"/>";
        }

    }
}
