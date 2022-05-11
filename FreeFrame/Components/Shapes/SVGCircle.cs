using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    public class SVGCircle : Shape
    {
        public SVGCircle(XmlReader reader) : this(
            Convert.ToInt32(reader["r"]),
            Convert.ToInt32(reader["cx"]),
            Convert.ToInt32(reader["cy"])) // TODO: Error handler if r, cx or cy are not here
        { }
        public SVGCircle() : this(0, 0, 0) { }
        public SVGCircle(int r, int cx, int cy)
        {
            IsCornerRadiusChangeable = false;
            X = cx - r;
            Y = cy - r;
            Height = r*2;
            Width = Height;

            ImplementObject();
        }

        public override string ToString() => $"cx: {X + Width / 2}, cy: {Y + Height / 2}, r: {Width / 2}";

        public override float[] GetVertices() => new float[] { X, Y, X + Width, Y, X + Width, Y + Height, X, Y + Height }; // x, y, x, y, x, y, ... (clockwise)
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1, 2, 0, 2, 3 }; // TODO: please dont hardcode


        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X, Y));
            points.Add(new Vector2i(X + Width, Y));
            points.Add(new Vector2i(X + Width, Y + Height));
            points.Add(new Vector2i(X, Y + Height));
            return points;
        }

        public override void ImplementObject()
        {
            foreach (Renderer vao in Vaos)
                vao.DeleteObjects();
            Vaos.Clear();

            Vaos.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.Triangles, this));
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
