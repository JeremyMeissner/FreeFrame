﻿using System;
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

        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
        public int Width { get => _width; set => _width = value; }
        public int Height { get => _height; set => _height = value; }
        public int Rx { get => _rx; set => _rx = value; }
        public int Ry { get => _ry; set => _ry = value; }
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
            Rx = rx;
            Ry = ry;
        }
        public override float[] GetVertices() => new float[] { X, Y, X + Width, Y, X + Width, Y + Height, X, Y + Height }; // x, y, x, y, x, y, ... (clockwise)
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1, 2, 0, 2, 3 }; // TODO: please dont hardcode


        public override string ToString() => $"x: {X}, y: {Y}, width: {Width}, height: {Height}, rx: {Rx}, ry: {Ry}";

        public override Hitbox Hitbox()
        {
            Hitbox hitbox = new Hitbox();

            hitbox.Areas.Add(new Hitbox.Area(X, Y, Width, Height));

            return hitbox;
        }
    }
}
