using FreeFrame.Components.Shapes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeFrame
{
    public class Selector
    {
        protected enum SelectorType
        {
            Edge,
            Move
        }

        // I'm actually implementing selection for selector
        protected List<(VertexArrayObject vao, Vector4i[] vertices, SelectorType type)> _vaos;
        public Selector()
        {
            _vaos = new List<(VertexArrayObject vao, Vector4i[] vertices, SelectorType type)>();
        }
        public void Select(Shape shape)
        {
            _vaos.ForEach(i => i.vao.DeleteObjects());
            _vaos.Clear();

            List<Vector2i> points = shape.GetSelectablePoints();

            // Edge
            float[] vertices = new float[] { points.Min(i => i.X), points.Min(i => i.Y), points.Max(i => i.X), points.Min(i => i.Y), points.Max(i => i.X), points.Max(i => i.Y), points.Min(i => i.X), points.Max(i => i.Y) };
            _vaos.Add((new VertexArrayObject(vertices, new uint[] { 0, 1, 2, 3 }, PrimitiveType.LineLoop), vertices, SelectorType.Edge));

            vertices = new float[] { points.Min(i => i.X) - 5, points.Min(i => i.Y) - 5, points.Min(i => i.X) + 5, points.Min(i => i.Y) - 5, points.Min(i => i.X) + 5, points.Min(i => i.Y) + 5, points.Min(i => i.X) - 5, points.Min(i => i.Y) + 5 };
            _vaos.Add((new VertexArrayObject(vertices, new uint[] { 0, 1, 2, 0, 2, 3 }, PrimitiveType.Triangles), vertices, SelectorType.Move));

        }
        public void Draw(Vector2i clientSize)
        {
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(3.0f);
            foreach ((VertexArrayObject vao, float[] vertices, SelectorType type) part in _vaos)
            {
                if (part.type == SelectorType.Edge)
                    part.vao.Draw(clientSize, new Color4(0, 125, 200, 255));
                else
                    part.vao.Draw(clientSize, new Color4(0, 125, 255, 255));
            }
            GL.Disable(EnableCap.LineSmooth);

        }
        public bool HitBox(Vector2i mousePosition)
        {

            (_, float[] vertices, _) = _vaos.Where(part => part.type == SelectorType.Move).Single();
            foreach (var item in collection)
            {

            }
            return true;
        }
    }
}
