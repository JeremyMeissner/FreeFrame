using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace FreeFrame.Components.Shapes
{
    public class SVGRectangle : Shape
    {
        #region Geometry properties
        private int _x;
        private int _y;

        private int _width;
        private int _height;

        private int _rx; // Rounded in the x axes
        private int _ry; // Rounded in the y axes
        #endregion

        public SVGRectangle(XmlReader reader) : this(
            Convert.ToInt32(reader["width"]),
            Convert.ToInt32(reader["height"]),
            Convert.ToInt32(reader["x"]),
            Convert.ToInt32(reader["y"]),
            Convert.ToInt32(reader["rx"]),
            Convert.ToInt32(reader["ry"])) // TODO: Error handler if one of the properties in reader or note here, it should be dynamic
        {
        }
        public SVGRectangle(): this(0, 0, 0, 0) { }
        public SVGRectangle(int width, int height) : this(width, height, 0, 0) { }
        public SVGRectangle(int width, int height, int x, int y) : this(width, height, x, y, 0, 0) { }
        public SVGRectangle(int width, int height, int x, int y, int rx, int ry)
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
