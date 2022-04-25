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
        }
        public override float[] GetVertices() => new float[] { X1, Y1, X2, Y2 }; // x, y, x, y, x, y, ... (clockwise)
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 }; // TODO: please dont hardcode


        public override string ToString() => $"x1: {X1}, y1: {Y1}, x2: {X2}, y2: {Y2}";

        public override Hitbox Hitbox()
        {
            Hitbox hitbox = new Hitbox();

            hitbox.Areas.Add(new Hitbox.Area(0, 0, 0, 0)); // TODO: please dont hardcode

            return hitbox;
        }
    }
}
