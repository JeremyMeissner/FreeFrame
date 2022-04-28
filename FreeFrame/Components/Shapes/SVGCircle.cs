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
        #region Geometry properties
        private int _cx;
        private int _cy;
        private int _r;
        #endregion
        public SVGCircle(XmlReader reader) : this(
            Convert.ToInt32(reader["r"]),
            Convert.ToInt32(reader["cx"]),
            Convert.ToInt32(reader["cy"])) // TODO: Error handler if r, cx or cy are not here
        { }
        public SVGCircle() : this(0, 0, 0) { }
        public SVGCircle(int r, int cx, int cy)
        {
            _cx = cx;
            _cy = cy;
            _r = r;
        }
        public override void Draw(Vector2i clientSize) => throw new NotImplementedException();

        public override string ToString() => $"cx: {_cx}, cy: {_cy}, r: {_r}";

        public override float[] GetVertices() => new float[] { _cx - _r, _cy - _r, _cx + _r, _cy - _r, _cx + _r, _cy + _r, _cx - _r, _cy + _r }; // x, y, x, y, x, y, ... (clockwise)

        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1, 2, 0, 2, 3 }; // TODO: please dont hardcode

        public override Hitbox Hitbox()
        {
            throw new NotImplementedException();
        }

        public override List<Vector2i> GetSelectablePoints()
        {
            throw new NotImplementedException();
        }

        public override void UpdateProperties(DefaultProperties properties)
        {
            throw new NotImplementedException();
        }

        public override void ImplementObject()
        {
            throw new NotImplementedException();
        }

        public override void Move(Vector2i position)
        {
            throw new NotImplementedException();
        }

        public override void Resize(Vector2i size)
        {
            throw new NotImplementedException();
        }
    }
}
