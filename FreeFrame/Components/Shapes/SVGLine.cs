﻿using OpenTK.Graphics.OpenGL4;
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
        const string DefaultColor = "#000000FF";
        #endregion

        public SVGLine(XmlReader reader) : this(
            Convert.ToInt32(reader["x1"]),
            Convert.ToInt32(reader["y1"]),
            Convert.ToInt32(reader["x2"]),
            Convert.ToInt32(reader["y2"]),
            Convert.ToString(reader["fill"])) // TODO: Error handler if one of the properties in reader is note here, it should be dynamic
        {
        }
        public SVGLine() : this(DefaultX1, DefaultY1, DefaultX2, DefaultY2, DefaultColor) { }
        public SVGLine(int x1, int y1, int x2, int y2, string color)
        {
            IsCornerRadiusChangeable = false;

            X = x1;
            Y = y1;
            Width = x2 - X;
            Height = y2 - Y;
            Color = Importer.HexadecimalToRGB(color);

            ImplementObject();
        }
        public override float[] GetVertices() => new float[] { X, Y, Width + X, Height + Y }; // x, y, x, y, x, y, ... (clockwise)
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 }; // TODO: please dont hardcode
        public override string ToString() => $"<line x1=\"{X}\" y1=\"{Y}\" x2=\"{Width + X}\" y2=\"{Height + Y}\" fill=\"{ColorToHexadecimal(Color)}\"/>";
        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X, Y));
            points.Add(new Vector2i(Width + X, Height + Y));
            return points;
        }
        public override void ImplementObject()
        {
            foreach (Renderer vao in Vaos)
                vao.DeleteObjects();
            Vaos.Clear();

            Vaos.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.Lines, this ));
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
