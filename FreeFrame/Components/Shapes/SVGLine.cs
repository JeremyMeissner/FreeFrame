using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    internal class SVGLine : Shape
    {
        #region Default values
        const int DefaultX1 = 0;
        const int DefaultY1 = 0;
        const int DefaultX2 = 0;
        const int DefaultY2 = 0;
        #endregion

        #region Geometry properties
        #endregion

        public SVGLine(XmlReader reader) : this(
            Convert.ToInt32(reader["x1"]),
            Convert.ToInt32(reader["y1"]),
            Convert.ToInt32(reader["x2"]),
            Convert.ToInt32(reader["y2"])) // TODO: Error handler if one of the properties in reader is note here, it should be dynamic
        {
        }
        public SVGLine() : this(DefaultX1, DefaultY1, DefaultX2, DefaultY2) { }
        public SVGLine(int x1, int y1, int x2, int y2)
        {
            X = x1;
            Y = y1;
            Width = x2 - X;
            Height = y2 - Y;

            ImplementObject();
        }
        public override float[] GetVertices() => new float[] { X, Y, Width + X, Height + Y }; // x, y, x, y, x, y, ... (clockwise)
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 }; // TODO: please dont hardcode

        public override void Draw(Vector2i clientSize)
        {
            foreach (VertexArrayObject vao in _vaos)
                vao.Draw(clientSize, Color, this); // Because that color doesnt depend of the shape TODO: Make it dependend
        }
        public override string ToString() => $"x1: {X}, y1: {Y}, x2: {Width + X}, y2: {Height + Y}";


        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X, Y));
            points.Add(new Vector2i(Width + X, Height + Y));
            return points;
        }


        public override void ImplementObject()
        {
            foreach (VertexArrayObject vao in _vaos)
                vao.DeleteObjects();
            _vaos.Clear();

            _vaos.Add(new VertexArrayObject(GetVertices(), GetVerticesIndexes(), PrimitiveType.Lines, this ));
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
