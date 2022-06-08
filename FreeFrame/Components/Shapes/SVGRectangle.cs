using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FreeFrame.Components.Shapes
{
    public class SVGRectangle : Shape
    {
        #region Default values
        const int DefaultX = 0;
        const int DefaultY = 0;
        const int DefaultWidth = 0;
        const int DefaultHeight = 0;
        const int DefaultRX = 0;
        const int DefaultRY = 0;
        #endregion

        public SVGRectangle(XmlReader reader) : this(
            Convert.ToInt32(reader["width"]),
            Convert.ToInt32(reader["height"]),
            Convert.ToInt32(reader["x"]),
            Convert.ToInt32(reader["y"]),
            Convert.ToInt32(reader["rx"]),
            Convert.ToInt32(reader["ry"]),
            Convert.ToString(reader["fill"] == null ? DefaultColor : reader["fill"]))
        {
        }
        public SVGRectangle() : this(DefaultWidth, DefaultHeight, DefaultX, DefaultY) { }
        public SVGRectangle(int width, int height) : this(width, height, DefaultX, DefaultY) { }
        public SVGRectangle(int width, int height, int x, int y) : this(width, height, x, y, DefaultRX, DefaultRY, DefaultColor) { }
        public SVGRectangle(int width, int height, int x, int y, int rx, int ry, string color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            CornerRadius = Math.Max(rx, ry);
            Color = Importer.HexadecimalToRGB(color);

            ImplementObject();
        }

        /// <summary>
        /// Should return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        public override float[] GetVertices() => new float[] { X, Y, X + Width, Y, X + Width, Y + Height, X, Y + Height }; // x, y, x, y, x, y, ... (clockwise)

        /// <summary>
        /// Should return the indexes position of the triangles
        /// </summary>
        /// <returns>array of indexes</returns>
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1, 2, 0, 2, 3 };

        /// <summary>
        /// Reset the renderers and create new ones (use when update any properties of the shape)
        /// </summary>
        public override void ImplementObject()
        {
            foreach (Renderer render in Renderers)
                render.DeleteObjects();
            Renderers.Clear();

            Renderers.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.Triangles, this));
        }

        /// <summary>
        /// Retrieve the points that made the shape detectable
        /// </summary>
        /// <returns>Position of all the points</returns>
        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X, Y));
            points.Add(new Vector2i(X + Width, Y));
            points.Add(new Vector2i(X + Width, Y + Height));
            points.Add(new Vector2i(X, Y + Height));
            return points;
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
            return $"<rect x=\"{X}\" y=\"{Y}\" width=\"{Width}\" height=\"{Height}\" rx=\"{CornerRadius}\" ry=\"{CornerRadius}\" fill=\"{ColorToHexadecimal(Color)}\"/>";
        }
    }
}
