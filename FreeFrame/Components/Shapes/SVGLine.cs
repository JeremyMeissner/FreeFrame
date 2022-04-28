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
        private int _x1;
        private int _y1;
        private int _x2;
        private int _y2;

        public int X1 { get => _x1; set => _x1 = value; }
        public int Y1 { get => _y1; set => _y1 = value; }
        public int X2 { get => _x2; set => _x2 = value; }
        public int Y2 { get => _y2; set => _y2 = value; }
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
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;

            Properties = new DefaultProperties()
            {
                x = X1,
                y = Y1,
                width = X2 - X1,
                height = Y2 - Y1,
                color = Properties.color
            };

            ImplementObject();
        }
        public override float[] GetVertices() => new float[] { X1, Y1, X2, Y2 }; // x, y, x, y, x, y, ... (clockwise)
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 }; // TODO: please dont hardcode

        public override void Draw(Vector2i clientSize)
        {
            foreach (VertexArrayObject vao in _vaos)
                vao.Draw(clientSize, Properties.color); // Because that color doesnt depend of the shape TODO: Make it dependend
        }
        public override string ToString() => $"x1: {X1}, y1: {Y1}, x2: {X2}, y2: {Y2}";

        public override Hitbox Hitbox()
        {
            Hitbox hitbox = new Hitbox();

            hitbox.Areas.Add(new Hitbox.Area(X1, Y1, X2, Y2));

            return hitbox;
        }

        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X1, Y1));
            points.Add(new Vector2i(X2, Y2));
            return points;
        }

        public override void UpdateProperties(DefaultProperties properties)
        {
            X1 = Properties.x;
            Y1 = Properties.y;
            X2 = X1 + Properties.width;
            Y2 = Y1 + Properties.height;

            Properties = properties;

            ImplementObject();
        }

        public override void ImplementObject()
        {
            foreach (VertexArrayObject vao in _vaos)
                vao.DeleteObjects();
            _vaos.Clear();

            _vaos.Add(new VertexArrayObject(GetVertices(), GetVerticesIndexes(), PrimitiveType.Lines));
        }

        public override void Move(Vector2i position)
        {
            int width = X2 - X1;
            int height = Y2 - Y1;

            X1 = position.X;
            Y1 = position.Y;

            X2 = position.X + width;
            Y2 = position.Y + height;


            Properties = new DefaultProperties()
            {
                x = X1,
                y = Y1,
                width = width,
                height = height,
                color = Properties.color
            };

            ImplementObject();
        }

        public override void Resize(Vector2i size)
        {
            X2 = X1 + size.X;
            Y2 = Y1 + size.Y;

            Properties = new DefaultProperties()
            {
                x = X1,
                y = Y1,
                width = X2 - X1,
                height = Y2 - Y1,
                color = Properties.color
            };

            ImplementObject();
        }
    }
}
