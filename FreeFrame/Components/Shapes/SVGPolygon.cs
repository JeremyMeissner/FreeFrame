using System;
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

        public List<Vector2i> Points { get => _points; set => _points = value; }
        #endregion

        public SVGPolygon() { }
        public SVGPolygon(XmlReader reader)
        {
            IsResizeable = false;
            IsCornerRadiusChangeable = false;

            string points = reader["points"] ?? throw new Exception("points not here"); // TODO: Error handler if points is note here
            MatchCollection matches = _pointsAttributeRegex.Matches(points); // Retrieve every points

            foreach (Match match in matches)
                Points.Add((Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value)));

            string color = reader["fill"] ?? DefaultColor; // TODO: Error handler if d is note here
            Color = Importer.HexadecimalToRGB(color);

            // Fill geometry values
            X = Points.Min(i => i.X);
            Y = Points.Min(i => i.Y);
            Width = Points.Max(i => i.X) - Points.Min(i => i.X);
            Height = Points.Max(i => i.Y) - Points.Min(i => i.Y);

            // Made points relatives
            for (int i = 0; i < Points.Count; i++)
                Points[i] = new Vector2i(Points[i].X - X, Points[i].Y - Y); // delta

            if (Points.Count == 3)
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

            Points.Add(new Vector2i(X + Width / 2, Y));
            Points.Add(new Vector2i(X, Y + Height));
            Points.Add(new Vector2i(X + Width, Y + Height));

            ImplementObject();
        }

        /// <summary>
        /// Should return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        public override float[] GetVertices()
        {
            if (Points.Count == 3) // Because a triangle is also base on the size
                return new float[] { X + Width / 2, Y, X, Y + Height, X + Width, Y + Height }; // TODO: Use Select, Cast, whatever                                                                   
            else // Any other polygon
            {
                List<float> vertices = new List<float>();
                foreach (Vector2i point in Points)
                {
                    vertices.Add(X + point.X);
                    vertices.Add(Y + point.Y);
                }
                return vertices.ToArray();
            }
        }

        /// <summary>
        /// Should return the indexes position of the triangles
        /// </summary>
        /// <returns>array of indexes</returns>
        public override uint[] GetVerticesIndexes()
        {
            if (Points.Count == 3) // Triangle
                return new uint[] { 0, 1, 2 };
            else // Any other polygon
                return Enumerable.Range(0, Points.Count).Select(i => (uint)i).ToArray();
        }

        /// <summary>
        /// Reset the renderers and create new ones (use when update any properties of the shape)
        /// </summary>
        public override void ImplementObject()
        {
            foreach (Renderer render in Renderers)
                render.DeleteObjects();
            Renderers.Clear();

            if (Points.Count == 3) // Triangle
                Renderers.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.Triangles, this));
            else
                Renderers.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.LineLoop, this));
        }

        /// <summary>
        /// Retrieve the points that made the shape detectable
        /// </summary>
        /// <returns>Position of all the points</returns>
        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> selectablePoints = new List<Vector2i>();
            if (Points.Count == 3) // Because a triangle is also base on the size
            {
                selectablePoints.Add(new Vector2i(X, Y));
                selectablePoints.Add(new Vector2i(X + Width, Y));
            }
            foreach (Vector2i point in Points)
                selectablePoints.Add(new Vector2i(X + point.X, Y + point.Y));
            return selectablePoints;
        }

        /// <summary>
        /// Move the current shape to the given position
        /// </summary>
        /// <param name="position">New position</param>
        public override void Move(Vector2i position)
        {
            X = position.X;
            Y = position.Y;
            ImplementObject();
        }

        /// <summary>
        /// Resize the current shape to the given size
        /// </summary>
        /// <param name="size">New size</param>
        public override void Resize(Vector2i size)
        {
            Width = size.X;
            Height = size.Y;
            ImplementObject();
        }

        /// <summary>
        /// Retrieve the Shape in the SVG format
        /// </summary>
        /// <returns>string of the SVG format</returns>
        public override string ToString()
        {
            string output = "<polygon points=\"";
            if (Points.Count == 3) // Because a triangle is also base on the size
                output += String.Format("{0},{1} {2},{3} {4},{5}", X + Width / 2, Y, X, Y + Height, X + Width, Y + Height);
            else
                foreach (Vector2i point in Points)
                    output += string.Format("{0},{1} ", X + point.X, Y + point.Y);
            return output.Trim() + $"\" fill=\"{ColorToHexadecimal(Color)}\"/>";
        }

    }
}
