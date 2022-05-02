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
        public struct Area
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public Area(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }
        public enum SelectorType
        {
            Edge,
            Move,
            Resize,
            None
        }

        // I'm actually implementing selection for selector
        protected List<(VertexArrayObject vao, Area hitbox, SelectorType type)> _vaos;
        public Selector()
        {
            _vaos = new List<(VertexArrayObject vao, Area hitbox, SelectorType type)>();
        }
        public void Select(Shape shape)
        {
            _vaos.ForEach(i => i.vao.DeleteObjects());
            _vaos.Clear();

            //List<Vector2i> points = shape.GetSelectablePoints();

            // Edge
            Area hitbox = new Area
            {
                X = shape.X,
                Y = shape.Y,
                Width = shape.Width,
                Height = shape.Height
            };
            //Area hitbox = new Area
            //{
            //    X = points.Min(i => i.X),
            //    Y = points.Min(i => i.Y),
            //    Width = points.Max(i => i.X) - points.Min(i => i.X),
            //    Height = points.Max(i => i.Y) - points.Min(i => i.Y)
            //};
            _vaos.Add((new VertexArrayObject(AreaToFloatArray(hitbox), new uint[] { 0, 1, 2, 3 }, PrimitiveType.LineLoop), hitbox, SelectorType.Edge));

            // Move selector (top-left)
            hitbox = new Area
            {
                X = shape.X - 5,
                Y = shape.Y - 5,
                Width = 10,
                Height = 10
            }; 
            //hitbox = new Area
            //{
            //    X = points.Min(i => i.X) - 5,
            //    Y = points.Min(i => i.Y) - 5,
            //    Width = 10,
            //    Height = 10
            //};
            _vaos.Add((new VertexArrayObject(AreaToFloatArray(hitbox), new uint[] { 0, 1, 2, 0, 2, 3 }, PrimitiveType.Triangles), hitbox, SelectorType.Move));

            // Move selector (bottom-right)
            hitbox = new Area
            {
                X = shape.X + shape.Width - 5,
                Y = shape.Y + shape.Height - 5,
                Width = 10,
                Height = 10
            };
            //hitbox = new Area
            //{
            //    X = points.Max(i => i.X) - 5,
            //    Y = points.Max(i => i.Y) - 5,
            //    Width = 10,
            //    Height = 10
            //};
            _vaos.Add((new VertexArrayObject(AreaToFloatArray(hitbox), new uint[] { 0, 1, 2, 0, 2, 3 }, PrimitiveType.Triangles), hitbox, SelectorType.Resize));

        }
        public static float[] AreaToFloatArray(Area area)
        {
            return new float[]
            {
                area.X, area.Y, area.X + area.Width, area.Y, area.X + area.Width, area.Y + area.Height, area.X, area.Y + area.Height // Clockwise
            };
        }
        public void Draw(Vector2i clientSize)
        {
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(3.0f);
            foreach ((VertexArrayObject vao, Area _, SelectorType type) part in _vaos)
            {
                if (part.type == SelectorType.Edge)
                    part.vao.Draw(clientSize, new Color4(0, 125, 200, 255));
                else
                    part.vao.Draw(clientSize, new Color4(0, 125, 255, 255));
            }
            GL.Disable(EnableCap.LineSmooth);

        }
        public (bool, SelectorType?) HitBox(Vector2i mousePosition)
        {
            if (_vaos.Count > 0)
                foreach ((VertexArrayObject _, Area hitbox, SelectorType type) part in _vaos)
                    if (part.hitbox.X < mousePosition.X && part.hitbox.X + part.hitbox.Width > mousePosition.X && part.hitbox.Y < mousePosition.Y && part.hitbox.Y + part.hitbox.Height > mousePosition.Y)
                        return (true, part.type);
            return (false, null);
        }
    }
}
