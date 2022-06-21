using FreeFrame.Components.Shapes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FreeFrame
{
    public class Selector : IDrawable
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
            public float[] ToFloatArray() => new float[] { X, Y, X + Width, Y, X + Width, Y + Height, X, Y + Height }; // Clockwise

        }
        public enum SelectorType
        {
            Edge,
            Move,
            Resize,
            None
        }

        private List<(Renderer vao, Area hitbox, SelectorType type)> _selectors;

        public Selector()
        {
            _selectors = new List<(Renderer vao, Area hitbox, SelectorType type)>();
        }

        /// <summary>
        /// Select the shape to draw the selector with
        /// </summary>
        /// <param name="shape">Selected shape</param>
        public void Select(Shape shape)
        {
            // Delete current Selector
            DeleteObjects();

            // Edge
            Area hitbox = new Area
            {
                X = shape.X,
                Y = shape.Y,
                Width = shape.Width,
                Height = shape.Height
            };
            _selectors.Add((new Renderer(hitbox.ToFloatArray(), new uint[] { 0, 1, 2, 3 }, PrimitiveType.LineLoop), hitbox, SelectorType.Edge)); // The edge is only a square empty


            if (shape.IsMoveable)
            {
                // Move selector (top-left)
                hitbox = new Area
                {
                    X = shape.X - 5,
                    Y = shape.Y - 5,
                    Width = 10,
                    Height = 10
                };
                _selectors.Add((new Renderer(hitbox.ToFloatArray(), new uint[] { 0, 1, 2, 0, 2, 3 }, PrimitiveType.Triangles), hitbox, SelectorType.Move)); // The corner are filled so it's two triangles
            }

            if (shape.IsResizeable)
            {
                // Resize selector (bottom-right)
                hitbox = new Area
                {
                    X = shape.X + shape.Width - 5,
                    Y = shape.Y + shape.Height - 5,
                    Width = 10,
                    Height = 10
                };
                _selectors.Add((new Renderer(hitbox.ToFloatArray(), new uint[] { 0, 1, 2, 0, 2, 3 }, PrimitiveType.Triangles), hitbox, SelectorType.Resize)); // The corner are filled so it's two triangles
            }
        }

        /// <summary>
        /// Trigger draw element through OpenGL context
        /// </summary>
        /// <param name="clientSize">Window size</param>
        public void Draw(Vector2i clientSize)
        {
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(3.0f);

            foreach ((Renderer vao, Area _, SelectorType type) selector in _selectors)
            {
                if (selector.type == SelectorType.Edge) // The color is not the same for the edge and the corners
                    selector.vao.Draw(clientSize, new Color4(0, 125, 200, 255));
                else
                    selector.vao.Draw(clientSize, new Color4(0, 125, 255, 255));
            }

            GL.Disable(EnableCap.LineSmooth);
        }
        /// <summary>
        /// Return true if the given mouse position is in the selector hitbox
        /// </summary>
        /// <param name="mousePosition">Mouse position</param>
        /// <returns>true if touching the selector (and give the selector type), false otherwise (and null) </returns>
        public (bool, SelectorType?) HitBox(Vector2i mousePosition)
        {
            if (_selectors.Count > 0)
                foreach ((Renderer _, Area hitbox, SelectorType type) selector in _selectors)
                    if (selector.hitbox.X < mousePosition.X && selector.hitbox.X + selector.hitbox.Width > mousePosition.X && selector.hitbox.Y < mousePosition.Y && selector.hitbox.Y + selector.hitbox.Height > mousePosition.Y)
                        return (true, selector.type);
            return (false, null);
        }

        /// <summary>
        /// Delete all the renderers for each selector
        /// </summary>
        public void DeleteObjects()
        {
            _selectors.ForEach(i => i.vao.DeleteObjects());
            _selectors.Clear();
        }
    }
}
