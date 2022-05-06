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
            Convert.ToInt32(reader["ry"])) // TODO: Error handler if one of the properties in reader is note here, it should be dynamic
        {
        }
        public SVGRectangle() : this(DefaultWidth, DefaultHeight, DefaultX, DefaultY) { }
        public SVGRectangle(int width, int height) : this(width, height, DefaultX, DefaultY) { }
        public SVGRectangle(int width, int height, int x, int y) : this(width, height, x, y, DefaultRX, DefaultRY) { }
        public SVGRectangle(int width, int height, int x, int y, int rx, int ry)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            CornerRadius = Math.Max(rx, ry);

            ImplementObject();
        }
        public override void ImplementObject()
        {
            foreach (VertexArrayObject vao in Vaos)
                vao.DeleteObjects();
            Vaos.Clear();

            Vaos.Add(new VertexArrayObject(GetVertices(), GetVerticesIndexes(), PrimitiveType.Triangles, this));
        }
        public override float[] GetVertices() => new float[] { X, Y, X + Width, Y, X + Width, Y + Height, X, Y + Height }; // x, y, x, y, x, y, ... (clockwise)
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1, 2, 0, 2, 3 }; // TODO: please dont hardcode

        public override string ToString() => $"x: {X}, y: {Y}, width: {Width}, height: {Height}, rx: {CornerRadius}, ry: {CornerRadius}";

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X, Y));
            points.Add(new Vector2i(X + Width, Y));
            points.Add(new Vector2i(X + Width, Y + Height));
            points.Add(new Vector2i(X, Y + Height));
            return points;
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
    }
}
