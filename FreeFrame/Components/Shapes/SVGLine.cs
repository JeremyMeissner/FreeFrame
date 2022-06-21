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
    public class SVGLine : Shape
    {
        #region Default values
        const int DefaultX1 = 0;
        const int DefaultY1 = 0;
        const int DefaultX2 = 0;
        const int DefaultY2 = 0;
        #endregion

        public SVGLine(XmlReader reader) : this(
            Convert.ToInt32(reader["x1"]),
            Convert.ToInt32(reader["y1"]),
            Convert.ToInt32(reader["x2"]),
            Convert.ToInt32(reader["y2"]),
            Convert.ToString(reader["fill"] == null ? DefaultColor : reader["fill"]))
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

        /// <summary>
        /// Return the vertices position in NDC format
        /// </summary>
        /// <returns>array of vertices position. x, y, x, y, ... (clockwise)</returns>
        public override float[] GetVertices() => new float[] { X, Y, X + Width, Y + Height }; // x, y, x, y, x, y, ... (clockwise)

        /// <summary>
        /// Return the indexes position of the lines
        /// </summary>
        /// <returns>array of indexes</returns>
        public override uint[] GetVerticesIndexes() => new uint[] { 0, 1 };

        /// <summary>
        /// Reset the renderers and create new ones (use when update any properties of the shape)
        /// </summary>
        public override void ImplementObject()
        {
            foreach (Renderer vao in Renderers)
                vao.DeleteObjects();
            Renderers.Clear();

            Renderers.Add(new Renderer(GetVertices(), GetVerticesIndexes(), PrimitiveType.Lines, this ));
        }

        /// <summary>
        /// Retrieve the points that made the shape detectable
        /// </summary>
        /// <returns>Position of all the points</returns>
        public override List<Vector2i> GetSelectablePoints()
        {
            List<Vector2i> points = new();
            points.Add(new Vector2i(X, Y));
            points.Add(new Vector2i(Width + X, Height + Y));
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
        public override string ToString() => $"<line x1=\"{X}\" y1=\"{Y}\" x2=\"{Width + X}\" y2=\"{Height + Y}\" fill=\"{ColorToHexadecimal(Color)}\"/>";
    }
}
