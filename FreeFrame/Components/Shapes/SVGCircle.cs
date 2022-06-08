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
        #region Default values
        const int DefaultR = 0;
        const int DefaultCY = 0;
        const int DefaultCX = 0;
        #endregion
        public SVGCircle(XmlReader reader) : this(
            Convert.ToInt32(reader["r"]),
            Convert.ToInt32(reader["cx"]),
            Convert.ToInt32(reader["cy"]),
            Convert.ToString(reader["fill"] == null ? DefaultColor : reader["fill"]))
        { }
        public SVGCircle() : this(DefaultR, DefaultCX, DefaultCY, "#000000FF") { }
        public SVGCircle(int r, int cx, int cy, string color)
        {
            IsCornerRadiusChangeable = false;
            X = cx - r;
            Y = cy - r;
            Height = r*2;
            Width = Height;
            Color = Importer.HexadecimalToRGB(color);

            ImplementObject();
        }

        /// <summary>
        /// Return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        public override float[] GetVertices() => new float[] { X, Y, X + Width, Y, X + Width, Y + Height, X, Y + Height }; // x, y, x, y, x, y, ... (clockwise)

        /// <summary>
        /// Return the indexes position of the triangles
        /// </summary>
        /// <returns>array of indexes</returns>
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1, 2, 0, 2, 3 };

        /// <summary>
        /// Reset the renderers and create new ones (use when update any properties of the shape)
        /// </summary>
        public override void ImplementObject()
        {
            foreach (Renderer render in Renderers)
                render.DeleteObjects();
            Renderers.Clear();

            Renderers.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.Triangles, this));
        }

        /// <summary>
        /// Retrieve the points that made the shape detectable
        /// </summary>
        /// <returns>Position of all the points</returns>
        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X, Y));
            points.Add(new Vector2i(X + Width, Y));
            points.Add(new Vector2i(X + Width, Y + Height));
            points.Add(new Vector2i(X, Y + Height));
            return points;
        }

        /// <summary>
        /// Move the current shape to the given position
        /// </summary>
        /// <param name="position">New position</param>
        public override void Move(Vector2i position)
        {
            X = position.X;
            Y = position.Y;
            ImplementObject();
        }

        /// <summary>
        /// Resize the current shape to the given size
        /// </summary>
        /// <param name="size">New size</param>
        public override void Resize(Vector2i size)
        {
            Width = size.X;
            Height = size.Y;
            ImplementObject();
        }

        /// <summary>
        /// Retrieve the Shape in the SVG format
        /// </summary>
        /// <returns>string of the SVG format</returns>
        public override string ToString() => $"<circle cx=\"{X + Width / 2}\" cy=\"{Y + Height / 2}\" r=\"{Width / 2}\" fill=\"{ColorToHexadecimal(Color)}\"/>";
    }
}
