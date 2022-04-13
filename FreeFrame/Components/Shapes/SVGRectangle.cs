using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using OpenTK.Graphics.OpenGL4;

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
            Convert.ToInt32(reader["ry"])) // TODO: Error handler if one of the properties in reader is note here, it should be dynamic
        {
        }
        public SVGRectangle() : this(DefaultWidth, DefaultHeight, DefaultX, DefaultY) { }
        public SVGRectangle(int width, int height) : this(width, height, DefaultX, DefaultY) { }
        public SVGRectangle(int width, int height, int x, int y) : this(width, height, x, y, DefaultRX, DefaultRY) { }
        public SVGRectangle(int width, int height, int x, int y, int rx, int ry)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _rx = rx;
            _ry = ry;
        }
        public override float[] GetVertices()
        {
            if (_window == null)
                throw new Exception("Trying to convert to NDC but no Window is binded");

            // x, y, x, y, x, y, ... (clockwise)
            return _window.ConvertToNDC(_x, _y, _x + _width, _y, _x + _width, _y + _height, _x, _y + _height);
        }
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1, 2, 0, 2, 3 }; // TODO: please dont hardcode


        public override string ToString() => $"x: {_x}, y: {_y}, width: {_width}, height: {_height}, rx: {_rx}, ry: {_ry}";
    }
}
