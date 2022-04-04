using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    class SVGRectangle : Shape
    {
        #region Geometry properties
        private float _x;
        private float _y;

        private float _width;
        private float _height;

        private float _rx; // Rounded in the x axes
        private float _ry; // Rounded in the y axes
        #endregion

        public SVGRectangle(XmlReader reader) : this(
            Convert.ToInt32(reader["x"]),
            Convert.ToInt32(reader["y"]),
            Convert.ToInt32(reader["width"]),
            Convert.ToInt32(reader["height"]),
            Convert.ToInt32(reader["rx"]),
            Convert.ToInt32(reader["ry"]))
        { }
        public SVGRectangle(float width, float height) : this(width, height, 0, 0) { }
        public SVGRectangle(float width, float height, float x, float y) : this(width, height, x, y, 0, 0) { }
        public SVGRectangle(float width, float height, float x, float y, float rx, float ry)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _rx = rx;
            _ry = ry;
        }
        
        public override string ToString() => $"x: {_x}, y: {_y}, width: {_width}, height: {_height}, rx: {_rx}, ry: {_ry}";
    }
}
